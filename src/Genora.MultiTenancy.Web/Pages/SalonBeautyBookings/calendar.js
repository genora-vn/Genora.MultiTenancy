$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var bookingService = genora.multiTenancy.appServices.salonBeauties.salonBeautyBooking;
    var calendarEl = document.getElementById('SalonCalendar');
    var selectedStylists = [];
    var selectedServiceId = '';

    if (!calendarEl) {
        return;
    }

    function getStatusColor(status) {
        if (!status) return '#9CA3AF';
        var s = (status + '').toLowerCase();
        if (s === '0' || s === 'new') return '#F59E0B';
        if (s === '1' || s === 'confirmed') return '#2563EB';
        if (s === '2' || s === 'completed') return '#10B981';
        if (s === '3' || s === 'cancelled') return '#EF4444';
        return '#9CA3AF';
    }

    function toIsoDateOnly(date) {
        var year = date.getFullYear();
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var day = String(date.getDate()).padStart(2, '0');
        return year + '-' + month + '-' + day;
    }

    function loadFilters(calendar) {
        return $.when(
            bookingService.getStylistLookup().then(function (data) {
                var $list = $('#StylistCheckboxList');
                $list.empty();
                selectedStylists = [];
                (data || []).forEach(function (s) {
                    $list.append('<li><label><input type="checkbox" class="stylist-filter-cb" value="' + s.id + '" checked /> <span>' + s.displayName + '</span></label></li>');
                    selectedStylists.push(s.id);
                });
            }),
            bookingService.getServiceLookup().then(function (data) {
                var $sel = $('#ServiceFilterSelect');
                $sel.find('option:not(:first)').remove();
                (data || []).forEach(function (s) {
                    $sel.append('<option value="' + s.id + '">' + s.name + '</option>');
                });
            })
        ).then(function () {
            if (calendar) calendar.refetchEvents();
        });
    }

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'timeGridWeek',
        locale: 'vi',
        height: '100%',
        firstDay: 1,
        slotMinTime: '07:00:00',
        slotMaxTime: '22:00:00',
        allDaySlot: false,
        nowIndicator: true,
        expandRows: true,
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        buttonText: {
            today: 'Hôm nay',
            month: 'Tháng',
            week: 'Tuần',
            day: 'Ngày'
        },
        events: function (fetchInfo, successCallback, failureCallback) {
            var from = toIsoDateOnly(fetchInfo.start);
            var to = toIsoDateOnly(fetchInfo.end);
            bookingService.getCalendarEvents(from, to, null, selectedServiceId || null)
                .then(function (data) {
                    var filtered = data || [];
                    if (selectedStylists.length > 0) {
                        filtered = filtered.filter(function (e) { return selectedStylists.indexOf(e.stylistId) >= 0; });
                    }
                    successCallback(filtered.map(function (e) {
                        var color = e.statusColor || getStatusColor(e.status);
                        return {
                            id: e.id,
                            title: (e.customerName || '') + ' - ' + (e.serviceName || ''),
                            start: e.start,
                            end: e.end,
                            backgroundColor: color,
                            borderColor: color,
                            extendedProps: e
                        };
                    }));
                })
                .catch(function (error) {
                    console.error(error);
                    failureCallback(error);
                });
        },
        eventClick: function (info) {
            window.open('/SalonBeautyBookings/Detail?id=' + info.event.id, '_blank');
        },
        eventContent: function (arg) {
            var e = arg.event.extendedProps || {};
            var timeStr = arg.event.start ? arg.event.start.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) : '';
            return {
                html: '<div class="salon-calendar-event">' +
                    '<div class="salon-calendar-event-time">' + timeStr + ' · ' + (e.customerName || '') + '</div>' +
                    '<div class="salon-calendar-event-service">' + (e.serviceName || '') + '</div>' +
                    '<div class="salon-calendar-event-stylist">' + (e.stylistName || '') + '</div>' +
                    '</div>'
            };
        }
    });

    calendar.render();
    loadFilters(calendar);

    $(document).on('change', '.stylist-filter-cb', function () {
        selectedStylists = [];
        $('.stylist-filter-cb:checked').each(function () { selectedStylists.push($(this).val()); });
        $('#SelectAllStylists').prop('checked', $('.stylist-filter-cb').length === $('.stylist-filter-cb:checked').length);
        calendar.refetchEvents();
    });

    $('#ServiceFilterSelect').on('change', function () {
        selectedServiceId = $(this).val();
        calendar.refetchEvents();
    });

    $('#SelectAllStylists').on('change', function () {
        var checked = $(this).is(':checked');
        $('.stylist-filter-cb').prop('checked', checked);
        selectedStylists = [];
        if (checked) {
            $('.stylist-filter-cb').each(function () { selectedStylists.push($(this).val()); });
        }
        calendar.refetchEvents();
    });

    $('#CalendarRefreshButton').on('click', function () {
        calendar.refetchEvents();
    });
});
