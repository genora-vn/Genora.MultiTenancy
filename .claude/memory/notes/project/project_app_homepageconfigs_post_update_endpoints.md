---
name: app-homepageconfigs-post-update-endpoints
description: AppHomePageConfigs update APIs switched to explicit HttpApi POST endpoints; AppService update methods hidden from auto remote exposure.
type: project
originSessionId: e8a8c0ce-195d-42bd-aeea-fb5a2b8bf845
---
AppHomePageConfigs update operations are now exposed via explicit `POST` endpoints in `AppHomePageConfigsController` under `api/app/app-home-page-config`, while corresponding update methods in `AppHomePageConfigService` are marked `[RemoteService(false)]`.

**Why:** Production environment cannot reliably use `PUT`, so update flows must avoid PUT entirely for this feature.

**How to apply:** For future changes in AppHomePageConfigs update flows, extend/maintain POST endpoints in the HttpApi controller and keep AppService update methods non-remote to avoid ABP auto-generated PUT endpoints.