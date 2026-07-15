(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlGiftExchange;
    var currentPage = 1, totalPages = 0, totalRecords = 0;
    var currentDetailId = null;
    var HOTLINE = '1900 545 435';

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }

    // Trạng thái mới: 0=Thất bại, 1=Thành công, 2=Đang xử lý, 3=Đã sử dụng
    var statusNames = { 0: 'Thất bại', 1: 'Thành công', 2: 'Đang xử lý', 3: 'Đã sử dụng' };

    function getStatusBadge(status) {
        var text = statusNames[status] || '-';
        var cls = 'hl-ge-default';
        if (status === 0) cls = 'hl-ge-rejected';
        else if (status === 1) cls = 'hl-ge-completed';
        else if (status === 2) cls = 'hl-ge-pending';
        else if (status === 3) cls = 'hl-ge-approved';
        return '<span class="badge ' + cls + '">' + text + '</span>';
    }

    function fmtMoney(v) {
        if (v == null || v === '') return '-';
        var n = typeof v === 'string' ? parseFloat(v) : v;
        if (isNaN(n)) return v;
        return new Intl.NumberFormat('vi-VN').format(n) + 'đ';
    }

    // Parse response gốc UrBox (lưu ở urBoxResponse) → object voucher đầu tiên
    function parseUrBox(item) {
        var out = { code: item.urBoxVoucherCode || null, name: item.giftName || null, price: null,
                    codeImage: null, expired: null, expiredTime: null, linkGift: null, serial: null,
                    codeDisplay: null, cartNo: null, moneyTotal: null };
        if (!item.urBoxResponse) return out;
        try {
            var r = JSON.parse(item.urBoxResponse);
            var data = r.data || {};
            var cart = data.cart || {};
            out.cartNo = cart.cartNo || cart.id || null;
            out.moneyTotal = cart.money_total || null;
            if (cart.link_gift && cart.link_gift.length) out.linkGift = cart.link_gift[0];
            var g = (cart.code_link_gift && cart.code_link_gift.length) ? cart.code_link_gift[0] : null;
            if (g) {
                out.code = g.code || out.code;
                out.price = g.price != null ? g.price : out.price;
                out.codeImage = g.code_image || null;
                out.expired = g.expired || null;
                out.expiredTime = g.expired_time || null;
                out.serial = g.serial || null;
                out.codeDisplay = g.code_display || null;
                if (!out.linkGift) out.linkGift = g.link || null;
            }
        } catch (e) { /* ignore parse error */ }
        return out;
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
            // Duyệt/từ chối chỉ khi Đang xử lý (2)
            if (item.status === 2) {
                actions = '<button class="btn btn-sm btn-outline-success btn-approve me-1" data-id="' + item.id + '" title="Duyệt"><i class="fa fa-check"></i></button>' +
                    '<button class="btn btn-sm btn-outline-danger btn-reject me-1" data-id="' + item.id + '" title="Từ chối"><i class="fa fa-times"></i></button>';
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
                '<td class="text-center text-nowrap">' + actions +
                '<button class="btn btn-sm btn-outline-primary btn-detail" data-id="' + item.id + '" title="Chi tiết"><i class="fa fa-eye"></i></button></td>' +
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
        var status = $('#FilterStatus').val() !== '' ? parseInt($('#FilterStatus').val()) : null;
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

    function infoRow(label, value) {
        return '<div class="hl-vc-row"><span class="hl-vc-label">' + label + '</span>' +
               '<span class="hl-vc-value">' + (value != null && value !== '' ? value : '-') + '</span></div>';
    }

    function showDetail(id) {
        service.get(id).then(function (item) {
            currentDetailId = id;
            var v = parseUrBox(item);
            var date = item.creationTime ? new Date(item.creationTime).toLocaleString('vi-VN') : '-';

            var left =
                '<div class="hl-vc-card">' +
                '<div class="hl-vc-title"><i class="fa fa-receipt me-2"></i>Thông tin đơn</div>' +
                infoRow('Mã yêu cầu', '<strong>' + (item.exchangeCode || '') + '</strong>') +
                infoRow('Khách hàng', item.customerName) +
                infoRow('Mã KH', '<code>' + (item.customerCode || '-') + '</code>') +
                infoRow('SĐT', item.customerPhone) +
                infoRow('Ngày tạo', date) +
                infoRow('Trạng thái', getStatusBadge(item.status)) +
                infoRow('Tổng điểm/tiền', '<strong class="text-danger">' + fmtMoney(item.totalPointsUsed) + '</strong>') +
                '</div>';

            var qrBlock = v.codeImage
                ? '<div class="hl-vc-qr"><img src="' + v.codeImage + '" alt="Mã voucher" /></div>'
                : '';

            var linkBtn = v.linkGift
                ? '<a href="' + v.linkGift + '" target="_blank" rel="noopener" class="btn btn-primary btn-sm w-100 mt-2">' +
                  '<i class="fa fa-external-link-alt me-1"></i>Xem chi tiết quà (UrBox)</a>'
                : '';

            var right =
                '<div class="hl-vc-card">' +
                '<div class="hl-vc-title"><i class="fa fa-gift me-2"></i>Voucher UrBox</div>' +
                infoRow('Tên voucher', '<strong>' + (v.name || item.giftName || '-') + '</strong>') +
                infoRow('Mã voucher', v.code ? '<code>' + v.code + '</code>' : '-') +
                infoRow('Serial', v.serial) +
                infoRow('Số tiền', fmtMoney(v.price != null ? v.price : v.moneyTotal)) +
                infoRow('Hạn sử dụng', v.expired) +
                infoRow('Hiệu lực', v.codeDisplay) +
                infoRow('Hotline', HOTLINE) +
                qrBlock +
                linkBtn +
                '</div>';

            $('#GiftDetailBody').html('<div class="row g-3"><div class="col-md-6">' + left + '</div><div class="col-md-6">' + right + '</div></div>');

            // Duyệt/từ chối chỉ khi Đang xử lý (2)
            if (item.status === 2) $('#GiftDetailFooter').show();
            else $('#GiftDetailFooter').hide();

            new bootstrap.Modal(document.getElementById('GiftDetailModal')).show();
        });
    }

    function approveOrReject(id, isApproved) {
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
