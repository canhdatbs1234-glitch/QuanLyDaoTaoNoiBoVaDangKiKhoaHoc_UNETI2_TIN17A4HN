//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Services;
using Microsoft.AspNetCore.Authorization;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;

namespace QuanLyDaoTaoNoiBo.Controllers;

[SessionAuthorize(Roles = "QuanTri")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class LinhVucDaoTaoController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa, string? trangThai, int page = 1, string? sort = "tenLinhVuc_az")
    {
        const int pageSize = 10;
        var query = db.LinhVucDaoTaos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            query = query.Where(x => x.TenLinhVuc.Contains(k) || (x.MoTa != null && x.MoTa.Contains(k)));
        }
        if (trangThai == "hoatdong") query = query.Where(x => x.TrangThai);
        else if (trangThai == "ngung") query = query.Where(x => !x.TrangThai);

        var allowedSorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tenLinhVuc_az", "tenLinhVuc_za", "tenLinhVuc_short", "tenLinhVuc_long",
            "moTa_az", "moTa_za", "moTa_short", "moTa_long",
            "trangThai_az", "trangThai_za", "trangThai_hoatDongFirst", "trangThai_ngungFirst"
        };
        if (string.IsNullOrWhiteSpace(sort) || !allowedSorts.Contains(sort)) sort = "tenLinhVuc_az";

        var totalCount = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        IOrderedQueryable<LinhVucDaoTao> sorted = sort switch
        {
            "tenLinhVuc_za" => query.OrderByDescending(x => x.TenLinhVuc),
            "tenLinhVuc_short" => query.OrderBy(x => x.TenLinhVuc.Length).ThenBy(x => x.TenLinhVuc),
            "tenLinhVuc_long" => query.OrderByDescending(x => x.TenLinhVuc.Length).ThenBy(x => x.TenLinhVuc),
            "moTa_az" => query.OrderBy(x => x.MoTa ?? string.Empty),
            "moTa_za" => query.OrderByDescending(x => x.MoTa ?? string.Empty),
            "moTa_short" => query.OrderBy(x => x.MoTa == null ? 0 : x.MoTa.Length).ThenBy(x => x.MoTa ?? string.Empty),
            "moTa_long" => query.OrderByDescending(x => x.MoTa == null ? 0 : x.MoTa.Length).ThenBy(x => x.MoTa ?? string.Empty),
            "trangThai_az" => query.OrderBy(x => x.TrangThai ? "Hoạt động" : "Ngừng hoạt động").ThenBy(x => x.TenLinhVuc),
            "trangThai_za" => query.OrderByDescending(x => x.TrangThai ? "Hoạt động" : "Ngừng hoạt động").ThenBy(x => x.TenLinhVuc),
            "trangThai_hoatDongFirst" => query.OrderByDescending(x => x.TrangThai).ThenBy(x => x.TenLinhVuc),
            "trangThai_ngungFirst" => query.OrderBy(x => x.TrangThai).ThenByDescending(x => x.TenLinhVuc),
            _ => query.OrderBy(x => x.TenLinhVuc)
        };

        ViewData["TuKhoa"] = tuKhoa;
        ViewData["TrangThai"] = trangThai;
        ViewData["Sort"] = sort;
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalCount"] = totalCount;
        return View(await sorted.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await db.LinhVucDaoTaos.AsNoTracking().SingleOrDefaultAsync(x => x.MaLinhVuc == id);
        if (entity is null) return NotFound();
        ViewData["SoLuongLienQuan"] = await db.KhoaDaoTaos.CountAsync(x => x.MaLinhVuc == id);
        return View(entity);
    }
    [HttpGet]
    public IActionResult Create() => View(new LinhVucDaoTaoViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LinhVucDaoTaoViewModel model)
    {
        await ValidateName(model);
        if (!ModelState.IsValid) return View(model);
        var entity = new LinhVucDaoTao();
        Apply(model, entity);
        db.Add(entity);
        if (!await Save()) return View(model);
        TempData["Success"] = "Đã thêm lĩnh vực.";
        return RedirectToAction(nameof(Details), new { id = entity.MaLinhVuc });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.LinhVucDaoTaos.AsNoTracking().SingleOrDefaultAsync(x => x.MaLinhVuc == id);
        if (e is null) return NotFound();
        return View(new LinhVucDaoTaoViewModel { MaLinhVuc = e.MaLinhVuc, TenLinhVuc = e.TenLinhVuc, MoTa = e.MoTa, TrangThai = e.TrangThai });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LinhVucDaoTaoViewModel model)
    {
        if (id != model.MaLinhVuc) return BadRequest();
        var entity = await db.LinhVucDaoTaos.SingleOrDefaultAsync(x => x.MaLinhVuc == id);
        if (entity is null) return NotFound();
        await ValidateName(model);
        if (!ModelState.IsValid) return View(model);
        Apply(model, entity);
        if (!await Save()) return View(model);
        TempData["Success"] = "Đã cập nhật lĩnh vực.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Không xóa dữ liệu nếu làm mất tính toàn vẹn lịch sử: chỉ xóa khi chưa có dữ liệu liên quan.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.LinhVucDaoTaos.SingleOrDefaultAsync(x => x.MaLinhVuc == id);
        if (entity is null) return NotFound();
        if (await db.KhoaDaoTaos.AnyAsync(x => x.MaLinhVuc == id))
            TempData["Error"] = "Không thể xóa lĩnh vực đã có khóa đào tạo. Hãy bỏ chọn \"Đang hoạt động\" để ngừng sử dụng và giữ lịch sử.";
        else
        {
            db.Remove(entity);
            try
            {
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã xóa lĩnh vực.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException) { TempData["Error"] = "Không thể xóa: dữ liệu đã thay đổi hoặc đang được tham chiếu."; }
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task ValidateName(LinhVucDaoTaoViewModel model)
    {
        var ten = model.TenLinhVuc?.Trim() ?? "";
        if (ten.Length > 0 && await db.LinhVucDaoTaos.AnyAsync(x => x.TenLinhVuc == ten && x.MaLinhVuc != model.MaLinhVuc))
            ModelState.AddModelError(nameof(model.TenLinhVuc), "Tên lĩnh vực đã tồn tại.");
    }

    private async Task<bool> Save()
    {
        try { await db.SaveChangesAsync(); return true; }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể lưu: tên đã tồn tại hoặc dữ liệu vừa thay đổi.");
            return false;
        }
    }

    private static void Apply(LinhVucDaoTaoViewModel m, LinhVucDaoTao e)
    {
        e.TenLinhVuc = m.TenLinhVuc.Trim();
        e.MoTa = string.IsNullOrWhiteSpace(m.MoTa) ? null : m.MoTa.Trim();
        e.TrangThai = m.TrangThai;
    }
}
