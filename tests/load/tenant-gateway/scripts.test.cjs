const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const test = require('node:test');
const assert = require('node:assert/strict');
function load(overrides = {}, factory) {
    const requests = [], metrics = {}, checks = [];
    let iteration = 0;
    class Metric {
        constructor(name) { this.name = name; metrics[name] = []; }
        add(value, tags) { metrics[this.name].push({ value, tags }); }
    }
    const context = {
        __ENV: { HL25_BASE_URL: 'https://hl25.example.test', HLG_BASE_URL: 'https://hlg.example.test',
            CONFIRM_HL25: 'https://hl25.example.test', CONFIRM_HLG: 'https://hlg.example.test', TENANT_GATEWAY_LOAD_APPROVED: 'yes', ...overrides },
        Rate: Metric, Counter: Metric, Trend: Metric,
        exec: { scenario: { get iterationInTest() { return iteration; } } },
        check: (response, conditions, tags) => checks.push({ ok: Object.values(conditions).every(fn => fn(response)), tags }),
        http: { expectedStatuses: (...a) => a, setResponseCallback: () => {}, get(url, parameters) {
            requests.push({ url, parameters });
            return factory ? factory(url) : { status: 200, headers: {}, timings: { duration: 10 }, json: () => url.includes('/hl25/') ? { success: true, data: {} } : { data: [] } };
        } }
    };
    const source = fs.readFileSync(path.join(__dirname, 'read-rps.js'), 'utf8').replace(/^import .*;\r?$/gm, '').replace(/export /g, '');
    const script = vm.runInNewContext(source + '\n({options,hl25,hlg})', context);
    return { ...script, requests, metrics, checks, setIteration: i => { iteration = i; } };
}
test('approval, distinct confirmed targets and valid rates are mandatory', () => {
    for (const settings of [{ TENANT_GATEWAY_LOAD_APPROVED: '' }, { CONFIRM_HLG: 'https://wrong.test' },
        { HLG_BASE_URL: 'https://hl25.example.test', CONFIRM_HLG: 'https://hl25.example.test' },
        { HL25_BASE_URL: 'https://x.test/path', CONFIRM_HL25: 'https://x.test/path' }]) assert.throws(() => load(settings), /Explicit approval/);
    assert.throws(() => load({ HLG_RPS: '0' }), /positive integer/);
    assert.throws(() => load({ MODE: 'wrong' }), /MODE/);
});
test('independent 500/300 scenarios send one read per iteration with tenant tags', () => {
    const s = load({ HL25_RPS: '500', HLG_RPS: '300' });
    for (let i=0;i<4;i++) { s.setIteration(i); s.hl25(); }
    for (let i=0;i<3;i++) { s.setIteration(i); s.hlg(); }
    assert.equal(s.requests.length,7);
    assert.equal(new Set(s.requests.map(r => r.url)).size,7);
    assert.equal(s.options.scenarios.hl25.rate,500); assert.equal(s.options.scenarios.hlg.rate,300);
    assert.equal(s.options.maxRedirects,0);
    assert.ok(s.checks.every(c => c.ok));
    assert.equal(s.metrics.gateway_business_success.filter(m => m.tags.tenant==='hlg' && m.value===1).length,3);
    assert.deepEqual(Array.from(s.options.thresholds['gateway_business_success{tenant:hlg}']),['count>0']);
});
test('HLG HTTP200 numeric business error and non-JSON are failures', () => {
    for (const body of [() => ({error:400,data:null}), () => { throw new Error('html'); }]) {
        const s=load({},()=>({status:200,headers:{},json:body})); s.hlg();
        assert.equal(s.metrics.gateway_business_failures[0].value,true);
        assert.equal(s.checks[0].ok,false);
    }
});
test('overload validates both envelope types and retry-after; capacity still rejects429', () => {
    const factory=url=>({status:429,headers:{'Retry-After':'1'},json:()=>({error:url.includes('/hl25/')?'Hl25:RateLimitExceeded':429})});
    const capacity=load({},factory),overload=load({MODE:'overload'},factory);
    capacity.hl25(); capacity.hlg(); overload.hl25(); overload.hlg();
    assert.ok(capacity.checks.every(c=>!c.ok)); assert.ok(overload.checks.every(c=>c.ok));
    assert.equal(overload.metrics.gateway_success_duration.length,0);
    const invalid=load({MODE:'overload'},()=>({status:429,headers:{},json:()=>({error:429})})); invalid.hlg();
    assert.equal(invalid.checks[0].ok,false);
});
test('deployment examples pin matching distinct GUIDs, hosts, quotas and empty secrets/origins', () => {
    for (const environment of ['Staging','Production']) {
        const dir=path.resolve(__dirname,'../../../docs/tenant-gateway');
        const gateway=JSON.parse(fs.readFileSync(path.join(dir,`gateway.${environment}.example.json`))).TenantGateway.Tenants;
        const guard=JSON.parse(fs.readFileSync(path.join(dir,`abp-guard.${environment}.example.json`))).TenantGatewayGuard;
        const routes=JSON.parse(fs.readFileSync(path.join(dir,`ocelot-routes.${environment}.example.json`)));
        assert.equal(gateway.hl25.PermitLimit,500); assert.equal(gateway.hlg.PermitLimit,300);
        assert.notEqual(gateway.hl25.TenantId,gateway.hlg.TenantId); assert.equal(guard.Enabled,true);
        for (const [name,t] of Object.entries(gateway)) {
            assert.equal(t.SharedKey,''); assert.equal(t.BackendAddress,'');
            assert.equal(guard.Tenants[name].TenantId,t.TenantId); assert.equal(guard.Tenants[name].SharedKey,'');
            assert.deepEqual(guard.Tenants[name].PathPrefixes,['/api/mini-app/'+name]);
            const route=routes.find(r=>r.UpstreamHost===t.PublicHosts[0]);
            assert.equal(route.UpstreamHeaderTransform.Host,t.PublicHosts[0]);
            assert.equal(t.BackendTenantOrigin,'https://'+t.PublicHosts[0]);
        }
    }
});
