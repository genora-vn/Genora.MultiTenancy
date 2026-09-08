$(function () {
    // Init Summernote cho các vùng HTML (Giới thiệu / Thể lệ chương trình).
    $('.html-editor').each(function () {
        var $editor = $(this);
        if ($editor.next('.note-editor').length) {
            return; // đã init
        }
        $editor.summernote({
            height: 240,
            minHeight: 180,
            maxHeight: 600,
            focus: false,
            toolbar: [
                ['style', ['style']],
                ['font', ['bold', 'italic', 'underline', 'strikethrough', 'superscript', 'subscript', 'clear']],
                ['fontname', ['fontname']],
                ['fontsize', ['fontsize']],
                ['color', ['forecolor', 'backcolor']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['height', ['height']],
                ['table', ['table']],
                ['insert', ['link', 'picture', 'video', 'hr']],
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
