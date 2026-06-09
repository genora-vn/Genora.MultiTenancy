$(function () {
    var caddieService = genora.multiTenancy.appServices.caddies.caddie;
    var createModal = new abp.ModalManager(abp.appPath + 'AppCaddies/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'AppCaddies/EditModal');

    var canEdit = $('#CanEdit').val() === 'true';
    var canDelete = $('#CanDelete').val() === 'true';

    var dataTable = $('#CaddiesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(caddieService.getList, function () {
                return {
                    filter: undefined,
                    status: undefined
                };
            }),
            columnDefs: [
                {
                    title: 'Thao tác',
                    orderable: false,
                    width: '60px',
                    render: function (data, type, row) {
                        var items = [];
                        items.push('<li><a class="dropdown-item caddie-action-detail" data-id="' + row.id + '"><i class="fa fa-eye me-2 text-primary"></i>Xem chi tiết</a></li>');
                        if (canEdit) {
                            items.push('<li><a class="dropdown-item caddie-action-edit" data-id="' + row.id + '"><i class="fa fa-pencil me-2"></i>Sửa Caddy</a></li>');
                            items.push('<li><a class="dropdown-item caddie-action-schedule" data-id="' + row.id + '"><i class="fa fa-calendar me-2"></i>Xem lịch làm việc</a></li>');
                        }
                        if (canDelete) {
                            items.push('<li><hr class="dropdown-divider"></li>');
                            items.push('<li><a class="dropdown-item text-danger caddie-action-delete" data-id="' + row.id + '" data-name="' + row.caddieName + '"><i class="fa fa-trash me-2"></i>Xóa Caddy</a></li>');
                        }
                        return '<div class="dropdown"><button class="caddie-action-btn dropdown-toggle" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button><ul class="dropdown-menu dropdown-menu-end shadow-sm">' + items.join('') + '</ul></div>';
                    }
                },
                {
                    title: 'Mã Caddy',
                    data: 'caddieCode',
                    width: '100px',
                    render: function (data) {
                        return '<span class="caddie-code">' + data + '</span>';
                    }
                },
                {
                    title: 'Tên Caddy',
                    data: 'caddieName',
                    render: function (data, type, row) {
                        var avatar = (row.avatar && row.avatar.trim()) ? row.avatar : '/images/default-avatar.png';
                        return '<div class="caddie-name-cell">' +
                            '<img src="' + avatar + '" class="caddie-avatar" onerror="this.src=\'/images/default-avatar.png\'" />' +
                            '<span class="caddie-name">' + data + '</span></div>';
                    }
                },
                {
                    title: 'Ngày vào làm',
                    data: 'joinDate',
                    render: function (data) {
                        if (!data) return '<span style="color:#707783">—</span>';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                    }
                },
                {
                    title: 'Ngoại ngữ',
                    data: 'languages',
                    orderable: false,
                    render: function (data) {
                        if (!data || data.length === 0) return '<span style="color:#707783">—</span>';
                        var langColors = ['caddie-lang-badge-primary', 'caddie-lang-badge-tertiary', 'caddie-lang-badge-default'];
                        return data.map(function (lang, i) {
                            var cls = langColors[i % langColors.length];
                            return '<span class="caddie-lang-badge ' + cls + '">' + lang + '</span>';
                        }).join('');
                    }
                },
                {
                    title: 'Ngày được KH Booking',
                    data: 'lastBookingDate',
                    render: function (data) {
                        if (!data) return '<span style="color:#707783">—</span>';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                    }
                },
                {
                    title: 'Đánh giá sao',
                    data: 'ratingAvg',
                    render: function (data) {
                        // floor-based: 4.1 → 4 filled, 0.0 → 0 filled
                        var filled = Math.floor(data || 0);
                        var stars = '';
                        for (var i = 1; i <= 5; i++) {
                            if (i <= filled)
                                stars += '<i class="fa fa-star" style="color:#f59e0b;font-size:13px;"></i>';
                            else
                                stars += '<i class="fa fa-star" style="color:#cbd5e1;font-size:13px;"></i>';
                        }
                        var label = data > 0
                            ? ' <span style="font-size:11px;color:var(--caddie-on-surface-variant);margin-left:2px;">' + parseFloat(data).toFixed(1) + '</span>'
                            : '';
                        return '<span title="' + parseFloat(data).toFixed(1) + '/5">' + stars + label + '</span>';
                    }
                },
                {
                    title: 'Trạng thái',
                    data: 'status',
                    width: '80px',
                    render: function (data, type, row) {
                        var checked = data === 1 ? 'checked' : '';
                        if (!canEdit) {
                            return data === 1
                                ? '<span class="badge bg-success">Hoạt động</span>'
                                : '<span class="badge bg-secondary">Ngừng</span>';
                        }
                        return '<label class="caddie-toggle-switch">' +
                            '<input type="checkbox" ' + checked + ' data-id="' + row.id + '" class="caddie-status-toggle">' +
                            '<span class="caddie-toggle-slider"></span></label>';
                    }
                }
            ]
        })
    );

    // Toggle status
    $(document).on('change', '.caddie-status-toggle', function () {
        var id = $(this).data('id');
        var newStatus = this.checked ? 1 : 2;
        caddieService.updateStatus(id, newStatus).then(function () {
            abp.notify.success('Cập nhật trạng thái thành công');
        }).catch(function () {
            abp.notify.error('Cập nhật trạng thái thất bại');
            dataTable.ajax.reload();
        });
    });

    // Create
    $('#NewCaddieButton').click(function () {
        createModal.open();
    });

    createModal.onResult(function () {
        dataTable.ajax.reload();
        abp.notify.success('Thêm Caddy thành công');
    });

    // Edit
    $(document).on('click', '.caddie-action-edit', function () {
        editModal.open({ id: $(this).data('id') });
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
        abp.notify.success('Cập nhật Caddy thành công');
    });

    // Delete
    $(document).on('click', '.caddie-action-delete', function () {
        var id = $(this).data('id');
        var name = $(this).data('name');
        abp.message.confirm('Bạn có chắc chắn muốn xóa Caddy "' + name + '"?', 'Xác nhận xóa')
            .then(function (confirmed) {
                if (confirmed) {
                    caddieService.delete(id).then(function () {
                        dataTable.ajax.reload();
                        abp.notify.success('Đã xóa Caddy');
                    });
                }
            });
    });

    // Detail
    $(document).on('click', '.caddie-action-detail', function () {
        window.location.href = '/AppCaddies/Detail?id=' + $(this).data('id');
    });

    // Schedule
    $(document).on('click', '.caddie-action-schedule', function () {
        window.location.href = '/AppCaddieSchedules?caddieId=' + $(this).data('id');
    });
});
