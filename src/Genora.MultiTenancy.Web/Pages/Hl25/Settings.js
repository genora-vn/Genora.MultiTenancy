$(function () {
    // Init Summernote cho Thể lệ chương trình (RulesHtml).
    $('.html-editor').each(function () {
        var $editor = $(this);
        if ($editor.next('.note-editor').length) {
            return; // đã init
        }
        $editor.summernote({
            height: 220,
            minHeight: 150,
            maxHeight: 500,
            focus: false,
            toolbar: [
                ['style', ['style']],
                ['font', ['bold', 'italic', 'underline', 'clear']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['insert', ['link', 'picture', 'video']],
                ['view', ['fullscreen', 'codeview', 'help']]
            ]
        });
    });

    // Init flatpickr cho input ngày giờ.
    if (window.flatpickr) {
        flatpickr('.flatpickr-datetime', {
            enableTime: true,
            dateFormat: 'Y-m-d H:i',
            time_24hr: true
        });
    }
});
