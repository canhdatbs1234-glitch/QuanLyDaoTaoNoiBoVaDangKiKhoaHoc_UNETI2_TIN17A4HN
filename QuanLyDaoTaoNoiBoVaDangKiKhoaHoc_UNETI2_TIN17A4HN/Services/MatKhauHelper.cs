//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using Microsoft.AspNetCore.Identity;
using QuanLyDaoTaoNoiBo.Models.Entities;

namespace QuanLyDaoTaoNoiBo.Services;
public static class MatKhauHelper
{
    private static readonly PasswordHasher<TaiKhoan> Hasher = new();

    public static string Bam(TaiKhoan taiKhoan, string matKhau) => Hasher.HashPassword(taiKhoan, matKhau);
    private static bool DaBam(string giaTri) =>
        giaTri.Length > 40 && giaTri.StartsWith("AQAAAA", StringComparison.Ordinal);
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
