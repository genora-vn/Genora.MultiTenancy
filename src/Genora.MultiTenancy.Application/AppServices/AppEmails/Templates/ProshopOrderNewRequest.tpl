{{ 
    # Tự động map fallback biến root
    m = model ?? $root ?? continuation
    
    order_code = m.order_code ?? m.OrderCode ?? order_code ?? OrderCode
    bag_tag = m.bag_tag ?? m.BagTag ?? bag_tag ?? BagTag
    customer_name = m.customer_name ?? m.CustomerName ?? customer_name ?? CustomerName
    customer_phone = m.customer_phone ?? m.CustomerPhone ?? customer_phone ?? CustomerPhone
    creation_time_text = m.creation_time_text ?? m.CreationTimeText ?? creation_time_text ?? CreationTimeText
    total_amount_text = m.total_amount_text ?? m.TotalAmountText ?? total_amount_text ?? TotalAmountText
    service_status = m.service_status ?? m.ServiceStatus ?? service_status ?? ServiceStatus
    service_status_text = m.service_status_text ?? m.ServiceStatusText ?? service_status_text ?? ServiceStatusText
    payment_status = m.payment_status ?? m.PaymentStatus ?? payment_status ?? PaymentStatus
    payment_status_text = m.payment_status_text ?? m.PaymentStatusText ?? payment_status_text ?? PaymentStatusText
    payment_method_text = m.payment_method_text ?? m.PaymentMethodText ?? payment_method_text ?? PaymentMethodText
    note = m.note ?? m.Note ?? note ?? Note
    cancel_reason = m.cancel_reason ?? m.CancelReason ?? cancel_reason ?? CancelReason
    cancel_note = m.cancel_note ?? m.CancelNote ?? cancel_note ?? CancelNote
    
    items_list = m.items ?? m.Items ?? items ?? Items
}}

<table cellpadding="0" cellspacing="0" border="0" width="600" style="width:600px;border-collapse:collapse;font-family:Arial,sans-serif;font-size:14px;color:#333333;border:1px solid #b7b7b7;margin:0 auto;">
    <!-- HEADER -->
    <tr>
        <td style="background:#0f4c81;color:#ffffff;text-align:center;padding:16px;font-size:18px;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;">
            THÔNG BÁO ĐƠN HÀNG PROSHOP MỚI
        </td>
    </tr>
    <tr>
        <td style="background:#1b6ca8;color:#ffffff;text-align:center;padding:16px;font-size:13px;font-weight:600;">
            DỊCH VỤ DỤNG CỤ & PHỤ KIỆN GOLF (PROSHOP)
        </td>
    </tr>

    <tr><td style="background:#e3f2fd;height:8px;"></td></tr>

    <!-- THÔNG TIN ĐƠN HÀNG -->
    <tr>
        <td style="background:#bbdefb;padding:8px 12px;font-weight:700;text-transform:uppercase;color:#0d47a1;border-bottom:1px solid #90caf9;">
            THÔNG TIN ĐƠN HÀNG
        </td>
    </tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="0" style="border-collapse:collapse;font-size:13px;">
                <tr style="background:#f4f8fb;">
                    <td width="38%" style="font-weight:600;">Mã đơn hàng:</td>
                    <td><b style="color:#0d47a1;font-size:14px;">{{ order_code }}</b></td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Mã Bag Tag (Thẻ túi):</td>
                    <td><span style="background:#e3f2fd;color:#0d47a1;padding:3px 10px;border-radius:12px;font-weight:bold;border:1px solid #90caf9;">{{ bag_tag }}</span></td>
                </tr>
                <tr style="background:#f4f8fb;">
                    <td style="font-weight:600;">Trạng thái dịch vụ:</td>
                    <td>
                        {{ if service_status == 5 }}
                            <span style="background:#ffebee;color:#c62828;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ service_status_text != null && service_status_text != "" ? service_status_text : "Đã hủy" }}</span>
                        {{ else if service_status == 3 }}
                            <span style="background:#e8f5e9;color:#2e7d32;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ service_status_text != null && service_status_text != "" ? service_status_text : "Sẵn sàng giao" }}</span>
                        {{ else if service_status == 4 }}
                            <span style="background:#e0f2f1;color:#00695c;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ service_status_text != null && service_status_text != "" ? service_status_text : "Đã giao" }}</span>
                        {{ else }}
                            <span style="background:#e3f2fd;color:#0d47a1;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ service_status_text != null && service_status_text != "" ? service_status_text : "Đơn mới" }}</span>
                        {{ end }}
                    </td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Thời gian đặt:</td>
                    <td>{{ creation_time_text }}</td>
                </tr>
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <!-- KHÁCH HÀNG & GIAO HÀNG -->
    <tr>
        <td style="background:#bbdefb;padding:8px 12px;font-weight:700;text-transform:uppercase;color:#0d47a1;border-bottom:1px solid #90caf9;">
            KHÁCH HÀNG & GIAO HÀNG
        </td>
    </tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="0" style="border-collapse:collapse;font-size:13px;">
                <tr style="background:#f4f8fb;">
                    <td width="38%" style="font-weight:600;">Tên khách hàng:</td>
                    <td><b>{{ customer_name }}</b></td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Số điện thoại:</td>
                    <td>{{ customer_phone }}</td>
                </tr>
                <tr style="background:#f4f8fb;">
                    <td style="font-weight:600;">Ghi chú / Địa điểm giao:</td>
                    <td>
                        {{ if note != null && note != "" }}
                            <b style="color:#d97706;">{{ note }}</b>
                        {{ else }}
                            <span style="color:#888888;font-style:italic;">Không có ghi chú</span>
                        {{ end }}
                    </td>
                </tr>
                {{ if service_status == 5 || (cancel_reason != null && cancel_reason != "") }}
                <tr style="background:#ffebee;">
                    <td style="font-weight:600;color:#c62828;">Lý do hủy:</td>
                    <td style="color:#c62828;">{{ cancel_note ?? cancel_reason ?? "Hủy bởi hệ thống/khách hàng" }}</td>
                </tr>
                {{ end }}
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <!-- CHI TIẾT SẢN PHẨM PROSHOP -->
    <tr>
        <td style="background:#bbdefb;padding:8px 12px;font-weight:700;text-transform:uppercase;color:#0d47a1;border-bottom:1px solid #90caf9;">
            CHI TIẾT SẢN PHẨM ({{ items_list ? (items_list | array.size) : 0 }})
        </td>
    </tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="1" bordercolor="#e0e0e0" style="border-collapse:collapse;font-size:13px;">
                <thead>
                    <tr style="background:#1b6ca8;color:#ffffff;text-align:left;">
                        <th style="padding:8px;">Tên sản phẩm</th>
                        <th style="padding:8px;text-align:right;">Đơn giá</th>
                        <th style="padding:8px;text-align:center;">SL</th>
                        <th style="padding:8px;text-align:right;">Thành tiền</th>
                    </tr>
                </thead>
                <tbody>
                    {{ for item in items_list }}
                    {{ 
                        item_name = item.item_name ?? item.ItemName
                        price_text = item.price_text ?? item.PriceText
                        quantity = item.quantity ?? item.Quantity
                        amount_text = item.amount_text ?? item.AmountText
                        item_note = item.note ?? item.Note
                    }}
                    <tr>
                        <td style="padding:8px;">
                            <b style="color:#0d47a1;">{{ item_name }}</b>
                            {{ if item_note != null && item_note != "" }}
                                <br/><span style="color:#d97706;font-size:11px;font-style:italic;">* Ghi chú: {{ item_note }}</span>
                            {{ end }}
                        </td>
                        <td style="padding:8px;text-align:right;">{{ price_text }}</td>
                        <td style="padding:8px;text-align:center;"><b>{{ quantity }}</b></td>
                        <td style="padding:8px;text-align:right;"><b>{{ amount_text }}</b></td>
                    </tr>
                    {{ end }}
                </tbody>
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <!-- THANH TOÁN -->
    <tr>
        <td style="background:#bbdefb;padding:8px 12px;font-weight:700;text-transform:uppercase;color:#0d47a1;border-bottom:1px solid #90caf9;">
            THANH TOÁN
        </td>
    </tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="0" style="border-collapse:collapse;font-size:13px;">
                <tr style="background:#f4f8fb;">
                    <td width="38%" style="font-weight:600;">Trạng thái thanh toán:</td>
                    <td>
                        {{ if payment_status == 2 }}
                            <span style="background:#e8f5e9;color:#2e7d32;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ payment_status_text != null && payment_status_text != "" ? payment_status_text : "Đã thanh toán" }}</span>
                        {{ else if payment_status == 3 }}
                            <span style="background:#ffebee;color:#c62828;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ payment_status_text != null && payment_status_text != "" ? payment_status_text : "Thanh toán thất bại" }}</span>
                        {{ else }}
                            <span style="background:#fff8e1;color:#f57f17;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ payment_status_text != null && payment_status_text != "" ? payment_status_text : "Chưa thanh toán" }}</span>
                        {{ end }}
                    </td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Phương thức thanh toán:</td>
                    <td>{{ payment_method_text }}</td>
                </tr>
                <tr style="background:#bbdefb;font-size:14px;">
                    <td style="font-weight:700;color:#0d47a1;">Tổng cộng:</td>
                    <td style="font-weight:700;color:#0d47a1;font-size:16px;">{{ total_amount_text }}</td>
                </tr>
            </table>
        </td>
    </tr>

    <!-- FOOTER -->
    <tr>
        <td style="padding:16px 8px;text-align:center;font-style:italic;font-weight:700;color:#555555;background:#f9f9f9;border-top:1px solid #e0e0e0;">
            Vui lòng kiểm tra và chuẩn bị sản phẩm Proshop để giao cho khách hàng kịp thời!
        </td>
    </tr>
</table>