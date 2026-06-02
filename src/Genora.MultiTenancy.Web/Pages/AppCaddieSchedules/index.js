$(function () {
    var scheduleService = genora.multiTenancy.appServices.caddies.caddieSchedule;
    var createModal = new abp.ModalManager(abp.appPath + 'AppCaddieSchedules/CreateModal');
    var canEdit = $('#CanEdit').val() === 'true';

    // Click on schedule card → show detail modal
    $(document).on('click', '.caddie-schedule-card', function () {
        var $card = $(this);
        var id = $card.data('id');

        // Find schedule data from rendered cards
        var name = $card.find('.caddie-schedule-card-name').text();
        var code = $card.find('.caddie-schedule-card-code').text();
        var timeText = $card.find('.caddie-schedule-card-info').first().text().trim();
        var noteText = $card.find('.caddie-schedule-card-info').last().text().trim();

        var statusText = 'Trống lịch';
        if ($card.hasClass('booked')) statusText = 'Đang phục vụ';
        if ($card.hasClass('off')) statusText = 'Nghỉ';

        var shiftText = 'Sáng';
        // Determine shift from time
        if (timeText.indexOf('12:') >= 0 || timeText.indexOf('13:') >= 0 || timeText.indexOf('14:') >= 0) shiftText = 'Chiều';
        if (timeText.indexOf('18:') >= 0 || timeText.indexOf('19:') >= 0 || timeText.indexOf('20:') >= 0) shiftText = 'Tối';

        $('#modalCaddieName').text(name);
        $('#modalCaddieCode').text(code);
        $('#modalTime').text(timeText || '—');
        $('#modalStatus').text(statusText);
        $('#modalShift').text(shiftText);
        $('#modalNote').text(noteText !== timeText ? noteText : '—');
        $('#btnEditSchedule').data('id', id);

        var modal = new bootstrap.Modal(document.getElementById('scheduleDetailModal'));
        modal.show();
    });

    // Edit schedule from detail modal
    $('#btnEditSchedule').click(function () {
        var id = $(this).data('id');
        bootstrap.Modal.getInstance(document.getElementById('scheduleDetailModal')).hide();
        createModal.open({ id: id });
    });

    // FAB: New Schedule
    $('#NewScheduleButton').click(function () {
        createModal.open();
    });

    createModal.onResult(function () {
        window.location.reload();
    });
});
