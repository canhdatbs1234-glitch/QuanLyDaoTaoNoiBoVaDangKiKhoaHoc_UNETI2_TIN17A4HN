//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using System.ComponentModel.DataAnnotations;
namespace QuanLyDaoTaoNoiBo.Models.Entities;
public class TaiKhoan
{
    [Key]
    public int MaTaiKhoan { get; set; }

    [Required, StringLength(50)]
    public string TenDangNhap { get; set; } = string.Empty;
    [Required, StringLength(500)]
    public string MatKhau { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required, StringLength(254), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [RegularExpression("^(QuanTri|QuanLy|NhanVien|GiangVien)$")]
    public string VaiTro { get; set; } = "NhanVien";

    [Required]
    public bool TrangThai { get; set; } = true;

    public NhanVien? NhanVien { get; set; }
}
