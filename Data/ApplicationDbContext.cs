// Họ và tên: [Cả nhóm / nhóm trưởng - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: DbContext, DbSet, cấu hình quan hệ và chỉ mục (Fluent API).

using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A4HN.Models;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A4HN.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<PhongBan> PhongBans => Set<PhongBan>();
    public DbSet<LinhVucDaoTao> LinhVucDaoTaos => Set<LinhVucDaoTao>();
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<KhoaDaoTao> KhoaDaoTaos => Set<KhoaDaoTao>();
    public DbSet<LopDaoTao> LopDaoTaos => Set<LopDaoTao>();
    public DbSet<DangKyDaoTao> DangKyDaoTaos => Set<DangKyDaoTao>();
    public DbSet<BuoiHoc> BuoiHocs => Set<BuoiHoc>();
    public DbSet<DiemDanh> DiemDanhs => Set<DiemDanh>();
    public DbSet<KetQuaDaoTao> KetQuaDaoTaos => Set<KetQuaDaoTao>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // Không trùng tên đăng nhập / tên phòng ban / tên lĩnh vực
        mb.Entity<TaiKhoan>().HasIndex(x => x.TenDangNhap).IsUnique();
        mb.Entity<PhongBan>().HasIndex(x => x.TenPhongBan).IsUnique();
        mb.Entity<LinhVucDaoTao>().HasIndex(x => x.TenLinhVuc).IsUnique();

        // TaiKhoan (1) - (0..1) NhanVien
        mb.Entity<NhanVien>()
            .HasOne(n => n.TaiKhoan)
            .WithOne(t => t.NhanVien)
            .HasForeignKey<NhanVien>(n => n.MaTaiKhoan);

        // DangKyDaoTao (1) - (0..1) KetQuaDaoTao
        mb.Entity<KetQuaDaoTao>()
            .HasOne(k => k.DangKyDaoTao)
            .WithOne(d => d.KetQuaDaoTao)
            .HasForeignKey<KetQuaDaoTao>(k => k.MaDangKy);

        // Mỗi nhân viên chỉ có 1 bản ghi điểm danh cho 1 buổi học
        mb.Entity<DiemDanh>().HasIndex(x => new { x.MaBuoiHoc, x.MaNhanVien }).IsUnique();

        // Không xóa dây chuyền (giữ lịch sử đào tạo, tránh lỗi "multiple cascade paths" trên SQL Server)
        foreach (var fk in mb.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
