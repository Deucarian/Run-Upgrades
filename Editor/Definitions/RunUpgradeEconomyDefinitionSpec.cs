using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.RunUpgrades.Authoring;
using Deucarian.Attacks.Authoring;
using Deucarian.WeaponSystems.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.RunUpgrades.Editor.Definitions
{
    [Serializable]
    public sealed class RunUpgradeEconomyDefinitionSpec
    {
        [DefinitionField("_rarity")] public RunUpgradeRarity Rarity = RunUpgradeRarity.Common;
        [DefinitionField("_weight")] public int Weight = 5;
        [DefinitionField("_maxRank")] public int MaxRank = 3;
        [DefinitionField("_costs")] public int[] Costs = Array.Empty<int>();
    }
}
