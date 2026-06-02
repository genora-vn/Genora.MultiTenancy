$(function () {
    var bookingService = genora.multiTenancy.appServices.caddies.caddieBooking;
    var canEdit = $('#CanEdit').val() === 'true';
    var canDelete = $('#CanDelete').val() === 'true';

    var dataTable = $('#CaddieBookingsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(bookingService.getList, function () {
                return {
                    filter: $('#BookingFilter').val() || undefined,
                    status: $('#BookingStatusFilter').val() || undefined,
                    paymentStatus: $('#BookingPaymentFilter').val() || undefined,
                    fromDate: $('#BookingFromDate').val() || undefined,
                    toDate: $('#BookingToDate').val() || undefined
                };
            }),
            columnDefs: [
                {
                    title: 'Thao tác',
                    orderable: false,
                    width: '60px',
                    render: function (data, type, row) {
                        var items = [];
                        items.push('<li><a class="dropdown-item booking-action-detail" data-id="' + row.id + '"><i class="fa fa-eye me-2 text-primary"></i>Xem chi tiết</a></li>');
                        if (canEdit && row.status !== 3 && row.status !== 4) {
                            items.push('<li><a class="dropdown-item booking-action-status" data-id="' + row.id + '" data-status="' + row.status + '"><i class="fa fa-exchange-alt me-2"></i>Cập nhật trạng thái</a></li>');
                        }
                        if (canDelete) {
                            items.push('<li><hr class="dropdown-divider"></li>');
                            items.push('<li><a class="dropdown-item text-danger booking-action-delete" data-id="' + row.id + '" data-code="' + row.bookingCode + '"><i class="fa fa-trash me-2"></i>Xóa</a></li>');
                        }
                        return '<div class="dropdown"><button class="caddie-action-btn dropdown-toggle" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button><ul class="dropdown-menu dropdown-menu-end shadow-sm">' + items.join('') + '</ul></div>';
                    }
                },
                {
                    title: 'Mã Booking',
                    data: 'bookingCode',
                    render: function (data) { return '<span class="caddie-code">' + data + '</span>'; }
                },
                {
                    title: 'Khách hàng',
                    data: 'customerName',
                    render: function (data, type, row) {
                        return '<strong>' + data + '</strong><br/><small style="color:var(--caddie-on-surface-variant);">' + (row.phoneMasked || '') + '</small>';
                    }
                },
                {
                    title: 'Caddy',
                    data: 'caddieName',
                    render: function (data, type, row) {
                        return '<strong>' + (data || '—') + '</strong><br/><small style="color:var(--caddie-primary);">' + (row.caddieCode || '') + '</small>';
                    }
                },
                {
                    title: 'Ngày chơi',
                    data: 'bookingDate',
                    render: function (data, type, row) {
                        if (!data) return '—';
                        var date = luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                        var time = row.startTime ? row.startTime.substring(0, 5) : '';
                        return date + (time ? '<br/><small style="color:var(--caddie-on-surface-variant);">' + time + '</small>' : '');
                    }
                },
                {
                    title: 'Số hố',
                    data: 'numberOfHoles',
                    width: '60px',
                    render: function (data) { return data ? data + ' hố' : '—'; }
                },
                {
                    title: 'Trạng thái',
                    data: 'status',
                    render: function (data, type, row) {
                        var colors = { 1: 'background:#dbeafe;color:#1e40af;', 2: 'background:#dcfce7;color:#166534;', 3: 'background:#f0fdf4;color:#15803d;', 4: 'background:#fef2f2;color:#991b1b;' };
                        var style = colors[data] || 'background:#f3f4f6;color:#6b7280;';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">' + (row.statusText || '—') + '</span>';
                    }
                },
                {
                    title: 'Thanh toán',
                    data: 'paymentStatus',
                    render: function (data, type, row) {
                        var style = data === 2
                            ? 'background:#dcfce7;color:#166534;border:1px solid #bbf7d0;'
                            : 'background:#fef9c3;color:#a16207;border:1px solid #fde68a;';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">' + (row.paymentStatusText || '—') + '</span>';
                    }
                },
                {
                    title: 'Check-in',
                    data: 'checkinStatus',
                    render: function (data, type, row) {
                        var style = data === 2
                            ? 'background:#dcfce7;color:#166534;'
                            : 'background:#f3f4f6;color:#6b7280;';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">' + (row.checkinStatusText || '—') + '</span>';
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

        // Show only valid next statuses
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

    // Detail (navigate to caddie detail page)
    $(document).on('click', '.booking-action-detail', function () {
        var id = $(this).data('id');
        abp.notify.info('Chi tiết booking #' + id + ' — sẽ hiển thị khi hoàn thiện module.');
    });
});
