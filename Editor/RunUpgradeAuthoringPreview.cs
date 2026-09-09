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
    internal static class RunUpgradeAuthoringPreview
    {
        internal static void TrackPreviewSource(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state, RunUpgradeGameContentPreviewController previewController)
        {
            string key = state.Creating
                ? "__draft_upgrade__"
                : context.SelectedItem == null
                    ? string.Empty
                    : context.SelectedItem.Key;
            state.SetPreviewSource(key, () => previewController?.Stop());
        }

        internal static void DrawPreviewLab(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState draft, RunUpgradeProviderV2State state)
        {
            RunUpgradeAuthoringState source = state.Creating ? draft : state.EditingState;
            if (source == null)
            {
                EditorGUILayout.LabelField("Select an upgrade to preview.", DeucarianEditorStyles.MutedLabel);
                return;
            }

            source.EnsureEffects();
            bool dirty = !state.Creating && state.EditingContext != null && state.EditingContext.IsDirty;
            int selectedRank = GetSelectedRank(source, state);
            GameContentPreviewLabModel model = new GameContentPreviewLabModel
            {
                Title = "Upgrade Preview Lab",
                PreviewTitle = string.IsNullOrWhiteSpace(source.DisplayName) ? "Upgrade Preview" : source.DisplayName,
                ScopeLabel = RunUpgradeProviderV2PreviewModel.GetScopeLabel(state.Creating, dirty),
                PrimaryAsset = GetPrimaryPreviewAsset(source),
                EmptyText = "No target visual asset assigned.",
                Chips = RunUpgradeProviderV2PreviewModel.BuildChips(source, state),
                DrawControls = () => DrawPreviewControls(source, state),
                DrawContext = () => DrawPreviewContext(context, source, selectedRank),
                DrawBody = () => DrawPreviewBody(context, source, state, selectedRank)
            };

            state.PreviewScroll = EditorGUILayout.BeginScrollView(state.PreviewScroll);
            context.Preview.SetStatus(state.PreviewStatus);
            GameContentPreviewLabRenderer.Draw(context.Preview, model);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawPreviewControls(RunUpgradeAuthoringState source, RunUpgradeProviderV2State state)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string playLabel = state.PreviewPlaying ? "Pause" : "Preview";
                if (DeucarianEditorMiniToolbar.Button(playLabel, true, GUILayout.Width(64f), GUILayout.Height(22f)))
                {
                    state.PreviewPlaying = !state.PreviewPlaying;
                    if (state.PreviewPlaying)
                        state.PreviewStartTime = EditorApplication.timeSinceStartup;
                    else
                        state.PausedNormalizedTime = 0.5f;
                }

                if (DeucarianEditorMiniToolbar.Button("Stop", true, GUILayout.Width(48f), GUILayout.Height(22f)))
                    state.StopPreview();
                if (DeucarianEditorMiniToolbar.Button(state.PreviewLoop ? "Loop" : "Once", true, GUILayout.Width(48f), GUILayout.Height(22f)))
                    state.PreviewLoop = !state.PreviewLoop;
                if (DeucarianEditorMiniToolbar.Button(state.PreviewMuted ? "Muted" : "Audio", true, GUILayout.Width(56f), GUILayout.Height(22f)))
                    state.PreviewMuted = !state.PreviewMuted;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (DeucarianEditorMiniToolbar.Button("Rank 1", true, GUILayout.Width(58f), GUILayout.Height(22f)))
                    state.SelectedRank = 1;
                if (DeucarianEditorMiniToolbar.Button("Middle", true, GUILayout.Width(58f), GUILayout.Height(22f)))
                    state.SelectedRank = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(1, source.MaxRank) * 0.5f));
                if (DeucarianEditorMiniToolbar.Button("Max", true, GUILayout.Width(48f), GUILayout.Height(22f)))
                    state.SelectedRank = Mathf.Max(1, source.MaxRank);
                GUILayout.FlexibleSpace();
                if (DeucarianEditorMiniToolbar.Button(state.PreviewRenderMode == GameContentAuthoringActionPreviewRenderMode.Game ? "Game" : "Debug", true, GUILayout.Width(58f), GUILayout.Height(22f)))
                    state.PreviewRenderMode = state.PreviewRenderMode == GameContentAuthoringActionPreviewRenderMode.Game
                        ? GameContentAuthoringActionPreviewRenderMode.Debug
                        : GameContentAuthoringActionPreviewRenderMode.Game;
            }
        }

        private static void DrawPreviewContext(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState source, int selectedRank)
        {
            RunUpgradeEffectAuthoringState effect = RunUpgradeAuthoringEffectSummary.PrimaryEffect(source);
            context.Preview.DrawSummaryRow("Target", RunUpgradeAuthoringEffectSummary.GetTargetAssetLabel(effect));
            context.Preview.DrawSummaryRow("Effect", RunUpgradeAuthoringEffectSummary.BuildModifierBehavior(effect));
            context.Preview.DrawSummaryRow("Rank", selectedRank.ToString(CultureInfo.InvariantCulture) + " / " + Math.Max(1, source.MaxRank).ToString(CultureInfo.InvariantCulture));
        }

        private static void DrawPreviewBody(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState source, RunUpgradeProviderV2State state, int selectedRank)
        {
            RunUpgradeEffectAuthoringState effect = RunUpgradeAuthoringEffectSummary.PrimaryEffect(source);
            context.Preview.DrawSummaryRows(new[]
            {
                RunUpgradeAuthoringSummary.Row("Before / After", RunUpgradeAuthoringEffectSummary.BuildBeforeAfterSummary(effect, selectedRank)),
                RunUpgradeAuthoringSummary.Row("Rank Cost", RunUpgradeAuthoringEffectSummary.GetCostForRank(source, selectedRank).ToString(CultureInfo.InvariantCulture)),
                RunUpgradeAuthoringSummary.Row("Target Preview", RunUpgradeAuthoringEffectSummary.BuildTargetPreviewLabel(effect)),
                RunUpgradeAuthoringSummary.Row("Content", RunUpgradeAuthoringSummary.BuildHumanSummary(source))
            });

            RunUpgradeAuthoringFields.DrawSummaryRows(RunUpgradeGameContentPreviewSummaries.BuildRankTimeline(source));
            if (state.PreviewRenderMode == GameContentAuthoringActionPreviewRenderMode.Debug)
            {
                context.Preview.DrawSummaryRows(new[]
                {
                    RunUpgradeAuthoringSummary.Row("Raw Target ID", RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetTargetId()),
                    RunUpgradeAuthoringSummary.Row("Raw Effect ID", RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetEffectId()),
                    RunUpgradeAuthoringSummary.Row("Modifier Type", effect.ModifierType.ToString()),
                    RunUpgradeAuthoringSummary.Row("Target Kind", effect.TargetKind.ToString())
                });
            }
        }

        internal static int GetSelectedRank(RunUpgradeAuthoringState state, RunUpgradeProviderV2State previewState)
        {
            int max = Math.Max(1, state == null ? 1 : state.MaxRank);
            if (previewState == null)
                return 1;
            previewState.SelectedRank = Mathf.Clamp(previewState.SelectedRank <= 0 ? 1 : previewState.SelectedRank, 1, max);
            return previewState.SelectedRank;
        }

        private static UnityEngine.Object GetPrimaryPreviewAsset(RunUpgradeAuthoringState state)
        {
            if (state == null)
                return null;
            if (state.Icon != null)
                return state.Icon;
            RunUpgradeEffectAuthoringState effect = RunUpgradeAuthoringEffectSummary.PrimaryEffect(state);
            if (effect.Attack != null)
                return effect.Attack;
            if (effect.Weapon != null)
                return effect.Weapon;
            if (effect.Enemy != null)
                return effect.Enemy;
            return null;
        }
    }
}
