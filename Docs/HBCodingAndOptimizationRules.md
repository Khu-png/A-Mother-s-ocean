# HB Coding And Optimization Rules

## Adoption And Sources

The user requested these conventions on 2026-10-11 for future Unity work.
Read all four pages of the coding PDF and all five pages of the optimization
PDF, including their illustrated examples. Source files:

- `C:/Users/Admin/Downloads/[HB Optimize Rule] - Code rule.pdf`
- `C:/Users/Admin/Downloads/[HB Optimize Rule] - Optimize Code Rule.pdf`

Project version at adoption: Unity `6000.3.12f1`.
The user confirmed that the two PDFs are the primary standard. Apply their full
coding and optimization guidance when writing or reviewing code. Additional user
rules supplement that standard and tighten only the topics explicitly specified.
Assistant recommendations must remain distinct from user requirements.
The sections below consolidate the sources rather than reproduce them verbatim.
The final section records implementation judgment where source advice is
absolute, platform-dependent, or uses older API names. Check current official
Unity documentation and profile relevant behavior when applying those items.

## Additional User Rules

The user added rules on 2026-10-11. These supplement the primary PDF standard;
where a specific added requirement is stricter, use that requirement:

- Every project-owned C# file must contain fewer than 250 physical lines,
  including whitespace and comments (maximum 249).
- Keep methods short and straightforward, with one responsibility. The PDF
  recommends methods under 100 lines; the user added no numeric method limit.
  Split workflows into meaningful steps rather than compressing code to meet
  line counts. Do not treat the assistant's earlier 30-line suggestion as a rule.
- Do not use Find-style object discovery, GetComponent/TryGetComponent,
  GetComponentInChildren/GetComponentInParent, GetChild, childCount, or parent
  traversal to retrieve dependencies. Bind typed Inspector references or pass
  explicit references and keep track of runtime objects when creating them.
- Do not read renderer material/materials/sharedMaterial/sharedMaterials or use
  GetMaterials/GetSharedMaterials to retrieve materials at runtime. Supply the
  required material asset explicitly. Avoid Shader.Find for rendering dependencies.
- Assigning an explicit parent/material is not dependency retrieval. Accessing
  gameObject, using a typed registry, or Editor SerializedObject.FindProperty
  does not perform a scene object search.
- Never add defensive null checks, silent early returns, fallback object creation,
  or similar guards as a patch for missing references, invalid ownership, broken
  initialization order, or another structural defect. Correct the source of the
  invalid state and make required dependencies explicit. Retain null handling only
  when null is an intentional, documented feature state or part of an external API
  contract.
- Keep class responsibilities small even when using partial files. Do not use
  partial declarations solely to conceal a large or complicated class.
- Review project-owned code under Assets/_Game; preserve third-party packages.

## Naming And Organization

- Use descriptive English names. Prefer nouns for variables and verbs for methods.
- Use PascalCase for classes, interfaces, enums, events, methods, namespaces,
  properties, and public fields. Prefix interfaces with `I` and abstract classes
  with `Abstract`.
- Use camelCase for private/protected fields and parameters. Use uppercase names
  for `const` and `readonly` fields, with underscores for word separation.
- Use English PascalCase names for project-owned folders and GameObjects.
  Keep gameplay assets under `Assets/_Game`; preserve third-party organization.
- Use named constants or configuration fields instead of unexplained numbers
  and repeated string literals. Centralize shared constants in `Constant.cs`
  when useful; keep feature-specific values close to their owner.
- Prefer enums for finite states and explicit types for readability.
- Give each class and method one clear responsibility; avoid duplicated code.
- Keep formatting consistent. Aim for at most three nesting levels and short,
  focused methods. Follow the stricter user limit of fewer than 250 lines per file.
- Keep comments concise. Use `TODO`, `FIXME`, `UNDONE`, and `TEST` for actionable
  notes, and remove temporary test code when its purpose is complete.

## Unity Code

- Place Unity lifecycle/message methods after fields and before other methods.
  Follow the source convention of omitting access modifiers on these methods.
  Remove unused empty callbacks.
- References to Update restrictions also cover FixedUpdate and LateUpdate.
- Cache repeatedly accessed transforms and components. Prefer serialized typed
  component references over a GameObject used only to retrieve a component.
- Never use object/component discovery or hierarchy traversal listed in the
  additional user rules, even during initialization. Wire references explicitly.
  Avoid Resources.Load, Instantiate, and Destroy in Update callbacks or repeated
  loops; use pooling for repeated spawning.
- Avoid SendMessage and string-based Invoke. Use typed calls and explicit timing.
- Use CompareTag for tag checks. Do not use GameObject names as gameplay identity.
- Avoid logic that depends on child order, child count, or implicit parent/child
  component searches; prefer explicit references.
- Remove unnecessary logs or guard diagnostic logging with `UNITY_EDITOR` or an
  appropriate development-only directive. Include print calls in this policy.
- Use tolerance-based comparisons for approximate float/vector behavior. Prefer
  squared distance when checking a radius without needing an actual distance.
- Remove list elements safely during iteration, such as by iterating backward.
- Prefer explicit gameplay data as the source of truth. For example, attack range
  should drive collider configuration rather than be inferred from its radius.
- Use pooling for frequently recycled gameplay objects and a coherent UI manager
  for shared UI flow. Reserve singletons for justified management responsibilities.
- Follow a moving camera in LateUpdate when appropriate for the movement pipeline.
- Avoid negative scale where an alternative fits the asset and animation setup.
- Prefer triggers for detection-only interactions. Avoid Stay callbacks when an
  enter/exit-based model can satisfy the requested behavior.
- Prefer simple, suitable collider shapes to unnecessary MeshColliders.

## Inspector, Assets, And Rendering

- Mark genuinely stationary objects with suitable Static flags.
- Inspect draw calls and rendering behavior with the Frame Debugger; evaluate GPU
  instancing for repeated compatible meshes/materials.
- Evaluate a representative medium-quality configuration for ordinary devices.
- Disable Raycast Target on non-interactive UI graphics without breaking input
  blocking or interaction. Check UI layout at 1080x1920 and 720x1600, plus the
  target project's orientations, aspect ratios, and safe areas.
- Use texture atlases where they improve compatible rendering batches.
- Choose texture dimensions and compression appropriate to the target platform;
  the sources recommend dimensions divisible by four and DXT1/DXT5 according to
  alpha needs. Treat these as target-specific guidance, not universal settings.
- Consider RenderTextures when a feature actually needs a rendered 3D view in UI.
- Avoid accidental material duplication and unnecessary runtime material changes.
- When assigning several Inspector references, use Inspector locking as needed.

## Review And Verification

- Review gameplay behavior, folder organization, scripts, and visible UI results.
- Report concrete bugs, rule violations, and performance evidence with locations.
  Screenshots can support findings when helpful.
- Finish reviews with a concise overall assessment and practical next steps.
- The PDF's shared spreadsheet and per-student tabs describe its training review
  workflow. They do not authorize opening, editing, or publishing to that sheet.
- Verify compilation and relevant gameplay when tools permit. Distinguish checks
  actually performed from checks that still require running Unity or a device.

## Context-Dependent Guidance

These are application decisions, distinct from the source documents' wording:

- Do not assume Vector3.Distance avoids a square root or is inherently cheaper
  than magnitude. Check the API/implementation; use squared thresholds when apt.
- Follow the explicit-type style, but do not attribute boxing to `var` by itself.
  Evaluate actual conversions and allocations.
- Prefer `for` for indexed hot paths when appropriate; do not assume it always
  outperforms `foreach`. Measure the concrete collection and runtime.
- A single return is a readability preference. Early guards can be appropriate
  when they reduce nesting and clarify validation.
- Do not replace collisions with triggers if physical collision response is
  required. Collider selection depends on geometry and physics requirements,
  rather than an unconditional Sphere > Capsule > Box ranking.
- Use platform-supported compression rather than forcing DXT on every target.
  Static flags, quality settings, instancing, atlases, and RenderTextures need
  validation against the render pipeline and feature requirements.
- Check the current API spelling/version rather than copying the sources' API
  reference examples verbatim. Use cached Animator parameter hashes when the API
  requires parameter identifiers; an enum alone does not replace that binding.
- MaterialPropertyBlock, shared materials, or controlled instances may be suitable
  for runtime visuals; choose based on pipeline compatibility and measured cost.
- Initialization or pool prewarming may legitimately create objects in a bounded
  loop. Document that purpose and keep expensive creation outside repeated paths.
- Preserve serialized field names and Unity asset references during style changes.
  Use serialization migration when a rename is necessary for the requested task.
- Apply these conventions within the requested scope, and use profiling evidence
  before broad performance changes. Do not refactor the entire project unasked.
