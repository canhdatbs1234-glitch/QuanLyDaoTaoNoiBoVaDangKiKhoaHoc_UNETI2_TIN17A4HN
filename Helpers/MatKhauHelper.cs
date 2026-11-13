// Họ và tên: [SV1 - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: Băm mật khẩu (SHA256) để không lưu mật khẩu thô. Có thể nâng cấp lên ASP.NET Core Identity/BCrypt.

using System.Security.Cryptography;
using System.Text;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Helpers;

public static class MatKhauHelper
{
    public static string Bam(string matKhau)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(matKhau));
        return Convert.ToHexString(bytes);
    }
}
