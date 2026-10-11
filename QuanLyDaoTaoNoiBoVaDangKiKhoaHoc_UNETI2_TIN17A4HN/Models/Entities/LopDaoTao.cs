// Họ và tên: Trần Trung Hiếu
// Mã sinh viên: 23103100217
// Nội dung thực hiện: Khởi tạo Entity 

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo.Models.Entities;

public class LopDaoTao : IValidatableObject
{
    [Key]
    public int MaLop { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int MaKhoaDaoTao { get; set; }

    [Required, StringLength(200)]
    public string TenLop { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateOnly NgayBatDau { get; set; }

    [Required, DataType(DataType.Date)]
    public DateOnly NgayKetThuc { get; set; }

    [Required]
    public DateTime HanDangKy { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int SoLuongToiDa { get; set; }

    [Required, StringLength(250)]
    public string DiaDiem { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [RegularExpression("^(MoDangKy|DongDangKy|DangHoc|HoanThanh|DaHuy)$")]
    public string TrangThai { get; set; } = "MoDangKy";

    [ForeignKey(nameof(MaKhoaDaoTao))]
    public KhoaDaoTao KhoaDaoTao { get; set; } = null!;

    public int? MaGiangVien { get; set; }
    [ForeignKey(nameof(MaGiangVien))]
    public virtual TaiKhoan? GiangVien { get; set; }

    public ICollection<DangKyDaoTao> DangKyDaoTaos { get; set; } = new List<DangKyDaoTao>();
    public ICollection<BuoiHoc> BuoiHocs { get; set; } = new List<BuoiHoc>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NgayKetThuc < NgayBatDau)
            yield return new ValidationResult("Ngày kết thúc phải từ ngày bắt đầu trở đi.", [nameof(NgayKetThuc)]);
        if (DateOnly.FromDateTime(HanDangKy) > NgayBatDau)
            yield return new ValidationResult("Hạn đăng ký không được sau ngày bắt đầu.", [nameof(HanDangKy)]);
    }
}
