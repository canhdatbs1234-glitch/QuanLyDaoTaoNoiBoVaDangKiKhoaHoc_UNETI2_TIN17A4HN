// Họ và tên: Phạm Việt Anh
// Mã sinh viên: 23103100231
// Nội dung thực hiện: Module 4 - Buổi học, Lịch học và Điểm danh

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class DiemDanhViewModel
{
    [Range(1, int.MaxValue)]
    public int MaBuoiHoc { get; set; }
    [ValidateNever] public int MaLop { get; set; }
    [ValidateNever] public string TenLop { get; set; } = "";
    [ValidateNever] public DateOnly NgayHoc { get; set; }
    [ValidateNever] public TimeOnly GioBatDau { get; set; }
    [ValidateNever] public TimeOnly GioKetThuc { get; set; }
    [ValidateNever] public string NoiDung { get; set; } = "";
    [ValidateNever] public bool ChoPhepLuu { get; set; }
    [ValidateNever] public string? LyDoKhongChoLuu { get; set; }
    [Required]
    public List<DiemDanhHocVienViewModel> HocViens { get; set; } = [];
}

public class DiemDanhHocVienViewModel
{
    [Range(1, int.MaxValue)]
    public int MaNhanVien { get; set; }
    [ValidateNever] public string HoTen { get; set; } = "";
    [ValidateNever] public string TenPhongBan { get; set; } = "";
    [Required(ErrorMessage = "Chọn trạng thái điểm danh.")]
    [RegularExpression("^(CoMat|CoPhep|VangMat)$", ErrorMessage = "Trạng thái điểm danh không hợp lệ.")]
    public string TrangThai { get; set; } = "";
    [StringLength(1000)]
    public string? GhiChu { get; set; }
    // Giá trị gốc dùng phát hiện người khác đã sửa sau khi mở form.
    public DateTime? ThoiGianCapNhat { get; set; }
    public static IReadOnlyDictionary<string, string> TrangThais { get; } = new Dictionary<string, string>
    {
        ["CoMat"] = "Có mặt",
        ["CoPhep"] = "Vắng có phép",
        ["VangMat"] = "Vắng không phép"
    };
}

1