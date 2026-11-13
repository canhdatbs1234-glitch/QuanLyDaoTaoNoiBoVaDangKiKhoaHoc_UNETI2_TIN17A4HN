# Module 2 – Các thay đổi (Trần Trung Hiếu)

## Yêu cầu 1: Tự lọc khi chọn dropdown (bỏ nút "Lọc")
- Trang Lớp đào tạo và Khóa đào tạo: chọn Lĩnh vực / Trạng thái / Sắp xếp / Ngày / Chỗ trống / Hạn đăng ký là danh sách tự lọc.
- Ô tìm kiếm vẫn dùng nút "Tìm kiếm" hoặc phím Enter. Mỗi lần đổi bộ lọc quay về trang 1, chuyển trang vẫn giữ điều kiện.
- File: `wwwroot/js/auto-filter.js` (mới), `Views/LopDaoTao/Index.cshtml`, `Views/KhoaDaoTao/Index.cshtml`.

## Yêu cầu 2: Xuất Excel / PDF
- Nút "Xuất Excel", "Xuất PDF" ở 2 trang; xuất toàn bộ kết quả theo đúng tìm kiếm/lọc/sắp xếp đang áp dụng (không Skip/Take).
- File: `Services/Module2ExportService.cs` (mới), `Controllers/LopDaoTaoController.cs`, `Controllers/KhoaDaoTaoController.cs`,
  `Models/ViewModels/*ListViewModel.cs` (thêm RouteForExport), `wwwroot/css/site.css`, `.csproj` (thêm ClosedXML, QuestPDF).

## Cách chạy
    dotnet restore
    dotnet run
Nếu restore báo không tìm thấy phiên bản package: `dotnet add package ClosedXML` và `dotnet add package QuestPDF`.

## Sửa lỗi logic: số chỗ còn bị âm (-1 / 15)
- Nguyên nhân: dữ liệu mẫu có Lớp 16 (sức chứa 15) nhưng có 16 đăng ký đã duyệt. Code duyệt đăng ký đã chặn đúng, lỗi nằm ở dữ liệu.
- Sửa: `QuanLyDaoTaoNoiBo_DB.sql` (Lớp 16 sức chứa 15 -> 20); `Services/LopDaoTaoRules.cs` thêm `SuaLopVuotSucChua`
  và gọi trong `Data/DbInitializer.cs` để tự sửa database đã có (lớp nào đã duyệt > tối đa thì nâng tối đa = số đã duyệt, chỗ còn = 0).
- Thêm: không cho tạo lớp (hoặc đổi sang) khóa đang ngừng hoạt động (`LopDaoTaoController.ValidateCourse`).
