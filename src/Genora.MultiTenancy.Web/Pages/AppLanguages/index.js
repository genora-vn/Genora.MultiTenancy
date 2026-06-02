$(function () {
    var langService = genora.multiTenancy.appServices.caddies.caddieLanguage;
    var createModal = new abp.ModalManager(abp.appPath + 'AppLanguages/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'AppLanguages/EditModal');

    var canEdit = $('#CanEdit').val() === 'true';
    var canDelete = $('#CanDelete').val() === 'true';

    var dataTable = $('#LanguagesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(langService.getList),
            columnDefs: [
                {
                    title: 'Thao tác',
                    orderable: false,
                    width: '60px',
                    render: function (data, type, row) {
                        var items = [];
                        if (canEdit) {
                            items.push('<li><a class="dropdown-item lang-action-edit" data-id="' + row.id + '"><i class="fa fa-pencil me-2"></i>Sửa</a></li>');
                        }
                        if (canDelete) {
                            items.push('<li><a class="dropdown-item text-danger lang-action-delete" data-id="' + row.id + '" data-name="' + row.languageName + '"><i class="fa fa-trash me-2"></i>Xóa</a></li>');
                        }
                        if (items.length === 0) return '';
                        return '<div class="dropdown"><button class="caddie-action-btn dropdown-toggle" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button><ul class="dropdown-menu dropdown-menu-end shadow-sm">' + items.join('') + '</ul></div>';
                    }
                },
                {
                    title: 'Mã ngôn ngữ',
                    data: 'languageCode',
                    render: function (data) { return '<span class="caddie-code">' + data + '</span>'; }
                },
                {
                    title: 'Tên ngôn ngữ',
                    data: 'languageName',
                    render: function (data) { return '<strong>' + data + '</strong>'; }
                },
                {
                    title: 'Tên gốc',
                    data: 'nativeName',
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
                    width: '100px',
                    render: function (data) {
                        return data === 1
                            ? '<span style="background:#dcfce7;color:#166534;font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">Hoạt động</span>'
                            : '<span style="background:#f3f4f6;color:#6b7280;font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">Ngừng</span>';
                    }
                }
            ]
        })
    );

    $('#NewLanguageButton').click(function () { createModal.open(); });
    createModal.onResult(function () { dataTable.ajax.reload(); abp.notify.success('Thêm ngôn ngữ thành công'); });

    $(document).on('click', '.lang-action-edit', function () { editModal.open({ id: $(this).data('id') }); });
    editModal.onResult(function () { dataTable.ajax.reload(); abp.notify.success('Cập nhật ngôn ngữ thành công'); });

    $(document).on('click', '.lang-action-delete', function () {
        var id = $(this).data('id');
        var name = $(this).data('name');
        abp.message.confirm('Bạn có chắc chắn muốn xóa ngôn ngữ "' + name + '"?', 'Xác nhận xóa')
            .then(function (confirmed) {
                if (confirmed) {
                    langService.delete(id).then(function () { dataTable.ajax.reload(); abp.notify.success('Đã xóa ngôn ngữ'); });
                }
            });
    });
});
