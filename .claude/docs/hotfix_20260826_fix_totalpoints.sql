/* =====================================================================================
   HOTFIX 20260826 — DATA FIX: sửa TotalPointsUsed bị GẤP ĐÔI cho bản ghi đổi quà UrBox cũ
   Bảng: HL.AppHlGiftExchanges  (SQL Server / T-SQL)
   -------------------------------------------------------------------------------------
   NGUYÊN NHÂN:
   - Code cũ (trước hotfix) tính: TotalPointsUsed = SUM(PointsRequired * Quantity).
   - Nhưng FE đã gửi PointsRequired = đơn giá * số lượng (đã nhân số lượng rồi),
     nên nhân lại Quantity ở backend → TotalPointsUsed bị gấp (Quantity) lần.
   - Bản ghi UrBox thường 1 dòng item: PointsRequired = firstItem.PointsRequired,
     TotalPointsUsed = PointsRequired * Quantity.
   GIÁ TRỊ ĐÚNG: TotalPointsUsed = PointsRequired (vì PointsRequired đã gồm số lượng).

   CÁCH NHẬN DIỆN BẢN GHI SAI (an toàn, chỉ đụng UrBox):
   - Quantity > 1                                  (Quantity = 1 thì không thể bị gấp, không cần sửa)
   - TotalPointsUsed = PointsRequired * Quantity   (đúng dấu vân tay của bug)
   - ExchangeCode LIKE 'UB-%'                       (chỉ đổi quà UrBox, tránh đụng module khác)

   LƯU Ý QUAN TRỌNG:
   - CHẠY BƯỚC 1 (SELECT) TRƯỚC để soát danh sách bản ghi sẽ sửa.
   - Backup bảng / DB trước khi chạy UPDATE trên production.
   - Bước 3 (hoàn lại BonusAmount đã trừ dư) là TÙY CHỌN và RỦI RO — đọc kỹ cảnh báo.
   ===================================================================================== */


/* ---------------------------------------------------------------------------
   BƯỚC 1 — DRY RUN: xem trước các bản ghi sẽ bị sửa (KHÔNG thay đổi dữ liệu)
   --------------------------------------------------------------------------- */
SELECT
    Id,
    ExchangeCode,
    CustomerCode,
    CustomerName,
    Quantity,
    PointsRequired,
    TotalPointsUsed                              AS TotalPointsUsed_HienTai_Sai,
    PointsRequired                               AS TotalPointsUsed_SauKhiSua_Dung,
    (TotalPointsUsed - PointsRequired)           AS ChenhLech_DaTruDu,
    CreationTime
FROM HL.AppHlGiftExchanges
WHERE Quantity > 1
  AND TotalPointsUsed = PointsRequired * Quantity
  AND ExchangeCode LIKE 'UB-%'
ORDER BY CreationTime DESC;


/* ---------------------------------------------------------------------------
   BƯỚC 2 — FIX: cập nhật TotalPointsUsed = PointsRequired (bọc transaction)
   Chạy sau khi đã kiểm tra kết quả BƯỚC 1.
   --------------------------------------------------------------------------- */
BEGIN TRANSACTION;

UPDATE HL.AppHlGiftExchanges
SET TotalPointsUsed = PointsRequired
WHERE Quantity > 1
  AND TotalPointsUsed = PointsRequired * Quantity
  AND ExchangeCode LIKE 'UB-%';

-- Kiểm tra số dòng ảnh hưởng có khớp với BƯỚC 1 không:
PRINT CONCAT(N'So dong da sua: ', @@ROWCOUNT);

-- Nếu số dòng ĐÚNG như mong đợi → COMMIT. Nếu sai → ROLLBACK.
COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION;   -- bỏ comment dòng này (và comment COMMIT ở trên) nếu muốn hủy


/* ---------------------------------------------------------------------------
   BƯỚC 3 — (TÙY CHỌN, RỦI RO) Hoàn lại BonusAmount đã bị trừ DƯ
   ---------------------------------------------------------------------------
   Bối cảnh: khi đổi quà thành công, hệ thống trừ dbo.AppCustomers.BonusAmount
   theo TotalPointsUsed. Với các bản ghi cũ bị gấp đôi, khách đã bị trừ dư =
   (TotalPointsUsed_cu - PointsRequired) = PointsRequired * (Quantity - 1).

   ⚠️ CẢNH BÁO — CHỈ chạy nếu nghiệp vụ xác nhận muốn hoàn tiền cho khách:
   - Số dư BonusAmount hiện tại có thể đã thay đổi (khách đổi thêm quà/tích thêm).
   - Chỉ nên chạy MỘT LẦN. Chạy lại sẽ cộng dư nhiều lần.
   - NÊN chạy TRƯỚC BƯỚC 2 (khi TotalPointsUsed còn giá trị cũ để tính chênh lệch),
     HOẶC tính chênh lệch = PointsRequired * (Quantity - 1) như dưới (không phụ thuộc bước 2).
   - Bước này ĐÃ comment sẵn. Bỏ comment thủ công sau khi soát kỹ.

   -- 3a. Xem trước số tiền hoàn cho từng khách:
   -- SELECT
   --     ge.CustomerCode,
   --     SUM(ge.PointsRequired * (ge.Quantity - 1)) AS SoTienHoanLai
   -- FROM HL.AppHlGiftExchanges ge
   -- WHERE ge.Quantity > 1
   --   AND ge.ExchangeCode LIKE 'UB-%'
   --   -- Điều kiện nhận diện bản ghi từng bị gấp đôi:
   --   AND (
   --        ge.TotalPointsUsed = ge.PointsRequired                       -- đã chạy BƯỚC 2 (giá trị đã đúng)
   --        OR ge.TotalPointsUsed = ge.PointsRequired * ge.Quantity      -- chưa chạy BƯỚC 2 (còn giá trị cũ)
   --       )
   -- GROUP BY ge.CustomerCode;

   -- 3b. Cộng hoàn vào BonusAmount (bọc transaction):
   -- BEGIN TRANSACTION;
   -- ;WITH refund AS (
   --     SELECT ge.CustomerCode,
   --            SUM(ge.PointsRequired * (ge.Quantity - 1)) AS Amount
   --     FROM HL.AppHlGiftExchanges ge
   --     WHERE ge.Quantity > 1
   --       AND ge.ExchangeCode LIKE 'UB-%'
   --       AND ge.TotalPointsUsed = ge.PointsRequired   -- giả định BƯỚC 2 đã chạy
   --     GROUP BY ge.CustomerCode
   -- )
   -- UPDATE c
   -- SET c.BonusAmount = c.BonusAmount + r.Amount
   -- FROM dbo.AppCustomers c
   -- INNER JOIN refund r ON r.CustomerCode = c.CustomerCode;
   -- PRINT CONCAT(N'So khach hoan BonusAmount: ', @@ROWCOUNT);
   -- COMMIT TRANSACTION;
   --------------------------------------------------------------------------- */
