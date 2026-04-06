$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appProCategories.appProCategory;
    var proCategoryExcelService = genora.multiTenancy.controllers.appProCategoryExcel;
    var canEdit = $('#CanEditProCategory').val() === 'true';

    var createModal = new abp.ModalManager('/AppProCategories/CreateModal');
    var editModal   = new abp.ModalManager('/AppProCategories/EditModal');

    function renderToggle(checked, itemId, disabled) {
        var isChecked  = checked  ? 'checked'  : '';
        var isDisabled = disabled ? 'disabled' : '';
        return `
            <label class="fnb-switch">
                <input type="checkbox"
                       class="pro-category-toggle"
                       data-id="${itemId}"
                       ${isChecked}
                       ${isDisabled} />
                <span class="fnb-switch-slider"></span>
            </label>
        `;
    }

    function getFilter() {
        var isActive = $('#ProCategoryActiveFilter').val();
        return {
            filterText: $('#ProCategoryFilterText').val() || null,
            isActive: isActive === '' ? null : (isActive === 'true')
        };
    }

    var dataTable = $('#ProCategoryTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true, serverSide: true, paging: true, searching: false, scrollX: true,
            order: [[3, 'asc']],
            ajax: abp.libs.datatables.createAjax(service.getList, getFilter),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: () => abp.auth.isGranted('MultiTenancy.AppProCategories.Edit') || abp.auth.isGranted('MultiTenancy.HostAppProCategories.Edit'),
                                action: data => editModal.open({ id: data.record.id })
                            },
                            {
                                text: l('Delete'),
                                visible: () => abp.auth.isGranted('MultiTenancy.AppProCategories.Delete') || abp.auth.isGranted('MultiTenancy.HostAppProCategories.Delete'),
                                confirmMessage: data => l('AreYouSureToDelete', data.record.name),
                                action: data => service.delete(data.record.id).then(() => {
                                    abp.notify.success(l('DeletedSuccessfully'));
                                    dataTable.ajax.reload();
                                })
                            }
                        ]
                    }
                },
                { title: l('Code'),      data: 'code',      defaultContent: '' },
                { title: l('Name'),      data: 'name' },
                { title: l('SortOrder'), data: 'sortOrder' },
                {
                    title: l('IsActive'), data: 'isActive', orderable: false,
                    render: function (data, type, row) {
                        return renderToggle(data, row.id, !canEdit);
                    }
                }
            ]
        })
    );

    // Toggle IsActive
    $('#ProCategoryTable').on('change', '.pro-category-toggle', function () {
        var $t = $(this), id = $t.data('id'), checked = $t.is(':checked');
        $t.prop('disabled', true);
        proCategoryExcelService.setActive(id, { isActive: checked })
            .then(function () { abp.notify.success(l('SavedSuccessfully')); })
            .catch(function () { $t.prop('checked', !checked); })
            .always(function () { $t.prop('disabled', !canEdit); });
    });

    // Excel
    $('#DownloadProCategoryTemplateBtn').click(e => { e.preventDefault(); genora.excel.download('api/app/app-pro-category-excel/template', {}); });
    $('#ExportProCategoryExcelButton').click(e => { e.preventDefault(); genora.excel.download('api/app/app-pro-category-excel/export', getFilter()); });
    $('#ImportProCategoryExcelButton').click(e => { e.preventDefault(); $('#ProCategoryExcelFileInput').click(); });
    $('#ProCategoryExcelFileInput').change(function (e) {
        genora.excel.upload({ url: 'api/app/app-pro-category-excel/import', fileInput: e.target,
            onSuccess: () => { abp.notify.success(l('ImportSuccessfully')); dataTable.ajax.reload(); }});
    });

    $('#NewProCategoryButton').click(e => { e.preventDefault(); createModal.open(); });
    $('#SearchProCategoryButton, #RefreshProCategoryButton').click(e => { e.preventDefault(); dataTable.ajax.reload(); });
    $('#ProCategoryFilterText').on('keydown', e => { if (e.key === 'Enter') { e.preventDefault(); dataTable.ajax.reload(); } });

    createModal.onResult(() => { abp.notify.success(l('SavedSuccessfully')); dataTable.ajax.reload(); });
    editModal.onResult(() => { abp.notify.success(l('SavedSuccessfully')); dataTable.ajax.reload(); });
});
