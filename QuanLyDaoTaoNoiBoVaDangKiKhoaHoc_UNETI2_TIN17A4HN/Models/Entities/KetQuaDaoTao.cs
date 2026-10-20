// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Khởi tạo Entity và DbContext

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo.Models.Entities;

public class KetQuaDaoTao
{
    [Key]
    public int MaKetQua { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int MaDangKy { get; set; }

    [Range(typeof(decimal), "0", "10")]
    public decimal? DiemDanhGia { get; set; }

    [Required, Range(typeof(decimal), "0", "100")]
    public decimal TyLeThamDu { get; set; }

    [Required, StringLength(30)]
    [RegularExpression("^(ChuaDanhGia|Dat|KhongDat)$")]
    public string KetQua { get; set; } = "ChuaDanhGia";

    public DateTime? NgayDanhGia { get; set; }

    [StringLength(1000)]
    public string? GhiChu { get; set; }

    [ForeignKey(nameof(MaDangKy))]
    public DangKyDaoTao DangKyDaoTao { get; set; } = null!;
}
