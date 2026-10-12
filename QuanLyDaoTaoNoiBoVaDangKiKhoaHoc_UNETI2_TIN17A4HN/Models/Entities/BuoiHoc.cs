using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo.Models.Entities;

public class BuoiHoc : IValidatableObject
{
    [Key]
    public int MaBuoiHoc { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int MaLop { get; set; }

    [Required, DataType(DataType.Date)]
    public DateOnly NgayHoc { get; set; }

    [Required, DataType(DataType.Time)]
    public TimeOnly GioBatDau { get; set; }

    [Required, DataType(DataType.Time)]
    public TimeOnly GioKetThuc { get; set; }

    [Required, StringLength(2000)]
    public string NoiDung { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string DiaDiem { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [RegularExpression("^(ChuaDienRa|DaDienRa|DaHuy)$")]
    public string TrangThai { get; set; } = "ChuaDienRa";

    [ForeignKey(nameof(MaLop))]
    public LopDaoTao LopDaoTao { get; set; } = null!;

    public ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (GioKetThuc <= GioBatDau)
            yield return new ValidationResult("Giờ kết thúc phải sau giờ bắt đầu trong cùng ngày.", [nameof(GioKetThuc)]);
    }
}
