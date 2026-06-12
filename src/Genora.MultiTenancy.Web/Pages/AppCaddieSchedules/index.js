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

    // ── Excel Import with Preview ──────────────────────────────────
    var importFile = null;

    $('#btnImportExcel').click(function () {
        $('#excelFileInput').click();
    });

    $('#excelFileInput').on('change', function () {
        importFile = this.files[0];
        if (!importFile) return;

        var formData = new FormData();
        formData.append('file', importFile);

        abp.ui.setBusy();
        $.ajax({
            url: '/api/app/caddie-schedule-excel/preview',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (result) {
                abp.ui.clearBusy();
                if (result.totalRows === 0) {
                    abp.message.warn('File Excel không có dữ liệu hợp lệ.', 'Preview');
                    importFile = null;
                    return;
                }

                // Build preview table
                var html = '<table class="table table-sm table-bordered" style="font-size:12px;"><thead><tr><th>Ngày</th><th>Ca</th><th>Giờ BĐ</th><th>Giờ KT</th><th>Trạng thái</th><th>Ghi chú</th></tr></thead><tbody>';
                result.items.forEach(function (item) {
                    var date = item.workDate ? luxon.DateTime.fromISO(item.workDate).toFormat('dd/MM/yyyy') : '—';
                    html += '<tr><td>' + date + '</td><td>' + (item.shiftCodeText || '') + '</td><td>' + (item.startTime || '') + '</td><td>' + (item.endTime || '') + '</td><td>' + (item.slotStatusText || '') + '</td><td>' + (item.note || '') + '</td></tr>';
                });
                html += '</tbody></table>';

                $('#importPreviewContent').html(html);
                $('#importPreviewCount').text(result.totalRows + ' bản ghi');
                new bootstrap.Modal(document.getElementById('importPreviewModal')).show();
            },
            error: function (xhr) {
                abp.ui.clearBusy();
                importFile = null;
                var errMsg = 'Đọc file thất bại.';
                if (xhr.responseJSON && xhr.responseJSON.error && xhr.responseJSON.error.message) {
                    errMsg = xhr.responseJSON.error.message;
                }
                abp.message.error(errMsg, 'Lỗi Preview');
            }
        });

        // Reset file input value (allows re-selecting same file)
        $(this).val('');
    });

    // Confirm import after preview
    $('#btnConfirmImport').click(function () {
        if (!importFile) { abp.notify.error('Không tìm thấy file. Vui lòng chọn lại.'); return; }

        var formData = new FormData();
        formData.append('file', importFile);

        bootstrap.Modal.getInstance(document.getElementById('importPreviewModal')).hide();
        abp.ui.setBusy();
        $.ajax({
            url: '/api/app/caddie-schedule-excel/confirm-import',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (result) {
                abp.ui.clearBusy();
                importFile = null;
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
                importFile = null;
                var errMsg = 'Import thất bại.';
                if (xhr.responseJSON && xhr.responseJSON.error && xhr.responseJSON.error.message) {
                    errMsg = xhr.responseJSON.error.message;
                }
                abp.message.error(errMsg, 'Lỗi Import');
            }
        });
    });

    // ── Schedule Template ─────────────────────────────────────────────
    var weekStart = $('#WeekStart').val();
    var applyWeekPicker = flatpickr('#applyTargetWeek', { dateFormat: 'd/m/Y' });

    function loadCaddieSelect($select) {
        $select.find('option:not(:first)').remove();
        if (window.__caddieItems) {
            window.__caddieItems.forEach(function (c) {
                $select.append('<option value="' + c.value + '">' + c.text + '</option>');
            });
        }
    }

    // Save Template
    $('#btnSaveTemplate').click(function () {
        loadCaddieSelect($('#templateCaddieId'));
        var weekLabel = luxon.DateTime.fromISO(weekStart).toFormat('dd/MM') + ' — ' + luxon.DateTime.fromISO(weekStart).plus({ days: 6 }).toFormat('dd/MM/yyyy');
        $('#saveTemplateWeekLabel').text(weekLabel);
        new bootstrap.Modal(document.getElementById('saveTemplateModal')).show();
    });

    $('#btnConfirmSaveTemplate').click(function () {
        var caddieId = $('#templateCaddieId').val();
        var templateName = $('#templateName').val();

        if (!caddieId) { abp.notify.error('Vui lòng chọn Caddie'); return; }

        abp.ui.setBusy();
        $.ajax({
            url: '/api/app/caddie-schedule-excel/save-template',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ caddieId: caddieId, weekStart: weekStart, templateName: templateName }),
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (result) {
                abp.ui.clearBusy();
                bootstrap.Modal.getInstance(document.getElementById('saveTemplateModal')).hide();
                abp.notify.success('Đã lưu template (' + result.length + ' khung giờ)');
            },
            error: function (xhr) {
                abp.ui.clearBusy();
                var errMsg = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error.message : 'Lưu template thất bại';
                abp.message.error(errMsg, 'Lỗi');
            }
        });
    });

    // Apply Template
    $('#btnApplyTemplate').click(function () {
        loadCaddieSelect($('#applyCaddieId'));
        new bootstrap.Modal(document.getElementById('applyTemplateModal')).show();
    });

    $('#btnConfirmApplyTemplate').click(function () {
        var caddieId = $('#applyCaddieId').val();
        var targetWeekStr = $('#applyTargetWeek').val();

        if (!caddieId) { abp.notify.error('Vui lòng chọn Caddie'); return; }
        if (!targetWeekStr) { abp.notify.error('Vui lòng chọn ngày bắt đầu tuần'); return; }

        // Parse dd/mm/yyyy to ISO
        var parts = targetWeekStr.split('/');
        var targetWeekStart = parts[2] + '-' + parts[1] + '-' + parts[0];

        abp.ui.setBusy();
        $.ajax({
            url: '/api/app/caddie-schedule-excel/apply-template',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ caddieId: caddieId, targetWeekStart: targetWeekStart }),
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (result) {
                abp.ui.clearBusy();
                bootstrap.Modal.getInstance(document.getElementById('applyTemplateModal')).hide();
                var msg = 'Đã tạo ' + result.generatedCount + ' khung giờ.';
                if (result.skippedCount > 0) msg += ' Bỏ qua ' + result.skippedCount + ' khung giờ đã tồn tại.';
                abp.notify.success(msg);
                setTimeout(function () { window.location.reload(); }, 1500);
            },
            error: function (xhr) {
                abp.ui.clearBusy();
                var errMsg = xhr.responseJSON && xhr.responseJSON.error ? xhr.responseJSON.error.message : 'Áp dụng template thất bại';
                abp.message.error(errMsg, 'Lỗi');
            }
        });
    });
});
