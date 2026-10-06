// Họ và tên: Đàm Quang HUy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Tạo Entity Model

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A4HN.Models
{
    public class KetQuaDaoTao
    {
        [Key]
        public int MaKetQua { get; set; }

        public int MaDangKy { get; set; }

        public double? DiemDanhGia { get; set; }

        public double TyLeThamDu { get; set; }

        [StringLength(50)]
        public string KetQua { get; set; }

        public DateTime? NgayDanhGia { get; set; }

        [StringLength(500)]
        public string GhiChu { get; set; }

        [ForeignKey("MaDangKy")]
        public virtual DangKyDaoTao DangKyDaoTao { get; set; }
    }
}
