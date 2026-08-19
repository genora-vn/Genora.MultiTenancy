---
name: VietQR deeplink — dùng HTTPS thay vì vietqr:// scheme
description: Zalo Mini App trả lỗi -1403 với vietqr:// scheme; phải dùng https://dl.vietqr.io/pay thay thế
type: project
originSessionId: 5f94524f-c322-4fcf-8e8e-519f0aff4a55
---
`VietQrApiClient.BuildDeeplink()` trước đây dùng scheme `vietqr://pay?app=...` để mở trực tiếp app ngân hàng từ Zalo Mini App — gây lỗi **-1403** (Zalo từ chối scheme không phải HTTP/HTTPS).

**Fix đã áp dụng (commit ed2e244):**
```csharp
// Trước (bị lỗi -1403):
return $"{DeeplinkScheme}?app={app}&ba={r.AccountNumber}&am={r.Amount}&tn={tn}&nn={nn}";

// Sau (hoạt động):
return $"https://dl.vietqr.io/pay?app={app}&ba={r.AccountNumber}&am={r.Amount}&tn={tn}";
```

Lưu ý: bỏ param `nn` (accountName) và không dùng DeeplinkScheme nữa. VietQR HTTPS link vẫn redirect sang app ngân hàng đúng cách.

**Why:** Zalo Mini App chặn custom URL schemes (deeplink) vì policy bảo mật; chỉ cho phép HTTP/HTTPS links.

**How to apply:** Bất kỳ link mở app ngân hàng trong Zalo Mini App phải dùng `https://dl.vietqr.io/pay` thay vì scheme `vietqr://`.
