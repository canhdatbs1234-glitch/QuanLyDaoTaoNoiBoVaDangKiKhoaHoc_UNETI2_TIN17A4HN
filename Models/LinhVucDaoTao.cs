// Họ và tên: Nguyễn Cảnh Đạt
// Mã sinh viên: 23103100224
// Nội dung thực hiện: Module 1 - Model LinhVucDaoTao quản lý lĩnh vực đào tạo với các thuộc tính MaLinhVuc, TenLinhVuc, MoTa, TrangThai và quan hệ 1-n với KhoaDaoTao.
using System;
using System.Collections.Generic;

namespace QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Models;

public partial class LinhVucDaoTao
{
    public int MaLinhVuc { get; set; }

    public string TenLinhVuc { get; set; } = null!;

    public string? MoTa { get; set; }

    public bool TrangThai { get; set; }

    public virtual ICollection<KhoaDaoTao> KhoaDaoTaos { get; set; } = new List<KhoaDaoTao>();
}
