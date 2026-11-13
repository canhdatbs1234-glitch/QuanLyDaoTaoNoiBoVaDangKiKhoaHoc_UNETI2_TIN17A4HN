// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - ViewModel Dashboard (mục 9.6).
using System.Globalization;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class LopSapBatDauViewModel
{
    public int MaLop { get; set; }
    public string TenLop { get; set; } = "";
    public string TenKhoa { get; set; } = "";
    public DateOnly NgayBatDau { get; set; }
    public string DiaDiem { get; set; } = "";
    public int SoLuongToiDa { get; set; }
    public int SoChoCon { get; set; }
    public int SoNgayConLai { get; set; }
}

public class DashboardViewModel
{
    public DateOnly HomNay { get; set; }
    public int SoNgaySapBatDau { get; set; }

    // Tổng quan
    public int SoNhanVienDangHoatDong { get; set; }
    public int SoKhoaDangMo { get; set; }
    public int SoLopDangMoDangKy { get; set; }
    public int SoLopSapBatDau { get; set; }

    // Đăng ký
    public int SoDangKyChoDuyet { get; set; }
    public int SoDangKyDaDuyet { get; set; }

    // Lớp
    public int SoLopDangHoc { get; set; }
    public int SoLopDaKetThuc { get; set; }

    // Kết quả (chỉ tính kết quả đã xác nhận)
    public int SoLuotHoanThanh { get; set; }
    public int SoLuotKhongHoanThanh { get; set; }
    public int SoKetQuaChoXacNhan { get; set; }

    public IReadOnlyList<LopSapBatDauViewModel> LopSapBatDau { get; set; } = [];

    public int TongLuotCoKetQua => SoLuotHoanThanh + SoLuotKhongHoanThanh;

    /// <summary>null khi chưa có kết quả nào được xác nhận (tránh chia cho 0).</summary>
    public decimal? TyLeHoanThanhChung => TongLuotCoKetQua == 0
        ? null
        : Math.Round(SoLuotHoanThanh * 100m / TongLuotCoKetQua, 2);

    // Dùng InvariantCulture để CSS "width:66.67%" không bị đổi thành "66,67%" theo culture máy.
    public string TyLeCss => (TyLeHoanThanhChung ?? 0m).ToString("0.##", CultureInfo.InvariantCulture);
}