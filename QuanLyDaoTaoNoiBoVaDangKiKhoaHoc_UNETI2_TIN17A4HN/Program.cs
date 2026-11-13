 //Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Services;

if (args.Contains("--test-excel"))
{
    var exitCode = await NhanVienExcelTestRunner.RunAsync(args);
    Environment.ExitCode = exitCode;
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<SessionAuthorizationFilter>();   // mặc định mọi action phải đăng nhập (trừ [AllowAnonymous])
});
builder.Services.AddScoped<DangKyDaoTaoService>();
builder.Services.AddScoped<KetQuaDaoTaoService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<DiemDanhService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Thiếu chuỗi kết nối DefaultConnection."))
        // Snapshot migration cũ lệch nhẹ so với model hiện tại; bỏ qua cảnh báo để Migrate() vẫn chạy.
        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

// ===== XÁC THỰC & PHÂN QUYỀN: dùng Session (lưu trên server, trình duyệt chỉ giữ Session ID) =====
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);   // hết hạn sau 30 phút không hoạt động
    options.Cookie.Name = ".QLDT.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Tạo/cập nhật schema bằng migrations và nạp dữ liệu mẫu Module 1 (không xóa dữ liệu hiện có).
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();   // Kích hoạt Session (BẮT BUỘC đứng trước khi action chạy)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TaiKhoan}/{action=DangNhap}/{id?}");

app.Run();
