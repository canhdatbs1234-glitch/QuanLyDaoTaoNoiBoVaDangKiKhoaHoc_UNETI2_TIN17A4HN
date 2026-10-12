// Họ và tên: Phí Bảo Nam
// Mã sinh viên: 23103100236
// Nội dung thực hiện: Module 3 - Nhân viên, Đăng ký và Duyệt đào tạo

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Filters;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
using QuanLyDaoTaoNoiBo.Services;

namespace QuanLyDaoTaoNoiBo.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DangKyDaoTaoController(ApplicationDbContext db, DangKyDaoTaoService service) : Controller
{
    [HttpGet, SessionRoles("NhanVien")]
    public async Task<IActionResult> Index()
    {
        var now = service.Now;
        var today = service.Today;
        var employeeId = HttpContext.Session.GetInt32("MaNhanVien");
        ViewData["CoHoSo"] = employeeId.HasValue && await db.NhanViens.AnyAsync(x =>
            x.MaNhanVien == employeeId && x.MaTaiKhoan == AccountId && x.TrangThai);
        ViewData["DaDangKy"] = employeeId.HasValue
            ? (await db.DangKyDaoTaos.Where(x => x.MaNhanVien == employeeId).Select(x => x.MaLop).ToListAsync()).ToHashSet()
            : new HashSet<int>();
        return View(await db.LopDaoTaos.AsNoTracking()
            .Where(x => x.TrangThai == "MoDangKy" && x.HanDangKy >= now && x.NgayBatDau > today && x.KhoaDaoTao.TrangThai)
            .OrderBy(x => x.NgayBatDau).Select(x => new LopDaoTaoViewModel
            {
                MaLop = x.MaLop,
                TenLop = x.TenLop,
                TenKhoaDaoTao = x.KhoaDaoTao.TenKhoaDaoTao,
                NgayBatDau = x.NgayBatDau,
                NgayKetThuc = x.NgayKetThuc,
                HanDangKy = x.HanDangKy,
                DiaDiem = x.DiaDiem,
                SoLuongToiDa = x.SoLuongToiDa,
                SoDaDuyet = x.DangKyDaoTaos.Count(d => d.TrangThai == "DaDuyet"),
                TrangThai = x.TrangThai
            }).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("NhanVien")]
    public async Task<IActionResult> DangKy(int maLop)
    {
        var result = await service.DangKy(AccountId, EmployeeId, maLop);
        Notify(result);
        return RedirectToAction(result.Success ? nameof(LichSuDangKy) : nameof(Index));
    }

    [HttpGet, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> DuyetDangKy() => View(await Project(
        db.DangKyDaoTaos.AsNoTracking().Where(x => x.TrangThai == "ChoDuyet")
            .OrderBy(x => x.NgayDangKy).ThenBy(x => x.MaDangKy)).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> DuyetDangKy(int maDangKy, string? quyetDinh, string? lyDo)
    {
        Notify(await service.Duyet(AccountId, maDangKy, quyetDinh, lyDo));
        return RedirectToAction(nameof(DuyetDangKy));
    }

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("NhanVien")]
    public async Task<IActionResult> HuyDangKy(int maDangKy, string? lyDo)
    {
        Notify(await service.Huy(AccountId, EmployeeId, maDangKy, lyDo));
        return RedirectToAction(nameof(LichSuDangKy));
    }

    [HttpGet, SessionRoles("NhanVien")]
    public async Task<IActionResult> LichSuDangKy()
    {
        var id = EmployeeId;
        var ownProfile = await db.NhanViens.AnyAsync(x => x.MaNhanVien == id && x.MaTaiKhoan == AccountId);
        if (!ownProfile)
        {
            TempData["Error"] = "Tài khoản chưa liên kết với hồ sơ nhân viên.";
            return View(new List<DangKyDaoTaoViewModel>());
        }
        return View(await Project(db.DangKyDaoTaos.AsNoTracking().Where(x => x.MaNhanVien == id)
            .OrderByDescending(x => x.NgayDangKy).ThenByDescending(x => x.MaDangKy)).ToListAsync());
    }

    private int AccountId => HttpContext.Session.GetInt32("MaTaiKhoan") ?? 0;
    private int EmployeeId => HttpContext.Session.GetInt32("MaNhanVien") ?? 0;
    private void Notify(DangKyResult result) => TempData[result.Success ? "Success" : "Error"] = result.Message;

    private IQueryable<DangKyDaoTaoViewModel> Project(IQueryable<DangKyDaoTao> query)
    {
        var today = service.Today;
        return query.Select(x => new DangKyDaoTaoViewModel
        {
            MaDangKy = x.MaDangKy,
            MaNhanVien = x.MaNhanVien,
            MaLop = x.MaLop,
            TenNhanVien = x.NhanVien.HoTen,
            TenPhongBan = x.NhanVien.PhongBan.TenPhongBan,
            TenLop = x.LopDaoTao.TenLop,
            TenKhoaDaoTao = x.LopDaoTao.KhoaDaoTao.TenKhoaDaoTao,
            NgayBatDau = x.LopDaoTao.NgayBatDau,
            NgayKetThuc = x.LopDaoTao.NgayKetThuc,
            DiaDiem = x.LopDaoTao.DiaDiem,
            NgayDangKy = x.NgayDangKy,
            NgayDuyet = x.NgayDuyet,
            TrangThai = x.TrangThai,
            LyDo = x.GhiChu,
            CoTheHuy = (x.TrangThai == "ChoDuyet" || x.TrangThai == "DaDuyet") && x.LopDaoTao.NgayBatDau > today
        });
    }
}

