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
    public sealed class RunUpgradeEffectsDefinitionSpec
    {
        [DefinitionField("_effects")] public RunUpgradeEffectSpec[] Effects = Array.Empty<RunUpgradeEffectSpec>();
        [DefinitionField("_prerequisites")] public string[] Prerequisites = Array.Empty<string>();
        [DefinitionField("_exclusions")] public string[] Exclusions = Array.Empty<string>();
    }
}
