const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const test = require('node:test');

const source = fs.readFileSync(__dirname + '/../src/Genora.MultiTenancy.Web/Pages/HoaLinh/BlouseConfig/index.js', 'utf8');
const settle = () => new Promise(resolve => setImmediate(resolve));
const campaign = () => ({
    id: 'campaign', programName: 'Áo Blouse', introductionHtml: '<p>Giới thiệu</p>',
    freeShirtLimit: 2, pointsPerShirt: 150, maxExchangeShirt: 0,
    sizeChartImageUrl: '/uploads/hl-blouse/banner.png', isActive: true,
    startTime: '2026-09-30T08:37:00', endTime: '2026-10-30T23:59:00'
});
function format(date) {
    const pad = n => String(n).padStart(2, '0');
    return `${pad(date.getDate())}/${pad(date.getMonth() + 1)}/${date.getFullYear()} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
}
function deferred(promise) {
    return {
        then(fn) { return deferred(promise.then(fn)); },
        catch(fn) { return deferred(promise.catch(fn)); },
        always(fn) { return deferred(promise.finally(fn)); }
    };
}
function harness({ initial = campaign(), noPicker = false, load, save, valid = true } = {}) {
    const fields = {}, props = {}, events = {}, pickers = {}, calls = [], warnings = [], errors = [], successes = [];
    let stored = initial;
    const document = { getElementById() { return { reportValidity: () => valid }; } };
    function $(selector) {
        if (typeof selector === 'function') { selector(); return; }
        const id = typeof selector === 'string' ? selector.replace(/^#/, '') : 'document';
        const api = {
            val(value) { if (arguments.length) { fields[id] = value; return api; } return fields[id] ?? ''; },
            prop(key, value) { props[id + ':' + key] = value; return api; },
            is() { return !!props[id + ':checked']; },
            click(fn) { events[id + ':click'] = fn; return api; },
            on(event, fn) { events[id + ':' + event] = fn; return api; },
            attr() { return api; }, show() { return api; }, hide() { return api; },
            empty() { return api; }, append() { return api; }, html() { return api; }
        };
        return api;
    }
    const service = {
        getCampaign() { calls.push({ name: 'get' }); return deferred(load ? load : Promise.resolve(stored)); },
        getSizes() { return deferred(Promise.resolve({ items: [] })); },
        saveCampaign(input) {
            calls.push({ name: 'save', input });
            return deferred(save ? save(input) : Promise.resolve(stored = { ...input, id: 'campaign' }));
        }
    };
    const context = {
        $, document, Date, console,
        bootstrap: { Modal: class {} },
        abp: { ui: { setBusy() {}, clearBusy() {} }, notify: {
            warn: m => warnings.push(m), error: m => errors.push(m), success: m => successes.push(m)
        } },
        genora: { multiTenancy: { appServices: { hoaLinh: { hlBlouseAdmin: service } } } }
    };
    if (!noPicker) context.flatpickr = (selector, options) => {
        const picker = { selectedDates: [], options, setDate(date) {
            // The real flatpickr parser misreads ISO strings with this display format.
            // Guard its boundary: API strings must be converted to Date before setDate.
            assert.ok(date === null || date instanceof Date, 'setDate must receive a Date, not an ISO string');
            picker.selectedDates = date ? [date] : [];
            fields[selector.slice(1)] = date ? format(date) : '';
        } };
        return pickers[selector] = picker;
    };
    context.window = context;
    vm.runInNewContext(source, context);
    return { fields, props, events, pickers, calls, warnings, errors, successes,
        submit() { events['CampaignForm:submit']({ preventDefault() {} }); },
        saved() { return calls.filter(c => c.name === 'save'); }
    };
}

test('ISO dates display correctly and repeated save/load keeps exact local minutes', async () => {
    const h = harness(); await settle();
    assert.equal(h.fields.StartTime, '30/09/2026 08:37');
    assert.equal(h.fields.EndTime, '30/10/2026 23:59');
    for (let i = 0; i < 3; i++) {
        h.submit(); await settle();
        assert.equal(h.saved()[i].input.startTime, '2026-09-30T08:37:00');
        assert.equal(h.saved()[i].input.endTime, '2026-10-30T23:59:00');
        assert.equal(h.fields.EndTime, '30/10/2026 23:59');
    }
    assert.equal(h.calls.filter(c => c.name === 'get').length, 1, 'render the saved DTO instead of refetching stale config');
    const reloaded = harness({ initial: h.saved()[2].input }); await settle();
    assert.equal(reloaded.fields.StartTime, h.fields.StartTime);
    assert.equal(reloaded.fields.EndTime, h.fields.EndTime);
});

test('new calendar selections and typed input replace previously loaded selectedDates', async () => {
    const h = harness(); await settle();
    h.pickers['#StartTime'].setDate(new Date(2026, 9, 1, 0, 1));
    h.fields.EndTime = '31/10/2026 23:58'; // No picker blur/event has happened yet.
    h.submit(); await settle();
    assert.equal(h.saved()[0].input.startTime, '2026-10-01T00:01:00');
    assert.equal(h.saved()[0].input.endTime, '2026-10-31T23:58:00');
});

test('every editable field, false flags, and zero numeric values survive save/reload', async () => {
    const h = harness(); await settle();
    Object.assign(h.fields, { ProgramName: '  Cấu hình mới  ', IntroductionHtml: '<p>Mới</p>',
        FreeShirtLimit: '0', PointsPerShirt: '0', MaxExchangeShirt: '0', SizeChartImageUrl: '/uploads/new.png' });
    h.props['IsActive:checked'] = false;
    h.submit(); await settle();
    const data = h.saved()[0].input;
    assert.equal(data.programName, 'Cấu hình mới');
    assert.equal(data.introductionHtml, '<p>Mới</p>');
    assert.equal(data.sizeChartImageUrl, '/uploads/new.png');
    assert.equal(data.isActive, false);
    for (const field of ['freeShirtLimit', 'pointsPerShirt', 'maxExchangeShirt']) assert.equal(data[field], 0);
    assert.equal(h.fields.FreeShirtLimit, 0);
    assert.equal(h.props['IsActive:checked'], false);
});

test('clearing dates sends null rather than stale calendar selections', async () => {
    const h = harness(); await settle();
    h.fields.StartTime = ''; h.fields.EndTime = '';
    h.submit(); await settle();
    assert.equal(h.saved()[0].input.startTime, null);
    assert.equal(h.saved()[0].input.endTime, null);
    assert.equal(h.fields.StartTime, ''); assert.equal(h.fields.EndTime, '');
});

test('valid integer exponent input is not truncated by parseInt', async () => {
    const h = harness(); await settle();
    h.fields.PointsPerShirt = '1e3'; h.submit(); await settle();
    assert.equal(h.saved()[0].input.pointsPerShirt, 1000);
});

for (const value of ['31/09/2026 12:00', '30/09/2026 25:00', '29/02/2026 08:00', 'invalid']) {
    test(`invalid typed date is rejected: ${value}`, async () => {
        const h = harness(); await settle(); h.fields.StartTime = value;
        h.submit(); await settle();
        assert.equal(h.saved().length, 0); assert.equal(h.warnings.length, 1);
    });
}

test('end before start is rejected without saving', async () => {
    const h = harness(); await settle(); h.fields.EndTime = '30/09/2026 08:36';
    h.submit(); await settle(); assert.equal(h.saved().length, 0); assert.equal(h.warnings.length, 1);
});

test('missing flatpickr still loads and saves dates instead of replacing them with null', async () => {
    const h = harness({ noPicker: true }); await settle();
    assert.equal(h.fields.StartTime, '30/09/2026 08:37');
    h.submit(); await settle(); assert.equal(h.saved()[0].input.startTime, campaign().startTime);
});

test('load failure blocks saving default values over existing data', async () => {
    const h = harness({ load: Promise.reject(new Error('Network')) }); await settle();
    h.submit(); assert.equal(h.saved().length, 0);
    assert.equal(h.props['BtnSaveCampaign:disabled'], true); assert.equal(h.errors.length, 1);
});

test('save is blocked while loading or saving and failed save preserves user edits', async () => {
    let finishLoad, rejectSave;
    const h = harness({ load: new Promise(resolve => { finishLoad = resolve; }),
        save: () => new Promise((resolve, reject) => { rejectSave = reject; }) });
    h.submit(); assert.equal(h.saved().length, 0);
    finishLoad(campaign()); await settle();
    h.fields.ProgramName = 'Chưa lưu'; h.fields.EndTime = '01/11/2026 10:11';
    h.submit(); h.submit(); assert.equal(h.saved().length, 1);
    rejectSave(new Error('Server error')); await settle();
    assert.equal(h.fields.ProgramName, 'Chưa lưu'); assert.equal(h.fields.EndTime, '01/11/2026 10:11');
    assert.equal(h.successes.length, 0); assert.equal(h.errors.length, 1);
    assert.equal(h.props['BtnSaveCampaign:disabled'], false);
});

test('native form validation prevents empty, negative, or fractional numeric submission', async () => {
    const h = harness({ valid: false }); await settle(); h.submit();
    assert.equal(h.saved().length, 0);
    const markup = fs.readFileSync(__dirname + '/../src/Genora.MultiTenancy.Web/Pages/HoaLinh/BlouseConfig/Index.cshtml', 'utf8');
    for (const field of ['FreeShirtLimit', 'PointsPerShirt', 'MaxExchangeShirt']) {
        assert.match(markup, new RegExp(`id="${field}"[^>]*required min="0" max="2147483647" step="1"`));
    }
});

test('an empty tenant campaign can be created', async () => {
    const h = harness({ initial: null }); await settle();
    h.fields.ProgramName = 'Chương trình mới'; h.submit(); await settle();
    assert.equal(h.saved().length, 1); assert.equal(h.successes.length, 1);
});
