<table cellpadding="0" cellspacing="0" border="0" width="600" style="width:600px;border-collapse:collapse;font-family:Arial,sans-serif;font-size:14px;color:#000;border:1px solid #b7b7b7;">
    <tr>
        <td style="background:#355f93;color:#fff;text-align:center;padding:10px 16px 6px 16px;font-size:18px;font-weight:700;">
            YÊU CẦU ĐẶT CHỖ MỚI
        </td>
    </tr>
    <tr>
        <td style="background:#355f93;color:#fff;text-align:center;padding:0 16px 6px 16px;font-size:16px;font-style:italic;">
            {{ model.GolfCourseName }}
        </td>
    </tr>
    <tr>
        <td style="background:#355f93;color:#fff;padding:0 16px 10px 16px;font-size:12px;text-align:right;">
            Hotline: {{ model.GolfCourseHotline }}<br />
            Địa chỉ: {{ model.GolfCourseAddress }}
        </td>
    </tr>

    <tr><td style="background:#e9eef4;height:8px;"></td></tr>

    <tr><td style="background:#cfd9e6;padding:6px 8px;font-weight:700;">THÔNG TIN ĐẶT CHỖ</td></tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="4" cellspacing="0" border="0" style="border-collapse:collapse;">
                <tr style="background:#dfe6ee;">
                    <td width="38%">Mã đặt chỗ:</td>
                    <td>{{ model.BookingCode }}</td>
                </tr>
                <tr style="background:#dfe6ee;">
                    <td>Tên khách:</td>
                    <td>{{ model.BookerName }}</td>
                </tr>
                <tr style="background:#dfe6ee;">
                    <td>Người chơi cùng:</td>
                    <td>{{ if model.PlayersText != "" }}{{ model.PlayersText }}{{ else }}Không có{{ end }}</td>
                </tr>
                <tr style="background:#dfe6ee;">
                    <td>Số điện thoại:</td>
                    <td>{{ model.BookerPhone }}</td>
                </tr>
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="4" cellspacing="0" border="0" style="border-collapse:collapse;">
                <tr style="background:#d8d8d8;">
                    <td width="38%">Ngày chơi</td>
                    <td>{{ model.PlayDateText }}</td>
                </tr>
                <tr>
                    <td>Tee time đăng ký</td>
                    <td>{{ model.TeeTimeFromText }} - {{ model.TeeTimeToText }}</td>
                </tr>
                <tr style="background:#d8d8d8;">
                    <td>Số lượng người chơi</td>
                    <td>{{ model.NumberOfGolfers }}</td>
                </tr>
                <tr>
                    <td>Loại khách</td>
                    <td>{{ model.CustomerTypeSummary }}</td>
                </tr>
                <tr style="background:#d8d8d8;">
                    <td>Chương trình ưu đãi</td>
                    <td>{{ if model.PromotionText != "" }}{{ model.PromotionText }}{{ else }}Không có{{ end }}</td>
                </tr>
            </table>
        </td>
    </tr>

    <tr><td style="background:#cfd9e6;padding:6px 8px;font-weight:700;">THANH TOÁN</td></tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="4" cellspacing="0" border="0" style="border-collapse:collapse;">
                {{ if model.HasPriceBreakdownItems }}
                    {{ for item in model.PriceBreakdownItems }}
                        <tr>
                            <td width="38%">Đơn giá/Khách ({{ item.customer_type_name }})</td>
                            <td><b>{{ item.price_text }} x {{ item.count }}</b></td>
                        </tr>
                    {{ end }}
                {{ else }}
                    <tr>
                        <td width="38%">Đơn giá/Khách</td>
                        <td><b>{{ model.PricePerGolferText }} x {{ model.NumberOfGolfers }}</b></td>
                    </tr>
                {{ end }}

                <tr>
                    <td>Tổng giá trị đặt chỗ</td>
                    <td><b>{{ model.TotalAmountText }}</b></td>
                </tr>
                {{ if model.HasCaddieFee }}
                <tr>
                    <td>Tổng phí đặt Caddie</td>
                    <td><b>{{ model.TotalCaddieFeeText }}</b></td>
                </tr>
                <tr>
                    <td>Tổng cộng</td>
                    <td><b>{{ model.GrandTotalText }}</b></td>
                </tr>
                {{ end }}
                <tr>
                    <td>Phương thức thanh toán</td>
                    <td>{{ model.PaymentMethod }}</td>
                </tr>
            </table>
        </td>
    </tr>

    <tr><td style="background:#cfd9e6;padding:6px 8px;font-weight:700;">YÊU CẦU ĐẶC BIỆT KHÁC (nếu có)</td></tr>
    <tr>
        <td style="padding:8px 8px;line-height:1.55;">
            {{ if model.OtherRequests != "" }}
                {{ for line in (model.OtherRequests | string.replace "\r" "" | string.split "\n") }}
                    {{ if line != "" }}
                        <div style="margin:0 0 4px 0;">{{ line }}</div>
                    {{ end }}
                {{ end }}
            {{ else }}
                <div>Không có</div>
            {{ end }}
        </td>
    </tr>

    <tr><td style="background:#cfd9e6;padding:6px 8px;font-weight:700;">THÔNG TIN XUẤT HÓA ĐƠN (nếu có)</td></tr>
    <tr>
        <td style="padding:8px 8px;line-height:1.55;">
            {{ if model.IsExportInvoice }}
                <div style="margin:0 0 4px 0;">Tên công ty: {{ model.CompanyName }}</div>
                <div style="margin:0 0 4px 0;">Mã số thuế: {{ model.TaxCode }}</div>
                <div style="margin:0 0 4px 0;">Địa chỉ: {{ model.CompanyAddress }}</div>
                <div style="margin:0;">Email nhận hóa đơn: {{ model.InvoiceEmail }}</div>
            {{ else }}
                <div>Không yêu cầu</div>
            {{ end }}
        </td>
    </tr>

    <tr>
        <td style="padding:18px 8px;text-align:center;font-style:italic;font-weight:700;">
            Vui lòng kiểm tra và cập nhật trạng thái booking trên hệ thống!
        </td>
    </tr>
</table>