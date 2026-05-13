$(function () {
    var l = abp.localization.getResource('MultiTenancy');


    function resolveSalonService(name) {
        var root = genora.multiTenancy.appServices && genora.multiTenancy.appServices.salonBeauties;
        if (!root || !root[name]) {
            throw new Error('Salon Beauty application service proxy not found: genora.multiTenancy.appServices.salonBeauties.' + name);
        }
        return root[name];
    }

    var categoryService = resolveSalonService('salonBeautyServiceCategory');

    var createModal = new abp.ModalManager('/SalonBeautyServiceCategories/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyServiceCategories/EditModal');
    var detailModal = new abp.ModalManager('/SalonBeautyServiceCategories/DetailModal');
    var canEdit = $('#CanEditSalonServiceCategory').val() === 'true';
    var canDelete = $('#CanDeleteSalonServiceCategory').val() === 'true';

    function parseNullableByte(val) {
        val = (val || '').toString().trim();
        return val === '' ? null : parseInt(val, 10);
    }

    function htmlEncode(value) { return $('<div/>').text(value || '').html(); }

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

    function findSalonCategoryFormFromModal($modal) {
        var $form = $modal.find('form.service-category-form');
        if (!$form.length) $form = $modal.closest('form.service-category-form');
        if (!$form.length) $form = $('form.service-category-form').filter(function () { return $(this).find('.service-category-modal').length > 0; }).last();
        return $form;
    }

    function findSalonCategoryModal($form) {
        if (!$form || !$form.length) return $('.modal.show').has('.service-category-modal').last();
        var $modal = $form.closest('.modal');
        if (!$modal.length) $modal = $form.find('.modal').first();
        if (!$modal.length) $modal = $('.modal.show').filter(function () { return $(this).find('.service-category-modal').length > 0; }).last();
        return $modal;
    }

    function cleanupSalonCategoryModalDom($form) {
        var $modal = findSalonCategoryModal($form);
        if ($form && $form.length) $form.remove();
        if ($modal && $modal.length) {
            var $modalForm = $modal.closest('form.service-category-form');
            if ($modalForm.length) $modalForm.remove();
            else $modal.remove();
        }
        $('.modal').filter(function () { return $(this).find('.service-category-modal').length > 0; }).remove();
        $('form.service-category-form').filter(function () { return $(this).find('.service-category-modal').length > 0; }).remove();
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open').css({ overflow: '', paddingRight: '' }).removeAttr('data-bs-overflow').removeAttr('data-bs-padding-right');
        if (dataTable) dataTable.columns.adjust();
    }

    function closeSalonCategoryModal($form) {
        var $modal = findSalonCategoryModal($form);
        markSalonFormAsClean($form);
        suppressUnsavedChangesConfirmTemporarily(3000);
        if ($modal && $modal.length) {
            try { $modal.off('hide.bs.modal'); } catch (e) { }
            try { $modal.removeClass('show').hide().attr('aria-hidden', 'true').removeAttr('aria-modal role'); } catch (e) { }
        }
        setTimeout(function () { cleanupSalonCategoryModalDom($form); }, 30);
    }

    function buildListInput(request) {
        request = request || {};
        var length = request.length || 10;
        return {
            filterText: ($('#SalonServiceCategoryKeywordFilter').val() || '').trim() || null,
            status: parseNullableByte($('#SalonServiceCategoryStatusFilter').val()),
            skipCount: request.start || 0,
            maxResultCount: length > 100 ? 100 : length,
            sorting: 'sortOrder asc, name asc'
        };
    }

    function renderName(data, type, row) {
        return '<div class="service-info-cell">'
            + '<div class="service-info-icon"><i class="fa fa-folder-open"></i></div>'
            + '<div class="service-info-text"><strong>' + htmlEncode(row.name) + '</strong>'
            + '<span>ID: ' + htmlEncode(row.id ? row.id.substring(0, 8) : '') + '</span></div>'
            + '</div>';
    }

    function renderDescription(data, type, row) {
        return htmlEncode(row.description || '--');
    }

    function renderStatus(data, type, row) {
        var active = row.status === 1;
        var text = row.statusText || (active ? l('SalonBeautyCustomer:StatusActive') : l('SalonBeautyCustomer:StatusInactive'));
        return '<span class="service-status-dot ' + (active ? 'active' : 'inactive') + '">' + htmlEncode(text) + '</span>';
    }

    var dataTable = $('#SalonServiceCategoriesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            order: [],
            ajax: abp.libs.datatables.createAjax(categoryService.getList, buildListInput),
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
                                confirmMessage: function (data) { return abp.utils.formatString(l('SalonBeautyServiceCategories:DeleteConfirm'), data.record.name); },
                                action: function (data) {
                                    categoryService.delete(data.record.id).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            }
                        ]
                    },
                    width: '120px'
                },
                { title: l('SalonBeautyServiceCategory:Name'), data: 'name', render: renderName, width: '260px' },
                { title: l('SalonBeautyServiceCategory:Description'), data: 'description', render: renderDescription, width: '360px' },
                { title: l('SalonBeautyServiceCategory:SortOrder'), data: 'sortOrder', width: '120px' },
                { title: l('SalonBeautyServiceCategory:Status'), data: 'status', render: renderStatus, width: '160px' }
            ]
        })
    );

    $('#SearchSalonServiceCategoryButton').on('click', function (e) { e.preventDefault(); dataTable.ajax.reload(null, true); });
    $('#SalonServiceCategoryKeywordFilter').on('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); dataTable.ajax.reload(null, true); } });
    $('#SalonServiceCategoryStatusFilter').on('change', function () { dataTable.ajax.reload(null, true); });
    $('#NewSalonServiceCategoryButton').on('click', function (e) { e.preventDefault(); createModal.open(); });

    function updateCategoryStatusSwitch($toggle) {
        var $box = $toggle.closest('.service-status-box');
        var active = $toggle.is(':checked');
        $box.find('.service-category-status-value').val(active ? '1' : '0');
        var $text = $box.find('.service-category-status-toggle-text');
        $text.text(active ? $text.data('active') : $text.data('inactive'));
    }

    function normalizeNumberInput($input, max) {
        var value = ($input.val() || '').replace(/\D/g, '');
        if (max) value = value.substring(0, max);
        $input.val(value);
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

    function isCategoryFormValid($form, showWarning) {
        var name = getFieldValue($form, 'Category.Name', 'Category_Name');
        var status = getHiddenFieldValue($form, 'Status');
        var sortOrder = getFieldValue($form, 'Category.SortOrder', 'Category_SortOrder') || '0';
        var valid = true;
        var message = null;

        if (!name) { valid = false; message = l('SalonBeautyServiceCategories:NameRequired'); }
        else if (parseInt(sortOrder, 10) < 0) { valid = false; message = l('SalonBeautyServiceCategories:SortOrderInvalid'); }
        else if (status !== '0' && status !== '1') { valid = false; message = l('SalonBeautyServiceCategories:StatusInvalid'); }

        if (!valid && showWarning && message) abp.notify.warn(message);
        return valid;
    }

    function updateSubmitState($form) {
        if (!$form || !$form.length) return;
        var valid = isCategoryFormValid($form, false);
        $form.find('.service-category-submit-button').prop('disabled', !valid).toggleClass('disabled', !valid);
    }

    $(document).on('input keyup paste', '.service-category-number-input', function () {
        var $input = $(this);
        setTimeout(function () { normalizeNumberInput($input, 4); updateSubmitState($input.closest('form')); }, 0);
    });

    $(document).on('input keyup change paste', '.service-category-form input, .service-category-form select, .service-category-form textarea', function () {
        var $form = $(this).closest('form');
        setTimeout(function () { updateSubmitState($form); }, 0);
    });

    $(document).on('change', '.service-category-status-toggle', function () { updateCategoryStatusSwitch($(this)); updateSubmitState($(this).closest('form')); });

    function submitSalonCategoryForm($form) {
        $form.find('.service-category-number-input').each(function () { normalizeNumberInput($(this), 4); });
        $form.find('.service-category-status-toggle').each(function () { updateCategoryStatusSwitch($(this)); });

        if (!isCategoryFormValid($form, true)) { updateSubmitState($form); return false; }

        var $submitButton = $form.find('.service-category-submit-button');
        if ($submitButton.prop('disabled')) return false;
        $submitButton.prop('disabled', true).addClass('disabled');

        var formData = new FormData($form[0]);
        abp.ajax({ type: 'POST', url: $form.attr('action'), data: formData, processData: false, contentType: false })
            .done(function () {
                closeSalonCategoryModal($form);
                abp.notify.success(l('SavedSuccessfully'));
                dataTable.ajax.reload(function () { dataTable.columns.adjust(); }, false);
            })
            .always(function () { updateSubmitState($form); });
        return false;
    }

    $(document).on('click', '.service-category-submit-button', function (e) { e.preventDefault(); e.stopImmediatePropagation(); return submitSalonCategoryForm($(this).closest('form')); });
    $(document).on('submit', '.service-category-form', function (e) { e.preventDefault(); e.stopImmediatePropagation(); return submitSalonCategoryForm($(this)); });

    function initializeSalonCategoryForm($form) {
        if (!$form || !$form.length) return;
        $form.find('.service-category-number-input').each(function () { normalizeNumberInput($(this), 4); });
        $form.find('.service-category-status-toggle').each(function () { updateCategoryStatusSwitch($(this)); });
        updateSubmitState($form);
        setTimeout(function () { updateSubmitState($form); }, 150);
    }

    $(document).on('shown.bs.modal', '.modal', function () { initializeSalonCategoryForm(findSalonCategoryFormFromModal($(this))); });
    $(document).on('hidden.bs.modal', '.modal', function () { updateSubmitState(findSalonCategoryFormFromModal($(this))); });
});
