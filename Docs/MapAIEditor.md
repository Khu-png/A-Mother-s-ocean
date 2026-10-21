# Map AI Editor

## Open The Tool

Open `Tools > A Mother's Ocean > Map AI Editor` in Unity 6.

The window has three areas:

- Left: grid size, difficulty, puzzle focus, sample count, seed, brushes, and cell locks.
- Center: editable map preview. Use the mouse wheel to zoom and middle-drag to pan.
- Right: validation, routes, dead-end analysis, overlays, and AI provider status.

## Workflow

1. Select a `LevelData` asset and choose `Open`, or start from the current grid.
2. Choose the requested dimensions. `Generate Samples` resizes the document automatically;
   `Resize Grid` remains available when you want to resize before manual editing.
3. Choose difficulty and a puzzle focus (`TargetOrder`, `RotationTiming`, or
   `GlobalRotation`). Difficulty automatically configures target and tile counts.
   Set `Unique samples` and select `Generate Samples`. Seed is under `Reproducibility`.
4. Use `Previous` / `Next` to compare each unique valid map. Generation does not modify
   the document yet; each sample uses a deterministic seed derived from the selected seed.
5. Select `Apply Preview` or `Discard Preview`.
6. Use `Save` for the selected asset or `Save As` to create another `LevelData`.
7. Expand `Save As Game Level`, assign the scene's `Level Manager`, enter the level number,
   then choose `Create Level Data`. Save the scene or prefab after the level is registered.

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
- Line of sight is not verified because the current map data has no obstacle geometry beyond
  grid connectivity.

When a budget is reached, the tool labels route results as partial and does not claim that all
paths were found. Suggested time is editable design guidance derived from the completion route
and difficulty preset; it is not written into gameplay data automatically.

## Generation

Generator version 3 creates and evaluates multiple candidates, selecting the candidate closest
to the chosen difficulty's decision-point target. Puzzle focus guides target placement and the
rotation mechanics. `RotationTiming` places a rotator on the solution route and requires pipe
orientation to change for the route to remain traversable. `GlobalRotation` also places an
off-route pipe at a junction so the same rotation changes another route. The solver tracks
rotation phase, collected targets, and non-repeating paths. The same version, seed, settings, and
locked cells reproduce the same output. An impossible locked layout is rejected with an explicit
reason instead of accepting an invalid map.

The supported range currently follows the project's existing `MapBuilderLimits`: 1-10 columns
and 1-10 rows. Very small dimensions can be rejected because they cannot contain two reachable
endpoints. Automated checks cover 5x5, 8x8, 10x10, and 7x4. A local managed benchmark completed
20 generation-and-analysis runs at 10x10 in about 4 ms on the development machine; validate
editor and device performance with Unity Profiler before increasing the project limit.

## AI Status

No LLM editor provider is configured in the project. `MapAiToolApi` exposes versioned map reads,
generation, analysis, proposals, and version matching. An integration implements
`IMapAiEditorProvider` and registers it through `MapAiProviderRegistry.Provider`.

AI proposals are previews and are rejected when their base map version differs from the current
document. The installed Unity AI Inference package is a model runtime and is not treated as a
configured map-design assistant.

## Verification

Run `Tools > A Mother's Ocean > Run Map AI Editor Self Tests` to check reachable and blocked
maps, directional movement, bounded search, dead ends, target completion, non-repeating routes,
deterministic seeds, locked cells, configured tile counts, ratios, and supported dimensions.
