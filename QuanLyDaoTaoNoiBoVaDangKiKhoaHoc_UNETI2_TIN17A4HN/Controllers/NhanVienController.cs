//Họ và tên : Phí Bảo Nam - 23103100236
//Module 3: Quản lý nhân viên – Đăng ký – Duyệt đào tạo.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Services;
using Microsoft.AspNetCore.Authorization;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;

namespace QuanLyDaoTaoNoiBo.Controllers;

/// <summary>
/// Quản lý hồ sơ nhân viên: xem danh sách, chi tiết, thêm mới, sửa và xóa hồ sơ.
/// Phân quyền: Dành riêng cho Quản trị viên (QuanTri) và Nhân sự - Đào tạo (QuanLy).
/// </summary>
[SessionAuthorize(Roles = "QuanTri,QuanLy")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class NhanVienController(ApplicationDbContext db) : Controller
{
    /// <summary>
    /// Hiển thị danh sách nhân viên kèm tìm kiếm, lọc theo phòng ban, trạng thái và phân trang.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, int? maPhongBan, string? trangThai,
        string sortOrder = "name_asc", int pageIndex = 1)
    {
        searchTerm = searchTerm?.Trim();
        ModelState.Remove(nameof(searchTerm));
        if (trangThai is not ("hoatdong" or "ngung")) trangThai = null;
        if (!NhanVienListViewModel.SortOptions.ContainsKey(sortOrder ?? "")) sortOrder = "name_asc";
        if (maPhongBan <= 0) maPhongBan = null;

        var query = db.NhanViens.AsNoTracking()
            .Include(nv => nv.PhongBan)
            .Include(nv => nv.TaiKhoan)
            .AsQueryable();

        // Tìm kiếm theo: mã nhân viên, họ tên, email, số điện thoại, tên phòng ban hoặc tên đăng nhập
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var keyword = searchTerm.Trim();
            var keywordLower = keyword.ToLower();
            var maNvText = keyword.StartsWith("NV", StringComparison.OrdinalIgnoreCase)
                ? keyword[2..].Trim()
                : keyword.TrimStart('#').Trim();
            var laMaNv = int.TryParse(maNvText, out var maNv);

            query = query.Where(nv =>
                (laMaNv && nv.MaNhanVien == maNv) ||
                nv.MaNhanVien.ToString().Contains(keyword) ||
                nv.HoTen.ToLower().Contains(keywordLower) ||
                EF.Functions.Collate(nv.HoTen, "SQL_Latin1_General_CP1_CI_AI").Contains(keyword) ||
                nv.Email.ToLower().Contains(keywordLower) ||
                (nv.SoDienThoai != null && nv.SoDienThoai.ToLower().Contains(keywordLower)) ||
                nv.PhongBan.TenPhongBan.ToLower().Contains(keywordLower) ||
                EF.Functions.Collate(nv.PhongBan.TenPhongBan, "SQL_Latin1_General_CP1_CI_AI").Contains(keyword) ||
                (nv.TaiKhoan != null && nv.TaiKhoan.TenDangNhap.ToLower().Contains(keywordLower)));
        }

        // Lọc theo phòng ban và trạng thái
        if (maPhongBan.HasValue)
        {
            query = query.Where(nv => nv.MaPhongBan == maPhongBan.Value);
        }

        if (trangThai == "hoatdong")
        {
            query = query.Where(nv => nv.TrangThai);
        }
        else if (trangThai == "ngung")
        {
            query = query.Where(nv => !nv.TrangThai);
        }

        // Đếm tổng số lượng bản ghi để phân trang
        var total = await query.CountAsync();
        const int pageSize = 10;
        pageIndex = Math.Clamp(pageIndex, 1, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));

        // Sắp xếp dữ liệu
        var sorted = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(nv => nv.HoTen),
            "date_asc" => query.OrderBy(nv => nv.NgayVaoLam),
            "date_desc" => query.OrderByDescending(nv => nv.NgayVaoLam),
            _ => query.OrderBy(nv => nv.HoTen)
        };

        // Phân trang bằng Skip/Take kết hợp ThenBy khóa chính để đảm bảo ổn định
        var rows = await sorted.ThenBy(nv => nv.MaNhanVien)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(new NhanVienListViewModel
        {
            NhanViens = rows,
            SearchTerm = searchTerm,
            MaPhongBan = maPhongBan,
            TrangThai = trangThai,
            SortOrder = sortOrder ?? "name_asc",
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = total,
            PhongBans = await db.PhongBans.AsNoTracking()
                .OrderBy(pb => pb.TenPhongBan)
                .Select(pb => new SelectListItem(pb.TenPhongBan, pb.MaPhongBan.ToString()))
                .ToListAsync()
        });
    }

    /// <summary>
    /// Xuất danh sách nhân sự ra file Excel (.xlsx).
    /// Giữ nguyên bộ lọc và từ khóa tìm kiếm đang áp dụng trên giao diện.
    /// File xuất có cấu trúc chuẩn thống nhất và có thể import ngược trở lại hệ thống.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportExcel(string? searchTerm, int? maPhongBan, string? trangThai, string sortOrder = "name_asc")
    {
        searchTerm = searchTerm?.Trim();
        if (trangThai is not ("hoatdong" or "ngung")) trangThai = null;
        if (!NhanVienListViewModel.SortOptions.ContainsKey(sortOrder ?? "")) sortOrder = "name_asc";
        if (maPhongBan <= 0) maPhongBan = null;

        var query = db.NhanViens.AsNoTracking()
            .Include(nv => nv.PhongBan)
            .Include(nv => nv.TaiKhoan)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var keyword = searchTerm.Trim();
            var keywordLower = keyword.ToLower();
            var maNvText = keyword.StartsWith("NV", StringComparison.OrdinalIgnoreCase)
                ? keyword[2..].Trim()
                : keyword.TrimStart('#').Trim();
            var laMaNv = int.TryParse(maNvText, out var maNv);

            query = query.Where(nv =>
                (laMaNv && nv.MaNhanVien == maNv) ||
                nv.MaNhanVien.ToString().Contains(keyword) ||
                nv.HoTen.ToLower().Contains(keywordLower) ||
                EF.Functions.Collate(nv.HoTen, "SQL_Latin1_General_CP1_CI_AI").Contains(keyword) ||
                nv.Email.ToLower().Contains(keywordLower) ||
                (nv.SoDienThoai != null && nv.SoDienThoai.ToLower().Contains(keywordLower)) ||
                nv.PhongBan.TenPhongBan.ToLower().Contains(keywordLower) ||
                EF.Functions.Collate(nv.PhongBan.TenPhongBan, "SQL_Latin1_General_CP1_CI_AI").Contains(keyword) ||
                (nv.TaiKhoan != null && nv.TaiKhoan.TenDangNhap.ToLower().Contains(keywordLower)));
        }

        if (maPhongBan.HasValue)
        {
            query = query.Where(nv => nv.MaPhongBan == maPhongBan.Value);
        }

        if (trangThai == "hoatdong")
        {
            query = query.Where(nv => nv.TrangThai);
        }
        else if (trangThai == "ngung")
        {
            query = query.Where(nv => !nv.TrangThai);
        }

        var sorted = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(nv => nv.HoTen),
            "date_asc" => query.OrderBy(nv => nv.NgayVaoLam),
            "date_desc" => query.OrderByDescending(nv => nv.NgayVaoLam),
            _ => query.OrderBy(nv => nv.HoTen)
        };

        var rows = await sorted.ThenBy(nv => nv.MaNhanVien).ToListAsync();

        var pbName = maPhongBan.HasValue
            ? (await db.PhongBans.FindAsync(maPhongBan.Value))?.TenPhongBan ?? $"Mã {maPhongBan}"
            : "Tất cả phòng ban";
        var ttName = trangThai == "hoatdong" ? "Đang làm việc" : trangThai == "ngung" ? "Ngừng hoạt động" : "Tất cả trạng thái";
        var dieuKien = $"Từ khóa: {(string.IsNullOrEmpty(searchTerm) ? "Tất cả" : searchTerm)} | Phòng ban: {pbName} | Trạng thái: {ttName}";

        var nguoiXuat = HttpContext.Session.GetString("HoTen") ?? "Quản trị viên";
        var bytes = NhanVienExcelService.ExportNhanVien(rows, dieuKien, nguoiXuat);
        var fileName = $"DanhSachNhanSu_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(bytes, NhanVienExcelService.ExcelContentType, fileName);
    }

    /// <summary>
    /// Tải file Excel mẫu chuẩn (.xlsx) để nhập nhân sự.
    /// Bao gồm sheet dữ liệu mẫu, sheet tra cứu mã phòng ban và sheet hướng dẫn.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DownloadTemplate()
    {
        var phongBans = await db.PhongBans.AsNoTracking().OrderBy(p => p.TenPhongBan).ToListAsync();
        var bytes = NhanVienExcelService.TaoFileMau(phongBans);
        return File(bytes, NhanVienExcelService.ExcelContentType, "Mau_Import_NhanSu.xlsx");
    }

    /// <summary>
    /// Màn hình nhập danh sách nhân sự từ Excel.
    /// </summary>
    [HttpGet]
    public IActionResult Import()
    {
        return View((NhanVienImportPreviewResult?)null);
    }

    /// <summary>
    /// Tiếp nhận file Excel upload, kiểm tra cấu trúc, kiểm tra dữ liệu và hiển thị bảng xem trước (Preview).
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadExcel(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn một file Excel (.xlsx) để tải lên.";
            return View("Import", (NhanVienImportPreviewResult?)null);
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Hệ thống chỉ hỗ trợ file Excel có định dạng .xlsx.";
            return View("Import", (NhanVienImportPreviewResult?)null);
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        ms.Position = 0;

        var preview = await NhanVienExcelService.KiemTraVaXemTruocAsync(ms, file.FileName, db);

        if (preview.HasValidRows)
        {
            var token = Guid.NewGuid().ToString("N");
            var validRows = preview.Rows.Where(r => r.IsValid).ToList();
            var json = System.Text.Json.JsonSerializer.Serialize(validRows);
            HttpContext.Session.SetString($"ImportValidRows_{token}", json);
            preview.SessionToken = token;
        }

        return View("Import", preview);
    }

    /// <summary>
    /// Admin/HR xác nhận nhập các dòng hợp lệ vào cơ sở dữ liệu.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmImport(string sessionToken)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            TempData["Error"] = "Mã phiên làm việc không hợp lệ.";
            return RedirectToAction(nameof(Import));
        }

        var sessionKey = $"ImportValidRows_{sessionToken}";
        var json = HttpContext.Session.GetString(sessionKey);
        if (string.IsNullOrEmpty(json))
        {
            TempData["Error"] = "Phiên làm việc nhập Excel đã hết hạn hoặc đã được xác nhận trước đó. Vui lòng tải lại file.";
            return RedirectToAction(nameof(Import));
        }

        List<NhanVienImportRowDto>? validRows;
        try
        {
            validRows = System.Text.Json.JsonSerializer.Deserialize<List<NhanVienImportRowDto>>(json);
        }
        catch
        {
            TempData["Error"] = "Không thể giải mã dữ liệu xem trước.";
            return RedirectToAction(nameof(Import));
        }

        if (validRows == null || validRows.Count == 0)
        {
            TempData["Error"] = "Không có dòng dữ liệu hợp lệ nào để nhập.";
            return RedirectToAction(nameof(Import));
        }

        try
        {
            var (successCount, skipped) = await NhanVienExcelService.LuuImportAsync(validRows, db);
            HttpContext.Session.Remove(sessionKey);

            var thongBao = $"Đã nhập thành công {successCount} hồ sơ nhân sự vào hệ thống.";
            if (skipped.Count > 0)
            {
                thongBao += $" Đã bỏ qua {skipped.Count} dòng do dữ liệu trùng: {string.Join("; ", skipped)}";
            }

            TempData["Success"] = thongBao;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Quá trình lưu dữ liệu thất bại do lỗi hệ thống: {ex.Message}. Toàn bộ giao dịch đã được hoàn tác.";
            return RedirectToAction(nameof(Import));
        }
    }

    /// <summary>
    /// Xem chi tiết hồ sơ nhân viên, thông tin tài khoản và số liệu đào tạo tham gia.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await db.NhanViens.AsNoTracking()
            .Include(nv => nv.TaiKhoan)
            .Include(nv => nv.PhongBan)
            .Include(nv => nv.DangKyDaoTaos)
            .Include(nv => nv.DiemDanhs)
            .SingleOrDefaultAsync(nv => nv.MaNhanVien == id);

        if (entity is null) return NotFound();

        var model = new NhanVienViewModel
        {
            MaNhanVien = entity.MaNhanVien,
            MaTaiKhoan = entity.MaTaiKhoan,
            HoTen = entity.HoTen,
            MaPhongBan = entity.MaPhongBan,
            NgaySinh = entity.NgaySinh,
            SoDienThoai = entity.SoDienThoai,
            Email = entity.Email,
            NgayVaoLam = entity.NgayVaoLam,
            TrangThai = entity.TrangThai,
            TenDangNhap = entity.TaiKhoan?.TenDangNhap ?? string.Empty,
            VaiTroTaiKhoan = entity.TaiKhoan?.VaiTro ?? string.Empty,
            TenPhongBan = entity.PhongBan.TenPhongBan,
            SoKhoaDaDangKy = entity.DangKyDaoTaos.Count,
            SoBuoiDaDiemDanh = entity.DiemDanhs.Count
        };

        return View(model);
    }

    /// <summary>
    /// Hiển thị form tạo hồ sơ nhân viên mới.
    /// Chỉ cho phép liên kết tài khoản đã được cấp nhưng chưa gán cho nhân viên khác.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new NhanVienViewModel
        {
            NgaySinh = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7).AddYears(-22)),
            NgayVaoLam = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7))
        };
        await NapDanhSachChon(model);
        return View(model);
    }

    /// <summary>
    /// Lưu hồ sơ nhân viên mới vào database.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NhanVienViewModel model)
    {
        await XuLyDuLieuNhapGiaoDien(model);
        await KiemTraDuLieu(model);

        if (ModelState.IsValid)
        {
            var entity = new NhanVien
            {
                MaTaiKhoan = model.MaTaiKhoan,
                HoTen = model.HoTen.Trim(),
                MaPhongBan = model.MaPhongBan,
                NgaySinh = model.NgaySinh,
                SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim(),
                Email = model.Email.Trim(),
                NgayVaoLam = model.NgayVaoLam,
                TrangThai = model.TrangThai
            };

            db.NhanViens.Add(entity);
            try
            {
                await db.SaveChangesAsync();
                TempData["Success"] = $"Đã tạo hồ sơ nhân viên cho {entity.HoTen} thành công.";
                return RedirectToAction(nameof(Details), new { id = entity.MaNhanVien });
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu hồ sơ nhân viên do dữ liệu bị trùng hoặc vi phạm ràng buộc hệ thống.");
            }
        }

        await NapDanhSachChon(model);
        return View(model);
    }

    /// <summary>
    /// Hiển thị form chỉnh sửa hồ sơ nhân viên.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await db.NhanViens
            .Include(nv => nv.TaiKhoan)
            .Include(nv => nv.PhongBan)
            .SingleOrDefaultAsync(nv => nv.MaNhanVien == id);
        if (entity is null) return NotFound();

        var model = new NhanVienViewModel
        {
            MaNhanVien = entity.MaNhanVien,
            MaTaiKhoan = entity.MaTaiKhoan,
            HoTen = entity.HoTen,
            MaPhongBan = entity.MaPhongBan,
            NgaySinh = entity.NgaySinh,
            SoDienThoai = entity.SoDienThoai,
            Email = entity.Email,
            NgayVaoLam = entity.NgayVaoLam,
            TrangThai = entity.TrangThai,
            TenDangNhap = entity.TaiKhoan?.TenDangNhap ?? string.Empty,
            TenDangNhapInput = entity.TaiKhoan?.TenDangNhap ?? string.Empty,
            TenPhongBan = entity.PhongBan?.TenPhongBan ?? string.Empty,
            TenPhongBanInput = entity.PhongBan?.TenPhongBan ?? string.Empty
        };

        await NapDanhSachChon(model, currentMaTaiKhoan: entity.MaTaiKhoan);
        return View(model);
    }

    /// <summary>
    /// Lưu thông tin cập nhật hồ sơ nhân viên.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NhanVienViewModel model)
    {
        if (id != model.MaNhanVien) return BadRequest();

        var entity = await db.NhanViens
            .Include(nv => nv.TaiKhoan)
            .Include(nv => nv.PhongBan)
            .SingleOrDefaultAsync(nv => nv.MaNhanVien == id);
        if (entity is null) return NotFound();

        await XuLyDuLieuNhapGiaoDien(model);
        await KiemTraDuLieu(model, currentMaNhanVien: id, currentMaPhongBan: entity.MaPhongBan);

        if (ModelState.IsValid)
        {
            entity.MaTaiKhoan = model.MaTaiKhoan;
            entity.HoTen = model.HoTen.Trim();
            entity.MaPhongBan = model.MaPhongBan;
            entity.NgaySinh = model.NgaySinh;
            entity.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim();
            entity.Email = model.Email.Trim();
            entity.NgayVaoLam = model.NgayVaoLam;
            entity.TrangThai = model.TrangThai;

            try
            {
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật hồ sơ nhân viên thành công.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật hồ sơ nhân viên do dữ liệu bị trùng hoặc vi phạm ràng buộc.");
            }
        }

        model.TenDangNhap = entity.TaiKhoan?.TenDangNhap ?? string.Empty;
        model.TenPhongBan = entity.PhongBan?.TenPhongBan ?? string.Empty;
        await NapDanhSachChon(model, currentMaTaiKhoan: entity.MaTaiKhoan);
        return View(model);
    }

    /// <summary>
    /// Xóa hồ sơ nhân viên.
    /// Quy định đề bài: Xóa hồ sơ không xóa tài khoản.
    /// Nếu hồ sơ đã có đăng ký đào tạo hoặc điểm danh thì chỉ chuyển sang ngừng hoạt động.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.NhanViens.FindAsync(id);
        if (entity is null) return NotFound();

        // Kiểm tra xem nhân viên đã có dữ liệu đăng ký hoặc điểm danh hay chưa
        var coDangKy = await db.DangKyDaoTaos.AnyAsync(dk => dk.MaNhanVien == id);
        var coDiemDanh = await db.DiemDanhs.AnyAsync(dd => dd.MaNhanVien == id);

        if (coDangKy || coDiemDanh)
        {
            // Chuyển sang trạng thái ngừng hoạt động để bảo toàn lịch sử
            entity.TrangThai = false;
            await db.SaveChangesAsync();
            TempData["Success"] = "Hồ sơ nhân viên đã có dữ liệu đào tạo/điểm danh nên đã được chuyển sang trạng thái “Ngừng hoạt động” để giữ lịch sử.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Nếu chưa từng có dữ liệu liên quan thì tiến hành xóa bản ghi hồ sơ
        db.NhanViens.Remove(entity);
        try
        {
            await db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa hồ sơ nhân viên thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Không thể xóa hồ sơ nhân viên vì đang có dữ liệu tham chiếu.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    /// <summary>
    /// Nạp danh sách phòng ban hoạt động và tài khoản hợp lệ vào SelectList.
    /// </summary>
    private async Task NapDanhSachChon(NhanVienViewModel model, int? currentMaTaiKhoan = null)
    {
        // Chỉ chọn phòng ban đang hoạt động (hoặc phòng ban hiện tại của nhân viên)
        model.PhongBans = await db.PhongBans.AsNoTracking()
            .Where(pb => pb.TrangThai || pb.MaPhongBan == model.MaPhongBan)
            .OrderBy(pb => pb.TenPhongBan)
            .Select(pb => new SelectListItem(pb.TenPhongBan, pb.MaPhongBan.ToString()))
            .ToListAsync();

        // Tài khoản: chỉ lấy các tài khoản chưa liên kết với nhân viên nào (hoặc tài khoản hiện tại khi Sửa)
        var taiKhoanDaDung = await db.NhanViens.AsNoTracking()
            .Where(nv => !currentMaTaiKhoan.HasValue || nv.MaTaiKhoan != currentMaTaiKhoan.Value)
            .Select(nv => nv.MaTaiKhoan)
            .ToListAsync();

        model.TaiKhoans = await db.TaiKhoans.AsNoTracking()
            .Where(tk => !taiKhoanDaDung.Contains(tk.MaTaiKhoan))
            .OrderBy(tk => tk.TenDangNhap)
            .Select(tk => new SelectListItem(
                $"{tk.TenDangNhap} ({tk.HoTen} - {tk.VaiTro})",
                tk.MaTaiKhoan.ToString()))
            .ToListAsync();
    }

    /// <summary>
    /// Xử lý phân giải dữ liệu nếu người dùng gõ tên phòng ban hoặc tên tài khoản thay vì chọn từ dropdown.
    /// </summary>
    private async Task XuLyDuLieuNhapGiaoDien(NhanVienViewModel model)
    {
        // 1. Phân giải phòng ban nếu người dùng gõ tên hoặc mã
        if (!string.IsNullOrWhiteSpace(model.TenPhongBanInput))
        {
            var pbText = model.TenPhongBanInput.Trim();
            if (int.TryParse(pbText, out var parsedPbId))
            {
                model.MaPhongBan = parsedPbId;
            }
            else
            {
                var pbExact = await db.PhongBans.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.TenPhongBan.ToLower() == pbText.ToLower());
                if (pbExact != null)
                {
                    model.MaPhongBan = pbExact.MaPhongBan;
                }
                else
                {
                    var pbContains = await db.PhongBans.AsNoTracking()
                        .FirstOrDefaultAsync(p => EF.Functions.Collate(p.TenPhongBan, "SQL_Latin1_General_CP1_CI_AI").Contains(pbText));
                    model.MaPhongBan = pbContains?.MaPhongBan ?? 0;
                }
            }
        }
        else if (string.IsNullOrWhiteSpace(model.TenPhongBanInput) && model.MaPhongBan <= 0)
        {
            model.MaPhongBan = 0;
        }

        // 2. Phân giải tài khoản nếu người dùng gõ tên đăng nhập hoặc email (hoặc xóa trắng để hủy liên kết)
        if (string.IsNullOrWhiteSpace(model.TenDangNhapInput))
        {
            model.MaTaiKhoan = null;
        }
        else
        {
            var tkText = model.TenDangNhapInput.Trim();
            var spaceIdx = tkText.IndexOf(' ');
            var username = spaceIdx > 0 ? tkText[..spaceIdx].Trim() : tkText;

            if (int.TryParse(username, out var parsedTkId))
            {
                model.MaTaiKhoan = parsedTkId;
            }
            else
            {
                var tk = await db.TaiKhoans.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TenDangNhap.ToLower() == username.ToLower() || t.Email.ToLower() == username.ToLower());
                if (tk != null)
                {
                    model.MaTaiKhoan = tk.MaTaiKhoan;
                }
                else
                {
                    model.MaTaiKhoan = null;
                    ModelState.AddModelError(nameof(model.MaTaiKhoan), $"Không tìm thấy tài khoản có tên đăng nhập hoặc email '{username}'.");
                }
            }
        }
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của tài khoản liên kết, phòng ban và các thông tin hồ sơ.
    /// </summary>
    private async Task KiemTraDuLieu(NhanVienViewModel model, int? currentMaNhanVien = null, int? currentMaPhongBan = null)
    {
        // Kiểm tra mã nhân viên không bị trùng khi thêm mới
        if (!currentMaNhanVien.HasValue && model.MaNhanVien > 0 &&
            await db.NhanViens.AnyAsync(nv => nv.MaNhanVien == model.MaNhanVien))
        {
            ModelState.AddModelError(nameof(model.MaNhanVien), "Mã nhân viên đã tồn tại trong hệ thống.");
        }

        // Kiểm tra phòng ban có tồn tại và đang hoạt động
        if (model.MaPhongBan <= 0)
        {
            ModelState.AddModelError(nameof(model.MaPhongBan), "Vui lòng chọn hoặc gõ tên phòng ban hợp lệ.");
        }
        else
        {
            var phongBan = await db.PhongBans.AsNoTracking().SingleOrDefaultAsync(pb => pb.MaPhongBan == model.MaPhongBan);
            if (phongBan is null)
            {
                ModelState.AddModelError(nameof(model.MaPhongBan), "Phòng ban được chọn không tồn tại.");
            }
            else if (!phongBan.TrangThai && (!currentMaPhongBan.HasValue || phongBan.MaPhongBan != currentMaPhongBan.Value))
            {
                ModelState.AddModelError(nameof(model.MaPhongBan), "Phòng ban được chọn hiện đã ngừng hoạt động.");
            }
        }

        // Kiểm tra tài khoản nếu có chọn/nhập
        if (model.MaTaiKhoan.HasValue && model.MaTaiKhoan.Value > 0)
        {
            var taiKhoan = await db.TaiKhoans.FindAsync(model.MaTaiKhoan.Value);
            if (taiKhoan is null)
            {
                ModelState.AddModelError(nameof(model.MaTaiKhoan), "Tài khoản được chọn không tồn tại.");
            }
            else
            {
                // Kiểm tra tài khoản đã liên kết với nhân viên khác chưa
                var daGan = await db.NhanViens.AnyAsync(nv => nv.MaTaiKhoan == model.MaTaiKhoan.Value &&
                                                              (!currentMaNhanVien.HasValue || nv.MaNhanVien != currentMaNhanVien.Value));
                if (daGan)
                {
                    ModelState.AddModelError(nameof(model.MaTaiKhoan), "Tài khoản này đã được liên kết với một hồ sơ nhân viên khác.");
                }
            }
        }

        // Kiểm tra email không bị trùng với hồ sơ nhân viên khác
        var email = model.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var trungEmail = await db.NhanViens.AnyAsync(nv =>
                nv.Email.ToLower() == email.ToLower() &&
                (!currentMaNhanVien.HasValue || nv.MaNhanVien != currentMaNhanVien.Value));
            if (trungEmail)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng cho một hồ sơ nhân viên khác.");
            }
        }

        // Kiểm tra số điện thoại không bị trùng với hồ sơ nhân viên khác (nếu có nhập)
        var sdt = model.SoDienThoai?.Trim();
        if (!string.IsNullOrWhiteSpace(sdt))
        {
            var trungSdt = await db.NhanViens.AnyAsync(nv =>
                nv.SoDienThoai != null && nv.SoDienThoai == sdt &&
                (!currentMaNhanVien.HasValue || nv.MaNhanVien != currentMaNhanVien.Value));
            if (trungSdt)
            {
                ModelState.AddModelError(nameof(model.SoDienThoai), "Số điện thoại này đã được sử dụng cho một hồ sơ nhân viên khác.");
            }
        }
    }
}
