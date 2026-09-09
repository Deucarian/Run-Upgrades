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
    internal static class RunUpgradeAuthoringSession
    {
        internal static void EnsureDefaultMode(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state, IReadOnlyList<RunUpgradeProviderV2ListItem> items)
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

        internal static void EnsureEditingState(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state)
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

            state.EditingState = RunUpgradeAuthoringDraft.FromUpgradeAsset(selected);
            string fingerprint = RunUpgradeAuthoringDraft.BuildStateFingerprint(state.EditingState);
            state.EditingContext = new GameContentAuthoringObjectEditorContext(context.SelectedItem, fingerprint);
            state.SelectedRank = 1;
            state.LastEditResult = null;
        }

        internal static void HandleEditCommand(GameContentAuthoringSurfaceContext context, RunUpgradeProviderV2State state, RunUpgradeDefinitionAsset asset, GameContentAuthoringCommand command)
        {
            if (command == GameContentAuthoringCommand.Revert)
            {
                state.EditingState = RunUpgradeAuthoringDraft.FromUpgradeAsset(asset);
                string fingerprint = RunUpgradeAuthoringDraft.BuildStateFingerprint(state.EditingState);
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
                state.EditingState = RunUpgradeAuthoringDraft.FromUpgradeAsset(asset);
                string fingerprint = RunUpgradeAuthoringDraft.BuildStateFingerprint(state.EditingState);
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

        internal static void Create(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState draft, RunUpgradeProviderV2State state)
        {
            GameContentCreationResult result = RunUpgradeDefinitionAssetCreator.CreateAssets(draft);
            context.Authoring.SetCreationResult(result);
            if (result != null && result.Succeeded)
            {
                state.Creating = false;
                context.RefreshLibrary();
            }
        }

        internal static GameContentAuthoringValidationResult ValidateDraft(RunUpgradeAuthoringState draft)
        {
            RunUpgradeDefinitionAsset preview = RunUpgradeDefinitionAssetCreator.BuildTransient(draft);
            try
            {
                return RunUpgradeDefinitionAssetCreator.ValidateForCreation(draft, preview);
            }
            finally
            {
                RunUpgradeDefinitionAssetCreator.DestroyTransient(preview);
            }
        }
    }
}
