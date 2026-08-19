---
name: feedback-hl-dual-permission-and-json-parse
description: "Hoa Linh module — 2 bug fixes quan trọng: dual permission Host/Tenant + smart JSON array parse"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: dcf06de4-47b3-4551-89dd-8fabec86f6de
---

## Bug 1: 403 Forbidden khi Host admin vào trang Hoa Linh

**Vấn đề:** `[Authorize(AppHlProducts.Default)]` là permission Tenant-side. Host admin được gán `HostAppHlProducts.Default` → ABP reject vì mismatch.

**Fix:** Bỏ `[Authorize]` attribute cứng, thay bằng helper `P()` + `CheckPermissionAsync()`:
```csharp
private string P(string tenantPerm, string hostPerm)
    => _currentTenant.Id.HasValue ? tenantPerm : hostPerm;

private async Task CheckPermissionAsync(string tenantPerm, string hostPerm)
{
    var perm = P(tenantPerm, hostPerm);
    var result = await _authService.AuthorizeAsync(perm);
    if (!result.Succeeded) throw new AbpAuthorizationException(...);
}
```

**Why:** ABP multi-tenant dual permission — Tenant permission require feature, Host permission không. Dùng `[Authorize]` cứng chỉ check 1 trong 2.
**How to apply:** Mọi AppService dùng chung cho Host + Tenant phải dùng pattern này thay vì `[Authorize(perm)]`.

---

## Bug 2: JSON parse error — API trả array thay vì paged object

**Vấn đề:** API Hoa Linh một số endpoint (Products, Orders, Campaigns) trả array `[...]` thay vì `{"total_records":..., "data":[...]}`. Code expect `HlPagedResponse<T>` → JsonException.

**Fix:** `DeserializeSmartResponse<T>()` detect array response (`json.StartsWith("[")`) và wrap thành `HlPagedResponse<T>` bằng reflection:
- Parse array thành `List<T>` 
- Tạo `HlPagedResponse<T>` với TotalRecords=count, Page=1, Limit=count, TotalPages=1

**Why:** API bên thứ 3 không nhất quán format. Cần robust parsing.
**How to apply:** Khi thêm API HL mới, không cần lo format — `DeserializeSmartResponse` tự handle cả 2 case.

[[project-hoalinh-phase2-complete]]
