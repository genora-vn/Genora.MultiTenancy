$(function () {
    window.hlgAdmin.init({
        "service": "hlgGameAdmin",
        "folder": "Games",
        "readOnly": false,
        "columns": [
            {
                "data": "name",
                "label": "Name",
                "kind": "text"
            },
            {
                "data": "type",
                "label": "Type",
                "kind": "gameType"
            },
            {
                "data": "status",
                "label": "Status",
                "kind": "gameStatus"
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
        ],
        "children": "Questions"
    });
});