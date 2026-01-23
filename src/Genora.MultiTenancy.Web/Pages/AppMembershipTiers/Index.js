$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appMembershipTiers.appMembershipTier;

    var createModal = new abp.ModalManager('/AppMembershipTiers/CreateModal');
    var editModal = new abp.ModalManager('/AppMembershipTiers/EditModal');

    var dataTable = $('#MembershipTiersTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: true,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(service.getListWithFilter, function (request) {
                return {
                    filter: request.search?.value || null,
                    skipCount: request.start,
                    maxResultCount: request.length,
                    sorting: request.columns?.[request.order?.[0]?.column]?.data
                        ? request.columns[request.order[0].column].data + ' ' + request.order[0].dir
                        : null
                };
            }),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('MultiTenancy.AppMembershipTiers.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppMembershipTiers.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('MultiTenancy.AppMembershipTiers.Delete') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppMembershipTiers.Delete'),
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
                { title: l('MembershipTierCode'), data: "code" },
                { title: l('MembershipTierName'), data: "name" },
                { title: l('Description'), data: "description" },
                { title: l('MinTotalSpending'), data: "minTotalSpending" },
                { title: l('MinRounds'), data: "minRounds" },
                { title: l('EvaluationPeriod'), data: "evaluationPeriod" },
                { title: l('DisplayOrder'), data: "displayOrder" },
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

    $('#NewMembershipTierButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    createModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully'));
        dataTable.ajax.reload();
    });

    // Optional: validate MinTotalSpending / MinRounds >= 0
    //$(document).on('submit', '#CreateMembershipTierForm, #EditMembershipTierForm', function (e) {
    //    var $form = $(this);
    //    var spending = parseFloat($form.find('input[name$=".MinTotalSpending"]').val());
    //    var rounds = parseInt($form.find('input[name$=".MinRounds"]').val(), 10);

    //    if (!isNaN(spending) && spending < 0) {
    //        e.preventDefault();
    //        abp.message.warn(l('MinTotalSpendingMustBePositive'));
    //        return;
    //    }

    //    if (!isNaN(rounds) && rounds < 0) {
    //        e.preventDefault();
    //        abp.message.warn(l('MinRoundsMustBePositive'));
    //    }
    //});
});