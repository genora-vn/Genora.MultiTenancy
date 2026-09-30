$(function () {
    var l = abp.localization.getResource('MultiTenancy');

    window.hlgAdmin.init({"service": "hlgWinnerAdmin", "folder": "Winners", "readOnly": false, "columns": [{"data": "customerName", "label": "FullName", "kind": "text"}, {"data": "rank", "label": "Rank", "kind": "number"}, {"data": "score", "label": "Score", "kind": "number"}, {"data": "isActive", "label": "Published", "kind": "bool"}]});

    $('#HlgDownloadWinnerTemplate').on('click', function (event) {
        event.preventDefault();
        if (!window.genora || !genora.excel) {
            abp.notify.error(l('Hlg:ExcelHelperUnavailable'));
            return;
        }
        genora.excel.download('api/app/hlg-winner-excel/template', {});
    });

    $('#HlgImportWinners').on('click', function (event) {
        event.preventDefault();
        $('#HlgWinnerExcelFile').trigger('click');
    });

    $('#HlgWinnerExcelFile').on('change', function (event) {
        if (!window.genora || !genora.excel) {
            abp.notify.error(l('Hlg:ExcelHelperUnavailable'));
            return;
        }
        genora.excel.upload({
            url: 'api/app/hlg-winner-excel/import',
            fileInput: event.target,
            onSuccess: function () {
                $('#HlgFilter').trigger('submit');
            }
        });
    });
});
