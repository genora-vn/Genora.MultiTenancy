# Production ingress là Apache (XAMPP), KHÔNG phải IIS

## Bối cảnh
Debug mãi không hiểu vì sao sửa `web.config` của site IIS `Genora.Ingress.Production` (đúng binding `.191:443`, đúng rule, effective config xác nhận) mà request vẫn bỏ qua rule — kể cả rule `.*` → CustomResponse 418 đặt đầu tiên cũng không chạy.

## Nguyên nhân
Trên server production Genora, **cổng `103.157.218.191:443` do XAMPP Apache (`C:\xampp\apache\bin\httpd.exe`) chiếm**, không phải IIS/http.sys. Apache có vhost wildcard `ServerAlias *.genora.vn` proxy thẳng vào ABP `http://103.157.218.174:8868/`. IIS site ingress có binding `.191:443` nhưng http.sys không listen được (Apache giữ trước) → IIS ingress **không bao giờ nhận request** cho các host này.

## Quy tắc
- **Trước khi debug URL Rewrite/ARR trên IIS, kiểm ai thật sự giữ cổng 443:**
  ```powershell
  Get-NetTCPConnection -LocalPort 443 -State Listen | Select LocalAddress, OwningProcess
  # PID 4 / System = IIS(http.sys). httpd.exe = Apache(XAMPP).
  ```
- Test dứt điểm "IIS site có nằm trong luồng không": chèn rule `.*` → `CustomResponse` status lạ (vd 418) lên ĐẦU web.config. Nếu request không trả status đó → IIS không xử lý request (có tầng khác chặn trước).
- Dấu nhận biết response đi qua đâu: ABP Web strip `X-Powered-By`; gateway/Apache thì không. Có `X-Powered-By: ASP.NET` = KHÔNG phải ABP trực tiếp.
- Trên máy này, ingress/reverse-proxy thật cho `*.genora.vn` nằm ở `C:\xampp\apache\conf\extra\httpd-vhost.conf`, không phải IIS. Muốn chèn tầng (gateway rate-limit) phải sửa vhost Apache (`ProxyPass /path http://127.0.0.1:5088/path`), đặt vhost `ServerName` cụ thể TRƯỚC vhost wildcard `*.genora.vn`.

Chi tiết: `memory/notes/project/project_gateway_production_apache_ingress_20260923.md`.
