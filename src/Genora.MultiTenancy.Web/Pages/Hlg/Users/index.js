$(function () {
    window.hlgAdmin.init({
        "service": "hlgUserAdmin",
        "folder": "Users",
        "readOnly": true, "detail": true,
        "columns": [
            {
                "data": "customerCode",
                "label": "CustomerCode",
                "kind": "text"
            },
            {
                "data": "fullName",
                "label": "FullName",
                "kind": "text"
            },
            {
                "data": "phoneNumber",
                "label": "PhoneNumber",
                "kind": "text"
            },
            {
                "data": "zaloId",
                "label": "ZaloId",
                "kind": "text"
            },
            {
                "data": "customerType",
                "label": "CustomerType",
                "kind": "customerType"
            },
            {
                "data": "bonusPoint",
                "label": "BonusPoint",
                "kind": "number"
            },
            {
                "data": "isRegistered",
                "label": "IsRegistered",
                "kind": "bool"
            },
            {
                "data": "isActive",
                "label": "IsActive",
                "kind": "bool"
            }
        ]
    });
});
