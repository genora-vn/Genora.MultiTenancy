$(function () {
    var skillService = genora.multiTenancy.appServices.caddies.caddieSkill;
    var createModal = new abp.ModalManager(abp.appPath + 'AppCaddieSkills/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'AppCaddieSkills/EditModal');

    var canEdit = $('#CanEdit').val() === 'true';
    var canDelete = $('#CanDelete').val() === 'true';

    var dataTable = $('#CaddieSkillsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(skillService.getList),
            columnDefs: [
                {
                    title: 'Thao tác',
                    orderable: false,
                    width: '60px',
                    render: function (data, type, row) {
                        var items = [];
                        if (canEdit) {
                            items.push('<li><a class="dropdown-item skill-action-edit" data-id="' + row.id + '"><i class="fa fa-pencil me-2"></i>Sửa</a></li>');
                        }
                        if (canDelete) {
                            items.push('<li><a class="dropdown-item text-danger skill-action-delete" data-id="' + row.id + '" data-name="' + row.skillName + '"><i class="fa fa-trash me-2"></i>Xóa</a></li>');
                        }
                        if (items.length === 0) return '';
                        return '<div class="dropdown"><button class="caddie-action-btn dropdown-toggle" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button><ul class="dropdown-menu dropdown-menu-end shadow-sm">' + items.join('') + '</ul></div>';
                    }
                },
                {
                    title: 'Mã kỹ năng',
                    data: 'skillCode',
                    render: function (data) { return '<span class="caddie-code">' + data + '</span>'; }
                },
                {
                    title: 'TÊN KỸ NĂNG / CHUYÊN MÔN',
                    data: 'skillName',
                    render: function (data) { return '<strong>' + data + '</strong>'; }
                },
                {
                    title: 'Ghi chú nội bộ',
                    data: 'description',
                    render: function (data) { return data || '<span style="color:#707783">—</span>'; }
                },
                {
                    title: 'Thứ tự',
                    data: 'sortOrder',
                    width: '80px'
                },
                {
                    title: 'Trạng thái',
                    data: 'status',
                    width: '120px',
                    render: function (data, type, row) {
                        if (!canEdit) {
                            return data === 1
                                ? '<span style="background:#dcfce7;color:#166534;font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">Hoạt động</span>'
                                : '<span style="background:#f3f4f6;color:#6b7280;font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">Ngừng</span>';
                        }
                        var checked = data === 1 ? 'checked' : '';
                        return '<label class="caddie-toggle-switch">' +
                            '<input type="checkbox" class="skill-toggle-status" data-id="' + row.id + '" ' + checked + ' />' +
                            '<span class="caddie-toggle-slider"></span>' +
                            '</label>';
                    }
                }
            ]
        })
    );

    // Toggle status
    $(document).on('change', '.skill-toggle-status', function () {
        var id = $(this).data('id');
        var newStatus = $(this).is(':checked') ? 1 : 2;
        var $toggle = $(this);

        skillService.updateStatus(id, newStatus).then(function () {
            abp.notify.success(newStatus === 1 ? 'Đã bật hoạt động' : 'Đã tắt hoạt động');
        }).catch(function (err) {
            // Revert toggle on error
            $toggle.prop('checked', !$toggle.is(':checked'));
            abp.notify.error(err.message || 'Cập nhật thất bại');
        });
    });

    $('#NewSkillButton').click(function () { createModal.open(); });
    createModal.onResult(function () { dataTable.ajax.reload(); abp.notify.success('Thêm kỹ năng thành công'); });

    $(document).on('click', '.skill-action-edit', function () { editModal.open({ id: $(this).data('id') }); });
    editModal.onResult(function () { dataTable.ajax.reload(); abp.notify.success('Cập nhật kỹ năng thành công'); });

    $(document).on('click', '.skill-action-delete', function () {
        var id = $(this).data('id');
        var name = $(this).data('name');
        abp.message.confirm('Bạn có chắc chắn muốn xóa kỹ năng "' + name + '"?', 'Xác nhận xóa')
            .then(function (confirmed) {
                if (confirmed) {
                    skillService.delete(id).then(function () { dataTable.ajax.reload(); abp.notify.success('Đã xóa kỹ năng'); });
                }
            });
    });
});
