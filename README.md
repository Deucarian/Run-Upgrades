# Deucarian Run Upgrades

## Typed definition workflow

Definitions describe reusable upgrades. Offered choices are runtime handles: selecting an old or already consumed offer is rejected by the same core API.

Start with the [Definition Workflow walkthrough](Documentation~/DefinitionWorkflow.md).
Import **Definition Workflow** in Package Manager for a configured sample scene
and short caller scripts. Definitions can be edited as assets or editable C# declarations; generated keys
work in code and Inspector dropdowns.


`com.deucarian.run-upgrades` owns deterministic roguelite run upgrade drafting and selected-upgrade state.

It does not apply effects. Games translate selected `RunUpgradeEffectDescriptor` values through their own adapters into stats, weapon configuration, Auto Defense modules, Tower Defense towers, or other package APIs.

Runtime dependency: `com.deucarian.gameplay-foundation`.

The package is suitable for Idle Auto Defense and classic Tower Defense because it drafts abstract choices and returns explicit effect descriptors; games decide how those effects mutate their own runtime state.

## Generated keys in code and the Inspector

Project run upgrade definitions generate named, typed C# keys automatically. A `.g.cs` file is generated C# that Unity compiles normally. The generator runs in the editor; the player uses the compiled key code.

1. Create or edit a `RunUpgradeDefinitionAsset` under your project's `Assets` folder using the existing authoring workflow. Keep its stable ID unique and give it a display name, for example `Damage`.
2. Let Unity finish importing and compiling. The editor produces `Assets/DeucarianGeneratedKeys/UpgradeKey/ProjectUpgrades.g.cs` and its generated assembly definition.
3. Configure the runtime owner once, then use the generated key in code or select the same definition from a serialized field dropdown.

Configure RunUpgradeHost with a RunUpgradeProfile containing the existing catalog, run state and seed. The catalog must include the upgrade. Draft choices still use owner-issued handles, and the existing effect owner applies the resulting state.

After creating the `Damage` definition, a caller can use:

```csharp
using Deucarian.RunUpgrades;
using Deucarian.RunUpgrades.Authoring;
using Deucarian.Generated;
using UnityEngine;

public sealed class GeneratedKeyExample : MonoBehaviour
{
    [SerializeField] private RunUpgradeHost upgrades;
    [SerializeField] private UpgradeKey definition = ProjectUpgrades.Damage;

    public int Rank => upgrades.GetRank(definition);
}
```

The `definition` field exposes existing `UpgradeKey` choices in the Inspector. A direct code call uses the same typed value:

```csharp
int rank = upgrades.GetRank(ProjectUpgrades.Damage);
```

The caller retains a typed identity, without a reference to the definition asset. Misspelled generated members and keys from another domain fail compilation. A valid key does not configure a scene or add the definition to its runtime catalog; follow [Simple usage](Documentation~/SimpleUsage.md) for scope setup.

**Updating definitions:** edit the source asset. Changing its display name changes the generated member after regeneration, so update old code references. Existing serialized selections retain their stable ID. Deleting a definition removes its member and marks serialized selections as missing. Duplicate IDs or generated names must be corrected at the source. Renaming only the asset file leaves its display name and ID unchanged.

**Assemblies and source control:** callers with their own asmdef reference `Deucarian.GeneratedKeys.UpgradeKey` in addition to the package assemblies they use; `Assembly-CSharp` sees it automatically. Commit source assets, generated `.g.cs`, generated `.asmdef` files and their `.meta` files together. Edit source definitions instead of generated files.

**If a key is missing or stale:** reimport a source definition and let Unity finish compilation. Check that the asset is under `Assets`, its name/ID are valid and automatic generation has not been disabled by a test harness. Inspector and build validation report missing selections and stale generated output. Custom bundle/content pipelines should invoke the shared validator for their additional content.

[Shared generation, serialization and build-validation guide](https://github.com/Deucarian/Editor/blob/develop/Documentation~/TypedKeys.md).

## Game Content Authoring

Run Upgrades contributes the `Upgrades` lens to `Tools/Deucarian/Game Content Authoring`. It inspects immutable Upgrade-capable records from the globally selected pack and provides semantic filters for Weapon Upgrade, Passive, Pickup / Magnet, Mutation, Evolution, and Meta Upgrade capabilities without duplicating the underlying record.

The common projection shows description, category, rarity, weight, rank limit, effect, amount, target, prerequisites, class gates, references, and comparison text. Template packages register `IGameContentRecordProjectionAdapter<UpgradeContentRecordProjection>` adapters and retain ownership of their schema and game-specific fields. External JSON sources are read-only, preserve canonical pack-scoped identity, and support reference and compatible-lens navigation.

Selecting `Project Content` preserves the existing standalone `RunUpgradeDefinitionAsset` creation and editing workflow under `Assets/GameContent`. Creation is unavailable for read-only packs, All Packs, and contexts without an explicit writable backend.

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

## Simple typed usage

See [Simple usage](Documentation~/SimpleUsage.md) for the short caller, Inspector selections and one-time scoped setup.
