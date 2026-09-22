$(function () {
    var l = abp.localization.getResource('MultiTenancy');
    var $page = $('#HlgAdmin');
    var gameId = $page.attr('data-parent-id');

    window.hlgAdmin.init({
        "service": "hlgQuestionAdmin",
        "folder": "Questions",
        "readOnly": false,
        "columns": [
            {
                "data": "index",
                "label": "Index",
                "kind": "number"
            },
            {
                "data": "content",
                "label": "Content",
                "kind": "text"
            },
            {
                "data": "timeLimitSec",
                "label": "TimeLimitSec",
                "kind": "number"
            },
            {
                "data": "scoreMultiplier",
                "label": "ScoreMultiplier",
                "kind": "number"
            },
            {
                "data": "isActive",
                "label": "IsActive",
                "kind": "bool"
            }
        ]
    });

    $('#HlgDownloadQuestionTemplate').on('click', function (event) {
        event.preventDefault();
        if (!window.genora || !genora.excel) {
            abp.notify.error(l('Hlg:ExcelHelperUnavailable'));
            return;
        }
        genora.excel.download('api/app/hlg-question-excel/template', { gameId: gameId });
    });

    $('#HlgImportQuestions').on('click', function (event) {
        event.preventDefault();
        $('#HlgQuestionExcelFile').trigger('click');
    });

    $('#HlgQuestionExcelFile').on('change', function (event) {
        if (!window.genora || !genora.excel) {
            abp.notify.error(l('Hlg:ExcelHelperUnavailable'));
            return;
        }
        genora.excel.upload({
            url: 'api/app/hlg-question-excel/import',
            fileInput: event.target,
            data: { gameId: gameId },
            onSuccess: function () {
                $('#HlgFilter').trigger('submit');
            }
        });
    });
});
