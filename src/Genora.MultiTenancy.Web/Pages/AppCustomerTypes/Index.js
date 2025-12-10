$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appCustomerTypes.appCustomerType;

    var createModal = new abp.ModalManager('/AppCustomerTypes/CreateModal');
    var editModal = new abp.ModalManager('/AppCustomerTypes/EditModal');

    var dataTable = $('#CustomerTypesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: true,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(service.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('MultiTenancy.AppCustomerTypes.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppCustomerTypes.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('MultiTenancy.AppCustomerTypes.Delete') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppCustomerTypes.Delete'),
                                confirmMessage: function (data) {
                                    return l('AreYouSureToDelete', data.record.name);
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
                { title: l('CustomerTypeCode'), data: "code" },
                { title: l('CustomerTypeName'), data: "name" },
                { title: l('CustomerTypeDescription'), data: "description" },
                { title: l('CustomerTypeColorCode'), data: "colorCode" },
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

    createModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    $('#NewAppSettingButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    // ============================
    // HEX COLOR VALIDATION
    // ============================
    //function isValidHexColor(value) {
    //    if (!value || value.trim() === '') {
    //        return true; // cho phép để trống
    //    }
    //    return /^#([0-9A-Fa-f]{6})$/.test(value);
    //}

    //// Dùng event delegation vì form được load qua AJAX
    //$(document).on('submit', '#CreateCustomerTypeForm, #EditCustomerTypeForm', function (e) {
    //    debugger
    //    var $form = $(this);
    //    var $colorInput = $form.find('input[name$=".ColorCode"]');

    //    if ($colorInput.length === 0) {
    //        return; // không có field -> bỏ qua
    //    }

    //    var value = $colorInput.val();

    //    if (!isValidHexColor(value)) {
    //        e.preventDefault(); // chặn submit
    //        abp.message.warn(l('InvalidHexColor'));
    //    }
    //});
});
