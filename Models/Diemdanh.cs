// Họ và tên: Phạm Việt Anh
// Mã sinh viên: 23103100231
// Nội dung thực hiện: Tạo Entity Model

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo_UNETI17_TI17A1HN.Models
{
    public class DiemDanh
    {
        [Key]
        public int MaDiemDanh { get; set; }

        public int MaBuoiHoc { get; set; }

        public int MaNhanVien { get; set; }

        public int TrangThai { get; set; }

        public DateTime ThoiGianCapNhat { get; set; }

        [StringLength(500)]
        public string GhiChu { get; set; }

        [ForeignKey("MaBuoiHoc")]
        public virtual BuoiHoc BuoiHoc { get; set; }

        [ForeignKey("MaNhanVien")]
        public virtual NhanVien NhanVien { get; set; }
    }
}
