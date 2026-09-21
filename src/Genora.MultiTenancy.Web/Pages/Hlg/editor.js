(function (root) {
    'use strict';
    function move(items, index, delta) {
        var target = index + delta;
        if (index >= 0 && index < items.length && target >= 0 && target < items.length) {
            var value = items.splice(index, 1)[0]; items.splice(target, 0, value);
        }
        return items;
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
        function upload(file) {
            var form = new FormData(); form.append('file', file);
            return fetch(abp.appPath + 'Hlg/Upload', { method: 'POST', headers: { RequestVerificationToken: abp.security.antiForgery.getToken() }, body: form })
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
                var current = select.val();
                if (!current || current === '00000000-0000-0000-0000-000000000000') select.val('');
                select.select2({ width: '100%', dropdownParent: modal, placeholder: l('Hlg:Select'), allowClear: true,
                    ajax: { delay: 250, transport: function (params, success, failure) {
                        var page = params.data.page || 1;
                        return lookup.getList({ kind: select.attr('data-hlg-lookup'), parentId: parentId(), filterText: params.data.term || '', skipCount: (page - 1) * 50, maxResultCount: 50 })
                            .then(function (data) { success({ results: data.items.map(function (x) { return { id: x.id, text: x.name }; }), pagination: { more: page * 50 < data.totalCount } }); }, failure);
                    } }
                });
                if (current && current !== '00000000-0000-0000-0000-000000000000') {
                    lookup.getList({ kind: select.attr('data-hlg-lookup'), id: current, maxResultCount: 1 }).then(function (data) {
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
                editor.summernote({ height: 240, toolbar: [['style', ['style', 'bold', 'italic', 'underline']], ['para', ['ul', 'ol']], ['insert', ['link', 'picture']], ['view', ['codeview']]],
                    callbacks: { onChange: function (html) { editor.val(html); }, onImageUpload: function (files) {
                        Array.from(files).forEach(function (file) { upload(file).then(function (data) { editor.summernote('insertImage', data.url); }).catch(function () { abp.notify.error(l('Hlg:UploadFailed')); }); });
                    } }
                });
            });
            container.find('input[data-hlg-image], input[name$="ThumbnailUrl"], input[name$="ImageUrl"], input[name$="BannerUrl"]').each(function () {
                var target = $(this); if (target.data('hlg-upload')) return; target.data('hlg-upload', true);
                var file = $('<input type="file" accept="image/png,image/jpeg,image/webp,image/gif" class="form-control mt-2">').attr('aria-label', l('Hlg:UploadImage')).insertAfter(target);
                file.on('change', function () { if (this.files[0]) upload(this.files[0]).then(function (data) { target.val(data.url).trigger('change'); }).catch(function () { abp.notify.error(l('Hlg:UploadFailed')); }); });
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
                        var label = l('Hlg:' + (f[0] === 'Id' ? 'RelatedProduct' : f[0]));
                        $('<label class="form-label d-block">').text(label).appendTo(row);
                        var input;
                        if (f[1] === 'rich') input = $('<textarea rows="4" class="form-control hlg-rich mb-2">');
                        else if (f[1] === 'kind' || f[1] === 'placement') {
                            input = $('<select class="form-select mb-2">');
                            (f[1] === 'kind' ? ['Image', 'Video'] : ['Hero', 'Information', 'Knowledge', 'Related']).forEach(function (key, i) { $('<option>').val(i + 1).text(l('Hlg:' + key)).appendTo(input); });
                        } else if (f[1] === 'lookup') { input = $('<select class="form-select mb-2" data-hlg-lookup="products">'); $('<option>').val('').text(l('Hlg:Select')).appendTo(input); if (item) $('<option>').val(item).text(item).appendTo(input); }
                        else input = $('<input type="text" class="form-control mb-2">');
                        var name = prefix + '[' + index + ']' + (kind === 'related' ? '' : '.' + f[0]);
                        input.attr('name', name).attr('data-field', f[0])
                            .val(kind === 'related' ? item : item[f[0]] || (f[1] === 'kind' || f[1] === 'placement' ? 1 : '')).appendTo(row);
                        if ((kind === 'knowledge' && (f[0] === 'Title' || f[0] === 'Content')) ||
                            (kind === 'media' && f[0] === 'Url') || kind === 'related') {
                            input.attr({ 'data-val': 'true', 'data-val-required': l('Hlg:FieldRequired', label), 'aria-required': 'true' });
                            $('<span class="text-danger field-validation-valid">')
                                .attr({ 'data-valmsg-for': name, 'data-valmsg-replace': 'true' }).appendTo(row);
                        }
                        if (kind === 'media' && (f[0] === 'PosterUrl' || (f[0] === 'Url' && Number(item.Kind || 1) === 1))) input.attr('data-hlg-image', 'true');
                    });
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
