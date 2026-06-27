(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;
    var currentPage = 1, totalPages = 0, totalRecords = 0;

    flatpickr('#FilterDateFrom', { dateFormat: 'd/m/Y', allowInput: true });
    flatpickr('#FilterDateTo', { dateFormat: 'd/m/Y', allowInput: true });

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }
    function parseDateVN(str) { if (!str) return null; var p = str.split('/'); return p.length === 3 ? p[2] + '-' + p[1] + '-' + p[0] : null; }

    function getFilters() {
        return {
            dataType: $('#FilterDataType').val() || null,
            isError: $('#FilterIsError').val() === 'true' ? true : ($('#FilterIsError').val() === 'false' ? false : null),
            dateFrom: parseDateVN($('#FilterDateFrom').val()),
            dateTo: parseDateVN($('#FilterDateTo').val())
        };
    }

    function renderTable(data) {
        var tbody = $('#HlApiLogsTable tbody'); tbody.empty();
        if (!data || data.length === 0) {
            tbody.append('<tr><td colspan="8" class="text-center text-muted py-4"><i class="fa fa-info-circle me-1"></i> Không có dữ liệu log</td></tr>');
            return;
        }
        data.forEach(function (item) {
            var statusBadge = item.isError ? '<span class="badge hl-badge-error">Lỗi</span>' : '<span class="badge hl-badge-success">' + (item.responseStatusCode || 'OK') + '</span>';
            var time = item.creationTime ? new Date(item.creationTime).toLocaleString('vi-VN') : '';
            tbody.append('<tr class="' + (item.isError ? 'hl-log-error' : '') + '"><td><small>' + time + '</small></td><td><code>' + (item.httpMethod || '') + '</code></td><td class="hl-log-url" title="' + (item.requestUrl || '') + '"><small>' + (item.requestUrl || '') + '</small></td><td>' + (item.dataType || '-') + '</td><td>' + (item.callerSource || '-') + '</td><td class="text-center">' + statusBadge + '</td><td class="text-end">' + (item.durationMs || 0) + ' ms</td><td><small class="text-danger">' + (item.errorMessage || '') + '</small></td></tr>');
        });
    }

    function renderPagination() {
        var paging = $('#Pagination'); paging.empty();
        var showing = Math.min(getPageSize(), totalRecords - (currentPage - 1) * getPageSize());
        $('#PagingInfo').html('Hiển thị <strong>' + (showing > 0 ? showing : 0) + '</strong> / <strong>' + totalRecords + '</strong> log');
        if (totalPages <= 1) return;
        paging.append('<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Previous</a></li>');
        var s = Math.max(1, currentPage - 2), e = Math.min(totalPages, s + 4); if (e - s < 4) s = Math.max(1, e - 4);
        for (var i = s; i <= e; i++) paging.append('<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>');
        paging.append('<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next</a></li>');
    }

    function loadData() {
        var f = getFilters();
        abp.ui.setBusy('#ApiLogTableContainer');
        service.getApiLogs(currentPage, getPageSize(), f.dataType, f.isError, f.dateFrom, f.dateTo)
            .then(function (r) {
                if (r.success && r.data) {
                    totalRecords = r.data.totalRecords;
                    totalPages = r.data.totalPages;
                    renderTable(r.data.data);
                } else { renderTable([]); totalRecords = 0; totalPages = 0; }
                renderPagination();
            })
            .catch(function (err) { abp.notify.error('Lỗi'); console.error(err); })
            .always(function () { abp.ui.clearBusy('#ApiLogTableContainer'); });
    }

    // Events — search chỉ khi nhấn button
    $('#BtnSearch').click(function () { currentPage = 1; loadData(); });
    $('#BtnRefresh').click(function () { currentPage = 1; loadData(); });
    $('#PageSize').change(function () { currentPage = 1; loadData(); });
    $(document).on('click', '#Pagination .page-link', function (e) { e.preventDefault(); var p = parseInt($(this).data('page')); if (p >= 1 && p <= totalPages && p !== currentPage) { currentPage = p; loadData(); } });

    // Delete
    $('#BtnDelete').click(function () {
        abp.message.confirm('Bạn có chắc muốn xóa log theo bộ lọc hiện tại?', 'Xác nhận xóa', function (confirmed) {
            if (!confirmed) return;
            var f = getFilters();
            service.deleteApiLogs(f.dataType, f.isError, f.dateFrom, f.dateTo).then(function (r) {
                if (r.success) { abp.notify.success(r.message || 'Đã xóa'); currentPage = 1; loadData(); }
                else { abp.notify.error(r.error || 'Lỗi'); }
            }).catch(function (err) { abp.notify.error('Lỗi'); });
        });
    });

    loadData();
})();
