$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appHomePageConfigs.appHomePageConfig;

    var editWidgetModal = new abp.ModalManager(abp.appPath + 'AppHomePageConfigs/EditWidgetModal');
    var featureGridModal = new abp.ModalManager(abp.appPath + 'AppHomePageConfigs/FeatureGridModal');

    var canEdit =
        abp.auth.isGranted('MultiTenancy.AppHomePageConfigs.Edit') ||
        abp.auth.isGranted('MultiTenancy.HostAppHomePageConfigs.Edit');

    function renderEnabled(d) {
        return d ? l('Yes') : l('No');
    }

    // Switch column (On/Off)
    function renderToggleSwitch(data, type, row) {
        var checked = row.isEnabled ? 'checked' : '';
        var disabled = canEdit ? '' : 'disabled';

        // Bootstrap switch style
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
            order: [[2, "asc"]], // DisplayOrder
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
                        if (!canEdit) {
                            $(td).find('.hp-toggle').prop('disabled', true);
                        }
                    }
                }
            ],

            drawCallback: function () {
                enableRowDragDrop();
            }
        })
    );

    editWidgetModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload(null, false);
    });

    featureGridModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        dataTable.ajax.reload(null, false);
    });

    // Toggle nhanh bằng switch (delegate event)
    $('#AppHomePageWidgetsTable').on('change', '.hp-toggle', function () {
        if (!canEdit) return;

        var id = $(this).data('id');
        var isEnabled = $(this).is(':checked');

        service.updateWidget({ id: id, isEnabled: isEnabled })
            .then(function () {
                // Reload cập nhật cột text IsActive
                dataTable.ajax.reload(null, false);
            })
            .catch(function () {
                $(this).prop('checked', !isEnabled);
            }.bind(this));
    });

    function enableRowDragDrop() {
        var $tbody = $('#AppHomePageWidgetsTable tbody');

        // Bind events 1 lần, nhưng draggable attr set lại mỗi draw
        if ($tbody.data('dragBound') !== '1') {
            $tbody.data('dragBound', '1');

            var draggedRow = null;

            $tbody.on('dragstart', 'tr', function (e) {
                // Không kéo khi đang click vào button/dropdown/input
                var $t = $(e.target);
                if ($t.closest('button, a, input, select, textarea, .dropdown, .btn').length) {
                    e.preventDefault();
                    return;
                }

                draggedRow = this;
                e.originalEvent.dataTransfer.effectAllowed = 'move';
                $(this).addClass('dragging');
            });

            $tbody.on('dragend', 'tr', function () {
                $(this).removeClass('dragging');
            });

            $tbody.on('dragover', 'tr', function (e) {
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

        // Mỗi lần draw phải set lại draggable + cursor cho các row hiện tại
        $tbody.find('tr')
            .attr('draggable', canEdit ? 'true' : 'false')
            .toggleClass('hp-draggable', canEdit);
    }
});