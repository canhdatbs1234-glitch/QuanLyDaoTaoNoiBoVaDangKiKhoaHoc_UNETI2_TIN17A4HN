// Họ và tên: Nguyễn Cảnh Đạt
// Mã sinh viên: 23103100224
// Nội dung thực hiện: Module 1 - Controller Tai Khoan quản lý đăng nhập/đăng xuất với phân quyền Admin/NhanVien và kiểm tra trạng thái tài khoản khóa/mở.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Data;
using QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.ViewModels;
using System.Linq;

namespace QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.TaiKhoans.FirstOrDefault(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == model.MatKhau);

                if (user != null)
                {
                    if (!user.TrangThai)
                    {
                        ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                        return View(model);
                    }

                    HttpContext.Session.SetInt32("MaTaiKhoan", user.MaTaiKhoan);
                    HttpContext.Session.SetString("HoTen", user.HoTen ?? "");
                    HttpContext.Session.SetString("VaiTro", user.VaiTro ?? "NhanVien");

                    if (user.VaiTro == "Admin")
                    {
                        return RedirectToAction("Index", "PhongBan");
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
