$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appBookings.appBooking;

    var editModal = new abp.ModalManager('/AppBookings/EditModal');

    function getFilter() {
        return {
            filterText: $('#BookingFilterText').val(),
            status: $('#BookingStatusFilter').val(),
            source: $('#BookingSourceFilter').val()
        };
    }

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
                        items: [
                            {
                                text: l('Edit'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppBookings.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppBookings.Edit');
                                },
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
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
                        var name = data.customerName || '';
                        var phone = data.customerPhone || '';
                        if (name && phone) {
                            return name + ' (' + phone + ')';
                        }
                        return name || phone || '';
                    }
                },
                { title: l('BookingGolfCourse'), data: "golfCourseName" },
                {
                    title: l('BookingPlayDate'),
                    data: "playDate",
                    render: function (data) {
                        if (!data) return '';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                    }
                },
                { title: l('BookingNumberOfGolfers'), data: "numberOfGolfers" },
                {
                    title: l('BookingTotalPrice'),
                    data: "totalPrice",
                    render: function (amount) {
                        if (amount == null) return '';
                        // format VNĐ chỗ này cho dễ nhìn
                        return amount.toLocaleString('vi-VN');
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
                            case 0: return '<span class="badge bg-secondary">' + l('BookingStatus:Pending') + '</span>';
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

    $('#SearchBookingButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });
});