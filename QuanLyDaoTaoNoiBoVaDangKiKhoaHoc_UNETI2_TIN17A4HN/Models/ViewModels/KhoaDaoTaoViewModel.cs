// Họ và tên: Trần Trung Hiếu
// Mã sinh viên: 23103100217
// Nội dung thực hiện: Module 2 - Quản lý Khóa đào tạo và Lớp đào tạo

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class KhoaDaoTaoViewModel
{
    public int MaKhoaDaoTao { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên khóa."), StringLength(200), Display(Name = "Tên khóa đào tạo")]
    public string TenKhoaDaoTao { get; set; } = "";
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn lĩnh vực."), Display(Name = "Lĩnh vực")]
    public int MaLinhVuc { get; set; }
    [StringLength(2000), Display(Name = "Mô tả")]
    public string? MoTa { get; set; }
    [Range(typeof(decimal), "0.01", "9999.99", ErrorMessage = "Số giờ phải lớn hơn 0 và không vượt 9999,99."), Display(Name = "Số giờ dự kiến")]
    public decimal SoGioDuKien { get; set; }
    [Range(typeof(decimal), "0", "10", ErrorMessage = "Điểm phải từ 0 đến 10."), Display(Name = "Điểm đạt tối thiểu")]
    public decimal DiemDatToiThieu { get; set; } = 5;
    [Range(typeof(decimal), "0", "100", ErrorMessage = "Tỷ lệ phải từ 0 đến 100%."), Display(Name = "Tỷ lệ tham dự tối thiểu (%)")]
    public decimal TyLeThamDuToiThieu { get; set; } = 80;
    [Display(Name = "Đang hoạt động")]
    public bool TrangThai { get; set; } = true;
    [ValidateNever]
    public IEnumerable<SelectListItem> LinhVucs { get; set; } = [];
}
