# AI Map Designer

## Open The Tool

Mở `Tools > A Mother's Ocean > AI Map Designer` trong Unity 6.

The window has three areas:

- Left: grid size, difficulty, puzzle focus, sample count, seed, brushes, and cell locks.
- Center: editable map preview. Use the mouse wheel to zoom and middle-drag to pan.
- Right: validation, routes, dead-end analysis, overlays, and AI provider status.

## Workflow

1. Chọn tài nguyên `LevelData` rồi nhấn `Mở`, hoặc bắt đầu từ lưới hiện tại.
2. Chọn kích thước mong muốn. `Tạo bản đồ mẫu` tự đổi kích thước tài liệu;
   dùng `Đổi kích thước lưới` nếu muốn chỉnh kích thước trước khi vẽ thủ công.
3. Choose difficulty and a puzzle focus (`TargetOrder`, `RotationTiming`, or
   `GlobalRotation`). Difficulty automatically configures target and tile counts.
   Đặt `Số bản đồ khác nhau` rồi chọn `Tạo bản đồ mẫu`. Hạt giống nằm trong `Tái tạo kết quả`.
4. Dùng `Trước` / `Sau` để so sánh từng bản đồ hợp lệ. Việc tạo mẫu không chỉnh sửa
   the document yet; each sample uses a deterministic seed derived from the selected seed.
5. Chọn `Áp dụng bản xem trước` hoặc `Bỏ bản xem trước`.
6. Nhấn `Lưu` để lưu tài nguyên đang chọn hoặc `Lưu thành` để tạo `LevelData` khác.
7. Mở rộng `Lưu thành màn chơi`, gán `Quản lý màn chơi` trong scene, nhập số màn,
   rồi chọn `Tạo dữ liệu màn chơi`. Lưu scene hoặc prefab sau khi đăng ký màn chơi.

Manual paint, resize, lock, and preview application support Unity Undo/Redo. Locked cells
are never changed by generation. A locked layout that prevents valid endpoint placement or
disconnects the map is rejected with a reason.
Click the selected tile button again to deselect it; while no tile is selected, clicking the
map does not paint.

## Analysis

- BFS validates reachability and calculates the shortest S-to-F distance for equal-cost moves.
- Reverse-edge BFS calculates distance to F for directional tiles.
- Backtracking enumerates non-repeating S-to-F routes within the configured path, length,
  and expansion budgets. A completion route must contain every `T` before ending at `F`.
- Map highlights use one view mode at a time. `Normal` restores the original tile colors;
  the legend identifies completion paths, dead ends, and misleading candidates.
- Dead-end analysis reports the leaf, junction, and required return depth separately from
  backtracking operations.
- A misleading candidate approaches F geometrically while its real path distance requires
  returning toward a junction. It remains a design candidate, not a guarantee of player error.
- Near-win dead ends are non-repeating routes that collect at least one target, leave exactly
  one uncollected, and run out of legal moves. The overlay highlights the longest detected route.
  This is a structural signal, not proof that players will mistake it for a winning route.
- Line of sight is not verified because the current map data has no obstacle geometry beyond
  grid connectivity.

When a budget is reached, the tool labels route results as partial and does not claim that all
paths were found. Suggested time is editable design guidance derived from the completion route,
detected near-win dead ends, and difficulty preset; it is not written into gameplay data
automatically.

## Generation

Generator version 4 creates and evaluates multiple candidates, selecting the candidate closest
to the chosen difficulty's complexity target. Scoring favors decision points and routes that get
stuck after collecting all but one target; generic dead-end count is not used as a difficulty
bonus. Puzzle focus guides target placement and the rotation mechanics. `RotationTiming` places
a rotator on the solution route and requires pipe
orientation to change for the route to remain traversable. `GlobalRotation` also places an
off-route pipe at a junction so the same rotation changes another route. The solver tracks
rotation phase, collected targets, and non-repeating paths. The same version, seed, settings, and
locked cells reproduce the same output. An impossible locked layout is rejected with an explicit
reason instead of accepting an invalid map.

The supported range currently follows the project's existing `MapBuilderLimits`: 1-10 columns
and 1-10 rows. Very small dimensions can be rejected because they cannot contain two reachable
endpoints. Automated checks cover 5x5, 8x8, 10x10, and 7x4. A previous-version managed benchmark
completed 20 generation-and-analysis runs at 10x10 in about 4 ms on the development machine.
Version 4 continues bounded route search to find near-win failures, so remeasure with Unity
Profiler before increasing the project limit.

## AI Status

No LLM editor provider is configured in the project. `MapAiToolApi` exposes versioned map reads,
generation, analysis, proposals, and version matching. An integration implements
`IMapAiEditorProvider` and registers it through `MapAiProviderRegistry.Provider`.

AI proposals are previews and are rejected when their base map version differs from the current
document. The installed Unity AI Inference package is a model runtime and is not treated as a
configured map-design assistant.

## Verification

Chạy `Tools > A Mother's Ocean > Chạy kiểm thử trình chỉnh sửa bản đồ` để kiểm tra bản đồ có đường đi và bị chặn
maps, directional movement, bounded search, dead ends, target completion, non-repeating routes,
deterministic seeds, locked cells, configured tile counts, ratios, and supported dimensions.
