$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appGolfCourses.appGolfCourse;

    var createModal = new abp.ModalManager('/AppGolfCourses/CreateModal');
    var editModal = new abp.ModalManager('/AppGolfCourses/EditModal');

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
