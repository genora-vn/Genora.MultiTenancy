/* =============================================================================
   CẤP BÙ TIỀN THƯỞNG CHƯA BAO GIỜ VÀO QUỸ — CHẠY TỪNG KHÁCH MỘT
   -----------------------------------------------------------------------------
   BỐI CẢNH: code cũ RedeemFromCampaignAsync dùng `if (customer != null)`, nên khi
   tra khách theo SĐT không thấy row AppCustomers thì lô + sổ cái vẫn được ghi mà
   BonusAmount KHÔNG hề tăng — không throw, không log. Dấu vết: bản ghi Earn có
   CustomerId = NULL và BalanceAmountAfter = 0.

   10 SĐT bị ảnh hưởng, tổng 4.400.000đ:
     0939024627  C83N1000225  1.500.000   PB2609160001
     0983149373  C82N1000186    600.000   PB2609120004
     0987248299  C7459024832   500.000    PB2609040004
     0339041399  C79N1004637   300.000    PB2609080035
     0362856233  C83N1000160   300.000    PB2609140009
     0908333349  C79N1006517   300.000    PB2609110010
     0935596555  C44N1000492   300.000    PB2609210005
     0862396989  C79N1005858   200.000    PB2609080028
     0983564293  C6813007986   200.000    PB2609090044
     0984063520  C79N1005960   200.000    PB2609160019

   KHÔNG áp dụng cho 0903008200 (dữ liệu test, anh đã xác nhận bỏ qua).

   CÁCH DÙNG: sửa DUY NHẤT dòng @Phone, chạy cả file. Script tự tra số tiền từ
   DB — không hardcode. Chạy 2 lần cho cùng 1 SĐT thì lần 2 tự bỏ qua.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @T     uniqueidentifier = '27E348A9-036C-BEF8-FA2B-3A22FC202C26';
DECLARE @Phone nvarchar(20)     = '0939024627';   -- <<< SỬA DUY NHẤT DÒNG NÀY
DECLARE @Now   datetime2(7)     = SYSDATETIME();
DECLARE @Marker nvarchar(50)    = 'BU-' + @Phone; -- khóa chống chạy trùng

/* ---------------------------------------------------------------------------
   BƯỚC 1 — Xem hiện trạng. ĐỌC KỸ TRƯỚC KHI COMMIT.
   --------------------------------------------------------------------------- */
SELECT 'HIEN TRANG' AS Buoc, c.Id AS CustomerId, c.PhoneNumber, c.CustomerCode,
       c.FullName, c.BonusAmount AS SoDuHienTai
FROM   dbo.AppCustomers c
WHERE  c.TenantId = @T AND c.IsDeleted = 0 AND c.PhoneNumber = @Phone;

-- Các bản ghi cấp thưởng CHƯA vào quỹ (CustomerId NULL + BalanceAmountAfter 0)
SELECT 'CHUA VAO QUY' AS Buoc, t.Id AS TxnId, t.CustomerCode, t.RefCode,
       t.Value AS SoTien, t.BalanceAmountAfter, t.CreationTime
FROM   HL.AppHlPointTransactions t
WHERE  t.TenantId = @T AND t.IsDeleted = 0 AND t.Type = 1 AND t.Unit = 2
  AND  t.CustomerPhone = @Phone
  AND  t.CustomerId IS NULL AND t.BalanceAmountAfter = 0;

-- Đã cấp bù trước đó chưa?
SELECT 'DA BU TRUOC DO' AS Buoc, t.Id, t.Value, t.CreationTime, t.Description
FROM   HL.AppHlPointTransactions t
WHERE  t.TenantId = @T AND t.Type = 4 AND t.RefCode = @Marker;

/* ---------------------------------------------------------------------------
   BƯỚC 2 — Ghi. Đọc kết quả BƯỚC 1 và phần KIỂM TRA ở dưới rồi mới COMMIT.
   --------------------------------------------------------------------------- */
BEGIN TRANSACTION;

DECLARE @CustomerId uniqueidentifier, @CustomerCode nvarchar(50),
        @CustomerName nvarchar(250), @SoDuTruoc decimal(18,2);

SELECT @CustomerId = c.Id, @CustomerCode = c.CustomerCode,
       @CustomerName = c.FullName, @SoDuTruoc = c.BonusAmount
FROM   dbo.AppCustomers c
WHERE  c.TenantId = @T AND c.IsDeleted = 0 AND c.PhoneNumber = @Phone;

DECLARE @SoTienBu decimal(18,2) = (
    SELECT ISNULL(SUM(t.Value), 0)
    FROM   HL.AppHlPointTransactions t
    WHERE  t.TenantId = @T AND t.IsDeleted = 0 AND t.Type = 1 AND t.Unit = 2
      AND  t.CustomerPhone = @Phone
      AND  t.CustomerId IS NULL AND t.BalanceAmountAfter = 0
);

-- Ba chốt an toàn
IF @CustomerId IS NULL
BEGIN
    RAISERROR('Khong tim thay khach hang voi SDT nay. Dung lai.', 16, 1);
    ROLLBACK TRANSACTION; RETURN;
END
IF @SoTienBu <= 0
BEGIN
    RAISERROR('Khong co khoan cap thuong nao chua vao quy cho SDT nay. Dung lai.', 16, 1);
    ROLLBACK TRANSACTION; RETURN;
END
IF EXISTS (SELECT 1 FROM HL.AppHlPointTransactions
           WHERE TenantId = @T AND Type = 4 AND RefCode = @Marker)
BEGIN
    RAISERROR('SDT nay DA duoc cap bu truoc do. Dung lai de khong bu 2 lan.', 16, 1);
    ROLLBACK TRANSACTION; RETURN;
END

-- 2a. Cộng vào quỹ
UPDATE dbo.AppCustomers
SET    BonusAmount = BonusAmount + @SoTienBu,
       LastModificationTime = @Now
WHERE  Id = @CustomerId;

DECLARE @SoDuSau decimal(18,2) =
    (SELECT BonusAmount FROM dbo.AppCustomers WHERE Id = @CustomerId);

-- 2b. Ghi sổ cái Adjust để lịch sử khớp số dư
INSERT INTO HL.AppHlPointTransactions
    (Id, TenantId, CustomerId, CustomerCode, CustomerName, CustomerPhone,
     Type, Unit, Value, BalancePointAfter, BalanceAmountAfter,
     RefCode, Description,
     ExtraProperties, ConcurrencyStamp, CreationTime, IsDeleted)
SELECT NEWID(), @T, @CustomerId, @CustomerCode, @CustomerName, @Phone,
       4,                 -- Adjust
       2,                 -- Amount
       @SoTienBu,
       (SELECT BonusPoint FROM dbo.AppCustomers WHERE Id = @CustomerId),
       @SoDuSau,
       @Marker,
       N'Cap bu tien thuong chua vao quy do loi code cu (if customer != null bo qua viec cong tien)',
       '{}', LOWER(REPLACE(CAST(NEWID() AS varchar(36)), '-', '')), @Now, 0;

-- 2c. Gán lại CustomerId cho lô + sổ cái đang NULL, để dữ liệu nhất quán
UPDATE b
SET    b.CustomerId = @CustomerId, b.LastModificationTime = @Now
FROM   HL.AppHlPointBatches b
WHERE  b.TenantId = @T AND b.IsDeleted = 0
  AND  b.CustomerPhone = @Phone AND b.CustomerId IS NULL;

UPDATE t
SET    t.CustomerId = @CustomerId, t.LastModificationTime = @Now
FROM   HL.AppHlPointTransactions t
WHERE  t.TenantId = @T AND t.IsDeleted = 0 AND t.Type = 1
  AND  t.CustomerPhone = @Phone AND t.CustomerId IS NULL;

/* ---------------------------------------------------------------------------
   KIỂM TRA — đọc rồi mới COMMIT
   --------------------------------------------------------------------------- */
SELECT 'KET QUA' AS Buoc, @Phone AS SDT, @CustomerCode AS MaKH,
       @SoDuTruoc AS SoDuTruoc, @SoTienBu AS SoTienBu, @SoDuSau AS SoDuSau,
       CASE WHEN @SoDuSau = @SoDuTruoc + @SoTienBu THEN 'OK'
            ELSE 'SAI — ROLLBACK' END AS KetLuan;

-- >>> ĐÚNG thì gõ:  COMMIT TRANSACTION;
-- >>> SAI thì gõ:   ROLLBACK TRANSACTION;

/* ---------------------------------------------------------------------------
   HOÀN TÁC (chỉ khi đã COMMIT mà cần quay lại) — sửa @Phone rồi chạy:

   DECLARE @T uniqueidentifier = '27E348A9-036C-BEF8-FA2B-3A22FC202C26';
   DECLARE @Phone nvarchar(20) = '0939024627';
   DECLARE @Marker nvarchar(50) = 'BU-' + @Phone;
   DECLARE @V decimal(18,2) = (SELECT SUM(Value) FROM HL.AppHlPointTransactions
                               WHERE TenantId=@T AND Type=4 AND RefCode=@Marker);
   UPDATE dbo.AppCustomers SET BonusAmount = BonusAmount - @V
   WHERE TenantId=@T AND IsDeleted=0 AND PhoneNumber=@Phone;
   DELETE FROM HL.AppHlPointTransactions
   WHERE TenantId=@T AND Type=4 AND RefCode=@Marker;
   --------------------------------------------------------------------------- */
