// Họ và tên: [Cả nhóm / nhóm trưởng - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: Dữ liệu mẫu: 4 phòng ban, 4 lĩnh vực, 22 nhân viên, 8 khóa, 12 lớp, buổi học, đăng ký, điểm danh, kết quả.

using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Helpers;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Models;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Data;

public static class DbSeeder
{
    // Tài khoản kiểm thử:
    //   admin / Admin@123      (Admin - Nhân sự/Đào tạo)
    //   gv01  / GiangVien@123  (Giảng viên)
    //   nv01..nv22 / NhanVien@123 (Nhân viên)
    public static void Seed(ApplicationDbContext db)
    {
        if (db.TaiKhoans.Any()) return;   // đã có dữ liệu thì bỏ qua

        var homNay = DateTime.Today;

        // ---------- Phòng ban, lĩnh vực ----------
        var phongBans = new[] { "Nhân sự", "Kế toán", "Kỹ thuật", "Kinh doanh" }
            .Select(t => new PhongBan { TenPhongBan = t, MoTa = $"Phòng {t}" }).ToList();

        var linhVucs = new[] { "Kỹ năng mềm", "Chuyên môn nghiệp vụ", "Công nghệ thông tin", "An toàn - Tuân thủ" }
            .Select(t => new LinhVucDaoTao { TenLinhVuc = t, MoTa = $"Các khóa thuộc lĩnh vực {t}" }).ToList();

        db.PhongBans.AddRange(phongBans);
        db.LinhVucDaoTaos.AddRange(linhVucs);

        // ---------- Tài khoản + nhân viên ----------
        db.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = "admin", MatKhau = MatKhauHelper.Bam("Admin@123"),
            HoTen = "Quản trị đào tạo", Email = "admin@congty.vn", VaiTro = QuyenHan.Admin
        });
        db.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = "gv01", MatKhau = MatKhauHelper.Bam("GiangVien@123"),
            HoTen = "Giảng viên 01", Email = "gv01@congty.vn", VaiTro = QuyenHan.GiangVien
        });

        var nhanViens = new List<NhanVien>();
        for (int i = 1; i <= 22; i++)
        {
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = $"nv{i:00}", MatKhau = MatKhauHelper.Bam("NhanVien@123"),
                HoTen = $"Nhân viên {i:00}", Email = $"nv{i:00}@congty.vn", VaiTro = QuyenHan.NhanVien
            };
            db.TaiKhoans.Add(taiKhoan);

            nhanViens.Add(new NhanVien
            {
                TaiKhoan = taiKhoan,
                HoTen = taiKhoan.HoTen,
                PhongBan = phongBans[(i - 1) % 4],
                NgaySinh = new DateTime(1990 + i % 10, 1 + i % 12, 1 + i % 27),
                SoDienThoai = $"09000000{i:00}",
                Email = taiKhoan.Email,
                NgayVaoLam = homNay.AddMonths(-6 * i),
                TrangThai = TrangThaiNhanVien.DangLamViec
            });
        }
        db.NhanViens.AddRange(nhanViens);

        // ---------- Khóa đào tạo (8 khóa, mỗi lĩnh vực 2 khóa) ----------
        var tenKhoas = new[]
        {
            "Giao tiếp hiệu quả", "Quản lý thời gian",
            "Kế toán quản trị cơ bản", "Phân tích báo cáo tài chính",
            "Lập trình C# cơ bản", "An toàn thông tin",
            "An toàn lao động", "Tuân thủ nội quy công ty"
        };
        var khoas = tenKhoas.Select((ten, i) => new KhoaDaoTao
        {
            TenKhoaDaoTao = ten,
            LinhVucDaoTao = linhVucs[i / 2],
            MoTa = $"Nội dung khóa {ten}.",
            SoGioDuKien = 8 + (i % 3) * 4,
            DiemDatToiThieu = 5 + i % 3,                 // 5, 6, 7
            TyLeThamDuToiThieu = 70 + (i % 2) * 10,      // 70, 80
            TrangThai = TrangThaiHoatDong.HoatDong
        }).ToList();
        db.KhoaDaoTaos.AddRange(khoas);

        // ---------- Lớp đào tạo (12 lớp, đủ các tình huống kiểm thử) ----------
        // (chỉ số khóa, ngày bắt đầu so với hôm nay, số ngày, hạn đăng ký so với hôm nay, số lượng tối đa, trạng thái)
        var cauHinhLop = new (int Khoa, int BatDau, int SoNgay, int Han, int Max, string TrangThai)[]
        {
            (0, -60, 6, -65, 25, TrangThaiLop.DaKetThuc),
            (1, -45, 6, -50, 25, TrangThaiLop.DaKetThuc),
            (2, -30, 6, -35, 25, TrangThaiLop.DaKetThuc),
            (3,  -3, 8,  -6, 25, TrangThaiLop.DangHoc),
            (4,  -1, 8,  -4, 25, TrangThaiLop.DangHoc),
            (5,  10, 8,   7, 20, TrangThaiLop.DangNhanDangKy),   // lớp 6: còn chỗ, còn hạn
            (6,  15, 6,  12,  3, TrangThaiLop.DangNhanDangKy),   // lớp 7: sẽ HẾT CHỖ (max = 3)
            (7,   5, 6,  -1, 20, TrangThaiLop.DangNhanDangKy),   // lớp 8: HẾT HẠN đăng ký
            (0,  30, 6,  25, 20, TrangThaiLop.ChuaMoDangKy),
            (1,  20, 6,  15, 20, TrangThaiLop.DaKhoaDangKy),
            (2,  12, 6,   9, 20, TrangThaiLop.DaHuy),
            (3,  12, 6,   9, 20, TrangThaiLop.DangNhanDangKy),   // lớp 12: TRÙNG LỊCH với lớp 6
        };
        var lops = cauHinhLop.Select((c, i) => new LopDaoTao
        {
            KhoaDaoTao = khoas[c.Khoa],
            TenLop = $"{khoas[c.Khoa].TenKhoaDaoTao} - Lớp {i + 1:00}",
            NgayBatDau = homNay.AddDays(c.BatDau),
            NgayKetThuc = homNay.AddDays(c.BatDau + c.SoNgay),
            HanDangKy = homNay.AddDays(c.Han),
            SoLuongToiDa = c.Max,
            DiaDiem = i % 2 == 0 ? "Phòng họp A - Tầng 3" : "Phòng đào tạo B - Tầng 5",
            TrangThai = c.TrangThai
        }).ToList();
        db.LopDaoTaos.AddRange(lops);

        // ---------- Đăng ký ----------
        var dangKys = new List<DangKyDaoTao>();
        void DangKy(int soNv, int soLop, string trangThai, string? ghiChu = null)
        {
            var lop = lops[soLop - 1];
            var ngayDangKy = lop.NgayBatDau.AddDays(-14);
            dangKys.Add(new DangKyDaoTao
            {
                NhanVien = nhanViens[soNv - 1],
                LopDaoTao = lop,
                NgayDangKy = ngayDangKy,
                TrangThai = trangThai,
                NgayDuyet = trangThai == TrangThaiDangKy.DaDuyet ? ngayDangKy.AddDays(1) : null,
                GhiChu = ghiChu
            });
        }
        void DangKyNhieu(int tuNv, int denNv, int soLop, string trangThai)
        {
            for (int nv = tuNv; nv <= denNv; nv++) DangKy(nv, soLop, trangThai);
        }

        DangKyNhieu(1, 6, 1, TrangThaiDangKy.DaDuyet);
        DangKy(7, 1, TrangThaiDangKy.TuChoi, "Không thuộc đối tượng đào tạo của khóa");
        DangKyNhieu(5, 10, 2, TrangThaiDangKy.DaDuyet);
        DangKyNhieu(9, 13, 3, TrangThaiDangKy.DaDuyet);
        DangKyNhieu(1, 4, 4, TrangThaiDangKy.DaDuyet);
        DangKyNhieu(5, 8, 5, TrangThaiDangKy.DaDuyet);
        DangKyNhieu(13, 15, 6, TrangThaiDangKy.DaDuyet);
        DangKyNhieu(16, 17, 6, TrangThaiDangKy.ChoDuyet);
        DangKyNhieu(19, 21, 7, TrangThaiDangKy.DaDuyet);                 // lớp 7 đã đủ 3/3
        DangKy(22, 7, TrangThaiDangKy.ChoDuyet);                         // duyệt sẽ bị chặn vì hết chỗ
        DangKy(1, 8, TrangThaiDangKy.DaHuy, "Nhân viên tự hủy");
        DangKy(13, 12, TrangThaiDangKy.ChoDuyet);                        // duyệt sẽ bị chặn vì trùng lịch với lớp 6
        db.DangKyDaoTaos.AddRange(dangKys);

        // ---------- Buổi học (mỗi lớp 4 buổi, cách nhau 2 ngày) ----------
        var buoiHocs = new List<BuoiHoc>();
        foreach (var lop in lops)
        {
            for (int k = 0; k < 4; k++)
            {
                var ngay = lop.NgayBatDau.AddDays(k * 2);
                buoiHocs.Add(new BuoiHoc
                {
                    LopDaoTao = lop,
                    NgayHoc = ngay,
                    GioBatDau = new TimeOnly(8, 0),
                    GioKetThuc = new TimeOnly(11, 0),
                    NoiDung = $"Buổi {k + 1}: nội dung của {lop.TenLop}",
                    DiaDiem = lop.DiaDiem,
                    TrangThai = lop.TrangThai == TrangThaiLop.DaHuy ? TrangThaiBuoiHoc.DaHuy
                              : ngay < homNay ? TrangThaiBuoiHoc.DaDienRa
                              : TrangThaiBuoiHoc.DuKien
                });
            }
        }
        db.BuoiHocs.AddRange(buoiHocs);

        // ---------- Điểm danh (chỉ các buổi đã diễn ra, đa dạng trạng thái) ----------
        var diemDanhs = new List<DiemDanh>();
        foreach (var dk in dangKys.Where(d => d.TrangThai == TrangThaiDangKy.DaDuyet))
        {
            var buoiDaDienRa = buoiHocs
                .Where(b => b.LopDaoTao == dk.LopDaoTao && b.TrangThai == TrangThaiBuoiHoc.DaDienRa)
                .ToList();
            var viTriNv = nhanViens.IndexOf(dk.NhanVien!);

            for (int k = 0; k < buoiDaDienRa.Count; k++)
            {
                var trangThai = ((viTriNv + k) % 5) switch
                {
                    0 => TrangThaiDiemDanh.VangKhongPhep,
                    1 => TrangThaiDiemDanh.VangCoPhep,
                    2 => TrangThaiDiemDanh.DiMuon,
                    _ => TrangThaiDiemDanh.CoMat
                };
                diemDanhs.Add(new DiemDanh
                {
                    BuoiHoc = buoiDaDienRa[k],
                    NhanVien = dk.NhanVien,
                    TrangThai = trangThai,
                    ThoiGianCapNhat = buoiDaDienRa[k].NgayHoc.AddHours(12)
                });
            }
        }
        db.DiemDanhs.AddRange(diemDanhs);

        // ---------- Kết quả (cho các lớp đã kết thúc) ----------
        foreach (var dk in dangKys.Where(d => d.TrangThai == TrangThaiDangKy.DaDuyet
                                           && d.LopDaoTao!.TrangThai == TrangThaiLop.DaKetThuc))
        {
            var lop = dk.LopDaoTao!;
            var khoa = lop.KhoaDaoTao!;
            var tongBuoi = buoiHocs.Count(b => b.LopDaoTao == lop && b.TrangThai != TrangThaiBuoiHoc.DaHuy);
            var soCoMat = diemDanhs.Count(d => d.NhanVien == dk.NhanVien && d.BuoiHoc!.LopDaoTao == lop
                                            && (d.TrangThai == TrangThaiDiemDanh.CoMat || d.TrangThai == TrangThaiDiemDanh.DiMuon));
            var tyLe = QuyTacNghiepVu.TinhTyLeThamDu(soCoMat, tongBuoi);

            var viTriNv = nhanViens.IndexOf(dk.NhanVien!);
            decimal diem = 4 + (viTriNv * 3) % 7;     // 4..10, đa dạng để có cả Hoàn thành / Không hoàn thành
            var daXacNhan = viTriNv % 3 != 0;

            db.KetQuaDaoTaos.Add(new KetQuaDaoTao
            {
                DangKyDaoTao = dk,
                DiemDanhGia = diem,
                TyLeThamDu = tyLe,
                KetQua = QuyTacNghiepVu.XacDinhKetQua(diem, tyLe, khoa.DiemDatToiThieu, khoa.TyLeThamDuToiThieu),
                NgayDanhGia = lop.NgayKetThuc.AddDays(1),
                DaXacNhan = daXacNhan,
                NgayXacNhan = daXacNhan ? lop.NgayKetThuc.AddDays(2) : null
            });
        }

        db.SaveChanges();
    }
}
