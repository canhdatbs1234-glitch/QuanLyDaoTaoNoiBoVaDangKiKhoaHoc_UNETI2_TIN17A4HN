//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using Microsoft.AspNetCore.Identity;
using QuanLyDaoTaoNoiBo.Models.Entities;

namespace QuanLyDaoTaoNoiBo.Services;

/// <summary>Băm và kiểm tra mật khẩu bằng PasswordHasher (PBKDF2) của ASP.NET Core.</summary>
public static class MatKhauHelper
{
    private static readonly PasswordHasher<TaiKhoan> Hasher = new();

    public static string Bam(TaiKhoan taiKhoan, string matKhau) => Hasher.HashPassword(taiKhoan, matKhau);

    // Hash định dạng V3 của Identity luôn bắt đầu bằng "AQAAAA" (Base64 của byte 0x01 + prf).
    private static bool DaBam(string giaTri) =>
        giaTri.Length > 40 && giaTri.StartsWith("AQAAAA", StringComparison.Ordinal);

    /// <summary>
    /// Kiểm tra mật khẩu đăng nhập (hỗ trợ cả mật khẩu đã băm và mật khẩu thô nếu nhập trực tiếp trong SQL).
    /// </summary>
    public static bool KiemTra(TaiKhoan taiKhoan, string matKhau)
    {
        if (DaBam(taiKhoan.MatKhau))
        {
            var ketQua = Hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhau, matKhau);
            return ketQua != PasswordVerificationResult.Failed;
        }

        return taiKhoan.MatKhau == matKhau;
    }
}
