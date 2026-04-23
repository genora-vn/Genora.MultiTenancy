$(function () {
    var l       = abp.localization.getResource('MultiTenancy');
    var fnb     = window.genoraFnb;
    var service = genora.multiTenancy.appServices.appProOrders.appProOrder;

    var serviceStatusModal  = new abp.ModalManager('/AppProOrders/UpdateServiceStatusModal');
    var paymentStatusModal  = new abp.ModalManager('/AppProOrders/UpdatePaymentStatusModal');
    var cancelModal         = new abp.ModalManager('/AppProOrders/CancelModal');
    var detailModal         = new abp.ModalManager('/AppProOrders/DetailModal');

    var autoRefreshTimer = null;

    // ── Flatpickr date pickers ────────────────────────────────────────────────
    if (window.flatpickr) {
        $('.public-time-input').flatpickr({
            dateFormat: 'Y-m-d',
            allowInput: true
        });
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    function canEdit() {
        return abp.auth.isGranted('MultiTenancy.AppProOrders.Edit') ||
               abp.auth.isGranted('MultiTenancy.HostAppProOrders.Edit');
    }

    function canDelete() {
        return abp.auth.isGranted('MultiTenancy.AppProOrders.Delete') ||
               abp.auth.isGranted('MultiTenancy.HostAppProOrders.Delete');
    }

    function renderUpdateButton(id, type, title) {
        if (!canEdit() || !id) return '';
        return ' <button type="button" class="btn btn-sm btn-outline-secondary pro-inline-update-btn" '
            + 'data-pro-update-type="' + type + '" data-pro-id="' + id + '" '
            + 'title="' + title + '"><i class="fa fa-pen"></i></button>';
    }

    // ProServiceStatus: Created=1, Processing=2, Ready=3, Delivered=4, Cancelled=5
    function renderServiceStatus(s, row) {
        s = Number(s);
        var badge = '';
        if (s === 1) badge = '<span class="fnb-badge fnb-badge--neutral">'  + l('ProServiceStatus:Created')    + '</span>';
        else if (s === 2) badge = '<span class="fnb-badge fnb-badge--info">'     + l('ProServiceStatus:Processing') + '</span>';
        else if (s === 3) badge = '<span class="fnb-badge fnb-badge--primary">'  + l('ProServiceStatus:Ready')      + '</span>';
        else if (s === 4) badge = '<span class="fnb-badge fnb-badge--success">'  + l('ProServiceStatus:Delivered')  + '</span>';
        else if (s === 5) badge = '<span class="fnb-badge fnb-badge--danger">'   + l('ProServiceStatus:Cancelled')  + '</span>';
        return badge + renderUpdateButton(row && row.id, 'service', l('UpdateServiceStatus'));
    }

    // ProPaymentStatus: Unpaid=1, Paid=2, Refunded=3
    function renderPaymentStatus(s, row) {
        s = Number(s);
        var badge = '';
        if (s === 1) badge = '<span class="fnb-badge fnb-badge--warning">'  + l('ProPaymentStatus:Unpaid')   + '</span>';
        else if (s === 2) badge = '<span class="fnb-badge fnb-badge--success">'  + l('ProPaymentStatus:Paid')     + '</span>';
        else if (s === 3) badge = '<span class="fnb-badge fnb-badge--neutral">'  + l('ProPaymentStatus:Refunded') + '</span>';
        return badge + renderUpdateButton(row && row.id, 'payment', l('UpdatePaymentStatus'));
    }

    // Created(1) → Processing(2) → Ready(3) → Delivered(4)
    function getNextServiceAction(record) {
        if (!record) return null;
        var s = Number(record.serviceStatus || 0);
        if (s === 1) return { value: 2, text: l('ProServiceStatus:Processing') };
        if (s === 2) return { value: 3, text: l('ProServiceStatus:Ready') };
        if (s === 3) return { value: 4, text: l('ProServiceStatus:Delivered') };
        return null;
    }

    function getFilter() {
        return {
            filterText:       fnb.toNullableString($('#ProOrderFilterText').val()),
            serviceStatus:    fnb.toNullableInt($('#ProOrderServiceStatusFilter').val()),
            paymentStatus:    fnb.toNullableInt($('#ProOrderPaymentStatusFilter').val()),
            creationTimeFrom: fnb.toNullableString($('#ProOrderCreationTimeFrom').val()),
            creationTimeTo:   fnb.toNullableString($('#ProOrderCreationTimeTo').val())
        };
    }

    // ── DataTable ────────────────────────────────────────────────────────────

    var dataTable = $('#ProOrderTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging:     true,
            searching:  false,
            scrollX:    true,
            order: [[7, 'desc']],
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

                            // ── Xem chi tiết (Board/Detail) ──────────────────
                            {
                                text: l('ViewDetail'),
                                action: function (data) {
                                    var id = fnb.safeId(data);
                                    if (!id) return;
                                    window.location.href = '/AppProOrders/Board/Detail?id=' + id;
                                }
                            },

                            // ── Cập nhật nhanh trạng thái tiếp theo ──────────
                            {
                                text: function (data) {
                                    var next = getNextServiceAction(data.record || data);
                                    return next ? next.text : '';
                                },
                                visible: function (data) {
                                    return canEdit() && !!getNextServiceAction(data.record || data);
                                },
                                action: function (data) {
                                    var record = data.record || data;
                                    var next   = getNextServiceAction(record);
                                    if (!next) return;

                                    service.updateServiceStatus(record.id, {
                                        serviceStatus: next.value,
                                        internalNote:  record.internalNote || null
                                    }).then(function () {
                                        abp.notify.success(l('SavedSuccessfully'));
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            },

                            // ── Huỷ đơn ──────────────────────────────────────
                            {
                                text: l('CancelOrder'),
                                visible: function (data) {
                                    if (!canDelete()) return false;
                                    var record = data.record || data;
                                    if (!record) return false;
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
                { title: l('OrderCode'),    data: 'orderCode' },
                { title: l('BagTag'),       data: 'bagTag',        defaultContent: '' },
                { title: l('CustomerName'), data: 'customerName',  defaultContent: '' },
                {
                    title: l('CustomerPhoneMasked'),
                    data: 'customerPhone',
                    render: function (data, type, row) {
                        if (typeof UIHelper !== 'undefined' && UIHelper.renderPhoneWithTooltip) {
                            return UIHelper.renderPhoneWithTooltip(data, row.customerPhone);
                        }
                        return data || '';
                    }
                },
                {
                    title: l('TotalAmount'),
                    data: 'totalAmount',
                    render: function (data) {
                        return '<span class="fnb-price kitchen-payment-grand">' +
                               fnb.formatCurrency(data) +
                               '<span class="vnd-symbol">đ</span></span>';
                    }
                },
                {
                    title: l('ServiceStatus'),
                    data: 'serviceStatus',
                    render: function (s, type, row) { return renderServiceStatus(s, row); }
                },
                {
                    title: l('PaymentStatus'),
                    data: 'paymentStatus',
                    render: function (s, type, row) { return renderPaymentStatus(s, row); }
                },
                {
                    title: l('CreationTime'),
                    data: 'creationTime',
                    render: function (data) { return fnb.formatDateTime(data); }
                }
            ],
            drawCallback: function () {
                $('[data-toggle="tooltip"]').tooltip({ container: 'body', trigger: 'hover' });
            }
        })
    );

    // ── Auto-refresh ──────────────────────────────────────────────────────────

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
        var interval = parseInt($('#ProOrderAutoRefreshInterval').val() || '0', 10);
        if (!interval || interval <= 0) return;
        autoRefreshTimer = setInterval(function () {
            if (!document.hidden) reloadOrdersSilently();
        }, interval);
    }

    // ── SignalR realtime (pro-orders hub) ─────────────────────────────────────

    if (window.genoraProNotify) {
        window.genoraProNotify.init({
            viewAllUrl: '/AppProOrders',
            detailUrl: function (id) { return '/AppProOrders/Board/Detail?id=' + id; },
            onCreated: function () { reloadOrdersSilently(); },
            onUpdated: function () { reloadOrdersSilently(); }
        });
    }

    // ── Modal callbacks ───────────────────────────────────────────────────────

    serviceStatusModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload(null, false);
    });

    paymentStatusModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload(null, false);
    });

    cancelModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload(null, false);
    });

    // ── Nút filter / search / refresh ────────────────────────────────────────

    $('#SearchProOrderButton, #RefreshProOrderButton').on('click', function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    $('#ProOrderFilterText').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload();
        }
    });

    $('#ExportProOrderExcelButton').on('click', function (e) {
        e.preventDefault();
        genora.excel.download('api/app/app-pro-order-excel/export', getFilter());
    });

    $('#ProOrderAutoRefreshInterval').on('change', function () {
        startAutoRefresh();
    });

    $(document).on('visibilitychange', function () {
        if (document.hidden) {
            stopAutoRefresh();
        } else {
            startAutoRefresh();
        }
    });

    $('#ProOrderTable').on('click', '.pro-inline-update-btn', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var id = $(this).data('pro-id');
        var type = $(this).data('pro-update-type');
        if (!id) return;
        if (type === 'service') {
            serviceStatusModal.open({ id: id });
        } else if (type === 'payment') {
            paymentStatusModal.open({ id: id });
        }
    });

    startAutoRefresh();
});
