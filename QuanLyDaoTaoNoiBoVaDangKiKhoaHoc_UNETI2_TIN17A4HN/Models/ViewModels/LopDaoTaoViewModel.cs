// Họ và tên: Trần Trung Hiếu
// Mã sinh viên: 23103100217
// Nội dung thực hiện: Module 2 - Quản lý Khóa đào tạo và Lớp đào tạo

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class LopDaoTaoViewModel : IValidatableObject
{
    public int MaLop { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khóa đào tạo."), Display(Name = "Khóa đào tạo")]
    public int MaKhoaDaoTao { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên lớp."), StringLength(200), Display(Name = "Tên lớp")]
    public string TenLop { get; set; } = "";
    [ValidateNever]
    public string TenKhoaDaoTao { get; set; } = "";
    [ValidateNever]
    public string TenLinhVuc { get; set; } = "";
    [Required, DataType(DataType.Date), Display(Name = "Ngày bắt đầu")]
    public DateOnly NgayBatDau { get; set; }
    [Required, DataType(DataType.Date), Display(Name = "Ngày kết thúc")]
    public DateOnly NgayKetThuc { get; set; }
    [Required, Display(Name = "Hạn đăng ký")]
    public DateTime HanDangKy { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Sức chứa phải lớn hơn 0."), Display(Name = "Số lượng tối đa")]
    public int SoLuongToiDa { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập địa điểm."), StringLength(250), Display(Name = "Địa điểm")]
    public string DiaDiem { get; set; } = "";
    [Required, RegularExpression("^(MoDangKy|DongDangKy|DangHoc|HoanThanh|DaHuy)$"), Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "MoDangKy";
    // Giá trị này được truy vấn từ DB, không dùng giá trị client gửi khi lưu.
    public int SoDaDuyet { get; set; }
    public int SoChoCon => SoLuongToiDa - SoDaDuyet;
    [Display(Name = "Giảng viên phụ trách")]
    public int? MaGiangVien { get; set; }
    [ValidateNever]
    public string TenGiangVien { get; set; } = "";

    [ValidateNever]
    public IEnumerable<SelectListItem> KhoaDaoTaos { get; set; } = [];
    [ValidateNever]
    public IEnumerable<SelectListItem> GiangViens { get; set; } = [];

    public static IReadOnlyDictionary<string, string> TrangThais { get; } = new Dictionary<string, string>
    {
        ["MoDangKy"] = "Mở đăng ký",
        ["DongDangKy"] = "Đóng đăng ký",
        ["DangHoc"] = "Đang học",
        ["HoanThanh"] = "Hoàn thành",
        ["DaHuy"] = "Đã hủy"
    };
    public string TenTrangThai => TrangThais.GetValueOrDefault(TrangThai, TrangThai);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NgayBatDau == default)
            yield return new("Vui lòng nhập ngày bắt đầu.", [nameof(NgayBatDau)]);
        if (NgayKetThuc == default || NgayKetThuc < NgayBatDau)
            yield return new("Ngày kết thúc phải từ ngày bắt đầu trở đi.", [nameof(NgayKetThuc)]);
        if (HanDangKy == default || DateOnly.FromDateTime(HanDangKy) > NgayBatDau)
            yield return new("Hạn đăng ký phải hợp lệ và không sau ngày bắt đầu.", [nameof(HanDangKy)]);
    }
}

