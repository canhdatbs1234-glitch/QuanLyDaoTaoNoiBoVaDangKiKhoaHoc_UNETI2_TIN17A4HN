// Họ và tên: [SV2 - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: ViewModel danh sách lớp: điều kiện tìm kiếm/lọc/sắp xếp/phân trang và dòng hiển thị.

using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Helpers;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Models;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.ViewModels;

/// <summary>Các điều kiện lấy từ query string (GET) để giữ nguyên khi chuyển trang.</summary>
public class LopDaoTaoFilter
{
    public string? TuKhoa { get; set; }          // tên khóa, tên lớp, tên lĩnh vực, địa điểm
    public int? MaLinhVuc { get; set; }
    public string? TrangThai { get; set; }
    public DateTime? TuNgay { get; set; }        // khoảng ngày bắt đầu
    public DateTime? DenNgay { get; set; }
    public string? ConCho { get; set; }          // "con" | "het"
    public string? HanDangKy { get; set; }       // "con" | "het"
    public string? SapXep { get; set; }          // ten_asc, ten_desc, ngay_asc, ngay_desc, max_asc, max_desc, cho_asc, cho_desc
    public int Trang { get; set; } = 1;
}

public class LopDaoTaoItemViewModel
{
    public int MaLop { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public string TenKhoaDaoTao { get; set; } = string.Empty;
    public int MaLinhVuc { get; set; }
    public string TenLinhVuc { get; set; } = string.Empty;
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public DateTime HanDangKy { get; set; }
    public int SoLuongToiDa { get; set; }
    public int SoDaDuyet { get; set; }
    public string? DiaDiem { get; set; }
    public string TrangThai { get; set; } = string.Empty;

    // Thuộc tính tính toán phía C# (KHÔNG dùng trong Where/OrderBy của LINQ to Entities)
    public int SoChoCon => SoLuongToiDa - SoDaDuyet;
    public bool ConHanDangKy => HanDangKy.Date >= DateTime.Today;
}

public class LopDaoTaoListViewModel
{
    public LopDaoTaoFilter Filter { get; set; } = new();
    public PagedResult<LopDaoTaoItemViewModel> KetQua { get; set; } = new();
    public List<LinhVucDaoTao> LinhVucs { get; set; } = new();
}
