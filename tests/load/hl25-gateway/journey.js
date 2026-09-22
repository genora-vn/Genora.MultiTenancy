import http from 'k6/http';
import { check, sleep } from 'k6';
import exec from 'k6/execution';
import { Rate } from 'k6/metrics';

const base = (__ENV.BASE_URL || '').replace(/\/$/, '');
if (__ENV.HL25_LOAD_APPROVED !== 'yes' || __ENV.HL25_ALLOW_WRITES !== 'yes' ||
    !base || base !== __ENV.CONFIRM_TARGET || !/^[a-zA-Z0-9-]{1,24}$/.test(__ENV.RUN_ID || '') || !__ENV.IMAGE_FILE)
    throw new Error('Approved isolated target + HL25_ALLOW_WRITES=yes + RUN_ID (1..24 alphanumeric/dash) + IMAGE_FILE required.');
const image = open(__ENV.IMAGE_FILE, 'b');
const failures = new Rate('hl25_business_failures');
const rate = Number(__ENV.JOURNEYS_PER_SECOND || 1);
if (!Number.isInteger(rate) || rate < 1) throw new Error('JOURNEYS_PER_SECOND must be a positive integer.');
export const options = {
    maxRedirects: 0,
    scenarios: { journeys: { executor: 'constant-arrival-rate', rate, timeUnit: '1s',
        duration: __ENV.DURATION || '2m', preAllocatedVUs: Number(__ENV.PREALLOCATED_VUS || 100),
        maxVUs: Number(__ENV.MAX_VUS || 1500) } },
    thresholds: { http_req_failed: ['rate<0.001'], hl25_business_failures: ['rate<0.01'],
        checks: ['rate>0.99'], dropped_iterations: ['count==0'],
        'http_req_duration{kind:business}': ['p(95)<2000', 'p(99)<5000'],
        'http_req_duration{kind:upload}': ['p(95)<5000'] }
};
function call(method, path, payload, upload = false) {
    const response = http.request(method, base + '/api/mini-app/hl25/' + path,
        payload === undefined ? null : upload ? payload : JSON.stringify(payload),
        { headers: upload ? {} : { 'Content-Type': 'application/json' }, timeout: '30s',
            tags: { name: 'hl25/' + path.split('?')[0], kind: upload ? 'upload' : 'business' } });
    let body;
    try { body = response.json(); } catch (_) { body = null; }
    const ok = response.status === 200 && body && body.success === true;
    failures.add(!ok);
    check(response, { 'HTTP200 and business success': () => ok });
    return ok ? body.data : null; // No automatic retry of registration/share/spin/upload.
}
export function setup() {
    const templates = call('GET', 'frames/templates');
    if (!templates || !templates.length) throw new Error('Test tenant needs an active campaign/template.');
    return templates[0];
}
export default function (template) {
    const zaloUserId = 'loadtest-' + __ENV.RUN_ID + '-' + exec.scenario.iterationInTest;
    const query = '?zaloUserId=' + encodeURIComponent(zaloUserId);
    if (!call('GET', 'config')) return;
    if (!call('POST', 'participants/register', { zaloUserId, fullName: 'HL25 load test', hasConsent: true })) return;
    if (!call('GET', 'participants/me' + query)) return;
    if (!call('GET', 'frames/campaigns')) return;
    if (!call('GET', 'frames/templates?campaignId=' + template.campaignId)) return;
    const upload = call('POST', 'upload-image', { file: http.file(image, 'load-test.png', 'image/png') }, true);
    if (!upload) return;
    const frame = call('POST', 'frames', { zaloUserId, campaignId: template.campaignId,
        templateId: template.id, resultImageUrl: upload.url, wishMessage: 'Load test ' + __ENV.RUN_ID,
        renameWithTimestamp: false });
    if (!frame) return;
    if (!call('GET', 'wheel' + query)) return;
    if (!call('POST', 'wheel/spin', { zaloUserId })) return;
    if (!call('POST', 'frames/share', { zaloUserId, frameCreationId: frame.frameCreationId, sharePlatform: 1 })) return;
    if (!call('POST', 'wheel/spin', { zaloUserId })) return;
    const me = call('GET', 'participants/me' + query);
    check(me, { 'two earned turns, no negative turns, at most one gift': value => value &&
        value.earnedCycles === 2 && value.remainingSpinTurns === 0 && value.totalGiftsWon <= 1 });
    call('GET', 'gifts');
    call('GET', 'me/gifts' + query);
    call('GET', 'me/frames' + query);
    call('GET', 'me/spin-turns' + query);
    call('GET', 'me/spins' + query);
    sleep(Number(__ENV.THINK_SECONDS || 3));
}
