$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    var service = genora.multiTenancy.appServices.appZaloAuths.appZaloAuth;

    var createModal = new abp.ModalManager('/AppZaloAuths/CreateModal');
    var editModal = new abp.ModalManager('/AppZaloAuths/EditModal');

    async function copyText(text) {
        if (!text) return;
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return;
        }
        var ta = document.createElement('textarea');
        ta.value = text;
        ta.style.position = 'fixed';
        ta.style.left = '-9999px';
        document.body.appendChild(ta);
        ta.focus();
        ta.select();
        document.execCommand('copy');
        document.body.removeChild(ta);
    }

    function getFilter() {
        var isActiveRaw = $('#IsActive').val();
        return {
            filterText: $('#FilterText').val(),
            isActive: isActiveRaw === "" ? null : (isActiveRaw === "true")
        };
    }

    var table = $('#ZaloAuthTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            processing: true,
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(service.getList, getFilter),
            columnDefs: [
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                confirmMessage: function (data) {
                                    return l('AreYouSureToDelete', data.record.appId);
                                },
                                action: function (data) {
                                    service.delete(data.record.id).then(function () {
                                        abp.notify.success(l('DeletedSuccessfully'));
                                        table.ajax.reload();
                                    });
                                }
                            }
                        ]
                    }
                },
                { title: l('AppId'), data: "appId" },
                { title: l('State'), data: "state" },
                {
                    title: l('AccessToken'),
                    data: "accessTokenMasked",
                    render: function (v, type, row) {
                        var masked = v || '';
                        return `
        <div class="d-flex align-items-center gap-2">
          <code class="text-truncate" style="max-width:220px">${masked}</code>
          <button type="button" class="btn btn-sm btn-outline-primary js-copy-token"
              data-id="${row.id}" data-kind="access">
              <i class="fa fa-copy"></i>
          </button>
        </div>`;
                    }
                },
                {
                    title: l('RefreshToken'),
                    data: "refreshTokenMasked",
                    render: function (v, type, row) {
                        var masked = v || '';
                        return `
        <div class="d-flex align-items-center gap-2">
          <code class="text-truncate" style="max-width:220px">${masked}</code>
          <button type="button" class="btn btn-sm btn-outline-primary js-copy-token"
              data-id="${row.id}" data-kind="refresh">
              <i class="fa fa-copy"></i>
          </button>
        </div>`;
                    }
                },
                {
                    title: l('ExpiresAt'),
                    data: "expireTokenTime",
                    render: function (v) {
                        return v ? luxon.DateTime.fromISO(v).toFormat('dd/MM/yyyy HH:mm') : '';
                    }
                },
                {
                    title: l('IsActive'),
                    data: "isActive",
                    render: function (v) {
                        return v
                            ? '<span class="badge bg-success">' + l('Yes') + '</span>'
                            : '<span class="badge bg-secondary">' + l('No') + '</span>';
                    }
                }
            ]
        })
    );

    $('#ZaloConnectBtn').click(function () {
        abp.ajax({
            url: '/api/host/zalo-auth/authorize-url',
            type: 'GET'
        }).done(function (res) {
            window.open(res.authorizeUrl, '_blank');
        });
    });

    $('#ZaloRefreshBtn').click(function () {
        abp.ajax({
            url: '/api/host/zalo-auth/refresh-now',
            type: 'POST'
        }).done(function () {
            abp.notify.success(l('RefreshedSuccessfully'));
            table.ajax.reload();
        });
    });

    $('#NewAppZaloAuthButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#SearchButton').click(function (e) {
        e.preventDefault();
        table.ajax.reload();
    });

    createModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        table.ajax.reload();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        table.ajax.reload();
    });

    $('#ZaloAuthTable').on('click', '.js-copy-token', function () {
        var id = $(this).data('id');
        var kind = $(this).data('kind');

        abp.ajax({
            url: '/api/host/zalo-auth/' + id + '/token?kind=' + kind,
            type: 'GET'
        }).done(async function (res) {
            // ✅ res = { token: "..." }
            await copyText(res.token);
            abp.notify.success('Copied');
        });
    });
});
