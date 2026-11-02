/* =============================================================================
   GIÁM SÁT HÀNG NGÀY — ĐỔI QUÀ HOA LINH
   CÓ DÒNG TRẢ VỀ = CÓ VẤN ĐỀ MỚI. Rỗng = bình thường.

   Cơ sở: BonusAmount = Σ cấp + Σ hoàn − Σ đã đổi.
   Tính cả giao dịch xóa mềm, vì code không hoàn quỹ khi admin xóa mềm.

   BẢO TRÌ: cấp bù cho SĐT nào (xem 20261031_hl_bu_tien_thuong_chua_vao_quy.sql)
   thì XOÁ dòng đó khỏi @Baseline. Để lại thì query sẽ kêu đúng SĐT đó — nó tự
   nhắc cập nhật chứ không im lặng bỏ qua.
   ============================================================================= */

DECLARE @T uniqueidentifier = '27E348A9-036C-BEF8-FA2B-3A22FC202C26';

-- 18 SĐT đã điều tra xong và chấp nhận giữ nguyên, kèm độ lệch đã biết.
DECLARE @Baseline TABLE (Phone nvarchar(20) PRIMARY KEY, LechDaBiet decimal(18,2));
INSERT @Baseline VALUES
 -- 7 ca vượt hạn mức đã xử lý (BonusAmount clamp ở 0 nên lệch dương)
 ('0905556117', 5000000), ('0987380790', 2000000), ('0986172373',  600000),
 ('0369248492',  400000), ('0933856863',  400000), ('0838172900',  200000),
 ('0937782277',  200000),
 -- 11 ca tiền thưởng chưa vào quỹ (lệch âm), giữ nguyên chờ khách khiếu nại
 ('0939024627',-1500000), ('0983149373', -600000), ('0903008200', -599998),
 ('0987248299', -500000), ('0339041399', -300000), ('0362856233', -300000),
 ('0908333349', -300000), ('0935596555', -300000), ('0862396989', -200000),
 ('0983564293', -200000), ('0984063520', -200000);

WITH grant_ AS (
    SELECT CustomerPhone AS Phone,
           SUM(CASE WHEN Type = 1 THEN Value ELSE 0 END) AS Granted,
           SUM(CASE WHEN Type = 4 THEN Value ELSE 0 END) AS Adjusted
    FROM   HL.AppHlPointTransactions
    WHERE  TenantId = @T AND IsDeleted = 0 AND Unit = 2 AND CustomerPhone IS NOT NULL
    GROUP BY CustomerPhone
),
spend AS (
    SELECT CustomerPhone AS Phone,
           SUM(CASE WHEN AmountUsed > 0 THEN AmountUsed
                    ELSE CAST(TotalPointsUsed AS decimal(18,2)) END) AS SpentAll,
           COUNT(*) AS TxnCount
    FROM   HL.AppHlGiftExchanges
    WHERE  TenantId = @T AND Status IN (1, 3) AND CustomerPhone IS NOT NULL
    GROUP BY CustomerPhone
),
j AS (
    SELECT COALESCE(g.Phone, s.Phone) AS Phone,
           ISNULL(g.Granted,0) + ISNULL(g.Adjusted,0) AS TongCap,
           ISNULL(s.SpentAll,0) AS DaDoi,
           ISNULL(s.TxnCount,0) AS SoGD
    FROM grant_ g FULL OUTER JOIN spend s ON s.Phone = g.Phone
)
-- (A) VƯỢT HẠN MỨC MỚI
SELECT 'A. VUOT HAN MUC MOI' AS Loi, j.Phone, c.CustomerCode,
       j.TongCap, j.DaDoi, j.DaDoi - j.TongCap AS Lech,
       ISNULL(c.BonusAmount,0) AS SoDu, j.SoGD,
       N'Guard bi vong qua — xem ngay' AS XuLy
FROM   j
LEFT JOIN dbo.AppCustomers c ON c.TenantId=@T AND c.IsDeleted=0 AND c.PhoneNumber=j.Phone
WHERE  j.DaDoi > j.TongCap
  AND  j.Phone NOT IN (SELECT Phone FROM @Baseline)

UNION ALL
-- (B) LỆCH SỔ SÁCH MỚI
SELECT 'B. LECH SO SACH MOI', j.Phone, c.CustomerCode,
       j.TongCap, j.DaDoi, c.BonusAmount - (j.TongCap - j.DaDoi),
       c.BonusAmount, j.SoGD,
       N'Co bug moi, chua gay thiet hai — xem trong ngay'
FROM   j
JOIN   dbo.AppCustomers c ON c.TenantId=@T AND c.IsDeleted=0 AND c.PhoneNumber=j.Phone
LEFT JOIN @Baseline b ON b.Phone = j.Phone
WHERE  c.BonusAmount - (j.TongCap - j.DaDoi) <> ISNULL(b.LechDaBiet, 0)

UNION ALL
-- (C) SỐ DƯ ÂM
SELECT 'C. SO DU AM', c.PhoneNumber, c.CustomerCode,
       NULL, NULL, c.BonusAmount, c.BonusAmount, NULL,
       N'Guard bi bo qua — rollback'
FROM   dbo.AppCustomers c
WHERE  c.TenantId=@T AND c.IsDeleted=0 AND (c.BonusAmount < 0 OR c.BonusPoint < 0)

UNION ALL
-- (D) THIẾU BÚT TOÁN: đổi quà thành công 24h qua mà không có bút toán trừ
SELECT 'D. THIEU BUT TOAN', ge.CustomerPhone, ge.CustomerCode,
       NULL, ge.AmountUsed, NULL, NULL, NULL,
       N'SpendAsync bi bo qua — ROLLBACK NGAY'
FROM   HL.AppHlGiftExchanges ge
WHERE  ge.TenantId=@T AND ge.Status IN (1,3) AND ge.AmountUsed > 0
  AND  ge.CreationTime > DATEADD(DAY,-1,GETDATE())
  AND  NOT EXISTS (SELECT 1 FROM HL.AppHlPointTransactions t
                   WHERE t.TenantId=@T AND t.Type=2 AND t.RefCode=ge.ExchangeCode)
ORDER BY Loi, Lech DESC;

/* -----------------------------------------------------------------------------
   XÁC NHẬN TÍCH CỰC — chạy sau giao dịch đổi quà ĐẦU TIÊN hậu deploy.
   Query giám sát ở trên rỗng chỉ chứng minh "chưa có gì sai", không chứng minh
   "fix đang chạy". Bảng dưới mới là bằng chứng SpendAsync thật sự hoạt động.
   ----------------------------------------------------------------------------- */
DECLARE @FixFrom datetime2 = '2026-11-02';   -- ngày deploy

SELECT ge.CreationTime, ge.ExchangeCode, ge.CustomerPhone, ge.CustomerCode,
       ge.GiftName, ge.AmountUsed, ge.TotalPointsUsed, ge.Status,
       ge.RequestId,
       t.Value           AS ButToanTru,      -- phải = -AmountUsed
       t.BalanceAmountAfter AS SoDuSauKhiTru,
       c.BonusAmount     AS SoDuHienTai,     -- phải = BalanceAmountAfter
       CASE WHEN t.Id IS NULL                              THEN 'FAIL — khong co but toan tru'
            WHEN t.Value <> -ge.AmountUsed                 THEN 'FAIL — so tien tru khong khop'
            WHEN c.BonusAmount <> t.BalanceAmountAfter      THEN 'FAIL — so du lech but toan'
            ELSE 'OK' END AS KetLuan
FROM   HL.AppHlGiftExchanges ge
LEFT JOIN HL.AppHlPointTransactions t
       ON t.TenantId=@T AND t.Type=2 AND t.RefCode=ge.ExchangeCode
LEFT JOIN dbo.AppCustomers c
       ON c.TenantId=@T AND c.IsDeleted=0 AND c.PhoneNumber=ge.CustomerPhone
WHERE  ge.TenantId=@T AND ge.CreationTime >= @FixFrom
ORDER BY ge.CreationTime DESC;
