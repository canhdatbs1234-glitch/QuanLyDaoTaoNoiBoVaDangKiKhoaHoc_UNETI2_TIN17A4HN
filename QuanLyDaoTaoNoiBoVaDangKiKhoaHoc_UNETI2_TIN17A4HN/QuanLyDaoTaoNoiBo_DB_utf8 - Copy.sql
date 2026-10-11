USE [QuanLyDaoTaoNoiBo_DB]
GO
/***** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 10/8/2026 10:23:47 PM *****/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BuoiHoc]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BuoiHoc](
	[MaBuoiHoc] [int] IDENTITY(1,1) NOT NULL,
	[MaLop] [int] NOT NULL,
	[NgayHoc] [date] NOT NULL,
	[GioBatDau] [time](7) NOT NULL,
	[GioKetThuc] [time](7) NOT NULL,
	[NoiDung] [nvarchar](2000) NOT NULL,
	[DiaDiem] [nvarchar](250) NOT NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_BuoiHoc] PRIMARY KEY CLUSTERED 
(
	[MaBuoiHoc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DangKyDaoTao]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DangKyDaoTao](
	[MaDangKy] [int] IDENTITY(1,1) NOT NULL,
	[MaNhanVien] [int] NOT NULL,
	[MaLop] [int] NOT NULL,
	[NgayDangKy] [datetime2](7) NOT NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
	[NgayDuyet] [datetime2](7) NULL,
	[GhiChu] [nvarchar](1000) NULL,
 CONSTRAINT [PK_DangKyDaoTao] PRIMARY KEY CLUSTERED 
(
	[MaDangKy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DiemDanh]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DiemDanh](
	[MaDiemDanh] [int] IDENTITY(1,1) NOT NULL,
	[MaBuoiHoc] [int] NOT NULL,
	[MaNhanVien] [int] NOT NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
	[ThoiGianCapNhat] [datetime2](7) NOT NULL,
	[GhiChu] [nvarchar](1000) NULL,
 CONSTRAINT [PK_DiemDanh] PRIMARY KEY CLUSTERED 
(
	[MaDiemDanh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KetQuaDaoTao]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KetQuaDaoTao](
	[MaKetQua] [int] IDENTITY(1,1) NOT NULL,
	[MaDangKy] [int] NOT NULL,
	[DiemDanhGia] [decimal](4, 2) NULL,
	[TyLeThamDu] [decimal](5, 2) NOT NULL,
	[KetQua] [nvarchar](30) NOT NULL,
	[NgayDanhGia] [datetime2](7) NULL,
	[GhiChu] [nvarchar](1000) NULL,
 CONSTRAINT [PK_KetQuaDaoTao] PRIMARY KEY CLUSTERED 
(
	[MaKetQua] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KhoaDaoTao]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KhoaDaoTao](
	[MaKhoaDaoTao] [int] IDENTITY(1,1) NOT NULL,
	[TenKhoaDaoTao] [nvarchar](200) NOT NULL,
	[MaLinhVuc] [int] NOT NULL,
	[MoTa] [nvarchar](2000) NULL,
	[SoGioDuKien] [decimal](6, 2) NOT NULL,
	[DiemDatToiThieu] [decimal](4, 2) NOT NULL,
	[TyLeThamDuToiThieu] [decimal](5, 2) NOT NULL,
	[TrangThai] [bit] NOT NULL,
 CONSTRAINT [PK_KhoaDaoTao] PRIMARY KEY CLUSTERED 
(
	[MaKhoaDaoTao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LinhVucDaoTao]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LinhVucDaoTao](
	[MaLinhVuc] [int] IDENTITY(1,1) NOT NULL,
	[TenLinhVuc] [nvarchar](150) NOT NULL,
	[MoTa] [nvarchar](1000) NULL,
	[TrangThai] [bit] NOT NULL,
 CONSTRAINT [PK_LinhVucDaoTao] PRIMARY KEY CLUSTERED 
(
	[MaLinhVuc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LopDaoTao]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LopDaoTao](
	[MaLop] [int] IDENTITY(1,1) NOT NULL,
	[MaKhoaDaoTao] [int] NOT NULL,
	[TenLop] [nvarchar](200) NOT NULL,
	[NgayBatDau] [date] NOT NULL,
	[NgayKetThuc] [date] NOT NULL,
	[HanDangKy] [datetime2](7) NOT NULL,
	[SoLuongToiDa] [int] NOT NULL,
	[DiaDiem] [nvarchar](250) NOT NULL,
	[TrangThai] [nvarchar](30) NOT NULL,
	[MaGiangVien] [int] NULL,
 CONSTRAINT [PK_LopDaoTao] PRIMARY KEY CLUSTERED 
(
	[MaLop] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NhanVien]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NhanVien](
	[MaNhanVien] [int] IDENTITY(1,1) NOT NULL,
	[MaTaiKhoan] [int] NOT NULL,
	[HoTen] [nvarchar](100) NOT NULL,
	[MaPhongBan] [int] NOT NULL,
	[NgaySinh] [date] NOT NULL,
	[SoDienThoai] [nvarchar](20) NULL,
	[Email] [nvarchar](254) NOT NULL,
	[NgayVaoLam] [date] NOT NULL,
	[TrangThai] [bit] NOT NULL,
 CONSTRAINT [PK_NhanVien] PRIMARY KEY CLUSTERED 
(
	[MaNhanVien] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PhongBan]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhongBan](
	[MaPhongBan] [int] IDENTITY(1,1) NOT NULL,
	[TenPhongBan] [nvarchar](100) NOT NULL,
	[MoTa] [nvarchar](1000) NULL,
	[TrangThai] [bit] NOT NULL,
 CONSTRAINT [PK_PhongBan] PRIMARY KEY CLUSTERED 
(
	[MaPhongBan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TaiKhoan]    Script Date: 10/8/2026 10:23:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaiKhoan](
	[MaTaiKhoan] [int] IDENTITY(1,1) NOT NULL,
	[TenDangNhap] [nvarchar](50) NOT NULL,
	[MatKhau] [nvarchar](500) NOT NULL,
	[HoTen] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](254) NOT NULL,
	[VaiTro] [nvarchar](30) NOT NULL,
	[TrangThai] [bit] NOT NULL,
 CONSTRAINT [PK_TaiKhoan] PRIMARY KEY CLUSTERED 
(
	[MaTaiKhoan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[BuoiHoc]  WITH CHECK ADD  CONSTRAINT [FK_BuoiHoc_LopDaoTao_MaLop] FOREIGN KEY([MaLop])
REFERENCES [dbo].[LopDaoTao] ([MaLop])
GO
ALTER TABLE [dbo].[BuoiHoc] CHECK CONSTRAINT [FK_BuoiHoc_LopDaoTao_MaLop]
GO
ALTER TABLE [dbo].[DangKyDaoTao]  WITH CHECK ADD  CONSTRAINT [FK_DangKyDaoTao_LopDaoTao_MaLop] FOREIGN KEY([MaLop])
REFERENCES [dbo].[LopDaoTao] ([MaLop])
GO
ALTER TABLE [dbo].[DangKyDaoTao] CHECK CONSTRAINT [FK_DangKyDaoTao_LopDaoTao_MaLop]
GO
ALTER TABLE [dbo].[DangKyDaoTao]  WITH CHECK ADD  CONSTRAINT [FK_DangKyDaoTao_NhanVien_MaNhanVien] FOREIGN KEY([MaNhanVien])
REFERENCES [dbo].[NhanVien] ([MaNhanVien])
GO
ALTER TABLE [dbo].[DangKyDaoTao] CHECK CONSTRAINT [FK_DangKyDaoTao_NhanVien_MaNhanVien]
GO
ALTER TABLE [dbo].[DiemDanh]  WITH CHECK ADD  CONSTRAINT [FK_DiemDanh_BuoiHoc_MaBuoiHoc] FOREIGN KEY([MaBuoiHoc])
REFERENCES [dbo].[BuoiHoc] ([MaBuoiHoc])
GO
ALTER TABLE [dbo].[DiemDanh] CHECK CONSTRAINT [FK_DiemDanh_BuoiHoc_MaBuoiHoc]
GO
ALTER TABLE [dbo].[DiemDanh]  WITH CHECK ADD  CONSTRAINT [FK_DiemDanh_NhanVien_MaNhanVien] FOREIGN KEY([MaNhanVien])
REFERENCES [dbo].[NhanVien] ([MaNhanVien])
GO
ALTER TABLE [dbo].[DiemDanh] CHECK CONSTRAINT [FK_DiemDanh_NhanVien_MaNhanVien]
GO
ALTER TABLE [dbo].[KetQuaDaoTao]  WITH CHECK ADD  CONSTRAINT [FK_KetQuaDaoTao_DangKyDaoTao_MaDangKy] FOREIGN KEY([MaDangKy])
REFERENCES [dbo].[DangKyDaoTao] ([MaDangKy])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[KetQuaDaoTao] CHECK CONSTRAINT [FK_KetQuaDaoTao_DangKyDaoTao_MaDangKy]
GO
ALTER TABLE [dbo].[KhoaDaoTao]  WITH CHECK ADD  CONSTRAINT [FK_KhoaDaoTao_LinhVucDaoTao_MaLinhVuc] FOREIGN KEY([MaLinhVuc])
REFERENCES [dbo].[LinhVucDaoTao] ([MaLinhVuc])
GO
ALTER TABLE [dbo].[KhoaDaoTao] CHECK CONSTRAINT [FK_KhoaDaoTao_LinhVucDaoTao_MaLinhVuc]
GO
ALTER TABLE [dbo].[LopDaoTao]  WITH CHECK ADD  CONSTRAINT [FK_LopDaoTao_KhoaDaoTao_MaKhoaDaoTao] FOREIGN KEY([MaKhoaDaoTao])
REFERENCES [dbo].[KhoaDaoTao] ([MaKhoaDaoTao])
GO
ALTER TABLE [dbo].[LopDaoTao] CHECK CONSTRAINT [FK_LopDaoTao_KhoaDaoTao_MaKhoaDaoTao]
GO
ALTER TABLE [dbo].[LopDaoTao]  WITH CHECK ADD  CONSTRAINT [FK_LopDaoTao_TaiKhoan_MaGiangVien] FOREIGN KEY([MaGiangVien])
REFERENCES [dbo].[TaiKhoan] ([MaTaiKhoan])
GO
ALTER TABLE [dbo].[LopDaoTao] CHECK CONSTRAINT [FK_LopDaoTao_TaiKhoan_MaGiangVien]
GO
ALTER TABLE [dbo].[NhanVien]  WITH CHECK ADD  CONSTRAINT [FK_NhanVien_PhongBan_MaPhongBan] FOREIGN KEY([MaPhongBan])
REFERENCES [dbo].[PhongBan] ([MaPhongBan])
GO
ALTER TABLE [dbo].[NhanVien] CHECK CONSTRAINT [FK_NhanVien_PhongBan_MaPhongBan]
GO
ALTER TABLE [dbo].[NhanVien]  WITH CHECK ADD  CONSTRAINT [FK_NhanVien_TaiKhoan_MaTaiKhoan] FOREIGN KEY([MaTaiKhoan])
REFERENCES [dbo].[TaiKhoan] ([MaTaiKhoan])
GO
ALTER TABLE [dbo].[NhanVien] CHECK CONSTRAINT [FK_NhanVien_TaiKhoan_MaTaiKhoan]
GO
ALTER TABLE [dbo].[BuoiHoc]  WITH CHECK ADD  CONSTRAINT [CK_BuoiHoc_Gio] CHECK  (([GioKetThuc]>[GioBatDau]))
GO
ALTER TABLE [dbo].[BuoiHoc] CHECK CONSTRAINT [CK_BuoiHoc_Gio]
GO
ALTER TABLE [dbo].[BuoiHoc]  WITH CHECK ADD  CONSTRAINT [CK_BuoiHoc_TrangThai] CHECK  (([TrangThai]=N'DaHuy' OR [TrangThai]=N'DaDienRa' OR [TrangThai]=N'ChuaDienRa'))
GO
ALTER TABLE [dbo].[BuoiHoc] CHECK CONSTRAINT [CK_BuoiHoc_TrangThai]
GO
ALTER TABLE [dbo].[DangKyDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_DangKyDaoTao_NgayDuyet] CHECK  (([NgayDuyet] IS NULL OR [NgayDuyet]>=[NgayDangKy]))
GO
ALTER TABLE [dbo].[DangKyDaoTao] CHECK CONSTRAINT [CK_DangKyDaoTao_NgayDuyet]
GO
ALTER TABLE [dbo].[DangKyDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_DangKyDaoTao_TrangThai] CHECK  (([TrangThai]=N'DaHuy' OR [TrangThai]=N'TuChoi' OR [TrangThai]=N'DaDuyet' OR [TrangThai]=N'ChoDuyet'))
GO
ALTER TABLE [dbo].[DangKyDaoTao] CHECK CONSTRAINT [CK_DangKyDaoTao_TrangThai]
GO
ALTER TABLE [dbo].[DiemDanh]  WITH CHECK ADD  CONSTRAINT [CK_DiemDanh_TrangThai] CHECK  (([TrangThai]=N'CoPhep' OR [TrangThai]=N'DiMuon' OR [TrangThai]=N'VangMat' OR [TrangThai]=N'CoMat'))
GO
ALTER TABLE [dbo].[DiemDanh] CHECK CONSTRAINT [CK_DiemDanh_TrangThai]
GO
ALTER TABLE [dbo].[KetQuaDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_KetQuaDaoTao_Diem] CHECK  (([DiemDanhGia] IS NULL OR [DiemDanhGia]>=(0) AND [DiemDanhGia]<=(10)))
GO
ALTER TABLE [dbo].[KetQuaDaoTao] CHECK CONSTRAINT [CK_KetQuaDaoTao_Diem]
GO
ALTER TABLE [dbo].[KetQuaDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_KetQuaDaoTao_KetQua] CHECK  (([KetQua]=N'KhongDat' OR [KetQua]=N'Dat' OR [KetQua]=N'ChuaDanhGia'))
GO
ALTER TABLE [dbo].[KetQuaDaoTao] CHECK CONSTRAINT [CK_KetQuaDaoTao_KetQua]
GO
ALTER TABLE [dbo].[KetQuaDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_KetQuaDaoTao_TyLe] CHECK  (([TyLeThamDu]>=(0) AND [TyLeThamDu]<=(100)))
GO
ALTER TABLE [dbo].[KetQuaDaoTao] CHECK CONSTRAINT [CK_KetQuaDaoTao_TyLe]
GO
ALTER TABLE [dbo].[KhoaDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_KhoaDaoTao_Diem] CHECK  (([DiemDatToiThieu]>=(0) AND [DiemDatToiThieu]<=(10)))
GO
ALTER TABLE [dbo].[KhoaDaoTao] CHECK CONSTRAINT [CK_KhoaDaoTao_Diem]
GO
ALTER TABLE [dbo].[KhoaDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_KhoaDaoTao_SoGio] CHECK  (([SoGioDuKien]>(0) AND [SoGioDuKien]<=(9999.99)))
GO
ALTER TABLE [dbo].[KhoaDaoTao] CHECK CONSTRAINT [CK_KhoaDaoTao_SoGio]
GO
ALTER TABLE [dbo].[KhoaDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_KhoaDaoTao_TyLe] CHECK  (([TyLeThamDuToiThieu]>=(0) AND [TyLeThamDuToiThieu]<=(100)))
GO
ALTER TABLE [dbo].[KhoaDaoTao] CHECK CONSTRAINT [CK_KhoaDaoTao_TyLe]
GO
ALTER TABLE [dbo].[LopDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_LopDaoTao_HanDangKy] CHECK  ((CONVERT([date],[HanDangKy])<=[NgayBatDau]))
GO
ALTER TABLE [dbo].[LopDaoTao] CHECK CONSTRAINT [CK_LopDaoTao_HanDangKy]
GO
ALTER TABLE [dbo].[LopDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_LopDaoTao_Ngay] CHECK  (([NgayKetThuc]>=[NgayBatDau]))
GO
ALTER TABLE [dbo].[LopDaoTao] CHECK CONSTRAINT [CK_LopDaoTao_Ngay]
GO
ALTER TABLE [dbo].[LopDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_LopDaoTao_SoLuong] CHECK  (([SoLuongToiDa]>(0)))
GO
ALTER TABLE [dbo].[LopDaoTao] CHECK CONSTRAINT [CK_LopDaoTao_SoLuong]
GO
ALTER TABLE [dbo].[LopDaoTao]  WITH CHECK ADD  CONSTRAINT [CK_LopDaoTao_TrangThai] CHECK  (([TrangThai]=N'DaHuy' OR [TrangThai]=N'HoanThanh' OR [TrangThai]=N'DangHoc' OR [TrangThai]=N'DongDangKy' OR [TrangThai]=N'MoDangKy'))
GO
ALTER TABLE [dbo].[LopDaoTao] CHECK CONSTRAINT [CK_LopDaoTao_TrangThai]
GO
ALTER TABLE [dbo].[NhanVien]  WITH CHECK ADD  CONSTRAINT [CK_NhanVien_NgayVaoLam] CHECK  (([NgayVaoLam]>=[NgaySinh]))
GO
ALTER TABLE [dbo].[NhanVien] CHECK CONSTRAINT [CK_NhanVien_NgayVaoLam]
GO
ALTER TABLE [dbo].[TaiKhoan]  WITH CHECK ADD  CONSTRAINT [CK_TaiKhoan_VaiTro] CHECK  (([VaiTro]=N'GiangVien' OR [VaiTro]=N'NhanVien' OR [VaiTro]=N'QuanLy' OR [VaiTro]=N'QuanTri'))
GO
ALTER TABLE [dbo].[TaiKhoan] CHECK CONSTRAINT [CK_TaiKhoan_VaiTro]
GO
