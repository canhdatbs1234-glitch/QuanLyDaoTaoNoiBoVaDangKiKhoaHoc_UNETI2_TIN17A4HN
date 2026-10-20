// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Kết quả đào tạo, Lịch sử đào tạo và Dashboard Thống kê

using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTaoNoiBo.Filters;
using QuanLyDaoTaoNoiBo.Services;

namespace QuanLyDaoTaoNoiBo.Controllers;

[SessionRoles("QuanTri", "QuanLy")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DashboardController(DashboardService service) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() => View(await service.Load());
}

