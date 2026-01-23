$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appBookings.appBooking;
    var editModal = new abp.ModalManager('/AppBookings/EditModal');
    var viewModal = new abp.ModalManager('/AppBookings/ViewModal');

    // =========================
    // Date utils
    // =========================
    function pad2(n) { return n < 10 ? ('0' + n) : String(n); }

    function todayDdMmYyyy() {
        var d = new Date();
        return pad2(d.getDate()) + '/' + pad2(d.getMonth() + 1) + '/' + d.getFullYear();
    }

    function ensureDefaultTodayForDateInputs() {
        var today = todayDdMmYyyy();
        if (!$('#PlayDateFrom').val()) $('#PlayDateFrom').val(today);
        if (!$('#PlayDateTo').val()) $('#PlayDateTo').val(today);
    }

    function parseDdMmYyyyToIso(text) {
        if (!text) return null;
        var parts = String(text).split('/');
        if (parts.length !== 3) return null;
        return parts[2] + '-' + parts[1] + '-' + parts[0];
    }

    // =========================
    // Date picker (flatpickr)
    // =========================
    function initDatePickers() {
        ensureDefaultTodayForDateInputs();

        if (!window.flatpickr) return;

        flatpickr("#PlayDateFrom", {
            dateFormat: "d/m/Y",
            allowInput: true,
            defaultDate: new Date()
        });

        flatpickr("#PlayDateTo", {
            dateFormat: "d/m/Y",
            allowInput: true,
            defaultDate: new Date()
        });
    }
    initDatePickers();

    // =========================
    // Filter
    // =========================
    function getFilter() {
        return {
            filterText: $('#BookingFilterText').val(),
            status: $('#BookingStatusFilter').val() || null,
            source: $('#BookingSourceFilter').val() || null,
            playDateFrom: parseDdMmYyyyToIso($('#PlayDateFrom').val()),
            playDateTo: parseDdMmYyyyToIso($('#PlayDateTo').val())
        };
    }

    // =========================
    // Render helpers
    // =========================
    function fmtTimeSpanToHHmm(ts) {
        if (!ts) return '';
        var s = String(ts);
        return s.length >= 5 ? s.substring(0, 5) : s;
    }

    function renderPlayTime(row) {
        if (!row) return '';
        var from = fmtTimeSpanToHHmm(row.timeFrom);
        var to = fmtTimeSpanToHHmm(row.timeTo);
        if (!from && !to) return '';
        if (from && to) return from + ' - ' + to;
        return from || to;
    }

    function renderInvoiceBadge(v) {
        return v
            ? '<span class="badge bg-success">Có</span>'
            : '<span class="badge bg-danger">Không</span>';
    }

    // =========================
    // RowAction helpers (IMPORTANT)
    // =========================
    function getRowRecord(data) {
        // ABP rowAction callbacks đôi khi trả { record: {...} }, đôi khi trả thẳng {...}
        if (!data) return null;
        if (data.record) return data.record;
        return data;
    }

    function getRowStatus(data) {
        var rec = getRowRecord(data);
        if (!rec) return null;
        // status thường là number
        return (typeof rec.status === 'number') ? rec.status : null;
    }

    function isCancelledStatus(status) {
        return status === 4 || status === 5;
    }

    function canEdit() {
        return abp.auth.isGranted('MultiTenancy.AppBookings.Edit') ||
            abp.auth.isGranted('MultiTenancy.HostAppBookings.Edit');
    }

    function canDelete() {
        return abp.auth.isGranted('MultiTenancy.AppBookings.Delete') ||
            abp.auth.isGranted('MultiTenancy.HostAppBookings.Delete');
    }

    // =========================
    // DataTable
    // =========================
    var dataTable = $('#BookingsTable').DataTable(
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
                        // ✅ Actions luôn hiện để còn xem được (kể cả booking đã hủy)
                        visible: function () { return true; },
                        items: [
                            {
                                text: l('View') || 'Xem',
                                visible: function () { return true; },
                                action: function (data) {
                                    var rec = getRowRecord(data);
                                    if (!rec || !rec.id) return;
                                    viewModal.open({ id: rec.id });
                                }
                            },
                            {
                                text: l('Edit'),
                                visible: function (data) {
                                    if (!canEdit()) return false;
                                    var s = getRowStatus(data);
                                    if (s === null) return true;
                                    return !isCancelledStatus(s);
                                },
                                action: function (data) {
                                    var rec = getRowRecord(data);
                                    if (!rec || !rec.id) return;

                                    var s = (typeof rec.status === 'number') ? rec.status : null;
                                    if (s !== null && isCancelledStatus(s)) {
                                        abp.notify.warn('Booking đã hủy, không thể sửa.');
                                        return;
                                    }

                                    editModal.open({ id: rec.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: function (data) {
                                    if (!canDelete()) return false;
                                    var s = getRowStatus(data);
                                    if (s === null) return true;
                                    return !isCancelledStatus(s);
                                },
                                confirmMessage: function (data) {
                                    var rec = getRowRecord(data) || {};
                                    var code = rec.bookingCode || '';
                                    var baseMsg = l('DeletionConfirmationMessage') || 'Bạn có chắc muốn xóa?';
                                    return code ? (baseMsg + ' (' + code + ')') : baseMsg;
                                },
                                action: function (data) {
                                    var rec = getRowRecord(data);
                                    if (!rec || !rec.id) return;

                                    var s = (typeof rec.status === 'number') ? rec.status : null;
                                    if (s !== null && isCancelledStatus(s)) {
                                        abp.notify.warn('Booking đã hủy, không thể xóa.');
                                        return;
                                    }

                                    service['delete'](rec.id).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted') || 'Đã xóa thành công');
                                        dataTable.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },

                { title: l('BookingCode'), data: "bookingCode" },
                {
                    title: l('BookingCustomer'),
                    data: null,
                    render: function (data) {
                        var name = (data && data.customerName) || '';
                        var phone = (data && data.customerPhone) || '';
                        if (name && phone) return name + ' (' + phone + ')';
                        return name || phone || '';
                    }
                },

                { title: l('CustomerType'), data: "customerType" },

                {
                    title: l('BookingPlayDate'),
                    data: "playDate",
                    render: function (data) {
                        if (!data) return '';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                    }
                },
                {
                    title: 'Giờ chơi',
                    data: null,
                    render: function (data) {
                        return renderPlayTime(data);
                    }
                },

                { title: l('BookingNumberOfGolfers'), data: "numberOfGolfers" },

                {
                    title: l('BookingTotalPrice'),
                    data: "totalAmount",
                    render: function (amount) {
                        if (amount == null) return '';
                        return Number(amount).toLocaleString('vi-VN');
                    }
                },

                {
                    title: 'Xuất hóa đơn',
                    data: "isExportInvoice",
                    render: function (v) {
                        return renderInvoiceBadge(!!v);
                    }
                },

                {
                    title: l('BookingPaymentMethod'),
                    data: "paymentMethod",
                    render: function (pm) {
                        switch (pm) {
                            case 0: return l('PaymentMethod:COD');
                            case 1: return l('PaymentMethod:Online');
                            case 2: return l('PaymentMethod:BankTransfer');
                            default: return '';
                        }
                    }
                },

                {
                    title: l('BookingStatus'),
                    data: "status",
                    render: function (s) {
                        switch (s) {
                            case 0: return '<span class="badge bg-secondary">' + l('BookingStatus:Processing') + '</span>';
                            case 1: return '<span class="badge bg-info">' + l('BookingStatus:Confirmed') + '</span>';
                            case 2: return '<span class="badge bg-primary">' + l('BookingStatus:Paid') + '</span>';
                            case 3: return '<span class="badge bg-success">' + l('BookingStatus:Completed') + '</span>';
                            case 4: return '<span class="badge bg-warning">' + l('BookingStatus:CancelledRefund') + '</span>';
                            case 5: return '<span class="badge bg-danger">' + l('BookingStatus:CancelledNoRefund') + '</span>';
                            default: return '';
                        }
                    }
                },

                {
                    title: l('BookingSource'),
                    data: "source",
                    render: function (src) {
                        switch (src) {
                            case 0: return l('BookingSource:MiniApp');
                            case 1: return l('BookingSource:Hotline');
                            case 2: return l('BookingSource:Agent');
                            default: return '';
                        }
                    }
                }
            ]
        })
    );

    // Search
    $('#SearchBookingButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    $('#BookingFilterText').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload();
        }
    });

    // Export excel theo filter
    $('#ExportExcelBtn').click(function () {
        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.download('api/app/app-booking-excel/export', getFilter());
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });
});
