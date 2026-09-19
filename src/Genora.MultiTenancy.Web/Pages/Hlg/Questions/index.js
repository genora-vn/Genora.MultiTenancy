$(function () {
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
});
