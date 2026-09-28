// Họ và tên: [SV5 - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: ViewModel Dashboard (số liệu tổng quan).

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.ViewModels;

public class DashboardViewModel
{
    public int TongNhanVienHoatDong { get; set; }
    public int SoLopDangMo { get; set; }
    public int SoDangKyChoDuyet { get; set; }
    public int SoDangKyDaDuyet { get; set; }
    public int SoLopDangHoc { get; set; }
    public int SoLopDaKetThuc { get; set; }
    public int SoLuotHoanThanh { get; set; }
    public int SoLuotKhongHoanThanh { get; set; }
    public int SoLopSapBatDau { get; set; }
    public decimal TyLeHoanThanhChung { get; set; }

    // TODO (SV5): thêm các danh sách thống kê LINQ/GroupBy (theo phòng ban, theo lớp, theo tháng/quý...)
}
