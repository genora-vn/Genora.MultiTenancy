window.genoraFnb = (function () {
    function toNullableInt(value) {
        if (value === undefined || value === null || value === '') return null;
        var n = parseInt(value, 10);
        return isNaN(n) ? null : n;
    }

    function toNullableString(value) {
        if (value === undefined || value === null) return null;
        value = String(value).trim();
        return value === '' ? null : value;
    }

    function formatCurrency(value) {
        var number = Number(value || 0);
        return number.toLocaleString('vi-VN') + 'đ';
    }

    function formatDateTime(value) {
        if (!value) return '';

        try {
            if (window.luxon && luxon.DateTime) {
                return luxon.DateTime.fromISO(value).toFormat('dd/MM/yyyy HH:mm');
            }

            var d = new Date(value);
            if (isNaN(d.getTime())) return value;

            var dd = String(d.getDate()).padStart(2, '0');
            var mm = String(d.getMonth() + 1).padStart(2, '0');
            var yyyy = d.getFullYear();
            var hh = String(d.getHours()).padStart(2, '0');
            var mi = String(d.getMinutes()).padStart(2, '0');

            return dd + '/' + mm + '/' + yyyy + ' ' + hh + ':' + mi;
        } catch (e) {
            console.error('formatDateTime error:', e, value);
            return value;
        }
    }

    function boolBadge(value, yesText, noText) {
        return value
            ? '<span class="fnb-badge fnb-badge--success">' + yesText + '</span>'
            : '<span class="fnb-badge fnb-badge--neutral">' + noText + '</span>';
    }

    function safeRecord(data) {
        return !!(data && data.record);
    }

    function safeId(data) {
        return safeRecord(data) && data.record.id ? data.record.id : null;
    }

    function createServerAjax(serviceMethod, buildInput, fallbackSorting) {
        return function (requestData, callback) {
            var sorting = fallbackSorting || '';

            if (requestData.columns && requestData.order && requestData.order.length) {
                var orderCol = requestData.order[0].column;
                var orderDir = requestData.order[0].dir;
                var colName = requestData.columns[orderCol] && requestData.columns[orderCol].data;

                if (colName) {
                    sorting = colName + ' ' + orderDir;
                }
            }

            var input = $.extend({}, buildInput(), {
                skipCount: requestData.start || 0,
                maxResultCount: requestData.length || 10,
                sorting: sorting
            });

            serviceMethod(input)
                .then(function (result) {
                    callback({
                        recordsTotal: result.totalCount || 0,
                        recordsFiltered: result.totalCount || 0,
                        data: result.items || []
                    });
                })
                .catch(function (error) {
                    console.error('DataTable load error:', error);
                    if (window.abp && abp.notify) {
                        var l = abp.localization.getResource('MultiTenancy');
                        abp.notify.error(l('Error:Generic'));
                    }

                    callback({
                        recordsTotal: 0,
                        recordsFiltered: 0,
                        data: []
                    });
                });
        };
    }

    return {
        toNullableInt: toNullableInt,
        toNullableString: toNullableString,
        formatCurrency: formatCurrency,
        formatDateTime: formatDateTime,
        boolBadge: boolBadge,
        safeRecord: safeRecord,
        safeId: safeId,
        createServerAjax: createServerAjax
    };
})();