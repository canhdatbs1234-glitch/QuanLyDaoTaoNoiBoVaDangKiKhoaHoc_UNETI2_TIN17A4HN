// Họ và tên: Phí Bảo Nam
// Mã sinh viên: [Điền MSSV của bạn]
// Nội dung thực hiện: Module 5 - Kết quả đào tạo, Lịch sử đào tạo và Dashboard Thống kê

using System.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Models.Entities;

namespace QuanLyDaoTaoNoiBo.Data;

// NgayDanhGia != null là dấu xác nhận/khóa bền vững, dùng cột đã có, không đổi schema.
public sealed class KetQuaDaKhoaException() : DbUpdateException(
    "Kết quả đã xác nhận và khóa. Không được sửa kết quả hoặc dữ liệu làm thay đổi đánh giá.");

public partial class ApplicationDbContext
{
    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await ProtectLockedResults(cancellationToken);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        await using var transaction = await Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ProtectLockedResults(cancellationToken);
        var count = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return count;
    }

    private async Task ProtectLockedResults(CancellationToken ct)
    {
        ChangeTracker.DetectChanges();
        var classes = new HashSet<int>(); var lessons = new HashSet<int>();
        var courses = new HashSet<int>(); var registrations = new HashSet<int>();
        foreach (var entry in ChangeTracker.Entries().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            int Current(string p) => (int)entry.Property(p).CurrentValue!;
            int Original(string p) => (int)entry.Property(p).OriginalValue!;
            bool Changed(params string[] names) => entry.State == EntityState.Deleted ||
                names.Any(p => entry.Property(p).IsModified);
            switch (entry.Entity)
            {
                case KetQuaDaoTao:
                    registrations.Add(Current(nameof(KetQuaDaoTao.MaDangKy)));
                    if (entry.State != EntityState.Added) registrations.Add(Original(nameof(KetQuaDaoTao.MaDangKy)));
                    break;
                case DangKyDaoTao when entry.State == EntityState.Added || Changed("MaNhanVien", "MaLop", "TrangThai"):
                    classes.Add(Current("MaLop"));
                    if (entry.State != EntityState.Added)
                    {
                        registrations.Add(Current("MaDangKy"));
                        classes.Add(Original("MaLop"));
                    }
                    break;
                case DiemDanh:
                    lessons.Add(Current("MaBuoiHoc"));
                    if (entry.State != EntityState.Added) lessons.Add(Original("MaBuoiHoc"));
                    break;
                case BuoiHoc:
                    classes.Add(Current("MaLop"));
                    if (entry.State != EntityState.Added) classes.Add(Original("MaLop"));
                    break;
                case LopDaoTao when entry.State != EntityState.Added && Changed("MaKhoaDaoTao", "NgayBatDau", "NgayKetThuc", "TrangThai"):
                    classes.Add(Current("MaLop")); break;
                case KhoaDaoTao when entry.State != EntityState.Added && Changed("DiemDatToiThieu", "TyLeThamDuToiThieu"):
                    courses.Add(Current("MaKhoaDaoTao")); break;
            }
        }
        if (lessons.Count > 0)
            classes.UnionWith(await BuoiHocs.AsNoTracking().Where(x => lessons.Contains(x.MaBuoiHoc)).Select(x => x.MaLop).ToListAsync(ct));
        if (registrations.Count == 0 && classes.Count == 0 && courses.Count == 0) return;
        if (await KetQuaDaoTaos.AsNoTracking().AnyAsync(x => x.NgayDanhGia != null &&
            (registrations.Contains(x.MaDangKy) || classes.Contains(x.DangKyDaoTao.MaLop) ||
             courses.Contains(x.DangKyDaoTao.LopDaoTao.MaKhoaDaoTao)), ct))
            throw new KetQuaDaKhoaException();
    }
}

