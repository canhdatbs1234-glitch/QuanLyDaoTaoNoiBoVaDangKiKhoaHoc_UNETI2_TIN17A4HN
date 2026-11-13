// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - ViewModel danh sách kết quả và form đánh giá/xác nhận kết quả đào tạo.
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyDaoTaoNoiBo.Models.Entities;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

/// <summary>Một dòng trong danh sách kết quả (mỗi dòng = một đăng ký đã duyệt).</summary>
public class KetQuaDongViewModel
{
    public int MaDangKy { get; set; }
    public int? MaKetQua { get; set; }
    public int MaLop { get; set; }
    public string TenLop { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string TenPhongBan { get; set; } = "";
    public string TrangThaiLop { get; set; } = "";
    public DateOnly NgayKetThucLop { get; set; }
    public decimal? DiemDanhGia { get; set; }
    public decimal? TyLeThamDu { get; set; }
    public string KetQua { get; set; } = KetQuaConst.ChuaDanhGia;
    public bool DaXacNhan { get; set; }
    public DateTime? NgayDanhGia { get; set; }
    public bool DuDieuKienDanhGia { get; set; }

    public string TenKetQua => KetQuaConst.Ten(KetQua);
    public string KetQuaCss => KetQua switch
    {
        KetQuaConst.HoanThanh => "status-on",
        KetQuaConst.KhongHoanThanh => "status-off",
        _ => "status-muted"
    };
}

public class KetQuaDaoTaoListViewModel
{
    public IReadOnlyList<KetQuaDongViewModel> Items { get; set; } = [];
    public string? SearchTerm { get; set; }
    public int? MaLop { get; set; }
    public int? MaPhongBan { get; set; }
    public string? TrangThaiKetQua { get; set; }   // chua | Dat | KhongDat
    public string? XacNhan { get; set; }           // da | chua
    public string SortOrder { get; set; } = "lop";
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    public IEnumerable<SelectListItem> Lops { get; set; } = [];
    public IEnumerable<SelectListItem> PhongBans { get; set; } = [];
    public bool CoTheXacNhan { get; set; }

    public static IReadOnlyDictionary<string, string> SortOptions { get; } = new Dictionary<string, string>
    {
        ["lop"] = "Theo lớp, rồi tên học viên",
        ["ten_asc"] = "Tên học viên A–Z",
        ["ten_desc"] = "Tên học viên Z–A",
        ["diem_desc"] = "Điểm cao → thấp",
        ["diem_asc"] = "Điểm thấp → cao",
        ["tyle_desc"] = "Tỷ lệ tham dự cao → thấp",
        ["tyle_asc"] = "Tỷ lệ tham dự thấp → cao"
    };

    // Giữ nguyên toàn bộ điều kiện tìm kiếm/lọc/sắp xếp khi chuyển trang.
    public Dictionary<string, string> RouteForPage(int page) => new()
    {
        ["SearchTerm"] = SearchTerm ?? "",
        ["MaLop"] = MaLop?.ToString() ?? "",
        ["MaPhongBan"] = MaPhongBan?.ToString() ?? "",
        ["TrangThaiKetQua"] = TrangThaiKetQua ?? "",
        ["XacNhan"] = XacNhan ?? "",
        ["SortOrder"] = SortOrder,
        ["PageIndex"] = page.ToString()
    };
}

/// <summary>Dữ liệu hiển thị cho trang đánh giá. Controller tự dựng, KHÔNG model-bind từ form.</summary>
public class DanhGiaKetQuaViewModel
{
    public int MaDangKy { get; set; }
    public int? MaKetQua { get; set; }
    public string HoTen { get; set; } = "";
    public string TenPhongBan { get; set; } = "";
    public string TenLop { get; set; } = "";
    public string TenKhoa { get; set; } = "";
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }

    // Căn cứ do hệ thống tính từ điểm danh và ngưỡng của khóa
    public int TongSoBuoiHopLe { get; set; }
    public int SoBuoiCoMat { get; set; }
    public decimal TyLeThamDu { get; set; }
    public decimal TyLeThamDuToiThieu { get; set; }
    public decimal DiemDatToiThieu { get; set; }

    // Giá trị người dùng nhập (chuỗi, đọc theo InvariantCulture)
    public string? DiemNhap { get; set; }
    public string? GhiChu { get; set; }
    public string? KetQuaDuKien { get; set; }

    // Kết quả đã lưu
    public string KetQuaHienTai { get; set; } = KetQuaConst.ChuaDanhGia;
    public decimal? DiemDaLuu { get; set; }
    public decimal? TyLeDaLuu { get; set; }
    public DateTime? NgayDanhGia { get; set; }
    public string? GhiChuDaLuu { get; set; }
    public bool DaXacNhan { get; set; }
    public DateTime? NgayXacNhan { get; set; }
    public string? TenNguoiXacNhan { get; set; }

    // Trạng thái thao tác
    public string? LyDoKhongDanhGia { get; set; }
    public bool ChoPhepLuu { get; set; }
    public bool ChoPhepXacNhan { get; set; }
    public bool LaDieuChinh { get; set; }
    public string? ThongBaoLoi { get; set; }

    public string TenKetQuaHienTai => KetQuaConst.Ten(KetQuaHienTai);
    public string KetQuaCss => KetQuaHienTai switch
    {
        KetQuaConst.HoanThanh => "status-on",
        KetQuaConst.KhongHoanThanh => "status-off",
        _ => "status-muted"
    };
}