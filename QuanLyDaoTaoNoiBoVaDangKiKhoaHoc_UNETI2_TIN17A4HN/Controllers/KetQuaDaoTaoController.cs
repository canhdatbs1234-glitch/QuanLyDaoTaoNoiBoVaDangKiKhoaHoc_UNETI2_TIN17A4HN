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
    private const string RoleQuanTriGiangVien = "QuanTri,GiangVien";
    private const string RoleQuanTri = "QuanTri";

    // =====================================================================
    // DANH SÁCH KẾT QUẢ: Truy vấn -> Tìm kiếm -> Lọc -> Sắp xếp -> Phân trang
    // =====================================================================
    [HttpGet, SessionAuthorize(Roles = RoleQuanTriGiangVien)]
    public async Task<IActionResult> Index(int? maLop, int? maPhongBan, string? trangThaiKetQua,
        string? xacNhan, string? searchTerm, string sortOrder = "lop", int pageIndex = 1)
    {
        var maTk = HttpContext.Session.GetMaTaiKhoan() ?? 0;
        var vaiTro = HttpContext.Session.GetVaiTro();
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
            CoTheXacNhan = vaiTro == "QuanTri",
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
    [HttpGet, SessionAuthorize(Roles = RoleQuanTriGiangVien)]
    public async Task<IActionResult> DanhGia(int maDangKy)
    {
        var dangKy = await TaiDangKyAsync(maDangKy);
        if (dangKy is null) return NotFound();
        if (!KetQuaDaoTaoService.CoQuyenTrenLop(dangKy.LopDaoTao, HttpContext.Session.GetMaTaiKhoan() ?? 0, HttpContext.Session.GetVaiTro()))
            return Forbid();

        return View(await DungFormAsync(dangKy, null, null, tuForm: false));
    }

    [HttpPost, ValidateAntiForgeryToken, SessionAuthorize(Roles = RoleQuanTriGiangVien)]
    public async Task<IActionResult> DanhGia(int maDangKy, string? diemDanhGia, string? ghiChu)
    {
        var dangKy = await TaiDangKyAsync(maDangKy);
        if (dangKy is null) return NotFound();
        var maTk = HttpContext.Session.GetMaTaiKhoan() ?? 0;
        var vaiTro = HttpContext.Session.GetVaiTro();
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
    [HttpPost, ValidateAntiForgeryToken, SessionAuthorize(Roles = RoleQuanTri)]
    public async Task<IActionResult> XacNhan(int maKetQua, int maDangKy)
    {
        var ketQua = await service.XacNhanAsync(maKetQua, HttpContext.Session.GetMaTaiKhoan() ?? 0, HttpContext.Session.GetVaiTro());
        TempData[ketQua.ThanhCong ? "Success" : "Error"] = ketQua.ThongBao;
        return RedirectToAction(nameof(DanhGia), new { maDangKy });
    }

    // =====================================================================
    // LỊCH SỬ ĐÀO TẠO (mục 9.5)
    //  - Nhân viên: chỉ xem của chính mình
    //  - Quản trị / Nhân sự - Đào tạo: xem của bất kỳ nhân viên (từ trang chi tiết nhân viên)
    // =====================================================================
    [HttpGet, SessionAuthorize]
    public async Task<IActionResult> LichSuDaoTao(int? maNhanVien, string? searchTerm,
        string? trangThaiDangKy, string? ketQua, int pageIndex = 1)
    {
        var vaiTro = HttpContext.Session.GetVaiTro();
        var maTk = HttpContext.Session.GetMaTaiKhoan() ?? 0;
        var quanLyXem = vaiTro == "QuanTri";
        if (!quanLyXem && vaiTro != "NhanVien") return Forbid();

        NhanVien? nv;
        if (quanLyXem)
        {
            if (maNhanVien is null or <= 0) return RedirectToAction("Index", "NhanVien");
            nv = await db.NhanViens.AsNoTracking().Include(n => n.PhongBan)
                .SingleOrDefaultAsync(n => n.MaNhanVien == maNhanVien.Value);
            if (nv is null) return NotFound();
        }
        else
        {
            // Nhân viên: luôn lấy hồ sơ theo TÀI KHOẢN ĐANG ĐĂNG NHẬP, không tin tham số trên URL.
            nv = await db.NhanViens.AsNoTracking().Include(n => n.PhongBan)
                .SingleOrDefaultAsync(n => n.MaTaiKhoan == maTk);
            if (nv is null) return View(new LichSuDaoTaoViewModel { ChuaCoHoSo = true });
            if (maNhanVien.HasValue && maNhanVien.Value != nv.MaNhanVien) return Forbid();
        }

        var maNv = nv.MaNhanVien;
        searchTerm = searchTerm?.Trim();

        // Truy vấn gốc: MỌI đăng ký của nhân viên (không xóa dữ liệu -> giữ trọn lịch sử)
        var tatCa = db.DangKyDaoTaos.AsNoTracking().Where(d => d.MaNhanVien == maNv);
        var query = tatCa;

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(d => d.LopDaoTao.TenLop.Contains(searchTerm)
                                  || d.LopDaoTao.KhoaDaoTao.TenKhoaDaoTao.Contains(searchTerm));
        if (!string.IsNullOrEmpty(trangThaiDangKy) && TrangThaiDangKyConst.TenTrangThai.ContainsKey(trangThaiDangKy))
            query = query.Where(d => d.TrangThai == trangThaiDangKy);

        switch (ketQua)
        {
            case "chua":
                query = query.Where(d => d.TrangThai == TrangThaiDangKyConst.DaDuyet
                                      && (d.KetQuaDaoTao == null || !d.KetQuaDaoTao.DaXacNhan));
                break;
            case KetQuaConst.HoanThanh:
                query = query.Where(d => d.KetQuaDaoTao != null && d.KetQuaDaoTao.DaXacNhan
                                      && d.KetQuaDaoTao.KetQua == KetQuaConst.HoanThanh);
                break;
            case KetQuaConst.KhongHoanThanh:
                query = query.Where(d => d.KetQuaDaoTao != null && d.KetQuaDaoTao.DaXacNhan
                                      && d.KetQuaDaoTao.KetQua == KetQuaConst.KhongHoanThanh);
                break;
        }

        query = query.OrderByDescending(d => d.LopDaoTao.NgayBatDau).ThenByDescending(d => d.MaDangKy);

        // Phân trang sau khi đã lọc + sắp xếp
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)PageSize);
        pageIndex = Math.Clamp(pageIndex, 1, Math.Max(totalPages, 1));

        var items = await query
            .Skip((pageIndex - 1) * PageSize)
            .Take(PageSize)
            .Select(d => new LichSuDongViewModel
            {
                MaDangKy = d.MaDangKy,
                MaLop = d.MaLop,
                TenLop = d.LopDaoTao.TenLop,
                TenKhoa = d.LopDaoTao.KhoaDaoTao.TenKhoaDaoTao,
                DiaDiem = d.LopDaoTao.DiaDiem,
                NgayBatDau = d.LopDaoTao.NgayBatDau,
                NgayKetThuc = d.LopDaoTao.NgayKetThuc,
                TrangThaiDangKy = d.TrangThai,
                NgayDangKy = d.NgayDangKy,
                NgayDuyet = d.NgayDuyet,
                GhiChuDangKy = d.GhiChu,
                CoKetQua = d.KetQuaDaoTao != null && d.KetQuaDaoTao.KetQua != KetQuaConst.ChuaDanhGia,
                KetQua = d.KetQuaDaoTao != null ? d.KetQuaDaoTao.KetQua : KetQuaConst.ChuaDanhGia,
                DiemDanhGia = d.KetQuaDaoTao != null ? d.KetQuaDaoTao.DiemDanhGia : null,
                TyLeThamDu = d.KetQuaDaoTao != null ? d.KetQuaDaoTao.TyLeThamDu : (decimal?)null,
                DaXacNhan = d.KetQuaDaoTao != null && d.KetQuaDaoTao.DaXacNhan,
                NgayXacNhan = d.KetQuaDaoTao != null ? d.KetQuaDaoTao.NgayXacNhan : null,
                XemSoBo = quanLyXem
            })
            .ToListAsync();

        // Tỷ lệ tham dự tạm tính cho các lớp đã duyệt: gom theo lớp để tránh truy vấn từng dòng (N+1).
        var maLops = items.Where(i => i.DaDuyet).Select(i => i.MaLop).Distinct().ToList();
        if (maLops.Count > 0)
        {
            var buoiHopLe = service.BuoiHopLe(LopDaoTaoRules.BayGioVietNam()).Where(b => maLops.Contains(b.MaLop));

            var tongTheoLop = await buoiHopLe
                .GroupBy(b => b.MaLop)
                .Select(g => new { MaLop = g.Key, So = g.Count() })
                .ToDictionaryAsync(x => x.MaLop, x => x.So);

            var coMatTheoLop = await (from dd in db.DiemDanhs.AsNoTracking()
                                      join b in buoiHopLe on dd.MaBuoiHoc equals b.MaBuoiHoc
                                      where dd.MaNhanVien == maNv && dd.TrangThai == "CoMat"
                                      group b by b.MaLop into g
                                      select new { MaLop = g.Key, So = g.Count() })
                                     .ToDictionaryAsync(x => x.MaLop, x => x.So);

            foreach (var item in items.Where(i => i.DaDuyet))
            {
                item.TongSoBuoiHopLe = tongTheoLop.GetValueOrDefault(item.MaLop);
                item.SoBuoiCoMat = coMatTheoLop.GetValueOrDefault(item.MaLop);
            }
        }

        // Tổng hợp trên TOÀN BỘ lịch sử (không phụ thuộc bộ lọc), chỉ tính kết quả đã xác nhận
        var daXacNhan = db.KetQuaDaoTaos.AsNoTracking()
            .Where(k => k.DangKyDaoTao.MaNhanVien == maNv && k.DaXacNhan);

        var model = new LichSuDaoTaoViewModel
        {
            QuanLyXem = quanLyXem,
            MaNhanVien = maNv,
            HoTen = nv.HoTen,
            TenPhongBan = nv.PhongBan.TenPhongBan,
            Email = nv.Email,
            TongDangKy = await tatCa.CountAsync(),
            SoDaDuyet = await tatCa.CountAsync(d => d.TrangThai == TrangThaiDangKyConst.DaDuyet),
            SoHoanThanh = await daXacNhan.CountAsync(k => k.KetQua == KetQuaConst.HoanThanh),
            SoKhongHoanThanh = await daXacNhan.CountAsync(k => k.KetQua == KetQuaConst.KhongHoanThanh),
            TyLeThamDuTrungBinh = await daXacNhan.AverageAsync(k => (decimal?)k.TyLeThamDu),
            SearchTerm = searchTerm,
            TrangThaiDangKy = trangThaiDangKy,
            KetQua = ketQua,
            PageIndex = pageIndex,
            PageSize = PageSize,
            TotalCount = total,
            Items = items
        };
        return View(model);
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
        var vaiTro = HttpContext.Session.GetVaiTro();
        var daXacNhan = kq is { DaXacNhan: true };

        // Tỷ lệ tham dự luôn tính lại từ dữ liệu điểm danh hiện tại.
        var (tong, coMat) = await service.DemThamDuAsync(lop.MaLop, dk.MaNhanVien, bayGio);
        var tyLe = KetQuaDaoTaoService.TinhTyLeThamDu(coMat, tong);

        var lyDo = KetQuaDaoTaoService.LyDoKhongDuDieuKienDanhGia(dk, lop, DateOnly.FromDateTime(bayGio));
        if (lyDo is null && tong == 0)
            lyDo = "Lớp chưa có buổi học hợp lệ (đã kết thúc, chưa hủy) nên chưa thể tính tỷ lệ tham dự.";
        if (lyDo is null && daXacNhan && vaiTro != "QuanTri")
            lyDo = "Kết quả đã được xác nhận. Chỉ Admin / Nhân sự - Đào tạo mới được điều chỉnh.";

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
            ChoPhepXacNhan = vaiTro == "QuanTri" && kq is not null && !daXacNhan
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