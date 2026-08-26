$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.hl25.hl25Participant;

    var editModal = new abp.ModalManager('/Hl25/ParticipantEditModal');
    var grantModal = new abp.ModalManager('/Hl25/ParticipantGrantModal');

    function badge(text, cls) {
        return '<span class="badge ' + cls + '">' + text + '</span>';
    }

    var genderMap = {
        0: 'Không xác định',
        1: 'Nam',
        2: 'Nữ',
        3: 'Khác'
    };

    function fmtDate(v) {
        if (!v) return '';
        return new Date(v).toLocaleString('vi-VN');
    }

    function fmtDateOnly(v) {
        if (!v) return '';
        return new Date(v).toLocaleDateString('vi-VN');
    }

    var canEdit = abp.auth.isGranted('MultiTenancy.AppHl25Participants.Edit') ||
        abp.auth.isGranted('MultiTenancy.HostAppHl25Participants.Edit');

    // Flatpickr cho bộ lọc ngày
    if (window.flatpickr) {
        flatpickr('#FilterJoinedFrom', { dateFormat: 'Y-m-d' });
        flatpickr('#FilterJoinedTo', { dateFormat: 'Y-m-d' });
    }

    function getFilter() {
        var followOa = $('#FilterFollowOa').val();
        var consent = $('#FilterConsent').val();
        return {
            filterText: $('#FilterText').val() || null,
            isFollowingOa: followOa === '' ? null : (followOa === 'true'),
            hasConsent: consent === '' ? null : (consent === 'true'),
            joinedFrom: $('#FilterJoinedFrom').val() || null,
            joinedTo: $('#FilterJoinedTo').val() || null
        };
    }

    var dataTable = $('#ParticipantsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [[3, 'desc']],
            ajax: abp.libs.datatables.createAjax(service.getList, getFilter),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: canEdit,
                                action: function (data) { editModal.open({ id: data.record.id }); }
                            },
                            {
                                text: 'Cộng lượt quay',
                                visible: canEdit,
                                action: function (data) { grantModal.open({ id: data.record.id }); }
                            }
                        ]
                    }
                },
                { title: 'Họ tên', data: 'fullName', render: function (v) { return v || '(chưa cập nhật)'; } },
                { title: 'SĐT', data: 'phoneNumber' },
                { title: 'Ngày tham gia', data: 'joinedTime', render: fmtDate },
                { title: 'Giới tính', data: 'gender', render: function (g) { return genderMap[g] || g; } },
                {
                    title: 'Follow OA',
                    data: 'isFollowingOa',
                    render: function (v) { return v ? badge('Có', 'bg-success') : badge('Chưa', 'bg-secondary'); }
                },
                {
                    title: 'Consent',
                    data: 'hasConsent',
                    render: function (v) { return v ? badge('Đồng ý', 'bg-success') : badge('Chưa', 'bg-secondary'); }
                },
                { title: 'Lượt còn lại', data: 'remainingSpinTurns' },
                { title: 'Tổng lượt', data: 'totalSpinTurns' },
                { title: 'Quà trúng', data: 'totalGiftsWon' },
                {
                    title: 'Địa chỉ nhận quà',
                    data: 'receiveAddress',
                    render: function (v) {
                        if (!v) return '';
                        return v.length > 40 ? v.substring(0, 40) + '…' : v;
                    }
                }
            ]
        })
    );

    $('#ApplyFilterBtn').click(function () {
        dataTable.ajax.reload();
    });

    $('#FilterText').on('keypress', function (e) {
        if (e.which === 13) { dataTable.ajax.reload(); }
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    grantModal.onResult(function () {
        abp.notify.success('Đã cộng lượt quay.');
        dataTable.ajax.reload();
    });

    // Xuất Excel theo filter
    $('#ExportParticipantsBtn').click(function (e) {
        e.preventDefault();
        if (!window.genora || !genora.excel) {
            abp.notify.error('Excel helper chưa được load');
            return;
        }
        genora.excel.download('api/app/hl25-participant-excel/export', getFilter());
    });
});
