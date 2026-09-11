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
    public sealed class RunUpgradeDefinitionSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("_icon")] public Sprite Icon;
        [DefinitionField("_description")] public string Description = "Increase authored combat output.";
        [DefinitionField("_tags")] public string[] Tags = Array.Empty<string>();
        [DefinitionSection("_economy", typeof(RunUpgradeEconomyDefinitionAsset))] public RunUpgradeEconomyDefinitionSpec Economy = new RunUpgradeEconomyDefinitionSpec();
        [DefinitionSection("_effects", typeof(RunUpgradeEffectsDefinitionAsset))] public RunUpgradeEffectsDefinitionSpec Effects = new RunUpgradeEffectsDefinitionSpec();
    }
}
