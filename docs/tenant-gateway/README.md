# Tenant Mini-App Gateway — Cấu hình rate-limit production

> Tài liệu triển khai **rate-limit lưu lượng mini-app** cho các tenant (Dược phẩm Hoa Linh, Hoa Linh Miền Nam) trên production.
> Trạng thái: **ĐANG CHẠY** (đã verify 2026-09-23: burst test trả hỗn hợp 200 + 429).

## ⚠️ Điều quan trọng nhất phải nhớ

**Ingress trên production là XAMPP Apache, KHÔNG phải IIS.** Cổng `103.157.218.191:443` do `C:\xampp\apache\bin\httpd.exe` giữ. Trước khi debug bất cứ điều gì về routing/rate-limit, LUÔN kiểm ai giữ cổng 443:

```powershell
Get-NetTCPConnection -LocalPort 443 -State Listen | Select-Object LocalAddress, OwningProcess
# PID 4 / System = IIS(http.sys) ; httpd.exe = Apache(XAMPP)
```

Site IIS `Genora.Ingress.Production` (nếu còn) **không nằm trong luồng** vì Apache chiếm 443. Mọi sửa đổi routing/rate-limit phải làm ở **vhost Apache**.

## Kiến trúc 3 tầng

```
Client HTTPS (Zalo Mini App)
  → Apache :443 (.191, XAMPP)  [ingress + TLS + reverse proxy]
      /api/mini-app/hl25|hlg (public, non-admin) → YARP gateway 127.0.0.1:5088  [rate-limit + inject key] → ABP :8868 [guard]
      /api/mini-app/.../admin                     → ABP 103.157.218.174:8868    [bypass gateway, giữ auth]
      còn lại (static, account, /api/mini-app/hl Sales, signalr) → ABP :8868
```

| Thành phần | Site IIS | Binding | Env | Ghi chú |
|---|---|---|---|---|
| Ingress | (Apache, không phải IIS) | `.191:443` | — | `httpd-vhost.conf` |
| YARP Gateway | `Genora.Tenant.Gateway` | `*:5088` | Production | No Managed Code, Integrated, **MaxProcesses=1** |
| ABP Backend | `Genora.MultiTenancy` | `*:8868` | Production | Guard bật |

| Host | Profile | Tenant | TenantId | Quota/s |
|---|---|---|---|---|
| duocpham-hoalinh.genora.vn | Hl25 | Dược phẩm Hoa Linh | 209567fc-4850-44e9-11c8-3a23c58d15a4 | 500 |
| hoalinh.genora.vn | Hlg | Hoa Linh Miền Nam | 27e348a9-036c-bef8-fa2b-3a22fc202c26 | 300 |

> `hoalinh.genora.vn` còn phục vụ Hoa Linh Sales `/api/mini-app/hl` → đi thẳng ABP, KHÔNG rate-limit. KHÔNG đưa `/api/mini-app/hl` vào guard PathPrefixes (sẽ làm Sales bị 403).

## File trong thư mục này

| File | Đích trên server | Bí mật? |
|---|---|---|
| `apache/genora-miniapp-vhosts.conf` | chèn vào `C:\xampp\apache\conf\extra\httpd-vhost.conf` (trước vhost `*.genora.vn`) | Không |
| `gateway/appsettings.Production.json` | thư mục publish site `Genora.Tenant.Gateway` | **Có** (SharedKey — điền trước khi dùng) |
| `gateway/web.config` | site `Genora.Tenant.Gateway` | Không |
| `abp/appsettings.Production.guard.json` | **merge** 2 section vào `appsettings.Production.json` của site `Genora.MultiTenancy` | **Có** (SharedKey) |
| `iis-ingress-alternative/web.config` | CHỈ dùng nếu sau này chuyển ingress sang IIS (hiện KHÔNG dùng) | Không |

> Các file có `<REPLACE_...>` là placeholder. Giá trị secret KHÔNG được commit lên git — điền trực tiếp trên server hoặc qua environment variables.

## Sinh SharedKey

Mỗi tenant một key riêng, ≥32 ký tự ASCII không khoảng trắng. Key của cùng tenant phải **giống nhau** giữa Gateway và ABP guard.

```powershell
# Sinh 1 key ngẫu nhiên (base64 32 byte)
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Max 256 } | ForEach-Object { [byte]$_ }))
```

Điền cùng một giá trị vào `TenantGateway:Tenants:<name>:SharedKey` (gateway) và `TenantGatewayGuard:Tenants:<name>:SharedKey` (ABP).

## Các bước triển khai

1. **Publish** (từ repo root):
   ```powershell
   dotnet publish src/Genora.MultiTenancy.Gateway -c Release -o artifacts/gateway-iis
   dotnet publish src/Genora.MultiTenancy.Web -c Release -o artifacts/abp-web
   ```
2. **Gateway:** copy `gateway/appsettings.Production.json` (đã điền key) + `gateway/web.config` vào thư mục publish gateway. App pool: No Managed Code, Integrated, **Maximum Worker Processes = 1**. Binding `*:5088`.
3. **ABP:** merge 2 section trong `abp/appsettings.Production.guard.json` (đã điền key khớp gateway) vào `appsettings.Production.json` của site ABP. KHÔNG ghi đè các section khác (ConnectionStrings/Zalo/UrBox...). Restart site ABP.
4. **Apache:** chèn 2 vhost trong `apache/genora-miniapp-vhosts.conf` vào `httpd-vhost.conf`, đặt **trước** vhost wildcard `tenant-proxy.genora.vn`. Kiểm cú pháp + reload:
   ```powershell
   & C:\xampp\apache\bin\httpd.exe -t
   # reload qua XAMPP Control Panel (Stop/Start Apache); máy không chạy Apache dạng service.
   ```
5. **Firewall** cổng 5088 chỉ cho localhost (tránh gọi thẳng .191:5088/.174:5088 né rate-limit):
   ```powershell
   New-NetFirewallRule -DisplayName "Block external 5088" -Direction Inbound -LocalPort 5088 -Protocol TCP -RemoteAddress Internet -Action Block
   ```

## Kiểm thử

```powershell
# 1) Gateway sống + inject key + guard OK (trực tiếp, tách Apache)
curl.exe -i http://127.0.0.1:5088/api/mini-app/hl25/config -H "Host: duocpham-hoalinh.genora.vn"   # 200 success

# 2) Public qua Apache -> gateway (response CÓ X-Powered-By = qua gateway; ABP strip header này)
curl.exe -i https://duocpham-hoalinh.genora.vn/api/mini-app/hl25/config                # 200
curl.exe -i https://hoalinh.genora.vn/api/mini-app/hlg/knowledge/categories            # 200

# 3) Gọi thẳng ABP không key -> 403 (guard hoạt động)
curl.exe -i http://127.0.0.1:8868/api/mini-app/hl25/config -H "Host: duocpham-hoalinh.genora.vn"   # 403

# 4) Sai host -> 404
curl.exe -i https://hoalinh.genora.vn/api/mini-app/hl25/config                          # 404

# 5) Test chặn 429: tạm hạ PermitLimit xuống 2, recycle gateway, bắn burst đồng thời
#    (config gateway đọc lúc startup => PHẢI recycle app pool sau khi đổi PermitLimit)
$gw = (Get-IISSite 'Genora.Tenant.Gateway').Applications['/'].ApplicationPoolName
Restart-WebAppPool -Name $gw
$u='https://duocpham-hoalinh.genora.vn/api/mini-app/hl25/config'
$jobs = 1..20 | ForEach-Object { Start-Job { param($u) curl.exe -s -o $null -w "%{http_code}" $u } -ArgumentList $u }
$jobs | Wait-Job | Receive-Job | Group-Object | Format-Table Name, Count; $jobs | Remove-Job
# Kỳ vọng: hỗn hợp 200 + 429. Test xong trả PermitLimit về 500/300 rồi recycle lại.
```

## Lưu ý vận hành

- Gateway đọc config **lúc startup** → đổi `PermitLimit` (hay bất kỳ config nào) phải `Restart-WebAppPool 'Genora.Tenant.Gateway'` mới có hiệu lực.
- Quota rate-limit là **process-local** (SlidingWindow 1s, 10 segment). Giữ **1 worker**; nhiều worker / overlapped recycle sẽ nhân quota lên.
- Endpoint public phải nằm trong `ApiProfiles` (Hl25/Hlg) — liệt kê tường minh trong `src/Genora.MultiTenancy.Gateway/GatewayApiProfiles.cs`. Endpoint mới của FE mà chưa có trong profile → gateway trả **404**; phải bổ sung profile + rebuild gateway (và guard PathPrefix nếu cần).
- Guard bật → gọi thẳng `8868` vào path hl25/hlg mà không có key sẽ **403**. Đó là đúng, KHÔNG tắt guard để chữa 403.
- Chưa load-test SLA thực tế (500/300 là ngân sách tối đa, không phải cam kết throughput). Điều chỉnh quota theo số đo thật.

## Bối cảnh & bài học

Chi tiết hành trình chẩn đoán (vì sao sửa IIS không có tác dụng, cách phát hiện Apache giữ 443) nằm ở:
`.claude/memory/notes/project/project_gateway_production_apache_ingress_20260923.md` và
`.claude/memory/notes/feedback/feedback_prod_apache_ingress_not_iis.md`.
