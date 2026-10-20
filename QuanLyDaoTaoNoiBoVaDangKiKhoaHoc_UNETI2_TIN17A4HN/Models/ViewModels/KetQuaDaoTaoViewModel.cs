1// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Kết quả đào tạo, Lịch sử đào tạo và Dashboard Thống kê

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class KetQuaDaoTaoViewModel
{
    public int MaDangKy { get; set; }
    [ValidateNever] public int MaNhanVien { get; set; }
    [ValidateNever] public string HoTen { get; set; } = "";
    [ValidateNever] public string TenLop { get; set; } = "";
    [ValidateNever] public string TenKhoaDaoTao { get; set; } = "";
    [ValidateNever] public DateOnly NgayBatDau { get; set; }
    [ValidateNever] public DateOnly NgayKetThuc { get; set; }
    [Range(typeof(decimal), "0", "10", ErrorMessage = "Điểm phải từ 0 đến 10.")]
    public decimal? DiemDanhGia { get; set; }
    [ValidateNever] public decimal TyLeThamDu { get; set; }
    [ValidateNever] public string KetQua { get; set; } = "ChuaDanhGia";
    [ValidateNever] public DateTime? NgayDanhGia { get; set; }
    [ValidateNever] public int TongSoBuoi { get; set; }
    [ValidateNever] public int SoBuoiCoMat { get; set; }
    [ValidateNever] public int SoBuoiChuaDiemDanh { get; set; }
    public bool DaKhoa => NgayDanhGia.HasValue;
    public string TenKetQua => KetQua switch
    { "Dat" => "Hoàn thành", "KhongDat" => "Không hoàn thành", _ => "Chưa đánh giá" };
}

public class DanhGiaLopViewModel
{
    public int? MaLop { get; set; }
    public string Version { get; set; } = "";
    public List<KetQuaDaoTaoViewModel> HocViens { get; set; } = [];
    [ValidateNever] public IEnumerable<SelectListItem> Lops { get; set; } = [];
    [ValidateNever] public string TenLop { get; set; } = "";
    [ValidateNever] public decimal DiemDatToiThieu { get; set; }
    [ValidateNever] public decimal TyLeThamDuToiThieu { get; set; }
    [ValidateNever] public int TongSoBuoi { get; set; }
    public bool DaKhoa => HocViens.Count > 0 && HocViens.All(x => x.DaKhoa);
}

public class LichSuDaoTaoViewModel
{
    public int? MaNhanVien { get; set; }
    public string HoTen { get; set; } = "";
    public IEnumerable<SelectListItem> NhanViens { get; set; } = [];
    public IReadOnlyList<KetQuaDaoTaoViewModel> KetQuas { get; set; } = [];
}

