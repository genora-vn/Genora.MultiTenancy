{{ 
    # Tự động map fallback biến root
    m = model ?? $root ?? continuation
    
    booking_code = m.booking_code ?? m.BookingCode ?? booking_code ?? BookingCode
    customer_name = m.customer_name ?? m.CustomerName ?? customer_name ?? CustomerName
    customer_phone = m.customer_phone ?? m.CustomerPhone ?? customer_phone ?? CustomerPhone
    golf_course_name = m.golf_course_name ?? m.GolfCourseName ?? golf_course_name ?? GolfCourseName
    booking_date_text = m.booking_date_text ?? m.BookingDateText ?? booking_date_text ?? BookingDateText
    start_time_text = m.start_time_text ?? m.StartTimeText ?? start_time_text ?? StartTimeText
    number_of_holes = m.number_of_holes ?? m.NumberOfHoles ?? number_of_holes ?? NumberOfHoles
    creation_time_text = m.creation_time_text ?? m.CreationTimeText ?? creation_time_text ?? CreationTimeText
    total_caddie_fee_text = m.total_caddie_fee_text ?? m.TotalCaddieFeeText ?? total_caddie_fee_text ?? TotalCaddieFeeText
    status = m.status ?? m.Status ?? status ?? Status
    status_text = m.status_text ?? m.StatusText ?? status_text ?? StatusText
    payment_status = m.payment_status ?? m.PaymentStatus ?? payment_status ?? PaymentStatus
    payment_status_text = m.payment_status_text ?? m.PaymentStatusText ?? payment_status_text ?? PaymentStatusText
    payment_method_text = m.payment_method_text ?? m.PaymentMethodText ?? payment_method_text ?? PaymentMethodText
    note = m.note ?? m.Note ?? note ?? Note
    cancel_reason = m.cancel_reason ?? m.CancelReason ?? cancel_reason ?? CancelReason
    
    caddies_list = m.caddies ?? m.Caddies ?? caddies ?? Caddies
}}

<table cellpadding="0" cellspacing="0" border="0" width="600" style="width:600px;border-collapse:collapse;font-family:Arial,sans-serif;font-size:14px;color:#333333;border:1px solid #b7b7b7;margin:0 auto;">
    <!-- HEADER -->
    <tr>
        <td style="background:#1b5e20;color:#ffffff;text-align:center;padding:16px;font-size:18px;font-weight:700;text-transform:uppercase;letter-spacing:0.5px;">
            THÔNG BÁO YÊU CẦU ĐẶT CADDIE MỚI
        </td>
    </tr>
    <tr>
        <td style="background:#2e7d32;color:#ffffff;text-align:center;padding:16px;font-size:13px;font-weight:600;">
            DỊCH VỤ CADDIE GOLF (MINI APP)
        </td>
    </tr>

    <tr><td style="background:#e8f5e9;height:8px;"></td></tr>

    <!-- THÔNG TIN ĐẶT CADDIE -->
    <tr>
        <td style="background:#c8e6c9;padding:8px 12px;font-weight:700;text-transform:uppercase;color:#1b5e20;border-bottom:1px solid #a5d6a7;">
            THÔNG TIN BOOKING CADDIE
        </td>
    </tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="0" style="border-collapse:collapse;font-size:13px;">
                <tr style="background:#f1f8e9;">
                    <td width="38%" style="font-weight:600;">Mã đặt Caddie:</td>
                    <td><b style="color:#2e7d32;font-size:14px;">{{ booking_code }}</b></td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Sân Golf:</td>
                    <td><b style="color:#1b5e20;">{{ golf_course_name }}</b></td>
                </tr>
                <tr style="background:#f1f8e9;">
                    <td style="font-weight:600;">Ngày chơi:</td>
                    <td><b>{{ booking_date_text }}</b> lúc <b>{{ start_time_text }}</b></td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Số hố:</td>
                    <td><b>{{ number_of_holes }} hố</b></td>
                </tr>
                <tr style="background:#f1f8e9;">
                    <td style="font-weight:600;">Trạng thái đặt:</td>
                    <td>
                        {{ if status == 4 }}
                            <span style="background:#ffebee;color:#c62828;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ status_text != null && status_text != "" ? status_text : "Đã hủy" }}</span>
                        {{ else if status == 2 }}
                            <span style="background:#e8f5e9;color:#2e7d32;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ status_text != null && status_text != "" ? status_text : "Đã xác nhận" }}</span>
                        {{ else if status == 3 }}
                            <span style="background:#e0f2f1;color:#00695c;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ status_text != null && status_text != "" ? status_text : "Hoàn thành" }}</span>
                        {{ else }}
                            <span style="background:#e8f5e9;color:#1b5e20;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ status_text != null && status_text != "" ? status_text : "Mới" }}</span>
                        {{ end }}
                    </td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Thời gian tạo:</td>
                    <td>{{ creation_time_text }}</td>
                </tr>
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <!-- KHÁCH HÀNG -->
    <tr>
        <td style="background:#c8e6c9;padding:8px 12px;font-weight:700;text-transform:uppercase;color:#1b5e20;border-bottom:1px solid #a5d6a7;">
            THÔNG TIN KHÁCH HÀNG
        </td>
    </tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="0" style="border-collapse:collapse;font-size:13px;">
                <tr style="background:#f1f8e9;">
                    <td width="38%" style="font-weight:600;">Tên khách hàng:</td>
                    <td><b>{{ customer_name }}</b></td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Số điện thoại:</td>
                    <td>{{ customer_phone }}</td>
                </tr>
                <tr style="background:#f1f8e9;">
                    <td style="font-weight:600;">Ghi chú yêu cầu:</td>
                    <td>
                        {{ if note != null && note != "" }}
                            <b style="color:#d97706;">{{ note }}</b>
                        {{ else }}
                            <span style="color:#888888;font-style:italic;">Không có ghi chú</span>
                        {{ end }}
                    </td>
                </tr>
                {{ if status == 4 || (cancel_reason != null && cancel_reason != "") }}
                <tr style="background:#ffebee;">
                    <td style="font-weight:600;color:#c62828;">Lý do hủy:</td>
                    <td style="color:#c62828;">{{ cancel_reason ?? "Hủy bởi hệ thống/khách hàng" }}</td>
                </tr>
                {{ end }}
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <!-- DANH SÁCH CADDIE CHỌN -->
    <tr>
        <td style="background:#c8e6c9;padding:8px 12px;font-weight:700;text-transform:uppercase;color:#1b5e20;border-bottom:1px solid #a5d6a7;">
            DANH SÁCH CADDIE YÊU CẦU ({{ caddies_list ? (caddies_list | array.size) : 0 }})
        </td>
    </tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="1" bordercolor="#e0e0e0" style="border-collapse:collapse;font-size:13px;">
                <thead>
                    <tr style="background:#2e7d32;color:#ffffff;text-align:left;">
                        <th style="padding:8px;">Mã Caddie</th>
                        <th style="padding:8px;">Tên Caddie</th>
                        <th style="padding:8px;text-align:center;">Giới tính</th>
                        <th style="padding:8px;">Ghi chú caddie</th>
                    </tr>
                </thead>
                <tbody>
                    {{ for cd in caddies_list }}
                    {{ 
                        cd_code = cd.caddie_code ?? cd.CaddieCode
                        cd_name = cd.caddie_name ?? cd.CaddieName
                        gender_text = cd.gender_text ?? cd.GenderText
                        cd_note = cd.note ?? cd.Note
                    }}
                    <tr>
                        <td style="padding:8px;"><b style="color:#2e7d32;">{{ cd_code }}</b></td>
                        <td style="padding:8px;"><b>{{ cd_name }}</b></td>
                        <td style="padding:8px;text-align:center;">{{ gender_text }}</td>
                        <td style="padding:8px;">
                            {{ if cd_note != null && cd_note != "" }}
                                <span style="color:#d97706;font-style:italic;">{{ cd_note }}</span>
                            {{ else }}
                                <span style="color:#a0a0a0;">—</span>
                            {{ end }}
                        </td>
                    </tr>
                    {{ end }}
                </tbody>
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <!-- THANH TOÁN -->
    <tr>
        <td style="background:#c8e6c9;padding:8px 12px;font-weight:700;text-transform:uppercase;color:#1b5e20;border-bottom:1px solid #a5d6a7;">
            THANH TOÁN PHÍ CADDIE
        </td>
    </tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="0" style="border-collapse:collapse;font-size:13px;">
                <tr style="background:#f1f8e9;">
                    <td width="38%" style="font-weight:600;">Trạng thái thanh toán:</td>
                    <td>
                        {{ if payment_status == 2 }}
                            <span style="background:#e8f5e9;color:#2e7d32;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ payment_status_text != null && payment_status_text != "" ? payment_status_text : "Đã thanh toán" }}</span>
                        {{ else }}
                            <span style="background:#fff8e1;color:#f57f17;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ payment_status_text != null && payment_status_text != "" ? payment_status_text : "Chưa thanh toán" }}</span>
                        {{ end }}
                    </td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Phương thức thanh toán:</td>
                    <td>{{ payment_method_text }}</td>
                </tr>
                <tr style="background:#c8e6c9;font-size:14px;">
                    <td style="font-weight:700;color:#1b5e20;">Phí Caddie tổng cộng:</td>
                    <td style="font-weight:700;color:#1b5e20;font-size:16px;">{{ total_caddie_fee_text }}</td>
                </tr>
            </table>
        </td>
    </tr>

    <!-- FOOTER -->
    <tr>
        <td style="padding:16px 8px;text-align:center;font-style:italic;font-weight:700;color:#555555;background:#f9f9f9;border-top:1px solid #e0e0e0;">
            Vui lòng kiểm tra và sắp xếp Caddie phục vụ khách hàng đúng thời gian đặt!
        </td>
    </tr>
</table>