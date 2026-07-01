# Deucarian Run Upgrades

`com.deucarian.run-upgrades` owns deterministic roguelite run upgrade drafting and selected-upgrade state.

It does not apply effects. Games translate selected `RunUpgradeEffectDescriptor` values through their own adapters into stats, weapon configuration, Auto Defense modules, Tower Defense towers, or other package APIs.

Runtime dependency: `com.deucarian.gameplay-foundation`.

The package is suitable for Idle Auto Defense and classic Tower Defense because it drafts abstract choices and returns explicit effect descriptors; games decide how those effects mutate their own runtime state.

## Install

Stable:

```json
"com.deucarian.run-upgrades": "https://github.com/Deucarian/Run-Upgrades.git#main"
```

Development:

```json
"com.deucarian.run-upgrades": "https://github.com/Deucarian/Run-Upgrades.git#develop"
```

Use `#main` for stable package consumption and `#develop` when testing active package work.

## When To Use This

Use this package when you need Pure C# roguelite run upgrade core plus Unity authoring assets/providers for definitions, drafts, choices, prerequisites, exclusions, and snapshots.

Do not use this package to take ownership of capabilities outside its `AGENTS.md` boundary. Reusable behavior should stay with the package that owns that capability in the Package Registry governance docs.

## Quick Start

1. Install the package through Deucarian Package Installer or Unity Package Manager using the URL above.
2. Let Unity finish resolving packages and compiling assemblies.
3. Start from the package README sections above and the public runtime/editor APIs in this repository.

## Integrations

Direct Deucarian package dependencies:

- `com.deucarian.gameplay-foundation`
- `com.deucarian.attacks`
- `com.deucarian.weapon-systems`
- `com.deucarian.editor`
- `com.deucarian.game-content-authoring`

Install optional companion packages only when their owned capability is needed by production code, samples, or tests.

## Validation

Run the shared package validator from this repository root:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Documentation-only updates should still pass:

```powershell
git diff --check
```

## Troubleshooting

- Package does not resolve: confirm the stable or development Git URL matches the Package Registry entry and that required Deucarian dependencies are installed.
- Unity compile errors after install: let Package Manager finish resolving dependencies, then check asmdef references against `package.json` dependencies.
- Behavior appears to belong in another package: consult `AGENTS.md` and the Package Registry governance docs before moving or duplicating code.

## License

MIT. See `LICENSE.md`.
