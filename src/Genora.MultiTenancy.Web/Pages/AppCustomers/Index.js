$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appCustomers.appCustomer;

    var createModal = new abp.ModalManager('/AppCustomers/CreateModal');
    var editModal = new abp.ModalManager('/AppCustomers/EditModal');

    function initDobPicker(modalManager) {
        if (!window.flatpickr) {
            return;
        }

        modalManager.getModal().find('.dob-input').each(function () {
            flatpickr(this, {
                dateFormat: "d/m/Y",   // dd/MM/yyyy
                allowInput: true
            });
        });
    }

    // Khi mở modal tạo mới
    createModal.onOpen(function () {
        initDobPicker(createModal);
    });

    // Khi mở modal chỉnh sửa
    editModal.onOpen(function () {
        initDobPicker(editModal);
    });

    function getFilter() {
        return {
            filterText: $('#CustomerFilterText').val()
        };
    }

    var dataTable = $('#CustomersTable').DataTable(
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
                                visible: abp.auth.isGranted('MultiTenancy.AppCustomers.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppCustomers.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('MultiTenancy.AppCustomers.Delete') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppCustomers.Delete'),
                                confirmMessage: function (data) {
                                    return l('AreYouSureToDelete', data.record.fullName);
                                },
                                action: function (data) {
                                    service.delete(data.record.id).then(function () {
                                        dataTable.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                { title: l('CustomerPhoneNumber'), data: "phoneNumber" },
                { title: l('CustomerFullName'), data: "fullName" },
                { title: l('CustomerType'), data: "customerTypeName" },
                { title: l('CustomerCode'), data: "customerCode" },
                {
                    title: l('IsActive'),
                    data: "isActive",
                    render: function (active) {
                        return active
                            ? '<span class="badge bg-success">' + l('Yes') + '</span>'
                            : '<span class="badge bg-secondary">' + l('No') + '</span>';
                    }
                }
            ]
        })
    );

    $('#NewCustomerButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#SearchCustomerButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    createModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });
});
