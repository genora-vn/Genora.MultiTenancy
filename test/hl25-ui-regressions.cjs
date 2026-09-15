const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const root = path.resolve(__dirname, '..');
const pages = path.join(root, 'src/Genora.MultiTenancy.Web/Pages/Hl25');

function wheelTables(canEdit) {
    const tables = {};
    function $(arg) {
        if (typeof arg === 'function') { arg(); return; }
        const element = {
            DataTable(config) { tables[arg] = config; return { ajax: { reload() {} } }; },
            click() { return element; }, on() { return element; }, submit() { return element; }
        };
        return element;
    }
    $.fn = { dataTable: { render: { text: () => value => value } } };
    const context = {
        $, abp: {
            localization: { getResource: () => key => key }, auth: { isGranted: () => canEdit },
            ModalManager: function () { this.onResult = () => {}; },
            libs: { datatables: { normalizeConfiguration: config => config, createAjax: () => () => {} } }
        },
        genora: { multiTenancy: { appServices: { hl25: {
            hl25Gift: {}, hl25WheelConfig: {}, hl25SpinTurnLog: {}, hl25SpinLog: {}
        } } } }
    };
    vm.runInNewContext(fs.readFileSync(path.join(pages, 'Wheel.js'), 'utf8'), context);
    return tables;
}

test('spin history initializes and visibility accepts the raw ABP row, including missing rows', () => {
    const tables = wheelTables(true);
    assert.ok(tables['#SpinLogsTable']);
    const action = tables['#SpinLogsTable'].columnDefs[0].rowAction.items[0];
    assert.equal(action.visible({ rewardStatus: 1 }), true);
    assert.equal(action.visible({ rewardStatus: 2 }), false);
    assert.equal(action.visible({ rewardStatus: 3 }), false);
    assert.equal(action.visible(undefined), false);
    assert.equal(action.visible(null), false);
    assert.equal(wheelTables(false)['#SpinLogsTable'].columnDefs[0].rowAction.items[0].visible({ rewardStatus: 1 }), false);
});

test('VND formats millions without losing precision or turning decimal zeros into extra digits', () => {
    const context = { window: {}, genora: {} };
    context.window.genora = context.genora;
    vm.runInNewContext(fs.readFileSync(path.join(pages, 'GiftModal.js'), 'utf8'), context);
    const money = context.genora.hl25GiftModal;
    assert.equal(money.formatMoney('1000000'), '1.000.000');
    assert.equal(money.formatMoney('1.000.000,00'), '1.000.000');
    assert.equal(money.formatMoney('1000000,50'), '1.000.000,5');
    assert.equal(money.normalizeMoney('1.000.000,50'), '1000000.50');
    assert.equal(money.formatMoney('9999999999999999,99'), '9.999.999.999.999.999,99');
    assert.equal(money.formatMoney(''), '');
    assert.equal(money.formatMoney('1.2.3'), '1.2.3'); // Invalid input stays invalid for validation.
});

test('all requested enum dropdowns use explicit options, each with vi/en resources', () => {
    const resources = ['vi', 'en'].map(lang => JSON.parse(fs.readFileSync(path.join(root,
        `src/Genora.MultiTenancy.Domain.Shared/Localization/MultiTenancy/${lang}.json`), 'utf8').replace(/^\uFEFF/, '')).Texts);
    for (const file of ['CampaignCreateModal.cshtml', 'CampaignEditModal.cshtml', 'ParticipantEditModal.cshtml', '_GiftForm.cshtml']) {
        const source = fs.readFileSync(path.join(pages, file), 'utf8');
        assert.doesNotMatch(source, /<abp-select\b/);
        const selects = [...source.matchAll(/<select asp-for="[^"]+"[^>]*>([\s\S]*?)<\/select>/g)];
        assert.ok(selects.length > 0, file);
        for (const [, body] of selects) {
            const options = [...body.matchAll(/<option value="(\d+)">@L\["(Enum:Hl25[^"]+)"\]<\/option>/g)];
            assert.ok(options.length >= 3, file);
            assert.equal(new Set(options.map(x => x[1])).size, options.length);
            for (const [, , key] of options) for (const texts of resources) assert.ok(texts[key], key);
        }
    }
});
