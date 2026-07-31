$(function () {
    var bookingService = genora.multiTenancy.appServices.caddies.caddieBooking;
    var canEdit = $('#CanEdit').val() === 'true';
    var canDelete = $('#CanDelete').val() === 'true';

    // Init flatpickr for date filter
    flatpickr('#BookingDateFilter', { dateFormat: 'd/m/Y', allowInput: true });

    var dataTable = $('#CaddieBookingsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(bookingService.getList, function () {
                var dateVal = $('#BookingDateFilter').val();
                var isoDate = '';
                if (dateVal) { var parts = dateVal.split('/'); if (parts.length === 3) isoDate = parts[2] + '-' + parts[1] + '-' + parts[0]; }
                return {
                    filter: $('#BookingFilter').val() || undefined,
                    status: $('#BookingStatusFilter').val() || undefined,
                    paymentStatus: $('#BookingPaymentFilter').val() || undefined,
                    checkinStatus: $('#BookingCheckinFilter').val() || undefined,
                    fromDate: isoDate || undefined,
                    toDate: isoDate || undefined
                };
            }),
            columnDefs: [
                {
                    title: 'Hành động',
                    orderable: false,
                    width: '80px',
                    render: function (data, type, row) {
                        var items = [];
                        items.push('<li><a class="dropdown-item booking-action-detail" data-id="' + row.id + '"><i class="fa fa-eye me-2 text-primary"></i>Chi tiết Booking</a></li>');
                        if (canEdit && row.status !== 3 && row.status !== 4) {
                            items.push('<li><a class="dropdown-item booking-action-status" data-id="' + row.id + '" data-status="' + row.status + '"><i class="fa fa-exchange-alt me-2"></i>Cập nhật TT Chơi</a></li>');
                            items.push('<li><a class="dropdown-item booking-action-payment" data-id="' + row.id + '" data-payment="' + row.paymentStatus + '"><i class="fa fa-credit-card me-2"></i>Cập nhật TT Thanh toán</a></li>');
                            items.push('<li><a class="dropdown-item booking-action-checkin" data-id="' + row.id + '" data-checkin="' + row.checkinStatus + '"><i class="fa fa-map-marker-alt me-2"></i>Cập nhật TT Checkin</a></li>');
                        }
                        if (canEdit && row.status !== 3 && row.status !== 4) {
                            items.push('<li><hr class="dropdown-divider"></li>');
                            items.push('<li><a class="dropdown-item text-danger booking-action-cancel" data-id="' + row.id + '" data-code="' + row.bookingCode + '"><i class="fa fa-ban me-2"></i>Hủy yêu cầu</a></li>');
                        }
                        return '<div class="dropdown"><button class="caddie-action-btn dropdown-toggle" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button><ul class="dropdown-menu dropdown-menu-end shadow-sm">' + items.join('') + '</ul></div>';
                    }
                },
                {
                    title: 'Mã Booking',
                    data: 'bookingCode',
                    render: function (data) { return '<span style="font-weight:700;color:var(--caddie-primary);">#' + data + '</span>'; }
                },
                {
                    title: 'Ngày & Giờ đặt',
                    data: 'creationTime',
                    render: function (data) {
                        if (!data) return '—';
                        var dt = luxon.DateTime.fromISO(data);
                        return '<p class="mb-0" style="font-size:13px;">' + dt.toFormat('dd/MM/yyyy') + '</p><p class="mb-0" style="font-size:11px;color:var(--caddie-on-surface-variant);">' + dt.toFormat('hh:mm a') + '</p>';
                    }
                },
                {
                    title: 'Tên khách hàng',
                    data: 'customerName',
                    render: function (data) { return '<span style="font-weight:500;">' + (data || '—') + '</span>'; }
                },
                {
                    title: 'Ngày & Giờ chơi',
                    data: 'bookingDate',
                    render: function (data, type, row) {
                        if (!data) return '—';
                        var date = luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                        var time = row.startTime ? row.startTime.substring(0, 5) : '';
                        return '<p class="mb-0" style="font-weight:600;">' + date + '</p>' + (time ? '<p class="mb-0" style="font-size:11px;color:var(--caddie-on-surface-variant);">' + time + '</p>' : '');
                    }
                },
                {
                    title: 'Caddy',
                    data: 'caddieNames',
                    render: function (data, type, row) {
                        var names = data || row.caddieName || '';
                        if (!names) return '<span style="font-size:13px;color:var(--caddie-on-surface-variant);">—</span>';
                        var first = names.split(',')[0].trim();
                        var initials = (first || '?').split(' ').map(function(n) { return n[0]; }).join('').substring(0, 2).toUpperCase();
                        return '<div class="d-flex align-items-center gap-2">' +
                            '<span class="d-inline-flex align-items-center justify-content-center rounded-circle" style="width:28px;height:28px;background:var(--caddie-surface-container-high);color:var(--caddie-primary);font-size:10px;font-weight:700;flex-shrink:0;">' + initials + '</span>' +
                            '<span style="font-size:13px;">' + names + '</span></div>';
                    }
                },
                {
                    title: 'Tổng phí Caddy',
                    data: 'totalCaddieFee',
                    className: 'text-end',
                    render: function (data) {
                        var v = Number(data || 0);
                        return '<span style="font-weight:600;">' + v.toLocaleString('vi-VN') + 'đ</span>';
                    }
                },
                {
                    title: 'TT Thanh toán',
                    data: 'paymentStatus',
                    className: 'text-center',
                    render: function (data, type, row) {
                        var style = data === 2
                            ? 'background:var(--caddie-surface-container-high);color:var(--caddie-primary);'
                            : 'background:#fef2f2;color:#991b1b;';
                        var icon = canEdit && row.status !== 4 ? '<i class="fa fa-pencil-alt ms-1 booking-inline-payment" data-id="' + row.id + '" data-payment="' + data + '" style="cursor:pointer;font-size:9px;opacity:0.6;"></i>' : '';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 10px;border-radius:20px;text-transform:uppercase;">' + (row.paymentStatusText || '—') + '</span>' + icon;
                    }
                },
                {
                    title: 'TT Chơi',
                    data: 'status',
                    className: 'text-center',
                    render: function (data, type, row) {
                        var colors = { 1: 'background:#dbeafe;color:#1e40af;', 2: 'background:#fef3c7;color:#92400e;', 3: 'background:var(--caddie-surface-container-high);color:var(--caddie-on-surface-variant);', 4: 'background:#fef2f2;color:#991b1b;' };
                        var style = colors[data] || 'background:#f3f4f6;color:#6b7280;';
                        var icon = canEdit && data !== 3 && data !== 4 ? '<i class="fa fa-pencil-alt ms-1 booking-inline-status" data-id="' + row.id + '" data-status="' + data + '" style="cursor:pointer;font-size:9px;opacity:0.6;"></i>' : '';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 10px;border-radius:20px;text-transform:uppercase;">' + (row.statusText || '—') + '</span>' + icon;
                    }
                },
                {
                    title: 'TT Checkin',
                    data: 'checkinStatus',
                    className: 'text-center',
                    render: function (data, type, row) {
                        var style = data === 2
                            ? 'background:#dcfce7;color:#166534;'
                            : 'background:#f3f4f6;color:#6b7280;';
                        var icon = canEdit && row.status !== 4 && data !== 2 ? '<i class="fa fa-pencil-alt ms-1 booking-inline-checkin" data-id="' + row.id + '" data-checkin="' + data + '" style="cursor:pointer;font-size:9px;opacity:0.6;"></i>' : '';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 10px;border-radius:20px;text-transform:uppercase;">' + (row.checkinStatusText || '—') + '</span>' + icon;
                    }
                }
            ]
        })
    );

    // Search
    $('#BtnSearch').click(function () { dataTable.ajax.reload(); });
    $('#BookingFilter').on('keypress', function (e) { if (e.which === 13) dataTable.ajax.reload(); });

    // ── Status Update Modal ──────────────────────────────────────────
    $(document).on('click', '.booking-action-status, .booking-inline-status', function () {
        var id = $(this).data('id');
        var currentStatus = $(this).data('status');
        $('#statusBookingId').val(id);
        var $select = $('#statusNewValue');
        $select.find('option').hide();
        if (currentStatus === 1) { $select.find('option[value="2"]').show(); $select.find('option[value="4"]').show(); $select.val('2'); }
        if (currentStatus === 2) { $select.find('option[value="3"]').show(); $select.find('option[value="4"]').show(); $select.val('3'); }
        $('#cancelReasonGroup').hide();
        new bootstrap.Modal(document.getElementById('statusUpdateModal')).show();
    });

    $('#statusNewValue').change(function () { $('#cancelReasonGroup').toggle($(this).val() === '4'); });

    $('#btnConfirmStatus').click(function () {
        var id = $('#statusBookingId').val();
        var newStatus = parseInt($('#statusNewValue').val());
        var cancelReason = $('#cancelReasonInput').val();
        if (newStatus === 4 && !cancelReason) { abp.notify.error('Vui lòng nhập lý do hủy'); return; }
        bookingService.updateStatus(id, { status: newStatus, cancelReason: cancelReason }).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('statusUpdateModal')).hide();
            dataTable.ajax.reload();
            abp.notify.success('Cập nhật trạng thái thành công');
        }).catch(function (err) { abp.notify.error(err.message || 'Cập nhật thất bại'); });
    });

    // ── Payment Update Modal ─────────────────────────────────────────
    $(document).on('click', '.booking-action-payment, .booking-inline-payment', function () {
        var id = $(this).data('id');
        var currentPayment = $(this).data('payment');
        $('#paymentBookingId').val(id);
        $('#paymentNewValue').val(currentPayment === 1 ? '2' : '1');
        new bootstrap.Modal(document.getElementById('paymentUpdateModal')).show();
    });

    $('#btnConfirmPayment').click(function () {
        var id = $('#paymentBookingId').val();
        var newPayment = parseInt($('#paymentNewValue').val());
        bookingService.updateStatus(id, { paymentStatus: newPayment }).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('paymentUpdateModal')).hide();
            dataTable.ajax.reload();
            abp.notify.success('Cập nhật thanh toán thành công');
        }).catch(function (err) { abp.notify.error(err.message || 'Cập nhật thất bại'); });
    });

    // ── Checkin Update Modal ─────────────────────────────────────────
    $(document).on('click', '.booking-action-checkin, .booking-inline-checkin', function () {
        var id = $(this).data('id');
        $('#checkinBookingId').val(id);
        new bootstrap.Modal(document.getElementById('checkinUpdateModal')).show();
    });

    $('#btnConfirmCheckin').click(function () {
        var id = $('#checkinBookingId').val();
        var newCheckin = parseInt($('#checkinNewValue').val());
        bookingService.updateStatus(id, { checkinStatus: newCheckin }).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('checkinUpdateModal')).hide();
            dataTable.ajax.reload();
            abp.notify.success('Cập nhật check-in thành công');
        }).catch(function (err) { abp.notify.error(err.message || 'Cập nhật thất bại'); });
    });

    // ── Cancel ───────────────────────────────────────────────────────
    $(document).on('click', '.booking-action-cancel', function () {
        var id = $(this).data('id');
        $('#statusBookingId').val(id);
        $('#statusNewValue').val('4');
        $('#cancelReasonGroup').show();
        $('#cancelReasonInput').val('');
        new bootstrap.Modal(document.getElementById('statusUpdateModal')).show();
    });

    // ── Detail ───────────────────────────────────────────────────────
    $(document).on('click', '.booking-action-detail', function () {
        window.location.href = '/AppCaddieBookings/Detail?id=' + $(this).data('id');
    });
});
