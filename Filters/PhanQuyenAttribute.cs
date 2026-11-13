// Họ và tên: Nguyễn Cảnh Đạt
// Mã sinh viên: 23103100224
// Nội dung thực hiện: Module 1 - Phân quyền truy cập cho các Controller/Action dựa trên vai trò Admin/NhanVien với CustomAuthorizeAttribute.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace QuanLyDaoTaoNoiBo_UNETI17_TI17A4HN.Filters
{
    public class CustomAuthorizeAttribute : ActionFilterAttribute
    {
        public string? Roles { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");
            if (string.IsNullOrEmpty(vaiTro))
            {
                context.Result = new RedirectToActionResult("Login", "TaiKhoan", null);
                return;
            }

            if (!string.IsNullOrEmpty(Roles))
            {
                var allowedRoles = Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim());
                if (!allowedRoles.Contains(vaiTro))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
