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
    context.genora.hoaLinhSales.download = (route, filter) => calls.push({ name: 'excel', route, filter });
    if (module) vm.runInContext(fs.readFileSync(__dirname + '/../src/Genora.MultiTenancy.Web/Pages/HoaLinh/' + module + '/index.js', 'utf8'), context);
    return { context, calls, events, warnings };
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
