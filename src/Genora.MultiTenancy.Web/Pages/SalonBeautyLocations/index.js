$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    function resolveSalonService(name) {
        var root = genora.multiTenancy.appServices && genora.multiTenancy.appServices.salonBeauties;
        if (!root || !root[name]) {
            throw new Error('Salon Beauty application service proxy not found: genora.multiTenancy.appServices.salonBeauties.' + name);
        }
        return root[name];
    }

    var service = resolveSalonService('salonBeautyLocation');

    var createModal = new abp.ModalManager('/SalonBeautyLocations/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyLocations/EditModal');
    var detailModal = new abp.ModalManager('/SalonBeautyLocations/DetailModal');
    var canEdit = $('#CanEditSalonLocation').val() === 'true';
    var canDelete = $('#CanDeleteSalonLocation').val() === 'true';

    function htmlEncode(value) {
        return $('<div/>').text(value || '').html();
    }

    function imageUrlOrFallback(row) {
        return row.imageUrl || '/images/getting-started/no-photo-square.png';
    }

    function parseBool(val) {
        val = (val || '').toString().trim().toLowerCase();
        if (val === 'true') return true;
        if (val === 'false') return false;
        return null;
    }

    function buildListInput(request) {
        request = request || {};
        var length = request.length || 10;
        return {
            filterText: ($('#SalonLocationKeywordFilter').val() || '').trim() || null,
            isActive: parseBool($('#SalonLocationStatusFilter').val()),
            isShowOnApp: parseBool($('#SalonLocationShowOnAppFilter').val()),
            skipCount: request.start || 0,
            maxResultCount: length > 100 ? 100 : length,
            sorting: 'sortOrder asc, name asc'
        };
    }

    function renderLocationInfo(data, type, row) {
        return '<div class="location-info-cell">'
            + '<img src="' + htmlEncode(imageUrlOrFallback(row)) + '" alt="" />'
            + '<div class="location-info-text">'
            + '<strong>' + htmlEncode(row.name) + '</strong>'
            + '<span>' + htmlEncode(row.address || '') + '</span>'
            + '</div>'
            + '</div>';
    }

    function renderContact(data, type, row) {
        var phone = row.phone ? '<i class="fa fa-phone text-muted"></i> ' + htmlEncode(row.phone) : '<span class="text-muted">--</span>';
        return '<div class="location-contact-cell">' + phone + '</div>';
    }

    function renderHours(data, type, row) {
        var open = row.openTimeText || row.openTime || '';
        var close = row.closeTimeText || row.closeTime || '';
        return '<span class="location-hours-cell"><i class="fa fa-clock-o"></i>' + htmlEncode(open + ' - ' + close) + '</span>';
    }

    function renderStatus(data, type, row) {
        if (!canEdit) {
            var text = row.isActiveText || (row.isActive ? l('SalonBeautyLocations:StatusActive') : l('SalonBeautyLocations:StatusInactive'));
            return '<span class="location-status-dot ' + (row.isActive ? 'active' : 'inactive') + '">' + htmlEncode(text) + '</span>';
        }
        var checked = row.isActive ? 'checked' : '';
        return '<label class="location-inline-switch" data-location-id="' + row.id + '">'
            + '<input type="checkbox" class="location-inline-status-toggle" ' + checked + ' />'
            + '<span class="location-switch-slider"></span>'
            + '</label>';
    }

    function renderShowOnApp(data, type, row) {
        if (!canEdit) {
            return row.isShowOnApp
                ? '<span class="location-badge active">' + htmlEncode(row.isShowOnAppText || l('Yes')) + '</span>'
                : '<span class="location-badge inactive">' + htmlEncode(row.isShowOnAppText || l('No')) + '</span>';
        }
        var checked = row.isShowOnApp ? 'checked' : '';
        return '<label class="location-inline-switch" data-location-id="' + row.id + '">'
            + '<input type="checkbox" class="location-inline-showonapp-toggle" ' + checked + ' />'
            + '<span class="location-switch-slider"></span>'
            + '</label>';
    }

    var dataTable = $('#SalonLocationsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [],
            ajax: abp.libs.datatables.createAjax(service.getList, buildListInput),
            pageLength: 10,
            lengthMenu: [10, 25, 50, 100],
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('ViewDetails'),
                                action: function (data) { detailModal.open({ id: data.record.id }); }
                            },
                            {
                                text: l('Edit'),
                                visible: function () { return canEdit; },
                                action: function (data) { editModal.open({ id: data.record.id }); }
                            },
                            {
                                text: l('Delete'),
                                visible: function () { return canDelete; },
                                confirmMessage: function (data) {
                                    return abp.utils.formatString(l('SalonBeautyLocations:DeleteConfirm'), data.record.name);
                                },
                                action: function (data) {
                                    service.delete(data.record.id).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            }
                        ]
                    },
                    width: '120px'
                },
                { title: l('SalonBeautyLocations:ColumnInfo'), data: 'name', render: renderLocationInfo, width: '320px' },
                { title: l('SalonBeautyLocations:ColumnContact'), data: 'phone', render: renderContact, width: '180px' },
                { title: l('SalonBeautyLocations:ColumnHours'), data: 'openTime', render: renderHours, width: '180px' },
                { title: l('SalonBeautyLocations:Status'), data: 'isActive', render: renderStatus, orderable: false, width: '120px' },
                { title: l('SalonBeautyLocations:IsShowOnApp'), data: 'isShowOnApp', render: renderShowOnApp, orderable: false, width: '140px' }
            ]
        })
    );

    $('#SearchSalonLocationButton').on('click', function (e) {
        e.preventDefault();
        dataTable.ajax.reload(null, true);
    });

    $('#SalonLocationKeywordFilter').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload(null, true);
        }
    });

    $('#SalonLocationStatusFilter,#SalonLocationShowOnAppFilter').on('change', function () {
        dataTable.ajax.reload(null, true);
    });

    $('#NewSalonLocationButton').on('click', function (e) {
        e.preventDefault();
        createModal.open();
    });

    function markFormAsClean($form) {
        if (!$form || !$form.length) return;
        $form.find('input, textarea, select').each(function () {
            try {
                if (this.type === 'checkbox' || this.type === 'radio') {
                    this.defaultChecked = this.checked;
                } else if (this.tagName && this.tagName.toLowerCase() === 'select') {
                    $(this).find('option').each(function () { this.defaultSelected = this.selected; });
                } else {
                    this.defaultValue = this.value;
                }
            } catch (e) { }
        });
        try { $form.removeClass('dirty was-validated'); } catch (e) { }
        try { $form.data('dirty', false).data('isDirty', false).data('changed', false); } catch (e) { }
        try { $form.attr('data-dirty', 'false').attr('data-is-dirty', 'false'); } catch (e) { }
    }

    function suppressUnsavedChangesConfirmTemporarily(milliseconds) {
        if (!abp || !abp.message || !abp.message.confirm) return function () { };
        var originalConfirm = abp.message.confirm;
        var restored = false;
        var timeout = milliseconds || 3000;
        abp.message.confirm = function (message, title, callback) {
            var msg = (message || '').toString().toLowerCase();
            var ttl = (title || '').toString().toLowerCase();
            var isUnsavedChangesConfirm =
                msg.indexOf('chưa được lưu') >= 0 || msg.indexOf('unsaved') >= 0 ||
                msg.indexOf('not saved') >= 0 || ttl.indexOf('bạn có chắc') >= 0 ||
                ttl.indexOf('are you sure') >= 0;
            if (isUnsavedChangesConfirm) {
                if ($.isFunction(callback)) callback(true);
                if ($.Deferred) return $.Deferred().resolve(true).promise();
                return Promise.resolve(true);
            }
            return originalConfirm.apply(this, arguments);
        };
        var restore = function () { if (restored) return; restored = true; if (abp && abp.message) abp.message.confirm = originalConfirm; };
        setTimeout(restore, timeout);
        return restore;
    }

    function findFormFromModal($modal) {
        var $form = $modal.find('form.location-form');
        if (!$form.length) $form = $modal.closest('form.location-form');
        return $form;
    }

    function findModal($form) {
        if (!$form || !$form.length) return $('.modal.show').has('.location-modal').last();
        var $modal = $form.closest('.modal');
        if (!$modal.length) $modal = $form.find('.modal').first();
        return $modal;
    }

    function cleanupModalDom($form) {
        var $modal = findModal($form);
        if ($form && $form.length) $form.remove();
        if ($modal && $modal.length) {
            var $modalForm = $modal.closest('form.location-form');
            if ($modalForm.length) $modalForm.remove();
            else $modal.remove();
        }
        $('.modal').filter(function () { return $(this).find('.location-modal').length > 0; }).remove();
        $('form.location-form').filter(function () { return $(this).find('.location-modal').length > 0; }).remove();
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open').css({ overflow: '', paddingRight: '' })
            .removeAttr('data-bs-overflow').removeAttr('data-bs-padding-right');
        if (dataTable) dataTable.columns.adjust();
    }

    function closeModal($form) {
        var $modal = findModal($form);
        markFormAsClean($form);
        suppressUnsavedChangesConfirmTemporarily(3000);
        if ($modal && $modal.length) {
            try { $modal.off('hide.bs.modal'); } catch (e) { }
            try { $modal.removeClass('show').hide().attr('aria-hidden', 'true').removeAttr('aria-modal role'); } catch (e) { }
        }
        setTimeout(function () { cleanupModalDom($form); }, 30);
    }

    function getFieldValue($form, name) {
        var $field = $form.find('[name="' + name + '"]');
        return $.trim(($field.val() || '').toString());
    }

    function getHiddenValue($form, suffix) {
        var $field = $form.find('input[type="hidden"][name$=".' + suffix + '"]');
        return $.trim(($field.val() || '').toString());
    }

    function getImageState($form) {
        var uploadMode = $form.find('.location-is-upload-image').val() === 'true';
        var imageUrl = getHiddenValue($form, 'ImageUrl');
        var fileInput = $form.find('.location-image-file-input')[0];
        var hasFile = !!(fileInput && fileInput.files && fileInput.files.length > 0);
        return { uploadMode: uploadMode, imageUrl: imageUrl, hasFile: hasFile };
    }

    function syncImageUrlFromText($form) {
        var url = $.trim(($form.find('.location-image-url-text').val() || '').toString());
        $form.find('.location-image-url-input').val(url);
    }

    function normalizePhoneInput($input) {
        var value = ($input.val() || '').replace(/\D/g, '').substring(0, 11);
        $input.val(value);
    }

    function syncTimeFields($form) {
        var open = ($form.find('.location-open-time').val() || '08:00').trim();
        var close = ($form.find('.location-close-time').val() || '21:00').trim();
        if (!/^\d{2}:\d{2}$/.test(open)) open = '08:00';
        if (!/^\d{2}:\d{2}$/.test(close)) close = '21:00';
        $form.find('.location-open-time-value').val(open + ':00');
        $form.find('.location-close-time-value').val(close + ':00');
        return { open: open, close: close };
    }

    function updateStatusSwitch($toggle) {
        var $form = $toggle.closest('form');
        var active = $toggle.is(':checked');
        $form.find('.location-status-value').val(active ? 'true' : 'false');
        var $text = $form.find('.location-status-toggle-text');
        $text.text(active ? $text.data('active') : $text.data('inactive'));
    }

    function updateShowOnAppSwitch($toggle) {
        var $form = $toggle.closest('form');
        var active = $toggle.is(':checked');
        $form.find('.location-showonapp-value').val(active ? 'true' : 'false');
        var $text = $form.find('.location-showonapp-toggle-text');
        $text.text(active ? $text.data('active') : $text.data('inactive'));
    }

    function isFormValid($form, showWarning) {
        var name = getFieldValue($form, 'Location.Name');
        var address = getFieldValue($form, 'Location.Address');
        var phone = getFieldValue($form, 'Location.Phone');
        var times = syncTimeFields($form);
        var isActive = getHiddenValue($form, 'IsActive') === 'true';
        var isShowOnApp = getHiddenValue($form, 'IsShowOnApp') === 'true';

        var valid = true;
        var message = null;

        if (!name) { valid = false; message = l('SalonBeautyLocations:NameRequired'); }
        else if (!address) { valid = false; message = l('SalonBeautyLocations:AddressRequired'); }
        else if (phone && !/^0\d{9,10}$/.test(phone)) { valid = false; message = l('SalonBeautyLocations:PhoneInvalid'); }
        else if (times.open >= times.close) { valid = false; message = l('SalonBeautyLocations:OpenCloseInvalid'); }
        else if (isShowOnApp && !isActive) { valid = false; message = l('SalonBeautyLocations:ShowOnAppRequiresActive'); }

        if (!valid && showWarning && message) abp.notify.warn(message);
        return valid;
    }

    function updateSubmitState($form) {
        if (!$form || !$form.length) return;
        var valid = isFormValid($form, false);
        $form.find('.location-submit-button').prop('disabled', !valid).toggleClass('disabled', !valid);
    }

    $(document).on('input keyup paste', '.location-phone-input', function () {
        var $input = $(this);
        setTimeout(function () { normalizePhoneInput($input); updateSubmitState($input.closest('form')); }, 0);
    });

    $(document).on('input keyup change paste', '.location-form input, .location-form select, .location-form textarea', function () {
        var $form = $(this).closest('form');
        setTimeout(function () { updateSubmitState($form); }, 0);
    });

    $(document).on('change', '.location-status-toggle', function () {
        updateStatusSwitch($(this));
        updateSubmitState($(this).closest('form'));
    });

    $(document).on('change', '.location-showonapp-toggle', function () {
        updateShowOnAppSwitch($(this));
        updateSubmitState($(this).closest('form'));
    });

    function previewImage($form, src, hintText) {
        var $img = $form.find('.location-image-preview-img');
        if ($img.length) $img.attr('src', src || '/images/getting-started/no-photo-square.png').show();
        if (hintText) $form.find('.location-image-file-hint').text(hintText);
    }

    function setImageMode($form, mode) {
        var uploadMode = mode === 'upload';
        $form.find('.location-is-upload-image').val(uploadMode ? 'true' : 'false');
        $form.find('.location-image-url-panel').toggle(!uploadMode);
        $form.find('.location-image-upload-panel').toggle(uploadMode);
        $form.find('.location-image-mode').removeClass('active');
        $form.find('.location-image-mode input[value="' + mode + '"]').prop('checked', true).closest('.location-image-mode').addClass('active');
        if (!uploadMode) {
            var $file = $form.find('.location-image-file-input');
            if ($file.length) $file.val('');
            syncImageUrlFromText($form);
            var url = getHiddenValue($form, 'ImageUrl');
            if (url) previewImage($form, url, l('SalonBeautyLocations:ImagePreviewFromUrl'));
            else previewImage($form, '/images/getting-started/no-photo-square.png', l('SalonBeautyLocations:ImageNoFile'));
        }
        updateSubmitState($form);
    }

    function initializeImageUpload($form) {
        if (!$form || !$form.length || $form.data('image-initialized')) return;
        $form.data('image-initialized', true);
        var currentUrl = $.trim(($form.find('.location-image-url-input').val() || '').toString());
        $form.find('.location-image-url-text').val(currentUrl);
        previewImage($form, currentUrl || '/images/getting-started/no-photo-square.png', currentUrl ? l('SalonBeautyLocations:ImagePreviewFromUrl') : l('SalonBeautyLocations:ImageNoFile'));
        setImageMode($form, 'url');
    }

    function initializeForm($form) {
        if (!$form || !$form.length) return;
        $form.find('.location-phone-input').each(function () { normalizePhoneInput($(this)); });
        $form.find('.location-status-toggle').each(function () { updateStatusSwitch($(this)); });
        $form.find('.location-showonapp-toggle').each(function () { updateShowOnAppSwitch($(this)); });
        syncTimeFields($form);
        initializeImageUpload($form);
        updateSubmitState($form);
        setTimeout(function () { updateSubmitState($form); }, 150);
    }

    $(document).on('shown.bs.modal', '.modal', function () {
        initializeForm(findFormFromModal($(this)));
    });

    $(document).on('change', '.location-image-mode input', function () {
        var $input = $(this);
        setImageMode($input.closest('form'), $input.val());
    });

    $(document).on('input keyup paste change', '.location-image-url-text', function () {
        var $input = $(this);
        var $form = $input.closest('form');
        syncImageUrlFromText($form);
        var url = $.trim(($input.val() || '').toString());
        previewImage($form, url || '/images/getting-started/no-photo-square.png', url ? l('SalonBeautyLocations:ImagePreviewFromUrl') : l('SalonBeautyLocations:ImageNoFile'));
        updateSubmitState($form);
    });

    $(document).on('click keydown', '.location-image-drop-zone', function (e) {
        if ($(e.target).closest('.location-image-file-input').length) return;
        if (e.type === 'keydown' && e.key !== 'Enter' && e.key !== ' ') return;
        e.preventDefault();
        e.stopPropagation();
        var input = $(this).find('.location-image-file-input')[0];
        if (input) input.click();
    });

    $(document).on('click', '.location-image-file-input', function (e) {
        e.stopPropagation();
    });

    $(document).on('dragover', '.location-image-drop-zone', function (e) {
        e.preventDefault();
        $(this).addClass('drag-over');
    });

    $(document).on('dragleave drop', '.location-image-drop-zone', function (e) {
        e.preventDefault();
        $(this).removeClass('drag-over');
    });

    $(document).on('drop', '.location-image-drop-zone', function (e) {
        var files = e.originalEvent && e.originalEvent.dataTransfer ? e.originalEvent.dataTransfer.files : null;
        if (!files || !files.length) return;
        var input = $(this).find('.location-image-file-input')[0];
        if (!input) return;
        try { input.files = files; } catch (ex) { }
        $(input).trigger('change');
    });

    $(document).on('change', '.location-image-file-input', function () {
        var $input = $(this);
        var $form = $input.closest('form');
        var file = this.files && this.files.length ? this.files[0] : null;
        if (!file) { updateSubmitState($form); return; }
        if (file.size > 2 * 1024 * 1024) {
            abp.notify.warn(l('SalonBeautyLocations:ImageMaxSize'));
            $input.val('');
            previewImage($form, '/images/getting-started/no-photo-square.png', l('SalonBeautyLocations:ImageNoFile'));
            updateSubmitState($form);
            return;
        }
        if (!file.type || !file.type.match(/^image\//)) {
            abp.notify.warn(l('SalonBeautyLocations:ImageInvalidType'));
            $input.val('');
            previewImage($form, '/images/getting-started/no-photo-square.png', l('SalonBeautyLocations:ImageNoFile'));
            updateSubmitState($form);
            return;
        }
        var reader = new FileReader();
        reader.onload = function (e) { previewImage($form, e.target.result, file.name); updateSubmitState($form); };
        reader.readAsDataURL(file);
    });

    function submitForm($form) {
        $form.find('.location-phone-input').each(function () { normalizePhoneInput($(this)); });
        $form.find('.location-status-toggle').each(function () { updateStatusSwitch($(this)); });
        $form.find('.location-showonapp-toggle').each(function () { updateShowOnAppSwitch($(this)); });
        syncTimeFields($form);

        if (!isFormValid($form, true)) {
            updateSubmitState($form);
            return false;
        }

        var $submit = $form.find('.location-submit-button');
        if ($submit.prop('disabled')) return false;
        $submit.prop('disabled', true).addClass('disabled');

        syncImageUrlFromText($form);
        var formData = new FormData($form[0]);

        abp.ajax({
            type: 'POST',
            url: $form.attr('action'),
            data: formData,
            processData: false,
            contentType: false
        }).done(function () {
            closeModal($form);
            abp.notify.success(l('SavedSuccessfully'));
            dataTable.ajax.reload(function () { dataTable.columns.adjust(); }, false);
        }).always(function () {
            updateSubmitState($form);
        });

        return false;
    }

    $(document).on('click', '.location-submit-button', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return submitForm($(this).closest('form'));
    });

    $(document).on('submit', '.location-form', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return submitForm($(this));
    });

    $(document).on('change', '.location-inline-status-toggle', function () {
        var $toggle = $(this);
        var $label = $toggle.closest('.location-inline-switch');
        var locationId = $label.data('location-id');
        var newValue = $toggle.is(':checked');

        service.updateActive(locationId, newValue).then(function () {
            abp.notify.success(l('SavedSuccessfully'));
            dataTable.ajax.reload(null, false);
        }).catch(function (error) {
            $toggle.prop('checked', !newValue);
            var message = (error && (error.message || (error.responseJSON && error.responseJSON.error && error.responseJSON.error.message))) || l('SalonBeautyLocations:UpdateStatusFailed');
            abp.notify.error(message);
        });
    });

    $(document).on('change', '.location-inline-showonapp-toggle', function () {
        var $toggle = $(this);
        var $label = $toggle.closest('.location-inline-switch');
        var locationId = $label.data('location-id');
        var newValue = $toggle.is(':checked');

        service.updateShowOnApp(locationId, newValue).then(function () {
            abp.notify.success(l('SavedSuccessfully'));
            dataTable.ajax.reload(null, false);
        }).catch(function (error) {
            $toggle.prop('checked', !newValue);
            var message = (error && (error.message || (error.responseJSON && error.responseJSON.error && error.responseJSON.error.message))) || l('SalonBeautyLocations:UpdateShowOnAppFailed');
            abp.notify.error(message);
        });
    });
});
