(function (window, $) {
    if (window.genoraFnbNotify) {
        return;
    }

    var connection = null;
    var initialized = false;
    var shellBound = false;
    var storageBound = false;

    var state = {
        notifications: [],
        unreadCount: 0
    };

    var options = {
        viewAllUrl: '/AppFnbOrders',
        detailUrl: function (id) { return '/AppFnbOrders/Kitchen/Detail?id=' + id; },
        notifyOnUpdate: false,
        soundOnUpdate: false,
        onCreated: null,
        onUpdated: null
    };

    var orderAudio = null;
    var audioUnlocked = false;
    var lastSoundAt = 0;

    function getUserKey() {
        try {
            var userId = window.abp?.currentUser?.id || 'anonymous';
            var tenantId = window.abp?.currentTenant?.id || 'host';
            var host = window.location?.host || 'unknown-host';
            return 'genora:fnb:notify:v3:' + host + ':' + tenantId + ':' + userId;
        } catch {
            return 'genora:fnb:notify:v3:unknown-host:host:anonymous';
        }
    }

    function getStorage() {
        try {
            return window.localStorage;
        } catch {
            return null;
        }
    }

    function pick(obj, camel, pascal) {
        return obj && (obj[camel] ?? obj[pascal]);
    }

    function normalizeStatus(value) {
        if (value === null || value === undefined) return 0;
        if (typeof value === 'number') return value;

        var s = String(value).toLowerCase();
        if (s === '1' || s === 'created') return 1;
        if (s === '2' || s === 'preparing') return 2;
        if (s === '3' || s === 'delivering') return 3;
        if (s === '4' || s === 'served') return 4;
        if (s === '5' || s === 'cancelled') return 5;

        if (s === '1' || s === 'unpaid') return 1;
        if (s === '2' || s === 'paid') return 2;
        if (s === '3' || s === 'failed') return 3;

        return Number(value) || 0;
    }

    function buildNotificationKey(item) {
        return [
            item.id || '',
            item.latestActivityTitle || '',
            item.latestActivityDescription || '',
            item.creationTime || '',
            item.itemsSummary || ''
        ].join('|');
    }

    function loadState() {
        try {
            var storage = getStorage();
            if (!storage) return;

            var raw = storage.getItem(getUserKey());
            if (!raw) return;

            var parsed = JSON.parse(raw);
            state.notifications = Array.isArray(parsed.notifications) ? parsed.notifications : [];
            state.unreadCount = Number(parsed.unreadCount || 0);
        } catch (e) {
            console.warn('Cannot load FNB notification state', e);
        }
    }

    function saveState() {
        try {
            var storage = getStorage();
            if (!storage) return;

            storage.setItem(getUserKey(), JSON.stringify({
                notifications: state.notifications,
                unreadCount: state.unreadCount
            }));
        } catch (e) {
            console.warn('Cannot save FNB notification state', e);
        }
    }

    function bindStorageSync() {
        if (storageBound) return;
        storageBound = true;

        window.addEventListener('storage', function (e) {
            if (e.key !== getUserKey()) return;
            loadState();
            renderNotifications();
        });
    }

    function normalizePayload(payload) {
        if (!payload) return null;

        var item = {
            id: pick(payload, 'id', 'Id'),
            orderCode: pick(payload, 'orderCode', 'OrderCode'),
            bagTag: pick(payload, 'bagTag', 'BagTag'),
            customerName: pick(payload, 'customerName', 'CustomerName'),
            customerPhone: pick(payload, 'customerPhone', 'CustomerPhone'),
            customerPhoneMasked: pick(payload, 'customerPhoneMasked', 'CustomerPhoneMasked'),
            customerTypeName: pick(payload, 'customerTypeName', 'CustomerTypeName'),
            customerTypeColorCode: pick(payload, 'customerTypeColorCode', 'CustomerTypeColorCode'),
            note: pick(payload, 'note', 'Note'),
            totalAmount: Number(pick(payload, 'totalAmount', 'TotalAmount') || 0),
            totalQuantity: Number(pick(payload, 'totalQuantity', 'TotalQuantity') || 0),
            creationTime: pick(payload, 'creationTime', 'CreationTime'),
            serviceStatus: normalizeStatus(pick(payload, 'serviceStatus', 'ServiceStatus')),
            paymentStatus: normalizeStatus(pick(payload, 'paymentStatus', 'PaymentStatus')),
            primaryImageUrl: pick(payload, 'primaryImageUrl', 'PrimaryImageUrl'),
            itemsSummary: pick(payload, 'itemsSummary', 'ItemsSummary'),
            itemNotesSummary: pick(payload, 'itemNotesSummary', 'ItemNotesSummary'),
            latestActivityTitle: pick(payload, 'latestActivityTitle', 'LatestActivityTitle'),
            latestActivityDescription: pick(payload, 'latestActivityDescription', 'LatestActivityDescription'),
            itemNames: pick(payload, 'itemNames', 'ItemNames') || [],
            items: pick(payload, 'items', 'Items') || [],
            isRead: !!pick(payload, 'isRead', 'IsRead')
        };

        item._key = buildNotificationKey(item);
        return item;
    }

    function ensureAudio() {
        orderAudio = document.getElementById('fnbOrderNotificationAudio');

        if (!orderAudio) {
            var audioHtml = [
                '<audio id="fnbOrderNotificationAudio" preload="auto" style="display:none;">',
                '   <source src="/sounds/notification.mp3" type="audio/mpeg" />',
                '</audio>'
            ].join('');

            $('body').append(audioHtml);
            orderAudio = document.getElementById('fnbOrderNotificationAudio');
        }

        if (orderAudio && !orderAudio.dataset.boundError) {
            orderAudio.dataset.boundError = '1';
            orderAudio.addEventListener('error', function () {
                console.error('Audio load error:', orderAudio.error, orderAudio.currentSrc);
            });
        }
    }

    function unlockAudio() {
        if (!orderAudio || audioUnlocked) return;

        try {
            orderAudio.muted = true;
            orderAudio.currentTime = 0;

            var promise = orderAudio.play();
            if (promise && typeof promise.then === 'function') {
                promise.then(function () {
                    orderAudio.pause();
                    orderAudio.currentTime = 0;
                    orderAudio.muted = false;
                    audioUnlocked = true;
                    console.log('FNB audio unlocked');
                }).catch(function (err) {
                    orderAudio.muted = false;
                    console.warn('FNB audio unlock failed:', err);
                });
            } else {
                orderAudio.pause();
                orderAudio.currentTime = 0;
                orderAudio.muted = false;
                audioUnlocked = true;
            }
        } catch (err) {
            orderAudio.muted = false;
            console.warn('FNB audio unlock exception:', err);
        }
    }

    function playSound() {
        if (!orderAudio) return;

        var now = Date.now();
        if (now - lastSoundAt < 1000) return;
        lastSoundAt = now;

        try {
            orderAudio.pause();
            orderAudio.currentTime = 0;

            var promise = orderAudio.play();
            if (promise && typeof promise.catch === 'function') {
                promise.catch(function (err) {
                    console.warn('Cannot play FNB sound:', err);
                });
            }
        } catch (err) {
            console.warn('Cannot play FNB sound:', err);
        }
    }

    function ensureNotificationShell() {
        if (!$('#FnbTopbarNotifyHost').length) {
            var shellHtml = `
                <li id="FnbTopbarNotifyHost" class="nav-item fnb-topbar-host">
                    <div class="fnb-topbar-notify">
                        <button id="FnbNotificationBell" type="button" class="fnb-bell-btn" title="Thông báo đơn hàng">
                            <i class="fa fa-bell"></i>
                            <span id="FnbNotificationBadge" class="fnb-bell-badge d-none">0</span>
                        </button>

                        <div id="FnbNotificationPanel" class="fnb-notify-panel d-none">
                            <div class="fnb-notify-panel__header">
                                <div class="fnb-notify-title">Thông báo đơn hàng</div>
                                <button id="FnbMarkAllRead" type="button" class="fnb-notify-link">Đánh dấu đã xem</button>
                            </div>

                            <div id="FnbNotificationList" class="fnb-notify-list"></div>

                            <div class="fnb-notify-panel__footer">
                                <a href="${options.viewAllUrl}" class="fnb-notify-view-all">Xem danh sách đơn hàng</a>
                            </div>
                        </div>
                    </div>
                </li>
            `;

            var $languageHost = $('#languageDropdown').closest('li, .nav-item, .dropdown');
            var $userHost = $('#userDropdown').closest('li, .nav-item, .dropdown');
            var $insertBefore = $languageHost.length ? $languageHost : $userHost;

            if ($insertBefore.length) {
                $insertBefore.before(shellHtml);
            } else {
                var $nav = $('.navbar-nav').first();
                if ($nav.length) {
                    $nav.append(shellHtml);
                }
            }
        }

        if (!$('#FnbToastContainer').length) {
            $('body').append('<div id="FnbToastContainer" class="fnb-toast-container"></div>');
        }
    }

    function closeNotificationPanel() {
        $('#FnbNotificationPanel').addClass('d-none');
    }

    function escapeHtml(text) {
        if (text === null || text === undefined) return '';
        return $('<div/>').text(text).html();
    }

    function formatDateTime(value) {
        if (window.genoraFnb && typeof window.genoraFnb.formatDateTime === 'function') {
            return window.genoraFnb.formatDateTime(value);
        }

        if (!value) return '';
        try {
            var d = new Date(value);
            if (isNaN(d.getTime())) return value;

            var hh = String(d.getHours()).padStart(2, '0');
            var mm = String(d.getMinutes()).padStart(2, '0');
            var dd = String(d.getDate()).padStart(2, '0');
            var MM = String(d.getMonth() + 1).padStart(2, '0');
            var yyyy = d.getFullYear();

            return hh + ':' + mm + ' ' + dd + '/' + MM + '/' + yyyy;
        } catch {
            return value;
        }
    }

    function formatPhone(phoneMasked, phone) {
        return phoneMasked || phone || '';
    }

    function formatItems(items) {
        if (!items || !items.length) return '';
        return items.map(function (x) { return x.itemName + ' x' + x.quantity; }).join(', ');
    }

    function buildItemNotes(items) {
        if (!items || !items.length) return '';
        var notes = items
            .filter(function (x) { return x.note; })
            .map(function (x) { return x.itemName + ': ' + x.note; });
        return notes.join(' • ');
    }

    function renderNotificationItem(item) {
        var customer = [item.customerName, formatPhone(item.customerPhoneMasked, item.customerPhone), item.bagTag]
            .filter(Boolean)
            .join(' • ');

        var imageUrl = item.primaryImageUrl || '/images/fnb/default-food.png';
        var itemNotes = item.itemNotesSummary || buildItemNotes(item.items);
        var title = item.latestActivityTitle
            ? escapeHtml(item.latestActivityTitle)
            : ('Đơn mới #' + escapeHtml(item.orderCode || ''));

        return `
            <div class="fnb-notify-item ${item.isRead ? '' : 'unread'}" data-id="${item.id}">
                <div class="fnb-notify-thumb">
                    <img src="${imageUrl}" alt="food" onerror="this.src='/images/fnb/default-food.png'" />
                </div>
                <div>
                    <div class="fnb-notify-title-row">
                        <div class="fnb-notify-item-title">${title}</div>
                        <div class="fnb-notify-time">${formatDateTime(item.creationTime)}</div>
                    </div>

                    <div class="fnb-notify-meta">${escapeHtml(customer)}</div>
                    <div class="fnb-notify-text">${escapeHtml(item.itemsSummary || formatItems(item.items))}</div>

                    ${item.note ? `<div class="fnb-notify-note"><strong>Ghi chú khách:</strong> ${escapeHtml(item.note)}</div>` : ''}
                    ${itemNotes ? `<div class="fnb-notify-note"><strong>Ghi chú món:</strong> ${escapeHtml(itemNotes)}</div>` : ''}
                </div>
            </div>
        `;
    }

    function renderNotifications() {
        var html = state.notifications.length
            ? state.notifications.map(renderNotificationItem).join('')
            : '<div class="p-3 text-muted">Chưa có thông báo mới.</div>';

        $('#FnbNotificationList').html(html);

        $('#FnbNotificationBadge')
            .text(state.unreadCount)
            .toggleClass('d-none', state.unreadCount <= 0);
    }

    function showNotificationToast(item) {
        var imageUrl = item.primaryImageUrl || '/images/fnb/default-food.png';
        var title = item.latestActivityTitle || ('Đơn mới #' + (item.orderCode || ''));

        var html = `
            <div class="fnb-toast" data-id="${item.id}">
                <div class="d-flex gap-3">
                    <img src="${imageUrl}" style="width:56px;height:56px;border-radius:12px;object-fit:cover;" onerror="this.src='/images/fnb/default-food.png'" />
                    <div class="flex-grow-1">
                        <div style="font-weight:700;color:#111827;">${escapeHtml(title)}</div>
                        <div style="font-size:12px;color:#475569;">${escapeHtml(item.customerName || '')} • ${escapeHtml(item.customerPhoneMasked || item.customerPhone || '')}</div>
                        <div style="font-size:12px;color:#64748b;margin-top:4px;">${escapeHtml(item.itemsSummary || '')}</div>
                    </div>
                </div>
            </div>
        `;

        var $toast = $(html);
        $('#FnbToastContainer').prepend($toast);

        setTimeout(function () {
            $toast.fadeOut(250, function () {
                $(this).remove();
            });
        }, 7000);
    }

    function pushNotification(payload, playBell) {
        var item = normalizePayload(payload);
        if (!item || !item.id) return;

        var existed = state.notifications.find(function (x) {
            return x._key === item._key;
        });

        if (existed) {
            return;
        }

        item.isRead = false;
        state.notifications.unshift(item);

        if (state.notifications.length > 50) {
            state.notifications = state.notifications.slice(0, 50);
        }

        state.unreadCount++;
        saveState();
        renderNotifications();
        showNotificationToast(item);

        if (playBell) {
            playSound();
        }
    }

    function markAllRead() {
        state.notifications.forEach(function (x) { x.isRead = true; });
        state.unreadCount = 0;
        saveState();
        renderNotifications();
    }

    function markNotificationRead(id) {
        var item = state.notifications.find(function (x) {
            return String(x.id) === String(id);
        });

        if (!item || item.isRead) return;

        item.isRead = true;
        state.unreadCount = Math.max(0, state.unreadCount - 1);
        saveState();
        renderNotifications();
    }

    function bindShellEvents() {
        if (shellBound) return;
        shellBound = true;

        document.addEventListener('click', unlockAudio, { once: true });
        document.addEventListener('keydown', unlockAudio, { once: true });
        document.addEventListener('touchstart', unlockAudio, { once: true });

        $(document).on('click', '#FnbNotificationBell', function (e) {
            e.preventDefault();
            e.stopPropagation();
            $('#FnbNotificationPanel').toggleClass('d-none');
        });

        $(document).on('click', '#FnbMarkAllRead', function (e) {
            e.preventDefault();
            markAllRead();
        });

        $(document).on('click', function (e) {
            if (!$(e.target).closest('#FnbTopbarNotifyHost').length) {
                closeNotificationPanel();
            }
        });

        $(document).on('keydown', function (e) {
            if (e.key === 'Escape') {
                closeNotificationPanel();
            }
        });

        $(document).on('click', '.fnb-notify-item, .fnb-toast', function () {
            var id = $(this).data('id');
            markNotificationRead(id);
            window.location.href = options.detailUrl(id);
        });
    }

    function startRealtime() {
        if (connection) return;

        if (!window.signalR || !window.signalR.HubConnectionBuilder) {
            console.warn('SignalR client is not loaded for FNB notify.');
            return;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/signalr-hubs/fnb-orders")
            .withAutomaticReconnect()
            .build();

        connection.on("fnb.order.created", function (payload) {
            var item = normalizePayload(payload);
            pushNotification(item, true);

            if (typeof options.onCreated === 'function') {
                options.onCreated(item);
            }
        });

        connection.on("fnb.order.updated", function (payload) {
            var item = normalizePayload(payload);

            if (options.notifyOnUpdate) {
                pushNotification(item, !!options.soundOnUpdate);
            }

            if (typeof options.onUpdated === 'function') {
                options.onUpdated(item);
            }
        });

        connection.start()
            .then(function () {
                console.log('FNB shared SignalR connected');
            })
            .catch(function (err) {
                console.error('FNB shared SignalR error:', err);
            });
    }

    function init(userOptions) {
        options = $.extend({}, options, userOptions || {});

        if (!initialized) {
            loadState();
            ensureAudio();
            ensureNotificationShell();
            bindShellEvents();
            bindStorageSync();
            renderNotifications();
            startRealtime();
            initialized = true;
        } else {
            ensureNotificationShell();
            renderNotifications();
            startRealtime();
        }

        return {
            invokePingBell: function () {
                if (!connection) {
                    return Promise.reject('SignalR not started');
                }
                return connection.invoke("PingBell");
            },
            markAllRead: markAllRead,
            closePanel: closeNotificationPanel
        };
    }

    window.genoraFnbNotify = {
        init: init
    };
})(window, window.jQuery);