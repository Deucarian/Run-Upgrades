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
    internal sealed class RunUpgradeProviderV2State : GameContentAuthoringProviderSessionState<RunUpgradeAuthoringState>
    {
        public int SelectedRank = 1;

        public void BeginCreate()
        {
            Creating = true;
            WizardStep = 0;
            DetailScroll = Vector2.zero;
            SelectedRank = 1;
            ClearEditingState();
            PreviewStatus = "Previewing draft upgrade";
        }

        protected override void OnProviderSessionReset()
        {
            SelectedRank = 1;
        }
    }

    internal sealed class RunUpgradeProviderV2View
    {
        private static readonly string[] DetailPages =
        {
            "Overview",
            "Target",
            "Effect",
            "Ranks / Cost",
            "Presentation",
            "References",
            "Advanced"
        };

        private static readonly string[] WizardSteps =
        {
            "Identity",
            "Target",
            "Effect",
            "Ranks / Cost",
            "Presentation",
            "Review"
        };

        public void Draw(
            GameContentAuthoringSurfaceContext context,
            RunUpgradeAuthoringState draft,
            RunUpgradeGameContentPreviewController previewController,
            RunUpgradeProviderV2State state)
        {
            if (context == null || draft == null || state == null)
                return;

            draft.EnsureEffects();
            IReadOnlyList<RunUpgradeProviderV2ListItem> items = RunUpgradeProviderV2ListItem.Build(context.AuthoredItems);
            EnsureDefaultMode(context, state, items);
            EnsureEditingState(context, state);
            TrackPreviewSource(context, state, previewController);

            GameContentAuthoringWorkbench.Draw(
                context,
                () => DrawUpgradeList(context, state, items),
                () => DrawDetailOrWizard(context, draft, state),
                () => DrawPreviewLab(context, draft, state));
        }

        private static void EnsureDefaultMode(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state, IReadOnlyList<RunUpgradeProviderV2ListItem> items)
        {
            if (items.Count == 0)
            {
                state.Creating = true;
                state.ClearEditingState();
                return;
            }

            if (!state.Creating && context.SelectedItem == null)
            {
                context.SelectItem(items[0].Source);
                context.RequestRepaint();
            }
        }

        private static void EnsureEditingState(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state)
        {
            if (state.Creating || context.SelectedItem == null)
            {
                state.ClearEditingState();
                return;
            }

            RunUpgradeDefinitionAsset selected = context.SelectedItem.Asset as RunUpgradeDefinitionAsset;
            if (selected == null)
            {
                state.ClearEditingState();
                return;
            }

            if (state.EditingContext != null && string.Equals(state.EditingContext.Key, context.SelectedItem.Key, StringComparison.Ordinal) && state.EditingState != null)
                return;

            state.EditingState = FromUpgradeAsset(selected);
            string fingerprint = BuildStateFingerprint(state.EditingState);
            state.EditingContext = new GameContentAuthoringObjectEditorContext(context.SelectedItem, fingerprint);
            state.SelectedRank = 1;
            state.LastEditResult = null;
        }

        private static void TrackPreviewSource(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state, RunUpgradeGameContentPreviewController previewController)
        {
            string key = state.Creating
                ? "__draft_upgrade__"
                : context.SelectedItem == null
                    ? string.Empty
                    : context.SelectedItem.Key;
            state.SetPreviewSource(key, () => previewController?.Stop());
        }

        private static void DrawUpgradeList(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state, IReadOnlyList<RunUpgradeProviderV2ListItem> items)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Upgrades", DeucarianEditorStyles.SectionTitle);
                GUILayout.FlexibleSpace();
                if (DeucarianEditorMiniToolbar.Button("Refresh", true, GUILayout.Width(62f), GUILayout.Height(22f)))
                    context.RefreshLibrary();
            }

            state.SearchText = DeucarianEditorSearchField.Draw(state.SearchText, "Search upgrades", GUILayout.ExpandWidth(true));
            if (DeucarianEditorButtons.Secondary("Create New", true, GUILayout.Height(24f)))
            {
                state.BeginCreate();
                context.ClearSelection();
                context.RequestRepaint();
            }

            GUILayout.Space(DeucarianEditorSpacing.Small);
            state.ListScroll = EditorGUILayout.BeginScrollView(state.ListScroll);
            int shown = 0;
            for (int i = 0; i < items.Count; i++)
            {
                RunUpgradeProviderV2ListItem item = items[i];
                if (!item.Matches(state.SearchText))
                    continue;

                shown++;
                DrawUpgradeCard(context, state, item);
            }

            if (shown == 0)
                EditorGUILayout.LabelField(items.Count == 0 ? "No authored upgrades found." : "No upgrades match the current search.", DeucarianEditorStyles.MutedLabel);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawUpgradeCard(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state, RunUpgradeProviderV2ListItem item)
        {
            bool selected = !state.Creating && context.IsSelected(item.Source);
            var chips = new[]
            {
                new DeucarianEditorStatusChip(item.TargetTypeLabel, DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(item.ModifierLabel, DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(item.ReadinessLabel, item.ReadinessStatus),
                new DeucarianEditorStatusChip(item.RankCostLabel, item.HasValidRanks ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error),
                new DeucarianEditorStatusChip(item.TargetAssetLabel, item.HasTarget ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error, item.TargetTooltip),
                new DeucarianEditorStatusChip(item.HasIcon ? "Icon" : "NoIcon", item.HasIcon ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Disabled)
            };

            bool clicked = DeucarianEditorCompactObjectCard.Draw(
                item.DisplayName,
                item.StableId,
                selected,
                chips,
                () =>
                {
                    if (DeucarianEditorMiniToolbar.PingButton(item.Source.Asset))
                        GUI.FocusControl(null);
                },
                null,
                GUILayout.ExpandWidth(true));

            if (clicked && item.Source != null)
            {
                state.Creating = false;
                state.DetailScroll = Vector2.zero;
                state.SelectedRank = 1;
                context.SelectItem(item.Source);
                if (Event.current != null)
                    Event.current.Use();
            }
        }

        private static void DrawDetailOrWizard(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState draft, RunUpgradeProviderV2State state)
        {
            state.DetailScroll = EditorGUILayout.BeginScrollView(state.DetailScroll);
            if (state.Creating)
                DrawCreateWizard(context, draft, state);
            else
                DrawSelectedUpgrade(context, state);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSelectedUpgrade(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state)
        {
            RunUpgradeDefinitionAsset asset = context.SelectedItem == null ? null : context.SelectedItem.Asset as RunUpgradeDefinitionAsset;
            if (asset == null || state.EditingState == null || state.EditingContext == null)
            {
                EditorGUILayout.LabelField("Select an upgrade to edit.", DeucarianEditorStyles.MutedLabel);
                return;
            }

            RunUpgradeAuthoringState edit = state.EditingState;
            edit.EnsureEffects();
            string fingerprint = BuildStateFingerprint(edit);
            GameContentAuthoringValidationResult validation = RunUpgradeDefinitionAssetCreator.ValidateForUpdate(edit, asset);
            state.EditingContext.Capture(fingerprint, validation);
            context.Authoring.SetValidation(validation);

            DrawHeader(edit.DisplayName, edit.UpgradeId, BuildUpgradeChips(edit, validation));
            GameContentAuthoringCommand command = GameContentAuthoringCommandBar.Draw(
                GameContentAuthoringWorkbenchMode.Edit,
                validation.IsValid,
                state.EditingContext.IsDirty,
                "Save",
                state.LastEditResult == null ? state.EditingContext.StatusMessage : state.LastEditResult.Message);
            HandleEditCommand(context, state, asset, command);

            state.DetailPage = DeucarianEditorSegmentedControl.DrawPageChips(state.DetailPage, DetailPages);
            GUILayout.Space(DeucarianEditorSpacing.Small);
            switch (Mathf.Clamp(state.DetailPage, 0, DetailPages.Length - 1))
            {
                case 0:
                    DrawOverview(context, edit, context.SelectedItem, false);
                    break;
                case 1:
                    DrawTarget(context, edit, context.SelectedItem);
                    break;
                case 2:
                    DrawEffect(context, edit);
                    break;
                case 3:
                    DrawRanksCost(context, edit, state);
                    break;
                case 4:
                    DrawPresentation(context, edit);
                    break;
                case 5:
                    DrawReferences(context, edit, context.SelectedItem);
                    break;
                default:
                    DrawAdvanced(context, edit, context.SelectedItem, asset);
                    break;
            }

            GameContentAuthoringProviderGUI.DrawValidationIssues(
                validation,
                GameContentAuthoringValidationSummaryStyle.Counts,
                false);
        }

        private static void HandleEditCommand(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state, RunUpgradeDefinitionAsset asset, GameContentAuthoringCommand command)
        {
            if (command == GameContentAuthoringCommand.Revert)
            {
                state.EditingState = FromUpgradeAsset(asset);
                string fingerprint = BuildStateFingerprint(state.EditingState);
                state.EditingContext.Accept(fingerprint, "Reverted");
                state.LastEditResult = null;
                GUI.FocusControl(null);
                context.RequestRepaint();
                return;
            }

            if (command != GameContentAuthoringCommand.Save)
                return;

            state.LastEditResult = RunUpgradeDefinitionAssetCreator.UpdateExistingAsset(asset, state.EditingState);
            if (state.LastEditResult != null && state.LastEditResult.Succeeded)
            {
                state.EditingState = FromUpgradeAsset(asset);
                string fingerprint = BuildStateFingerprint(state.EditingState);
                state.EditingContext.Accept(fingerprint, "Saved");
                context.RefreshLibrary();
            }
            else if (state.EditingContext != null && state.LastEditResult != null)
            {
                state.EditingContext.SetStatus(state.LastEditResult.Message);
            }

            GUI.FocusControl(null);
            context.RequestRepaint();
        }

        private static void DrawCreateWizard(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState draft, RunUpgradeProviderV2State state)
        {
            draft.EnsureEffects();
            RunUpgradeDefinitionAsset preview = RunUpgradeDefinitionAssetCreator.BuildTransient(draft);
            GameContentAuthoringValidationResult validation;
            try
            {
                validation = RunUpgradeDefinitionAssetCreator.ValidateForCreation(draft, preview);
            }
            finally
            {
                RunUpgradeDefinitionAssetCreator.DestroyTransient(preview);
            }

            DrawHeader("New Upgrade", draft.UpgradeId, BuildUpgradeChips(draft, validation));
            GameContentAuthoringCommand command = GameContentAuthoringCommandBar.Draw(GameContentAuthoringWorkbenchMode.Create, validation.IsValid, true, "Create");
            if (command == GameContentAuthoringCommand.Create)
            {
                GameContentCreationResult result = RunUpgradeDefinitionAssetCreator.CreateAssets(draft);
                context.Authoring.SetCreationResult(result);
                if (result != null && result.Succeeded)
                {
                    state.Creating = false;
                    context.RefreshLibrary();
                }
            }

            state.WizardStep = DeucarianEditorWizardHeader.Draw(state.WizardStep, WizardSteps);
            GUILayout.Space(DeucarianEditorSpacing.Small);
            switch (Mathf.Clamp(state.WizardStep, 0, WizardSteps.Length - 1))
            {
                case 0:
                    DrawOverview(context, draft, null, true);
                    break;
                case 1:
                    DrawTarget(context, draft, null);
                    break;
                case 2:
                    DrawEffect(context, draft);
                    break;
                case 3:
                    DrawRanksCost(context, draft, state);
                    break;
                case 4:
                    DrawPresentation(context, draft);
                    break;
                default:
                    DrawReview(context, draft, validation);
                    break;
            }

            GameContentAuthoringProviderGUI.DrawValidationIssues(
                validation,
                GameContentAuthoringValidationSummaryStyle.Counts,
                false);
            context.Authoring.DrawCreationResult();
        }

        private static void DrawHeader(string title, string subtitle, IReadOnlyList<DeucarianEditorStatusChip> chips)
        {
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(title) ? "Upgrade" : title, DeucarianEditorStyles.SectionTitle);
            if (!string.IsNullOrWhiteSpace(subtitle))
                EditorGUILayout.LabelField(subtitle, DeucarianEditorStyles.MutedLabel);
            DeucarianEditorStatusChipRow.Draw(chips);
        }

        private static void DrawOverview(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentLibraryItem selectedItem, bool creating)
        {
            state.UpgradeId = context.Authoring.DrawTextField("Stable ID", state.UpgradeId);
            state.DisplayName = context.Authoring.DrawTextField("Display Name", state.DisplayName);
            state.Description = context.Authoring.DrawTextArea("Description", state.Description);
            state.TagsCsv = context.Authoring.DrawTextField("Tags", state.TagsCsv);
            if (creating)
                state.OutputRoot = context.Authoring.DrawOutputRootField(state.OutputRoot);

            DrawSummaryRows(
                Row("Target", GetPrimaryTargetTypeLabel(state)),
                Row("Modifier", GetPrimaryModifierLabel(state)),
                Row("Summary", BuildHumanSummary(state)),
                Row("Used By", selectedItem == null ? "New draft" : BuildReverseReferenceSummary(selectedItem)));
        }

        private static void DrawTarget(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentLibraryItem selectedItem)
        {
            RunUpgradeEffectAuthoringState effect = PrimaryEffect(state);
            effect.TargetKind = context.Authoring.DrawEnumPopup("Target Type", effect.TargetKind);
            DrawRelevantTargetFields(effect);
            effect.TargetIdOverride = context.Authoring.DrawTextField("Target ID Override", effect.TargetIdOverride);

            DrawSummaryRows(
                Row("Target Package", GetTargetPackageLabel(effect)),
                Row("Target Asset", GetTargetAssetLabel(effect)),
                Row("Target Stat", GetTargetStatLabel(effect.TargetKind)),
                Row("Target Readiness", string.IsNullOrWhiteSpace(RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetTargetId()) ? "Missing target" : "Ready"),
                Row("Content Set", selectedItem == null ? "Draft target membership resolved after creation" : BuildTargetReferenceSummary(selectedItem)));
        }

        private static void DrawEffect(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state)
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
                        Row("Behavior", BuildModifierBehavior(effect)),
                        Row("Before / After", BuildBeforeAfterSummary(effect, 1)),
                        Row("Compatibility", BuildCompatibilityLabel(effect)));
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

        private static void DrawRanksCost(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, RunUpgradeProviderV2State previewState)
        {
            state.Rarity = context.Authoring.DrawEnumPopup("Rarity", state.Rarity);
            state.Weight = context.Authoring.DrawIntField("Draft Weight", state.Weight);
            state.MaxRank = context.Authoring.DrawIntField("Max Rank", state.MaxRank);
            state.CostsCsv = context.Authoring.DrawTextField("Per-Rank Costs", state.CostsCsv);
            state.PrerequisitesCsv = context.Authoring.DrawTextField("Prerequisites", state.PrerequisitesCsv);
            state.ExclusionsCsv = context.Authoring.DrawTextField("Exclusions", state.ExclusionsCsv);

            DrawSummaryRows(RunUpgradeGameContentPreviewSummaries.BuildRankTimeline(state));
            DrawSummaryRows(
                Row("Selected Rank", GetSelectedRank(state, previewState).ToString(CultureInfo.InvariantCulture)),
                Row("Final Impact", BuildBeforeAfterSummary(PrimaryEffect(state), Math.Max(1, state.MaxRank))));
        }

        private static void DrawPresentation(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state)
        {
            state.Icon = DrawObjectField("Icon", state.Icon);
            state.Description = context.Authoring.DrawTextArea("Description", state.Description);
            DrawSummaryRows(
                Row("Icon", state.Icon == null ? "Not assigned" : state.Icon.name),
                Row("Audio", "Not supported by upgrade schema"),
                Row("Presentation", string.IsNullOrWhiteSpace(state.Description) ? "Text only" : "Icon/text ready"));
        }

        private static void DrawReferences(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentLibraryItem selectedItem)
        {
            DrawSummaryRows(
                Row("Content Sets", CountReverse(selectedItem, GameContentLibraryKind.ContentSet).ToString(CultureInfo.InvariantCulture)),
                Row("Content Packs", CountReverse(selectedItem, GameContentLibraryKind.ContentPack).ToString(CultureInfo.InvariantCulture)),
                Row("Target", GetTargetAssetLabel(PrimaryEffect(state))));

            GameContentAuthoringProviderGUI.DrawReferenceList("Used By", selectedItem == null ? null : selectedItem.ReverseReferences);
            GameContentAuthoringProviderGUI.DrawReferenceList("Direct References", selectedItem == null ? null : selectedItem.DirectReferences);
        }

        private static void DrawAdvanced(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentLibraryItem selectedItem, RunUpgradeDefinitionAsset asset)
        {
            DrawSummaryRows(
                Row("Path", selectedItem == null ? RunUpgradeDefinitionAssetCreator.GetPreviewLines(state)[0] : selectedItem.Path),
                Row("Economy Section", asset != null && asset.Economy != null ? AssetDatabase.GetAssetPath(asset.Economy) : "Missing"),
                Row("Effects Section", asset != null && asset.Effects != null ? AssetDatabase.GetAssetPath(asset.Effects) : "Missing"),
                Row("Effect ID", RunUpgradeDefinitionAssetCreator.ToRecipe(PrimaryEffect(state)).GetEffectId()),
                Row("Target ID", RunUpgradeDefinitionAssetCreator.ToRecipe(PrimaryEffect(state)).GetTargetId()));

            if (DeucarianEditorButtons.Secondary("Copy Report", true, GUILayout.Width(104f), GUILayout.Height(24f)))
                EditorGUIUtility.systemCopyBuffer = BuildAdvancedReport(selectedItem, state);
        }

        private static void DrawReview(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentAuthoringValidationResult validation)
        {
            IReadOnlyList<string> lines = RunUpgradeDefinitionAssetCreator.GetPreviewLines(state);
            for (int i = 0; i < lines.Count; i++)
                EditorGUILayout.LabelField(lines[i], DeucarianEditorStyles.MutedLabel);

            DrawSummaryRows(
                Row("Readiness", new GameContentAuthoringValidationSummary(validation).CountLabel),
                Row("Target", GetTargetAssetLabel(PrimaryEffect(state))),
                Row("Effect", BuildModifierBehavior(PrimaryEffect(state))),
                Row("Ranks", Math.Max(1, state.MaxRank).ToString(CultureInfo.InvariantCulture)));
        }

        private static void DrawPreviewLab(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState draft, RunUpgradeProviderV2State state)
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
            RunUpgradeEffectAuthoringState effect = PrimaryEffect(source);
            context.Preview.DrawSummaryRow("Target", GetTargetAssetLabel(effect));
            context.Preview.DrawSummaryRow("Effect", BuildModifierBehavior(effect));
            context.Preview.DrawSummaryRow("Rank", selectedRank.ToString(CultureInfo.InvariantCulture) + " / " + Math.Max(1, source.MaxRank).ToString(CultureInfo.InvariantCulture));
        }

        private static void DrawPreviewBody(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState source, RunUpgradeProviderV2State state, int selectedRank)
        {
            RunUpgradeEffectAuthoringState effect = PrimaryEffect(source);
            context.Preview.DrawSummaryRows(new[]
            {
                Row("Before / After", BuildBeforeAfterSummary(effect, selectedRank)),
                Row("Rank Cost", GetCostForRank(source, selectedRank).ToString(CultureInfo.InvariantCulture)),
                Row("Target Preview", BuildTargetPreviewLabel(effect)),
                Row("Content", BuildHumanSummary(source))
            });

            DrawSummaryRows(RunUpgradeGameContentPreviewSummaries.BuildRankTimeline(source));
            if (state.PreviewRenderMode == GameContentAuthoringActionPreviewRenderMode.Debug)
            {
                context.Preview.DrawSummaryRows(new[]
                {
                    Row("Raw Target ID", RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetTargetId()),
                    Row("Raw Effect ID", RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetEffectId()),
                    Row("Modifier Type", effect.ModifierType.ToString()),
                    Row("Target Kind", effect.TargetKind.ToString())
                });
            }
        }

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

        private static RunUpgradeEffectAuthoringState PrimaryEffect(RunUpgradeAuthoringState state)
        {
            state.EnsureEffects();
            return state.Effects[0];
        }

        private static void DrawRelevantTargetFields(RunUpgradeEffectAuthoringState effect)
        {
            switch (GetTargetTypeLabel(effect))
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

        private static string GetPrimaryTargetTypeLabel(RunUpgradeAuthoringState state)
        {
            return GetTargetTypeLabel(PrimaryEffect(state));
        }

        private static string GetPrimaryModifierLabel(RunUpgradeAuthoringState state)
        {
            return GetModifierLabel(PrimaryEffect(state).ModifierType);
        }

        private static string GetTargetPackageLabel(RunUpgradeEffectAuthoringState effect)
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

        private static string GetTargetStatLabel(RunUpgradeAuthoringTargetKind kind)
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

        private static string GetTargetAssetLabel(RunUpgradeEffectAuthoringState effect)
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

        private static string BuildTargetPreviewLabel(RunUpgradeEffectAuthoringState effect)
        {
            string type = GetTargetTypeLabel(effect);
            string target = GetTargetAssetLabel(effect);
            return type + ": " + target;
        }

        private static string BuildCompatibilityLabel(RunUpgradeEffectAuthoringState effect)
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

        private static string BuildModifierBehavior(RunUpgradeEffectAuthoringState effect)
        {
            if (effect == null)
                return "No effect";
            return GetModifierLabel(effect.ModifierType) + " " + effect.Amount.ToString("0.##", CultureInfo.InvariantCulture)
                + " to " + GetTargetStatLabel(effect.TargetKind);
        }

        private static int GetSelectedRank(RunUpgradeAuthoringState state, RunUpgradeProviderV2State previewState)
        {
            int max = Math.Max(1, state == null ? 1 : state.MaxRank);
            if (previewState == null)
                return 1;
            previewState.SelectedRank = Mathf.Clamp(previewState.SelectedRank <= 0 ? 1 : previewState.SelectedRank, 1, max);
            return previewState.SelectedRank;
        }

        private static int GetCostForRank(RunUpgradeAuthoringState state, int rank)
        {
            int[] costs = RunUpgradeDefinitionAssetCreator.ParseCosts(state == null ? string.Empty : state.CostsCsv);
            if (costs.Length == 0 || rank <= 0)
                return 0;
            return costs[Mathf.Clamp(rank - 1, 0, costs.Length - 1)];
        }

        private static UnityEngine.Object GetPrimaryPreviewAsset(RunUpgradeAuthoringState state)
        {
            if (state == null)
                return null;
            if (state.Icon != null)
                return state.Icon;
            RunUpgradeEffectAuthoringState effect = PrimaryEffect(state);
            if (effect.Attack != null)
                return effect.Attack;
            if (effect.Weapon != null)
                return effect.Weapon;
            if (effect.Enemy != null)
                return effect.Enemy;
            return null;
        }

        private static string BuildTargetReferenceSummary(GameContentLibraryItem selectedItem)
        {
            if (selectedItem == null || selectedItem.ReverseReferences.Count == 0)
                return "No known content set reference";
            int sets = CountReverse(selectedItem, GameContentLibraryKind.ContentSet);
            return sets.ToString(CultureInfo.InvariantCulture) + " content set(s)";
        }

        private static string BuildReverseReferenceSummary(GameContentLibraryItem item)
        {
            if (item == null || item.ReverseReferences.Count == 0)
                return "0 set(s), 0 pack(s)";
            return CountReverse(item, GameContentLibraryKind.ContentSet).ToString(CultureInfo.InvariantCulture) + " set(s), "
                + CountReverse(item, GameContentLibraryKind.ContentPack).ToString(CultureInfo.InvariantCulture) + " pack(s)";
        }

        private static int CountReverse(GameContentLibraryItem item, GameContentLibraryKind kind)
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

        private static IReadOnlyList<DeucarianEditorStatusChip> BuildUpgradeChips(RunUpgradeAuthoringState state, GameContentAuthoringValidationResult validation)
        {
            RunUpgradeEffectAuthoringState effect = PrimaryEffect(state);
            return new[]
            {
                new DeucarianEditorStatusChip(GetTargetTypeLabel(effect), DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(GetModifierLabel(effect.ModifierType), DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(validation != null && validation.ErrorCount > 0 ? "Blocked" : validation != null && validation.WarningCount > 0 ? "Warnings" : "Ready", validation != null && validation.ErrorCount > 0 ? DeucarianEditorStatus.Error : validation != null && validation.WarningCount > 0 ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(Math.Max(1, state.MaxRank).ToString(CultureInfo.InvariantCulture) + " ranks", state.MaxRank > 0 ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error),
                new DeucarianEditorStatusChip(effect.Attack != null || effect.Weapon != null || effect.Enemy != null || !string.IsNullOrWhiteSpace(effect.TargetIdOverride) ? "Target" : "NoTarget", effect.Attack != null || effect.Weapon != null || effect.Enemy != null || !string.IsNullOrWhiteSpace(effect.TargetIdOverride) ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error)
            };
        }

        private static void DrawSummaryRows(params GameContentAuthoringPreviewRow[] rows)
        {
            GameContentAuthoringProviderGUI.DrawSummaryRows((IReadOnlyList<GameContentAuthoringPreviewRow>)rows, true);
        }

        private static void DrawSummaryRows(IReadOnlyList<GameContentAuthoringPreviewRow> rows)
        {
            GameContentAuthoringProviderGUI.DrawSummaryRows(rows, true);
        }

        private static void DrawSummaryRows(IReadOnlyList<GameContentAuthoringPreviewTimelineItem> items)
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

        private static string BuildHumanSummary(RunUpgradeAuthoringState state)
        {
            RunUpgradeEffectAuthoringState effect = PrimaryEffect(state);
            return GetTargetTypeLabel(effect) + ", " + GetModifierLabel(effect.ModifierType) + ", "
                + Math.Max(1, state.MaxRank).ToString(CultureInfo.InvariantCulture) + " rank(s)";
        }

        private static string BuildAdvancedReport(GameContentLibraryItem item, RunUpgradeAuthoringState state)
        {
            RunUpgradeEffectAuthoringState effect = PrimaryEffect(state);
            return "Upgrade: " + state.DisplayName + Environment.NewLine
                + "ID: " + state.UpgradeId + Environment.NewLine
                + "Path: " + (item == null ? "(draft)" : item.Path) + Environment.NewLine
                + "Target: " + RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetTargetId() + Environment.NewLine
                + "Effect: " + RunUpgradeDefinitionAssetCreator.ToRecipe(effect).GetEffectId() + Environment.NewLine
                + "Ranks: " + state.MaxRank.ToString(CultureInfo.InvariantCulture);
        }

        private static string AssetKey(UnityEngine.Object asset)
        {
            if (asset == null)
                return string.Empty;
            string path = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrWhiteSpace(path) ? asset.GetInstanceID().ToString(CultureInfo.InvariantCulture) : path;
        }

        private static GameContentAuthoringPreviewRow Row(string label, string value)
        {
            return new GameContentAuthoringPreviewRow(label, value);
        }
    }

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
                new DeucarianEditorStatusChip(RunUpgradeProviderV2View.GetTargetTypeLabel(effect), hasTarget ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error),
                new DeucarianEditorStatusChip(RunUpgradeProviderV2View.GetModifierLabel(effect.ModifierType), DeucarianEditorStatus.Info),
                new DeucarianEditorStatusChip(previewState == null || previewState.PreviewMuted ? "Muted" : "Audio", previewState == null || previewState.PreviewMuted ? DeucarianEditorStatus.Disabled : DeucarianEditorStatus.Success),
                new DeucarianEditorStatusChip(Math.Max(1, state.MaxRank).ToString(CultureInfo.InvariantCulture) + " ranks", state.MaxRank > 0 ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Error)
            };
        }
    }

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
            ModifierLabel = primary == null ? "Custom" : RunUpgradeProviderV2View.GetModifierLabel(primary.ModifierType);
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
            return RunUpgradeProviderV2View.GetModifierLabel(modifierType);
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
            return RunUpgradeProviderV2View.GetTargetTypeLabel(state);
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
