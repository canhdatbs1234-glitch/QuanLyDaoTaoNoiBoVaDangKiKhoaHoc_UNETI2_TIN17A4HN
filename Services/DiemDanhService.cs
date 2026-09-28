// Họ và tên: [SV4 - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: Nghiệp vụ điểm danh: điều kiện điểm danh và tính tỷ lệ tham dự (LINQ).

using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Data;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Helpers;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Services;

public interface IDiemDanhService
{
    /// <summary>Buổi tồn tại & không hủy; nhân viên có đăng ký "Đã duyệt" của lớp; chưa điểm danh trùng.</summary>
    Task<KetQuaKiemTra> KiemTraDieuKienDiemDanhAsync(int maBuoiHoc, int maNhanVien);

    /// <summary>TyLeThamDu = SoBuoiCoMat / TongSoBuoiHopLe x 100 (quy tắc tính xem QuyDinhNhom).</summary>
    Task<(int TongSoBuoi, int SoBuoiCoMat, decimal TyLeThamDu)> TinhTyLeThamDuAsync(int maNhanVien, int maLop);
}

public class DiemDanhService : IDiemDanhService
{
    private readonly ApplicationDbContext _db;

    public DiemDanhService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<KetQuaKiemTra> KiemTraDieuKienDiemDanhAsync(int maBuoiHoc, int maNhanVien)
    {
        var buoi = await _db.BuoiHocs.AsNoTracking().FirstOrDefaultAsync(b => b.MaBuoiHoc == maBuoiHoc);
        if (buoi == null)
            return KetQuaKiemTra.Loi("Buổi học không tồn tại.");

        if (buoi.TrangThai == TrangThaiBuoiHoc.DaHuy)
            return KetQuaKiemTra.Loi("Buổi học đã hủy, không thể điểm danh.");

        var thuocLop = await _db.DangKyDaoTaos.AnyAsync(d =>
            d.MaLop == buoi.MaLop && d.MaNhanVien == maNhanVien && d.TrangThai == TrangThaiDangKy.DaDuyet);
        if (!thuocLop)
            return KetQuaKiemTra.Loi("Nhân viên không thuộc danh sách lớp (chưa có đăng ký Đã duyệt).");

        var daCo = await _db.DiemDanhs.AnyAsync(d => d.MaBuoiHoc == maBuoiHoc && d.MaNhanVien == maNhanVien);
        if (daCo)
            return KetQuaKiemTra.Loi("Đã có bản ghi điểm danh của nhân viên này cho buổi học này.");

        return KetQuaKiemTra.Ok();
    }

    public async Task<(int TongSoBuoi, int SoBuoiCoMat, decimal TyLeThamDu)> TinhTyLeThamDuAsync(int maNhanVien, int maLop)
    {
        // Tổng số buổi hợp lệ = các buổi của lớp chưa bị hủy
        var tongSoBuoi = await _db.BuoiHocs
            .CountAsync(b => b.MaLop == maLop && b.TrangThai != TrangThaiBuoiHoc.DaHuy);

        var trangThaiTinhCoMat = new List<string> { TrangThaiDiemDanh.CoMat };
        if (QuyDinhNhom.DiMuonTinhLaCoMat)
            trangThaiTinhCoMat.Add(TrangThaiDiemDanh.DiMuon);

        var diemDanhCuaNv = _db.DiemDanhs.Where(d =>
            d.MaNhanVien == maNhanVien
            && d.BuoiHoc!.MaLop == maLop
            && d.BuoiHoc.TrangThai != TrangThaiBuoiHoc.DaHuy);

        var soBuoiCoMat = await diemDanhCuaNv.CountAsync(d => trangThaiTinhCoMat.Contains(d.TrangThai));

        if (QuyDinhNhom.VangCoPhepKhongTinhVaoTong)
        {
            var soVangCoPhep = await diemDanhCuaNv.CountAsync(d => d.TrangThai == TrangThaiDiemDanh.VangCoPhep);
            tongSoBuoi -= soVangCoPhep;
        }

        return (tongSoBuoi, soBuoiCoMat, QuyTacNghiepVu.TinhTyLeThamDu(soBuoiCoMat, tongSoBuoi));
    }
}
