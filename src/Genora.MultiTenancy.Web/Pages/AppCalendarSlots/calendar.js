$(function () {
    console.log("calendar.js loaded");

    var l = abp.localization.getResource('MultiTenancy');

    var slotService = genora.multiTenancy.appServices.appCalendarSlots.appCalendarSlot;

    var viewModal = new abp.ModalManager('/AppCalendarSlots/ViewModal');
    var editModal = new abp.ModalManager('/AppCalendarSlots/DetailModal');

    var golfCourseId = $('#GolfCourseId').val();
    var calendarEl = document.getElementById('Calendar');

    if (!calendarEl) {
        console.error("#Calendar element not found");
        return;
    }

    var lastViewedSlotId = null;

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        locale: 'vi',
        height: '100%',
        selectable: true,
        selectMirror: true,
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },

        // 1) CLICK ngày ở month view (không có giờ) -> default 06:00-06:30
        dateClick: function (info) {
            //debugger;
            lastViewedSlotId = null;

            function pad(n) { return n < 10 ? "0" + n : "" + n; }
            function toHHmm(d) { return pad(d.getHours()) + ":" + pad(d.getMinutes()); }

            // month view: chỉ có ngày -> default 06:00-06:30
            var applyDate = info.dateStr.substring(0, 10); // yyyy-MM-dd
            var timeFrom = "06:00";
            var timeTo = "06:30";

            // timeGridWeek/timeGridDay: info.date có giờ/phút
            // (FullCalendar sẽ gọi dateClick cho cả timeGrid)
            if (info.view && (info.view.type === "timeGridWeek" || info.view.type === "timeGridDay")) {
                var start = info.date; // Date local
                timeFrom = toHHmm(start);

                var end30 = new Date(start.getTime() + 30 * 60 * 1000);
                timeTo = toHHmm(end30);
            }

            editModal.open({
                golfCourseId: golfCourseId,
                applyDate: applyDate,
                timeFrom: timeFrom,
                timeTo: timeTo
            });
        },

        // 2) SELECT slot thời gian ở week/day -> có start/end
        select: function (info) {
            // info.start / info.end là Date
            // Nếu user chỉ click 1 điểm (không kéo), FullCalendar vẫn trả start/end theo slot nhỏ nhất
            var start = info.start;
            var end = info.end;
            //debugger;
            // ApplyDate lấy theo start (yyyy-MM-dd)
            var applyDate = start.toISOString().substring(0, 10);

            // timeFrom HH:mm
            function pad(n) { return n < 10 ? "0" + n : "" + n; }
            function toTimeText(d) { return pad(d.getHours()) + ":" + pad(d.getMinutes()); }

            var timeFrom = toTimeText(start);

            // TimeTo = start + 30 phút (bỏ qua end của selection)
            var end30 = new Date(start.getTime() + 30 * 60 * 1000);
            var timeTo = toTimeText(end30);

            lastViewedSlotId = null;

            editModal.open({
                golfCourseId: golfCourseId,
                applyDate: applyDate,
                timeFrom: timeFrom,
                timeTo: timeTo
            });

            calendar.unselect();
        },

        eventClick: function (info) {
            var slotId = info.event.id;
            lastViewedSlotId = slotId;
            viewModal.open({ id: slotId });
        },

        events: function (fetchInfo, successCallback, failureCallback) {
            slotService.getList({
                golfCourseId: golfCourseId,
                applyDateFrom: fetchInfo.startStr,
                applyDateTo: fetchInfo.endStr,
                maxResultCount: 1000,
                skipCount: 0
            }).then(function (result) {
                var events = result.items.map(function (slot) {
                    var datePart = slot.applyDate.substring(0, 10);
                    var start = datePart + 'T' + slot.timeFrom;
                    var end = datePart + 'T' + slot.timeTo;

                    return {
                        id: slot.id,
                        title: slot.timeFrom.substring(0, 5) + ' - ' + slot.timeTo.substring(0, 5),
                        start: start,
                        end: end,
                        allDay: false
                    };
                });

                successCallback(events);
            }).catch(function (error) {
                console.error(error);
                failureCallback(error);
            });
        }
    });

    calendar.render();

    // Sau khi tạo/sửa thì reload calendar
    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        calendar.refetchEvents();

        // Nếu trước đó mở từ View → edit, thì sau khi lưu hiện lại view
        if (lastViewedSlotId) {
            viewModal.open({ id: lastViewedSlotId });
        }
    });

    // Gắn hành vi Edit/Delete khi ViewModal open
    viewModal.onOpen(function () {
        var modal = viewModal.getModal();
        //debugger;
        // tránh gắn nhiều lần
        modal.off('click', '#CalendarSlotView_Edit');
        modal.off('click', '#CalendarSlotView_Delete');

        modal.on('click', '#CalendarSlotView_Edit', function (e) {
            e.preventDefault();

            var id = modal.find('#CalendarSlotView_Id').val();
            var applyDate = modal.find('#CalendarSlotView_ApplyDate').val(); // yyyy-MM-dd

            lastViewedSlotId = id;

            // Ẩn view, mở edit
            viewModal.close();
            editModal.open({
                id: id,
                golfCourseId: golfCourseId,
                applyDate: applyDate
            });
        });

        modal.on('click', '#CalendarSlotView_Delete', function (e) {
            e.preventDefault();

            var id = modal.find('#CalendarSlotView_Id').val();
            var timeRange = modal.find('#CalendarSlotView_TimeRange').val() || '';

            abp.message.confirm(
                l('AreYouSureToDelete', timeRange),
                l('AreYouSure')
            ).then(function (confirmed) {
                if (!confirmed) {
                    return;
                }

                slotService.delete(id).then(function () {
                    abp.notify.success(l('DeletedSuccessfully'));
                    viewModal.close();
                    calendar.refetchEvents();
                });
            });
        });
    });
});
