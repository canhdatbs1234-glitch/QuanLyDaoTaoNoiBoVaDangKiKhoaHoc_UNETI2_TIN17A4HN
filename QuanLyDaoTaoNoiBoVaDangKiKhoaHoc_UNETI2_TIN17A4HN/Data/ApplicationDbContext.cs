using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBoVaDangKiKhoaHoc_UNETI2_TIN17A4HN.Models;

namespace QuanLyDaoTaoNoiBoVaDangKiKhoaHoc_UNETI2_TIN17A4HN.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.ToTable("TaiKhoan", table => table.HasCheckConstraint(
                "CK_TaiKhoan_VaiTro", "[VaiTro] IN (N'QuanTri', N'QuanLy', N'NhanVien', N'GiangVien')"));
            entity.HasIndex(x => x.TenDangNhap).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<PhongBan>().ToTable("PhongBan");
        modelBuilder.Entity<LinhVucDaoTao>().ToTable("LinhVucDaoTao");

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.ToTable("NhanVien", table => table.HasCheckConstraint(
                "CK_NhanVien_NgayVaoLam", "[NgayVaoLam] >= [NgaySinh]"));
            entity.Property(x => x.NgaySinh).HasColumnType("date");
            entity.Property(x => x.NgayVaoLam).HasColumnType("date");
            entity.HasIndex(x => x.MaTaiKhoan).IsUnique();
            entity.HasOne(x => x.TaiKhoan).WithOne(x => x.NhanVien)
                .HasForeignKey<NhanVien>(x => x.MaTaiKhoan)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.PhongBan).WithMany(x => x.NhanViens)
                .HasForeignKey(x => x.MaPhongBan)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<KhoaDaoTao>(entity =>
        {
            entity.ToTable("KhoaDaoTao", table =>
            {
                table.HasCheckConstraint("CK_KhoaDaoTao_SoGio", "[SoGioDuKien] > 0 AND [SoGioDuKien] <= 9999.99");
                table.HasCheckConstraint("CK_KhoaDaoTao_Diem", "[DiemDatToiThieu] BETWEEN 0 AND 10");
                table.HasCheckConstraint("CK_KhoaDaoTao_TyLe", "[TyLeThamDuToiThieu] BETWEEN 0 AND 100");
            });
            entity.Property(x => x.SoGioDuKien).HasPrecision(6, 2);
            entity.Property(x => x.DiemDatToiThieu).HasPrecision(4, 2);
            entity.Property(x => x.TyLeThamDuToiThieu).HasPrecision(5, 2);
            entity.HasOne(x => x.LinhVucDaoTao).WithMany(x => x.KhoaDaoTaos)
                .HasForeignKey(x => x.MaLinhVuc)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<LopDaoTao>(entity =>
        {
            entity.ToTable("LopDaoTao", table =>
            {
                table.HasCheckConstraint("CK_LopDaoTao_Ngay", "[NgayKetThuc] >= [NgayBatDau]");
                table.HasCheckConstraint("CK_LopDaoTao_HanDangKy", "CAST([HanDangKy] AS date) <= [NgayBatDau]");
                table.HasCheckConstraint("CK_LopDaoTao_SoLuong", "[SoLuongToiDa] > 0");
                table.HasCheckConstraint("CK_LopDaoTao_TrangThai",
                    "[TrangThai] IN (N'MoDangKy', N'DongDangKy', N'DangHoc', N'HoanThanh', N'DaHuy')");
            });
            entity.Property(x => x.NgayBatDau).HasColumnType("date");
            entity.Property(x => x.NgayKetThuc).HasColumnType("date");
            entity.Property(x => x.HanDangKy).HasColumnType("datetime2");
            entity.HasOne(x => x.KhoaDaoTao).WithMany(x => x.LopDaoTaos)
                .HasForeignKey(x => x.MaKhoaDaoTao)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.GiangVien).WithMany()
                .HasForeignKey(x => x.MaGiangVien)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DangKyDaoTao>(entity =>
        {
            entity.ToTable("DangKyDaoTao", table =>
            {
                table.HasCheckConstraint("CK_DangKyDaoTao_NgayDuyet", "[NgayDuyet] IS NULL OR [NgayDuyet] >= [NgayDangKy]");
                table.HasCheckConstraint("CK_DangKyDaoTao_TrangThai",
                    "[TrangThai] IN (N'ChoDuyet', N'DaDuyet', N'TuChoi', N'DaHuy')");
            });
            // Một nhân viên chỉ có một bản ghi đăng ký trong mỗi lớp.
            entity.HasIndex(x => new { x.MaNhanVien, x.MaLop }).IsUnique();
            entity.Property(x => x.NgayDangKy).HasColumnType("datetime2");
            entity.Property(x => x.NgayDuyet).HasColumnType("datetime2");
            entity.HasOne(x => x.NhanVien).WithMany(x => x.DangKyDaoTaos)
                .HasForeignKey(x => x.MaNhanVien)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.LopDaoTao).WithMany(x => x.DangKyDaoTaos)
                .HasForeignKey(x => x.MaLop)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<BuoiHoc>(entity =>
        {
            entity.ToTable("BuoiHoc", table =>
            {
                table.HasCheckConstraint("CK_BuoiHoc_Gio", "[GioKetThuc] > [GioBatDau]");
                table.HasCheckConstraint("CK_BuoiHoc_TrangThai", "[TrangThai] IN (N'ChuaDienRa', N'DaDienRa', N'DaHuy')");
            });
            entity.Property(x => x.NgayHoc).HasColumnType("date");
            entity.Property(x => x.GioBatDau).HasColumnType("time");
            entity.Property(x => x.GioKetThuc).HasColumnType("time");
            entity.HasOne(x => x.LopDaoTao).WithMany(x => x.BuoiHocs)
                .HasForeignKey(x => x.MaLop)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DiemDanh>(entity =>
        {
            entity.ToTable("DiemDanh", table => table.HasCheckConstraint(
                "CK_DiemDanh_TrangThai", "[TrangThai] IN (N'CoMat', N'VangMat', N'DiMuon', N'CoPhep')"));
            entity.HasIndex(x => new { x.MaBuoiHoc, x.MaNhanVien }).IsUnique();
            entity.Property(x => x.ThoiGianCapNhat).HasColumnType("datetime2");
            // Bảo toàn lịch sử điểm danh: buổi học có điểm danh không được xóa trực tiếp.
            entity.HasOne(x => x.BuoiHoc).WithMany(x => x.DiemDanhs)
                .HasForeignKey(x => x.MaBuoiHoc)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.NhanVien).WithMany(x => x.DiemDanhs)
                .HasForeignKey(x => x.MaNhanVien)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<KetQuaDaoTao>(entity =>
        {
            entity.ToTable("KetQuaDaoTao", table =>
            {
                table.HasCheckConstraint("CK_KetQuaDaoTao_Diem", "[DiemDanhGia] IS NULL OR [DiemDanhGia] BETWEEN 0 AND 10");
                table.HasCheckConstraint("CK_KetQuaDaoTao_TyLe", "[TyLeThamDu] BETWEEN 0 AND 100");
                table.HasCheckConstraint("CK_KetQuaDaoTao_KetQua", "[KetQua] IN (N'ChuaDanhGia', N'Dat', N'KhongDat')");
            });
            entity.Property(x => x.DiemDanhGia).HasPrecision(4, 2);
            entity.Property(x => x.TyLeThamDu).HasPrecision(5, 2);
            entity.Property(x => x.NgayDanhGia).HasColumnType("datetime2");
            entity.HasIndex(x => x.MaDangKy).IsUnique();
            // Kết quả là thành phần phụ thuộc của đăng ký; chỉ xóa theo đăng ký.
            entity.HasOne(x => x.DangKyDaoTao).WithOne(x => x.KetQuaDaoTao)
                .HasForeignKey<KetQuaDaoTao>(x => x.MaDangKy)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
