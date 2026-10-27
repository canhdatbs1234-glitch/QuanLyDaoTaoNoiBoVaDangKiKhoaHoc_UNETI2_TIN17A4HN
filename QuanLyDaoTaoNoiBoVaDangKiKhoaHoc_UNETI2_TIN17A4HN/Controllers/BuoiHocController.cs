// Họ và tên: Phạm Việt Anh
// Mã sinh viên: 23103100231
// Nội dung thực hiện: Module 4 - Buổi học, Lịch học và Điểm danh

using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Filters;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;

namespace QuanLyDaoTaoNoiBo.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class BuoiHocController(ApplicationDbContext db, TimeProvider clock) : Controller
{
    private int AccountId => HttpContext.Session.GetInt32("MaTaiKhoan") ?? 0;
    private bool Manager => HttpContext.Session.GetString("VaiTro") is "QuanTri" or "QuanLy";

    [HttpGet, SessionRoles("QuanTri", "QuanLy", "GiangVien")]
    public Task<IActionResult> Index(int? maLop) => LichHoc(maLop);

    [HttpGet]
    public async Task<IActionResult> LichHoc(int? maLop)
    {
        var allowed = db.LopDaoTaos.AsNoTracking().AsQueryable();
        if (!Manager)
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro == "GiangVien")
                allowed = allowed.Where(x => x.MaGiangVien == AccountId);
            else
                allowed = allowed.Where(x => x.DangKyDaoTaos.Any(d => d.TrangThai == "DaDuyet" && d.NhanVien.MaTaiKhoan == AccountId));
        }
        if (maLop.HasValue && !await allowed.AnyAsync(x => x.MaLop == maLop.Value))
            return StatusCode(StatusCodes.Status403Forbidden);
        var lessons = db.BuoiHocs.AsNoTracking().Where(x => allowed.Any(l => l.MaLop == x.MaLop));
        if (maLop.HasValue) lessons = lessons.Where(x => x.MaLop == maLop.Value);
        var model = new LichHocViewModel
        {
            MaLop = maLop,
            Lops = await allowed.OrderBy(x => x.TenLop).Select(x => new SelectListItem(x.TenLop, x.MaLop.ToString())).ToListAsync(),
            BuoiHocs = await Project(lessons.OrderBy(x => x.NgayHoc).ThenBy(x => x.GioBatDau).ThenBy(x => x.MaBuoiHoc)).ToListAsync()
        };
        return View("LichHoc", model);
    }

    [HttpGet, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> Create(int? maLop)
    {
        var model = new BuoiHocViewModel { MaLop = maLop ?? 0, GioBatDau = new TimeOnly(8, 0), GioKetThuc = new TimeOnly(10, 0) };
        if (maLop.HasValue)
        {
            var lop = await db.LopDaoTaos.FindAsync(maLop.Value);
            if (lop is null) return NotFound();
            model.NgayHoc = lop.NgayBatDau; model.DiaDiem = lop.DiaDiem;
        }
        await Populate(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("QuanTri", "QuanLy")]
    public Task<IActionResult> Create(BuoiHocViewModel model) => Store(model, null);

    [HttpGet, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await Project(db.BuoiHocs.AsNoTracking().Where(x => x.MaBuoiHoc == id)).SingleOrDefaultAsync();
        if (model is null) return NotFound();
        await Populate(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("QuanTri", "QuanLy")]
    public Task<IActionResult> Edit(int id, BuoiHocViewModel model) =>
        id != model.MaBuoiHoc ? Task.FromResult<IActionResult>(BadRequest()) : Store(model, id);

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> Delete(int id)
    {
        var lesson = await db.BuoiHocs.FindAsync(id);
        if (lesson is null) return NotFound();
        if (await db.DiemDanhs.AnyAsync(x => x.MaBuoiHoc == id))
            TempData["Error"] = "Không thể xóa buổi đã điểm danh. Có thể sửa trạng thái thành Đã hủy.";
        else
        {
            db.Remove(lesson);
            try { await db.SaveChangesAsync(); TempData["Success"] = "Đã xóa buổi học."; }
            catch (DbUpdateException) { TempData["Error"] = "Buổi học đã được tham chiếu hoặc vừa thay đổi."; }
        }
        return RedirectToAction(nameof(Index), new { maLop = lesson.MaLop });
    }

    private async Task<IActionResult> Store(BuoiHocViewModel model, int? id)
    {
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var lesson = id.HasValue ? await db.BuoiHocs.FindAsync(id.Value) : new BuoiHoc();
            if (lesson is null) return NotFound();
            var lop = await db.LopDaoTaos.SingleOrDefaultAsync(x => x.MaLop == model.MaLop);
            if (lop is null) ModelState.AddModelError(nameof(model.MaLop), "Lớp không tồn tại.");
            else
            {
                if (lop.TrangThai == "DaHuy") ModelState.AddModelError(nameof(model.MaLop), "Không chỉnh lịch của lớp đã hủy.");
                if (!model.NgayHoc.HasValue || model.NgayHoc < lop.NgayBatDau || model.NgayHoc > lop.NgayKetThuc)
                    ModelState.AddModelError(nameof(model.NgayHoc), "Ngày học phải nằm trong khoảng ngày bắt đầu và kết thúc lớp.");
            }
            if (!model.GioBatDau.HasValue || !model.GioKetThuc.HasValue || model.GioKetThuc <= model.GioBatDau)
                ModelState.AddModelError(nameof(model.GioKetThuc), "Giờ kết thúc phải sau giờ bắt đầu.");
            if (model.NgayHoc.HasValue && model.GioKetThuc.HasValue && model.TrangThai == "DaDienRa" &&
                model.NgayHoc.Value.ToDateTime(model.GioKetThuc.Value) > clock.GetUtcNow().ToOffset(TimeSpan.FromHours(7)).DateTime)
                ModelState.AddModelError(nameof(model.TrangThai), "Buổi học chưa kết thúc, không thể đánh dấu đã diễn ra.");
            if (id.HasValue && await db.DiemDanhs.AnyAsync(x => x.MaBuoiHoc == id.Value) &&
                (lesson.MaLop != model.MaLop || lesson.NgayHoc != model.NgayHoc ||
                 lesson.GioBatDau != model.GioBatDau || lesson.GioKetThuc != model.GioKetThuc))
                ModelState.AddModelError("", "Buổi đã điểm danh không được chuyển lớp hoặc đổi ngày/giờ.");
            if (model.TrangThai != "DaHuy" && model.NgayHoc.HasValue && model.GioBatDau.HasValue && model.GioKetThuc.HasValue &&
                await db.BuoiHocs.AnyAsync(x => x.MaLop == model.MaLop && x.MaBuoiHoc != (id ?? 0) &&
                    x.TrangThai != "DaHuy" && x.NgayHoc == model.NgayHoc &&
                    x.GioBatDau < model.GioKetThuc && model.GioBatDau < x.GioKetThuc))
                ModelState.AddModelError(nameof(model.GioBatDau), "Trùng giờ với một buổi khác trong lớp.");
            if (ModelState.IsValid)
            {
                lesson.MaLop = model.MaLop; lesson.NgayHoc = model.NgayHoc!.Value;
                lesson.GioBatDau = model.GioBatDau!.Value; lesson.GioKetThuc = model.GioKetThuc!.Value;
                lesson.NoiDung = model.NoiDung.Trim(); lesson.DiaDiem = model.DiaDiem.Trim(); lesson.TrangThai = model.TrangThai;
                if (!id.HasValue) db.Add(lesson);
                await db.SaveChangesAsync(); await transaction.CommitAsync();
                TempData["Success"] = "Đã lưu buổi học.";
                return RedirectToAction(nameof(Index), new { maLop = model.MaLop });
            }
        }
        catch (DbUpdateException) { ModelState.AddModelError("", "Dữ liệu vừa thay đổi, vui lòng tải lại và thử lại."); }
        catch (SqlException ex) when (ex.Number is 1205 or 1222)
        { ModelState.AddModelError("", "Lịch học đang được cập nhật đồng thời. Vui lòng thử lại."); }
        await Populate(model);
        return View(id.HasValue ? "Edit" : "Create", model);
    }

    private async Task Populate(BuoiHocViewModel model) =>
        model.Lops = await db.LopDaoTaos.AsNoTracking().OrderBy(x => x.TenLop)
            .Select(x => new SelectListItem(x.TenLop, x.MaLop.ToString())).ToListAsync();

    private static IQueryable<BuoiHocViewModel> Project(IQueryable<BuoiHoc> query) => query.Select(x => new BuoiHocViewModel
    {
        MaBuoiHoc = x.MaBuoiHoc,
        MaLop = x.MaLop,
        TenLop = x.LopDaoTao.TenLop,
        TenKhoaDaoTao = x.LopDaoTao.KhoaDaoTao.TenKhoaDaoTao,
        TrangThaiLop = x.LopDaoTao.TrangThai,
        NgayHoc = x.NgayHoc,
        GioBatDau = x.GioBatDau,
        GioKetThuc = x.GioKetThuc,
        NoiDung = x.NoiDung,
        DiaDiem = x.DiaDiem,
        TrangThai = x.TrangThai
    });
}

