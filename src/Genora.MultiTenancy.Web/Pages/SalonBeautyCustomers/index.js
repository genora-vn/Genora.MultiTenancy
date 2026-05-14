$(function () {
    var l = abp.localization.getResource('MultiTenancy');


    function resolveSalonService(name) {
        var root = genora.multiTenancy.appServices && genora.multiTenancy.appServices.salonBeauties;
        if (!root || !root[name]) {
            throw new Error('Salon Beauty application service proxy not found: genora.multiTenancy.appServices.salonBeauties.' + name);
        }
        return root[name];
    }

    var service = resolveSalonService('salonBeautyCustomer');

    var createModal = new abp.ModalManager('/SalonBeautyCustomers/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyCustomers/EditModal');
    var canEdit = $('#CanEditSalonCustomer').val() === 'true';
    var canDelete = $('#CanDeleteSalonCustomer').val() === 'true';


    function initSalonDatePicker(context) {
        if (!window.flatpickr) return;

        var $context = context ? $(context) : $(document);
        $context.find('input.dob-input, input.salon-birthday-input').each(function () {
            if (this._flatpickr) return;

            flatpickr(this, {
                dateFormat: "d/m/Y",
                allowInput: true,
                clickOpens: true,
                maxDate: "today",
                disableMobile: true
            });
        });
    }


    function markSalonFormAsClean($form) {
        if (!$form || !$form.length) return;

        // ABP/Bootstrap dirty-check compares current values with default values.
        // After a successful save we intentionally close the ajax modal, so make
        // the form clean before any close/hide/remove operation can run.
        $form.find('input, textarea, select').each(function () {
            var el = this;
            try {
                if (el.type === 'checkbox' || el.type === 'radio') {
                    el.defaultChecked = el.checked;
                } else if (el.tagName && el.tagName.toLowerCase() === 'select') {
                    $(el).find('option').each(function () {
                        this.defaultSelected = this.selected;
                    });
                } else {
                    el.defaultValue = el.value;
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
        var timeout = milliseconds || 2500;
        var restored = false;

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
                if ($.isFunction(callback)) {
                    callback(true);
                }

                if ($.Deferred) {
                    return $.Deferred().resolve(true).promise();
                }

                return Promise.resolve(true);
            }

            return originalConfirm.apply(this, arguments);
        };

        var restore = function () {
            if (restored) return;
            restored = true;
            if (abp && abp.message) {
                abp.message.confirm = originalConfirm;
            }
        };

        setTimeout(restore, timeout);
        return restore;
    }

    function findSalonModal($form) {
        var $modal = $form.closest('.modal');

        // In our Razor modal files the structure is:
        // <form class="salon-customer-form"><abp-modal>...</abp-modal></form>
        // Therefore the .modal element is a CHILD of the form, not an ancestor.
        if (!$modal.length) {
            $modal = $form.find('.modal').first();
        }

        if (!$modal.length) {
            $modal = $('.modal.show').filter(function () {
                var $m = $(this);
                return $m.find('.salon-customer-modal').length > 0 ||
                    $m.closest('form.salon-customer-form').length > 0;
            }).first();
        }

        return $modal;
    }

    function cleanupSalonModalDom($form) {
        var $modal = findSalonModal($form);

        // Remove the exact ajax-loaded form wrapper first. This is the important part:
        // ABP ModalManager injects the returned Razor page into the document, and in this
        // screen the form wraps the modal; hiding only .modal is not enough.
        if ($form && $form.length) {
            $form.remove();
        }

        if ($modal && $modal.length) {
            var $modalForm = $modal.closest('form.salon-customer-form');
            if ($modalForm.length) {
                $modalForm.remove();
            } else {
                $modal.remove();
            }
        }

        // Safety cleanup for any remaining hidden Salon customer modal instance.
        $('.modal').filter(function () {
            var $m = $(this);
            return $m.find('.salon-customer-modal').length > 0 ||
                $m.closest('form.salon-customer-form').length > 0;
        }).remove();

        $('.modal-backdrop').remove();
        $('body')
            .removeClass('modal-open')
            .css({ overflow: '', paddingRight: '' })
            .removeAttr('data-bs-overflow')
            .removeAttr('data-bs-padding-right');

        if (dataTable) {
            dataTable.columns.adjust();
        }
    }

    function closeSalonModal($form) {
        var $modal = findSalonModal($form);

        // Save succeeded, so this is no longer an "unsaved changes" case.
        // Do NOT trigger the normal close/cancel buttons here; ABP will show its
        // dirty-form confirmation. Mark clean, suppress that confirmation briefly,
        // then remove the ajax modal wrapper directly.
        markSalonFormAsClean($form);
        suppressUnsavedChangesConfirmTemporarily(3000);

        if ($modal.length) {
            try {
                $modal.removeClass('show')
                    .hide()
                    .attr('aria-hidden', 'true')
                    .removeAttr('aria-modal')
                    .removeAttr('role');
            } catch (e1) { }
        }

        setTimeout(function () {
            cleanupSalonModalDom($form);
        }, 50);
    }

    function parseNullableByte(val) {
        val = (val || '').trim();
        return val === '' ? null : parseInt(val, 10);
    }

    function getDateRange() {
        var period = $('#SalonTimePeriodFilter').val();
        if (!period) return { dateFrom: null, dateTo: null };

        var now = new Date();
        var to = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59);
        var from;

        if (period === 'today') {
            from = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0);
        } else {
            var days = parseInt(period, 10);
            from = new Date(now.getFullYear(), now.getMonth(), now.getDate() - days, 0, 0, 0);
        }

        return {
            dateFrom: from.toISOString(),
            dateTo: to.toISOString()
        };
    }

    function buildListInput(request) {
        request = request || {};
        var dates = getDateRange();
        var length = request.length || 10;
        return {
            filterText: ($('#SalonKeywordFilter').val() || '').trim() || null,
            dateFrom: dates.dateFrom,
            dateTo: dates.dateTo,
            customerGroup: ($('#SalonCustomerGroupFilter').val() || '').trim() || null,
            source: parseNullableByte($('#SalonSourceFilter').val()),
            status: parseNullableByte($('#SalonStatusFilter').val()),
            skipCount: request.start || 0,
            maxResultCount: length > 100 ? 100 : length,
            sorting: request.order && request.order.length ? null : 'totalSpent desc'
        };
    }

    function formatMoney(value) {
        value = value || 0;
        return value.toLocaleString('vi-VN');
    }

    function htmlEncode(value) {
        return $('<div/>').text(value || '').html();
    }

    function avatarUrl(row) {
        return row.avatar || '/images/getting-started/no-photo-square.png';
    }

    function renderCustomer(data, type, row) {
        return '<div class="salon-customer-cell">'
            + '<img src="' + htmlEncode(avatarUrl(row)) + '" alt="" />'
            + '<div><strong>' + htmlEncode(row.name) + '</strong>'
            + '<span>' + htmlEncode(row.customerCode || '') + '</span></div>'
            + '</div>';
    }

    function renderMembership(level) {
        level = level || 'NEW';
        return '<span class="salon-member-badge ' + level.toLowerCase() + '">' + htmlEncode(l('SalonBeautyCustomers:MembershipPrefix')) + ' ' + htmlEncode(level) + '</span>';
    }

    function renderStatus(data, type, row) {
        var active = row.status === 1;
        return '<span class="salon-status-dot ' + (active ? 'active' : 'inactive') + '">'
            + htmlEncode(active ? l('SalonBeautyCustomer:StatusActive') : l('SalonBeautyCustomer:StatusInactive'))
            + '</span>';
    }

    var dataTable = $('#SalonCustomersTable').DataTable(
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
                                text: l('SalonBeautyCustomers:ViewDetail'),
                                action: function (data) {
                                    location.href = '/SalonBeautyCustomers/Detail?id=' + data.record.id;
                                }
                            },
                            {
                                text: l('Edit'),
                                visible: function () { return canEdit; },
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: function () { return canDelete; },
                                confirmMessage: function (data) {
                                    return l('SalonBeautyCustomers:DeleteConfirm', data.record.name);
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
                { title: l('SalonBeautyCustomer:Name'), data: 'name', render: renderCustomer, width: '260px' },
                { title: l('SalonBeautyCustomer:Phone'), data: 'phoneMasked', width: '140px' },
                { title: l('SalonBeautyCustomers:MembershipLevel'), data: 'membershipLevel', render: renderMembership, width: '150px' },
                {
                    title: l('SalonBeautyCustomers:TotalSpent'),
                    data: 'totalSpent',
                    className: 'text-end salon-money-cell',
                    render: function (data) { return formatMoney(data); },
                    width: '150px'
                },
                {
                    title: l('SalonBeautyCustomers:LastBooking'),
                    data: 'lastBookingDate',
                    render: function (data) {
                        if (!data) return '--';
                        var d = new Date(data);
                        if (isNaN(d.getTime())) return '--';
                        return ('0' + d.getDate()).slice(-2) + '/' + ('0' + (d.getMonth() + 1)).slice(-2) + '/' + d.getFullYear();
                    },
                    width: '120px'
                },
                { title: l('SalonBeautyCustomer:Status'), data: 'status', render: renderStatus, width: '150px' }
            ]
        })
    );

    $('#SearchSalonCustomerButton').on('click', function (e) {
        e.preventDefault();
        dataTable.ajax.reload(null, true);
    });

    $('#SalonKeywordFilter').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            dataTable.ajax.reload(null, true);
        }
    });

    $('#SalonTimePeriodFilter,#SalonCustomerGroupFilter,#SalonSourceFilter,#SalonStatusFilter').on('change', function () {
        dataTable.ajax.reload(null, true);
    });

    $('#NewSalonCustomerButton').on('click', function (e) {
        e.preventDefault();
        createModal.open();
    });

    createModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload(function () {
            dataTable.columns.adjust();
        }, false);
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload(function () {
            dataTable.columns.adjust();
        }, false);
    });

    function normalizePhoneInput($input) {
        var value = ($input.val() || '').replace(/\D/g, '').substring(0, 11);
        $input.val(value);
    }

    function updateStatusSwitch($toggle) {
        var $box = $toggle.closest('.salon-status-box');
        var active = $toggle.is(':checked');
        $box.find('.salon-status-value').val(active ? '1' : '0');
        var $text = $box.find('.salon-status-toggle-text');
        $text.text(active ? $text.data('active') : $text.data('inactive'));
    }

    function getFieldValue($form, name, id) {
        var $field = $form.find('[name="' + name + '"]');
        if (!$field.length && id) {
            $field = $form.find('#' + id);
        }
        return $.trim(($field.val() || '').toString());
    }

    function parseSalonDate(value) {
        value = $.trim(value || '');
        if (!value) return null;

        // HTML date input normally returns yyyy-MM-dd. Some browser/locale helpers can return dd/MM/yyyy.
        var ymd = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
        if (ymd) {
            return new Date(parseInt(ymd[1], 10), parseInt(ymd[2], 10) - 1, parseInt(ymd[3], 10));
        }

        var dmy = /^(\d{1,2})[\/.-](\d{1,2})[\/.-](\d{4})$/.exec(value);
        if (dmy) {
            return new Date(parseInt(dmy[3], 10), parseInt(dmy[2], 10) - 1, parseInt(dmy[1], 10));
        }

        var parsed = new Date(value);
        return isNaN(parsed.getTime()) ? null : parsed;
    }


    function previewCustomerAvatar($form, src) {
        var safeSrc = src || '/images/getting-started/no-photo-square.png';
        $form.find('.salon-avatar-preview-img').attr('src', safeSrc);
    }

    function setCustomerAvatarMode($form, mode) {
        var upload = mode === 'upload';
        $form.find('.salon-avatar-is-upload').val(upload ? 'true' : 'false');
        $form.find('.salon-avatar-url-panel').toggleClass('d-none', upload);
        $form.find('.salon-avatar-upload-panel').toggleClass('d-none', !upload);
        if (!upload) {
            var url = $.trim($form.find('.salon-avatar-url-text').val() || '');
            $form.find('.salon-avatar-url-input').val(url);
            previewCustomerAvatar($form, url);
        }
    }

    function initializeCustomerAvatarUpload($form) {
        if (!$form || !$form.length) return;
        var currentUrl = $.trim($form.find('.salon-avatar-url-input').val() || $form.find('.salon-avatar-url-text').val() || '');
        previewCustomerAvatar($form, currentUrl);
        setCustomerAvatarMode($form, 'url');
    }

    function isCustomerFormValid($form) {
        var name = getFieldValue($form, 'Customer.Name', 'Customer_Name');
        var phone = getFieldValue($form, 'Customer.Phone', 'Customer_Phone');
        var source = getFieldValue($form, 'Customer.Source', 'Customer_Source');
        var email = getFieldValue($form, 'Customer.Email', 'Customer_Email');
        var birthday = getFieldValue($form, 'Customer.Birthday', 'Customer_Birthday');
        var today = new Date();
        today.setHours(0, 0, 0, 0);

        if (!name || !phone || source === '') return false;
        if (!/^0\d{9,10}$/.test(phone)) return false;
        if (email && !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) return false;
        if (birthday) {
            var bd = parseSalonDate(birthday);
            if (!bd || bd > today) return false;
        }

        return true;
    }

    function updateSubmitState($form) {
        if (!$form || !$form.length) return;
        var valid = isCustomerFormValid($form);
        $form.find('.salon-submit-button').prop('disabled', !valid).toggleClass('disabled', !valid);
    }

    $(document).on('input keyup paste', '.salon-phone-input', function () {
        var $input = $(this);
        setTimeout(function () {
            normalizePhoneInput($input);
            updateSubmitState($input.closest('form'));
        }, 0);
    });

    $(document).on('input keyup change paste', '.salon-customer-form input, .salon-customer-form select, .salon-customer-form textarea', function () {
        var $form = $(this).closest('form');
        setTimeout(function () { updateSubmitState($form); }, 0);
    });

    $(document).on('change', '.salon-status-toggle', function () {
        updateStatusSwitch($(this));
        updateSubmitState($(this).closest('form'));
    });

    function submitSalonCustomerForm($form) {
        $form.find('.salon-phone-input').each(function () { normalizePhoneInput($(this)); });
        $form.find('.salon-status-toggle').each(function () { updateStatusSwitch($(this)); });
        var avatarMode = $form.find('input[name="CustomerAvatarMode"]:checked').val() || 'url';
        setCustomerAvatarMode($form, avatarMode);

        if (!isCustomerFormValid($form)) {
            abp.notify.warn(l('SalonBeautyCustomers:RequiredFormWarning'));
            updateSubmitState($form);
            return false;
        }

        var $submitButton = $form.find('.salon-submit-button');
        if ($submitButton.prop('disabled')) return false;

        $submitButton.prop('disabled', true).addClass('disabled');

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
            dataTable.ajax.reload(function () {
                dataTable.columns.adjust();
            }, false);
        }).always(function () {
            updateSubmitState($form);
        });

        return false;
    }

    $(document).on('click', '.salon-submit-button', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return submitSalonCustomerForm($(this).closest('form'));
    });

    $(document).on('submit', '.salon-customer-form', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return submitSalonCustomerForm($(this));
    });


    $(document).on('change', 'input[name="CustomerAvatarMode"]', function () {
        setCustomerAvatarMode($(this).closest('form'), $(this).val());
        updateSubmitState($(this).closest('form'));
    });

    $(document).on('input keyup paste change', '.salon-avatar-url-text', function () {
        var $form = $(this).closest('form');
        var url = $.trim($(this).val() || '');
        $form.find('.salon-avatar-url-input').val(url);
        previewCustomerAvatar($form, url);
    });

    $(document).on('click', '.salon-avatar-select-btn, .salon-avatar-preview-circle', function (e) {
        e.preventDefault();
        var $form = $(this).closest('form');
        setCustomerAvatarMode($form, 'upload');
        $form.find('input[name="CustomerAvatarMode"][value="upload"]').prop('checked', true);
        var input = $form.find('.salon-avatar-file-input')[0];
        if (input) input.click();
    });

    $(document).on('click', '.salon-avatar-file-input', function (e) { e.stopPropagation(); });

    $(document).on('change', '.salon-avatar-file-input', function () {
        var input = this;
        var $form = $(input).closest('form');
        var file = input.files && input.files[0];
        $form.find('.salon-avatar-is-upload').val('true');
        if (!file) {
            previewCustomerAvatar($form, '/images/getting-started/no-photo-square.png');
            return;
        }
        if (file.size > 2 * 1024 * 1024) {
            abp.notify.warn('Ảnh đại diện tối đa 2MB.');
            input.value = '';
            return;
        }
        if (!/^image\/(jpeg|png|webp)$/.test(file.type || '')) {
            abp.notify.warn('Chỉ hỗ trợ ảnh JPG, PNG hoặc WebP.');
            input.value = '';
            return;
        }
        var reader = new FileReader();
        reader.onload = function (e) { previewCustomerAvatar($form, e.target.result); };
        reader.readAsDataURL(file);
    });

    function initializeSalonCustomerForm($form) {
        if (!$form || !$form.length) return;
        initSalonDatePicker($form);
        initializeCustomerAvatarUpload($form);
        $form.find('.salon-phone-input').each(function () { normalizePhoneInput($(this)); });
        $form.find('.salon-status-toggle').each(function () { updateStatusSwitch($(this)); });
        updateSubmitState($form);
        setTimeout(function () { updateSubmitState($form); }, 150); // handles browser autofill/date picker late updates
    }


    $(document).on('focusin click', 'input.dob-input, input.salon-birthday-input', function () {
        initSalonDatePicker($(this).closest('.modal').length ? $(this).closest('.modal') : document);
        if (this._flatpickr) this._flatpickr.open();
    });

    $(document).on('click', '.salon-date-picker-trigger', function () {
        var input = $(this).closest('.salon-input-icon').find('input.dob-input, input.salon-birthday-input')[0];
        if (!input) return;
        initSalonDatePicker($(input).closest('.modal'));
        if (input._flatpickr) input._flatpickr.open();
        else $(input).trigger('focus');
    });

    $(document).on('shown.bs.modal', '.modal', function () {
        var $modal = $(this);
        var $form = $modal.find('.salon-customer-form');
        if ($form.length) {
            initializeSalonCustomerForm($form);
        }
    });

    $(document).on('hidden.bs.modal', '.modal', function () {
        var $form = $(this).find('.salon-customer-form');
        if ($form.length) {
            updateSubmitState($form);
        }
    });

    $('#ExportSalonCustomerButton').on('click', function () {
        var rows = dataTable.rows({ search: 'applied' }).data().toArray();
        if (!rows.length) {
            abp.notify.warn(l('SalonBeautyCustomers:NoDataToExport'));
            return;
        }

        var csv = ['CustomerCode,Name,Phone,MembershipLevel,TotalSpent,TotalBooking,LoyaltyPoint,Status'];
        rows.forEach(function (x) {
            csv.push([
                x.customerCode,
                x.name,
                x.phone,
                x.membershipLevel,
                x.totalSpent,
                x.totalBooking,
                x.loyaltyPoint,
                x.statusText
            ].map(function (v) {
                v = (v === null || v === undefined) ? '' : String(v);
                return '"' + v.replace(/"/g, '""') + '"';
            }).join(','));
        });

        var blob = new Blob(["\ufeff" + csv.join('\n')], { type: 'text/csv;charset=utf-8;' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = 'salon-beauty-customers.csv';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    });
});
