$(function () {
    var l = abp.localization.getResource('MultiTenancy');


    function resolveSalonService(name) {
        var root = genora.multiTenancy.appServices && genora.multiTenancy.appServices.salonBeauties;
        if (!root || !root[name]) {
            throw new Error('Salon Beauty application service proxy not found: genora.multiTenancy.appServices.salonBeauties.' + name);
        }
        return root[name];
    }

    var service = resolveSalonService('salonBeautyService');

    var createModal = new abp.ModalManager('/SalonBeautyServices/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyServices/EditModal');
    var detailModal = new abp.ModalManager('/SalonBeautyServices/DetailModal');
    var canEdit = $('#CanEditSalonService').val() === 'true';
    var canDelete = $('#CanDeleteSalonService').val() === 'true';

    function markSalonFormAsClean($form) {
        if (!$form || !$form.length) return;
        $form.find('input, textarea, select').each(function () {
            try {
                if (this.type === 'checkbox' || this.type === 'radio') this.defaultChecked = this.checked;
                else if (this.tagName && this.tagName.toLowerCase() === 'select') $(this).find('option').each(function () { this.defaultSelected = this.selected; });
                else this.defaultValue = this.value;
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
        abp.message.confirm = function (message, title, callback) {
            var msg = (message || '').toString().toLowerCase();
            var ttl = (title || '').toString().toLowerCase();
            var isUnsaved = msg.indexOf('chưa được lưu') >= 0 || msg.indexOf('unsaved') >= 0 || msg.indexOf('not saved') >= 0 || ttl.indexOf('bạn có chắc') >= 0 || ttl.indexOf('are you sure') >= 0;
            if (isUnsaved) {
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
        setTimeout(restore, milliseconds || 3000);
        return restore;
    }

    function findSalonFormFromModal($modal) {
        var $form = $modal.find('form.service-form');
        if (!$form.length) $form = $modal.closest('form.service-form');
        if (!$form.length) $form = $('form.service-form').filter(function () { return $(this).find('.service-modal').length > 0; }).last();
        return $form;
    }

    function findSalonModal($form) {
        if (!$form || !$form.length) return $('.modal.show').has('.service-modal').last();
        var $modal = $form.closest('.modal');
        if (!$modal.length) $modal = $form.find('.modal').first();
        if (!$modal.length) $modal = $('.modal.show').filter(function () { return $(this).find('.service-modal').length > 0; }).last();
        return $modal;
    }

    function cleanupSalonModalDom($form) {
        var $modal = findSalonModal($form);
        if ($form && $form.length) $form.remove();
        if ($modal && $modal.length) {
            var $modalForm = $modal.closest('form.service-form');
            if ($modalForm.length) $modalForm.remove();
            else $modal.remove();
        }
        $('.modal').filter(function () { return $(this).find('.service-modal').length > 0; }).remove();
        $('form.service-form').filter(function () { return $(this).find('.service-modal').length > 0; }).remove();
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open').css({ overflow: '', paddingRight: '' }).removeAttr('data-bs-overflow').removeAttr('data-bs-padding-right');
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
        var categoryId = ($('#SalonServiceCategoryFilter').val() || '').trim();
        return {
            filterText: ($('#SalonServiceKeywordFilter').val() || '').trim() || null,
            categoryId: categoryId || null,
            status: parseNullableByte($('#SalonServiceStatusFilter').val()),
            isShowOnApp: parseBool($('#SalonServiceShowOnAppFilter').val()),
            skipCount: request.start || 0,
            maxResultCount: length > 100 ? 100 : length,
            sorting: 'sortOrder asc, name asc'
        };
    }

    function htmlEncode(value) { return $('<div/>').text(value || '').html(); }

    function badgeClass(text, fallback) {
        var s = (text || '').toString().toLowerCase();
        if (s.indexOf('master') >= 0 || s.indexOf('level 5') >= 0 || s.indexOf('bậc') >= 0) return 'master';
        if (s.indexOf('senior') >= 0 || s.indexOf('level 4') >= 0 || s.indexOf('level 3') >= 0) return 'senior';
        if (s.indexOf('hair') >= 0 || s.indexOf('tóc') >= 0) return 'hair-stylist';
        if (s.indexOf('shampoo') >= 0 || s.indexOf('gội') >= 0) return 'technician';
        return fallback || 'junior';
    }

    function renderService(data, type, row) {
        return '<div class="service-info-cell">'
            + '<div class="service-info-icon"><i class="fa fa-spa"></i></div>'
            + '<div class="service-info-text"><strong>' + htmlEncode(row.name) + '</strong>'
            + '<span>ID: ' + htmlEncode(row.id ? row.id.substring(0, 8) : '') + '</span></div>'
            + '</div>';
    }

    function renderCategory(data, type, row) {
        return '<span class="service-badge category">' + htmlEncode(row.categoryName || '--') + '</span>';
    }

    function renderRole(data, type, row) {
        var text = row.applicableRoleText || '--';
        if (!row.applicableRole) return '--';
        return '<span class="service-badge ' + badgeClass(text, 'hair-stylist') + '">' + htmlEncode(text) + '</span>';
    }

    function renderLevel(data, type, row) {
        var text = row.applicableLevelText || '--';
        if (!row.applicableLevel) return '--';
        return '<span class="service-badge ' + badgeClass(text, 'junior') + '">' + htmlEncode(text) + '</span>';
    }

    function renderPrice(data, type, row) {
        return '<strong class="service-price">' + htmlEncode(row.priceText || data || '0') + 'đ</strong>';
    }

    function renderDuration(data, type, row) {
        return htmlEncode(row.durationText || ((data || 0) + ' ' + l('SalonBeautyServices:MinutesUnit')));
    }

    function renderStatus(data, type, row) {
        var active = row.status === 1;
        var text = row.statusText || (active ? l('SalonBeautyCustomer:StatusActive') : l('SalonBeautyCustomer:StatusInactive'));
        return '<span class="service-status-dot ' + (active ? 'active' : 'inactive') + '">' + htmlEncode(text) + '</span>';
    }

    function renderShowOnApp(data, type, row) {
        if (!canEdit) {
            return row.isShowOnApp
                ? '<span class="service-badge active">' + htmlEncode(row.isShowOnAppText || l('Yes')) + '</span>'
                : '<span class="service-badge inactive">' + htmlEncode(row.isShowOnAppText || l('No')) + '</span>';
        }
        var checked = row.isShowOnApp ? 'checked' : '';
        return '<label class="service-inline-switch" data-service-id="' + row.id + '">'
            + '<input type="checkbox" class="service-inline-showonapp-toggle" ' + checked + ' />'
            + '<span class="service-switch-slider"></span>'
            + '</label>';
    }

    var dataTable = $('#SalonServicesTable').DataTable(
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
                            { text: l('ViewDetails'), action: function (data) { detailModal.open({ id: data.record.id }); } },
                            { text: l('Edit'), visible: function () { return canEdit; }, action: function (data) { editModal.open({ id: data.record.id }); } },
                            {
                                text: l('Delete'),
                                visible: function () { return canDelete; },
                                confirmMessage: function (data) { return abp.utils.formatString(l('SalonBeautyServices:DeleteConfirm'), data.record.name); },
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
                { title: l('SalonBeautyService:Category'), data: 'categoryName', render: renderCategory, width: '150px' },
                { title: l('SalonBeautyServices:ColumnServiceInfo'), data: 'name', render: renderService, width: '260px' },
                { title: l('SalonBeautyService:Duration'), data: 'duration', render: renderDuration, width: '120px' },
                { title: l('SalonBeautyService:Price'), data: 'price', render: renderPrice, width: '140px' },
                { title: l('SalonBeautyService:ApplicableRole'), data: 'applicableRole', render: renderRole, width: '150px' },
                { title: l('SalonBeautyService:ApplicableLevel'), data: 'applicableLevel', render: renderLevel, width: '150px' },
                { title: l('SalonBeautyService:Status'), data: 'status', render: renderStatus, width: '160px' },
                { title: l('SalonBeautyService:IsShowOnApp'), data: 'isShowOnApp', render: renderShowOnApp, orderable: false, width: '140px' }
            ]
        })
    );

    $('#SearchSalonServiceButton').on('click', function (e) { e.preventDefault(); dataTable.ajax.reload(null, true); });
    $('#SalonServiceKeywordFilter').on('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); dataTable.ajax.reload(null, true); } });
    $('#SalonServiceCategoryFilter,#SalonServiceStatusFilter,#SalonServiceShowOnAppFilter').on('change', function () { dataTable.ajax.reload(null, true); });
    $('#NewSalonServiceButton').on('click', function (e) { e.preventDefault(); createModal.open(); });

    function updateStatusSwitch($toggle) {
        var $box = $toggle.closest('.service-status-box');
        var active = $toggle.is(':checked');
        $box.find('.service-status-value').val(active ? '1' : '0');
        var $text = $box.find('.service-status-toggle-text');
        $text.text(active ? $text.data('active') : $text.data('inactive'));
    }

    function updateShowOnAppSwitch($toggle) {
        var $box = $toggle.closest('.service-status-box');
        var active = $toggle.is(':checked');
        $box.find('.service-showonapp-value').val(active ? 'true' : 'false');
        var $text = $box.find('.service-showonapp-toggle-text');
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

    function normalizePriceValue(value) {
        return (value || '').toString().replace(/[^0-9]/g, '');
    }

    function formatPriceValue(value) {
        var digits = normalizePriceValue(value);
        if (!digits) return '';
        return digits.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    }

    function normalizeNumberInput($input, max) {
        var value = ($input.val() || '').replace(/\D/g, '');
        if (max) value = value.substring(0, max);
        $input.val(value);
    }

    function normalizePriceInput($input) {
        $input.val(formatPriceValue($input.val()));
    }

    function isServiceFormValid($form, showWarning) {
        var name = getFieldValue($form, 'Service.Name', 'Service_Name');
        var categoryId = getFieldValue($form, 'Service.CategoryId', 'Service_CategoryId');
        var price = normalizePriceValue(getFieldValue($form, 'Service.Price', 'Service_Price'));
        var duration = getFieldValue($form, 'Service.Duration', 'Service_Duration');
        var role = getFieldValue($form, 'Service.ApplicableRole', 'Service_ApplicableRole');
        var level = getFieldValue($form, 'Service.ApplicableLevel', 'Service_ApplicableLevel');
        var isShowOnApp = getHiddenFieldValue($form, 'IsShowOnApp') === 'true';
        var status = getHiddenFieldValue($form, 'Status');

        var valid = true;
        var message = null;
        if (!name) { valid = false; message = l('SalonBeautyServices:NameRequired'); }
        else if (!categoryId) { valid = false; message = l('SalonBeautyServices:CategoryRequired'); }
        else if (!price || parseFloat(price) < 0) { valid = false; message = l('SalonBeautyServices:PriceInvalid'); }
        else if (!duration || parseInt(duration, 10) <= 0) { valid = false; message = l('SalonBeautyServices:DurationInvalid'); }
        else if (!role) { valid = false; message = l('SalonBeautyServices:RoleRequired'); }
        else if (!level) { valid = false; message = l('SalonBeautyServices:LevelRequired'); }
        else if (isShowOnApp && status !== '1') { valid = false; message = l('SalonBeautyServices:ShowOnAppRequiresActive'); }

        if (!valid && showWarning && message) abp.notify.warn(message);
        return valid;
    }

    function updateSubmitState($form) {
        if (!$form || !$form.length) return;
        var valid = isServiceFormValid($form, false);
        $form.find('.service-submit-button').prop('disabled', !valid).toggleClass('disabled', !valid);
    }

    $(document).on('input keyup paste', '.service-price-input', function () {
        var $input = $(this);
        setTimeout(function () { normalizePriceInput($input); updateSubmitState($input.closest('form')); }, 0);
    });

    $(document).on('input keyup paste', '.service-number-input', function () {
        var $input = $(this);
        setTimeout(function () { normalizeNumberInput($input, 4); updateSubmitState($input.closest('form')); }, 0);
    });

    $(document).on('input keyup change paste', '.service-form input, .service-form select, .service-form textarea', function () {
        var $form = $(this).closest('form');
        setTimeout(function () { updateSubmitState($form); }, 0);
    });

    $(document).on('change', '.service-status-toggle', function () { updateStatusSwitch($(this)); updateSubmitState($(this).closest('form')); });
    $(document).on('change', '.service-showonapp-toggle', function () { updateShowOnAppSwitch($(this)); updateSubmitState($(this).closest('form')); });

    function submitSalonServiceForm($form) {
        $form.find('.service-number-input').each(function () { normalizeNumberInput($(this), 4); });
        $form.find('.service-price-input').each(function () {
            var raw = normalizePriceValue($(this).val());
            $(this).val(raw || '0');
        });
        $form.find('.service-status-toggle').each(function () { updateStatusSwitch($(this)); });
        $form.find('.service-showonapp-toggle').each(function () { updateShowOnAppSwitch($(this)); });

        if (!isServiceFormValid($form, true)) { updateSubmitState($form); return false; }

        var $submitButton = $form.find('.service-submit-button');
        if ($submitButton.prop('disabled')) return false;
        $submitButton.prop('disabled', true).addClass('disabled');

        var formData = new FormData($form[0]);
        abp.ajax({ type: 'POST', url: $form.attr('action'), data: formData, processData: false, contentType: false })
            .done(function () {
                closeSalonModal($form);
                abp.notify.success(l('SavedSuccessfully'));
                dataTable.ajax.reload(function () { dataTable.columns.adjust(); }, false);
            })
            .always(function () { updateSubmitState($form); });
        return false;
    }

    $(document).on('click', '.service-submit-button', function (e) { e.preventDefault(); e.stopImmediatePropagation(); return submitSalonServiceForm($(this).closest('form')); });
    $(document).on('submit', '.service-form', function (e) { e.preventDefault(); e.stopImmediatePropagation(); return submitSalonServiceForm($(this)); });

    function initializeSalonServiceForm($form) {
        if (!$form || !$form.length) return;
        $form.find('.service-price-input').each(function () { normalizePriceInput($(this)); });
        $form.find('.service-number-input').each(function () { normalizeNumberInput($(this), 4); });
        $form.find('.service-status-toggle').each(function () { updateStatusSwitch($(this)); });
        $form.find('.service-showonapp-toggle').each(function () { updateShowOnAppSwitch($(this)); });
        updateSubmitState($form);
        setTimeout(function () { updateSubmitState($form); }, 150);
    }

    $(document).on('shown.bs.modal', '.modal', function () { initializeSalonServiceForm(findSalonFormFromModal($(this))); });
    $(document).on('hidden.bs.modal', '.modal', function () { updateSubmitState(findSalonFormFromModal($(this))); });

    $(document).on('change', '.service-inline-showonapp-toggle', function () {
        var $toggle = $(this);
        var $label = $toggle.closest('.service-inline-switch');
        var serviceId = $label.data('service-id');
        var newValue = $toggle.is(':checked');

        service.get(serviceId).then(function (item) {
            if (newValue && item.status !== 1) {
                $toggle.prop('checked', !newValue);
                abp.notify.warn(l('SalonBeautyServices:ShowOnAppRequiresActive'));
                return;
            }
            var updateDto = {
                name: item.name,
                categoryId: item.categoryId,
                price: item.price,
                duration: item.duration,
                applicableRole: item.applicableRole,
                applicableLevel: item.applicableLevel,
                status: item.status,
                isShowOnApp: newValue,
                note: item.note,
                sortOrder: item.sortOrder
            };
            service.update(serviceId, updateDto).then(function () {
                abp.notify.success(l('SavedSuccessfully'));
                dataTable.ajax.reload(null, false);
            }).catch(function () {
                $toggle.prop('checked', !newValue);
                abp.notify.error(l('SalonBeautyServices:UpdateShowOnAppFailed'));
            });
        }).catch(function () {
            $toggle.prop('checked', !newValue);
            abp.notify.error(l('SalonBeautyServices:UpdateShowOnAppFailed'));
        });
    });
});
