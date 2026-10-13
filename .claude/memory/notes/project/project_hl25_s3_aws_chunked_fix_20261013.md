# HL25/HLG — Fix S3 `Content-Encoding: aws-chunked` (ảnh không đọc được) — 2026-10-13

## Bối cảnh
- Branch `hotfix/20261013`. Nối tiếp task S3 Storage Migration (đã merge vào `dev`, LIVE).
- API `GET /api/mini-app/hl25/me/frames?zaloUserId=...` trả `resultImageUrl` là URL S3 trực tiếp
  (VD `https://s3.ods.vn/c01-hoalinh-prod-01/uploads/hl25/{tenant}/{zalo}_{timestamp}.jpg`).
- URL trả **200 OK nhưng kèm header `Content-Encoding: aws-chunked`** → browser/Zalo mini app và
  `curl --compressed` cố giải nén theo encoding "aws-chunked" không hợp lệ → lỗi
  `(61) Unrecognized content encoding type` → không hiển thị được ảnh.

## Root cause
- `AWSSDK.S3` version **4.0.104.2** (SDK .NET **V4**; AWSSDK.Core 4.0.102.9).
- SDK V4 **bật mặc định flexible-checksum (CRC32)** cho mọi upload, stream checksum qua
  **`aws-chunked` content-encoding** (`RequestChecksumCalculation` mặc định = `WHEN_SUPPORTED`).
- Trên AWS thật header này bị strip khi GET → vô hại. Trên **S3-compatible (ods.vn)** store
  **lưu luôn `Content-Encoding: aws-chunked`** vào metadata object → trả lại khi GET → vỡ ở client.
- `DisablePayloadSigning = true` (đã set sẵn) **chỉ tắt SigV4 payload signing, KHÔNG tắt checksum/chunked**
  → vẫn lỗi. Đây là bẫy chính.

## Fix (chỉ code, KHÔNG remediate ảnh cũ — theo quyết định user)
1. **`MultiTenancyApplicationModule.cs`** (`AmazonS3Config`, DI singleton `IAmazonS3`): thêm
   - `RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED`
   - `ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED`
   - (enum trong `Amazon.Runtime`, SDK V4 dùng SCREAMING_CASE `WHEN_REQUIRED`, KHÔNG phải `WhenRequired`).
   - → PutObject hết `aws-chunked`. Áp dụng cả hl25 + hlg (dùng chung client).
2. **`S3ImageStorageProvider.cs`** (`CopyAsync`, dùng cho rename ảnh thiệp theo ZaloUserId+timestamp):
   - `MetadataDirective = S3MetadataDirective.REPLACE` + `ContentType = GuessContentType(destKey)`,
     **không set** `ContentEncoding` → object copy/rename sạch kể cả khi nguồn cũ còn `aws-chunked`.
   - Thêm helper `GuessContentType` (map đuôi file → image/jpeg|png|gif|webp, video/mp4).

## KHÔNG đụng
- Cấu trúc response API, `ToFullUrl` (idempotent, giữ nguyên URL S3 tuyệt đối), DTO, controller,
  `me/frames`, Local provider. Task yêu cầu "API đã đúng, chỉ sửa đường dẫn ảnh".

## Verify
- Build HttpApi + Application: **0 errors** (chỉ warnings pre-existing).
- `dotnet test` filter AppImages|Hl25|Hlg: **196/196 PASS, 0 failed** (tests mock `IAmazonS3`, không vỡ
  khi thêm field vào PutObjectRequest/CopyObjectRequest).
- **Chưa deploy/commit.** Ảnh cũ (upload từ ~09/10) vẫn còn aws-chunked — user test bằng ảnh TẠO MỚI.

## Cách verify thủ công sau deploy
- Tạo thiệp mới qua mini app → gọi `me/frames` → mở `resultImageUrl`.
- `curl -I "<url>"` → KHÔNG còn `Content-Encoding: aws-chunked`.
- `curl --compressed -o NUL "<url>"` → không còn lỗi (61).

## CURL test API (response không đổi)
```bash
curl -s "https://duocpham-hoalinh.genora.vn/api/mini-app/hl25/me/frames?zaloUserId=7974578522474094620"
# Local: curl -s "https://localhost:44374/api/mini-app/hl25/me/frames?zaloUserId=7974578522474094620"
```
