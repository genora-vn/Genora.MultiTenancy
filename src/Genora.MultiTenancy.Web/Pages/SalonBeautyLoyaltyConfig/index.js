$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    function resolveSalonService(name) {
        var root = genora.multiTenancy.appServices && genora.multiTenancy.appServices.salonBeauties;
        if (!root || !root[name]) {
            throw new Error('Salon Beauty proxy not found: ' + name);
        }
        return root[name];
    }

    var configService = resolveSalonService('salonBeautyLoyaltyConfig');
    var tierService = resolveSalonService('salonBeautyLoyaltyBonusTier');
    var canEdit = $('#CanEditBonusTier').val() === 'true';

    var tierModal = new abp.ModalManager('/SalonBeautyLoyaltyConfig/BonusTierModal');

    $('#LoyaltyConfigForm').on('submit', function (e) {
        e.preventDefault();
        var rate = parseFloat($('#ExchangeRateInput').val()) || 0;
        if (rate <= 0) {
            abp.notify.warn(l('SalonBeautyLoyaltyConfig:ExchangeRateInvalid'));
            return;
        }
        configService.update({ exchangeRate: rate }).then(function () {
            abp.notify.success(l('SavedSuccessfully'));
        });
    });

    function htmlEncode(value) { return $('<div/>').text(value || '').html(); }
    function formatPoint(v) { if (v == null || v === '') return '0'; return parseInt(v, 10).toLocaleString('vi-VN'); }
    function getRowRecord(data) { if (!data) return null; if (data.record) return data.record; return data; }

    function buildListInput(request) {
        request = request || {};
        return {
            skipCount: request.start || 0,
            maxResultCount: 100,
            sorting: 'displayOrder asc, minAmount asc'
        };
    }

    var dataTable = $('#BonusTiersTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: false,
            searching: false,
            info: false,
            ajax: abp.libs.datatables.createAjax(tierService.getList, buildListInput),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: function (data) { return canEdit && getRowRecord(data); },
                                action: function (data) { var rec = getRowRecord(data); if (rec) tierModal.open({ id: rec.id }); }
                            },
                            {
                                text: l('Delete'),
                                visible: function (data) { return canEdit && getRowRecord(data); },
                                confirmMessage: function (data) {
                                    var rec = getRowRecord(data) || {};
                                    return abp.utils.formatString(l('SalonBeautyLoyaltyConfig:DeleteTierConfirm'), rec.name || '');
                                },
                                action: function (data) {
                                    var rec = getRowRecord(data); if (!rec) return;
                                    tierService.delete(rec.id).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            }
                        ]
                    },
                    width: '90px'
                },
                { title: l('SalonBeautyLoyaltyConfig:TierName'), data: 'name', render: function (d) { return htmlEncode(d); } },
                { title: l('SalonBeautyLoyaltyConfig:TierMinAmount'), data: 'minAmount', render: function (d) { return parseInt(d || 0, 10).toLocaleString('vi-VN') + 'đ'; }, width: '160px' },
                { title: l('SalonBeautyLoyaltyConfig:TierBonus'), data: 'bonusPoint', render: function (d) { return '<span class="text-success">+' + formatPoint(d) + ' ' + l('SalonBeautyDeposits:PointUnit') + '</span>'; }, width: '140px' },
                { title: l('Description'), data: 'description', render: function (d) { return htmlEncode(d); } },
                { title: l('SalonBeautyLoyaltyConfig:DisplayOrder'), data: 'displayOrder', width: '90px', className: 'text-center' },
                { title: l('SalonBeautyLoyaltyConfig:TierActive'), data: 'isActive', render: function (d) { return d ? '<span class="badge bg-success">' + l('Active') + '</span>' : '<span class="badge bg-secondary">' + l('Inactive') + '</span>'; }, width: '110px' }
            ]
        })
    );

    $('#NewBonusTierButton').on('click', function (e) { e.preventDefault(); tierModal.open(); });
    tierModal.onResult(function () { dataTable.ajax.reload(null, false); });
});
