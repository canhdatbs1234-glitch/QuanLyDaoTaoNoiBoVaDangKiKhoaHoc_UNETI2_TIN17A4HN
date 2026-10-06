// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Controller Đánh giá kết quả

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A4HN.Data;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A4HN.Models;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A4HN.ViewModels;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A4HN.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A4HN.Controllers
{
    public class KetQuaDaoTaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KetQuaDaoTaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==============================
        // ADMIN: Đánh giá kết quả lớp học
        // ==============================
        [CustomAuthorize(Roles = "Admin")]
        public async Task<IActionResult> DanhGiaLop(int maLop)
        {
            var lop = await _context.LopDaoTaos
                .Include(l => l.KhoaDaoTao)
                .FirstOrDefaultAsync(l => l.MaLop == maLop);

            if (lop == null) return NotFound();

            var ketQuas = await _context.KetQuaDaoTaos
                .Include(k => k.DangKyDaoTao)
                .ThenInclude(d => d.NhanVien)
                .ThenInclude(n => n.PhongBan)
                .Where(k => k.DangKyDaoTao.MaLop == maLop && k.DangKyDaoTao.TrangThai == 1)
                .ToListAsync();

            var vm = new DanhGiaLopViewModel
            {
                MaLop = maLop,
                TenLop = lop.TenLop,
                TyLeThamDuToiThieu = lop.KhoaDaoTao.TyLeThamDuToiThieu,
                DiemDatToiThieu = lop.KhoaDaoTao.DiemDatToiThieu,
                DanhSachHocVien = ketQuas.Select(k => new DanhGiaHocVienViewModel
                {
                    MaKetQua = k.MaKetQua,
                    HoTen = k.DangKyDaoTao.NhanVien.HoTen,
                    PhongBan = k.DangKyDaoTao.NhanVien.PhongBan?.TenPhongBan,
                    TyLeThamDu = k.TyLeThamDu,
                    DiemDanhGia = k.DiemDanhGia,
                    KetQua = k.KetQua,
                    GhiChu = k.GhiChu
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]
        public async Task<IActionResult> LuuDanhGia(DanhGiaLopViewModel vm)
        {
            foreach (var item in vm.DanhSachHocVien)
            {
                var kq = await _context.KetQuaDaoTaos.FindAsync(item.MaKetQua);
                if (kq != null)
                {
                    kq.DiemDanhGia = item.DiemDanhGia;
                    kq.GhiChu = item.GhiChu;

                    // Chỉ cập nhật ngày đánh giá khi có nhập điểm
                    if (item.DiemDanhGia != null && kq.NgayDanhGia == null)
                    {
                        kq.NgayDanhGia = DateTime.Now;
                    }

                    // Tự động xác định Hoàn thành
                    if (kq.DiemDanhGia != null)
                    {
                        if (kq.TyLeThamDu >= vm.TyLeThamDuToiThieu && kq.DiemDanhGia >= vm.DiemDatToiThieu)
                        {
                            kq.KetQua = "Hoàn thành";
                        }
                        else
                        {
                            kq.KetQua = "Không hoàn thành";
                        }
                    }
                    _context.Update(kq);
                }
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã lưu kết quả đánh giá thành công.";
            return RedirectToAction(nameof(DanhGiaLop), new { maLop = vm.MaLop });
        }

        // ==============================
        // NHÂN VIÊN: Tra cứu lịch sử
        // ==============================
        [CustomAuthorize(Roles = "NhanVien,Admin")]
        public async Task<IActionResult> TraCuuLichSu()
        {
            var maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");
            var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(n => n.MaTaiKhoan == maTaiKhoan);

            if (nhanVien == null) return Content("Không tìm thấy thông tin.");

            var lichSu = await _context.KetQuaDaoTaos
                .Include(k => k.DangKyDaoTao)
                .ThenInclude(d => d.LopDaoTao)
                .ThenInclude(l => l.KhoaDaoTao)
                .Where(k => k.DangKyDaoTao.MaNhanVien == nhanVien.MaNhanVien)
                .OrderByDescending(k => k.DangKyDaoTao.LopDaoTao.NgayKetThuc)
                .ToListAsync();

            return View(lichSu);
        }
    }
}
