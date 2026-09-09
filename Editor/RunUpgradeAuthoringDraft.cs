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
    internal static class RunUpgradeAuthoringDraft
    {
        public static RunUpgradeAuthoringState FromUpgradeAsset(RunUpgradeDefinitionAsset asset)
        {
            var state = new RunUpgradeAuthoringState();
            state.Effects.Clear();
            if (asset == null)
            {
                state.EnsureEffects();
                return state;
            }

            state.UpgradeId = asset.Id;
            state.DisplayName = asset.DisplayName;
            state.Icon = asset.Icon;
            state.Description = asset.Description;
            state.TagsCsv = string.Join(", ", asset.Tags);
            state.OutputRoot = "Assets/GameContent/Upgrades";

            RunUpgradeEconomyDefinitionAsset economy = asset.Economy;
            if (economy != null)
            {
                state.Rarity = economy.Rarity;
                state.Weight = economy.Weight;
                state.MaxRank = economy.MaxRank;
                state.CostsCsv = string.Join(", ", economy.Costs);
            }

            RunUpgradeEffectsDefinitionAsset effects = asset.Effects;
            if (effects != null)
            {
                state.PrerequisitesCsv = string.Join(", ", effects.Prerequisites);
                state.ExclusionsCsv = string.Join(", ", effects.Exclusions);
                for (int i = 0; i < effects.Effects.Count; i++)
                {
                    RunUpgradeEffectRecipe recipe = effects.Effects[i];
                    if (recipe == null)
                        continue;

                    state.Effects.Add(new RunUpgradeEffectAuthoringState
                    {
                        TargetKind = recipe.TargetKind,
                        ModifierType = recipe.ModifierType,
                        Amount = recipe.Amount,
                        Attack = recipe.Attack,
                        Weapon = recipe.Weapon,
                        Enemy = recipe.Enemy,
                        TargetIdOverride = recipe.TargetIdOverride,
                        EffectIdOverride = recipe.EffectIdOverride
                    });
                }
            }

            state.EnsureEffects();
            return state;
        }

        public static string BuildStateFingerprint(RunUpgradeAuthoringState state)
        {
            if (state == null)
                return string.Empty;

            state.EnsureEffects();
            var builder = new StringBuilder();
            builder.Append(state.UpgradeId).Append('|')
                .Append(state.DisplayName).Append('|')
                .Append(AssetKey(state.Icon)).Append('|')
                .Append(state.Description).Append('|')
                .Append(state.TagsCsv).Append('|')
                .Append(state.Rarity).Append('|')
                .Append(state.Weight).Append('|')
                .Append(state.MaxRank).Append('|')
                .Append(state.CostsCsv).Append('|')
                .Append(state.PrerequisitesCsv).Append('|')
                .Append(state.ExclusionsCsv);

            for (int i = 0; i < state.Effects.Count; i++)
            {
                RunUpgradeEffectAuthoringState effect = state.Effects[i];
                builder.Append('|')
                    .Append(effect.TargetKind).Append('|')
                    .Append(effect.ModifierType).Append('|')
                    .Append(effect.Amount.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                    .Append(AssetKey(effect.Attack)).Append('|')
                    .Append(AssetKey(effect.Weapon)).Append('|')
                    .Append(AssetKey(effect.Enemy)).Append('|')
                    .Append(effect.TargetIdOverride).Append('|')
                    .Append(effect.EffectIdOverride);
            }

            return builder.ToString();
        }

        private static string AssetKey(UnityEngine.Object asset)
        {
            if (asset == null)
                return string.Empty;
            string path = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrWhiteSpace(path) ? asset.GetInstanceID().ToString(CultureInfo.InvariantCulture) : path;
        }
    }
}
