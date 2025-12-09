//$(function () {
//    var l = abp.localization.getResource('MultiTenancy');
//    var createModal = new abp.ModalManager(abp.appPath + 'AppSetings/CreateModal');
//    var editModal = new abp.ModalManager(abp.appPath + 'AppSetings/EditModal');

//    const isHost = !abp.currentTenant.isAvailable; // ⭐ Host?

//    var dataTable = $('#AppSettingsTable').DataTable(
//        abp.libs.datatables.normalizeConfiguration({
//            serverSide: true,
//            paging: true,
//            order: [[1, "asc"]],
//            searching: false,
//            scrollX: true,
//            ajax: abp.libs.datatables.createAjax(genora.multiTenancy.apps.appSettings.getList),
//            columnDefs: [
//                {
//                    title: l('Actions'),
//                    rowAction: {
//                        items: [
//                            {
//                                text: l('Edit'),
//                                // Host: luôn hiển thị; Tenant: theo quyền
//                                visible: abp.auth.isGranted('MultiTenancy.AppSetings.Edit') ||
//                                    abp.auth.isGranted('MultiTenancy.HostAppSetings.Edit'),
//                                action: function (data) {
//                                    editModal.open({ id: data.record.id });
//                                }
//                            },
//                            {
//                                text: l('Delete'),
//                                visible: abp.auth.isGranted('MultiTenancy.AppSetings.Delete') ||
//                                    abp.auth.isGranted('MultiTenancy.HostAppSetings.Delete'),
//                                confirmMessage: function (data) {
//                                    return l('AppSetingDeletionConfirmationMessage', data.record.name);
//                                },
//                                action: function (data) {
//                                    genora.multiTenancy.apps.appSettings
//                                        .delete(data.record.id)
//                                        .then(function () {
//                                            abp.notify.success(l('DeletedSuccessfully'));
//                                            dataTable.ajax.reload();
//                                        });
//                                }
//                            }
//                        ]
//                    }
//                },
//                { title: l('SettingKey'), data: "settingKey " },
//                { title: l('SettingValue'), data: "settingValue" },
//                { title: l('SettingType '), data: "settingType " },
//                { title: l('Description'), data: "description" },
//                { title: l('IsActive'), data: "isActive " },
//                { title: l('CreationTime'), data: "creationTime", dataFormat: "datetime" }
//            ]
//        })
//    );

//    createModal.onResult(function () {
//        abp.notify.success(l('CreatedSuccessfully'));
//        dataTable.ajax.reload();
//    });

//    editModal.onResult(function () {
//        abp.notify.success(l('SavedSuccessfully'));
//        dataTable.ajax.reload();
//    });

//    $('#NewAppSettingButton').click(function (e) {
//        e.preventDefault();
//        createModal.open();
//    });
//});
$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var createModal = new abp.ModalManager(abp.appPath + 'AppSettings/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'AppSettings/EditModal');

    var dataTable = $('#AppSettingsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(genora.multiTenancy.apps.appSettings.appSetting.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('MultiTenancy.AppSettings.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppSettings.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
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