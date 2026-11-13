// Họ và tên: Phạm Việt Anh
// Mã sinh viên: 23103100231
// Nội dung thực hiện: Module 4 - Buổi học, Lịch học và Điểm danh
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Data;
using QuanLyDaoTaoNoiBo.Models.Entities;
using QuanLyDaoTaoNoiBo.Models.ViewModels;

namespace QuanLyDaoTaoNoiBo.Services;

public record DiemDanhResult(bool Success, string Message);

public class DiemDanhService(ApplicationDbContext db, TimeProvider clock)
{
    public DateTime Now => clock.GetUtcNow().ToOffset(TimeSpan.FromHours(7)).DateTime;

    public async Task<DiemDanhViewModel?> Load(int accountId, int lessonId)
    {
        var lesson = await db.BuoiHocs.AsNoTracking().Include(x => x.LopDaoTao)
            .SingleOrDefaultAsync(x => x.MaBuoiHoc == lessonId);
        if (lesson is null) return null;
        if (!await IsManager(accountId) && lesson.LopDaoTao.MaGiangVien != accountId) return null;
        var rows = await db.DangKyDaoTaos.AsNoTracking()
            .Where(x => x.MaLop == lesson.MaLop && x.TrangThai == "DaDuyet")
            .OrderBy(x => x.NhanVien.HoTen).ThenBy(x => x.MaNhanVien)
            .Select(x => new DiemDanhHocVienViewModel
            {
                MaNhanVien = x.MaNhanVien,
                HoTen = x.NhanVien.HoTen,
                TenPhongBan = x.NhanVien.PhongBan.TenPhongBan
            }).ToListAsync();
        var existing = await db.DiemDanhs.AsNoTracking().Where(x => x.MaBuoiHoc == lessonId).ToDictionaryAsync(x => x.MaNhanVien);
        foreach (var row in rows)
        {
            if (!existing.TryGetValue(row.MaNhanVien, out var mark)) continue;
            // DiMuon là mã cũ, người phụ trách phải chọn lại một trong 3 trạng thái mới.
            row.TrangThai = mark.TrangThai == "DiMuon" ? "" : mark.TrangThai;
            row.GhiChu = mark.GhiChu;
            row.ThoiGianCapNhat = mark.ThoiGianCapNhat;
        }
        var error = LessonError(lesson);
        if (await db.KetQuaDaoTaos.AnyAsync(x => x.DangKyDaoTao.MaLop == lesson.MaLop && x.NgayDanhGia != null))
            error = "Lớp đã khóa kết quả, không thể sửa điểm danh.";
        return new DiemDanhViewModel
        {
            MaBuoiHoc = lessonId,
            MaLop = lesson.MaLop,
            TenLop = lesson.LopDaoTao.TenLop,
            NgayHoc = lesson.NgayHoc,
            GioBatDau = lesson.GioBatDau,
            GioKetThuc = lesson.GioKetThuc,
            NoiDung = lesson.NoiDung,
            HocViens = rows,
            ChoPhepLuu = error is null && rows.Count > 0,
            LyDoKhongChoLuu = error ?? (rows.Count == 0 ? "Lớp chưa có học viên được duyệt." : null)
        };
    }

    public async Task<DiemDanhResult> Save(int accountId, DiemDanhViewModel model)
    {
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var lesson = await db.BuoiHocs.Include(x => x.LopDaoTao).SingleOrDefaultAsync(x => x.MaBuoiHoc == model.MaBuoiHoc);
            if (lesson is null) return new(false, "Buổi học không tồn tại.");
            if (!await IsManager(accountId) && lesson.LopDaoTao.MaGiangVien != accountId) return new(false, "Không có quyền điểm danh.");
            var error = LessonError(lesson);
            if (error is not null) return new(false, error);
            var approved = await db.DangKyDaoTaos.Where(x => x.MaLop == lesson.MaLop && x.TrangThai == "DaDuyet")
                .Select(x => x.MaNhanVien).ToListAsync();
            if (model.HocViens is null || approved.Count == 0 ||
                model.HocViens.Count != approved.Count ||
                model.HocViens.Select(x => x.MaNhanVien).Distinct().Count() != model.HocViens.Count ||
                !approved.ToHashSet().SetEquals(model.HocViens.Select(x => x.MaNhanVien)))
                return new(false, "Danh sách học viên không hợp lệ hoặc vừa thay đổi. Hãy tải lại trang.");
            if (model.HocViens.Any(x => !DiemDanhHocVienViewModel.TrangThais.ContainsKey(x.TrangThai ?? "") || x.GhiChu?.Length > 1000))
                return new(false, "Chọn đủ trạng thái hợp lệ; ghi chú tối đa 1000 ký tự.");
            var marks = await db.DiemDanhs.Where(x => x.MaBuoiHoc == lesson.MaBuoiHoc).ToDictionaryAsync(x => x.MaNhanVien);
            // Kiểm tra tất cả trước khi thay đổi bất kỳ bản ghi nào.
            foreach (var row in model.HocViens)
            {
                var expected = marks.GetValueOrDefault(row.MaNhanVien)?.ThoiGianCapNhat;
                if (expected != row.ThoiGianCapNhat)
                    return new(false, "Điểm danh đã được người khác cập nhật. Hãy tải lại trước khi lưu.");
            }
            var updatedAt = Now;
            foreach (var row in model.HocViens)
            {
                if (!marks.TryGetValue(row.MaNhanVien, out var mark))
                {
                    mark = new DiemDanh { MaBuoiHoc = lesson.MaBuoiHoc, MaNhanVien = row.MaNhanVien };
                    db.DiemDanhs.Add(mark);
                }
                mark.TrangThai = row.TrangThai; mark.GhiChu = row.GhiChu?.Trim();
                // Giữ phiên bản tăng ngay cả khi đồng hồ trả về cùng một thời điểm.
                mark.ThoiGianCapNhat = mark.ThoiGianCapNhat >= updatedAt ? mark.ThoiGianCapNhat.AddTicks(1) : updatedAt;
            }
            if (lesson.NgayHoc.ToDateTime(lesson.GioKetThuc) <= updatedAt)
                lesson.TrangThai = "DaDienRa";
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new(true, "Đã lưu điểm danh cho toàn bộ học viên.");
        }
        catch (Exception ex) when (Conflict(ex))
        {
            db.ChangeTracker.Clear();
            return new(false, "Dữ liệu đang được cập nhật đồng thời. Hãy tải lại và thử lại.");
        }
    }

    public async Task<BaoCaoThamGiaViewModel?> Report(int accountId, int classId)
    {
        var account = await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(x => x.MaTaiKhoan == accountId && x.TrangThai);
        if (account is null) return null;
        var manager = account.VaiTro == "QuanTri";
        var lop = await db.LopDaoTaos.AsNoTracking().SingleOrDefaultAsync(x => x.MaLop == classId);
        if (lop is null) return null;
        var giangVien = account.VaiTro == "GiangVien" && lop.MaGiangVien == accountId;
        if (!manager && !giangVien && (account.VaiTro != "NhanVien" || !await db.DangKyDaoTaos.AnyAsync(x =>
            x.MaLop == classId && x.TrangThai == "DaDuyet" && x.NhanVien.MaTaiKhoan == accountId))) return null;
        var now = Now; var today = DateOnly.FromDateTime(now); var time = TimeOnly.FromDateTime(now);
        var all = db.BuoiHocs.AsNoTracking().Where(x => x.MaLop == classId);
        var total = await all.CountAsync();
        var valid = await all.Where(x => lop.TrangThai != "DaHuy" && x.TrangThai != "DaHuy" &&
            (x.NgayHoc < today || (x.NgayHoc == today && x.GioKetThuc <= time)))
            .OrderBy(x => x.NgayHoc).ThenBy(x => x.GioBatDau).ToListAsync();
        var peopleQuery = db.DangKyDaoTaos.AsNoTracking().Where(x => x.MaLop == classId && x.TrangThai == "DaDuyet");
        if (!manager) peopleQuery = peopleQuery.Where(x => x.NhanVien.MaTaiKhoan == accountId);
        var people = await peopleQuery.OrderBy(x => x.NhanVien.HoTen)
            .Select(x => new { x.MaNhanVien, x.NhanVien.HoTen }).ToListAsync();
        var ids = valid.Select(x => x.MaBuoiHoc).ToArray();
        var personIds = people.Select(x => x.MaNhanVien).ToArray();
        var marks = await db.DiemDanhs.AsNoTracking().Where(x => ids.Contains(x.MaBuoiHoc) && personIds.Contains(x.MaNhanVien))
            .ToDictionaryAsync(x => (x.MaNhanVien, x.MaBuoiHoc));
        var results = people.Select(p =>
        {
            var details = valid.Select(b =>
            {
                var mark = marks.GetValueOrDefault((p.MaNhanVien, b.MaBuoiHoc));
                return new ChiTietThamGiaViewModel
                {
                    NgayHoc = b.NgayHoc,
                    GioBatDau = b.GioBatDau,
                    NoiDung = b.NoiDung,
                    TrangThai = mark?.TrangThai,
                    ThoiGianCapNhat = mark?.ThoiGianCapNhat,
                    GhiChu = mark?.GhiChu
                };
            }).ToList();
            return new ThamGiaNhanVienViewModel
            {
                MaNhanVien = p.MaNhanVien,
                HoTen = p.HoTen,
                TongSoBuoiHopLe = valid.Count,
                SoBuoiCoMat = details.Count(x => x.TrangThai == "CoMat"),
                SoBuoiVangCoPhep = details.Count(x => x.TrangThai == "CoPhep"),
                SoBuoiVangKhongPhep = details.Count(x => x.TrangThai == "VangMat"),
                SoBuoiDiMuon = details.Count(x => x.TrangThai == "DiMuon"),
                SoBuoiChuaDiemDanh = details.Count(x => x.TrangThai is null),
                ChiTiet = details
            };
        }).ToList();
        return new BaoCaoThamGiaViewModel { MaLop = classId, TenLop = lop.TenLop, TongSoBuoi = total, TongSoBuoiHopLe = valid.Count, NhanViens = results };
    }

    private Task<bool> IsManager(int id) => db.TaiKhoans.AnyAsync(x => x.MaTaiKhoan == id && x.TrangThai &&
        x.VaiTro == "QuanTri");
    private string? LessonError(BuoiHoc lesson)
    {
        if (lesson.TrangThai == "DaHuy" || lesson.LopDaoTao.TrangThai == "DaHuy") return "Buổi học hoặc lớp đã hủy.";
        if (lesson.NgayHoc.ToDateTime(lesson.GioBatDau) > Now) return "Chỉ điểm danh khi buổi học đã bắt đầu.";
        return null;
    }
    private static bool Conflict(Exception ex) => ex is KetQuaDaKhoaException or DbUpdateConcurrencyException ||
        ex is SqlException { Number: 1205 or 1222 or 2601 or 2627 or 547 } ||
        (ex.InnerException is not null && Conflict(ex.InnerException));
}