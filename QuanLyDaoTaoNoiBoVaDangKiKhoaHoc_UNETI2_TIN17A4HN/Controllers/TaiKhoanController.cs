//Họ và tên: Nguyễn Cảnh Đạt
//Mã sinh viên: 23103100224
//Module 1: Tài khoản – Đăng nhập – Phân quyền – Phòng ban – Lĩnh vực đào tạo.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Filters;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
namespace QuanLyDaoTaoNoiBo.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TaiKhoanController(ApplicationDbContext db) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult DangNhap() => View(new DangNhapViewModel());
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapViewModel model)
    {
        if (!ModelState.IsValid) return LoginError(model);
        var username = model.TenDangNhap.Trim();
        var account = await db.TaiKhoans.SingleOrDefaultAsync(x => x.TenDangNhap == username);
        var result = PasswordVerificationResult.Failed;
        if (account is not null && account.TrangThai)
        {
            if (account.MatKhau == model.MatKhau)
            {
                result = PasswordVerificationResult.Success;
            }
        }
        if (account is null || result == PasswordVerificationResult.Failed ||
            account.VaiTro is not ("QuanTri" or "QuanLy" or "NhanVien" or "GiangVien"))
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.");
            return LoginError(model);
        }
        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32("MaTaiKhoan", account.MaTaiKhoan);
        HttpContext.Session.SetString("HoTen", account.HoTen);
        HttpContext.Session.SetString("VaiTro", account.VaiTro);
        var employeeId = await db.NhanViens.Where(x => x.MaTaiKhoan == account.MaTaiKhoan)
            .Select(x => (int?)x.MaNhanVien).SingleOrDefaultAsync();
        if (employeeId.HasValue) HttpContext.Session.SetInt32("MaNhanVien", employeeId.Value);
        return RedirectToAction("Index", "Home");
    }
    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public IActionResult DangXuat()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(DangNhap));
    }
    // ===== Quản lý tài khoản (Admin/Nhân sự-Đào tạo) =====
    private static readonly string[] CacVaiTro = ["QuanTri", "QuanLy", "NhanVien", "GiangVien"];

    [HttpGet, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> Index(string? tuKhoa, string? vaiTro, string? trangThai)
    {
        var query = db.TaiKhoans.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            query = query.Where(x => x.TenDangNhap.Contains(k) || x.HoTen.Contains(k) || x.Email.Contains(k));
        }
        if (!string.IsNullOrEmpty(vaiTro) && CacVaiTro.Contains(vaiTro)) query = query.Where(x => x.VaiTro == vaiTro);
        if (trangThai == "hoatdong") query = query.Where(x => x.TrangThai);
        else if (trangThai == "khoa") query = query.Where(x => !x.TrangThai);
        ViewData["TuKhoa"] = tuKhoa; ViewData["VaiTro"] = vaiTro; ViewData["TrangThai"] = trangThai;
        return View(await query.OrderBy(x => x.TenDangNhap).ToListAsync());
    }

    [HttpGet, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await db.TaiKhoans.AsNoTracking().Include(x => x.NhanVien).SingleOrDefaultAsync(x => x.MaTaiKhoan == id);
        if (entity is null) return NotFound();
        if (!CanManage(entity)) return Deny();
        return View(entity);
    }

    [HttpGet, SessionRoles("QuanTri", "QuanLy")]
    public IActionResult Create() => View(new TaiKhoanViewModel());

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> Create(TaiKhoanViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.MatKhau))
            ModelState.AddModelError(nameof(model.MatKhau), "Vui lòng nhập mật khẩu.");
        if (CurrentRole() == "QuanLy" && model.VaiTro == "QuanTri")
            ModelState.AddModelError(nameof(model.VaiTro), "Bạn không có quyền tạo tài khoản Quản trị.");
        await ValidateUnique(model);
        if (!ModelState.IsValid) return View(model);

        var entity = new TaiKhoan
        {
            TenDangNhap = model.TenDangNhap.Trim(),
            MatKhau = model.MatKhau!,
            HoTen = model.HoTen.Trim(),
            Email = model.Email.Trim(),
            VaiTro = model.VaiTro,
            TrangThai = model.TrangThai
        };
        db.TaiKhoans.Add(entity);
        if (!await Save()) return View(model);
        TempData["Success"] = "Đã thêm tài khoản.";
        return RedirectToAction(nameof(Details), new { id = entity.MaTaiKhoan });
    }

    [HttpGet, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(x => x.MaTaiKhoan == id);
        if (e is null) return NotFound();
        if (!CanManage(e)) return Deny();
        return View(new TaiKhoanViewModel
        {
            MaTaiKhoan = e.MaTaiKhoan,
            TenDangNhap = e.TenDangNhap,
            HoTen = e.HoTen,
            Email = e.Email,
            VaiTro = e.VaiTro,
            TrangThai = e.TrangThai
        });
    }

    [HttpPost, ValidateAntiForgeryToken, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> Edit(int id, TaiKhoanViewModel model)
    {
        if (id != model.MaTaiKhoan) return BadRequest();
        var entity = await db.TaiKhoans.SingleOrDefaultAsync(x => x.MaTaiKhoan == id);
        if (entity is null) return NotFound();
        if (!CanManage(entity)) return Deny();
        if (CurrentRole() == "QuanLy" && model.VaiTro == "QuanTri")
            ModelState.AddModelError(nameof(model.VaiTro), "Bạn không có quyền gán vai trò Quản trị.");
        if (id == HttpContext.Session.GetInt32("MaTaiKhoan") && (model.VaiTro != entity.VaiTro || !model.TrangThai))
            ModelState.AddModelError("", "Không thể tự khóa hoặc tự đổi vai trò của tài khoản đang đăng nhập.");
        if (model.VaiTro != "GiangVien" && await db.LopDaoTaos.AnyAsync(x => x.MaGiangVien == id))
            ModelState.AddModelError(nameof(model.VaiTro), "Tài khoản đang được phân công phụ trách lớp, không thể đổi khỏi vai trò Giảng viên.");
        await ValidateUnique(model);
        if (!ModelState.IsValid) return View(model);

        entity.TenDangNhap = model.TenDangNhap.Trim(); entity.HoTen = model.HoTen.Trim();
        entity.Email = model.Email.Trim(); entity.VaiTro = model.VaiTro; entity.TrangThai = model.TrangThai;
        if (!string.IsNullOrWhiteSpace(model.MatKhau)) entity.MatKhau = model.MatKhau;
        if (!await Save()) return View(model);
        TempData["Success"] = "Đã cập nhật tài khoản.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Không xóa tài khoản để giữ lịch sử đào tạo; chỉ khóa / mở khóa.
    [HttpPost, ValidateAntiForgeryToken, SessionRoles("QuanTri", "QuanLy")]
    public async Task<IActionResult> KhoaMo(int id)
    {
        var entity = await db.TaiKhoans.SingleOrDefaultAsync(x => x.MaTaiKhoan == id);
        if (entity is null) return NotFound();
        if (!CanManage(entity)) return Deny();
        if (id == HttpContext.Session.GetInt32("MaTaiKhoan"))
        {
            TempData["Error"] = "Không thể tự khóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Details), new { id });
        }
        entity.TrangThai = !entity.TrangThai;
        try
        {
            await db.SaveChangesAsync();
            TempData["Success"] = entity.TrangThai ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản; tài khoản này không thể đăng nhập.";
        }
        catch (DbUpdateException) { TempData["Error"] = "Không thể cập nhật trạng thái tài khoản."; }
        return RedirectToAction(nameof(Details), new { id });
    }

    private string? CurrentRole() => HttpContext.Session.GetString("VaiTro");
    // Nhân sự-Đào tạo (QuanLy) không được sửa/khóa tài khoản Quản trị.
    private bool CanManage(TaiKhoan target) => !(CurrentRole() == "QuanLy" && target.VaiTro == "QuanTri");
    private IActionResult Deny()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View("~/Views/Shared/KhongCoQuyen.cshtml");
    }

    private async Task ValidateUnique(TaiKhoanViewModel model)
    {
        var ten = model.TenDangNhap?.Trim() ?? "";
        var mail = model.Email?.Trim() ?? "";
        if (ten.Length > 0 && await db.TaiKhoans.AnyAsync(x => x.TenDangNhap == ten && x.MaTaiKhoan != model.MaTaiKhoan))
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");
        if (mail.Length > 0 && await db.TaiKhoans.AnyAsync(x => x.Email == mail && x.MaTaiKhoan != model.MaTaiKhoan))
            ModelState.AddModelError(nameof(model.Email), "Email đã được dùng cho tài khoản khác.");
    }

    private async Task<bool> Save()
    {
        try { await db.SaveChangesAsync(); return true; }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể lưu: tên đăng nhập/email đã tồn tại hoặc dữ liệu vừa thay đổi.");
            return false;
        }
    }

    private IActionResult LoginError(DangNhapViewModel model)
    {
        model.MatKhau = string.Empty;
        if (ModelState.TryGetValue(nameof(model.MatKhau), out var entry))
        {
            entry.RawValue = null;
            entry.AttemptedValue = null;
        }
        return View("DangNhap", model);
    }
}
