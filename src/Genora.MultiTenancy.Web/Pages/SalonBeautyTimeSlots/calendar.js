$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    function resolveSalonService(name) {
        var root = genora.multiTenancy.appServices && genora.multiTenancy.appServices.salonBeauties;
        if (!root || !root[name]) {
            throw new Error('Salon Beauty application service proxy not found: genora.multiTenancy.appServices.salonBeauties.' + name);
        }
        return root[name];
    }

    var slotService = resolveSalonService('salonBeautyTimeSlot');
    var locationService = resolveSalonService('salonBeautyLocation');
    var stylistService = resolveSalonService('salonBeautyStylist');

    var canEdit = $('#CanEditTimeSlotCalendar').val() === 'true';
    var presetStylistId = $('#CalendarPresetStylistId').val() || '';
    var currentSlotId = null;
    var calendar = null;

    function htmlEncode(value) { return $('<div/>').text(value || '').html(); }

    function statusKey(status) {
        if (status === 0) return 'off';
        if (status === 2) return 'full';
        return 'on';
    }

    function statusText(status) {
        if (status === 0) return l('Enum:SalonBeautyTimeSlotStatus.Off');
        if (status === 2) return l('Enum:SalonBeautyTimeSlotStatus.Full');
        return l('Enum:SalonBeautyTimeSlotStatus.On');
    }

    function pad(n) { return String(n).padStart(2, '0'); }

    function buildEventTime(workDate, time) {
        var date = new Date(workDate);
        var parts = (time || '00:00:00').split(':');
        date.setHours(parseInt(parts[0], 10) || 0, parseInt(parts[1], 10) || 0, parseInt(parts[2], 10) || 0, 0);
        return date.getFullYear() + '-' + pad(date.getMonth() + 1) + '-' + pad(date.getDate())
            + 'T' + pad(date.getHours()) + ':' + pad(date.getMinutes()) + ':00';
    }

    function loadLocationFilter() {
        return locationService.getLookup().then(function (items) {
            var $sel = $('#CalendarLocationFilter');
            $sel.empty();
            $sel.append('<option value="">' + l('SalonBeautyTimeSlots:AllLocations') + '</option>');
            (items || []).forEach(function (it) {
                $sel.append('<option value="' + it.id + '">' + htmlEncode(it.name) + '</option>');
            });
        });
    }

    function loadStylistFilter() {
        return stylistService.getList({ skipCount: 0, maxResultCount: 200, sorting: 'displayName asc' }).then(function (res) {
            var $sel = $('#CalendarStylistFilter');
            $sel.empty();
            $sel.append('<option value="">' + l('SalonBeautyTimeSlots:AllStylists') + '</option>');
            (res.items || []).forEach(function (it) {
                $sel.append('<option value="' + it.id + '">' + htmlEncode(it.displayName) + '</option>');
            });
            if (presetStylistId) $sel.val(presetStylistId);
        });
    }

    function fetchEvents(info, success, failure) {
        var input = {
            fromDate: info.startStr,
            toDate: info.endStr,
            locationId: $('#CalendarLocationFilter').val() || null,
            stylistId: $('#CalendarStylistFilter').val() || null,
            status: $('#CalendarStatusFilter').val() === '' ? null : parseInt($('#CalendarStatusFilter').val(), 10)
        };

        slotService.getCalendarEvents(input).then(function (slots) {
            var events = (slots || []).map(function (s) {
                var key = statusKey(s.status);
                return {
                    id: s.id,
                    title: (s.stylistName || '') + ' • ' + statusText(s.status),
                    start: buildEventTime(s.workDate, s.startTime),
                    end: buildEventTime(s.workDate, s.endTime),
                    classNames: ['timeslot-event', key],
                    extendedProps: {
                        stylistName: s.stylistName,
                        locationName: s.locationName,
                        status: s.status
                    }
                };
            });
            success(events);
        }).catch(function (err) {
            if (failure) failure(err);
        });
    }

    function initCalendar() {
        var el = document.getElementById('TimeSlotCalendar');
        if (!el) return;
        calendar = new FullCalendar.Calendar(el, {
            initialView: 'timeGridWeek',
            locale: 'vi',
            firstDay: 1,
            allDaySlot: false,
            slotMinTime: '06:00:00',
            slotMaxTime: '23:00:00',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay'
            },
            events: fetchEvents,
            eventClick: function (info) {
                if (!canEdit) return;
                currentSlotId = info.event.id;
                var info1 = info.event.extendedProps;
                var startStr = info.event.start ? FullCalendar.formatDate(info.event.start, { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit' }) : '';
                $('#UpdateSlotStatusInfo').text(
                    (info1.stylistName || '') + ' — ' + (info1.locationName || '') + ' — ' + startStr
                );
                $('#UpdateSlotStatusModal').modal('show');
            }
        });
        calendar.render();
    }

    Promise.all([loadLocationFilter(), loadStylistFilter()]).then(function () {
        initCalendar();
    });

    $('#CalendarLocationFilter, #CalendarStylistFilter, #CalendarStatusFilter').on('change', function () {
        if (calendar) calendar.refetchEvents();
    });

    $('#CalendarRefreshButton').on('click', function () {
        if (calendar) calendar.refetchEvents();
    });

    $(document).on('click', '#UpdateSlotStatusModal [data-status]', function () {
        if (!currentSlotId) return;
        var status = parseInt($(this).data('status'), 10);
        slotService.updateStatus(currentSlotId, { status: status }).then(function () {
            abp.notify.success(l('SavedSuccessfully'));
            $('#UpdateSlotStatusModal').modal('hide');
            currentSlotId = null;
            if (calendar) calendar.refetchEvents();
        }).catch(function () {
            abp.notify.error(l('SalonBeautyTimeSlots:UpdateStatusFailed'));
        });
    });
});
