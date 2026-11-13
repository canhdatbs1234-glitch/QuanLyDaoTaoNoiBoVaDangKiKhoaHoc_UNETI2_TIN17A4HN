//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class PhongBanViewModel
{
    public int MaPhongBan { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên phòng ban."), StringLength(100, ErrorMessage = "Tên phòng ban tối đa 100 ký tự.")]
    [Display(Name = "Tên phòng ban")]
    public string TenPhongBan { get; set; } = "";

    [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool TrangThai { get; set; } = true;
}
