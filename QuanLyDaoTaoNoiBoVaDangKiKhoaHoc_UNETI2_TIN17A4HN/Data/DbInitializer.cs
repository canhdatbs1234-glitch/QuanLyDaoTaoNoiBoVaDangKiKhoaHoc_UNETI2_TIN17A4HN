using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTaoNoiBo.Models.Entities;

namespace QuanLyDaoTaoNoiBo.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Làm sạch dữ liệu cũ và cập nhật lại
        context.Database.EnsureDeleted();
        context.Database.Migrate();

        // Toàn bộ dữ liệu mẫu được ghi trong cùng một transaction.
        using var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable);

        var phongBanMau = new[]
        {
            (Ten: "Phòng Công nghệ thông tin", MoTa: "Quản lý hạ tầng và phát triển phần mềm."),
            (Ten: "Phòng Nhân sự", MoTa: "Quản lý nhân sự và đào tạo nội bộ."),
            (Ten: "Phòng Kế toán", MoTa: "Quản lý tài chính, kế toán."),
            (Ten: "Phòng Đào tạo", MoTa: "Tổ chức và quản lý các chương trình đào tạo.")
        };
        foreach (var item in phongBanMau)
        {
            if (!context.PhongBans.Any(x => x.TenPhongBan == item.Ten))
                context.PhongBans.Add(new PhongBan { TenPhongBan = item.Ten, MoTa = item.MoTa });
        }

        var linhVucMau = new[]
        {
            (Ten: "Công nghệ thông tin", MoTa: "Lập trình, cơ sở dữ liệu và chuyển đổi số."),
            (Ten: "Kỹ năng mềm", MoTa: "Giao tiếp, làm việc nhóm và quản lý thời gian."),
            (Ten: "Quản lý và lãnh đạo", MoTa: "Quản lý đội ngũ và điều hành công việc."),
            (Ten: "Ngoại ngữ", MoTa: "Ngoại ngữ phục vụ công việc.")
        };
        foreach (var item in linhVucMau)
        {
            if (!context.LinhVucDaoTaos.Any(x => x.TenLinhVuc == item.Ten))
                context.LinhVucDaoTaos.Add(new LinhVucDaoTao { TenLinhVuc = item.Ten, MoTa = item.MoTa });
        }
        context.SaveChanges();

        // Thêm Admin
        var admin = GetOrCreateAccount(context, "admin", "Admin@123", "Quản trị viên UNETI",
            "admin@uneti.example", "QuanTri");
        context.SaveChanges();
        var phongCNTT = context.PhongBans.First(x => x.TenPhongBan == "Phòng Công nghệ thông tin");
        if (!context.NhanViens.Any(x => x.MaTaiKhoan == admin.MaTaiKhoan))
        {
            context.NhanViens.Add(new NhanVien
            {
                TaiKhoan = admin,
                PhongBan = phongCNTT,
                HoTen = admin.HoTen,
                Email = admin.Email,
                NgaySinh = new DateOnly(1990, 5, 15),
                NgayVaoLam = new DateOnly(2020, 9, 1)
            });
        }
        context.SaveChanges();

        var random = new Random(42);
        var danhSachPhongBan = context.PhongBans.ToList();

        // Thêm 10 Giảng viên
        var giangViens = new List<TaiKhoan>();
        var hoVaTenLot = new[] { "Nguyễn Văn", "Trần Thị", "Lê Hoàng", "Phạm Thu", "Hoàng Minh", "Đặng Thùy", "Vũ Quốc", "Bùi Thanh", "Đỗ Ngọc", "Ngô Gia" };
        var ten = new[] { "An", "Bình", "Châu", "Dung", "Hùng", "Hoa", "Khánh", "Linh", "Minh", "Nga" };

        for (int i = 1; i <= 10; i++)
        {
            var hoTen = $"{hoVaTenLot[random.Next(hoVaTenLot.Length)]} {ten[random.Next(ten.Length)]}";
            var gv = GetOrCreateAccount(context, $"giangvien{i}", "123456", hoTen, $"giangvien{i}@uneti.example", "GiangVien");
            giangViens.Add(gv);
        }
        context.SaveChanges();

        foreach (var gv in giangViens)
        {
            if (!context.NhanViens.Any(x => x.MaTaiKhoan == gv.MaTaiKhoan))
            {
                context.NhanViens.Add(new NhanVien
                {
                    TaiKhoan = gv,
                    PhongBan = danhSachPhongBan[random.Next(danhSachPhongBan.Count)],
                    HoTen = gv.HoTen,
                    Email = gv.Email,
                    NgaySinh = new DateOnly(1975 + random.Next(20), random.Next(1, 13), random.Next(1, 28)),
                    NgayVaoLam = new DateOnly(2022, random.Next(1, 13), random.Next(1, 28))
                });
            }
        }
        context.SaveChanges();

        // Thêm 50 Nhân viên
        var nhanViens = new List<TaiKhoan>();
        for (int i = 1; i <= 50; i++)
        {
            var hoTen = $"{hoVaTenLot[random.Next(hoVaTenLot.Length)]} {ten[random.Next(ten.Length)]} {i}";
            var nv = GetOrCreateAccount(context, $"nv{i}", "123456", hoTen, $"nv{i}@uneti.example", "NhanVien");
            nhanViens.Add(nv);
        }
        context.SaveChanges();

        foreach (var nv in nhanViens)
        {
            if (!context.NhanViens.Any(x => x.MaTaiKhoan == nv.MaTaiKhoan))
            {
                context.NhanViens.Add(new NhanVien
                {
                    TaiKhoan = nv,
                    PhongBan = danhSachPhongBan[random.Next(danhSachPhongBan.Count)],
                    HoTen = nv.HoTen,
                    Email = nv.Email,
                    NgaySinh = new DateOnly(1990 + random.Next(10), random.Next(1, 13), random.Next(1, 28)),
                    NgayVaoLam = new DateOnly(2022, random.Next(1, 13), random.Next(1, 28))
                });
            }
        }
        context.SaveChanges();

        var linhVucCNTT = context.LinhVucDaoTaos.First(x => x.TenLinhVuc == "Công nghệ thông tin");
        var linhVucKyNang = context.LinhVucDaoTaos.First(x => x.TenLinhVuc == "Kỹ năng mềm");
        var khoaLapTrinh = GetOrCreateCourse(context, "Lập trình ASP.NET Core MVC", linhVucCNTT,
            "Xây dựng ứng dụng MVC và làm việc với Entity Framework Core.", 30m);
        var khoaKyNang = GetOrCreateCourse(context, "Kỹ năng giao tiếp và làm việc nhóm", linhVucKyNang,
            "Thực hành giao tiếp hiệu quả và phối hợp nhóm trong công việc.", 12m);
        context.SaveChanges();

        // Tính ngày theo Việt Nam
        var homNay = DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime);

        for (int i = 1; i <= 25; i++)
        {
            var khoa = (i % 2 == 0) ? khoaLapTrinh : khoaKyNang;
            var tenLop = $"{khoa.TenKhoaDaoTao} - Lớp {i:D2}";
            var gvPhuTrach = giangViens[random.Next(giangViens.Count)];
            var start = homNay.AddDays(random.Next(-10, 45));
            AddClassIfMissing(context, khoa, tenLop, start, start.AddDays(30), random.Next(15, 40), $"Phòng máy A{random.Next(100, 500)}", gvPhuTrach.MaTaiKhoan);
        }
        context.SaveChanges();

        // Lấy ra lớp và nhân viên (đã lưu)
        var lopDaLuu = context.LopDaoTaos.ToList();
        var nhanVienDaLuu = context.NhanViens.Where(x => x.TaiKhoan.VaiTro == "NhanVien").ToList();

        foreach (var nv in nhanVienDaLuu)
        {
            // Mỗi nhân viên đăng ký ngẫu nhiên 3-8 lớp
            int soLopDangKy = random.Next(3, 9);
            var lopDangKy = lopDaLuu.OrderBy(x => random.Next()).Take(soLopDangKy).ToList();

            foreach (var lop in lopDangKy)
            {
                if (!context.DangKyDaoTaos.Any(x => x.MaLop == lop.MaLop && x.MaNhanVien == nv.MaNhanVien))
                {
                    context.DangKyDaoTaos.Add(new DangKyDaoTao
                    {
                        MaNhanVien = nv.MaNhanVien,
                        MaLop = lop.MaLop,
                        NgayDangKy = homNay.AddDays(-random.Next(5, 15)).ToDateTime(new TimeOnly(8, 0)),
                        NgayDuyet = homNay.AddDays(-random.Next(1, 4)).ToDateTime(new TimeOnly(10, 0)),
                        TrangThai = "DaDuyet"
                    });
                }
            }
        }
        context.SaveChanges();
        transaction.Commit();
    }

    private static TaiKhoan GetOrCreateAccount(ApplicationDbContext context, string username,
        string password, string fullName, string email, string role)
    {
        var existing = context.TaiKhoans.SingleOrDefault(x => x.TenDangNhap == username);
        if (existing is not null)
            return existing; // Không ghi đè mật khẩu, vai trò hay thông tin đã sửa.

        var account = new TaiKhoan
        {
            TenDangNhap = username,
            HoTen = fullName,
            Email = email,
            VaiTro = role
        };
        account.MatKhau = password;
        context.TaiKhoans.Add(account);
        return account;
    }

    private static KhoaDaoTao GetOrCreateCourse(ApplicationDbContext context, string name,
        LinhVucDaoTao field, string description, decimal hours)
    {
        var existing = context.KhoaDaoTaos.FirstOrDefault(x =>
            x.TenKhoaDaoTao == name && x.MaLinhVuc == field.MaLinhVuc);
        if (existing is not null)
            return existing;

        var course = new KhoaDaoTao
        {
            TenKhoaDaoTao = name,
            LinhVucDaoTao = field,
            MoTa = description,
            SoGioDuKien = hours,
            DiemDatToiThieu = 5m,
            TyLeThamDuToiThieu = 80m
        };
        context.KhoaDaoTaos.Add(course);
        return course;
    }

    private static void AddClassIfMissing(ApplicationDbContext context, KhoaDaoTao course,
        string name, DateOnly start, DateOnly end, int capacity, string location, int? maGiangVien = null)
    {
        if (context.LopDaoTaos.Any(x => x.MaKhoaDaoTao == course.MaKhoaDaoTao && x.TenLop == name))
            return;

        context.LopDaoTaos.Add(new LopDaoTao
        {
            KhoaDaoTao = course,
            TenLop = name,
            NgayBatDau = start,
            NgayKetThuc = end,
            HanDangKy = start.AddDays(-1).ToDateTime(new TimeOnly(23, 59, 59)),
            SoLuongToiDa = capacity,
            DiaDiem = location,
            TrangThai = "MoDangKy",
            MaGiangVien = maGiangVien
        });
    }
}
