$(function () {
    window.hlgAdmin.init({
        "service": "hlgRankingAdmin",
        "folder": "Ranking",
        "readOnly": false,
        "columns": [
            {
                "data": "title",
                "label": "Title",
                "kind": "text"
            },
            {
                "data": "startAt",
                "label": "StartAt",
                "kind": "date"
            },
            {
                "data": "endAt",
                "label": "EndAt",
                "kind": "date"
            },
            {
                "data": "isActive",
                "label": "IsActive",
                "kind": "bool"
            }
        ]
    });
});