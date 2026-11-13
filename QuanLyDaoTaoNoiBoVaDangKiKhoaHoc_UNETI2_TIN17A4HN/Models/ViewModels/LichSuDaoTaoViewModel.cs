// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - ViewModel lịch sử đào tạo của nhân viên (mục 9.5).
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Services;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class LichSuDongViewModel
{
    public int MaDangKy { get; set; }
    public int MaLop { get; set; }
    public string TenLop { get; set; } = "";
    public string TenKhoa { get; set; } = "";
    public string DiaDiem { get; set; } = "";
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }

    public string TrangThaiDangKy { get; set; } = "";
    public DateTime NgayDangKy { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public string? GhiChuDangKy { get; set; }      // lý do từ chối/hủy

    public bool CoKetQua { get; set; }             // đã được đánh giá
    public string KetQua { get; set; } = KetQuaConst.ChuaDanhGia;
    public decimal? DiemDanhGia { get; set; }
    public decimal? TyLeThamDu { get; set; }
    public bool DaXacNhan { get; set; }
    public DateTime? NgayXacNhan { get; set; }
    public bool XemSoBo { get; set; }              // true khi người xem là Quản trị/Nhân sự

    // Tỷ lệ tạm tính từ điểm danh hiện tại (khi chưa có kết quả chính thức)
    public int TongSoBuoiHopLe { get; set; }
    public int SoBuoiCoMat { get; set; }

    public bool DaDuyet => TrangThaiDangKy == TrangThaiDangKyConst.DaDuyet;
    public string TenTrangThaiDangKy =>
        TrangThaiDangKyConst.TenTrangThai.GetValueOrDefault(TrangThaiDangKy, TrangThaiDangKy);
    public string TrangThaiDangKyCss => TrangThaiDangKy switch
    {
        "DaDuyet" => "status-on",
        "ChoDuyet" => "status-warn",
        "TuChoi" => "status-off",
        _ => "status-muted"
    };

    /// <summary>true: hiển thị điểm/tỷ lệ/kết quả đã lưu; false: hiển thị tạm tính.</summary>
    public bool HienKetQuaDaLuu => DaDuyet && (DaXacNhan || (XemSoBo && CoKetQua));
    public bool HienTyLeTamTinh => DaDuyet && !HienKetQuaDaLuu && TongSoBuoiHopLe > 0;
    public decimal TyLeTamTinh => KetQuaDaoTaoService.TinhTyLeThamDu(SoBuoiCoMat, TongSoBuoiHopLe);

    public string TenKetQuaHienThi
    {
        get
        {
            if (!DaDuyet) return "—";
            if (DaXacNhan) return KetQuaConst.Ten(KetQua);
            if (XemSoBo && CoKetQua) return $"Chờ xác nhận ({KetQuaConst.Ten(KetQua)})";
            return "Chờ kết quả";
        }
    }

    public string KetQuaCss => !HienKetQuaDaLuu ? "status-muted" : KetQua switch
    {
        KetQuaConst.HoanThanh => "status-on",
        KetQuaConst.KhongHoanThanh => "status-off",
        _ => "status-muted"
    };
}

public class LichSuDaoTaoViewModel
{
    public bool ChuaCoHoSo { get; set; }
    public bool QuanLyXem { get; set; }

    // Thông tin nhân viên
    public int MaNhanVien { get; set; }
    public string HoTen { get; set; } = "";
    public string TenPhongBan { get; set; } = "";
    public string Email { get; set; } = "";

    // Tổng hợp (chỉ tính kết quả đã xác nhận)
    public int TongDangKy { get; set; }
    public int SoDaDuyet { get; set; }
    public int SoHoanThanh { get; set; }
    public int SoKhongHoanThanh { get; set; }
    public decimal? TyLeThamDuTrungBinh { get; set; }

    // Tìm kiếm / lọc / phân trang
    public string? SearchTerm { get; set; }
    public string? TrangThaiDangKy { get; set; }
    public string? KetQua { get; set; }            // chua | Dat | KhongDat
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public IReadOnlyList<LichSuDongViewModel> Items { get; set; } = [];

    public static IReadOnlyDictionary<string, string> TrangThaiDangKyOptions => TrangThaiDangKyConst.TenTrangThai;

    public Dictionary<string, string> RouteForPage(int page) => new()
    {
        ["MaNhanVien"] = MaNhanVien.ToString(),
        ["SearchTerm"] = SearchTerm ?? "",
        ["TrangThaiDangKy"] = TrangThaiDangKy ?? "",
        ["KetQua"] = KetQua ?? "",
        ["PageIndex"] = page.ToString()
    };
}
