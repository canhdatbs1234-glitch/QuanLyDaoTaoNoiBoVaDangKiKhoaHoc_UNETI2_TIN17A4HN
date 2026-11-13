//Họ và tên: Trần Trung Hiếu
//Mã sinh viên: 23103100217
//Module 2

using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;

namespace QuanLyDaoTaoNoiBo.Services;

/// <summary>
/// Quy tắc nhận đăng ký của lớp đào tạo (Module 2, mục 6.2 và 6.8).
/// Module đăng ký nên gọi LyDoKhongNhanDangKy trước khi tạo/duyệt một đăng ký mới.
/// </summary>
public static class LopDaoTaoRules
{
    /// <summary>Giờ hiện tại theo múi giờ Việt Nam (UTC+7), cùng quy ước với form tạo lớp.</summary>
    public static DateTime BayGioVietNam() => DateTime.UtcNow.AddHours(7);

    /// <summary>
    /// Trả về null nếu lớp được nhận đăng ký; ngược lại trả về lý do từ chối.
    /// soDaDuyet là số đăng ký đang có hiệu lực/được duyệt (tính từ DB, không nhập tay).
    /// </summary>
    public static string? LyDoKhongNhanDangKy(string trangThai, DateTime hanDangKy,
        int soLuongToiDa, int soDaDuyet, DateTime? bayGio = null)
    {
        if (trangThai != "MoDangKy")
            return "Lớp không ở trạng thái “Đang nhận đăng ký”.";
        if (hanDangKy < (bayGio ?? BayGioVietNam()))
            return "Lớp đã hết hạn đăng ký.";
        if (soLuongToiDa - soDaDuyet <= 0)
            return "Lớp đã hết chỗ.";
        return null;
    }

    /// <summary>
    /// Sửa dữ liệu cũ bị sai logic: lớp có số đăng ký đã duyệt LỚN HƠN số lượng tối đa (số chỗ còn âm).
    /// Nâng SoLuongToiDa lên bằng đúng số đã duyệt để SoChoCon = 0 (hết chỗ) thay vì âm.
    /// Dùng ExecuteUpdate nên không đụng đến rule khóa kết quả trong SaveChanges; chạy lại nhiều lần vẫn an toàn.
    /// </summary>
    public static void SuaLopVuotSucChua(ApplicationDbContext db, string trangThaiDaDuyet = "DaDuyet")
    {
        var lopVuot = db.LopDaoTaos.AsNoTracking()
            .Select(l => new { l.MaLop, l.SoLuongToiDa, SoDaDuyet = l.DangKyDaoTaos.Count(d => d.TrangThai == trangThaiDaDuyet) })
            .Where(x => x.SoDaDuyet > x.SoLuongToiDa)
            .ToList();
        foreach (var lop in lopVuot)
        {
            var soMoi = lop.SoDaDuyet;
            db.LopDaoTaos.Where(l => l.MaLop == lop.MaLop)
                .ExecuteUpdate(s => s.SetProperty(l => l.SoLuongToiDa, soMoi));
        }
    }
}
