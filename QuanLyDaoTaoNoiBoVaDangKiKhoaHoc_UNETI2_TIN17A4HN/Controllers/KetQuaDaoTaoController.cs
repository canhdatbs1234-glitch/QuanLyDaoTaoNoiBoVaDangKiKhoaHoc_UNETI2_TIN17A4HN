// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Kết quả đào tạo, Lịch sử đào tạo và Dashboard Thống kê

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Filters;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
using QuanLyDaoTaoNoiBo.Services;

namespace QuanLyDaoTaoNoiBo.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class KetQuaDaoTaoController(ApplicationDbContext db, KetQuaDaoTaoService service) : Controller
{
    private int AccountId => HttpContext.Session.GetInt32("MaTaiKhoan") ?? 0;
    private bool Manager => HttpContext.Session.GetString("VaiTro") is "QuanTri" or "QuanLy";

    [HttpGet, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> DanhGia(int? maLop)
    {
        var options = await db.LopDaoTaos.AsNoTracking().Where(x => x.TrangThai != "DaHuy" && x.NgayKetThuc < service.Today)
            .OrderByDescending(x => x.NgayKetThuc).Select(x => new SelectListItem(x.TenLop, x.MaLop.ToString())).ToListAsync();
        var model = maLop.HasValue ? await service.Load(AccountId, maLop.Value) : new DanhGiaLopViewModel();
        if (model is null) return BadRequest("Chỉ đánh giá lớp đã kết thúc, không bị hủy.");
        model.Lops = options;
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> DanhGia(DanhGiaLopViewModel model)
    {
        var result = ModelState.IsValid ? await service.SaveDraft(AccountId, model) :
            new KetQuaResult(false, "Điểm không hợp lệ. Nhập điểm từ 0 đến 10.");
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(DanhGia), new { maLop = model.MaLop });
    }

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> XacNhanKetQua(int maLop, string? version)
    {
        var result = await service.Confirm(AccountId, maLop, version);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(DanhGia), new { maLop });
    }

    [HttpGet]
    public async Task<IActionResult> LichSuDaoTao(int? maNhanVien)
    {
        var model = new LichSuDaoTaoViewModel();
        if (Manager)
        {
            model.NhanViens = await db.NhanViens.AsNoTracking().OrderBy(x => x.HoTen)
                .Select(x => new SelectListItem(x.HoTen + " — #" + x.MaNhanVien, x.MaNhanVien.ToString())).ToListAsync();
            if (!maNhanVien.HasValue) return View(model);
        }
        else
        {
            var own = await db.NhanViens.Where(x => x.MaTaiKhoan == AccountId).Select(x => (int?)x.MaNhanVien).SingleOrDefaultAsync();
            if (!own.HasValue || (maNhanVien.HasValue && maNhanVien != own)) return StatusCode(403);
            maNhanVien = own;
        }
        var employee = await db.NhanViens.AsNoTracking().SingleOrDefaultAsync(x => x.MaNhanVien == maNhanVien);
        if (employee is null) return NotFound();
        model.MaNhanVien = employee.MaNhanVien; model.HoTen = employee.HoTen;
        var rows = await db.DangKyDaoTaos.AsNoTracking().Where(x => x.MaNhanVien == employee.MaNhanVien && x.TrangThai == "DaDuyet")
            .OrderByDescending(x => x.LopDaoTao.NgayBatDau).ThenByDescending(x => x.MaDangKy)
            .Select(x => new KetQuaDaoTaoViewModel
            {
                MaDangKy = x.MaDangKy,
                MaNhanVien = x.MaNhanVien,
                HoTen = x.NhanVien.HoTen,
                TenLop = x.LopDaoTao.TenLop,
                TenKhoaDaoTao = x.LopDaoTao.KhoaDaoTao.TenKhoaDaoTao,
                NgayBatDau = x.LopDaoTao.NgayBatDau,
                NgayKetThuc = x.LopDaoTao.NgayKetThuc,
                DiemDanhGia = x.KetQuaDaoTao != null && x.KetQuaDaoTao.NgayDanhGia != null ? x.KetQuaDaoTao.DiemDanhGia : null,
                TyLeThamDu = x.KetQuaDaoTao != null && x.KetQuaDaoTao.NgayDanhGia != null ? x.KetQuaDaoTao.TyLeThamDu : 0,
                KetQua = x.KetQuaDaoTao != null && x.KetQuaDaoTao.NgayDanhGia != null ? x.KetQuaDaoTao.KetQua : "ChuaDanhGia",
                NgayDanhGia = x.KetQuaDaoTao == null ? null : x.KetQuaDaoTao.NgayDanhGia
            }).ToListAsync();
        // Chưa xác nhận: tỷ lệ hiện tại từ điểm danh, không công bố điểm/kết quả nháp.
        var pendingIds = rows.Where(x => !x.DaKhoa).Select(x => x.MaDangKy).ToArray();
        var today = service.Today; var time = TimeOnly.FromDateTime(service.Now);
        var live = await db.DangKyDaoTaos.AsNoTracking().Where(x => pendingIds.Contains(x.MaDangKy))
            .Select(x => new
            {
                x.MaDangKy,
                Total = x.LopDaoTao.BuoiHocs.Count(b => x.LopDaoTao.TrangThai != "DaHuy" && b.TrangThai != "DaHuy" &&
                    (b.NgayHoc < today || (b.NgayHoc == today && b.GioKetThuc <= time))),
                Present = x.LopDaoTao.BuoiHocs.Count(b => x.LopDaoTao.TrangThai != "DaHuy" && b.TrangThai != "DaHuy" &&
                    (b.NgayHoc < today || (b.NgayHoc == today && b.GioKetThuc <= time)) &&
                    b.DiemDanhs.Any(d => d.MaNhanVien == x.MaNhanVien && d.TrangThai == "CoMat"))
            }).ToDictionaryAsync(x => x.MaDangKy);
        foreach (var row in rows.Where(x => !x.DaKhoa))
        {
            var value = live[row.MaDangKy]; row.TyLeThamDu = KetQuaDaoTaoService.Ratio(value.Present, value.Total);
        }
        model.KetQuas = rows;
        return View(model);
    }
}

