(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;

    function formatCurrency(v) {
        if (!v && v !== 0) return '';
        return new Intl.NumberFormat('vi-VN').format(v) + 'đ';
    }

    function getStatusBadge(status) {
        if (!status) return '<span class="badge hl-badge-default">-</span>';
        var s = status.toLowerCase();
        var cls = 'hl-badge-default';
        if (s.includes('khởi tạo') || s.includes('khoi tao')) cls = 'hl-badge-created';
        else if (s.includes('đang xử lý') || s.includes('dang xu ly') || s.includes('processing')) cls = 'hl-badge-processing';
        else if (s.includes('hoàn thành') || s.includes('hoan thanh') || s.includes('completed')) cls = 'hl-badge-completed';
        else if (s.includes('thanh toán') || s.includes('thanh toan') || s.includes('paid')) cls = 'hl-badge-paid';
        else if (s.includes('hủy') || s.includes('huy') || s.includes('cancel')) cls = 'hl-badge-cancelled';
        else if (s.includes('từ chối') || s.includes('tu choi') || s.includes('reject')) cls = 'hl-badge-rejected';
        else if (s.includes('trả hàng') || s.includes('tra hang') || s.includes('return')) cls = 'hl-badge-returned';
        return '<span class="badge ' + cls + '">' + status + '</span>';
    }

    // Load KPIs
    service.getProducts(1, 1).then(function (r) {
        $('#KpiProducts').text(r.success && r.data ? r.data.totalRecords.toLocaleString('vi-VN') : '0');
    });
    service.getCustomers(1, 1).then(function (r) {
        $('#KpiCustomers').text(r.success && r.data ? r.data.totalRecords.toLocaleString('vi-VN') : '0');
    });
    service.getOrderHeaders(1, 1).then(function (r) {
        $('#KpiOrders').text(r.success && r.data ? r.data.totalRecords.toLocaleString('vi-VN') : '0');
    });
    service.getSalemans(1, 1).then(function (r) {
        $('#KpiSalemans').text(r.success && r.data ? r.data.totalRecords.toLocaleString('vi-VN') : '0');
    });

    // Recent Orders (headers)
    service.getOrderHeaders(1, 5).then(function (r) {
        var tbody = $('#RecentOrders');
        tbody.empty();
        if (r.success && r.data && r.data.data && r.data.data.length > 0) {
            r.data.data.forEach(function (item) {
                tbody.append(
                    '<tr>' +
                    '<td><code class="small">' + (item.orderNumber || '').substring(0, 25) + '</code></td>' +
                    '<td><small>' + (item.customerName || '').substring(0, 30) + '</small></td>' +
                    '<td><small>' + (item.orderDate || '') + '</small></td>' +
                    '<td>' + getStatusBadge(item.orderStatus) + '</td>' +
                    '</tr>'
                );
            });
        } else {
            tbody.append('<tr><td colspan="4" class="text-center text-muted">Không có dữ liệu</td></tr>');
        }
    });

    // Recent Products
    service.getProducts(1, 5).then(function (r) {
        var tbody = $('#RecentProducts');
        tbody.empty();
        if (r.success && r.data && r.data.data && r.data.data.length > 0) {
            r.data.data.forEach(function (item) {
                tbody.append(
                    '<tr>' +
                    '<td><code>' + (item.productCode || '') + '</code></td>' +
                    '<td><small>' + (item.productName || '').substring(0, 40) + '</small></td>' +
                    '<td><small>' + (item.brandName || '') + '</small></td>' +
                    '<td class="text-end">' + formatCurrency(item.productPrice) + '</td>' +
                    '</tr>'
                );
            });
        } else {
            tbody.append('<tr><td colspan="4" class="text-center text-muted">Không có dữ liệu</td></tr>');
        }
    });
})();
