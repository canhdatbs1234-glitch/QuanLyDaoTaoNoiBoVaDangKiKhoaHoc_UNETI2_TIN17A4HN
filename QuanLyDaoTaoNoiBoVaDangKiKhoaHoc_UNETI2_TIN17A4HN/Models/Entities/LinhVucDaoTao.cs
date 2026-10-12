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
