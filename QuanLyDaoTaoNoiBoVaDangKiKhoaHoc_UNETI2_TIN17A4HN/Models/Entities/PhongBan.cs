//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using System.ComponentModel.DataAnnotations;
namespace QuanLyDaoTaoNoiBo.Models.Entities;
public class PhongBan
{
    [Key]
    public int MaPhongBan { get; set; }
    [Required, StringLength(100)]
    public string TenPhongBan { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? MoTa { get; set; }
    [Required]
    public bool TrangThai { get; set; } = true;
    public ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}
