//Họ và tên : Phí Bảo Nam - 23103100236
//Module 3: Quản lý nhân viên – Đăng ký – Duyệt đào tạo.
//Service xử lý Xuất file Excel, Tạo file Excel mẫu và Nhập dữ liệu nhân sự hàng loạt bằng ClosedXML.
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;

namespace QuanLyDaoTaoNoiBo.Services;

public static class NhanVienExcelService
{
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string VersionCode = "QUANLYNHANSU_NHANVIEN_V1";

    private const string NavyHex = "#0B2A66";
    private static readonly Regex PhoneRegex = new(@"^(\+84|0)[1-9]\d{8,9}$", RegexOptions.Compiled);

    public static readonly string[] RequiredColumns =
    [
        "Họ tên", "Mã phòng ban", "Ngày sinh", "Email", "Ngày vào làm", "Trạng thái"
    ];

    public static readonly string[] StandardHeaders =
    [
        "STT", "Họ tên", "Mã phòng ban", "Ngày sinh", "Số điện thoại", "Email", "Ngày vào làm", "Trạng thái"
    ];

    // =========================================================================
    // 1. XUẤT EXCEL DANH SÁCH NHÂN SỰ
    // =========================================================================
    public static byte[] ExportNhanVien(IReadOnlyList<NhanVien> nhanViens, string? dieuKien, string nguoiXuat)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("NhanVien");

        // Metadata phiên bản mẫu chuẩn
        ThemMetadata(wb);

        // Hàng 1: Tiêu đề cột chuẩn (giúp file export có thể import lại trực tiếp)
        for (var i = 0; i < StandardHeaders.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = StandardHeaders[i];
            cell.Style.Font.SetBold().Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml(NavyHex))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }
        ws.Row(1).Height = 26;

        // Điền dữ liệu từng nhân sự
        for (var r = 0; r < nhanViens.Count; r++)
        {
            var nv = nhanViens[r];
            var rowIdx = r + 2;

            // STT
            ws.Cell(rowIdx, 1).SetValue(r + 1);
            ws.Cell(rowIdx, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Họ tên
            ws.Cell(rowIdx, 2).SetValue(nv.HoTen);

            // Mã phòng ban
            ws.Cell(rowIdx, 3).SetValue(nv.MaPhongBan);
            ws.Cell(rowIdx, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Ngày sinh (dd/MM/yyyy)
            ws.Cell(rowIdx, 4).SetValue(nv.NgaySinh.ToString("dd/MM/yyyy"));
            ws.Cell(rowIdx, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Số điện thoại (dạng text để giữ nguyên số 0 đầu)
            var phoneCell = ws.Cell(rowIdx, 5);
            phoneCell.Style.NumberFormat.Format = "@";
            phoneCell.SetValue(nv.SoDienThoai ?? string.Empty);
            phoneCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Email
            ws.Cell(rowIdx, 6).SetValue(nv.Email);

            // Ngày vào làm (dd/MM/yyyy)
            ws.Cell(rowIdx, 7).SetValue(nv.NgayVaoLam.ToString("dd/MM/yyyy"));
            ws.Cell(rowIdx, 7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Trạng thái
            var statusCell = ws.Cell(rowIdx, 8);
            statusCell.SetValue(nv.TrangThai ? "Đang làm việc" : "Ngừng hoạt động");
            statusCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Viền ô
            ws.Range(rowIdx, 1, rowIdx, StandardHeaders.Length).Style.Border
                .SetInsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        ws.Columns().AdjustToContents();
        foreach (var col in ws.ColumnsUsed())
        {
            if (col.Width < 12) col.Width = 12;
            if (col.Width > 45) col.Width = 45;
        }

        // Đóng băng dòng tiêu đề
        ws.SheetView.FreezeRows(1);
        if (nhanViens.Count > 0)
        {
            ws.Range(1, 1, nhanViens.Count + 1, StandardHeaders.Length).SetAutoFilter();
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // =========================================================================
    // 2. TẠO FILE EXCEL MẪU CHUẨN
    // =========================================================================
    public static byte[] TaoFileMau(IReadOnlyList<PhongBan> phongBans)
    {
        using var wb = new XLWorkbook();
        ThemMetadata(wb);

        // --- Sheet 1: NhanVien (sheet nhập dữ liệu) ---
        var wsData = wb.Worksheets.Add("NhanVien");
        for (var i = 0; i < StandardHeaders.Length; i++)
        {
            var cell = wsData.Cell(1, i + 1);
            cell.Value = StandardHeaders[i];
            cell.Style.Font.SetBold().Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml(NavyHex))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }
        wsData.Row(1).Height = 26;

        // Một số dòng dữ liệu mẫu tham khảo
        var samplePb1 = phongBans.FirstOrDefault()?.MaPhongBan ?? 1;
        var samplePb2 = phongBans.Skip(1).FirstOrDefault()?.MaPhongBan ?? samplePb1;

        var sampleData = new[]
        {
            (STT: 1, HoTen: "Nguyễn Văn An", MaPB: samplePb1, NgaySinh: "15/05/1995", Sdt: "0912345678", Email: "nguyenvanan.uneti@gmail.com", NgayVaoLam: "01/08/2022", TrangThai: "Đang làm việc"),
            (STT: 2, HoTen: "Trần Thị Mai", MaPB: samplePb2, NgaySinh: "20/10/1998", Sdt: "0987654321", Email: "tranmai.uneti@gmail.com", NgayVaoLam: "15/01/2023", TrangThai: "Đang làm việc"),
            (STT: 3, HoTen: "Lê Hoàng Long", MaPB: samplePb1, NgaySinh: "08/03/1992", Sdt: "0905123456", Email: "lehoanglong.uneti@gmail.com", NgayVaoLam: "10/06/2021", TrangThai: "Ngừng hoạt động")
        };

        for (var i = 0; i < sampleData.Length; i++)
        {
            var r = i + 2;
            var item = sampleData[i];
            wsData.Cell(r, 1).SetValue(item.STT).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            wsData.Cell(r, 2).SetValue(item.HoTen);
            wsData.Cell(r, 3).SetValue(item.MaPB).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            wsData.Cell(r, 4).SetValue(item.NgaySinh).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var phoneCell = wsData.Cell(r, 5);
            phoneCell.Style.NumberFormat.Format = "@";
            phoneCell.SetValue(item.Sdt);
            phoneCell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            wsData.Cell(r, 6).SetValue(item.Email);
            wsData.Cell(r, 7).SetValue(item.NgayVaoLam).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            wsData.Cell(r, 8).SetValue(item.TrangThai).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            wsData.Range(r, 1, r, StandardHeaders.Length).Style.Border
                .SetInsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        wsData.Columns().AdjustToContents();
        foreach (var col in wsData.ColumnsUsed())
        {
            if (col.Width < 14) col.Width = 14;
            if (col.Width > 45) col.Width = 45;
        }
        wsData.SheetView.FreezeRows(1);

        // --- Sheet 2: DanhSachPhongBan (tra cứu mã phòng ban) ---
        var wsPb = wb.Worksheets.Add("DanhSachPhongBan");
        string[] pbHeaders = ["Mã phòng ban", "Tên phòng ban", "Trạng thái", "Ghi chú"];
        for (var i = 0; i < pbHeaders.Length; i++)
        {
            var cell = wsPb.Cell(1, i + 1);
            cell.Value = pbHeaders[i];
            cell.Style.Font.SetBold().Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml(NavyHex))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            cell.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }
        wsPb.Row(1).Height = 24;

        for (var i = 0; i < phongBans.Count; i++)
        {
            var r = i + 2;
            var pb = phongBans[i];
            wsPb.Cell(r, 1).SetValue(pb.MaPhongBan).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            wsPb.Cell(r, 2).SetValue(pb.TenPhongBan);
            wsPb.Cell(r, 3).SetValue(pb.TrangThai ? "Đang hoạt động" : "Ngừng hoạt động").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            wsPb.Cell(r, 4).SetValue(pb.TrangThai ? "Hợp lệ để nhập" : "Không thể gán nhân viên mới");
            wsPb.Range(r, 1, r, pbHeaders.Length).Style.Border
                .SetInsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }
        wsPb.Columns().AdjustToContents();

        // --- Sheet 3: HuongDan (hướng dẫn chi tiết) ---
        var wsHd = wb.Worksheets.Add("HuongDan");
        wsHd.Cell(1, 1).Value = "HƯỚNG DẪN NHẬP DANH SÁCH NHÂN SỰ TỪ FILE EXCEL";
        wsHd.Range(1, 1, 1, 4).Merge().Style.Font.SetBold().Font.SetFontSize(14).Font.SetFontColor(XLColor.FromHtml(NavyHex));

        string[] hdHeaders = ["Tên cột", "Bắt buộc", "Quy tắc / Định dạng dữ liệu", "Ví dụ"];
        for (var i = 0; i < hdHeaders.Length; i++)
        {
            var cell = wsHd.Cell(3, i + 1);
            cell.Value = hdHeaders[i];
            cell.Style.Font.SetBold().Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml(NavyHex))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        var hdRows = new[]
        {
            ("STT", "Không", "Số thứ tự, hỗ trợ theo dõi", "1, 2, 3..."),
            ("Họ tên", "Có", "Họ và tên nhân viên, tối đa 100 ký tự", "Nguyễn Văn An"),
            ("Mã phòng ban", "Có", "Số nguyên dương, phải tồn tại và đang hoạt động (tra cứu sheet DanhSachPhongBan)", "1, 2, 3..."),
            ("Ngày sinh", "Có", "Định dạng ngày dd/MM/yyyy. Không được ở tương lai", "15/05/1995"),
            ("Số điện thoại", "Không", "Gồm 10-11 chữ số, bắt đầu bằng 0 hoặc +84. Không được trùng trong hệ thống", "0912345678"),
            ("Email", "Có", "Email đúng định dạng, tối đa 254 ký tự. Không được trùng trong hệ thống", "an.nv@uneti.edu.vn"),
            ("Ngày vào làm", "Có", "Định dạng dd/MM/yyyy. Không sau ngày hiện tại và không trước ngày sinh", "01/08/2022"),
            ("Trạng thái", "Có", "Chấp nhận: 'Đang làm việc' hoặc 'Ngừng hoạt động' (hoặc 1 / 0)", "Đang làm việc")
        };

        for (var i = 0; i < hdRows.Length; i++)
        {
            var r = i + 4;
            var item = hdRows[i];
            wsHd.Cell(r, 1).SetValue(item.Item1).Style.Font.SetBold();
            wsHd.Cell(r, 2).SetValue(item.Item2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            wsHd.Cell(r, 3).SetValue(item.Item3);
            wsHd.Cell(r, 4).SetValue(item.Item4);
            wsHd.Range(r, 1, r, 4).Style.Border
                .SetInsideBorder(XLBorderStyleValues.Thin)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        }

        wsHd.Cell(13, 1).Value = "LƯU Ý QUAN TRỌNG:";
        wsHd.Cell(13, 1).Style.Font.SetBold().Font.SetFontColor(XLColor.Red);
        wsHd.Cell(14, 1).Value = "1. Không đổi tên các cột ở dòng 1 của sheet 'NhanVien'.";
        wsHd.Cell(15, 1).Value = "2. File xuất (Export) từ hệ thống có thể chỉnh sửa và import lại trực tiếp nếu đúng quy tắc.";
        wsHd.Cell(16, 1).Value = "3. Hệ thống chỉ nhập hồ sơ nhân sự, không tự tạo tài khoản người dùng đăng nhập.";
        wsHd.Cell(17, 1).Value = "4. Các dòng có lỗi dữ liệu sẽ được hệ thống chỉ rõ và cho phép bỏ qua để nhập các dòng hợp lệ.";

        wsHd.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static void ThemMetadata(XLWorkbook wb)
    {
        wb.CustomProperties.Add("AppTemplateCode", VersionCode);
        wb.CustomProperties.Add("CreatedAt", DateTime.UtcNow.ToString("O"));

        // Thêm sheet ẩn chứa metadata phiên bản
        var metaSheet = wb.Worksheets.Add("_Metadata");
        metaSheet.Cell(1, 1).Value = "TemplateVersion";
        metaSheet.Cell(1, 2).Value = VersionCode;
        metaSheet.Visibility = XLWorksheetVisibility.VeryHidden;
    }

    // =========================================================================
    // 3. ĐỌC VÀ KIỂM TRA (PREVIEW) IMPORT EXCEL
    // =========================================================================
    public static async Task<NhanVienImportPreviewResult> KiemTraVaXemTruocAsync(
        Stream stream, string fileName, ApplicationDbContext db)
    {
        var phongBans = await db.PhongBans.AsNoTracking().ToDictionaryAsync(pb => pb.MaPhongBan);
        var existingEmails = await db.NhanViens.AsNoTracking()
            .Select(nv => new { nv.Email, nv.HoTen, nv.MaNhanVien })
            .ToDictionaryAsync(x => x.Email.ToLower().Trim(), x => (x.HoTen, x.MaNhanVien));

        var existingPhones = await db.NhanViens.AsNoTracking()
            .Where(nv => !string.IsNullOrEmpty(nv.SoDienThoai))
            .Select(nv => new { SoDienThoai = nv.SoDienThoai!, nv.HoTen })
            .ToDictionaryAsync(x => ChuanHoaSoDienThoai(x.SoDienThoai), x => x.HoTen);

        return KiemTraVaXemTruocStream(stream, fileName, phongBans, existingEmails, existingPhones);
    }

    public static NhanVienImportPreviewResult KiemTraVaXemTruocStream(
        Stream stream, string fileName,
        Dictionary<int, PhongBan> phongBans,
        Dictionary<string, (string HoTen, int MaNhanVien)> existingEmails,
        Dictionary<string, string> existingPhones)
    {
        var result = new NhanVienImportPreviewResult { FileName = fileName };

        // 3.1. Kiểm tra định dạng cơ bản
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            result.IsStructureValid = false;
            result.StructureErrorMessage = "Hệ thống chỉ hỗ trợ file Excel định dạng .xlsx.";
            return result;
        }

        XLWorkbook wb;
        try
        {
            wb = new XLWorkbook(stream);
        }
        catch (Exception ex)
        {
            result.IsStructureValid = false;
            result.StructureErrorMessage = $"Không thể mở hoặc đọc nội dung file Excel. Chi tiết lỗi: {ex.Message}";
            return result;
        }

        using (wb)
        {
            // 3.2. Tìm sheet dữ liệu: ưu tiên sheet 'NhanVien', nếu không có thì lấy sheet đầu tiên
            IXLWorksheet? ws = wb.Worksheets.FirstOrDefault(s =>
                string.Equals(s.Name.Trim(), "NhanVien", StringComparison.OrdinalIgnoreCase));

            if (ws == null)
            {
                ws = wb.Worksheets.FirstOrDefault(s => s.Visibility == XLWorksheetVisibility.Visible);
            }

            if (ws == null)
            {
                result.IsStructureValid = false;
                result.StructureErrorMessage = "File Excel không chứa bất kỳ sheet dữ liệu hiển thị nào.";
                return result;
            }

            // 3.3. Xác định dòng tiêu đề (Header row)
            var (headerRowIndex, colMap) = TimDongTieuDe(ws);

            if (headerRowIndex == 0 || colMap.Count == 0)
            {
                result.IsStructureValid = false;
                result.StructureErrorMessage = "Không tìm thấy dòng tiêu đề hợp lệ trong file Excel. " +
                    "File cần có các cột: Họ tên, Mã phòng ban, Ngày sinh, Email, Ngày vào làm, Trạng thái.";
                return result;
            }

            // Kiểm tra các cột bắt buộc
            var thieuCot = new List<string>();
            foreach (var req in RequiredColumns)
            {
                if (!colMap.ContainsKey(ChuanHoaTenCot(req)))
                {
                    thieuCot.Add(req);
                }
            }

            if (thieuCot.Count > 0)
            {
                result.IsStructureValid = false;
                result.StructureErrorMessage = $"Cấu trúc file không đúng mẫu quy định. Thiếu các cột bắt buộc: {string.Join(", ", thieuCot)}.";
                return result;
            }

            var homNay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));

            // Tập hợp kiểm tra trùng lặp trong nội bộ file Excel
            var fileEmails = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var filePhones = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var lastRowUsed = ws.LastRowUsed()?.RowNumber() ?? headerRowIndex;

            // 3.5. Đọc và kiểm tra từng dòng dữ liệu
            for (var r = headerRowIndex + 1; r <= lastRowUsed; r++)
            {
                var row = ws.Row(r);
                if (row.IsEmpty()) continue;

                var dto = new NhanVienImportRowDto { RowIndex = r };

                // Đọc Họ tên
                dto.HoTen = LayGiaTriChuoi(row, colMap, "Họ tên");

                // Đọc Mã phòng ban
                var maPbStr = LayGiaTriChuoi(row, colMap, "Mã phòng ban");

                // Đọc Ngày sinh
                var ngaySinhRaw = LayGiaTriO(row, colMap, "Ngày sinh");

                // Đọc Số điện thoại
                dto.SoDienThoai = LayGiaTriChuoi(row, colMap, "Số điện thoại");
                if (string.IsNullOrWhiteSpace(dto.SoDienThoai)) dto.SoDienThoai = null;

                // Đọc Email
                dto.Email = LayGiaTriChuoi(row, colMap, "Email");

                // Đọc Ngày vào làm
                var ngayVaoLamRaw = LayGiaTriO(row, colMap, "Ngày vào làm");

                // Đọc Trạng thái
                var trangThaiStr = LayGiaTriChuoi(row, colMap, "Trạng thái");

                // Bỏ qua nếu dòng hoàn toàn trống ở tất cả các trường chính
                if (string.IsNullOrWhiteSpace(dto.HoTen) && string.IsNullOrWhiteSpace(dto.Email) &&
                    string.IsNullOrWhiteSpace(maPbStr) && ngaySinhRaw == null && ngayVaoLamRaw == null)
                {
                    continue;
                }

                // ---- KIỂM TRA TỪNG TRƯỜNG DỮ LIỆU ----

                // 1. Họ tên
                if (string.IsNullOrWhiteSpace(dto.HoTen))
                {
                    dto.Errors.Add("Họ tên không được để trống.");
                }
                else if (dto.HoTen.Length > 100)
                {
                    dto.Errors.Add("Họ tên vượt quá độ dài tối đa (100 ký tự).");
                }

                // 2. Mã phòng ban
                if (string.IsNullOrWhiteSpace(maPbStr) || !int.TryParse(maPbStr, out var maPb))
                {
                    dto.Errors.Add("Mã phòng ban phải là một số nguyên hợp lệ.");
                }
                else
                {
                    dto.MaPhongBan = maPb;
                    if (!phongBans.TryGetValue(maPb, out var pb))
                    {
                        dto.Errors.Add($"Mã phòng ban #{maPb} không tồn tại trong hệ thống.");
                    }
                    else
                    {
                        dto.TenPhongBan = pb.TenPhongBan;
                        if (!pb.TrangThai)
                        {
                            dto.Errors.Add($"Phòng ban '{pb.TenPhongBan}' (mã #{maPb}) hiện đã ngừng hoạt động.");
                        }
                    }
                }

                // 3. Email
                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    dto.Errors.Add("Email không được để trống.");
                }
                else
                {
                    dto.Email = dto.Email.Trim();
                    if (dto.Email.Length > 254)
                    {
                        dto.Errors.Add("Email vượt quá 254 ký tự.");
                    }
                    else if (!new EmailAddressAttribute().IsValid(dto.Email))
                    {
                        dto.Errors.Add("Email không đúng định dạng.");
                    }
                    else
                    {
                        var normEmail = dto.Email.ToLowerInvariant();

                        // Trùng trong cùng file
                        if (fileEmails.TryGetValue(normEmail, out var prevRow))
                        {
                            dto.Errors.Add($"Email trùng lặp với dòng {prevRow} trong cùng file.");
                        }
                        else
                        {
                            fileEmails[normEmail] = r;
                        }

                        // Trùng với DB
                        if (existingEmails.TryGetValue(normEmail, out var existingNv))
                        {
                            dto.Errors.Add($"Email đã tồn tại trong database (thuộc nhân viên: {existingNv.HoTen} - #{existingNv.MaNhanVien}).");
                        }
                    }
                }

                // 4. Số điện thoại
                if (!string.IsNullOrWhiteSpace(dto.SoDienThoai))
                {
                    var cleanPhone = ChuanHoaSoDienThoai(dto.SoDienThoai);
                    if (!PhoneRegex.IsMatch(cleanPhone))
                    {
                        dto.Errors.Add("Số điện thoại không đúng định dạng (cần 10-11 chữ số, bắt đầu bằng 0 hoặc +84).");
                    }
                    else
                    {
                        dto.SoDienThoai = cleanPhone;

                        // Trùng trong file
                        if (filePhones.TryGetValue(cleanPhone, out var prevPhoneRow))
                        {
                            dto.Errors.Add($"Số điện thoại trùng lặp với dòng {prevPhoneRow} trong cùng file.");
                        }
                        else
                        {
                            filePhones[cleanPhone] = r;
                        }

                        // Trùng DB
                        if (existingPhones.TryGetValue(cleanPhone, out var existingName))
                        {
                            dto.Errors.Add($"Số điện thoại đã tồn tại trong database (nhân viên: {existingName}).");
                        }
                    }
                }

                // 5. Ngày sinh
                var (ngaySinhOk, parsedNgaySinh) = ChuyenDoiNgay(ngaySinhRaw);
                if (!ngaySinhOk || parsedNgaySinh == default)
                {
                    dto.Errors.Add("Ngày sinh không hợp lệ (định dạng chuẩn: dd/MM/yyyy).");
                }
                else
                {
                    dto.NgaySinh = parsedNgaySinh;
                    if (dto.NgaySinh > homNay)
                    {
                        dto.Errors.Add("Ngày sinh không được ở tương lai.");
                    }
                }

                // 6. Ngày vào làm
                var (ngayVaoLamOk, parsedNgayVaoLam) = ChuyenDoiNgay(ngayVaoLamRaw);
                if (!ngayVaoLamOk || parsedNgayVaoLam == default)
                {
                    dto.Errors.Add("Ngày vào làm không hợp lệ (định dạng chuẩn: dd/MM/yyyy).");
                }
                else
                {
                    dto.NgayVaoLam = parsedNgayVaoLam;
                    if (dto.NgayVaoLam > homNay)
                    {
                        dto.Errors.Add("Ngày vào làm không được sau ngày hiện tại (giờ Việt Nam).");
                    }
                    if (ngaySinhOk && parsedNgaySinh != default && dto.NgayVaoLam < dto.NgaySinh)
                    {
                        dto.Errors.Add("Ngày vào làm không được trước ngày sinh.");
                    }
                }

                // 7. Trạng thái
                var (trangThaiOk, parsedTrangThai) = ChuyenDoiTrangThai(trangThaiStr);
                if (!trangThaiOk)
                {
                    dto.Errors.Add("Trạng thái không hợp lệ (nhập 'Đang làm việc' hoặc 'Ngừng hoạt động').");
                }
                else
                {
                    dto.TrangThai = parsedTrangThai;
                }

                result.Rows.Add(dto);
            }

            if (result.Rows.Count == 0)
            {
                result.IsStructureValid = false;
                result.StructureErrorMessage = "File Excel không chứa bất kỳ dòng dữ liệu nào để import.";
            }

            return result;
        }
    }

    // =========================================================================
    // 4. LƯU IMPORT VÀO CƠ SỞ DỮ LIỆU BẰNG TRANSACTION
    // =========================================================================
    public static async Task<(int SuccessCount, List<string> SkippedMessages)> LuuImportAsync(
        IReadOnlyList<NhanVienImportRowDto> validRows, ApplicationDbContext db)
    {
        var skipped = new List<string>();
        if (validRows.Count == 0) return (0, skipped);

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var homNay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var successCount = 0;

            // Lấy lại danh sách email và sđt từ DB để kiểm tra lần cuối trước khi thêm
            var dbEmails = await db.NhanViens.Select(nv => nv.Email.ToLower().Trim()).ToListAsync();
            var dbPhones = await db.NhanViens.Where(nv => nv.SoDienThoai != null)
                .Select(nv => nv.SoDienThoai!.Trim()).ToListAsync();

            var addedEmails = new HashSet<string>(dbEmails, StringComparer.OrdinalIgnoreCase);
            var addedPhones = new HashSet<string>(dbPhones, StringComparer.OrdinalIgnoreCase);

            // Kiểm tra các tài khoản chưa liên kết nếu có trùng email
            var taiKhoanDaDung = await db.NhanViens
                .Where(nv => nv.MaTaiKhoan.HasValue)
                .Select(nv => nv.MaTaiKhoan!.Value)
                .ToListAsync();

            var taiKhoansChuaDung = await db.TaiKhoans
                .Where(tk => !taiKhoanDaDung.Contains(tk.MaTaiKhoan))
                .ToListAsync();

            var taiKhoanByEmail = taiKhoansChuaDung
                .GroupBy(tk => tk.Email.ToLower().Trim())
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var r in validRows)
            {
                var normEmail = r.Email.Trim().ToLowerInvariant();
                if (addedEmails.Contains(normEmail))
                {
                    skipped.Add($"Dòng {r.RowIndex}: Bị bỏ qua do email '{r.Email}' đã tồn tại trong database.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(r.SoDienThoai))
                {
                    var cleanPhone = ChuanHoaSoDienThoai(r.SoDienThoai);
                    if (addedPhones.Contains(cleanPhone))
                    {
                        skipped.Add($"Dòng {r.RowIndex}: Bị bỏ qua do số điện thoại '{cleanPhone}' đã tồn tại trong database.");
                        continue;
                    }
                    addedPhones.Add(cleanPhone);
                }

                // Nếu có tài khoản người dùng trùng email đang chờ liên kết thì tự động liên kết
                int? maTaiKhoan = null;
                if (taiKhoanByEmail.TryGetValue(normEmail, out var tkMatch))
                {
                    maTaiKhoan = tkMatch.MaTaiKhoan;
                    taiKhoanByEmail.Remove(normEmail); // Đã gán
                }

                var entity = new NhanVien
                {
                    HoTen = r.HoTen.Trim(),
                    MaPhongBan = r.MaPhongBan,
                    NgaySinh = r.NgaySinh,
                    SoDienThoai = string.IsNullOrWhiteSpace(r.SoDienThoai) ? null : r.SoDienThoai.Trim(),
                    Email = r.Email.Trim(),
                    NgayVaoLam = r.NgayVaoLam,
                    TrangThai = r.TrangThai,
                    MaTaiKhoan = maTaiKhoan
                };

                db.NhanViens.Add(entity);
                addedEmails.Add(normEmail);
                successCount++;
            }

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return (successCount, skipped);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // =========================================================================
    // HÀM TIỆN ÍCH HỖ TRỢ
    // =========================================================================
    private static (int HeaderRowIndex, Dictionary<string, int> ColMap) TimDongTieuDe(IXLWorksheet ws)
    {
        var maxScan = Math.Min(15, ws.LastRowUsed()?.RowNumber() ?? 1);
        for (var r = 1; r <= maxScan; r++)
        {
            var row = ws.Row(r);
            var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var c = 1; c <= 25; c++)
            {
                var txt = row.Cell(c).GetString().Trim();
                if (!string.IsNullOrEmpty(txt))
                {
                    var norm = ChuanHoaTenCot(txt);
                    if (!colMap.ContainsKey(norm))
                    {
                        colMap[norm] = c;
                    }
                }
            }

            // Kiểm tra xem dòng này có chứa ít nhất 3 cột chuẩn không
            var matchCount = RequiredColumns.Count(req => colMap.ContainsKey(ChuanHoaTenCot(req)));
            if (matchCount >= 3)
            {
                return (r, colMap);
            }
        }

        return (0, []);
    }

    public static string ChuanHoaTenCot(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var s = raw.Trim().ToLowerInvariant();
        // Loại bỏ khoảng trắng thừa và dấu ngoặc nếu có
        s = Regex.Replace(s, @"\s+", " ");
        s = s.Replace("(*)", "").Replace("*", "").Trim();
        return s;
    }

    private static string LayGiaTriChuoi(IXLRow row, Dictionary<string, int> colMap, string colName)
    {
        var norm = ChuanHoaTenCot(colName);
        if (colMap.TryGetValue(norm, out var colIdx))
        {
            return row.Cell(colIdx).GetString().Trim();
        }
        return string.Empty;
    }

    private static object? LayGiaTriO(IXLRow row, Dictionary<string, int> colMap, string colName)
    {
        var norm = ChuanHoaTenCot(colName);
        if (colMap.TryGetValue(norm, out var colIdx))
        {
            var cell = row.Cell(colIdx);
            if (cell.IsEmpty()) return null;
            if (cell.DataType == XLDataType.DateTime)
            {
                return cell.GetDateTime();
            }
            return cell.GetString().Trim();
        }
        return null;
    }

    private static string ChuanHoaSoDienThoai(string phone)
    {
        return Regex.Replace(phone.Trim(), @"[\s\.\-\(\)]", "");
    }

    public static (bool Ok, DateOnly Result) ChuyenDoiNgay(object? val)
    {
        if (val == null) return (false, default);

        if (val is DateTime dt)
        {
            return (true, DateOnly.FromDateTime(dt));
        }

        var str = val.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(str)) return (false, default);

        // Các định dạng ngày thông dụng
        string[] formats =
        [
            "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy",
            "yyyy-MM-dd", "yyyy/MM/dd", "MM/dd/yyyy", "M/d/yyyy"
        ];

        if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
        {
            return (true, DateOnly.FromDateTime(parsed));
        }

        if (DateTime.TryParse(str, new CultureInfo("vi-VN"), DateTimeStyles.None, out parsed))
        {
            return (true, DateOnly.FromDateTime(parsed));
        }

        return (false, default);
    }

    public static (bool Ok, bool Result) ChuyenDoiTrangThai(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) return (false, false);
        var s = str.Trim().ToLowerInvariant();

        if (s is "đang làm việc" or "dang lam viec" or "hoạt động" or "hoat dong" or "1" or "true" or "active")
        {
            return (true, true);
        }

        if (s is "ngừng hoạt động" or "ngung hoat dong" or "nghỉ việc" or "nghi viec" or "0" or "false" or "inactive")
        {
            return (true, false);
        }

        return (false, false);
    }
}
