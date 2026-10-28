// Họ và tên: Đàm Quang Huy
// Mã sinh viên: 23103100197
// Nội dung thực hiện: Module 5 - Tính tỷ lệ tham dự, xác định Hoàn thành/Không hoàn thành, đánh giá và xác nhận kết quả.
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Models.Entities;

namespace QuanLyDaoTaoNoiBo.Services;

public record KetQuaXuLy(bool ThanhCong, string ThongBao);

public class KetQuaDaoTaoService(ApplicationDbContext db)
{
    private static KetQuaXuLy Ok(string msg) => new(true, msg);
    private static KetQuaXuLy Loi(string msg) => new(false, msg);

    // ================= 1. Hàm tính toán thuần (dễ giải thích, dễ kiểm thử) =================

    /// <summary>TyLeThamDu = SoBuoiCoMat / TongSoBuoiHopLe x 100 (cùng cách làm tròn với báo cáo của Module 4).</summary>
    public static decimal TinhTyLeThamDu(int soBuoiCoMat, int tongSoBuoiHopLe)
        => tongSoBuoiHopLe <= 0 ? 0m : Math.Round(soBuoiCoMat * 100m / tongSoBuoiHopLe, 2);

    /// <summary>Hoàn thành khi đồng thời đạt tỷ lệ tham dự VÀ điểm đánh giá (mục 9.3).</summary>
    public static string XacDinhKetQua(decimal tyLeThamDu, decimal? diemDanhGia, decimal tyLeToiThieu, decimal diemToiThieu)
    {
        if (diemDanhGia is null) return KetQuaConst.ChuaDanhGia;
        return tyLeThamDu >= tyLeToiThieu && diemDanhGia.Value >= diemToiThieu
            ? KetQuaConst.HoanThanh
            : KetQuaConst.KhongHoanThanh;
    }

    /// <summary>Lớp đủ điều kiện đánh giá: không bị hủy và (đã HoanThanh hoặc đã qua ngày kết thúc).</summary>
    public static bool LopDuDieuKienDanhGia(string trangThaiLop, DateOnly ngayKetThuc, DateOnly homNay)
        => trangThaiLop != "DaHuy" && (trangThaiLop == "HoanThanh" || homNay > ngayKetThuc);

    /// <summary>Trả về null nếu được đánh giá; ngược lại trả về lý do (mục 9.2).</summary>
    public static string? LyDoKhongDuDieuKienDanhGia(DangKyDaoTao dangKy, LopDaoTao lop, DateOnly homNay)
    {
        if (dangKy.TrangThai != TrangThaiDangKyConst.DaDuyet)
            return "Chỉ đăng ký đã duyệt mới được đánh giá.";
        if (lop.TrangThai == "DaHuy")
            return "Lớp đã bị hủy nên không thể đánh giá.";
        return LopDuDieuKienDanhGia(lop.TrangThai, lop.NgayKetThuc, homNay)
            ? null
            : "Lớp chưa kết thúc nên chưa đủ điều kiện đánh giá.";
    }

    /// <summary>Quản trị/Nhân sự: mọi lớp. Giảng viên: chỉ lớp được giao.</summary>
    public static bool CoQuyenTrenLop(LopDaoTao lop, int maTaiKhoan, string? vaiTro)
        => vaiTro is "QuanTri" or "QuanLy" || (vaiTro == "GiangVien" && lop.MaGiangVien == maTaiKhoan);

    // ================= 2. Tỷ lệ tham dự tính từ dữ liệu điểm danh =================

    /// <summary>Buổi học hợp lệ: buổi và lớp không bị hủy, buổi đã kết thúc theo giờ Việt Nam.</summary>
    public IQueryable<BuoiHoc> BuoiHopLe(DateTime bayGio)
    {
        var homNay = DateOnly.FromDateTime(bayGio);
        var gioNay = TimeOnly.FromDateTime(bayGio);
        return db.BuoiHocs.AsNoTracking().Where(b =>
            b.TrangThai != "DaHuy" && b.LopDaoTao.TrangThai != "DaHuy"
            && (b.NgayHoc < homNay || (b.NgayHoc == homNay && b.GioKetThuc <= gioNay)));
    }

    /// <summary>Đếm tổng số buổi hợp lệ của lớp và số buổi nhân viên Có mặt.</summary>
    public async Task<(int TongSoBuoiHopLe, int SoBuoiCoMat)> DemThamDuAsync(
        int maLop, int maNhanVien, DateTime bayGio, CancellationToken ct = default)
    {
        var buoiHopLe = BuoiHopLe(bayGio).Where(b => b.MaLop == maLop);
        var tong = await buoiHopLe.CountAsync(ct);
        var coMat = await (from d in db.DiemDanhs.AsNoTracking()
                           join b in buoiHopLe on d.MaBuoiHoc equals b.MaBuoiHoc
                           where d.MaNhanVien == maNhanVien && d.TrangThai == "CoMat"
                           select d.MaDiemDanh).CountAsync(ct);
        return (tong, coMat);
    }

    // ================= 3. Đánh giá (tạo/cập nhật kết quả) =================

    public async Task<KetQuaXuLy> LuuDanhGiaAsync(int maDangKy, decimal? diem, string? ghiChu,
        int maTaiKhoan, string? vaiTro, CancellationToken ct = default)
    {
        var dangKy = await db.DangKyDaoTaos
            .Include(x => x.LopDaoTao).ThenInclude(l => l.KhoaDaoTao)
            .Include(x => x.KetQuaDaoTao)
            .SingleOrDefaultAsync(x => x.MaDangKy == maDangKy, ct);
        if (dangKy is null) return Loi("Không tìm thấy đăng ký đào tạo.");

        var lop = dangKy.LopDaoTao;
        var khoa = lop.KhoaDaoTao;

        if (!CoQuyenTrenLop(lop, maTaiKhoan, vaiTro))
            return Loi("Bạn không có quyền đánh giá học viên của lớp này.");

        var bayGio = LopDaoTaoRules.BayGioVietNam();
        var lyDo = LyDoKhongDuDieuKienDanhGia(dangKy, lop, DateOnly.FromDateTime(bayGio));
        if (lyDo is not null) return Loi(lyDo);

        if (diem is null) return Loi("Vui lòng nhập điểm đánh giá.");
        if (diem < 0 || diem > 10) return Loi("Điểm đánh giá phải nằm trong thang điểm 0–10.");
        diem = Math.Round(diem.Value, 2, MidpointRounding.AwayFromZero);
        ghiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim();

        var kq = dangKy.KetQuaDaoTao;
        var ghiChuMoi = ghiChu;

        // Kết quả đã xác nhận: chỉ Quản trị viên được điều chỉnh và bắt buộc ghi lý do (mục 9.4).
        if (kq is { DaXacNhan: true })
        {
            if (vaiTro != "QuanTri") return Loi("Kết quả đã được xác nhận. Chỉ Quản trị viên mới được điều chỉnh.");
            if (ghiChu is null) return Loi("Điều chỉnh kết quả đã xác nhận bắt buộc phải ghi lý do.");
            var dong = $"[Điều chỉnh {bayGio:dd/MM/yyyy HH:mm} - tài khoản #{maTaiKhoan}] {ghiChu}";
            ghiChuMoi = string.IsNullOrWhiteSpace(kq.GhiChu) ? dong : kq.GhiChu + "\n" + dong;
            if (ghiChuMoi.Length > 1000) return Loi("Ghi chú vượt quá 1000 ký tự, hãy viết ngắn lại lý do.");
            db.ChoPhepSuaKetQuaDaKhoa = true;
        }

        // Tỷ lệ tham dự và kết quả do HỆ THỐNG tính, không nhận từ người dùng.
        var (tong, coMat) = await DemThamDuAsync(lop.MaLop, dangKy.MaNhanVien, bayGio, ct);
        if (tong == 0)
        {
            db.ChoPhepSuaKetQuaDaKhoa = false;
            return Loi("Lớp chưa có buổi học hợp lệ (đã kết thúc, chưa hủy) nên chưa thể tính tỷ lệ tham dự.");
        }
        var tyLe = TinhTyLeThamDu(coMat, tong);
        var ketQua = XacDinhKetQua(tyLe, diem, khoa.TyLeThamDuToiThieu, khoa.DiemDatToiThieu);

        if (kq is null)
        {
            kq = new KetQuaDaoTao { MaDangKy = dangKy.MaDangKy };
            db.KetQuaDaoTaos.Add(kq);
        }
        kq.DiemDanhGia = diem;
        kq.TyLeThamDu = tyLe;
        kq.KetQua = ketQua;
        kq.NgayDanhGia = bayGio;
        kq.GhiChu = ghiChuMoi;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (KetQuaDaKhoaException ex) { return Loi(ex.Message); }
        catch (DbUpdateException) { return Loi("Không lưu được vì dữ liệu vừa bị thay đổi. Hãy tải lại trang và thử lại."); }
        finally { db.ChoPhepSuaKetQuaDaKhoa = false; }

        return Ok($"Đã lưu: tham dự {tyLe:0.##}% ({coMat}/{tong} buổi), điểm {diem:0.##} → {KetQuaConst.Ten(ketQua)}.");
    }

    // ================= 4. Xác nhận kết quả =================

    public async Task<KetQuaXuLy> XacNhanAsync(int maKetQua, int maTaiKhoan, string? vaiTro, CancellationToken ct = default)
    {
        if (vaiTro is not ("QuanTri" or "QuanLy"))
            return Loi("Chỉ Quản trị viên hoặc Nhân sự - Đào tạo mới được xác nhận kết quả.");

        var kq = await db.KetQuaDaoTaos
            .Include(x => x.DangKyDaoTao).ThenInclude(d => d.LopDaoTao).ThenInclude(l => l.KhoaDaoTao)
            .SingleOrDefaultAsync(x => x.MaKetQua == maKetQua, ct);
        if (kq is null) return Loi("Không tìm thấy kết quả đào tạo.");
        if (kq.DaXacNhan) return Loi("Kết quả này đã được xác nhận trước đó.");
        if (kq.KetQua == KetQuaConst.ChuaDanhGia || kq.DiemDanhGia is null)
            return Loi("Kết quả chưa được đánh giá nên chưa thể xác nhận.");

        var dangKy = kq.DangKyDaoTao;
        var lop = dangKy.LopDaoTao;
        var khoa = lop.KhoaDaoTao;
        var bayGio = LopDaoTaoRules.BayGioVietNam();

        var lyDo = LyDoKhongDuDieuKienDanhGia(dangKy, lop, DateOnly.FromDateTime(bayGio));
        if (lyDo is not null) return Loi(lyDo);

        // KIỂM TRA LẠI trước khi xác nhận: điểm danh/ngưỡng có thể đã đổi từ lúc đánh giá.
        var (tong, coMat) = await DemThamDuAsync(lop.MaLop, dangKy.MaNhanVien, bayGio, ct);
        var tyLe = TinhTyLeThamDu(coMat, tong);
        var ketQua = XacDinhKetQua(tyLe, kq.DiemDanhGia, khoa.TyLeThamDuToiThieu, khoa.DiemDatToiThieu);

        if (tyLe != kq.TyLeThamDu || ketQua != kq.KetQua)
        {
            kq.TyLeThamDu = tyLe;
            kq.KetQua = ketQua;
            kq.NgayDanhGia = bayGio;
            await db.SaveChangesAsync(ct);
            return Loi($"Dữ liệu điểm danh hoặc ngưỡng đã thay đổi. Hệ thống đã tính lại: tham dự {tyLe:0.##}% → " +
                       $"{KetQuaConst.Ten(ketQua)}. Vui lòng kiểm tra rồi bấm xác nhận lại.");
        }

        kq.DaXacNhan = true;
        kq.NgayXacNhan = bayGio;
        kq.MaNguoiXacNhan = maTaiKhoan;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Loi("Không xác nhận được vì dữ liệu vừa bị thay đổi. Hãy tải lại trang."); }

        return Ok("Đã xác nhận kết quả đào tạo.");
    }
}