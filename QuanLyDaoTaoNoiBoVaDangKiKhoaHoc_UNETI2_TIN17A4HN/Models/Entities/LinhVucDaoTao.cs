//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using System.ComponentModel.DataAnnotations;
namespace QuanLyDaoTaoNoiBo.Models.Entities;

public class LinhVucDaoTao
{
    [Key]
    public int MaLinhVuc { get; set; }
    [Required, StringLength(150)]
    public string TenLinhVuc { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? MoTa { get; set; }
    [Required]
    public bool TrangThai { get; set; } = true;
    public ICollection<KhoaDaoTao> KhoaDaoTaos { get; set; } = new List<KhoaDaoTao>();
}
