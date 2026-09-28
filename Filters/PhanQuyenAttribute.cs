// Họ và tên: [SV1 - điền họ tên]
// Mã sinh viên: [điền MSSV]
// Nội dung thực hiện: Attribute phân quyền tại Controller: kiểm tra Session, không chỉ ẩn nút trên View.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Helpers;

namespace QuanLyDaoTaoNoiBo_UNETI2_TI17A1HN.Filters;

/// <summary>
/// [PhanQuyen]                          -> chỉ cần đã đăng nhập
/// [PhanQuyen(QuyenHan.Admin)]          -> chỉ Admin
/// [PhanQuyen(QuyenHan.Admin, QuyenHan.GiangVien)] -> Admin hoặc Giảng viên
/// Đặt ở cấp Controller hoặc Action.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PhanQuyenAttribute : Attribute, IActionFilter
{
    private readonly string[] _vaiTros;

    public PhanQuyenAttribute(params string[] vaiTros)
    {
        _vaiTros = vaiTros;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var vaiTro = context.HttpContext.Session.GetString(SessionKeys.VaiTro);

        if (string.IsNullOrEmpty(vaiTro))
        {
            // Chưa đăng nhập -> về trang đăng nhập
            context.Result = new RedirectToActionResult("Login", "TaiKhoan",
                new { returnUrl = request.Path + request.QueryString });
            return;
        }

        if (_vaiTros.Length > 0 && !_vaiTros.Contains(vaiTro))
        {
            // Đã đăng nhập nhưng không đủ quyền
            context.Result = new RedirectToActionResult("TuChoiTruyCap", "TaiKhoan", null);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
