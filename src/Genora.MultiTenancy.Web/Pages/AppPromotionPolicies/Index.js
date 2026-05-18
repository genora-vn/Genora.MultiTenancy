$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appPromotionPolicies.appPromotionPolicy;

    var createModal = new abp.ModalManager('/AppPromotionPolicies/CreateModal');
    var editModal = new abp.ModalManager('/AppPromotionPolicies/EditModal');

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

    createModal.onOpen(function () {
        initNewsEditor(createModal.getModal());
    });

    editModal.onOpen(function () {
        initNewsEditor(editModal.getModal());
    });

    function escapeHtml(value) {
        if (value === null || value === undefined) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function stripHtml(value) {
        if (value === null || value === undefined) return '';
        var div = document.createElement('div');
        div.innerHTML = String(value);
        return (div.textContent || div.innerText || '').trim();
    }

    var dataTable = $('#PromotionPolicyTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(service.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('MultiTenancy.AppPromotionPolicies.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppPromotionPolicies.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('MultiTenancy.AppPromotionPolicies.Delete') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppPromotionPolicies.Delete'),
                                confirmMessage: function (data) {
                                    return l('AreYouSureToDelete', data.record.policyTitle || data.record.golfCourseName || '');
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
                { title: l('PromotionPolicy:GolfCourse'), data: "golfCourseName" },
                {
                    title: l('PromotionPolicy:PromotionType'),
                    data: "promotionTypeName",
                    render: function (value, type, row) {
                        var color = row.promotionTypeColor || '#9ca3af';
                        var name = escapeHtml(value || '');
                        return '<span class="promotion-policy-color-dot" style="background:' + color + '"></span>' + name;
                    }
                },
                { title: l('PromotionPolicy:PolicyTitle'), data: "policyTitle" },
                {
                    title: l('PromotionPolicy:CancellationPolicyHours'),
                    data: "cancellationPolicyHours",
                    render: function (value) {
                        if (value === null || value === undefined || value === '') return '';
                        return value + ' ' + (l('Hours') || 'giờ');
                    }
                },
                {
                    title: l('PromotionPolicy:CancellationPolicyContent'),
                    data: "cancellationPolicyContent",
                    render: function (value) {
                        var text = stripHtml(value);
                        if (!text) return '';
                        return '<div class="promotion-policy-content-cell" title="' + escapeHtml(text) + '">' + escapeHtml(text) + '</div>';
                    }
                }
            ]
        })
    );

    $('#NewPromotionPolicy').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    createModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload();
    });
});
