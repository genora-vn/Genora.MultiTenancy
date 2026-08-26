$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var giftService = genora.multiTenancy.appServices.hl25.hl25Gift;
    var wheelService = genora.multiTenancy.appServices.hl25.hl25WheelConfig;
    var turnLogService = genora.multiTenancy.appServices.hl25.hl25SpinTurnLog;
    var spinLogService = genora.multiTenancy.appServices.hl25.hl25SpinLog;

    // ============ Helpers ============
    function badge(text, cls) {
        return '<span class="badge ' + cls + '">' + text + '</span>';
    }

    var giftStatusMap = {
        0: badge('Còn hàng', 'bg-success'),
        1: badge('Hết hàng', 'bg-secondary'),
        2: badge('Vô hiệu', 'bg-dark')
    };

    var spinTurnSourceMap = {
        1: 'Chia sẻ Zalo',
        2: 'Chia sẻ Facebook',
        3: 'Admin cấp',
        4: 'Khác'
    };

    var rewardStatusMap = {
        0: badge('Chờ xử lý', 'bg-secondary'),
        1: badge('Trúng - chờ trao', 'bg-warning text-dark'),
        2: badge('Không trúng', 'bg-light text-dark'),
        3: badge('Đã trao', 'bg-success'),
        4: badge('Đã hủy', 'bg-danger')
    };

    function fmtDate(v) {
        if (!v) return '';
        var d = new Date(v);
        return d.toLocaleString('vi-VN');
    }

    // ============ TAB 1: Gifts ============
    var createGiftModal = new abp.ModalManager('/Hl25/GiftCreateModal');
    var editGiftModal = new abp.ModalManager('/Hl25/GiftEditModal');

    var giftsTable = $('#GiftsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(giftService.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('MultiTenancy.AppHl25Wheel.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppHl25Wheel.Edit'),
                                action: function (data) {
                                    editGiftModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('MultiTenancy.AppHl25Wheel.Delete') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppHl25Wheel.Delete'),
                                confirmMessage: function (data) {
                                    return 'Xóa quà "' + data.record.name + '"?';
                                },
                                action: function (data) {
                                    giftService.delete(data.record.id).then(function () {
                                        giftsTable.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                {
                    title: 'Ảnh',
                    data: 'imageUrl',
                    orderable: false,
                    render: function (url) {
                        return url ? '<img src="' + url + '" style="height:32px;border-radius:4px;" />' : '';
                    }
                },
                { title: 'Tên quà', data: 'name' },
                { title: 'Tổng SL', data: 'totalQuantity' },
                { title: 'Còn lại', data: 'remainingQuantity' },
                {
                    title: 'Giá trị',
                    data: 'value',
                    render: function (v) {
                        return (v || v === 0) ? new Intl.NumberFormat('vi-VN').format(v) + 'đ' : '';
                    }
                },
                {
                    title: 'Trạng thái',
                    data: 'status',
                    render: function (s) { return giftStatusMap[s] || s; }
                }
            ]
        })
    );

    $('#NewGiftButton').click(function (e) {
        e.preventDefault();
        createGiftModal.open();
    });

    createGiftModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        giftsTable.ajax.reload();
    });
    editGiftModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        giftsTable.ajax.reload();
    });

    // ============ TAB 2: Wheel config + slots ============
    var giftOptionsCache = [];

    function loadGiftOptions() {
        return giftService.getList({ maxResultCount: 1000, skipCount: 0 }).then(function (r) {
            giftOptionsCache = (r.items || []).map(function (g) {
                return { id: g.id, name: g.name };
            });
        });
    }

    function giftSelectHtml(selectedId) {
        var html = '<select class="form-select form-select-sm slot-gift">';
        html += '<option value="">— Chúc may mắn —</option>';
        giftOptionsCache.forEach(function (g) {
            var sel = (selectedId === g.id) ? ' selected' : '';
            html += '<option value="' + g.id + '"' + sel + '>' + g.name + '</option>';
        });
        html += '</select>';
        return html;
    }

    function addSlotRow(slot) {
        slot = slot || { label: '', giftId: '', winRate: 0 };
        var row = $(
            '<tr>' +
            '<td><input type="text" class="form-control form-control-sm slot-label" value="' + (slot.label || '') + '" /></td>' +
            '<td>' + giftSelectHtml(slot.giftId) + '</td>' +
            '<td><input type="number" class="form-control form-control-sm slot-rate" min="0" max="100" step="0.01" value="' + (slot.winRate || 0) + '" /></td>' +
            '<td class="text-end"><button type="button" class="btn btn-sm btn-outline-danger slot-del"><i class="fa fa-trash"></i></button></td>' +
            '</tr>'
        );
        $('#SlotsBody').append(row);
        recalcTotal();
    }

    function recalcTotal() {
        var total = 0;
        $('#SlotsBody .slot-rate').each(function () {
            total += parseFloat($(this).val()) || 0;
        });
        var $badge = $('#wcTotalRate');
        $badge.text('Tổng: ' + total + '%');
        $badge.removeClass('bg-secondary bg-success bg-danger');
        $badge.addClass(Math.abs(total - 100) < 0.01 ? 'bg-success' : 'bg-danger');
    }

    $('#AddSlotButton').click(function () { addSlotRow(); });
    $('#SlotsBody').on('click', '.slot-del', function () {
        $(this).closest('tr').remove();
        recalcTotal();
    });
    $('#SlotsBody').on('input', '.slot-rate', recalcTotal);

    function loadWheelConfig() {
        return wheelService.get().then(function (cfg) {
            $('#wcTitle').val(cfg.title || '');
            $('#wcSubTitle').val(cfg.subTitle || '');
            if (cfg.primaryColor) $('#wcPrimaryColor').val(cfg.primaryColor);
            if (cfg.secondaryColor) $('#wcSecondaryColor').val(cfg.secondaryColor);
            $('#wcIsActive').prop('checked', cfg.isActive);

            $('#SlotsBody').empty();
            (cfg.slots || []).forEach(function (s) { addSlotRow(s); });
            recalcTotal();
        });
    }

    $('#WheelConfigForm').submit(function (e) {
        e.preventDefault();

        var slots = [];
        $('#SlotsBody tr').each(function (idx) {
            var $tr = $(this);
            slots.push({
                giftId: $tr.find('.slot-gift').val() || null,
                label: $tr.find('.slot-label').val(),
                winRate: parseFloat($tr.find('.slot-rate').val()) || 0,
                displayOrder: idx
            });
        });

        var input = {
            title: $('#wcTitle').val(),
            subTitle: $('#wcSubTitle').val(),
            primaryColor: $('#wcPrimaryColor').val(),
            secondaryColor: $('#wcSecondaryColor').val(),
            isActive: $('#wcIsActive').prop('checked'),
            slots: slots
        };

        wheelService.update(input).then(function () {
            abp.notify.success('Đã lưu cấu hình vòng quay.');
            loadWheelConfig();
        });
    });

    // ============ TAB 3: Spin turn logs ============
    $('#TurnLogsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [[2, 'desc']],
            ajax: abp.libs.datatables.createAjax(turnLogService.getList),
            columnDefs: [
                { title: 'Người tham gia', data: 'participantName', render: function (v) { return v || '(chưa cập nhật)'; } },
                { title: 'SĐT', data: 'participantPhone' },
                { title: 'Thời gian', data: 'grantedTime', render: fmtDate },
                { title: 'Nguồn', data: 'source', render: function (s) { return spinTurnSourceMap[s] || s; } },
                { title: 'Số lượt +', data: 'turnsAdded' },
                { title: 'Ghi chú', data: 'note' }
            ]
        })
    );

    // ============ TAB 4: Spin logs ============
    var canEditReward = abp.auth.isGranted('MultiTenancy.AppHl25Wheel.Edit') ||
        abp.auth.isGranted('MultiTenancy.HostAppHl25Wheel.Edit');

    var spinLogsTable = $('#SpinLogsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [[2, 'desc']],
            ajax: abp.libs.datatables.createAjax(spinLogService.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: 'Đánh dấu đã trao',
                                visible: function (data) {
                                    return canEditReward && data.record.rewardStatus === 1;
                                },
                                action: function (data) {
                                    // 3 = Delivered
                                    spinLogService.updateRewardStatus(data.record.id, 3).then(function () {
                                        abp.notify.success('Đã cập nhật trạng thái trao thưởng.');
                                        spinLogsTable.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                { title: 'Người chơi', data: 'participantName', render: function (v) { return v || '(chưa cập nhật)'; } },
                { title: 'Thời gian quay', data: 'spinTime', render: fmtDate },
                { title: 'Quà trúng', data: 'giftNameSnapshot', render: function (v) { return v || '<span class="text-muted">Không trúng</span>'; } },
                {
                    title: 'Trạng thái',
                    data: 'rewardStatus',
                    render: function (s) { return rewardStatusMap[s] || s; }
                },
                { title: 'Thời gian trao', data: 'deliveredTime', render: fmtDate }
            ]
        })
    );

    // ============ Lazy-load khi mở tab config ============
    $('#tab-config').on('shown.bs.tab', function () {
        if (giftOptionsCache.length === 0) {
            loadGiftOptions().then(loadWheelConfig);
        } else {
            loadWheelConfig();
        }
    });
});
