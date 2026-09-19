$(function () {
    window.hlgAdmin.init({
        "service": "hlgCategoryAdmin",
        "folder": "Categories",
        "readOnly": false,
        "columns": [
            {
                "data": "name",
                "label": "Name",
                "kind": "text"
            },
            {
                "data": "description",
                "label": "Description",
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
        ],
        "children": "Products"
    });
});