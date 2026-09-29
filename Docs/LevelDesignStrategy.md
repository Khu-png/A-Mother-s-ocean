# A Mother's Ocean - Level Design Strategy v1

## 1. Mục tiêu thiết kế

Người chơi dẫn cá mẹ đi qua bản đồ, cứu toàn bộ cá con (Target) và tới đích. Trải nghiệm chính không nằm ở tốc độ thao tác mà ở việc quan sát, dự đoán và chọn đúng thứ tự di chuyển.

Mỗi level cần tạo ra ít nhất một khoảnh khắc người chơi có thể diễn đạt bằng câu:

> "Mình phải làm việc này trước, vì nếu làm việc kia trước thì đường đi sẽ thay đổi hoặc bị khóa."

Ba nguyên tắc chính:

1. Mỗi level chỉ có một ý tưởng chính.
2. Mọi lựa chọn sai đều phải dạy cho người chơi một quy luật.
3. Độ khó tăng bằng chiều sâu suy luận, không chỉ bằng số lượng tile.

## 2. Luật tạo nên puzzle

- Người chơi di chuyển theo bốn hướng trên lưới.
- Không thể đi qua Block.
- Không thể đi lại một ô đã đánh dấu, ngoại trừ lùi đúng về ô trước đó.
- Phải thu thập tất cả Target trước khi Finish mở.
- Pipe chỉ cho phép đi theo các đầu nối hiện tại của nó.
- Khi bước vào Rotate Button, tất cả Pipe cùng xoay một bước.
- Khi lùi khỏi Rotate Button, trạng thái xoay được hoàn tác.

Hệ quả thiết kế quan trọng: một lời giải hoàn chỉnh là một đường không tự cắt, đi từ Start, qua tất cả Target, rồi tới Finish trong đúng các trạng thái xoay cần thiết.

## 3. Từ vựng chiến thuật

### 3.1. Target Ordering

Người chơi phải lấy các Target theo đúng thứ tự. Một Target trông gần hơn có thể không phải Target nên lấy trước.

### 3.2. Rotation Timing

Người chơi phải quyết định lấy Target trước hay kích hoạt Rotate Button trước.

### 3.3. Rotation Phase

Nhiều Rotate Button khác nhau tạo ra các trạng thái xoay liên tiếp. Người chơi phải dự đoán Pipe sau một hoặc nhiều lần xoay.

### 3.4. Route Commitment

Khi đi vào một khu vực, đường đã đánh dấu khiến người chơi không thể sử dụng lại lối cũ. Người chơi phải kiểm tra lối ra trước khi đi vào.

### 3.5. False Opportunity

Một đường ngắn hoặc một Target gần được đặt để hấp dẫn người chơi, nhưng chọn nó quá sớm sẽ phá hỏng kế hoạch dài hạn.

### 3.6. Global Consequence

Một Rotate Button ở một nơi làm thay đổi Pipe tại nơi khác. Hành động và hậu quả không nằm cạnh nhau, buộc người chơi phải quan sát toàn bản đồ.

## 4. Các trục đo độ khó

Không dùng một chỉ số duy nhất để kết luận độ khó. Mỗi level được mô tả bằng các trục sau:

| Trục | Dễ | Trung bình | Khó |
|---|---|---|---|
| Điểm lựa chọn | 0-1 | 2-3 | 4 trở lên |
| Độ sâu nhánh sai | 1-2 bước | 3-5 bước | 6 bước trở lên |
| Cơ chế kết hợp | 1 | 2 | 3 trở lên |
| Số trạng thái xoay cần dự đoán | 0-1 | 2 | 3-4 |
| Hậu quả | Nhìn thấy ngay | Cách vài bước | Ở khu vực khác hoặc gần cuối |
| Lời giải | Có thể có nhiều | Nên ít | Nên có một lời giải chính |

Độ dài đường đi chỉ là tải thao tác. Nó không được dùng một mình để tăng độ khó.

## 5. Nhịp dạy một cơ chế

Mỗi cơ chế mới đi qua năm giai đoạn:

1. Giới thiệu: cơ chế xuất hiện trong tình huống không thể thất bại.
2. Luyện tập: người chơi phải chủ động sử dụng cơ chế.
3. Biến thể: cách dùng quen thuộc không còn đủ.
4. Kết hợp: cơ chế mới kết hợp với một cơ chế cũ.
5. Kiểm tra: level không hướng dẫn và có lựa chọn đánh lạc hướng.

Sau một level kiểm tra khó nên có một level ngắn hơn để tạo nhịp nghỉ.

## 6. Kế hoạch 12 level đầu

Các chỉ số dưới đây là mục tiêu thiết kế. Chúng cần được Solver và playtest xác nhận sau khi dựng map.

### Level 1 - Reunion

- Bài học: di chuyển, thu thập Target và tới Finish.
- Cấu trúc: một hành lang thẳng `S -> T -> F`.
- Lựa chọn sai: không có.
- Độ khó mục tiêu: 1/5.
- Đường giải mục tiêu: 4-6 bước.

### Level 2 - Two Children

- Bài học: Finish chỉ mở khi thu thập đủ Target.
- Cấu trúc: hai Target nằm trên cùng một tuyến đường có một khúc cua.
- Lựa chọn sai: người chơi có thể đi gần Finish nhưng thấy Finish chưa mở.
- Độ khó mục tiêu: 1/5.
- Điểm lựa chọn mục tiêu: 0-1.

### Level 3 - Which One First?

- Bài học: thứ tự Target quan trọng vì không thể đi lại đường cũ.
- Cấu trúc: một ngã rẽ với hai Target; chỉ một thứ tự cho phép đi tiếp đến Finish.
- Lựa chọn sai: Target gần hơn trông hấp dẫn nhưng làm người chơi tự khóa đường.
- Độ khó mục tiêu: 2/5.
- Nhánh sai mục tiêu: 2-3 bước.

### Level 4 - Follow the Pipe

- Bài học: đọc đầu vào và đầu ra của Pipe thẳng.
- Cấu trúc: một Pipe thẳng nằm trên đường bắt buộc, chưa có Rotate Button.
- Lựa chọn sai: thử đi xuyên Pipe từ cạnh không kết nối.
- Độ khó mục tiêu: 1/5, level nghỉ.

### Level 5 - Around the Corner

- Bài học: đọc Pipe góc.
- Cấu trúc: dùng hai loại góc khác nhau trong một đường ngắn.
- Lựa chọn sai: chọn một Pipe có hình gần đúng nhưng đầu ra sai hướng.
- Độ khó mục tiêu: 2/5.
- Điểm lựa chọn mục tiêu: 1-2.

### Level 6 - No Way Back

- Bài học: kiểm tra lối ra trước khi đi vào một khu vực.
- Cấu trúc: Target nằm trong một nhánh có Pipe; vào sai phía sẽ không thể hoàn thành tuyến còn lại.
- Chiến thuật chính: Route Commitment.
- Độ khó mục tiêu: 2/5.
- Nhánh sai mục tiêu: 3-4 bước.

### Level 7 - The Current Changes

- Bài học: Rotate Button làm tất cả Pipe xoay.
- Cấu trúc: một Button và một Pipe; Button nằm trên đường bắt buộc.
- Lựa chọn sai: không có lựa chọn gây thất bại dài.
- Độ khó mục tiêu: 1/5, level giới thiệu.

### Level 8 - Before the Turn

- Bài học: phải lấy Target trước khi kích hoạt Rotate Button.
- Cấu trúc lời giải: `S -> T1 -> O -> Pipe -> T2 -> F`.
- Lựa chọn sai: Button gần Start hơn T1 nên người chơi muốn bước vào trước.
- Chiến thuật chính: Rotation Timing.
- Độ khó mục tiêu: 2/5.
- Số lời giải mong muốn: 1.

### Level 9 - Open One, Close One

- Bài học: một lần xoay đồng thời mở một Pipe và đóng một Pipe khác.
- Cấu trúc: Target đầu tiên phải được lấy ở trạng thái ban đầu; Target thứ hai chỉ tới được sau khi xoay.
- Lựa chọn sai: xoay trước vì nhìn thấy đường tới Target thứ hai.
- Chiến thuật chính: Global Consequence.
- Độ khó mục tiêu: 3/5.
- Nhánh sai mục tiêu: 4-5 bước.

### Level 10 - Second Tide

- Bài học: dự đoán hai pha xoay liên tiếp.
- Cấu trúc: dùng hai Rotate Button khác nhau. Button thứ nhất mở khu giữa; Button thứ hai mở đường tới Finish.
- Lựa chọn sai: đi qua Button thứ hai trước khi lấy Target ở pha thứ nhất.
- Chiến thuật chính: Rotation Phase.
- Độ khó mục tiêu: 3/5.
- Trạng thái xoay cần theo dõi: 3 trạng thái, gồm ban đầu.

### Level 11 - The Tempting Child

- Bài học: Target gần nhất không nhất thiết nên được cứu trước.
- Cấu trúc: một Target gần Start nằm sau tuyến đường mà người chơi cần dùng về sau.
- Lựa chọn sai: lấy Target gần trước, sau đó đường đã đánh dấu chặn tuyến chính.
- Chiến thuật chính: False Opportunity + Route Commitment.
- Độ khó mục tiêu: 4/5.
- Nhánh sai mục tiêu: 5-7 bước.

### Level 12 - Mother's Plan

- Bài học: tổng hợp toàn bộ chương đầu.
- Cấu trúc: ba Target, hai Rotate Button khác nhau, Pipe thẳng và Pipe góc.
- Yêu cầu: lập thứ tự Target, dự đoán hai pha xoay và tránh tự cắt đường.
- Lựa chọn sai: có hai tuyến trông hợp lệ nhưng mỗi tuyến vi phạm một điều kiện khác nhau.
- Độ khó mục tiêu: 4/5.
- Số lời giải mong muốn: 1.
- Không giới thiệu cơ chế mới trong level này.

## 7. Mẫu thẻ thiết kế level

Sao chép mẫu này trước khi dựng một level mới:

```text
Tên level:
Vị trí trong campaign:
Độ khó mục tiêu: /5

Ý tưởng chính:
Điều người chơi đã biết:
Điều level muốn dạy hoặc kiểm tra:

Trình tự lời giải dự kiến:
S -> ... -> F

Lựa chọn hấp dẫn nhưng sai:
Vì sao người chơi muốn chọn nó:
Khi nào người chơi nhận ra sai:
Điều họ học được sau khi sai:

Số bước dự kiến:
Số điểm lựa chọn:
Độ sâu nhánh sai:
Số lần kích hoạt Rotate Button:
Số lời giải mong muốn:
```

## 8. Quy trình sản xuất một level

1. Viết thẻ thiết kế trước khi mở Map Builder.
2. Vẽ riêng đường giải đúng từ Start qua tất cả Target tới Finish.
3. Thêm Pipe và Rotate Button để tạo điều kiện logic cho đường đúng.
4. Thêm tối đa một nhánh sai trong level dạy cơ chế, tối đa ba nhánh sai trong level kiểm tra.
5. Tự chơi theo đường đúng và theo từng giả thuyết sai.
6. Dùng Solver kiểm tra khả năng giải, số lời giải và các chỉ số cấu trúc.
7. Cho ít nhất ba người chưa biết lời giải chơi thử.
8. Sửa level dựa trên nơi họ thực sự mắc kẹt, không chỉ dựa trên thời gian hoàn thành.

## 9. Dữ liệu playtest cần ghi

| Dữ liệu | Mục đích |
|---|---|
| Thời gian hoàn thành | So sánh tương đối giữa các level |
| Số lần restart | Đo mức độ thất bại |
| Số lần đi lùi | Đo lượng thử sai |
| Ô hoặc khu vực mắc kẹt | Tìm điểm khó thực tế |
| Lựa chọn sai đầu tiên | Kiểm tra nhánh đánh lạc hướng |
| Cách người chơi giải thích lời giải | Kiểm tra họ có hiểu chiến thuật không |

Không thay đổi độ khó chỉ dựa trên một người chơi. Dùng trung vị của ít nhất ba người và quan sát xem họ có cùng mắc một lỗi hay không.

## 10. Vai trò của Level Solver

Solver không thay designer quyết định level có vui hay không. Solver có nhiệm vụ kiểm tra các tuyên bố thiết kế:

- Level có giải được không?
- Có đúng một lời giải chính không?
- Lời giải ngắn nhất dài bao nhiêu?
- Có bao nhiêu điểm lựa chọn và ngõ cụt?
- Nhánh sai sâu nhất dài bao nhiêu bước?
- Lời giải cần đi qua bao nhiêu Rotate Button?

Designer tạo ra ý nghĩa của câu đố. Solver kiểm tra cấu trúc của câu đố. Playtest xác nhận trải nghiệm của người chơi.

