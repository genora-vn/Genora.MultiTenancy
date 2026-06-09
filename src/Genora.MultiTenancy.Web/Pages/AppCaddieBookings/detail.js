$(function () {
    var bookingService = genora.multiTenancy.appServices.caddies.caddieBooking;
    var caddieService = genora.multiTenancy.appServices.caddies.caddie;
    var bookingId = $('#BookingId').val();
    var currentCaddieId = $('#CurrentCaddieId').val();

    // ── Phone hover ─────────────────────────────────────────────────
    $('.golfer-phone-hover, .caddie-phone-hover').each(function () {
        var $el = $(this);
        var masked = $el.data('masked') || '';
        var full = $el.data('full') || '';
        $el.on('mouseenter', function () { $el.text(full); });
        $el.on('mouseleave', function () { $el.text(masked); });
    });

    // ── Update Status Button ────────────────────────────────────────
    $('#btnUpdateStatus').click(function () {
        $('#modalCancelGroup').hide();
        new bootstrap.Modal(document.getElementById('statusModal')).show();
    });

    $('#modalStatusValue').change(function () {
        $('#modalCancelGroup').toggle($(this).val() === '4');
    });

    $('#btnConfirmStatus').click(function () {
        var newStatus = parseInt($('#modalStatusValue').val());
        var cancelReason = $('#modalCancelReason').val();

        if (newStatus === 4 && !cancelReason) {
            abp.notify.error('Vui lòng nhập lý do hủy');
            return;
        }

        bookingService.updateStatus(bookingId, { status: newStatus, cancelReason: cancelReason }).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('statusModal')).hide();
            abp.notify.success('Cập nhật trạng thái thành công');
            window.location.reload();
        }).catch(function (err) {
            abp.notify.error(err.message || 'Cập nhật thất bại');
        });
    });

    // ── Cancel Button ────────────────────────────────────────────────
    $('#btnCancelBooking').click(function () {
        $('#modalStatusValue').val('4');
        $('#modalCancelGroup').show();
        $('#modalCancelReason').val('');
        new bootstrap.Modal(document.getElementById('statusModal')).show();
    });

    // ── Change Caddy Button ─────────────────────────────────────────
    $('#btnChangeCaddy').click(function () {
        caddieService.getList({ maxResultCount: 100, status: 1 }).then(function (res) {
            var $select = $('#modalNewCaddieId');
            $select.find('option:not(:first)').remove();
            res.items.forEach(function (c) {
                if (c.id !== currentCaddieId) {
                    $select.append('<option value="' + c.id + '">' + c.caddieName + ' (' + c.caddieCode + ')</option>');
                }
            });
            new bootstrap.Modal(document.getElementById('changeCaddyModal')).show();
        });
    });

    $('#btnConfirmChangeCaddy').click(function () {
        var newCaddieId = $('#modalNewCaddieId').val();
        var note = $('#modalChangeCaddyNote').val();

        if (!newCaddieId) {
            abp.notify.error('Vui lòng chọn Caddy mới');
            return;
        }

        bookingService.changeCaddy(bookingId, newCaddieId, note).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('changeCaddyModal')).hide();
            abp.notify.success('Đã thay đổi Caddy thành công');
            window.location.reload();
        }).catch(function (err) {
            abp.notify.error(err.message || 'Thay đổi Caddy thất bại');
        });
    });
});
