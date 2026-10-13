# TASK

ID: 20261012_hlg_winners_by_game

MODULE: HLG

TYPE: Refactor

BRANCH: hotfix/20261012

STATUS: Done (code + test xong, chờ user commit/test local/deploy staging; FE chuyển sang truyền GameId)

OWNER: User

GOAL:
Chuyển API winners từ RankingEventId sang RankingEventGameId.
Chỉ trả về dữ liệu của game đã kết thúc.

EXPECTED OUTPUT:
- API dùng GameId
- Game chưa kết thúc trả về rỗng
- Có CURL test

MEMORY UPDATE REQUIRED:
- ACTIVE_CONTEXT.md
- PROJECT_STATE.md
- TASK_LOG.md

Tiếp tục cập nhật module Hoa Linh Gamification (HLG) với các yêu cầu sau:
Bối cảnh:
- Hiện tại API lấy ra danh sách trúng thưởng đang lấy theo chiến dịch, không đúng với bảng xếp hạng lấy theo trò chơi. Vì là danh sách trúng thưởng sẽ chạy theo thời gian của 1 trò chơi cụ thể (nhiều trò chơi trong 1 chiến dịch), thế nên cần lấy danh sách trúng thưởng của từng trò chơi đã kết thúc thay vì lấy ra toàn bộ danh sách người trúng thưởng theo chiến dịch.
- Dữ liệu ví dụ của Chiến dịch và các trò chơi (game) thuộc chiến dịch:
Bảng Chiến dịch (HLG.AppHlgRankingEvents):
Id	TenantId	Title	Description	StartAt	EndAt	IsActive	ExtraProperties	ConcurrencyStamp	CreationTime	CreatorId	LastModificationTime	LastModifierId	IsDeleted	DeleterId	DeletionTime	GameId
F6356FD2-ABB1-5D62-327F-3A23C04F7781	NULL	Sự kiện xếp hạng Dạ Hương	Sự kiện chiến dịch về nhãn hàng Dạ Hương	2026-10-07 00:00:00.0000000	2026-10-31 23:09:00.0000000	1	{}	226ac90855b546bd982691beebf04e2d	2026-09-17 13:51:39.7986692	NULL	2026-10-12 15:56:29.1496459	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	0	NULL	NULL	72AE3886-B2C3-1AE4-6627-3A23C04F7688
Bảng Trò chơi thuộc chiến dịch (HLG.AppHlgGames):
Id	TenantId	EventId	GameId	DisplayOrder	ExtraProperties	ConcurrencyStamp	CreationTime	CreatorId	LastModificationTime	LastModifierId	IsDeleted	DeleterId	DeletionTime
27AAA848-3739-A338-8936-3A2427A47D8A	NULL	F6356FD2-ABB1-5D62-327F-3A23C04F7781	72AE3886-B2C3-1AE4-6627-3A23C04F7688	0	{}	b76b4c80926d4fca8f95bc6b564e0f31	2026-10-07 15:25:25.1541130	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	NULL	NULL	0	NULL	NULL
D0FC7FBE-2DAB-DE10-3364-3A2427A47D8B	NULL	F6356FD2-ABB1-5D62-327F-3A23C04F7781	433F248A-761B-D1C4-1712-3A2427A043F5	2	{}	09d8011461814494b8003af1987443ba	2026-10-07 15:25:25.1551022	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	NULL	NULL	0	NULL	NULL
210B2257-1A90-7F47-743F-3A2427A47D8B	NULL	F6356FD2-ABB1-5D62-327F-3A23C04F7781	ADBA3E71-7B77-246B-E300-3A2427A0EE46	3	{}	9333b85566404dc181cc45df75905c6b	2026-10-07 15:25:25.1551298	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	NULL	NULL	0	NULL	NULL
6E3AFFAB-D8BF-10A9-ED67-3A2427A47D8B	NULL	F6356FD2-ABB1-5D62-327F-3A23C04F7781	C439A8F3-81AD-E549-6832-3A24279F9C04	1	{}	2d6c3484ea684389b1f2a3bc1063468e	2026-10-07 15:25:25.1547931	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	NULL	NULL	0	NULL	NULL
Bảng cấu hình trò chơi cho chiến dịch (HLG.AppHlgRankingEventGames):
Id	TenantId	Name	Type	ImageUrl	Description	Rules	RewardDescription	Status	StartAt	EndAt	BaseScorePerQuestion	DisplayOrder	IsActive	ExtraProperties	ConcurrencyStamp	CreationTime	CreatorId	LastModificationTime	LastModifierId	IsDeleted	DeleterId	DeletionTime	BadgeText	BannerUrl	AllowedWrongAnswers	QuestionsPerPlay
72AE3886-B2C3-1AE4-6627-3A23C04F7688	NULL	Thử tài tốc độ: Dạ Hương Tuần 1	1	https://hoalinh-staging.genora.vn//uploads/hlg/8cfc81eb-4693-2434-c5b2-3a21cccfe131/e90c9151aa574a4d8ad051b04f329735.jpg	Tất cả người dùng Zalo có tài khoản hợp lệ và đang sinh sống tại Việt Nam.	<ol><li>Mở Mini App Game từ menu Zalo.</li><li>Nhấn nút “Bắt đầu” để tham gia trả lời câu hỏi.</li><li>Hoàn thành thử thách để tích lũy điểm thưởng.</li></ol>	<p>Điểm số dựa trên số câu trả lời đúng.</p><p>Hệ thống sẽ ghi nhận và lưu lại số điểm ngay khi kết thúc lượt chơi.</p>	3	2026-09-01 00:00:00.0000000	2026-10-12 15:51:00.0000000	10	1	1	{}	13a38316229f4175bf5ff5cf11aa4ad7	2026-09-17 13:51:39.5615081	NULL	2026-10-12 15:56:29.1496932	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	0	NULL	NULL	NULL	https://hoalinh-staging.genora.vn//uploads/hlg/8cfc81eb-4693-2434-c5b2-3a21cccfe131/e90c9151aa574a4d8ad051b04f329735.jpg	1	10
C439A8F3-81AD-E549-6832-3A24279F9C04	NULL	Thử tài tốc độ: Dạ Hương Tuần 2	1	https://hoalinh-staging.genora.vn//uploads/hlg/8cfc81eb-4693-2434-c5b2-3a21cccfe131/e90c9151aa574a4d8ad051b04f329735.jpg	Tất cả người dùng Zalo có tài khoản hợp lệ và đang sinh sống tại Việt Nam.	<ol><li>Mở Mini App Game từ menu Zalo.</li><li>Nhấn nút “Bắt đầu” để tham gia trả lời câu hỏi.</li><li>Hoàn thành thử thách để tích lũy điểm thưởng.</li></ol>	<p>Điểm số dựa trên số câu trả lời đúng.</p><p>Hệ thống sẽ ghi nhận và lưu lại số điểm ngay khi kết thúc lượt chơi.</p>	2	2026-10-12 15:54:00.0000000	2026-10-17 23:59:00.0000000	10	2	1	{}	5956bec2a99745d0ae9c742d08d6b75b	2026-10-07 15:20:05.2552594	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	2026-10-12 15:53:53.1481352	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	0	NULL	NULL	NULL	https://hoalinh-staging.genora.vn//uploads/hlg/8cfc81eb-4693-2434-c5b2-3a21cccfe131/e90c9151aa574a4d8ad051b04f329735.jpg	1	10
433F248A-761B-D1C4-1712-3A2427A043F5	NULL	Thử tài tốc độ: Dạ Hương Tuần 3	1	https://hoalinh-staging.genora.vn//uploads/hlg/8cfc81eb-4693-2434-c5b2-3a21cccfe131/e90c9151aa574a4d8ad051b04f329735.jpg	Tất cả người dùng Zalo có tài khoản hợp lệ và đang sinh sống tại Việt Nam.	<ol><li>Mở Mini App Game từ menu Zalo.</li><li>Nhấn nút “Bắt đầu” để tham gia trả lời câu hỏi.</li><li>Hoàn thành thử thách để tích lũy điểm thưởng.</li></ol>	<p>Điểm số dựa trên số câu trả lời đúng.</p><p>Hệ thống sẽ ghi nhận và lưu lại số điểm ngay khi kết thúc lượt chơi.</p>	1	2026-10-18 00:00:00.0000000	2026-10-24 23:59:00.0000000	10	3	1	{}	762f171217414d73b56d3432345fd3fd	2026-10-07 15:20:48.2469009	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	NULL	NULL	0	NULL	NULL	NULL	https://hoalinh-staging.genora.vn//uploads/hlg/8cfc81eb-4693-2434-c5b2-3a21cccfe131/e90c9151aa574a4d8ad051b04f329735.jpg	1	10
ADBA3E71-7B77-246B-E300-3A2427A0EE46	NULL	Thử tài tốc độ: Dạ Hương Tuần 4	1	https://hoalinh-staging.genora.vn//uploads/hlg/8cfc81eb-4693-2434-c5b2-3a21cccfe131/e90c9151aa574a4d8ad051b04f329735.jpg	Tất cả người dùng Zalo có tài khoản hợp lệ và đang sinh sống tại Việt Nam.	<ol><li>Mở Mini App Game từ menu Zalo.</li><li>Nhấn nút “Bắt đầu” để tham gia trả lời câu hỏi.</li><li>Hoàn thành thử thách để tích lũy điểm thưởng.</li></ol>	<p>Điểm số dựa trên số câu trả lời đúng.</p><p>Hệ thống sẽ ghi nhận và lưu lại số điểm ngay khi kết thúc lượt chơi.</p>	1	2026-10-25 00:00:00.0000000	2026-10-31 23:59:00.0000000	100	4	1	{}	72b15088c63b41c5afe894d6dad658ea	2026-10-07 15:21:31.8469924	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	2026-10-07 15:22:23.8951588	BF1D1E04-64BF-FE46-00F0-3A238E1B584D	0	NULL	NULL	NULL	https://hoalinh-staging.genora.vn//uploads/hlg/8cfc81eb-4693-2434-c5b2-3a21cccfe131/e90c9151aa574a4d8ad051b04f329735.jpg	1	10
Yêu cầu chỉnh sửa:
- Hãy cập nhật lại API lấy ra Danh sách trúng thưởng đổi param truyền vào là ID của chiến dịch thành ID của trò chơi đã kết thúc thuộc chiến dịch (vì trò chơi phải kết thúc thì mới trao giải).
- Đây là API lấy ra Danh sách trúng thưởng theo Chiến dịch hiện tại:
CURL: curl --location 'https://spirited-whooping-uncouth.ngrok-free.dev/api/mini-app/hlg/ranking/events/F6356FD2-ABB1-5D62-327F-3A23C04F7781/winners'
RESPONSE: {
    "data": [
        {
            "prizeId": "f7e7af73-91ed-67c1-8211-3a23db1d7417",
            "rank": 1,
            "userId": "af5702ca-f51e-d440-f87d-3a24264c71b7",
            "displayName": "Đặng Anh Tần",
            "avatarUrl": "https://s120-26-ava-talk.zadn.vn/11/f45ac7d1a989021235d6aa5d987a16a8.jpg?key=q1nCqocL1aISWlx7XAyoZA&time=1796978633",
            "score": 90,
            "isCurrentUser": false
        }
    ]
}
- Đây là API lấy ra Danh sách trúng thưởng theo Trò chơi Thử tài tốc độ: Dạ Hương Tuần 1 đã kết thúc mong muốn:
CURL: curl --location 'https://spirited-whooping-uncouth.ngrok-free.dev/api/mini-app/hlg/ranking/events/72AE3886-B2C3-1AE4-6627-3A23C04F7688/winners' 
- Tương tự đây là API lấy ra Danh sách trúng thưởng theo Trò chơi Thử tài tốc độ: Dạ Hương Tuần 2 đã kết thúc mong muốn:
CURL: curl --location 'https://spirited-whooping-uncouth.ngrok-free.dev/api/mini-app/hlg/ranking/events/C439A8F3-81AD-E549-6832-3A24279F9C04/winners'
- Còn Trò chơi các tuần khác chưa kết thúc thì không có dữ liệu trả về.
- Gửi lại CURL API để anh test và ghép lên front end Zalo mini app.
Thông tin task bàn giao cho em, hãy lập kế hoạch và xác nhận lại sau khi phân tích xong để anh chuyển sang chế độ build để em chỉnh sửa:
1. Task thuộc module nào? (HLG / Hoa Linh Sales / HL25 / Golf / Salon / Gateway / khác):
Trả lời: Task thuộc Hoa Linh Gamification module (HLG).
2. Loại việc: tính năng mới, sửa bug, refactor, hay vận hành (apply migration/deploy)?
Trả lời: Đây là loại việc refactor không phải tính năng mới. Không cần apply migration còn anh tự commit code và test trên local trước khi deploy lên staging để test.
3. Làm trên nhánh nào? Tiếp tục dev hay tạo nhánh mới (hotfix/* hoặc feature/*)?:
Trả lời: Em hãy làm trên nhánh anh đã tạo là: hotfix/20261012 (nhánh anh đã checkout).
Sau khi hoàn tất luôn luôn lưu memory, cập nhật trạng thái dự án như rules anh đã lưu trong memory system (các file cập nhật như: ACTIVE_CONTEXT.md, MEMORY.md, PROJECT_STATE.md, TASK_LOG.md,... hãy kiểm tra cần cập nhật file nào trong memory system thì cập nhật cho đầy đủ để sau đọc lại là biết và tiếp tục các task anh bàn giao).