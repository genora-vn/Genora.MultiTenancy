const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const test = require('node:test');
const assert = require('node:assert/strict');

function load(file, overrides = {}, responseFactory) {
    const requests = [], metrics = {}, checks = [];
    let iteration = 0;
    const http = {
        expectedStatuses: (...statuses) => statuses,
        setResponseCallback: () => {},
        file: () => 'mock-file',
        request(method, url, body, parameters) {
            requests.push({ method, url, body, parameters });
            return responseFactory ? responseFactory(url) : { status: 200, headers: {}, json: () => ({ success: true, data: {} }) };
        },
        get(url, parameters) { return this.request('GET', url, null, parameters); }
    };
    const context = {
        __ENV: { BASE_URL: 'https://test.example', CONFIRM_TARGET: 'https://test.example',
            HL25_LOAD_APPROVED: 'yes', ...overrides }, http,
        exec: { scenario: { get iterationInTest() { return iteration; } } },
        Rate: class { constructor(name) { this.name = name; metrics[name] = []; } add(value) { metrics[this.name].push(!!value); } },
        check: (value, conditions) => { const ok = Object.values(conditions).every(fn => fn(value)); checks.push(ok); return ok; },
        sleep: () => {}, open: () => new Uint8Array([1, 2, 3])
    };
    const source = fs.readFileSync(path.join(__dirname, file), 'utf8')
        .replace(/^import .*;\r?$/gm, '').replace('export const options', 'const options')
        .replace('export default function', 'function run').replace('export function setup', 'function setup');
    const script = vm.runInNewContext(source + '\n({run,options,setup:typeof setup === "function" ? setup : null})', context);
    return { ...script, requests, metrics, checks, setIteration: value => { iteration = value; } };
}

test('read load refuses missing approval and mismatched target', () => {
    assert.throws(() => load('read-rps.js', { HL25_LOAD_APPROVED: '' }), /approved test environment/);
    assert.throws(() => load('read-rps.js', { CONFIRM_TARGET: 'https://other.example' }), /approved test environment/);
});
test('one read iteration issues exactly one request and cycles all four cache endpoints', () => {
    const script = load('read-rps.js');
    for (let i = 0; i < 4; i++) { script.setIteration(i); script.run(); }
    assert.equal(script.requests.length, 4);
    assert.equal(new Set(script.requests.map(r => r.url)).size, 4);
    assert.equal(script.options.maxRedirects, 0);
    assert.ok(script.checks.every(Boolean));
});
test('HTTP200 business failure is counted as failure', () => {
    const script = load('read-rps.js', {}, () => ({ status: 200, json: () => ({ success: false }), headers: {} }));
    script.run();
    assert.deepEqual(script.metrics.hl25_business_failures, [true]);
    assert.deepEqual(script.checks, [false]);
});
test('429 is an error for capacity but an explicit measured rejection for overload', () => {
    const response = () => ({ status: 429, headers: { 'Retry-After': '1' }, json: () => ({ error: 'Hl25:RateLimitExceeded' }) });
    const capacity = load('read-rps.js', {}, response), overload = load('read-rps.js', { MODE: 'overload' }, response);
    capacity.run(); overload.run();
    assert.deepEqual(capacity.metrics.hl25_business_failures, [true]);
    assert.deepEqual(overload.metrics.hl25_business_failures, [false]);
    assert.deepEqual(overload.metrics.hl25_throttled, [true]);
    assert.deepEqual(overload.checks, [true]);
});
test('journey refuses missing write permission or data run ID', () => {
    assert.throws(() => load('journey.js'), /HL25_ALLOW_WRITES/);
    assert.throws(() => load('journey.js', { HL25_ALLOW_WRITES: 'yes', IMAGE_FILE: 'test.png' }), /RUN_ID/);
});
test('failed registration is not retried and no dependent write is sent', () => {
    const script = load('journey.js', { HL25_ALLOW_WRITES: 'yes', RUN_ID: 'test', IMAGE_FILE: 'test.png' },
        url => ({ status: url.endsWith('register') ? 503 : 200, headers: {}, json: () => ({ success: !url.endsWith('register'), data: {} }) }));
    script.run({ id: 'template', campaignId: 'campaign' });
    assert.equal(script.requests.length, 2);
    assert.equal(script.requests.filter(r => r.method === 'POST').length, 1);
});
test('complete journey sends 17 requests and uses server frame ID in share', () => {
    const script = load('journey.js', { HL25_ALLOW_WRITES: 'yes', RUN_ID: 'test', IMAGE_FILE: 'test.png' },
        () => ({ status: 200, headers: {}, json: () => ({ success: true,
            data: { frameCreationId: 'server-frame', url: 'https://test.example/image.png', earnedCycles: 2, remainingSpinTurns: 0, totalGiftsWon: 1 } }) }));
    script.run({ id: 'template', campaignId: 'campaign' });
    assert.equal(script.requests.length, 17);
    const share = script.requests.find(r => r.url.endsWith('/frames/share'));
    assert.equal(JSON.parse(share.body).frameCreationId, 'server-frame');
    assert.ok(script.checks.every(Boolean));
});
