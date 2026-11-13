//Họ và tên: Trần Trung Hiếu
//Mã sinh viên: 23103100217
//Module 2

using Microsoft.AspNetCore.Mvc.Rendering;
namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class LopDaoTaoListViewModel
{
    public IReadOnlyList<LopDaoTaoViewModel> Lops { get; set; } = [];
    public string? SearchTerm { get; set; }
    public int? MaLinhVuc { get; set; }
    public string? TrangThai { get; set; }
    public string? TinhTrangCho { get; set; }      // con | het
    public string? TinhTrangHan { get; set; }      // con | het (hạn đăng ký)
    public DateOnly? TuNgay { get; set; }          // ngày bắt đầu từ
    public DateOnly? DenNgay { get; set; }         // ngày bắt đầu đến
    public string SortOrder { get; set; } = "name_asc";
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    public IEnumerable<SelectListItem> LinhVucs { get; set; } = [];

    public static IReadOnlyDictionary<string, string> SortOptions { get; } = new Dictionary<string, string>
    {
        ["name_asc"] = "Tên lớp A–Z",
        ["name_desc"] = "Tên lớp Z–A",
        ["course_asc"] = "Tên khóa A–Z",
        ["course_desc"] = "Tên khóa Z–A",
        ["date_asc"] = "Ngày bắt đầu gần → xa",
        ["date_desc"] = "Ngày bắt đầu xa → gần",
        ["max_asc"] = "Số lượng tối đa tăng dần",
        ["max_desc"] = "Số lượng tối đa giảm dần",
        ["seats_asc"] = "Số chỗ còn tăng dần",
        ["seats_desc"] = "Số chỗ còn giảm dần"
    };

    // Giữ nguyên toàn bộ điều kiện tìm kiếm/lọc/sắp xếp khi chuyển trang.
    public Dictionary<string, string> RouteForPage(int page) => new()
    {
        ["SearchTerm"] = SearchTerm ?? "",
        ["MaLinhVuc"] = MaLinhVuc?.ToString() ?? "",
        ["TrangThai"] = TrangThai ?? "",
        ["TinhTrangCho"] = TinhTrangCho ?? "",
        ["TinhTrangHan"] = TinhTrangHan ?? "",
        ["TuNgay"] = TuNgay?.ToString("yyyy-MM-dd") ?? "",
        ["DenNgay"] = DenNgay?.ToString("yyyy-MM-dd") ?? "",
        ["SortOrder"] = SortOrder,
        ["PageIndex"] = page.ToString()
    };

    // Điều kiện hiện tại (không có PageIndex) để nút Xuất Excel/PDF xuất đúng kết quả đang lọc.
    public Dictionary<string, string> RouteForExport() => new()
    {
        ["SearchTerm"] = SearchTerm ?? "",
        ["MaLinhVuc"] = MaLinhVuc?.ToString() ?? "",
        ["TrangThai"] = TrangThai ?? "",
        ["TinhTrangCho"] = TinhTrangCho ?? "",
        ["TinhTrangHan"] = TinhTrangHan ?? "",
        ["TuNgay"] = TuNgay?.ToString("yyyy-MM-dd") ?? "",
        ["DenNgay"] = DenNgay?.ToString("yyyy-MM-dd") ?? "",
        ["SortOrder"] = SortOrder
    };
}
