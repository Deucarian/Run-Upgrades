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
    internal sealed class RunUpgradeProviderV2ListItem
    {
        private RunUpgradeProviderV2ListItem(GameContentLibraryItem source, RunUpgradeDefinitionAsset asset)
        {
            Source = source;
            Asset = asset;
            StableId = asset == null ? source == null ? string.Empty : source.Id : asset.Id;
            DisplayName = asset == null ? source == null ? "Upgrade" : source.DisplayName : asset.DisplayName;
            Tags = asset == null ? string.Empty : string.Join(", ", asset.Tags);
            RunUpgradeEffectRecipe primary = GetPrimaryEffect(asset);
            TargetTypeLabel = GetTargetTypeLabel(primary);
            ModifierLabel = primary == null ? "Custom" : RunUpgradeAuthoringEffectSummary.GetModifierLabel(primary.ModifierType);
            HasTarget = primary != null && !string.IsNullOrWhiteSpace(primary.GetTargetId());
            TargetAssetLabel = HasTarget ? "Target" : "NoTarget";
            TargetTooltip = primary == null ? "Missing effect" : primary.GetTargetId();
            HasIcon = asset != null && asset.Icon != null;
            int maxRank = asset != null && asset.Economy != null ? asset.Economy.MaxRank : 0;
            int[] costs = asset != null && asset.Economy != null ? asset.Economy.Costs : Array.Empty<int>();
            HasValidRanks = maxRank > 0 && (costs.Length == 0 || costs.Length == maxRank);
            RankCostLabel = maxRank > 0 ? maxRank.ToString(CultureInfo.InvariantCulture) + " rank" + (maxRank == 1 ? string.Empty : "s") : "NoRanks";
            ReadinessLabel = source == null ? "Ready" : source.ValidationLabel;
            ReadinessStatus = source != null && source.ErrorCount > 0 ? DeucarianEditorStatus.Error : source != null && source.WarningCount > 0 ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success;
        }

        public GameContentLibraryItem Source { get; }
        public RunUpgradeDefinitionAsset Asset { get; }
        public string StableId { get; }
        public string DisplayName { get; }
        public string Tags { get; }
        public string TargetTypeLabel { get; }
        public string ModifierLabel { get; }
        public bool HasTarget { get; }
        public string TargetAssetLabel { get; }
        public string TargetTooltip { get; }
        public bool HasIcon { get; }
        public bool HasValidRanks { get; }
        public string RankCostLabel { get; }
        public string ReadinessLabel { get; }
        public DeucarianEditorStatus ReadinessStatus { get; }

        public static IReadOnlyList<RunUpgradeProviderV2ListItem> Build(IReadOnlyList<GameContentLibraryItem> items)
        {
            if (items == null || items.Count == 0)
                return Array.Empty<RunUpgradeProviderV2ListItem>();
            var result = new List<RunUpgradeProviderV2ListItem>();
            for (int i = 0; i < items.Count; i++)
            {
                RunUpgradeDefinitionAsset asset = items[i].Asset as RunUpgradeDefinitionAsset;
                if (asset != null)
                    result.Add(new RunUpgradeProviderV2ListItem(items[i], asset));
            }

            result.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        public bool Matches(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return true;
            string value = searchText.Trim();
            return Contains(DisplayName, value)
                || Contains(StableId, value)
                || Contains(TargetTypeLabel, value)
                || Contains(ModifierLabel, value)
                || Contains(Tags, value)
                || Contains(TargetTooltip, value);
        }

        public static string GetTargetTypeLabelForTests(RunUpgradeEffectRecipe recipe)
        {
            return GetTargetTypeLabel(recipe);
        }

        public static string GetModifierLabelForTests(RunUpgradeModifierType modifierType)
        {
            return RunUpgradeAuthoringEffectSummary.GetModifierLabel(modifierType);
        }

        private static string GetTargetTypeLabel(RunUpgradeEffectRecipe recipe)
        {
            if (recipe == null)
                return "Custom";
            var state = new RunUpgradeEffectAuthoringState
            {
                TargetKind = recipe.TargetKind,
                ModifierType = recipe.ModifierType,
                Amount = recipe.Amount,
                Attack = recipe.Attack,
                Weapon = recipe.Weapon,
                Enemy = recipe.Enemy,
                TargetIdOverride = recipe.TargetIdOverride,
                EffectIdOverride = recipe.EffectIdOverride
            };
            return RunUpgradeAuthoringEffectSummary.GetTargetTypeLabel(state);
        }

        private static RunUpgradeEffectRecipe GetPrimaryEffect(RunUpgradeDefinitionAsset asset)
        {
            if (asset == null || asset.Effects == null || asset.Effects.Effects.Count == 0)
                return null;
            return asset.Effects.Effects[0];
        }

        private static bool Contains(string text, string value)
        {
            return (text ?? string.Empty).IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
