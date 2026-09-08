$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var campaignService = genora.multiTenancy.appServices.hl25.hl25FrameCampaign;
    var templateService = genora.multiTenancy.appServices.hl25.hl25FrameTemplate;
    var creationService = genora.multiTenancy.appServices.hl25.hl25FrameCreation;

    function badge(text, cls) {
        return '<span class="badge ' + cls + '">' + text + '</span>';
    }

    function e(enumName, val) { return l('Enum:' + enumName + ':' + val) || val; }

    var campaignStatusMap = {
        0: badge(e('Hl25CampaignStatus', 0), 'bg-secondary'),
        1: badge(e('Hl25CampaignStatus', 1), 'bg-success'),
        2: badge(e('Hl25CampaignStatus', 2), 'bg-warning text-dark'),
        3: badge(e('Hl25CampaignStatus', 3), 'bg-dark')
    };

    var sharePlatformMap = {
        0: '<span class="text-muted">' + e('Hl25SharePlatform', 0) + '</span>',
        1: badge(e('Hl25SharePlatform', 1), 'bg-primary'),
        2: badge(e('Hl25SharePlatform', 2), 'bg-info text-dark')
    };

    function fmtDate(v) {
        if (!v) return '';
        return new Date(v).toLocaleString('vi-VN');
    }

    var canCreate = abp.auth.isGranted('MultiTenancy.AppHl25Frames.Create') ||
        abp.auth.isGranted('MultiTenancy.HostAppHl25Frames.Create');
    var canEdit = abp.auth.isGranted('MultiTenancy.AppHl25Frames.Edit') ||
        abp.auth.isGranted('MultiTenancy.HostAppHl25Frames.Edit');
    var canDelete = abp.auth.isGranted('MultiTenancy.AppHl25Frames.Delete') ||
        abp.auth.isGranted('MultiTenancy.HostAppHl25Frames.Delete');

    // ============ TAB 1: Campaigns ============
    var createCampaignModal = new abp.ModalManager('/Hl25/CampaignCreateModal');
    var editCampaignModal = new abp.ModalManager('/Hl25/CampaignEditModal');

    var campaignsTable = $('#CampaignsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(campaignService.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: canEdit,
                                action: function (data) { editCampaignModal.open({ id: data.record.id }); }
                            },
                            {
                                text: l('Delete'),
                                visible: canDelete,
                                confirmMessage: function (data) { return 'Xóa chiến dịch "' + data.record.name + '"?'; },
                                action: function (data) {
                                    campaignService.delete(data.record.id).then(function () {
                                        campaignsTable.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                { title: 'Tên chiến dịch', data: 'name' },
                { title: 'Số mẫu', data: 'templateCount' },
                { title: 'Bắt đầu', data: 'startTime', render: fmtDate },
                { title: 'Kết thúc', data: 'endTime', render: fmtDate },
                { title: 'Trạng thái', data: 'status', render: function (s) { return campaignStatusMap[s] || s; } }
            ]
        })
    );

    $('#NewCampaignButton').click(function (e) {
        e.preventDefault();
        createCampaignModal.open();
    });

    createCampaignModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        campaignsTable.ajax.reload();
        loadCampaignFilterOptions();
    });
    editCampaignModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        campaignsTable.ajax.reload();
        loadCampaignFilterOptions();
    });

    // ============ TAB 2: Templates ============
    var createTemplateModal = new abp.ModalManager('/Hl25/TemplateCreateModal');
    var editTemplateModal = new abp.ModalManager('/Hl25/TemplateEditModal');

    var templatesTable = $('#TemplatesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(templateService.getList, function () {
                var campaignId = $('#TemplateCampaignFilter').val();
                return { campaignId: campaignId || null };
            }),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: canEdit,
                                action: function (data) { editTemplateModal.open({ id: data.record.id }); }
                            },
                            {
                                text: l('Delete'),
                                visible: canDelete,
                                confirmMessage: function (data) { return 'Xóa mẫu "' + data.record.name + '"?'; },
                                action: function (data) {
                                    templateService.delete(data.record.id).then(function () {
                                        templatesTable.ajax.reload();
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
                        return url ? '<img src="' + url + '" style="height:40px;border-radius:4px;" />' : '';
                    }
                },
                { title: 'Tên mẫu', data: 'name' },
                { title: 'Thứ tự', data: 'displayOrder' },
                {
                    title: 'Kích hoạt',
                    data: 'isActive',
                    render: function (a) {
                        return a ? badge('Có', 'bg-success') : badge('Không', 'bg-secondary');
                    }
                }
            ]
        })
    );

    $('#NewTemplateButton').click(function (e) {
        e.preventDefault();
        createTemplateModal.open();
    });
    $('#TemplateCampaignFilter').on('change', function () {
        templatesTable.ajax.reload();
    });

    createTemplateModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        templatesTable.ajax.reload();
    });
    editTemplateModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        templatesTable.ajax.reload();
    });

    // ============ TAB 3: Frame creations ============
    $('#CreationsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [[2, 'desc']],
            ajax: abp.libs.datatables.createAjax(creationService.getList),
            columnDefs: [
                {
                    title: 'Ảnh thiệp',
                    data: 'resultImageUrl',
                    orderable: false,
                    render: function (url) {
                        return url ? '<img src="' + url + '" style="height:40px;border-radius:4px;" />' : '';
                    }
                },
                { title: 'Người tạo', data: 'participantName', render: function (v) { return v || '(chưa cập nhật)'; } },
                { title: 'Thời gian', data: 'createdTime', render: fmtDate },
                { title: 'Chiến dịch', data: 'campaignName', render: function (v) { return v || '-'; } },
                {
                    title: 'Lời chúc',
                    data: 'wishMessage',
                    render: function (v) {
                        if (!v) return '';
                        return v.length > 50 ? v.substring(0, 50) + '…' : v;
                    }
                },
                { title: 'Chia sẻ', data: 'sharePlatform', render: function (s) { return sharePlatformMap[s] || s; } },
                {
                    title: 'Link',
                    data: 'shareLink',
                    orderable: false,
                    render: function (url) {
                        return url ? '<a href="' + url + '" target="_blank" rel="noopener"><i class="fa fa-link"></i></a>' : '';
                    }
                }
            ]
        })
    );

    // ============ Campaign filter dropdown ============
    function loadCampaignFilterOptions() {
        campaignService.getList({ maxResultCount: 1000, skipCount: 0 }).then(function (r) {
            var $sel = $('#TemplateCampaignFilter');
            var current = $sel.val();
            $sel.find('option:not(:first)').remove();
            (r.items || []).forEach(function (c) {
                $sel.append('<option value="' + c.id + '">' + c.name + '</option>');
            });
            if (current) $sel.val(current);
        });
    }

    loadCampaignFilterOptions();
});
