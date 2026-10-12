//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class TaiKhoanViewModel
{
    public int MaTaiKhoan { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập."), StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự.")]
    [RegularExpression(@"^\S+$", ErrorMessage = "Tên đăng nhập không được chứa khoảng trắng.")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = "";
    [StringLength(500, ErrorMessage = "Mật khẩu tối đa 500 ký tự.")]
    [DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string? MatKhau { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên."), StringLength(100)]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập email."), StringLength(254)]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
    [RegularExpression("^(QuanTri|QuanLy|NhanVien|GiangVien)$", ErrorMessage = "Vai trò không hợp lệ.")]
    [Display(Name = "Vai trò")]
    public string VaiTro { get; set; } = "NhanVien";

    [Display(Name = "Đang hoạt động (không bị khóa)")]
    public bool TrangThai { get; set; } = true;
}