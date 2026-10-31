# =============================================================================
# Test luồng đổi quà UrBox sau fix — CHẠY TRÊN STAGING
# 4 ca: đủ số dư / thiếu số dư / 2 request đồng thời / SĐT lệch mã KH
#
# NGUỒN CHÂN LÝ = dbo.AppCustomers.BonusAmount (khóa theo SĐT).
# Lô (AppHlPointBatches.RemainingValue) chỉ là sổ sách đối soát, KHÔNG chặn hạn mức
# — lô cũ chưa từng bị trừ nên không phản ánh phần khách đã tiêu.
#
# KHÔNG cần backfill database. Script backfill cũ đã bị huỷ, đừng chạy.
# =============================================================================

$BaseUrl   = 'https://staging.your-host/api/mini-app/urbox'   # <-- sửa
$SqlServer = 'localhost'                                       # <-- sửa
$SqlDb     = 'Genora'                                          # <-- sửa
$TenantId  = '27E348A9-036C-BEF8-FA2B-3A22FC202C26'

# priceId thật của 1 voucher mệnh giá nhỏ (lấy từ GET $BaseUrl/gifts)
$PriceId   = '13058'      # <-- sửa: Bách Hóa Xanh 100.000
$PriceVal  = 100000       # <-- sửa cho khớp mệnh giá trên

# Chọn từ staging:
#   Rich = BonusAmount >= $PriceVal * 2   (đủ cho ca 1 và ca 3)
#   Poor = BonusAmount = 0
#   Skew = BonusAmount >= $PriceVal VÀ batch.CustomerCode <> AppCustomers.CustomerCode
$PhoneRich = '0xxxxxxxxx'; $CodeRich = 'Cxxxxxxxxx'   # <-- sửa
$PhonePoor = '0xxxxxxxxx'; $CodePoor = 'Cxxxxxxxxx'   # <-- sửa
$PhoneSkew = '0xxxxxxxxx'; $CodeSkew = 'Cxxxxxxxxx'   # <-- sửa (mã trong LÔ, không phải mã trong AppCustomers)

# -----------------------------------------------------------------------------
function Invoke-Sql([string]$Query) {
    Invoke-Sqlcmd -ServerInstance $SqlServer -Database $SqlDb -Query $Query -TrustServerCertificate
}

# Trả về số dư thật (BonusAmount) + số lô để tham khảo
function Get-Balance([string]$Phone) {
    Invoke-Sql @"
SELECT ISNULL(c.BonusAmount,0) AS SoDu,
       (SELECT ISNULL(SUM(b.RemainingValue),0) FROM HL.AppHlPointBatches b
        WHERE b.TenantId='$TenantId' AND b.IsDeleted=0 AND b.Unit=2
          AND b.CustomerPhone='$Phone' AND b.Status=1 AND b.ExpireDate>GETDATE()) AS LoThamKhao
FROM dbo.AppCustomers c
WHERE c.TenantId='$TenantId' AND c.IsDeleted=0 AND c.PhoneNumber='$Phone';
"@
}

function Get-Ledger([string]$Phone) {
    Invoke-Sql @"
SELECT TOP 3 Type, Value, BalanceAmountAfter, RefCode, CreationTime
FROM HL.AppHlPointTransactions
WHERE TenantId='$TenantId' AND CustomerPhone='$Phone'
ORDER BY CreationTime DESC;
"@
}

function Get-LastExchange([string]$Phone) {
    Invoke-Sql @"
SELECT TOP 1 ExchangeCode, Status, AmountUsed, TotalPointsUsed, UrBoxVoucherCode,
       CASE WHEN UrBoxResponse IS NULL THEN 'KHONG GOI URBOX' ELSE 'DA GOI URBOX' END AS GoiUrBox,
       InternalNote, CreationTime
FROM HL.AppHlGiftExchanges
WHERE TenantId='$TenantId' AND CustomerPhone='$Phone'
ORDER BY CreationTime DESC;
"@
}

function Invoke-Redeem([string]$Code, [string]$Phone, [string]$ReqId = $null) {
    $body = @{
        siteUserId   = $Code
        phone        = $Phone
        customerName = 'Test KH'
        rank         = ''
        items        = @(@{
            priceId        = $PriceId
            quantity       = 1
            giftName       = 'Test voucher'
            pointsRequired = $PriceVal
        })
    }
    if ($ReqId) { $body.requestId = $ReqId }

    try {
        Invoke-RestMethod -Uri "$BaseUrl/redeem" -Method Post `
            -ContentType 'application/json' -Body ($body | ConvertTo-Json -Depth 5)
    } catch {
        [pscustomobject]@{ done = -1; msg = $_.Exception.Message }
    }
}

function Show([string]$T) { Write-Host "`n=== $T ===" -ForegroundColor Cyan }
function Pass([string]$M) { Write-Host "PASS — $M" -ForegroundColor Green }
function Fail([string]$M) { Write-Host "FAIL — $M" -ForegroundColor Red }

# =============================================================================
Show "CA 1 — Đủ số dư: thành công, BonusAmount giảm đúng $PriceVal"
$b1 = Get-Balance $PhoneRich
Write-Host "Trước:  BonusAmount=$($b1.SoDu)  (lô tham khảo=$($b1.LoThamKhao))"
$r1 = Invoke-Redeem $CodeRich $PhoneRich
Write-Host "Kết quả: done=$($r1.done) status=$($r1.status) msg=$($r1.msg)"
$a1 = Get-Balance $PhoneRich
$d1 = $b1.SoDu - $a1.SoDu
Write-Host "Sau:    BonusAmount=$($a1.SoDu)  đã trừ=$d1"
if ($r1.done -eq 1 -and $d1 -eq $PriceVal) { Pass "trừ đúng $d1" }
else { Fail "kỳ vọng done=1 và trừ $PriceVal, thực tế done=$($r1.done) trừ=$d1" }
Write-Host "Sổ cái:"; Get-Ledger $PhoneRich | Format-Table -AutoSize

# =============================================================================
Show "CA 2 — Thiếu số dư: bị chặn, UrBox KHÔNG được gọi"
$b2 = Get-Balance $PhonePoor
Write-Host "Trước:  BonusAmount=$($b2.SoDu)"
$r2 = Invoke-Redeem $CodePoor $PhonePoor
Write-Host "Kết quả: done=$($r2.done) status=$($r2.status) msg=$($r2.msg)"
$a2 = Get-Balance $PhonePoor
$e2 = Get-LastExchange $PhonePoor
$e2 | Format-List
if ($r2.done -ne 1 -and $r2.status -eq 403 -and $a2.SoDu -eq $b2.SoDu `
    -and $e2.GoiUrBox -eq 'KHONG GOI URBOX' -and -not $e2.UrBoxVoucherCode) {
    Pass "chặn ở server, số dư không đổi, voucher không phát hành"
} else {
    Fail "kỳ vọng status=403, số dư không đổi, KHONG GOI URBOX, không có voucher code"
}

# =============================================================================
Show "CA 3 — Hai request đồng thời: không double-spend"
$b3 = Get-Balance $PhoneRich
Write-Host "Trước:  BonusAmount=$($b3.SoDu)"
# requestId KHÁC nhau → test lock, không phải test idempotency
$jobs = 1..2 | ForEach-Object {
    Start-ThreadJob -ScriptBlock {
        param($u, $c, $p, $pid2, $pv, $rid)
        $b = @{ siteUserId=$c; phone=$p; customerName='Test KH'; requestId=$rid
                items=@(@{ priceId=$pid2; quantity=1; pointsRequired=$pv }) }
        try {
            Invoke-RestMethod -Uri "$u/redeem" -Method Post -ContentType 'application/json' `
                -Body ($b | ConvertTo-Json -Depth 5)
        } catch { [pscustomobject]@{ done=-1; msg=$_.Exception.Message } }
    } -ArgumentList $BaseUrl, $CodeRich, $PhoneRich, $PriceId, $PriceVal, ([guid]::NewGuid().ToString())
}
$res3 = $jobs | Wait-Job | Receive-Job
$res3 | ForEach-Object { Write-Host "  done=$($_.done) status=$($_.status) msg=$($_.msg)" }
$jobs | Remove-Job
$a3 = Get-Balance $PhoneRich
$d3 = $b3.SoDu - $a3.SoDu
$ok3 = ($res3 | Where-Object { $_.done -eq 1 }).Count
Write-Host "Sau: BonusAmount=$($a3.SoDu)  đã trừ=$d3  số request thành công=$ok3"
if ($d3 -eq ($ok3 * $PriceVal)) { Pass "trừ $d3 cho $ok3 lần thành công — khớp, không double-spend" }
else { Fail "DOUBLE-SPEND: trừ $d3 nhưng có $ok3 lần thành công" }

# =============================================================================
Show "CA 4 — SĐT lệch mã KH (nguyên nhân gốc sự cố): phải trừ được"
Invoke-Sql @"
SELECT DISTINCT b.CustomerCode AS MaTrongLo, c.CustomerCode AS MaTrongAppCustomers, c.BonusAmount
FROM HL.AppHlPointBatches b
JOIN dbo.AppCustomers c ON c.TenantId='$TenantId' AND c.PhoneNumber=b.CustomerPhone
WHERE b.TenantId='$TenantId' AND b.CustomerPhone='$PhoneSkew' AND b.Unit=2;
"@ | Format-Table -AutoSize

$b4 = Get-Balance $PhoneSkew
Write-Host "Trước:  BonusAmount=$($b4.SoDu)"
$r4 = Invoke-Redeem $CodeSkew $PhoneSkew
Write-Host "Kết quả: done=$($r4.done) status=$($r4.status) msg=$($r4.msg)"
$a4 = Get-Balance $PhoneSkew
$d4 = $b4.SoDu - $a4.SoDu
Write-Host "Sau:    BonusAmount=$($a4.SoDu)  đã trừ=$d4"
if ($r4.done -eq 1 -and $d4 -eq $PriceVal) { Pass "RC3 đã đóng — lệch mã KH vẫn trừ đúng" }
else { Fail "RC3 CHƯA ĐÓNG hoặc không đủ số dư. done=$($r4.done) trừ=$d4" }

# =============================================================================
Show "BẤT BIẾN TOÀN HỆ THỐNG — cả 3 query PHẢI rỗng"

Invoke-Sql @"
-- (A) Số dư âm
SELECT 'SO DU AM' AS Loi, PhoneNumber, CustomerCode, BonusAmount
FROM dbo.AppCustomers
WHERE TenantId='$TenantId' AND IsDeleted=0 AND (BonusAmount < 0 OR BonusPoint < 0);
"@ | Format-Table -AutoSize

Invoke-Sql @"
-- (B) Đổi quà thành công mà thiếu bút toán Spend (RC4 phải đã đóng)
SELECT 'THIEU SO CAI' AS Loi, ge.ExchangeCode, ge.CustomerPhone, ge.AmountUsed
FROM HL.AppHlGiftExchanges ge
WHERE ge.TenantId='$TenantId' AND ge.Status IN (1,3) AND ge.AmountUsed > 0
  AND ge.CreationTime > DATEADD(HOUR,-1,GETDATE())
  AND NOT EXISTS (SELECT 1 FROM HL.AppHlPointTransactions t
                  WHERE t.TenantId='$TenantId' AND t.Type=2 AND t.RefCode=ge.ExchangeCode);
"@ | Format-Table -AutoSize

Invoke-Sql @"
-- (C) Tổng đã đổi quà > tổng được cấp (vượt hạn mức) — tính từ lúc fix chạy
WITH g AS (SELECT CustomerPhone AS Phone, SUM(ConvertedValue) AS Granted
           FROM HL.AppHlPointBatches
           WHERE TenantId='$TenantId' AND IsDeleted=0 AND Unit=2 GROUP BY CustomerPhone),
     s AS (SELECT CustomerPhone AS Phone, SUM(AmountUsed) AS Spent
           FROM HL.AppHlGiftExchanges
           WHERE TenantId='$TenantId' AND Status IN (1,3) AND IsDeleted=0
             AND CreationTime > DATEADD(HOUR,-1,GETDATE())
           GROUP BY CustomerPhone)
SELECT 'VUOT HAN MUC' AS Loi, s.Phone, g.Granted, s.Spent
FROM s JOIN g ON g.Phone = s.Phone
WHERE s.Spent > g.Granted;
"@ | Format-Table -AutoSize

Write-Host "`nXong. Ba bảng bất biến ở trên PHẢI rỗng." -ForegroundColor Yellow
