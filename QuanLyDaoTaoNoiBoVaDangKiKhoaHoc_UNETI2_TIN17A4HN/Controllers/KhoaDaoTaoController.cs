// Họ và tên: Trần Trung Hiếu
// Mã sinh viên: 23103100217
// Nội dung thực hiện: Module 2 - Kết quả đào tạo, Lịch sử đào tạo và Dashboard Thống kê

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Filters;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;

namespace QuanLyDaoTaoNoiBo.Controllers;

[SessionRoles("QuanTri", "QuanLy")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class KhoaDaoTaoController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() => View(await db.KhoaDaoTaos.AsNoTracking()
        .Include(x => x.LinhVucDaoTao).OrderBy(x => x.TenKhoaDaoTao).ThenBy(x => x.MaKhoaDaoTao).ToListAsync());

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

