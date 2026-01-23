Hệ thống Zalo Mini App ghi nhận yêu cầu đặt chỗ mới.

1. Thông tin đặt chỗ:
• Mã đặt chỗ: {{ model.BookingCode }}
• Người đặt chỗ: {{ model.BookerName }}
• Số điện thoại: {{ model.BookerPhone }}
• Ngày chơi: {{ model.PlayDateText  }}
• Tee time đăng ký: {{ model.TeeTimeFromText }} - {{ model.TeeTimeToText }}
• Số lượng người chơi: {{ model.NumberOfGolfers }}
• Loại khách: {{ model.CustomerTypeSummary }}

2. Thanh toán:
• Tổng giá trị booking: {{ model.TotalAmountText }}
• Phương thức thanh toán: {{ model.PaymentMethod }}

3. Yêu cầu khác (nếu có)
{{ if model.OtherRequests != "" -}}
{{ model.OtherRequests }}
{{- else -}}
Không có
{{- end }}

4. Thông tin xuất hóa đơn (nếu có)
{{ if model.IsExportInvoice -}}
• Tên công ty: {{ model.CompanyName }}
• Mã số thuế: {{ model.TaxCode }}
• Địa chỉ: {{ model.CompanyAddress }}
• Email nhận hóa đơn: {{ model.InvoiceEmail }}
{{- else -}}
Không yêu cầu xuất hóa đơn
{{- end }}

Vui lòng kiểm tra và cập nhật trạng thái booking trên hệ thống.
