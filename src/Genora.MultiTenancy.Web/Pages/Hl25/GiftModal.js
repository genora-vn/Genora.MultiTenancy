(function () {
    window.genora = window.genora || {};
    var moneyPattern = /^(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d{1,2})?$/;

    function normalizeMoney(value) {
        return String(value || '').trim().replace(/\./g, '').replace(',', '.');
    }

    function formatMoney(value) {
        var raw = String(value || '').trim();
        if (!raw || !moneyPattern.test(raw)) return raw;
        var parts = raw.replace(/\./g, '').split(',');
        var integer = parts[0].replace(/^0+(?=\d)/, '');
        var fraction = parts[1] ? parts[1].replace(/0+$/, '') : '';
        return integer.replace(/\B(?=(\d{3})+(?!\d))/g, '.') + (fraction ? ',' + fraction : '');
    }

    function patchMoneyValidator() {
        if (!$.validator || $.validator.hl25MoneyPatched) return;
        ['number', 'range'].forEach(function (name) {
            var original = $.validator.methods[name];
            $.validator.methods[name] = function (value, element, param) {
                if ($(element).hasClass('hl25-money-input')) value = normalizeMoney(value);
                return original.call(this, value, element, param);
            };
        });
        $.validator.addMethod('hl25Vnd', function (value, element) {
            return this.optional(element) || moneyPattern.test(value.trim());
        });
        $.validator.hl25MoneyPatched = true;
    }

    function init(selector) {
        var $form = $(selector);
        var l = abp.localization.getResource('MultiTenancy');
        patchMoneyValidator();
        var $money = $form.find('.hl25-money-input');
        $money.val(formatMoney($money.val()));
        $money.on('blur', function () { this.value = formatMoney(this.value); });
        if ($.validator) {
            if ($.validator.unobtrusive) $.validator.unobtrusive.parse($form);
            $form.validate();
            $money.rules('add', { hl25Vnd: true, messages: { hl25Vnd: l('Hl25:InvalidGiftValue') } });
        }

        $form.find('.hl25-gift-image').each(function () {
            var $card = $(this), $file = $card.find('.hl25-image-file'), $url = $card.find('.hl25-image-url');
            var version = 0;
            function preview(url) {
                $card.find('.hl25-image-preview').attr('src', url || '').toggleClass('d-none', !url);
                $card.find('.hl25-image-empty').toggleClass('d-none', !!url);
            }
            $url.on('input', function () { version++; $file.val(''); preview(this.value.trim()); });
            $file.on('change', function () {
                var file = this.files && this.files[0];
                var current = ++version;
                if (!file) { preview($url.val()); return; }
                if (file.size > 5 * 1024 * 1024 || !/^image\/(png|jpeg)$/.test(file.type)) {
                    abp.message.warn(l(file.size > 5 * 1024 * 1024 ? 'Hl25:AssetTooLarge' : 'Hl25:InvalidGiftImage'));
                    this.value = ''; preview($url.val()); return;
                }
                var reader = new FileReader();
                reader.onload = function (event) { if (version === current) preview(event.target.result); };
                reader.readAsDataURL(file);
            });
            $card.find('.hl25-image-clear').on('click', function () {
                version++; $file.val(''); $url.val(''); preview('');
            });
        });
    }

    genora.hl25GiftModal = { init: init, formatMoney: formatMoney, normalizeMoney: normalizeMoney };
})();
