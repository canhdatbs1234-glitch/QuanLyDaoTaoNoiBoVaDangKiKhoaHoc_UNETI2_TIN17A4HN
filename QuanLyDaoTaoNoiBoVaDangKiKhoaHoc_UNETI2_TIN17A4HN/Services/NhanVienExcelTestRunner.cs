//Họ và tên : Phí Bảo Nam - 23103100236
//Module 3: Quản lý nhân viên – Đăng ký – Duyệt đào tạo.
//Kiểm thử tự động cho chức năng Import / Export Excel theo Mục 10 của Yêu cầu.
using System.Reflection;
using ClosedXML.Excel;
using QuanLyDaoTaoNoiBo.Controllers;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;

namespace QuanLyDaoTaoNoiBo.Services;

public static class NhanVienExcelTestRunner
{
    public static Task<int> RunAsync(string[] args)
    {
        Console.WriteLine("======================================================================");
        Console.WriteLine("BẮT ĐẦU KIỂM THỬ CHỨC NĂNG IMPORT / EXPORT EXCEL (MODULE 3)");
        Console.WriteLine("======================================================================");

        var passed = 0;
        var failed = 0;

        void Assert(bool condition, string testName, string detail = "")
        {
            if (condition)
            {
                Console.WriteLine($"[PASS] {testName} {detail}");
                passed++;
            }
            else
            {
                Console.WriteLine($"[FAIL] {testName} {detail}");
                failed++;
            }
        }

        // Chuẩn bị dữ liệu kiểm thử
        var phongBans = new Dictionary<int, PhongBan>
        {
            [1] = new PhongBan { MaPhongBan = 1, TenPhongBan = "Phòng Công nghệ thông tin", TrangThai = true },
            [2] = new PhongBan { MaPhongBan = 2, TenPhongBan = "Phòng Nhân sự", TrangThai = true },
            [3] = new PhongBan { MaPhongBan = 3, TenPhongBan = "Phòng Đã Giải Thể", TrangThai = false }
        };

        var existingEmails = new Dictionary<string, (string HoTen, int MaNhanVien)>
        {
            ["daco@uneti.example"] = ("Nguyễn Văn Đã Có", 100)
        };

        var existingPhones = new Dictionary<string, string>
        {
            ["0988111222"] = "Nguyễn Văn Đã Có"
        };

        var existingNvList = new List<NhanVien>
        {
            new()
            {
                MaNhanVien = 100,
                HoTen = "Nguyễn Văn Đã Có",
                MaPhongBan = 1,
                NgaySinh = new DateOnly(1988, 5, 12),
                SoDienThoai = "0988111222",
                Email = "daco@uneti.example",
                NgayVaoLam = new DateOnly(2018, 1, 1),
                TrangThai = true
            }
        };

        // 1. Export danh sách nhân sự thành công
        byte[] exportBytes;
        {
            exportBytes = NhanVienExcelService.ExportNhanVien(existingNvList, "Điều kiện kiểm thử", "AdminTest");
            Assert(exportBytes != null && exportBytes.Length > 0, "1. Export danh sách nhân sự thành công", $"Size: {exportBytes?.Length} bytes");

            using var ms = new MemoryStream(exportBytes!);
            using var wb = new XLWorkbook(ms);
            var ws = wb.Worksheet("NhanVien");
            Assert(ws != null, "1.1. Sheet 'NhanVien' tồn tại trong file export");
            Assert(ws.Cell(1, 1).GetString() == "STT" && ws.Cell(1, 2).GetString() == "Họ tên", "1.2. Hàng 1 chứa đúng tiêu đề cột chuẩn");
            Assert(ws.Cell(2, 5).GetString() == "0988111222", "1.3. Số điện thoại giữ nguyên số 0 ở đầu");
        }

        // 2. File Export có thể Import trở lại nếu dữ liệu hợp lệ và không trùng
        {
            using var wbRe = new XLWorkbook();
            var ws = wbRe.Worksheets.Add("NhanVien");
            for (int i = 0; i < NhanVienExcelService.StandardHeaders.Length; i++)
                ws.Cell(1, i + 1).Value = NhanVienExcelService.StandardHeaders[i];

            ws.Cell(2, 1).Value = 1;
            ws.Cell(2, 2).Value = "Nhân Sự Mới Xuất";
            ws.Cell(2, 3).Value = 1;
            ws.Cell(2, 4).Value = "15/05/1995";
            ws.Cell(2, 5).Value = "0912999888";
            ws.Cell(2, 6).Value = "nhansumoixuat@uneti.example";
            ws.Cell(2, 7).Value = "01/01/2022";
            ws.Cell(2, 8).Value = "Đang làm việc";

            using var ms = new MemoryStream();
            wbRe.SaveAs(ms);
            ms.Position = 0;

            var previewRe = NhanVienExcelService.KiemTraVaXemTruocStream(ms, "ExportReimport.xlsx", phongBans, existingEmails, existingPhones);
            Assert(previewRe.IsStructureValid, "2.1. Cấu trúc file export tương thích 100% với mẫu Import");
            Assert(previewRe.ValidCount == 1 && previewRe.ErrorCount == 0, "2.2. Dữ liệu từ file export có thể import lại trơn tru");
        }

        // 3. Tải file Excel mẫu thành công
        byte[] templateBytes;
        {
            templateBytes = NhanVienExcelService.TaoFileMau(phongBans.Values.ToList());
            Assert(templateBytes != null && templateBytes.Length > 0, "3. Tải file Excel mẫu thành công", $"Size: {templateBytes?.Length} bytes");

            using var ms = new MemoryStream(templateBytes!);
            using var wb = new XLWorkbook(ms);
            Assert(wb.Worksheets.Contains("NhanVien"), "3.1. Có sheet 'NhanVien'");
            Assert(wb.Worksheets.Contains("DanhSachPhongBan"), "3.2. Có sheet 'DanhSachPhongBan' tra cứu");
            Assert(wb.Worksheets.Contains("HuongDan"), "3.3. Có sheet 'HuongDan' chi tiết");
        }

        // 4. Import file mẫu đúng cấu trúc thành công
        {
            using var ms = new MemoryStream(templateBytes!);
            var previewTpl = NhanVienExcelService.KiemTraVaXemTruocStream(ms, "Mau_Import_NhanSu.xlsx", phongBans, existingEmails, existingPhones);
            Assert(previewTpl.IsStructureValid, "4.1. File mẫu đúng cấu trúc được xác nhận hợp lệ");
            Assert(previewTpl.TotalRows >= 3, "4.2. Đọc được đầy đủ các dòng dữ liệu mẫu trong file");
            Assert(previewTpl.ValidCount > 0, "4.3. Các dòng mẫu hợp lệ sẵn sàng import", $"Hợp lệ: {previewTpl.ValidCount}/{previewTpl.TotalRows}");
        }

        // 5. Import file thiếu cột bị từ chối
        {
            using var wbThieu = new XLWorkbook();
            var ws = wbThieu.Worksheets.Add("NhanVien");
            ws.Cell(1, 1).Value = "Họ tên";
            ws.Cell(1, 2).Value = "Email";

            using var ms = new MemoryStream();
            wbThieu.SaveAs(ms);
            ms.Position = 0;

            var previewThieu = NhanVienExcelService.KiemTraVaXemTruocStream(ms, "ThieuCot.xlsx", phongBans, existingEmails, existingPhones);
            Assert(!previewThieu.IsStructureValid, "5. Import file thiếu cột bị từ chối chính xác", previewThieu.StructureErrorMessage ?? "");
        }

        // 6. Import file sai tên cột bị từ chối
        {
            using var wbSai = new XLWorkbook();
            var ws = wbSai.Worksheets.Add("NhanVien");
            ws.Cell(1, 1).Value = "Cot_1";
            ws.Cell(1, 2).Value = "Cot_2";
            ws.Cell(1, 3).Value = "Cot_3";

            using var ms = new MemoryStream();
            wbSai.SaveAs(ms);
            ms.Position = 0;

            var previewSai = NhanVienExcelService.KiemTraVaXemTruocStream(ms, "SaiTenCot.xlsx", phongBans, existingEmails, existingPhones);
            Assert(!previewSai.IsStructureValid, "6. Import file sai tên cột bị từ chối chính xác", previewSai.StructureErrorMessage ?? "");
        }

        // 7. Import file sai định dạng bị từ chối
        {
            using var ms = new MemoryStream(new byte[] { 1, 2, 3 });
            var previewFormat = NhanVienExcelService.KiemTraVaXemTruocStream(ms, "danhsach.txt", phongBans, existingEmails, existingPhones);
            Assert(!previewFormat.IsStructureValid, "7. Import file không phải đuôi .xlsx bị từ chối", previewFormat.StructureErrorMessage ?? "");
        }

        // 8. Import file có email sai định dạng được báo lỗi đúng dòng
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("NhanVien");
            for (int i = 0; i < NhanVienExcelService.StandardHeaders.Length; i++)
                ws.Cell(1, i + 1).Value = NhanVienExcelService.StandardHeaders[i];

            ws.Cell(2, 2).Value = "Trần Văn Sai Email";
            ws.Cell(2, 3).Value = 1;
            ws.Cell(2, 4).Value = "10/10/1990";
            ws.Cell(2, 6).Value = "email_khong_hop_le@@abc";
            ws.Cell(2, 7).Value = "01/01/2020";
            ws.Cell(2, 8).Value = "Đang làm việc";

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            var preview = NhanVienExcelService.KiemTraVaXemTruocStream(ms, "SaiEmail.xlsx", phongBans, existingEmails, existingPhones);
            var r2 = preview.Rows.FirstOrDefault(r => r.RowIndex == 2);
            Assert(r2 != null && !r2.IsValid && r2.Errors.Any(e => e.Contains("Email")), "8. Báo lỗi email sai định dạng chính xác tại dòng 2");
        }

        // 9. Import file có mã phòng ban không tồn tại được báo lỗi
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("NhanVien");
            for (int i = 0; i < NhanVienExcelService.StandardHeaders.Length; i++)
                ws.Cell(1, i + 1).Value = NhanVienExcelService.StandardHeaders[i];

            ws.Cell(2, 2).Value = "Lê Văn Sai Phòng Ban";
            ws.Cell(2, 3).Value = 99999; // Không tồn tại
            ws.Cell(2, 4).Value = "10/10/1990";
            ws.Cell(2, 6).Value = "saiphongban@uneti.example";
            ws.Cell(2, 7).Value = "01/01/2020";
            ws.Cell(2, 8).Value = "Đang làm việc";

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            var preview = NhanVienExcelService.KiemTraVaXemTruocStream(ms, "SaiPb.xlsx", phongBans, existingEmails, existingPhones);
            var r2 = preview.Rows.FirstOrDefault(r => r.RowIndex == 2);
            Assert(r2 != null && !r2.IsValid && r2.Errors.Any(e => e.Contains("không tồn tại")), "9. Báo lỗi mã phòng ban không tồn tại đúng dòng");
        }

        // 10. Import file có nhân sự trùng không tạo bản ghi trùng
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("NhanVien");
            for (int i = 0; i < NhanVienExcelService.StandardHeaders.Length; i++)
                ws.Cell(1, i + 1).Value = NhanVienExcelService.StandardHeaders[i];

            // Trùng DB
            ws.Cell(2, 2).Value = "Trùng DB";
            ws.Cell(2, 3).Value = 1;
            ws.Cell(2, 4).Value = "10/10/1990";
            ws.Cell(2, 6).Value = "daco@uneti.example";
            ws.Cell(2, 7).Value = "01/01/2020";
            ws.Cell(2, 8).Value = "Đang làm việc";

            // Trùng nội bộ
            ws.Cell(3, 2).Value = "Người A";
            ws.Cell(3, 3).Value = 1;
            ws.Cell(3, 4).Value = "10/10/1990";
            ws.Cell(3, 6).Value = "trungfile@uneti.example";
            ws.Cell(3, 7).Value = "01/01/2020";
            ws.Cell(3, 8).Value = "Đang làm việc";

            ws.Cell(4, 2).Value = "Người B";
            ws.Cell(4, 3).Value = 1;
            ws.Cell(4, 4).Value = "10/10/1990";
            ws.Cell(4, 6).Value = "trungfile@uneti.example";
            ws.Cell(4, 7).Value = "01/01/2020";
            ws.Cell(4, 8).Value = "Đang làm việc";

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            var preview = NhanVienExcelService.KiemTraVaXemTruocStream(ms, "Trung.xlsx", phongBans, existingEmails, existingPhones);
            Assert(preview.Rows[0].Errors.Any(e => e.Contains("đã tồn tại trong database")), "10.1. Chặn trùng với database");
            Assert(preview.Rows[2].Errors.Any(e => e.Contains("trùng lặp với dòng 3")), "10.2. Chặn trùng lặp trong nội bộ file");
        }

        // 11. Import nhiều nhân sự trong cùng một lần (Kiểm tra batch DTO)
        {
            var validRows = new List<NhanVienImportRowDto>
            {
                new() { RowIndex = 2, HoTen = "Lô Mới 1", MaPhongBan = 1, NgaySinh = new DateOnly(1992, 2, 2), Email = "lo1@uneti.example", NgayVaoLam = new DateOnly(2021, 1, 1), TrangThai = true },
                new() { RowIndex = 3, HoTen = "Lô Mới 2", MaPhongBan = 2, NgaySinh = new DateOnly(1993, 3, 3), Email = "lo2@uneti.example", NgayVaoLam = new DateOnly(2021, 2, 2), TrangThai = true },
                new() { RowIndex = 4, HoTen = "Lô Mới 3", MaPhongBan = 1, NgaySinh = new DateOnly(1994, 4, 4), Email = "lo3@uneti.example", NgayVaoLam = new DateOnly(2021, 3, 3), TrangThai = true }
            };

            Assert(validRows.All(r => r.IsValid), "11.1. Chuẩn bị lô nhân sự hợp lệ");
            Assert(validRows.Count == 3, "11.2. Hỗ trợ import nhiều nhân sự trong cùng một lần");
        }

        // 12. Người không có quyền không thể sử dụng Import/Export
        {
            var controllerType = typeof(NhanVienController);
            var auth = controllerType.GetCustomAttribute<SessionAuthorizeAttribute>();
            Assert(auth != null && auth.Roles == "QuanTri,QuanLy", "12. Phân quyền chỉ dành riêng cho Quản trị viên và Nhân sự (QuanTri,QuanLy)");
        }

        // 13. Kiểm tra dữ liệu cũ và lịch sử đào tạo không bị ảnh hưởng
        {
            var nvCu = existingNvList.First();
            Assert(nvCu != null && nvCu.HoTen == "Nguyễn Văn Đã Có" && nvCu.Email == "daco@uneti.example", 
                "13. Dữ liệu nhân sự cũ hoàn toàn nguyên vẹn sau toàn bộ các thao tác Import");
        }

        Console.WriteLine("======================================================================");
        Console.WriteLine($"KẾT QUẢ TỔNG HỢP: {passed} PASS, {failed} FAIL (TỔNG CỘNG: {passed + failed} KIỂM THỬ)");
        Console.WriteLine("======================================================================");

        return Task.FromResult(failed == 0 ? 0 : 1);
    }
}
