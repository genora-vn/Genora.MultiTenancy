$(function () {
    console.log('Index.js loaded at', new Date().toISOString());

    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appCalendarSlots.appCalendarSlot;
    var detailModal = new abp.ModalManager('/AppCalendarSlots/DetailModal');

    // =========================
    // Date picker
    // =========================
    function initCalendarDatePicker() {
        if (!window.flatpickr) return;

        flatpickr(".calendar-date-input", {
            dateFormat: "d/m/Y",
            allowInput: true,
            defaultDate: new Date()
        });
    }
    initCalendarDatePicker();

    // =========================
    // Filters
    // =========================
    function getSelectedGolfCourseId() {
        return $('#SelectedGolfCourseId').val();
    }

    function parseDdMmYyyyToIso(text) {
        if (!text) return null;
        var parts = String(text).split('/');
        if (parts.length !== 3) return null;
        return parts[2] + '-' + parts[1] + '-' + parts[0];
    }

    function getSelectedFromDate() {
        return parseDdMmYyyyToIso($('#CalendarFromDate').val());
    }

    function getSelectedToDate() {
        return parseDdMmYyyyToIso($('#CalendarToDate').val());
    }

    function getFilter() {
        return {
            golfCourseId: getSelectedGolfCourseId(),
            applyDateFrom: getSelectedFromDate(),
            applyDateTo: getSelectedToDate()
        };
    }

    // =========================
    // Selection
    // =========================
    var selectedIds = new Set();
    var $wrapper = null;

    function refreshBulkButtons() {
        $('#BulkStatusDropdownBtn').prop('disabled', selectedIds.size === 0);
    }

    function $headerSelectAll() {
        if (!$wrapper || !$wrapper.length) return $();
        // Bắt cả checkbox header do DataTables scroll clone (cột 0)
        return $wrapper.find('th[data-dt-column="0"] input[type="checkbox"]');
    }

    function syncHeaderSelectAll() {
        if (!$wrapper || !$wrapper.length) return;

        var allRows = slotsTable.rows().data().toArray(); // all pages
        var $heads = $headerSelectAll();

        if (!allRows.length) {
            $heads.prop('checked', false).prop('indeterminate', false);
            return;
        }

        var total = allRows.length;
        var selectedCount = 0;
        for (var i = 0; i < total; i++) {
            if (selectedIds.has(String(allRows[i].id))) selectedCount++;
        }

        $heads
            .prop('checked', selectedCount === total)
            .prop('indeterminate', selectedCount > 0 && selectedCount < total);
    }

    function syncRowCheckboxesCurrentPage() {
        if (!$wrapper || !$wrapper.length) return;

        // checkbox row nằm trong dt-scroll-body
        $wrapper.find('.dt-scroll-body .js-row-select').each(function () {
            var id = String($(this).data('id') || '');
            $(this).prop('checked', selectedIds.has(id));
        });
    }

    // =========================
    // DataTable init
    // =========================
    var slotsTable = $('#CalendarSlotsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: false,
            paging: true,
            searching: false,
            info: false,
            autoWidth: false,
            ordering: false,
            data: [],
            columnDefs: [
                {
                    title: '<input type="checkbox" id="SelectAllSlots" class="form-check-input" />',
                    data: null,
                    width: "36px",
                    orderable: false,
                    searchable: false,
                    render: function (data, type, row) {
                        var id = String(row.id || '');
                        var checked = selectedIds.has(id) ? 'checked' : '';
                        return '<input type="checkbox" class="form-check-input js-row-select" data-id="' + id + '" ' + checked + ' />';
                    }
                },
                {
                    title: l('PlayDate'),
                    data: "applyDate",
                    render: function (data) {
                        if (!data) return '';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                    }
                },
                { title: l('TimeFrom'), data: "timeFrom" },
                { title: l('TimeTo'), data: "timeTo" },
                { title: l('PromotionType'), data: "promotionType" },
                { title: l('MaxSlots'), data: "maxSlots" },
                { title: l('InternalNote'), data: "internalNote" },
                {
                    title: l('Status'),
                    data: "isActive",
                    render: function (v) {
                        return v
                            ? '<span class="badge bg-success">Active</span>'
                            : '<span class="badge bg-secondary">Inactive</span>';
                    }
                },
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

    // ✅ DataTables v2 wrapper đúng như inspect: #CalendarSlotsTable_wrapper
    $wrapper = $('#CalendarSlotsTable_wrapper');
    if (!$wrapper.length) $wrapper = $('#CalendarSlotsTable').closest('.dt-container');

    // =========================
    // Select All (ALL PAGES theo FILTER)
    // =========================
    var selectAllSelector = 'th[data-dt-column="0"] input[type="checkbox"]';

    // Dùng CHANGE (ổn định), lấy thẳng this.checked (KHÔNG đảo !)
    $wrapper
        .off('change.selectAll', selectAllSelector)
        .on('change.selectAll', selectAllSelector, function (e) {
            e.stopPropagation();

            var willCheck = this.checked; // ✅ trạng thái mới sau toggle
            console.log('[SelectAll] change, willCheck=', willCheck, 'rows=', slotsTable.rows().data().length);

            // đồng bộ cả 2 header checkbox
            $headerSelectAll().prop('checked', willCheck).prop('indeterminate', false);

            // Select all theo toàn bộ data (all pages)
            var allRows = slotsTable.rows().data().toArray();
            for (var i = 0; i < allRows.length; i++) {
                var id = String(allRows[i].id);
                if (willCheck) selectedIds.add(id);
                else selectedIds.delete(id);
            }

            // update UI page hiện tại
            syncRowCheckboxesCurrentPage();
            refreshBulkButtons();
            syncHeaderSelectAll();
        });

    // draw: sync UI theo selectedIds
    slotsTable.on('draw', function () {
        syncRowCheckboxesCurrentPage();
        refreshBulkButtons();
        syncHeaderSelectAll();
    });

    // Row checkbox
    $wrapper.on('change', '.js-row-select', function (e) {
        e.stopPropagation();
        var id = String($(this).data('id') || '');
        if (!id) return;

        if (this.checked) selectedIds.add(id);
        else selectedIds.delete(id);

        refreshBulkButtons();
        syncHeaderSelectAll();
    });

    // =========================
    // Load data
    // =========================
    function loadSlotsForDay() {
        var golfCourseId = getSelectedGolfCourseId();
        var fromDate = getSelectedFromDate();
        var toDate = getSelectedToDate();

        if (!golfCourseId || !fromDate || !toDate) {
            slotsTable.clear().draw();
            selectedIds.clear();
            refreshBulkButtons();
            syncHeaderSelectAll();
            return;
        }

        service.getByDate({
            golfCourseId: golfCourseId,
            applyDateFrom: fromDate,
            applyDateTo: toDate
        }).then(function (result) {
            result = result || [];

            slotsTable.clear();
            slotsTable.rows.add(result);
            slotsTable.draw();

            // remove selectedIds không còn tồn tại
            var idsInTable = new Set(result.map(function (x) { return String(x.id); }));
            selectedIds.forEach(function (id) {
                if (!idsInTable.has(id)) selectedIds.delete(id);
            });

            refreshBulkButtons();
            syncHeaderSelectAll();
        });
    }

    $('#FilterSlotsButton').click(function (e) {
        e.preventDefault();
        loadSlotsForDay();
    });

    // Add
    $('#AddSlotButton').click(function (e) {
        e.preventDefault();

        var golfCourseId = getSelectedGolfCourseId();
        var fromDate = getSelectedFromDate();
        var toDate = getSelectedToDate();

        if (!golfCourseId || !fromDate || !toDate) {
            abp.notify.warn(l('PleaseSelectGolfCourse'));
            return;
        }

        detailModal.open({
            golfCourseId: golfCourseId,
            applyDate: fromDate
        });
    });

    // Edit
    $('#CalendarSlotsTable').on('click', '.js-edit-slot', function () {
        var id = String($(this).data('id') || '');
        if (!id) return;

        var golfCourseId = getSelectedGolfCourseId();
        var fromDate = getSelectedFromDate();
        var toDate = getSelectedToDate();

        detailModal.open({
            id: id,
            golfCourseId: golfCourseId,
            applyDateFrom: fromDate,
            applyDateTo: toDate
        });
    });

    // Bulk update status (giữ nguyên)
    $(document).on('click', '.js-bulk-status', function () {
        var isActive = String($(this).data('active')) === 'true';

        var ids = Array.from(selectedIds);
        if (!ids.length) {
            abp.notify.warn('Vui lòng chọn ít nhất 1 dòng.');
            return;
        }

        abp.message.confirm(
            'Bạn có chắc muốn cập nhật trạng thái cho ' + ids.length + ' dòng?',
            'Xác nhận',
            function (confirmed) {
                if (!confirmed) return;

                service.updateStatusBulk({
                    ids: ids,
                    isActive: isActive
                }).then(function (updatedCount) {
                    abp.notify.success('Đã cập nhật ' + updatedCount + ' dòng.');
                    selectedIds.clear();
                    refreshBulkButtons();
                    loadSlotsForDay();
                });
            }
        );
    });

    // Open calendar
    $('#OpenCalendarButton').click(function (e) {
        e.preventDefault();

        var golfCourseId = getSelectedGolfCourseId();
        if (!golfCourseId) {
            abp.notify.warn(l('PleaseSelectGolfCourse'));
            return;
        }

        var url = '/AppCalendarSlots/Calendar?golfCourseId=' + encodeURIComponent(golfCourseId);
        window.open(url, '_blank');
    });

    // Download template
    $('#DownloadTemplateBtn').click(function () {
        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        var golfCourseId = getSelectedGolfCourseId();
        if (!golfCourseId) {
            abp.notify.warn(l('PleaseSelectGolfCourse'));
            return;
        }

        genora.excel.download(
            'api/app/app-calendar-excel/template',
            { golfCourseId: golfCourseId }
        );
    });

    // Import excel
    $('#ImportExcelInput').change(function (e) {
        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.upload({
            url: 'api/app/app-calendar-excel/import',
            fileInput: e.target,
            onSuccess: function () {
                loadSlotsForDay();
            }
        });
    });

    // Export excel
    $('#ExportExcelBtn').click(function () {
        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }

        genora.excel.download(
            'api/app/app-calendar-excel/export',
            getFilter()
        );
    });

    // Modal result
    detailModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        loadSlotsForDay();
    });
});
