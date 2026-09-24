# HLG — Cấu hình lượt chơi trắc nghiệm (2026-09-24)

Branch `feature/nghiadt-hoalinh-gamification`, baseline/current HEAD `ca7e0c08daf2938fa23e0c71da9f3d4eb4e8959d`. Các thay đổi của task này chưa commit. Worktree đã có sẵn thay đổi tách menu Knowledge và các file log; toàn bộ được bảo toàn.

Đã bổ sung full chain cho hai cấu hình game trắc nghiệm:

- `QuestionsPerPlay`: số câu active tối đa trả về trong một lượt, lấy theo `HlgQuestion.Index`; session snapshot `TotalQuestions` bằng số câu thực tế đã trả.
- `AllowedWrongAnswers`: số câu được phép sai, được lưu và trả trong game list/detail/start response; giá trị được snapshot vào session khi bắt đầu.
- Create/Edit Razor chỉ hiện hai trường khi `Type = Quiz`; giới hạn tương ứng 1–1000 và 0–1000.
- Game trắc nghiệm mới/chỉnh sửa bắt buộc nhập cả hai và `AllowedWrongAnswers <= QuestionsPerPlay`.
- Game loại khác lưu hai giá trị là null. Game Quiz cũ có null vẫn giữ hành vi cũ ở runtime: dùng toàn bộ câu active và không có giới hạn sai cho tới khi được chỉnh sửa/lưu lại.
- Game đã có session không được đổi type, base score hoặc hai cấu hình lượt chơi sau khi chúng đã được thiết lập. Riêng dữ liệu pre-migration đang null được phép cấu hình lần đầu; session cũ vẫn giữ snapshot `TotalQuestions` và không có wrong-answer limit.

Rule được xác nhận bổ sung: game thất bại khi `WrongAnswerCount > AllowedWrongAnswers`; bằng giới hạn vẫn được chơi. `/answer` đếm từ answer server-side, đánh dấu session kết thúc khi vượt giới hạn và trả `wrongAnswerCount`, `allowedWrongAnswers`, `gameFailed=true` cùng message `Trò chơi thất bại`. `/finish` đối soát lại từ answers và trả cùng trạng thái. Game thất bại không cộng `Customer.BonusPoint`.

Migration mới:

- `20260924023725_AddHlgQuizPlayConfiguration`
- Thêm hai cột nullable `AllowedWrongAnswers`, `QuestionsPerPlay` vào `HLG.AppHlgGames` và snapshot nullable `AllowedWrongAnswers` vào `HLG.AppHlgGameSessions`.
- SQL idempotent: `Migrations/Scripts/AddHlgQuizPlayConfiguration.sql`.
- Migration **chưa apply** vào database.

Verification thực chạy:

- Follow-up wrong-answer enforcement: Web Release build PASS; full build 0 errors/384 existing warnings, final incremental rebuild after the last service patch 0 errors/52 existing warnings.
- Application HLG tests: PASS 57/57 (final Release rerun).
- Web HLG tests: PASS 19/19 (final Release rerun; Debug output đang bị một tiến trình khác khóa DLL nên không dùng kết quả lần Debug đó).
- JavaScript HLG tests: PASS 11/11 (final rerun).
- EF `has-pending-model-changes`: PASS, no pending model changes.
- `git diff --check`: PASS; chỉ có cảnh báo line-ending LF/CRLF, không có whitespace error.

Trong lúc test đã sửa mock cũ của `HlgDesignWebTests.Live_Feed_Only_Joins_A_Visible_Game` từ `GameDto` sang đúng return type hiện tại `GameDetailDto`; đây là compile regression có sẵn sau cập nhật API gần đây.
