//Họ và tên: Trần Trung Hiếu
//Mã sinh viên: 23103100217
//Module 2

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Services;
using Microsoft.AspNetCore.Authorization;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
namespace QuanLyDaoTaoNoiBo.Controllers;

[SessionAuthorize(Roles = "QuanTri")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class KhoaDaoTaoController(ApplicationDbContext db) : Controller
{
    private const int PageSize = 10;

    // Chuẩn hóa tham số (dùng chung cho Index, xuất Excel và xuất PDF).
    private static KhoaDaoTaoListViewModel ChuanHoa(string? searchTerm, int? maLinhVuc, string? trangThai, string? sortOrder)
    {
        searchTerm = searchTerm?.Trim();
        if (trangThai is not ("hoatdong" or "ngung")) trangThai = null;
        if (!KhoaDaoTaoListViewModel.SortOptions.ContainsKey(sortOrder ?? "")) sortOrder = "name_asc";
        if (maLinhVuc <= 0) maLinhVuc = null;
        return new KhoaDaoTaoListViewModel
        {
            SearchTerm = searchTerm, MaLinhVuc = maLinhVuc, TrangThai = trangThai, SortOrder = sortOrder!
        };
    }

    // Truy vấn -> Tìm kiếm -> Lọc (chưa sắp xếp, chưa phân trang).
    private IQueryable<KhoaDaoTao> BuildFilteredQuery(KhoaDaoTaoListViewModel f)
    {
        var query = db.KhoaDaoTaos.AsNoTracking().Include(x => x.LinhVucDaoTao).AsQueryable();
        // Tìm kiếm: tên khóa, tên lĩnh vực.
        if (!string.IsNullOrWhiteSpace(f.SearchTerm))
        {
            var searchTerm = f.SearchTerm;
            query = query.Where(x => x.TenKhoaDaoTao.Contains(searchTerm) ||
                x.LinhVucDaoTao.TenLinhVuc.Contains(searchTerm));
        }
        // Lọc.
        if (f.MaLinhVuc.HasValue) query = query.Where(x => x.MaLinhVuc == f.MaLinhVuc);
        if (f.TrangThai == "hoatdong") query = query.Where(x => x.TrangThai);
        if (f.TrangThai == "ngung") query = query.Where(x => !x.TrangThai);
        return query;
    }

    // Sắp xếp; ThenBy khóa chính để thứ tự ổn định.
    private static IOrderedQueryable<KhoaDaoTao> Sort(IQueryable<KhoaDaoTao> query, string sortOrder)
    {
        var sorted = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(x => x.TenKhoaDaoTao),
            "hours_asc" => query.OrderBy(x => x.SoGioDuKien),
            "hours_desc" => query.OrderByDescending(x => x.SoGioDuKien),
            _ => query.OrderBy(x => x.TenKhoaDaoTao)
        };
        return sorted.ThenBy(x => x.MaKhoaDaoTao);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, int? maLinhVuc, string? trangThai,
        string sortOrder = "name_asc", int pageIndex = 1)
    {
        var f = ChuanHoa(searchTerm, maLinhVuc, trangThai, sortOrder);
        var filtered = BuildFilteredQuery(f);

        var total = await filtered.CountAsync();
        pageIndex = Math.Clamp(pageIndex, 1, Math.Max(1, (int)Math.Ceiling(total / (double)PageSize)));
        // Phân trang bằng Skip/Take trong truy vấn SQL.
        var rows = await Sort(filtered, f.SortOrder).Skip((pageIndex - 1) * PageSize).Take(PageSize).ToListAsync();

        f.Khoas = rows;
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
    public async Task<IActionResult> ExportExcel(string? searchTerm, int? maLinhVuc, string? trangThai, string? sortOrder)
    {
        var (rows, dieuKien) = await LayDuLieuXuat(searchTerm, maLinhVuc, trangThai, sortOrder);
        var bytes = Module2ExportService.KhoaExcel(rows, dieuKien, HttpContext.Session.GetHoTen() ?? "");
        return File(bytes, Module2ExportService.ExcelContentType, $"DanhSachKhoaDaoTao_{LopDaoTaoRules.BayGioVietNam():yyyyMMdd_HHmm}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(string? searchTerm, int? maLinhVuc, string? trangThai, string? sortOrder)
    {
        var (rows, dieuKien) = await LayDuLieuXuat(searchTerm, maLinhVuc, trangThai, sortOrder);
        var bytes = Module2ExportService.KhoaPdf(rows, dieuKien, HttpContext.Session.GetHoTen() ?? "");
        return File(bytes, Module2ExportService.PdfContentType, $"DanhSachKhoaDaoTao_{LopDaoTaoRules.BayGioVietNam():yyyyMMdd_HHmm}.pdf");
    }

    private async Task<(List<KhoaDaoTao> Rows, string DieuKien)> LayDuLieuXuat(string? searchTerm, int? maLinhVuc,
        string? trangThai, string? sortOrder)
    {
        var f = ChuanHoa(searchTerm, maLinhVuc, trangThai, sortOrder);
        var rows = await Sort(BuildFilteredQuery(f), f.SortOrder).ToListAsync();

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(f.SearchTerm)) parts.Add($"Từ khóa “{f.SearchTerm}”");
        if (f.MaLinhVuc.HasValue)
        {
            var ten = await db.LinhVucDaoTaos.AsNoTracking().Where(x => x.MaLinhVuc == f.MaLinhVuc)
                .Select(x => x.TenLinhVuc).FirstOrDefaultAsync();
            parts.Add($"Lĩnh vực: {ten ?? f.MaLinhVuc.ToString()}");
        }
        if (f.TrangThai is not null) parts.Add(f.TrangThai == "hoatdong" ? "Trạng thái: Hoạt động" : "Trạng thái: Ngừng hoạt động");
        var dieuKien = (parts.Count == 0 ? "Tất cả khóa (không lọc)" : string.Join("; ", parts))
            + " · Sắp xếp: " + KhoaDaoTaoListViewModel.SortOptions[f.SortOrder];
        return (rows, dieuKien);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await db.KhoaDaoTaos.AsNoTracking().Include(x => x.LinhVucDaoTao)
            .Include(x => x.LopDaoTaos).SingleOrDefaultAsync(x => x.MaKhoaDaoTao == id);
        return entity is null ? NotFound() : View(entity);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new KhoaDaoTaoViewModel();
        await Populate(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KhoaDaoTaoViewModel model)
    {
        await ValidateField(model);
        if (ModelState.IsValid)
        {
            var entity = new KhoaDaoTao();
            Apply(model, entity);
            db.Add(entity);
            await db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm khóa đào tạo.";
            return RedirectToAction(nameof(Details), new { id = entity.MaKhoaDaoTao });
        }
        await Populate(model);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await db.KhoaDaoTaos.FindAsync(id);
        if (entity is null) return NotFound();
        var model = new KhoaDaoTaoViewModel
        {
            MaKhoaDaoTao = entity.MaKhoaDaoTao,
            TenKhoaDaoTao = entity.TenKhoaDaoTao,
            MaLinhVuc = entity.MaLinhVuc,
            MoTa = entity.MoTa,
            SoGioDuKien = entity.SoGioDuKien,
            DiemDatToiThieu = entity.DiemDatToiThieu,
            TyLeThamDuToiThieu = entity.TyLeThamDuToiThieu,
            TrangThai = entity.TrangThai
        };
        await Populate(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KhoaDaoTaoViewModel model)
    {
        if (id != model.MaKhoaDaoTao) return BadRequest();
        var entity = await db.KhoaDaoTaos.FindAsync(id);
        if (entity is null) return NotFound();
        await ValidateField(model);
        if (ModelState.IsValid)
        {
            Apply(model, entity);
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return NotFound(); }
            catch (KetQuaDaKhoaException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await Populate(model);
                return View(model);
            }
            TempData["Success"] = "Đã cập nhật khóa đào tạo.";
            return RedirectToAction(nameof(Details), new { id });
        }
        await Populate(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.KhoaDaoTaos.FindAsync(id);
        if (entity is null) return NotFound();
        if (await db.LopDaoTaos.AnyAsync(x => x.MaKhoaDaoTao == id))
            TempData["Error"] = "Không thể xóa khóa đã có lớp. Hãy chuyển khóa sang ngừng hoạt động.";
        else
        {
            db.Remove(entity);
            try
            {
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã xóa khóa đào tạo.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException) { TempData["Error"] = "Không thể xóa: dữ liệu đã thay đổi hoặc đang được tham chiếu."; }
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task ValidateField(KhoaDaoTaoViewModel model)
    {
        if (!await db.LinhVucDaoTaos.AnyAsync(x => x.MaLinhVuc == model.MaLinhVuc))
            ModelState.AddModelError(nameof(model.MaLinhVuc), "Lĩnh vực không tồn tại.");
    }

    private async Task Populate(KhoaDaoTaoViewModel model) =>
        model.LinhVucs = await db.LinhVucDaoTaos.AsNoTracking().OrderBy(x => x.TenLinhVuc)
            .Select(x => new SelectListItem(x.TenLinhVuc, x.MaLinhVuc.ToString())).ToListAsync();

    private static void Apply(KhoaDaoTaoViewModel m, KhoaDaoTao e)
    {
        e.TenKhoaDaoTao = m.TenKhoaDaoTao.Trim(); e.MaLinhVuc = m.MaLinhVuc; e.MoTa = m.MoTa;
        e.SoGioDuKien = m.SoGioDuKien; e.DiemDatToiThieu = m.DiemDatToiThieu;
        e.TyLeThamDuToiThieu = m.TyLeThamDuToiThieu; e.TrangThai = m.TrangThai;
    }
}

