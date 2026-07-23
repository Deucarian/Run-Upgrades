# Deucarian Run Upgrades Agent Notes

Package ID: `com.deucarian.run-upgrades`
Repository: `Deucarian/Run-Upgrades`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/main/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- Roguelite run upgrade definitions, drafts, choices, prerequisites, exclusions, upgrade snapshots, Unity run-upgrade authoring assets, and Game Content Authoring providers for run upgrade definitions.

Registered capabilities:
- None.

This package must not own:

- Attack orchestration, weapon fire cadence, projectile lifecycle, combat damage resolution, persistent progression, currencies, rewards, saving/loading, encounter scheduling, world spawning/navigation, UI, monetization, or product-specific upgrade economies.

## Dependencies

Allowed dependency shape:

- Runtime upgrade logic may depend on Gameplay Foundation.
- Authoring/runtime descriptors may depend on Attacks and Weapon Systems for effect targets.
- Editor surfaces may depend on Editor and Game Content Authoring.

Required dependencies and why:

- `com.deucarian.gameplay-foundation`: shared gameplay IDs and deterministic primitives.
- `com.deucarian.attacks`: attack authoring references targeted by run upgrade effects.
- `com.deucarian.weapon-systems`: weapon authoring references targeted by run upgrade effects.
- `com.deucarian.editor`: shared editor shell/resources for authoring surfaces.
- `com.deucarian.game-content-authoring`: provider registration and validation UI for run upgrade content.

Optional/version-defined dependencies:

- None.

Architecture exceptions:

- None.

## Policies

- Keep this package focused on run-scoped upgrade choice logic and authoring.
- Do not add hard dependencies on Progression, Persistence, Auto Defense, Defense Games, Projectiles, Combat, UI, Monetization, or template packages without a governance update.
- Long-term account progression, currencies, and reward payout belong in Progression or the owning game/template.
- Logging: Do not introduce direct Unity Debug calls.
- Unity object lifetime: Use Common only if production code directly owns transient Unity object cleanup.
- Testing: Test fixture teardown may use Unity `DestroyImmediate` directly.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, Package Installer fallback, and Bootstrap fallback together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.
