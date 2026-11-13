using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTaoNoiBo.Migrations
{
    /// <summary>Module 2: bổ sung trạng thái lớp "Chưa mở đăng ký" (ChuaMoDangKy).</summary>
    public partial class ThemTrangThaiChuaMoDangKy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LopDaoTao_TrangThai",
                table: "LopDaoTao");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LopDaoTao_TrangThai",
                table: "LopDaoTao",
                sql: "[TrangThai] IN (N'ChuaMoDangKy', N'MoDangKy', N'DongDangKy', N'DangHoc', N'HoanThanh', N'DaHuy')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LopDaoTao_TrangThai",
                table: "LopDaoTao");

            migrationBuilder.Sql("UPDATE [LopDaoTao] SET [TrangThai] = N'MoDangKy' WHERE [TrangThai] = N'ChuaMoDangKy'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LopDaoTao_TrangThai",
                table: "LopDaoTao",
                sql: "[TrangThai] IN (N'MoDangKy', N'DongDangKy', N'DangHoc', N'HoanThanh', N'DaHuy')");
        }
    }
}
