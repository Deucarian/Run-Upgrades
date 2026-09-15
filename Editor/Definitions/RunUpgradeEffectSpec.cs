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
    public sealed class RunUpgradeEffectSpec
    {
        [DefinitionField("_targetKind")] public RunUpgradeAuthoringTargetKind TargetKind = RunUpgradeAuthoringTargetKind.AttackDamage;
        [DefinitionField("_modifierType")] public RunUpgradeModifierType ModifierType = RunUpgradeModifierType.Additive;
        [DefinitionField("_amount")] public double Amount = 1d;
        [DefinitionField("_attack")] public AttackDefinitionAsset Attack;
        [DefinitionField("_weapon")] public WeaponDefinitionAsset Weapon;
        [DefinitionField("_enemy")] public EnemyDefinitionAsset Enemy;
        [DefinitionField("_targetIdOverride")] public string TargetIdOverride = string.Empty;
        [DefinitionField("_effectIdOverride")] public string EffectIdOverride = string.Empty;
    }
}
