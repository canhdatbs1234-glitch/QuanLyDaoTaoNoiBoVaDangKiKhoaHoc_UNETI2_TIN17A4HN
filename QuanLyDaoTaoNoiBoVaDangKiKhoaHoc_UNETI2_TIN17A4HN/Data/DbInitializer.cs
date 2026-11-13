//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Models;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Services;
namespace QuanLyDaoTaoNoiBo.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Chỉ áp dụng migrations; tuyệt đối không xóa database để giữ dữ liệu đã nhập.
        context.Database.Migrate();

        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name = 'IX_NhanVien_MaTaiKhoan' 
                      AND object_id = OBJECT_ID('dbo.NhanVien') 
                      AND has_filter = 0
                )
                BEGIN
                    DROP INDEX [IX_NhanVien_MaTaiKhoan] ON [dbo].[NhanVien];
                END;

                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'NhanVien' AND COLUMN_NAME = 'MaTaiKhoan' AND IS_NULLABLE = 'NO'
                )
                BEGIN
                    ALTER TABLE [dbo].[NhanVien] ALTER COLUMN [MaTaiKhoan] INT NULL;
                END;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes 
                    WHERE name = 'IX_NhanVien_MaTaiKhoan' 
                      AND object_id = OBJECT_ID('dbo.NhanVien')
                )
                BEGIN
                    CREATE UNIQUE NONCLUSTERED INDEX [IX_NhanVien_MaTaiKhoan] 
                    ON [dbo].[NhanVien]([MaTaiKhoan]) 
                    WHERE [MaTaiKhoan] IS NOT NULL;
                END;");
        }
        catch { }

        var phongBanMau = new[]
        {
            (Ten: "Phòng Công nghệ thông tin", MoTa: "Quản lý hạ tầng và phát triển phần mềm."),
            (Ten: "Phòng Nhân sự", MoTa: "Quản lý nhân sự và đào tạo nội bộ."),
            (Ten: "Phòng Kế toán", MoTa: "Quản lý tài chính, kế toán."),
            (Ten: "Phòng Đào tạo", MoTa: "Tổ chức và quản lý các chương trình đào tạo.")
        };
        foreach (var item in phongBanMau)
        {
            if (!context.PhongBans.Any(x => x.TenPhongBan == item.Ten))
                context.PhongBans.Add(new PhongBan { TenPhongBan = item.Ten, MoTa = item.MoTa });
        }

        var linhVucMau = new[]
        {
            (Ten: "Công nghệ thông tin", MoTa: "Lập trình, cơ sở dữ liệu và chuyển đổi số."),
            (Ten: "Kỹ năng mềm", MoTa: "Giao tiếp, làm việc nhóm và quản lý thời gian."),
            (Ten: "Quản lý và lãnh đạo", MoTa: "Quản lý đội ngũ và điều hành công việc."),
            (Ten: "Ngoại ngữ", MoTa: "Ngoại ngữ phục vụ công việc.")
        };
        foreach (var item in linhVucMau)
        {
            if (!context.LinhVucDaoTaos.Any(x => x.TenLinhVuc == item.Ten))
                context.LinhVucDaoTaos.Add(new LinhVucDaoTao { TenLinhVuc = item.Ten, MoTa = item.MoTa });
        }

        // Tài khoản mẫu (mật khẩu được băm trước khi lưu).
        TaoTaiKhoanNeuChuaCo(context, "admin", "Admin@123", "Quản trị viên UNETI", "admin@uneti.example", "QuanTri", true);
        TaoTaiKhoanNeuChuaCo(context, "nhansu", "NhanSu@123", "Chuyên viên Nhân sự - Đào tạo", "nhansu@uneti.example", "QuanTri", true);
        TaoTaiKhoanNeuChuaCo(context, "nhanvien", "NhanVien@123", "Nguyễn Văn Nhân Viên", "nhanvien@uneti.example", "NhanVien", true);
        TaoTaiKhoanNeuChuaCo(context, "giangvien", "GiangVien@123", "Trần Thị Giảng Viên", "giangvien@uneti.example", "GiangVien", true);
        TaoTaiKhoanNeuChuaCo(context, "nhanvienkhoa", "NhanVien@123", "Lê Văn Bị Khóa", "nhanvienkhoa@uneti.example", "NhanVien", false);

        context.SaveChanges();
        LopDaoTaoRules.SuaLopVuotSucChua(context); // Module 2: không để số chỗ còn âm
        TaoBuoiHocMau(context);
        TaoDiemDanhMau(context);
    }
    private static void TaoBuoiHocMau(ApplicationDbContext context)
    {
        // Chỉ nạp khi bảng BuoiHoc đang trống để không ghi đè dữ liệu đã nhập.
        if (context.BuoiHocs.Any()) return;

        var bayGio = DateTime.UtcNow.AddHours(7);
        var khungGio = new[]
        {
            (Bd: new TimeOnly(8, 0),   Kt: new TimeOnly(10, 0)),
            (Bd: new TimeOnly(10, 30), Kt: new TimeOnly(12, 0)),
            (Bd: new TimeOnly(13, 30), Kt: new TimeOnly(15, 30))
        };
        var noiDung = new[]
        {
            "Giới thiệu và mục tiêu khóa học",
            "Nội dung chuyên đề chính",
            "Thực hành và tổng kết"
        };

        var lops = context.LopDaoTaos.Where(x => x.TrangThai != "DaHuy").ToList();
        foreach (var lop in lops)
        {
            var soNgay = lop.NgayKetThuc.DayNumber - lop.NgayBatDau.DayNumber;
            for (var i = 0; i < 3; i++)
            {
                // Buổi 1 ở ngày bắt đầu, buổi 2 ở giữa, buổi 3 ở ngày kết thúc của lớp.
                var ngay = lop.NgayBatDau.AddDays(soNgay * i / 2);
                var gio = khungGio[i];
                context.BuoiHocs.Add(new BuoiHoc
                {
                    MaLop = lop.MaLop,
                    NgayHoc = ngay,
                    GioBatDau = gio.Bd,
                    GioKetThuc = gio.Kt,
                    NoiDung = $"Buổi {i + 1}: {noiDung[i]}",
                    DiaDiem = lop.DiaDiem,
                    TrangThai = ngay.ToDateTime(gio.Kt) <= bayGio ? "DaDienRa" : "ChuaDienRa"
                });
            }
        }
        context.SaveChanges();
    }

    private static void TaoDiemDanhMau(ApplicationDbContext context)
    {
        if (context.DiemDanhs.Any()) return;

        var trangThai = new[] { "CoMat", "CoMat", "CoPhep", "VangMat" };
        var dem = 0;

        var buoiDaQua = context.BuoiHocs
            .Where(x => x.TrangThai == "DaDienRa"
                && !context.KetQuaDaoTaos.Any(k => k.DangKyDaoTao.MaLop == x.MaLop && k.NgayDanhGia != null))
            .ToList();

        foreach (var buoi in buoiDaQua)
        {
            var nhanViens = context.DangKyDaoTaos
                .Where(x => x.MaLop == buoi.MaLop && x.TrangThai == "DaDuyet")
                .Select(x => x.MaNhanVien)
                .ToList();

            foreach (var maNhanVien in nhanViens)
            {
                context.DiemDanhs.Add(new DiemDanh
                {
                    MaBuoiHoc = buoi.MaBuoiHoc,
                    MaNhanVien = maNhanVien,
                    TrangThai = trangThai[dem++ % trangThai.Length],
                    ThoiGianCapNhat = buoi.NgayHoc.ToDateTime(buoi.GioKetThuc)
                });
            }
        }
        context.SaveChanges();
    }

    private static void TaoTaiKhoanNeuChuaCo(ApplicationDbContext context, string tenDangNhap, string matKhau,
        string hoTen, string email, string vaiTro, bool trangThai)
    {
        if (context.TaiKhoans.Any(x => x.TenDangNhap == tenDangNhap || x.Email == email)) return;

        var taiKhoan = new TaiKhoan
        {
            TenDangNhap = tenDangNhap,
            HoTen = hoTen,
            Email = email,
            VaiTro = vaiTro,
            TrangThai = trangThai
        };
        taiKhoan.MatKhau = MatKhauHelper.Bam(taiKhoan, matKhau);
        context.TaiKhoans.Add(taiKhoan);
    }
}
