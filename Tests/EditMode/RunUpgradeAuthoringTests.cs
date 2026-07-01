using System;
using System.Collections.Generic;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.RunUpgrades.Editor;
using Deucarian.RunUpgrades.Authoring;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.RunUpgrades.Tests
{
    public sealed class RunUpgradeAuthoringTests
    {
        [Test]
        public void RunUpgradeDefinitionAssetConvertsToRuntimeDefinition()
        {
            RunUpgradeDefinitionAsset asset = RunUpgradeDefinitionAsset.CreateTransient(
                "upgrade.authoring.damage",
                "Damage Upgrade",
                RunUpgradeRarity.Common,
                5,
                3,
                new[]
                {
                    new RunUpgradeEffectRecipe(
                        RunUpgradeAuthoringTargetKind.AttackDamage,
                        RunUpgradeModifierType.Additive,
                        1.5,
                        targetIdOverride: "weapon.authoring")
                },
                new[] { 10, 20, 35 });

            try
            {
                RunUpgradeDefinitionValidationReport report = RunUpgradeDefinitionValidator.Validate(asset);
                RunUpgradeDefinition runtime = asset.ToRuntimeDefinition();

                Assert.IsTrue(report.IsValid);
                Assert.AreEqual("upgrade.authoring.damage", runtime.Id.Value);
                Assert.AreEqual(3, runtime.MaxRank);
                Assert.AreEqual("template.direct.damage_bonus", runtime.Effects[0].EffectId.Value);
                Assert.AreEqual("weapon.authoring", runtime.Effects[0].TargetId.Value);
            }
            finally
            {
                Object.DestroyImmediate(asset.Effects);
                Object.DestroyImmediate(asset.Economy);
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void RunUpgradeValidationRejectsInvalidModifierAndMissingTarget()
        {
            RunUpgradeDefinitionAsset asset = RunUpgradeDefinitionAsset.CreateTransient(
                "upgrade.authoring.invalid",
                "Invalid Upgrade",
                RunUpgradeRarity.Common,
                1,
                1,
                new[]
                {
                    new RunUpgradeEffectRecipe(
                        RunUpgradeAuthoringTargetKind.ProjectileSpeed,
                        RunUpgradeModifierType.Multiplicative,
                        0)
                });

            try
            {
                RunUpgradeDefinitionValidationReport report = RunUpgradeDefinitionValidator.Validate(asset);

                Assert.IsFalse(report.IsValid);
                Assert.That(FindIssue(report, "Effects[0].Modifier"), Is.True);
                Assert.That(FindIssue(report, "Effects[0].Target"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(asset.Effects);
                Object.DestroyImmediate(asset.Economy);
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void RunUpgradeValidationRejectsNegativeMultiplicativeModifier()
        {
            RunUpgradeDefinitionAsset asset = RunUpgradeDefinitionAsset.CreateTransient(
                "upgrade.authoring.bad-multiplier",
                "Bad Multiplier",
                RunUpgradeRarity.Common,
                1,
                1,
                new[]
                {
                    new RunUpgradeEffectRecipe(
                        RunUpgradeAuthoringTargetKind.ProjectileSpeed,
                        RunUpgradeModifierType.Multiplicative,
                        -0.5,
                        targetIdOverride: "projectile.authoring")
                });

            try
            {
                RunUpgradeDefinitionValidationReport report = RunUpgradeDefinitionValidator.Validate(asset);

                Assert.IsFalse(report.IsValid);
                Assert.That(FindIssue(report, "Effects[0].Modifier"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(asset.Effects);
                Object.DestroyImmediate(asset.Economy);
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void RunUpgradeValidationRejectsInvalidRankAndCostData()
        {
            RunUpgradeDefinitionAsset asset = RunUpgradeDefinitionAsset.CreateTransient(
                "upgrade.authoring.bad-economy",
                "Bad Economy",
                RunUpgradeRarity.Common,
                1,
                0,
                new[]
                {
                    new RunUpgradeEffectRecipe(
                        RunUpgradeAuthoringTargetKind.AttackDamage,
                        RunUpgradeModifierType.Additive,
                        1,
                        targetIdOverride: "weapon.authoring")
                },
                new[] { -1 });

            try
            {
                RunUpgradeDefinitionValidationReport report = RunUpgradeDefinitionValidator.Validate(asset);

                Assert.IsFalse(report.IsValid);
                Assert.That(FindIssue(report, "Economy.MaxRank"), Is.True);
                Assert.That(FindIssue(report, "Economy.Costs[0]"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(asset.Effects);
                Object.DestroyImmediate(asset.Economy);
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void RunUpgradePreviewSummaryHandlesMissingTargets()
        {
            var state = new RunUpgradeAuthoringState();
            state.EnsureEffects();
            state.Effects[0].Attack = null;
            state.Effects[0].Weapon = null;
            state.Effects[0].Enemy = null;
            state.Effects[0].TargetIdOverride = string.Empty;

            Assert.DoesNotThrow(() =>
            {
                Assert.That(RunUpgradeGameContentPreviewSummaries.BuildAffectedRows(state).Count, Is.GreaterThan(0));
                Assert.That(RunUpgradeGameContentPreviewSummaries.BuildRankTimeline(state).Count, Is.GreaterThan(0));
                Assert.That(RunUpgradeGameContentPreviewSummaries.BuildCompatibilityRows(state).Count, Is.GreaterThan(0));
                Assert.That(RunUpgradeGameContentPreviewSummaries.BuildWarnings(state).Count, Is.GreaterThan(0));
                StringAssert.Contains("effect(s)", RunUpgradeGameContentPreviewSummaries.PreviewRankImpact(state));
            });
        }

        [Test]
        public void RunUpgradeAuthoringProviderRegistersWithSharedWindow()
        {
            Assert.IsTrue(GameContentAuthoringProviderRegistry.IsProviderRegistered("com.deucarian.run-upgrades.upgrade"));
        }

        [Test]
        public void RunUpgradeAuthoringProviderUsesCustomV2Surface()
        {
            var provider = new RunUpgradeAuthoringProvider();

            Assert.That(provider, Is.InstanceOf<IGameContentAuthoringSurfaceProvider>());
            Assert.That(provider.ProviderId, Is.EqualTo("com.deucarian.run-upgrades.upgrade"));
            Assert.That(RunUpgradeProviderV2PreviewModel.ExposesRedundantSelectButton, Is.False);
        }

        [Test]
        public void RunUpgradeProviderV2ListModel_ClassifiesTargetAndModifierTypes()
        {
            Assert.That(RunUpgradeProviderV2ListItem.GetTargetTypeLabelForTests(new RunUpgradeEffectRecipe(RunUpgradeAuthoringTargetKind.AttackDamage, RunUpgradeModifierType.Additive, 1d, targetIdOverride: "attack.test")), Is.EqualTo("Attack"));
            Assert.That(RunUpgradeProviderV2ListItem.GetTargetTypeLabelForTests(new RunUpgradeEffectRecipe(RunUpgradeAuthoringTargetKind.WeaponStat, RunUpgradeModifierType.SetValue, 2d, targetIdOverride: "weapon.test")), Is.EqualTo("Weapon"));
            Assert.That(RunUpgradeProviderV2ListItem.GetTargetTypeLabelForTests(new RunUpgradeEffectRecipe(RunUpgradeAuthoringTargetKind.EnemyReward, RunUpgradeModifierType.Multiplicative, 1.1d, targetIdOverride: "economy.test")), Is.EqualTo("Economy"));
            Assert.That(RunUpgradeProviderV2ListItem.GetTargetTypeLabelForTests(new RunUpgradeEffectRecipe(RunUpgradeAuthoringTargetKind.StatusEffectDuration, RunUpgradeModifierType.Additive, 5d, targetIdOverride: "status.test")), Is.EqualTo("Status"));
            Assert.That(RunUpgradeProviderV2ListItem.GetModifierLabelForTests(RunUpgradeModifierType.Additive), Is.EqualTo("Add"));
            Assert.That(RunUpgradeProviderV2ListItem.GetModifierLabelForTests(RunUpgradeModifierType.Multiplicative), Is.EqualTo("Multiply"));
            Assert.That(RunUpgradeProviderV2ListItem.GetModifierLabelForTests(RunUpgradeModifierType.SetValue), Is.EqualTo("Set"));
        }

        [Test]
        public void RunUpgradeProviderV2Preview_DraftAndUnsavedExposeCompactChips()
        {
            var state = new RunUpgradeAuthoringState();
            state.EnsureEffects();
            state.Effects[0].TargetIdOverride = string.Empty;
            var previewState = new RunUpgradeProviderV2State
            {
                Creating = true,
                PreviewMuted = true,
                PreviewRenderMode = GameContentAuthoringActionPreviewRenderMode.Game
            };

            Assert.That(RunUpgradeProviderV2PreviewModel.GetScopeLabel(true, false), Is.EqualTo("Draft"));
            Assert.That(RunUpgradeProviderV2PreviewModel.GetScopeLabel(false, true), Is.EqualTo("Unsaved"));
            AssertChip(RunUpgradeProviderV2PreviewModel.BuildChips(state, previewState), "Game", DeucarianEditorStatus.Info);
            AssertChip(RunUpgradeProviderV2PreviewModel.BuildChips(state, previewState), "Attack", DeucarianEditorStatus.Error);
            AssertChip(RunUpgradeProviderV2PreviewModel.BuildChips(state, previewState), "Muted", DeucarianEditorStatus.Disabled);
        }

        [Test]
        public void RunUpgradeProviderV2Preview_ChangingDraftFieldsUpdatesFingerprintAndImpact()
        {
            var state = new RunUpgradeAuthoringState
            {
                UpgradeId = "upgrade.v2.draft",
                DisplayName = "Draft Upgrade",
                MaxRank = 3,
                CostsCsv = "5, 10, 15"
            };
            state.EnsureEffects();
            state.Effects[0].TargetKind = RunUpgradeAuthoringTargetKind.AttackDamage;
            state.Effects[0].ModifierType = RunUpgradeModifierType.Additive;
            state.Effects[0].Amount = 1d;
            state.Effects[0].TargetIdOverride = "attack.draft";

            string before = RunUpgradeProviderV2View.BuildStateFingerprint(state);
            string impactBefore = RunUpgradeProviderV2View.BuildBeforeAfterSummary(state.Effects[0], 2);
            state.Effects[0].ModifierType = RunUpgradeModifierType.Multiplicative;
            state.Effects[0].Amount = 1.5d;
            state.MaxRank = 4;
            string after = RunUpgradeProviderV2View.BuildStateFingerprint(state);
            string impactAfter = RunUpgradeProviderV2View.BuildBeforeAfterSummary(state.Effects[0], 2);

            Assert.That(after, Is.Not.EqualTo(before));
            Assert.That(impactBefore, Does.Contain("+ 2"));
            Assert.That(impactAfter, Does.Contain("x 2.25"));
        }

        [Test]
        public void RunUpgradeProviderV2State_CreateAndProviderSelectionClearTransientPreviewState()
        {
            var state = new RunUpgradeProviderV2State
            {
                EditingState = new RunUpgradeAuthoringState { DisplayName = "Dirty Edit" },
                ActivePreviewKey = "selected.upgrade"
            };

            state.BeginCreate();

            Assert.That(state.Creating, Is.True);
            Assert.That(state.WizardStep, Is.EqualTo(0));
            Assert.That(state.EditingState, Is.Null);
            Assert.That(state.PreviewStatus, Is.EqualTo("Previewing draft upgrade"));

            state.ResetProviderSession();

            Assert.That(state.Creating, Is.False);
            Assert.That(state.ActivePreviewKey, Is.Empty);
            Assert.That(state.EditingState, Is.Null);
            Assert.That(state.PreviewStatus, Is.EqualTo("Preview idle"));
        }

        [Test]
        public void RunUpgradeDefinitionAssetCreator_UpdateExistingAssetSavesSelectedSections()
        {
            const string folder = "Assets/RunUpgradeAuthoringUpdateTests";
            AssetDatabase.DeleteAsset(folder);
            AssetDatabase.CreateFolder("Assets", "RunUpgradeAuthoringUpdateTests");

            RunUpgradeDefinitionAsset root = ScriptableObject.CreateInstance<RunUpgradeDefinitionAsset>();
            RunUpgradeEconomyDefinitionAsset economy = ScriptableObject.CreateInstance<RunUpgradeEconomyDefinitionAsset>();
            RunUpgradeEffectsDefinitionAsset effects = ScriptableObject.CreateInstance<RunUpgradeEffectsDefinitionAsset>();
            economy.Configure(RunUpgradeRarity.Common, 5, 2, new[] { 10, 20 });
            effects.Configure(
                new[] { new RunUpgradeEffectRecipe(RunUpgradeAuthoringTargetKind.AttackDamage, RunUpgradeModifierType.Additive, 1d, targetIdOverride: "attack.before") },
                Array.Empty<string>(),
                Array.Empty<string>());
            root.Configure("upgrade.v2.save", "Before Upgrade", null, "Before", new[] { "before" }, economy, effects);
            AssetDatabase.CreateAsset(root, folder + "/upgrade.v2.save_RunUpgradeDefinition.asset");
            GameContentAuthoringEditorAssets.AddSubAsset(economy, root, "upgrade.v2.save_Economy");
            GameContentAuthoringEditorAssets.AddSubAsset(effects, root, "upgrade.v2.save_Effects");
            AssetDatabase.SaveAssets();

            try
            {
                var edit = new RunUpgradeAuthoringState
                {
                    UpgradeId = "upgrade.v2.save",
                    DisplayName = "Saved Upgrade",
                    Description = "After",
                    TagsCsv = "saved, v2",
                    Rarity = RunUpgradeRarity.Rare,
                    Weight = 7,
                    MaxRank = 3,
                    CostsCsv = "5, 15, 35",
                    PrerequisitesCsv = "upgrade.required",
                    ExclusionsCsv = "upgrade.excluded"
                };
                edit.EnsureEffects();
                edit.Effects[0].TargetKind = RunUpgradeAuthoringTargetKind.Range;
                edit.Effects[0].ModifierType = RunUpgradeModifierType.Multiplicative;
                edit.Effects[0].Amount = 1.25d;
                edit.Effects[0].TargetIdOverride = "weapon.after";

                GameContentCreationResult result = RunUpgradeDefinitionAssetCreator.UpdateExistingAsset(root, edit);

                Assert.That(result.Succeeded, Is.True, result.Message);
                Assert.That(root.DisplayName, Is.EqualTo("Saved Upgrade"));
                Assert.That(root.Description, Is.EqualTo("After"));
                Assert.That(root.Tags, Does.Contain("saved"));
                Assert.That(root.Economy.Rarity, Is.EqualTo(RunUpgradeRarity.Rare));
                Assert.That(root.Economy.Weight, Is.EqualTo(7));
                Assert.That(root.Economy.MaxRank, Is.EqualTo(3));
                Assert.That(root.Economy.Costs, Is.EqualTo(new[] { 5, 15, 35 }));
                Assert.That(root.Effects.Prerequisites, Does.Contain("upgrade.required"));
                Assert.That(root.Effects.Exclusions, Does.Contain("upgrade.excluded"));
                Assert.That(root.Effects.Effects[0].TargetKind, Is.EqualTo(RunUpgradeAuthoringTargetKind.Range));
                Assert.That(root.Effects.Effects[0].ModifierType, Is.EqualTo(RunUpgradeModifierType.Multiplicative));
                Assert.That(root.Effects.Effects[0].TargetIdOverride, Is.EqualTo("weapon.after"));
            }
            finally
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }

        [Test]
        public void RunUpgradeProviderV2RevertReloadsSavedUpgradeData()
        {
            RunUpgradeDefinitionAsset asset = RunUpgradeDefinitionAsset.CreateTransient(
                "upgrade.v2.revert",
                "Saved Data",
                RunUpgradeRarity.Common,
                3,
                2,
                new[] { new RunUpgradeEffectRecipe(RunUpgradeAuthoringTargetKind.AttackDamage, RunUpgradeModifierType.Additive, 2d, targetIdOverride: "attack.saved") },
                new[] { 10, 20 },
                "Saved description",
                new[] { "saved" });

            try
            {
                RunUpgradeAuthoringState edit = RunUpgradeProviderV2View.FromUpgradeAsset(asset);
                edit.DisplayName = "Unsaved Data";
                edit.Effects[0].Amount = 99d;
                RunUpgradeAuthoringState reverted = RunUpgradeProviderV2View.FromUpgradeAsset(asset);

                Assert.That(reverted.DisplayName, Is.EqualTo("Saved Data"));
                Assert.That(reverted.Effects[0].Amount, Is.EqualTo(2d));
                Assert.That(RunUpgradeProviderV2View.BuildStateFingerprint(reverted), Is.Not.EqualTo(RunUpgradeProviderV2View.BuildStateFingerprint(edit)));
            }
            finally
            {
                Object.DestroyImmediate(asset.Effects);
                Object.DestroyImmediate(asset.Economy);
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void RunUpgradeDefinitionAssetCreator_UpdateValidationBlocksMissingTargetAndInvalidEconomy()
        {
            RunUpgradeDefinitionAsset root = ScriptableObject.CreateInstance<RunUpgradeDefinitionAsset>();
            var edit = new RunUpgradeAuthoringState
            {
                UpgradeId = "upgrade.v2.invalid",
                DisplayName = "Invalid Upgrade",
                Weight = 0,
                MaxRank = 0,
                CostsCsv = "bad"
            };
            edit.EnsureEffects();
            edit.Effects[0].TargetIdOverride = string.Empty;

            try
            {
                GameContentAuthoringValidationResult validation = RunUpgradeDefinitionAssetCreator.ValidateForUpdate(edit, root);

                Assert.That(validation.IsValid, Is.False);
                Assert.That(FindIssue(validation, "Effects[0].Target", GameContentAuthoringValidationSeverity.Error), Is.True);
                Assert.That(FindIssue(validation, "Economy.Weight", GameContentAuthoringValidationSeverity.Error), Is.True);
                Assert.That(FindIssue(validation, "Economy.MaxRank", GameContentAuthoringValidationSeverity.Error), Is.True);
                Assert.That(FindIssue(validation, "Economy.Costs", GameContentAuthoringValidationSeverity.Error), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RunUpgradeDuplicateIdsAreDetectedInAssets()
        {
            const string folder = "Assets/RunUpgradeAuthoringDuplicateTests";
            const string id = "upgrade.authoring.duplicate";
            AssetDatabase.DeleteAsset(folder);
            AssetDatabase.CreateFolder("Assets", "RunUpgradeAuthoringDuplicateTests");
            RunUpgradeDefinitionAsset asset = ScriptableObject.CreateInstance<RunUpgradeDefinitionAsset>();
            asset.Configure(id, "Duplicate Upgrade", null, string.Empty, Array.Empty<string>(), null, null);
            AssetDatabase.CreateAsset(asset, folder + "/DuplicateUpgrade.asset");
            AssetDatabase.SaveAssets();

            try
            {
                Assert.IsTrue(GameContentAuthoringEditorAssets.HasDuplicateId<RunUpgradeDefinitionAsset>(id, candidate => candidate.Id));
            }
            finally
            {
                AssetDatabase.DeleteAsset(folder);
            }
        }

        private static bool FindIssue(RunUpgradeDefinitionValidationReport report, string path)
        {
            for (int i = 0; i < report.Issues.Count; i++)
                if (report.Issues[i].Path == path)
                    return true;
            return false;
        }

        private static bool FindIssue(GameContentAuthoringValidationResult report, string path, GameContentAuthoringValidationSeverity severity)
        {
            for (int i = 0; i < report.Issues.Count; i++)
                if (report.Issues[i].Path == path && report.Issues[i].Severity == severity)
                    return true;
            return false;
        }

        private static void AssertChip(IReadOnlyList<DeucarianEditorStatusChip> chips, string label, DeucarianEditorStatus status)
        {
            for (int i = 0; i < chips.Count; i++)
            {
                if (chips[i].Label == label)
                {
                    Assert.That(chips[i].Status, Is.EqualTo(status));
                    return;
                }
            }

            Assert.Fail("Expected chip " + label + ".");
        }
    }
}
