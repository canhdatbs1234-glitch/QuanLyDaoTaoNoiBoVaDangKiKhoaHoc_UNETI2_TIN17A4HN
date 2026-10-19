//Họ và tên: Trần Trung Hiếu
//Mã sinh viên: 23103100217
//Module 2


using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Filters;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
using QuanLyDaoTaoNoiBo.Services;

namespace QuanLyDaoTaoNoiBo.Controllers;

[SessionRoles("QuanTri", "QuanLy")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class LopDaoTaoController(ApplicationDbContext db) : Controller
{
    // "DaDuyet" là giá trị trong DB hiện tại; nhãn tiếng Việt tương ứng là "Đã duyệt".
    private const string Approved = "DaDuyet";

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, int? maLinhVuc, string? trangThai,
        string? tinhTrangCho, string? tinhTrangHan, DateOnly? tuNgay, DateOnly? denNgay,
        string sortOrder = "name_asc", int pageIndex = 1)
    {
        searchTerm = searchTerm?.Trim();
        if (!LopDaoTaoViewModel.TrangThais.ContainsKey(trangThai ?? "")) trangThai = null;
        if (tinhTrangCho is not ("con" or "het")) tinhTrangCho = null;
        if (tinhTrangHan is not ("con" or "het")) tinhTrangHan = null;
        if (!LopDaoTaoListViewModel.SortOptions.ContainsKey(sortOrder ?? "")) sortOrder = "name_asc";
        if (maLinhVuc <= 0) maLinhVuc = null;
        // Nếu người dùng nhập ngược khoảng ngày thì tự đổi chỗ để vẫn ra kết quả hợp lý.
        if (tuNgay.HasValue && denNgay.HasValue && tuNgay > denNgay) (tuNgay, denNgay) = (denNgay, tuNgay);

        var now = LopDaoTaoRules.BayGioVietNam();
        var query = db.LopDaoTaos.AsNoTracking().AsQueryable();
        // Tìm kiếm: tên khóa, tên lớp, tên lĩnh vực, địa điểm.
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(x => x.TenLop.Contains(searchTerm) ||
                x.KhoaDaoTao.TenKhoaDaoTao.Contains(searchTerm) ||
                x.KhoaDaoTao.LinhVucDaoTao.TenLinhVuc.Contains(searchTerm) ||
                x.DiaDiem.Contains(searchTerm));
        // Lọc.
        if (maLinhVuc.HasValue) query = query.Where(x => x.KhoaDaoTao.MaLinhVuc == maLinhVuc);
        if (trangThai is not null) query = query.Where(x => x.TrangThai == trangThai);
        if (tuNgay.HasValue) query = query.Where(x => x.NgayBatDau >= tuNgay.Value);
        if (denNgay.HasValue) query = query.Where(x => x.NgayBatDau <= denNgay.Value);
        if (tinhTrangCho == "con")
            query = query.Where(x => x.SoLuongToiDa > x.DangKyDaoTaos.Count(d => d.TrangThai == Approved));
        if (tinhTrangCho == "het")
            query = query.Where(x => x.SoLuongToiDa <= x.DangKyDaoTaos.Count(d => d.TrangThai == Approved));
        if (tinhTrangHan == "con") query = query.Where(x => x.HanDangKy >= now);
        if (tinhTrangHan == "het") query = query.Where(x => x.HanDangKy < now);

        var total = await query.CountAsync();
        const int pageSize = 10;
        pageIndex = Math.Clamp(pageIndex, 1, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));
        // Sắp xếp.
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
        // ThenBy khóa chính bảo đảm phân trang ổn định khi giá trị sắp xếp trùng nhau.
        // Phân trang bằng Skip/Take ngay trong truy vấn SQL (không tải hết dữ liệu).
        var rows = await Project(sorted.ThenBy(x => x.MaLop).Skip((pageIndex - 1) * pageSize).Take(pageSize)).ToListAsync();
        return View(new LopDaoTaoListViewModel
        {
            Lops = rows,
            SearchTerm = searchTerm,
            MaLinhVuc = maLinhVuc,
            TrangThai = trangThai,
            TinhTrangCho = tinhTrangCho,
            TinhTrangHan = tinhTrangHan,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            SortOrder = sortOrder,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = total,
            LinhVucs = await db.LinhVucDaoTaos.AsNoTracking().OrderBy(x => x.TenLinhVuc)
                .Select(x => new SelectListItem(x.TenLinhVuc, x.MaLinhVuc.ToString())).ToListAsync()
        });
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
        await ValidateCourse(model);
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

    private async Task ValidateCourse(LopDaoTaoViewModel model)
    {
        if (!await db.KhoaDaoTaos.AnyAsync(x => x.MaKhoaDaoTao == model.MaKhoaDaoTao))
            ModelState.AddModelError(nameof(model.MaKhoaDaoTao), "Khóa đào tạo không tồn tại.");
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

