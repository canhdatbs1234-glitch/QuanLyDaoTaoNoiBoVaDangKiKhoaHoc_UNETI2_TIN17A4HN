//Họ và tên: Trần Trung Hiếu
//Mã sinh viên: 23103100217
//Module 2 - Chức năng nâng cao: Xuất Excel / PDF danh sách lớp và khóa đào tạo.

using ClosedXML.Excel;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace QuanLyDaoTaoNoiBo.Services;

public static class Module2ExportService
{
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string PdfContentType = "application/pdf";

    private const string NavyHex = "#0B2A66";
    private const string FontFamily = "Arial"; // Arial có đủ dấu tiếng Việt trên Windows.

    static Module2ExportService() => QuestPDF.Settings.License = LicenseType.Community;

    private static string Ngay(DateOnly d) => d.ToString("dd/MM/yyyy");

    // ===================== LỚP ĐÀO TẠO =====================
    private static readonly string[] LopHeaders =
        ["STT", "Tên lớp", "Khóa đào tạo", "Lĩnh vực", "Ngày bắt đầu", "Ngày kết thúc", "Hạn đăng ký",
         "Địa điểm", "Giảng viên", "Trạng thái", "Số lượng tối đa", "Đã duyệt", "Số chỗ còn"];

    private static string[] LopRow(int stt, LopDaoTaoViewModel x) =>
    [
        stt.ToString(), x.TenLop, x.TenKhoaDaoTao, x.TenLinhVuc, Ngay(x.NgayBatDau), Ngay(x.NgayKetThuc),
        x.HanDangKy.ToString("dd/MM/yyyy HH:mm"), x.DiaDiem,
        string.IsNullOrEmpty(x.TenGiangVien) ? "Chưa phân công" : x.TenGiangVien, x.TenTrangThai,
        x.SoLuongToiDa.ToString(), x.SoDaDuyet.ToString(), x.SoChoCon.ToString()
    ];

    public static byte[] LopExcel(IReadOnlyList<LopDaoTaoViewModel> rows, string dieuKien, string nguoiXuat) =>
        ToExcel("Lớp đào tạo", "DANH SÁCH LỚP ĐÀO TẠO", dieuKien, nguoiXuat, LopHeaders,
            rows.Select((x, i) => LopRow(i + 1, x)).ToList(), numericCols: [0, 10, 11, 12]);

    public static byte[] LopPdf(IReadOnlyList<LopDaoTaoViewModel> rows, string dieuKien, string nguoiXuat) =>
        ToPdf("DANH SÁCH LỚP ĐÀO TẠO", dieuKien, nguoiXuat, LopHeaders,
            rows.Select((x, i) => LopRow(i + 1, x)).ToList(),
            widths: [26, 110, 100, 60, 52, 52, 62, 70, 66, 56, 40, 40, 40]);

    // ===================== KHÓA ĐÀO TẠO =====================
    private static readonly string[] KhoaHeaders =
        ["STT", "Tên khóa đào tạo", "Lĩnh vực", "Số giờ dự kiến", "Điểm đạt tối thiểu", "Tham dự tối thiểu (%)", "Trạng thái"];

    private static string[] KhoaRow(int stt, KhoaDaoTao x) =>
    [
        stt.ToString(), x.TenKhoaDaoTao, x.LinhVucDaoTao.TenLinhVuc, x.SoGioDuKien.ToString("0.##"),
        x.DiemDatToiThieu.ToString("0.##"), x.TyLeThamDuToiThieu.ToString("0.##"),
        x.TrangThai ? "Hoạt động" : "Ngừng hoạt động"
    ];

    public static byte[] KhoaExcel(IReadOnlyList<KhoaDaoTao> rows, string dieuKien, string nguoiXuat) =>
        ToExcel("Khóa đào tạo", "DANH SÁCH KHÓA ĐÀO TẠO", dieuKien, nguoiXuat, KhoaHeaders,
            rows.Select((x, i) => KhoaRow(i + 1, x)).ToList(), numericCols: [0, 3, 4, 5]);

    public static byte[] KhoaPdf(IReadOnlyList<KhoaDaoTao> rows, string dieuKien, string nguoiXuat) =>
        ToPdf("DANH SÁCH KHÓA ĐÀO TẠO", dieuKien, nguoiXuat, KhoaHeaders,
            rows.Select((x, i) => KhoaRow(i + 1, x)).ToList(),
            widths: [30, 200, 110, 70, 70, 80, 90]);

    // ===================== DÙNG CHUNG =====================
    private static string DongNguoiXuat(string nguoiXuat, int tong) =>
        $"Ngày xuất: {LopDaoTaoRules.BayGioVietNam():dd/MM/yyyy HH:mm} · Người xuất: {nguoiXuat} · Tổng: {tong} dòng";

    private static byte[] ToExcel(string sheetName, string title, string dieuKien, string nguoiXuat,
        string[] headers, List<string[]> rows, int[] numericCols)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);
        var lastCol = headers.Length;

        ws.Cell(1, 1).Value = title;
        ws.Range(1, 1, 1, lastCol).Merge().Style.Font.SetBold().Font.SetFontSize(16)
            .Font.SetFontColor(XLColor.FromHtml(NavyHex));
        ws.Cell(2, 1).Value = "Điều kiện: " + dieuKien;
        ws.Range(2, 1, 2, lastCol).Merge();
        ws.Cell(3, 1).Value = DongNguoiXuat(nguoiXuat, rows.Count);
        ws.Range(3, 1, 3, lastCol).Merge();
        ws.Range(2, 1, 3, 1).Style.Font.SetItalic().Font.SetFontColor(XLColor.Gray);

        const int headerRow = 5;
        for (var c = 0; c < lastCol; c++) ws.Cell(headerRow, c + 1).Value = headers[c];
        var head = ws.Range(headerRow, 1, headerRow, lastCol);
        head.Style.Font.SetBold().Font.SetFontColor(XLColor.White).Fill.SetBackgroundColor(XLColor.FromHtml(NavyHex))
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center).Alignment.SetWrapText();

        for (var r = 0; r < rows.Count; r++)
            for (var c = 0; c < lastCol; c++)
            {
                var cell = ws.Cell(headerRow + 1 + r, c + 1);
                var text = rows[r][c];
                if (numericCols.Contains(c) && decimal.TryParse(text, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.CurrentCulture, out var number))
                {
                    cell.Value = number;
                    cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                }
                else cell.Value = text;
            }

        var body = ws.Range(headerRow, 1, headerRow + Math.Max(rows.Count, 1), lastCol);
        body.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin).Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        ws.Columns().AdjustToContents(headerRow, headerRow + rows.Count);
        foreach (var col in ws.ColumnsUsed()) if (col.Width > 45) col.Width = 45;
        ws.SheetView.FreezeRows(headerRow);
        ws.Range(headerRow, 1, headerRow + rows.Count, lastCol).SetAutoFilter();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static byte[] ToPdf(string title, string dieuKien, string nguoiXuat,
        string[] headers, List<string[]> rows, float[] widths)
    {
        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(24);
            page.DefaultTextStyle(t => t.FontFamily(FontFamily).FontSize(8));

            page.Header().PaddingBottom(8).Column(col =>
            {
                col.Item().Text(title).FontSize(15).Bold().FontColor(NavyHex);
                col.Item().Text("Điều kiện: " + dieuKien).FontSize(8).FontColor(Colors.Grey.Darken1);
                col.Item().Text(DongNguoiXuat(nguoiXuat, rows.Count)).FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(cols => { foreach (var w in widths) cols.RelativeColumn(w); });
                table.Header(h =>
                {
                    foreach (var text in headers)
                        h.Cell().Background(NavyHex).Padding(3).AlignMiddle()
                            .Text(text).Bold().FontColor(Colors.White);
                });
                foreach (var row in rows)
                    foreach (var text in row)
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(text);
            });

            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("Trang ").FontSize(8); t.CurrentPageNumber().FontSize(8);
                t.Span(" / ").FontSize(8); t.TotalPages().FontSize(8);
            });
        })).GeneratePdf();
    }
}
