//Họ và tên: Trần Trung Hiếu
//Mã sinh viên: 23103100217
//Module 2


using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using Microsoft.AspNetCore.Authorization;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
using QuanLyDaoTaoNoiBo.Services;
namespace QuanLyDaoTaoNoiBo.Controllers;

[SessionAuthorize(Roles = "QuanTri")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class LopDaoTaoController(ApplicationDbContext db) : Controller
{
    // "DaDuyet" là giá trị trong DB hiện tại; nhãn tiếng Việt tương ứng là "Đã duyệt".
    private const string Approved = "DaDuyet";

    private const int PageSize = 10;

    // Chuẩn hóa tham số tìm kiếm/lọc/sắp xếp (dùng chung cho Index, xuất Excel và xuất PDF).
    private static LopDaoTaoListViewModel ChuanHoa(string? searchTerm, int? maLinhVuc, string? trangThai,
        string? tinhTrangCho, string? tinhTrangHan, DateOnly? tuNgay, DateOnly? denNgay, string? sortOrder)
    {
        searchTerm = searchTerm?.Trim();
        if (!LopDaoTaoViewModel.TrangThais.ContainsKey(trangThai ?? "")) trangThai = null;
        if (tinhTrangCho is not ("con" or "het")) tinhTrangCho = null;
        if (tinhTrangHan is not ("con" or "het")) tinhTrangHan = null;
        if (!LopDaoTaoListViewModel.SortOptions.ContainsKey(sortOrder ?? "")) sortOrder = "name_asc";
        if (maLinhVuc <= 0) maLinhVuc = null;
        // Nếu người dùng nhập ngược khoảng ngày thì tự đổi chỗ để vẫn ra kết quả hợp lý.
        if (tuNgay.HasValue && denNgay.HasValue && tuNgay > denNgay) (tuNgay, denNgay) = (denNgay, tuNgay);
        return new LopDaoTaoListViewModel
        {
            SearchTerm = searchTerm, MaLinhVuc = maLinhVuc, TrangThai = trangThai,
            TinhTrangCho = tinhTrangCho, TinhTrangHan = tinhTrangHan,
            TuNgay = tuNgay, DenNgay = denNgay, SortOrder = sortOrder!
        };
    }

    // Truy vấn -> Tìm kiếm -> Lọc (chưa sắp xếp, chưa phân trang).
    private IQueryable<LopDaoTao> BuildFilteredQuery(LopDaoTaoListViewModel f)
    {
        var now = LopDaoTaoRules.BayGioVietNam();
        var query = db.LopDaoTaos.AsNoTracking().AsQueryable();
        // Tìm kiếm: tên khóa, tên lớp, tên lĩnh vực, địa điểm.
        if (!string.IsNullOrWhiteSpace(f.SearchTerm))
        {
            var searchTerm = f.SearchTerm;
            query = query.Where(x => x.TenLop.Contains(searchTerm) ||
                x.KhoaDaoTao.TenKhoaDaoTao.Contains(searchTerm) ||
                x.KhoaDaoTao.LinhVucDaoTao.TenLinhVuc.Contains(searchTerm) ||
                x.DiaDiem.Contains(searchTerm));
        }
        // Lọc.
        if (f.MaLinhVuc.HasValue) query = query.Where(x => x.KhoaDaoTao.MaLinhVuc == f.MaLinhVuc);
        if (f.TrangThai is not null) query = query.Where(x => x.TrangThai == f.TrangThai);
        if (f.TuNgay.HasValue) query = query.Where(x => x.NgayBatDau >= f.TuNgay.Value);
        if (f.DenNgay.HasValue) query = query.Where(x => x.NgayBatDau <= f.DenNgay.Value);
        if (f.TinhTrangCho == "con")
            query = query.Where(x => x.SoLuongToiDa > x.DangKyDaoTaos.Count(d => d.TrangThai == Approved));
        if (f.TinhTrangCho == "het")
            query = query.Where(x => x.SoLuongToiDa <= x.DangKyDaoTaos.Count(d => d.TrangThai == Approved));
        if (f.TinhTrangHan == "con") query = query.Where(x => x.HanDangKy >= now);
        if (f.TinhTrangHan == "het") query = query.Where(x => x.HanDangKy < now);
        return query;
    }

    // Sắp xếp. ThenBy khóa chính bảo đảm thứ tự ổn định khi giá trị sắp xếp trùng nhau.
    private static IOrderedQueryable<LopDaoTao> Sort(IQueryable<LopDaoTao> query, string sortOrder)
    {
        var sorted = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(x => x.TenLop),
            "course_asc" => query.OrderBy(x => x.KhoaDaoTao.TenKhoaDaoTao),
            "course_desc" => query.OrderByDescending(x => x.KhoaDaoTao.TenKhoaDaoTao),
            "date_asc" => query.OrderBy(x => x.NgayBatDau),
            "date_desc" => query.OrderByDescending(x => x.NgayBatDau),
            "max_asc" => query.OrderBy(x => x.SoLuongToiDa),
            "max_desc" => query.OrderByDescending(x => x.SoLuongToiDa),
            "seats_asc" => query.OrderBy(x => x.SoLuongToiDa - x.DangKyDaoTaos.Count(d => d.TrangThai == Approved)),
            "seats_desc" => query.OrderByDescending(x => x.SoLuongToiDa - x.DangKyDaoTaos.Count(d => d.TrangThai == Approved)),
            _ => query.OrderBy(x => x.TenLop)
        };
        return sorted.ThenBy(x => x.MaLop);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, int? maLinhVuc, string? trangThai,
        string? tinhTrangCho, string? tinhTrangHan, DateOnly? tuNgay, DateOnly? denNgay,
        string sortOrder = "name_asc", int pageIndex = 1)
    {
        var f = ChuanHoa(searchTerm, maLinhVuc, trangThai, tinhTrangCho, tinhTrangHan, tuNgay, denNgay, sortOrder);
        var filtered = BuildFilteredQuery(f);

        var total = await filtered.CountAsync();
        pageIndex = Math.Clamp(pageIndex, 1, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));
        // Phân trang bằng Skip/Take ngay trong truy vấn SQL (không tải hết dữ liệu).
        var rows = await Project(Sort(filtered, f.SortOrder).Skip((pageIndex - 1) * PageSize).Take(PageSize)).ToListAsync();

        f.Lops = rows;
        f.PageIndex = pageIndex;
        f.PageSize = PageSize;
        f.TotalCount = total;
        f.LinhVucs = await db.LinhVucDaoTaos.AsNoTracking().OrderBy(x => x.TenLinhVuc)
            .Select(x => new SelectListItem(x.TenLinhVuc, x.MaLinhVuc.ToString())).ToListAsync();
        return View(f);
    }

    // ===== Chức năng nâng cao: xuất Excel / PDF theo đúng điều kiện tìm kiếm - lọc - sắp xếp đang áp dụng =====
    // Dùng cùng truy vấn với Index nhưng KHÔNG Skip/Take: xuất toàn bộ kết quả, không chỉ trang đang xem.
    [HttpGet]
    public async Task<IActionResult> ExportExcel(string? searchTerm, int? maLinhVuc, string? trangThai,
        string? tinhTrangCho, string? tinhTrangHan, DateOnly? tuNgay, DateOnly? denNgay, string? sortOrder)
    {
        var (rows, dieuKien) = await LayDuLieuXuat(searchTerm, maLinhVuc, trangThai, tinhTrangCho, tinhTrangHan, tuNgay, denNgay, sortOrder);
        var bytes = Module2ExportService.LopExcel(rows, dieuKien, HttpContext.Session.GetHoTen() ?? "");
        return File(bytes, Module2ExportService.ExcelContentType, $"DanhSachLopDaoTao_{LopDaoTaoRules.BayGioVietNam():yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(string? searchTerm, int? maLinhVuc, string? trangThai,
        string? tinhTrangCho, string? tinhTrangHan, DateOnly? tuNgay, DateOnly? denNgay, string? sortOrder)
    {
        var (rows, dieuKien) = await LayDuLieuXuat(searchTerm, maLinhVuc, trangThai, tinhTrangCho, tinhTrangHan, tuNgay, denNgay, sortOrder);
        var bytes = Module2ExportService.LopPdf(rows, dieuKien, HttpContext.Session.GetHoTen() ?? "");
        return File(bytes, Module2ExportService.PdfContentType, $"DanhSachLopDaoTao_{LopDaoTaoRules.BayGioVietNam():yyyyMMdd_HHmm}.pdf");
    }

    private async Task<(List<LopDaoTaoViewModel> Rows, string DieuKien)> LayDuLieuXuat(string? searchTerm, int? maLinhVuc,
        string? trangThai, string? tinhTrangCho, string? tinhTrangHan, DateOnly? tuNgay, DateOnly? denNgay, string? sortOrder)
    {
        var f = ChuanHoa(searchTerm, maLinhVuc, trangThai, tinhTrangCho, tinhTrangHan, tuNgay, denNgay, sortOrder);
        var rows = await Project(Sort(BuildFilteredQuery(f), f.SortOrder)).ToListAsync();

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(f.SearchTerm)) parts.Add($"Từ khóa “{f.SearchTerm}”");
        if (f.MaLinhVuc.HasValue)
        {
            var ten = await db.LinhVucDaoTaos.AsNoTracking().Where(x => x.MaLinhVuc == f.MaLinhVuc)
                .Select(x => x.TenLinhVuc).FirstOrDefaultAsync();
            parts.Add($"Lĩnh vực: {ten ?? f.MaLinhVuc.ToString()}");
        }
        if (f.TrangThai is not null) parts.Add($"Trạng thái: {LopDaoTaoViewModel.TrangThais[f.TrangThai]}");
        if (f.TuNgay.HasValue) parts.Add($"Bắt đầu từ {f.TuNgay:dd/MM/yyyy}");
        if (f.DenNgay.HasValue) parts.Add($"Bắt đầu đến {f.DenNgay:dd/MM/yyyy}");
        if (f.TinhTrangCho is not null) parts.Add(f.TinhTrangCho == "con" ? "Còn chỗ" : "Hết chỗ");
        if (f.TinhTrangHan is not null) parts.Add(f.TinhTrangHan == "con" ? "Còn hạn đăng ký" : "Hết hạn đăng ký");
        var dieuKien = (parts.Count == 0 ? "Tất cả lớp (không lọc)" : string.Join("; ", parts))
            + " · Sắp xếp: " + LopDaoTaoListViewModel.SortOptions[f.SortOrder];
        return (rows, dieuKien);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await Project(db.LopDaoTaos.AsNoTracking().Where(x => x.MaLop == id)).SingleOrDefaultAsync();
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime);
        var model = new LopDaoTaoViewModel
        {
            NgayBatDau = today.AddDays(7),
            NgayKetThuc = today.AddDays(14),
            HanDangKy = today.AddDays(6).ToDateTime(new TimeOnly(23, 59)),
            SoLuongToiDa = 30
        };
        await Populate(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LopDaoTaoViewModel model)
    {
        await ValidateCourse(model);
        if (ModelState.IsValid)
        {
            var entity = new LopDaoTao();
            Apply(model, entity);
            db.Add(entity);
            await db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm lớp đào tạo.";
            return RedirectToAction(nameof(Details), new { id = entity.MaLop });
        }
        await Populate(model);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await Project(db.LopDaoTaos.AsNoTracking().Where(x => x.MaLop == id)).SingleOrDefaultAsync();
        if (model is null) return NotFound();
        await Populate(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LopDaoTaoViewModel model)
    {
        if (id != model.MaLop) return BadRequest();
        using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var entity = await db.LopDaoTaos.FindAsync(id);
        if (entity is null) return NotFound();
        await ValidateCourse(model, entity.MaKhoaDaoTao);
        var approved = await db.DangKyDaoTaos.CountAsync(x => x.MaLop == id && x.TrangThai == Approved);
        if (await db.BuoiHocs.AnyAsync(x => x.MaLop == id &&
            (x.NgayHoc < model.NgayBatDau || x.NgayHoc > model.NgayKetThuc)))
            ModelState.AddModelError(nameof(model.NgayKetThuc), "Khoảng ngày của lớp phải bao gồm các buổi học đã tạo.");
        if (model.SoLuongToiDa < approved)
            ModelState.AddModelError(nameof(model.SoLuongToiDa), $"Sức chứa không thể nhỏ hơn {approved} đăng ký đã duyệt.");
        if (ModelState.IsValid)
        {
            Apply(model, entity);
            try { await db.SaveChangesAsync(); await transaction.CommitAsync(); }
            catch (DbUpdateConcurrencyException) { return NotFound(); }
            catch (KetQuaDaKhoaException ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", ex.Message);
                await Populate(model);
                return View(model);
            }
            TempData["Success"] = "Đã cập nhật lớp đào tạo.";
            return RedirectToAction(nameof(Details), new { id });
        }
        await transaction.RollbackAsync();
        model.SoDaDuyet = approved;
        await Populate(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.LopDaoTaos.FindAsync(id);
        if (entity is null) return NotFound();
        if (await db.DangKyDaoTaos.AnyAsync(x => x.MaLop == id) || await db.BuoiHocs.AnyAsync(x => x.MaLop == id))
            TempData["Error"] = "Không thể xóa lớp đã có đăng ký hoặc buổi học. Hãy chuyển trạng thái sang Đã hủy.";
        else
        {
            db.Remove(entity);
            try
            {
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã xóa lớp đào tạo.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException) { TempData["Error"] = "Không thể xóa: dữ liệu đã thay đổi hoặc đang được tham chiếu."; }
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    // Khóa phải tồn tại; khi tạo lớp mới hoặc đổi sang khóa khác thì khóa phải đang hoạt động.
    private async Task ValidateCourse(LopDaoTaoViewModel model, int? khoaHienTai = null)
    {
        var khoa = await db.KhoaDaoTaos.AsNoTracking().Where(x => x.MaKhoaDaoTao == model.MaKhoaDaoTao)
            .Select(x => new { x.TrangThai }).SingleOrDefaultAsync();
        if (khoa is null)
            ModelState.AddModelError(nameof(model.MaKhoaDaoTao), "Khóa đào tạo không tồn tại.");
        else if (!khoa.TrangThai && khoaHienTai != model.MaKhoaDaoTao)
            ModelState.AddModelError(nameof(model.MaKhoaDaoTao), "Khóa đào tạo đang ngừng hoạt động, không thể tạo lớp mới.");
    }

    private async Task Populate(LopDaoTaoViewModel model)
    {
        model.KhoaDaoTaos = await db.KhoaDaoTaos.AsNoTracking().OrderBy(x => x.TenKhoaDaoTao)
            .Select(x => new SelectListItem(x.TenKhoaDaoTao, x.MaKhoaDaoTao.ToString())).ToListAsync();
        model.GiangViens = await db.TaiKhoans.AsNoTracking().Where(x => x.VaiTro == "GiangVien").OrderBy(x => x.HoTen)
            .Select(x => new SelectListItem(x.HoTen, x.MaTaiKhoan.ToString())).ToListAsync();
    }

    private static void Apply(LopDaoTaoViewModel m, LopDaoTao e)
    {
        e.MaKhoaDaoTao = m.MaKhoaDaoTao; e.TenLop = m.TenLop.Trim();
        e.NgayBatDau = m.NgayBatDau; e.NgayKetThuc = m.NgayKetThuc; e.HanDangKy = m.HanDangKy;
        e.SoLuongToiDa = m.SoLuongToiDa; e.DiaDiem = m.DiaDiem.Trim(); e.TrangThai = m.TrangThai;
        e.MaGiangVien = m.MaGiangVien;
    }

    private static IQueryable<LopDaoTaoViewModel> Project(IQueryable<LopDaoTao> query) =>
        query.Select(x => new LopDaoTaoViewModel
        {
            MaLop = x.MaLop,
            MaKhoaDaoTao = x.MaKhoaDaoTao,
            TenLop = x.TenLop,
            TenKhoaDaoTao = x.KhoaDaoTao.TenKhoaDaoTao,
            TenLinhVuc = x.KhoaDaoTao.LinhVucDaoTao.TenLinhVuc,
            NgayBatDau = x.NgayBatDau,
            NgayKetThuc = x.NgayKetThuc,
            HanDangKy = x.HanDangKy,
            SoLuongToiDa = x.SoLuongToiDa,
            DiaDiem = x.DiaDiem,
            TrangThai = x.TrangThai,
            SoDaDuyet = x.DangKyDaoTaos.Count(d => d.TrangThai == Approved),
            MaGiangVien = x.MaGiangVien,
            TenGiangVien = x.GiangVien != null ? x.GiangVien.HoTen : ""
        });
}

