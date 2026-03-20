$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appFnbOrders.appFnbOrder;

    var detailModal = new abp.ModalManager('/AppFnbOrders/DetailModal');
    var serviceStatusModal = new abp.ModalManager('/AppFnbOrders/UpdateServiceStatusModal');
    var paymentStatusModal = new abp.ModalManager('/AppFnbOrders/UpdatePaymentStatusModal');
    var cancelModal = new abp.ModalManager('/AppFnbOrders/CancelModal');

    if (window.flatpickr) {
        $('.public-time-input').flatpickr({
            dateFormat: "Y-m-d",
            allowInput: true
        });
    }

    function toNullableInt(value) {
        if (value === undefined || value === null || value === '') {
            return null;
        }

        var n = parseInt(value, 10);
        return isNaN(n) ? null : n;
    }

    function toNullableString(value) {
        if (value === undefined || value === null) {
            return null;
        }

        value = String(value).trim();
        return value === '' ? null : value;
    }

    function getFilter() {
        return {
            filterText: toNullableString($('#FnbOrderFilterText').val()),
            serviceStatus: toNullableInt($('#FnbOrderServiceStatusFilter').val()),
            paymentStatus: toNullableInt($('#FnbOrderPaymentStatusFilter').val()),
            creationTimeFrom: toNullableString($('#FnbOrderCreationTimeFrom').val()),
            creationTimeTo: toNullableString($('#FnbOrderCreationTimeTo').val())
        };
    }

    function renderServiceStatus(s) {
        s = Number(s);

        if (s === 1) return '<span class="badge bg-secondary">' + l('FnbServiceStatus:Created') + '</span>';
        if (s === 2) return '<span class="badge bg-info text-dark">' + l('FnbServiceStatus:Preparing') + '</span>';
        if (s === 3) return '<span class="badge bg-primary">' + l('FnbServiceStatus:Delivering') + '</span>';
        if (s === 4) return '<span class="badge bg-success">' + l('FnbServiceStatus:Served') + '</span>';
        if (s === 5) return '<span class="badge bg-danger">' + l('FnbServiceStatus:Cancelled') + '</span>';

        return '';
    }

    function renderPaymentStatus(s) {
        s = Number(s);

        if (s === 1) return '<span class="badge bg-warning text-dark">' + l('FnbPaymentStatus:Unpaid') + '</span>';
        if (s === 2) return '<span class="badge bg-success">' + l('FnbPaymentStatus:Paid') + '</span>';
        if (s === 3) return '<span class="badge bg-danger">' + l('FnbPaymentStatus:Failed') + '</span>';

        return '';
    }

    function renderDateTime(data) {
        if (!data) return '';

        try {
            if (window.luxon && luxon.DateTime) {
                return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy HH:mm');
            }

            var d = new Date(data);
            if (isNaN(d.getTime())) return data;

            var dd = String(d.getDate()).padStart(2, '0');
            var mm = String(d.getMonth() + 1).padStart(2, '0');
            var yyyy = d.getFullYear();
            var hh = String(d.getHours()).padStart(2, '0');
            var mi = String(d.getMinutes()).padStart(2, '0');

            return dd + '/' + mm + '/' + yyyy + ' ' + hh + ':' + mi;
        } catch (e) {
            console.error('renderDateTime error:', e, data);
            return data;
        }
    }

    var dataTable = $('#FnbOrderTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [[7, "desc"]],
            ajax: function (requestData, callback) {
                var input = $.extend({}, getFilter(), {
                    skipCount: requestData.start,
                    maxResultCount: requestData.length,
                    sorting: 'creationTime desc'
                });

                service.getList(input)
                    .then(function (result) {
                        callback({
                            recordsTotal: result.totalCount || 0,
                            recordsFiltered: result.totalCount || 0,
                            data: result.items || []
                        });
                    })
                    .catch(function (error) {
                        console.error('FnbOrder getList error:', error);
                        abp.notify.error(l('Error:Generic'));
                        callback({
                            recordsTotal: 0,
                            recordsFiltered: 0,
                            data: []
                        });
                    });
            },
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('View'),
                                action: function (data) {
                                    if (!data || !data.record || !data.record.id) return;
                                    detailModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('UpdateServiceStatus'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit');
                                },
                                action: function (data) {
                                    if (!data || !data.record || !data.record.id) return;
                                    serviceStatusModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('UpdatePaymentStatus'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit');
                                },
                                action: function (data) {
                                    if (!data || !data.record || !data.record.id) return;
                                    paymentStatusModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('CancelOrder'),
                                visible: function (data) {
                                    if (!(abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit'))) {
                                        return false;
                                    }

                                    if (!data || !data.record) {
                                        return false;
                                    }

                                    var serviceStatus = Number(data.record.serviceStatus);
                                    return serviceStatus !== 4 && serviceStatus !== 5;
                                },
                                action: function (data) {
                                    if (!data || !data.record || !data.record.id) return;
                                    cancelModal.open({ id: data.record.id });
                                }
                            }
                        ]
                    }
                },
                { title: l('OrderCode'), data: "orderCode" },
                { title: l('BagTag'), data: "bagTag" },
                { title: l('CustomerName'), data: "customerName", defaultContent: "" },
                {
                    title: l('TotalAmount'),
                    data: "totalAmount",
                    render: function (data) {
                        var value = Number(data || 0);
                        return value.toLocaleString('vi-VN');
                    }
                },
                {
                    title: l('ServiceStatus'),
                    data: "serviceStatus",
                    render: function (s) {
                        return renderServiceStatus(s);
                    }
                },
                {
                    title: l('PaymentStatus'),
                    data: "paymentStatus",
                    render: function (s) {
                        return renderPaymentStatus(s);
                    }
                },
                {
                    title: l('CreationTime'),
                    data: "creationTime",
                    render: function (data) {
                        return renderDateTime(data);
                    }
                }
            ]
        })
    );

    $('#SearchFnbOrderButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    detailModal.onResult(function () {
        dataTable.ajax.reload();
    });

    serviceStatusModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    paymentStatusModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    cancelModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });
});