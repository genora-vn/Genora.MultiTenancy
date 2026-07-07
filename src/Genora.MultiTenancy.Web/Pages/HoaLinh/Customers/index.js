(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;
    var currentPage = 1, totalPages = 0, totalRecords = 0, allData = [];

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }

    function getFilteredData() {
        var search = ($('#FilterText').val() || '').toLowerCase();
        var channel = $('#FilterChannel').val();
        var gkhl = $('#FilterGkhl').val();
        var source = $('#FilterSource').val();
        var filtered = allData;
        if (search) filtered = filtered.filter(function (i) {
            return (i.custCode && i.custCode.toLowerCase().includes(search)) || (i.custName && i.custName.toLowerCase().includes(search)) || (i.custPhone && i.custPhone.includes(search)) || (i.dsrName && i.dsrName.toLowerCase().includes(search));
        });
        if (channel) filtered = filtered.filter(function (i) { return i.custChannel === channel; });
        if (gkhl === 'true') filtered = filtered.filter(function (i) { return i.isGkhl === true; });
        if (gkhl === 'false') filtered = filtered.filter(function (i) { return !i.isGkhl; });
        if (source) filtered = filtered.filter(function (i) { return String(i.source) === source; });
        return filtered;
    }

    function sourceBadge(item) {
        var txt = item.sourceText || '';
        if (item.source === 5) return '<span class="badge hl-src-dms">' + txt + '</span>';
        if (item.source === 1) return '<span class="badge hl-src-mini">' + txt + '</span>';
        return txt ? '<span class="badge hl-src-other">' + txt + '</span>' : '<span class="text-muted">-</span>';
    }

    function renderTable() {
        var filtered = getFilteredData();
        totalRecords = filtered.length;
        var ps = getPageSize();
        totalPages = Math.ceil(totalRecords / ps) || 1;
        if (currentPage > totalPages) currentPage = totalPages;
        var pageData = filtered.slice((currentPage - 1) * ps, currentPage * ps);
        var tbody = $('#HlCustomersTable tbody'); tbody.empty();
        if (pageData.length === 0) { tbody.append('<tr><td colspan="11" class="text-center text-muted py-4">Không có dữ liệu</td></tr>'); }
        else {
            pageData.forEach(function (item) {
                var gkhlBadge = item.isGkhl ? '<span class="badge hl-badge-gkhl">Có</span>' : '<span class="text-muted">-</span>';
                var tierBadge = item.membershipTier ? '<span class="badge hl-tier-badge">' + item.membershipTier + '</span>' : '-';
                var points = item.accumulatedPoints != null ? item.accumulatedPoints.toLocaleString('vi-VN') : '-';
                tbody.append(
                    '<tr>' +
                    '<td class="text-center"><a href="/HoaLinh/Customers/Detail?phone=' + encodeURIComponent(item.custPhone || '') + '" class="btn btn-sm btn-outline-primary" title="Xem chi tiết"><i class="fa fa-eye"></i></a></td>' +
                    '<td><code>' + (item.custCode || '') + '</code></td>' +
                    '<td>' + (item.custName || '') + '</td>' +
                    '<td>' + (item.custPhone || '') + '</td>' +
                    '<td>' + (item.custChannel || '') + '</td>' +
                    '<td><small>' + (item.custGroup || '') + '</small></td>' +
                    '<td><small>' + (item.dsrName || '') + '</small></td>' +
                    '<td class="text-center">' + gkhlBadge + '</td>' +
                    '<td class="text-center">' + tierBadge + '</td>' +
                    '<td class="text-end">' + points + '</td>' +
                    '<td class="text-center">' + sourceBadge(item) + '</td>' +
                    '</tr>'
                );
            });
        }
        renderPagination();
    }

    function renderPagination() {
        var paging = $('#Pagination'); paging.empty();
        var showing = Math.min(getPageSize(), totalRecords - (currentPage - 1) * getPageSize());
        $('#PagingInfo').html('Hiển thị <strong>' + (showing > 0 ? showing : 0) + '</strong> / <strong>' + totalRecords + '</strong> khách hàng');
        if (totalPages <= 1) return;
        paging.append('<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Previous</a></li>');
        var s = Math.max(1, currentPage - 2), e = Math.min(totalPages, s + 4); if (e - s < 4) s = Math.max(1, e - 4);
        for (var i = s; i <= e; i++) paging.append('<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>');
        paging.append('<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next</a></li>');
    }

    function loadData() {
        var search = $('#FilterText').val() || '';
        abp.ui.setBusy('#CustomerTableContainer');
        service.getCustomers(1, 500, search || null).then(function (r) {
            allData = (r.success && r.data) ? (r.data.data || []) : [];
            currentPage = 1; renderTable();
        }).catch(function (err) { abp.notify.error('Lỗi'); allData = []; renderTable(); })
          .always(function () { abp.ui.clearBusy('#CustomerTableContainer'); });
    }

    function resetFilters() {
        $('#FilterText').val('');
        $('#FilterChannel').val('');
        $('#FilterGkhl').val('');
        $('#FilterSource').val('');
        currentPage = 1;
        loadData();
    }

    $('#BtnSearch').click(function () { loadData(); });
    $('#BtnRefresh').click(function () { resetFilters(); });
    $('#FilterText').keypress(function (e) { if (e.which === 13) loadData(); });
    $('#FilterSource').change(function () { currentPage = 1; renderTable(); });
    $('#FilterChannel').change(function () { currentPage = 1; renderTable(); });
    $('#FilterGkhl').change(function () { currentPage = 1; renderTable(); });
    $('#PageSize').change(function () { currentPage = 1; renderTable(); });
    $(document).on('click', '#Pagination .page-link', function (e) { e.preventDefault(); var p = parseInt($(this).data('page')); if (p >= 1 && p <= totalPages && p !== currentPage) { currentPage = p; renderTable(); } });

    loadData();
})();
