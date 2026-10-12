using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo.Models.Entities;

public class DangKyDaoTao : IValidatableObject
{
    [Key]
    public int MaDangKy { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int MaNhanVien { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int MaLop { get; set; }

    [Required]
    public DateTime NgayDangKy { get; set; }

    [Required, StringLength(30)]
    [RegularExpression("^(ChoDuyet|DaDuyet|TuChoi|DaHuy)$")]
    public string TrangThai { get; set; } = "ChoDuyet";

    public DateTime? NgayDuyet { get; set; }

    [StringLength(1000)]
    public string? GhiChu { get; set; }

    [ForeignKey(nameof(MaNhanVien))]
    public NhanVien NhanVien { get; set; } = null!;

    [ForeignKey(nameof(MaLop))]
    public LopDaoTao LopDaoTao { get; set; } = null!;

    public KetQuaDaoTao? KetQuaDaoTao { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NgayDuyet.HasValue && NgayDuyet.Value < NgayDangKy)
            yield return new ValidationResult("Ngày duyệt không được trước ngày đăng ký.", [nameof(NgayDuyet)]);
    }
}
