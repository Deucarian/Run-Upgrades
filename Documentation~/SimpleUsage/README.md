# Simple usage

Copy the reference example into your project, add SimpleUsageExample and assign its scoped host references. Its serialized definition fields use the same typed keys as code.

Configure RunUpgradeHost once with a RunUpgradeProfile containing the existing catalog, run state and seed. Display Offer().Choices and pass the selected handle to Choose. A new draft expires the previous offer; a successful choice consumes it. Pass the resulting authoritative upgrade state to the existing effect application owner. RunUpgradeDefinitionAssets generate `Deucarian.Generated.ProjectUpgrades` keys.

Definitions are authored once in SampleDefinitions.cs where applicable; the caller never invents an ID. Replace the sample set with your project's central definitions. A selected key proves its identity and payload type; startup still needs to bind that definition in the correct scope. Missing configuration reports how to fix it. Dynamic targets and choices are issued by their owner instead of selected from a definition dropdown.
```csharp
using UnityEngine;
using Deucarian.RunUpgrades.Authoring;
namespace Deucarian.RunUpgrades.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        [SerializeField] private RunUpgradeHost upgrades;
        [SerializeField] private UpgradeKey upgrade = Upgrades.Damage;
        public int Rank => upgrades.GetRank(upgrade);
        public UpgradeDraft Offer() => upgrades.Draft();
        public RunUpgradeSelectionResult Choose(UpgradeChoiceHandle choice) => upgrades.Select(choice);
    }
}
```
