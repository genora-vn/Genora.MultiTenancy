$(function () {
    var scheduleService = genora.multiTenancy.appServices.caddies.caddieSchedule;
    var caddieService = genora.multiTenancy.appServices.caddies.caddie;
    var createModal = new abp.ModalManager(abp.appPath + 'AppCaddieSchedules/CreateModal');
    var canEdit = $('#CanEdit').val() === 'true';

    // ── Status filter on Calendar View ────────────────────────────────
    $('#FilterStatus').on('change', function () {
        var status = $(this).val();
        if (!status) {
            // Show all
            $('.caddie-schedule-card').show();
        } else {
            $('.caddie-schedule-card').each(function () {
                var cardStatus = $(this).data('status');
                $(this).toggle(String(cardStatus) === status);
            });
        }
    });

    // ── Click on schedule card → show detail modal ────────────────────
    $(document).on('click', '.caddie-schedule-card', function () {
        var $card = $(this);
        var id = $card.data('id');
        var caddieId = $card.data('caddie-id');

        // Load caddie info for avatar
        if (caddieId) {
            caddieService.get(caddieId).then(function (caddie) {
                if (caddie.avatar && caddie.avatar.startsWith('/uploads/')) {
                    $('#modalCaddieAvatar').attr('src', caddie.avatar);
                } else {
                    $('#modalCaddieAvatar').attr('src', '/images/default-avatar.png');
                }
            }).catch(function () {
                $('#modalCaddieAvatar').attr('src', '/images/default-avatar.png');
            });
        }

        var name = $card.find('.caddie-schedule-card-name').text();
        var code = $card.find('.caddie-schedule-card-code').text();
        var timeInfo = $card.find('.caddie-schedule-card-info').first().text().trim();

        // Get shift text from the card
        var shiftText = '—';
        $card.find('.caddie-schedule-card-info').each(function () {
            var txt = $(this).text().trim();
            if (txt.startsWith('Ca:')) {
                shiftText = txt.replace('Ca:', '').trim();
            }
        });

        var statusText = 'Trống lịch';
        if ($card.hasClass('booked')) statusText = 'Đang phục vụ';
        if ($card.hasClass('off')) statusText = 'Nghỉ';

        // Find note (last info that's not time or shift)
        var noteText = '—';
        var infos = $card.find('.caddie-schedule-card-info');
        if (infos.length > 2) {
            noteText = infos.last().text().trim();
        }

        $('#modalCaddieName').text(name);
        $('#modalCaddieCode').text(code);
        $('#modalTime').text(timeInfo || '—');
        $('#modalStatus').text(statusText);
        $('#modalShift').text(shiftText);
        $('#modalNote').text(noteText);
        $('#btnEditSchedule').data('id', id);

        var modal = new bootstrap.Modal(document.getElementById('scheduleDetailModal'));
        modal.show();
    });

    // ── Edit schedule from detail modal ───────────────────────────────
    $('#btnEditSchedule').click(function () {
        var id = $(this).data('id');
        bootstrap.Modal.getInstance(document.getElementById('scheduleDetailModal')).hide();
        createModal.open({ id: id });
    });

    // ── Delete single schedule from detail modal ─────────────────────
    $('#btnDeleteSchedule').click(function () {
        var id = $('#btnEditSchedule').data('id');
        abp.message.confirm('Bạn có chắc chắn muốn xóa ca làm việc này?', 'Xác nhận xóa').then(function (confirmed) {
            if (confirmed) {
                scheduleService.delete(id).then(function () {
                    bootstrap.Modal.getInstance(document.getElementById('scheduleDetailModal')).hide();
                    abp.notify.success('Đã xóa ca làm việc');
                    window.location.reload();
                }).catch(function (err) {
                    abp.notify.error(err.message || 'Không thể xóa ca làm việc');
                });
            }
        });
    });

    // ── Delete Range Modal ────────────────────────────────────────────
    var deleteFromDatePicker = flatpickr('#deleteFromDate', { dateFormat: 'd/m/Y' });
    var deleteToDatePicker = flatpickr('#deleteToDate', { dateFormat: 'd/m/Y' });

    $('#btnDeleteRange').click(function () {
        new bootstrap.Modal(document.getElementById('deleteRangeModal')).show();
    });

    $('#btnConfirmDeleteRange').click(function () {
        var fromDateStr = $('#deleteFromDate').val();
        var toDateStr = $('#deleteToDate').val();
        var fromTime = $('#deleteFromTime').val() || null;
        var toTime = $('#deleteToTime').val() || null;

        if (!fromDateStr || !toDateStr) {
            abp.notify.error('Vui lòng chọn từ ngày và đến ngày');
            return;
        }

        // Parse dd/mm/yyyy to ISO
        var fromParts = fromDateStr.split('/');
        var toParts = toDateStr.split('/');
        var fromDate = fromParts[2] + '-' + fromParts[1] + '-' + fromParts[0];
        var toDate = toParts[2] + '-' + toParts[1] + '-' + toParts[0];

        var payload = {
            fromDate: fromDate,
            toDate: toDate,
            fromTime: fromTime ? fromTime + ':00' : null,
            toTime: toTime ? toTime + ':00' : null
        };

        abp.ui.setBusy();
        $.ajax({
            url: '/api/app/caddie-schedule-excel/delete-range',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (result) {
                abp.ui.clearBusy();
                bootstrap.Modal.getInstance(document.getElementById('deleteRangeModal')).hide();
                var msg = 'Đã xóa ' + result.deletedCount + '/' + result.totalFound + ' ca làm việc.';
                if (result.skippedCount > 0) {
                    msg += ' Bỏ qua ' + result.skippedCount + ' ca đang có booking.';
                }
                abp.notify.success(msg);
                setTimeout(function () { window.location.reload(); }, 1500);
            },
            error: function (xhr) {
                abp.ui.clearBusy();
                var errMsg = 'Xóa thất bại.';
                if (xhr.responseJSON && xhr.responseJSON.error && xhr.responseJSON.error.message) {
                    errMsg = xhr.responseJSON.error.message;
                }
                abp.notify.error(errMsg);
            }
        });
    });

    // ── FAB: New Schedule ─────────────────────────────────────────────
    $('#NewScheduleButton').click(function () {
        createModal.open();
    });

    createModal.onResult(function () {
        window.location.reload();
    });

    // ── Excel Import ──────────────────────────────────────────────────
    $('#btnImportExcel').click(function () {
        $('#excelFileInput').click();
    });

    $('#excelFileInput').on('change', function () {
        var file = this.files[0];
        if (!file) return;

        var formData = new FormData();
        formData.append('file', file);

        abp.ui.setBusy();
        $.ajax({
            url: '/api/app/caddie-schedule-excel/upload',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (result) {
                abp.ui.clearBusy();
                var msg = 'Import hoàn tất: ' + result.successCount + '/' + result.totalRows + ' thành công.';
                if (result.errorCount > 0) {
                    msg += '\n\nLỗi (' + result.errorCount + '):\n' + (result.errors || []).join('\n');
                    abp.message.warn(msg, 'Kết quả Import').then(function () {
                        window.location.reload();
                    });
                } else {
                    abp.notify.success(msg);
                    setTimeout(function () { window.location.reload(); }, 1500);
                }
            },
            error: function (xhr) {
                abp.ui.clearBusy();
                var errMsg = 'Import thất bại.';
                if (xhr.responseJSON && xhr.responseJSON.error && xhr.responseJSON.error.message) {
                    errMsg = xhr.responseJSON.error.message;
                }
                abp.message.error(errMsg, 'Lỗi Import');
            }
        });

        // Reset file input
        $(this).val('');
    });
});
