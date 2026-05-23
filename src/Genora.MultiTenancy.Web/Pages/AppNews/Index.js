$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appNewsServices.appNews;

    var createModal = new abp.ModalManager('/AppNews/CreateModal');
    var editModal = new abp.ModalManager('/AppNews/EditModal');

    function initNewsEditor(modal) {
        var $editor = modal.find('.news-content-editor');
        if (!$editor.length) {
            return;
        }

        if ($editor.next('.note-editor').length) {
            return;
        }

        $editor.summernote({
            height: 250,
            minHeight: 150,
            maxHeight: 600,
            focus: false,
            toolbar: [
                ['style', ['style']],
                ['font', ['bold', 'italic', 'underline', 'clear']],
                ['font2', ['superscript', 'subscript']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['insert', ['link', 'picture', 'video']],
                ['view', ['fullscreen', 'codeview', 'help']]
            ]
        });
    }

    // Lazy load ContentHtml sau khi modal Edit đã mở để tránh chậm khi nội dung lớn (ảnh base64)
    function lazyLoadEditContent(modal) {
        var $editor = modal.find('.news-content-editor');
        if (!$editor.length) return;

        var $form = modal.find('form');
        var newsId = $form.find('input[name="Id"]').val();
        if (!newsId) return;

        // Hiển thị placeholder loading nhỏ trong summernote
        if ($editor.summernote) {
            try { $editor.summernote('code', '<p class="text-muted"><em>Đang tải nội dung…</em></p>'); } catch (e) { }
        }

        $.ajax({
            url: abp.appPath + 'AppNews/EditModal',
            type: 'GET',
            data: { id: newsId, handler: 'Content' },
            cache: false
        }).done(function (res) {
            var html = (res && typeof res.contentHtml === 'string') ? res.contentHtml : '';
            try {
                if ($editor.next('.note-editor').length) {
                    $editor.summernote('code', html);
                } else {
                    $editor.val(html);
                }
            } catch (e) {
                $editor.val(html);
            }
        }).fail(function () {
            try { $editor.summernote('code', ''); } catch (e) { $editor.val(''); }
            abp.notify.warn(l('NewsContentLoadFailed') || 'Không tải được nội dung tin tức.');
        });
    }

    createModal.onOpen(function () {
        initNewsEditor(createModal.getModal());
    });

    editModal.onOpen(function () {
        var $modal = editModal.getModal();
        initNewsEditor($modal);
        lazyLoadEditContent($modal);
    });

    function initPublicDatePicker(modalManager) {
        if (!window.flatpickr) {
            return;
        }

        modalManager.getModal().find('.public-time-input').each(function () {
            flatpickr(this, {
                dateFormat: "d/m/Y",
                allowInput: true
            });
        });
    }

    createModal.onOpen(function () {
        initPublicDatePicker(createModal);
    });

    editModal.onOpen(function () {
        initPublicDatePicker(editModal);
    });

    function getFilter() {
        return {
            filterText: $('#NewsFilterText').val(),
            status: $('#NewsStatusFilter').val()
        };
    }

    var dataTable = $('#NewsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(service.getList, getFilter),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppNews.Edit') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppNews.Edit');
                                },
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: function () {
                                    return abp.auth.isGranted('MultiTenancy.AppNews.Delete') ||
                                        abp.auth.isGranted('MultiTenancy.HostAppNews.Delete');
                                },
                                confirmMessage: function (data) {
                                    return l('AreYouSureToDelete', data.record.title);
                                },
                                action: function (data) {
                                    service.delete(data.record.id).then(function () {
                                        dataTable.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                { title: l('Title'), data: "title" },
                {
                    title: l('PublishDate'),
                    data: "publishedAt",
                    render: function (data) {
                        if (!data) return '';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                    }
                },
                {
                    title: l('NewsStatus'),
                    data: "status",
                    render: function (s) {
                        if (s === 0) return '<span class="badge bg-warning">' + l('NewsStatus:Draft') + '</span>';
                        if (s === 1) return '<span class="badge bg-success">' + l('NewsStatus:Published') + '</span>';
                        if (s === 2) return '<span class="badge bg-secondary">' + l('NewsStatus:Hidden') + '</span>';
                        return '';
                    }
                },
                { title: l('DisplayOrder'), data: "displayOrder" }
            ]
        })
    );

    $('#NewNewsButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#SearchNewsButton').click(function (e) {
        e.preventDefault();
        dataTable.ajax.reload();
    });

    createModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });
});
