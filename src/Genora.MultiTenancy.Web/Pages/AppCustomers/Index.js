$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appCustomers.appCustomer;

    var createModal = new abp.ModalManager('/AppCustomers/CreateModal');
    var editModal = new abp.ModalManager('/AppCustomers/EditModal');

    function initPicker() {
        if (!window.flatpickr) return;
        flatpickr('#CreatedFrom', { dateFormat: "d/m/Y", allowInput: true });
        flatpickr('#CreatedTo', { dateFormat: "d/m/Y", allowInput: true });
    }
    initPicker();

    function parseNullableGuid(val) {
        val = (val || '').trim();
        return val ? val : null;
    }
    function parseNullableBool(val) {
        val = (val || '').trim();
        if (!val) return null;
        return val === 'true';
    }

    function parseDateDdMmYyyy(input) {
        input = (input || '').trim();
        if (!input) return null;
        var parts = input.split('/');
        if (parts.length !== 3) return null;
        var dd = parts[0], mm = parts[1], yyyy = parts[2];
        return `${yyyy}-${mm}-${dd}T00:00:00`;
    }

    // ✅ chỉ lấy keyword từ ô search của DataTable
    function buildListInput(request) {
        return {
            filterText: (request.search?.value || '').trim() || null,
            customerTypeId: parseNullableGuid($('#CustomerTypeId').val()),
            isActive: parseNullableBool($('#IsActiveFilter').val()),
            createdFrom: parseDateDdMmYyyy($('#CreatedFrom').val()),
            createdTo: parseDateDdMmYyyy($('#CreatedTo').val()),

            skipCount: request.start,
            maxResultCount: request.length,
            sorting: null
        };
    }

    var dataTable = $('#CustomersTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: true,
            scrollX: true,

            ajax: abp.libs.datatables.createAjax(service.getList, buildListInput),

            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('MultiTenancy.AppCustomers.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppCustomers.Edit'),
                                action: function (data) { editModal.open({ id: data.record.id }); }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('MultiTenancy.AppCustomers.Delete') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppCustomers.Delete'),
                                confirmMessage: function (data) { return l('AreYouSureToDelete', data.record.fullName); },
                                action: function (data) {
                                    service.delete(data.record.id).then(function () { dataTable.ajax.reload(); });
                                }
                            }
                        ]
                    }
                },

                { title: l('CustomerCode'), data: "customerCode" },
                { title: l('CustomerFullName'), data: "fullName" },
                { title: l('VgaCode'), data: "vgaCode" },
                { title: l('CustomerPhoneNumber'), data: "phoneNumber" },
                {
                    title: l('DateOfBirth'),
                    data: "dateOfBirth",
                    dataFormat: "date"
                },

                { title: l('CustomerType'), data: "customerTypeName" },

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

    // Tắt filter ngay khi gõ search của DataTable: chỉ search khi bấm Search
    $('#CustomersTable_filter input')
        .off('.DT')
        .on('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                // Enter = search
                dataTable.ajax.reload();
            }
        });

    $('#SearchCustomerButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    $('#NewCustomerButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    createModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    $('#DownloadCustomerTemplateBtn').click(function () {
        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.download(
            'api/app/app-customer-excel/template',
            {}
        );
    });

    $('#ImportCustomerExcelInput').change(function (e) {
        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.upload({
            url: 'api/app/app-customer-excel/import',
            fileInput: e.target,
            onSuccess: function () {
                dataTable.ajax.reload();
            }
        });
    });
});
