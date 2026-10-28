// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Khóa dữ liệu kết quả đào tạo đã xác nhận (mục 9.4).
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Models.Entities;

namespace QuanLyDaoTaoNoiBo.Data;

public partial class ApplicationDbContext
{
    private const string LoiSuaKetQua =
        "Kết quả đã được xác nhận nên không thể sửa/xóa trực tiếp. Chỉ Quản trị viên được điều chỉnh kèm lý do.";
    private const string LoiDoiNguong =
        "Không thể đổi điểm đạt/tỷ lệ tham dự tối thiểu vì khóa đã có kết quả đào tạo được xác nhận.";
    private const string LoiDoiLop =
        "Không thể đổi ngày, khóa hoặc trạng thái lớp vì lớp đã có kết quả đào tạo được xác nhận.";

    /// <summary>Chỉ KetQuaDaoTaoService đặt true khi Quản trị viên điều chỉnh kết quả đã xác nhận (có ghi chú lý do).</summary>
    public bool ChoPhepSuaKetQuaDaKhoa { get; set; }

    private sealed record ThayDoiCanKiemTra(List<int> KhoaDoiNguong, List<int> LopDoiThongTin, bool SuaKetQuaDaXacNhan);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var tc = PhatHienThayDoi();
        if (tc.SuaKetQuaDaXacNhan) throw new KetQuaDaKhoaException(LoiSuaKetQua);
        if (tc.KhoaDoiNguong.Count > 0 && KetQuaDaXacNhanCuaKhoa(tc.KhoaDoiNguong).Any())
            throw new KetQuaDaKhoaException(LoiDoiNguong);
        if (tc.LopDoiThongTin.Count > 0 && KetQuaDaXacNhanCuaLop(tc.LopDoiThongTin).Any())
            throw new KetQuaDaKhoaException(LoiDoiLop);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var tc = PhatHienThayDoi();
        if (tc.SuaKetQuaDaXacNhan) throw new KetQuaDaKhoaException(LoiSuaKetQua);
        if (tc.KhoaDoiNguong.Count > 0 && await KetQuaDaXacNhanCuaKhoa(tc.KhoaDoiNguong).AnyAsync(cancellationToken))
            throw new KetQuaDaKhoaException(LoiDoiNguong);
        if (tc.LopDoiThongTin.Count > 0 && await KetQuaDaXacNhanCuaLop(tc.LopDoiThongTin).AnyAsync(cancellationToken))
            throw new KetQuaDaKhoaException(LoiDoiLop);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private ThayDoiCanKiemTra PhatHienThayDoi()
    {
        ChangeTracker.DetectChanges();
        var khoa = new List<int>();
        var lop = new List<int>();
        var suaKetQua = false;

        foreach (var e in ChangeTracker.Entries())
        {
            if (e.State is not (EntityState.Modified or EntityState.Deleted)) continue;

            switch (e.Entity)
            {
                case KetQuaDaoTao _ when !ChoPhepSuaKetQuaDaKhoa:
                    // So sánh theo GIÁ TRỊ GỐC: bước xác nhận (false -> true) vẫn được phép lưu.
                    if (e.OriginalValues.GetValue<bool>(nameof(KetQuaDaoTao.DaXacNhan))) suaKetQua = true;
                    break;

                case KhoaDaoTao k when e.State == EntityState.Modified
                    && (e.Property(nameof(KhoaDaoTao.DiemDatToiThieu)).IsModified
                        || e.Property(nameof(KhoaDaoTao.TyLeThamDuToiThieu)).IsModified):
                    khoa.Add(k.MaKhoaDaoTao);
                    break;

                case LopDaoTao l when e.State == EntityState.Modified
                    && (e.Property(nameof(LopDaoTao.NgayBatDau)).IsModified
                        || e.Property(nameof(LopDaoTao.NgayKetThuc)).IsModified
                        || e.Property(nameof(LopDaoTao.MaKhoaDaoTao)).IsModified
                        || (e.Property(nameof(LopDaoTao.TrangThai)).IsModified && l.TrangThai != "HoanThanh")):
                    lop.Add(l.MaLop);
                    break;
            }
        }
        return new ThayDoiCanKiemTra(khoa, lop, suaKetQua);
    }

    private IQueryable<KetQuaDaoTao> KetQuaDaXacNhanCuaKhoa(List<int> maKhoas) =>
        KetQuaDaoTaos.AsNoTracking()
            .Where(x => x.DaXacNhan && maKhoas.Contains(x.DangKyDaoTao.LopDaoTao.MaKhoaDaoTao));

    private IQueryable<KetQuaDaoTao> KetQuaDaXacNhanCuaLop(List<int> maLops) =>
        KetQuaDaoTaos.AsNoTracking()
            .Where(x => x.DaXacNhan && maLops.Contains(x.DangKyDaoTao.MaLop));
}