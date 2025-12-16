$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appCalendarSlots.appCalendarSlot;

    var detailModal = new abp.ModalManager('/AppCalendarSlots/DetailModal');

    function initCalendarDatePicker() {
        if (!window.flatpickr) {
            return;
        }

        flatpickr(".calendar-date-input", {
            dateFormat: "d/m/Y",
            allowInput: true,
            defaultDate: new Date()
        });
    }

    initCalendarDatePicker();

    function getSelectedGolfCourseId() {
        return $('#SelectedGolfCourseId').val();
    }

    function getSelectedApplyDate() {
        var text = $('#CalendarApplyDate').val();
        if (!text) {
            return null;
        }
        var parts = text.split('/');
        if (parts.length !== 3) {
            return null;
        }
        // dd/MM/yyyy -> yyyy-MM-dd
        return parts[2] + '-' + parts[1] + '-' + parts[0];
    }

    var slotsTable = $('#CalendarSlotsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: false, // load all cho 1 ngày
            paging: false,
            searching: false,
            info: false,
            autoWidth: false,
            ordering: false,
            data: [],
            columnDefs: [
                { title: l('TimeFrom'), data: "timeFrom" },
                { title: l('TimeTo'), data: "timeTo" },
                { title: l('PromotionType'), data: "promotionType" },
                { title: l('MaxSlots'), data: "maxSlots" },
                { title: l('InternalNote'), data: "internalNote" },
                {
                    title: l('Actions'),
                    data: null,
                    render: function (data, type, row) {
                        return '<button type="button" class="btn btn-sm btn-primary js-edit-slot" data-id="' + row.id + '">' +
                            l('Edit') + '</button>';
                    }
                }
            ]
        })
    );

    function loadSlotsForDay() {
        var golfCourseId = getSelectedGolfCourseId();
        var applyDate = getSelectedApplyDate();

        if (!golfCourseId || !applyDate) {
            slotsTable.clear().draw();
            $('#SelectedDateText').text('');
            return;
        }

        $('#SelectedDateText').text($('#CalendarApplyDate').val());

        service.getByDate({
            golfCourseId: golfCourseId,
            applyDate: applyDate
        }).then(function (result) {
            slotsTable.clear();
            slotsTable.rows.add(result);
            slotsTable.draw();
        });
    }

    $('#FilterSlotsButton').click(function (e) {
        e.preventDefault();
        loadSlotsForDay();
    });

    // Thêm mới: mở detail modal với Id = null, truyền golfCourseId & applyDate
    $('#AddSlotButton').click(function (e) {
        e.preventDefault();

        var golfCourseId = getSelectedGolfCourseId();
        var applyDate = getSelectedApplyDate(); // yyyy-MM-dd

        if (!golfCourseId || !applyDate) {
            abp.notify.warn(l('PleaseSelectGolfCourse'));
            return;
        }

        detailModal.open({
            golfCourseId: golfCourseId,
            applyDate: applyDate
        });
    });

    // Edit: click button trong table
    $('#CalendarSlotsTable').on('click', '.js-edit-slot', function () {
        var id = String($(this).data('id') || '');
        if (!id) return;
        var golfCourseId = getSelectedGolfCourseId();
        var applyDate = getSelectedApplyDate();

        detailModal.open({
            id: id,
            golfCourseId: golfCourseId,
            applyDate: applyDate
        });
    });

    $('#OpenCalendarButton').click(function (e) {
        e.preventDefault();
        var golfCourseId = $('#SelectedGolfCourseId').val();
        if (!golfCourseId) {
            abp.notify.warn(l('PleaseSelectGolfCourse'));
            return;
        }
        var url = '/AppCalendarSlots/Calendar?golfCourseId=' + encodeURIComponent(golfCourseId);
        window.open(url, '_blank');
    });

    detailModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        loadSlotsForDay();
    });
});
