/* Your Global Scripts */
// ===== GENORA EXCEL FUNCTION =====
(function () {
    if (!window.genora) {
        window.genora = {};
    }

    // Sử dụng cấu hình của genora debug vào để thấy nó có những cái gì
    window.genora.excel = {
        download: function (url, query) {
            var finalUrl = abp.appPath + url;
            if (query) {
                finalUrl += '?' + $.param(query);
            }
            window.location.href = finalUrl;
        },

        upload: function (options) {
            if (!options || !options.url || !options.fileInput) {
                abp.notify.error('Lỗi cấu hình');
                return;
            }

            var file = options.fileInput.files[0];
            if (!file) return;

            if (!file.name.endsWith('.xlsx')) {
                abp.notify.warn('Chỉ hỗ trợ file .xlsx');
                return;
            }

            var formData = new FormData();
            formData.append('file', file);

            abp.ui.setBusy();

            abp.ajax({
                url: abp.appPath + options.url,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false
            })
                .done(function () {
                    // Các thông báo thì dùng abp.notify để show ra front end
                    abp.notify.success('Import Excel thành công');
                    if (options.onSuccess) {
                        options.onSuccess();
                    }
                })
                .fail(function (error) {
                    // Các thông báo lỗi thì dùng abp.notify để show ra front end
                    if (error?.responseJSON?.error?.message) {
                        abp.message.error(
                            error.responseJSON.error.details,
                            error.responseJSON.error.message
                        );
                    } else {
                        abp.notify.error('Import Excel thất bại');
                    }
                })
                .always(function () {
                    abp.ui.clearBusy();
                    options.fileInput.value = '';
                });
        }
    };
})();