//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Filters;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;

namespace QuanLyDaoTaoNoiBo.Controllers;

[SessionRoles("QuanTri", "QuanLy")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class LinhVucDaoTaoController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa, string? trangThai)
    {
        var query = db.LinhVucDaoTaos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            query = query.Where(x => x.TenLinhVuc.Contains(k) || (x.MoTa != null && x.MoTa.Contains(k)));
        }
        if (trangThai == "hoatdong") query = query.Where(x => x.TrangThai);
        else if (trangThai == "ngung") query = query.Where(x => !x.TrangThai);
        ViewData["TuKhoa"] = tuKhoa; ViewData["TrangThai"] = trangThai;
        return View(await query.OrderBy(x => x.TenLinhVuc).ToListAsync());
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
            TempData["Error"] = "";
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
