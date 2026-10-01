(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlGiftReceiptAdmin;
    var sales = genora.hoaLinhSales;
    var l = abp.localization.getResource('MultiTenancy');
    var page = 1, total = 0, requestNumber = 0;
    var modal;
    function text(key) { return l('HlGiftReceipt:' + key); }
    function escape(value) { return $('<span>').text(value == null ? '' : String(value)).html(); }
    function date(value) { return value ? new Date(value).toLocaleString(abp.localization.currentCulture.name) : ''; }
    function size() { return Number($('#PageSize').val()) || 20; }
    function filter() {
        if (!document.getElementById('ReceiptFilters').reportValidity()) return null;
        var dates = sales.dates();
        if (!dates) return null;
        return Object.assign(dates, {
            filter: $('#FilterText').val().trim(), custCode: $('#FilterCustCode').val().trim(),
            phoneNumber: $('#FilterPhone').val().trim(), campaignCode: $('#FilterCampaign').val().trim(),
            voucherCode: $('#FilterVoucher').val().trim(),
            campaignPeriod: $('#FilterPeriod').val() === '' ? null : Number($('#FilterPeriod').val()),
            status: $('#FilterStatus').val() === '' ? null : Number($('#FilterStatus').val())
        });
    }
    function render(items) {
        var body = $('#ReceiptTable tbody').empty();
        if (!items.length) body.append('<tr><td colspan="9" class="text-center text-muted">' + escape(text('Empty')) + '</td></tr>');
        items.forEach(function (x) {
            body.append('<tr><td><small>' + escape(x.receiptCode) + '</small></td><td>' + escape(x.custName)
                + '<br><small>' + escape(x.custCode) + '<br>' + escape(x.phoneNumber) + '</small></td><td>' + escape(x.address)
                + '</td><td>' + escape(x.campaignName) + '<br><small>' + escape(x.campaignCode) + ' / ' + escape(x.campaignPeriod)
                + '</small></td><td>' + escape(x.voucherName) + '<br><small>' + escape(x.voucherCode) + '</small></td><td>' + escape(x.quantity)
                + '</td><td>' + escape(date(x.confirmedAt)) + '</td><td><span class="badge bg-success">' + escape(text('Confirmed'))
                + '</span></td><td><button type="button" class="btn btn-sm btn-outline-primary receipt-detail" data-id="' + escape(x.id)
                + '">' + escape(text('Detail')) + '</button></td></tr>');
        });
        $('#ReceiptCount').text(text('Total') + ': ' + total);
        $('#PageNumber').text(page + ' / ' + Math.max(1, Math.ceil(total / size())));
        $('#BtnPrevious').prop('disabled', page <= 1);
        $('#BtnNext').prop('disabled', page * size() >= total);
    }
    function load() {
        var input = filter(); if (!input) return;
        var request = ++requestNumber;
        abp.ui.setBusy('#ReceiptResults');
        service.getList(Object.assign(input, { skipCount: (page - 1) * size(), maxResultCount: size() }))
            .then(function (r) { if (request === requestNumber) { total = r.totalCount; render(r.items); } })
            .catch(function () { if (request === requestNumber) abp.notify.error(text('LoadError')); })
            .always(function () { if (request === requestNumber) abp.ui.clearBusy('#ReceiptResults'); });
    }
    function detail(id) {
        service.get(id).then(function (x) {
            var fields = $('#ReceiptDetailFields').empty();
            var values = [
                ['ReceiptCode', x.receiptCode], ['CustCode', x.custCode], ['Customer', x.custName], ['Phone', x.phoneNumber], ['Address', x.address],
                ['CampaignCode', x.campaignCode], ['Campaign', x.campaignName], ['Period', x.campaignPeriod],
                ['From', date(x.campaignStartDate)], ['To', date(x.campaignEndDate)], ['VoucherCode', x.voucherCode], ['Gift', x.voucherName],
                ['VoucherType', x.voucherType], ['VoucherValue', x.voucherValue], ['Quantity', x.quantity], ['ConfirmedAt', date(x.confirmedAt)],
                ['Status', text('Confirmed')], ['Tier', x.membershipTier], ['Sales', x.accumulatedSales], ['Points', x.accumulatedPoints],
                ['DsrCode', x.dsrCode], ['DsrName', x.dsrName], ['DistributorCode', x.distributorCode], ['DistributorName', x.distributorName],
                ['Source', x.source], ['Note', x.note], ['CreatedAt', date(x.creationTime)]
            ];
            values.forEach(function (pair) { fields.append('<dt class="col-sm-4">' + escape(text(pair[0]))
                + '</dt><dd class="col-sm-8 text-break">' + escape(pair[1]) + '</dd>'); });
            modal.show();
        }).catch(function () { abp.notify.error(text('LoadError')); });
    }
    $(function () {
        modal = new bootstrap.Modal(document.getElementById('ReceiptDetail'));
        $('#ReceiptFilters').on('submit', function (e) { e.preventDefault(); page = 1; load(); });
        $('#BtnReset').on('click', function () { document.getElementById('ReceiptFilters').reset(); page = 1; load(); });
        $('#PageSize').on('change', function () { page = 1; load(); });
        $('#BtnPrevious').on('click', function () { if (page > 1) { page--; load(); } });
        $('#BtnNext').on('click', function () { if (page * size() < total) { page++; load(); } });
        $('#BtnExportExcel').on('click', function () { var input = filter(); if (input) sales.download('api/app/hl-sales-excel/gift-receipts', input, this); });
        $(document).on('click', '.receipt-detail', function () { detail($(this).data('id')); });
        load();
    });
})();
