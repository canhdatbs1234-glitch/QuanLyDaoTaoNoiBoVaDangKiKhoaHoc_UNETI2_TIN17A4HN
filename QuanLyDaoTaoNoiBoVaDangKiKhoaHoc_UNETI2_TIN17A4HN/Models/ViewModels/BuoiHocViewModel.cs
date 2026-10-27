// Họ và tên: Phạm Việt Anh
// Mã sinh viên: 23103100231
// Nội dung thực hiện: Module 4 - Buổi học, Lịch học và Điểm danh


using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class BuoiHocViewModel : IValidatableObject
{
    public int MaBuoiHoc { get; set; }
    [Range(1, int.MaxValue), Display(Name = "Lớp đào tạo")]
    public int MaLop { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn ngày học."), DataType(DataType.Date), Display(Name = "Ngày học")]
    public DateOnly? NgayHoc { get; set; }
    [Required, DataType(DataType.Time), Display(Name = "Giờ bắt đầu")]
    public TimeOnly? GioBatDau { get; set; }
    [Required, DataType(DataType.Time), Display(Name = "Giờ kết thúc")]
    public TimeOnly? GioKetThuc { get; set; }
    [Required, StringLength(2000), Display(Name = "Nội dung")]
    public string NoiDung { get; set; } = "";
    [Required, StringLength(250), Display(Name = "Địa điểm")]
    public string DiaDiem { get; set; } = "";
    [Required, RegularExpression("^(ChuaDienRa|DaDienRa|DaHuy)$"), Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "ChuaDienRa";
    [ValidateNever] public string TenLop { get; set; } = "";
    [ValidateNever] public string TenKhoaDaoTao { get; set; } = "";
    [ValidateNever] public string TrangThaiLop { get; set; } = "";
    [ValidateNever] public IEnumerable<SelectListItem> Lops { get; set; } = [];
    public string TenTrangThai => TrangThai switch
    {
        "DaHuy" => "Đã hủy",
        "DaDienRa" => "Đã diễn ra",
        _ => "Chưa diễn ra"
    };
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (GioBatDau.HasValue && GioKetThuc.HasValue && GioKetThuc <= GioBatDau)
            yield return new("Giờ kết thúc phải sau giờ bắt đầu.", [nameof(GioKetThuc)]);
    }
}

public class LichHocViewModel
{
    public int? MaLop { get; set; }
    public IEnumerable<SelectListItem> Lops { get; set; } = [];
    public IReadOnlyList<BuoiHocViewModel> BuoiHocs { get; set; } = [];
}

1