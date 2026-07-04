(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;
    var currentPage = 1;
    var totalPages = 0;
    var totalRecords = 0;

    function getPageSize() { return parseInt($('#PageSize').val()) || 20; }
    function formatCurrency(value) { if (!value && value !== 0) return ''; return new Intl.NumberFormat('vi-VN').format(value) + 'đ'; }

    // Load brands dropdown
    service.getBrands(1, 500).then(function (r) {
        var brands = r.success && r.data ? (r.data.data || r.data) : [];
        if (Array.isArray(brands)) {
            brands.forEach(function (b) {
                $('#FilterBrand').append('<option value="' + b.brandCode + '">' + (b.brandName || '') + '</option>');
            });
        }
    });

    function renderTable(data) {
        var tbody = $('#HlProductsTable tbody');
        tbody.empty();
        if (!data || data.length === 0) {
            tbody.append('<tr><td colspan="8" class="text-center text-muted py-4">Không có dữ liệu</td></tr>');
            return;
        }
        data.forEach(function (item) {
            var imgSrc = item.imageUrl || item.imageAvatarUrl || '';
            var imgHtml = imgSrc
                ? '<img src="' + imgSrc + '" class="hl-product-img" />'
                : '<div class="hl-product-img d-flex align-items-center justify-content-center"><i class="fa fa-image text-muted"></i></div>';
            var statusBadge = item.isActive
                ? '<span class="badge hl-badge-active">Hoạt động</span>'
                : '<span class="badge hl-badge-inactive">Ngừng</span>';
            var code = item.productCode || item.productGroupCode || '';
            var name = item.productName || item.productGroupName || '';
            tbody.append(
                '<tr class="hl-clickable" data-code="' + code + '" data-img="' + (imgSrc || '') + '">' +
                '<td class="text-center">' + imgHtml + '</td>' +
                '<td><code>' + code + '</code></td>' +
                '<td>' + name + '</td>' +
                '<td>' + (item.productGroupName || '') + '</td>' +
                '<td>' + (item.brandName || '') + '</td>' +
                '<td>' + (item.productUnit || '') + '</td>' +
                '<td class="text-end fw-semibold">' + formatCurrency(item.productPrice) + '</td>' +
                '<td class="text-center">' + statusBadge + '</td>' +
                '</tr>'
            );
        });
    }

    function renderPagination() {
        var paging = $('#Pagination');
        paging.empty();
        var showing = Math.min(getPageSize(), totalRecords - (currentPage - 1) * getPageSize());
        $('#PagingInfo').html('Hiển thị <strong>' + (showing > 0 ? showing : 0) + '</strong> / <strong>' + totalRecords + '</strong> sản phẩm');
        if (totalPages <= 1) return;
        paging.append('<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage - 1) + '">Previous</a></li>');
        var s = Math.max(1, currentPage - 2), e = Math.min(totalPages, s + 4);
        if (e - s < 4) s = Math.max(1, e - 4);
        for (var i = s; i <= e; i++) paging.append('<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link" href="#" data-page="' + i + '">' + i + '</a></li>');
        paging.append('<li class="page-item ' + (currentPage === totalPages ? 'disabled' : '') + '"><a class="page-link" href="#" data-page="' + (currentPage + 1) + '">Next</a></li>');
    }

    function loadData() {
        var search = $('#FilterText').val() || '';
        var brandCode = $('#FilterBrand').val() || '';
        abp.ui.setBusy('#ProductTableContainer');

        if (brandCode) {
            service.getProductsByBrand(brandCode).then(function (result) {
                var data = result.success ? (result.data || []) : [];
                if (search) {
                    var sl = search.toLowerCase();
                    data = data.filter(function (item) {
                        return (item.productGroupCode && item.productGroupCode.toLowerCase().includes(sl)) ||
                            (item.productGroupName && item.productGroupName.toLowerCase().includes(sl)) ||
                            (item.brandName && item.brandName.toLowerCase().includes(sl));
                    });
                }
                totalRecords = data.length;
                var ps = getPageSize();
                totalPages = Math.ceil(totalRecords / ps) || 1;
                if (currentPage > totalPages) currentPage = totalPages;
                renderTable(data.slice((currentPage - 1) * ps, currentPage * ps));
                renderPagination();
            }).catch(function (err) { abp.notify.error('Lỗi'); console.error(err); })
              .always(function () { abp.ui.clearBusy('#ProductTableContainer'); });
        } else {
            service.getProducts(currentPage, getPageSize(), search || null).then(function (result) {
                if (result.success && result.data) {
                    totalRecords = result.data.totalRecords;
                    totalPages = result.data.totalPages;
                    renderTable(result.data.data);
                } else { renderTable([]); totalRecords = 0; totalPages = 0; }
                renderPagination();
            }).catch(function (err) { abp.notify.error('Lỗi'); console.error(err); })
              .always(function () { abp.ui.clearBusy('#ProductTableContainer'); });
        }
    }

    function showDetail(productCode, rowImgUrl) {
        service.getProductDetail(productCode).then(function (result) {
            var body = $('#ProductDetailBody'); body.empty();
            if (result.success && result.data && result.data.length > 0) {
                var p = result.data[0];
                var imgSrc = p.imageAvatarUrl || p.imageUrl || rowImgUrl || '';
                var imgHtml = imgSrc
                    ? '<img src="' + imgSrc + '" class="hl-detail-img mb-3" />'
                    : '<div class="hl-detail-img-placeholder"><i class="fa fa-image fa-3x text-muted"></i></div>';
                body.html(
                    '<div class="row"><div class="col-md-4 text-center">' + imgHtml + '</div>' +
                    '<div class="col-md-8"><table class="table table-sm table-borderless">' +
                    '<tr><td class="text-muted" style="width:140px">Mã SP:</td><td><strong>' + (p.productCode || p.productGroupCode || '') + '</strong></td></tr>' +
                    '<tr><td class="text-muted">Tên SP:</td><td><strong>' + (p.productName || p.productGroupName || '') + '</strong></td></tr>' +
                    '<tr><td class="text-muted">Nhóm SP:</td><td>' + (p.productGroupName || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">Thương hiệu:</td><td>' + (p.brandName || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">ĐVT:</td><td>' + (p.productUnit || '-') + '</td></tr>' +
                    '<tr><td class="text-muted">Giá bán:</td><td class="fw-bold text-primary">' + formatCurrency(p.productPrice) + '</td></tr>' +
                    '<tr><td class="text-muted">Trạng thái:</td><td>' + (p.isActive ? '<span class="badge hl-badge-active">Hoạt động</span>' : '<span class="badge hl-badge-inactive">Ngừng</span>') + '</td></tr>' +
                    '</table></div></div>'
                );
            } else { body.html('<p class="text-muted text-center">Không tìm thấy</p>'); }
            new bootstrap.Modal(document.getElementById('ProductDetailModal')).show();
        });
    }

    // Events — search chỉ khi nhấn button hoặc Enter
    $('#BtnSearch').click(function () { currentPage = 1; loadData(); });
    $('#BtnRefresh').click(function () { $('#FilterText').val(''); $('#FilterBrand').val(''); currentPage = 1; loadData(); });
    $('#FilterText').keypress(function (e) { if (e.which === 13) { currentPage = 1; loadData(); } });
    $('#PageSize').change(function () { currentPage = 1; loadData(); });
    $(document).on('click', '#Pagination .page-link', function (e) {
        e.preventDefault();
        var page = parseInt($(this).data('page'));
        if (page >= 1 && page <= totalPages && page !== currentPage) { currentPage = page; loadData(); }
    });
    $(document).on('click', '#HlProductsTable tbody tr.hl-clickable', function () {
        var code = $(this).data('code');
        var img = $(this).data('img') || '';
        if (code) showDetail(code, img);
    });

    loadData();
})();
