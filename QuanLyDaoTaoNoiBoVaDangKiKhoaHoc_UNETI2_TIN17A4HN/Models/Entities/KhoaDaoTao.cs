using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo.Models.Entities;

public class KhoaDaoTao
{
    [Key]
    public int MaKhoaDaoTao { get; set; }

    [Required, StringLength(200)]
    public string TenKhoaDaoTao { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int MaLinhVuc { get; set; }

    [StringLength(2000)]
    public string? MoTa { get; set; }

    [Required, Range(typeof(decimal), "0.01", "9999.99")]
    public decimal SoGioDuKien { get; set; }

    [Required, Range(typeof(decimal), "0", "10")]
    public decimal DiemDatToiThieu { get; set; } = 5m;

    [Required, Range(typeof(decimal), "0", "100")]
    public decimal TyLeThamDuToiThieu { get; set; } = 80m;

    [Required]
    public bool TrangThai { get; set; } = true;

    [ForeignKey(nameof(MaLinhVuc))]
    public LinhVucDaoTao LinhVucDaoTao { get; set; } = null!;

    public ICollection<LopDaoTao> LopDaoTaos { get; set; } = new List<LopDaoTao>();
}
