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

    function getFilter() {
        return {
            filterText: $('#FnbOrderFilterText').val(),
            serviceStatus: $('#FnbOrderServiceStatusFilter').val() || null,
            paymentStatus: $('#FnbOrderPaymentStatusFilter').val() || null,
            creationTimeFrom: $('#FnbOrderCreationTimeFrom').val() || null,
            creationTimeTo: $('#FnbOrderCreationTimeTo').val() || null
        };
    }

    function renderServiceStatus(s) {
        if (s === 1) return '<span class="badge bg-secondary">' + l('FnbServiceStatus:Created') + '</span>';
        if (s === 2) return '<span class="badge bg-info text-dark">' + l('FnbServiceStatus:Preparing') + '</span>';
        if (s === 3) return '<span class="badge bg-primary">' + l('FnbServiceStatus:Delivering') + '</span>';
        if (s === 4) return '<span class="badge bg-success">' + l('FnbServiceStatus:Served') + '</span>';
        if (s === 5) return '<span class="badge bg-danger">' + l('FnbServiceStatus:Cancelled') + '</span>';
        return '';
    }

    function renderPaymentStatus(s) {
        if (s === 1) return '<span class="badge bg-warning text-dark">' + l('FnbPaymentStatus:Unpaid') + '</span>';
        if (s === 2) return '<span class="badge bg-success">' + l('FnbPaymentStatus:Paid') + '</span>';
        if (s === 3) return '<span class="badge bg-danger">' + l('FnbPaymentStatus:Failed') + '</span>';
        return '';
    }

    var dataTable = $('#FnbOrderTable').DataTable(
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
                                    paymentStatusModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('CancelOrder'),
                                visible: function (data) {
                                    return (abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit')) &&
                                        data.record.serviceStatus !== 4 &&
                                        data.record.serviceStatus !== 5;
                                },
                                action: function (data) {
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
                        return (data || 0).toLocaleString('vi-VN');
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
                        if (!data) return '';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy HH:mm');
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