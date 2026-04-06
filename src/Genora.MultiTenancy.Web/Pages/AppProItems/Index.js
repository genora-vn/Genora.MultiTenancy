$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appProItems.appProItem;
    var canEdit = $('#CanEditProItem').val() === 'true';

    var createModal = new abp.ModalManager('/AppProItems/CreateModal');
    var editModal   = new abp.ModalManager('/AppProItems/EditModal');

    function formatVND(v) { return new Intl.NumberFormat('vi-VN').format(v || 0) + ' đ'; }

    // Toggle Hoạt động (IsActive)
    function renderToggle(checked, itemId, disabled) {
        var isChecked  = checked  ? 'checked'  : '';
        var isDisabled = disabled ? 'disabled' : '';
        return `
            <label class="fnb-switch">
                <input type="checkbox"
                       class="pro-item-toggle"
                       data-id="${itemId}"
                       ${isChecked}
                       ${isDisabled} />
                <span class="fnb-switch-slider"></span>
            </label>
        `;
    }

    // Toggle Còn hàng (IsAvailable)
    function renderAvailableToggle(checked, itemId, disabled) {
        var isChecked  = checked  ? 'checked'  : '';
        var isDisabled = disabled ? 'disabled' : '';
        return `
            <label class="fnb-switch">
                <input type="checkbox"
                       class="pro-item-available-toggle"
                       data-id="${itemId}"
                       ${isChecked}
                       ${isDisabled} />
                <span class="fnb-switch-slider"></span>
            </label>
        `;
    }

    function getFilter() {
        var isActive = $('#ProItemActiveFilter').val();
        return {
            filterText: $('#ProItemFilterText').val() || null,
            isActive: isActive === '' ? null : (isActive === 'true')
        };
    }

    var dataTable = $('#ProItemTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true, serverSide: true, paging: true, searching: false, scrollX: true,
            order: [[4, 'asc']],
            ajax: abp.libs.datatables.createAjax(service.getList, getFilter),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: () => abp.auth.isGranted('MultiTenancy.AppProItems.Edit') || abp.auth.isGranted('MultiTenancy.HostAppProItems.Edit'),
                                action: data => editModal.open({ id: data.record.id })
                            },
                            {
                                text: l('Delete'),
                                visible: () => abp.auth.isGranted('MultiTenancy.AppProItems.Delete') || abp.auth.isGranted('MultiTenancy.HostAppProItems.Delete'),
                                confirmMessage: data => l('AreYouSureToDelete', data.record.name),
                                action: data => service.delete(data.record.id).then(() => {
                                    abp.notify.success(l('DeletedSuccessfully'));
                                    dataTable.ajax.reload();
                                })
                            }
                        ]
                    }
                },
                { title: l('Code'),         data: 'code',         defaultContent: '' },
                { title: l('Name'),         data: 'name' },
                { title: l('CategoryName'), data: 'categoryName',  defaultContent: '' },
                { title: l('SortOrder'),    data: 'sortOrder' },
                { title: l('Price'),        data: 'price', render: v => formatVND(v) },
                {
                    title: 'Còn hàng', data: 'isAvailable', orderable: false,
                    render: function (data, type, row) {
                        return renderAvailableToggle(data, row.id, !canEdit);
                    }
                },
                {
                    title: l('IsActive'), data: 'isActive', orderable: false,
                    render: function (data, type, row) {
                        return renderToggle(data, row.id, !canEdit);
                    }
                }
            ]
        })
    );

    var proItemExcelService = genora.multiTenancy.controllers.appProItemExcel;

    // Switcher: Hoạt động (IsActive)
    $('#ProItemTable').on('change', '.pro-item-toggle', function () {
        var $t = $(this), id = $t.data('id'), checked = $t.is(':checked');
        $t.prop('disabled', true);
        proItemExcelService.setState(id, { isActive: checked })
            .then(function () { abp.notify.success(l('SavedSuccessfully')); })
            .catch(function () { $t.prop('checked', !checked); })
            .always(function () { $t.prop('disabled', !canEdit); });
    });

    // Switcher: Còn hàng (IsAvailable)
    $('#ProItemTable').on('change', '.pro-item-available-toggle', function () {
        var $t = $(this), id = $t.data('id'), checked = $t.is(':checked');
        $t.prop('disabled', true);
        proItemExcelService.setState(id, { isAvailable: checked })
            .then(function () { abp.notify.success(l('SavedSuccessfully')); })
            .catch(function () { $t.prop('checked', !checked); })
            .always(function () { $t.prop('disabled', !canEdit); });
    });

    $('#DownloadProItemTemplateBtn').click(e => { e.preventDefault(); genora.excel.download('api/app/app-pro-item-excel/template', {}); });
    $('#ExportProItemExcelButton').click(e => { e.preventDefault(); genora.excel.download('api/app/app-pro-item-excel/export', getFilter()); });
    $('#ImportProItemExcelButton').click(e => { e.preventDefault(); $('#ProItemExcelFileInput').click(); });
    $('#ProItemExcelFileInput').change(function (e) {
        genora.excel.upload({ url: 'api/app/app-pro-item-excel/import', fileInput: e.target,
            onSuccess: () => { abp.notify.success(l('ImportSuccessfully')); dataTable.ajax.reload(); }});
    });

    $('#NewProItemButton').click(e => { e.preventDefault(); createModal.open(); });
    $('#SearchProItemButton, #RefreshProItemButton').click(e => { e.preventDefault(); dataTable.ajax.reload(); });
    $('#ProItemFilterText').on('keydown', e => { if (e.key === 'Enter') { e.preventDefault(); dataTable.ajax.reload(); } });

    createModal.onResult(() => { abp.notify.success(l('SavedSuccessfully')); dataTable.ajax.reload(); });
    editModal.onResult(() => { abp.notify.success(l('SavedSuccessfully')); dataTable.ajax.reload(); });
});
