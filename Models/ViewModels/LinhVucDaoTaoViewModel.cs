//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using System.ComponentModel.DataAnnotations;
namespace QuanLyDaoTaoNoiBo.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
public class LinhVucDaoTaoViewModel
{
    public int MaLinhVuc { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên lĩnh vực."), StringLength(150, ErrorMessage = "Tên lĩnh vực tối đa 150 ký tự.")]
    [Display(Name = "Tên lĩnh vực")]
    public string TenLinhVuc { get; set; } = "";

    [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool TrangThai { get; set; } = true;
}
