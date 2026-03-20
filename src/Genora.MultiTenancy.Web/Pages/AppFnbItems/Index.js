$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appFnbItems.appFnbItem;
    var categoryService = genora.multiTenancy.appServices.appFnbCategories.appFnbCategory;

    var createModal = new abp.ModalManager('/AppFnbItems/CreateModal');
    var editModal = new abp.ModalManager('/AppFnbItems/EditModal');

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
            filterText: $('#FnbItemFilterText').val(),
            categoryId: $('#FnbItemCategoryIdFilter').val() || null,
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
            ajax: abp.libs.datatables.createAjax(service.getList, getFilter),
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
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppFnbItems.Delete') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbItems.Delete');
                                },
                                confirmMessage: function (data) {
                                    return l('AreYouSureToDelete', data.record.name);
                                },
                                action: function (data) {
                                    service.delete(data.record.id).then(function () {
                                        abp.notify.success(l('DeletedSuccessfully'));
                                        dataTable.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                { title: l('Name'), data: "name" },
                { title: l('Category'), data: "categoryName" },
                {
                    title: l('Price'),
                    data: "price",
                    render: function (data) {
                        return (data || 0).toLocaleString('vi-VN');
                    }
                },
                { title: l('SortOrder'), data: "sortOrder" },
                {
                    title: l('IsActive'),
                    data: "isActive",
                    render: function (data) {
                        return data
                            ? '<span class="badge bg-success">' + l('Yes') + '</span>'
                            : '<span class="badge bg-secondary">' + l('No') + '</span>';
                    }
                },
                {
                    title: l('IsAvailable'),
                    data: "isAvailable",
                    render: function (data) {
                        return data
                            ? '<span class="badge bg-success">' + l('Yes') + '</span>'
                            : '<span class="badge bg-warning text-dark">' + l('No') + '</span>';
                    }
                }
            ]
        })
    );

    $('#NewFnbItemButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#SearchFnbItemButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
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