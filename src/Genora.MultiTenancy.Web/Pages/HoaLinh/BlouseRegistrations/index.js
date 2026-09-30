(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlBlouseAdmin;
    var sales = genora.hoaLinhSales;
    sales.initDates();
    var currentPage = 1, totalPages = 0, totalRecords = 0;
    var currentDetailId = null;

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }

    var statusNames = { 0: 'Chờ xác nhận', 1: 'Đã xác nhận', 2: 'Đang xử lý', 3: 'Đã giao', 4: 'Đã hủy', 5: 'Từ chối' };
    var statusClass = { 0: 'hl-bl-pending', 1: 'hl-bl-confirmed', 2: 'hl-bl-processing', 3: 'hl-bl-delivered', 4: 'hl-bl-cancelled', 5: 'hl-bl-rejected' };
    var styleNames = { 1: 'Nam', 2: 'Nữ' };
    var itemTypeNames = { 1: 'Tặng miễn phí', 2: 'Đổi điểm' };
    var businessTypeNames = { 1: 'Nhà thuốc', 2: 'Quầy thuốc', 3: 'Khác' };

    function getStatusBadge(status) {
        return '<span class="badge ' + (statusClass[status] || 'hl-bl-cancelled') + '">' + (statusNames[status] || '-') + '</span>';
    }

    function renderTable(items) {
        var tbody = $('#HlBlouseTable tbody'); tbody.empty();
        if (!items || items.length === 0) {
            tbody.append('<tr><td colspan="10" class="text-center text-muted py-4">Không có dữ liệu</td></tr>');
            return;
        }
        items.forEach(function (item) {
            var date = item.creationTime ? new Date(item.creationTime).toLocaleDateString('vi-VN') : '';
            tbody.append(
                '<tr>' +
                '<td><code>' + (item.registrationCode || '') + '</code></td>' +
                '<td><small>' + (item.customerName || '') + '<br><code>' + (item.customerCode || '') + '</code> · ' + (item.customerPhone || '') + '</small></td>' +
                '<td>' + (item.printedName || item.storeName || '') + '</td>' +
                '<td class="text-center">' + (item.freeQuantity || 0) + '</td>' +
                '<td class="text-center">' + (item.exchangeQuantity || 0) + '</td>' +
                '<td class="text-center"><strong>' + (item.totalQuantity || 0) + '</strong></td>' +
                '<td class="text-center">' + (item.totalPointsUsed || 0) + '</td>' +
                '<td>' + getStatusBadge(item.status) + '</td>' +
                '<td><small>' + date + '</small></td>' +
                '<td class="text-center"><button class="btn btn-sm btn-outline-primary btn-detail" data-id="' + item.id + '" title="Chi tiết"><i class="fa fa-eye"></i></button></td>' +
                '</tr>'
            );
        });
    }

    function renderPagination() {
        var paging = $('#Pagination'); paging.empty();
        var showing = Math.min(getPageSize(), totalRecords - (currentPage - 1) * getPageSize());
        $('#PagingInfo').html('Hiển thị <strong>' + (showing > 0 ? showing : 0) + '</strong> / <strong>' + totalRecords + '</strong> đơn');
        if (totalPages <= 1) return;
        paging.append('<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Previous</a></li>');
        var s = Math.max(1, currentPage - 2), e = Math.min(totalPages, s + 4); if (e - s < 4) s = Math.max(1, e - 4);
        for (var i = s; i <= e; i++) paging.append('<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>');
        paging.append('<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next</a></li>');
    }

    function getFilter() {
        var dates = sales.dates();
        if (!dates) return null;
        return Object.assign(dates, {
            filter: $('#FilterText').val() || null,
            status: $('#FilterStatus').val() !== '' ? parseInt($('#FilterStatus').val()) : null,
            skipCount: (currentPage - 1) * getPageSize(), maxResultCount: getPageSize()
        });
    }

    function loadData() {
        var filter = getFilter();
        if (!filter) return;
        abp.ui.setBusy('#BlouseContainer');
        service.getList(filter)
            .then(function (r) {
                totalRecords = r.totalCount || 0;
                totalPages = Math.ceil(totalRecords / getPageSize()) || 1;
                renderTable(r.items);
                renderPagination();
            })
            .catch(function (err) { abp.notify.error('Lỗi khi tải dữ liệu'); console.error(err); renderTable([]); renderPagination(); })
            .always(function () { abp.ui.clearBusy('#BlouseContainer'); });
    }

    function infoRow(label, value) {
        return '<div class="hl-vc-row"><span class="hl-vc-label">' + label + '</span>' +
               '<span class="hl-vc-value">' + (value != null && value !== '' ? value : '-') + '</span></div>';
    }

    function renderItemsTable(title, items) {
        if (!items || items.length === 0) return '';
        var rows = items.map(function (i) {
            return '<tr><td>' + (styleNames[i.style] || '') + '</td><td>' + (i.sizeCode || '') + '</td>' +
                '<td>' + (i.weightRange || '') + '</td><td class="text-center">' + (i.quantity || 0) + '</td>' +
                '<td class="text-center">' + (i.totalPoints || 0) + '</td></tr>';
        }).join('');
        return '<div class="hl-vc-title mt-2">' + title + '</div>' +
            '<table class="table table-sm table-bordered mb-2"><thead class="table-light"><tr>' +
            '<th>Dáng</th><th>Size</th><th>Cân nặng</th><th class="text-center">SL</th><th class="text-center">Điểm</th>' +
            '</tr></thead><tbody>' + rows + '</tbody></table>';
    }

    function showDetail(id) {
        service.get(id).then(function (item) {
            currentDetailId = id;
            var date = item.creationTime ? new Date(item.creationTime).toLocaleString('vi-VN') : '-';
            var businessType = item.businessType === 3 ? (item.businessTypeName || 'Khác') : (businessTypeNames[item.businessType] || '-');

            var left =
                '<div class="hl-vc-card">' +
                '<div class="hl-vc-title"><i class="fa fa-user me-2"></i>Người đăng ký</div>' +
                infoRow('Mã đơn', '<strong>' + (item.registrationCode || '') + '</strong>') +
                infoRow('Khách hàng', item.customerName) +
                infoRow('Mã KH', '<code>' + (item.customerCode || '-') + '</code>') +
                infoRow('SĐT', item.customerPhone) +
                infoRow('Đại diện tiếp nhận', item.receiverName) +
                infoRow('Địa chỉ giao nhận', item.deliveryAddress) +
                infoRow('Ngày tạo', date) +
                infoRow('Trạng thái', getStatusBadge(item.status)) +
                (item.internalNote ? infoRow('Ghi chú nội bộ', item.internalNote) : '') +
                '</div>';

            var right =
                '<div class="hl-vc-card">' +
                '<div class="hl-vc-title"><i class="fa fa-shirt me-2"></i>Thông tin in áo</div>' +
                infoRow('Loại hình KD', businessType) +
                infoRow('Tên cửa hàng', item.storeName) +
                infoRow('Tên in trên áo', '<strong>' + (item.printedName || '-') + '</strong>') +
                (item.note ? infoRow('Ghi chú thêm', item.note) : '') +
                infoRow('Áo tặng', item.freeQuantity) +
                infoRow('Áo đổi điểm', item.exchangeQuantity) +
                infoRow('Tổng áo', '<strong>' + (item.totalQuantity || 0) + '</strong>') +
                infoRow('Điểm quy đổi (ghi nhận)', item.totalPointsUsed) +
                '</div>';

            var free = (item.items || []).filter(function (i) { return i.itemType === 1; });
            var exchange = (item.items || []).filter(function (i) { return i.itemType === 2; });
            var itemsHtml = '<div class="hl-vc-card mt-3">' +
                renderItemsTable('<i class="fa fa-gift me-2"></i>Áo tặng miễn phí', free) +
                renderItemsTable('<i class="fa fa-coins me-2"></i>Áo đổi bằng điểm', exchange) +
                (free.length === 0 && exchange.length === 0 ? '<div class="text-muted text-center py-2">Không có dòng áo</div>' : '') +
                '</div>';

            $('#BlouseDetailBody').html('<div class="row g-3"><div class="col-md-6">' + left + '</div><div class="col-md-6">' + right + '</div></div>' + itemsHtml);
            $('#StatusSelect').val(item.status);
            $('#StatusNote').val('');
            new bootstrap.Modal(document.getElementById('BlouseDetailModal')).show();
        });
    }

    function updateStatus() {
        if (!currentDetailId) return;
        var status = parseInt($('#StatusSelect').val());
        var note = $('#StatusNote').val() || '';
        service.updateStatus({ id: currentDetailId, status: status, internalNote: note })
            .then(function () {
                abp.notify.success('Đã cập nhật trạng thái');
                bootstrap.Modal.getInstance(document.getElementById('BlouseDetailModal'))?.hide();
                loadData();
            })
            .catch(function (err) { abp.notify.error('Lỗi: ' + (err.message || '')); });
    }

    // Events
    $('#BtnExportExcel').click(function () {
        var filter = getFilter();
        if (filter) sales.download('api/app/hl-sales-excel/blouse-registrations', filter, this);
    });
    $('#BtnSearch').click(function () { currentPage = 1; loadData(); });
    $('#BtnRefresh').click(function () { $('#FilterText').val(''); $('#FilterStatus').val(''); sales.clearDates(); currentPage = 1; loadData(); });
    $('#FilterText').keypress(function (e) { if (e.which === 13) { currentPage = 1; loadData(); } });
    $('#PageSize').change(function () { currentPage = 1; loadData(); });
    $(document).on('click', '#Pagination .page-link', function (e) { e.preventDefault(); var p = parseInt($(this).data('page')); if (p >= 1 && p <= totalPages && p !== currentPage) { currentPage = p; loadData(); } });
    $(document).on('click', '.btn-detail', function () { showDetail($(this).data('id')); });
    $('#BtnUpdateStatus').click(updateStatus);

    loadData();
})();
