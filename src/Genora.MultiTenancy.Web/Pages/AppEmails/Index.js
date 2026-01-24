$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appEmails.appEmail;

    var createModal = new abp.ModalManager('/AppEmails/CreateModal');
    var editModal = new abp.ModalManager('/AppEmails/EditModal');

    function getFilter() {
        var statusVal = $('#EmailStatusFilter').val();
        return {
            filterText: $('#EmailFilterText').val(),
            status: statusVal === "" ? null : parseInt(statusVal),
            bookingCode: $('#BookingCodeFilter').val()
        };
    }

    function renderStatusBadge(s) {
        if (s === 0) return '<span class="badge bg-warning">' + l('EmailStatus:Pending') + '</span>';
        if (s === 1) return '<span class="badge bg-info">' + l('EmailStatus:Sending') + '</span>';
        if (s === 2) return '<span class="badge bg-success">' + l('EmailStatus:Sent') + '</span>';
        if (s === 3) return '<span class="badge bg-danger">' + l('EmailStatus:Failed') + '</span>';
        if (s === 4) return '<span class="badge bg-secondary">' + l('EmailStatus:Abandoned') + '</span>';
        return '';
    }

    function formatDateTime(data) {
        if (!data) return '';
        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy HH:mm');
    }

    var dataTable = $('#EmailsTable').DataTable(
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
                                text: l('View'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id, readonlyParam: true });
                                }
                            },
                            {
                                text: l('Edit'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppEmails.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppEmails.Edit');
                                },
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('SendNow'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppEmails.Send') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppEmails.Send');
                                },
                                action: function (data) {
                                    service.sendNow(data.record.id).then(function () {
                                        abp.notify.success(l('QueuedSuccessfully'));
                                        dataTable.ajax.reload();
                                    });
                                }
                            },
                            {
                                text: l('Resend'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppEmails.Resend') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppEmails.Resend');
                                },
                                confirmMessage: function () {
                                    return l('AreYouSureToResend');
                                },
                                action: function (data) {
                                    service.resend(data.record.id).then(function () {
                                        abp.notify.success(l('QueuedSuccessfully'));
                                        dataTable.ajax.reload();
                                    });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppEmails.Delete') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppEmails.Delete');
                                },
                                confirmMessage: function () {
                                    return l('AreYouSure');
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
                { title: l('Subject'), data: "subject", width: "260px" },
                { title: l('ToEmails'), data: "toEmails", width: "220px" },
                {
                    title: l('EmailStatus'),
                    data: "status",
                    render: function (s) { return renderStatusBadge(s); },
                    width: "120px"
                },
                { title: l('TryCount'), data: "tryCount", width: "80px" },
                { title: l('BookingCode'), data: "bookingCode", width: "150px" },
                {
                    title: l('SentTime'),
                    data: "sentTime",
                    render: function (d) { return formatDateTime(d); },
                    width: "150px"
                },
                {
                    title: l('LastError'),
                    data: "lastError",
                    render: function (d) {
                        if (!d) return '';
                        var short = d.length > 60 ? (d.substring(0, 60) + '...') : d;
                        return `<span title="${abp.utils.escapeHtml(d)}">${abp.utils.escapeHtml(short)}</span>`;
                    },
                    width: "240px"
                }
            ]
        })
    );

    $('#NewEmailButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#SearchEmailButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    createModal.onResult(function () {
        abp.notify.success(l('QueuedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });
});
