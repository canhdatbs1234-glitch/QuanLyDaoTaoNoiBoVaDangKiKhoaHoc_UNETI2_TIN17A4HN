// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Kết quả đào tạo, Lịch sử đào tạo và Dashboard Thống kê

namespace QuanLyDaoTaoNoiBo.Models.ViewModels;

public class DashboardViewModel
{
    public int TongNhanVienHoatDong { get; set; }
    public int SoKhoaDangMo { get; set; }
    public int SoLopDangMo { get; set; }
    public int DangKyChoDuyet { get; set; }
    public int DangKyDaDuyet { get; set; }
    public int SoLopDangHoc { get; set; }
    public int SoLopDaKetThuc { get; set; }
    public int LuotHoanThanh { get; set; }
    public int LuotKhongHoanThanh { get; set; }
    public decimal TyLeHoanThanh => LuotHoanThanh + LuotKhongHoanThanh == 0 ? 0 :
        Math.Round(LuotHoanThanh * 100m / (LuotHoanThanh + LuotKhongHoanThanh), 2);
    public ThongKeViewModel ThongKe { get; set; } = new();
}

