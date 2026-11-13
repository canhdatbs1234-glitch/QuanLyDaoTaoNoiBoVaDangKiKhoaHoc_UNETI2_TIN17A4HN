using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;

namespace QuanLyDaoTaoNoiBo.Services;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class SessionAuthorizeAttribute : Attribute
{
    public string? Roles { get; set; }
}
public sealed class SessionAuthorizationFilter(ApplicationDbContext db) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var metadata = context.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAllowAnonymous>().Any()) return;

        var session = context.HttpContext.Session;
        await session.LoadAsync();

        var id = session.GetMaTaiKhoan();
        var vaiTro = session.GetVaiTro();

        var account = id.HasValue
            ? await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(x => x.MaTaiKhoan == id.Value)
            : null;

        if (account is null || !account.TrangThai || account.VaiTro != vaiTro)
        {
            session.Clear();
            context.Result = new RedirectToActionResult("DangNhap", "TaiKhoan", null);   // chưa đăng nhập
            return;
        }

        foreach (var attr in metadata.OfType<SessionAuthorizeAttribute>())
        {
            if (string.IsNullOrWhiteSpace(attr.Roles)) continue;
            var allowed = attr.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!allowed.Contains(vaiTro))
            {
                context.Result = new RedirectToActionResult("KhongCoQuyen", "TaiKhoan", null);   // sai vai trò
                return;
            }
        }
    }
}