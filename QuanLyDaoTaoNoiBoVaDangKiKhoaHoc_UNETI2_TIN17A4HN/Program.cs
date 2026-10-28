//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddScoped<DangKyDaoTaoService>();
builder.Services.AddScoped<KetQuaDaoTaoService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Thiếu chuỗi kết nối DefaultConnection."))
        // Snapshot migration cũ lệch nhẹ so với model hiện tại; bỏ qua cảnh báo để Migrate() vẫn chạy.
        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

// ===== XÁC THỰC: Cookie Authentication (Claims-based) =====
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/DangNhap";            // 401: chưa đăng nhập
        options.AccessDeniedPath = "/TaiKhoan/KhongCoQuyen";  // 403: sai vai trò
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Mỗi request kiểm tra lại tài khoản trong DB: bị khóa hoặc đổi vai trò thì cookie cũ mất hiệu lực ngay.
        options.Events.OnValidatePrincipal = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var id = context.Principal?.GetMaTaiKhoan();
            var account = id.HasValue
                ? await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(x => x.MaTaiKhoan == id.Value)
                : null;

            if (account is null || !account.TrangThai || account.VaiTro != context.Principal!.GetVaiTro())
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

// ===== PHÂN QUYỀN: mặc định mọi action phải đăng nhập (trừ [AllowAnonymous]) =====
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
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
app.UseAuthentication();   // Giải mã cookie -> HttpContext.User (BẮT BUỘC đứng trước UseAuthorization)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TaiKhoan}/{action=DangNhap}/{id?}");

app.Run();
