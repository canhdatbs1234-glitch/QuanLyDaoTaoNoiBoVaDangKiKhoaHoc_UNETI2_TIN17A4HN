// Họ và tên: [SV1 - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: ViewModel đăng nhập.

using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
