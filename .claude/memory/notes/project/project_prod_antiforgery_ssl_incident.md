---
name: prod-antiforgery-ssl-incident
description: Production may terminate TLS at proxy and forward HTTP to app, causing antiforgery failures when SecurePolicy is Always.
type: project
originSessionId: e8a8c0ce-195d-42bd-aeea-fb5a2b8bf845
---
Production environment can terminate TLS at reverse proxy and forward non-SSL requests to the app process. Setting `AntiforgeryOptions.Cookie.SecurePolicy = Always` then causes runtime failures (`current request is not an SSL request`) on login and `/Abp/ApplicationConfigurationScript`.

**Why:** App sees internal HTTP without trusted forwarded-proto in that deployment path, so antiforgery token generation crashes before UI scripts load.

**How to apply:** Keep global antiforgery cookie policy at `SameAsRequest` for this deployment model, and for selected API endpoints (like AppHomePageConfigs update POST endpoints) use `[IgnoreAntiforgeryToken]` when requests are same-origin authenticated and protected by ABP auth/permissions.