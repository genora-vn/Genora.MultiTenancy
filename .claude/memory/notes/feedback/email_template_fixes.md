---
name: Email template rendering fixes
description: Fixed BookingNewRequest and BookingChangeRequest email sending - Scriban syntax and model passing issues
type: feedback
originSessionId: 56881ace-08af-4e26-bdd5-ef91e2a72f7a
---
**Root causes of email sending failure:**

1. **Invalid Scriban syntax `!= empty`** (BookingNewRequest.tpl:46, BookingChangeRequest.tpl:48)
   - Changed `{{ if model.PriceBreakdownItems != empty and ... }}`
   - To: `{{ if model.PriceBreakdownItems != null and ... }}`
   - **Why:** `empty` is not a valid Scriban keyword. Use `null` for null checks.

2. **Template model passed as null** (AppEmailSenderService:146)
   - Was: `await _templateRenderer.RenderAsync(templateName, model: null, globalContext: ...)`
   - Now: `await _templateRenderer.RenderAsync(templateName, model, globalContext: ...)`
   - **Why:** Even though model was in globalContext, passing `model: null` causes issues with template rendering

3. **Debug code checked VFS paths** (AppEmailSenderService:131-142)
   - Removed hardcoded debugging code checking for BookingChangeRequest.tpl file existence
   - Removed unused IVirtualFileProvider injection

**How to apply:** When checking email template rendering errors, look for:
- Scriban syntax errors (use grep for "`!= empty`" or "`== empty`")
- Model parameter passing in RenderAsync calls
- Invalid Scriban keywords in .tpl files

**Templates affected:**
- BookingNewRequest.tpl
- BookingChangeRequest.tpl
