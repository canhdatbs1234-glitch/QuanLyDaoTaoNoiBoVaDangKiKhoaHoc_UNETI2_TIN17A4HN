// Họ và tên: [Cả nhóm / nhóm trưởng - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: Kết quả kiểm tra nghiệp vụ (thành công / thông báo lỗi) trả về từ các Service.

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Helpers;

public class KetQuaKiemTra
{
    public bool ThanhCong { get; private init; }
    public string? ThongBao { get; private init; }

    public static KetQuaKiemTra Ok() => new() { ThanhCong = true };
    public static KetQuaKiemTra Loi(string thongBao) => new() { ThanhCong = false, ThongBao = thongBao };
}
