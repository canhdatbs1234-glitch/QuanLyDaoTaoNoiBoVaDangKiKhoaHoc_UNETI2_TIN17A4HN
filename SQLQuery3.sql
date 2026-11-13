DECLARE @homNay DATE = CAST(DATEADD(HOUR, 7, GETUTCDATE()) AS DATE);
SELECT
  (SELECT COUNT(*) FROM NhanVien WHERE TrangThai = 1)                          AS NhanVienHoatDong,
  (SELECT COUNT(*) FROM KhoaDaoTao WHERE TrangThai = 1)                        AS KhoaDangMo,
  (SELECT COUNT(*) FROM LopDaoTao WHERE TrangThai = N'MoDangKy')               AS LopMoDangKy,
  (SELECT COUNT(*) FROM Loplop_ WHERE 1=0)                                    AS _bo_qua,
  (SELECT COUNT(*) FROM DangKyDaoTao WHERE TrangThai = N'ChoDuyet')            AS DangKyChoDuyet,
  (SELECT COUNT(*) FROM DangKyDaoTao WHERE TrangThai = N'DaDuyet')             AS DangKyDaDuyet,
  (SELECT COUNT(*) FROM LopDaoTao WHERE TrangThai = N'DangHoc')                AS LopDangHoc,
  (SELECT COUNT(*) FROM LopDaoTao WHERE TrangThai = N'HoanThanh')              AS LopDaKetThuc,
  (SELECT COUNT(*) FROM KetQuaDaoTao WHERE DaXacNhan = 1 AND KetQua = N'Dat')      AS LuotHoanThanh,
  (SELECT COUNT(*) FROM KetQuaDaoTao WHERE DaXacNhan = 1 AND KetQua = N'KhongDat') AS LuotKhongHoanThanh,
  (SELECT COUNT(*) FROM KetQuaDaoTao WHERE DaXacNhan = 0 AND KetQua <> N'ChuaDanhGia') AS KetQuaChoXacNhan,
  (SELECT COUNT(*) FROM LopDaoTao
    WHERE TrangThai IN (N'ChuaMoDangKy', N'MoDangKy', N'DongDangKy')
      AND NgayBatDau > @homNay AND NgayBatDau <= DATEADD(DAY, 30, @homNay))    AS LopSapBatDau;