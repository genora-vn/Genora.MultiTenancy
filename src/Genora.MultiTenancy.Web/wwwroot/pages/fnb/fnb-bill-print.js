(function (window) {
    function toViMoney(value) {
        const num = Number(value || 0);
        return num.toLocaleString('vi-VN');
    }

    function escapeHtml(text) {
        return String(text || '')
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#039;');
    }

    function buildRows(items) {
        if (!Array.isArray(items)) return '';

        return items.map(function (x) {
            return `
<tr>
    <td class="col-name">${escapeHtml(x.itemName)}</td>
    <td class="col-qty">${x.quantity || 0}</td>
    <td class="col-price">${toViMoney(x.price)}</td>
    <td class="col-amount">${toViMoney(x.amount)}</td>
</tr>`;
        }).join('');
    }

    function buildBillHtml(payload, paperWidth) {
        const widthMm = Number(paperWidth || 80) <= 57 ? 57 : 80;
        const rows = buildRows(payload.items || []);

        return `
<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="UTF-8" />
<title>In hóa đơn</title>
<style>
* { box-sizing: border-box; margin: 0; padding: 0; font-family: 'Courier New', Courier, monospace; font-size: 13px; color: #000; }
body { background-color: #fff; padding: 0; }
.bill-container { width: ${widthMm}mm; background: #fff; padding: 8px 10px; margin: 0 auto; }
.text-center { text-align: center; }
.bold { font-weight: bold; }
.header { margin-bottom: 12px; }
.header h1 { font-size: 16px; text-transform: uppercase; margin-bottom: 4px; }
.header p { font-size: 12px; line-height: 1.4; }
.divider { border-top: 1px dashed #000; margin: 8px 0; }
.info-section { line-height: 1.5; margin-bottom: 8px; }
.info-row { display: flex; justify-content: space-between; gap: 10px; }
.items-table { width: 100%; border-collapse: collapse; margin-bottom: 8px; }
.items-table th, .items-table td { padding: 5px 0; vertical-align: top; font-size: ${widthMm <= 57 ? '11px' : '12px'}; }
.items-table th { border-bottom: 1px solid #000; text-align: left; }
.col-qty { width: 12%; text-align: center; }
.col-price { width: 28%; text-align: right; }
.col-amount { width: 28%; text-align: right; }
.col-name { width: 32%; }
.totals-section { line-height: 1.6; }
.footer { margin-top: 12px; line-height: 1.4; font-size: 12px; }
@media print {
    @page { size: ${widthMm}mm auto; margin: 0; }
    body { background-color: #fff; padding: 0; margin: 0; }
    .bill-container { width: 100%; box-shadow: none; padding: 4px 6px; }
}
</style>
</head>
<body>
<div class="bill-container">
    <div class="header text-center">
        <h1 class="bold">${escapeHtml(payload.shopName || 'LAGUNA GOLF LĂNG CÔ')}</h1>
        <p>${escapeHtml(payload.shopAddress || 'Xã Lộc Vĩnh, Phú Lộc, Thừa Thiên Huế')}</p>
        <p>Hotline: ${escapeHtml(payload.shopPhone || '0234.3695.888')}</p>
        <p class="bold">${escapeHtml(payload.kioskLabel || '--- F&B KIOSK #09 ---')}</p>
    </div>

    <div class="divider"></div>

    <div class="info-section">
        <div class="info-row"><span>Số HD: <span class="bold">${escapeHtml(payload.orderCode || '')}</span></span></div>
        <div class="info-row"><span>Thời gian: ${escapeHtml(payload.creationTimeText || '')}</span></div>
        <div class="info-row"><span>Thu ngân: ${escapeHtml(payload.cashierName || 'Admin')}</span></div>
    </div>

    <div class="divider"></div>

    <div class="info-section">
        <div class="info-row"><span>Golfer: <span class="bold">${escapeHtml(payload.customerName || 'Khách lẻ')}</span></span></div>
        <div class="info-row"><span>Loại KH: ${escapeHtml(payload.customerTypeName || 'Khách lẻ')}</span></div>
        <div class="info-row"><span>Số Bag: ${escapeHtml(payload.bagTag || '')}</span></div>
    </div>

    <div class="divider"></div>

    <table class="items-table">
        <thead>
            <tr>
                <th class="col-name">Món</th>
                <th class="col-qty">SL</th>
                <th class="col-price">Giá</th>
                <th class="col-amount">T.Tiền</th>
            </tr>
        </thead>
        <tbody>${rows}</tbody>
    </table>

    <div class="divider"></div>

    <div class="totals-section">
        <div class="info-row"><span>Cộng tiền hàng:</span><span class="bold">${toViMoney(payload.subtotal)}</span></div>
        <div class="info-row"><span>Phí dịch vụ:</span><span>${toViMoney(payload.serviceFee)}</span></div>
        <div class="info-row"><span>Thuế VAT:</span><span>${toViMoney(payload.vat)}</span></div>
        <div class="info-row"><span>Giảm giá:</span><span>${toViMoney(payload.discount)}</span></div>
        <div class="divider"></div>
        <div class="info-row" style="font-size: 15px;">
            <span class="bold">TỔNG CỘNG (VNĐ):</span>
            <span class="bold">${toViMoney(payload.total)}</span>
        </div>
    </div>

    <div class="divider"></div>

    <div class="info-section">
        <div class="info-row"><span>Trạng thái: <span class="bold">${escapeHtml(payload.serviceStatusText || '')}</span></span></div>
        <div class="info-row"><span>Ghi chú: ${escapeHtml(payload.note || '—')}</span></div>
    </div>

    <div class="footer text-center">
        <p class="bold">CẢM ƠN QUÝ KHÁCH!</p>
        <p>Hẹn gặp lại anh tại Tee-box.</p>
        <p style="font-style: italic; font-size: 11px; margin-top: 5px;">Powered by Genora</p>
    </div>
</div>
</body>
</html>`;
    }

    window.fnbPrintOrderBill = function (payload, paperWidth) {
        const billHtml = buildBillHtml(payload, paperWidth);
        const printWindow = window.open('', '_blank', 'width=420,height=760');
        if (!printWindow) return;

        printWindow.document.open();
        printWindow.document.write(billHtml);
        printWindow.document.close();

        setTimeout(function () {
            printWindow.focus();
            printWindow.print();
        }, 300);
    };

    window.fnbBuildBillHtml = buildBillHtml;
})(window);