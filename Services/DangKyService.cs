// Họ và tên: [SV3 - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: Nghiệp vụ đăng ký: kiểm tra điều kiện, tính số chỗ còn, kiểm tra trùng lịch (LINQ).

using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Data;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Helpers;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Models;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Services;

public interface IDangKyService
{
    /// <summary>Kiểm tra tất cả điều kiện trước khi TẠO đăng ký (mục 7.3).</summary>
    Task<KetQuaKiemTra> KiemTraDieuKienDangKyAsync(int maNhanVien, int maLop);

    /// <summary>SoChoCon = SoLuongToiDa - số đăng ký "Đã duyệt".</summary>
    Task<int> TinhSoChoConAsync(int maLop);

    /// <summary>Nhân viên đã có lớp "Đã duyệt" khác giao nhau về thời gian với lớp này chưa?</summary>
    Task<bool> TrungLichAsync(int maNhanVien, LopDaoTao lop);
}

public class DangKyService : IDangKyService
{
    private readonly ApplicationDbContext _db;

    public DangKyService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> TinhSoChoConAsync(int maLop)
    {
        var lop = await _db.LopDaoTaos.AsNoTracking().FirstOrDefaultAsync(l => l.MaLop == maLop);
        if (lop == null) return 0;

        var soDaDuyet = await _db.DangKyDaoTaos
            .CountAsync(d => d.MaLop == maLop && d.TrangThai == TrangThaiDangKy.DaDuyet);

        return lop.SoLuongToiDa - soDaDuyet;
    }

    public async Task<bool> TrungLichAsync(int maNhanVien, LopDaoTao lop)
    {
        // Lớp A và Lớp B trùng nếu: A.BatDau <= B.KetThuc && B.BatDau <= A.KetThuc
        // TODO (nâng cao): kiểm tra ở mức BuoiHoc nếu nhóm quản lý lịch theo buổi.
        return await _db.DangKyDaoTaos.AnyAsync(d =>
            d.MaNhanVien == maNhanVien
            && d.TrangThai == TrangThaiDangKy.DaDuyet
            && d.MaLop != lop.MaLop
            && d.LopDaoTao!.NgayBatDau <= lop.NgayKetThuc
            && lop.NgayBatDau <= d.LopDaoTao.NgayKetThuc);
    }

    public async Task<KetQuaKiemTra> KiemTraDieuKienDangKyAsync(int maNhanVien, int maLop)
    {
        var nhanVien = await _db.NhanViens.AsNoTracking().FirstOrDefaultAsync(n => n.MaNhanVien == maNhanVien);
        if (nhanVien == null || nhanVien.TrangThai != TrangThaiNhanVien.DangLamViec)
            return KetQuaKiemTra.Loi("Nhân viên không tồn tại hoặc đã ngừng hoạt động.");

        var lop = await _db.LopDaoTaos.AsNoTracking().FirstOrDefaultAsync(l => l.MaLop == maLop);
        if (lop == null)
            return KetQuaKiemTra.Loi("Lớp đào tạo không tồn tại.");

        if (lop.TrangThai != TrangThaiLop.DangNhanDangKy)
            return KetQuaKiemTra.Loi("Lớp hiện không nhận đăng ký.");

        if (DateTime.Today > lop.HanDangKy.Date)
            return KetQuaKiemTra.Loi("Đã quá hạn đăng ký.");

        if (await TinhSoChoConAsync(maLop) <= 0)
            return KetQuaKiemTra.Loi("Lớp đã hết chỗ.");

        // Đăng ký "đang hiệu lực" = Chờ duyệt hoặc Đã duyệt
        var daDangKy = await _db.DangKyDaoTaos.AnyAsync(d =>
            d.MaNhanVien == maNhanVien && d.MaLop == maLop
            && (d.TrangThai == TrangThaiDangKy.ChoDuyet || d.TrangThai == TrangThaiDangKy.DaDuyet));
        if (daDangKy)
            return KetQuaKiemTra.Loi("Bạn đã đăng ký lớp này rồi.");

        if (await TrungLichAsync(maNhanVien, lop))
            return KetQuaKiemTra.Loi("Lớp này trùng lịch với một lớp bạn đã được duyệt.");

        // TODO: các điều kiện bổ sung của khóa/lớp nếu nhóm khai báo
        return KetQuaKiemTra.Ok();
    }
}
