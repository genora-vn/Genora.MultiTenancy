$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appGolfCourses.appGolfCourse;

    var createModal = new abp.ModalManager('/AppGolfCourses/CreateModal');
    var editModal = new abp.ModalManager('/AppGolfCourses/EditModal');

    function initNewsEditor(modal) {
        var $editor = modal.find('.news-content-editor');
        if (!$editor.length) {
            return;
        }

        // Nếu đã init rồi thì không init lại
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
    // Khi mở modal tạo mới
    createModal.onOpen(function () {
        initNewsEditor(createModal.getModal());
    });

    // Khi mở modal sửa
    editModal.onOpen(function () {
        initNewsEditor(editModal.getModal());
    });

    var dataTable = $('#GolfCoursesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: true,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(service.getList),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('MultiTenancy.AppGolfCourses.Edit') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppGolfCourses.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('MultiTenancy.AppGolfCourses.Delete') ||
                                    abp.auth.isGranted('MultiTenancy.HostAppGolfCourses.Delete'),
                                confirmMessage: function (data) {
                                    return l('AreYouSureToDelete', data.record.name);
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
                { title: l('GolfCourseCode'), data: "code" },
                { title: l('GolfCourseName'), data: "name" },
                { title: l('Province'), data: "province" },
                { title: l('Phone'), data: "phone" },
                { title: l('Website'), data: "website" },
                {
                    title: l('IsActive'),
                    data: "isActive",
                    render: function (active) {
                        return active
                            ? '<span class="badge bg-success">' + l('Yes') + '</span>'
                            : '<span class="badge bg-secondary">' + l('No') + '</span>';
                    }
                }
            ]
        })
    );

    $('#NewGolfCourseButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    createModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully'));
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully'));
        dataTable.ajax.reload();
    });

    createModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully'));
        dataTable.ajax.reload();
    });

    //// Validate URL
    //function isValidUrl(value) {
    //    if (!value || value.trim() === '') {
    //        return true;
    //    }
    //    try {
    //        new URL(value);
    //        return true;
    //    } catch (e) {
    //        return false;
    //    }
    //}

    //$(document).on('submit', '#CreateGolfCourseForm, #EditGolfCourseForm', function (e) {
    //    var $form = $(this);
    //    var website = $form.find('input[name$=".Website"]').val();
    //    var fanpage = $form.find('input[name$=".FanpageUrl"]').val();

    //    if (!isValidUrl(website) || !isValidUrl(fanpage)) {
    //        e.preventDefault();
    //        abp.message.warn(l('InvalidUrl'));
    //    }
    //});
});
