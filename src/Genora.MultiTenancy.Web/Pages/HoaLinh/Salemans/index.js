(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;
    var currentPage = 1, totalPages = 0, totalRecords = 0, allData = [];

    // Màu cho từng khu vực
    var areaColors = {
        'HCM-OTC': { bg: '#dbeafe', color: '#1e40af' },
        'HCM-GT': { bg: '#dcfce7', color: '#166534' },
        'MD': { bg: '#fef3c7', color: '#92400e' },
        'MN': { bg: '#fce7f3', color: '#9d174d' },
        'MT': { bg: '#e0e7ff', color: '#3730a3' },
        'MB': { bg: '#ccfbf1', color: '#115e59' },
        'TN': { bg: '#fef9c3', color: '#854d0e' },
        'DN': { bg: '#f3e8ff', color: '#6b21a8' }
    };

    function getAreaBadge(area) {
        if (!area) return '<span class="text-muted">-</span>';
        var style = areaColors[area] || { bg: '#f1f5f9', color: '#475569' };
        return '<span class="badge" style="background-color:' + style.bg + ';color:' + style.color + ';font-weight:600">' + area + '</span>';
    }

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }

    function getFilteredData() {
        var search = ($('#FilterText').val() || '').toLowerCase();
        var area = $('#FilterArea').val();
        var gender = $('#FilterGender').val();
        var filtered = allData;
        if (search) filtered = filtered.filter(function (i) {
            return (i.dsrCode && i.dsrCode.toLowerCase().includes(search)) ||
                (i.dsrName && i.dsrName.toLowerCase().includes(search)) ||
                (i.workPhone && i.workPhone.includes(search)) ||
                (i.email && i.email.toLowerCase().includes(search));
        });
        if (area) filtered = filtered.filter(function (i) { return i.area === area; });
        if (gender) filtered = filtered.filter(function (i) { return i.gentle === gender; });
        return filtered;
    }

    function renderTable() {
        var filtered = getFilteredData();
        totalRecords = filtered.length;
        var ps = getPageSize();
        totalPages = Math.ceil(totalRecords / ps) || 1;
        if (currentPage > totalPages) currentPage = totalPages;
        var pageData = filtered.slice((currentPage - 1) * ps, currentPage * ps);
        var tbody = $('#HlSalemansTable tbody'); tbody.empty();
        if (pageData.length === 0) {
            tbody.append('<tr><td colspan="7" class="text-center text-muted py-4">Không có dữ liệu</td></tr>');
        } else {
            pageData.forEach(function (item) {
                tbody.append(
                    '<tr class="hl-clickable" data-code="' + (item.dsrCode || '') + '">' +
                    '<td><code>' + (item.dsrCode || '') + '</code></td>' +
                    '<td><strong>' + (item.dsrName || '') + '</strong></td>' +
                    '<td>' + (item.gentle || '') + '</td>' +
                    '<td>' + (item.workPhone || '') + '</td>' +
                    '<td><small>' + (item.email || '') + '</small></td>' +
                    '<td>' + (item.province || '-') + '</td>' +
                    '<td>' + getAreaBadge(item.area) + '</td>' +
                    '</tr>'
                );
            });
        }
        renderPagination();
    }

    function renderPagination() {
        var paging = $('#Pagination'); paging.empty();
        var showing = Math.min(getPageSize(), totalRecords - (currentPage - 1) * getPageSize());
        $('#PagingInfo').html('Hiển thị <strong>' + (showing > 0 ? showing : 0) + '</strong> / <strong>' + totalRecords + '</strong> nhân viên');
        if (totalPages <= 1) return;
        paging.append('<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Previous</a></li>');
        var s = Math.max(1, currentPage - 2), e = Math.min(totalPages, s + 4); if (e - s < 4) s = Math.max(1, e - 4);
        for (var i = s; i <= e; i++) paging.append('<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>');
        paging.append('<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next</a></li>');
    }

    function loadData() {
        abp.ui.setBusy('#SalemanTableContainer');
        service.getSalemans(1, 500).then(function (r) {
            allData = (r.success && r.data) ? (r.data.data || r.data) : [];
            if (!Array.isArray(allData)) allData = [];
            // Populate area filter
            var areas = [...new Set(allData.map(function (i) { return i.area; }).filter(Boolean))].sort();
            var sel = $('#FilterArea'); sel.find('option:not(:first)').remove();
            areas.forEach(function (a) { sel.append('<option value="' + a + '">' + a + '</option>'); });
            currentPage = 1; renderTable();
        }).catch(function (err) { abp.notify.error('Lỗi'); allData = []; renderTable(); })
          .always(function () { abp.ui.clearBusy('#SalemanTableContainer'); });
    }

    function resetFilters() {
        $('#FilterText').val('');
        $('#FilterArea').val('');
        $('#FilterGender').val('');
        currentPage = 1;
        loadData();
    }

    function showDetail(dsrCode) {
        if (!dsrCode) return;
        service.getSalemanDetail(dsrCode).then(function (r) {
            var body = $('#SalemanDetailBody'); body.empty();
            if (r.success && r.data && r.data.length > 0) {
                var s = r.data[0];
                body.html(
                    '<table class="table table-sm table-borderless">' +
                    '<tr><td class="text-muted" style="width:130px">Mã NV:</td><td><strong>' + (s.dsrCode || '') + '</strong></td></tr>' +
                    '<tr><td class="text-muted">Họ và tên:</td><td><strong>' + (s.dsrName || '') + '</strong></td></tr>' +
                    '<tr><td class="text-muted">Giới tính:</td><td>' + (s.gentle || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">Ngày sinh:</td><td>' + (s.birthday || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">SĐT công việc:</td><td>' + (s.workPhone || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">SĐT cá nhân:</td><td>' + (s.cellPhone || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">Email:</td><td>' + (s.email || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">Tỉnh/TP:</td><td>' + (s.province || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">Khu vực:</td><td>' + getAreaBadge(s.area) + '</td></tr>' +
                    '</table>'
                );
            } else { body.html('<p class="text-muted text-center">Không tìm thấy</p>'); }
            new bootstrap.Modal(document.getElementById('SalemanDetailModal')).show();
        });
    }

    $('#BtnSearch').click(function () { currentPage = 1; renderTable(); });
    $('#BtnRefresh').click(function () { resetFilters(); });
    $('#FilterText').keypress(function (e) { if (e.which === 13) { currentPage = 1; renderTable(); } });
    $('#PageSize').change(function () { currentPage = 1; renderTable(); });
    $(document).on('click', '#Pagination .page-link', function (e) { e.preventDefault(); var p = parseInt($(this).data('page')); if (p >= 1 && p <= totalPages && p !== currentPage) { currentPage = p; renderTable(); } });
    $(document).on('click', '#HlSalemansTable tbody tr.hl-clickable', function () { var c = $(this).data('code'); if (c) showDetail(c); });

    loadData();
})();
