const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

function boot() {
    const elements = new Map(), pending = [], exports = [], errors = [];
    let busy = false, valid = true, dates = { dateFrom: '2026-09-25', dateTo: '2026-09-30' };
    const document = { getElementById: () => ({ reportValidity: () => valid, reset() {} }) };
    const html = value => String(value).replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;').replaceAll("'", '&#39;');
    function $(key) {
        if (typeof key === 'function') { key(); return; }
        if (key === '<span>') return { text(value) { return { html: () => html(value) }; } };
        if (!elements.has(key)) elements.set(key, { value: '', content: '', props: {}, events: {},
            val(value) { if (value === undefined) return this.value; this.value = value; return this; },
            text(value) { this.content = value; return this; }, empty() { this.content = ''; return this; },
            append(value) { this.content += value; return this; },
            prop(name, value) { this.props[name] = value; return this; },
            on(name, selector, handler) { this.events[name] = handler || selector; return this; }
        });
        return elements.get(key);
    }
    $('#PageSize').val('20');
    const service = { getList(input) {
        const request = { input: JSON.parse(JSON.stringify(input)), resolve() {}, reject() {}, finish() {} };
        const chain = { then(fn) { request.resolve = fn; return chain; }, catch(fn) { request.reject = fn; return chain; },
            always(fn) { request.finish = fn; return chain; } };
        pending.push(request); return chain;
    }};
    const context = { $, document, bootstrap: { Modal: class { show() {} } },
        genora: { multiTenancy: { appServices: { hoaLinh: { hlGiftReceiptAdmin: service } } },
            hoaLinhSales: { dates: () => dates && { ...dates }, download: (route, input) => exports.push({ route, input }) } },
        abp: { localization: { getResource: () => key => key, currentCulture: { name: 'vi-VN' } },
            ui: { setBusy: () => { busy = true; }, clearBusy: () => { busy = false; } },
            notify: { error: value => errors.push(value) } }
    };
    vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../src/Genora.MultiTenancy.Web/Pages/HoaLinh/GiftReceipts/index.js'), 'utf8'), context);
    return { $, pending, exports, errors, busy: () => busy, invalidate: () => { valid = false; },
        invalidDates: () => { dates = null; }, click: id => $(id).events.click.call($(id)),
        submit: () => $('#ReceiptFilters').events.submit({ preventDefault() {} }) };
}

test('list and Excel share all filters; Excel does not export only the current page', () => {
    const app = boot();
    for (const [id, value] of Object.entries({ FilterText: ' Nhà thuốc ', FilterCustCode: ' C01 ', FilterPhone: '0900000001',
        FilterCampaign: 'GIFT25NAM', FilterVoucher: 'QT34', FilterPeriod: '0', FilterStatus: '1' })) app.$('#' + id).val(value);
    app.submit(); app.click('#BtnExportExcel');
    const { skipCount, maxResultCount, ...list } = app.pending.at(-1).input;
    assert.equal(skipCount, 0); assert.equal(maxResultCount, 20);
    assert.deepEqual(JSON.parse(JSON.stringify(app.exports[0].input)), list);
    assert.equal(list.campaignPeriod, 0); assert.equal(list.status, 1);
    assert.equal(list.filter, 'Nhà thuốc'); assert.equal(list.dateTo, '2026-09-30');
    assert.equal(app.exports[0].route, 'api/app/hl-sales-excel/gift-receipts');
});

test('invalid dates or invalid form prevent both list requests and Excel export', () => {
    for (const invalidate of ['invalidate', 'invalidDates']) {
        const app = boot(); app[invalidate](); app.submit(); app.click('#BtnExportExcel');
        assert.equal(app.pending.length, 1); assert.equal(app.exports.length, 0);
    }
});

test('stale responses cannot overwrite current results or clear its loading indicator', () => {
    const app = boot(); app.submit();
    app.pending[0].resolve({ totalCount: 999, items: [] }); app.pending[0].finish();
    assert.equal(app.$('#ReceiptCount').content, ''); assert.equal(app.busy(), true);
    app.pending[1].resolve({ totalCount: 0, items: [] }); app.pending[1].finish();
    assert.equal(app.$('#ReceiptCount').content, 'HlGiftReceipt:Total: 0'); assert.equal(app.busy(), false);
    assert.equal(app.$('#BtnNext').props.disabled, true);
});

test('failed list request clears loading and displays an error', () => {
    const app = boot(); app.pending[0].reject(); app.pending[0].finish();
    assert.equal(app.busy(), false); assert.deepEqual(app.errors, ['HlGiftReceipt:LoadError']);
});

test('snapshot values are escaped before rendering the history table', () => {
    const app = boot(); app.pending[0].resolve({ totalCount: 1, items: [{ id: '1', custName: '<img src=x onerror=alert(1)>',
        address: '<script>alert(1)</script>', voucherName: '<b>Gift</b>', quantity: 1 }] });
    const table = app.$('#ReceiptTable tbody').content;
    assert.ok(table.includes('&lt;img')); assert.ok(table.includes('&lt;script&gt;'));
    assert.ok(!table.includes('<script>')); assert.ok(!table.includes('<img '));
});

test('next page sends the correct offset and applying a filter returns to page one', () => {
    const app = boot(); app.pending[0].resolve({ totalCount: 50, items: [] }); app.pending[0].finish();
    app.click('#BtnNext'); assert.equal(app.pending[1].input.skipCount, 20);
    app.submit(); assert.equal(app.pending[2].input.skipCount, 0);
});
