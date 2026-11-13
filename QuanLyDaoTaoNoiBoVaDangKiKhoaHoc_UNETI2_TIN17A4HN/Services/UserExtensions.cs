namespace QuanLyDaoTaoNoiBo.Services;
public static class UserExtensions
{
    public const string KeyMaTaiKhoan = "MaTaiKhoan";
    public const string KeyHoTen = "HoTen";
    public const string KeyVaiTro = "VaiTro";
    public const string KeyMaNhanVien = "MaNhanVien";

    public static int? GetMaTaiKhoan(this ISession session) => session.GetInt32(KeyMaTaiKhoan);

    public static string? GetHoTen(this ISession session) => session.GetString(KeyHoTen);

    public static string? GetVaiTro(this ISession session) => session.GetString(KeyVaiTro);

    public static int? GetMaNhanVien(this ISession session) => session.GetInt32(KeyMaNhanVien);

    public static bool DaDangNhap(this ISession session) => session.GetInt32(KeyMaTaiKhoan).HasValue;

    /// <summary>Ghi thông tin đăng nhập vào Session.</summary>
    public static void DangNhap(this ISession session, int maTaiKhoan, string hoTen, string vaiTro, int? maNhanVien)
    {
        session.Clear();
        session.SetInt32(KeyMaTaiKhoan, maTaiKhoan);
        session.SetString(KeyHoTen, hoTen);
        session.SetString(KeyVaiTro, vaiTro);          // QuanTri (Admin / Nhân sự - Đào tạo) / NhanVien / GiangVien
        if (maNhanVien.HasValue) session.SetInt32(KeyMaNhanVien, maNhanVien.Value);
    }
}
