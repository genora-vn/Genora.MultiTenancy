$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    // proxies
    var service = genora.multiTenancy.appServices.appSpecialDates.appSpecialDate;
    var gcService = genora.multiTenancy.appServices.appGolfCourses.appGolfCourse;

    var createModal = new abp.ModalManager(abp.appPath + 'AppSpecialDates/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'AppSpecialDates/EditModal');

    // golfCourseId -> name
    var golfCourseMap = {};

    function loadGolfCoursesMap() {
        return gcService.getList({ skipCount: 0, maxResultCount: 1000, sorting: "Name" })
            .then(function (res) {
                golfCourseMap = {};
                (res.items || []).forEach(function (x) {
                    golfCourseMap[x.id] = x.name;
                });
            });
    }

    function formatDdMMyyyy(x) {
        var dt = new Date(x);
        if (isNaN(dt.getTime())) return '';
        var dd = String(dt.getDate()).padStart(2, '0');
        var mm = String(dt.getMonth() + 1).padStart(2, '0');
        var yy = dt.getFullYear();
        return dd + '/' + mm + '/' + yy;
    }

    // ✅ mapping 0..6 = T2..CN (đúng với DB 31/96 của bạn)
    var weekdayLabels = {
        0: "T2",
        1: "T3",
        2: "T4",
        3: "T5",
        4: "T6",
        5: "T7",
        6: "CN"
    };

    function renderApDungCho(name, weekdays) {
        var n = (name || "").toLowerCase().trim();
        if (n === "ngày lễ" || n === "ngay le") return "—";

        // nếu server trả [] hoặc null => hiểu là "Tất cả"
        if (!weekdays || !weekdays.length) return "Tất cả";

        var arr = weekdays
            .map(function (x) { return parseInt(x, 10); })
            .filter(function (x) { return x >= 0 && x <= 6; })
            .sort(function (a, b) { return a - b; })
            .map(function (x) { return weekdayLabels[x] || String(x); });

        if (!arr.length) return "Tất cả";
        return arr.join(", ");
    }

    function initDataTable() {
        var dataTable = $('#AppSpecialDatesTable').DataTable(
            abp.libs.datatables.normalizeConfiguration({
                serverSide: true,
                paging: true,
                order: [[1, "asc"]],
                searching: true,
                scrollX: true,

                ajax: abp.libs.datatables.createAjax(service.getList, function (request) {
                    return {
                        filter: request.search?.value || null,
                        skipCount: request.start,
                        maxResultCount: request.length,
                        sorting: request.columns?.[request.order?.[0]?.column]?.data
                            ? request.columns[request.order[0].column].data + ' ' + request.order[0].dir
                            : null
                    };
                }),

                columnDefs: [
                    {
                        title: l('Actions'),
                        rowAction: {
                            items: [
                                {
                                    text: l('Edit'),
                                    visible: function () { return true; },
                                    action: function (data) { editModal.open({ id: data.record.id }); }
                                },
                                {
                                    text: l('Delete'),
                                    visible: function () { return true; },
                                    confirmMessage: function (data) {
                                        return l('DeletionConfirmationMessage') + ': ' + (data.record.name || '');
                                    },
                                    action: function (data) {
                                        service
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
                    { title: l('Description'), data: "description" },

                    // ✅ NEW COLUMN: Áp dụng cho
                    {
                        title: "Áp dụng cho",
                        data: "weekdays",
                        sortable: false,
                        render: function (d, type, row) {
                            return renderApDungCho(row?.name, d);
                        }
                    },

                    {
                        title: l('Dates'),
                        data: "dates",
                        sortable: false,
                        render: function (d) {
                            if (!d || !d.length) return '';
                            var arr = d.slice(0, 6).map(formatDdMMyyyy).filter(Boolean);
                            return arr.join(', ') + (d.length > 6 ? '…' : '');
                        }
                    },

                    {
                        title: l('GolfCourse'),
                        data: "golfCourseId",
                        render: function (d) {
                            if (!d) return '';
                            return golfCourseMap[d] || d; // fallback GUID
                        }
                    },

                    {
                        title: l('IsActive'),
                        data: "isActive",
                        render: function (active) {
                            return active
                                ? '<span class="badge bg-success">' + l('Yes') + '</span>'
                                : '<span class="badge bg-secondary">' + l('No') + '</span>';
                        }
                    },

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

        $('#NewAppSpecialDateButton').click(function (e) {
            e.preventDefault();
            createModal.open();
        });
    }

    loadGolfCoursesMap()
        .then(initDataTable)
        .catch(function (err) {
            console.error(err);
            initDataTable();
        });
});
