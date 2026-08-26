$(function () {
    var service = genora.multiTenancy.appServices.hl25.hl25Report;

    var frameChart = null;

    function getInput() {
        return {
            fromDate: $('#ReportFrom').val() || null,
            toDate: $('#ReportTo').val() || null,
            campaignId: null
        };
    }

    function fmtNumber(v) {
        return (v || v === 0) ? new Intl.NumberFormat('vi-VN').format(v) : '0';
    }

    // ===== Báo cáo 1: Frame =====
    function loadFrameStats(input) {
        service.getFrameStats(input).then(function (r) {
            $('#FrameTotalCreations').text(fmtNumber(r.totalCreations));
            $('#FrameTotalShared').text(fmtNumber(r.totalShared));
            $('#FrameUniqueParticipants').text(fmtNumber(r.uniqueParticipants));

            var labels = (r.byDate || []).map(function (x) { return x.date; });
            var creations = (r.byDate || []).map(function (x) { return x.creationCount; });
            var shared = (r.byDate || []).map(function (x) { return x.sharedCount; });

            var ctx = document.getElementById('FrameChart').getContext('2d');
            if (frameChart) frameChart.destroy();
            frameChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [
                        { label: 'Lượt tạo thiệp', data: creations, borderColor: '#0d6efd', backgroundColor: 'rgba(13,110,253,.1)', tension: .3, fill: true },
                        { label: 'Lượt chia sẻ', data: shared, borderColor: '#198754', backgroundColor: 'rgba(25,135,84,.1)', tension: .3, fill: true }
                    ]
                },
                options: {
                    responsive: true,
                    plugins: { legend: { position: 'top' } },
                    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                }
            });
        });
    }

    // ===== Báo cáo 2: Wheel participation =====
    function loadWheelParticipation(input) {
        service.getWheelParticipationStats(input).then(function (r) {
            $('#WheelUniqueSpinners').text(fmtNumber(r.uniqueSpinners));
            $('#WheelTotalSpins').text(fmtNumber(r.totalSpins));
            $('#WheelTotalTurnsGranted').text(fmtNumber(r.totalTurnsGranted));
            $('#WheelTotalWins').text(fmtNumber(r.totalWins));
        });
    }

    // ===== Báo cáo 3: Wheel by gift =====
    function loadWheelGiftStats(input) {
        service.getWheelGiftStats(input).then(function (r) {
            var tbody = $('#GiftStatsBody');
            tbody.empty();
            var rows = r.rows || [];
            if (rows.length === 0) {
                tbody.append('<tr><td colspan="6" class="text-center text-muted py-3">Chưa có dữ liệu</td></tr>');
                return;
            }
            rows.forEach(function (x) {
                tbody.append(
                    '<tr>' +
                    '<td>' + x.giftName + '</td>' +
                    '<td class="text-end">' + fmtNumber(x.totalQuantity) + '</td>' +
                    '<td class="text-end">' + fmtNumber(x.remainingQuantity) + '</td>' +
                    '<td class="text-end">' + fmtNumber(x.wonCount) + '</td>' +
                    '<td class="text-end">' + fmtNumber(x.deliveredCount) + '</td>' +
                    '<td class="text-end">' + x.winRatePercent + '%</td>' +
                    '</tr>'
                );
            });
        });
    }

    function loadAll() {
        var input = getInput();
        loadFrameStats(input);
        loadWheelParticipation(input);
        loadWheelGiftStats(input);
    }

    if (window.flatpickr) {
        flatpickr('#ReportFrom', { dateFormat: 'Y-m-d' });
        flatpickr('#ReportTo', { dateFormat: 'Y-m-d' });
    }

    $('#ApplyReportFilter').click(loadAll);

    loadAll();
});
