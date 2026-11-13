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
        TaoTaiKhoanNeuChuaCo(context, "admin", "Admin@123", "Quản trị viên UNETI", "admin@uneti.example", VaiTroTaiKhoan.QuanTri, true);
        TaoTaiKhoanNeuChuaCo(context, "nhansu", "NhanSu@123", "Chuyên viên Nhân sự - Đào tạo", "nhansu@uneti.example", VaiTroTaiKhoan.QuanLy, true);
        TaoTaiKhoanNeuChuaCo(context, "nhanvien", "NhanVien@123", "Nguyễn Văn Nhân Viên", "nhanvien@uneti.example", VaiTroTaiKhoan.NhanVien, true);
        TaoTaiKhoanNeuChuaCo(context, "giangvien", "GiangVien@123", "Trần Thị Giảng Viên", "giangvien@uneti.example", VaiTroTaiKhoan.GiangVien, true);
        TaoTaiKhoanNeuChuaCo(context, "nhanvienkhoa", "NhanVien@123", "Lê Văn Bị Khóa", "nhanvienkhoa@uneti.example", VaiTroTaiKhoan.NhanVien, false);

        context.SaveChanges();
    }

    private static void TaoTaiKhoanNeuChuaCo(ApplicationDbContext context, string tenDangNhap, string matKhau,
        string hoTen, string email, string vaiTro, bool trangThai)
    {
        // Không ghi đè mật khẩu, vai trò hay thông tin đã được chỉnh sửa.
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
