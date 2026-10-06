// Họ và tên: Phạm Việt Anh
// Mã sinh viên: 23103100231
// Nội dung thực hiện: Tạo Entity Model

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A4HN.Models
{
    public class BuoiHoc
    {
        [Key]
        public int MaBuoiHoc { get; set; }

        public int MaLop { get; set; }

        public DateTime NgayHoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        [StringLength(500)]
        public string NoiDung { get; set; }

        [StringLength(200)]
        public string DiaDiem { get; set; }

        public bool TrangThai { get; set; }

        [ForeignKey("MaLop")]
        public virtual LopDaoTao LopDaoTao { get; set; }

        public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();
    }
}
