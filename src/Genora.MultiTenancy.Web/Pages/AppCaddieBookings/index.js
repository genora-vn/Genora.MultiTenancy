$(function () {
    var bookingService = genora.multiTenancy.appServices.caddies.caddieBooking;
    var canEdit = $('#CanEdit').val() === 'true';
    var canDelete = $('#CanDelete').val() === 'true';

    // Init flatpickr for date filter
    flatpickr('#BookingDateFilter', {
        dateFormat: 'd/m/Y',
        allowInput: true
    });

    var dataTable = $('#CaddieBookingsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(bookingService.getList, function () {
                var dateVal = $('#BookingDateFilter').val();
                var isoDate = '';
                if (dateVal) {
                    var parts = dateVal.split('/');
                    if (parts.length === 3) isoDate = parts[2] + '-' + parts[1] + '-' + parts[0];
                }
                return {
                    filter: $('#BookingFilter').val() || undefined,
                    status: $('#BookingStatusFilter').val() || undefined,
                    paymentStatus: $('#BookingPaymentFilter').val() || undefined,
                    fromDate: isoDate || undefined,
                    toDate: isoDate || undefined
                };
            }),
            columnDefs: [
                {
                    title: 'Hành động',
                    orderable: false,
                    width: '70px',
                    render: function (data, type, row) {
                        var items = [];
                        items.push('<li><a class="dropdown-item booking-action-detail" data-id="' + row.id + '"><i class="fa fa-eye me-2 text-primary"></i>Chi tiết Booking</a></li>');
                        if (canEdit && row.status !== 3 && row.status !== 4) {
                            items.push('<li><a class="dropdown-item booking-action-status" data-id="' + row.id + '" data-status="' + row.status + '"><i class="fa fa-exchange-alt me-2"></i>Cập nhật trạng thái</a></li>');
                        }
                        if (canDelete) {
                            items.push('<li><hr class="dropdown-divider"></li>');
                            items.push('<li><a class="dropdown-item text-danger booking-action-delete" data-id="' + row.id + '" data-code="' + row.bookingCode + '"><i class="fa fa-trash me-2"></i>Hủy yêu cầu</a></li>');
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
                        return '<p class="mb-0" style="font-size:14px;">' + dt.toFormat('dd/MM/yyyy') + '</p><p class="mb-0" style="font-size:12px;color:var(--caddie-on-surface-variant);">' + dt.toFormat('hh:mm a') + '</p>';
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
                        return '<p class="mb-0" style="font-weight:600;">' + date + '</p>' + (time ? '<p class="mb-0" style="font-size:12px;color:var(--caddie-on-surface-variant);">' + time + '</p>' : '');
                    }
                },
                {
                    title: 'Caddy',
                    data: 'caddieName',
                    render: function (data, type, row) {
                        var initials = (data || '?').split(' ').map(function(n) { return n[0]; }).join('').substring(0, 2).toUpperCase();
                        return '<div class="d-flex align-items-center gap-2">' +
                            '<span class="d-inline-flex align-items-center justify-content-center rounded-circle" style="width:32px;height:32px;background:var(--caddie-surface-container-high);color:var(--caddie-primary);font-size:11px;font-weight:700;">' + initials + '</span>' +
                            '<span style="font-size:13px;">' + (data || '—') + '</span></div>';
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
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 10px;border-radius:20px;text-transform:uppercase;">' + (row.paymentStatusText || '—') + '</span>';
                    }
                },
                {
                    title: 'TT Chơi',
                    data: 'status',
                    render: function (data, type, row) {
                        var colors = {
                            1: 'background:#dbeafe;color:#1e40af;',
                            2: 'background:#fef3c7;color:#92400e;',
                            3: 'background:var(--caddie-surface-container-high);color:var(--caddie-on-surface-variant);',
                            4: 'background:#fef2f2;color:#991b1b;'
                        };
                        var style = colors[data] || 'background:#f3f4f6;color:#6b7280;';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 10px;border-radius:20px;text-transform:uppercase;">' + (row.statusText || '—') + '</span>';
                    }
                }
            ]
        })
    );

    // Search
    $('#BtnSearch').click(function () { dataTable.ajax.reload(); });
    $('#BookingFilter').on('keypress', function (e) { if (e.which === 13) dataTable.ajax.reload(); });

    // Status update
    $(document).on('click', '.booking-action-status', function () {
        var id = $(this).data('id');
        var currentStatus = $(this).data('status');
        $('#statusBookingId').val(id);

        var $select = $('#statusNewValue');
        $select.find('option').hide();
        if (currentStatus === 1) { $select.find('option[value="2"]').show(); $select.find('option[value="4"]').show(); $select.val('2'); }
        if (currentStatus === 2) { $select.find('option[value="3"]').show(); $select.find('option[value="4"]').show(); $select.val('3'); }

        $('#cancelReasonGroup').hide();
        var modal = new bootstrap.Modal(document.getElementById('statusUpdateModal'));
        modal.show();
    });

    $('#statusNewValue').change(function () {
        $('#cancelReasonGroup').toggle($(this).val() === '4');
    });

    $('#btnConfirmStatus').click(function () {
        var id = $('#statusBookingId').val();
        var newStatus = parseInt($('#statusNewValue').val());
        var cancelReason = $('#cancelReasonInput').val();

        if (newStatus === 4 && !cancelReason) {
            abp.notify.error('Vui lòng nhập lý do hủy');
            return;
        }

        bookingService.updateStatus(id, { status: newStatus, cancelReason: cancelReason }).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('statusUpdateModal')).hide();
            dataTable.ajax.reload();
            abp.notify.success('Cập nhật trạng thái thành công');
        }).catch(function (err) {
            abp.notify.error(err.message || 'Cập nhật thất bại');
        });
    });

    // Delete
    $(document).on('click', '.booking-action-delete', function () {
        var id = $(this).data('id');
        var code = $(this).data('code');
        abp.message.confirm('Bạn có chắc chắn muốn xóa booking "' + code + '"?', 'Xác nhận xóa')
            .then(function (confirmed) {
                if (confirmed) {
                    bookingService.delete(id).then(function () {
                        dataTable.ajax.reload();
                        abp.notify.success('Đã xóa booking');
                    });
                }
            });
    });

    // Detail - navigate to detail page
    $(document).on('click', '.booking-action-detail', function () {
        var id = $(this).data('id');
        window.location.href = '/AppCaddieBookings/Detail?id=' + id;
    });
});
