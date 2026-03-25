$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var fnb = window.genoraFnb;
    var service = genora.multiTenancy.appServices.appFnbOrders.appFnbOrder;

    var detailModal = new abp.ModalManager('/AppFnbOrders/DetailModal');
    var serviceStatusModal = new abp.ModalManager('/AppFnbOrders/UpdateServiceStatusModal');
    var paymentStatusModal = new abp.ModalManager('/AppFnbOrders/UpdatePaymentStatusModal');
    var cancelModal = new abp.ModalManager('/AppFnbOrders/CancelModal');

    var autoRefreshTimer = null;

    if (window.flatpickr) {
        $('.public-time-input').flatpickr({
            dateFormat: "Y-m-d",
            allowInput: true
        });
    }

    function getFilter() {
        return {
            filterText: fnb.toNullableString($('#FnbOrderFilterText').val()),
            serviceStatus: fnb.toNullableInt($('#FnbOrderServiceStatusFilter').val()),
            paymentStatus: fnb.toNullableInt($('#FnbOrderPaymentStatusFilter').val()),
            creationTimeFrom: fnb.toNullableString($('#FnbOrderCreationTimeFrom').val()),
            creationTimeTo: fnb.toNullableString($('#FnbOrderCreationTimeTo').val())
        };
    }

    function renderServiceStatus(s) {
        s = Number(s);

        if (s === 1) return '<span class="fnb-badge fnb-badge--neutral">' + l('FnbServiceStatus:Created') + '</span>';
        if (s === 2) return '<span class="fnb-badge fnb-badge--info">' + l('FnbServiceStatus:Preparing') + '</span>';
        if (s === 3) return '<span class="fnb-badge fnb-badge--primary">' + l('FnbServiceStatus:Delivering') + '</span>';
        if (s === 4) return '<span class="fnb-badge fnb-badge--success">' + l('FnbServiceStatus:Served') + '</span>';
        if (s === 5) return '<span class="fnb-badge fnb-badge--danger">' + l('FnbServiceStatus:Cancelled') + '</span>';

        return '';
    }

    function renderPaymentStatus(s) {
        s = Number(s);

        if (s === 1) return '<span class="fnb-badge fnb-badge--warning">' + l('FnbPaymentStatus:Unpaid') + '</span>';
        if (s === 2) return '<span class="fnb-badge fnb-badge--success">' + l('FnbPaymentStatus:Paid') + '</span>';
        if (s === 3) return '<span class="fnb-badge fnb-badge--danger">' + l('FnbPaymentStatus:Failed') + '</span>';

        return '';
    }

    var dataTable = $('#FnbOrderTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [[7, "desc"]],
            ajax: fnb.createServerAjax(service.getList, getFilter, 'creationTime desc'),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('View'),
                                action: function (data) {
                                    var id = fnb.safeId(data);
                                    if (!id) return;
                                    detailModal.open({ id: id });
                                }
                            },
                            {
                                text: function (data) {
                                    var next = getNextServiceAction(data);
                                    return next ? next.text : '';
                                },
                                visible: function (data) {
                                    var canEdit = abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit');

                                    return canEdit && !!getNextServiceAction(data);
                                },
                                action: function (data) {
                                    var next = getNextServiceAction(data.record);
                                    console.log("next data", data)
                                    console.log("next", next)
                                    if (!next) return;

                                    service.updateServiceStatus(data.record.id, {
                                        serviceStatus: next.value,
                                        internalNote: data.record.internalNote || null
                                    }).then(function () {
                                        abp.notify.success('Đã cập nhật trạng thái đơn hàng');
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            },
                            {
                                text: l('UpdateServiceStatus'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit');
                                },
                                action: function (data) {
                                    var id = fnb.safeId(data);
                                    if (!id) return;
                                    serviceStatusModal.open({ id: id });
                                }
                            },
                            {
                                text: l('UpdatePaymentStatus'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit');
                                },
                                action: function (data) {
                                    var id = fnb.safeId(data);
                                    if (!id) return;
                                    paymentStatusModal.open({ id: id });
                                }
                            },
                            {
                                text: l('CancelOrder'),
                                visible: function (data) {
                                    var canEdit = abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit');

                                    if (!canEdit || !data) {
                                        return false;
                                    }

                                    var status = Number(data.serviceStatus || 0);
                                    console.log("status", status)
                                    return status !== 4 && status !== 5;
                                },
                                action: function (data) {
                                    console.log("data", data)
                                    var id = data.record.id;
                                    console.log("id", id)
                                    if (!id) return;
                                    cancelModal.open({ id: id });
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
                        return '<span class="fnb-price">' + fnb.formatCurrency(data) + '</span>';
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
                        return fnb.formatDateTime(data);
                    }
                }
            ]
        })
    );

    function reloadOrdersSilently() {
        if (document.hidden) return;
        dataTable.ajax.reload(null, false);
    }

    function stopAutoRefresh() {
        if (autoRefreshTimer) {
            clearInterval(autoRefreshTimer);
            autoRefreshTimer = null;
        }
    }

    function startAutoRefresh() {
        stopAutoRefresh();

        var interval = parseInt($('#FnbOrderAutoRefreshInterval').val() || '0', 10);
        if (!interval || interval <= 0) return;

        autoRefreshTimer = setInterval(function () {
            reloadOrdersSilently();
        }, interval);
    }

    function getNextServiceAction(record) {
        console.log('record.serviceStatus', record.serviceStatus);
        var s = Number(record.serviceStatus || 0);

        if (s === 1) return { value: 2, text: 'Đang xử lý' };
        if (s === 2) return { value: 3, text: 'Đang giao' };
        if (s === 3) return { value: 4, text: 'Đã phục vụ' };

        return null;
    }

    $('#FnbOrderAutoRefreshInterval').on('change', function () {
        startAutoRefresh();
    });

    $(document).on('visibilitychange', function () {
        if (document.hidden) {
            stopAutoRefresh();
        } else {
            startAutoRefresh();
        }
    });

    $('#SearchFnbOrderButton, #RefreshFnbOrderButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    $('#FnbOrderFilterText').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload();
        }
    });

    $('.fnb-status-item').each(function () {
        const isCurrent = $(this).data('current');

        if (isCurrent) {
            $(this).addClass('disabled active')
                .css({ pointerEvents: 'none', opacity: 0.5 });

            $hidden.val($(this).data('value'));
        }
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

    startAutoRefresh();
});