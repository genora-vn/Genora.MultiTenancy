import http from 'k6/http';
import { check } from 'k6';
import exec from 'k6/execution';
import { Rate } from 'k6/metrics';

const base = (__ENV.BASE_URL || '').replace(/\/$/, '');
if (__ENV.HL25_LOAD_APPROVED !== 'yes' || !base || base !== __ENV.CONFIRM_TARGET)
    throw new Error('Set BASE_URL, identical CONFIRM_TARGET and HL25_LOAD_APPROVED=yes for an approved test environment.');
const overload = __ENV.MODE === 'overload';
const businessFailures = new Rate('hl25_business_failures');
const throttled = new Rate('hl25_throttled');
const rate = Number(__ENV.RPS || 50);
if (!Number.isInteger(rate) || rate < 1) throw new Error('RPS must be a positive integer.');
if (overload) http.setResponseCallback(http.expectedStatuses(200, 429));

export const options = {
    maxRedirects: 0,
    scenarios: { reads: { executor: 'constant-arrival-rate', rate, timeUnit: '1s',
        duration: __ENV.DURATION || '2m', preAllocatedVUs: Number(__ENV.PREALLOCATED_VUS || 100),
        maxVUs: Number(__ENV.MAX_VUS || 1500) } },
    thresholds: { http_req_failed: ['rate<0.001'], hl25_business_failures: ['rate<0.01'],
        checks: ['rate>0.99'], dropped_iterations: ['count==0'],
        http_req_duration: ['p(95)<500', 'p(99)<2000'] }
};
const paths = ['config', 'frames/campaigns', 'frames/templates', 'gifts'];
export default function () {
    const path = paths[exec.scenario.iterationInTest % paths.length];
    const response = http.get(base + '/api/mini-app/hl25/' + path,
        { tags: { name: 'hl25/' + path }, timeout: '15s' });
    const limited = response.status === 429;
    throttled.add(limited);
    let body;
    try { body = response.json(); } catch (_) { body = null; }
    const ok = response.status === 200 && body && body.success === true;
    businessFailures.add(!ok && !(overload && limited));
    check(response, { 'business success or declared overload429': () => ok ||
        (overload && limited && body && body.error === 'Hl25:RateLimitExceeded' &&
            Number(response.headers['Retry-After']) >= 1) });
}
