(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlAdmin;

    function formatCurrency(value) {
        if (!value && value !== 0) return '0';
        return new Intl.NumberFormat('vi-VN').format(value) + 'đ';
    }

    function getTierBadge(tier) {
        if (!tier) return '<span class="hl-loyalty-tier hl-tier-default">Chưa xếp hạng</span>';
        var cls = 'hl-tier-default';
        var t = tier.toLowerCase();
        if (t.includes('vàng') || t.includes('vang') || t.includes('gold')) cls = 'hl-tier-vang';
        else if (t.includes('bạc') || t.includes('bac') || t.includes('silver')) cls = 'hl-tier-bac';
        else if (t.includes('kim cương') || t.includes('kim cuong') || t.includes('diamond')) cls = 'hl-tier-kimcuong';
        return '<span class="hl-loyalty-tier ' + cls + '">' + tier + '</span>';
    }

    function showResult(customer) {
        $('#CustomerName').text(customer.custName || '-');
        $('#CustomerCode').text(customer.custCode || '');
        $('#MembershipTier').html(getTierBadge(customer.membershipTier || customer.loyaltyTier));

        var points = customer.loyaltyPoint != null ? customer.loyaltyPoint : (customer.accumulatedPoints || 0);
        $('#LoyaltyPoint').text(points.toLocaleString('vi-VN'));
        $('#AccumulatedSales').text(formatCurrency(customer.accumulatedSales));
        $('#PointsToNext').text(customer.pointsToNextTier != null ? customer.pointsToNextTier.toLocaleString('vi-VN') : '0');
        $('#NextTier').text(customer.nextMembershipTier || '-');

        $('#CustChannel').text(customer.custChannel || '-');
        $('#CustSubChannel').text(customer.custSubChannel || '-');
        $('#CustGroup').text(customer.custGroup || '-');
        $('#DsrName').text((customer.dsrName || '-') + (customer.dsrCode ? ' (' + customer.dsrCode + ')' : ''));
        $('#DistributorName').text(customer.distributorName || '-');
        $('#IsGkhl').html(customer.isGkhl
            ? '<span class="hl-gkhl-badge hl-gkhl-yes">Có tham gia</span>'
            : '<span class="hl-gkhl-badge hl-gkhl-no">Không</span>'
        );

        $('#LoyaltyResult').show();
        $('#LoyaltyEmpty').hide();
    }

    function search() {
        var phone = $('#FilterPhone').val().trim();
        if (!phone) { abp.notify.warn('Vui lòng nhập số điện thoại'); return; }

        abp.ui.setBusy('.abp-card');

        service.getCustomerByPhone(phone)
            .then(function (result) {
                if (result.success && result.data && result.data.length > 0) {
                    var customer = result.data[0];
                    if (customer.isCustomer === false) {
                        abp.notify.warn('Số điện thoại không tồn tại trong hệ thống Hoa Linh');
                        $('#LoyaltyResult').hide();
                        $('#LoyaltyEmpty').show();
                        return;
                    }
                    showResult(customer);
                } else {
                    abp.notify.warn(result.error || 'Không tìm thấy khách hàng');
                    $('#LoyaltyResult').hide();
                    $('#LoyaltyEmpty').show();
                }
            })
            .catch(function (err) { abp.notify.error('Lỗi khi tra cứu'); console.error(err); })
            .always(function () { abp.ui.clearBusy('.abp-card'); });
    }

    // Events
    $('#BtnSearch, #BtnRefresh').click(search);
    $('#FilterPhone').keypress(function (e) { if (e.which === 13) search(); });
})();
