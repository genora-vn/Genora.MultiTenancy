
using ClosedXML.Excel;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppCalendarSlots
{
    public class AppCalendarExcelTemplateGenerator : ITransientDependency
    {
        
        public IRemoteStreamContent GenerateTemplate(List<CustomerType> customerTypes)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Danh sách booking");

            // ===== TIÊU ĐỀ =====
            ws.Cell(1, 1).Value = "Mã sân golf";
            ws.Cell(1, 2).Value = "Ngày chơi (*)";
            ws.Cell(1, 3).Value = "Giờ bắt đầu (*)";
            ws.Cell(1, 4).Value = "Giờ kết thúc";
            ws.Cell(1, 5).Value = "Loại ưu đãi (*)";
            ws.Cell(1, 6).Value = "Số slot tối đa";
            ws.Cell(1, 7).Value = "Ghi chú";
            var totalCustomerTypes = customerTypes.Count;
            ws.Range(ws.Cell(1,8).Address, ws.Cell(1, 8 + totalCustomerTypes-1).Address).Merge();
            ws.Cell(1, 8).Value = "Bảng giá";
            var index = 0;
            for (var i = 8; i <= (8 + totalCustomerTypes -1); i++)
            {
                ws.Cell(2, i).Value = $"Giá {customerTypes[index].Name}";
                ws.Cell(3, i).Value = $"Giá áp dụng cho {customerTypes[index].Name}";
                index++;
            }
            //ws.Cell(2, 8).Value = "Loại khách hàng";
            //ws.Cell(2, 9).Value = "Giá áp dụng";

            ws.Row(1).Style.Font.Bold = true;
            ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Row(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Row(2).Style.Font.Bold = true;
            ws.Row(2).Style.Fill.BackgroundColor = XLColor.LightGray;
            //// ===== MÔ TẢ =====
            ws.Cell(3, 1).Value = "MONT-GOLFCLUB";
            ws.Cell(3, 2).Value = "dd/MM/yyyy";
            ws.Cell(3, 3).Value = "HH:mm (ex: 6:30)";
            ws.Cell(3, 4).Value = "HH:mm (ex: 6:30)";
            ws.Cell(3, 5).Value = "Normal/Promotion";
            ws.Cell(3, 6).Value = "Số nguyên nhỏ hơn 100";
            ws.Cell(3, 7).Value = "Ghi chú nội bộ";
            
            ws.Row(3).Style.Font.Italic = true;
            ws.Row(3).Style.Font.FontColor = XLColor.DarkGray;
            
            var enumPromation = Enum.GetNames(typeof(PromotionType));
            var sourse = $"\"{string.Join(",", enumPromation)}\"";
            ws.Range("E4:E1000").SetDataValidation().List(sourse, true);
  
            ws.Columns().AdjustToContents();
            
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new RemoteStreamContent(
                stream,
                $"Template_Import_Calendar_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            );
        }
    }
}
