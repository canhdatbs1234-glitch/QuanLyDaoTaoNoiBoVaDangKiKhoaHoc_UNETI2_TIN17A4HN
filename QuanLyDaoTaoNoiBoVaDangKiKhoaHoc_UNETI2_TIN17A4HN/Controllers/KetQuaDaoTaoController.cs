// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Danh sách kết quả, đánh giá và xác nhận kết quả đào tạo.
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;
using QuanLyDaoTaoNoiBo.Services;

namespace QuanLyDaoTaoNoiBo.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class KetQuaDaoTaoController(ApplicationDbContext db, KetQuaDaoTaoService service) : Controller
{
    private const int PageSize = 10;
    private const string RoleQuanLyGiangVien = "QuanTri,QuanLy,GiangVien";
    private const string RoleQuanLy = "QuanTri,QuanLy";

    // =====================================================================
    // DANH SÁCH KẾT QUẢ: Truy vấn -> Tìm kiếm -> Lọc -> Sắp xếp -> Phân trang
    // =====================================================================
    [HttpGet, Authorize(Roles = RoleQuanLyGiangVien)]
    public async Task<IActionResult> Index(int? maLop, int? maPhongBan, string? trangThaiKetQua,
        string? xacNhan, string? searchTerm, string sortOrder = "lop", int pageIndex = 1)
    {
        var maTk = User.GetMaTaiKhoan() ?? 0;
        var vaiTro = User.GetVaiTro();
        var laGiangVien = vaiTro == "GiangVien";
        searchTerm = searchTerm?.Trim();

        // Giảng viên chỉ thấy lớp được giao (kiểm tra tại Controller, không chỉ ẩn trên View).
        var lopsQuery = db.LopDaoTaos.AsNoTracking().AsQueryable();
        if (laGiangVien) lopsQuery = lopsQuery.Where(l => l.MaGiangVien == maTk);

        // 1. Truy vấn gốc: chỉ đăng ký ĐÃ DUYỆT của lớp chưa hủy.
        var query = db.DangKyDaoTaos.AsNoTracking()
            .Where(d => d.TrangThai == TrangThaiDangKyConst.DaDuyet && d.LopDaoTao.TrangThai != "DaHuy");
        if (laGiangVien) query = query.Where(d => d.LopDaoTao.MaGiangVien == maTk);

        // 2. Tìm kiếm
        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(d => d.NhanVien.HoTen.Contains(searchTerm) || d.LopDaoTao.TenLop.Contains(searchTerm));

        // 3. Lọc
        if (maLop > 0) query = query.Where(d => d.MaLop == maLop.Value);
        if (maPhongBan > 0) query = query.Where(d => d.NhanVien.MaPhongBan == maPhongBan.Value);
        switch (trangThaiKetQua)
        {
            case "chua":
                query = query.Where(d => d.KetQuaDaoTao == null || d.KetQuaDaoTao.KetQua == KetQuaConst.ChuaDanhGia);
                break;
            case KetQuaConst.HoanThanh:
                query = query.Where(d => d.KetQuaDaoTao != null && d.KetQuaDaoTao.KetQua == KetQuaConst.HoanThanh);
                break;
            case KetQuaConst.KhongHoanThanh:
                query = query.Where(d => d.KetQuaDaoTao != null && d.KetQuaDaoTao.KetQua == KetQuaConst.KhongHoanThanh);
                break;
        }
        if (xacNhan == "da") query = query.Where(d => d.KetQuaDaoTao != null && d.KetQuaDaoTao.DaXacNhan);
        else if (xacNhan == "chua") query = query.Where(d => d.KetQuaDaoTao == null || !d.KetQuaDaoTao.DaXacNhan);

        // 4. Sắp xếp (luôn có ThenBy MaDangKy để thứ tự ổn định khi phân trang)
        query = sortOrder switch
        {
            "ten_asc" => query.OrderBy(d => d.NhanVien.HoTen).ThenBy(d => d.MaDangKy),
            "ten_desc" => query.OrderByDescending(d => d.NhanVien.HoTen).ThenBy(d => d.MaDangKy),
            "diem_desc" => query.OrderByDescending(d => d.KetQuaDaoTao!.DiemDanhGia).ThenBy(d => d.MaDangKy),
            "diem_asc" => query.OrderBy(d => d.KetQuaDaoTao!.DiemDanhGia).ThenBy(d => d.MaDangKy),
            "tyle_desc" => query.OrderByDescending(d => d.KetQuaDaoTao!.TyLeThamDu).ThenBy(d => d.MaDangKy),
            "tyle_asc" => query.OrderBy(d => d.KetQuaDaoTao!.TyLeThamDu).ThenBy(d => d.MaDangKy),
            _ => query.OrderBy(d => d.LopDaoTao.TenLop).ThenBy(d => d.NhanVien.HoTen).ThenBy(d => d.MaDangKy)
        };
        if (!ListViewModelSortOk(sortOrder)) sortOrder = "lop";

        // 5. Phân trang: SAU KHI đã Where + OrderBy (Skip/Take chạy trên tập đã lọc và đã sắp xếp).
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)PageSize);
        pageIndex = Math.Clamp(pageIndex, 1, Math.Max(totalPages, 1));

        var items = await query
            .Skip((pageIndex - 1) * PageSize)
            .Take(PageSize)
            .Select(d => new KetQuaDongViewModel
            {
                MaDangKy = d.MaDangKy,
                MaKetQua = d.KetQuaDaoTao != null ? d.KetQuaDaoTao.MaKetQua : (int?)null,
                MaLop = d.MaLop,
                TenLop = d.LopDaoTao.TenLop,
                HoTen = d.NhanVien.HoTen,
                TenPhongBan = d.NhanVien.PhongBan.TenPhongBan,
                TrangThaiLop = d.LopDaoTao.TrangThai,
                NgayKetThucLop = d.LopDaoTao.NgayKetThuc,
                DiemDanhGia = d.KetQuaDaoTao != null ? d.KetQuaDaoTao.DiemDanhGia : null,
                TyLeThamDu = d.KetQuaDaoTao != null ? d.KetQuaDaoTao.TyLeThamDu : (decimal?)null,
                KetQua = d.KetQuaDaoTao != null ? d.KetQuaDaoTao.KetQua : KetQuaConst.ChuaDanhGia,
                DaXacNhan = d.KetQuaDaoTao != null && d.KetQuaDaoTao.DaXacNhan,
                NgayDanhGia = d.KetQuaDaoTao != null ? d.KetQuaDaoTao.NgayDanhGia : null
            })
            .ToListAsync();

        var homNay = DateOnly.FromDateTime(LopDaoTaoRules.BayGioVietNam());
        foreach (var item in items)
            item.DuDieuKienDanhGia = KetQuaDaoTaoService.LopDuDieuKienDanhGia(item.TrangThaiLop, item.NgayKetThucLop, homNay);

        var model = new KetQuaDaoTaoListViewModel
        {
            Items = items,
            SearchTerm = searchTerm,
            MaLop = maLop > 0 ? maLop : null,
            MaPhongBan = maPhongBan > 0 ? maPhongBan : null,
            TrangThaiKetQua = trangThaiKetQua,
            XacNhan = xacNhan,
            SortOrder = sortOrder,
            PageIndex = pageIndex,
            PageSize = PageSize,
            TotalCount = total,
            CoTheXacNhan = vaiTro is "QuanTri" or "QuanLy",
            Lops = await lopsQuery.OrderBy(l => l.TenLop)
                .Select(l => new SelectListItem(l.TenLop, l.MaLop.ToString())).ToListAsync(),
            PhongBans = await db.PhongBans.AsNoTracking().OrderBy(p => p.TenPhongBan)
                .Select(p => new SelectListItem(p.TenPhongBan, p.MaPhongBan.ToString())).ToListAsync()
        };
        return View(model);
    }

    private static bool ListViewModelSortOk(string sortOrder) =>
        KetQuaDaoTaoListViewModel.SortOptions.ContainsKey(sortOrder);

    // =====================================================================
    // ĐÁNH GIÁ
    // =====================================================================
    [HttpGet, Authorize(Roles = RoleQuanLyGiangVien)]
    public async Task<IActionResult> DanhGia(int maDangKy)
    {
        var dangKy = await TaiDangKyAsync(maDangKy);
        if (dangKy is null) return NotFound();
        if (!KetQuaDaoTaoService.CoQuyenTrenLop(dangKy.LopDaoTao, User.GetMaTaiKhoan() ?? 0, User.GetVaiTro()))
            return Forbid();

        return View(await DungFormAsync(dangKy, null, null, tuForm: false));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RoleQuanLyGiangVien)]
    public async Task<IActionResult> DanhGia(int maDangKy, string? diemDanhGia, string? ghiChu)
    {
        var dangKy = await TaiDangKyAsync(maDangKy);
        if (dangKy is null) return NotFound();
        var maTk = User.GetMaTaiKhoan() ?? 0;
        var vaiTro = User.GetVaiTro();
        if (!KetQuaDaoTaoService.CoQuyenTrenLop(dangKy.LopDaoTao, maTk, vaiTro)) return Forbid();

        // Điểm đọc bằng InvariantCulture để "7.5" không bị hiểu sai theo culture máy chủ.
        if (!TryParseDiem(diemDanhGia, out var diem))
        {
            var sai = await DungFormAsync(dangKy, diemDanhGia, ghiChu, tuForm: true);
            sai.ThongBaoLoi = "Điểm đánh giá phải là một số trong thang 0–10.";
            return View(sai);
        }

        var ketQua = await service.LuuDanhGiaAsync(maDangKy, diem, ghiChu, maTk, vaiTro);
        if (!ketQua.ThanhCong)
        {
            var loi = await DungFormAsync(dangKy, diemDanhGia, ghiChu, tuForm: true);
            loi.ThongBaoLoi = ketQua.ThongBao;
            return View(loi);
        }

        TempData["Success"] = ketQua.ThongBao;
        return RedirectToAction(nameof(DanhGia), new { maDangKy });
    }

    // =====================================================================
    // XÁC NHẬN: chỉ Quản trị / Nhân sự - Đào tạo
    // =====================================================================
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RoleQuanLy)]
    public async Task<IActionResult> XacNhan(int maKetQua, int maDangKy)
    {
        var ketQua = await service.XacNhanAsync(maKetQua, User.GetMaTaiKhoan() ?? 0, User.GetVaiTro());
        TempData[ketQua.ThanhCong ? "Success" : "Error"] = ketQua.ThongBao;
        return RedirectToAction(nameof(DanhGia), new { maDangKy });
    }

    // =====================================================================
    // HÀM HỖ TRỢ
    // =====================================================================
    private Task<DangKyDaoTao?> TaiDangKyAsync(int maDangKy) =>
        db.DangKyDaoTaos.AsNoTracking()
            .Include(x => x.NhanVien).ThenInclude(n => n.PhongBan)
            .Include(x => x.LopDaoTao).ThenInclude(l => l.KhoaDaoTao)
            .Include(x => x.KetQuaDaoTao).ThenInclude(k => k!.NguoiXacNhan)
            .SingleOrDefaultAsync(x => x.MaDangKy == maDangKy);

    private static bool TryParseDiem(string? text, out decimal diem)
    {
        diem = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        return decimal.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Number,
            CultureInfo.InvariantCulture, out diem);
    }

    private async Task<DanhGiaKetQuaViewModel> DungFormAsync(DangKyDaoTao dk, string? diemNhap, string? ghiChu, bool tuForm)
    {
        var lop = dk.LopDaoTao;
        var khoa = lop.KhoaDaoTao;
        var kq = dk.KetQuaDaoTao;
        var bayGio = LopDaoTaoRules.BayGioVietNam();
        var vaiTro = User.GetVaiTro();
        var daXacNhan = kq is { DaXacNhan: true };

        // Tỷ lệ tham dự luôn tính lại từ dữ liệu điểm danh hiện tại.
        var (tong, coMat) = await service.DemThamDuAsync(lop.MaLop, dk.MaNhanVien, bayGio);
        var tyLe = KetQuaDaoTaoService.TinhTyLeThamDu(coMat, tong);

        var lyDo = KetQuaDaoTaoService.LyDoKhongDuDieuKienDanhGia(dk, lop, DateOnly.FromDateTime(bayGio));
        if (lyDo is null && tong == 0)
            lyDo = "Lớp chưa có buổi học hợp lệ (đã kết thúc, chưa hủy) nên chưa thể tính tỷ lệ tham dự.";
        if (lyDo is null && daXacNhan && vaiTro != "QuanTri")
            lyDo = "Kết quả đã được xác nhận. Chỉ Quản trị viên mới được điều chỉnh.";

        var model = new DanhGiaKetQuaViewModel
        {
            MaDangKy = dk.MaDangKy,
            MaKetQua = kq?.MaKetQua,
            HoTen = dk.NhanVien.HoTen,
            TenPhongBan = dk.NhanVien.PhongBan.TenPhongBan,
            TenLop = lop.TenLop,
            TenKhoa = khoa.TenKhoaDaoTao,
            NgayBatDau = lop.NgayBatDau,
            NgayKetThuc = lop.NgayKetThuc,
            TongSoBuoiHopLe = tong,
            SoBuoiCoMat = coMat,
            TyLeThamDu = tyLe,
            TyLeThamDuToiThieu = khoa.TyLeThamDuToiThieu,
            DiemDatToiThieu = khoa.DiemDatToiThieu,
            KetQuaHienTai = kq?.KetQua ?? KetQuaConst.ChuaDanhGia,
            DiemDaLuu = kq?.DiemDanhGia,
            TyLeDaLuu = kq?.TyLeThamDu,
            NgayDanhGia = kq?.NgayDanhGia,
            GhiChuDaLuu = kq?.GhiChu,
            DaXacNhan = daXacNhan,
            NgayXacNhan = kq?.NgayXacNhan,
            TenNguoiXacNhan = kq?.NguoiXacNhan?.HoTen,
            LyDoKhongDanhGia = lyDo,
            ChoPhepLuu = lyDo is null,
            LaDieuChinh = daXacNhan,
            ChoPhepXacNhan = vaiTro is "QuanTri" or "QuanLy" && kq is not null && !daXacNhan
                             && kq.KetQua != KetQuaConst.ChuaDanhGia && lyDo is null,
            DiemNhap = tuForm ? diemNhap : kq?.DiemDanhGia?.ToString("0.##", CultureInfo.InvariantCulture),
            GhiChu = tuForm ? ghiChu : (daXacNhan ? null : kq?.GhiChu)
        };

        // Kết quả dự kiến theo điểm đang nhập (chỉ để xem trước, không lưu).
        if (tong > 0 && TryParseDiem(model.DiemNhap, out var diem))
            model.KetQuaDuKien = KetQuaConst.Ten(
                KetQuaDaoTaoService.XacDinhKetQua(tyLe, diem, khoa.TyLeThamDuToiThieu, khoa.DiemDatToiThieu));

        return model;
    }
}