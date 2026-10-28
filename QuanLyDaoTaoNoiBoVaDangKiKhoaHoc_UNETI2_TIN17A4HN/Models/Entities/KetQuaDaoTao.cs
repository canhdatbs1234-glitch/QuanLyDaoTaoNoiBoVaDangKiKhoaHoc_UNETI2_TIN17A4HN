// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 232103100197
// Nội dung thực hiện: Module 5 - Kết quả đào tạo, xác nhận hoàn thành, lịch sử đào tạo, Dashboard và thống kê.
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
    public string KetQua { get; set; } = KetQuaConst.ChuaDanhGia;

    public DateTime? NgayDanhGia { get; set; }

    [StringLength(1000)]
    public string? GhiChu { get; set; }

    // ===== MỚI (Mục 9.4): xác nhận kết quả =====
    /// <summary>true = Admin/Nhân sự đã kiểm tra và xác nhận; sau đó hạn chế sửa trực tiếp.</summary>
    public bool DaXacNhan { get; set; }
    public DateTime? NgayXacNhan { get; set; }
    public int? MaNguoiXacNhan { get; set; }

    [ForeignKey(nameof(MaDangKy))]
    public DangKyDaoTao DangKyDaoTao { get; set; } = null!;

    [ForeignKey(nameof(MaNguoiXacNhan))]
    public TaiKhoan? NguoiXacNhan { get; set; }
}

/// <summary>Hằng số kết quả. Giá trị lưu DB là Dat/KhongDat, hiển thị là Hoàn thành/Không hoàn thành.</summary>
public static class KetQuaConst
{
    public const string ChuaDanhGia = "ChuaDanhGia";
    public const string HoanThanh = "Dat";
    public const string KhongHoanThanh = "KhongDat";

    public static readonly IReadOnlyDictionary<string, string> TenKetQua = new Dictionary<string, string>
    {
        [ChuaDanhGia] = "Chưa đánh giá",
        [HoanThanh] = "Hoàn thành",
        [KhongHoanThanh] = "Không hoàn thành"
    };

    public static string Ten(string? ma) =>
        ma is not null && TenKetQua.TryGetValue(ma, out var ten) ? ten : "Không xác định";
}