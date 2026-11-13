// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - ViewModel thống kê LINQ (mục 9.7).
using System.Globalization;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class ThongKePhongBanRow
{
    public string TenPhongBan { get; set; } = "";
    public int SoNhanVienHoatDong { get; set; }
    public int SoNhanVienThamGia { get; set; }
    public int SoLuotDaoTao { get; set; }
}

/// <summary>Một dòng cho mỗi lớp: gom số liệu để dùng cho các báo cáo 2, 3, 4, 5, 6, 7.</summary>
public class ThongKeLopRow
{
    public int MaLop { get; set; }
    public string TenLop { get; set; } = "";
    public int MaKhoaDaoTao { get; set; }
    public string TenKhoa { get; set; } = "";
    public string TrangThai { get; set; } = "";
    public int SoLuongToiDa { get; set; }

    public int ChoDuyet { get; set; }
    public int DaDuyet { get; set; }
    public int TuChoi { get; set; }
    public int DaHuy { get; set; }

    public int HoanThanh { get; set; }
    public int KhongHoanThanh { get; set; }

    public int TongSoBuoiHopLe { get; set; }
    public int SoLuotCoMat { get; set; }
    public int SoLuotVangKhongPhep { get; set; }
    public int SoLuotVangCoPhep { get; set; }

    public int TongDangKy => ChoDuyet + DaDuyet + TuChoi + DaHuy;
    public int DangKyHieuLuc => ChoDuyet + DaDuyet;
    public int SoChoCon => Math.Max(0, SoLuongToiDa - DaDuyet);
    public int SoLuotVang => SoLuotVangKhongPhep + SoLuotVangCoPhep;
    public int TongCoKetQua => HoanThanh + KhongHoanThanh;
    public decimal? TyLeHoanThanh => TongCoKetQua == 0 ? null : Math.Round(HoanThanh * 100m / TongCoKetQua, 2);

    /// <summary>Mẫu số tỷ lệ tham dự: số buổi hợp lệ x số học viên đã duyệt.</summary>
    public int MauSoThamDu => TongSoBuoiHopLe * DaDuyet;
    public decimal? TyLeThamDuTrungBinh => MauSoThamDu == 0 ? null : Math.Round(SoLuotCoMat * 100m / MauSoThamDu, 2);

    public string TenTrangThai => LopDaoTaoViewModel.TrangThais.GetValueOrDefault(TrangThai, TrangThai);
}

public class ThongKeKhoaRow
{
    public int MaKhoaDaoTao { get; set; }
    public string TenKhoa { get; set; } = "";
    public int SoLop { get; set; }
    public int ChoDuyet { get; set; }
    public int DaDuyet { get; set; }
    public int TuChoi { get; set; }
    public int DaHuy { get; set; }
    public int HoanThanh { get; set; }
    public int KhongHoanThanh { get; set; }

    public int TongDangKy => ChoDuyet + DaDuyet + TuChoi + DaHuy;
    public int TongCoKetQua => HoanThanh + KhongHoanThanh;
    public decimal? TyLeHoanThanh => TongCoKetQua == 0 ? null : Math.Round(HoanThanh * 100m / TongCoKetQua, 2);
}

public class ThongKeNhanVienRow
{
    public string HoTen { get; set; } = "";
    public string TenPhongBan { get; set; } = "";
    public int SoKhoa { get; set; }
    public int SoLop { get; set; }
}

public class ThongKeThoiGianRow
{
    public string Nhan { get; set; } = "";
    public int SoLuot { get; set; }
    public int SoHoanThanh { get; set; }
}

public class ThongKeViewModel
{
    public int Nam { get; set; }
    public IReadOnlyList<int> Nams { get; set; } = [];

    public IReadOnlyList<ThongKePhongBanRow> PhongBans { get; set; } = [];
    public IReadOnlyList<ThongKeKhoaRow> Khoas { get; set; } = [];
    public IReadOnlyList<ThongKeLopRow> Lops { get; set; } = [];
    public IReadOnlyList<ThongKeLopRow> LopDongNhat { get; set; } = [];
    public IReadOnlyList<ThongKeNhanVienRow> NhanVienNhieuKhoa { get; set; } = [];
    public IReadOnlyList<ThongKeThoiGianRow> Thangs { get; set; } = [];
    public IReadOnlyList<ThongKeThoiGianRow> Quys { get; set; } = [];

    public decimal? TyLeThamDuTrungBinhChung { get; set; }
    public int TongSoLuotVang => Lops.Sum(l => l.SoLuotVang);
    public int MaxLuotThang => Thangs.Count == 0 ? 0 : Thangs.Max(t => t.SoLuot);

    public static string Pct(decimal? value) => value.HasValue ? value.Value.ToString("0.##") + "%" : "—";

    // Dùng InvariantCulture để CSS "width:66.67%" không bị đổi dấu phẩy theo culture máy.
    public static string Css(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}