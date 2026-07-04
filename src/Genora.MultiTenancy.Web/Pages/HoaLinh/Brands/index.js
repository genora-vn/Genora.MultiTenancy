(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;
    var currentPage = 1, totalPages = 0, totalRecords = 0, allData = [];

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }
    function formatCurrency(v) { if (!v && v !== 0) return ''; return new Intl.NumberFormat('vi-VN').format(v) + 'đ'; }

    function getFilteredData() {
        var search = ($('#FilterText').val() || '').toLowerCase();
        var status = $('#FilterStatus').val();
        var filtered = allData;
        if (search) filtered = filtered.filter(function (item) {
            return (item.brandCode && item.brandCode.toString().includes(search)) || (item.brandName && item.brandName.toLowerCase().includes(search));
        });
        if (status === 'true') filtered = filtered.filter(function (item) { return item.isActive === true; });
        if (status === 'false') filtered = filtered.filter(function (item) { return item.isActive === false; });
        return filtered;
    }

    function renderTable() {
        var filtered = getFilteredData();
        totalRecords = filtered.length;
        var ps = getPageSize();
        totalPages = Math.ceil(totalRecords / ps) || 1;
        if (currentPage > totalPages) currentPage = totalPages;
        var pageData = filtered.slice((currentPage - 1) * ps, currentPage * ps);
        var tbody = $('#HlBrandsTable tbody'); tbody.empty();
        if (pageData.length === 0) {
            tbody.append('<tr><td colspan="5" class="text-center text-muted py-4">Không có dữ liệu</td></tr>');
        } else {
            pageData.forEach(function (item) {
                var imgHtml = item.imageUrl
                    ? '<img src="' + item.imageUrl + '" class="hl-brand-img" alt="" />'
                    : '<div class="hl-brand-img hl-brand-img-placeholder"><i class="fa fa-image text-muted"></i></div>';
                var statusBadge = item.isActive ? '<span class="badge hl-badge-active">Hoạt động</span>' : '<span class="badge hl-badge-inactive">Ngừng</span>';
                tbody.append(
                    '<tr class="hl-clickable" data-code="' + (item.brandCode || '') + '">' +
                    '<td class="text-center">' + imgHtml + '</td>' +
                    '<td><code>' + (item.brandCode || '') + '</code></td>' +
                    '<td><strong>' + (item.brandName || '') + '</strong></td>' +
                    '<td class="text-center">' + (item.noOfProduct || 0) + '</td>' +
                    '<td class="text-center">' + statusBadge + '</td>' +
                    '</tr>'
                );
            });
        }
        renderPagination();
    }

    function renderPagination() {
        var paging = $('#Pagination'); paging.empty();
        var showing = Math.min(getPageSize(), totalRecords - (currentPage - 1) * getPageSize());
        $('#PagingInfo').html('Hiển thị <strong>' + (showing > 0 ? showing : 0) + '</strong> / <strong>' + totalRecords + '</strong> danh mục');
        if (totalPages <= 1) return;
        paging.append('<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Previous</a></li>');
        var s = Math.max(1, currentPage - 2), e = Math.min(totalPages, s + 4); if (e - s < 4) s = Math.max(1, e - 4);
        for (var i = s; i <= e; i++) paging.append('<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>');
        paging.append('<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next</a></li>');
    }

    function loadData() {
        abp.ui.setBusy('#BrandTableContainer');
        service.getBrands(1, 500).then(function (r) {
            allData = (r.success && r.data) ? (r.data.data || r.data) : [];
            if (!Array.isArray(allData)) allData = [];
            currentPage = 1; renderTable();
        }).catch(function (err) { abp.notify.error('Lỗi'); allData = []; renderTable(); })
          .always(function () { abp.ui.clearBusy('#BrandTableContainer'); });
    }

    function resetFilters() { $('#FilterText').val(''); $('#FilterStatus').val(''); currentPage = 1; loadData(); }

    function showDetail(brandCode) {
        if (!brandCode) return;
        abp.ui.setBusy('#BrandDetailBody');
        Promise.all([service.getBrandDetail(brandCode.toString()), service.getProductsByBrand(brandCode.toString())])
            .then(function (results) {
                var brand = results[0].success && results[0].data && results[0].data.length > 0 ? results[0].data[0] : null;
                var products = results[1].success && results[1].data ? results[1].data : [];
                var body = $('#BrandDetailBody'); body.empty();

                var imgHtml = (brand && brand.imageUrl)
                    ? '<img src="' + brand.imageUrl + '" class="hl-detail-brand-img" alt="" />'
                    : '<div class="hl-detail-brand-img hl-detail-brand-img-placeholder"><i class="fa fa-image fa-3x text-muted"></i></div>';

                var info = '<div class="row mb-3"><div class="col-md-6"><table class="table table-sm table-borderless">' +
                    '<tr><td class="text-muted" style="width:120px">Mã danh mục:</td><td><strong>' + (brand ? brand.brandCode : brandCode) + '</strong></td></tr>' +
                    '<tr><td class="text-muted">Tên:</td><td><strong>' + (brand ? brand.brandName : '-') + '</strong></td></tr>' +
                    '<tr><td class="text-muted">Số sản phẩm:</td><td>' + (brand ? (brand.noOfProduct || 0) : products.length) + '</td></tr>' +
                    '<tr><td class="text-muted">Trạng thái:</td><td>' + (brand && brand.isActive ? '<span class="badge hl-badge-active">Hoạt động</span>' : '<span class="badge hl-badge-inactive">Ngừng</span>') + '</td></tr>' +
                    '</table></div><div class="col-md-6 text-center">' + imgHtml + '</div></div>';

                var prodHtml = '';
                if (products.length > 0) {
                    prodHtml = '<h6 class="mb-2">Danh sách sản phẩm (' + products.length + ')</h6><div class="table-responsive"><table class="table table-sm table-bordered"><thead class="table-light"><tr><th>Mã SP</th><th>Tên sản phẩm</th><th>ĐVT</th><th class="text-end">Giá bán</th><th class="text-center">Trạng thái</th></tr></thead><tbody>';
                    products.forEach(function (p) { prodHtml += '<tr><td><code>' + (p.productGroupCode || '') + '</code></td><td>' + (p.productGroupName || '') + '</td><td>' + (p.productUnit || '') + '</td><td class="text-end">' + formatCurrency(p.productPrice) + '</td><td class="text-center">' + (p.isActive ? '<span class="badge hl-badge-active">Hoạt động</span>' : '<span class="badge hl-badge-inactive">Ngừng</span>') + '</td></tr>'; });
                    prodHtml += '</tbody></table></div>';
                }
                body.html(info + prodHtml);
                new bootstrap.Modal(document.getElementById('BrandDetailModal')).show();
            }).finally(function () { abp.ui.clearBusy('#BrandDetailBody'); });
    }

    $('#BtnSearch').click(function () { currentPage = 1; renderTable(); });
    $('#BtnRefresh').click(function () { resetFilters(); });
    $('#FilterText').keypress(function (e) { if (e.which === 13) { currentPage = 1; renderTable(); } });
    $('#PageSize').change(function () { currentPage = 1; renderTable(); });
    $(document).on('click', '#Pagination .page-link', function (e) { e.preventDefault(); var p = parseInt($(this).data('page')); if (p >= 1 && p <= totalPages && p !== currentPage) { currentPage = p; renderTable(); } });
    $(document).on('click', '#HlBrandsTable tbody tr.hl-clickable', function () { var c = $(this).data('code'); if (c) showDetail(c); });

    loadData();
})();
