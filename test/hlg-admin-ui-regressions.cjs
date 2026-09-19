const fs = require('node:fs');
const vm = require('node:vm');
const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');
const web = fs.existsSync(path.join(__dirname, 'Pages/Hlg/admin.js')) ? __dirname : path.join(__dirname, '../src/Genora.MultiTenancy.Web');
function harness(options = {}) {
    const fields = { HlgSearch: 'gift', HlgActive: 'false' }, events = {}, calls = [], modals = [];
    let grid, getInput;
    const proxy = { getList() {}, delete(id) { calls.push(['delete', id]); return Promise.resolve(); } };
    const attrs = { 'data-edit': 'true', 'data-delete': 'true', 'data-parent-id': 'game-1', ...options.attrs };
    function $(selector) {
        const id = selector.replace(/^#/, '');
        const api = { attr(name) { return attrs[name]; }, val(value) { if (arguments.length) { fields[id] = value; return api; } return fields[id]; },
            text(value) { calls.push(['error', value]); return api; }, removeClass() { return api; },
            on(event, callback) { events[id + ':' + event] = callback; return api; },
            DataTable(config) { grid = config; return { ajax: { reload(...args) { calls.push(['reload', ...args]); } } }; }
        }; return api;
    }
    const context = { console: { error() {} }, jQuery: $, location: {}, genora: { multiTenancy: {} },
        abp: { appPath: '/', localization: { getResource: () => (key) => key, currentCulture: { name: 'vi-VN' } }, notify: { success() {} },
            ModalManager: function (url) { modals.push(url); this.open = args => calls.push(['open', url, args]); this.onResult = () => {}; },
            libs: { datatables: { normalizeConfiguration: x => x, createAjax(service, filter) { getInput = filter; return service; } } } } };
    context.window = context;
    if (!options.missing) context.genora.multiTenancy[options.namespace || 'appServices'] = { hlg: { admin: { hlgRewardAdmin: proxy } } };
    vm.createContext(context); vm.runInContext(fs.readFileSync(path.join(web, 'Pages/Hlg/admin.js'), 'utf8'), context);
    return { context, proxy, calls, modals, events, fields, get grid() { return grid; }, input() { return getInput(); },
        init(extra = {}) { context.hlgAdmin.init({ service: 'hlgRewardAdmin', folder: 'Rewards', columns: [{ data: 'name', label: 'Name', kind: 'text' }], ...extra }); } };
}
test('resolves implementation namespace and contract namespace without an unsafe global lookup', () => {
    for (const namespace of ['appServices', 'appDtos']) { const h = harness({ namespace }); assert.equal(h.context.hlgAdmin.resolveService('hlgRewardAdmin'), h.proxy); }
    const h = harness({ missing: true }); delete h.context.genora;
    assert.throws(() => h.context.hlgAdmin.resolveService('hlgRewardAdmin'), /proxy not found/);
});
test('missing proxy shows a visible error before constructing a table', () => {
    const h = harness({ missing: true }); h.init(); assert.equal(h.grid, undefined); assert.equal(h.calls[0][0], 'error');
});
test('filters preserve false and nested parent ID; submits reset pagination', () => {
    const h = harness(); h.init(); assert.equal(h.input().isActive, false); assert.equal(h.input().parentId, 'game-1'); assert.equal(h.input().filterText, 'gift');
    h.fields.HlgActive = ''; assert.equal(h.input().isActive, null);
    h.events['HlgFilter:submit']({ preventDefault() {} }); assert.deepEqual(h.calls.at(-1), ['reload']);
});
test('renders untrusted names as text and formats unlimited stock', () => {
    const h = harness(); h.init({ columns: [{ data: 'name', label: 'Name', kind: 'text' }, { data: 'stock', label: 'StockQuantity', kind: 'stock' }] });
    assert.equal(h.grid.columnDefs[1].render('<img onerror="x">', 'display'), '&lt;img onerror=&quot;x&quot;&gt;');
    assert.equal(h.grid.columnDefs[2].render(null, 'display'), 'Hlg:Unlimited');
});
test('users are read-only even when page flags permit editing', () => {
    const h = harness(); h.init({ readOnly: true }); assert.equal(h.modals.length, 0); assert.equal(h.grid.columnDefs.length, 1);
});
test('edit and delete actions disappear without permissions', () => {
    const h = harness({ attrs: { 'data-edit': 'false', 'data-delete': 'false' } }); h.init(); assert.equal(h.grid.columnDefs.length, 1);
});
test('nested navigation and modal actions use the selected record', async () => {
    const h = harness(); h.init({ children: 'Questions' }); const actions = h.grid.columnDefs[0].rowAction.items;
    actions[0].action({ record: { id: 'a b' } }); assert.equal(h.context.location.href, '/Hlg/Questions?parentId=a%20b');
    actions[1].action({ record: { id: 'abc' } }); assert.equal(h.calls.at(-1)[2].id, 'abc');
    actions[2].action({ record: { id: 'abc' } }); await Promise.resolve(); assert.ok(h.calls.some(x => x[0] === 'delete' && x[1] === 'abc'));
});
