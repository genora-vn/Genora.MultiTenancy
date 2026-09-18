(function () {
    window.genora = window.genora || {};
    genora.hoaLinhSales = {
        parseDate: function (value) {
            if (!value) return null;
            var parts, year, month, day;
            if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
                parts = value.split('-'); year = +parts[0]; month = +parts[1]; day = +parts[2];
            } else if (/^\d{1,2}\/\d{1,2}\/\d{4}$/.test(value)) {
                parts = value.split('/'); year = +parts[2]; month = +parts[1]; day = +parts[0];
            } else return undefined;
            var date = new Date(0);
            date.setFullYear(year, month - 1, day);
            if (year < 1 || year > 9998 || date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day)
                return undefined;
            return String(year).padStart(4, '0') + '-' + String(month).padStart(2, '0') + '-' + String(day).padStart(2, '0');
        },
        initDates: function () {
            if (typeof flatpickr !== 'function') return;
            flatpickr('#FilterDateFrom', { dateFormat: 'd/m/Y', allowInput: true });
            flatpickr('#FilterDateTo', { dateFormat: 'd/m/Y', allowInput: true });
        },
        dates: function () {
            var from = this.parseDate(($('#FilterDateFrom').val() || '').trim());
            var to = this.parseDate(($('#FilterDateTo').val() || '').trim());
            if (from === undefined || to === undefined) {
                abp.notify.warn('Ngày tìm kiếm không hợp lệ. Vui lòng nhập dd/MM/yyyy.');
                return null;
            }
            if (from && to && from > to) {
                abp.notify.warn('Từ ngày không được lớn hơn đến ngày.');
                return null;
            }
            return { dateFrom: from, dateTo: to };
        },
        clearDates: function () {
            ['FilterDateFrom', 'FilterDateTo'].forEach(function (id) {
                var input = document.getElementById(id);
                if (input && input._flatpickr) input._flatpickr.clear();
                else $('#' + id).val('');
            });
        },
        download: function (route, filter, button) {
            var query = new URLSearchParams();
            Object.keys(filter).forEach(function (key) {
                if (filter[key] != null && filter[key] !== '') query.set(key, filter[key]);
            });
            $(button).prop('disabled', true);
            // Same-origin fetch sends authentication and tenant cookies automatically.
            return fetch(abp.appPath + route + '?' + query.toString(), { credentials: 'same-origin' })
                .then(async function (response) {
                    if (!response.ok || response.redirected || !response.headers.get('content-type')?.includes('spreadsheetml')) {
                        var error = await response.json().catch(function () { return null; });
                        throw new Error(error?.error?.message || 'Không thể xuất Excel. Vui lòng thử lại.');
                    }
                    var blob = await response.blob();
                    var disposition = response.headers.get('content-disposition') || '';
                    var match = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
                    var filename = match ? decodeURIComponent(match[1]) : 'HoaLinhSales.xlsx';
                    if (!match) {
                        match = /filename="?([^";]+)"?/i.exec(disposition);
                        if (match) filename = match[1];
                    }
                    var url = URL.createObjectURL(blob);
                    var link = document.createElement('a');
                    link.href = url; link.download = filename;
                    document.body.appendChild(link); link.click(); link.remove();
                    setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
                })
                .catch(function (error) { abp.notify.error(error.message); })
                .finally(function () { $(button).prop('disabled', false); });
        }
    };
})();
