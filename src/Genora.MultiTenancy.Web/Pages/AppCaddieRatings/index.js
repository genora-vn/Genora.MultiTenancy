$(function () {
    var ratingService = genora.multiTenancy.appServices.caddies.caddieRating;
    var canEdit = $('#CanEdit').val() === 'true';
    var canDelete = $('#CanDelete').val() === 'true';
    var currentRatingId = null;

    var dataTable = $('#CaddieRatingsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(ratingService.getList, function () {
                return {
                    approvalStatus: $('#RatingApprovalFilter').val() || undefined,
                    fromDate: $('#RatingFromDate').val() || undefined,
                    toDate: $('#RatingToDate').val() || undefined
                };
            }),
            columnDefs: [
                {
                    title: 'Thao tác',
                    orderable: false,
                    width: '60px',
                    render: function (data, type, row) {
                        var items = [];
                        items.push('<li><a class="dropdown-item rating-action-detail" data-id="' + row.id + '"><i class="fa fa-eye me-2 text-primary"></i>Xem chi tiết</a></li>');
                        if (canEdit && row.approvalStatus === 1) {
                            items.push('<li><a class="dropdown-item rating-action-approve" data-id="' + row.id + '"><i class="fa fa-check me-2 text-success"></i>Duyệt</a></li>');
                            items.push('<li><a class="dropdown-item rating-action-reject" data-id="' + row.id + '"><i class="fa fa-times me-2 text-danger"></i>Từ chối</a></li>');
                        }
                        if (canDelete) {
                            items.push('<li><hr class="dropdown-divider"></li>');
                            items.push('<li><a class="dropdown-item text-danger rating-action-delete" data-id="' + row.id + '"><i class="fa fa-trash me-2"></i>Xóa</a></li>');
                        }
                        return '<div class="dropdown"><button class="caddie-action-btn dropdown-toggle" data-bs-toggle="dropdown"><i class="fa fa-ellipsis-v"></i></button><ul class="dropdown-menu dropdown-menu-end shadow-sm">' + items.join('') + '</ul></div>';
                    }
                },
                {
                    title: 'Caddy',
                    data: 'caddieName',
                    render: function (data, type, row) {
                        return '<strong>' + (data || '—') + '</strong><br/><small style="color:var(--caddie-primary);">' + (row.caddieCode || '') + '</small>';
                    }
                },
                {
                    title: 'Khách hàng',
                    data: 'customerName',
                    render: function (data) { return '<strong>' + (data || '—') + '</strong>'; }
                },
                {
                    title: 'Ngày chơi',
                    data: 'bookingDate',
                    render: function (data, type, row) {
                        if (!data) return '—';
                        var date = luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                        var time = row.bookingStartTime ? row.bookingStartTime.substring(0, 5) : '';
                        return date + (time ? ' <small style="color:var(--caddie-on-surface-variant);">' + time + '</small>' : '');
                    }
                },
                {
                    title: 'Thời gian đánh giá',
                    data: 'creationTime',
                    render: function (data) {
                        if (!data) return '—';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy HH:mm');
                    }
                },
                {
                    title: 'Đánh giá',
                    data: 'overallRating',
                    render: function (data) {
                        var stars = '';
                        for (var i = 1; i <= 5; i++) {
                            stars += i <= data
                                ? '<i class="fa fa-star caddie-stars"></i>'
                                : '<i class="fa fa-star caddie-stars star-empty" style="color:#cbd5e1;"></i>';
                        }
                        return stars;
                    }
                },
                {
                    title: 'Nhận xét',
                    data: 'comment',
                    render: function (data) {
                        if (!data) return '<span style="color:#707783">—</span>';
                        var truncated = data.length > 40 ? data.substring(0, 40) + '...' : data;
                        return '<em style="color:var(--caddie-on-surface-variant);font-size:12px;">"' + truncated + '"</em>';
                    }
                },
                {
                    title: 'Trạng thái',
                    data: 'approvalStatus',
                    render: function (data, type, row) {
                        var styles = {
                            1: 'background:#fef9c3;color:#a16207;border:1px solid #fde68a;',
                            2: 'background:#dcfce7;color:#166534;border:1px solid #bbf7d0;',
                            3: 'background:#fef2f2;color:#991b1b;border:1px solid #fecaca;'
                        };
                        var style = styles[data] || 'background:#f3f4f6;color:#6b7280;';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">' + (row.approvalStatusText || '—') + '</span>';
                    }
                }
            ]
        })
    );

    // Search
    $('#BtnSearch').click(function () { dataTable.ajax.reload(); });

    // Detail
    $(document).on('click', '.rating-action-detail', function () {
        var id = $(this).data('id');
        currentRatingId = id;
        ratingService.get(id).then(function (dto) {
            $('#detailBookingCode').text('#' + (dto.bookingCode || '—'));
            var playDate = dto.bookingDate ? luxon.DateTime.fromISO(dto.bookingDate).toFormat('dd/MM/yyyy') : '';
            var playTime = dto.bookingStartTime ? dto.bookingStartTime.substring(0, 5) : '';
            $('#detailPlayDate').text(playDate + (playTime ? ' - ' + playTime : ''));
            $('#detailCustomerName').text(dto.customerName || '—');
            $('#detailComment').text(dto.comment ? '"' + dto.comment + '"' : '— Không có nhận xét —');

            // Skill ratings
            var skillHtml = '';
            if (dto.details && dto.details.length > 0) {
                dto.details.forEach(function (d) {
                    var stars = '';
                    for (var i = 1; i <= 5; i++) {
                        stars += i <= d.score
                            ? '<i class="fa fa-star caddie-stars"></i>'
                            : '<i class="fa fa-star" style="color:#cbd5e1;"></i>';
                    }
                    skillHtml += '<div class="col-6 d-flex justify-content-between align-items-center" style="font-size:0.875rem;"><span>' + (d.skillName || '—') + '</span><span>' + stars + '</span></div>';
                });
            } else {
                skillHtml = '<div class="col-12 text-muted">Không có đánh giá kỹ năng</div>';
            }
            $('#detailSkillRatings').html(skillHtml);

            // Show/hide approve/reject buttons
            if (dto.approvalStatus === 1 && canEdit) {
                $('#btnApproveRating, #btnRejectRating').show();
            } else {
                $('#btnApproveRating, #btnRejectRating').hide();
            }

            var modal = new bootstrap.Modal(document.getElementById('ratingDetailModal'));
            modal.show();
        });
    });

    // Approve from detail modal
    $('#btnApproveRating').click(function () {
        if (!currentRatingId) return;
        ratingService.approveReject(currentRatingId, { approvalStatus: 2 }).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('ratingDetailModal')).hide();
            dataTable.ajax.reload();
            abp.notify.success('Đã duyệt đánh giá');
        }).catch(function (err) { abp.notify.error(err.message || 'Lỗi'); });
    });

    // Reject from detail modal → open reason modal
    $('#btnRejectRating').click(function () {
        bootstrap.Modal.getInstance(document.getElementById('ratingDetailModal')).hide();
        $('#rejectReasonInput').val('');
        var modal = new bootstrap.Modal(document.getElementById('rejectReasonModal'));
        modal.show();
    });

    // Confirm reject
    $('#btnConfirmReject').click(function () {
        var reason = $('#rejectReasonInput').val();
        if (!reason) { abp.notify.error('Vui lòng nhập lý do từ chối'); return; }
        ratingService.approveReject(currentRatingId, { approvalStatus: 3, rejectReason: reason }).then(function () {
            bootstrap.Modal.getInstance(document.getElementById('rejectReasonModal')).hide();
            dataTable.ajax.reload();
            abp.notify.success('Đã từ chối đánh giá');
        }).catch(function (err) { abp.notify.error(err.message || 'Lỗi'); });
    });

    // Quick approve from dropdown
    $(document).on('click', '.rating-action-approve', function () {
        var id = $(this).data('id');
        abp.message.confirm('Duyệt đánh giá này?', 'Xác nhận').then(function (confirmed) {
            if (confirmed) {
                ratingService.approveReject(id, { approvalStatus: 2 }).then(function () {
                    dataTable.ajax.reload();
                    abp.notify.success('Đã duyệt đánh giá');
                });
            }
        });
    });

    // Quick reject from dropdown
    $(document).on('click', '.rating-action-reject', function () {
        currentRatingId = $(this).data('id');
        $('#rejectReasonInput').val('');
        var modal = new bootstrap.Modal(document.getElementById('rejectReasonModal'));
        modal.show();
    });

    // Delete
    $(document).on('click', '.rating-action-delete', function () {
        var id = $(this).data('id');
        abp.message.confirm('Bạn có chắc chắn muốn xóa đánh giá này?', 'Xác nhận xóa').then(function (confirmed) {
            if (confirmed) {
                ratingService.delete(id).then(function () {
                    dataTable.ajax.reload();
                    abp.notify.success('Đã xóa đánh giá');
                });
            }
        });
    });
});
