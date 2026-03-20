$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appFnbCategories.appFnbCategory;

    var createModal = new abp.ModalManager('/AppFnbCategories/CreateModal');
    var editModal = new abp.ModalManager('/AppFnbCategories/EditModal');

    function getFilter() {
        var isActive = $('#FnbCategoryActiveFilter').val();
        return {
            filterText: $('#FnbCategoryFilterText').val(),
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
            ajax: abp.libs.datatables.createAjax(service.getList, getFilter),
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
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppFnbCategories.Delete') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbCategories.Delete');
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
                { title: l('Code'), data: "code", defaultContent: "" },
                { title: l('SortOrder'), data: "sortOrder" },
                {
                    title: l('IsActive'),
                    data: "isActive",
                    render: function (data) {
                        return data
                            ? '<span class="badge bg-success">' + l('Yes') + '</span>'
                            : '<span class="badge bg-secondary">' + l('No') + '</span>';
                    }
                }
            ]
        })
    );

    $('#NewFnbCategoryButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#SearchFnbCategoryButton').click(function (e) {
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
});