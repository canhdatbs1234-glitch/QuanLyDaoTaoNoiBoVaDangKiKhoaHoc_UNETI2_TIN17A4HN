// Họ và tên: [Cả nhóm / nhóm trưởng - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: Hằng số dùng chung: khóa Session, vai trò, các trạng thái nghiệp vụ.

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Helpers;

public static class SessionKeys
{
    public const string MaTaiKhoan = "MaTaiKhoan";
    public const string HoTen = "HoTen";
    public const string VaiTro = "VaiTro";
}

public static class QuyenHan
{
    public const string Admin = "Admin";           // Admin / Nhân sự - Đào tạo
    public const string NhanVien = "NhanVien";
    public const string GiangVien = "GiangVien";   // tùy chọn: Giảng viên / Người phụ trách lớp
}

public static class TrangThaiTaiKhoan
{
    public const string HoatDong = "Hoạt động";
    public const string BiKhoa = "Bị khóa";
}

// Dùng cho PhongBan, LinhVucDaoTao, KhoaDaoTao
public static class TrangThaiHoatDong
{
    public const string HoatDong = "Hoạt động";
    public const string NgungHoatDong = "Ngừng hoạt động";
    public static readonly string[] TatCa = { HoatDong, NgungHoatDong };
}

public static class TrangThaiNhanVien
{
    public const string DangLamViec = "Đang làm việc";
    public const string NghiViec = "Nghỉ việc";
    public static readonly string[] TatCa = { DangLamViec, NghiViec };
}

public static class TrangThaiLop
{
    public const string ChuaMoDangKy = "Chưa mở đăng ký";
    public const string DangNhanDangKy = "Đang nhận đăng ký";
    public const string DaKhoaDangKy = "Đã khóa đăng ký";
    public const string DangHoc = "Đang học";
    public const string DaKetThuc = "Đã kết thúc";
    public const string DaHuy = "Đã hủy";
    public static readonly string[] TatCa = { ChuaMoDangKy, DangNhanDangKy, DaKhoaDangKy, DangHoc, DaKetThuc, DaHuy };
}

public static class TrangThaiDangKy
{
    public const string ChoDuyet = "Chờ duyệt";
    public const string DaDuyet = "Đã duyệt";
    public const string TuChoi = "Từ chối";
    public const string DaHuy = "Đã hủy";
    public static readonly string[] TatCa = { ChoDuyet, DaDuyet, TuChoi, DaHuy };
}

public static class TrangThaiBuoiHoc
{
    public const string DuKien = "Dự kiến";
    public const string DaDienRa = "Đã diễn ra";
    public const string DaHuy = "Đã hủy";
    public static readonly string[] TatCa = { DuKien, DaDienRa, DaHuy };
}

public static class TrangThaiDiemDanh
{
    public const string CoMat = "Có mặt";
    public const string VangCoPhep = "Vắng có phép";
    public const string VangKhongPhep = "Vắng không phép";
    public const string DiMuon = "Đi muộn";
    public static readonly string[] TatCa = { CoMat, VangCoPhep, VangKhongPhep, DiMuon };
}

public static class KetQuaHoanThanh
{
    public const string HoanThanh = "Hoàn thành";
    public const string KhongHoanThanh = "Không hoàn thành";
}

/// <summary>
/// Các quy định do NHÓM tự thống nhất (đề bài yêu cầu ghi rõ trong báo cáo và chương trình).
/// Sửa ở đây một chỗ, toàn hệ thống áp dụng thống nhất.
/// </summary>
public static class QuyDinhNhom
{
    public const decimal DiemToiDa = 10m;                            // thang điểm 0..10
    public static readonly bool DiMuonTinhLaCoMat = true;            // Đi muộn có tính là có mặt không?
    public static readonly bool VangCoPhepKhongTinhVaoTong = false;  // true: bỏ buổi vắng có phép khỏi mẫu số
}
