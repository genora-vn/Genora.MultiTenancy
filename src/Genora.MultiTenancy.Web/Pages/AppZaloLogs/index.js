$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appZaloAuths.appZaloLog;

    var viewModal = new abp.ModalManager('/AppZaloLogs/ViewModal');

    function toIsoDate(input) {
        if (!input) return null;

        input = String(input).trim();

        // already yyyy-MM-dd
        if (/^\d{4}-\d{2}-\d{2}$/.test(input)) return input;

        // dd/MM/yyyy  -> yyyy-MM-dd
        var m = input.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
        if (m) {
            var dd = m[1].padStart(2, '0');
            var mm = m[2].padStart(2, '0');
            var yyyy = m[3];
            return `${yyyy}-${mm}-${dd}`;
        }

        // fallback: try Date()
        var d = new Date(input);
        if (!isNaN(d.getTime())) {
            var yyyy = d.getFullYear();
            var mm = String(d.getMonth() + 1).padStart(2, '0');
            var dd = String(d.getDate()).padStart(2, '0');
            return `${yyyy}-${mm}-${dd}`;
        }

        return null;
    }

    function getFilter() {
        var httpStatusVal = $('#HttpStatus').val();

        return {
            logAction: $('#LogAction').val() || null,
            httpStatus: httpStatusVal ? parseInt(httpStatusVal, 10) : null,

            // ✅ ép iso ở đây
            from: toIsoDate($('#From').val()),
            to: toIsoDate($('#To').val()),

            filterText: $('#FilterText').val() || null
        };
    }

    var table = $('#ZaloLogsTable').DataTable(
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
                    data: null,
                    render: function (_, __, row) {
                        return '<button class="btn btn-sm btn-outline-primary js-view" data-id="' + row.id + '">' + l('View') + '</button>';
                    }
                },
                {
                    title: l('CreationTime'),
                    data: "creationTime",
                    render: function (v) {
                        return v ? luxon.DateTime.fromISO(v).toFormat('dd/MM/yyyy HH:mm:ss') : '';
                    }
                },
                { title: l('LogAction'), data: "action" },
                { title: l('HttpStatus'), data: "httpStatus" },
                { title: l('DurationMs'), data: "durationMs" },
                {
                    title: l('Endpoint'),
                    data: "endpoint",
                    render: function (v) { return v ? abp.utils.truncateString(v, 80) : ''; }
                },
                {
                    title: l('Error'),
                    data: "error",
                    render: function (v) {
                        if (!v) return '';
                        return '<span class="text-danger">' + abp.utils.truncateString(v, 100) + '</span>';
                    }
                }
            ]
        })
    );

    $('#ZaloLogsTable').on('click', '.js-view', function () {
        viewModal.open({ id: $(this).data('id') });
    });

    $('#SearchButton').click(function (e) {
        e.preventDefault();
        table.ajax.reload();
    });
});