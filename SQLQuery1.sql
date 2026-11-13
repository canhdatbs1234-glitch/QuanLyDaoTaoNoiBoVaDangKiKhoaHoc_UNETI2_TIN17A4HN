SELECT nv.MaNhanVien, nv.HoTen, tk.TenDangNhap
FROM DangKyDaoTao dk
JOIN NhanVien nv ON nv.MaNhanVien = dk.MaNhanVien
JOIN TaiKhoan tk ON tk.MaTaiKhoan = nv.MaTaiKhoan
WHERE dk.MaLop = (SELECT TOP 1 MaLop FROM LopDaoTao WHERE TrangThai = N'HoanThanh' ORDER BY MaLop DESC)
  AND dk.TrangThai = N'DaDuyet';