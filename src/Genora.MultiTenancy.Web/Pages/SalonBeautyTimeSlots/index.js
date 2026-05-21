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

    var createModal = new abp.ModalManager('/SalonBeautyTimeSlots/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyTimeSlots/EditModal');
    var canEdit = $('#CanEditTimeSlot').val() === 'true';
    var canDelete = $('#CanDeleteTimeSlot').val() === 'true';

    function pad2(n) { return String(n).padStart(2, '0'); }
    function htmlEncode(value) { return $('<div/>').text(value || '').html(); }
    function avatarUrl(row) { return row.stylistAvatar || '/images/getting-started/no-photo-square.png'; }

    function formatTime(t) {
        if (!t) return '';
        if (typeof t === 'string') {
            var parts = t.split(':');
            return parts.length >= 2 ? pad2(parts[0]) + ':' + pad2(parts[1]) : t;
        }
        return t;
    }

    function formatDate(d) {
        if (!d) return '';
        var date = new Date(d);
        if (isNaN(date.getTime())) return '';
        return pad2(date.getDate()) + '/' + pad2(date.getMonth() + 1) + '/' + date.getFullYear();
    }

    function toIsoDate(value) {
        if (!value) return null;
        var s = (value || '').trim();
        if (/^\d{4}-\d{2}-\d{2}$/.test(s)) return s;
        var m = s.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
        if (m) return m[3] + '-' + pad2(m[2]) + '-' + pad2(m[1]);
        return null;
    }

    function buildListInput(request) {
        request = request || {};
        var length = request.length || 10;
        return {
            filterText: ($('#TimeSlotKeywordFilter').val() || '').trim() || null,
            locationId: $('#TimeSlotLocationFilter').val() || null,
            fromDate: toIsoDate($('#TimeSlotFromDateFilter').val()),
            toDate: toIsoDate($('#TimeSlotToDateFilter').val()),
            skipCount: request.start || 0,
            maxResultCount: length > 100 ? 100 : length,
            sorting: 'stylistName asc'
        };
    }

    function loadLocationFilter() {
        var $select = $('#TimeSlotLocationFilter');
        return locationService.getLookup().then(function (items) {
            $select.find('option:not(:first)').remove();
            (items || []).forEach(function (it) {
                $select.append('<option value="' + it.id + '">' + htmlEncode(it.name) + '</option>');
            });
        });
    }

    /* Filter date pickers (flatpickr) */
    if (window.flatpickr) {
        flatpickr('#TimeSlotFromDateFilter', { dateFormat: 'd/m/Y', allowInput: true });
        flatpickr('#TimeSlotToDateFilter', { dateFormat: 'd/m/Y', allowInput: true });
    }

    loadLocationFilter();

    $('#TimeSlotLocationFilter,#TimeSlotFromDateFilter,#TimeSlotToDateFilter').on('change', function () {
        dataTable.ajax.reload(null, true);
    });

    function renderStylist(data, type, row) {
        return '<div class="timeslot-stylist-cell">'
            + '<img src="' + htmlEncode(avatarUrl(row)) + '" alt="" />'
            + '<div class="timeslot-stylist-text"><strong>' + htmlEncode(row.stylistName || '') + '</strong>'
            + '<span>' + (row.slotCount || 0) + ' ' + l('SalonBeautyTimeSlots:SlotsUnit') + '</span></div>'
            + '</div>';
    }

    function renderLocation(data, type, row) {
        return htmlEncode(row.locationName || '--');
    }

    function renderDateRange(data, type, row) {
        var f = formatDate(row.fromDate);
        var t = formatDate(row.toDate);
        if (!f && !t) return '--';
        return htmlEncode(f + ' → ' + t);
    }

    function renderTimeRange(data, type, row) {
        var f = formatTime(row.fromTime);
        var t = formatTime(row.toTime);
        if (!f && !t) return '--';
        return '<span class="timeslot-range-chip"><i class="fa fa-clock-o"></i>' + htmlEncode(f + ' - ' + t) + '</span>';
    }

    function renderActive(data, type, row) {
        var cls = row.isActive ? 'on' : 'off';
        var text = row.isActive ? l('Enum:SalonBeautyTimeSlotStatus.On') : l('Enum:SalonBeautyTimeSlotStatus.Off');
        return '<span class="timeslot-status-badge ' + cls + '">' + htmlEncode(text) + '</span>';
    }

    function renderShowOnApp(data, type, row) {
        return '<span class="timeslot-status-badge ' + (row.isShowOnApp ? 'on' : 'off') + '">'
            + htmlEncode(row.isShowOnApp ? l('Yes') : l('No')) + '</span>';
    }

    var dataTable = $('#TimeSlotsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [],
            ajax: abp.libs.datatables.createAjax(slotService.getList, buildListInput),
            pageLength: 10,
            lengthMenu: [10, 25, 50, 100],
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('SalonBeautyTimeSlots:OpenCalendar'),
                                action: function (data) {
                                    window.location.href = '/SalonBeautyTimeSlots/Calendar?stylistId=' + data.record.stylistId;
                                }
                            },
                            {
                                text: l('Edit'),
                                visible: function () { return canEdit; },
                                action: function (data) { editModal.open({ stylistId: data.record.stylistId }); }
                            },
                            {
                                text: l('Delete'),
                                visible: function () { return canDelete; },
                                confirmMessage: function (data) {
                                    return abp.utils.formatString(l('SalonBeautyTimeSlots:DeleteConfirm'), data.record.stylistName);
                                },
                                action: function (data) {
                                    slotService.deleteByStylist(data.record.stylistId).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            }
                        ]
                    },
                    width: '120px'
                },
                { title: l('SalonBeautyTimeSlots:Location'), data: 'locationName', render: renderLocation, width: '180px' },
                { title: l('SalonBeautyTimeSlots:Stylist'), data: 'stylistName', render: renderStylist, width: '260px' },
                { title: l('SalonBeautyTimeSlots:WorkDateRange'), data: 'fromDate', render: renderDateRange, orderable: false, width: '200px' },
                { title: l('SalonBeautyTimeSlots:WorkTimeRange'), data: 'fromTime', render: renderTimeRange, orderable: false, width: '170px' },
                { title: l('SalonBeautyTimeSlots:Status'), data: 'isActive', render: renderActive, orderable: false, width: '120px' },
                { title: l('SalonBeautyTimeSlots:IsShowOnApp'), data: 'isShowOnApp', render: renderShowOnApp, orderable: false, width: '120px' }
            ]
        })
    );

    // Expose reload function cho inline script trong modal cshtml gọi sau khi save
    window.salonTimeSlotReload = function () {
        dataTable.ajax.reload(null, false);
    };

    $('#SearchTimeSlotButton').on('click', function (e) {
        e.preventDefault();
        dataTable.ajax.reload(null, true);
    });

    $('#TimeSlotKeywordFilter').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload(null, true);
        }
    });

    $('#NewTimeSlotButton').on('click', function (e) {
        e.preventDefault();
        createModal.open();
    });

    createModal.onResult(function () {
        dataTable.ajax.reload(null, false);
    });
    editModal.onResult(function () {
        dataTable.ajax.reload(null, false);
    });
});
