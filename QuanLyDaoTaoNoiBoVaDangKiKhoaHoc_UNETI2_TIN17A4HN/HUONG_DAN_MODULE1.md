# Module 1 – Tài khoản, đăng nhập, phân quyền, phòng ban, lĩnh vực đào tạo

## Chạy
```powershell
dotnet restore
dotnet run
```
Ứng dụng tự `Migrate()` database `QuanLyDaoTaoNoiBo_DB` (LocalDB) và nạp dữ liệu mẫu **mà không xóa dữ liệu cũ**.
Nếu muốn tạo lại từ đầu: `dotnet ef database drop` rồi chạy lại.

## Tài khoản mẫu
| Tên đăng nhập | Mật khẩu | Vai trò |
| --- | --- | --- |
| admin | Admin@123 | QuanTri (Admin / Nhân sự - Đào tạo) |
| nhansu | NhanSu@123 | QuanTri (Admin / Nhân sự - Đào tạo) |
| nhanvien | NhanVien@123 | NhanVien |
| giangvien | GiangVien@123 | GiangVien |
| nhanvienkhoa | NhanVien@123 | NhanVien – **bị khóa** (để thử chặn đăng nhập) |

## Phân quyền
- Cả ứng dụng yêu cầu đăng nhập (filter `SessionAuthorizationFilter` + Session trong `Program.cs`), trừ action `[AllowAnonymous]`.
- `TaiKhoan`, `PhongBan`, `LinhVucDaoTao`: `[SessionAuthorize(Roles = "QuanTri")]` kiểm tra ở Controller.
- Hệ thống có 3 vai trò: `QuanTri` (Admin / Nhân sự - Đào tạo), `NhanVien`, `GiangVien`.
- Mỗi request đọc lại tài khoản từ DB: khóa tài khoản hoặc đổi vai trò có hiệu lực ngay (kiểm tra trong `SessionAuthorizationFilter`), Session đăng nhập bị hủy.
- Thiếu quyền → chuyển tới trang 403 (`TaiKhoan/KhongCoQuyen`).

## Quy tắc dữ liệu
- Tên đăng nhập, email, tên phòng ban, tên lĩnh vực: bắt buộc, không trùng (kiểm tra ở Controller + unique index).
- Mật khẩu bắt buộc khi tạo, tối thiểu 6 ký tự, lưu dạng băm `PasswordHasher`.
- Tài khoản không bị xóa, chỉ khóa/mở khóa. Phòng ban/lĩnh vực chỉ xóa khi chưa có dữ liệu liên quan,
  nếu không thì chuyển sang "Ngừng hoạt động".
- Phòng ban ngừng hoạt động: Module Nhân viên cần lọc `TrangThai == true` khi tạo/gán nhân viên mới.
