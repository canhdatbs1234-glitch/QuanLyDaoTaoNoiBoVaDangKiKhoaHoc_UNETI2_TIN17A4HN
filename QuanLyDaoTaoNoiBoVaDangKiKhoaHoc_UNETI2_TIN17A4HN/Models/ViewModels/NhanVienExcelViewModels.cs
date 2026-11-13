//Họ và tên : Phí Bảo Nam - 23103100236
//Module 3: Quản lý nhân viên – Đăng ký – Duyệt đào tạo.
//Bổ sung Import / Export Excel thông tin nhân sự.
namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

/// <summary>
/// Dữ liệu của một dòng trong file Excel khi thực hiện xem trước (Preview) Import.
/// </summary>
public class NhanVienImportRowDto
{
    public int RowIndex { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public int MaPhongBan { get; set; }
    public string TenPhongBan { get; set; } = string.Empty;
    public DateOnly NgaySinh { get; set; }
    public string? SoDienThoai { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateOnly NgayVaoLam { get; set; }
    public bool TrangThai { get; set; } = true;

    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; set; } = [];

    public string TenTrangThai => TrangThai ? "Đang làm việc" : "Ngừng hoạt động";
    public string KetQuaText => IsValid ? "Hợp lệ" : "Lỗi";
    public string KetQuaCss => IsValid ? "badge-success" : "badge-error";
}

/// <summary>
/// Kết quả đọc và kiểm tra cấu trúc / dữ liệu file Excel.
/// </summary>
public class NhanVienImportPreviewResult
{
    public string FileName { get; set; } = string.Empty;
    public bool IsStructureValid { get; set; } = true;
    public string? StructureErrorMessage { get; set; }

    public List<NhanVienImportRowDto> Rows { get; set; } = [];

    public int TotalRows => Rows.Count;
    public int ValidCount => Rows.Count(r => r.IsValid);
    public int ErrorCount => Rows.Count(r => !r.IsValid);

    public bool HasValidRows => ValidCount > 0;
    public string? SessionToken { get; set; }
}
