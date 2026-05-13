(function () {
    var baseUrl = '/api/app/salon-beauty/bookings';
    var createModal = new abp.ModalManager('/SalonBeautyBookings/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyBookings/EditModal');
    var dataTable;

    var canEdit = $('#CanEditBooking').val() === 'true';
    var canDelete = $('#CanDeleteBooking').val() === 'true';
    var canCancel = $('#CanCancelBooking').val() === 'true';

    function formatCurrency(val) {
        if (!val && val !== 0) return '0đ';
        return Number(val).toLocaleString('vi-VN') + 'đ';
    }

    function getStatusBadge(status, statusText) {
        var cls = 'status-new';
        if (status === 1) cls = 'status-confirmed';
        else if (status === 2) cls = 'status-completed';
        else if (status === 3) cls = 'status-cancelled';
        return '<span class="status-badge ' + cls + '">' + (statusText || status) + '</span>';
    }

    function formatDateTime(date, start, end) {
        if (!date) return '--';
        var d = new Date(date);
        var dateStr = d.toLocaleDateString('vi-VN');
        var timeStr = '';
        if (start) {
            var s = start.split(':');
            timeStr = s[0] + ':' + s[1];
            if (end) {
                var e = end.split(':');
                timeStr += ' - ' + e[0] + ':' + e[1];
            }
        }
        return '<div class="booking-datetime">' + dateStr + '</div>' +
               (timeStr ? '<div class="booking-time">' + timeStr + '</div>' : '');
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

    function buildActions(row) {
        var html = '<div class="dropdown">' +
            '<button class="btn btn-sm btn-light" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button>' +
            '<ul class="dropdown-menu dropdown-menu-end">' +
            '<li><a class="dropdown-item" href="/SalonBeautyBookings/Detail?id=' + row.id + '" target="_blank"><i class="fa fa-eye me-2"></i>Xem chi tiết</a></li>';
        if (canEdit) {
            html += '<li><a class="dropdown-item booking-edit-btn" href="#" data-id="' + row.id + '"><i class="fa fa-edit me-2"></i>Chỉnh sửa</a></li>';
        }
        if (canCancel && row.status !== 3) {
            html += '<li><a class="dropdown-item booking-cancel-btn text-warning" href="#" data-id="' + row.id + '"><i class="fa fa-ban me-2"></i>Hủy lịch</a></li>';
        }
        if (canDelete) {
            html += '<li><hr class="dropdown-divider"></li>' +
                '<li><a class="dropdown-item booking-delete-btn text-danger" href="#" data-id="' + row.id + '"><i class="fa fa-trash me-2"></i>Xóa</a></li>';
        }
        html += '</ul></div>';
        return html;
    }

    function getFilters() {
        return {
            filterText: $('#BookingFilterText').val(),
            fromDate: $('#BookingFromDateFilter').val() || null,
            toDate: $('#BookingToDateFilter').val() || null,
            status: $('#BookingStatusFilter').val() !== '' ? $('#BookingStatusFilter').val() : null,
            stylistId: $('#BookingStylistFilter').val() || null
        };
    }

    function loadStats() {
        var f = getFilters();
        var params = {};
        if (f.fromDate) params.fromDate = f.fromDate;
        if (f.toDate) params.toDate = f.toDate;
        $.get(baseUrl + '/statistics', params).done(function (data) {
            $('#StatTotalBookings').text(data.totalBookings);
            $('#StatTotalValue').text(formatCurrency(data.totalValue));
            $('#StatCompletionRate').text(data.completionRate + '%');
            $('#StatNewUnprocessed').text(data.newUnprocessedCount);
            $('#StatCompletionTrend').text(data.completionTrendText || 'Ổn định');
            if (data.totalBookingsChangePercent > 0) {
                $('#StatTotalChange').text('+' + data.totalBookingsChangePercent + '%').removeClass('is-neutral').css('color', '#16a34a');
            }
            if (data.totalValueChangePercent > 0) {
                $('#StatValueChange').text('+' + data.totalValueChangePercent + '%').removeClass('is-neutral').css('color', '#16a34a');
            }
        });
    }

    function loadStylistFilter() {
        $.get(baseUrl + '/stylist-lookup').done(function (data) {
            var $sel = $('#BookingStylistFilter');
            $sel.find('option:not(:first)').remove();
            $.each(data, function (i, s) {
                $sel.append('<option value="' + s.id + '">' + s.displayName + '</option>');
            });
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
                    return $.ajax({
                        url: baseUrl,
                        type: 'GET',
                        data: input
                    });
                }),
                columnDefs: [
                    {
                        title: 'Mã Booking',
                        data: 'bookingCode',
                        render: function (data, type, row) {
                            return '<a class="booking-code-link" href="/SalonBeautyBookings/Detail?id=' + row.id + '" target="_blank">' + data + '</a>';
                        }
                    },
                    {
                        title: 'Khách hàng',
                        data: null,
                        render: function (data, type, row) {
                            return buildCustomerCell(row);
                        }
                    },
                    {
                        title: 'Điện thoại',
                        data: 'customerPhoneMasked',
                        render: function (data) { return data || '--'; }
                    },
                    {
                        title: 'Dịch vụ',
                        data: null,
                        render: function (data, type, row) {
                            return buildServiceCell(row);
                        }
                    },
                    {
                        title: 'Stylist',
                        data: 'stylistName',
                        render: function (data) {
                            return data ? '<span class="stylist-name">' + data + '</span>' : '--';
                        }
                    },
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
                    {
                        title: 'Trạng thái',
                        data: null,
                        render: function (data, type, row) {
                            return getStatusBadge(row.status, row.statusText);
                        }
                    },
                    {
                        title: 'Tổng tiền',
                        data: 'totalAmount',
                        className: 'text-end',
                        render: function (data) {
                            return '<span class="booking-amount">' + formatCurrency(data) + '</span>';
                        }
                    },
                    {
                        title: '',
                        data: null,
                        orderable: false,
                        className: 'text-center',
                        render: function (data, type, row) {
                            return buildActions(row);
                        }
                    }
                ]
            })
        );
    }

    function deleteBooking(id) {
        abp.message.confirm('Bạn có chắc muốn xóa lịch đặt này?', 'Xác nhận xóa', function (confirmed) {
            if (!confirmed) return;
            $.ajax({ url: baseUrl + '/' + id, type: 'DELETE' })
                .done(function () {
                    abp.notify.success('Đã xóa lịch đặt.');
                    dataTable.ajax.reload();
                    loadStats();
                })
                .fail(function () { abp.notify.error('Xóa thất bại.'); });
        });
    }

    function cancelBooking(id) {
        abp.message.confirm('Bạn có chắc muốn hủy lịch đặt này?', 'Xác nhận hủy', function (confirmed) {
            if (!confirmed) return;
            $.ajax({
                url: baseUrl + '/' + id + '/cancel',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ cancelReason: 2, cancelNote: 'Hủy từ danh sách' })
            })
                .done(function () {
                    abp.notify.success('Đã hủy lịch đặt.');
                    dataTable.ajax.reload();
                    loadStats();
                })
                .fail(function () { abp.notify.error('Hủy thất bại.'); });
        });
    }

    $(function () {
        flatpickr('#BookingFromDateFilter', { dateFormat: 'd/m/Y', locale: 'vn', allowInput: true });
        flatpickr('#BookingToDateFilter', { dateFormat: 'd/m/Y', locale: 'vn', allowInput: true });

        loadStylistFilter();
        loadStats();
        initDataTable();

        $('#NewBookingButton').on('click', function () {
            createModal.open();
        });

        $('#SearchBookingButton').on('click', function () {
            dataTable.ajax.reload();
            loadStats();
        });

        $('#BookingFilterText').on('keypress', function (e) {
            if (e.which === 13) { dataTable.ajax.reload(); loadStats(); }
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
            cancelBooking($(this).data('id'));
        });

        createModal.onResult(function () {
            dataTable.ajax.reload();
            loadStats();
        });

        editModal.onResult(function () {
            dataTable.ajax.reload();
            loadStats();
        });
    });
})();
