(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;
    var phone = window.customerPhone;
    if (!phone) { abp.notify.error('Thiếu số điện thoại khách hàng'); return; }

    function formatCurrency(v) { if (!v && v !== 0) return '0'; return new Intl.NumberFormat('vi-VN').format(v); }

    function getStatusBadge(status) {
        if (!status) return '-';
        var s = status.toLowerCase(), cls = 'hl-badge-default';
        if (s.includes('hoàn thành')) cls = 'hl-badge-completed';
        else if (s.includes('đang xử lý')) cls = 'hl-badge-processing';
        else if (s.includes('thanh toán')) cls = 'hl-badge-paid';
        else if (s.includes('hủy')) cls = 'hl-badge-cancelled';
        else if (s.includes('khởi tạo')) cls = 'hl-badge-created';
        return '<span class="badge ' + cls + '">' + status + '</span>';
    }

    // Load profile
    service.getCustomerDetail(phone).then(function (r) {
        if (!r.success || !r.data || r.data.length === 0) { $('#CustName').text('Không tìm thấy'); return; }
        var c = r.data[0];
        $('#CustName').text(c.custName || '-');
        $('#CustCode').text(c.custCode || '');
        $('#CustPhone').text(c.custPhone || phone);
        $('#CustChannel').text(c.custChannel || '');

        if (c.membershipTier) {
            $('#TierBadge').text('⭐ ' + c.membershipTier).show();
        }

        // KPIs
        $('#KpiSales').text(formatCurrency(c.accumulatedSales));
        $('#KpiPoints').text((c.accumulatedPoints || 0).toLocaleString('vi-VN'));
        $('#KpiTierSub').text(c.membershipTier ? '🏆 ' + c.membershipTier : '');
        $('#KpiNextPoints').text(c.pointsToNextTier != null ? c.pointsToNextTier.toLocaleString('vi-VN') : '0');
        $('#KpiNextTier').text(c.nextMembershipTier ? 'Hạng ' + c.nextMembershipTier : '');
        $('#KpiGkhl').html(c.isGkhl ? '<span class="text-success">Có</span>' : '<span class="text-muted">Không</span>');
        $('#KpiGkhlStatus').text(c.gkhlContractStatus || '');

        // Info
        $('#InfoChannel').text(c.custChannel || '-');
        $('#InfoSubChannel').text(c.custSubChannel || '-');
        $('#InfoGroup').text(c.custGroup || '-');
        $('#InfoAddress').text(c.address || '-');
        $('#InfoBirthday').text(c.birthday || '-');
        $('#InfoDsr').text((c.dsrName || '-') + (c.dsrCode ? ' (' + c.dsrCode + ')' : ''));
        $('#InfoDistributor').text(c.distributorName || '-');

        // Load orders
        loadOrders(c.custCode);
        // Load branches
        loadBranches(phone);
    });

    function loadOrders(custCode) {
        if (!custCode) { $('#OrdersBody').html('<tr><td colspan="5" class="text-muted text-center">Không có dữ liệu</td></tr>'); return; }
        service.getOrderHeaders(1, 500).then(function (r) {
            var tbody = $('#OrdersBody'); tbody.empty();
            var orders = (r.success && r.data) ? (r.data.data || r.data) : [];
            if (!Array.isArray(orders)) orders = [];
            orders = orders.filter(function (o) { return o.customerCode === custCode; });
            if (orders.length === 0) { tbody.append('<tr><td colspan="5" class="text-muted text-center">Chưa có đơn hàng</td></tr>'); return; }
            orders.slice(0, 20).forEach(function (o) {
                tbody.append('<tr><td><code class="small">' + (o.orderNumber || '').substring(0, 25) + '</code></td><td><small>' + (o.orderDate || '') + '</small></td><td class="text-end fw-semibold">' + formatCurrency(o.totalAmount) + 'đ</td><td><small>' + (o.dsrName || '') + '</small></td><td>' + getStatusBadge(o.orderStatus) + '</td></tr>');
            });
            $('#OrdersPagingInfo').html('Hiển thị <strong>' + Math.min(20, orders.length) + '</strong> / <strong>' + orders.length + '</strong> đơn hàng');
        });
    }

    function loadBranches(phone) {
        service.getCustomerDetail(phone).then(function (r) {
            var tbody = $('#BranchesBody'); tbody.empty();
            var branches = (r.success && r.data) ? r.data : [];
            if (branches.length <= 1) { tbody.append('<tr><td colspan="5" class="text-muted text-center">Không có chi nhánh khác</td></tr>'); return; }
            branches.forEach(function (b) {
                tbody.append('<tr><td><code>' + (b.custCode || '') + '</code></td><td>' + (b.custName || '') + '</td><td><small>' + (b.address || '') + '</small></td><td>' + (b.custChannel || '') + '</td><td><small>' + (b.dsrName || '') + '</small></td></tr>');
            });
        });
    }
})();
