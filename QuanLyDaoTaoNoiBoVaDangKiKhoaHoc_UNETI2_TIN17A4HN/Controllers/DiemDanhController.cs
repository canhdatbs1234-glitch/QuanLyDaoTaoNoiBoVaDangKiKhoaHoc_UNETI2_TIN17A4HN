// Họ và tên: Phạm Việt Anh
// Mã sinh viên: 23103100231
// Nội dung thực hiện: Module 4 - Buổi học, Lịch học và Điểm danh
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
using QuanLyDaoTaoNoiBo.Services;

namespace QuanLyDaoTaoNoiBo.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DiemDanhController(ApplicationDbContext db, DiemDanhService service) : Controller
{
    private int AccountId => HttpContext.Session.GetMaTaiKhoan() ?? 0;

    [HttpGet, SessionAuthorize(Roles = "QuanTri,GiangVien")]
    public IActionResult Index() => RedirectToAction("Index", "BuoiHoc");

    [HttpGet, SessionAuthorize(Roles = "QuanTri,GiangVien")]
    public async Task<IActionResult> DiemDanhBuoiHoc(int id)
    {
        var model = await service.Load(AccountId, id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, SessionAuthorize(Roles = "QuanTri,GiangVien")]
    public async Task<IActionResult> DiemDanhBuoiHoc(int id, DiemDanhViewModel model)
    {
        if (id != model.MaBuoiHoc) return BadRequest();
        if (!ModelState.IsValid)
        {
            // Làm mới roster từ DB, không render tên/ID không đáng tin do client gửi.
            TempData["Error"] = "Vui lòng chọn đủ trạng thái hợp lệ và giới hạn ghi chú 1000 ký tự. Dữ liệu chưa được lưu.";
            return RedirectToAction(nameof(DiemDanhBuoiHoc), new { id });
        }
        var result = await service.Save(AccountId, model);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(DiemDanhBuoiHoc), new { id });
    }
    [HttpGet]
    public async Task<IActionResult> ThongKeThamGia(int? maLop)
    {
        var vaiTro = HttpContext.Session.GetVaiTro();
        var manager = vaiTro == "QuanTri";
        IQueryable<LopDaoTao> allowed = db.LopDaoTaos.AsNoTracking();
        if (!manager)
        {
            if (vaiTro == "GiangVien")
                allowed = allowed.Where(x => x.MaGiangVien == AccountId);
            else
                allowed = allowed.Where(x => x.DangKyDaoTaos.Any(d => d.TrangThai == "DaDuyet" && d.NhanVien.MaTaiKhoan == AccountId));
        }
        var options = await allowed.OrderBy(x => x.TenLop)
            .Select(x => new SelectListItem(x.TenLop, x.MaLop.ToString())).ToListAsync();
        if (!maLop.HasValue) return View(new BaoCaoThamGiaViewModel { Lops = options });
        var report = await service.Report(AccountId, maLop.Value);
        if (report is null) return StatusCode(StatusCodes.Status403Forbidden);
        report.Lops = options;
        return View(report);
    }
}