// Họ và tên: Trần Trung Hiếu
// Mã sinh viên: 23103100217
// Nội dung thực hiện: Module 2 - Quản lý Khóa đào tạo và Lớp đào tạo

using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class LopDaoTaoListViewModel
{
    public IReadOnlyList<LopDaoTaoViewModel> Lops { get; set; } = [];
    public string? SearchTerm { get; set; }
    public int? MaLinhVuc { get; set; }
    public string? TrangThai { get; set; }
    public string? TinhTrangCho { get; set; }
    public string SortOrder { get; set; } = "name_asc";
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    public IEnumerable<SelectListItem> LinhVucs { get; set; } = [];

    public Dictionary<string, string> RouteForPage(int page) => new()
    {
        ["SearchTerm"] = SearchTerm ?? "",
        ["MaLinhVuc"] = MaLinhVuc?.ToString() ?? "",
        ["TrangThai"] = TrangThai ?? "",
        ["TinhTrangCho"] = TinhTrangCho ?? "",
        ["SortOrder"] = SortOrder,
        ["PageIndex"] = page.ToString()
    };
}

