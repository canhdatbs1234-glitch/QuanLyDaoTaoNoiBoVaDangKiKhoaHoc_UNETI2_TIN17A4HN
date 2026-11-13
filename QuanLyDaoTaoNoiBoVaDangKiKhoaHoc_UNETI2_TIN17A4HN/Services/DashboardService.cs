// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Tính các chỉ số Dashboard bằng LINQ (mục 9.6).
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;

namespace QuanLyDaoTaoNoiBo.Services;
public class DashboardService(ApplicationDbContext db, KetQuaDaoTaoService ketQuaService)
{
    /// <summary>Lớp "sắp bắt đầu" = bắt đầu trong vòng bao nhiêu ngày tới.</summary>
    public const int SoNgaySapBatDau = 30;

    public async Task<DashboardViewModel> LayDashboardAsync(CancellationToken ct = default)
    {
        var homNay = DateOnly.FromDateTime(LopDaoTaoRules.BayGioVietNam());
        var hanSapBatDau = homNay.AddDays(SoNgaySapBatDau);

        // Đăng ký theo trạng thái -> 1 truy vấn GROUP BY
        var dangKy = await db.DangKyDaoTaos.AsNoTracking()
            .GroupBy(d => d.TrangThai)
            .Select(g => new { g.Key, So = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.So, ct);

        // Lớp theo trạng thái -> 1 truy vấn GROUP BY
        var lop = await db.LopDaoTaos.AsNoTracking()
            .GroupBy(l => l.TrangThai)
            .Select(g => new { g.Key, So = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.So, ct);

        // Kết quả ĐÃ XÁC NHẬN theo loại -> 1 truy vấn GROUP BY
        var ketQua = await db.KetQuaDaoTaos.AsNoTracking()
            .Where(k => k.DaXacNhan)
            .GroupBy(k => k.KetQua)
            .Select(g => new { g.Key, So = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.So, ct);

        // Lớp sắp bắt đầu: chưa hủy, chưa học, bắt đầu trong N ngày tới
        var sapBatDau = db.LopDaoTaos.AsNoTracking()
            .Where(l => l.NgayBatDau > homNay && l.NgayBatDau <= hanSapBatDau
                        && (l.TrangThai == "ChuaMoDangKy" || l.TrangThai == "MoDangKy" || l.TrangThai == "DongDangKy"));

        var dsSapBatDau = await sapBatDau
            .OrderBy(l => l.NgayBatDau).ThenBy(l => l.MaLop)
            .Take(5)
            .Select(l => new LopSapBatDauViewModel
            {
                MaLop = l.MaLop,
                TenLop = l.TenLop,
                TenKhoa = l.KhoaDaoTao.TenKhoaDaoTao,
                NgayBatDau = l.NgayBatDau,
                DiaDiem = l.DiaDiem,
                SoLuongToiDa = l.SoLuongToiDa,
                SoChoCon = l.SoLuongToiDa - l.DangKyDaoTaos.Count(d => d.TrangThai == TrangThaiDangKyConst.DaDuyet)
            })
            .ToListAsync(ct);
        foreach (var item in dsSapBatDau)
            item.SoNgayConLai = item.NgayBatDau.DayNumber - homNay.DayNumber;

        return new DashboardViewModel
        {
            HomNay = homNay,
            SoNgaySapBatDau = SoNgaySapBatDau,

            SoNhanVienDangHoatDong = await db.NhanViens.AsNoTracking().CountAsync(n => n.TrangThai, ct),
            SoKhoaDangMo = await db.KhoaDaoTaos.AsNoTracking().CountAsync(k => k.TrangThai, ct),
            SoLopDangMoDangKy = lop.GetValueOrDefault("MoDangKy"),
            SoLopSapBatDau = await sapBatDau.CountAsync(ct),

            SoDangKyChoDuyet = dangKy.GetValueOrDefault(TrangThaiDangKyConst.ChoDuyet),
            SoDangKyDaDuyet = dangKy.GetValueOrDefault(TrangThaiDangKyConst.DaDuyet),

            SoLopDangHoc = lop.GetValueOrDefault("DangHoc"),
            SoLopDaKetThuc = lop.GetValueOrDefault("HoanThanh"),

            SoLuotHoanThanh = ketQua.GetValueOrDefault(KetQuaConst.HoanThanh),
            SoLuotKhongHoanThanh = ketQua.GetValueOrDefault(KetQuaConst.KhongHoanThanh),
            SoKetQuaChoXacNhan = await db.KetQuaDaoTaos.AsNoTracking()
                .CountAsync(k => !k.DaXacNhan && k.KetQua != KetQuaConst.ChuaDanhGia, ct),

            LopSapBatDau = dsSapBatDau
        };
    }

    // =====================================================================
    // THỐNG KÊ LINQ (mục 9.7)
    // =====================================================================
    public async Task<ThongKeViewModel> LayThongKeAsync(int? nam, CancellationToken ct = default)
    {
        var bayGio = LopDaoTaoRules.BayGioVietNam();
        const string DaDuyet = TrangThaiDangKyConst.DaDuyet;

        // ---- A. Mỗi lớp 1 dòng: đăng ký theo trạng thái + kết quả đã xác nhận (báo cáo 2, 3, 4, 5) ----
        var lops = await db.LopDaoTaos.AsNoTracking()
            .OrderBy(l => l.TenLop)
            .Select(l => new ThongKeLopRow
            {
                MaLop = l.MaLop,
                TenLop = l.TenLop,
                MaKhoaDaoTao = l.MaKhoaDaoTao,
                TenKhoa = l.KhoaDaoTao.TenKhoaDaoTao,
                TrangThai = l.TrangThai,
                SoLuongToiDa = l.SoLuongToiDa,
                ChoDuyet = l.DangKyDaoTaos.Count(d => d.TrangThai == TrangThaiDangKyConst.ChoDuyet),
                DaDuyet = l.DangKyDaoTaos.Count(d => d.TrangThai == TrangThaiDangKyConst.DaDuyet),
                TuChoi = l.DangKyDaoTaos.Count(d => d.TrangThai == TrangThaiDangKyConst.TuChoi),
                DaHuy = l.DangKyDaoTaos.Count(d => d.TrangThai == TrangThaiDangKyConst.DaHuy),
                HoanThanh = l.DangKyDaoTaos.Count(d => d.KetQuaDaoTao != null && d.KetQuaDaoTao.DaXacNhan
                                                       && d.KetQuaDaoTao.KetQua == KetQuaConst.HoanThanh),
                KhongHoanThanh = l.DangKyDaoTaos.Count(d => d.KetQuaDaoTao != null && d.KetQuaDaoTao.DaXacNhan
                                                            && d.KetQuaDaoTao.KetQua == KetQuaConst.KhongHoanThanh)
            })
            .ToListAsync(ct);

        // ---- B. Tham dự và vắng theo lớp (báo cáo 6, 7): chỉ buổi hợp lệ, chỉ học viên đã duyệt ----
        var buoiHopLe = ketQuaService.BuoiHopLe(bayGio);

        var tongBuoi = await buoiHopLe
            .GroupBy(b => b.MaLop)
            .Select(g => new { MaLop = g.Key, So = g.Count() })
            .ToDictionaryAsync(x => x.MaLop, x => x.So, ct);

        var diemDanh = await (from dd in db.DiemDanhs.AsNoTracking()
                              join b in buoiHopLe on dd.MaBuoiHoc equals b.MaBuoiHoc
                              join dk in db.DangKyDaoTaos.AsNoTracking().Where(x => x.TrangThai == DaDuyet)
                                  on new { b.MaLop, dd.MaNhanVien } equals new { dk.MaLop, dk.MaNhanVien }
                              group dd by new { b.MaLop, dd.TrangThai } into g
                              select new { g.Key.MaLop, g.Key.TrangThai, So = g.Count() })
                             .ToListAsync(ct);

        foreach (var r in lops)
        {
            r.TongSoBuoiHopLe = tongBuoi.GetValueOrDefault(r.MaLop);
            r.SoLuotCoMat = diemDanh.Where(x => x.MaLop == r.MaLop && x.TrangThai == "CoMat").Sum(x => x.So);
            r.SoLuotVangKhongPhep = diemDanh.Where(x => x.MaLop == r.MaLop && x.TrangThai == "VangMat").Sum(x => x.So);
            r.SoLuotVangCoPhep = diemDanh.Where(x => x.MaLop == r.MaLop && x.TrangThai == "CoPhep").Sum(x => x.So);
        }

        var tongMau = lops.Sum(r => r.MauSoThamDu);
        var tyLeChung = tongMau == 0 ? (decimal?)null : Math.Round(lops.Sum(r => r.SoLuotCoMat) * 100m / tongMau, 2);

        // ---- Gộp theo khóa (báo cáo 2, 5) ----
        var khoas = lops
            .GroupBy(l => new { l.MaKhoaDaoTao, l.TenKhoa })
            .Select(g => new ThongKeKhoaRow
            {
                MaKhoaDaoTao = g.Key.MaKhoaDaoTao,
                TenKhoa = g.Key.TenKhoa,
                SoLop = g.Count(),
                ChoDuyet = g.Sum(x => x.ChoDuyet),
                DaDuyet = g.Sum(x => x.DaDuyet),
                TuChoi = g.Sum(x => x.TuChoi),
                DaHuy = g.Sum(x => x.DaHuy),
                HoanThanh = g.Sum(x => x.HoanThanh),
                KhongHoanThanh = g.Sum(x => x.KhongHoanThanh)
            })
            .OrderBy(k => k.TenKhoa)
            .ToList();

        // ---- Báo cáo 3: lớp nhiều đăng ký nhất ----
        var lopDongNhat = lops
            .Where(l => l.TrangThai != "DaHuy" && l.DangKyHieuLuc > 0)
            .OrderByDescending(l => l.DangKyHieuLuc).ThenBy(l => l.TenLop)
            .Take(5)
            .ToList();

        // ---- Báo cáo 1: nhân viên tham gia theo phòng ban ----
        var phongBanDs = await db.PhongBans.AsNoTracking()
            .OrderBy(p => p.TenPhongBan)
            .Select(p => new { p.MaPhongBan, p.TenPhongBan })
            .ToListAsync(ct);

        var nvHoatDong = await db.NhanViens.AsNoTracking()
            .Where(n => n.TrangThai)
            .GroupBy(n => n.MaPhongBan)
            .Select(g => new { MaPhongBan = g.Key, So = g.Count() })
            .ToDictionaryAsync(x => x.MaPhongBan, x => x.So, ct);

        // DISTINCT trong SQL trước (mỗi nhân viên 1 lần), đếm theo phòng ban sau.
        var capNvPhongBan = await db.DangKyDaoTaos.AsNoTracking()
            .Where(d => d.TrangThai == DaDuyet)
            .Select(d => new { d.MaNhanVien, d.NhanVien.MaPhongBan })
            .Distinct()
            .ToListAsync(ct);
        var nvThamGia = capNvPhongBan.GroupBy(x => x.MaPhongBan).ToDictionary(g => g.Key, g => g.Count());

        var luotTheoPhongBan = await db.DangKyDaoTaos.AsNoTracking()
            .Where(d => d.TrangThai == DaDuyet)
            .GroupBy(d => d.NhanVien.MaPhongBan)
            .Select(g => new { MaPhongBan = g.Key, So = g.Count() })
            .ToDictionaryAsync(x => x.MaPhongBan, x => x.So, ct);

        var phongBans = phongBanDs.Select(p => new ThongKePhongBanRow
        {
            TenPhongBan = p.TenPhongBan,
            SoNhanVienHoatDong = nvHoatDong.GetValueOrDefault(p.MaPhongBan),
            SoNhanVienThamGia = nvThamGia.GetValueOrDefault(p.MaPhongBan),
            SoLuotDaoTao = luotTheoPhongBan.GetValueOrDefault(p.MaPhongBan)
        }).ToList();

        // ---- Báo cáo 8: nhân viên tham gia nhiều KHÓA nhất ----
        var capNvKhoa = await db.DangKyDaoTaos.AsNoTracking()
            .Where(d => d.TrangThai == DaDuyet)
            .Select(d => new { d.MaNhanVien, d.LopDaoTao.MaKhoaDaoTao })
            .Distinct()
            .ToListAsync(ct);
        var soKhoa = capNvKhoa.GroupBy(x => x.MaNhanVien).ToDictionary(g => g.Key, g => g.Count());

        var soLop = await db.DangKyDaoTaos.AsNoTracking()
            .Where(d => d.TrangThai == DaDuyet)
            .GroupBy(d => d.MaNhanVien)
            .Select(g => new { MaNhanVien = g.Key, So = g.Count() })
            .ToDictionaryAsync(x => x.MaNhanVien, x => x.So, ct);

        var topNv = soKhoa
            .OrderByDescending(kv => kv.Value)
            .ThenByDescending(kv => soLop.GetValueOrDefault(kv.Key))
            .ThenBy(kv => kv.Key)
            .Take(10)
            .ToList();
        var topIds = topNv.Select(kv => kv.Key).ToList();

        var nvInfo = (await db.NhanViens.AsNoTracking()
                .Where(n => topIds.Contains(n.MaNhanVien))
                .Select(n => new { n.MaNhanVien, n.HoTen, TenPhongBan = n.PhongBan.TenPhongBan })
                .ToListAsync(ct))
            .ToDictionary(x => x.MaNhanVien);

        var nhanVienNhieuKhoa = topNv
            .Where(kv => nvInfo.ContainsKey(kv.Key))
            .Select(kv => new ThongKeNhanVienRow
            {
                HoTen = nvInfo[kv.Key].HoTen,
                TenPhongBan = nvInfo[kv.Key].TenPhongBan,
                SoKhoa = kv.Value,
                SoLop = soLop.GetValueOrDefault(kv.Key)
            })
            .ToList();

        // ---- Báo cáo 9: lượt đào tạo theo tháng / quý của một năm ----
        var nams = await db.LopDaoTaos.AsNoTracking()
            .Select(l => l.NgayBatDau.Year).Distinct().OrderByDescending(y => y)
            .ToListAsync(ct);
        var namHienTai = bayGio.Year;
        var namChon = nam is > 0 && nams.Contains(nam.Value)
            ? nam.Value
            : (nams.Contains(namHienTai) ? namHienTai : nams.FirstOrDefault(namHienTai));

        var theoThangDb = await db.DangKyDaoTaos.AsNoTracking()
            .Where(d => d.TrangThai == DaDuyet && d.LopDaoTao.NgayBatDau.Year == namChon)
            .GroupBy(d => d.LopDaoTao.NgayBatDau.Month)
            .Select(g => new
            {
                Thang = g.Key,
                SoLuot = g.Count(),
                SoHoanThanh = g.Count(d => d.KetQuaDaoTao != null && d.KetQuaDaoTao.DaXacNhan
                                           && d.KetQuaDaoTao.KetQua == KetQuaConst.HoanThanh)
            })
            .ToListAsync(ct);

        // Đủ 12 tháng (tháng không có dữ liệu = 0), quý = tổng 3 tháng
        var thangs = Enumerable.Range(1, 12).Select(t =>
        {
            var x = theoThangDb.FirstOrDefault(m => m.Thang == t);
            return new ThongKeThoiGianRow { Nhan = $"Tháng {t}", SoLuot = x?.SoLuot ?? 0, SoHoanThanh = x?.SoHoanThanh ?? 0 };
        }).ToList();

        var quys = Enumerable.Range(0, 4).Select(q => new ThongKeThoiGianRow
        {
            Nhan = $"Quý {q + 1}",
            SoLuot = thangs.Skip(q * 3).Take(3).Sum(t => t.SoLuot),
            SoHoanThanh = thangs.Skip(q * 3).Take(3).Sum(t => t.SoHoanThanh)
        }).ToList();

        return new ThongKeViewModel
        {
            Nam = namChon,
            Nams = nams,
            PhongBans = phongBans,
            Khoas = khoas,
            Lops = lops,
            LopDongNhat = lopDongNhat,
            NhanVienNhieuKhoa = nhanVienNhieuKhoa,
            Thangs = thangs,
            Quys = quys,
            TyLeThamDuTrungBinhChung = tyLeChung
        };
    }
}