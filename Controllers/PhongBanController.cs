// Họ và tên: Nguyễn Cảnh Đạt
// Mã sinh viên: 23103100224
// Nội dung thực hiện: Module 1 - Controller Phong Ban (CRUD)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Data;
using QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Models;
using QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Controllers
{
    [CustomAuthorize(Roles = "Admin")]
    public class PhongBanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhongBanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.PhongBans.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaPhongBan,TenPhongBan,MoTa,TrangThai")] PhongBan phongBan)
        {
            if (ModelState.IsValid)
            {
                if (_context.PhongBans.Any(p => p.TenPhongBan == phongBan.TenPhongBan))
                {
                    ModelState.AddModelError("TenPhongBan", "Tên phòng ban đã tồn tại.");
                    return View(phongBan);
                }

                _context.Add(phongBan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phongBan);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phongBan = await _context.PhongBans.FindAsync(id);
            if (phongBan == null)
            {
                return NotFound();
            }
            return View(phongBan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhongBan,TenPhongBan,MoTa,TrangThai")] PhongBan phongBan)
        {
            if (id != phongBan.MaPhongBan)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (_context.PhongBans.Any(p => p.TenPhongBan == phongBan.TenPhongBan && p.MaPhongBan != phongBan.MaPhongBan))
                {
                    ModelState.AddModelError("TenPhongBan", "Tên phòng ban đã tồn tại.");
                    return View(phongBan);
                }

                try
                {
                    _context.Update(phongBan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhongBanExists(phongBan.MaPhongBan))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(phongBan);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phongBan = await _context.PhongBans.FirstOrDefaultAsync(m => m.MaPhongBan == id);
            if (phongBan == null)
            {
                return NotFound();
            }

            return View(phongBan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phongBan = await _context.PhongBans.FindAsync(id);

            bool hasNhanVien = _context.NhanViens.Any(n => n.MaPhongBan == id);
            if (hasNhanVien)
            {
                ViewBag.ErrorMessage = "Không thể xóa phòng ban này vì đang có nhân viên thuộc phòng ban (Toàn vẹn dữ liệu).";
                return View("Delete", phongBan);
            }

            _context.PhongBans.Remove(phongBan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhongBanExists(int id)
        {
            return _context.PhongBans.Any(e => e.MaPhongBan == id);
        }
    }
}