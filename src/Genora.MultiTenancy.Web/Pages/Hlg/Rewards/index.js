$(function () {
    window.hlgAdmin.init({
        "service": "hlgRewardAdmin",
        "folder": "Rewards",
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
                "kind": "rewardType"
            },
            {
                "data": "pointCost",
                "label": "PointCost",
                "kind": "number"
            },
            {
                "data": "stockQuantity",
                "label": "StockQuantity",
                "kind": "stock"
            },
            {
                "data": "voucherCode",
                "label": "VoucherCode",
                "kind": "text"
            },
            {
                "data": "isActive",
                "label": "IsActive",
                "kind": "bool"
            }
        ]
    });
});