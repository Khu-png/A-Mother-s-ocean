# Unity Project Instructions

The user has requested that future coding and optimization follow the two HB
rule documents. Before writing or reviewing Unity code, read
`Docs/HBCodingAndOptimizationRules.md` and apply its project conventions and
context-dependent optimization guidance. The user confirmed that both PDFs are
the primary coding and optimization standard. Review the complete standard,
not just line counts or prohibited lookup APIs. Assistant suggestions are not
additional user rules.

Additional user rules from 2026-10-11 supplement the PDFs and tighten only the
specific topics below; they do not replace the rest of the PDF standard:
- Keep every project-owned C# file strictly below 250 physical lines (including
  comments and blank lines). Keep classes focused; do not hide complexity in
  arbitrary partial-file splits or compressed one-line methods.
- Keep methods short and simple, with one clear responsibility; split longer
  workflows at meaningful steps. The PDF recommends methods under 100 lines;
  there is no additional user-specified 30-line limit.
- Do not retrieve objects/components using Find-style APIs, GetComponent,
  TryGetComponent, GetChild, childCount, parent traversal, or parent/child component
  searches. Use typed Inspector references or explicit references passed by owners.
- Do not retrieve renderer materials at runtime, including material/materials,
  sharedMaterial/sharedMaterials getters, or GetMaterials/GetSharedMaterials.
  Do not use Shader.Find to discover rendering dependencies; wire assets explicitly.
- Assigning an explicit transform parent or supplied material is different from
  retrieving it. Ordinary gameObject access and typed registry lookups are not
  scene searches. Inspect the operation rather than banning words in identifiers.

Use these rules for new code and code touched by the requested task. Preserve
Unity serialization, Inspector references, prefab links, and gameplay behavior.
Avoid unrelated renaming or project-wide refactoring merely to enforce style.
Use the APIs supported by this project's Unity version. Validate performance
changes with profiling when possible; report unavailable verification honestly.
