$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var basePath = (abp.appPath || '/') + 'Documents/Manage';

    var $sectionsTbody = $('#tblSections tbody');
    var $pagesTbody = $('#tblPages tbody');
    var $filter = $('#filterSection');

    var canEdit = $('#DocsCanEdit').val() === 'true';
    var canDelete = $('#DocsCanDelete').val() === 'true';

    var sectionModal = new abp.ModalManager((abp.appPath || '/') + 'Documents/Manage/SectionModal');
    var pageModal = new abp.ModalManager((abp.appPath || '/') + 'Documents/Manage/PageModal');

    sectionModal.onResult(reload);
    pageModal.onResult(reload);

    $('#btnNewSection').on('click', function () {
        sectionModal.open();
    });

    $('#btnNewPage').on('click', function () {
        var sectionId = $filter.val();
        if (sectionId) {
            pageModal.open({ SectionId: sectionId });
        } else {
            pageModal.open();
        }
    });

    $filter.on('change', loadPages);

    $sectionsTbody.on('click', '.btn-edit-section', function () {
        sectionModal.open({ id: $(this).data('id') });
    });

    $sectionsTbody.on('click', '.btn-delete-section', function () {
        var id = $(this).data('id');
        var name = $(this).data('name');
        abp.message.confirm('Xoá chuyên mục "' + name + '"? Tất cả trang nội dung trong chuyên mục cũng sẽ bị xoá.')
            .then(function (c) {
                if (!c) return;
                abp.ajax({
                    type: 'POST',
                    url: basePath + '?handler=DeleteSection&id=' + encodeURIComponent(id)
                }).done(function () {
                    abp.notify.success(l('SuccessfullyDeleted'));
                    reload();
                });
            });
    });

    $pagesTbody.on('click', '.btn-edit-page', function () {
        pageModal.open({ id: $(this).data('id') });
    });

    $pagesTbody.on('click', '.btn-delete-page', function () {
        var id = $(this).data('id');
        var title = $(this).data('title');
        abp.message.confirm('Xoá trang "' + title + '"?')
            .then(function (c) {
                if (!c) return;
                abp.ajax({
                    type: 'POST',
                    url: basePath + '?handler=DeletePage&id=' + encodeURIComponent(id)
                }).done(function () {
                    abp.notify.success(l('SuccessfullyDeleted'));
                    reload();
                });
            });
    });

    function escapeHtml(s) {
        return $('<div/>').text(s == null ? '' : String(s)).html();
    }

    function statusPill(status) {
        var map = {
            0: ['draft', 'Bản nháp'],
            1: ['published', 'Hiển thị'],
            2: ['hidden', 'Ẩn']
        };
        var pair = map[status] || ['draft', '—'];
        return '<span class="docs-status-pill ' + pair[0] + '">' + pair[1] + '</span>';
    }

    function reload() {
        return $.when(loadSections(), loadPages());
    }

    function loadSections() {
        return abp.ajax({
            type: 'GET',
            url: basePath + '?handler=Sections',
            dataType: 'json'
        }).done(function (res) {
            $sectionsTbody.empty();
            var items = (res && res.items) ? res.items : [];

            if (items.length === 0) {
                $sectionsTbody.append('<tr><td colspan="3" class="text-center text-muted py-3">Chưa có chuyên mục nào.</td></tr>');
            } else {
                items.forEach(function (s) {
                    var actions = '';
                    if (canEdit) {
                        actions += '<button class="btn btn-sm btn-light btn-edit-section" data-id="' + s.id + '" title="Sửa"><i class="fa fa-pen"></i></button>';
                    }
                    if (canDelete) {
                        actions += ' <button class="btn btn-sm btn-light text-danger btn-delete-section" data-id="' + s.id + '" data-name="' + escapeHtml(s.name) + '" title="Xoá"><i class="fa fa-trash"></i></button>';
                    }

                    var iconCls = s.icon || 'fa fa-folder-open';
                    $sectionsTbody.append(
                        '<tr>' +
                        '<td><i class="' + iconCls + '"></i> <strong>' + escapeHtml(s.name) + '</strong>' +
                        '<br><small class="text-muted">/' + escapeHtml(s.slug) + '</small></td>' +
                        '<td>' + (s.pageCount || 0) + '</td>' +
                        '<td class="text-end">' + actions + '</td>' +
                        '</tr>'
                    );
                });
            }

            // Refresh filter dropdown while preserving selection.
            var prev = $filter.val();
            $filter.empty().append('<option value="">Tất cả chuyên mục</option>');
            items.forEach(function (s) {
                $filter.append('<option value="' + s.id + '">' + escapeHtml(s.name) + '</option>');
            });
            if (prev) $filter.val(prev);
        }).fail(function (xhr) {
            console.error('Load sections failed', xhr);
        });
    }

    function loadPages() {
        var sid = $filter.val();
        var url = basePath + '?handler=Pages';
        if (sid) url += '&sectionId=' + encodeURIComponent(sid);

        return abp.ajax({
            type: 'GET',
            url: url,
            dataType: 'json'
        }).done(function (res) {
            $pagesTbody.empty();

            var items = (res && res.items) ? res.items : [];
            if (items.length === 0) {
                $pagesTbody.append('<tr><td colspan="5" class="text-center text-muted py-3">Chưa có trang nội dung nào.</td></tr>');
                return;
            }

            items.forEach(function (p) {
                var actions = '<a class="btn btn-sm btn-light" href="/Documents/' + encodeURIComponent(p.sectionSlug) + '/' + encodeURIComponent(p.slug) + '" target="_blank" title="Xem"><i class="fa fa-external-link-alt"></i></a>';
                if (canEdit) {
                    actions += ' <button class="btn btn-sm btn-light btn-edit-page" data-id="' + p.id + '" title="Sửa"><i class="fa fa-pen"></i></button>';
                }
                if (canDelete) {
                    actions += ' <button class="btn btn-sm btn-light text-danger btn-delete-page" data-id="' + p.id + '" data-title="' + escapeHtml(p.title) + '" title="Xoá"><i class="fa fa-trash"></i></button>';
                }

                $pagesTbody.append(
                    '<tr>' +
                    '<td><strong>' + escapeHtml(p.title) + '</strong>' +
                    '<br><small class="text-muted">/' + escapeHtml(p.sectionSlug) + '/' + escapeHtml(p.slug) + '</small></td>' +
                    '<td>' + escapeHtml(p.sectionName) + '</td>' +
                    '<td>' + (p.displayOrder || 0) + '</td>' +
                    '<td>' + statusPill(p.status) + '</td>' +
                    '<td class="text-end">' + actions + '</td>' +
                    '</tr>'
                );
            });
        }).fail(function (xhr) {
            console.error('Load pages failed', xhr);
        });
    }

    reload();
});
