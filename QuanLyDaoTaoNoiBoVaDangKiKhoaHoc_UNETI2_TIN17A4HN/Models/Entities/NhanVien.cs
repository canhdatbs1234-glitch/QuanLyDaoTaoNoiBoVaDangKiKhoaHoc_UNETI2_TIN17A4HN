using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

1// Họ và tên: Phí Bảo Nam
// Mã sinh viên: 23103100236
// Nội dung thực hiện: Khởi tạo Entity và DbContext

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo.Models.Entities;

public class NhanVien
{
    [Key]
    public int MaNhanVien { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int MaTaiKhoan { get; set; }

    [Required, StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int MaPhongBan { get; set; }

    [Required, DataType(DataType.Date)]
    public DateOnly NgaySinh { get; set; }

    [StringLength(20), Phone]
    public string? SoDienThoai { get; set; }

    [Required, StringLength(254), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateOnly NgayVaoLam { get; set; }

    [Required]
    public bool TrangThai { get; set; } = true;

    [ForeignKey(nameof(MaTaiKhoan))]
    public TaiKhoan TaiKhoan { get; set; } = null!;

    [ForeignKey(nameof(MaPhongBan))]
    public PhongBan PhongBan { get; set; } = null!;

    public ICollection<DangKyDaoTao> DangKyDaoTaos { get; set; } = new List<DangKyDaoTao>();
    public ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();
}
