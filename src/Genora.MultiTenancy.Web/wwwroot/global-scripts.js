/* Your Global Scripts */
// ===== GENORA EXCEL FUNCTION =====
(function () {
    if (!window.genora) {
        window.genora = {};
    }

    window.genora.excel = {
        /**
         * Tải file Excel từ server.
         * Dùng fetch + Blob để force download, tránh browser mở URL thay vì tải.
         * Các query param có giá trị rỗng / null / undefined sẽ bị loại bỏ tự động.
         */
        download: function (url, query) {
            // Lọc bỏ param rỗng để tránh server binding lỗi
            var cleanQuery = {};
            if (query) {
                Object.keys(query).forEach(function (k) {
                    var v = query[k];
                    if (v !== null && v !== undefined && v !== '') {
                        cleanQuery[k] = v;
                    }
                });
            }

            var finalUrl = abp.appPath + url;
            var qs = $.param(cleanQuery);
            if (qs) finalUrl += '?' + qs;

            // Lấy CSRF token của ABP (nếu có)
            var headers = { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() || '' };

            abp.ui.setBusy();

            fetch(finalUrl, { method: 'GET', headers: headers, credentials: 'same-origin' })
                .then(function (response) {
                    if (!response.ok) {
                        return response.text().then(function (text) {
                            throw new Error('Server trả về lỗi ' + response.status + ': ' + text);
                        });
                    }

                    // Lấy tên file từ header Content-Disposition
                    var disposition = response.headers.get('Content-Disposition') || '';
                    var fileNameMatch = disposition.match(/filename\*?=(?:UTF-8'')?([^;"\n]+)/i);
                    var fileName = fileNameMatch
                        ? decodeURIComponent(fileNameMatch[1].replace(/"/g, ''))
                        : 'export.xlsx';

                    return response.blob().then(function (blob) {
                        return { blob: blob, fileName: fileName };
                    });
                })
                .then(function (result) {
                    var objectUrl = URL.createObjectURL(result.blob);
                    var a = document.createElement('a');
                    a.href = objectUrl;
                    a.download = result.fileName;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    URL.revokeObjectURL(objectUrl);
                })
                .catch(function (err) {
                    abp.notify.error(err.message || 'Xuất Excel thất bại');
                })
                .finally(function () {
                    abp.ui.clearBusy();
                });
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
                    abp.notify.success('Import Excel thành công');
                    if (options.onSuccess) {
                        options.onSuccess();
                    }
                })
                .fail(function (error) {
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
(function () {
    if (!window.HTMLInputElement) return;

    var _orig = HTMLInputElement.prototype.setSelectionRange;
    if (typeof _orig !== "function") return;

    if (HTMLInputElement.prototype.__patchedSetSelectionRangeForNumber) return;
    HTMLInputElement.prototype.__patchedSetSelectionRangeForNumber = true;

    HTMLInputElement.prototype.setSelectionRange = function (start, end, direction) {
        try {
            var t = (this.type || "").toLowerCase();
            if (t === "number") return;
            return _orig.call(this, start, end, direction);
        } catch (e) {
            var t2 = (this.type || "").toLowerCase();
            if (t2 === "number") return;
            throw e;
        }
    };
})();

(function () {
    function applyMenuTooltips() {
        document.querySelectorAll("li.lpx-inner-menu-item").forEach((li) => {
            const a = li.querySelector("a.lpx-menu-item-link");
            if (!a) return;

            const textEl = a.querySelector(".lpx-menu-item-text");
            const text = (textEl?.textContent || "").trim();
            if (!text) return;

            if (!li.getAttribute("title")) {
                li.setAttribute("title", text);
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", applyMenuTooltips);
    } else {
        applyMenuTooltips();
    }

    document.addEventListener("abp.dynamicScriptsInitialized", applyMenuTooltips);
})();

// Helper để hiển thị số điện thoại đã được mask, khi hover sẽ hiển thị số đầy đủ (nếu có)
var UIHelper = {
    renderPhoneWithTooltip: function (maskedPhone, fullPhone) {
        if (!maskedPhone) return "";
        var displayFull = fullPhone || maskedPhone;

        // Sử dụng data-toggle="tooltip" của Bootstrap
        return `<span data-toggle="tooltip" 
                      data-placement="top" 
                      title="${displayFull}" 
                      style="cursor:pointer; border-bottom: 1px dashed #007bff; color: #007bff; display: inline-block;">
                    ${maskedPhone}
                </span>`;
    }
};