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

    var createModal = new abp.ModalManager('/SalonBeautyTimeSlots/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyTimeSlots/EditModal');
    var canEdit = $('#CanEditTimeSlot').val() === 'true';
    var canDelete = $('#CanDeleteTimeSlot').val() === 'true';

    function htmlEncode(value) { return $('<div/>').text(value || '').html(); }
    function avatarUrl(row) { return row.stylistAvatar || '/images/getting-started/no-photo-square.png'; }

    function formatTime(t) {
        if (!t) return '';
        if (typeof t === 'string') {
            var parts = t.split(':');
            return parts.length >= 2 ? parts[0].padStart(2, '0') + ':' + parts[1].padStart(2, '0') : t;
        }
        return t;
    }

    function formatDate(d) {
        if (!d) return '';
        var date = new Date(d);
        if (isNaN(date.getTime())) return '';
        var dd = String(date.getDate()).padStart(2, '0');
        var mm = String(date.getMonth() + 1).padStart(2, '0');
        var yyyy = date.getFullYear();
        return dd + '/' + mm + '/' + yyyy;
    }

    function isoDate(d) {
        if (!d) return null;
        var date = new Date(d);
        if (isNaN(date.getTime())) return null;
        var yyyy = date.getFullYear();
        var mm = String(date.getMonth() + 1).padStart(2, '0');
        var dd = String(date.getDate()).padStart(2, '0');
        return yyyy + '-' + mm + '-' + dd;
    }

    function buildListInput(request) {
        request = request || {};
        var length = request.length || 10;
        return {
            filterText: ($('#TimeSlotKeywordFilter').val() || '').trim() || null,
            locationId: $('#TimeSlotLocationFilter').val() || null,
            fromDate: $('#TimeSlotFromDateFilter').val() || null,
            toDate: $('#TimeSlotToDateFilter').val() || null,
            skipCount: request.start || 0,
            maxResultCount: length > 100 ? 100 : length,
            sorting: 'stylistName asc'
        };
    }

    function loadLocationOptions($select, current) {
        return locationService.getLookup().then(function (items) {
            $select.empty();
            $select.append('<option value="">' + l('SalonBeautyTimeSlots:AllLocations') + '</option>');
            (items || []).forEach(function (it) {
                $select.append('<option value="' + it.id + '">' + htmlEncode(it.name) + '</option>');
            });
            if (current) $select.val(current);
        });
    }

    loadLocationOptions($('#TimeSlotLocationFilter'));

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
        if (!canEdit) {
            var text = row.isActive ? l('Enum:SalonBeautyTimeSlotStatus.On') : l('Enum:SalonBeautyTimeSlotStatus.Off');
            return '<span class="timeslot-status-badge ' + (row.isActive ? 'on' : 'off') + '">' + htmlEncode(text) + '</span>';
        }
        return '<span class="timeslot-status-badge ' + (row.isActive ? 'on' : 'off') + '">'
            + htmlEncode(row.isActive ? l('Enum:SalonBeautyTimeSlotStatus.On') : l('Enum:SalonBeautyTimeSlotStatus.Off'))
            + '</span>';
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

    /* Modal helpers */
    function loadLocationsIntoForm($form, current) {
        var $sel = $form.find('.timeslot-location-select');
        return locationService.getLookup().then(function (items) {
            $sel.empty();
            $sel.append('<option value="">' + l('SalonBeautyTimeSlots:LocationPlaceholder') + '</option>');
            (items || []).forEach(function (it) {
                $sel.append('<option value="' + it.id + '">' + htmlEncode(it.name) + '</option>');
            });
            if (current) $sel.val(current);
        });
    }

    function loadStylistsIntoForm($form, current) {
        var $sel = $form.find('.timeslot-stylist-select');
        if (!$sel.length) return Promise.resolve();
        return stylistService.getList({ skipCount: 0, maxResultCount: 200, sorting: 'displayName asc' }).then(function (res) {
            $sel.empty();
            $sel.append('<option value="">' + l('SalonBeautyTimeSlots:StylistPlaceholder') + '</option>');
            (res.items || []).forEach(function (it) {
                $sel.append('<option value="' + it.id + '">' + htmlEncode(it.displayName) + '</option>');
            });
            if (current) $sel.val(current);
        });
    }

    function recalcWeekdayMask($form) {
        var mask = 0;
        $form.find('.timeslot-weekday-chip.active').each(function () {
            mask |= (1 << parseInt($(this).data('day'), 10));
        });
        $form.find('.timeslot-weekday-mask').val(mask || 127);
    }

    function applyWeekdayMask($form, mask) {
        if (!mask) mask = 127;
        $form.find('.timeslot-weekday-chip').each(function () {
            var bit = 1 << parseInt($(this).data('day'), 10);
            $(this).toggleClass('active', (mask & bit) !== 0);
        });
        $form.find('.timeslot-weekday-mask').val(mask);
    }

    function buildRangeRow(start, end) {
        return '<div class="timeslot-range-row" data-range-row>'
            + '<input type="time" class="form-control timeslot-form-control timeslot-range-start" value="' + htmlEncode(start || '08:00') + '" />'
            + '<input type="time" class="form-control timeslot-form-control timeslot-range-end" value="' + htmlEncode(end || '12:00') + '" />'
            + '<button type="button" class="timeslot-remove-range-btn"><i class="fa fa-trash"></i></button>'
            + '</div>';
    }

    function setRanges($form, ranges) {
        var $list = $form.find('[data-ranges-list]');
        $list.empty();
        if (!ranges || !ranges.length) {
            $list.append(buildRangeRow('08:00', '12:00'));
            $list.append(buildRangeRow('13:00', '19:00'));
            return;
        }
        ranges.forEach(function (r) {
            $list.append(buildRangeRow(formatTime(r.startTime), formatTime(r.endTime)));
        });
    }

    function getRanges($form) {
        var ranges = [];
        $form.find('[data-range-row]').each(function () {
            var s = $(this).find('.timeslot-range-start').val();
            var e = $(this).find('.timeslot-range-end').val();
            if (s && e) {
                ranges.push({ startTime: s + ':00', endTime: e + ':00' });
            }
        });
        return ranges;
    }

    function syncStatusToggle($form) {
        $form.find('.timeslot-status-toggle').each(function () {
            var $t = $(this);
            var on = $t.is(':checked');
            $form.find('.timeslot-status-value').val(on ? '1' : '0');
            var $text = $form.find('.timeslot-status-toggle-text');
            $text.text(on ? $text.data('active') : $text.data('inactive'));
        });
        $form.find('.timeslot-showonapp-toggle').each(function () {
            var $t = $(this);
            var on = $t.is(':checked');
            $form.find('.timeslot-showonapp-value').val(on ? 'true' : 'false');
            var $text = $form.find('.timeslot-showonapp-toggle-text');
            $text.text(on ? $text.data('active') : $text.data('inactive'));
        });
    }

    $(document).on('change', '.timeslot-status-toggle, .timeslot-showonapp-toggle', function () {
        syncStatusToggle($(this).closest('form'));
    });

    $(document).on('click', '.timeslot-weekday-chip', function () {
        $(this).toggleClass('active');
        recalcWeekdayMask($(this).closest('form'));
    });

    $(document).on('click', '[data-add-range]', function () {
        var $form = $(this).closest('form');
        $form.find('[data-ranges-list]').append(buildRangeRow('14:00', '17:00'));
    });

    $(document).on('click', '.timeslot-remove-range-btn', function () {
        var $row = $(this).closest('[data-range-row]');
        var $list = $row.closest('[data-ranges-list]');
        if ($list.find('[data-range-row]').length <= 1) {
            abp.notify.warn(l('SalonBeautyTimeSlots:TimeRangeRequired'));
            return;
        }
        $row.remove();
    });

    /* Init forms when shown */
    $(document).on('shown.bs.modal', '.modal', function () {
        var $modal = $(this);
        var $form = $modal.find('form.timeslot-form');
        if (!$form.length) return;

        if ($form.is('#CreateTimeSlotForm')) {
            loadLocationsIntoForm($form);
            loadStylistsIntoForm($form);
            recalcWeekdayMask($form);
            syncStatusToggle($form);
            return;
        }

        if ($form.is('#EditTimeSlotForm')) {
            var stylistId = $form.find('.timeslot-edit-stylist-id').val();
            slotService.getByStylist(stylistId).then(function (data) {
                loadLocationsIntoForm($form, data.locationId);
                $form.find('.timeslot-stylist-display').val(data.stylistName || '');
                $form.find('.timeslot-from-date').val(isoDate(data.fromDate));
                $form.find('.timeslot-to-date').val(isoDate(data.toDate));
                applyWeekdayMask($form, data.weekdayMask || 127);
                setRanges($form, data.ranges || []);
                $form.find('textarea[name="Input.Note"]').val(data.note || '');
                var on = (data.status || 1) !== 0;
                $form.find('.timeslot-status-toggle').prop('checked', on);
                $form.find('.timeslot-showonapp-toggle').prop('checked', !!data.isShowOnApp);
                syncStatusToggle($form);
            });
        }
    });

    /* Submit */
    function submitForm($form, isEdit) {
        var locationId = $form.find('.timeslot-location-select').val();
        var fromDate = $form.find('.timeslot-from-date').val();
        var toDate = $form.find('.timeslot-to-date').val();
        var ranges = getRanges($form);
        var weekdayMask = parseInt($form.find('.timeslot-weekday-mask').val(), 10) || 127;
        var status = parseInt($form.find('.timeslot-status-value').val(), 10);
        if (isNaN(status)) status = 1;
        var isShowOnApp = $form.find('.timeslot-showonapp-value').val() === 'true';
        var note = $form.find('textarea[name="Input.Note"]').val() || null;

        if (!locationId) { abp.notify.warn(l('SalonBeautyTimeSlots:LocationRequired')); return false; }
        if (!fromDate || !toDate) { abp.notify.warn(l('SalonBeautyTimeSlots:DateRangeRequired')); return false; }
        if (new Date(fromDate) > new Date(toDate)) { abp.notify.warn(l('SalonBeautyTimeSlots:DateRangeInvalid')); return false; }
        if (!ranges.length) { abp.notify.warn(l('SalonBeautyTimeSlots:TimeRangeRequired')); return false; }
        for (var i = 0; i < ranges.length; i++) {
            if (ranges[i].startTime >= ranges[i].endTime) {
                abp.notify.warn(l('SalonBeautyTimeSlots:TimeRangeInvalid')); return false;
            }
        }

        var $submit = $form.find('.timeslot-submit-button');
        $submit.prop('disabled', true).addClass('disabled');

        var promise;
        if (isEdit) {
            var stylistId = $form.find('.timeslot-edit-stylist-id').val();
            promise = slotService.updateByStylist(stylistId, {
                locationId: locationId,
                stylistId: stylistId,
                fromDate: fromDate,
                toDate: toDate,
                ranges: ranges,
                weekdayMask: weekdayMask,
                isShowOnApp: isShowOnApp,
                status: status,
                note: note
            });
        } else {
            var newStylistId = $form.find('.timeslot-stylist-select').val();
            if (!newStylistId) { abp.notify.warn(l('SalonBeautyTimeSlots:StylistRequired')); $submit.prop('disabled', false); return false; }
            promise = slotService.create({
                locationId: locationId,
                stylistId: newStylistId,
                fromDate: fromDate,
                toDate: toDate,
                ranges: ranges,
                weekdayMask: weekdayMask,
                isShowOnApp: isShowOnApp,
                status: status,
                note: note
            });
        }

        promise.then(function () {
            abp.notify.success(l('SavedSuccessfully'));
            $form.closest('.modal').modal('hide');
            dataTable.ajax.reload(null, false);
        }).always(function () {
            $submit.prop('disabled', false).removeClass('disabled');
        });

        return false;
    }

    $(document).on('click', '#CreateTimeSlotForm .timeslot-submit-button', function (e) {
        e.preventDefault();
        return submitForm($(this).closest('form'), false);
    });

    $(document).on('click', '#EditTimeSlotForm .timeslot-submit-button', function (e) {
        e.preventDefault();
        return submitForm($(this).closest('form'), true);
    });
});
