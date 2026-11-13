SET NOCOUNT ON;
DECLARE @maLop INT;

SELECT TOP 1 @maLop = dk.MaLop
FROM DangKyDaoTao dk
WHERE dk.TrangThai = N'DaDuyet'
  AND NOT EXISTS (SELECT 1 FROM BuoiHoc b WHERE b.MaLop = dk.MaLop)
GROUP BY dk.MaLop
ORDER BY COUNT(*) DESC;

IF @maLop IS NULL
BEGIN
    PRINT N'Không tìm thấy lớp phù hợp (cần lớp có đăng ký đã duyệt và chưa có buổi học).';
    RETURN;
END

DECLARE @homNay DATE = CAST(DATEADD(HOUR, 7, GETUTCDATE()) AS DATE);

-- Biến lớp thành lớp đã kết thúc cách đây 3 ngày
UPDATE LopDaoTao
SET NgayBatDau = DATEADD(DAY, -20, @homNay),
    NgayKetThuc = DATEADD(DAY, -3, @homNay),
    HanDangKy = DATEADD(DAY, -21, CAST(@homNay AS DATETIME2)),
    TrangThai = N'HoanThanh'
WHERE MaLop = @maLop;

-- 4 buổi học trong quá khứ (ĐÃ SỬA LỖI TẠI ĐÂY)
INSERT INTO BuoiHoc (MaLop, NgayHoc, GioBatDau, GioKetThuc, NoiDung, DiaDiem, TrangThai)
SELECT @maLop, DATEADD(DAY, v.[off], @homNay), '08:00', '10:00',
       N'Buổi học số ' + CAST(v.s AS NVARCHAR(5)), l.DiaDiem, N'DaDienRa'
FROM (VALUES (1, -18), (2, -14), (3, -10), (4, -6)) AS v(s, [off])
CROSS JOIN LopDaoTao l
WHERE l.MaLop = @maLop;

-- Điểm danh cho 8 học viên đầu tiên với 4 mẫu khác nhau
;WITH hv AS (
    SELECT TOP 8 MaNhanVien, ROW_NUMBER() OVER (ORDER BY MaDangKy) AS r
    FROM DangKyDaoTao
    WHERE MaLop = @maLop AND TrangThai = N'DaDuyet'
    ORDER BY MaDangKy
)
INSERT INTO DiemDanh (MaBuoiHoc, MaNhanVien, TrangThai, ThoiGianCapNhat, GhiChu)
SELECT b.MaBuoiHoc, hv.MaNhanVien,
       CASE hv.r % 4
            WHEN 1 THEN N'CoMat'                                            -- 100%
            WHEN 3 THEN N'CoMat'                                            -- 100%
            WHEN 2 THEN CASE WHEN b.NgayHoc = DATEADD(DAY, -6, @homNay)
                             THEN N'CoPhep' ELSE N'CoMat' END               -- 75%
            ELSE CASE WHEN b.MaBuoiHoc % 2 = 0 THEN N'CoMat' ELSE N'VangMat' END  -- 50%
       END,
       DATEADD(DAY, 1, CAST(b.NgayHoc AS DATETIME2)), NULL
FROM BuoiHoc b CROSS JOIN hv
WHERE b.MaLop = @maLop;

SELECT l.MaLop, l.TenLop, l.TrangThai, l.NgayBatDau, l.NgayKetThuc 
FROM LopDaoTao l 
WHERE l.MaLop = @maLop;