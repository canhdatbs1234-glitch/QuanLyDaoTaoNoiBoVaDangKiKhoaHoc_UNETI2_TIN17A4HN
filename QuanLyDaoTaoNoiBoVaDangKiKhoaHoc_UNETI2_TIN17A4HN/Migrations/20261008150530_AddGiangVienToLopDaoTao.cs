using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTaoNoiBoVaDangKiKhoaHoc_UNETI2_TIN17A4HN.Migrations
{
    /// <inheritdoc />
    public partial class AddGiangVienToLopDaoTao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TaiKhoan_VaiTro",
                table: "TaiKhoan");

            migrationBuilder.AddColumn<int>(
                name: "MaGiangVien",
                table: "LopDaoTao",
                type: "int",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaiKhoan_VaiTro",
                table: "TaiKhoan",
                sql: "[VaiTro] IN (N'QuanTri', N'QuanLy', N'NhanVien', N'GiangVien')");

            migrationBuilder.CreateIndex(
                name: "IX_LopDaoTao_MaGiangVien",
                table: "LopDaoTao",
                column: "MaGiangVien");

            migrationBuilder.AddForeignKey(
                name: "FK_LopDaoTao_TaiKhoan_MaGiangVien",
                table: "LopDaoTao",
                column: "MaGiangVien",
                principalTable: "TaiKhoan",
                principalColumn: "MaTaiKhoan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopDaoTao_TaiKhoan_MaGiangVien",
                table: "LopDaoTao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TaiKhoan_VaiTro",
                table: "TaiKhoan");

            migrationBuilder.DropIndex(
                name: "IX_LopDaoTao_MaGiangVien",
                table: "LopDaoTao");

            migrationBuilder.DropColumn(
                name: "MaGiangVien",
                table: "LopDaoTao");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaiKhoan_VaiTro",
                table: "TaiKhoan",
                sql: "[VaiTro] IN (N'QuanTri', N'QuanLy', N'NhanVien')");
        }
    }
}
