//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
namespace QuanLyDaoTaoNoiBo.Models;

/// <summary>Các vai trò hợp lệ của tài khoản (khớp CHECK constraint trong database).</summary>
public static class VaiTroTaiKhoan
{
    public const string QuanTri = "QuanTri";       // Admin
    public const string QuanLy = "QuanLy";         // Nhân sự - Đào tạo
    public const string NhanVien = "NhanVien";     // Nhân viên
    public const string GiangVien = "GiangVien";   // Giảng viên / Người phụ trách lớp

    public static readonly string[] TatCa = [QuanTri, QuanLy, NhanVien, GiangVien];

    public static bool HopLe(string? vaiTro) => vaiTro is QuanTri or QuanLy or NhanVien or GiangVien;

    public static string Ten(string? vaiTro) => vaiTro switch
    {
        QuanTri => "Quản trị viên",
        QuanLy => "Nhân sự - Đào tạo",
        GiangVien => "Giảng viên",
        NhanVien => "Nhân viên",
        _ => "Không xác định"
    };
}
