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
    internal static class RunUpgradeAuthoringWizard
    {
        private static readonly string[] WizardSteps =
        {
            "Identity",
            "Target",
            "Effect",
            "Ranks / Cost",
            "Presentation",
            "Review"
        };

        internal static void DrawCreateWizard(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState draft, RunUpgradeProviderV2State state)
        {
            draft.EnsureEffects();
            GameContentAuthoringValidationResult validation = RunUpgradeAuthoringSession.ValidateDraft(draft);

            RunUpgradeAuthoringFields.DrawHeader("New Upgrade", draft.UpgradeId, RunUpgradeAuthoringSummary.BuildUpgradeChips(draft, validation));
            GameContentAuthoringCommand command = GameContentAuthoringCommandBar.Draw(GameContentAuthoringWorkbenchMode.Create, validation.IsValid, true, "Create");
            if (command == GameContentAuthoringCommand.Create)
            {
                RunUpgradeAuthoringSession.Create(context, draft, state);
            }

            state.WizardStep = DeucarianEditorWizardHeader.Draw(state.WizardStep, WizardSteps);
            GUILayout.Space(DeucarianEditorSpacing.Small);
            switch (Mathf.Clamp(state.WizardStep, 0, WizardSteps.Length - 1))
            {
                case 0:
                    RunUpgradeAuthoringFields.DrawOverview(context, draft, null, true);
                    break;
                case 1:
                    RunUpgradeAuthoringFields.DrawTarget(context, draft, null);
                    break;
                case 2:
                    RunUpgradeAuthoringFields.DrawEffect(context, draft);
                    break;
                case 3:
                    RunUpgradeAuthoringFields.DrawRanksCost(context, draft, state);
                    break;
                case 4:
                    RunUpgradeAuthoringFields.DrawPresentation(context, draft);
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

        private static void DrawReview(GameContentAuthoringSurfaceContext context, RunUpgradeAuthoringState state, GameContentAuthoringValidationResult validation)
        {
            IReadOnlyList<string> lines = RunUpgradeDefinitionAssetCreator.GetPreviewLines(state);
            for (int i = 0; i < lines.Count; i++)
                EditorGUILayout.LabelField(lines[i], DeucarianEditorStyles.MutedLabel);

            RunUpgradeAuthoringFields.DrawSummaryRows(
                RunUpgradeAuthoringSummary.Row("Readiness", new GameContentAuthoringValidationSummary(validation).CountLabel),
                RunUpgradeAuthoringSummary.Row("Target", RunUpgradeAuthoringEffectSummary.GetTargetAssetLabel(RunUpgradeAuthoringEffectSummary.PrimaryEffect(state))),
                RunUpgradeAuthoringSummary.Row("Effect", RunUpgradeAuthoringEffectSummary.BuildModifierBehavior(RunUpgradeAuthoringEffectSummary.PrimaryEffect(state))),
                RunUpgradeAuthoringSummary.Row("Ranks", Math.Max(1, state.MaxRank).ToString(CultureInfo.InvariantCulture)));
        }
    }
}
