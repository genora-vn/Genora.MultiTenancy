Hệ thống Zalo Mini App ghi nhận yêu cầu thay đổi đặt chỗ.

1. Thông tin booking:
• Mã đặt chỗ: {{ model.BookingCode }}
• Người đặt chỗ: {{ model.BookerName }}
• Số điện thoại: {{ model.BookerPhone }}

2. Thông tin trước thay đổi:
• Trạng thái cũ: {{ model.OldStatusText }}
• Hình thức thanh toán cũ: {{ model.OldPaymentMethodText }}
• Số lượng cũ: {{ model.OldNumberOfGolfers }}

3. Thông tin người chơi cũ (nếu có thay đổi):
{{ if model.HasPlayerChanges && model.OldPlayersText != "" -}}
{{ model.OldPlayersText }}
{{- else -}}
Không có
{{- end }}

4. Thông tin sau thay đổi (nếu có thay đổi):
{{ if model.HasHeaderChanges -}}
• Trạng thái mới: {{ model.NewStatusText }}
• Hình thức thanh toán mới: {{ model.NewPaymentMethodText }}
• Số lượng mới: {{ model.NewNumberOfGolfers }}
{{- else -}}
Không có
{{- end }}

5. Thông tin người chơi mới (nếu có thay đổi):
{{ if model.HasPlayerChanges && model.NewPlayersText != "" -}}
{{ model.NewPlayersText }}
{{- else -}}
Không có
{{- end }}

Vui lòng kiểm tra và cập nhật trạng thái booking trên hệ thống.