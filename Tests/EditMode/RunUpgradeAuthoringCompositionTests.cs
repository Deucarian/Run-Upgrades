using System.Globalization;
using Deucarian.RunUpgrades.Authoring;
using Deucarian.RunUpgrades.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.RunUpgrades.Tests
{
    public sealed class RunUpgradeAuthoringCompositionTests
    {
        [Test]
        public void MappingCopiesEffectsAndEconomyWithoutSharingMutableDraftState()
        {
            var asset = RunUpgradeDefinitionAsset.CreateTransient(
                "upgrade.composition", "Composition", RunUpgradeRarity.Common, 7, 3,
                new[]
                {
                    new RunUpgradeEffectRecipe(RunUpgradeAuthoringTargetKind.AttackDamage,
                        RunUpgradeModifierType.Additive, 1.5, targetIdOverride: "attack.target"),
                    new RunUpgradeEffectRecipe(RunUpgradeAuthoringTargetKind.Range,
                        RunUpgradeModifierType.Multiplicative, 1.25, targetIdOverride: "weapon.target")
                }, new[] { 10, 20, 30 }, prerequisites: new[] { "upgrade.before" },
                exclusions: new[] { "upgrade.excluded" });
            try
            {
                var first = RunUpgradeAuthoringDraft.FromUpgradeAsset(asset);
                var second = RunUpgradeAuthoringDraft.FromUpgradeAsset(asset);
                Assert.That(first.CostsCsv, Is.EqualTo("10, 20, 30"));
                Assert.That(first.PrerequisitesCsv, Is.EqualTo("upgrade.before"));
                Assert.That(first.ExclusionsCsv, Is.EqualTo("upgrade.excluded"));
                Assert.That(first.Effects[1].TargetIdOverride, Is.EqualTo("weapon.target"));
                first.Effects[0].Amount = 100;
                first.Effects.RemoveAt(1);
                Assert.That(second.Effects.Count, Is.EqualTo(2));
                Assert.That(second.Effects[0].Amount, Is.EqualTo(1.5));
                Assert.That(asset.Effects.Effects[0].Amount, Is.EqualTo(1.5));
            }
            finally
            {
                Object.DestroyImmediate(asset.Effects);
                Object.DestroyImmediate(asset.Economy);
                Object.DestroyImmediate(asset);
            }
        }

        [TestCase(RunUpgradeModifierType.Additive, 2, 3, "base -> base + 6")]
        [TestCase(RunUpgradeModifierType.Multiplicative, 2, 3, "base -> base x 8")]
        [TestCase(RunUpgradeModifierType.SetValue, 2, 3, "base -> 2")]
        public void RankPreviewKeepsDistinctModifierSemantics(RunUpgradeModifierType modifier, double amount, int rank, string expected)
        {
            var effect = new RunUpgradeEffectAuthoringState { ModifierType = modifier, Amount = amount };
            Assert.That(RunUpgradeAuthoringEffectSummary.BuildBeforeAfterSummary(effect, rank), Is.EqualTo(expected));
            Assert.That(effect.Amount, Is.EqualTo(amount));
        }

        [Test]
        public void FingerprintDetectsNestedEffectChangesAndIgnoresCurrentCulture()
        {
            var draft = new RunUpgradeAuthoringState();
            draft.EnsureEffects();
            draft.Effects[0].Amount = 1.25;
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                string saved = RunUpgradeAuthoringDraft.BuildStateFingerprint(draft);
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("nl-NL");
                Assert.That(RunUpgradeAuthoringDraft.BuildStateFingerprint(draft), Is.EqualTo(saved));
                draft.Effects[0].TargetIdOverride = "different.target";
                Assert.That(RunUpgradeAuthoringDraft.BuildStateFingerprint(draft), Is.Not.EqualTo(saved));
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Test]
        public void SessionResetClearsSelectedRankWithoutResettingAnotherSession()
        {
            var first = new RunUpgradeProviderV2State { SelectedRank = 3 };
            var second = new RunUpgradeProviderV2State { SelectedRank = 2 };
            first.ResetProviderSession();
            first.ResetProviderSession();
            Assert.That(first.SelectedRank, Is.EqualTo(1));
            Assert.That(second.SelectedRank, Is.EqualTo(2));
        }
    }
}
