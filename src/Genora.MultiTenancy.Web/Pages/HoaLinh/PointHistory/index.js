(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;
    var currentTab = 'txn';
    var currentPage = 1, totalPages = 0, totalRecords = 0;

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }
    function fmt(v) { return (v == null ? 0 : v).toLocaleString('vi-VN'); }

    function typeBadge(type, text) {
        var cls = 'hl-pt-adjust';
        if (type === 1) cls = 'hl-pt-earn';
        else if (type === 2) cls = 'hl-pt-spend';
        else if (type === 3) cls = 'hl-pt-expire';
        return '<span class="badge ' + cls + '">' + (text || '') + '</span>';
    }

    function unitBadge(unit, text) {
        var cls = unit === 2 ? 'hl-unit-amount' : 'hl-unit-point';
        return '<span class="badge ' + cls + '">' + (text || '') + '</span>';
    }

    function statusBadge(status, text) {
        var cls = status === 1 ? 'hl-pt-earn' : (status === 3 ? 'hl-pt-expire' : 'hl-unit-point');
        return '<span class="badge ' + cls + '">' + (text || '') + '</span>';
    }

    function valueCell(unit, value) {
        var cls = value >= 0 ? 'hl-val-plus' : 'hl-val-minus';
        var sign = value >= 0 ? '+' : '';
        var suffix = unit === 2 ? 'đ' : ' điểm';
        return '<span class="' + cls + '">' + sign + fmt(value) + suffix + '</span>';
    }

    function fmtDate(s) {
        if (!s) return '-';
        var d = new Date(s);
        return d.toLocaleDateString('vi-VN') + ' ' + d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }
    function fmtDay(s) { if (!s) return '-'; return new Date(s).toLocaleDateString('vi-VN'); }

    function renderTxn(items) {
        var tbody = $('#PointTable tbody'); tbody.empty();
        if (!items.length) { tbody.append('<tr><td colspan="10" class="text-center text-muted py-4">Không có dữ liệu</td></tr>'); return; }
        items.forEach(function (i) {
            tbody.append('<tr>' +
                '<td><code>' + (i.customerCode || '') + '</code></td>' +
                '<td>' + (i.customerName || '') + '<br><small class="text-muted">' + (i.customerPhone || '') + '</small></td>' +
                '<td class="text-center">' + typeBadge(i.type, i.typeText) + '</td>' +
                '<td class="text-center">' + unitBadge(i.unit, i.unitText) + '</td>' +
                '<td class="text-end">' + valueCell(i.unit, i.value) + '</td>' +
                '<td class="text-end">' + fmt(i.balancePointAfter) + '</td>' +
                '<td class="text-end">' + fmt(i.balanceAmountAfter) + 'đ</td>' +
                '<td><small>' + (i.refCode || '-') + '</small></td>' +
                '<td><small>' + (i.description || '') + '</small></td>' +
                '<td><small>' + fmtDate(i.creationTime) + '</small></td>' +
                '</tr>');
        });
    }

    function renderBatch(items) {
        var tbody = $('#PointTable tbody'); tbody.empty();
        if (!items.length) { tbody.append('<tr><td colspan="10" class="text-center text-muted py-4">Không có dữ liệu</td></tr>'); return; }
        items.forEach(function (i) {
            tbody.append('<tr>' +
                '<td><code>' + (i.batchCode || '') + '</code></td>' +
                '<td><code>' + (i.customerCode || '') + '</code></td>' +
                '<td>' + (i.customerName || '') + '</td>' +
                '<td><small>' + (i.campaignName || '') + '<br>' + (i.campaignCode || '') + '</small></td>' +
                '<td class="text-center">' + unitBadge(i.unit, i.unitText) + '</td>' +
                '<td class="text-end">' + fmt(i.convertedValue) + (i.unit === 2 ? 'đ' : '') + '</td>' +
                '<td class="text-end">' + fmt(i.remainingValue) + (i.unit === 2 ? 'đ' : '') + '</td>' +
                '<td class="text-center">' + statusBadge(i.status, i.statusText) + '</td>' +
                '<td><small>' + fmtDay(i.exchangedAt) + '</small></td>' +
                '<td><small>' + fmtDay(i.expireDate) + '</small></td>' +
                '</tr>');
        });
    }

    function load() {
        abp.ui.setBusy('#PointContainer');
        var ps = getPageSize();
        var search = $('#FilterText').val() || null;

        if (currentTab === 'txn') {
            var filter = {
                search: search,
                type: $('#FilterType').val() ? parseInt($('#FilterType').val()) : null,
                dateFrom: $('#FilterDateFrom').val() || null,
                dateTo: $('#FilterDateTo').val() || null,
                page: currentPage,
                limit: ps
            };
            service.getPointHistory(filter).then(function (r) {
                var d = (r.success && r.data) ? r.data : { data: [], totalRecords: 0, totalPages: 0 };
                totalRecords = d.totalRecords; totalPages = d.totalPages;
                renderTxn(d.data || []);
                renderPaging();
            }).catch(function () { abp.notify.error('Lỗi tải dữ liệu'); })
              .always(function () { abp.ui.clearBusy('#PointContainer'); });
        } else {
            service.getPointBatches(currentPage, ps, search).then(function (r) {
                var d = (r.success && r.data) ? r.data : { data: [], totalRecords: 0, totalPages: 0 };
                totalRecords = d.totalRecords; totalPages = d.totalPages;
                renderBatch(d.data || []);
                renderPaging();
            }).catch(function () { abp.notify.error('Lỗi tải dữ liệu'); })
              .always(function () { abp.ui.clearBusy('#PointContainer'); });
        }
    }

    function renderPaging() {
        var showing = Math.min(getPageSize(), totalRecords - (currentPage - 1) * getPageSize());
        $('#PagingInfo').html('Hiển thị <strong>' + (showing > 0 ? showing : 0) + '</strong> / <strong>' + totalRecords + '</strong> bản ghi');
        var paging = $('#Pagination'); paging.empty();
        if (totalPages <= 1) return;
        paging.append('<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Previous</a></li>');
        var s = Math.max(1, currentPage - 2), e = Math.min(totalPages, s + 4); if (e - s < 4) s = Math.max(1, e - 4);
        for (var i = s; i <= e; i++) paging.append('<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>');
        paging.append('<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next</a></li>');
    }

    function switchTab(tab) {
        currentTab = tab; currentPage = 1;
        $('#PointTabs .nav-link').removeClass('active');
        $('#PointTabs .nav-link[data-tab="' + tab + '"]').addClass('active');
        if (tab === 'txn') { $('#TxnHead').show(); $('#BatchHead').hide(); $('#TypeFilterWrap').show(); }
        else { $('#TxnHead').hide(); $('#BatchHead').show(); $('#TypeFilterWrap').hide(); }
        load();
    }

    $('#PointTabs .nav-link').click(function (e) { e.preventDefault(); switchTab($(this).data('tab')); });
    $('#BtnSearch').click(function () { currentPage = 1; load(); });
    $('#BtnRefresh').click(function () {
        $('#FilterText').val(''); $('#FilterType').val(''); $('#FilterDateFrom').val(''); $('#FilterDateTo').val('');
        currentPage = 1; load();
    });
    $('#FilterText').keypress(function (e) { if (e.which === 13) { currentPage = 1; load(); } });
    $('#PageSize').change(function () { currentPage = 1; load(); });
    $(document).on('click', '#Pagination .page-link', function (e) {
        e.preventDefault(); var p = parseInt($(this).data('page'));
        if (p >= 1 && p <= totalPages && p !== currentPage) { currentPage = p; load(); }
    });

    load();
})();
