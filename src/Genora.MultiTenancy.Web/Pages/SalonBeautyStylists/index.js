$(function () {
    var l = abp.localization.getResource('MultiTenancy');


    function resolveSalonService(name) {
        var root = genora.multiTenancy.appServices && genora.multiTenancy.appServices.salonBeauties;
        if (!root || !root[name]) {
            throw new Error('Salon Beauty application service proxy not found: genora.multiTenancy.appServices.salonBeauties.' + name);
        }
        return root[name];
    }

    var service = resolveSalonService('salonBeautyStylist');

    var createModal = new abp.ModalManager('/SalonBeautyStylists/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyStylists/EditModal');
    var detailModal = new abp.ModalManager('/SalonBeautyStylists/DetailModal');
    var canEdit = $('#CanEditSalonStylist').val() === 'true';
    var canDelete = $('#CanDeleteSalonStylist').val() === 'true';

    function markSalonFormAsClean($form) {
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
                msg.indexOf('chưa được lưu') >= 0 ||
                msg.indexOf('unsaved') >= 0 ||
                msg.indexOf('not saved') >= 0 ||
                ttl.indexOf('bạn có chắc') >= 0 ||
                ttl.indexOf('are you sure') >= 0;

            if (isUnsavedChangesConfirm) {
                if ($.isFunction(callback)) callback(true);
                if ($.Deferred) return $.Deferred().resolve(true).promise();
                return Promise.resolve(true);
            }

            return originalConfirm.apply(this, arguments);
        };

        var restore = function () {
            if (restored) return;
            restored = true;
            if (abp && abp.message) abp.message.confirm = originalConfirm;
        };

        setTimeout(restore, timeout);
        return restore;
    }

    function findSalonFormFromModal($modal) {
        var $form = $modal.find('form.stylist-form');
        if (!$form.length) $form = $modal.closest('form.stylist-form');
        if (!$form.length) $form = $('form.stylist-form').filter(function () { return $(this).find('.stylist-modal').length > 0; }).last();
        return $form;
    }

    function findSalonModal($form) {
        if (!$form || !$form.length) return $('.modal.show').has('.stylist-modal').last();

        var $modal = $form.closest('.modal');
        if (!$modal.length) $modal = $form.find('.modal').first();
        if (!$modal.length) $modal = $('.modal.show').filter(function () { return $(this).find('.stylist-modal').length > 0; }).last();
        return $modal;
    }

    function cleanupSalonModalDom($form) {
        var $modal = findSalonModal($form);

        if ($form && $form.length) $form.remove();

        if ($modal && $modal.length) {
            var $modalForm = $modal.closest('form.stylist-form');
            if ($modalForm.length) $modalForm.remove();
            else $modal.remove();
        }

        $('.modal').filter(function () { return $(this).find('.stylist-modal').length > 0; }).remove();
        $('form.stylist-form').filter(function () { return $(this).find('.stylist-modal').length > 0; }).remove();
        $('.modal-backdrop').remove();
        $('body')
            .removeClass('modal-open')
            .css({ overflow: '', paddingRight: '' })
            .removeAttr('data-bs-overflow')
            .removeAttr('data-bs-padding-right');

        if (dataTable) dataTable.columns.adjust();
    }

    function closeSalonModal($form) {
        var $modal = findSalonModal($form);
        markSalonFormAsClean($form);
        suppressUnsavedChangesConfirmTemporarily(3000);

        if ($modal && $modal.length) {
            try { $modal.off('hide.bs.modal'); } catch (e) { }
            try { $modal.removeClass('show').hide().attr('aria-hidden', 'true').removeAttr('aria-modal role'); } catch (e) { }
        }

        setTimeout(function () { cleanupSalonModalDom($form); }, 30);
    }

    function parseNullableByte(val) {
        val = (val || '').toString().trim();
        return val === '' ? null : parseInt(val, 10);
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
        var locationId = ($('#SalonStylistLocationFilter').val() || '').trim() || null;
        return {
            filterText: ($('#SalonStylistKeywordFilter').val() || '').trim() || null,
            locationId: locationId,
            level: parseNullableByte($('#SalonStylistLevelFilter').val()),
            role: parseNullableByte($('#SalonStylistRoleFilter').val()),
            status: parseNullableByte($('#SalonStylistStatusFilter').val()),
            isShowOnApp: parseBool($('#SalonStylistShowOnAppFilter').val()),
            skipCount: request.start || 0,
            maxResultCount: length > 100 ? 100 : length,
            sorting: 'sortOrder asc, displayName asc'
        };
    }

    function htmlEncode(value) {
        return $('<div/>').text(value || '').html();
    }

    function avatarUrl(row) {
        return row.avatar || '/images/getting-started/no-photo-square.png';
    }

    function badgeClass(text, fallback) {
        var s = (text || '').toString().toLowerCase();
        if (s.indexOf('master') >= 0 || s.indexOf('level 5') >= 0 || s.indexOf('bậc') >= 0) return 'master';
        if (s.indexOf('senior') >= 0 || s.indexOf('level 4') >= 0 || s.indexOf('level 3') >= 0) return 'senior';
        if (s.indexOf('manager') >= 0 || s.indexOf('quản') >= 0) return 'manager';
        if (s.indexOf('hair') >= 0 || s.indexOf('tóc') >= 0) return 'hair-stylist';
        if (s.indexOf('tech') >= 0 || s.indexOf('kỹ') >= 0) return 'technician';
        return fallback || 'junior';
    }

    function renderStylist(data, type, row) {
        var idText = row.code || row.stylistCode || (row.id ? row.id.substring(0, 8) : '');
        return '<div class="stylist-info-cell">'
            + '<img src="' + htmlEncode(avatarUrl(row)) + '" alt="" />'
            + '<div class="stylist-info-text"><strong>' + htmlEncode(row.displayName) + '</strong>'
            + '<span>ID: ' + htmlEncode(idText) + '</span></div>'
            + '</div>';
    }

    function renderLevel(data, type, row) {
        var text = row.levelText || (row.level ? ('Level ' + row.level) : '--');
        if (!row.level) return '--';
        return '<span class="stylist-badge ' + badgeClass(text, 'junior') + '">' + htmlEncode(text) + '</span>';
    }

    function renderRole(data, type, row) {
        var text = row.roleText || '--';
        if (!row.role) return '--';
        return '<span class="stylist-badge ' + badgeClass(text, 'hair-stylist') + '">' + htmlEncode(text) + '</span>';
    }

    function renderGender(data, type, row) {
        return htmlEncode(row.genderText || '--');
    }

    function renderStatus(data, type, row) {
        var active = row.status === 1;
        if (!canEdit) {
            var text = row.statusText || (active ? l('SalonBeautyCustomer:StatusActive') : l('SalonBeautyCustomer:StatusInactive'));
            return '<span class="stylist-status-dot ' + (active ? 'active' : 'inactive') + '">' + htmlEncode(text) + '</span>';
        }

        var checked = active ? 'checked' : '';
        return '<label class="stylist-inline-switch" data-stylist-id="' + row.id + '">'
            + '<input type="checkbox" class="stylist-inline-status-toggle" ' + checked + ' />'
            + '<span class="stylist-switch-slider"></span>'
            + '</label>';
    }

    function renderShowOnApp(data, type, row) {
        if (!canEdit) {
            return row.isShowOnApp
                ? '<span class="stylist-badge active">' + htmlEncode(row.isShowOnAppText || l('Yes')) + '</span>'
                : '<span class="stylist-badge inactive">' + htmlEncode(row.isShowOnAppText || l('No')) + '</span>';
        }

        var checked = row.isShowOnApp ? 'checked' : '';
        return '<label class="stylist-inline-switch" data-stylist-id="' + row.id + '">'
            + '<input type="checkbox" class="stylist-inline-showonapp-toggle" ' + checked + ' />'
            + '<span class="stylist-switch-slider"></span>'
            + '</label>';
    }

    var dataTable = $('#SalonStylistsTable').DataTable(
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
                                    return abp.utils.formatString(l('SalonBeautyStylists:DeleteConfirm'), data.record.displayName);
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
                { title: l('SalonBeautyStylists:ColumnStylistInfo'), data: 'displayName', render: renderStylist, width: '260px' },
                { title: l('SalonBeautyLocation:PageTitle'), data: 'locationName', render: function (data, type, row) { return htmlEncode(row.locationName || '--'); }, width: '160px' },
                { title: l('SalonBeautyStylist:Level'), data: 'level', render: renderLevel, width: '120px' },
                { title: l('SalonBeautyStylist:Gender'), data: 'gender', render: renderGender, width: '100px' },
                { title: l('SalonBeautyStylist:ExperienceYear'), data: 'experienceYear', render: function (data) { return htmlEncode((data || 0) + ' ' + l('SalonBeautyStylists:YearsUnit')); }, width: '130px' },
                { title: l('SalonBeautyStylist:Role'), data: 'role', render: renderRole, width: '150px' },
                { title: l('SalonBeautyStylist:Status'), data: 'status', render: renderStatus, width: '160px' },
                { title: l('SalonBeautyStylist:IsShowOnApp'), data: 'isShowOnApp', render: renderShowOnApp, orderable: false, width: '140px' }
            ]
        })
    );

    $('#SearchSalonStylistButton').on('click', function (e) {
        e.preventDefault();
        dataTable.ajax.reload(null, true);
    });

    $('#SalonStylistKeywordFilter').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload(null, true);
        }
    });

    $('#SalonStylistLocationFilter,#SalonStylistLevelFilter,#SalonStylistRoleFilter,#SalonStylistStatusFilter,#SalonStylistShowOnAppFilter').on('change', function () {
        dataTable.ajax.reload(null, true);
    });

    $('#NewSalonStylistButton').on('click', function (e) {
        e.preventDefault();
        createModal.open();
    });

    function normalizePhoneInput($input) {
        var value = ($input.val() || '').replace(/\D/g, '').substring(0, 11);
        $input.val(value);
    }

    function updateStatusSwitch($toggle) {
        var $box = $toggle.closest('.stylist-status-box');
        var active = $toggle.is(':checked');
        $box.find('.stylist-status-value').val(active ? '1' : '0');
        var $text = $box.find('.stylist-status-toggle-text');
        $text.text(active ? $text.data('active') : $text.data('inactive'));
    }

    function updateShowOnAppSwitch($toggle) {
        var $box = $toggle.closest('.stylist-status-box');
        var active = $toggle.is(':checked');
        $box.find('.stylist-showonapp-value').val(active ? 'true' : 'false');
        var $text = $box.find('.stylist-showonapp-toggle-text');
        $text.text(active ? $text.data('active') : $text.data('inactive'));
    }

    function getFieldValue($form, name, id) {
        var $field = $form.find('[name="' + name + '"]');
        if (!$field.length && id) $field = $form.find('#' + id);
        return $.trim(($field.val() || '').toString());
    }

    function getHiddenFieldValue($form, suffix) {
        var $field = $form.find('input[type="hidden"][name$=".' + suffix + '"]');
        return $.trim(($field.val() || '').toString());
    }

    function getAvatarState($form) {
        var uploadMode = $form.find('.stylist-is-upload-image').val() === 'true';
        var avatarUrl = getHiddenFieldValue($form, 'Avatar');
        var fileInput = $form.find('.stylist-avatar-file-input')[0];
        var hasFile = !!(fileInput && fileInput.files && fileInput.files.length > 0);
        return { uploadMode: uploadMode, avatarUrl: avatarUrl, hasFile: hasFile };
    }

    function syncAvatarUrlFromText($form) {
        var url = $.trim(($form.find('.stylist-avatar-url-text').val() || '').toString());
        $form.find('.stylist-avatar-url-input').val(url);
    }

    function isStylistFormValid($form, showWarning) {
        var displayName = getFieldValue($form, 'Stylist.DisplayName', 'Stylist_DisplayName');
        var phone = getFieldValue($form, 'Stylist.Phone', 'Stylist_Phone');
        var role = getFieldValue($form, 'Stylist.Role', 'Stylist_Role');
        var level = getFieldValue($form, 'Stylist.Level', 'Stylist_Level');
        var isShowOnApp = getHiddenFieldValue($form, 'IsShowOnApp') === 'true';
        var status = getHiddenFieldValue($form, 'Status');
        syncAvatarUrlFromText($form);
        var avatarState = getAvatarState($form);

        var valid = true;
        var message = null;

        if (!displayName) { valid = false; message = l('SalonBeautyStylists:DisplayNameRequired'); }
        else if (phone && !/^0\d{9,10}$/.test(phone)) { valid = false; message = l('SalonBeautyStylists:PhoneInvalid'); }
        else if (!role) { valid = false; message = l('SalonBeautyStylists:RoleRequired'); }
        else if (!level) { valid = false; message = l('SalonBeautyStylists:LevelRequired'); }
        else if (isShowOnApp && status !== '1') { valid = false; message = l('SalonBeautyStylists:ShowOnAppRequiresActive'); }
        else if (isShowOnApp && !avatarState.avatarUrl && !avatarState.hasFile) { valid = false; message = l('SalonBeautyStylists:ShowOnAppRequiresAvatar'); }

        if (!valid && showWarning && message) abp.notify.warn(message);
        return valid;
    }

    function updateSubmitState($form) {
        if (!$form || !$form.length) return;
        var valid = isStylistFormValid($form, false);
        $form.find('.stylist-submit-button').prop('disabled', !valid).toggleClass('disabled', !valid);
    }

    $(document).on('input keyup paste', '.stylist-phone-input', function () {
        var $input = $(this);
        setTimeout(function () {
            normalizePhoneInput($input);
            updateSubmitState($input.closest('form'));
        }, 0);
    });

    $(document).on('input keyup change paste', '.stylist-form input, .stylist-form select, .stylist-form textarea', function () {
        var $form = $(this).closest('form');
        setTimeout(function () { updateSubmitState($form); }, 0);
    });

    $(document).on('change', '.stylist-status-toggle', function () {
        updateStatusSwitch($(this));
        updateSubmitState($(this).closest('form'));
    });

    $(document).on('change', '.stylist-showonapp-toggle', function () {
        updateShowOnAppSwitch($(this));
        updateSubmitState($(this).closest('form'));
    });

    function submitSalonStylistForm($form) {
        $form.find('.stylist-phone-input').each(function () { normalizePhoneInput($(this)); });
        $form.find('.stylist-status-toggle').each(function () { updateStatusSwitch($(this)); });
        $form.find('.stylist-showonapp-toggle').each(function () { updateShowOnAppSwitch($(this)); });

        if (!isStylistFormValid($form, true)) {
            updateSubmitState($form);
            return false;
        }

        var $submitButton = $form.find('.stylist-submit-button');
        if ($submitButton.prop('disabled')) return false;

        $submitButton.prop('disabled', true).addClass('disabled');

        syncAvatarUrlFromText($form);
        var formData = new FormData($form[0]);

        abp.ajax({
            type: 'POST',
            url: $form.attr('action'),
            data: formData,
            processData: false,
            contentType: false
        }).done(function () {
            closeSalonModal($form);
            abp.notify.success(l('SavedSuccessfully'));
            dataTable.ajax.reload(function () { dataTable.columns.adjust(); }, false);
        }).always(function () {
            updateSubmitState($form);
        });

        return false;
    }

    $(document).on('click', '.stylist-submit-button', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return submitSalonStylistForm($(this).closest('form'));
    });

    $(document).on('submit', '.stylist-form', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return submitSalonStylistForm($(this));
    });

    function initializeSalonStylistForm($form) {
        if (!$form || !$form.length) return;
        $form.find('.stylist-phone-input').each(function () { normalizePhoneInput($(this)); });
        $form.find('.stylist-status-toggle').each(function () { updateStatusSwitch($(this)); });
        $form.find('.stylist-showonapp-toggle').each(function () { updateShowOnAppSwitch($(this)); });
        initializeAvatarUpload($form);
        updateSubmitState($form);
        setTimeout(function () { updateSubmitState($form); }, 150);
    }

    $(document).on('shown.bs.modal', '.modal', function () {
        initializeSalonStylistForm(findSalonFormFromModal($(this)));
    });

    $(document).on('hidden.bs.modal', '.modal', function () {
        updateSubmitState(findSalonFormFromModal($(this)));
    });

    function previewAvatar($form, src, hintText) {
        var $img = $form.find('.stylist-avatar-preview-img');
        if ($img.length) $img.attr('src', src || '/images/getting-started/no-photo-square.png').show();
        if (hintText) $form.find('.stylist-avatar-file-hint').text(hintText);
    }

    function setAvatarMode($form, mode) {
        var uploadMode = mode === 'upload';
        $form.find('.stylist-is-upload-image').val(uploadMode ? 'true' : 'false');
        $form.find('.stylist-avatar-url-panel').toggle(!uploadMode);
        $form.find('.stylist-avatar-upload-panel').toggle(uploadMode);

        $form.find('.stylist-avatar-mode').removeClass('active');
        $form.find('.stylist-avatar-mode input[value="' + mode + '"]').prop('checked', true).closest('.stylist-avatar-mode').addClass('active');

        if (!uploadMode) {
            var $file = $form.find('.stylist-avatar-file-input');
            if ($file.length) $file.val('');
            syncAvatarUrlFromText($form);
            var url = getHiddenFieldValue($form, 'Avatar');
            if (url) previewAvatar($form, url, l('SalonBeautyStylists:AvatarPreviewFromUrl'));
            else previewAvatar($form, '/images/getting-started/no-photo-square.png', l('SalonBeautyStylists:AvatarNoFile'));
        }

        updateSubmitState($form);
    }

    function initializeAvatarUpload($form) {
        if (!$form || !$form.length || $form.data('avatar-initialized')) return;
        $form.data('avatar-initialized', true);

        var currentUrl = $.trim(($form.find('.stylist-avatar-url-input').val() || '').toString());
        $form.find('.stylist-avatar-url-text').val(currentUrl);
        previewAvatar($form, currentUrl || '/images/getting-started/no-photo-square.png', currentUrl ? l('SalonBeautyStylists:AvatarPreviewFromUrl') : l('SalonBeautyStylists:AvatarNoFile'));
        setAvatarMode($form, 'url');
    }

    $(document).on('change', '.stylist-avatar-mode input', function () {
        var $input = $(this);
        setAvatarMode($input.closest('form'), $input.val());
    });

    $(document).on('input keyup paste change', '.stylist-avatar-url-text', function () {
        var $input = $(this);
        var $form = $input.closest('form');
        syncAvatarUrlFromText($form);
        var url = $.trim(($input.val() || '').toString());
        previewAvatar($form, url || '/images/getting-started/no-photo-square.png', url ? l('SalonBeautyStylists:AvatarPreviewFromUrl') : l('SalonBeautyStylists:AvatarNoFile'));
        updateSubmitState($form);
    });

    // Click vùng upload để mở file picker.
    // Lưu ý: input file đang nằm bên trong drop-zone nên click vào input sẽ bubble ngược lại drop-zone.
    // Nếu dùng $(input).trigger('click') ở đây sẽ tự gọi lặp vô hạn trên jQuery => Maximum call stack size exceeded.
    $(document).on('click keydown', '.stylist-avatar-drop-zone', function (e) {
        if ($(e.target).closest('.stylist-avatar-file-input').length) return;
        if (e.type === 'keydown' && e.key !== 'Enter' && e.key !== ' ') return;

        e.preventDefault();
        e.stopPropagation();

        var input = $(this).find('.stylist-avatar-file-input')[0];
        if (input) input.click();
    });

    $(document).on('click', '.stylist-avatar-file-input', function (e) {
        e.stopPropagation();
    });

    $(document).on('dragover', '.stylist-avatar-drop-zone', function (e) {
        e.preventDefault();
        $(this).addClass('drag-over');
    });

    $(document).on('dragleave drop', '.stylist-avatar-drop-zone', function (e) {
        e.preventDefault();
        $(this).removeClass('drag-over');
    });

    $(document).on('drop', '.stylist-avatar-drop-zone', function (e) {
        var files = e.originalEvent && e.originalEvent.dataTransfer ? e.originalEvent.dataTransfer.files : null;
        if (!files || !files.length) return;
        var input = $(this).find('.stylist-avatar-file-input')[0];
        if (!input) return;
        try {
            input.files = files;
        } catch (ex) { }
        $(input).trigger('change');
    });

    $(document).on('change', '.stylist-avatar-file-input', function () {
        var $input = $(this);
        var $form = $input.closest('form');
        var file = this.files && this.files.length ? this.files[0] : null;

        if (!file) {
            updateSubmitState($form);
            return;
        }

        if (file.size > 2 * 1024 * 1024) {
            abp.notify.warn(l('SalonBeautyStylists:AvatarMaxSize'));
            $input.val('');
            previewAvatar($form, '/images/getting-started/no-photo-square.png', l('SalonBeautyStylists:AvatarNoFile'));
            updateSubmitState($form);
            return;
        }

        if (!file.type || !file.type.match(/^image\//)) {
            abp.notify.warn(l('SalonBeautyStylists:AvatarInvalidType'));
            $input.val('');
            previewAvatar($form, '/images/getting-started/no-photo-square.png', l('SalonBeautyStylists:AvatarNoFile'));
            updateSubmitState($form);
            return;
        }

        var reader = new FileReader();
        reader.onload = function (e) {
            previewAvatar($form, e.target.result, file.name);
            updateSubmitState($form);
        };
        reader.readAsDataURL(file);
    });

    $(document).on('change', '.stylist-inline-status-toggle', function () {
        var $toggle = $(this);
        var $label = $toggle.closest('.stylist-inline-switch');
        var stylistId = $label.data('stylist-id');
        var newStatus = $toggle.is(':checked') ? 1 : 0;

        service.get(stylistId).then(function (stylist) {
            var updateDto = {
                locationId: stylist.locationId,
                displayName: stylist.displayName,
                avatar: stylist.avatar,
                phone: stylist.phone,
                gender: stylist.gender,
                role: stylist.role,
                level: stylist.level,
                experienceYear: stylist.experienceYear,
                status: newStatus,
                isShowOnApp: newStatus === 1 ? stylist.isShowOnApp : false,
                note: stylist.note,
                sortOrder: stylist.sortOrder
            };

            service.update(stylistId, updateDto).then(function () {
                abp.notify.success(l('SavedSuccessfully'));
                dataTable.ajax.reload(null, false);
            }).catch(function (error) {
                $toggle.prop('checked', !$toggle.is(':checked'));
                var message = error?.message || error?.responseJSON?.error?.message || l('SalonBeautyStylists:UpdateShowOnAppFailed');
                abp.notify.error(message);
            });
        }).catch(function () {
            $toggle.prop('checked', !$toggle.is(':checked'));
            abp.notify.error(l('SalonBeautyStylists:UpdateShowOnAppFailed'));
        });
    });

    $(document).on('change', '.stylist-inline-showonapp-toggle', function () {
        var $toggle = $(this);
        var $label = $toggle.closest('.stylist-inline-switch');
        var stylistId = $label.data('stylist-id');
        var newValue = $toggle.is(':checked');

        service.get(stylistId).then(function (stylist) {
            if (newValue && (!stylist.avatar || stylist.status !== 1)) {
                $toggle.prop('checked', !newValue);
                abp.notify.warn(stylist.status !== 1 ? l('SalonBeautyStylists:ShowOnAppRequiresActive') : l('SalonBeautyStylists:ShowOnAppRequiresAvatar'));
                return;
            }

            var updateDto = {
                locationId: stylist.locationId,
                displayName: stylist.displayName,
                avatar: stylist.avatar,
                phone: stylist.phone,
                gender: stylist.gender,
                role: stylist.role,
                level: stylist.level,
                experienceYear: stylist.experienceYear,
                status: stylist.status,
                isShowOnApp: newValue,
                note: stylist.note,
                sortOrder: stylist.sortOrder
            };

            service.update(stylistId, updateDto).then(function () {
                abp.notify.success(l('SavedSuccessfully'));
                dataTable.ajax.reload(null, false);
            }).catch(function () {
                $toggle.prop('checked', !newValue);
                abp.notify.error(l('SalonBeautyStylists:UpdateShowOnAppFailed'));
            });
        }).catch(function () {
            $toggle.prop('checked', !newValue);
            abp.notify.error(l('SalonBeautyStylists:UpdateShowOnAppFailed'));
        });
    });
});
