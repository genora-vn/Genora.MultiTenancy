$(function () {
    var caddieId = $('#CaddieId').val();
    var editModal = new abp.ModalManager(abp.appPath + 'AppCaddies/EditModal');

    // Edit FAB
    $('#btnEditCaddie').click(function () {
        editModal.open({ id: $(this).data('id') });
    });

    editModal.onResult(function () {
        window.location.reload();
    });

    // Review row click → open modal
    $(document).on('click', '.review-row', function () {
        var modal = new bootstrap.Modal(document.getElementById('reviewDetailModal'));
        modal.show();
    });

    // View booking detail button (placeholder)
    $('#btnViewBookingDetail').click(function () {
        abp.notify.info('Chi tiết booking sẽ được hiển thị khi module Booking Caddie hoàn thành.');
    });
});
