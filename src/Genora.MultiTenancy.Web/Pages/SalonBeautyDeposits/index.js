$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    function resolveSalonService(name) {
        var root = genora.multiTenancy.appServices && genora.multiTenancy.appServices.salonBeauties;
        if (!root || !root[name]) {
            throw new Error('Salon Beauty proxy not found: ' + name);
        }
        return root[name];
    }

    var service = resolveSalonService('salonBeautyDeposit');
    var createModal = new abp.ModalManager('/SalonBeautyDeposits/CreateModal');
    var editModal = new abp.ModalManager('/SalonBeautyDeposits/EditModal');
    var detailModal = new abp.ModalManager('/SalonBeautyDeposits/DetailModal');
    var canApprove = $('#CanApproveSalonDeposit').val() === 'true';
    var canCancel = $('#CanCancelSalonDeposit').val() === 'true';
    var canEdit = $('#CanEditSalonDeposit').val() === 'true';
    var canDelete = $('#CanDeleteSalonDeposit').val() === 'true';
    var pointUnit = l('SalonBeautyDeposits:PointUnit');

    function getRowRecord(data) { if (!data) return null; if (data.record) return data.record; return data; }
    function getRowStatus(data) { var rec = getRowRecord(data); if (!rec) return null; return (typeof rec.status === 'number') ? rec.status : null; }

    if ($.fn.select2) {
        $('#SalonDepositCustomerFilter').select2({
            placeholder: l('All'),
            allowClear: true,
            width: '100%'
        });
    }

    function htmlEncode(value) { return $('<div/>').text(value || '').html(); }

    function formatVnd(value) {
        if (value == null) return '0đ';
        return parseInt(value, 10).toLocaleString('vi-VN') + 'đ';
    }

    function formatPoint(value) {
        if (value == null || value === '') return '0';
        return parseInt(value, 10).toLocaleString('vi-VN');
    }

    function statusBadge(row) {
        var cls = row.status === 1 ? 'bg-warning' : (row.status === 2 ? 'bg-success' : 'bg-danger');
        return '<span class="badge ' + cls + '">' + htmlEncode(row.statusText) + '</span>';
    }

    // --- Deposit form helpers (shared cho CreateModal + EditModal) ---
    function stripDigits(v) {
        if (v == null) return '';
        var s = String(v).trim();
        if (!s) return '';
        // ASP.NET model binding render decimal là "5000000.00" hoặc "5000000,00".
        // Bóc phần thập phân ở cuối trước khi strip thousand separator.
        s = s.replace(/[.,]\d{1,2}$/, '');
        return s.replace(/\D/g, '');
    }

    function formatThousand(v) {
        var raw = stripDigits(v);
        if (!raw) return '';
        return raw.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    }

    function findDepositForm(modalManager) {
        // Form WRAPS abp-modal; abp.ModalManager đặt modal vào DOM, ta tìm form chứa modal đang show
        var $form = $('form.deposit-form').filter(function () {
            return $(this).find('.modal.show').length > 0;
        }).last();
        if (!$form.length) {
            $form = $('form.deposit-form').filter(function () {
                return $(this).find('.modal').length > 0;
            }).last();
        }
        return $form;
    }

    function recalcPreview($form) {
        var $input = $form.find('.deposit-amount-input');
        if (!$input.length) return;
        var amt = parseInt(stripDigits($input.val()), 10) || 0;
        var $rate = $form.find('.deposit-preview-rate');
        var $base = $form.find('.deposit-preview-base');
        var $bonus = $form.find('.deposit-preview-bonus');
        var $total = $form.find('.deposit-preview-total');
        var $tier = $form.find('.deposit-preview-tier');
        if (amt < 1000) {
            $rate.text('--'); $base.text('--'); $bonus.text('--'); $total.text('--');
            $tier.text('');
            return;
        }
        service.preview(amt).then(function (res) {
            $rate.text(parseInt(res.exchangeRate, 10).toLocaleString('vi-VN') + 'đ/1 ' + pointUnit);
            $base.text(formatPoint(res.basePoint));
            $bonus.text('+' + formatPoint(res.bonusPoint));
            $total.text(formatPoint(res.totalPoint));
            $tier.text(res.bonusTierName ? '(' + res.bonusTierName + ')' : '');
        }).catch(function () {
            $rate.text('--'); $base.text('--'); $bonus.text('--'); $total.text('--');
            $tier.text('');
        });
    }

    function initDepositSelect2($form) {
        if (!$.fn.select2) return;
        $form.find('.deposit-customer-select').each(function () {
            var $sel = $(this);
            if ($sel.hasClass('select2-hidden-accessible')) return;
            var $modalContent = $form.find('.modal-content').first();
            $sel.select2({
                placeholder: $sel.find('option:first').text(),
                allowClear: true,
                width: '100%',
                dropdownParent: $modalContent.length ? $modalContent : $(document.body)
            });
        });
    }

    function initDepositForm($form) {
        if (!$form || !$form.length) return;
        initDepositSelect2($form);
        $form.find('.deposit-amount-input').each(function () {
            $(this).val(formatThousand($(this).val()));
        });
        recalcPreview($form);
    }

    function closeDepositModal($form) {
        if (!$form || !$form.length) return;
        markDepositFormClean($form);
        suppressUnsavedChangesConfirmTemporarily(3000);
        var $modal = $form.find('.modal').first();
        if ($modal.length) {
            try {
                if (window.bootstrap && bootstrap.Modal) {
                    var inst = bootstrap.Modal.getInstance($modal[0]) || bootstrap.Modal.getOrCreateInstance($modal[0]);
                    inst.hide();
                } else if ($.fn.modal) {
                    $modal.modal('hide');
                }
            } catch (e) { /* ignore */ }
            try { $modal.off('hide.bs.modal'); } catch (e) { }
            try { $modal.removeClass('show').hide().attr('aria-hidden', 'true').removeAttr('aria-modal role'); } catch (e) { }
        }
        setTimeout(function () {
            $form.remove();
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css({ overflow: '', paddingRight: '' })
                .removeAttr('data-bs-overflow').removeAttr('data-bs-padding-right');
            if (dataTable) dataTable.columns.adjust();
        }, 30);
    }

    // Mark all form controls hiện tại làm "clean" (defaultValue = value) để ABP unsaved-guard không kích hoạt
    function markDepositFormClean($form) {
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
            } catch (e) { /* ignore */ }
        });
        try { $form.removeClass('dirty was-validated'); } catch (e) { }
        try { $form.data('dirty', false).data('isDirty', false).data('changed', false); } catch (e) { }
        try { $form.attr('data-dirty', 'false').attr('data-is-dirty', 'false'); } catch (e) { }
    }

    // Tạm override abp.message.confirm để skip prompt "unsaved changes" trong vài giây sau khi save
    function suppressUnsavedChangesConfirmTemporarily(milliseconds) {
        if (!abp || !abp.message || !abp.message.confirm) return function () { };
        var originalConfirm = abp.message.confirm;
        var restored = false;
        abp.message.confirm = function (message, title, callback) {
            var msg = (message || '').toString().toLowerCase();
            var ttl = (title || '').toString().toLowerCase();
            var isUnsaved =
                msg.indexOf('chưa được lưu') >= 0 ||
                msg.indexOf('chưa lưu') >= 0 ||
                msg.indexOf('unsaved') >= 0 ||
                msg.indexOf('not saved') >= 0 ||
                ttl.indexOf('bạn có chắc') >= 0 ||
                ttl.indexOf('are you sure') >= 0;
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

    function submitDepositForm($form) {
        if (!$form || !$form.length) return false;
        // Strip thousand separator trước khi POST (server bind decimal)
        $form.find('.deposit-amount-input').each(function () {
            $(this).val(stripDigits($(this).val()) || '0');
        });

        var $submitButton = $form.find('button[type="submit"], .service-btn-primary[type="submit"]').first();
        if (!$submitButton.length) $submitButton = $form.find('.service-btn-primary').first();
        if ($submitButton.prop('disabled')) return false;
        $submitButton.prop('disabled', true).addClass('disabled');

        var formData = new FormData($form[0]);
        abp.ajax({
            type: 'POST',
            url: $form.attr('action'),
            data: formData,
            processData: false,
            contentType: false
        })
            .done(function () {
                abp.notify.success(l('SavedSuccessfully'));
                closeDepositModal($form);
                if (dataTable) dataTable.ajax.reload(null, false);
            })
            .fail(function () {
                $submitButton.prop('disabled', false).removeClass('disabled');
                // Phục hồi format hiển thị nếu submit fail
                $form.find('.deposit-amount-input').each(function () {
                    $(this).val(formatThousand($(this).val()));
                });
            });
        return false;
    }

    // Format input + recalc preview khi user nhập (delegate vì form được render động)
    $(document).on('input keyup paste', '.deposit-amount-input', function () {
        var $i = $(this);
        var $f = $i.closest('form');
        setTimeout(function () {
            $i.val(formatThousand($i.val()));
            recalcPreview($f);
        }, 0);
    });

    // Submit handler: form wrap abp-modal nên ABP không tự bind submit
    $(document).on('click', '.deposit-form button[type="submit"]', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return submitDepositForm($(this).closest('form'));
    });
    $(document).on('submit', '.deposit-form', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        return submitDepositForm($(this));
    });

    // ABP ModalManager onOpen: chạy SAU khi modal HTML đã inject + bs.modal show xong
    function attachModalInit(modalManager) {
        modalManager.onOpen(function () {
            // Chờ một tick để DOM ổn định + abp-modal render xong
            setTimeout(function () {
                var $form = findDepositForm(modalManager);
                if ($form.length) initDepositForm($form);
            }, 50);
            setTimeout(function () {
                var $form = findDepositForm(modalManager);
                if ($form.length) initDepositForm($form);
            }, 200);
        });
    }
    attachModalInit(createModal);
    attachModalInit(editModal);

    // Fallback: shown.bs.modal (BS5 bubbles) — dùng find vì $(this) là .modal nằm BÊN TRONG form
    $(document).on('shown.bs.modal', '.modal', function () {
        var $modal = $(this);
        // Form wrap modal: từ .modal đi UP sẽ gặp form
        var $form = $modal.closest('form.deposit-form');
        if (!$form.length) {
            // Fallback nếu modal đã bị bs5 di chuyển: tìm form nào đang chứa modal show
            $form = findDepositForm();
        }
        if ($form.length) initDepositForm($form);
    });

    function buildListInput(request) {
        request = request || {};
        var length = request.length || 10;
        return {
            filterText: ($('#SalonDepositKeywordFilter').val() || '').trim() || null,
            customerId: ($('#SalonDepositCustomerFilter').val() || '').trim() || null,
            status: $('#SalonDepositStatusFilter').val() ? parseInt($('#SalonDepositStatusFilter').val(), 10) : null,
            paymentMethod: $('#SalonDepositPaymentMethodFilter').val() ? parseInt($('#SalonDepositPaymentMethodFilter').val(), 10) : null,
            skipCount: request.start || 0,
            maxResultCount: length > 100 ? 100 : length,
            sorting: 'creationTime desc'
        };
    }

    var dataTable = $('#SalonDepositsTable').DataTable(
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
                                action: function (data) { var rec = getRowRecord(data); if (rec) detailModal.open({ id: rec.id }); }
                            },
                            {
                                text: l('Edit'),
                                visible: function (data) {
                                    if (!canEdit) return false;
                                    var s = getRowStatus(data);
                                    if (s === null) return true;
                                    return s === 1;
                                },
                                action: function (data) { var rec = getRowRecord(data); if (rec) editModal.open({ id: rec.id }); }
                            },
                            {
                                text: l('SalonBeautyDeposits:Approve'),
                                visible: function (data) {
                                    if (!canApprove) return false;
                                    var s = getRowStatus(data);
                                    if (s === null) return true;
                                    return s === 1;
                                },
                                confirmMessage: function () { return l('SalonBeautyDeposits:ApproveConfirm'); },
                                action: function (data) {
                                    var rec = getRowRecord(data); if (!rec) return;
                                    service.approve(rec.id).then(function () {
                                        abp.notify.success(l('SavedSuccessfully'));
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            },
                            {
                                text: l('SalonBeautyDeposits:Cancel'),
                                visible: function (data) {
                                    if (!canCancel) return false;
                                    var s = getRowStatus(data);
                                    if (s === null) return true;
                                    return s === 1;
                                },
                                action: function (data) {
                                    var rec = getRowRecord(data); if (!rec) return;
                                    openCancelModal(rec.id, rec.transactionCode);
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: function (data) {
                                    if (!canDelete) return false;
                                    var s = getRowStatus(data);
                                    if (s === null) return true;
                                    return s !== 2;
                                },
                                confirmMessage: function () { return l('SalonBeautyDeposits:DeleteConfirm'); },
                                action: function (data) {
                                    var rec = getRowRecord(data); if (!rec) return;
                                    service.delete(rec.id).then(function () {
                                        abp.notify.success(l('SuccessfullyDeleted'));
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            }
                        ]
                    },
                    width: '120px'
                },
                { title: l('SalonBeautyDeposits:Code'), data: 'transactionCode', render: function (d, t, r) { return '<strong>' + htmlEncode(r.transactionCode) + '</strong>'; }, width: '160px' },
                { title: l('SalonBeautyDeposits:Customer'), data: 'customerName', render: function (d, t, r) { return htmlEncode(r.customerName) + ' <small class="text-muted d-block">' + htmlEncode(r.customerPhone || '') + '</small>'; }, width: '200px' },
                { title: l('SalonBeautyDeposits:Amount'), data: 'amount', render: function (d) { return '<strong>' + htmlEncode(formatVnd(d)) + '</strong>'; }, width: '130px' },
                { title: l('SalonBeautyDeposits:TotalPoint'), data: 'totalPoint', render: function (d, t, r) { return '<span class="text-success"><strong>+' + formatPoint(d) + ' ' + l('SalonBeautyDeposits:PointUnit') + '</strong></span>' + (r.bonusPoint > 0 ? ' <small class="text-muted">(+' + formatPoint(r.bonusPoint) + ' bonus)</small>' : ''); }, width: '180px' },
                { title: l('SalonBeautyDeposits:PaymentMethod'), data: 'paymentMethodText', render: function (d) { return htmlEncode(d); }, width: '130px' },
                { title: l('SalonBeautyDeposits:ReferenceCode'), data: 'referenceCode', render: function (d) { return d ? htmlEncode(d) : '<span class="text-muted">--</span>'; }, width: '140px' },
                { title: l('SalonBeautyDeposits:Status'), data: 'status', render: function (d, t, r) { return statusBadge(r); }, width: '120px' },
                { title: l('SalonBeautyDeposits:CreationTime'), data: 'creationTime', render: function (d) { return d ? luxon.DateTime.fromISO(d).toFormat('dd/MM/yyyy HH:mm') : ''; }, width: '150px' }
            ]
        })
    );

    // --- Cancel modal ---
    function openCancelModal(id, code) {
        $('#DepositCancelId').val(id);
        $('#DepositCancelTitle').text(code ? (l('SalonBeautyDeposits:CancelModalTitle') + ' - ' + code) : l('SalonBeautyDeposits:CancelModalTitle'));
        $('#DepositCancelReason').val('');
        $('#DepositCancelValidation').addClass('d-none');
        $('#DepositCancelConfirmBtn').prop('disabled', true);
        var el = document.getElementById('DepositCancelModal');
        if (el && window.bootstrap && bootstrap.Modal) {
            bootstrap.Modal.getOrCreateInstance(el).show();
        }
    }

    function closeCancelModal() {
        var el = document.getElementById('DepositCancelModal');
        if (el && window.bootstrap && bootstrap.Modal) {
            bootstrap.Modal.getOrCreateInstance(el).hide();
        }
    }

    $(document).on('input keyup change', '#DepositCancelReason', function () {
        var hasReason = $.trim($(this).val()).length > 0;
        $('#DepositCancelConfirmBtn').prop('disabled', !hasReason);
        if (hasReason) $('#DepositCancelValidation').addClass('d-none');
    });

    $(document).on('click', '#DepositCancelConfirmBtn', function () {
        var id = $('#DepositCancelId').val();
        var reason = $.trim($('#DepositCancelReason').val());
        if (!reason) {
            $('#DepositCancelValidation').removeClass('d-none');
            $('#DepositCancelConfirmBtn').prop('disabled', true);
            return;
        }
        var $btn = $(this);
        $btn.prop('disabled', true);
        service.cancel(id, { cancelReason: reason })
            .then(function () {
                closeCancelModal();
                abp.notify.success(l('SavedSuccessfully'));
                dataTable.ajax.reload(null, false);
            })
            .catch(function (err) {
                $btn.prop('disabled', false);
                var message = (err && err.message) || (err && err.responseJSON && err.responseJSON.error && err.responseJSON.error.message) || l('SalonBeautyDeposits:CancelFailed');
                abp.notify.error(message);
            });
    });

    $('#SearchSalonDepositButton').on('click', function (e) { e.preventDefault(); dataTable.ajax.reload(null, true); });
    $('#SalonDepositKeywordFilter').on('keydown', function (e) { if (e.key === 'Enter') { e.preventDefault(); dataTable.ajax.reload(null, true); } });
    $('#SalonDepositCustomerFilter,#SalonDepositStatusFilter,#SalonDepositPaymentMethodFilter').on('change', function () { dataTable.ajax.reload(null, true); });
    $('#NewSalonDepositButton').on('click', function (e) { e.preventDefault(); createModal.open(); });

    createModal.onResult(function () { dataTable.ajax.reload(null, false); });
    editModal.onResult(function () { dataTable.ajax.reload(null, false); });
    detailModal.onResult(function () { dataTable.ajax.reload(null, false); });
});
