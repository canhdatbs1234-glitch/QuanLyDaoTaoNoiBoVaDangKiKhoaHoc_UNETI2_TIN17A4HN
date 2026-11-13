using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuanLyDaoTaoNoiBo.Data;

#nullable disable

namespace QuanLyDaoTaoNoiBo.Migrations
{
    /// <summary>Gộp vai trò QuanLy (Nhân sự - Đào tạo) vào QuanTri: còn 3 vai trò QuanTri / NhanVien / GiangVien.</summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261106000000_GopVaiTroQuanLyVaoQuanTri")]
    public partial class GopVaiTroQuanLyVaoQuanTri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TaiKhoan_VaiTro",
                table: "TaiKhoan");

            migrationBuilder.Sql("UPDATE [TaiKhoan] SET [VaiTro] = N'QuanTri' WHERE [VaiTro] = N'QuanLy';");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaiKhoan_VaiTro",
                table: "TaiKhoan",
                sql: "[VaiTro] IN (N'QuanTri', N'NhanVien', N'GiangVien')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TaiKhoan_VaiTro",
                table: "TaiKhoan");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaiKhoan_VaiTro",
                table: "TaiKhoan",
                sql: "[VaiTro] IN (N'QuanTri', N'QuanLy', N'NhanVien', N'GiangVien')");
        }
    }
}
