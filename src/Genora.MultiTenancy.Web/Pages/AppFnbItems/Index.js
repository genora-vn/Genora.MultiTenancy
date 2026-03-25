$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var fnb = window.genoraFnb;
    var service = genora.multiTenancy.appServices.appFnbItems.appFnbItem;
    var categoryService = genora.multiTenancy.appServices.appFnbCategories.appFnbCategory;

    var createModal = new abp.ModalManager('/AppFnbItems/CreateModal');
    var editModal = new abp.ModalManager('/AppFnbItems/EditModal');
    var canEdit = $('#CanEditFnbItem').val() === 'true';

    function renderToggle(name, checked, itemId, disabled) {
        var isChecked = checked ? 'checked' : '';
        var isDisabled = disabled ? 'disabled' : '';
        return `
            <label class="fnb-switch">
                <input type="checkbox"
                       class="fnb-item-toggle"
                       data-name="${name}"
                       data-id="${itemId}"
                       ${isChecked}
                       ${isDisabled} />
                <span class="fnb-switch-slider"></span>
            </label>
        `;
    }

    function loadCategories() {
        categoryService.getList({
            skipCount: 0,
            maxResultCount: 1000,
            sorting: 'sortOrder asc',
            isActive: true
        }).then(function (res) {
            var $ddl = $('#FnbItemCategoryIdFilter');
            $ddl.empty().append('<option value="">' + l('All') + '</option>');

            (res.items || []).forEach(function (x) {
                $ddl.append('<option value="' + x.id + '">' + x.name + '</option>');
            });
        });
    }

    function getFilter() {
        var isActive = $('#FnbItemActiveFilter').val();
        var isAvailable = $('#FnbItemAvailableFilter').val();

        return {
            filterText: fnb.toNullableString($('#FnbItemFilterText').val()),
            categoryId: fnb.toNullableString($('#FnbItemCategoryIdFilter').val()),
            isActive: isActive === '' ? null : (isActive === 'true'),
            isAvailable: isAvailable === '' ? null : (isAvailable === 'true')
        };
    }

    var dataTable = $('#FnbItemTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [[5, "asc"]],
            ajax: fnb.createServerAjax(service.getList, getFilter, 'sortOrder asc'),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppFnbItems.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbItems.Edit');
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
                                    return abp.auth.isGranted('MultiTenancy.AppFnbItems.Delete') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbItems.Delete');
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
                {
                    title: '',
                    data: "imageUrl",
                    orderable: false,
                    width: "70px",
                    render: function (data) {
                        if (!data) {
                            return '<div class="fnb-empty-thumb"></div>';
                        }
                        return '<img class="fnb-thumb" src="' + data + '" alt="thumb" />';
                    }
                },
                {
                    title: l('Name'),
                    data: null,
                    render: function (data, type, row) {
                        var desc = row.description ? '<div class="fnb-item-table__desc">' + row.description + '</div>' : '';
                        return '<div class="fnb-item-table__name">' + (row.name || '') + '</div>' + desc;
                    }
                },
                {
                    title: l('Category'),
                    data: "categoryName",
                    render: function (data) {
                        if (!data) return '';
                        return '<span class="fnb-item-table__category-badge">' + data + '</span>';
                    }
                },
                {
                    title: l('Price'),
                    data: "price",
                    render: function (data) {
                        return '<span class="fnb-price">' + fnb.formatCurrency(data) + '</span>';
                    }
                },
                { title: l('SortOrder'), data: "sortOrder" },
                {
                    title: l('IsActive'),
                    data: "isActive",
                    orderable: false,
                    render: function (data, type, row) {
                        return renderToggle('isActive', data, row.id, !canEdit);
                    }
                },
                {
                    title: l('IsAvailable'),
                    data: "isAvailable",
                    orderable: false,
                    render: function (data, type, row) {
                        return renderToggle('isAvailable', data, row.id, !canEdit);
                    }
                }
            ]
        })
    );

    $('#FnbItemTable').on('change', '.fnb-item-toggle', function () {
        var $this = $(this);
        var id = $this.data('id');
        var name = $this.data('name');
        var checked = $this.is(':checked');

        if (!id || !name) return;

        var payload = {};
        payload[name] = checked;

        $this.prop('disabled', true);

        service.setState(id, payload)
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

    $('#DownloadFnbItemTemplateBtn, #ExportItemTemplateButton').click(function (e) {
        e.preventDefault();

        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.download(
            'api/app/app-fnb-item-excel/template',
            {}
        );
    });

    $('#ImportFnbItemExcelInput, #ItemExcelFileInput').change(function (e) {
        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.upload({
            url: 'api/app/app-fnb-item-excel/import',
            fileInput: e.target,
            onSuccess: function () {
                abp.notify.success(l('ImportSuccessfully'));
                dataTable.ajax.reload();
            }
        });
    });

    $('#ImportItemExcelButton').click(function (e) {
        e.preventDefault();
        $('#ItemExcelFileInput').click();
    });

    $('#ExportItemExcelButton').click(function (e) {
        e.preventDefault();

        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.download(
            'api/app/app-fnb-item-excel/export',
            getFilter()
        );
    });

    $('#NewFnbItemButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#SearchFnbItemButton, #RefreshFnbItemButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    $('#FnbItemFilterText').on('keydown', function (e) {
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

    loadCategories();
});