//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Models;

namespace QuanLyDaoTaoNoiBo.Controllers;

// Trang chủ sau đăng nhập; mọi vai trò hợp lệ đều truy cập được (SessionAuthorizationFilter đã bắt buộc đăng nhập).
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class HomeController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("VaiTro");
        if (role is VaiTroTaiKhoan.QuanTri or VaiTroTaiKhoan.QuanLy)
        {
            ViewData["SoTaiKhoan"] = await db.TaiKhoans.CountAsync();
            ViewData["SoTaiKhoanKhoa"] = await db.TaiKhoans.CountAsync(x => !x.TrangThai);
            ViewData["SoPhongBan"] = await db.PhongBans.CountAsync(x => x.TrangThai);
            ViewData["SoLinhVuc"] = await db.LinhVucDaoTaos.CountAsync(x => x.TrangThai);
        }
        return View();
    }
}
