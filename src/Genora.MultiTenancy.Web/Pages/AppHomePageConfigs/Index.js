$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appHomePageConfigs.appHomePageConfig;

    var createWidgetModal = new abp.ModalManager(abp.appPath + 'AppHomePageConfigs/CreateWidgetModal');
    var editWidgetModal = new abp.ModalManager(abp.appPath + 'AppHomePageConfigs/EditWidgetModal');
    var featureGridModal = new abp.ModalManager(abp.appPath + 'AppHomePageConfigs/FeatureGridModal');

    var canEdit =
        abp.auth.isGranted('MultiTenancy.AppHomePageConfigs.Edit') ||
        abp.auth.isGranted('MultiTenancy.HostAppHomePageConfigs.Edit');

    // Ensure antiforgery header for all ABP ajax calls
    (function () {
        var tokenProvider = function () {
            // ABP anti-forgery token getter
            return (abp.security && abp.security.antiForgery)
                ? abp.security.antiForgery.getToken()
                : null;
        };

        // Patch abp.ajax default headers (works even if proxy uses abp.ajax internally)
        var originalAjax = abp.ajax;
        abp.ajax = function (userOptions) {
            userOptions = userOptions || {};
            userOptions.headers = userOptions.headers || {};

            var t = tokenProvider();
            if (t && !userOptions.headers.RequestVerificationToken) {
                userOptions.headers.RequestVerificationToken = t;
            }

            return originalAjax(userOptions);
        };
    })();

    function renderEnabled(d) {
        return d ? l('Yes') : l('No');
    }

    function renderToggleSwitch(data, type, row) {
        var checked = row.isEnabled ? 'checked' : '';
        var disabled = canEdit ? '' : 'disabled';

        return `
          <div class="form-check form-switch m-0 d-flex justify-content-center">
            <input class="form-check-input hp-toggle"
                   type="checkbox"
                   role="switch"
                   data-id="${row.id}"
                   ${checked}
                   ${disabled} />
          </div>`;
    }

    var dataTable = $('#AppHomePageWidgetsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: true,
            order: [[2, "asc"]],
            scrollX: true,

            ajax: abp.libs.datatables.createAjax(service.getWidgetList, function (request) {
                return {
                    filter: request.search?.value || null,
                    skipCount: request.start,
                    maxResultCount: request.length,
                    sorting: request.columns?.[request.order?.[0]?.column]?.data
                        ? request.columns[request.order[0].column].data + ' ' + request.order[0].dir
                        : null
                };
            }),

            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: canEdit,
                                action: function (data) {
                                    editWidgetModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('HomePageConfig:EditFeatureGrid'),
                                visible: canEdit,
                                action: function (data) {
                                    if ((data.record.widgetKey || '') !== 'FeatureGrid') {
                                        abp.notify.warn(l('HomePageConfig:WidgetNotInScope') || 'N/A');
                                        return;
                                    }
                                    featureGridModal.open({ widgetId: data.record.id });
                                }
                            },
                            {
                                text: l('UpdateStatus'),
                                visible: canEdit,
                                action: function (data) {
                                    service.updateWidget({
                                        id: data.record.id,
                                        isEnabled: !data.record.isEnabled
                                    }).then(function () {
                                        dataTable.ajax.reload(null, false);
                                    });
                                }
                            }
                        ]
                    }
                },
                { title: 'WidgetKey', data: "widgetKey" },
                { title: 'Order', data: "displayOrder" },
                { title: 'Module', data: "moduleKey" },
                { title: l('Title'), data: "title" },
                { title: 'Limit', data: "limit" },
                { title: l('IsActive'), data: "isEnabled", render: renderEnabled },
                {
                    title: l('HomePageConfig:ToggleOnOff') || 'On/Off',
                    data: null,
                    orderable: false,
                    searchable: false,
                    className: "text-center",
                    render: renderToggleSwitch,
                    createdCell: function (td) {
                        $(td).find('.hp-toggle').prop('disabled', !canEdit);
                    },
                    visible: canEdit
                }
            ],

            drawCallback: function () {
                enableRowDragDrop();
            }
        })
    );

    // ✅ Create button
    $('#NewHomePageWidgetButton').click(function (e) {
        e.preventDefault();
        if (!canEdit) return;
        createWidgetModal.open();
    });

    createWidgetModal.onResult(function () {
        abp.notify.success(l('CreatedSuccessfully') || l('SavedSuccessfully'));
        dataTable.ajax.reload(null, false);
    });

    editWidgetModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload(null, false);
    });

    featureGridModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload(null, false);
    });

    // Toggle nhanh bằng switch
    $('#AppHomePageWidgetsTable').on('change', '.hp-toggle', function () {
        if (!canEdit) return;

        var $sw = $(this);
        var id = $sw.data('id');
        var isEnabled = $sw.is(':checked');

        service.updateWidget({ id: id, isEnabled: isEnabled })
            .then(function () {
                dataTable.ajax.reload(null, false);
            })
            .catch(function () {
                $sw.prop('checked', !isEnabled);
            });
    });

    function enableRowDragDrop() {
        var $tbody = $('#AppHomePageWidgetsTable tbody');

        if ($tbody.data('dragBound') !== '1') {
            $tbody.data('dragBound', '1');

            var draggedRow = null;

            $tbody.on('dragstart', 'tr', function (e) {
                if (!canEdit) { e.preventDefault(); return; }

                // Không kéo khi đang click vào button/dropdown/input/switch
                var $t = $(e.target);
                if ($t.closest('button, a, input, select, textarea, .dropdown, .btn, .form-check').length) {
                    e.preventDefault();
                    return;
                }

                draggedRow = this;
                e.originalEvent.dataTransfer.effectAllowed = 'move';
                try { e.originalEvent.dataTransfer.setData('text/plain', 'drag'); } catch { }
                $(this).addClass('dragging');
            });

            $tbody.on('dragend', 'tr', function () {
                $(this).removeClass('dragging');
            });

            $tbody.on('dragover', 'tr', function (e) {
                if (!canEdit) return;
                e.preventDefault();
                if (!draggedRow) return;

                var $this = $(this);
                var $dragged = $(draggedRow);
                if (!$dragged.length || !$this.length || draggedRow === this) return;

                var draggedIndex = $dragged.index();
                var targetIndex = $this.index();

                if (draggedIndex < targetIndex) $this.after($dragged);
                else $this.before($dragged);
            });

            $tbody.on('drop', 'tr', function (e) {
                if (!canEdit) return;
                e.preventDefault();
                if (!draggedRow) return;

                var ids = [];
                $tbody.find('tr').each(function () {
                    var row = dataTable.row(this).data();
                    if (row && row.id) ids.push(row.id);
                });

                draggedRow = null;
                if (!ids.length) return;

                service.updateWidgetOrder({ orderedIds: ids })
                    .then(function () {
                        dataTable.ajax.reload(null, false);
                    });
            });
        }

        // mỗi draw set lại draggable + cursor
        $tbody.find('tr')
            .attr('draggable', canEdit ? 'true' : 'false')
            .toggleClass('hp-draggable', canEdit)
            .css('cursor', canEdit ? 'all-scroll' : '');
    }
});