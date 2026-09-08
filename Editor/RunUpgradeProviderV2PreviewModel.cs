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
    internal static class RunUpgradeProviderV2PreviewModel
    {
        public const bool ExposesRedundantSelectButton = false;

        public static string GetScopeLabel(bool creating, bool unsaved)
        {
            if (creating)
                return "Draft";
            return unsaved ? "Unsaved" : "Selected";
        }

        public static IReadOnlyList<DeucarianEditorStatusChip> BuildChips(RunUpgradeAuthoringState state, RunUpgradeProviderV2State previewState)
        {
            if (state == null)
                return Array.Empty<DeucarianEditorStatusChip>();
            state.EnsureEffects();
            RunUpgradeEffectAuthoringState effect = state.Effects[0];
            bool debug = previewState != null && previewState.PreviewRenderMode == GameContentAuthoringActionPreviewRenderMode.Debug;
            bool hasTarget = effect.Attack != null || effect.Weapon != null || effect.Enemy != null || !string.IsNullOrWhiteSpace(effect.TargetIdOverride);
            return new[]
            {
                new DeucarianEditorStatusChip(debug ? "Debug" : "Game", debug ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(RunUpgradeAuthoringEffectSummary.GetTargetTypeLabel(effect), hasTarget ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error),
                new DeucarianEditorStatusChip(RunUpgradeAuthoringEffectSummary.GetModifierLabel(effect.ModifierType), DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(previewState == null || previewState.PreviewMuted ? "Muted" : "Audio", previewState == null || previewState.PreviewMuted ? DeucarianEditorStatus.Disabled : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(Math.Max(1, state.MaxRank).ToString(CultureInfo.InvariantCulture) + " ranks", state.MaxRank > 0 ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error)
            };
        }
    }
}
