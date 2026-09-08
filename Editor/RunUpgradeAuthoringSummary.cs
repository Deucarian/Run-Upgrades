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
    internal static class RunUpgradeAuthoringSummary
    {
        internal static string BuildTargetReferenceSummary(GameContentLibraryItem selectedItem)
        {
            if (selectedItem == null || selectedItem.ReverseReferences.Count == 0)
                return "No known content set reference";
            int sets = CountReverse(selectedItem, GameContentLibraryKind.ContentSet);
            return sets.ToString(CultureInfo.InvariantCulture) + " content set(s)";
        }

        internal static string BuildReverseReferenceSummary(GameContentLibraryItem item)
        {
            if (item == null || item.ReverseReferences.Count == 0)
                return "0 set(s), 0 pack(s)";
            return CountReverse(item, GameContentLibraryKind.ContentSet).ToString(CultureInfo.InvariantCulture) + " set(s), "
                + CountReverse(item, GameContentLibraryKind.ContentPack).ToString(CultureInfo.InvariantCulture) + " pack(s)";
        }

        internal static int CountReverse(GameContentLibraryItem item, GameContentLibraryKind kind)
        {
            if (item == null)
                return 0;
            int count = 0;
            for (int i = 0; i < item.ReverseReferences.Count; i++)
            {
                GameContentLibraryItem target = item.ReverseReferences[i].Target;
                if (target != null && target.Kind == kind)
                    count++;
            }

            return count;
        }

        internal static IReadOnlyList<DeucarianEditorStatusChip> BuildUpgradeChips(RunUpgradeAuthoringState state, GameContentAuthoringValidationResult validation)
        {
            RunUpgradeEffectAuthoringState effect = RunUpgradeAuthoringEffectSummary.PrimaryEffect(state);
            return new[]
            {
                new DeucarianEditorStatusChip(RunUpgradeAuthoringEffectSummary.GetTargetTypeLabel(effect), DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(RunUpgradeAuthoringEffectSummary.GetModifierLabel(effect.ModifierType), DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(validation != null && validation.ErrorCount > 0 ? "Blocked" : validation != null && validation.WarningCount > 0 ? "Warnings" : "Ready", validation != null && validation.ErrorCount > 0 ? DeucarianEditorStatus.Error : validation != null && validation.WarningCount > 0 ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(Math.Max(1, state.MaxRank).ToString(CultureInfo.InvariantCulture) + " ranks", state.MaxRank > 0 ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error),
                new DeucarianEditorStatusChip(effect.Attack != null || effect.Weapon != null || effect.Enemy != null || !string.IsNullOrWhiteSpace(effect.TargetIdOverride) ? "Target" : "NoTarget", effect.Attack != null || effect.Weapon != null || effect.Enemy != null || !string.IsNullOrWhiteSpace(effect.TargetIdOverride) ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error)
            };
        }

        internal static string BuildHumanSummary(RunUpgradeAuthoringState state)
        {
            RunUpgradeEffectAuthoringState effect = RunUpgradeAuthoringEffectSummary.PrimaryEffect(state);
            return RunUpgradeAuthoringEffectSummary.GetTargetTypeLabel(effect) + ", " + RunUpgradeAuthoringEffectSummary.GetModifierLabel(effect.ModifierType) + ", "
                + Math.Max(1, state.MaxRank).ToString(CultureInfo.InvariantCulture) + " rank(s)";
        }

        internal static string BuildAdvancedReport(GameContentLibraryItem item, RunUpgradeAuthoringState state)
        {
            RunUpgradeEffectAuthoringState effect = RunUpgradeAuthoringEffectSummary.PrimaryEffect(state);
            return "Upgrade: " + state.DisplayName + Environment.NewLine
                + "ID: " + state.UpgradeId + Environment.NewLine
                + "Path: " + (item == null ? "(draft)" : item.Path) + Environment.NewLine
                + "Target: " + RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetTargetId() + Environment.NewLine
                + "Effect: " + RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetEffectId() + Environment.NewLine
                + "Ranks: " + state.MaxRank.ToString(CultureInfo.InvariantCulture);
        }

        internal static GameContentAuthoringPreviewRow Row(string label, string value)
        {
            return new GameContentAuthoringPreviewRow(label, value);
        }
    }
}
