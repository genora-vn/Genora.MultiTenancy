---
name: Excel Export pattern cho Orders
description: Pattern thêm tính năng export Excel cho AppService Orders (FnB và Proshop)
type: project
---

**Pattern chuẩn để thêm Export Excel cho một Order service:**

### 1. Interface (`IApp*OrderService.cs`)
```csharp
using Volo.Abp.Content;
Task<IRemoteStreamContent> ExportExcelAsync(Get*OrderListInput input);
```

### 2. Service (`App*OrderService.cs`)
```csharp
using ClosedXML.Excel;
using System.IO;
using Volo.Abp.Content;

public async Task<IRemoteStreamContent> ExportExcelAsync(Get*OrderListInput input)
{
    await CheckPolicyAsync(GetRootPermission());
    var query = await BuildQueryAsync(input);  // tái dùng filter có sẵn
    var items = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.CreationTime));
    using var workbook = new XLWorkbook();
    var ws = workbook.Worksheets.Add("Sheet Name");
    // ... viết header và data ...
    return StreamToRemoteContent(workbook, $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
}

private static IRemoteStreamContent StreamToRemoteContent(XLWorkbook workbook, string fileName)
{
    var stream = new MemoryStream();
    workbook.SaveAs(stream);
    stream.Position = 0;
    return new RemoteStreamContent(stream, fileName,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
}
```

### 3. Controller (`HttpApi/Controllers/App*OrderExcelController.cs`)
```csharp
[ApiController]
[Route("api/app/app-*-order-excel")]
public class App*OrderExcelController : AbpController
{
    [HttpGet("export")]
    [DisableValidation]
    public Task<IRemoteStreamContent> Export([FromQuery] Get*OrderListInput input)
        => _service.ExportExcelAsync(input);
}
```

### 4. JS (`index.js`)
```js
$('#Export*OrderExcelButton').on('click', function (e) {
    e.preventDefault();
    genora.excel.download('api/app/app-*-order-excel/export', getFilter());
});
```

**Routes đã có:**
- FnB: `api/app/app-fnb-order-excel/export`
- Proshop: `api/app/app-pro-order-excel/export`

**Why:** Tái sử dụng `BuildQueryAsync` giúp filter export đồng bộ với filter trên bảng, không cần viết lại logic.
