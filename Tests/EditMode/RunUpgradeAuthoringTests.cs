using System;
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
    }
}
