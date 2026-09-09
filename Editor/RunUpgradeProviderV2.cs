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
            RunUpgradeAuthoringSession.EnsureDefaultMode(context, state, items);
            RunUpgradeAuthoringSession.EnsureEditingState(context, state);
            RunUpgradeAuthoringPreview.TrackPreviewSource(context, state, previewController);

            GameContentAuthoringWorkbench.Draw(
                context,
                () => RunUpgradeAuthoringLibrary.DrawUpgradeList(context, state, items),
                () => DrawDetailOrWizard(context, draft, state),
                () => RunUpgradeAuthoringPreview.DrawPreviewLab(context, draft, state));
        }

        private static void DrawDetailOrWizard(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState draft, RunUpgradeProviderV2State state)
        {
            state.DetailScroll = EditorGUILayout.BeginScrollView(state.DetailScroll);
            if (state.Creating)
                RunUpgradeAuthoringWizard.DrawCreateWizard(context, draft, state);
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
            string fingerprint = RunUpgradeAuthoringDraft.BuildStateFingerprint(edit);
            GameContentAuthoringValidationResult validation = RunUpgradeDefinitionAssetCreator.ValidateForUpdate(edit, asset);
            state.EditingContext.Capture(fingerprint, validation);
            context.Authoring.SetValidation(validation);

            RunUpgradeAuthoringFields.DrawHeader(edit.DisplayName, edit.UpgradeId, RunUpgradeAuthoringSummary.BuildUpgradeChips(edit, validation));
            GameContentAuthoringCommand command = GameContentAuthoringCommandBar.Draw(
                GameContentAuthoringWorkbenchMode.Edit,
                validation.IsValid,
                state.EditingContext.IsDirty,
                "Save",
                state.LastEditResult == null ? state.EditingContext.StatusMessage : state.LastEditResult.Message);
            RunUpgradeAuthoringSession.HandleEditCommand(context, state, asset, command);

            state.DetailPage = DeucarianEditorSegmentedControl.DrawPageChips(state.DetailPage, DetailPages);
            GUILayout.Space(DeucarianEditorSpacing.Small);
            switch (Mathf.Clamp(state.DetailPage, 0, DetailPages.Length - 1))
            {
                case 0:
                    RunUpgradeAuthoringFields.DrawOverview(context, edit, context.SelectedItem, false);
                    break;
                case 1:
                    RunUpgradeAuthoringFields.DrawTarget(context, edit, context.SelectedItem);
                    break;
                case 2:
                    RunUpgradeAuthoringFields.DrawEffect(context, edit);
                    break;
                case 3:
                    RunUpgradeAuthoringFields.DrawRanksCost(context, edit, state);
                    break;
                case 4:
                    RunUpgradeAuthoringFields.DrawPresentation(context, edit);
                    break;
                case 5:
                    RunUpgradeAuthoringFields.DrawReferences(context, edit, context.SelectedItem);
                    break;
                default:
                    RunUpgradeAuthoringFields.DrawAdvanced(context, edit, context.SelectedItem, asset);
                    break;
            }

            GameContentAuthoringProviderGUI.DrawValidationIssues(
                validation,
                GameContentAuthoringValidationSummaryStyle.Counts,
                false);
        }

        public static RunUpgradeAuthoringState FromUpgradeAsset(RunUpgradeDefinitionAsset asset)
        {
            return RunUpgradeAuthoringDraft.FromUpgradeAsset(asset);
        }

        public static string BuildStateFingerprint(RunUpgradeAuthoringState state)
        {
            return RunUpgradeAuthoringDraft.BuildStateFingerprint(state);
        }

        public static string GetTargetTypeLabel(RunUpgradeEffectAuthoringState effect)
        {
            return RunUpgradeAuthoringEffectSummary.GetTargetTypeLabel(effect);
        }

        public static string GetModifierLabel(RunUpgradeModifierType modifierType)
        {
            return RunUpgradeAuthoringEffectSummary.GetModifierLabel(modifierType);
        }

        public static string BuildBeforeAfterSummary(RunUpgradeEffectAuthoringState effect, int rank)
        {
            return RunUpgradeAuthoringEffectSummary.BuildBeforeAfterSummary(effect, rank);
        }
    }
}
