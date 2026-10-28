using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDaoTaoNoiBo.Migrations
{
    /// <inheritdoc />
    public partial class ThemXacNhanKetQua : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== Module 5: trường xác nhận kết quả =====
            migrationBuilder.AddColumn<bool>(
                name: "DaXacNhan",
                table: "KetQuaDaoTao",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaNguoiXacNhan",
                table: "KetQuaDaoTao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayXacNhan",
                table: "KetQuaDaoTao",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaDaoTao_MaNguoiXacNhan",
                table: "KetQuaDaoTao",
                column: "MaNguoiXacNhan");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KetQuaDaoTao_XacNhan",
                table: "KetQuaDaoTao",
                sql: "[DaXacNhan] = 0 OR [KetQua] <> N'ChuaDanhGia'");

            migrationBuilder.AddForeignKey(
                name: "FK_KetQuaDaoTao_TaiKhoan_MaNguoiXacNhan",
                table: "KetQuaDaoTao",
                column: "MaNguoiXacNhan",
                principalTable: "TaiKhoan",
                principalColumn: "MaTaiKhoan");

            // ===== Hai index của Module 1 bị thiếu trong snapshot: chỉ tạo nếu chưa có =====
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PhongBan_TenPhongBan' AND object_id = OBJECT_ID(N'[dbo].[PhongBan]'))
    CREATE UNIQUE INDEX [IX_PhongBan_TenPhongBan] ON [dbo].[PhongBan] ([TenPhongBan]);");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LinhVucDaoTao_TenLinhVuc' AND object_id = OBJECT_ID(N'[dbo].[LinhVucDaoTao]'))
    CREATE UNIQUE INDEX [IX_LinhVucDaoTao_TenLinhVuc] ON [dbo].[LinhVucDaoTao] ([TenLinhVuc]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KetQuaDaoTao_TaiKhoan_MaNguoiXacNhan",
                table: "KetQuaDaoTao");

            migrationBuilder.DropIndex(
                name: "IX_KetQuaDaoTao_MaNguoiXacNhan",
                table: "KetQuaDaoTao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_KetQuaDaoTao_XacNhan",
                table: "KetQuaDaoTao");

            migrationBuilder.DropColumn(name: "DaXacNhan", table: "KetQuaDaoTao");
            migrationBuilder.DropColumn(name: "MaNguoiXacNhan", table: "KetQuaDaoTao");
            migrationBuilder.DropColumn(name: "NgayXacNhan", table: "KetQuaDaoTao");
        }
    }
}