$(function () {
    window.hlgAdmin.init({
        "service": "hlgProductAdmin",
        "folder": "Products",
        "readOnly": false,
        "columns": [
            {
                "data": "name",
                "label": "Name",
                "kind": "text"
            },
            {
                "data": "summary",
                "label": "Summary",
                "kind": "text"
            },
            {
                "data": "displayOrder",
                "label": "DisplayOrder",
                "kind": "number"
            },
            {
                "data": "isActive",
                "label": "IsActive",
                "kind": "bool"
            }
        ]
    });
});
