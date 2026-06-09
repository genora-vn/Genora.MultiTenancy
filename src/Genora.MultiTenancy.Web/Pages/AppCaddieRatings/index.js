$(function () {
    var ratingService = genora.multiTenancy.appServices.caddies.caddieRating;
    var canEdit = $('#CanEdit').val() === 'true';
    var canDelete = $('#CanDelete').val() === 'true';
    var currentRatingId = null;

    // Load KPI stats
    function loadKpiStats() {
        ratingService.getList({ maxResultCount: 1 }).then(function (res) {
            $('#kpiTotalRatings').text(res.totalCount.toLocaleString());
        });
        ratingService.getList({ maxResultCount: 1, approvalStatus: 1 }).then(function (res) {
            $('#kpiPendingRatings').text(res.totalCount.toLocaleString());
        });
        // Rating avg: fetch ALL approved ratings, compute avg from overallRating
        ratingService.getList({ maxResultCount: 500, approvalStatus: 2 }).then(function (res) {
            if (res.items.length > 0) {
                var sum = 0;
                res.items.forEach(function (r) { sum += r.overallRating; });
                // Divide by TOTAL count (including approved) not just items loaded
                var avg = (sum / res.totalCount).toFixed(1);
                $('#kpiAvgRating').text(avg + ' / 5.0');
                var starsHtml = '';
                var filledStars = Math.floor(parseFloat(avg));
                for (var i = 1; i <= 5; i++) {
                    starsHtml += i <= filledStars
                        ? '<i class="fa fa-star" style="color:#f59e0b;font-size:12px;"></i>'
                        : '<i class="fa fa-star" style="color:#cbd5e1;font-size:12px;"></i>';
                }
                $('#kpiAvgStars').html(starsHtml);
            } else {
                $('#kpiAvgRating').text('0.0 / 5.0');
                $('#kpiAvgStars').html('');
            }
        });
    }
    loadKpiStats();

    function getInitials(name) {
        if (!name) return '?';
        return name.split(' ').map(function (n) { return n[0]; }).join('').substring(0, 2).toUpperCase();
    }

    function renderStars(rating, size) {
        size = size || '13px';
        var stars = '';
        var filled = Math.floor(rating);
        for (var i = 1; i <= 5; i++) {
            if (i <= filled)
                stars += '<i class="fa fa-star" style="color:#f59e0b;font-size:' + size + ';"></i>';
            else
                stars += '<i class="fa fa-star" style="color:#cbd5e1;font-size:' + size + ';"></i>';
        }
        return stars;
    }

    var dataTable = $('#CaddieRatingsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(ratingService.getList, function () {
                return {
                    filter: $('#RatingCaddyFilter').val() || undefined,
                    customerFilter: $('#RatingGolferFilter').val() || undefined,
                    approvalStatus: $('#RatingApprovalFilter').val() || undefined,
                    overallRating: $('#RatingScoreFilter').val() || undefined
                };
            }),
            columnDefs: [
                {
                    title: 'Thao tác',
                    orderable: false,
                    width: '90px',
                    render: function (data, type, row) {
                        var items = [];
                        items.push('<li><a class="dropdown-item rating-action-detail" data-id="' + row.id + '"><i class="fa fa-search me-2 text-info"></i>Xem nhanh đánh giá</a></li>');
                        items.push('<li><a class="dropdown-item rating-action-detail-page" data-id="' + row.id + '"><i class="fa fa-external-link-alt me-2 text-primary"></i>Xem chi tiết</a></li>');
                        if (canEdit && row.approvalStatus === 1) {
                            items.push('<li><hr class="dropdown-divider"></li>');
                            items.push('<li><a class="dropdown-item rating-action-approve" data-id="' + row.id + '"><i class="fa fa-check me-2 text-success"></i>Phê duyệt</a></li>');
                            items.push('<li><a class="dropdown-item rating-action-reject" data-id="' + row.id + '"><i class="fa fa-times me-2 text-danger"></i>Từ chối</a></li>');
                        }
                        if (canDelete) {
                            items.push('<li><hr class="dropdown-divider"></li>');
                            items.push('<li><a class="dropdown-item text-danger rating-action-delete" data-id="' + row.id + '"><i class="fa fa-trash me-2"></i>Xóa</a></li>');
                        }
                        return '<div class="dropdown"><button class="btn btn-sm btn-primary dropdown-toggle" data-bs-toggle="dropdown" style="font-size:11px;font-weight:700;border-radius:6px;padding:4px 10px;">Thao tác <i class="fa fa-chevron-down ms-1" style="font-size:9px;"></i></button><ul class="dropdown-menu dropdown-menu-end shadow-sm">' + items.join('') + '</ul></div>';
                    }
                },
                {
                    title: 'Mã ĐG',
                    data: 'bookingCode',
                    render: function (data) { return '<span style="font-weight:700;color:var(--caddie-primary);">#' + (data || '—') + '</span>'; }
                },
                {
                    title: 'Ngày / Giờ',
                    data: 'creationTime',
                    render: function (data) {
                        if (!data) return '—';
                        var dt = luxon.DateTime.fromISO(data);
                        return '<div><span style="font-size:13px;font-weight:600;">' + dt.toFormat('dd/MM/yyyy') + '</span><br/><span style="font-size:11px;color:var(--caddie-on-surface-variant);">' + dt.toFormat('HH:mm') + '</span></div>';
                    }
                },
                {
                    title: 'Golfer',
                    data: 'customerName',
                    render: function (data) { return '<span style="font-weight:500;">' + (data || '—') + '</span>'; }
                },
                {
                    title: 'Caddy được đánh giá',
                    data: 'caddieName',
                    render: function (data, type, row) {
                        var avatarHtml = '';
                        if (row.caddieAvatar) {
                            avatarHtml = '<img src="' + row.caddieAvatar + '" style="width:28px;height:28px;border-radius:50%;object-fit:cover;" onerror="this.style.display=\'none\';this.nextElementSibling.style.display=\'flex\';" />' +
                                '<span class="d-none align-items-center justify-content-center rounded-circle" style="width:28px;height:28px;background:var(--caddie-surface-container-high);font-size:10px;font-weight:700;">' + getInitials(data) + '</span>';
                        } else {
                            avatarHtml = '<span class="d-inline-flex align-items-center justify-content-center rounded-circle" style="width:28px;height:28px;background:var(--caddie-surface-container-high);font-size:10px;font-weight:700;">' + getInitials(data) + '</span>';
                        }
                        return '<div class="d-flex align-items-center gap-2">' + avatarHtml +
                            '<span style="font-size:13px;font-weight:600;">' + (data || '—') + '</span></div>';
                    }
                },
                {
                    title: 'Rating',
                    data: 'computedRating',
                    render: function (data, type, row) {
                        var rating = parseFloat(data || 0);
                        if (!rating || rating === 0) {
                            return '<span style="color:#9ca3af;font-size:12px;">Chưa đánh giá</span>';
                        }
                        return renderStars(rating) + ' <strong style="font-size:11px;margin-left:3px;">' + rating.toFixed(1) + '</strong>';
                    }
                },
                {
                    title: 'Trạng thái',
                    data: 'approvalStatus',
                    className: 'text-center',
                    render: function (data, type, row) {
                        var styles = { 1: 'background:#fef9c3;color:#a16207;', 2: 'background:var(--caddie-surface-container-high);color:var(--caddie-primary);', 3: 'background:#f3f4f6;color:#6b7280;' };
                        var labels = { 1: 'CHỜ DUYỆT', 2: 'ĐÃ DUYỆT', 3: 'ĐÃ ẨN' };
                        var style = styles[data] || 'background:#f3f4f6;color:#6b7280;';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 10px;border-radius:20px;">' + (labels[data] || row.approvalStatusText || '—') + '</span>';
                    }
                }
            ]
        })
    );

    // Search + score filter
    $('#BtnSearch').click(function () { dataTable.ajax.reload(); });
    $('#RatingCaddyFilter, #RatingGolferFilter').on('keypress', function (e) { if (e.which === 13) dataTable.ajax.reload(); });
    $('#RatingScoreFilter').change(function () { dataTable.ajax.reload(); });

    // Quick view detail modal
    $(document).on('click', '.rating-action-detail', function () {
        var id = $(this).data('id');
        currentRatingId = id;
        ratingService.get(id).then(function (dto) {
            $('#detailRatingCode').text('#' + (dto.bookingCode || dto.id.substring(0, 8)));
            $('#detailCustomerName').text(dto.customerName || '—');
            $('#detailCaddyName').text(dto.caddieName || '—');
            $('#detailCaddyCode').text(dto.caddieCode || '—');

            // Golfer avatar
            if (dto.customerAvatar) {
                $('#detailGolferAvatar').attr('src', dto.customerAvatar).css('display', 'block');
                $('#detailGolferInitials').css('display', 'none');
            } else {
                $('#detailGolferAvatar').css('display', 'none');
                $('#detailGolferInitials').text(getInitials(dto.customerName)).css('display', 'flex');
            }

            // Caddy avatar
            if (dto.caddieAvatar) {
                $('#detailCaddyAvatar').attr('src', dto.caddieAvatar).css('display', 'block');
                $('#detailCaddyInitials').css('display', 'none');
            } else {
                $('#detailCaddyAvatar').css('display', 'none');
                $('#detailCaddyInitials').text(getInitials(dto.caddieName)).css('display', 'flex');
            }

            var playDate = dto.bookingDate ? luxon.DateTime.fromISO(dto.bookingDate).toFormat('dd/MM/yyyy') : '—';
            var playTime = dto.bookingStartTime ? (typeof dto.bookingStartTime === 'string' ? dto.bookingStartTime.substring(0, 5) : '—') : '—';
            $('#detailPlayDate').text(playDate);
            $('#detailPlayTime').text(playTime);

            // Calculate avg rating from skills
            var avgRating = dto.overallRating;
            if (dto.details && dto.details.length > 0) {
                var sum = 0;
                dto.details.forEach(function (d) { sum += d.score; });
                avgRating = sum / dto.details.length;
            }

            // Overall stars
            var overallHtml = renderStars(avgRating, '16px');
            overallHtml += ' <strong style="margin-left:4px;">' + avgRating.toFixed(1) + '</strong>';
            $('#detailOverallStars').html(overallHtml);

            // Comment
            $('#detailComment').text(dto.comment ? '"' + dto.comment + '"' : '— Không có nhận xét —');

            // Skill ratings
            var skillHtml = '';
            if (dto.details && dto.details.length > 0) {
                dto.details.forEach(function (d) {
                    skillHtml += '<div class="d-flex justify-content-between align-items-center mb-3"><span style="font-weight:500;">' + (d.skillName || '—') + '</span><span>' + renderStars(d.score, '13px') + '</span></div>';
                });
            } else {
                skillHtml = '<div class="text-muted">Không có đánh giá kỹ năng chi tiết</div>';
            }
            $('#detailSkillRatings').html(skillHtml);

            // Show/hide buttons
            if (dto.approvalStatus === 1 && canEdit) {
                $('#btnApproveRating, #btnRejectRating').show();
            } else {
                $('#btnApproveRating, #btnRejectRating').hide();
            }

            new bootstrap.Modal(document.getElementById('ratingDetailModal')).show();
        });
    });

    // Open detail page
    $(document).on('click', '.rating-action-detail-page', function () {
        window.location.href = '/AppCaddieRatings/Detail?id=' + $(this).data('id');
    });

    // Approve
    $('#btnApproveRating').click(function () {
        if (!currentRatingId) return;
        ratingService.approveReject(currentRatingId, { approvalStatus: 2 }).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('ratingDetailModal')).hide();
            dataTable.ajax.reload();
            loadKpiStats();
            abp.notify.success('Đã phê duyệt đánh giá');
        }).catch(function (err) { abp.notify.error(err.message || 'Lỗi'); });
    });

    // Reject from detail modal
    $('#btnRejectRating').click(function () {
        bootstrap.Modal.getInstance(document.getElementById('ratingDetailModal')).hide();
        $('#rejectReasonInput').val('');
        new bootstrap.Modal(document.getElementById('rejectReasonModal')).show();
    });

    // Confirm reject
    $('#btnConfirmReject').click(function () {
        var reason = $('#rejectReasonInput').val();
        if (!reason) { abp.notify.error('Vui lòng nhập lý do từ chối'); return; }
        ratingService.approveReject(currentRatingId, { approvalStatus: 3, rejectReason: reason }).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('rejectReasonModal')).hide();
            dataTable.ajax.reload();
            loadKpiStats();
            abp.notify.success('Đã từ chối đánh giá');
        }).catch(function (err) { abp.notify.error(err.message || 'Lỗi'); });
    });

    // Quick approve from dropdown
    $(document).on('click', '.rating-action-approve', function () {
        var id = $(this).data('id');
        abp.message.confirm('Phê duyệt đánh giá này?', 'Xác nhận').then(function (confirmed) {
            if (confirmed) {
                ratingService.approveReject(id, { approvalStatus: 2 }).then(function () {
                    dataTable.ajax.reload();
                    loadKpiStats();
                    abp.notify.success('Đã phê duyệt đánh giá');
                });
            }
        });
    });

    // Quick reject from dropdown
    $(document).on('click', '.rating-action-reject', function () {
        currentRatingId = $(this).data('id');
        $('#rejectReasonInput').val('');
        new bootstrap.Modal(document.getElementById('rejectReasonModal')).show();
    });

    // Delete
    $(document).on('click', '.rating-action-delete', function () {
        var id = $(this).data('id');
        abp.message.confirm('Bạn có chắc chắn muốn xóa đánh giá này?', 'Xác nhận xóa').then(function (confirmed) {
            if (confirmed) {
                ratingService.delete(id).then(function () {
                    dataTable.ajax.reload();
                    loadKpiStats();
                    abp.notify.success('Đã xóa đánh giá');
                });
            }
        });
    });
});
