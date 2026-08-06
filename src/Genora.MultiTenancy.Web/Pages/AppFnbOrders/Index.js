$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var fnb = window.genoraFnb;
    var service = genora.multiTenancy.appServices.appFnbOrders.appFnbOrder;

    var serviceStatusModal = new abp.ModalManager('/AppFnbOrders/UpdateServiceStatusModal');
    var paymentStatusModal = new abp.ModalManager('/AppFnbOrders/UpdatePaymentStatusModal');
    var cancelModal        = new abp.ModalManager('/AppFnbOrders/CancelModal');
    var detailModal        = new abp.ModalManager('/AppFnbOrders/DetailModal');

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

    function canEditFnbOrder() {
        return abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
               abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit');
    }

    function renderUpdateButton(id, type, title) {
        if (!canEditFnbOrder() || !id) return '';
        return ' <button type="button" class="btn btn-sm btn-outline-secondary fnb-inline-update-btn" '
            + 'data-fnb-update-type="' + type + '" data-fnb-id="' + id + '" '
            + 'title="' + title + '"><i class="fa fa-pen"></i></button>';
    }

    function renderServiceStatus(s, row) {
        s = Number(s);
        var badge = '';

        if (s === 1) badge = '<span class="fnb-badge fnb-badge--neutral">' + l('FnbServiceStatus:Created') + '</span>';
        else if (s === 2) badge = '<span class="fnb-badge fnb-badge--info">' + l('FnbServiceStatus:Preparing') + '</span>';
        else if (s === 3) badge = '<span class="fnb-badge fnb-badge--primary">' + l('FnbServiceStatus:Delivering') + '</span>';
        else if (s === 4) badge = '<span class="fnb-badge fnb-badge--success">' + l('FnbServiceStatus:Served') + '</span>';
        else if (s === 5) badge = '<span class="fnb-badge fnb-badge--danger">' + l('FnbServiceStatus:Cancelled') + '</span>';

        return badge + renderUpdateButton(row && row.id, 'service', l('UpdateServiceStatus'));
    }

    function renderPaymentStatus(s, row) {
        s = Number(s);
        var badge = '';

        if (s === 1) badge = '<span class="fnb-badge fnb-badge--warning">' + l('FnbPaymentStatus:Unpaid') + '</span>';
        else if (s === 2) badge = '<span class="fnb-badge fnb-badge--success">' + l('FnbPaymentStatus:Paid') + '</span>';
        else if (s === 3) badge = '<span class="fnb-badge fnb-badge--danger">' + l('FnbPaymentStatus:Failed') + '</span>';

        return badge + renderUpdateButton(row && row.id, 'payment', l('UpdatePaymentStatus'));
    }

    function getNextServiceAction(record) {
        if (!record) return null;

        var s = Number(record.serviceStatus || 0);

        if (s === 1) return { value: 2, text: 'Đang xử lý' };
        if (s === 2) return { value: 3, text: 'Đang giao' };
        if (s === 3) return { value: 4, text: 'Đã phục vụ' };

        return null;
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
                            // ── Xem nhanh (modal) ────────────────────────────
                            {
                                text: l('View'),
                                action: function (data) {
                                    var id = fnb.safeId(data);
                                    if (!id) return;
                                    detailModal.open({ id: id });
                                }
                            },
                            // ── Xem chi tiết (Kitchen/Detail) ────────────────
                            {
                                text: l('ViewDetail'),
                                action: function (data) {
                                    var id = fnb.safeId(data);
                                    if (!id) return;
                                    window.location.href = `/AppFnbOrders/Kitchen/Detail?id=${id}`;
                                }
                            },
                            {
                                text: function (data) {
                                    var next = getNextServiceAction(data.record || data);
                                    return next ? next.text : '';
                                },
                                visible: function (data) {
                                    var canEdit = abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit');

                                    return canEdit && !!getNextServiceAction(data.record || data);
                                },
                                action: function (data) {
                                    var record = data.record || data;
                                    var next = getNextServiceAction(record);
                                    if (!next) return;

                                    service.updateServiceStatus(record.id, {
                                        serviceStatus: next.value,
                                        internalNote: record.internalNote || null
                                    }).then(function () {
                                        abp.notify.success('Đã cập nhật trạng thái đơn hàng');
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            },
                            {
                                text: l('CancelOrder'),
                                visible: function (data) {
                                    var canEdit = abp.auth.isGranted('MultiTenancy.AppFnbOrders.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppFnbOrders.Edit');

                                    var record = data.record || data;
                                    if (!canEdit || !record) {
                                        return false;
                                    }

                                    var status = Number(record.serviceStatus || 0);
                                    return status !== 4 && status !== 5;
                                },
                                action: function (data) {
                                    var record = data.record || data;
                                    if (!record || !record.id) return;
                                    cancelModal.open({ id: record.id });
                                }
                            }
                        ]
                    }
                },
                { title: l('OrderCode'), data: "orderCode" },
                { title: l('BagTag'), data: "bagTag" },
                { title: l('CustomerName'), data: "customerName", defaultContent: "" },
                {
                    title: l('CustomerPhoneMasked'),
                    data: "customerPhoneMasked",
                    // Sử dụng customerPhoneMasked để hiển thị, nhưng tooltip sẽ hiển thị customerPhone thực tế
                    render: function (data, type, row) {
                        return UIHelper.renderPhoneWithTooltip(data, row.customerPhone);
                    }
                },
                {
                    title: l('TotalAmount'),
                    data: "totalAmount",
                    render: function (data) {
                        return '<span class="fnb-price kitchen-payment-grand">' + fnb.formatCurrency(data) + '<span class="vnd-symbol">đ</span></span>';
                    }
                },
                {
                    title: l('ServiceStatus'),
                    data: "serviceStatus",
                    render: function (s, type, row) {
                        return renderServiceStatus(s, row);
                    }
                },
                {
                    title: l('PaymentStatus'),
                    data: "paymentStatus",
                    render: function (s, type, row) {
                        return renderPaymentStatus(s, row);
                    }
                },
                {
                    title: l('CreationTime'),
                    data: "creationTime",
                    render: function (data) {
                        return fnb.formatDateTime(data);
                    }
                }
            ],
            // Sau mỗi lần vẽ lại bảng, kích hoạt lại tooltip cho các phần tử mới
            drawCallback: function (settings) {
                // Kích hoạt tooltip của Bootstrap cho các phần tử mới vẽ
                $('[data-toggle="tooltip"]').tooltip({
                    container: 'body', // Đảm bảo tooltip không bị cắt bởi khung table
                    trigger: 'hover'   // Hiện khi di chuột vào
                });
            }
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

    var notify = window.genoraFnbNotify.init({
        viewAllUrl: '/AppFnbOrders',
        detailUrl: function (id) {
            return '/AppFnbOrders/Kitchen/Detail?id=' + id;
        },
        onCreated: function () {
            reloadOrdersSilently();
        },
        onUpdated: function () {
            reloadOrdersSilently();
        }
    });

    window.fnbPingBell = function () {
        return notify.invokePingBell();
    };

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

    $('#ExportFnbOrderExcelButton').on('click', function (e) {
        e.preventDefault();
        genora.excel.download('api/app/app-fnb-order-excel/export', getFilter());
    });

    $('#FnbOrderFilterText').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload();
        }
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

    $('#FnbOrderTable').on('click', '.fnb-inline-update-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var id = $(this).data('fnb-id');
        var type = $(this).data('fnb-update-type');
        if (!id) return;
        if (type === 'service') {
            serviceStatusModal.open({ id: id });
        } else if (type === 'payment') {
            paymentStatusModal.open({ id: id });
        }
    });

    startAutoRefresh();
});