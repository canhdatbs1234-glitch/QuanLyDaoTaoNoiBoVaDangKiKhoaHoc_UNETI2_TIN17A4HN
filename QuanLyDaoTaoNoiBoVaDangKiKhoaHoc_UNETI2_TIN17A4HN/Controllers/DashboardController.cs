// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Dashboard dành cho Admin / Nhân sự - Đào tạo.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTaoNoiBo.Services;

namespace QuanLyDaoTaoNoiBo.Controllers;
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DashboardController(DashboardService service) : Controller
{
    // Phân quyền tại Controller: chỉ Admin / Nhân sự - Đào tạo.
    [HttpGet, SessionAuthorize(Roles = "QuanTri")]
    public async Task<IActionResult> Index()
        => View(await service.LayDashboardAsync(HttpContext.RequestAborted));

    [HttpGet, SessionAuthorize(Roles = "QuanTri")]
    public async Task<IActionResult> ThongKe(int? nam)
    => View(await service.LayThongKeAsync(nam, HttpContext.RequestAborted));
}