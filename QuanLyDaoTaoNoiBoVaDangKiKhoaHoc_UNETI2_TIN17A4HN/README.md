# Hệ thống Quản lý Đào tạo Nội bộ và Đăng ký Khóa học UNETI

Ứng dụng ASP.NET Core 10 dùng Entity Framework Core SQL Server 10.0.8, gồm 10 Entity, `ApplicationDbContext`, seed mẫu, portal UNETI và đầy đủ Modules 1–5: đăng nhập, quản lý đào tạo, đăng ký, điểm danh, đánh giá và thống kê. Đường dẫn `/` mở trang đăng nhập.

## Chạy ứngdụng

`appsettings.json` đã cấu hình `DefaultConnection`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QuanLyDaoTaoNoiBo_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

`Server=.` yêu cầu SQL Server default instance trên máy local đang chạy và tài khoản Windows có quyền kết nối/tạo database. Đây không phải địa chỉ LocalDB hay named instance như `SQLEXPRESS`. `TrustServerCertificate=True` dùng theo yêu cầu cho môi trường local.

```powershell
dotnet build
dotnet run
```

`Program.cs` đăng ký DbContext, `AddControllersWithViews()`, `AddDistributedMemoryCache()` và `AddSession()`. Middleware `UseSession()` đặt sau routing và trước endpoint MVC. Session lưu trong bộ nhớ của tiến trình, hết hạn sau 30 phút không hoạt động và mất khi khởi động lại. Cấu hình này theo [hướng dẫn Session của Microsoft](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-10.0).

Ứng dụng tạo DI scope và gọi `DbInitializer.Initialize(context)` trước khi nhận request. Initializer gọi `Database.Migrate()` rồi seed; lỗi kết nối, migration hoặc seed sẽ dừng khởi động. `appsettings.json` giữ `Server=.` theo yêu cầu. Trên máy này, `appsettings.Development.json` dùng `Server=.\\SQLEXPRESS`; `Properties/launchSettings.json` chọn Development và cổng 5187 cho `dotnet run`.

Migration đầu tiên: `20261008143049_InitialCreate`. Gói `Microsoft.EntityFrameworkCore.Design` cùng phiên bản 10.0.8 với provider. `ApplicationDbContextFactory` tạo context cho CLI mà không chạy web/seed; mặc định Development, hỗ trợ biến môi trường và tham số ghi đè cấu hình.

```powershell
dotnet ef database update
dotnet run
```

Mở http://127.0.0.1:5187/. `InitialCreate` đã được tạo nên không chạy lại `migrations add InitialCreate`; khi thay đổi model, tạo migration tên mới. Nếu triển khai Production, cấu hình instance SQL Server phù hợp vì `Server=.` chỉ hoạt động khi có default instance.

## Module 1: đăng nhập, Session, phân quyền

- `TaiKhoanController.DangNhap` GET hiển thị form; POST dùng LINQ tìm tài khoản và `PasswordHasher<TaiKhoan>.VerifyHashedPassword` xác thực hash. Tài khoản bị khóa, vai trò không hợp lệ hoặc sai mật khẩu đều bị từ chối.
- Đăng nhập thành công lưu `MaTaiKhoan` (int), `HoTen`, `VaiTro` (string) trong Session. `Home/Index` chuyển đến `Home/QuanTri`, `Home/QuanLy` hoặc `Home/NhanVien` theo vai trò.
- `SessionAuthorizationFilter` áp dụng toàn cục cho MVC: mặc định yêu cầu đăng nhập, ngoại trừ action có `[AllowAnonymous]`. Mỗi request được bảo vệ kiểm tra lại tài khoản hoạt động và quyền hiện tại từ database.
- Dùng `[SessionRoles("QuanTri", "QuanLy")]` cho action quản lý; tài khoản đã đăng nhập nhưng sai quyền nhận HTTP 403. Session hết hạn chuyển về trang đăng nhập. Không dùng riêng `[Authorize]` để thay thế bộ lọc này vì cơ chế hiện tại là xác thực bằng Session, chưa đăng ký authentication scheme.
- Trang quản trị chỉ dành cho `QuanTri`, trang quản lý chỉ dành cho `QuanLy`, trang nhân viên chỉ dành cho `NhanVien`. Quản trị/Quản lý được chuyển tới Dashboard thống kê và có quyền quản lý Khóa/Lớp, Nhân viên, Duyệt đăng ký, Buổi học, Điểm danh và Đánh giá kết quả. Nhân viên có đăng ký, lịch sử đào tạo, lịch học và báo cáo tham dự cá nhân.
- Đăng xuất là POST có anti-forgery token và xóa Session. Form đăng nhập cũng kiểm tra token, validate phía server và không điền lại mật khẩu khi có lỗi. Tham khảo [anti-forgery trong ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery).
- Layout navy `#002B66` có header, vai trò, menu theo quyền và footer; CSS responsive không phụ thuộc CDN. `wwwroot/images/uneti-mark.svg` là biểu trưng chữ UNETI minh họa cho giao diện, không phải file logo chính thức của trường; có thể thay bằng logo được trường cung cấp.

Kiểm tra thủ công: truy cập `/Home/QuanTri` khi chưa đăng nhập phải chuyển về đăng nhập; đăng nhập `nhanvien` rồi truy cập URL đó phải nhận 403; đăng nhập `admin` phải vào Dashboard quản trị; sau khi đăng xuất không được truy cập lại trang được bảo vệ. Các nút menu bị ẩn không thay thế kiểm tra quyền phía server.

## Module 2: quản lý khóa và lớp đào tạo

- Đường dẫn: `/KhoaDaoTao/Index`, `/LopDaoTao/Index`. Đăng nhập bằng tài khoản Admin để thấy hai menu tương ứng. Dashboard cũ cũng dẫn tới danh sách lớp.
- Mỗi controller có Index, Create, Edit, Details và Delete (POST). Xác nhận xóa tại trang Details. Không xóa khóa đã có lớp hoặc lớp đã có đăng ký/buổi học; chuyển trạng thái nếu cần giữ lịch sử.
- Form dùng ViewModel riêng, không nhận navigation/entity nguyên khối từ client. Server kiểm tra khoảng số, ngày tháng, khóa ngoại tồn tại; sửa lớp không được giảm sức chứa thấp hơn số đăng ký đã duyệt. POST được bảo vệ bằng anti-forgery và phân quyền Session.
- `LopDaoTaoViewModel.SoChoCon = SoLuongToiDa - SoDaDuyet`. Database hiện dùng chuỗi `DaDuyet`, hiển thị “Đã duyệt”; không truy vấn chuỗi có dấu vì CHECK constraint hiện chỉ cho phép mã không dấu. Không trừ đăng ký chờ duyệt/từ chối/đã hủy.
- Tìm kiếm tên khóa, tên lớp hoặc địa điểm; lọc lĩnh vực, trạng thái lớp, còn chỗ (> 0) hoặc hết chỗ (<= 0). Tìm kiếm chịu quy tắc phân biệt hoa/thường/dấu của collation SQL Server hiện tại.
- Sắp xếp tên lớp A–Z/Z–A, ngày bắt đầu tăng/giảm, số chỗ còn tăng/giảm. Thêm khóa chính vào thứ tự để không trùng/mất hàng giữa các trang khi giá trị sắp xếp bằng nhau; xem [hướng dẫn phân trang EF Core](https://learn.microsoft.com/en-us/ef/core/querying/pagination).
- Truy vấn dùng LINQ trên SQL Server, Count rồi Skip/Take, 10 lớp/trang; chỉ lấy các trường cần thiết và số đăng ký đã duyệt, không tải toàn bộ đăng ký vào bộ nhớ. Bộ lọc GET giữ nguyên trên các link trang. Khi đổi bộ lọc sẽ về trang 1; số trang vượt giới hạn được đưa về trang hợp lệ.
- Còn chỗ chỉ biểu thị sức chứa, không tự khẳng định lớp còn hạn hoặc đang mở đăng ký. Module 3 kiểm tra thêm các điều kiện đó và kiểm soát đồng thời khi duyệt.
- Không thay đổi schema hoặc connection string cho Module 2. Nếu máy dùng SQLEXPRESS, có thể chạy với cấu hình tạm:

```powershell
dotnet run -- --ConnectionStrings:DefaultConnection 'Server=.\SQLEXPRESS;Database=QuanLyDaoTaoNoiBo_DB;Trusted_Connection=True;TrustServerCertificate=True;'
```

Đã kiểm tra build, tám trang MVC, quyền nhân viên (403), POST thiếu token (400), dữ liệu đầu vào không hợp lệ và ID không tồn tại (404). Kiểm thử SQL Server dùng database riêng với 13 lớp bổ sung để kiểm tra phân trang, sáu cách sắp xếp, tìm kiếm/lọc, số chỗ, CRUD và chặn xóa dữ liệu đang tham chiếu; database kiểm thử được xóa sau khi chạy.

## Module 3: nhân viên, đăng ký và duyệt đào tạo

Các đường dẫn:

- Admin/HR: `/NhanVien/Index`, `/DangKyDaoTao/DuyetDangKy`.
- Nhân viên: `/DangKyDaoTao/Index`, `/DangKyDaoTao/LichSuDangKy`.

`QuanLy` là vai trò HR trong module này, dùng chung hệ thống phân quyền hiện có; không thêm mã vai trò `HR` vào schema. Các mã trạng thái database vẫn là `MoDangKy` (Đang nhận đăng ký), `ChoDuyet`, `DaDuyet`, `TuChoi`, `DaHuy`.

**Nhân viên:** có Index, Create, Edit, Details và Delete POST tại trang Details. Form chọn tài khoản đã được cấp nhưng chưa liên kết với nhân viên khác, chọn phòng ban có thật; không tạo tài khoản/mật khẩu mới trong form hồ sơ. Ngày vào làm phải không sau hôm nay theo giờ Việt Nam và không trước ngày sinh. Họ tên/email hồ sơ nhân viên được lưu riêng với tài khoản. Xóa hồ sơ không xóa tài khoản; hồ sơ có đăng ký/điểm danh chỉ được chuyển sang ngừng hoạt động.

**Session và quyền sở hữu:** đăng nhập lưu thêm `MaNhanVien`. Bộ lọc toàn cục làm mới mã này từ `MaTaiKhoan` mỗi request. Service kiểm tra lại nhân viên thuộc đúng tài khoản và đang hoạt động; form đăng ký/hủy không nhận mã nhân viên làm căn cứ xác thực. Lịch sử chỉ truy vấn nhân viên hiện tại.

**Đăng ký:** nhân viên chọn lớp đang nhận đăng ký, còn hạn, chưa tới ngày bắt đầu, khóa đang hoạt động và còn chỗ. Chỗ chỉ bị trừ bởi đăng ký đã duyệt; chờ duyệt chưa giữ chỗ. Một nhân viên chỉ có một bản ghi cho mỗi lớp, kể cả đã hủy/từ chối; không đăng ký lại theo yêu cầu Module 3.

**Trùng lịch:** LINQ kiểm tra các đăng ký đã duyệt ở lớp khác chưa bị hủy:

```text
LopA.NgayBatDau <= LopB.NgayKetThuc
&& LopB.NgayBatDau <= LopA.NgayKetThuc
```

Khoảng ngày bao gồm cả hai đầu; chung ngày đầu/cuối cũng trùng. Thuật toán ở cấp lớp, không so khung giờ từng buổi học. Cả lúc đăng ký và duyệt đều chạy cùng phép kiểm tra.

**Duyệt/từ chối:** Admin/HR chỉ xử lý bản ghi Chờ duyệt. Duyệt kiểm tra lại sức chứa, lịch, trạng thái nhân viên/tài khoản/khóa và lớp chưa bắt đầu. Có thể duyệt sau hạn đăng ký, khi lớp đã đóng nhận đăng ký nhưng chưa bắt đầu. Từ chối bắt buộc có lý do. Cả hai lưu `NgayDuyet`, lý do lưu vào `GhiChu`.

**Hủy:** chỉ chủ đăng ký được hủy bản ghi Chờ duyệt/Đã duyệt trước ngày bắt đầu; không hủy từ ngày bắt đầu trở đi. Bắt buộc nhập lý do (tối đa 1000 ký tự). Giữ ngày duyệt cũ nếu đã từng duyệt; trạng thái Đã hủy không chiếm chỗ và không gây trùng lịch.

`DangKyDaoTaoService` dùng transaction SQL Server `Serializable` bao trùm đọc điều kiện và ghi kết quả; lỗi cạnh tranh/deadlock được chuyển thành thông báo tải lại và thử lại. Không dùng khóa trong bộ nhớ tiến trình. Tham khảo [transactions của EF Core](https://learn.microsoft.com/en-us/ef/core/saving/transactions) và [kiểm soát đồng thời](https://learn.microsoft.com/en-us/ef/core/saving/concurrency). Không cần migration mới.

Đã kiểm thử 33 tình huống trên database SQL Server riêng: CRUD nhân viên, tài khoản trùng liên kết, ngày vào làm tương lai, đăng ký trùng, hết chỗ/hạn, trùng lịch kể cả biên ngày, từ chối/hủy, giả mạo chủ sở hữu, duyệt đồng thời chỗ cuối và duyệt đồng thời hai lớp trùng lịch. Các kiểm thử HTTP xác nhận trang MVC, phân quyền 403, Session, validation và anti-forgery 400. Database kiểm thử đã được xóa; không thêm đăng ký thử vào database ứng dụng.

## Module 4: buổi học, lịch học và điểm danh

- Quản lý: `/BuoiHoc/Index`, chọn lớp rồi thêm/sửa/xóa buổi hoặc mở Điểm danh.
- Lịch học: `/BuoiHoc/LichHoc`; nhân viên chỉ thấy các lớp đã được duyệt của tài khoản hiện tại. Đổi tham số lớp sang lớp không thuộc mình sẽ nhận 403.
- Điểm danh: `/DiemDanh/DiemDanhBuoiHoc/{id}`.
- Báo cáo: `/DiemDanh/ThongKeThamGia?maLop={id}`; nhân viên chỉ nhận hàng thống kê và chi tiết của chính mình.

**Quyền:** schema hiện có `QuanTri`, `QuanLy`, `NhanVien`, chưa có vai trò hay quan hệ phân công giảng viên riêng. Module dùng `QuanTri`/`QuanLy` cho Admin/HR/người phụ trách điểm danh. Không gán quyền ghi điểm danh cho tài khoản `NhanVien`. Nếu cần giảng viên độc lập chỉ phụ trách lớp được giao, phải bổ sung vai trò, phân công và migration tương ứng.

**Buổi học:** ngày học nằm trong khoảng ngày lớp, giờ kết thúc sau giờ bắt đầu trong cùng ngày. Hai buổi của cùng lớp không được trùng giờ (hai buổi liền nhau được phép). Không chỉnh lịch lớp đã hủy; buổi đã có điểm danh không được đổi lớp/ngày/giờ hoặc xóa vật lý. Có thể chuyển buổi sang Đã hủy để loại khỏi báo cáo. Module 2 cũng kiểm tra khoảng ngày lớp khi sửa để không bỏ các buổi đã tạo ra ngoài lịch lớp.

**Điểm danh hàng loạt:** form chỉ tải học viên có đăng ký `DaDuyet`; chưa có bản ghi thì bắt buộc chọn trạng thái, không mặc định vắng/có mặt. Có thể điểm danh từ khi buổi bắt đầu; buổi tương lai, buổi hoặc lớp bị hủy không được điểm danh. POST tải lại danh sách hợp lệ từ database và đối chiếu đầy đủ tập mã nhân viên, từ chối danh sách thiếu, trùng hoặc chèn học viên khác. Trạng thái lưu:

| Hiển thị | Mã database |
| --- | --- |
| Có mặt | CoMat |
| Vắng có phép | CoPhep |
| Vắng không phép | VangMat |

Service dùng transaction Serializable, cập nhật bản ghi hiện có hoặc thêm nếu chưa có. Chỉ mục duy nhất `(MaBuoiHoc, MaNhanVien)` đã có từ Module 1 giữ ràng buộc ở SQL Server. Thời gian cập nhật tự gán theo giờ Việt Nam, cũng dùng để phát hiện form đã cũ; hai lần lưu đồng thời không ghi đè âm thầm. Cả lô được kiểm tra trước khi ghi. Form dùng chỉ số `HocViens[i]` theo [model binding của ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-10.0).

**Tỷ lệ tham dự:** `TongSoBuoi` là toàn bộ buổi của lớp; `TongSoBuoiHopLe` chỉ gồm buổi đã kết thúc theo ngày/giờ thực tế, buổi và lớp không bị hủy (không phụ thuộc việc người quản lý đã đổi cờ trạng thái hay chưa). Buổi đang học chưa vào mẫu số dù đã điểm danh. Công thức dùng decimal, làm tròn 2 chữ số:

```text
TyLeThamDu = TongSoBuoiHopLe == 0 ? 0 : SoBuoiCoMat * 100 / TongSoBuoiHopLe
```

Chưa điểm danh vẫn thuộc mẫu số nếu buổi hợp lệ, nhưng ghi riêng, không tự gán là vắng. Tổng vắng = vắng có phép + vắng không phép. Báo cáo có chi tiết từng buổi, ghi chú và thời gian cập nhật. Mã `DiMuon` cũ được hiển thị riêng, không tự quy đổi thành có mặt; người phụ trách cần xác nhận lại một trong ba trạng thái trên.

Không thay đổi schema và không cần migration. Kiểm thử dùng database SQL Server riêng, gồm ràng buộc lịch, roster đã duyệt, chống giả mạo/trùng/thiếu học viên, lưu lại và lưu đồng thời, form cũ, tỷ lệ phần trăm/mẫu số 0, buổi đang học/tương lai/đã hủy, quyền riêng tư, form MVC với thời gian cập nhật và anti-forgery. Database và tiến trình kiểm thử được dọn khi kết thúc; không thêm buổi hay điểm danh thử vào database ứng dụng.

## Dữ liệu mẫu và khởi tạo database

Trên database mới, initializer tạo:

- 4 phòng ban: Công nghệ thông tin, Nhân sự, Kế toán, Đào tạo.
- 4 lĩnh vực: Công nghệ thông tin, Kỹ năng mềm, Quản lý và lãnh đạo, Ngoại ngữ.
- 2 tài khoản và 2 nhân viên tương ứng, liên kết với phòng Công nghệ thông tin và Nhân sự.
- 2 khóa: Lập trình ASP.NET Core MVC; Kỹ năng giao tiếp và làm việc nhóm.
- 2 lớp tương ứng, bắt đầu sau ngày seed lần đầu 14 và 21 ngày theo giờ Việt Nam; hạn đăng ký là 23:59:59 ngày liền trước ngày bắt đầu.

| Tên đăng nhập | Mật khẩu mẫu | Vai trò lưu trong database |
| --- | --- | --- |
| admin | Admin@123 | QuanTri (Admin) |
| nhanvien | NhanVien@123 | NhanVien |

Mật khẩu mẫu phục vụ BTL local, được băm bằng `PasswordHasher<TaiKhoan>` trước khi lưu. Khi xây dựng đăng nhập, dùng `VerifyHashedPassword` để kiểm tra. Email mẫu dùng miền `uneti.example`.

Seed kiểm tra từng bản ghi theo tên/tài khoản/liên kết thay vì bỏ qua toàn bộ khi một bảng có dữ liệu. Các lần chạy lại không ghi đè mật khẩu, thông tin hay lịch lớp đã có. Toàn bộ thao tác thêm dữ liệu mẫu nằm trong một transaction; nếu một bước lỗi, dữ liệu seed trong lần chạy đó được rollback. Tạo schema nằm ngoài transaction seed. Nếu đổi các tên dùng để nhận diện dữ liệu mẫu, lần chạy sau có thể tạo lại bản ghi mang tên mẫu ban đầu.

Database mới được tạo bằng `InitialCreate`; startup và CLI dùng migrations. Với database cũ được tạo bằng `EnsureCreated`, phải backup, đối chiếu schema và thiết lập lịch sử migration phù hợp trước khi cập nhật, không chạy tạo lại các bảng đang có. Máy này đã kiểm tra schema khớp (cột, defaults, identity, indexes, CHECK, FK), xác minh backup và xác nhận lịch sử `InitialCreate`. Tham khảo [migrations của Microsoft](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/).

## Module 5: kết quả, lịch sử đào tạo và thống kê

- Admin/HR: `/Dashboard/Index` và `/KetQuaDaoTao/DanhGia`. Chọn lớp đã qua ngày kết thúc, nhập điểm cho toàn bộ học viên đã duyệt, bấm **Lưu điểm & Tính kết quả nháp**, kiểm tra rồi **Xác nhận & Khóa kết quả**.
- `/KetQuaDaoTao/LichSuDaoTao`: nhân viên chỉ xem hồ sơ của mình; Admin/HR chọn nhân viên hoặc truyền `maNhanVien`. Nhân viên không được xem điểm/kết quả nháp.
- Tỷ lệ tham dự = số buổi `CoMat` / số buổi không bị hủy × 100. `CoPhep` và `VangMat` không tính có mặt. Lớp không có buổi học hợp lệ không được xác nhận. Phải hoàn tất điểm danh trước khi khóa, kể cả chuyển mã `DiMuon` cũ sang một trạng thái điểm danh được Module 4 hỗ trợ.
- Chỉ hoàn thành khi **đồng thời** đủ điểm và tỷ lệ của khóa. Database lưu `Dat`/`KhongDat`, giao diện hiển thị **Hoàn thành / Không hoàn thành**. So sánh tỷ lệ chính xác trước khi làm tròn hai chữ số để hiển thị/lưu; ví dụ 2/3 buổi chưa đạt ngưỡng 66,67%.
- `NgayDanhGia` để trống với bản nháp; có giá trị khi xác nhận, đồng thời là dấu khóa bền vững. Xác nhận đặt lớp thành `HoanThanh`. Module này dùng các cột có sẵn, không cần thay schema hay migration.
- Service dùng transaction `Serializable`, kiểm tra lại quyền, danh sách học viên, điểm và phiên bản dữ liệu để từ chối form cũ. Không tin tỷ lệ/kết quả do trình duyệt gửi lên. `ApplicationDbContext.ResultLock.cs` bảo vệ các thao tác `SaveChanges`/`SaveChangesAsync`: không sửa/xóa kết quả đã khóa hoặc đổi đăng ký, lịch học, điểm danh, ngày lớp hay ngưỡng khóa học ảnh hưởng tới kết quả đó. Đây là bảo vệ trong ứng dụng EF; không thay thế quyền quản trị SQL trực tiếp.
- Dashboard dùng LINQ `GroupBy`, `Count`, `Distinct` và `Average` trên SQL Server. Số nhân viên theo phòng ban đếm người phân biệt đã được duyệt, loại lớp hủy. Thống kê đăng ký theo khóa/lớp gồm mọi trạng thái và hiển thị mọi lớp đồng hạng đông nhất. Tỷ lệ hoàn thành, tham dự trung bình chỉ tính kết quả đã xác nhận; không có dữ liệu hiển thị 0%.
- Lượt theo tháng/quý dựa vào **ngày bắt đầu lớp**, chỉ gồm đăng ký đã duyệt thuộc lớp không hủy; một người học nhiều lớp có nhiều lượt. Lớp đang học/đã kết thúc dựa theo ngày Việt Nam, không gồm lớp hủy.

Đã kiểm thử 45 tình huống SQL Server và HTTP: ngưỡng biên, làm tròn, lớp chưa kết thúc/hủy/rỗng, thiếu điểm/điểm sai, điểm danh chưa đủ, dữ liệu thay đổi, form MVC, CSRF, phân quyền, lịch sử riêng tư, khóa bền vững và các thống kê. Database kiểm thử riêng được xóa sau khi chạy; không tạo kết quả thử trong database ứng dụng.

## Quy ước dữ liệu

- Khóa chính `int` được SQL Server tự tăng theo convention của EF Core. Các FK bắt buộc dùng `int`, `[Required]`, `[Range]`.
- Tài khoản có thể chưa gắn nhân viên; mỗi nhân viên bắt buộc gắn một tài khoản riêng. Mỗi đăng ký có 0 hoặc 1 kết quả.
- `TrangThai` của TaiKhoan, PhongBan, LinhVucDaoTao, NhanVien, KhoaDaoTao là `bool`: `true` hoạt động, `false` ngừng hoạt động.
- Các trạng thái nghiệp vụ và vai trò là chuỗi không dấu; tập giá trị hợp lệ nằm trong `[RegularExpression]` và CHECK constraint tương ứng.
- Điểm theo thang 10, tỷ lệ tham dự theo phần trăm 0–100, số giờ cho phép phần lẻ. Các trường `decimal` được cấu hình precision rõ ràng.
- Ngày dùng `DateOnly`/SQL `date`, giờ dùng `TimeOnly`/SQL `time`; buổi học bắt đầu và kết thúc trong cùng ngày. Thời điểm dùng `DateTime`/SQL `datetime2`, quy ước giờ Việt Nam. Tầng nghiệp vụ phải gán ngày đăng ký, hạn đăng ký và cập nhật thời gian điểm danh mỗi lần sửa; SQL `datetime2` không lưu múi giờ.
- Hạn đăng ký được phép cùng ngày bắt đầu lớp. Điểm và ngày đánh giá được để trống khi chưa đánh giá. Mô tả, ghi chú và số điện thoại là tùy chọn.
- Tên đăng nhập và email tài khoản có chỉ mục duy nhất. Mỗi cặp nhân viên–lớp có một đăng ký; Module 3 không cho đăng ký lại sau hủy/từ chối. Mỗi cặp buổi học–nhân viên có một điểm danh.
- `MatKhau` chỉ chứa hash (ví dụ từ `PasswordHasher<TaiKhoan>`), không chứa mật khẩu thô. Dùng ViewModel riêng cho đăng ký/đăng nhập và các form MVC.

## Quan hệ và xóa dữ liệu

| Quan hệ | Kiểu | Khi xóa bản ghi cha |
| --- | --- | --- |
| TaiKhoan → NhanVien | 1–0..1 | NoAction |
| PhongBan → NhanVien | 1–n | NoAction |
| LinhVucDaoTao → KhoaDaoTao | 1–n | NoAction |
| KhoaDaoTao → LopDaoTao | 1–n | NoAction |
| NhanVien → DangKyDaoTao | 1–n | NoAction |
| LopDaoTao → DangKyDaoTao | 1–n | NoAction |
| LopDaoTao → BuoiHoc | 1–n | NoAction |
| BuoiHoc → DiemDanh | 1–n | NoAction |
| NhanVien → DiemDanh | 1–n | NoAction |
| DangKyDaoTao → KetQuaDaoTao | 1–0..1 | Cascade |

`NoAction` chặn xóa cha khi còn con. Ưu tiên ngừng hoạt động/hủy bằng trạng thái để giữ lịch sử. SQL cấu hình cascade từ đăng ký tới kết quả; Module 5 chặn xóa đăng ký có kết quả đã khóa qua `SaveChanges`. Cấu hình có một đường cascade, tránh multiple cascade paths trên SQL Server. Tham khảo [tài liệu Microsoft về cascade delete](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete).

## Kiểm tra nghiệp vụ khi triển khai Controller/Service

DataAnnotations phục vụ validation đầu vào MVC; EF Core không tự chạy toàn bộ DataAnnotations hoặc `IValidatableObject` khi `SaveChanges`. Context bổ sung CHECK constraint cho các giới hạn số, trạng thái và thứ tự ngày/giờ đã cấu hình.

Module 3 xử lý đăng ký, trùng lịch và sức chứa; Module 4 xử lý ngày học, roster đã duyệt và tỷ lệ tham dự; Module 5 kiểm tra điểm, ngưỡng hoàn thành và khóa kết quả. Điểm danh liên kết riêng tới nhân viên và buổi học; service Module 4 kiểm tra quan hệ đăng ký đúng lớp vì FK không tự bảo đảm điều đó.

`HoTen` và `Email` xuất hiện ở cả TaiKhoan và NhanVien theo đề bài; service cần quy định đồng bộ khi chỉnh sửa. Thay `[Điền MSSV của bạn]` trong các file bằng mã sinh viên thực tế.
