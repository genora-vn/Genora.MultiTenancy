$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var fnb = window.genoraFnb;
    var service = genora.multiTenancy.appServices.appFnbCategories.appFnbCategory;
    var canEdit = $('#CanEditFnbCategory').val() === 'true';

    var createModal = new abp.ModalManager('/AppFnbCategories/CreateModal');
    var editModal = new abp.ModalManager('/AppFnbCategories/EditModal');

    function renderToggle(checked, itemId, disabled) {
        var isChecked = checked ? 'checked' : '';
        var isDisabled = disabled ? 'disabled' : '';
        return `
            <label class="fnb-switch">
                <input type="checkbox"
                       class="fnb-category-toggle"
                       data-id="${itemId}"
                       ${isChecked}
                       ${isDisabled} />
                <span class="fnb-switch-slider"></span>
            </label>
        `;
    }

    function getFilter() {
        var isActive = $('#FnbCategoryActiveFilter').val();

        return {
            filterText: fnb.toNullableString($('#FnbCategoryFilterText').val()),
            isActive: isActive === '' ? null : (isActive === 'true')
        };
    }

    var dataTable = $('#FnbCategoryTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [[3, "asc"]],
            ajax: fnb.createServerAjax(service.getList, getFilter, 'sortOrder asc'),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppFnbCategories.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbCategories.Edit');
                                },
                                action: function (data) {
                                    var id = fnb.safeId(data);
                                    if (!id) return;
                                    editModal.open({ id: id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppFnbCategories.Delete') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbCategories.Delete');
                                },
                                confirmMessage: function (data) {
                                    return l('AreYouSureToDelete', data && data.record ? data.record.name : '');
                                },
                                action: function (data) {
                                    var id = fnb.safeId(data);
                                    if (!id) return;

                                    service.delete(id).then(function () {
                                        abp.notify.success(l('DeletedSuccessfully'));
                                        dataTable.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                { title: l('Name'), data: "name" },
                { title: l('Code'), data: "code", defaultContent: "" },
                { title: l('SortOrder'), data: "sortOrder" },
                {
                    title: l('IsActive'),
                    data: "isActive",
                    orderable: false,
                    render: function (data, type, row) {
                        return renderToggle(data, row.id, !canEdit);
                    }
                }
            ]
        })
    );

    $('#FnbCategoryTable').on('change', '.fnb-category-toggle', function () {
        var $this = $(this);
        var id = $this.data('id');
        var checked = $this.is(':checked');

        if (!id) return;

        $this.prop('disabled', true);

        service.setActive(id, { isActive: checked })
            .then(function () {
                abp.notify.success(l('SavedSuccessfully'));
            })
            .catch(function () {
                $this.prop('checked', !checked);
            })
            .always(function () {
                $this.prop('disabled', !canEdit);
            });
    });

    $('#DownloadFnbCategoryTemplateBtn, #ExportCategoryTemplateButton').click(function (e) {
        e.preventDefault();

        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.download(
            'api/app/app-fnb-category-excel/template',
            {}
        );
    });

    $('#ImportFnbCategoryExcelInput, #CategoryExcelFileInput').change(function (e) {
        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.upload({
            url: 'api/app/app-fnb-category-excel/import',
            fileInput: e.target,
            onSuccess: function () {
                abp.notify.success(l('ImportSuccessfully'));
                dataTable.ajax.reload();
            }
        });
    });

    $('#ImportCategoryExcelButton').click(function (e) {
        e.preventDefault();
        $('#CategoryExcelFileInput').click();
    });

    $('#ExportCategoryExcelButton').click(function (e) {
        e.preventDefault();

        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.download(
            'api/app/app-fnb-category-excel/export',
            getFilter()
        );
    });

    $('#NewFnbCategoryButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#SearchFnbCategoryButton, #RefreshFnbCategoryButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    $('#FnbCategoryFilterText').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload();
        }
    });

    createModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });
});