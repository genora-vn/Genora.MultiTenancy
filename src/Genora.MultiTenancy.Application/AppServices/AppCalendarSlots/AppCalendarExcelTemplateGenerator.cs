
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
        
        public IRemoteStreamContent GenerateTemplate(List<CustomerType> customerTypes, List<DomainModels.AppPromotionTypes.PromotionType> promotions)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Danh sách booking");

            // ===== TIÊU ĐỀ =====
            
            ws.Cell(1, 1).Value = "Mã sân golf";
            ws.Column(1).Width = 15;
            ws.Range(ws.Cell(1, 1).Address, ws.Cell(2, 1).Address).Merge();
            
            ws.Cell(1, 2).Value = "Ngày bắt đầu(*)";
            ws.Range(ws.Cell(1, 2).Address, ws.Cell(2, 2).Address).Merge();
            ws.Cell(1, 3).Value = "Ngày kết thúc(*)";
            ws.Range(ws.Cell(1, 3).Address, ws.Cell(2, 3).Address).Merge();
            ws.Cell(1, 4).Value = "Giờ bắt đầu (*)";
            ws.Range(ws.Cell(1, 4).Address, ws.Cell(2, 4).Address).Merge();
            ws.Cell(1, 5).Value = "Giờ kết thúc";
            ws.Range(ws.Cell(1, 5).Address, ws.Cell(2, 5).Address).Merge();
            ws.Cell(1, 6).Value = "Loại ưu đãi (*)";
            ws.Range(ws.Cell(1, 6).Address, ws.Cell(2, 6).Address).Merge();
            ws.Cell(1, 7).Value = "Số slot tối đa";
            ws.Range(ws.Cell(1, 7).Address, ws.Cell(2, 7).Address).Merge();
            ws.Cell(1, 8).Value = "Ghi chú";
            ws.Range(ws.Cell(1, 8).Address, ws.Cell(2, 8).Address).Merge();
            ws.Cell(1, 9).Value = "Gap (Tần suất)";
            ws.Range(ws.Cell(1, 9).Address, ws.Cell(2, 9).Address).Merge();


            var totalCustomerTypes = customerTypes.Count;
            ws.Range(ws.Cell(1,10).Address, ws.Cell(1, 10 + totalCustomerTypes-1).Address).Merge();
            ws.Cell(1, 10).Value = "Bảng giá";
            var index = 0;
            for (var i = 10; i < (10 + totalCustomerTypes); i++)
            {
                ws.Cell(2, i).Value = $"Giá {customerTypes[index].Name}";
                ws.Cell(3, i).Value = $"Giá áp dụng cho {customerTypes[index].Name}";
                index++;
            }
            ws.Range(ws.Cell(1, 1).Address, ws.Cell(2, 9 + totalCustomerTypes).Address).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(ws.Cell(1, 1).Address, ws.Cell(2, 9 + totalCustomerTypes).Address).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            //ws.Cell(2, 8).Value = "Loại khách hàng";
            //ws.Cell(2, 9).Value = "Giá áp dụng";

            ws.Row(1).Style.Font.Bold = true;
            ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Row(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Row(2).Style.Font.Bold = true;
            ws.Row(2).Style.Fill.BackgroundColor = XLColor.LightGray;
            //// ===== MÔ TẢ =====
            ws.Cell(3, 1).Value = "     MONT     ";
            ws.Cell(3, 2).Value = "dd/MM/yyyy";
            ws.Cell(3, 3).Value = "dd/MM/yyyy";
            ws.Cell(3, 4).Value = "HH:mm (ex: 6:30)";
            ws.Cell(3, 5).Value = "HH:mm (ex: 6:30)";
            ws.Cell(3, 6).Value = "Normal/Promotion";
            ws.Cell(3, 7).Value = "Số nguyên nhỏ hơn 100";
            ws.Cell(3, 8).Value = "Ghi chú nội bộ";
            ws.Cell(3, 9).Value = "Khoảng cách 2 teetime";
            //ws.Range(ws.Cell(3, 2).Address, ws.Cell(1000, 2).Address).Style.DateFormat.Format = "dd/MM/yyyy";
            //ws.Range(ws.Cell(3, 3).Address, ws.Cell(1000, 3).Address).Style.NumberFormat.Format = "HH:mm";
            //ws.Range(ws.Cell(3, 4).Address, ws.Cell(1000, 4).Address).Style.NumberFormat.Format = "HH:mm";
            ws.Row(3).Style.Font.FontColor = XLColor.DarkGray;

            //var rangeB = ws.Range("B2:B" + XLHelper.MaxRowNumber);
            //var valB = rangeB.SetDataValidation();
            //valB.Date.Between(new DateTime(2000, 1, 1), DateTime.Today);
            //valB.ErrorTitle = "Ngày không hợp lệ";
            //valB.ErrorMessage = "Vui lòng nhập ngày theo định dạng dd-MM-yyyy và trong khoảng cho phép";
            //valB.IgnoreBlanks = true;

            //// Validation cho cột C: Chỉ cho phép giờ (TimeSpan)
            //var rangeC = ws.Range("C2:C" + XLHelper.MaxRowNumber);
            //var valC = rangeC.SetDataValidation();
            //valC.Time.Between(TimeSpan.Zero, TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59)));
            //valC.ErrorTitle = "Giờ không hợp lệ";
            //valC.ErrorMessage = "Vui lòng nhập giờ theo định dạng HH:mm:ss (00:00:00 đến 23:59:59)";
            //valC.IgnoreBlanks = true;

            var enumPromation = promotions.Select(p => p.Name).ToList();
            var sourse = $"\"{string.Join(",", enumPromation)}\"";
            ws.Range("F4:F1000").SetDataValidation().List(sourse, true);
  
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
