$(function () {
    var caddieId = $('#CaddieId').val();
    var bookingService = genora.multiTenancy.appServices.caddies.caddieBooking;
    var ratingService = genora.multiTenancy.appServices.caddies.caddieRating;
    var editModal = new abp.ModalManager(abp.appPath + 'AppCaddies/EditModal');

    // ── Phone hover tooltip ─────────────────────────────────────────
    var $phoneEl = $('#phoneDisplay');
    var maskedPhone = $phoneEl.data('masked') || '';
    var fullPhone = $phoneEl.data('full') || '';
    $phoneEl.on('mouseenter', function () { $(this).text(fullPhone); });
    $phoneEl.on('mouseleave', function () { $(this).text(maskedPhone); });

    // ── Edit FAB ────────────────────────────────────────────────────
    $('#btnEditCaddie').click(function () {
        editModal.open({ id: $(this).data('id') });
    });
    editModal.onResult(function () { window.location.reload(); });

    // ── Helper: render star row (floor-based fill) ──────────────────
    function renderStars(avg, total) {
        total = total || 5;
        var filled = Math.floor(avg);
        var stars = '';
        for (var i = 1; i <= total; i++) {
            if (i <= filled)
                stars += '<i class="fa fa-star" style="color:#f59e0b;font-size:13px;"></i>';
            else
                stars += '<i class="fa fa-star" style="color:#cbd5e1;font-size:13px;"></i>';
        }
        return stars;
    }

    // ── Booking History Tab (DataTable) ─────────────────────────────
    var bookingTable = $('#tabBookingTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            pageLength: 10,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(bookingService.getList, function () {
                return { caddieId: caddieId, maxResultCount: 10 };
            }),
            columnDefs: [
                {
                    title: 'Mã Booking',
                    data: 'bookingCode',
                    render: function (data, type, row) {
                        return '<a href="/AppCaddieBookings/Detail?id=' + row.id + '" style="font-weight:700;color:var(--caddie-primary);text-decoration:none;">#' + data + '</a>';
                    }
                },
                {
                    title: 'Ngày & Giờ đặt',
                    data: 'creationTime',
                    render: function (data) {
                        if (!data) return '—';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy HH:mm');
                    }
                },
                {
                    title: 'Tên Golfer',
                    data: 'customerName',
                    render: function (data) { return '<strong>' + (data || '—') + '</strong>'; }
                },
                {
                    title: 'Ngày & Giờ chơi',
                    data: 'bookingDate',
                    render: function (data, type, row) {
                        if (!data) return '—';
                        var date = luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy');
                        var time = row.startTime ? row.startTime.substring(0, 5) : '';
                        return date + (time ? ' ' + time : '');
                    }
                },
                {
                    title: 'TT Thanh toán',
                    data: 'paymentStatus',
                    className: 'text-center',
                    render: function (data, type, row) {
                        var style = data === 2
                            ? 'background:#dcfce7;color:#166534;border:1px solid #bbf7d0;'
                            : 'background:#fef9c3;color:#a16207;border:1px solid #fde68a;';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">' + (row.paymentStatusText || '—') + '</span>';
                    }
                },
                {
                    title: 'TT Chơi',
                    data: 'status',
                    className: 'text-center',
                    render: function (data, type, row) {
                        var colors = { 1: 'background:#dbeafe;color:#1e40af;', 2: 'background:#fef3c7;color:#92400e;', 3: 'background:#dcfce7;color:#166534;', 4: 'background:#fef2f2;color:#991b1b;' };
                        var style = colors[data] || 'background:#f3f4f6;color:#6b7280;';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">' + (row.statusText || '—') + '</span>';
                    }
                },
                {
                    title: 'Đánh giá',
                    data: 'bookingRatingAvg',
                    className: 'text-center',
                    render: function (data) {
                        if (data === null || data === undefined) {
                            return '<span style="color:#9ca3af;font-size:12px;">Chưa đánh giá</span>';
                        }
                        return '<span title="' + data.toFixed(1) + '/5">' + renderStars(data) + ' <span style="font-size:11px;color:var(--caddie-on-surface-variant);margin-left:2px;">' + data.toFixed(1) + '</span></span>';
                    }
                }
            ]
        })
    );

    // ── Rating Tab (DataTable) ──────────────────────────────────────
    var ratingTable = $('#tabRatingTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            pageLength: 10,
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(ratingService.getList, function () {
                return { caddieId: caddieId, maxResultCount: 10 };
            }),
            columnDefs: [
                {
                    // Mã đánh giá (booking code) — link opens review detail modal
                    title: 'Mã đánh giá',
                    data: 'bookingCode',
                    render: function (data, type, row) {
                        var code = data ? '#' + data : '#' + row.id.substring(0, 8).toUpperCase();
                        return '<a href="javascript:void(0);" class="rating-view-detail" data-id="' + row.id + '" style="font-weight:700;color:var(--caddie-primary);text-decoration:none;">' + code + '</a>';
                    }
                },
                {
                    title: 'Ngày đánh giá',
                    data: 'creationTime',
                    render: function (data) {
                        if (!data) return '—';
                        return luxon.DateTime.fromISO(data).toFormat('dd/MM/yyyy HH:mm');
                    }
                },
                {
                    title: 'Khách hàng',
                    data: 'customerName',
                    render: function (data) { return '<strong>' + (data || '—') + '</strong>'; }
                },
                {
                    title: 'Đánh giá',
                    data: 'computedRating',
                    className: 'text-center',
                    render: function (data, type, row) {
                        var rating = parseFloat(data || 0);
                        if (!rating || rating === 0) {
                            return '<span style="color:#9ca3af;font-size:12px;">Chưa đánh giá</span>';
                        }
                        return renderStars(rating) + ' <strong style="font-size:11px;margin-left:2px;">' + rating.toFixed(1) + '</strong>';
                    }
                },
                {
                    title: 'Nhận xét',
                    data: 'comment',
                    render: function (data) {
                        if (!data) return '<span style="color:#707783">—</span>';
                        var truncated = data.length > 50 ? data.substring(0, 50) + '...' : data;
                        return '<em style="font-size:12px;color:var(--caddie-on-surface-variant);">"' + truncated + '"</em>';
                    }
                },
                {
                    title: 'Trạng thái',
                    data: 'approvalStatus',
                    className: 'text-center',
                    render: function (data, type, row) {
                        var styles = { 1: 'background:#fef9c3;color:#a16207;', 2: 'background:#dcfce7;color:#166534;', 3: 'background:#f3f4f6;color:#6b7280;' };
                        var style = styles[data] || '';
                        return '<span style="' + style + 'font-size:10px;font-weight:700;padding:4px 8px;border-radius:4px;text-transform:uppercase;">' + (row.approvalStatusText || '—') + '</span>';
                    }
                }
            ]
        })
    );

    // ── Rating detail modal ─────────────────────────────────────────
    $(document).on('click', '.rating-view-detail', function () {
        var id = $(this).data('id');
        ratingService.get(id).then(function (dto) {
            // Populate booking info card
            var bookingCode = dto.bookingCode ? '#' + dto.bookingCode : '—';
            var bookingDate = dto.bookingDate
                ? luxon.DateTime.fromISO(dto.bookingDate).toFormat('dd/MM/yyyy')
                : '—';
            var bookingTime = dto.bookingStartTime
                ? (typeof dto.bookingStartTime === 'string' ? dto.bookingStartTime.substring(0, 5) : '')
                : '';
            $('#reviewBookingCode').text(bookingCode);
            $('#reviewBookingDate').text(bookingDate + (bookingTime ? ' - ' + bookingTime : ''));

            // Populate customer card
            $('#reviewCustomerName').text(dto.customerName || '—');
            var avatar = dto.customerAvatar;
            if (avatar) {
                $('#reviewCustomerAvatar').attr('src', avatar).css('display', 'block');
                $('#reviewCustomerInitials').css('display', 'none');
            } else {
                $('#reviewCustomerAvatar').css('display', 'none');
                var initials = (dto.customerName || '?').split(' ').map(function(n){ return n[0]; }).join('').substring(0, 2).toUpperCase();
                $('#reviewCustomerInitials').text(initials).css('display', 'flex');
            }

            // Skill ratings
            var skillHtml = '';
            if (dto.details && dto.details.length > 0) {
                dto.details.forEach(function (d) {
                    var stars = '';
                    for (var i = 1; i <= 5; i++) {
                        if (i <= d.score)
                            stars += '<i class="fa fa-star" style="color:#f59e0b;font-size:15px;"></i>';
                        else
                            stars += '<i class="fa fa-star" style="color:#cbd5e1;font-size:15px;"></i>';
                    }
                    skillHtml += '<div class="d-flex justify-content-between align-items-center">'
                        + '<span style="font-size:0.875rem;font-weight:500;color:var(--caddie-on-surface-variant);">' + (d.skillName || '—') + '</span>'
                        + '<span style="display:flex;gap:3px;">' + stars + '</span>'
                        + '</div>';
                });
            } else {
                skillHtml = '<p style="color:#707783;margin:0;">Không có đánh giá kỹ năng</p>';
            }
            $('#reviewSkillRatings').html(skillHtml);

            // Comment
            var comment = dto.comment ? '"' + dto.comment + '"' : '— Không có nhận xét —';
            $('#reviewComment').text(comment);

            new bootstrap.Modal(document.getElementById('reviewDetailModal')).show();
        });
    });

    // ── Next Booking Card ────────────────────────────────────────────
    var caddieName = $('.caddie-profile-name').text() || 'Caddy';
    bookingService.getList({ caddieId: caddieId, maxResultCount: 1, status: 1 }).then(function (res) {
        if (res.items.length === 0) {
            return bookingService.getList({ caddieId: caddieId, maxResultCount: 1, status: 2 });
        }
        return res;
    }).then(function (res) {
        if (res.items.length > 0) {
            var b = res.items[0];
            var dateStr = luxon.DateTime.fromISO(b.bookingDate).toFormat('dd/MM/yyyy');
            var timeStr = b.startTime ? b.startTime.substring(0, 5) : '';
            $('#nextBookingDate').text(dateStr + (timeStr ? ' - ' + timeStr : ''));
            $('#nextBookingInfo').text('Khách: ' + b.customerName + ' | ' + b.statusText);
            $('#btnViewBookingDetail').data('booking-id', b.id);
        } else {
            $('#nextBookingDate').text('—');
            $('#nextBookingInfo').text('Không có lịch sắp tới');
        }
    });

    // View booking detail button
    $('#btnViewBookingDetail').click(function () {
        var bookingId = $(this).data('booking-id');
        if (bookingId) {
            window.location.href = '/AppCaddieBookings/Detail?id=' + bookingId;
        } else {
            abp.notify.info('Caddy ' + caddieName + ' không có lịch nào sắp tới.');
        }
    });
});
