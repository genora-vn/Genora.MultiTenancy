---
name: feedback_hl_payment_setting_names
description: HlPaymentService phải dùng ZaloPaymentSettingNames constants thay vì string cứng cho settings
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 37037663-1780-4c59-a844-c739db28b61a
---

HlPaymentService ban đầu dùng string cứng sai cho settings ("Zalo.MiniAppId", "Payment.BankName"...) gây lỗi `Undefined setting`. Phải dùng constants từ `ZaloPaymentSettingNames`:

- AppId → `ZaloPaymentSettingNames.AppId` = `"Genora.Zalo.MiniAppId"`
- PrivateKey → `ZaloPaymentSettingNames.PrivateKey` = `"Genora.Payment.Zalo.PrivateKey"`
- BankName → `ZaloPaymentSettingNames.BankName` = `"Genora.Payment.Bank.BankName"`
- AccountNumber → `ZaloPaymentSettingNames.BankAccountNumber` = `"Genora.Payment.Bank.AccountNumber"`
- AccountOwner → `ZaloPaymentSettingNames.BankAccountOwner` = `"Genora.Payment.Bank.AccountOwner"`
- Branch → `ZaloPaymentSettingNames.BankBranch` = `"Genora.Payment.Bank.Branch"`

**Why:** ABP SettingProvider throw `Undefined setting` nếu setting name không match với tên đã register trong SettingDefinitionProvider.

**How to apply:** Mọi payment service mới phải import `using Genora.MultiTenancy.AppServices.AppPayments;` và reference constants từ `ZaloPaymentSettingNames`, không dùng string cứng.

[[project_hoalinh_phase5_complete]] [[project_salon_beauty_miniapp_payment_endpoints]]
