(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlGiftExchange;
    var currentPage = 1, totalPages = 0, totalRecords = 0;
    var currentDetailId = null;

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }

    var statusNames = { 1: 'Chờ xử lý', 2: 'Đã duyệt', 3: 'Từ chối', 4: 'Hoàn thành' };

    function getStatusBadge(status) {
        var text = statusNames[status] || '-';
        var cls = 'hl-ge-default';
        if (status === 1) cls = 'hl-ge-pending';
        else if (status === 2) cls = 'hl-ge-approved';
        else if (status === 3) cls = 'hl-ge-rejected';
        else if (status === 4) cls = 'hl-ge-completed';
        return '<span class="badge ' + cls + '">' + text + '</span>';
    }

    function renderTable(items) {
        var tbody = $('#HlGiftTable tbody'); tbody.empty();
        if (!items || items.length === 0) {
            tbody.append('<tr><td colspan="8" class="text-center text-muted py-4">Không có dữ liệu</td></tr>');
            return;
        }
        items.forEach(function (item) {
            var date = item.creationTime ? new Date(item.creationTime).toLocaleDateString('vi-VN') : '';
            var actions = '';
            if (item.status === 1) {
                actions = '<button class="btn btn-sm btn-outline-success btn-approve me-1" data-id="' + item.id + '" title="Duyệt"><i class="fa fa-check"></i></button>' +
                    '<button class="btn btn-sm btn-outline-danger btn-reject" data-id="' + item.id + '" title="Từ chối"><i class="fa fa-times"></i></button>';
            }
            tbody.append(
                '<tr>' +
                '<td><code>' + (item.exchangeCode || '') + '</code></td>' +
                '<td><small>' + (item.customerName || '') + '<br><code>' + (item.customerCode || '') + '</code></small></td>' +
                '<td>' + (item.giftName || '') + '</td>' +
                '<td class="text-center">' + (item.pointsRequired || 0) + '</td>' +
                '<td class="text-center">' + (item.quantity || 1) + '</td>' +
                '<td>' + getStatusBadge(item.status) + '</td>' +
                '<td><small>' + date + '</small></td>' +
                '<td class="text-center">' + actions + ' <button class="btn btn-sm btn-outline-primary btn-detail" data-id="' + item.id + '" title="Chi tiết"><i class="fa fa-eye"></i></button></td>' +
                '</tr>'
            );
        });
    }

    function renderPagination() {
        var paging = $('#Pagination'); paging.empty();
        var showing = Math.min(getPageSize(), totalRecords - (currentPage - 1) * getPageSize());
        $('#PagingInfo').html('Hiển thị <strong>' + (showing > 0 ? showing : 0) + '</strong> / <strong>' + totalRecords + '</strong> yêu cầu');
        if (totalPages <= 1) return;
        paging.append('<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Previous</a></li>');
        var s = Math.max(1, currentPage - 2), e = Math.min(totalPages, s + 4); if (e - s < 4) s = Math.max(1, e - 4);
        for (var i = s; i <= e; i++) paging.append('<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>');
        paging.append('<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next</a></li>');
    }

    function loadData() {
        var filter = $('#FilterText').val() || null;
        var status = $('#FilterStatus').val() ? parseInt($('#FilterStatus').val()) : null;
        var skipCount = (currentPage - 1) * getPageSize();

        abp.ui.setBusy('#GiftExchangeContainer');
        service.getList({ skipCount: skipCount, maxResultCount: getPageSize(), filter: filter, status: status })
            .then(function (r) {
                totalRecords = r.totalCount || 0;
                totalPages = Math.ceil(totalRecords / getPageSize()) || 1;
                renderTable(r.items);
                renderPagination();
            })
            .catch(function (err) { abp.notify.error('Lỗi khi tải dữ liệu'); console.error(err); renderTable([]); renderPagination(); })
            .always(function () { abp.ui.clearBusy('#GiftExchangeContainer'); });
    }

    function showDetail(id) {
        service.get(id).then(function (item) {
            currentDetailId = id;
            var body = $('#GiftDetailBody'); body.empty();
            var date = item.creationTime ? new Date(item.creationTime).toLocaleString('vi-VN') : '';
            var approvedDate = item.approvedAt ? new Date(item.approvedAt).toLocaleString('vi-VN') : '-';

            body.html(
                '<div class="row"><div class="col-md-6"><table class="table table-sm table-borderless">' +
                '<tr><td class="text-muted" style="width:130px">Mã yêu cầu:</td><td><strong>' + (item.exchangeCode || '') + '</strong></td></tr>' +
                '<tr><td class="text-muted">Khách hàng:</td><td>' + (item.customerName || '') + '</td></tr>' +
                '<tr><td class="text-muted">Mã KH:</td><td><code>' + (item.customerCode || '') + '</code></td></tr>' +
                '<tr><td class="text-muted">SĐT:</td><td>' + (item.customerPhone || '-') + '</td></tr>' +
                '<tr><td class="text-muted">Ngày tạo:</td><td>' + date + '</td></tr>' +
                '<tr><td class="text-muted">Trạng thái:</td><td>' + getStatusBadge(item.status) + '</td></tr>' +
                '<tr><td class="text-muted">Ngày duyệt:</td><td>' + approvedDate + '</td></tr>' +
                '</table></div><div class="col-md-6"><table class="table table-sm table-borderless">' +
                '<tr><td class="text-muted" style="width:130px">Quà tặng:</td><td><strong>' + (item.giftName || '') + '</strong></td></tr>' +
                '<tr><td class="text-muted">Mã quà:</td><td>' + (item.giftCode || '-') + '</td></tr>' +
                '<tr><td class="text-muted">Điểm yêu cầu:</td><td><strong>' + (item.pointsRequired || 0) + '</strong></td></tr>' +
                '<tr><td class="text-muted">Số lượng:</td><td>' + (item.quantity || 1) + '</td></tr>' +
                '<tr><td class="text-muted">Tổng điểm:</td><td><strong class="text-danger">' + (item.totalPointsUsed || 0) + '</strong></td></tr>' +
                '<tr><td class="text-muted">Voucher UrBox:</td><td>' + (item.urBoxVoucherCode || '-') + '</td></tr>' +
                '<tr><td class="text-muted">Địa chỉ nhận:</td><td>' + (item.deliveryAddress || '-') + '</td></tr>' +
                '<tr><td class="text-muted">Ghi chú KH:</td><td>' + (item.note || '-') + '</td></tr>' +
                '<tr><td class="text-muted">Ghi chú nội bộ:</td><td>' + (item.internalNote || '-') + '</td></tr>' +
                '</table></div></div>'
            );

            // Show approve/reject buttons chỉ khi Pending
            if (item.status === 1) {
                $('#GiftDetailFooter').show();
            } else {
                $('#GiftDetailFooter').hide();
            }

            new bootstrap.Modal(document.getElementById('GiftDetailModal')).show();
        });
    }

    function approveOrReject(id, isApproved) {
        var notePrompt = isApproved ? '' : 'Lý do từ chối (tùy chọn):';
        var action = isApproved ? 'duyệt' : 'từ chối';

        abp.message.confirm('Bạn có chắc muốn ' + action + ' yêu cầu đổi quà này?', 'Xác nhận', function (confirmed) {
            if (!confirmed) return;
            service.approveOrReject({ id: id, isApproved: isApproved, internalNote: '' })
                .then(function () {
                    abp.notify.success('Đã ' + action + ' thành công');
                    bootstrap.Modal.getInstance(document.getElementById('GiftDetailModal'))?.hide();
                    loadData();
                })
                .catch(function (err) { abp.notify.error('Lỗi: ' + (err.message || '')); });
        });
    }

    // Events
    $('#BtnSearch').click(function () { currentPage = 1; loadData(); });
    $('#BtnRefresh').click(function () { $('#FilterText').val(''); $('#FilterStatus').val(''); currentPage = 1; loadData(); });
    $('#FilterText').keypress(function (e) { if (e.which === 13) { currentPage = 1; loadData(); } });
    $('#PageSize').change(function () { currentPage = 1; loadData(); });
    $(document).on('click', '#Pagination .page-link', function (e) { e.preventDefault(); var p = parseInt($(this).data('page')); if (p >= 1 && p <= totalPages && p !== currentPage) { currentPage = p; loadData(); } });

    $(document).on('click', '.btn-detail', function () { showDetail($(this).data('id')); });
    $(document).on('click', '.btn-approve', function (e) { e.stopPropagation(); approveOrReject($(this).data('id'), true); });
    $(document).on('click', '.btn-reject', function (e) { e.stopPropagation(); approveOrReject($(this).data('id'), false); });

    $('#BtnApprove').click(function () { if (currentDetailId) approveOrReject(currentDetailId, true); });
    $('#BtnReject').click(function () { if (currentDetailId) approveOrReject(currentDetailId, false); });

    loadData();
})();
