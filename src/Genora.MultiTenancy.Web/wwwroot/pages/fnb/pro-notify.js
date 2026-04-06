(function (window, $) {
    if (window.genoraProNotify) {
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
        viewAllUrl: '/AppProOrders',
        detailUrl: function (id) { return '/AppProOrders/Board/Detail?id=' + id; },
        notifyOnUpdate: false,
        soundOnUpdate: false,
        onCreated: null,
        onUpdated: null
    };

    var orderAudio = null;
    var audioUnlocked = false;
    var lastSoundAt = 0;

    // ── Storage ──────────────────────────────────────────────────────────────

    function getUserKey() {
        try {
            var userId   = window.abp?.currentUser?.id   || 'anonymous';
            var tenantId = window.abp?.currentTenant?.id || 'host';
            var host     = window.location?.host         || 'unknown-host';
            return 'genora:pro:notify:v1:' + host + ':' + tenantId + ':' + userId;
        } catch {
            return 'genora:pro:notify:v1:unknown-host:host:anonymous';
        }
    }

    function getStorage() {
        try { return window.localStorage; } catch { return null; }
    }

    function loadState() {
        try {
            var raw = getStorage()?.getItem(getUserKey());
            if (!raw) return;
            var parsed = JSON.parse(raw);
            state.notifications = Array.isArray(parsed.notifications) ? parsed.notifications : [];
            state.unreadCount   = Number(parsed.unreadCount || 0);
        } catch (e) { console.warn('Cannot load PRO notification state', e); }
    }

    function saveState() {
        try {
            getStorage()?.setItem(getUserKey(), JSON.stringify({
                notifications: state.notifications,
                unreadCount:   state.unreadCount
            }));
        } catch (e) { console.warn('Cannot save PRO notification state', e); }
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

    // ── Payload normalize ────────────────────────────────────────────────────

    function pick(obj, camel, pascal) {
        return obj && (obj[camel] ?? obj[pascal]);
    }

    function normalizeStatus(value) {
        if (value === null || value === undefined) return 0;
        if (typeof value === 'number') return value;
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

    function normalizePayload(payload) {
        if (!payload) return null;
        var item = {
            id:                       pick(payload, 'id', 'Id'),
            orderCode:                pick(payload, 'orderCode', 'OrderCode'),
            bagTag:                   pick(payload, 'bagTag', 'BagTag'),
            customerName:             pick(payload, 'customerName', 'CustomerName'),
            customerPhone:            pick(payload, 'customerPhone', 'CustomerPhone'),
            customerPhoneMasked:      pick(payload, 'customerPhoneMasked', 'CustomerPhoneMasked'),
            note:                     pick(payload, 'note', 'Note'),
            totalAmount:              Number(pick(payload, 'totalAmount', 'TotalAmount') || 0),
            totalQuantity:            Number(pick(payload, 'totalQuantity', 'TotalQuantity') || 0),
            creationTime:             pick(payload, 'creationTime', 'CreationTime'),
            serviceStatus:            normalizeStatus(pick(payload, 'serviceStatus', 'ServiceStatus')),
            paymentStatus:            normalizeStatus(pick(payload, 'paymentStatus', 'PaymentStatus')),
            primaryImageUrl:          pick(payload, 'primaryImageUrl', 'PrimaryImageUrl'),
            itemsSummary:             pick(payload, 'itemsSummary', 'ItemsSummary'),
            itemNotesSummary:         pick(payload, 'itemNotesSummary', 'ItemNotesSummary'),
            latestActivityTitle:      pick(payload, 'latestActivityTitle', 'LatestActivityTitle'),
            latestActivityDescription:pick(payload, 'latestActivityDescription', 'LatestActivityDescription'),
            items:                    pick(payload, 'items', 'Items') || [],
            isRead:                   !!pick(payload, 'isRead', 'IsRead')
        };
        item._key = buildNotificationKey(item);
        return item;
    }

    // ── Audio ────────────────────────────────────────────────────────────────

    function ensureAudio() {
        orderAudio = document.getElementById('proOrderNotificationAudio');
        if (!orderAudio) {
            $('body').append(
                '<audio id="proOrderNotificationAudio" preload="auto" style="display:none;">' +
                '  <source src="/sounds/notification.mp3" type="audio/mpeg" />' +
                '</audio>'
            );
            orderAudio = document.getElementById('proOrderNotificationAudio');
        }
    }

    function unlockAudio() {
        if (!orderAudio || audioUnlocked) return;
        try {
            orderAudio.muted = true;
            orderAudio.currentTime = 0;
            var p = orderAudio.play();
            if (p && typeof p.then === 'function') {
                p.then(function () {
                    orderAudio.pause();
                    orderAudio.currentTime = 0;
                    orderAudio.muted = false;
                    audioUnlocked = true;
                }).catch(function () { orderAudio.muted = false; });
            } else {
                orderAudio.pause();
                orderAudio.currentTime = 0;
                orderAudio.muted = false;
                audioUnlocked = true;
            }
        } catch (err) { orderAudio.muted = false; }
    }

    function playSound() {
        if (!orderAudio) return;
        var now = Date.now();
        if (now - lastSoundAt < 1000) return;
        lastSoundAt = now;
        try {
            orderAudio.pause();
            orderAudio.currentTime = 0;
            var p = orderAudio.play();
            if (p && typeof p.catch === 'function') {
                p.catch(function (err) { console.warn('Cannot play PRO sound:', err); });
            }
        } catch (err) { console.warn('Cannot play PRO sound:', err); }
    }

    // ── Notification shell ───────────────────────────────────────────────────

    function ensureNotificationShell() {
        if (!$('#ProTopbarNotifyHost').length) {
            var shellHtml = `
                <li id="ProTopbarNotifyHost" class="nav-item fnb-topbar-host">
                    <div class="fnb-topbar-notify">
                        <button id="ProNotificationBell" type="button" class="fnb-bell-btn pro-bell-btn" title="Thông báo đơn Proshop">
                            <i class="fa fa-shopping-bag"></i>
                            <span id="ProNotificationBadge" class="fnb-bell-badge d-none">0</span>
                        </button>

                        <div id="ProNotificationPanel" class="fnb-notify-panel d-none">
                            <div class="fnb-notify-panel__header">
                                <div class="fnb-notify-title">Thông báo đơn Proshop</div>
                                <button id="ProMarkAllRead" type="button" class="fnb-notify-link">Đánh dấu đã xem</button>
                            </div>

                            <div id="ProNotificationList" class="fnb-notify-list"></div>

                            <div class="fnb-notify-panel__footer">
                                <a href="${options.viewAllUrl}" class="fnb-notify-view-all">Xem danh sách đơn Proshop</a>
                            </div>
                        </div>
                    </div>
                </li>
            `;

            // Chèn trước FnB bell hoặc trước language/user dropdown
            var $fnbHost  = $('#FnbTopbarNotifyHost');
            var $langHost = $('#languageDropdown').closest('li, .nav-item, .dropdown');
            var $insertBefore = $fnbHost.length ? $fnbHost : $langHost;

            if ($insertBefore.length) {
                $insertBefore.before(shellHtml);
            } else {
                $('.navbar-nav').first().append(shellHtml);
            }
        }

        if (!$('#ProToastContainer').length) {
            $('body').append('<div id="ProToastContainer" class="fnb-toast-container pro-toast-container"></div>');
        }
    }

    function closeNotificationPanel() {
        $('#ProNotificationPanel').addClass('d-none');
    }

    // ── Render ───────────────────────────────────────────────────────────────

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
            var d  = new Date(value);
            if (isNaN(d.getTime())) return value;
            var hh = String(d.getHours()).padStart(2, '0');
            var mm = String(d.getMinutes()).padStart(2, '0');
            var dd = String(d.getDate()).padStart(2, '0');
            var MM = String(d.getMonth() + 1).padStart(2, '0');
            return hh + ':' + mm + ' ' + dd + '/' + MM + '/' + d.getFullYear();
        } catch { return value; }
    }

    function buildItemNotes(items) {
        if (!items || !items.length) return '';
        var notes = items
            .filter(function (x) { return x.note; })
            .map(function (x) { return x.itemName + ': ' + x.note; });
        return notes.join(' • ');
    }

    function renderNotificationItem(item) {
        var customer  = [item.customerName, item.customerPhoneMasked || item.customerPhone, item.bagTag]
            .filter(Boolean).join(' • ');
        var imageUrl  = item.primaryImageUrl || '/images/fnb/default-food.png';
        var title     = item.latestActivityTitle
            ? escapeHtml(item.latestActivityTitle)
            : ('Đơn Proshop #' + escapeHtml(item.orderCode || ''));
        var itemNotes = item.itemNotesSummary || buildItemNotes(item.items);

        return `
            <div class="fnb-notify-item ${item.isRead ? '' : 'unread'}" data-id="${item.id}" data-type="pro">
                <div class="fnb-notify-thumb">
                    <img src="${imageUrl}" alt="pro" onerror="this.src='/images/fnb/default-food.png'" />
                </div>
                <div>
                    <div class="fnb-notify-title-row">
                        <div class="fnb-notify-item-title">${title}</div>
                        <div class="fnb-notify-time">${formatDateTime(item.creationTime)}</div>
                    </div>
                    <div class="fnb-notify-meta">${escapeHtml(customer)}</div>
                    <div class="fnb-notify-text">${escapeHtml(item.itemsSummary || '')}</div>
                    ${item.note ? `<div class="fnb-notify-note"><strong>Ghi chú khách:</strong> ${escapeHtml(item.note)}</div>` : ''}
                    ${itemNotes ? `<div class="fnb-notify-note"><strong>Ghi chú món:</strong> ${escapeHtml(itemNotes)}</div>` : ''}
                </div>
            </div>
        `;
    }

    function renderNotifications() {
        var html = state.notifications.length
            ? state.notifications.map(renderNotificationItem).join('')
            : '<div class="p-3 text-muted">Chưa có thông báo đơn Proshop.</div>';

        $('#ProNotificationList').html(html);
        $('#ProNotificationBadge')
            .text(state.unreadCount)
            .toggleClass('d-none', state.unreadCount <= 0);
    }

    function showNotificationToast(item) {
        var imageUrl = item.primaryImageUrl || '/images/fnb/default-food.png';
        var title    = item.latestActivityTitle || ('Đơn Proshop #' + (item.orderCode || ''));

        var html = `
            <div class="fnb-toast pro-toast" data-id="${item.id}" data-type="pro">
                <div class="d-flex gap-3">
                    <img src="${imageUrl}" style="width:56px;height:56px;border-radius:12px;object-fit:cover;" onerror="this.src='/images/fnb/default-food.png'" />
                    <div class="flex-grow-1">
                        <div style="font-weight:700;color:#111827;">${escapeHtml(title)}</div>
                        <div style="font-size:12px;color:#475569;">${escapeHtml(item.customerName || '')} • ${escapeHtml(item.customerPhoneMasked || item.customerPhone || '')} • BagTag: ${escapeHtml(item.bagTag || '')}</div>
                        <div style="font-size:12px;color:#64748b;margin-top:4px;">${escapeHtml(item.itemsSummary || '')}</div>
                    </div>
                </div>
            </div>
        `;

        var $toast = $(html);
        $('#ProToastContainer').prepend($toast);
        setTimeout(function () { $toast.fadeOut(250, function () { $(this).remove(); }); }, 7000);
    }

    function pushNotification(payload, playBell) {
        var item = normalizePayload(payload);
        if (!item || !item.id) return;

        var existed = state.notifications.find(function (x) { return x._key === item._key; });
        if (existed) return;

        item.isRead = false;
        state.notifications.unshift(item);
        if (state.notifications.length > 50) state.notifications = state.notifications.slice(0, 50);

        state.unreadCount++;
        saveState();
        renderNotifications();
        showNotificationToast(item);
        if (playBell) playSound();
    }

    function markAllRead() {
        state.notifications.forEach(function (x) { x.isRead = true; });
        state.unreadCount = 0;
        saveState();
        renderNotifications();
    }

    function markNotificationRead(id) {
        var item = state.notifications.find(function (x) { return String(x.id) === String(id); });
        if (!item || item.isRead) return;
        item.isRead = true;
        state.unreadCount = Math.max(0, state.unreadCount - 1);
        saveState();
        renderNotifications();
    }

    // ── Event binding ────────────────────────────────────────────────────────

    function bindShellEvents() {
        if (shellBound) return;
        shellBound = true;

        document.addEventListener('click',      unlockAudio, { once: true });
        document.addEventListener('keydown',    unlockAudio, { once: true });
        document.addEventListener('touchstart', unlockAudio, { once: true });

        $(document).on('click', '#ProNotificationBell', function (e) {
            e.preventDefault();
            e.stopPropagation();
            $('#ProNotificationPanel').toggleClass('d-none');
        });

        $(document).on('click', '#ProMarkAllRead', function (e) {
            e.preventDefault();
            markAllRead();
        });

        $(document).on('click', function (e) {
            if (!$(e.target).closest('#ProTopbarNotifyHost').length) {
                closeNotificationPanel();
            }
        });

        $(document).on('keydown', function (e) {
            if (e.key === 'Escape') closeNotificationPanel();
        });

        $(document).on('click', '.fnb-notify-item[data-type="pro"], .pro-toast', function () {
            var id = $(this).data('id');
            markNotificationRead(id);
            window.location.href = options.detailUrl(id);
        });
    }

    // ── SignalR ──────────────────────────────────────────────────────────────

    function startRealtime() {
        if (connection) return;

        if (!window.signalR || !window.signalR.HubConnectionBuilder) {
            console.warn('SignalR client is not loaded for PRO notify.');
            return;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl('/signalr-hubs/pro-orders')
            .withAutomaticReconnect()
            .build();

        connection.on('pro.order.created', function (payload) {
            var item = normalizePayload(payload);
            pushNotification(item, true);
            if (typeof options.onCreated === 'function') options.onCreated(item);
        });

        connection.on('pro.order.updated', function (payload) {
            var item = normalizePayload(payload);
            if (options.notifyOnUpdate) pushNotification(item, !!options.soundOnUpdate);
            if (typeof options.onUpdated === 'function') options.onUpdated(item);
        });

        connection.start()
            .then(function () { console.log('PRO SignalR connected'); })
            .catch(function (err) { console.error('PRO SignalR error:', err); });
    }

    // ── Public API ───────────────────────────────────────────────────────────

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
                if (!connection) return Promise.reject('SignalR not started');
                return connection.invoke('PingBell');
            },
            markAllRead:  markAllRead,
            closePanel:   closeNotificationPanel
        };
    }

    window.genoraProNotify = { init: init };

})(window, window.jQuery);
