$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var service = genora.multiTenancy.appServices.appZaloAuths.appZaloAuth;

    var createModal = new abp.ModalManager('/AppZaloAuths/CreateModal');
    var editModal = new abp.ModalManager('/AppZaloAuths/EditModal');

    const TZ = 'Asia/Bangkok';

    function formatExpireLocal(isoUtc) {
        if (!isoUtc) return '';
        // isoUtc backend trả UTC ISO string
        return luxon.DateTime
            .fromISO(isoUtc, { zone: 'utc' })
            .setZone(TZ)
            .toFormat('dd/MM/yyyy HH:mm');
    }

    function renderTokenBanner(state) {
        // state: { expireTokenTime, isExpired }
        var $box = $('#ZaloTokenBanner');
        if (!$box.length) return;

        // reset
        $box.removeClass('alert-success alert-warning alert-danger d-none');

        if (!state || !state.expireTokenTime) {
            $box.addClass('alert-warning').html(`<b>Zalo Token:</b> Chưa cấu hình token. Vui lòng <b>ZaloConnect</b> hoặc nhập token thủ công.`);
            return;
        }

        var expText = formatExpireLocal(state.expireTokenTime);
        if (state.isExpired) {
            $box.addClass('alert-danger').html(`<b>Zalo Token:</b> <b>Expired</b>. Hết hạn lúc ${expText} (${TZ}). Vui lòng bấm <b>ZaloRefreshNow</b>.`);
            return;
        }

        $box.addClass('alert-success').html(`<b>Zalo Token:</b> Đang hiệu lực. Hết hạn lúc ${expText} (${TZ}).`);
    }

    function refreshTokenBanner() {
        return abp.ajax({
            url: '/api/host/zalo-auth/active',
            type: 'GET'
        }).done(function (st) {
            renderTokenBanner(st);
        }).fail(function () {
            // nếu lỗi API thì ẩn hoặc báo warning
            renderTokenBanner(null);
        });
    }

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
                                        refreshTokenBanner();
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
                    render: function (v, type, row) {
                        if (!v) return '';

                        var expLocal = luxon.DateTime.fromISO(v, { zone: 'utc' }).setZone(TZ);
                        var nowLocal = luxon.DateTime.local().setZone(TZ);
                        var isExpired = expLocal <= nowLocal;

                        var text = expLocal.toFormat('dd/MM/yyyy HH:mm') + ' (BKK)';

                        if (isExpired) {
                            return `<span class="badge bg-danger me-2">Expired</span>${text}`;
                        }
                        return `<span class="badge bg-success me-2">Valid</span>${text}`;
                    }
                },
                {
                    title: l('IsActive'),
                    data: "isActive",
                    render: function (v) {
                        return v
                            ? '<span class="badge bg-success">Còn hiệu lực</span>'
                            : '<span class="badge bg-secondary">Hết hiệu lực</span>';
                    }
                }
            ]
        })
    );

    // ✅ Load banner ngay khi vào trang
    refreshTokenBanner();

    // ✅ Auto refresh banner mỗi 60s
    var tokenBannerInterval = setInterval(function () {
        // tránh gọi khi tab bị ẩn (tiết kiệm)
        if (document.hidden) return;
        refreshTokenBanner();
    }, 60 * 1000);

    $(window).on('beforeunload', function () {
        if (tokenBannerInterval) clearInterval(tokenBannerInterval);
    });

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
            refreshTokenBanner(); // ✅ update banner sau refresh
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
        refreshTokenBanner();
    });

    editModal.onResult(function () {
        abp.notify.success(l('SavedSuccessfully'));
        table.ajax.reload();
        refreshTokenBanner();
    });

    $('#ZaloAuthTable').on('click', '.js-copy-token', function () {
        var id = $(this).data('id');
        var kind = $(this).data('kind');

        abp.ajax({
            url: '/api/host/zalo-auth/' + id + '/token?kind=' + kind,
            type: 'GET'
        }).done(async function (res) {
            await copyText(res.token);
            abp.notify.success('Copied');
        });
    });
});
