$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var createModal = new abp.ModalManager(abp.appPath + 'Books/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'Books/EditModal');

    const isHost = !abp.currentTenant.isAvailable; // ⭐ Host?

    var dataTable = $('#BooksTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(genora.multiTenancy.books.book.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                // Host: luôn hiển thị; Tenant: theo quyền
                                visible: abp.auth.isGranted('MultiTenancy.Books.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostBooks.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('MultiTenancy.Books.Delete') ||
                                    abp.auth.isGranted('MultiTenancy.HostBooks.Delete'),
                                confirmMessage: function (data) {
                                    return l('BookDeletionConfirmationMessage', data.record.name);
                                },
                                action: function (data) {
                                    genora.multiTenancy.books.book
                                        .delete(data.record.id)
                                        .then(function () {
                                            abp.notify.success(l('DeletedSuccessfully'));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                    }
                },
                { title: l('Name'), data: "name" },
                { title: l('Type'), data: "type", render: d => l('Enum:BookType.' + d) },
                { title: l('PublishDate'), data: "publishDate", dataFormat: "datetime" },
                { title: l('Price'), data: "price" },
                { title: l('CreationTime'), data: "creationTime", dataFormat: "datetime" }
            ]
        })
    );

    createModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    $('#NewBookButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});
