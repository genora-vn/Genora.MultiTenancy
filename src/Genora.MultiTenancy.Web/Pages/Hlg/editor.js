(function (root) {
    'use strict';
    function move(items, index, delta) {
        var target = index + delta;
        if (index >= 0 && index < items.length && target >= 0 && target < items.length) {
            var value = items.splice(index, 1)[0]; items.splice(target, 0, value);
        }
        return items;
    }
    function toAbsoluteUrl(url) {
        if (!url || /^(https?:|data:|blob:)/i.test(url)) return url;
        var base = (root.APP_BASE_URL || root.location.origin || '').replace(/\/+$/, '');
        if (!base) return url;
        return base + (url.charAt(0) === '/' ? url : '/' + url);
    }
    function init(modal) {
        var $ = root.jQuery, l = abp.localization.getResource('MultiTenancy');
        var lookup = root.hlgAdmin.resolveService('hlgLookupAdmin');
        var form = modal.find('form').addBack('form').first();
        if (!form.length) form = modal.closest('form');
        function syncRichEditors(container) {
            container.find('.hlg-rich').each(function () {
                var editor = $(this);
                if ($.fn.summernote && editor.next('.note-editor').length) editor.val(editor.summernote('code'));
            });
        }
        function richTextIsEmpty(value) {
            return $('<div>').html(value || '').text().replace(/\u00a0/g, ' ').trim() === '';
        }
        function activateInvalidTab() {
            var invalid = form.find('[aria-invalid="true"], .input-validation-error, .error').filter(':input').first();
            var pane = invalid.closest('.tab-pane');
            if (!pane.length) return;
            var trigger = modal.find('[data-bs-toggle="tab"][data-bs-target="#' + pane.attr('id') + '"]').get(0);
            if (trigger && root.bootstrap && root.bootstrap.Tab) root.bootstrap.Tab.getOrCreateInstance(trigger).show();
            invalid.trigger('focus');
        }
        function reparseValidation() {
            if (!form.length || !$.validator || !$.validator.unobtrusive) return true;
            form.removeData('validator').removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(form);
            var validator = form.data('validator');
            if (validator) validator.settings.ignore = [];
            return form.valid();
        }
        function upload(file, mediaType) {
            var form = new FormData(); form.append('file', file);
            var endpoint = abp.appPath + 'Hlg/Upload' + (mediaType === 'video' ? '?handler=Video' : '');
            return fetch(endpoint, { method: 'POST', headers: { RequestVerificationToken: abp.security.antiForgery.getToken() }, body: form })
                .then(function (r) { if (!r.ok) throw new Error(l('Hlg:UploadFailed')); return r.json(); });
        }
        function enhance(container) {
            container.find('[data-hlg-lookup]').each(function () {
                var select = $(this);
                if (select.data('hlg-ready')) return;
                select.data('hlg-ready', true);
                function parentId() {
                    var kind = select.attr('data-hlg-lookup');
                    return modal.find('[name="Input.' + (kind === 'brands' ? 'CategoryId' : kind === 'prizes' ? 'EventId' : '') + '"]').val() || null;
                }
                function excludeId() {
                    return select.attr('data-hlg-lookup') === 'products' ? modal.find('input[name="Id"]').val() || null : null;
                }
                var current = select.val();
                if (!current || current === '00000000-0000-0000-0000-000000000000') select.val('');
                select.select2({ width: '100%', dropdownParent: modal, placeholder: l('Hlg:Select'), allowClear: true,
                    ajax: { delay: 250, transport: function (params, success, failure) {
                        var page = params.data.page || 1;
                        return lookup.getList({ kind: select.attr('data-hlg-lookup'), parentId: parentId(), excludeId: excludeId(), filterText: params.data.term || '', skipCount: (page - 1) * 50, maxResultCount: 50 })
                            .then(function (data) { success({ results: data.items.map(function (x) { return { id: x.id, text: x.name }; }), pagination: { more: page * 50 < data.totalCount } }); }, failure);
                    } }
                });
                select.next('.select2-container').addClass('hlg-select2');
                if (current && current !== '00000000-0000-0000-0000-000000000000') {
                    lookup.getList({ kind: select.attr('data-hlg-lookup'), id: current, excludeId: excludeId(), maxResultCount: 1 }).then(function (data) {
                        if (!data.items.length) return;
                        var item = data.items[0];
                        select.find('option').filter(function () { return this.value === current; }).remove();
                        select.append(new Option(item.name, item.id, true, true)).trigger('change.select2');
                    });
                }
            });
            container.find('.hlg-rich').each(function () {
                var editor = $(this);
                if (!$.fn.summernote || editor.next('.note-editor').length) return;
                editor.summernote({
                    height: 240,
                    minHeight: 180,
                    maxHeight: 600,
                    focus: false,
                    toolbar: [
                        ['style', ['style']],
                        ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
                        ['fontname', ['fontname']],
                        ['fontsize', ['fontsize']],
                        ['color', ['forecolor', 'backcolor']],
                        ['para', ['ul', 'ol', 'paragraph']],
                        ['height', ['height']],
                        ['table', ['table']],
                        ['insert', ['link', 'picture', 'video', 'hr']],
                        ['view', ['fullscreen', 'codeview', 'help']]
                    ],
                    callbacks: { onChange: function (html) { editor.val(html); }, onImageUpload: function (files) {
                        Array.from(files).forEach(function (file) { upload(file).then(function (data) { editor.summernote('insertImage', toAbsoluteUrl(data.url)); }).catch(function () { abp.notify.error(l('Hlg:UploadFailed')); }); });
                    } }
                });
            });
            container.find('input[data-hlg-media-url]').each(function () {
                var target = $(this); if (target.data('hlg-upload')) return; target.data('hlg-upload', true);
                var row = target.closest('fieldset');
                var kind = row.find('[data-field="Kind"]');
                var file = $('<input type="file" class="form-control mt-2" data-hlg-media-upload>')
                    .attr('aria-label', l('Hlg:UploadImage')).insertAfter(target);
                var previewHost = target.closest('[data-hlg-image-field]').find('[data-hlg-image-preview]').first();
                var isModal = modal.is('.modal') || modal.closest('.modal').length || modal.find('.modal-dialog').length;
                if (!previewHost.length && isModal) {
                    var field = target.closest('.mb-3');
                    if (field.length) {
                        var inputColumn = $('<div class="col-md-8">');
                        field.children().appendTo(inputColumn);
                        var previewColumn = $('<div class="col-md-4 mt-3 mt-md-0">');
                        previewHost = $('<div class="hlg-image-preview-slot" data-hlg-image-preview>').appendTo(previewColumn);
                        field.addClass('row align-items-start').attr('data-hlg-image-field', '');
                        field.append(inputColumn, previewColumn);
                    }
                }
                var imagePreview = $('<img class="img-thumbnail hlg-image-preview d-none">').attr('alt', l('Hlg:ImagePreview'));
                var videoPreview = $('<video class="img-thumbnail hlg-video-preview d-none" controls preload="metadata">')
                    .attr('aria-label', l('Hlg:VideoPreview'));
                if (previewHost.length) previewHost.append(imagePreview, videoPreview);
                else target.parent().append(imagePreview.addClass('mt-2'), videoPreview.addClass('mt-2'));
                var objectUrl = null;
                function isVideo() { return Number(kind.val() || 1) === 2; }
                function releaseObjectUrl() {
                    if (!objectUrl) return;
                    root.URL.revokeObjectURL(objectUrl);
                    objectUrl = null;
                }
                function showPreview(url) {
                    imagePreview.addClass('d-none').removeAttr('src');
                    videoPreview.addClass('d-none').removeAttr('src');
                    if (videoPreview.get(0)) videoPreview.get(0).load();
                    if (!url) return;
                    if (isVideo()) {
                        videoPreview.attr('src', url).removeClass('d-none');
                        videoPreview.get(0).load();
                    } else imagePreview.attr('src', url).removeClass('d-none');
                }
                function syncKind() {
                    file.attr('accept', isVideo() ? 'video/mp4,video/webm,video/ogg,video/quicktime' : 'image/png,image/jpeg,image/webp,image/gif');
                    file.attr('aria-label', l(isVideo() ? 'Hlg:UploadVideo' : 'Hlg:UploadImage')).val('');
                    releaseObjectUrl();
                    showPreview((target.val() || '').trim());
                }
                imagePreview.on('error', function () { $(this).addClass('d-none'); });
                imagePreview.on('load', function () { $(this).removeClass('d-none'); });
                videoPreview.on('error', function () { $(this).addClass('d-none'); });
                target.on('input change', function () { releaseObjectUrl(); showPreview(target.val().trim()); });
                kind.on('change', syncKind);
                syncKind();
                file.on('change', function () {
                    var selectedFile = this.files[0];
                    if (!selectedFile) return;
                    releaseObjectUrl();
                    objectUrl = root.URL.createObjectURL(selectedFile);
                    showPreview(objectUrl);
                    file.prop('disabled', true);
                    upload(selectedFile, isVideo() ? 'video' : 'image').then(function (data) {
                        target.val(data.url).trigger('change');
                    }).catch(function () {
                        releaseObjectUrl();
                        showPreview((target.val() || '').trim());
                        file.val('');
                        abp.notify.error(l(isVideo() ? 'Hlg:VideoUploadFailed' : 'Hlg:UploadFailed'));
                    }).finally(function () { file.prop('disabled', false); });
                });
            });
            container.find('input[data-hlg-image], input[name$="ThumbnailUrl"], input[name$="ImageUrl"], input[name$="BannerUrl"]').each(function () {
                var target = $(this); if (target.data('hlg-upload')) return; target.data('hlg-upload', true);
                var file = $('<input type="file" accept="image/png,image/jpeg,image/webp,image/gif" class="form-control mt-2">').attr('aria-label', l('Hlg:UploadImage')).insertAfter(target);
                var previewHost = target.closest('[data-hlg-image-field]').find('[data-hlg-image-preview]').first();
                var isModal = modal.is('.modal') || modal.closest('.modal').length || modal.find('.modal-dialog').length;
                if (!previewHost.length && isModal) {
                    var field = target.closest('.mb-3');
                    if (field.length) {
                        var inputColumn = $('<div class="col-md-8">');
                        field.children().appendTo(inputColumn);
                        var previewColumn = $('<div class="col-md-4 mt-3 mt-md-0">');
                        previewHost = $('<div class="hlg-image-preview-slot" data-hlg-image-preview>')
                            .appendTo(previewColumn);
                        field.addClass('row align-items-start').attr('data-hlg-image-field', '');
                        field.append(inputColumn, previewColumn);
                    }
                }
                var preview = $('<img class="img-thumbnail hlg-image-preview d-none">').attr('alt', l('Hlg:ImagePreview'));
                if (previewHost.length) preview.appendTo(previewHost);
                else preview.addClass('mt-2').insertAfter(file);
                var objectUrl = null;
                function releaseObjectUrl() {
                    if (!objectUrl) return;
                    root.URL.revokeObjectURL(objectUrl);
                    objectUrl = null;
                }
                function showPreview(url) {
                    if (!url) {
                        preview.removeAttr('src').addClass('d-none');
                        return;
                    }
                    preview.attr('src', url).removeClass('d-none');
                }
                preview.on('error', function () { $(this).addClass('d-none'); });
                preview.on('load', function () { $(this).removeClass('d-none'); });
                target.on('input change', function () {
                    releaseObjectUrl();
                    showPreview(target.val().trim());
                });
                showPreview((target.val() || '').trim());
                file.on('change', function () {
                    var selectedFile = this.files[0];
                    if (!selectedFile) return;
                    releaseObjectUrl();
                    objectUrl = root.URL.createObjectURL(selectedFile);
                    showPreview(objectUrl);
                    file.prop('disabled', true);
                    upload(selectedFile).then(function (data) {
                        target.val(data.url).trigger('change');
                    }).catch(function () {
                        releaseObjectUrl();
                        showPreview((target.val() || '').trim());
                        file.val('');
                        abp.notify.error(l('Hlg:UploadFailed'));
                    }).finally(function () {
                        file.prop('disabled', false);
                    });
                });
            });
        }
        modal.find('.hlg-collection').each(function () {
            var box = $(this), kind = box.attr('data-kind'), prefix = box.attr('data-prefix');
            var items = JSON.parse(box.find('.hlg-initial').val() || '[]');
            var rows = $('<div>').appendTo(box);
            function read() {
                var values = [];
                rows.children().each(function () {
                    var row = $(this);
                    if (kind === 'related') values.push(row.find('[data-field="Id"]').val() || '');
                    else {
                        var item = {};
                        row.find('[data-field]').each(function () { var value = $(this).val(); item[$(this).attr('data-field')] = $(this).is('select') ? Number(value) : value; });
                        values.push(item);
                    }
                }); return values;
            }
            function draw() {
                rows.find('.hlg-rich').each(function () { if ($(this).next('.note-editor').length) $(this).summernote('destroy'); });
                rows.empty();
                items.forEach(function (item, index) {
                    var row = $('<fieldset class="border rounded p-3 mb-3">').appendTo(rows);
                    var fields = kind === 'knowledge' ? [['Title', 'text'], ['Content', 'rich']] : kind === 'media' ? [['Kind', 'kind'], ['Placement', 'placement'], ['Url', 'text'], ['PosterUrl', 'text'], ['AltText', 'text']] : [['Id', 'lookup']];
                    fields.forEach(function (f) {
                        var field = $('<div class="mb-3">').appendTo(row);
                        var label = l('Hlg:' + (f[0] === 'Id' ? 'RelatedProduct' : f[0]));
                        $('<label class="form-label d-block">').text(label).appendTo(field);
                        var input;
                        if (f[1] === 'rich') input = $('<textarea rows="4" class="form-control hlg-rich mb-2">');
                        else if (f[1] === 'kind' || f[1] === 'placement') {
                            input = $('<select class="form-select mb-2">');
                            (f[1] === 'kind' ? ['Image', 'Video'] : ['Hero', 'Information', 'Knowledge', 'Related']).forEach(function (key, i) { $('<option>').val(i + 1).text(l('Hlg:' + key)).appendTo(input); });
                        } else if (f[1] === 'lookup') { input = $('<select class="form-select mb-2" data-hlg-lookup="products">'); $('<option>').val('').text(l('Hlg:Select')).appendTo(input); if (item) $('<option>').val(item).text(item).appendTo(input); }
                        else input = $('<input type="text" class="form-control mb-2">');
                        var name = prefix + '[' + index + ']' + (kind === 'related' ? '' : '.' + f[0]);
                        input.attr('name', name).attr('data-field', f[0])
                            .val(kind === 'related' ? item : item[f[0]] || (f[1] === 'kind' || f[1] === 'placement' ? 1 : '')).appendTo(field);
                        if ((kind === 'knowledge' && (f[0] === 'Title' || f[0] === 'Content')) ||
                            (kind === 'media' && f[0] === 'Url') || kind === 'related') {
                            input.attr({ 'data-val': 'true', 'data-val-required': l('Hlg:FieldRequired', label), 'aria-required': 'true' });
                            $('<span class="text-danger field-validation-valid">')
                                .attr({ 'data-valmsg-for': name, 'data-valmsg-replace': 'true' }).appendTo(field);
                        }
                        if (kind === 'media' && f[0] === 'Url') input.attr('data-hlg-media-url', 'true');
                        if (kind === 'media' && f[0] === 'PosterUrl') input.attr('data-hlg-image', 'true');
                    });
                    if (kind === 'media') {
                        var kindInput = row.find('[data-field="Kind"]');
                        var mediaUrl = row.find('[data-field="Url"]');
                        var posterField = row.find('[data-field="PosterUrl"]').closest('.mb-3');
                        var posterInput = posterField.find('[data-field="PosterUrl"]');
                        function syncMediaKind() {
                            var video = Number(kindInput.val() || 1) === 2;
                            posterField.toggleClass('d-none', !video);
                            posterInput.prop('disabled', !video);
                            if (!video) posterInput.val('').trigger('change');
                            mediaUrl.trigger('change');
                        }
                        kindInput.on('change', syncMediaKind);
                        syncMediaKind();
                    }
                    [['MoveUp', -1], ['MoveDown', 1], ['Remove', 0]].forEach(function (action) {
                        $('<button type="button" class="btn btn-sm btn-outline-secondary me-2 mt-2">').text(l('Hlg:' + action[0])).appendTo(row).on('click', function () {
                            items = read(); if (action[1]) move(items, index, action[1]); else items.splice(index, 1); draw();
                        });
                    });
                }); enhance(rows);
            }
            function compact() {
                syncRichEditors(rows);
                items = read().filter(function (item) {
                    if (kind === 'related') return !!item;
                    if (kind === 'knowledge') return !!((item.Title || '').trim() || !richTextIsEmpty(item.Content));
                    return !!((item.Url || '').trim() || (item.PosterUrl || '').trim() || (item.AltText || '').trim());
                });
                draw();
            }
            box.data('hlg-compact', compact);
            $('<button type="button" class="btn btn-outline-primary mb-3">').text(l('Hlg:Add')).appendTo(box).on('click', function () {
                items = read(); items.push(kind === 'related' ? '' : {}); draw();
            }); draw();
        });
        modal.find('[name="Input.CategoryId"]').on('change', function () { modal.find('[name="Input.BrandId"]').val('').trigger('change'); });
        modal.find('[name="Input.EventId"]').on('change', function () { modal.find('[name="Input.PrizeId"]').val('').trigger('change'); });
        enhance(modal);
        if (form.length && !form.data('hlg-submit-ready')) {
            form.data('hlg-submit-ready', true);
            form.get(0).addEventListener('submit', function (event) {
                syncRichEditors(form);
                modal.find('.hlg-collection').each(function () {
                    var compact = $(this).data('hlg-compact');
                    if (compact) compact();
                });
                if (!reparseValidation()) {
                    event.preventDefault();
                    event.stopImmediatePropagation();
                    activateInvalidTab();
                    abp.notify.warn(l('Hlg:FixValidationErrors'));
                }
            }, true);
        }
    }
    root.hlgEditor = { init: init, move: move };
})(window);
