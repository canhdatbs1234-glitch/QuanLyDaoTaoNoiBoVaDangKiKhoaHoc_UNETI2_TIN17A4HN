//Họ và tên: Trần Trung Hiếu
//Mã sinh viên: 23103100217
//Module 2

using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyDaoTaoNoiBo.Models.Entities;
namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class KhoaDaoTaoListViewModel
{
    public IReadOnlyList<KhoaDaoTao> Khoas { get; set; } = [];
    public string? SearchTerm { get; set; }
    public int? MaLinhVuc { get; set; }
    public string? TrangThai { get; set; }          // hoatdong | ngung
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
        ["name_asc"] = "Tên khóa A–Z",
        ["name_desc"] = "Tên khóa Z–A",
        ["hours_asc"] = "Số giờ tăng dần",
        ["hours_desc"] = "Số giờ giảm dần"
    };

    public Dictionary<string, string> RouteForPage(int page) => new()
    {
        ["SearchTerm"] = SearchTerm ?? "",
        ["MaLinhVuc"] = MaLinhVuc?.ToString() ?? "",
        ["TrangThai"] = TrangThai ?? "",
        ["SortOrder"] = SortOrder,
        ["PageIndex"] = page.ToString()
    };

    // Điều kiện hiện tại (không có PageIndex) để nút Xuất Excel/PDF xuất đúng kết quả đang lọc.
    public Dictionary<string, string> RouteForExport() => new()
    {
        ["SearchTerm"] = SearchTerm ?? "",
        ["MaLinhVuc"] = MaLinhVuc?.ToString() ?? "",
        ["TrangThai"] = TrangThai ?? "",
        ["SortOrder"] = SortOrder
    };
}
