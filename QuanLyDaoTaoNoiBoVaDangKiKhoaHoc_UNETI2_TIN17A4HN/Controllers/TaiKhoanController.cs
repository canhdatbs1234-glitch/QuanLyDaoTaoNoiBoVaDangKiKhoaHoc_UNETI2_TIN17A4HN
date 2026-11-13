//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Models;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
using QuanLyDaoTaoNoiBo.Services;

namespace QuanLyDaoTaoNoiBo.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TaiKhoanController(ApplicationDbContext db) : Controller
{
    // ===== Đăng nhập / Đăng xuất =====
    [AllowAnonymous, HttpGet]
    public IActionResult DangNhap()
    {
        if (HttpContext.Session.DaDangNhap())
            return RedirectToAction("Index", "Home");
        return View(new DangNhapViewModel());
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapViewModel model)
    {
        if (!ModelState.IsValid) return LoginError(model);

        var username = model.TenDangNhap.Trim();
        var account = await db.TaiKhoans.SingleOrDefaultAsync(x => x.TenDangNhap == username);

        var matKhauDung = account is not null && MatKhauHelper.KiemTra(account, model.MatKhau);

        if (account is null || !matKhauDung || !(account.VaiTro is "QuanTri" or "NhanVien" or "GiangVien"))
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            return LoginError(model);
        }
        if (!account.TrangThai)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
            return LoginError(model);
        }
        var employeeId = await db.NhanViens.AsNoTracking().Where(x => x.MaTaiKhoan == account.MaTaiKhoan)
            .Select(x => (int?)x.MaNhanVien).FirstOrDefaultAsync();

        HttpContext.Session.DangNhap(account.MaTaiKhoan, account.HoTen, account.VaiTro, employeeId);

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public IActionResult DangXuat()
    {
        HttpContext.Session.Clear();   // xóa sạch phiên làm việc
        return RedirectToAction(nameof(DangNhap));
    }
    [HttpGet]
    public IActionResult KhongCoQuyen()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View("KhongCoQuyen");
    }
    [HttpGet, SessionAuthorize(Roles = "QuanTri")]
    public async Task<IActionResult> Index(
        string? tuKhoa, string? vaiTro, string? trangThai,
        int page = 1, string? sort = "tenDangNhap_az")
    {
        const int pageSize = 10;
        var query = db.TaiKhoans.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            query = query.Where(x => x.TenDangNhap.Contains(k) || x.HoTen.Contains(k) || x.Email.Contains(k));
        }
        if (!string.IsNullOrEmpty(vaiTro) && (vaiTro is "QuanTri" or "NhanVien" or "GiangVien"))
            query = query.Where(x => x.VaiTro == vaiTro);
        if (trangThai == "hoatdong") query = query.Where(x => x.TrangThai);
        else if (trangThai == "khoa") query = query.Where(x => !x.TrangThai);

        var allowedSorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tenDangNhap_az", "tenDangNhap_za", "tenDangNhap_short", "tenDangNhap_long",
            "hoTen_az", "hoTen_za", "hoTen_short", "hoTen_long",
            "email_az", "email_za", "email_short", "email_long",
            "vaiTro_az", "vaiTro_za", "vaiTro_quanTriFirst", "vaiTro_giangVienFirst",
            "trangThai_az", "trangThai_za", "trangThai_hoatDongFirst", "trangThai_khoaFirst"
        };
        if (string.IsNullOrWhiteSpace(sort) || !allowedSorts.Contains(sort)) sort = "tenDangNhap_az";

        var totalCount = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        IOrderedQueryable<TaiKhoan> sorted = sort switch
        {
            "tenDangNhap_za" => query.OrderByDescending(x => x.TenDangNhap),
            "tenDangNhap_short" => query.OrderBy(x => x.TenDangNhap.Length).ThenBy(x => x.TenDangNhap),
            "tenDangNhap_long" => query.OrderByDescending(x => x.TenDangNhap.Length).ThenBy(x => x.TenDangNhap),
            "vaiTro_az" => query.OrderBy(x => x.VaiTro == "QuanTri" ? "Admin / Nhân sự - Đào tạo" : x.VaiTro == "GiangVien" ? "Giảng viên" : "Nhân viên").ThenBy(x => x.HoTen),
            "hoTen_az" => query.OrderBy(x => x.HoTen),
            "hoTen_za" => query.OrderByDescending(x => x.HoTen),
            "hoTen_short" => query.OrderBy(x => x.HoTen.Length).ThenBy(x => x.HoTen),
            "hoTen_long" => query.OrderByDescending(x => x.HoTen.Length).ThenBy(x => x.HoTen),
            "email_az" => query.OrderBy(x => x.Email),
            "email_za" => query.OrderByDescending(x => x.Email),
            "email_short" => query.OrderBy(x => x.Email.Length).ThenBy(x => x.Email),
            "email_long" => query.OrderByDescending(x => x.Email.Length).ThenBy(x => x.Email),
            "vaiTro_za" => query.OrderByDescending(x => x.VaiTro == "QuanTri" ? "Admin / Nhân sự - Đào tạo" : x.VaiTro == "GiangVien" ? "Giảng viên" : "Nhân viên").ThenBy(x => x.HoTen),
            "vaiTro_quanTriFirst" => query.OrderBy(x => x.VaiTro == "QuanTri" ? 0 : x.VaiTro == "NhanVien" ? 1 : 2).ThenBy(x => x.HoTen),
            "vaiTro_giangVienFirst" => query.OrderBy(x => x.VaiTro == "GiangVien" ? 0 : x.VaiTro == "NhanVien" ? 1 : 2).ThenBy(x => x.HoTen),
            "trangThai_az" => query.OrderBy(x => x.TrangThai ? "Hoạt động" : "Bị khóa").ThenBy(x => x.TenDangNhap),
            "trangThai_za" => query.OrderByDescending(x => x.TrangThai ? "Hoạt động" : "Bị khóa").ThenBy(x => x.TenDangNhap),
            "trangThai_hoatDongFirst" => query.OrderByDescending(x => x.TrangThai).ThenBy(x => x.TenDangNhap),
            "trangThai_khoaFirst" => query.OrderBy(x => x.TrangThai).ThenByDescending(x => x.TenDangNhap),
            _ => query.OrderBy(x => x.TenDangNhap)
        };

        ViewData["TuKhoa"] = tuKhoa;
        ViewData["VaiTro"] = vaiTro;
        ViewData["TrangThai"] = trangThai;
        ViewData["Sort"] = sort;
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalCount"] = totalCount;

        var items = await sorted.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return View(items);
    }

    [HttpGet, SessionAuthorize(Roles = "QuanTri")]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await db.TaiKhoans.AsNoTracking().Include(x => x.NhanVien).SingleOrDefaultAsync(x => x.MaTaiKhoan == id);
        if (entity is null) return NotFound();
        return View(entity);
    }

    [HttpGet, SessionAuthorize(Roles = "QuanTri")]
    public IActionResult Create() => View(new TaiKhoanViewModel());

    [HttpPost, ValidateAntiForgeryToken, SessionAuthorize(Roles = "QuanTri")]
    public async Task<IActionResult> Create(TaiKhoanViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MatKhau))
            ModelState.AddModelError(nameof(model.MatKhau), "Vui lòng nhập mật khẩu.");
        await ValidateUnique(model);
        if (!ModelState.IsValid) return View(model);

        var entity = new TaiKhoan
        {
            TenDangNhap = model.TenDangNhap.Trim(),
            HoTen = model.HoTen.Trim(),
            Email = model.Email.Trim(),
            VaiTro = model.VaiTro,
            TrangThai = model.TrangThai
        };
        entity.MatKhau = MatKhauHelper.Bam(entity, model.MatKhau!);
        db.TaiKhoans.Add(entity);
        if (!await Save()) return View(model);

        TempData["Success"] = "Đã thêm tài khoản.";
        return RedirectToAction(nameof(Details), new { id = entity.MaTaiKhoan });
    }

    [HttpGet, SessionAuthorize(Roles = "QuanTri")]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(x => x.MaTaiKhoan == id);
        if (e is null) return NotFound();
        return View(new TaiKhoanViewModel
        {
            MaTaiKhoan = e.MaTaiKhoan,
            TenDangNhap = e.TenDangNhap,
            HoTen = e.HoTen,
            Email = e.Email,
            VaiTro = e.VaiTro,
            TrangThai = e.TrangThai
        });
    }

    [HttpPost, ValidateAntiForgeryToken, SessionAuthorize(Roles = "QuanTri")]
    public async Task<IActionResult> Edit(int id, TaiKhoanViewModel model)
    {
        if (id != model.MaTaiKhoan) return BadRequest();
        var entity = await db.TaiKhoans.SingleOrDefaultAsync(x => x.MaTaiKhoan == id);
        if (entity is null) return NotFound();

        if (id == HttpContext.Session.GetMaTaiKhoan() && (model.VaiTro != entity.VaiTro || !model.TrangThai))
            ModelState.AddModelError(string.Empty, "Không thể tự khóa hoặc tự đổi vai trò của tài khoản đang đăng nhập.");
        if (model.VaiTro != "GiangVien" && await db.LopDaoTaos.AnyAsync(x => x.MaGiangVien == id))
            ModelState.AddModelError(nameof(model.VaiTro), "Tài khoản đang được phân công phụ trách lớp, không thể đổi khỏi vai trò Giảng viên.");
        await ValidateUnique(model);
        if (!ModelState.IsValid) return View(model);

        entity.TenDangNhap = model.TenDangNhap.Trim();
        entity.HoTen = model.HoTen.Trim();
        entity.Email = model.Email.Trim();
        entity.VaiTro = model.VaiTro;
        entity.TrangThai = model.TrangThai;
        if (!string.IsNullOrWhiteSpace(model.MatKhau)) entity.MatKhau = MatKhauHelper.Bam(entity, model.MatKhau);
        if (!await Save()) return View(model);

        TempData["Success"] = "Đã cập nhật tài khoản.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Không xóa tài khoản để giữ lịch sử đào tạo; chỉ khóa / mở khóa.
    [HttpPost, ValidateAntiForgeryToken, SessionAuthorize(Roles = "QuanTri")]
    public async Task<IActionResult> KhoaMo(int id)
    {
        var entity = await db.TaiKhoans.SingleOrDefaultAsync(x => x.MaTaiKhoan == id);
        if (entity is null) return NotFound();
        if (id == HttpContext.Session.GetMaTaiKhoan())
        {
            TempData["Error"] = "Không thể tự khóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Details), new { id });
        }

        entity.TrangThai = !entity.TrangThai;
        try
        {
            await db.SaveChangesAsync();
            TempData["Success"] = entity.TrangThai
                ? "Đã mở khóa tài khoản."
                : "Đã khóa tài khoản; tài khoản này không thể đăng nhập.";
        }
        catch (DbUpdateException) { TempData["Error"] = "Không thể cập nhật trạng thái tài khoản."; }
        return RedirectToAction(nameof(Details), new { id });
    }

    // ===== Hàm hỗ trợ =====
    private async Task ValidateUnique(TaiKhoanViewModel model)
    {
        var ten = model.TenDangNhap?.Trim() ?? "";
        var mail = model.Email?.Trim() ?? "";
        if (ten.Length > 0 && await db.TaiKhoans.AnyAsync(x => x.TenDangNhap == ten && x.MaTaiKhoan != model.MaTaiKhoan))
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");
        if (mail.Length > 0 && await db.TaiKhoans.AnyAsync(x => x.Email == mail && x.MaTaiKhoan != model.MaTaiKhoan))
            ModelState.AddModelError(nameof(model.Email), "Email đã được dùng cho tài khoản khác.");
    }

    private async Task<bool> Save()
    {
        try { await db.SaveChangesAsync(); return true; }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Không thể lưu: tên đăng nhập/email đã tồn tại hoặc dữ liệu vừa thay đổi.");
            return false;
        }
    }

    private IActionResult LoginError(DangNhapViewModel model)
    {
        model.MatKhau = string.Empty;
        if (ModelState.TryGetValue(nameof(model.MatKhau), out var entry))
        {
            entry.RawValue = null;
            entry.AttemptedValue = null;
        }
        return View("DangNhap", model);
    }
}
