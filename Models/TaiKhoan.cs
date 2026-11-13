// Họ và tên: Nguyễn Cảnh Đạt
// Mã sinh viên: 23103100224
// Nội dung thực hiện: Module 1- Model TaiKhoan quản lý tài khoản với các thuộc tính MaTaiKhoan, TenDangNhap, MatKhau, HoTen, Email, VaiTro, TrangThai, NgayTao và quan hệ 1-n với DangKyDaoTao, KetQuaDaoTao, LopDaoTao và quan hệ 1-1 với NhanVien.
using System;
using System.Collections.Generic;

namespace QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Models;

public partial class TaiKhoan
{
    public int MaTaiKhoan { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string? Email { get; set; }

    public string VaiTro { get; set; } = null!;

    public bool TrangThai { get; set; }

    public DateTime NgayTao { get; set; }

    public virtual ICollection<DangKyDaoTao> DangKyDaoTaos { get; set; } = new List<DangKyDaoTao>();

    public virtual ICollection<KetQuaDaoTao> KetQuaDaoTaos { get; set; } = new List<KetQuaDaoTao>();

    public virtual ICollection<LopDaoTao> LopDaoTaos { get; set; } = new List<LopDaoTao>();

    public virtual NhanVien? NhanVien { get; set; }
}
