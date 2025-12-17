$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appZaloAuths.appZaloAuth;

    var createModal = new abp.ModalManager('/AppZaloAuths/CreateModal');
    var editModal = new abp.ModalManager('/AppZaloAuths/EditModal');

    function getFilter() {
        var isActiveRaw = $('#IsActive').val();
        return {
            filterText: $('#FilterText').val(),
            isActive: isActiveRaw === "" ? null : (isActiveRaw === "true")
        };
    }

    var table = $('#ZaloAuthTable').DataTable(
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
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                confirmMessage: function (data) {
                                    return l('AreYouSureToDelete', data.record.appId);
                                },
                                action: function (data) {
                                    service.delete(data.record.id).then(function () {
                                        abp.notify.success(l('DeletedSuccessfully'));
                                        table.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                { title: l('AppId'), data: "appId" },
                { title: l('State'), data: "state" },
                {
                    title: l('ExpiresAt'),
                    data: "expireTokenTime",
                    render: function (v) {
                        return v ? luxon.DateTime.fromISO(v).toFormat('dd/MM/yyyy HH:mm') : '';
                    }
                },
                {
                    title: l('IsActive'),
                    data: "isActive",
                    render: function (v) {
                        return v
                            ? '<span class="badge bg-success">' + l('Yes') + '</span>'
                            : '<span class="badge bg-secondary">' + l('No') + '</span>';
                    }
                }
            ]
        })
    );

    $('#ZaloConnectBtn').click(function () {
        abp.ajax({
            url: '/api/host/zalo-auth/authorize-url',
            type: 'GET'
        }).done(function (res) {
            window.open(res.authorizeUrl, '_blank');
        });
    });

    $('#ZaloRefreshBtn').click(function () {
        abp.ajax({
            url: '/api/host/zalo-auth/refresh-now',
            type: 'POST'
        }).done(function () {
            abp.notify.success(l('RefreshedSuccessfully'));
            table.ajax.reload();
        });
    });

    $('#NewAppZaloAuthButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#SearchButton').click(function (e) {
        e.preventDefault();
        table.ajax.reload();
    });

    createModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        table.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        table.ajax.reload();
    });
});
