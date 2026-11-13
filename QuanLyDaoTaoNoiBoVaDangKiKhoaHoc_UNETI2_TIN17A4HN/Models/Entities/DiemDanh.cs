using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo.Models.Entities;

public class DiemDanh
{
    [Key]
    public int MaDiemDanh { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int MaBuoiHoc { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int MaNhanVien { get; set; }

    [Required, StringLength(30)]
    [RegularExpression("^(CoMat|VangMat|DiMuon|CoPhep)$")]
    public string TrangThai { get; set; } = "VangMat";

    [Required]
    public DateTime ThoiGianCapNhat { get; set; }

    [StringLength(1000)]
    public string? GhiChu { get; set; }

    [ForeignKey(nameof(MaBuoiHoc))]
    public BuoiHoc BuoiHoc { get; set; } = null!;

    [ForeignKey(nameof(MaNhanVien))]
    public NhanVien NhanVien { get; set; } = null!;
}
