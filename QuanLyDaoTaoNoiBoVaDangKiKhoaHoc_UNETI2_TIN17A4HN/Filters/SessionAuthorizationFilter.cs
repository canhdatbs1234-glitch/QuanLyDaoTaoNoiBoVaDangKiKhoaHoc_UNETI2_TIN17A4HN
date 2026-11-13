//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;

namespace QuanLyDaoTaoNoiBo.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class SessionRolesAttribute(params string[] roles) : Attribute
{
    public string[] Roles { get; } = roles;
}
public sealed class SessionAuthorizationFilter(ApplicationDbContext db) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any()) return;

        var session = context.HttpContext.Session;
        await session.LoadAsync(context.HttpContext.RequestAborted);
        var id = session.GetInt32("MaTaiKhoan");
        var account = id.HasValue
            ? await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(x => x.MaTaiKhoan == id.Value,
                context.HttpContext.RequestAborted)
            : null;
        if (account is null || !account.TrangThai || account.VaiTro is not ("QuanTri" or "QuanLy" or "NhanVien" or "GiangVien"))
        {
            session.Clear();
            context.Result = new RedirectToActionResult("DangNhap", "TaiKhoan", null);
            return;
        }
        session.SetString("HoTen", account.HoTen);
        session.SetString("VaiTro", account.VaiTro);
        var employeeId = await db.NhanViens.AsNoTracking().Where(x => x.MaTaiKhoan == account.MaTaiKhoan)
            .Select(x => (int?)x.MaNhanVien).SingleOrDefaultAsync(context.HttpContext.RequestAborted);
        if (employeeId.HasValue) session.SetInt32("MaNhanVien", employeeId.Value);
        else session.Remove("MaNhanVien");
        var requirements = context.ActionDescriptor.EndpointMetadata.OfType<SessionRolesAttribute>();
        if (requirements.Any(x => !x.Roles.Contains(account.VaiTro)))
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Result = new ViewResult { ViewName = "~/Views/Shared/KhongCoQuyen.cshtml" };
        }
    }
}
