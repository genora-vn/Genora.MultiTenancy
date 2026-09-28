// Read-only forecast for five 0.05% gift slots plus 99.75% no-gift.
// Draws use the same cumulative intervals as Hl25WheelDistribution.PickWeighted.
// Inventory is capped per gift; every simulated spin is eligible.

const dailySpins = [1131, 5476, 6020, 9441];
const giftProbability = 0.0005;
const remainingDays = 32; // 2026-09-29 through 2026-10-30, inclusive.

function simulateDepletion(quantityPerGift, batches, seed) {
    const depletionSpins = [];
    for (let batch = 0; batch < batches; batch++) {
        let state = (seed ^ batch) >>> 0;
        const issued = [0, 0, 0, 0, 0];
        let remaining = quantityPerGift * issued.length;
        let spin = 0;
        while (remaining > 0) {
            state = (Math.imul(state, 1664525) + 1013904223) >>> 0;
            const slot = Math.floor((state / 4294967296) / giftProbability);
            if (slot < issued.length && issued[slot] < quantityPerGift) {
                issued[slot]++;
                remaining--;
            }
            spin++;
        }
        depletionSpins.push(spin);
    }
    depletionSpins.sort((a, b) => a - b);
    const at = (fraction) => depletionSpins[Math.floor((batches - 1) * fraction)];
    return {
        quantityPerGift,
        totalInventory: quantityPerGift * 5,
        batches,
        mean: depletionSpins.reduce((sum, value) => sum + value, 0) / batches,
        min: depletionSpins[0],
        median: at(0.5),
        p95: at(0.95),
        max: depletionSpins.at(-1),
    };
}

const scenarios = [
    simulateDepletion(10, 1000, 0x9e3779b9),
    simulateDepletion(50, 500, 0xc2b2ae35),
    simulateDepletion(200, 200, 0x9e3779b9),
    simulateDepletion(1000, 100, 0x85ebca6b),
];

console.log(JSON.stringify({
    daily: dailySpins.map((spins, index) => ({
        date: `2026-09-${25 + index}`,
        spins,
        expectedGifts: spins * giftProbability * 5,
        expectedPerGift: spins * giftProbability,
    })),
    observedAverageDailySpins: dailySpins.reduce((sum, value) => sum + value, 0) / dailySpins.length,
    remainingDays,
    scenarios,
}, null, 2));
