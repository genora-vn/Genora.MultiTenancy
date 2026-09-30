(function () {
    var service = genora.multiTenancy.appServices.hoaLinh.hlBlouseAdmin;
    var styleNames = { 1: 'Nam', 2: 'Nữ' };
    var sizeModal;
    var fpStart, fpEnd;

    // ===== Banner preview =====
    function updatePreview(val) {
        var img = $('#BannerPreview'), none = $('#BannerNoPreview');
        if (val) { img.attr('src', val).show(); none.hide(); }
        else { img.hide().attr('src', ''); none.show(); }
    }

    // ===== Campaign =====
    function loadCampaign() {
        service.getCampaign().then(function (c) {
            if (!c) return;
            $('#ProgramName').val(c.programName || '');
            $('#IntroductionHtml').val(c.introductionHtml || '');
            $('#FreeShirtLimit').val(c.freeShirtLimit ?? 2);
            $('#PointsPerShirt').val(c.pointsPerShirt ?? 150);
            $('#MaxExchangeShirt').val(c.maxExchangeShirt ?? 0);
            $('#SizeChartImageUrl').val(c.sizeChartImageUrl || '');
            updatePreview(c.sizeChartImageUrl || '');
            if (fpStart) fpStart.setDate(c.startTime || null, false);
            if (fpEnd) fpEnd.setDate(c.endTime || null, false);
            $('#IsActive').prop('checked', !!c.isActive);
        }).catch(function (err) { console.error(err); });
    }

    // Chuyển Date (giờ local flatpickr đang hiển thị) thành chuỗi "naive" KHÔNG có hậu tố Z/offset.
    // Lý do: nếu dùng toISOString() (UTC), server lưu đúng giờ UTC nhưng khi đọc lại từ SQL Server
    // DateTime có Kind=Unspecified nên serialize KHÔNG kèm Z -> browser hiểu nhầm là giờ local
    // -> lệch theo offset timezone mỗi lần lưu/tải lại (trông như bị "reset" về giá trị khác).
    // Gửi chuỗi naive giữ nguyên đúng giờ người dùng đã chọn qua mỗi vòng lưu/tải.
    function toNaiveIsoString(date) {
        if (!date) return null;
        var pad = function (n) { return String(n).padStart(2, '0'); };
        return date.getFullYear() + '-' + pad(date.getMonth() + 1) + '-' + pad(date.getDate()) +
            'T' + pad(date.getHours()) + ':' + pad(date.getMinutes()) + ':' + pad(date.getSeconds());
    }

    function saveCampaign() {
        var name = ($('#ProgramName').val() || '').trim();
        if (!name) { abp.notify.warn('Vui lòng nhập tên chương trình'); return; }
        var start = fpStart && fpStart.selectedDates[0] ? toNaiveIsoString(fpStart.selectedDates[0]) : null;
        var end = fpEnd && fpEnd.selectedDates[0] ? toNaiveIsoString(fpEnd.selectedDates[0]) : null;
        var input = {
            programName: name,
            introductionHtml: $('#IntroductionHtml').val() || null,
            freeShirtLimit: parseInt($('#FreeShirtLimit').val()) || 0,
            pointsPerShirt: parseInt($('#PointsPerShirt').val()) || 0,
            maxExchangeShirt: parseInt($('#MaxExchangeShirt').val()) || 0,
            sizeChartImageUrl: $('#SizeChartImageUrl').val() || null,
            startTime: start,
            endTime: end,
            isActive: $('#IsActive').is(':checked')
        };
        abp.ui.setBusy('#CampaignForm');
        service.saveCampaign(input)
            .then(function () { abp.notify.success('Đã lưu cấu hình'); loadCampaign(); })
            .catch(function (err) { abp.notify.error('Lỗi: ' + (err.message || '')); })
            .always(function () { abp.ui.clearBusy('#CampaignForm'); });
    }

    // ===== Banner upload (chọn tệp từ máy) =====
    // Lưu path tương đối vào input; preview dùng full URL server trả về (path tương đối cũng hiển thị được vì same-origin).
    function uploadBanner(file) {
        if (!file) return;
        if (file.size > 5 * 1024 * 1024) { abp.notify.warn('Ảnh vượt quá 5MB'); $('#BannerFile').val(''); return; }
        var form = new FormData();
        form.append('file', file);
        abp.ui.setBusy('#CampaignForm');
        fetch(abp.appPath + 'HoaLinh/BlouseConfig?handler=UploadImage', {
            method: 'POST',
            headers: { RequestVerificationToken: abp.security.antiForgery.getToken() },
            body: form,
            credentials: 'same-origin'
        }).then(function (r) {
            if (!r.ok) return r.json().then(function (e) { throw new Error(e.message || 'Upload thất bại'); });
            return r.json();
        }).then(function (data) {
            $('#SizeChartImageUrl').val(data.path || data.url || '');
            updatePreview(data.url || data.path || '');
            abp.notify.success('Đã tải ảnh lên');
        }).catch(function (err) {
            abp.notify.error(err.message || 'Upload thất bại');
        }).finally(function () {
            abp.ui.clearBusy('#CampaignForm');
            $('#BannerFile').val('');
        });
    }

    // ===== Sizes =====
    var sizesById = {};

    function renderSizes(items) {
        var tbody = $('#HlBlouseSizeTable tbody'); tbody.empty();
        sizesById = {};
        if (!items || items.length === 0) {
            tbody.append('<tr><td colspan="7" class="text-center text-muted py-3">Chưa có size</td></tr>');
            return;
        }
        items.forEach(function (s) {
            sizesById[s.id] = s;
            tbody.append(
                '<tr>' +
                '<td>' + (styleNames[s.style] || '') + '</td>' +
                '<td><strong>' + (s.sizeCode || '') + '</strong></td>' +
                '<td>' + (s.weightRange || '') + '</td>' +
                '<td class="text-center">' + (s.stockQuantity || 0) + '</td>' +
                '<td class="text-center">' + (s.displayOrder || 0) + '</td>' +
                '<td class="text-center">' + (s.isActive ? '<i class="fa fa-check text-success"></i>' : '<i class="fa fa-times text-muted"></i>') + '</td>' +
                '<td class="text-center text-nowrap">' +
                '<button class="btn btn-sm btn-outline-primary btn-edit-size me-1" data-id="' + s.id + '" title="Sửa"><i class="fa fa-edit"></i></button>' +
                '<button class="btn btn-sm btn-outline-danger btn-del-size" data-id="' + s.id + '" title="Xóa"><i class="fa fa-trash"></i></button>' +
                '</td></tr>'
            );
        });
    }

    function loadSizes() {
        abp.ui.setBusy('#SizeContainer');
        service.getSizes()
            .then(function (r) { renderSizes(r.items); })
            .catch(function (err) { console.error(err); renderSizes([]); })
            .always(function () { abp.ui.clearBusy('#SizeContainer'); });
    }

    function openSizeModal(size) {
        if (size) {
            $('#SizeModalTitle').html('<i class="fa fa-ruler me-2"></i>Sửa size');
            $('#SizeId').val(size.id);
            $('#SizeStyle').val(size.style);
            $('#SizeCode').val(size.sizeCode);
            $('#WeightRange').val(size.weightRange || '');
            $('#StockQuantity').val(size.stockQuantity || 0);
            $('#DisplayOrder').val(size.displayOrder || 0);
            $('#SizeIsActive').prop('checked', !!size.isActive);
        } else {
            $('#SizeModalTitle').html('<i class="fa fa-ruler me-2"></i>Thêm size');
            $('#SizeId').val('');
            $('#SizeStyle').val('1');
            $('#SizeCode').val('');
            $('#WeightRange').val('');
            $('#StockQuantity').val(0);
            $('#DisplayOrder').val(0);
            $('#SizeIsActive').prop('checked', true);
        }
        sizeModal.show();
    }

    function saveSize() {
        var code = ($('#SizeCode').val() || '').trim();
        if (!code) { abp.notify.warn('Vui lòng nhập mã size'); return; }
        var input = {
            style: parseInt($('#SizeStyle').val()),
            sizeCode: code,
            weightRange: $('#WeightRange').val() || null,
            stockQuantity: parseInt($('#StockQuantity').val()) || 0,
            displayOrder: parseInt($('#DisplayOrder').val()) || 0,
            isActive: $('#SizeIsActive').is(':checked')
        };
        var id = $('#SizeId').val();
        var promise = id ? service.updateSize(id, input) : service.createSize(input);
        promise
            .then(function () { abp.notify.success('Đã lưu size'); sizeModal.hide(); loadSizes(); })
            .catch(function (err) { abp.notify.error('Lỗi: ' + (err.message || '')); });
    }

    function deleteSize(id) {
        abp.message.confirm('Bạn có chắc muốn xóa size này?', 'Xác nhận', function (ok) {
            if (!ok) return;
            service.deleteSize(id)
                .then(function () { abp.notify.success('Đã xóa'); loadSizes(); })
                .catch(function (err) { abp.notify.error('Lỗi: ' + (err.message || '')); });
        });
    }

    // Events
    $(function () {
        sizeModal = new bootstrap.Modal(document.getElementById('SizeModal'));
        if (window.flatpickr) {
            var opts = { enableTime: true, dateFormat: 'd/m/Y H:i', time_24hr: true, allowInput: true, minuteIncrement: 5 };
            if (flatpickr.l10ns && flatpickr.l10ns.vn) opts.locale = flatpickr.l10ns.vn;
            fpStart = flatpickr('#StartTime', opts);
            fpEnd = flatpickr('#EndTime', opts);
        }
        $('#BtnSaveCampaign').click(saveCampaign);
        $('#SizeChartImageUrl').on('input', function () { updatePreview($(this).val()); });
        $('#BannerFile').on('change', function () { uploadBanner(this.files && this.files[0]); });
        $('#BtnAddSize').click(function () { openSizeModal(null); });
        $('#BtnSaveSize').click(saveSize);
        $(document).on('click', '.btn-edit-size', function () { openSizeModal(sizesById[$(this).data('id')]); });
        $(document).on('click', '.btn-del-size', function () { deleteSize($(this).data('id')); });
        loadCampaign();
        loadSizes();
    });
})();
