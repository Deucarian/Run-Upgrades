using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Deucarian.Attacks.Authoring;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.RunUpgrades.Authoring;
using Deucarian.WeaponSystems.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.RunUpgrades.Editor
{
    internal static class RunUpgradeAuthoringEffectSummary
    {
        public static string GetTargetTypeLabel(RunUpgradeEffectAuthoringState effect)
        {
            if (effect == null)
                return "Custom";
            if (effect.Attack != null)
                return "Attack";
            if (effect.Weapon != null)
                return "Weapon";
            if (effect.Enemy != null)
                return "Enemy";

            switch (effect.TargetKind)
            {
                case RunUpgradeAuthoringTargetKind.EnemyReward:
                    return "Economy";
                case RunUpgradeAuthoringTargetKind.StatusEffectPower:
                case RunUpgradeAuthoringTargetKind.StatusEffectDuration:
                    return "Status";
                case RunUpgradeAuthoringTargetKind.WeaponStat:
                case RunUpgradeAuthoringTargetKind.ProjectileSpeed:
                case RunUpgradeAuthoringTargetKind.Range:
                    return "Weapon";
                case RunUpgradeAuthoringTargetKind.AttackDamage:
                case RunUpgradeAuthoringTargetKind.AttackRate:
                    return "Attack";
                default:
                    return "Custom";
            }
        }

        public static string GetModifierLabel(RunUpgradeModifierType modifierType)
        {
            switch (modifierType)
            {
                case RunUpgradeModifierType.Additive:
                    return "Add";
                case RunUpgradeModifierType.Multiplicative:
                    return "Multiply";
                case RunUpgradeModifierType.SetValue:
                    return "Set";
                default:
                    return "Custom";
            }
        }

        public static string BuildBeforeAfterSummary(RunUpgradeEffectAuthoringState effect, int rank)
        {
            if (effect == null)
                return "No effect";

            rank = Math.Max(1, rank);
            double amount = effect.Amount * rank;
            switch (effect.ModifierType)
            {
                case RunUpgradeModifierType.Additive:
                    return "base -> base + " + amount.ToString("0.##", CultureInfo.InvariantCulture);
                case RunUpgradeModifierType.Multiplicative:
                    return "base -> base x " + Math.Pow(effect.Amount, rank).ToString("0.##", CultureInfo.InvariantCulture);
                case RunUpgradeModifierType.SetValue:
                    return "base -> " + effect.Amount.ToString("0.##", CultureInfo.InvariantCulture);
                default:
                    return "custom modifier";
            }
        }

        internal static RunUpgradeEffectAuthoringState PrimaryEffect(RunUpgradeAuthoringState state)
        {
            state.EnsureEffects();
            return state.Effects[0];
        }

        internal static string GetPrimaryTargetTypeLabel(RunUpgradeAuthoringState state)
        {
            return GetTargetTypeLabel(PrimaryEffect(state));
        }

        internal static string GetPrimaryModifierLabel(RunUpgradeAuthoringState state)
        {
            return GetModifierLabel(PrimaryEffect(state).ModifierType);
        }

        internal static string GetTargetPackageLabel(RunUpgradeEffectAuthoringState effect)
        {
            string type = GetTargetTypeLabel(effect);
            if (type == "Attack" || type == "Status")
                return "Attacks";
            if (type == "Weapon")
                return "Weapon Systems";
            if (type == "Enemy")
                return "Attacks / Enemies";
            if (type == "Economy")
                return "Run / Economy";
            return "Custom";
        }

        internal static string GetTargetStatLabel(RunUpgradeAuthoringTargetKind kind)
        {
            switch (kind)
            {
                case RunUpgradeAuthoringTargetKind.AttackDamage:
                    return "Damage";
                case RunUpgradeAuthoringTargetKind.AttackRate:
                    return "Fire rate";
                case RunUpgradeAuthoringTargetKind.ProjectileSpeed:
                    return "Projectile speed";
                case RunUpgradeAuthoringTargetKind.Range:
                    return "Range";
                case RunUpgradeAuthoringTargetKind.EnemyReward:
                    return "Reward";
                case RunUpgradeAuthoringTargetKind.WeaponStat:
                    return "Weapon stat";
                case RunUpgradeAuthoringTargetKind.StatusEffectPower:
                    return "Status power";
                case RunUpgradeAuthoringTargetKind.StatusEffectDuration:
                    return "Status duration";
                default:
                    return "Custom";
            }
        }

        internal static string GetTargetAssetLabel(RunUpgradeEffectAuthoringState effect)
        {
            if (effect == null)
                return "Not assigned";
            if (effect.Attack != null)
                return effect.Attack.DisplayName + " (" + effect.Attack.Id + ")";
            if (effect.Weapon != null)
                return effect.Weapon.DisplayName + " (" + effect.Weapon.Id + ")";
            if (effect.Enemy != null)
                return effect.Enemy.DisplayName + " (" + effect.Enemy.Id + ")";
            string target = RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetTargetId();
            return string.IsNullOrWhiteSpace(target) ? "Not assigned" : target;
        }

        internal static string BuildTargetPreviewLabel(RunUpgradeEffectAuthoringState effect)
        {
            string type = GetTargetTypeLabel(effect);
            string target = GetTargetAssetLabel(effect);
            return type + ": " + target;
        }

        internal static string BuildCompatibilityLabel(RunUpgradeEffectAuthoringState effect)
        {
            if (effect == null)
                return "Missing effect";
            if (double.IsNaN(effect.Amount) || double.IsInfinity(effect.Amount))
                return "Invalid amount";
            if (effect.ModifierType == RunUpgradeModifierType.Multiplicative && effect.Amount <= 0d)
                return "Multiplier must be greater than zero";
            if (effect.ModifierType == RunUpgradeModifierType.SetValue && effect.Amount < 0d)
                return "Set value cannot be negative";
            return "Compatible";
        }

        internal static string BuildModifierBehavior(RunUpgradeEffectAuthoringState effect)
        {
            if (effect == null)
                return "No effect";
            return GetModifierLabel(effect.ModifierType) + " " + effect.Amount.ToString("0.##", CultureInfo.InvariantCulture)
                + " to " + GetTargetStatLabel(effect.TargetKind);
        }

        internal static int GetCostForRank(RunUpgradeAuthoringState state, int rank)
        {
            int[] costs = RunUpgradeDefinitionAssetCreator.ParseCosts(state == null ? string.Empty : state.CostsCsv);
            if (costs.Length == 0 || rank <= 0)
                return 0;
            return costs[Mathf.Clamp(rank - 1, 0, costs.Length - 1)];
        }
    }
}
