(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;
    var orderService = genora.multiTenancy.appServices.hoaLinh.hlOrder;
    var currentPage = 1, totalPages = 0, totalRecords = 0, allData = [];

    flatpickr('#FilterDateFrom', { dateFormat: 'd/m/Y', allowInput: true });
    flatpickr('#FilterDateTo', { dateFormat: 'd/m/Y', allowInput: true });

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }
    function formatCurrency(v) { if (!v && v !== 0) return ''; return new Intl.NumberFormat('vi-VN').format(v) + 'đ'; }
    function parseDateVN(str) { if (!str) return null; var p = str.split('/'); return p.length === 3 ? p[2] + '-' + p[1] + '-' + p[0] : null; }

    var statusMap = { 1: 'Khởi tạo', 2: 'Đang xử lý', 3: 'Hoàn thành', 4: 'Đã thanh toán', 5: 'Đã hủy', 6: 'Từ chối', 7: 'Đã trả hàng' };
    var deliveryStatusMap = { 1: 'Đơn mới', 2: 'Đang xử lý', 3: 'Đang giao', 4: 'Hoàn thành', 5: 'Đã hủy' };
    var paymentStatusMap = { 1: 'Chưa thanh toán', 2: 'Đã thanh toán', 3: 'Công nợ' };

    function getStatusBadge(status, statusCode) {
        var text = status || statusMap[statusCode] || deliveryStatusMap[statusCode] || '-';
        var s = text.toLowerCase(), cls = 'hl-badge-default';
        if (s.includes('đơn mới') || s.includes('khởi tạo')) cls = 'hl-badge-created';
        else if (s.includes('đang xử lý') || s.includes('đang giao')) cls = 'hl-badge-processing';
        else if (s.includes('hoàn thành')) cls = 'hl-badge-completed';
        else if (s.includes('thanh toán')) cls = 'hl-badge-paid';
        else if (s.includes('hủy')) cls = 'hl-badge-cancelled';
        else if (s.includes('từ chối')) cls = 'hl-badge-rejected';
        else if (s.includes('trả hàng')) cls = 'hl-badge-returned';
        return '<span class="badge ' + cls + '">' + text + '</span>';
    }

    function getFilteredData() {
        var search = ($('#FilterText').val() || '').toLowerCase();
        var source = $('#FilterSource').val();
        var status = $('#FilterStatus').val();
        var dateFrom = parseDateVN($('#FilterDateFrom').val());
        var dateTo = parseDateVN($('#FilterDateTo').val());
        var filtered = allData;
        if (source === 'genora') filtered = filtered.filter(function (i) { return i._source === 'genora'; });
        else if (source === 'hoalinh') filtered = filtered.filter(function (i) { return i._source === 'hoalinh'; });
        if (search) filtered = filtered.filter(function (i) {
            return (i._orderCode && i._orderCode.toLowerCase().includes(search)) ||
                (i._customerName && i._customerName.toLowerCase().includes(search)) ||
                (i._dsrName && i._dsrName.toLowerCase().includes(search));
        });
        if (status) { var sc = parseInt(status); filtered = filtered.filter(function (i) { return i._statusCode === sc; }); }
        if (dateFrom) filtered = filtered.filter(function (i) { return i._orderDate && i._orderDate >= dateFrom; });
        if (dateTo) filtered = filtered.filter(function (i) { return i._orderDate && i._orderDate <= dateTo; });
        return filtered;
    }

    function renderTable() {
        var filtered = getFilteredData();
        totalRecords = filtered.length;
        var ps = getPageSize();
        totalPages = Math.ceil(totalRecords / ps) || 1;
        if (currentPage > totalPages) currentPage = totalPages;
        var pageData = filtered.slice((currentPage - 1) * ps, currentPage * ps);
        var tbody = $('#HlOrdersTable tbody'); tbody.empty();
        if (pageData.length === 0) { tbody.append('<tr><td colspan="7" class="text-center text-muted py-4">Không có dữ liệu</td></tr>'); }
        else {
            pageData.forEach(function (item) {
                var sourceBadge = item._source === 'genora'
                    ? '<span class="badge hl-badge-genora">Genora</span>'
                    : '<span class="badge hl-badge-hoalinh">Hoa Linh</span>';
                var statusHtml = getStatusBadge(item._statusText, item._statusCode);
                // Icon cập nhật trạng thái chỉ cho Genora orders
                if (item._source === 'genora') {
                    statusHtml += ' <button class="btn btn-outline-secondary hl-btn-status btn-update-status" data-id="' + item._id + '" data-ds="' + (item._raw.deliveryStatus || 1) + '" data-ps="' + (item._raw.paymentStatus || 1) + '" title="Cập nhật trạng thái"><i class="fa fa-pen"></i></button>';
                }
                var salesCol = item._source === 'genora' ? (item._raw.receiverName || '') : (item._dsrName || '');
                tbody.append(
                    '<tr class="hl-clickable" data-source="' + item._source + '" data-id="' + (item._id || '') + '" data-order="' + (item._orderCode || '') + '">' +
                    '<td class="text-center">' + sourceBadge + '</td>' +
                    '<td><code class="small">' + (item._orderCode || '') + '</code></td>' +
                    '<td><small>' + (item._customerName || '') + '</small></td>' +
                    '<td class="text-end fw-semibold">' + formatCurrency(item._totalAmount) + '</td>' +
                    '<td>' + statusHtml + '</td>' +
                    '<td><small>' + (item._orderDate || '') + '</small></td>' +
                    '<td><small>' + salesCol + '</small></td>' +
                    '</tr>'
                );
            });
        }
        renderPagination();
    }

    function renderPagination() {
        var paging = $('#Pagination'); paging.empty();
        var showing = Math.min(getPageSize(), totalRecords - (currentPage - 1) * getPageSize());
        $('#PagingInfo').html('Hiển thị <strong>' + (showing > 0 ? showing : 0) + '</strong> / <strong>' + totalRecords + '</strong> đơn hàng');
        if (totalPages <= 1) return;
        paging.append('<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Previous</a></li>');
        var s = Math.max(1, currentPage - 2), e = Math.min(totalPages, s + 4); if (e - s < 4) s = Math.max(1, e - 4);
        for (var i = s; i <= e; i++) paging.append('<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>');
        paging.append('<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next</a></li>');
    }

    function loadData() {
        abp.ui.setBusy('#OrderTableContainer');
        allData = [];
        var p1 = service.getOrderHeaders(1, 500).then(function (r) {
            var hlOrders = (r.success && r.data) ? (r.data.data || r.data) : [];
            if (!Array.isArray(hlOrders)) hlOrders = [];
            hlOrders.forEach(function (o) {
                allData.push({
                    _source: 'hoalinh', _id: o.orderNumber, _orderCode: o.orderNumber,
                    _customerName: o.customerName, _totalAmount: o.totalAmount,
                    _statusCode: o.orderStatusCode, _statusText: o.orderStatus,
                    _orderDate: o.orderDate, _dsrName: o.dsrName, _raw: o
                });
            });
        });
        var p2 = orderService.getList({ skipCount: 0, maxResultCount: 500 }).then(function (r) {
            if (r && r.items) {
                r.items.forEach(function (o) {
                    allData.push({
                        _source: 'genora', _id: o.id, _orderCode: o.orderCode,
                        _customerName: o.customerName, _totalAmount: o.totalAmount,
                        _statusCode: o.deliveryStatus, _statusText: deliveryStatusMap[o.deliveryStatus] || '',
                        _orderDate: o.creationTime ? o.creationTime.substring(0, 10) : '',
                        _dsrName: '', _raw: o
                    });
                });
            }
        });
        Promise.all([p1, p2]).then(function () {
            allData.sort(function (a, b) { return (b._orderDate || '').localeCompare(a._orderDate || ''); });
            currentPage = 1; renderTable();
        }).catch(function (err) { console.error(err); renderTable(); })
          .finally(function () { abp.ui.clearBusy('#OrderTableContainer'); });
    }

    function showDetail(source, id, orderCode) {
        var body = $('#OrderDetailBody'); body.empty();
        if (source === 'genora') {
            orderService.get(id).then(function (o) {
                if (!o) { body.html('<p class="text-muted text-center">Không tìm thấy</p>'); return; }
                var payMethod = { 1: 'Tiền mặt (COD)', 2: 'Chuyển khoản' };
                var info = '<div class="row mb-3"><div class="col-md-6"><table class="table table-sm table-borderless">' +
                    '<tr><td class="text-muted">Mã đơn:</td><td><strong>' + (o.orderCode || '') + '</strong> <span class="badge hl-badge-genora">Genora</span></td></tr>' +
                    '<tr><td class="text-muted">Khách hàng:</td><td>' + (o.customerName || '') + '</td></tr>' +
                    '<tr><td class="text-muted">SĐT:</td><td>' + (o.customerPhone || '') + '</td></tr>' +
                    '<tr><td class="text-muted">Chi nhánh:</td><td>' + (o.branchName || '') + '</td></tr>' +
                    '<tr><td class="text-muted">Địa chỉ giao:</td><td>' + (o.deliveryAddress || '') + '</td></tr>' +
                    '<tr><td class="text-muted">Người nhận:</td><td>' + (o.receiverName || '') + ' - ' + (o.receiverPhone || '') + '</td></tr>' +
                    '</table></div><div class="col-md-6"><table class="table table-sm table-borderless">' +
                    '<tr><td class="text-muted">Trạng thái đơn hàng:</td><td>' + getStatusBadge(deliveryStatusMap[o.deliveryStatus], null) + '</td></tr>' +
                    '<tr><td class="text-muted">Trạng thái thanh toán:</td><td>' + getStatusBadge(paymentStatusMap[o.paymentStatus], null) + '</td></tr>' +
                    '<tr><td class="text-muted">Phương thức:</td><td>' + (payMethod[o.paymentMethod] || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">Ngày tạo:</td><td>' + (o.creationTime ? new Date(o.creationTime).toLocaleString('vi-VN') : '') + '</td></tr>' +
                    '<tr><td class="text-muted">Ghi chú:</td><td>' + (o.note || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">Tạm tính:</td><td>' + formatCurrency(o.subTotal) + '</td></tr>' +
                    '<tr><td class="text-muted">Giảm giá:</td><td class="text-danger">-' + formatCurrency(o.discountAmount) + '</td></tr>' +
                    '<tr><td class="text-muted"><strong>Tổng thanh toán:</strong></td><td><strong class="text-primary">' + formatCurrency(o.totalAmount) + '</strong></td></tr>' +
                    '</table></div></div>';
                var items = '';
                if (o.items && o.items.length > 0) {
                    items = '<h6 class="mb-2">Sản phẩm (' + o.items.length + ')</h6><table class="table table-sm table-bordered"><thead class="table-light"><tr><th>Sản phẩm</th><th>Thương hiệu</th><th>ĐVT</th><th class="text-end">Đơn giá</th><th class="text-center">SL</th><th class="text-end">Thành tiền</th></tr></thead><tbody>';
                    o.items.forEach(function (i) { items += '<tr><td>' + (i.productName || '') + '<br><small class="text-muted">' + (i.productCode || '') + '</small></td><td><small>' + (i.brandName || '') + '</small></td><td>' + (i.productUnit || '') + '</td><td class="text-end">' + formatCurrency(i.price) + '</td><td class="text-center">' + (i.quantity || '') + '</td><td class="text-end fw-bold">' + formatCurrency(i.amount) + '</td></tr>'; });
                    items += '</tbody></table>';
                }
                body.html(info + items);
            });
        } else {
            service.getOrderDetail(orderCode).then(function (r) {
                if (!r.success || !r.data || r.data.length === 0) { body.html('<p class="text-muted text-center">Không tìm thấy</p>'); return; }
                var first = r.data[0];
                var info = '<div class="row mb-3"><div class="col-md-6"><table class="table table-sm table-borderless">' +
                    '<tr><td class="text-muted">Mã đơn:</td><td><strong>' + (first.orderNumber || '') + '</strong> <span class="badge hl-badge-hoalinh">Hoa Linh</span></td></tr>' +
                    '<tr><td class="text-muted">Khách hàng:</td><td>' + (first.customerName || '') + '</td></tr>' +
                    '<tr><td class="text-muted">Mã KH:</td><td><code>' + (first.customerCode || '') + '</code></td></tr>' +
                    '<tr><td class="text-muted">Địa chỉ:</td><td>' + (first.deliveryAddress || '') + '</td></tr>' +
                    '</table></div><div class="col-md-6"><table class="table table-sm table-borderless">' +
                    '<tr><td class="text-muted">Trạng thái:</td><td>' + getStatusBadge(first.orderStatus, null) + '</td></tr>' +
                    '<tr><td class="text-muted">Ngày đặt:</td><td>' + (first.orderDate || '') + ' ' + (first.orderTime || '') + '</td></tr>' +
                    '<tr><td class="text-muted">Ngày giao:</td><td>' + (first.deliveryDate || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">NV Sales:</td><td>' + (first.dsrName || '') + '</td></tr>' +
                    '</table></div></div>';
                var items = '<h6 class="mb-2">Sản phẩm (' + r.data.length + ')</h6><table class="table table-sm table-bordered"><thead class="table-light"><tr><th>Sản phẩm</th><th>ĐVT</th><th class="text-end">Đơn giá</th><th class="text-center">SL</th><th class="text-end">Giá trị</th><th class="text-end">Chiết khấu</th><th class="text-end">Thành tiền</th><th>Loại</th></tr></thead><tbody>';
                r.data.forEach(function (i) { items += '<tr><td>' + (i.productName || '') + '</td><td>' + (i.productUnit || '') + '</td><td class="text-end">' + formatCurrency(i.productPrice) + '</td><td class="text-center">' + (i.quantity || '') + '</td><td class="text-end">' + formatCurrency(i.grossValue) + '</td><td class="text-end text-danger">' + (i.schemeValue ? '-' + formatCurrency(i.schemeValue) : '-') + '</td><td class="text-end fw-bold">' + formatCurrency(i.totalAmount) + '</td><td><small>' + (i.productSaleType || '') + '</small></td></tr>'; });
                items += '</tbody></table>';
                body.html(info + items);
            });
        }
        new bootstrap.Modal(document.getElementById('OrderDetailModal')).show();
    }

    // Update Status
    function openUpdateStatus(id, ds, ps) {
        $('#UpdateOrderId').val(id);
        $('input[name="deliveryStatus"][value="' + ds + '"]').prop('checked', true);
        $('input[name="paymentStatus"][value="' + ps + '"]').prop('checked', true);
        $('#UpdateNote').val('');
        new bootstrap.Modal(document.getElementById('UpdateStatusModal')).show();
    }

    $('#BtnSaveStatus').click(function () {
        var id = $('#UpdateOrderId').val();
        var ds = parseInt($('input[name="deliveryStatus"]:checked').val()) || null;
        var ps = parseInt($('input[name="paymentStatus"]:checked').val()) || null;
        var note = $('#UpdateNote').val() || null;

        orderService.updateStatus({ id: id, deliveryStatus: ds, paymentStatus: ps, internalNote: note })
            .then(function () {
                abp.notify.success('Cập nhật trạng thái thành công');
                bootstrap.Modal.getInstance(document.getElementById('UpdateStatusModal')).hide();
                loadData();
            })
            .catch(function (err) { abp.notify.error('Lỗi: ' + (err.message || '')); });
    });

    // Events
    $('#BtnSearch').click(function () { currentPage = 1; renderTable(); });
    $('#BtnRefresh').click(function () { $('#FilterText').val(''); $('#FilterSource').val(''); $('#FilterStatus').val(''); $('#FilterDateFrom').val(''); $('#FilterDateTo').val(''); loadData(); });
    $('#FilterText').keypress(function (e) { if (e.which === 13) { currentPage = 1; renderTable(); } });
    $('#PageSize').change(function () { currentPage = 1; renderTable(); });
    $(document).on('click', '#Pagination .page-link', function (e) { e.preventDefault(); var p = parseInt($(this).data('page')); if (p >= 1 && p <= totalPages && p !== currentPage) { currentPage = p; renderTable(); } });
    $(document).on('click', '#HlOrdersTable tbody tr.hl-clickable', function (e) {
        if ($(e.target).closest('.btn-update-status').length) return; // Skip if clicking update button
        var source = $(this).data('source'), id = $(this).data('id'), order = $(this).data('order');
        showDetail(source, id, order);
    });
    $(document).on('click', '.btn-update-status', function (e) {
        e.stopPropagation();
        openUpdateStatus($(this).data('id'), $(this).data('ds'), $(this).data('ps'));
    });

    loadData();
})();
