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
public class PhongBanController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa, string? trangThai, int page = 1, string? sort = "tenPhongBan_az")
    {
        const int pageSize = 10;
        var query = db.PhongBans.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            query = query.Where(x => x.TenPhongBan.Contains(k) || (x.MoTa != null && x.MoTa.Contains(k)));
        }
        if (trangThai == "hoatdong") query = query.Where(x => x.TrangThai);
        else if (trangThai == "ngung") query = query.Where(x => !x.TrangThai);

        var allowedSorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tenPhongBan_az", "tenPhongBan_za", "tenPhongBan_short", "tenPhongBan_long",
            "moTa_az", "moTa_za", "moTa_short", "moTa_long",
            "trangThai_az", "trangThai_za", "trangThai_hoatDongFirst", "trangThai_ngungFirst"
        };
        if (string.IsNullOrWhiteSpace(sort) || !allowedSorts.Contains(sort)) sort = "tenPhongBan_az";

        var totalCount = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        IOrderedQueryable<PhongBan> sorted = sort switch
        {
            "tenPhongBan_za" => query.OrderByDescending(x => x.TenPhongBan),
            "tenPhongBan_short" => query.OrderBy(x => x.TenPhongBan.Length).ThenBy(x => x.TenPhongBan),
            "tenPhongBan_long" => query.OrderByDescending(x => x.TenPhongBan.Length).ThenBy(x => x.TenPhongBan),
            "moTa_az" => query.OrderBy(x => x.MoTa ?? string.Empty),
            "moTa_za" => query.OrderByDescending(x => x.MoTa ?? string.Empty),
            "moTa_short" => query.OrderBy(x => x.MoTa == null ? 0 : x.MoTa.Length).ThenBy(x => x.MoTa ?? string.Empty),
            "moTa_long" => query.OrderByDescending(x => x.MoTa == null ? 0 : x.MoTa.Length).ThenBy(x => x.MoTa ?? string.Empty),
            "trangThai_az" => query.OrderBy(x => x.TrangThai ? "Hoạt động" : "Ngừng hoạt động").ThenBy(x => x.TenPhongBan),
            "trangThai_za" => query.OrderByDescending(x => x.TrangThai ? "Hoạt động" : "Ngừng hoạt động").ThenBy(x => x.TenPhongBan),
            "trangThai_hoatDongFirst" => query.OrderByDescending(x => x.TrangThai).ThenBy(x => x.TenPhongBan),
            "trangThai_ngungFirst" => query.OrderBy(x => x.TrangThai).ThenByDescending(x => x.TenPhongBan),
            _ => query.OrderBy(x => x.TenPhongBan)
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
        var entity = await db.PhongBans.AsNoTracking().SingleOrDefaultAsync(x => x.MaPhongBan == id);
        if (entity is null) return NotFound();
        ViewData["SoLuongLienQuan"] = await db.NhanViens.CountAsync(x => x.MaPhongBan == id);
        return View(entity);
    }

    [HttpGet]
    public IActionResult Create() => View(new PhongBanViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PhongBanViewModel model)
    {
        await ValidateName(model);
        if (!ModelState.IsValid) return View(model);
        var entity = new PhongBan();
        Apply(model, entity);
        db.Add(entity);
        if (!await Save()) return View(model);
        TempData["Success"] = "Đã thêm phòng ban.";
        return RedirectToAction(nameof(Details), new { id = entity.MaPhongBan });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.PhongBans.AsNoTracking().SingleOrDefaultAsync(x => x.MaPhongBan == id);
        if (e is null) return NotFound();
        return View(new PhongBanViewModel { MaPhongBan = e.MaPhongBan, TenPhongBan = e.TenPhongBan, MoTa = e.MoTa, TrangThai = e.TrangThai });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PhongBanViewModel model)
    {
        if (id != model.MaPhongBan) return BadRequest();
        var entity = await db.PhongBans.SingleOrDefaultAsync(x => x.MaPhongBan == id);
        if (entity is null) return NotFound();
        await ValidateName(model);
        if (!ModelState.IsValid) return View(model);
        Apply(model, entity);
        if (!await Save()) return View(model);
        TempData["Success"] = "Đã cập nhật phòng ban.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Không xóa dữ liệu nếu làm mất tính toàn vẹn lịch sử: chỉ xóa khi chưa có dữ liệu liên quan.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.PhongBans.SingleOrDefaultAsync(x => x.MaPhongBan == id);
        if (entity is null) return NotFound();
        if (await db.NhanViens.AnyAsync(x => x.MaPhongBan == id))
            TempData["Error"] = "Không thể xóa phòng ban đã có nhân viên. Hãy bỏ chọn \"Đang hoạt động\" để ngừng sử dụng và giữ lịch sử.";
        else
        {
            db.Remove(entity);
            try
            {
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã xóa phòng ban.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException) { TempData["Error"] = "Không thể xóa: dữ liệu đã thay đổi hoặc đang được tham chiếu."; }
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task ValidateName(PhongBanViewModel model)
    {
        var ten = model.TenPhongBan?.Trim() ?? "";
        if (ten.Length > 0 && await db.PhongBans.AnyAsync(x => x.TenPhongBan == ten && x.MaPhongBan != model.MaPhongBan))
            ModelState.AddModelError(nameof(model.TenPhongBan), "Tên phòng ban đã tồn tại.");
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

    private static void Apply(PhongBanViewModel m, PhongBan e)
    {
        e.TenPhongBan = m.TenPhongBan.Trim();
        e.MoTa = string.IsNullOrWhiteSpace(m.MoTa) ? null : m.MoTa.Trim();
        e.TrangThai = m.TrangThai;
    }
}
