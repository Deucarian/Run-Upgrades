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
    internal static class RunUpgradeAuthoringFields
    {
        internal static void DrawHeader(string title, string subtitle, IReadOnlyList<DeucarianEditorStatusChip> chips)
        {
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(title) ? "Upgrade" : title, DeucarianEditorStyles.SectionTitle);
            if (!string.IsNullOrWhiteSpace(subtitle))
                EditorGUILayout.LabelField(subtitle, DeucarianEditorStyles.MutedLabel);
            DeucarianEditorStatusChipRow.Draw(chips);
        }

        internal static void DrawOverview(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentLibraryItem selectedItem, bool creating)
        {
            state.UpgradeId = context.Authoring.DrawTextField("Stable ID", state.UpgradeId);
            state.DisplayName = context.Authoring.DrawTextField("Display Name", state.DisplayName);
            state.Description = context.Authoring.DrawTextArea("Description", state.Description);
            state.TagsCsv = context.Authoring.DrawTextField("Tags", state.TagsCsv);
            if (creating)
                state.OutputRoot = context.Authoring.DrawOutputRootField(state.OutputRoot);

            DrawSummaryRows(
                RunUpgradeAuthoringSummary.Row("Target", RunUpgradeAuthoringEffectSummary.GetPrimaryTargetTypeLabel(state)),
                RunUpgradeAuthoringSummary.Row("Modifier", RunUpgradeAuthoringEffectSummary.GetPrimaryModifierLabel(state)),
                RunUpgradeAuthoringSummary.Row("Summary", RunUpgradeAuthoringSummary.BuildHumanSummary(state)),
                RunUpgradeAuthoringSummary.Row("Used By", selectedItem == null ? "New draft" : RunUpgradeAuthoringSummary.BuildReverseReferenceSummary(selectedItem)));
        }

        internal static void DrawTarget(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentLibraryItem selectedItem)
        {
            RunUpgradeEffectAuthoringState effect = RunUpgradeAuthoringEffectSummary.PrimaryEffect(state);
            effect.TargetKind = context.Authoring.DrawEnumPopup("Target Type", effect.TargetKind);
            DrawRelevantTargetFields(effect);
            effect.TargetIdOverride = context.Authoring.DrawTextField("Target ID Override", effect.TargetIdOverride);

            DrawSummaryRows(
                RunUpgradeAuthoringSummary.Row("Target Package", RunUpgradeAuthoringEffectSummary.GetTargetPackageLabel(effect)),
                RunUpgradeAuthoringSummary.Row("Target Asset", RunUpgradeAuthoringEffectSummary.GetTargetAssetLabel(effect)),
                RunUpgradeAuthoringSummary.Row("Target Stat", RunUpgradeAuthoringEffectSummary.GetTargetStatLabel(effect.TargetKind)),
                RunUpgradeAuthoringSummary.Row("Target Readiness", string.IsNullOrWhiteSpace(RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetTargetId()) ? "Missing target" : "Ready"),
                RunUpgradeAuthoringSummary.Row("Content Set", selectedItem == null ? "Draft target membership resolved after creation" : RunUpgradeAuthoringSummary.BuildTargetReferenceSummary(selectedItem)));
        }

        internal static void DrawEffect(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state)
        {
            state.EnsureEffects();
            for (int i = 0; i < state.Effects.Count; i++)
            {
                RunUpgradeEffectAuthoringState effect = state.Effects[i];
                bool remove = false;
                DeucarianEditorCards.DrawInlineCard(() =>
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Effect " + (i + 1).ToString(CultureInfo.InvariantCulture), DeucarianEditorStyles.SectionTitle);
                        GUILayout.FlexibleSpace();
                        if (DeucarianEditorMiniToolbar.Button("Remove", state.Effects.Count > 1, GUILayout.Width(70f), GUILayout.Height(22f)))
                            remove = true;
                    }

                    if (remove)
                        return;

                    effect.ModifierType = context.Authoring.DrawEnumPopup("Modifier", effect.ModifierType);
                    effect.Amount = context.Authoring.DrawDoubleField("Value", effect.Amount);
                    effect.EffectIdOverride = context.Authoring.DrawTextField("Effect ID Override", effect.EffectIdOverride);
                    DrawSummaryRows(
                        RunUpgradeAuthoringSummary.Row("Behavior", RunUpgradeAuthoringEffectSummary.BuildModifierBehavior(effect)),
                        RunUpgradeAuthoringSummary.Row("Before / After", RunUpgradeAuthoringEffectSummary.BuildBeforeAfterSummary(effect, 1)),
                        RunUpgradeAuthoringSummary.Row("Compatibility", RunUpgradeAuthoringEffectSummary.BuildCompatibilityLabel(effect)));
                });

                if (remove)
                {
                    state.Effects.RemoveAt(i);
                    break;
                }
            }

            GUILayout.Space(DeucarianEditorSpacing.Small);
            if (DeucarianEditorButtons.Secondary("Add Effect", true, GUILayout.Height(24f)))
                state.Effects.Add(new RunUpgradeEffectAuthoringState());
        }

        internal static void DrawRanksCost(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, RunUpgradeProviderV2State previewState)
        {
            state.Rarity = context.Authoring.DrawEnumPopup("Rarity", state.Rarity);
            state.Weight = context.Authoring.DrawIntField("Draft Weight", state.Weight);
            state.MaxRank = context.Authoring.DrawIntField("Max Rank", state.MaxRank);
            state.CostsCsv = context.Authoring.DrawTextField("Per-Rank Costs", state.CostsCsv);
            state.PrerequisitesCsv = context.Authoring.DrawTextField("Prerequisites", state.PrerequisitesCsv);
            state.ExclusionsCsv = context.Authoring.DrawTextField("Exclusions", state.ExclusionsCsv);

            DrawSummaryRows(RunUpgradeGameContentPreviewSummaries.BuildRankTimeline(state));
            DrawSummaryRows(
                RunUpgradeAuthoringSummary.Row("Selected Rank", RunUpgradeAuthoringPreview.GetSelectedRank(state, previewState).ToString(CultureInfo.InvariantCulture)),
                RunUpgradeAuthoringSummary.Row("Final Impact", RunUpgradeAuthoringEffectSummary.BuildBeforeAfterSummary(RunUpgradeAuthoringEffectSummary.PrimaryEffect(state), Math.Max(1, state.MaxRank))));
        }

        internal static void DrawPresentation(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state)
        {
            state.Icon = DrawObjectField("Icon", state.Icon);
            state.Description = context.Authoring.DrawTextArea("Description", state.Description);
            DrawSummaryRows(
                RunUpgradeAuthoringSummary.Row("Icon", state.Icon == null ? "Not assigned" : state.Icon.name),
                RunUpgradeAuthoringSummary.Row("Audio", "Not supported by upgrade schema"),
                RunUpgradeAuthoringSummary.Row("Presentation", string.IsNullOrWhiteSpace(state.Description) ? "Text only" : "Icon/text ready"));
        }

        internal static void DrawReferences(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentLibraryItem selectedItem)
        {
            DrawSummaryRows(
                RunUpgradeAuthoringSummary.Row("Content Sets", RunUpgradeAuthoringSummary.CountReverse(selectedItem, GameContentLibraryKind.ContentSet).ToString(CultureInfo.InvariantCulture)),
                RunUpgradeAuthoringSummary.Row("Content Packs", RunUpgradeAuthoringSummary.CountReverse(selectedItem, GameContentLibraryKind.ContentPack).ToString(CultureInfo.InvariantCulture)),
                RunUpgradeAuthoringSummary.Row("Target", RunUpgradeAuthoringEffectSummary.GetTargetAssetLabel(RunUpgradeAuthoringEffectSummary.PrimaryEffect(state))));

            GameContentAuthoringProviderGUI.DrawReferenceList("Used By", selectedItem == null ? null : selectedItem.ReverseReferences);
            GameContentAuthoringProviderGUI.DrawReferenceList("Direct References", selectedItem == null ? null : selectedItem.DirectReferences);
        }

        internal static void DrawAdvanced(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentLibraryItem selectedItem, RunUpgradeDefinitionAsset asset)
        {
            DrawSummaryRows(
                RunUpgradeAuthoringSummary.Row("Path", selectedItem == null ? RunUpgradeDefinitionAssetCreator.GetPreviewLines(state)[0] : selectedItem.Path),
                RunUpgradeAuthoringSummary.Row("Economy Section", asset != null && asset.Economy != null ? AssetDatabase.GetAssetPath(asset.Economy) : "Missing"),
                RunUpgradeAuthoringSummary.Row("Effects Section", asset != null && asset.Effects != null ? AssetDatabase.GetAssetPath(asset.Effects) : "Missing"),
                RunUpgradeAuthoringSummary.Row("Effect ID", RunUpgradeDefinitionAssetCreator.ToRecipe(RunUpgradeAuthoringEffectSummary.PrimaryEffect(state)).GetEffectId()),
                RunUpgradeAuthoringSummary.Row("Target ID", RunUpgradeDefinitionAssetCreator.ToRecipe(RunUpgradeAuthoringEffectSummary.PrimaryEffect(state)).GetTargetId()));

            if (DeucarianEditorButtons.Secondary("Copy Report", true, GUILayout.Width(104f), GUILayout.Height(24f)))
                EditorGUIUtility.systemCopyBuffer = RunUpgradeAuthoringSummary.BuildAdvancedReport(selectedItem, state);
        }

        private static void DrawRelevantTargetFields(RunUpgradeEffectAuthoringState effect)
        {
            switch (RunUpgradeAuthoringEffectSummary.GetTargetTypeLabel(effect))
            {
                case "Attack":
                case "Status":
                    effect.Attack = DrawObjectField("Attack", effect.Attack);
                    break;
                case "Weapon":
                    effect.Weapon = DrawObjectField("Weapon", effect.Weapon);
                    if (effect.TargetKind == RunUpgradeAuthoringTargetKind.AttackDamage || effect.TargetKind == RunUpgradeAuthoringTargetKind.AttackRate)
                        effect.Attack = DrawObjectField("Attack", effect.Attack);
                    break;
                case "Enemy":
                case "Economy":
                    effect.Enemy = DrawObjectField("Enemy", effect.Enemy);
                    break;
                default:
                    effect.Attack = DrawObjectField("Attack", effect.Attack);
                    effect.Weapon = DrawObjectField("Weapon", effect.Weapon);
                    effect.Enemy = DrawObjectField("Enemy", effect.Enemy);
                    break;
            }
        }

        internal static void DrawSummaryRows(params GameContentAuthoringPreviewRow[] rows)
        {
            GameContentAuthoringProviderGUI.DrawSummaryRows((IReadOnlyList<GameContentAuthoringPreviewRow>)rows, true);
        }

        internal static void DrawSummaryRows(IReadOnlyList<GameContentAuthoringPreviewRow> rows)
        {
            GameContentAuthoringProviderGUI.DrawSummaryRows(rows, true);
        }

        internal static void DrawSummaryRows(IReadOnlyList<GameContentAuthoringPreviewTimelineItem> items)
        {
            if (items == null || items.Count == 0)
                return;
            for (int i = 0; i < items.Count; i++)
            {
                GameContentAuthoringPreviewTimelineItem item = items[i];
                string detail = string.IsNullOrWhiteSpace(item.Detail) ? item.TimeLabel : item.Detail;
                DeucarianEditorFieldRow.Draw(item.Label, () => EditorGUILayout.LabelField(detail ?? string.Empty, DeucarianEditorStyles.MutedLabel));
            }
        }

        private static T DrawObjectField<T>(string label, T value) where T : UnityEngine.Object
        {
            T next = value;
            DeucarianEditorFieldRow.Draw(label, () =>
            {
                next = (T)EditorGUILayout.ObjectField(value, typeof(T), false);
                if (DeucarianEditorMiniToolbar.PingButton(next))
                    GUI.FocusControl(null);
            });
            return next;
        }
    }
}
