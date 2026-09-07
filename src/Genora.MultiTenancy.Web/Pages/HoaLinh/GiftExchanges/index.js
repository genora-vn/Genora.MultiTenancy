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

    // Parse response gốc UrBox (lưu ở urBoxResponse) → DANH SÁCH voucher + lý do thất bại
    // Mỗi giao dịch có thể đổi số lượng > 1 nên trả về mảng vouchers (code_link_gift[]).
    function parseUrBox(item) {
        var out = {
            vouchers: [],          // danh sách voucher đã đổi
            name: item.giftName || null,
            cartNo: null,
            moneyTotal: null,
            failReason: null       // lý do thất bại (msg từ UrBox khi done != 1)
        };
        if (!item.urBoxResponse) return out;
        try {
            var r = JSON.parse(item.urBoxResponse);

            // Lý do thất bại: khi done != 1 (hoặc status != 200) → lấy msg gốc từ UrBox.
            var done = r.done;
            var status = r.status;
            if ((done != null && done !== 1) || (status != null && status !== 200)) {
                out.failReason = r.msg || null;
            }

            var data = r.data || {};
            var cart = data.cart || {};
            out.cartNo = cart.cartNo || cart.id || null;
            out.moneyTotal = cart.money_total || null;

            var linkList = cart.link_gift || [];
            var codes = cart.code_link_gift || [];
            codes.forEach(function (g, idx) {
                out.vouchers.push({
                    code: g.code || null,
                    price: g.price != null ? g.price : null,
                    codeImage: g.code_image || null,
                    expired: g.expired || null,
                    expiredTime: g.expired_time || null,
                    serial: g.serial || null,
                    codeDisplay: g.code_display || null,
                    giftTitle: g.gift_title || null,
                    // Link riêng từng quà: ưu tiên g.link, fallback link_gift[idx]
                    linkGift: g.link || (linkList[idx] || null)
                });
            });

            // Fallback: chưa redeem xong nhưng đã có mã voucher lưu ở entity.
            if (out.vouchers.length === 0 && item.urBoxVoucherCode) {
                out.vouchers.push({ code: item.urBoxVoucherCode, price: null, codeImage: null,
                                    expired: null, serial: null, codeDisplay: null, linkGift: null });
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

            // Lý do thất bại: chỉ hiển thị khi trạng thái Thất bại (0) và parse được msg từ UrBoxResponse.
            var failRow = (item.status === 0 && v.failReason)
                ? infoRow('Lý do thất bại', '<span class="text-danger">' + v.failReason + '</span>')
                : '';

            var left =
                '<div class="hl-vc-card">' +
                '<div class="hl-vc-title"><i class="fa fa-receipt me-2"></i>Thông tin đơn</div>' +
                infoRow('Mã yêu cầu', '<strong>' + (item.exchangeCode || '') + '</strong>') +
                infoRow('Khách hàng', item.customerName) +
                infoRow('Mã KH', '<code>' + (item.customerCode || '-') + '</code>') +
                infoRow('SĐT', item.customerPhone) +
                infoRow('Ngày tạo', date) +
                infoRow('Trạng thái', getStatusBadge(item.status)) +
                failRow +
                infoRow('Tổng điểm/tiền', '<strong class="text-danger">' + fmtMoney(item.totalPointsUsed) + '</strong>') +
                '</div>';

            // Render TỪNG voucher (giao dịch có thể đổi số lượng > 1). Mỗi voucher là 1 block riêng.
            var vouchers = v.vouchers || [];
            var voucherCount = vouchers.length;

            function renderVoucherBlock(g, idx) {
                var qrBlock = g.codeImage
                    ? '<div class="hl-vc-qr"><img src="' + g.codeImage + '" alt="Mã voucher" /></div>'
                    : '';
                var linkBtn = g.linkGift
                    ? '<a href="' + g.linkGift + '" target="_blank" rel="noopener" class="btn btn-primary btn-sm w-100 mt-2">' +
                      '<i class="fa fa-external-link-alt me-1"></i>Xem chi tiết quà (UrBox)</a>'
                    : '';
                var header = voucherCount > 1
                    ? '<div class="hl-vc-item-head">Voucher #' + (idx + 1) + '</div>'
                    : '';
                return '<div class="hl-vc-item">' +
                    header +
                    infoRow('Tên voucher', '<strong>' + (g.giftTitle || v.name || item.giftName || '-') + '</strong>') +
                    infoRow('Mã voucher', g.code ? '<code>' + g.code + '</code>' : '-') +
                    infoRow('Serial', g.serial) +
                    infoRow('Số tiền', fmtMoney(g.price != null ? g.price : v.moneyTotal)) +
                    infoRow('Hạn sử dụng', g.expired) +
                    infoRow('Hiệu lực', g.codeDisplay) +
                    qrBlock +
                    linkBtn +
                    '</div>';
            }

            var voucherBody;
            if (voucherCount === 0) {
                voucherBody = '<div class="text-muted text-center py-3">Chưa có voucher</div>';
            } else {
                voucherBody = '<div class="hl-vc-list">' +
                    vouchers.map(renderVoucherBlock).join('') +
                    '</div>';
            }

            var countLabel = voucherCount > 0 ? ' (' + voucherCount + ' voucher)' : '';
            var right =
                '<div class="hl-vc-card">' +
                '<div class="hl-vc-title"><i class="fa fa-gift me-2"></i>Voucher UrBox' + countLabel + '</div>' +
                voucherBody +
                infoRow('Hotline', HOTLINE) +
                '</div>';

            $('#GiftDetailBody').html('<div class="row g-3"><div class="col-md-6 mb-3">' + left + '</div><div class="col-md-6 mb-3">' + right + '</div></div>');

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
