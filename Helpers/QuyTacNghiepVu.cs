// Họ và tên: [SV5 - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: Công thức nghiệp vụ thuần (tỷ lệ tham dự, xác định Hoàn thành/Không hoàn thành).

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Helpers;

public static class QuyTacNghiepVu
{
    /// <summary>TyLeThamDu = SoBuoiCoMat / TongSoBuoiHopLe x 100%</summary>
    public static decimal TinhTyLeThamDu(int soBuoiCoMat, int tongSoBuoiHopLe)
        => tongSoBuoiHopLe <= 0 ? 0m : Math.Round(soBuoiCoMat * 100m / tongSoBuoiHopLe, 2);

    /// <summary>Hoàn thành khi TyLeThamDu >= TyLeThamDuToiThieu VÀ DiemDanhGia >= DiemDatToiThieu.</summary>
    public static string XacDinhKetQua(decimal diemDanhGia, decimal tyLeThamDu,
                                       decimal diemDatToiThieu, decimal tyLeThamDuToiThieu)
        => tyLeThamDu >= tyLeThamDuToiThieu && diemDanhGia >= diemDatToiThieu
            ? KetQuaHoanThanh.HoanThanh
            : KetQuaHoanThanh.KhongHoanThanh;
}
