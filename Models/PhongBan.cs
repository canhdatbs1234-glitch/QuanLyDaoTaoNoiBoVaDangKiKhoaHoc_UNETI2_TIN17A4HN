// Họ và tên: Nguyễn Cảnh Đạt
// Mã sinh viên: 23103100224
// Nội dung thực hiện: Module 1- Model PhongBan quản lý phòng ban với các thuộc tính MaPhongBan, TenPhongBan, MoTa, TrangThai và quan hệ 1-n với NhanVien.
using System;
using System.Collections.Generic;

namespace QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Models;

public partial class PhongBan
{
    public int MaPhongBan { get; set; }

    public string TenPhongBan { get; set; } = null!;

    public string? MoTa { get; set; }

    public bool TrangThai { get; set; }

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}

