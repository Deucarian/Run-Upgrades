using System;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.RunUpgrades.Editor;
using NUnit.Framework;

namespace Deucarian.RunUpgrades.Tests
{
    public sealed class PackAwareUpgradeLensTests
    {
        [Test]
        public void Lens_MatchesEveryUpgradeSubtypeWithoutDuplicatingIdentity()
        {
            var provider = new RunUpgradeAuthoringProvider();
            GameContentRecordDescriptor passive = Record(
                "upgrade.passive",
                GameContentRecordCapabilities.Upgrade,
                GameContentRecordCapabilities.Passive);
            GameContentRecordDescriptor evolution = Record(
                "upgrade.evolution",
                GameContentRecordCapabilities.Upgrade,
                GameContentRecordCapabilities.Evolution);

            Assert.That(provider.ProviderId, Is.EqualTo("com.deucarian.run-upgrades.upgrade"));
            Assert.That(provider.Lens.Matches(passive), Is.True);
            Assert.That(provider.Lens.Matches(evolution), Is.True);
            Assert.That(passive.CanonicalKey.SourceRecordId, Is.EqualTo("upgrade.passive"));
            Assert.That(evolution.CanonicalKey.SourceRecordId, Is.EqualTo("upgrade.evolution"));
        }

        [Test]
        public void Projection_PreservesEconomyEffectsReferencesAndComparisonMetadata()
        {
            GameContentRecordDescriptor record = Record("upgrade.damage", GameContentRecordCapabilities.Upgrade);
            var projection = new UpgradeContentRecordProjection(
                record,
                "Increase Arc damage.",
                "Weapon",
                "Rare",
                1.25,
                8,
                "DamageMultiplier",
                0.15,
                "weapon.arc",
                "Requires Arc rank 3",
                "Arcane class",
                "weapon.arc, evolution.arc",
                "+15% per rank");

            Assert.That(projection.Weight, Is.EqualTo(1.25));
            Assert.That(projection.MaxRank, Is.EqualTo(8));
            Assert.That(projection.EffectAmount, Is.EqualTo(0.15));
            Assert.That(projection.ReferenceSummary, Does.Contain("evolution.arc"));
            Assert.That(projection.ComparisonSummary, Is.EqualTo("+15% per rank"));
        }

        [TestCase((int)UpgradePackAwareSubtypeFilter.WeaponUpgrade, "weapon")]
        [TestCase((int)UpgradePackAwareSubtypeFilter.Passive, "passive")]
        [TestCase((int)UpgradePackAwareSubtypeFilter.PickupMagnet, "pickup")]
        [TestCase((int)UpgradePackAwareSubtypeFilter.Mutation, "mutation")]
        [TestCase((int)UpgradePackAwareSubtypeFilter.Evolution, "evolution")]
        [TestCase((int)UpgradePackAwareSubtypeFilter.MetaUpgrade, "meta")]
        public void SemanticSubtypeFilter_MatchesOnlyItsCapability(
            int filterValue,
            string id)
        {
            var filter = (UpgradePackAwareSubtypeFilter)filterValue;
            GameContentRecordCapability capability;
            switch (filter)
            {
                case UpgradePackAwareSubtypeFilter.WeaponUpgrade:
                    capability = GameContentRecordCapabilities.WeaponUpgrade;
                    break;
                case UpgradePackAwareSubtypeFilter.Passive:
                    capability = GameContentRecordCapabilities.Passive;
                    break;
                case UpgradePackAwareSubtypeFilter.PickupMagnet:
                    capability = GameContentRecordCapabilities.PickupMagnet;
                    break;
                case UpgradePackAwareSubtypeFilter.Mutation:
                    capability = GameContentRecordCapabilities.Mutation;
                    break;
                case UpgradePackAwareSubtypeFilter.Evolution:
                    capability = GameContentRecordCapabilities.Evolution;
                    break;
                default:
                    capability = GameContentRecordCapabilities.MetaUpgrade;
                    break;
            }
            GameContentRecordDescriptor matching = Record(
                "upgrade." + id,
                GameContentRecordCapabilities.Upgrade,
                capability);
            GameContentRecordDescriptor other = Record(
                "upgrade.other",
                GameContentRecordCapabilities.Upgrade);

            Assert.That(UpgradePackAwareLensView.MatchesSubtype(matching, filter), Is.True);
            Assert.That(UpgradePackAwareLensView.MatchesSubtype(other, filter), Is.False);
            Assert.That(matching.CanonicalKey.SourceRecordId, Is.EqualTo("upgrade." + id));
        }

        private static GameContentRecordDescriptor Record(
            string id,
            params GameContentRecordCapability[] capabilities)
        {
            return new GameContentRecordDescriptor(
                "test-pack::upgrades::" + id,
                id,
                "upgrades",
                null,
                id,
                string.Empty,
                string.Empty,
                Array.Empty<GameContentMetadataDescriptor>(),
                null,
                "Assets/upgrades.json",
                "upgrades[0]",
                Array.Empty<GameContentRecordReferenceDescriptor>(),
                Array.Empty<GameContentRecordReferenceDescriptor>(),
                GameContentAuthoringValidationResult.Valid,
                0,
                null,
                string.Empty,
                new GameContentRecordKey("com.deucarian.tests", "test-pack", id, "upgrades", "upgrades[0]"),
                capabilities);
        }
    }
}
