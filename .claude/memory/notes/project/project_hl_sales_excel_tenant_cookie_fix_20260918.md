# Hoa Linh Sales — Excel download tenant-cookie API fix (2026-09-18)

- User browser error on all three pages: `abp.multiTenancy.getTenantIdCookie is not a function`, sales.js line 49. Error occurs before fetch; no backend Excel request is sent.
- Root cause: shared downloader introduced a tenant-cookie API not available in this ABP runtime. Local `wwwroot/libs/abp/core/abp.js` also does not define it.
- Fix: remove manual tenant-cookie API/header logic, retain same-origin fetch with `credentials: 'same-origin'` so browser sends authentication/tenant cookies; keep existing filters, Blob download, filenames, API/HTML error checks and button restoration.
- Scope: `/HoaLinh/PointHistory`, `/HoaLinh/GiftExchanges`, `/HoaLinh/Orders` all use `Pages/HoaLinh/sales.js`.
- Previous JS tests replaced download with a mock and therefore missed this runtime error. Added 4 tests running the real implementation: each of the 3 export routes with missing tenant-cookie method, credentials/filter preservation, filename/Blob click/button restoration; API-error/login-HTML rejection.
- Validation: **12 JS regression tests pass**, `node --check sales.js` and git diff --check pass. No backend change, migration or database write. Browser download against live host not verified in this session.
- Reload pages with Ctrl+F5 after serving the updated JS; pages already use asp-append-version.
- Branch `dev`; change not committed/deployed by agent. Existing log-file changes preserved.
