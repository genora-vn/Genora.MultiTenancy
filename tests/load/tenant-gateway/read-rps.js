import http from 'k6/http';
import { check } from 'k6';
import exec from 'k6/execution';
import { Rate, Counter, Trend } from 'k6/metrics';

const targets = { hl25: (__ENV.HL25_BASE_URL || '').replace(/\/$/, ''), hlg: (__ENV.HLG_BASE_URL || '').replace(/\/$/, '') };
if (__ENV.TENANT_GATEWAY_LOAD_APPROVED !== 'yes' || targets.hl25 !== __ENV.CONFIRM_HL25 || targets.hlg !== __ENV.CONFIRM_HLG ||
    targets.hl25 === targets.hlg || Object.values(targets).some(t => !/^https:\/\/[a-z0-9.-]+(?::\d+)?$/i.test(t)))
    throw new Error('Explicit approval and two distinct HTTPS origins matching CONFIRM_HL25/CONFIRM_HLG are required.');
const mode = __ENV.MODE || 'capacity';
if (!['capacity', 'overload'].includes(mode)) throw new Error('MODE must be capacity or overload.');
const overload = mode === 'overload';
const failures = new Rate('gateway_business_failures');
const throttled = new Rate('gateway_throttled');
const success = new Counter('gateway_business_success');
const latency = new Trend('gateway_success_duration', true);
if (overload) http.setResponseCallback(http.expectedStatuses(200, 429));
const scenarios = {}, thresholds = { dropped_iterations: ['count==0'] };
for (const tenant of ['hl25', 'hlg']) {
    const rate = Number(__ENV[tenant.toUpperCase() + '_RPS'] || 25);
    if (!Number.isInteger(rate) || rate < 1) throw new Error('RPS must be a positive integer for each tenant.');
    scenarios[tenant] = { executor: 'constant-arrival-rate', exec: tenant, rate, timeUnit: '1s', duration: __ENV.DURATION || '2m',
        preAllocatedVUs: 50, maxVUs: 1000, tags: { tenant } };
    thresholds['http_req_failed{tenant:' + tenant + '}'] = ['rate<0.001'];
    thresholds['checks{tenant:' + tenant + '}'] = ['rate>0.99'];
    thresholds['gateway_business_failures{tenant:' + tenant + '}'] = ['rate<0.01'];
    thresholds['gateway_business_success{tenant:' + tenant + '}'] = ['count>0'];
    thresholds['gateway_success_duration{tenant:' + tenant + '}'] = ['p(95)<1000', 'p(99)<3000'];
}
export const options = { maxRedirects: 0, scenarios, thresholds };
const paths = { hl25: ['config', 'frames/campaigns', 'frames/templates', 'gifts'], hlg: ['knowledge/categories', 'games', 'rewards'] };
function read(tenant) {
    const path = paths[tenant][exec.scenario.iterationInTest % paths[tenant].length];
    const tags = { tenant };
    const response = http.get(targets[tenant] + '/api/mini-app/' + tenant + '/' + path,
        { tags: { ...tags, name: tenant + '/' + path }, timeout: '15s' });
    let body;
    try { body = response.json(); } catch (_) { body = null; }
    const limited = response.status === 429;
    const ok = response.status === 200 && body && (tenant === 'hl25' ? body.success === true : body.error == null && body.data != null);
    const valid429 = limited && body && (tenant === 'hl25' ? body.error === 'Hl25:RateLimitExceeded' : body.error === 429) && Number(response.headers['Retry-After']) >= 1;
    throttled.add(limited, tags);
    success.add(ok ? 1 : 0, tags);
    failures.add(!ok && !(overload && valid429), tags);
    if (ok) latency.add(response.timings.duration, tags);
    check(response, { 'business success or declared overload429': () => !!ok || !!(overload && valid429) }, tags);
}
export function hl25() { read('hl25'); }
export function hlg() { read('hlg'); }
