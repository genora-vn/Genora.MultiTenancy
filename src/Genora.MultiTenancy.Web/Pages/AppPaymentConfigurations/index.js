$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appPaymentConfigurations.appPaymentConfiguration;

    var createModal = new abp.ModalManager('/AppPaymentConfigurations/CreateModal');
    var editModal   = new abp.ModalManager('/AppPaymentConfigurations/EditModal');

    // Service trả List<T> (array thuần), không phải {items, totalCount}
    // nên dùng custom ajax thay vì abp.libs.datatables.createAjax
    function loadData(requestData, callback) {
        service.getList()
            .then(function (result) {
                // result là array hoặc {items:[...]}
                var rows = Array.isArray(result) ? result : (result.items || []);
                callback({
                    recordsTotal: rows.length,
                    recordsFiltered: rows.length,
                    data: rows
                });
            })
            .catch(function () {
                callback({ recordsTotal: 0, recordsFiltered: 0, data: [] });
            });
    }

    var dataTable = $('#PaymentConfigurationsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: false,
            paging: true,
            searching: false,
            scrollX: true,
            order: [[1, 'asc']],
            ajax: loadData,
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppPaymentConfigurations.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppPaymentConfigurations.Edit');
                                },
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppPaymentConfigurations.Delete') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppPaymentConfigurations.Delete');
                                },
                                confirmMessage: function (data) {
                                    return l('DeletionConfirmationMessage', data.record.paymentProviderName);
                                },
                                action: function (data) {
                                    service.delete(data.record.id)
                                        .then(function () {
                                            abp.notify.success(l('SuccessfullyDeleted'));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                    }
                },
                {
                    title: l('DisplayOrder'),
                    data: 'displayOrder',
                    orderable: true
                },
                {
                    title: l('PaymentProviderName'),
                    data: 'paymentProviderName',
                    orderable: true
                },
                {
                    title: l('AccountNumber'),
                    data: 'accountNumber',
                    render: function (data) { return data || '—'; }
                },
                {
                    title: l('AccountName'),
                    data: 'accountName',
                    render: function (data) { return data || '—'; }
                },
                {
                    title: l('BankBin'),
                    data: 'bankBin',
                    render: function (data) { return data || '—'; }
                },
                {
                    title: l('PaymentDescription'),
                    data: 'description',
                    render: function (data) { return data || '—'; }
                },
                {
                    title: l('IsActive'),
                    data: 'isActive',
                    render: function (data) {
                        return data
                            ? '<span class="badge bg-success">Đang dùng</span>'
                            : '<span class="badge bg-secondary">Tắt</span>';
                    }
                }
            ]
        })
    );

    $('#NewPaymentConfigButton').on('click', function () {
        createModal.open();
    });

    $('#RefreshPaymentConfigButton').on('click', function () {
        dataTable.ajax.reload();
    });

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });
});
