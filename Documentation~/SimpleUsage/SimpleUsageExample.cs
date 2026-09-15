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
