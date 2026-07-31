<table cellpadding="0" cellspacing="0" border="0" width="600" style="width:600px;border-collapse:collapse;font-family:Arial,sans-serif;font-size:14px;color:#000;border:1px solid #b7b7b7;">
    <!-- HEADER -->
    <tr>
        <td style="background:#355f93;color:#fff;text-align:center;padding:12px 16px 6px 16px;font-size:18px;font-weight:700;text-transform:uppercase;">
            THÔNG BÁO ĐƠN HÀNG MỚI
        </td>
    </tr>
    <tr>
        <td style="background:#355f93;color:#fff;text-align:center;padding:0 16px 10px 16px;font-size:14px;font-weight:600;">
            {{ model.branch_name ?? model.BranchName }}
        </td>
    </tr>

    <tr><td style="background:#e9eef4;height:8px;"></td></tr>

    <!-- THÔNG TIN ĐƠN HÀNG -->
    <tr><td style="background:#cfd9e6;padding:6px 8px;font-weight:700;text-transform:uppercase;">THÔNG TIN ĐƠN HÀNG</td></tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="0" style="border-collapse:collapse;font-size:13px;">
                <tr style="background:#dfe6ee;">
                    <td width="38%" style="font-weight:600;">Mã đơn:</td>
                    <td><b>{{ model.order_code ?? model.OrderCode }}</b></td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Trạng thái đơn hàng:</td>
                    <td><span style="background:#e3f2fd;color:#0d47a1;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ model.order_status_text ?? model.OrderStatusText }}</span></td>
                </tr>
                <tr style="background:#dfe6ee;">
                    <td style="font-weight:600;">Ngày tạo:</td>
                    <td>{{ model.creation_time_text ?? model.CreationTimeText }}</td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Chi nhánh:</td>
                    <td>{{ model.branch_name ?? model.BranchName }}</td>
                </tr>
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <!-- THÔNG TIN KHÁCH HÀNG & GIAO HÀNG -->
    <tr><td style="background:#cfd9e6;padding:6px 8px;font-weight:700;text-transform:uppercase;">THÔNG TIN KHÁCH HÀNG & GIAO HÀNG</td></tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="0" style="border-collapse:collapse;font-size:13px;">
                <tr style="background:#dfe6ee;">
                    <td width="38%" style="font-weight:600;">Khách hàng:</td>
                    <td>{{ model.customer_name ?? model.CustomerName }}</td>
                </tr>
                <tr>
                    <td style="font-weight:600;">SĐT khách hàng:</td>
                    <td>{{ model.customer_phone ?? model.CustomerPhone }}</td>
                </tr>
                <tr style="background:#dfe6ee;">
                    <td style="font-weight:600;">Người nhận:</td>
                    <td><b>{{ model.receiver_name ?? model.ReceiverName }}</b> - {{ model.receiver_phone ?? model.ReceiverPhone }}</td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Địa chỉ giao:</td>
                    <td>{{ model.shipping_address ?? model.ShippingAddress }}</td>
                </tr>
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <!-- DANH SÁCH SẢN PHẨM -->
    <tr>
        <td style="background:#cfd9e6;padding:6px 8px;font-weight:700;text-transform:uppercase;">
            CHI TIẾT SẢN PHẨM ({{ model.total_items_count ?? model.TotalItemsCount }})
        </td>
    </tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="1" bordercolor="#e0e0e0" style="border-collapse:collapse;font-size:13px;">
                <thead>
                    <tr style="background:#d8d8d8;text-align:left;">
                        <th style="padding:6px;">Sản phẩm</th>
                        <th style="padding:6px;">Thương hiệu</th>
                        <th style="padding:6px;text-align:center;">ĐVT</th>
                        <th style="padding:6px;text-align:right;">Đơn giá</th>
                        <th style="padding:6px;text-align:center;">SL</th>
                        <th style="padding:6px;text-align:right;">Thành tiền</th>
                    </tr>
                </thead>
                <tbody>
                    {{ for item in (model.items ?? model.Items) }}
                    <tr>
                        <td style="padding:6px;">
                            <b>{{ item.product_name ?? item.ProductName }}</b><br/>
                            <span style="color:#777;font-size:11px;">Mã: {{ item.product_code ?? item.ProductCode }}</span>
                        </td>
                        <td style="padding:6px;">{{ item.brand_name ?? item.BrandName }}</td>
                        <td style="padding:6px;text-align:center;">{{ item.product_unit ?? item.ProductUnit }}</td>
                        <td style="padding:6px;text-align:right;">{{ item.unit_price_text ?? item.UnitPriceText }}</td>
                        <td style="padding:6px;text-align:center;">{{ item.quantity ?? item.Quantity }}</td>
                        <td style="padding:6px;text-align:right;"><b>{{ item.total_price_text ?? item.TotalPriceText }}</b></td>
                    </tr>
                    {{ end }}
                </tbody>
            </table>
        </td>
    </tr>

    <tr><td style="background:#efefef;height:8px;"></td></tr>

    <!-- THANH TOÁN -->
    <tr><td style="background:#cfd9e6;padding:6px 8px;font-weight:700;text-transform:uppercase;">THANH TOÁN</td></tr>
    <tr>
        <td style="padding:0;">
            <table width="100%" cellpadding="6" cellspacing="0" border="0" style="border-collapse:collapse;font-size:13px;">
                <tr style="background:#dfe6ee;">
                    <td width="38%" style="font-weight:600;">Trạng thái thanh toán:</td>
                    <td><span style="background:#fff8e1;color:#f57f17;padding:2px 8px;border-radius:10px;font-size:12px;font-weight:bold;">{{ model.payment_status_text ?? model.PaymentStatusText }}</span></td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Phương thức:</td>
                    <td>{{ model.payment_method_text ?? model.PaymentMethodText }}</td>
                </tr>
                <tr style="background:#dfe6ee;">
                    <td style="font-weight:600;">Tạm tính:</td>
                    <td>{{ model.sub_total_text ?? model.SubTotalText }}đ</td>
                </tr>
                <tr>
                    <td style="font-weight:600;">Giảm giá:</td>
                    {{ discount = model.discount_text ?? model.DiscountText }}
                    {{ if discount != null && discount != "" && discount != "0" }}
                        <td>{{ discount }}</td>
                    {{ else }}
                        <td>0đ</td>
                    {{ end }}
                </tr>
                <tr style="background:#d8d8d8;font-size:14px;">
                    <td style="font-weight:700;color:#0d47a1;">Tổng thanh toán:</td>
                    <td style="font-weight:700;color:#0d47a1;font-size:15px;">{{ model.grand_total_text ?? model.GrandTotalText }}đ</td>
                </tr>
            </table>
        </td>
    </tr>

    <!-- FOOTER -->
    <tr>
        <td style="padding:16px 8px;text-align:center;font-style:italic;font-weight:700;color:#333;">
            Vui lòng kiểm tra và xử lý đơn hàng trên hệ thống!
        </td>
    </tr>
</table>