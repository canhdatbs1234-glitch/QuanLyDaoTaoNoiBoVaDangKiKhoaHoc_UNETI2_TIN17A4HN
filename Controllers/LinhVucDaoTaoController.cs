// Họ và tên: Nguyễn Cảnh Đạt
// Mã sinh viên: 23103100224
// Nội dung thực hiện: Kernel ASP.NET Core MVC Controller quản lý lĩnh vực đào tạo (CRUD) với phân quyền Admin và kiểm tra trùng tên lĩnh vực.
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
    public class LinhVucDaoTaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LinhVucDaoTaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.LinhVucDaoTaos.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLinhVuc,TenLinhVuc,MoTa,TrangThai")] LinhVucDaoTao linhVucDaoTao)
        {
            if (ModelState.IsValid)
            {
                if (_context.LinhVucDaoTaos.Any(l => l.TenLinhVuc == linhVucDaoTao.TenLinhVuc))
                {
                    ModelState.AddModelError("TenLinhVuc", "Tên lĩnh vực đã tồn tại.");
                    return View(linhVucDaoTao);
                }

                _context.Add(linhVucDaoTao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(linhVucDaoTao);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var linhVucDaoTao = await _context.LinhVucDaoTaos.FindAsync(id);
            if (linhVucDaoTao == null)
            {
                return NotFound();
            }
            return View(linhVucDaoTao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaLinhVuc,TenLinhVuc,MoTa,TrangThai")] LinhVucDaoTao linhVucDaoTao)
        {
            if (id != linhVucDaoTao.MaLinhVuc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (_context.LinhVucDaoTaos.Any(l => l.TenLinhVuc == linhVucDaoTao.TenLinhVuc && l.MaLinhVuc != linhVucDaoTao.MaLinhVuc))
                {
                    ModelState.AddModelError("TenLinhVuc", "Tên lĩnh vực đã tồn tại.");
                    return View(linhVucDaoTao);
                }

                try
                {
                    _context.Update(linhVucDaoTao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LinhVucDaoTaoExists(linhVucDaoTao.MaLinhVuc))
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
            return View(linhVucDaoTao);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var linhVucDaoTao = await _context.LinhVucDaoTaos.FirstOrDefaultAsync(m => m.MaLinhVuc == id);
            if (linhVucDaoTao == null)
            {
                return NotFound();
            }

            return View(linhVucDaoTao);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var linhVucDaoTao = await _context.LinhVucDaoTaos.FindAsync(id);

            bool hasKhoaDaoTao = _context.KhoaDaoTaos.Any(k => k.MaLinhVuc == id);
            if (hasKhoaDaoTao)
            {
                ViewBag.ErrorMessage = "Không thể xóa lĩnh vực này vì đang có khóa đào tạo thuộc lĩnh vực.";
                return View("Delete", linhVucDaoTao);
            }

            _context.LinhVucDaoTaos.Remove(linhVucDaoTao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LinhVucDaoTaoExists(int id)
        {
            return _context.LinhVucDaoTaos.Any(e => e.MaLinhVuc == id);
        }
    }
}