using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTaoNoiBoVaDangKiKhoaHoc_UNETI2_TIN17A4HN.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LinhVucDaoTao",
                columns: table => new
                {
                    MaLinhVuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLinhVuc = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhVucDaoTao", x => x.MaLinhVuc);
                });

            migrationBuilder.CreateTable(
                name: "PhongBan",
                columns: table => new
                {
                    MaPhongBan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenPhongBan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongBan", x => x.MaPhongBan);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                    table.CheckConstraint("CK_TaiKhoan_VaiTro", "[VaiTro] IN (N'QuanTri', N'QuanLy', N'NhanVien')");
                });

            migrationBuilder.CreateTable(
                name: "KhoaDaoTao",
                columns: table => new
                {
                    MaKhoaDaoTao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhoaDaoTao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaLinhVuc = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SoGioDuKien = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    DiemDatToiThieu = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    TyLeThamDuToiThieu = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaDaoTao", x => x.MaKhoaDaoTao);
                    table.CheckConstraint("CK_KhoaDaoTao_Diem", "[DiemDatToiThieu] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_KhoaDaoTao_SoGio", "[SoGioDuKien] > 0 AND [SoGioDuKien] <= 9999.99");
                    table.CheckConstraint("CK_KhoaDaoTao_TyLe", "[TyLeThamDuToiThieu] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_KhoaDaoTao_LinhVucDaoTao_MaLinhVuc",
                        column: x => x.MaLinhVuc,
                        principalTable: "LinhVucDaoTao",
                        principalColumn: "MaLinhVuc");
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MaNhanVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaPhongBan = table.Column<int>(type: "int", nullable: false),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    NgayVaoLam = table.Column<DateOnly>(type: "date", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.MaNhanVien);
                    table.CheckConstraint("CK_NhanVien_NgayVaoLam", "[NgayVaoLam] >= [NgaySinh]");
                    table.ForeignKey(
                        name: "FK_NhanVien_PhongBan_MaPhongBan",
                        column: x => x.MaPhongBan,
                        principalTable: "PhongBan",
                        principalColumn: "MaPhongBan");
                    table.ForeignKey(
                        name: "FK_NhanVien_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "LopDaoTao",
                columns: table => new
                {
                    MaLop = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhoaDaoTao = table.Column<int>(type: "int", nullable: false),
                    TenLop = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    HanDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuongToiDa = table.Column<int>(type: "int", nullable: false),
                    DiaDiem = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopDaoTao", x => x.MaLop);
                    table.CheckConstraint("CK_LopDaoTao_HanDangKy", "CAST([HanDangKy] AS date) <= [NgayBatDau]");
                    table.CheckConstraint("CK_LopDaoTao_Ngay", "[NgayKetThuc] >= [NgayBatDau]");
                    table.CheckConstraint("CK_LopDaoTao_SoLuong", "[SoLuongToiDa] > 0");
                    table.CheckConstraint("CK_LopDaoTao_TrangThai", "[TrangThai] IN (N'MoDangKy', N'DongDangKy', N'DangHoc', N'HoanThanh', N'DaHuy')");
                    table.ForeignKey(
                        name: "FK_LopDaoTao_KhoaDaoTao_MaKhoaDaoTao",
                        column: x => x.MaKhoaDaoTao,
                        principalTable: "KhoaDaoTao",
                        principalColumn: "MaKhoaDaoTao");
                });

            migrationBuilder.CreateTable(
                name: "BuoiHoc",
                columns: table => new
                {
                    MaBuoiHoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLop = table.Column<int>(type: "int", nullable: false),
                    NgayHoc = table.Column<DateOnly>(type: "date", nullable: false),
                    GioBatDau = table.Column<TimeOnly>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeOnly>(type: "time", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DiaDiem = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuoiHoc", x => x.MaBuoiHoc);
                    table.CheckConstraint("CK_BuoiHoc_Gio", "[GioKetThuc] > [GioBatDau]");
                    table.CheckConstraint("CK_BuoiHoc_TrangThai", "[TrangThai] IN (N'ChuaDienRa', N'DaDienRa', N'DaHuy')");
                    table.ForeignKey(
                        name: "FK_BuoiHoc_LopDaoTao_MaLop",
                        column: x => x.MaLop,
                        principalTable: "LopDaoTao",
                        principalColumn: "MaLop");
                });

            migrationBuilder.CreateTable(
                name: "DangKyDaoTao",
                columns: table => new
                {
                    MaDangKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhanVien = table.Column<int>(type: "int", nullable: false),
                    MaLop = table.Column<int>(type: "int", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyDaoTao", x => x.MaDangKy);
                    table.CheckConstraint("CK_DangKyDaoTao_NgayDuyet", "[NgayDuyet] IS NULL OR [NgayDuyet] >= [NgayDangKy]");
                    table.CheckConstraint("CK_DangKyDaoTao_TrangThai", "[TrangThai] IN (N'ChoDuyet', N'DaDuyet', N'TuChoi', N'DaHuy')");
                    table.ForeignKey(
                        name: "FK_DangKyDaoTao_LopDaoTao_MaLop",
                        column: x => x.MaLop,
                        principalTable: "LopDaoTao",
                        principalColumn: "MaLop");
                    table.ForeignKey(
                        name: "FK_DangKyDaoTao_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "DiemDanh",
                columns: table => new
                {
                    MaDiemDanh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaBuoiHoc = table.Column<int>(type: "int", nullable: false),
                    MaNhanVien = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ThoiGianCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanh", x => x.MaDiemDanh);
                    table.CheckConstraint("CK_DiemDanh_TrangThai", "[TrangThai] IN (N'CoMat', N'VangMat', N'DiMuon', N'CoPhep')");
                    table.ForeignKey(
                        name: "FK_DiemDanh_BuoiHoc_MaBuoiHoc",
                        column: x => x.MaBuoiHoc,
                        principalTable: "BuoiHoc",
                        principalColumn: "MaBuoiHoc");
                    table.ForeignKey(
                        name: "FK_DiemDanh_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "KetQuaDaoTao",
                columns: table => new
                {
                    MaKetQua = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDangKy = table.Column<int>(type: "int", nullable: false),
                    DiemDanhGia = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    TyLeThamDu = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    KetQua = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaDaoTao", x => x.MaKetQua);
                    table.CheckConstraint("CK_KetQuaDaoTao_Diem", "[DiemDanhGia] IS NULL OR [DiemDanhGia] BETWEEN 0 AND 10");
                    table.CheckConstraint("CK_KetQuaDaoTao_KetQua", "[KetQua] IN (N'ChuaDanhGia', N'Dat', N'KhongDat')");
                    table.CheckConstraint("CK_KetQuaDaoTao_TyLe", "[TyLeThamDu] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_KetQuaDaoTao_DangKyDaoTao_MaDangKy",
                        column: x => x.MaDangKy,
                        principalTable: "DangKyDaoTao",
                        principalColumn: "MaDangKy",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_MaLop",
                table: "BuoiHoc",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyDaoTao_MaLop",
                table: "DangKyDaoTao",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyDaoTao_MaNhanVien_MaLop",
                table: "DangKyDaoTao",
                columns: new[] { "MaNhanVien", "MaLop" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_MaBuoiHoc_MaNhanVien",
                table: "DiemDanh",
                columns: new[] { "MaBuoiHoc", "MaNhanVien" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_MaNhanVien",
                table: "DiemDanh",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaDaoTao_MaDangKy",
                table: "KetQuaDaoTao",
                column: "MaDangKy",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhoaDaoTao_MaLinhVuc",
                table: "KhoaDaoTao",
                column: "MaLinhVuc");

            migrationBuilder.CreateIndex(
                name: "IX_LopDaoTao_MaKhoaDaoTao",
                table: "LopDaoTao",
                column: "MaKhoaDaoTao");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_MaPhongBan",
                table: "NhanVien",
                column: "MaPhongBan");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_MaTaiKhoan",
                table: "NhanVien",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_Email",
                table: "TaiKhoan",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiemDanh");

            migrationBuilder.DropTable(
                name: "KetQuaDaoTao");

            migrationBuilder.DropTable(
                name: "BuoiHoc");

            migrationBuilder.DropTable(
                name: "DangKyDaoTao");

            migrationBuilder.DropTable(
                name: "LopDaoTao");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "KhoaDaoTao");

            migrationBuilder.DropTable(
                name: "PhongBan");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "LinhVucDaoTao");
        }
    }
}
