// Họ và tên: Phạm Việt Anh
// Mã sinh viên: 23103100231
// Nội dung thực hiện: Module 4 - Buổi học, Lịch học và Điểm danh

using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class BaoCaoThamGiaViewModel
{
    public int? MaLop { get; set; }
    public string TenLop { get; set; } = "";
    public IEnumerable<SelectListItem> Lops { get; set; } = [];
    public int TongSoBuoi { get; set; }
    public int TongSoBuoiHopLe { get; set; }
    public IReadOnlyList<ThamGiaNhanVienViewModel> NhanViens { get; set; } = [];
}

public class ThamGiaNhanVienViewModel
{
    public int MaNhanVien { get; set; }
    public string HoTen { get; set; } = "";
    public int TongSoBuoiHopLe { get; set; }
    public int SoBuoiCoMat { get; set; }
    public int SoBuoiVangCoPhep { get; set; }
    public int SoBuoiVangKhongPhep { get; set; }
    public int SoBuoiDiMuon { get; set; }
    public int SoBuoiChuaDiemDanh { get; set; }
    public int SoBuoiVang => SoBuoiVangCoPhep + SoBuoiVangKhongPhep;
    public decimal TyLeThamDu => TongSoBuoiHopLe == 0 ? 0m :
        Math.Round(SoBuoiCoMat * 100m / TongSoBuoiHopLe, 2);
    public IReadOnlyList<ChiTietThamGiaViewModel> ChiTiet { get; set; } = [];
}

public class ChiTietThamGiaViewModel
{
    public DateOnly NgayHoc { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public string NoiDung { get; set; } = "";
    public string? TrangThai { get; set; }
    public DateTime? ThoiGianCapNhat { get; set; }
    public string? GhiChu { get; set; }
    public string TenTrangThai => TrangThai switch
    {
        "CoMat" => "Có mặt",
        "CoPhep" => "Vắng có phép",
        "VangMat" => "Vắng không phép",
        "DiMuon" => "Đi muộn (dữ liệu cũ)",
        _ => "Chưa điểm danh"
    };
}

1