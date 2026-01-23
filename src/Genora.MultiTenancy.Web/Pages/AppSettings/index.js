$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appSettings.appSetting;
    var createModal = new abp.ModalManager(abp.appPath + 'AppSettings/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'AppSettings/EditModal');

    var dataTable = $('#AppSettingsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: true,          // ✅ bật search box
            scrollX: true,

            // ✅ gọi method mới + map search.value -> SettingKey
            ajax: abp.libs.datatables.createAjax(service.getListWithFilter, function (request) {
                return {
                    settingKey: request.search?.value || null,
                    skipCount: request.start,
                    maxResultCount: request.length,
                    sorting: request.columns?.[request.order?.[0]?.column]?.data
                        ? request.columns[request.order[0].column].data + ' ' + request.order[0].dir
                        : null
                };
            }),

            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('MultiTenancy.AppSettings.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppSettings.Edit'),
                                action: function (data) { editModal.open({ id: data.record.id }); }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('MultiTenancy.AppSettings.Delete') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppSettings.Delete'),
                                confirmMessage: function (data) {
                                    return l('AppSettingDeletionConfirmationMessage', data.record.settingKey);
                                },
                                action: function (data) {
                                    genora.multiTenancy.apps.appSettings.appSetting
                                        .delete(data.record.id)
                                        .then(function () {
                                            abp.notify.success(l('DeletedSuccessfully'));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                    }
                },
                { title: l('SettingKey'), data: "settingKey" },
                { title: l('SettingValue'), data: "settingValue" },
                { title: l('SettingType'), data: "settingType" },
                { title: l('Description'), data: "description" },
                { title: l('IsActive'), data: "isActive", render: d => d ? l('Yes') : l('No') },
                { title: l('CreationTime'), data: "creationTime", dataFormat: "datetime" }
            ]
        })
    );

    createModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    $('#NewAppSettingButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});