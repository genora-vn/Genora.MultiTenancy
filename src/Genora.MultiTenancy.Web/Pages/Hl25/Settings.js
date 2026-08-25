$(function () {
    // Init Summernote cho các trường HTML (Thể lệ / Luật chơi / TVC).
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

    var MAX_BYTES = 5 * 1024 * 1024;

    // Preview + validate 5MB cho input ảnh (logo/banner).
    $('.asset-input').on('change', function () {
        var input = this;
        var previewSelector = $(input).data('preview');
        var file = input.files && input.files[0];
        if (!file) {
            return;
        }

        if (file.size > MAX_BYTES) {
            abp.message.warn('Ảnh vượt quá giới hạn 5MB. Vui lòng chọn ảnh nhỏ hơn.');
            input.value = '';
            return;
        }

        if (previewSelector) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $(previewSelector).attr('src', e.target.result).removeClass('d-none');
            };
            reader.readAsDataURL(file);
        }
    });
});
