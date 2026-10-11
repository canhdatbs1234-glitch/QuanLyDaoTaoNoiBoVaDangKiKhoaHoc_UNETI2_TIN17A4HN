using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Filters;
using QuanLyDaoTaoNoiBoVaDangKiKhoaHoc_UNETI2_TIN17A4HN.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SessionAuthorizationFilter>();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddScoped<SessionAuthorizationFilter>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<QuanLyDaoTaoNoiBo.Services.DangKyDaoTaoService>();
builder.Services.AddScoped<QuanLyDaoTaoNoiBo.Services.DiemDanhService>();
builder.Services.AddScoped<QuanLyDaoTaoNoiBo.Services.KetQuaDaoTaoService>();
builder.Services.AddScoped<QuanLyDaoTaoNoiBo.Services.DashboardService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Thiếu chuỗi kết nối DefaultConnection.")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(context);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=TaiKhoan}/{action=DangNhap}/{id?}");

app.Run();
