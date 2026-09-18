const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const test = require('node:test');

function harness(module, values = {}) {
    const events = {}, calls = [], warnings = [];
    const fields = { FilterDateFrom: '17/09/2026', FilterDateTo: '17/09/2026', PageSize: '20', ...values };
    function $(selector) {
        const id = typeof selector === 'string' ? selector.replace(/^#/, '') : '';
        const api = {
            val(value) { if (arguments.length) { fields[id] = value; return api; } return fields[id] || ''; },
            click(handler) { events[id] = handler; return api; },
            change() { return api; }, keypress() { return api; }, on() { return api; },
            empty() { return api; }, append() { return api; }, html() { return api; },
            prop() { return api; }, hide() { return api; }, show() { return api; },
            removeClass() { return api; }, addClass() { return api; }, data() { return 'batch'; }
        };
        return api;
    }
    function request(name) { return (...args) => {
        calls.push({ name, args });
        const result = { then() { return result; }, catch() { return result; }, always() { return result; } };
        return result;
    }; }
    const context = { window: {}, $, document: {}, console, Date, URLSearchParams,
        abp: { ui: { setBusy() {}, clearBusy() {} }, notify: { warn(message) { warnings.push(message); } } } };
    context.window = context;
    context.genora = { multiTenancy: { appServices: { hoaLinh: {
        hlAdmin: { getPointHistory: request('transactions'), getPointBatches: request('batches') },
        hlGiftExchange: { getList: request('gifts') }, hlOrder: {}, hlSalesExport: { getOrders: request('orders') }
    } } } };
    vm.createContext(context);
    vm.runInContext(fs.readFileSync(__dirname + '/../src/Genora.MultiTenancy.Web/Pages/HoaLinh/sales.js', 'utf8'), context);
    const download = context.genora.hoaLinhSales.download;
    context.genora.hoaLinhSales.download = (route, filter) => calls.push({ name: 'excel', route, filter });
    if (module) vm.runInContext(fs.readFileSync(__dirname + '/../src/Genora.MultiTenancy.Web/Pages/HoaLinh/' + module + '/index.js', 'utf8'), context);
    return { context, calls, events, warnings, download };
}

test('Vietnamese and ISO dates normalize; impossible and malformed dates are rejected', () => {
    const { context } = harness();
    const parse = context.genora.hoaLinhSales.parseDate;
    assert.equal(parse('17/09/2026'), '2026-09-17');
    assert.equal(parse('2026-09-17'), '2026-09-17');
    assert.equal(parse('29/02/2024'), '2024-02-29');
    assert.equal(parse('29/02/2026'), undefined);
    assert.equal(parse('31/09/2026'), undefined);
    assert.equal(parse('abc'), undefined);
    assert.equal(parse(''), null);
});
for (const [module, call] of [['PointHistory', 'transactions'], ['GiftExchanges', 'gifts'], ['Orders', 'orders']]) {
    test(module + ': list and export send the same ISO date range', () => {
        const h = harness(module, { FilterText: 'Ngọc', FilterStatus: '1' });
        const list = h.calls.find(x => x.name === call).args[0];
        h.events.BtnExportExcel.call({});
        const excel = h.calls.find(x => x.name === 'excel').filter;
        assert.equal(list.dateFrom, '2026-09-17');
        assert.equal(list.dateTo, '2026-09-17');
        assert.equal(excel.dateFrom, list.dateFrom);
        assert.equal(excel.dateTo, list.dateTo);
    });
    test(module + ': reversed dates block both search and export', () => {
        const h = harness(module, { FilterDateFrom: '18/09/2026' });
        h.events.BtnExportExcel.call({});
        assert.equal(h.calls.length, 0);
        assert.ok(h.warnings.length >= 2);
    });
}
test('PointHistory: batch tab uses date filters and selects batch Excel data', () => {
    const h = harness('PointHistory');
    h.events['PointTabs .nav-link'].call({}, { preventDefault() {} });
    const batch = h.calls.find(x => x.name === 'batches');
    assert.equal(batch.args[3], '2026-09-17');
    assert.equal(batch.args[4], '2026-09-17');
    h.events.BtnExportExcel.call({});
    assert.equal(h.calls.find(x => x.name === 'excel').filter.batches, true);
});


function downloadHarness(response) {
    const h = harness();
    const requests = [], links = [], states = [], errors = [], revoked = [];
    h.context.abp.appPath = '/';
    // Matches the reported runtime: multiTenancy exists, getTenantIdCookie does not.
    h.context.abp.multiTenancy = {};
    h.context.abp.notify.error = message => errors.push(message);
    h.context.$ = () => ({ prop(name, value) { states.push(value); } });
    h.context.fetch = async (url, options) => {
        requests.push({ url, options });
        return response;
    };
    h.context.document = {
        body: { appendChild() {} },
        createElement() {
            const link = { click() { links.push(this); }, remove() {} };
            return link;
        }
    };
    h.context.URL = { createObjectURL() { return 'blob:excel'; }, revokeObjectURL(url) { revoked.push(url); } };
    h.context.setTimeout = callback => callback();
    return { ...h, requests, links, states, errors, revoked };
}

for (const route of ['point-history', 'gift-exchanges', 'orders']) {
    test(route + ': real download works without getTenantIdCookie and keeps filters/cookies', async () => {
        const h = downloadHarness({
            ok: true, redirected: false,
            headers: { get(name) {
                return name === 'content-type' ? 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
                    : 'attachment; filename="HoaLinhSales.xlsx"';
            } },
            blob: async () => ({ workbook: true })
        });
        await h.download('api/app/hl-sales-excel/' + route,
            { dateFrom: '2026-09-18', dateTo: '2026-09-18', status: 0, batches: false, search: 'Ngọc', empty: null }, {});
        const request = h.requests[0];
        const url = new URL(request.url, 'https://example.test');
        assert.equal(url.pathname, '/api/app/hl-sales-excel/' + route);
        assert.equal(url.searchParams.get('dateFrom'), '2026-09-18');
        assert.equal(url.searchParams.get('dateTo'), '2026-09-18');
        assert.equal(url.searchParams.get('status'), '0');
        assert.equal(url.searchParams.get('batches'), 'false');
        assert.equal(url.searchParams.get('search'), 'Ngọc');
        assert.equal(url.searchParams.has('empty'), false);
        assert.equal(request.options.credentials, 'same-origin');
        assert.equal(h.links[0].download, 'HoaLinhSales.xlsx');
        assert.equal(h.links[0].href, 'blob:excel');
        assert.deepEqual(h.states, [true, false]);
        assert.deepEqual(h.revoked, ['blob:excel']);
        assert.equal(h.errors.length, 0);
    });
}

test('Real download rejects API error and HTML login page, restores button', async () => {
    for (const response of [
        { ok: false, redirected: false, headers: { get() { return 'application/json'; } },
            json: async () => ({ error: { message: 'Permission denied' } }) },
        { ok: true, redirected: true, headers: { get() { return 'text/html'; } },
            json: async () => { throw new Error('Not JSON'); } }
    ]) {
        const h = downloadHarness(response);
        await h.download('api/app/hl-sales-excel/orders', {}, {});
        assert.equal(h.links.length, 0);
        assert.equal(h.errors.length, 1);
        assert.deepEqual(h.states, [true, false]);
    }
});
