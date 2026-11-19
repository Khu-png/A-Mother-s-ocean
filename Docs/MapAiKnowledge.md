# Kiến thức thiết kế bản đồ cho AI (A Mother's Ocean)

Tài liệu này gom những gì đã biết về cách chơi, cách đo độ khó và gu thiết kế của người
làm game, để một AI (LLM hoặc bộ sinh bản đồ) đọc và làm theo. Khi nối AI vào ô
"Tích hợp AI" của cửa sổ AI Map Designer, hãy đưa nguyên file này vào phần ngữ cảnh,
kèm vài dòng gần nhất của `MapFeedback/feedback.jsonl`.

Mọi con số trong file được đo trên 6 level làm tay hiện có, bằng chương trình giải dùng
đúng luật game. Chỗ nào là ước đoán thì được ghi rõ.

## 1. Định dạng bản đồ

Bản đồ là mảng chuỗi, mỗi chuỗi là một hàng, **hàng đầu tiên nằm trên cùng**. Lưới từ
1 đến 10 cột và 1 đến 10 hàng. Mỗi ký tự là một ô:

| Ký tự | Ô | Ghi chú |
|---|---|---|
| `.` | Sàn trống | Đi qua tự do |
| `S` | Điểm xuất phát | Đúng một ô |
| `F` | Đích | Đúng một ô, bị khóa đến khi nhặt đủ `T` |
| `T` | Cá con (target) | Bước lên là nhặt |
| `X` | Đá | Không đi qua được |
| `O` | Ô đổi chiều (xoáy nước) | Bước lên thì **mọi ống một chiều đổi trạng thái** |
| `\|` | Ống thẳng dọc | Cho đi lên và xuống |
| `-` | Ống thẳng ngang | Cho đi trái và phải |
| `A` | Ống góc trái và dưới | |
| `B` | Ống góc trái và trên | |
| `C` | Ống góc trên và phải | |
| `E` | Ống góc phải và dưới | |

Hướng "trên, dưới, trái, phải" là hướng trên màn hình.

## 2. Luật di chuyển (khớp với code game)

- Cá mẹ đi từng ô theo 4 hướng. Không đi vào `X` hay ra ngoài lưới.
- **Ống một chiều:** một bước từ ô A sang ô B chỉ hợp lệ khi ống ở A (nếu có) cho đi theo
  hướng đó **và** ống ở B (nếu có) cho đi vào từ hướng ngược lại.
- **Không đi vào ô đã có vệt đường đi.** Ngoại lệ: quay lại ô ngay trước đó. Quay lại là
  **hoàn tác**: ô vừa rời được trả lại (cá con nhặt ở đó hiện lại), và nếu ô vừa rời là
  `O` thì các ống trả về trạng thái trước.
- **Ô đổi chiều `O`:** mỗi lần bước vào (không phải quay lại) thì
  - ống thẳng đảo giữa `|` và `-`;
  - ống góc quay theo vòng `A` → `E` → `C` → `B` → `A`.
  Nghĩa là ống thẳng chỉ phụ thuộc số ô `O` đã giẫm là chẵn hay lẻ, ống góc phụ thuộc số
  đó chia 4 dư mấy. Trạng thái ống luôn do **lộ trình đã đi** quyết định.
- **Đích:** `F` mở khi đã nhặt đủ `T`. Bước vào `F` đã mở là thắng. Đích bị khóa thì không
  vào được.
- Mỗi level có giới hạn thời gian. Vật phẩm (đóng băng, búa) chỉ là van xả cho người chơi:
  **bản đồ phải giải được mà không cần chúng**.

## 3. Thế nào là level hay (theo người thiết kế)

Level 3 được chọn là hay nhất, vì nó **lừa người chơi và bắt họ phải đoán nên giẫm bao
nhiêu ô đổi chiều thì mới ra đường đúng**. Rút ra các nguyên tắc:

1. **Ô đổi chiều phải quyết định lời giải.** Nếu lời giải không dùng ô đổi chiều nào, đó
   chỉ là đồ trang trí, câu đố không tồn tại.
2. **Số ô đổi chiều cần giẫm là thứ người chơi phải đoán.** Có nhiều ô `O` hơn số cần
   giẫm, để giẫm thừa hoặc thiếu đều dẫn tới bế tắc.
3. **Đường sai phải trông hợp lý.** Bẫy tốt là đường bế tắc rất lâu mới lộ ra, sâu bằng
   hoặc hơn chính lời giải.
4. **Không đòi lời giải duy nhất.** Level 3 có 3 lời giải và vẫn rất hay; điều quan trọng
   là chúng đều bắt buộc đúng số ô đổi chiều.

## 4. Phân tích level 3 (mẫu tốt)

```
F|T|E.
.SAO-.
XOTOTX
.XOT-.
..XO..
```
S ở (cột 1, hàng 1), F ở (0, 0). 5 ô `O`, 4 `T`, 6 ống, 3 lời giải, ngắn nhất 12 bước.
**Nếu ống không bao giờ xoay thì không giải được.** Cả 3 lời giải đều giẫm đúng **3 trong 5**
ô `O`, và đều bắt đầu bằng cách bước xuống ô `O` ngay dưới S. Một lời giải (tọa độ cột,
hàng; `O` là ô đổi chiều, `T` là cá con):

`(1,2)O (2,2)T (3,2)O (3,3)T (3,4)O (4,4) (4,3) (4,2)T (4,1) (4,0) (3,0) (2,0)T (1,0) (0,0)`

Bẫy điển hình: đi ngang từ S vào ống góc `A` ở (2,1) rồi vào `O` ở (3,1) thì bị chặn, vì
`A` không cho đi sang phải khi chưa xoay. Đường này nhìn gần và hấp dẫn nhưng là ngõ cụt.

Ngõ cụt sâu nhất dài 13 bước (hơn lời giải 12 bước), 35 ngõ cụt cho 3 lời giải.

## 5. Thang điểm độ khó

Điểm là tổng 5 phần (mỗi phần có trần, tổng có thể vượt 100):

| Phần | Cách tính | Tối đa |
|---|---|---|
| Ngõ cụt mỗi lời giải | 1,6 điểm cho mỗi ngõ cụt trên một lời giải | 60 |
| Bẫy sâu | 10 điểm cho mỗi 1,0 độ sâu của ngõ cụt sâu nhất so với độ dài lời giải; chỉ tính 1/4 khi hơn 10 lời giải | 20 |
| Ít lời giải | 1 lời giải: 15, 2: 12, 3: 9, 4 đến 5: 6, 6 đến 10: 3, hơn 10: 0 | 15 |
| Ô đổi chiều bắt buộc | 5 điểm cho mỗi ô `O` mà lời giải ngắn nhất phải giẫm | 25 |
| Độ dài tuyến | 1/3 điểm cho mỗi bước của lời giải ngắn nhất | 15 |

**Mốc:** Dễ dưới 50, Khó từ 50 đến 74, Siêu khó từ 75.

Ý nghĩa từng mức:
- **Dễ:** ít ngõ cụt, ô đổi chiều không bắt buộc hoặc chỉ cần một ô, nhiều lời giải.
- **Khó:** như level 3: nhiều ngõ cụt, bắt buộc giẫm đúng vài ô đổi chiều, ít lời giải.
- **Siêu khó:** lời giải gần như duy nhất, bẫy sâu, phải tính trước số ô đổi chiều.

Vì sao chọn các mốc này:
- **50:** sáu level chia rõ hai nhóm: level 1, 2, 4, 5, 6 nằm ở 13 đến 36, level 3 ở 58. Mốc
  nào trong 40 đến 55 cũng đúng; 50 là số tròn dễ nhớ.
- **75:** cao hơn level 3 khoảng 17 điểm, nghĩa là cần lời giải duy nhất, khoảng 4 ô đổi chiều
  bắt buộc và nhiều ngõ cụt hơn, mà vẫn làm ra được ở lưới nhỏ. Mốc 100 đòi đạt trần mọi phần
  cùng lúc nên gần như không bản đồ nào chạm tới, khiến nhãn Siêu khó không bao giờ xuất hiện.
- Điểm có thể vượt 100 với bản đồ cực khó; chỉ nhãn Siêu khó bắt đầu từ 75.

Điểm của 6 level làm tay:

| Level | Kích thước | Ô đổi chiều | Lời giải | Ngắn nhất | Ngõ cụt | Điểm | Mức |
|---|---|---|---|---|---|---|---|
| 1 | 6×1 | 0 | 1 | 5 | 0 | 17 | Dễ |
| 2 | 5×7 | 2 | 1 | 22 | 0 | 32 | Dễ |
| **3** | **6×5** | **5** | **3** | **12** | **35** | **58** | **Khó** |
| 4 | 5×5 | 0 | 5754 | 8 | 18590 | 13 | Dễ |
| 5 | 6×6 | 3 | 3 | 12 | 14 | 36 | Dễ |
| 6 | 6×5 | 1 | 3 | 11 | 5 | 24 | Dễ |

Lưu ý: **chưa có level Khó nào ngoài level 3 và chưa có level Siêu khó nào làm mẫu.** Mốc 75
là ngoại suy từ level 3. Điểm của bản đồ mở rộng (nhiều lời giải) chỉ là ước lượng vì
công cụ giới hạn số lần tìm đường.

## 6. Luật bố cục đang dùng

**Loại bản đồ ngay:**
- Có cả một hàng hoặc cột (từ 3 ô) toàn đá.
- Bản đồ có ô đổi chiều nhưng vẫn giải được khi ống không bao giờ xoay.

**Trừ điểm khi chọn bản đồ:**
- Đá chiếm từ 80% một cạnh: -6 mỗi cạnh.
- Hai ống góc giống nhau sát nhau: -3 mỗi cặp.
- Hai cá con sát nhau: -6 mỗi cặp.
- Từ 3 cá con trở lên trên cùng một hàng hoặc cột: -4 mỗi cá con thừa.
- Tuyến ngắn nhất quá ngắn so với số ô (tối thiểu 20%, 30%, 40% số ô theo Dễ, Khó, Siêu
  khó): -2 điểm mỗi bước thiếu.

**Cố ý KHÔNG cấm** (vì level 3, level hay nhất, vi phạm chúng):
- Hai ô đổi chiều nằm sát nhau.
- Các cá con nằm gần nhau miễn là không sát nhau và không thẳng hàng.

## 7. Dạng bản đồ xấu đã gặp

Bản đồ AI sinh ra bị chê:

```
XST.TX      (6 cột, 5 hàng, mức Siêu khó, 4 cá con)
.X.XTX
.OO-.X
-XXXTX
.BBF.X
```
Lý do: cả cột phải toàn đá, đá rải như nhiễu, hai xoáy nước và hai ống góc sát nhau,
ba cá con cùng một cột, và quan trọng nhất **lời giải không dùng ô đổi chiều nào** nên
không có câu đố (8 bước, 0 ô đổi chiều bắt buộc).

## 8. Cách AI nên trả lời khi được nhờ đề xuất bản đồ

Trả về:

```json
{ "rows": ["...", "..."], "explanation": "vì sao bản đồ này đạt mục tiêu" }
```

Bản đồ phải thỏa:
1. Đúng một `S`, đúng một `F`, ít nhất một `T`.
2. Mọi hàng cùng độ dài, tối đa 10×10.
3. Giải được: công cụ sẽ chạy chương trình giải để kiểm tra, AI không được tự khẳng định.
4. Nếu yêu cầu có ô đổi chiều: không giải được khi ống đứng yên; số ô `O` nhiều hơn số cần
   giẫm; có ít nhất một đường nhìn đúng nhưng sai khi ống xoay.
5. Đạt dải điểm của mức yêu cầu (mục 5) và không vi phạm luật bố cục (mục 6).

Sau khi nhận đề xuất, công cụ đo lại điểm. Nếu lệch mục tiêu, gửi lại số đo (ngõ cụt mỗi
lời giải, độ sâu bẫy, số ô `O` cần giẫm) để AI sửa.

## 9. Dữ liệu phản hồi

Mỗi lần người thiết kế bấm "Map tốt" hoặc "Map xấu", một dòng JSON được ghi vào
`MapFeedback/feedback.jsonl`:

| Trường | Ý nghĩa |
|---|---|
| `rating` | 1 là tốt, -1 là xấu |
| `reason` | lý do viết tự do của người thiết kế |
| `rows` | bản đồ được chấm |
| `difficulty`, `targetCount`, `has*`, `*Ratio` | thông số sinh |
| `canComplete`, `completionSteps`, `decisionPoints`, `deadEnds`, `completePaths` | số đo phân tích |
| `layout` | số đo bố cục (đá ở rìa, hàng đá đầy, cặp sát nhau...) |
| `rotation` | `rotatorCount`, `rotatorsOnRoute` nhỏ nhất và lớn nhất, `requiresRotation`, `decoyRoutes` |

Cách học: đọc các dòng `rating = -1` có `reason`, tìm điểm chung ở `layout` và `rotation`,
rồi biến thành luật hoặc đổi trọng số. Cần ít nhất khoảng 30 đến 50 phản hồi nhất quán
trước khi đổi trọng số tự động.

## 10. Còn thiếu và cần làm tiếp

- Một level Khó và một level Siêu khó **làm tay** thật để hiệu chỉnh mốc điểm.
- Thang điểm chưa đo thời gian và tỉ lệ thắng của người chơi thật; khi có dữ liệu chơi
  thật thì dùng chúng để chỉnh lại.
- Bộ sinh hiện chỉ bắt buộc **một** ô đổi chiều; level 3 cần 3 trong 5. Cần sinh nhiều ô
  đổi chiều đặt gần nhau ở giữa với ống ở nhiều chỗ.
- Hạng mục chưa quyết: có đưa số cá con, kích thước lưới và thời gian vào điểm độ khó hay không.

## Ice baby sharks (digits 1-9)

- A digit `1`-`9` in a map row is a baby shark frozen in ice. It counts as a target like `T`.
- The ice melts once at least that many *other* babies have been rescued. Until then the
  cell cannot be entered (the solver treats it as a wall). Undoing the route past the
  rescued babies freezes it again.
- `MapAiMapData.IsFrozenAt` carries the rule; `MapAiPathSearch` applies it in the move
  filter and in the decision-point count. The generator does not place ice yet - paint it
  by hand with the "Cá con băng" brush (counter slider 1-9) and press analyze.
- Level 1 style check: `S.T.1F` is solvable (one solution); `S1.T.F` is not.
