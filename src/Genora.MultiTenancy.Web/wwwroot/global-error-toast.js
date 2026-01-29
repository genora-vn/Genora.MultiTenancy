(function () {
    if (!window.abp) return;

    window.__genoraToastInstalled = true;

    window.__genoraPatchStatus = {
        notifyWrapped: false,
        ajaxShowErrorWrapped: false,
        messageErrorWrapped: false,
        jqueryAjaxErrorWrapped: false,
    };

    window.__genoraLastDetailedErrorAt = 0;

    var l = abp.localization.getResource("MultiTenancy");

    function pick(obj, keys) {
        if (!obj) return null;
        for (var i = 0; i < keys.length; i++) {
            var k = keys[i];
            if (obj[k] !== undefined && obj[k] !== null) return obj[k];
        }
        return null;
    }

    function safeText(x) {
        if (x === undefined || x === null) return "";
        if (typeof x === "string") return x.trim();
        try { return JSON.stringify(x); } catch (_) { return String(x); }
    }

    function normalizeData(data) {
        if (typeof data === "string") {
            try { return JSON.parse(data); } catch (_) { return {}; }
        }
        if (!data || typeof data !== "object") return {};
        return data;
    }

    function buildMessageText(err) {
        err = err || {};
        var data = normalizeData(err.data);

        var row = pick(data, ["RowNumber", "rowNumber", "row", "Row"]);
        var exMsg = pick(data, ["ExceptionMessage", "exceptionMessage"]);

        var base = safeText(err.message) || "Có lỗi xảy ra";

        var detail = "";
        if (row !== null && row !== undefined && String(row).trim() !== "") {
            detail = exMsg ? ("Dòng " + row + ": " + safeText(exMsg)) : ("Dòng " + row);
        } else if (exMsg) {
            detail = safeText(exMsg);
        }

        return detail ? (base + "\n" + detail) : base;
    }

    var _lastToastAt = 0;
    var _lastToastKey = "";
    var _pendingBase = null;

    function normKey(text) {
        return safeText(text).toLowerCase().replace(/\s+/g, " ").trim();
    }

    function cancelPendingBaseIfSame(baseKey) {
        if (!_pendingBase) return;
        if (_pendingBase.baseKey === baseKey) {
            clearTimeout(_pendingBase.timer);
            _pendingBase = null;
        }
    }

    function wrapNotifyOnce() {
        if (!abp.notify || !abp.notify.error) return false;
        if (abp.notify.__genoraWrapped) return true;

        var _orig = abp.notify.error.bind(abp.notify);

        abp.notify.__genoraOrigError = _orig;

        abp.notify.error = function (message, title, options) {
            var msg = (message || "").toString();
            var key = normKey(msg);
            var now = Date.now();

            try {
                var lower = msg.toLowerCase();
                var justDetailed = (now - (window.__genoraLastDetailedErrorAt || 0)) < 2500;
                if (justDetailed && lower.indexOf("import excel thất bại") > -1) {
                    return;
                }
            } catch (_) { }

            if (key && key === _lastToastKey && (now - _lastToastAt) < 1200) return;

            var looksLikeBase =
                msg &&
                msg.indexOf("\n") === -1 &&
                msg.toLowerCase().indexOf("dòng") === -1 &&
                msg.toLowerCase().indexOf("row") === -1;

            if (looksLikeBase) {
                if (_pendingBase) {
                    clearTimeout(_pendingBase.timer);
                    _pendingBase = null;
                }

                _pendingBase = {
                    baseKey: key,
                    timer: setTimeout(function () {
                        if (!_pendingBase || _pendingBase.baseKey !== key) return;
                        _pendingBase = null;

                        _lastToastKey = key;
                        _lastToastAt = Date.now();
                        _orig(msg, title, options);
                    }, 200)
                };

                return;
            }

            _lastToastKey = key;
            _lastToastAt = now;
            return _orig(msg, title, options);
        };

        abp.notify.__genoraWrapped = true;
        window.__genoraPatchStatus.notifyWrapped = true;
        return true;
    }

    function wrapMessageOnce() {
        if (!abp.message || !abp.message.error) return false;
        if (abp.message.__genoraWrapped) return true;

        abp.message.error = function (message, title, options) {
            var text = (typeof message === "string")
                ? message
                : (message && message.message) || "Có lỗi xảy ra";

            abp.notify.error(text);
            return Promise.resolve();
        };

        abp.message.__genoraWrapped = true;
        window.__genoraPatchStatus.messageErrorWrapped = true;
        return true;
    }

    function showDetailedToast(text) {
        window.__genoraLastDetailedErrorAt = Date.now();

        var orig = abp.notify && abp.notify.__genoraOrigError;
        if (typeof orig === "function") {
            orig(text);
            return;
        }

        abp.notify.error(text);
    }

    function wrapAjaxShowErrorOnce() {
        if (!abp.ajax || !abp.ajax.showError) return false;
        if (abp.ajax.__genoraWrapped) return true;

        var _origShowError = abp.ajax.showError;

        abp.ajax.showError = function (jqXHR, userOptions) {
            var status = jqXHR && jqXHR.status;

            if (status === 401) {
                return _origShowError.apply(this, arguments);
            }

            var payload = null;
            try {
                payload = jqXHR && (jqXHR.responseJSON || jqXHR.responseText);
                if (typeof payload === "string") {
                    try { payload = JSON.parse(payload); } catch (_) { }
                }
            } catch (_) { }

            var err = payload && payload.error ? payload.error : null;

            if (err) {
                var text = buildMessageText(err);

                cancelPendingBaseIfSame(normKey(safeText(err.message)));

                _lastToastKey = normKey(text);
                _lastToastAt = Date.now();

                window.__genoraLastToast = { status: status, err: err, text: text, payload: payload, jqId: jqXHR };

                showDetailedToast(text);
                return;
            }

            return _origShowError.apply(this, arguments);
        };

        abp.ajax.__genoraWrapped = true;
        window.__genoraPatchStatus.ajaxShowErrorWrapped = true;
        return true;
    }

    function wrapJqueryAjaxErrorOnce() {
        if (!window.jQuery) return false;
        if (window.__genoraJqAjaxErrorWrapped) return true;

        jQuery(document).ajaxError(function (_evt, jqXHR, _settings, _thrown) {
            try {
                if (window.__genoraLastToast && window.__genoraLastToast.jqId === jqXHR) return;

                var payload = jqXHR && (jqXHR.responseJSON || jqXHR.responseText);
                if (typeof payload === "string") {
                    try { payload = JSON.parse(payload); } catch (_) { payload = null; }
                }

                var err = payload && payload.error ? payload.error : null;
                if (!err) return;

                var text = buildMessageText(err);

                cancelPendingBaseIfSame(normKey(safeText(err.message)));

                var key = normKey(text);
                var now = Date.now();
                if (key && key === _lastToastKey && (now - _lastToastAt) < 1200) return;

                _lastToastKey = key;
                _lastToastAt = now;

                window.__genoraLastToast = { status: jqXHR.status, err: err, text: text, payload: payload, jqId: jqXHR };

                showDetailedToast(text);
            } catch (_) { }
        });

        window.__genoraJqAjaxErrorWrapped = true;
        window.__genoraPatchStatus.jqueryAjaxErrorWrapped = true;
        return true;
    }

    function install() {
        wrapNotifyOnce();
        wrapMessageOnce();
        wrapAjaxShowErrorOnce();
        wrapJqueryAjaxErrorOnce();
    }

    install();

    var tries = 0;
    var timer = setInterval(function () {
        tries++;
        install();

        var ok = window.__genoraPatchStatus.notifyWrapped &&
            window.__genoraPatchStatus.ajaxShowErrorWrapped &&
            window.__genoraPatchStatus.messageErrorWrapped &&
            (window.__genoraPatchStatus.jqueryAjaxErrorWrapped || !window.jQuery);

        if (ok || tries >= 30) clearInterval(timer);
    }, 200);
})();
