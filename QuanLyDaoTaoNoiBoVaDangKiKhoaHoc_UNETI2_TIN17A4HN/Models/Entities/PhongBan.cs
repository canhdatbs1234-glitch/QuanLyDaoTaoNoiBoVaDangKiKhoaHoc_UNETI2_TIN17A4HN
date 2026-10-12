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
