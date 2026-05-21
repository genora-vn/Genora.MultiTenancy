(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var bookingService = genora.multiTenancy.appServices.salonBeauties.salonBeautyBooking;

    var createModal = new abp.ModalManager('/SalonBeautyBookings/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyBookings/EditModal');
    var dataTable;
    var autoRefreshTimer = null;

    var canEdit = $('#CanEditBooking').val() === 'true';
    var canDelete = $('#CanDeleteBooking').val() === 'true';
    var canCancel = $('#CanCancelBooking').val() === 'true';
    var canUpdatePayment = $('#CanUpdatePaymentBooking').val() === 'true';
    var canUpdateStatus = canEdit;

    var STATUS_NEW = 0;
    var STATUS_CONFIRMED = 1;
    var STATUS_COMPLETED = 2;
    var STATUS_CANCELLED = 3;

    function normalizeStatus(status) {
        if (typeof status === 'number') return status;
        if (status === null || status === undefined) return null;
        var s = String(status).toLowerCase();
        if (s === 'new' || s === '0') return STATUS_NEW;
        if (s === 'confirmed' || s === '1') return STATUS_CONFIRMED;
        if (s === 'completed' || s === '2') return STATUS_COMPLETED;
        if (s === 'cancelled' || s === 'canceled' || s === '3') return STATUS_CANCELLED;
        return null;
    }

    function isCompleted(row) {
        return normalizeStatus(row.status) === STATUS_COMPLETED;
    }

    function isCancelled(row) {
        return normalizeStatus(row.status) === STATUS_CANCELLED;
    }

    function canCancelRow(row) {
        var s = normalizeStatus(row.status);
        return s === STATUS_NEW || s === STATUS_CONFIRMED;
    }

    function showModal(selector) {
        var el = document.querySelector(selector);
        if (!el) return;
        if (window.bootstrap && bootstrap.Modal) {
            bootstrap.Modal.getOrCreateInstance(el).show();
            return;
        }
        if ($.fn.modal) {
            $(selector).modal('show');
        }
    }

    function hideModal(selector) {
        var el = document.querySelector(selector);
        if (!el) return;
        if (window.bootstrap && bootstrap.Modal) {
            bootstrap.Modal.getOrCreateInstance(el).hide();
            return;
        }
        if ($.fn.modal) {
            $(selector).modal('hide');
        }
    }

    function formatCurrency(val) {
        if (!val && val !== 0) return '0đ';
        return Number(val).toLocaleString('vi-VN') + 'đ';
    }

    function toIsoDate(value) {
        if (!value) return null;
        var s = (value || '').trim();
        if (/^\d{4}-\d{2}-\d{2}$/.test(s)) return s;
        var m = s.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
        if (m) return m[3] + '-' + String(m[2]).padStart(2, '0') + '-' + String(m[1]).padStart(2, '0');
        return s;
    }

    function getStatusClass(status) {
        var normalized = normalizeStatus(status);
        var cls = 'status-new';
        if (normalized === STATUS_CONFIRMED) cls = 'status-confirmed';
        else if (normalized === STATUS_COMPLETED) cls = 'status-completed';
        else if (normalized === STATUS_CANCELLED) cls = 'status-cancelled';
        return cls;
    }

    function getStatusLabel(status, statusText) {
        if (statusText) return statusText;
        var normalized = normalizeStatus(status);
        if (normalized === STATUS_NEW) return 'Chờ xác nhận';
        if (normalized === STATUS_CONFIRMED) return 'Đang thực hiện';
        if (normalized === STATUS_COMPLETED) return 'Hoàn thành';
        if (normalized === STATUS_CANCELLED) return 'Đã hủy';
        return status || '--';
    }

    function getStatusBadge(status, statusText) {
        return '<span class="status-badge ' + getStatusClass(status) + '">' + getStatusLabel(status, statusText) + '</span>';
    }

    function getStatusCell(row) {
        var editIcon = '';
        if (canUpdateStatus && !isCancelled(row) && !isCompleted(row)) {
            editIcon = '<button type="button" class="booking-status-edit-btn" title="Cập nhật trạng thái" data-id="' + row.id + '" data-status="' + (row.status ?? '') + '" data-status-text="' + (row.statusText || '') + '"><i class="fa fa-pencil"></i></button>';
        }
        return '<div class="booking-status-cell">' + getStatusBadge(row.status, row.statusText) + editIcon + '</div>';
    }

    function getPaymentBadge(row) {
        var paymentStatus = row.paymentStatus;
        var text = row.paymentStatusText || '--';
        var normalized = typeof paymentStatus === 'string' ? paymentStatus.toLowerCase() : paymentStatus;
        var cls = 'payment-unpaid';
        if (normalized === 1 || normalized === 'partial') cls = 'payment-partial';
        else if (normalized === 2 || normalized === 'paid') cls = 'payment-paid';
        else if (normalized === 3 || normalized === 'refunded') cls = 'payment-refunded';

        var editIcon = '';
        if (canUpdatePayment && !isCancelled(row)) {
            editIcon = '<button type="button" class="booking-payment-edit-btn" title="Cập nhật thanh toán" data-id="' + row.id + '" data-payment-status="' + (row.paymentStatus ?? '') + '" data-payment-method="' + (row.paymentMethod ?? '') + '"><i class="fa fa-pencil"></i></button>';
        }

        return '<div class="booking-payment-cell"><span class="payment-badge ' + cls + '">' + text + '</span>' + editIcon + '</div>';
    }

    function buildCustomerCell(row) {
        var initials = (row.customerName || 'K').charAt(0).toUpperCase();
        var avatar = row.customerAvatar
            ? '<img src="' + row.customerAvatar + '" class="customer-avatar" onerror="this.style.display=\'none\';this.nextSibling.style.display=\'flex\'" /><div class="customer-avatar" style="display:none">' + initials + '</div>'
            : '<div class="customer-avatar">' + initials + '</div>';
        return '<div class="customer-cell">' + avatar +
            '<div><div class="customer-name">' + (row.customerName || '--') + '</div>' +
            '<div class="customer-phone">' + (row.customerPhoneMasked || row.customerPhone || '--') + '</div></div></div>';
    }

    function buildServiceCell(row) {
        var html = '<span class="service-summary">' + (row.servicesSummary || '--') + '</span>';
        if (row.serviceCount > 1) {
            html += '<span class="service-count-badge">' + row.serviceCount + ' dịch vụ</span>';
        }
        return html;
    }

    function disabledItem(icon, text) {
        return '<li><span class="dropdown-item disabled text-muted"><i class="fa ' + icon + ' me-2"></i>' + text + '</span></li>';
    }

    function buildActions(row) {
        var completed = isCompleted(row);
        var cancelled = isCancelled(row);
        var html = '<div class="dropdown">' +
            '<button class="btn btn-sm btn-light" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>' +
            '<ul class="dropdown-menu dropdown-menu-end">' +
            '<li><a class="dropdown-item" href="/SalonBeautyBookings/Detail?id=' + row.id + '"><i class="fa fa-eye me-2"></i>Xem chi tiết</a></li>';

        if (canEdit) {
            if (completed || cancelled) html += disabledItem('fa-edit', 'Chỉnh sửa');
            else html += '<li><a class="dropdown-item booking-edit-btn" href="#" data-id="' + row.id + '"><i class="fa fa-edit me-2"></i>Chỉnh sửa</a></li>';
        }

        if (canCancel) {
            if (completed || cancelled) html += disabledItem('fa-ban', 'Hủy lịch');
            else html += '<li><a class="dropdown-item booking-cancel-btn text-warning" href="#" data-id="' + row.id + '" data-status="' + row.status + '"><i class="fa fa-ban me-2"></i>Hủy lịch</a></li>';
        }

        if (canDelete) {
            html += '<li><hr class="dropdown-divider"></li>';
            if (completed) html += disabledItem('fa-trash', 'Xóa');
            else html += '<li><a class="dropdown-item booking-delete-btn text-danger" href="#" data-id="' + row.id + '"><i class="fa fa-trash me-2"></i>Xóa</a></li>';
        }

        html += '</ul></div>';
        return html;
    }


    function refreshBookingList(resetPaging) {
        reloadAll(resetPaging === true);
    }

    function setAutoRefresh(seconds) {
        if (autoRefreshTimer) {
            clearInterval(autoRefreshTimer);
            autoRefreshTimer = null;
        }
        seconds = parseInt(seconds || 0, 10);
        if (seconds > 0) {
            autoRefreshTimer = setInterval(function () {
                refreshBookingList(false);
            }, seconds * 1000);
        }
    }

    function getFilters() {
        return {
            filterText: $('#BookingFilterText').val(),
            fromDate: toIsoDate($('#BookingFromDateFilter').val()),
            toDate: toIsoDate($('#BookingToDateFilter').val()),
            status: $('#BookingStatusFilter').val() !== '' ? $('#BookingStatusFilter').val() : null,
            stylistId: $('#BookingStylistFilter').val() || null,
            locationId: $('#BookingLocationFilter').val() || null
        };
    }

    function reloadAll(resetPaging) {
        if (dataTable) {
            dataTable.ajax.reload(null, resetPaging === true);
        }
        loadStats();
    }

    function loadStats() {
        var f = getFilters();
        bookingService.getStatistics(f.fromDate, f.toDate).then(function (data) {
            $('#StatTotalBookings').text(data.totalBookings);
            $('#StatTotalValue').text(formatCurrency(data.totalValue));
            $('#StatCompletionRate').text(data.completionRate + '%');
            $('#StatNewUnprocessed').text(data.newUnprocessedCount);
            $('#StatCompletionTrend').text(data.completionTrendText || 'Ổn định');
            $('#StatTotalChange').text((data.totalBookingsChangePercent > 0 ? '+' : '') + data.totalBookingsChangePercent + '%');
            $('#StatValueChange').text((data.totalValueChangePercent > 0 ? '+' : '') + data.totalValueChangePercent + '%');
        });
    }

    function loadStylistFilter(locationId) {
        bookingService.getStylistLookup(locationId || null).then(function (data) {
            var $sel = $('#BookingStylistFilter');
            var prevVal = $sel.val();
            $sel.find('option:not(:first)').remove();
            $.each(data || [], function (i, s) {
                $sel.append('<option value="' + s.id + '">' + s.displayName + '</option>');
            });
            if (prevVal && $sel.find('option[value="' + prevVal + '"]').length) {
                $sel.val(prevVal);
            } else {
                $sel.val('');
            }
        });
    }

    function initDataTable() {
        dataTable = $('#SalonBeautyBookingsTable').DataTable(
            abp.libs.datatables.normalizeConfiguration({
                serverSide: true,
                paging: true,
                order: [[5, 'desc']],
                searching: false,
                scrollX: false,
                ajax: abp.libs.datatables.createAjax(function (input) {
                    var f = getFilters();
                    input.filterText = f.filterText;
                    input.fromDate = f.fromDate;
                    input.toDate = f.toDate;
                    if (f.status !== null) input.status = f.status;
                    if (f.stylistId) input.stylistId = f.stylistId;
                    if (f.locationId) input.locationId = f.locationId;
                    return bookingService.getList(input);
                }),
                columnDefs: [
                    {
                        title: 'Mã Booking',
                        data: 'bookingCode',
                        render: function (data, type, row) {
                            return '<a class="booking-code-link" href="/SalonBeautyBookings/Detail?id=' + row.id + data + '</a>';
                        }
                    },
                    { title: 'Khách hàng', data: null, render: function (data, type, row) { return buildCustomerCell(row); } },
                    { title: 'Điện thoại', data: 'customerPhoneMasked', render: function (data) { return data || '--'; } },
                    { title: 'Dịch vụ', data: null, render: function (data, type, row) { return buildServiceCell(row); } },
                    { title: 'Stylist', data: 'stylistName', render: function (data) { return data ? '<span class="stylist-name">' + data + '</span>' : '--'; } },
                    {
                        title: 'Ngày & Giờ',
                        data: null,
                        render: function (data, type, row) {
                            var st = row.startTime ? row.startTime.substring(0, 5) : '';
                            var et = row.endTime ? row.endTime.substring(0, 5) : '';
                            var d = new Date(row.bookingDate);
                            return '<div class="booking-datetime">' + d.toLocaleDateString('vi-VN') + '</div>' +
                                '<div class="booking-time">' + st + (et ? ' - ' + et : '') + '</div>';
                        }
                    },
                    { title: 'Trạng thái', data: null, orderable: false, render: function (data, type, row) { return getStatusCell(row); } },
                    { title: 'Thanh toán', data: null, orderable: false, render: function (data, type, row) { return getPaymentBadge(row); } },
                    { title: 'Tổng tiền', data: 'totalAmount', className: 'text-end', render: function (data) { return '<span class="booking-amount">' + formatCurrency(data) + '</span>'; } },
                    { title: '', data: null, orderable: false, className: 'text-center', render: function (data, type, row) { return buildActions(row); } }
                ]
            })
        );
    }

    function deleteBooking(id) {
        abp.message.confirm('Bạn có chắc muốn xóa lịch đặt này?', 'Xác nhận xóa', function (confirmed) {
            if (!confirmed) return;
            bookingService.delete(id).then(function () {
                abp.notify.success('Đã xóa lịch đặt.');
                reloadAll();
            }).catch(function (error) {
                var message = error?.message || error?.responseJSON?.error?.message || 'Xóa thất bại.';
                abp.notify.error(message);
            });
        });
    }

    function initListCancelModal(id) {
        $('#ListCancelBookingId').val(id);
        $('input[name="ListCancelReason"]').prop('checked', false);
        $('.sb-radio-card[data-list-cancel-reason]').removeClass('is-selected');
        $('#ListCancelNoteInput').val('');
        $('#ListCancelNoteValidation').addClass('d-none');
        $('#ListConfirmCancelBtn').prop('disabled', true);
        showModal('#ListCancelModal');
    }

    function validateListCancelModal() {
        var hasReason = $('input[name="ListCancelReason"]:checked').length > 0;
        var hasNote = $.trim($('#ListCancelNoteInput').val()).length > 0;
        $('#ListConfirmCancelBtn').prop('disabled', !(hasReason && hasNote));
        if (hasNote) $('#ListCancelNoteValidation').addClass('d-none');
    }

    function initListPaymentModal(btn) {
        var status = btn.data('payment-status') !== undefined && btn.data('payment-status') !== null && btn.data('payment-status') !== '' ? String(btn.data('payment-status')) : '0';
        var method = btn.data('payment-method');
        method = method !== undefined && method !== null ? String(method) : '';

        $('#ListPaymentBookingId').val(btn.data('id'));
        $('input[name="ListPaymentStatus"]').prop('checked', false);
        $('.sb-radio-card[data-payment-status]').removeClass('is-selected');
        $('input[name="ListPaymentStatus"][value="' + status + '"]').prop('checked', true).closest('.sb-radio-card').addClass('is-selected');

        $('input[name="ListPaymentMethod"]').prop('checked', false);
        $('.sb-radio-card[data-payment-method]').removeClass('is-selected');
        $('input[name="ListPaymentMethod"][value="' + method + '"]').prop('checked', true).closest('.sb-radio-card').addClass('is-selected');

        $('#ListConfirmPaymentBtn').prop('disabled', $('input[name="ListPaymentStatus"]:checked').length === 0);
        showModal('#ListPaymentModal');
    }

    function getNextStatus(current) {
        current = normalizeStatus(current);
        if (current === STATUS_NEW) return STATUS_CONFIRMED;
        if (current === STATUS_CONFIRMED) return STATUS_COMPLETED;
        return null;
    }

    function initListStatusModal(btn) {
        var current = normalizeStatus(btn.data('status'));
        var nextStatus = getNextStatus(current);
        var text = btn.data('status-text') || getStatusLabel(current);
        var cls = getStatusClass(current).replace('status-', 'sb-status-');

        $('#ListStatusBookingId').val(btn.data('id'));
        $('#ListStatusCurrentValue').val(current);
        $('#ListCurrentStatusPill').attr('class', 'sb-current-status-pill ' + cls).text(text);
        $('#ListStatusInternalNoteInput').val('');

        $('input[name="ListNextBookingStatus"]').prop('checked', false).prop('disabled', true);
        $('.sb-radio-card[data-list-status]').removeClass('is-selected is-current is-disabled');

        $('.sb-radio-card[data-list-status="' + current + '"]').addClass('is-current').find('input').prop('disabled', true);

        $('.sb-radio-card[data-list-status]').each(function () {
            var $card = $(this);
            var status = parseInt($card.attr('data-list-status'), 10);
            var $input = $card.find('input');
            if (status === nextStatus) {
                $card.removeClass('is-disabled');
                $input.prop('disabled', false);
            } else {
                $card.addClass('is-disabled');
                $input.prop('disabled', true);
            }
        });

        $('#ListConfirmStatusBtn').prop('disabled', true);
        showModal('#ListStatusModal');
    }

    $(function () {
        flatpickr('#BookingFromDateFilter', { dateFormat: 'd/m/Y', allowInput: true });
        flatpickr('#BookingToDateFilter', { dateFormat: 'd/m/Y', allowInput: true });

        loadStylistFilter();
        loadStats();
        initDataTable();
        $('#NewBookingButton').on('click', function () {
            createModal.open();
        });

        $('#SearchBookingButton').on('click', function () {
            reloadAll(true);
        });

        $('#RefreshBookingButton').on('click', function (e) {
            e.preventDefault();
            refreshBookingList(false);
        });

        $('#BookingAutoRefreshSelect').on('change', function () {
            setAutoRefresh($(this).val());
        });

        $('#BookingStatusFilter,#BookingStylistFilter').on('change', function () {
            reloadAll(true);
        });

        $('#BookingLocationFilter').on('change', function () {
            loadStylistFilter($(this).val());
            reloadAll(true);
        });

        $('#BookingFilterText').on('keypress', function (e) {
            if (e.which === 13) reloadAll(true);
        });

        $(document).on('click', '.booking-edit-btn', function (e) {
            e.preventDefault();
            editModal.open({ id: $(this).data('id') });
        });

        $(document).on('click', '.booking-delete-btn', function (e) {
            e.preventDefault();
            deleteBooking($(this).data('id'));
        });

        $(document).on('click', '.booking-cancel-btn', function (e) {
            e.preventDefault();
            initListCancelModal($(this).data('id'));
        });

        $(document).on('click', '.booking-payment-edit-btn', function (e) {
            e.preventDefault();
            e.stopPropagation();
            initListPaymentModal($(this));
        });

        $(document).on('click', '.booking-status-edit-btn', function (e) {
            e.preventDefault();
            e.stopPropagation();
            initListStatusModal($(this));
        });

        $(document).on('change', 'input[name="ListCancelReason"]', function () {
            $('.sb-radio-card[data-list-cancel-reason]').removeClass('is-selected');
            $(this).closest('.sb-radio-card').addClass('is-selected');
            validateListCancelModal();
        });

        $(document).on('change', 'input[name="ListPaymentStatus"]', function () {
            $('.sb-radio-card[data-payment-status]').removeClass('is-selected');
            $(this).closest('.sb-radio-card').addClass('is-selected');
            $('#ListConfirmPaymentBtn').prop('disabled', false);
        });

        $(document).on('change', 'input[name="ListPaymentMethod"]', function () {
            $('.sb-radio-card[data-payment-method]').removeClass('is-selected');
            $(this).closest('.sb-radio-card').addClass('is-selected');
        });

        $(document).on('change', 'input[name="ListNextBookingStatus"]', function () {
            $('.sb-radio-card[data-list-status]').removeClass('is-selected');
            $(this).closest('.sb-radio-card').addClass('is-selected');
            $('#ListConfirmStatusBtn').prop('disabled', false);
        });

        $('#ListCancelNoteInput').on('input keyup change', validateListCancelModal);

        $('#ListConfirmCancelBtn').on('click', function () {
            var id = $('#ListCancelBookingId').val();
            var reasonChecked = $('input[name="ListCancelReason"]:checked');
            var note = $.trim($('#ListCancelNoteInput').val());

            if (!reasonChecked.length || !note) {
                $('#ListCancelNoteValidation').removeClass('d-none');
                $('#ListConfirmCancelBtn').prop('disabled', true);
                return;
            }

            $('#ListConfirmCancelBtn').prop('disabled', true);
            bookingService.cancel(id, { cancelReason: parseInt(reasonChecked.val()), cancelNote: note })
                .then(function () {
                    hideModal('#ListCancelModal');
                    abp.notify.success('Đã hủy lịch đặt.');
                    reloadAll();
                })
                .catch(function (error) {
                    var message = error?.message || error?.responseJSON?.error?.message || 'Hủy thất bại.';
                    abp.notify.error(message);
                    $('#ListConfirmCancelBtn').prop('disabled', false);
                });
        });

        $('#ListConfirmPaymentBtn').on('click', function () {
            var id = $('#ListPaymentBookingId').val();
            var statusChecked = $('input[name="ListPaymentStatus"]:checked');
            if (!statusChecked.length) {
                $('#ListConfirmPaymentBtn').prop('disabled', true);
                return;
            }
            var status = parseInt(statusChecked.val());
            var methodValue = $('input[name="ListPaymentMethod"]:checked').val();
            var method = methodValue !== undefined && methodValue !== null && methodValue !== '' ? parseInt(methodValue) : null;

            $('#ListConfirmPaymentBtn').prop('disabled', true);
            bookingService.updatePayment(id, { paymentStatus: status, paymentMethod: method })
                .then(function () {
                    hideModal('#ListPaymentModal');
                    abp.notify.success('Cập nhật thanh toán thành công.');
                    reloadAll();
                })
                .catch(function (error) {
                    var message = error?.message || error?.responseJSON?.error?.message || 'Cập nhật thất bại.';
                    abp.notify.error(message);
                    $('#ListConfirmPaymentBtn').prop('disabled', false);
                });
        });

        $('#ListConfirmStatusBtn').on('click', function () {
            var id = $('#ListStatusBookingId').val();
            var selected = $('input[name="ListNextBookingStatus"]:checked');
            if (!selected.length) {
                $('#ListConfirmStatusBtn').prop('disabled', true);
                return;
            }

            var status = parseInt(selected.val());
            var note = $.trim($('#ListStatusInternalNoteInput').val());

            $('#ListConfirmStatusBtn').prop('disabled', true);
            bookingService.updateStatus(id, {
                status: status,
                note: note,
                internalNote: note,
                reason: note
            })
                .then(function () {
                    hideModal('#ListStatusModal');
                    abp.notify.success('Cập nhật trạng thái thành công.');
                    reloadAll();
                })
                .catch(function (error) {
                    var message = error?.message || error?.responseJSON?.error?.message || 'Cập nhật thất bại.';
                    abp.notify.error(message);
                    $('#ListConfirmStatusBtn').prop('disabled', false);
                });
        });

        createModal.onResult(function () {
            abp.notify.success('Đã tạo lịch đặt thành công.');
            reloadAll(true);
        });

        editModal.onResult(function () {
            abp.notify.success('Đã cập nhật lịch đặt.');
            reloadAll();
        });
    });
})();
