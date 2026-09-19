(function (root) {
    'use strict';
    function resolveService(name) {
        var app = root.genora && root.genora.multiTenancy;
        var namespaces = app ? [app.appServices, app.appDtos] : [];
        for (var i = 0; i < namespaces.length; i++) {
            var admin = namespaces[i] && namespaces[i].hlg && namespaces[i].hlg.admin;
            if (admin && admin[name] && typeof admin[name].getList === 'function') return admin[name];
        }
        throw new Error('HLG application service proxy not found: ' + name + '. Check /Abp/ServiceProxyScript.');
    }
    function escapeText(value) {
        return String(value == null ? '' : value).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }
    function init(config) {
        var $ = root.jQuery;
        var l = root.abp.localization.getResource('MultiTenancy');
        var page = $('#HlgAdmin');
        var service;
        try { service = resolveService(config.service); }
        catch (error) { $('#HlgError').text(l('Hlg:ProxyUnavailable')).removeClass('d-none'); root.console.error(error); return; }
        var parentId = page.attr('data-parent-id');
        var brandId = page.attr('data-brand-id');
        var editable = page.attr('data-edit') === 'true';
        var deletable = page.attr('data-delete') === 'true';
        var create = config.readOnly ? null : new abp.ModalManager('/Hlg/' + config.folder + '/CreateModal');
        var edit = config.readOnly ? null : new abp.ModalManager('/Hlg/' + config.folder + '/EditModal');
        var enums = {
            rewardType: ['Physical', 'Voucher'], gameType: ['Quiz', 'PictureToWord', 'KingOfVietnamese', 'SpinWheel', 'TileFlip'],
            gameStatus: ['Upcoming', 'Ongoing', 'Ended'], customerType: ['Pharmacy', 'Consumer', 'Retailer'],
            contentSlot: ['HomeBanner', 'KnowledgeCard', 'GamesCard', 'RankingCard', 'ShareLink'],
            fulfillmentStatus: ['Pending', 'Shipping', 'Delivered', 'Done']
        };
        var columns = config.columns.map(function (column) {
            return {
                title: l('Hlg:' + column.label), data: column.data, orderable: false,
                render: function (value, type) {
                    if (type !== 'display') return value;
                    if (column.kind === 'bool') return escapeText(l(value ? 'Hlg:Yes' : 'Hlg:No'));
                    if (enums[column.kind]) return escapeText(l('Hlg:' + (enums[column.kind][Number(value) - 1] || 'Unknown')));
                    if (column.kind === 'stock' && value == null) return escapeText(l('Hlg:Unlimited'));
                    if ((column.kind === 'number' || column.kind === 'stock') && value != null) return escapeText(Number(value).toLocaleString(abp.localization.currentCulture.name));
                    if (column.kind === 'date' && value) return escapeText(root.luxon.DateTime.fromISO(value).toFormat('dd/MM/yyyy HH:mm'));
                    return escapeText(value);
                }
            };
        });
        var actions = [];
        if (config.children) actions.push({
            text: l('Hlg:' + config.children), action: function (data) {
                root.location.href = abp.appPath + 'Hlg/' + config.children + '?parentId=' + encodeURIComponent(config.folder === 'Brands' ? data.record.categoryId : data.record.id) + (config.folder === 'Brands' ? '&brandId=' + encodeURIComponent(data.record.id) : '');
            }
        });
        if (config.extraChildren) actions.push({ text: l('Hlg:' + config.extraChildren), action: function (data) { root.location.href = abp.appPath + 'Hlg/' + config.extraChildren + '?parentId=' + encodeURIComponent(data.record.id); } });
        if (config.detail) actions.push({ text: l('Hlg:Details'), action: function (data) { new abp.ModalManager('/Hlg/' + config.folder + '/DetailModal').open({ id: data.record.id }); } });
        if (editable && !config.readOnly) actions.push({ text: l('Hlg:Edit'), action: function (data) { edit.open({ id: data.record.id }); } });
        if (deletable && !config.readOnly) actions.push({
            text: l('Hlg:Delete'), confirmMessage: function (data) {
                return l('Hlg:DeleteConfirm', data.record.name || data.record.title || data.record.content || '');
            }, action: function (data) {
                service.delete(data.record.id).then(function () { table.ajax.reload(null, false); abp.notify.success(l('Hlg:Deleted')); });
            }
        });
        if (actions.length) columns.unshift({ title: l('Hlg:Actions'), rowAction: { items: actions } });
        var table = $('#HlgTable').DataTable(abp.libs.datatables.normalizeConfiguration({
            processing: true, serverSide: true, paging: true, searching: false, ordering: false,
            scrollX: true, order: [], pageLength: 10, lengthMenu: [10, 25, 50, 100], columnDefs: columns,
            ajax: abp.libs.datatables.createAjax(service.getList, function () {
                var active = $('#HlgActive').val();
                return { filterText: $('#HlgSearch').val(), isActive: active === '' ? null : active === 'true', parentId: parentId || null, brandId: brandId || null,
                    status: $('#HlgStatus').val() || null, customerType: $('#HlgCustomerType').val() || null, isRegistered: $('#HlgRegistered').val() || null };
            })
        }));
        $('#HlgFilter').on('submit', function (event) { event.preventDefault(); table.ajax.reload(); });
        $('#HlgReset').on('click', function () { $('#HlgSearch, #HlgActive, #HlgStatus, #HlgCustomerType, #HlgRegistered').val(''); table.ajax.reload(); });
        if (create) {
            $('#HlgCreate').on('click', function () { create.open({ parentId: parentId, brandId: brandId }); });
            if (root.hlgEditor) {
                create.onOpen(function () { root.hlgEditor.init(create.getModal()); });
                edit.onOpen(function () { root.hlgEditor.init(edit.getModal()); });
            }
            create.onResult(function () { table.ajax.reload(); abp.notify.success(l('Hlg:Saved')); });
            edit.onResult(function () { table.ajax.reload(null, false); abp.notify.success(l('Hlg:Saved')); });
        }
    }
    root.hlgAdmin = { init: init, resolveService: resolveService, escapeText: escapeText };
})(window);
