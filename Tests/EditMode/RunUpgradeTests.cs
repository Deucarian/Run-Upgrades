using System;
using System.Linq;
using Deucarian.GameplayFoundation;
using NUnit.Framework;

namespace Deucarian.RunUpgrades.Tests
{
    public sealed class RunUpgradeTests
    {
        [Test]
        public void ValidUpgradeDefinition()
        {
            RunUpgradeDefinition upgrade = Upgrade("direct.damage", RunUpgradeRarity.Rare, 7, 3);
            Assert.AreEqual("direct.damage", upgrade.Id.Value);
            Assert.AreEqual(RunUpgradeRarity.Rare, upgrade.Rarity);
            Assert.AreEqual(7, upgrade.Weight);
            Assert.AreEqual(3, upgrade.MaxRank);
            Assert.AreEqual(1, upgrade.Effects.Count);
        }

        [Test]
        public void InvalidIdsAndDefinitionValuesAreRejected()
        {
            Assert.Throws<ArgumentException>(() => new RunUpgradeId("Bad Id"));
            Assert.Throws<ArgumentException>(() => new RunUpgradeEffectDescriptor(default, Target("x"), 1));
            Assert.Throws<ArgumentException>(() => new RunUpgradeEffectDescriptor(Effect("x"), default, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RunUpgradeEffectDescriptor(Effect("x"), Target("x"), double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RunUpgradeDefinition(Id("x"), RunUpgradeRarity.Common, 0, 1, Effects("x")));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RunUpgradeDefinition(Id("x"), RunUpgradeRarity.Common, 1, 0, Effects("x")));
        }

        [Test]
        public void DuplicateUpgradeIdsAreRejected()
        {
            Assert.Throws<ArgumentException>(() => new RunUpgradeCatalog(new[] { Upgrade("a"), Upgrade("a") }));
        }

        [Test]
        public void DraftIsDeterministicForSameSeed()
        {
            RunUpgradeCatalog catalog = Catalog();
            string first = DraftIds(catalog, 123, 0);
            string second = DraftIds(catalog, 123, 0);
            Assert.AreEqual(first, second);
        }

        [Test]
        public void DifferentSeedCanDiffer()
        {
            RunUpgradeCatalog catalog = Catalog();
            Assert.AreNotEqual(DraftIds(catalog, 1, 0), DraftIds(catalog, 2, 0));
        }

        [Test]
        public void RarityWeightingInfluencesDraft()
        {
            var catalog = new RunUpgradeCatalog(new[]
            {
                Upgrade("common", RunUpgradeRarity.Common, 1),
                Upgrade("legendary", RunUpgradeRarity.Legendary, 1000)
            });
            RunUpgradeDraft draft = RunUpgradeDraftService.Generate(catalog, new RunUpgradeState(), new RunUpgradeDraftRequest(1, 5));
            Assert.AreEqual("legendary", draft.Choices[0].Id.Value);
        }

        [Test]
        public void PrerequisiteFiltering()
        {
            var baseUpgrade = Upgrade("base");
            var child = Upgrade("child", prerequisites: new[] { Id("base") });
            var catalog = new RunUpgradeCatalog(new[] { baseUpgrade, child });
            var state = new RunUpgradeState();
            Assert.AreEqual("base", DraftIds(catalog, 10, 0, 2));
            state.Select(catalog, Id("base"));
            RunUpgradeDraft draft = RunUpgradeDraftService.Generate(catalog, state, new RunUpgradeDraftRequest(2, 10));
            Assert.That(string.Join(",", draft.Choices.Select(choice => choice.Id.Value).ToArray()), Does.Contain("child"));
        }

        [Test]
        public void ExclusionFiltering()
        {
            var left = Upgrade("left", exclusions: new[] { Id("right") });
            var right = Upgrade("right");
            var catalog = new RunUpgradeCatalog(new[] { left, right });
            var state = new RunUpgradeState();
            state.Select(catalog, Id("left"));
            RunUpgradeDraft draft = RunUpgradeDraftService.Generate(catalog, state, new RunUpgradeDraftRequest(2, 7));
            Assert.False(draft.Choices.Any(choice => choice.Id.Equals(Id("right"))));
        }

        [Test]
        public void MaxRankFiltering()
        {
            var catalog = new RunUpgradeCatalog(new[] { Upgrade("ranked", maxRank: 1), Upgrade("other") });
            var state = new RunUpgradeState();
            Assert.True(state.Select(catalog, Id("ranked")).Succeeded);
            RunUpgradeDraft draft = RunUpgradeDraftService.Generate(catalog, state, new RunUpgradeDraftRequest(2, 9));
            Assert.False(draft.Choices.Any(choice => choice.Id.Equals(Id("ranked"))));
        }

        [Test]
        public void RerollChangesDraftDeterministically()
        {
            RunUpgradeCatalog catalog = Catalog();
            string first = DraftIds(catalog, 77, 0);
            string reroll = DraftIds(catalog, 77, 1);
            Assert.AreNotEqual(first, reroll);
            Assert.AreEqual(reroll, DraftIds(catalog, 77, 1));
        }

        [Test]
        public void BanishedUpgradeIsExcluded()
        {
            RunUpgradeCatalog catalog = Catalog();
            var state = new RunUpgradeState();
            state.Banish(Id("direct.damage"));
            RunUpgradeDraft draft = RunUpgradeDraftService.Generate(catalog, state, new RunUpgradeDraftRequest(5, 2));
            Assert.False(draft.Choices.Any(choice => choice.Id.Equals(Id("direct.damage"))));
        }

        [Test]
        public void LockedChoicePersistsThroughReroll()
        {
            RunUpgradeCatalog catalog = Catalog();
            var lockedId = Id("projectile.speed");
            RunUpgradeDraft draft = RunUpgradeDraftService.Generate(catalog, new RunUpgradeState(), new RunUpgradeDraftRequest(3, 42, 3, new[] { lockedId }));
            Assert.AreEqual(lockedId, draft.Choices[0].Id);
        }

        [Test]
        public void SelectingUpgradeUpdatesState()
        {
            RunUpgradeCatalog catalog = Catalog();
            var state = new RunUpgradeState();
            RunUpgradeSelectionResult result = state.Select(catalog, Id("direct.damage"));
            Assert.True(result.Succeeded);
            Assert.AreEqual(1, state.GetRank(Id("direct.damage")));
        }

        [Test]
        public void DuplicateSelectionStopsAtMaxRank()
        {
            var catalog = new RunUpgradeCatalog(new[] { Upgrade("direct.damage", maxRank: 1) });
            var state = new RunUpgradeState();
            Assert.AreEqual(RunUpgradeSelectionStatus.Selected, state.Select(catalog, Id("direct.damage")).Status);
            Assert.AreEqual(RunUpgradeSelectionStatus.MaxRankReached, state.Select(catalog, Id("direct.damage")).Status);
        }

        [Test]
        public void SnapshotReconstruction()
        {
            RunUpgradeCatalog catalog = Catalog();
            var state = new RunUpgradeState();
            state.Select(catalog, Id("direct.damage"));
            state.Banish(Id("projectile.speed"));
            RunUpgradeSnapshot snapshot = state.CreateSnapshot();
            RunUpgradeState restored = RunUpgradeState.FromSnapshot(snapshot);
            Assert.AreEqual(1, restored.GetRank(Id("direct.damage")));
            Assert.True(restored.IsBanished(Id("projectile.speed")));
        }

        [Test]
        public void IdleAutoDefenseAdapterProof()
        {
            RunUpgradeDefinition upgrade = Upgrade("auto.direct.damage", effects: new[] { EffectDescriptor("stat.additive", "weapon.direct.damage", 2) });
            var statBlock = new StatBlock();
            statBlock.SetBaseValue(new StatId("weapon.direct.damage"), 5);
            ApplyStatEffect(statBlock, upgrade.Effects[0], "upgrade.auto.direct.damage");
            Assert.AreEqual(7, statBlock.GetValue(new StatId("weapon.direct.damage")));
        }

        [Test]
        public void ClassicTowerDefenseAdapterProof()
        {
            RunUpgradeDefinition upgrade = Upgrade("tower.range", effects: new[] { EffectDescriptor("stat.additive", "tower.archer.range", 1.5) });
            var statBlock = new StatBlock();
            statBlock.SetBaseValue(new StatId("tower.archer.range"), 4);
            ApplyStatEffect(statBlock, upgrade.Effects[0], "upgrade.tower.range");
            Assert.AreEqual(5.5, statBlock.GetValue(new StatId("tower.archer.range")));
        }

        private static RunUpgradeCatalog Catalog()
        {
            return new RunUpgradeCatalog(new[]
            {
                Upgrade("direct.damage", RunUpgradeRarity.Common, 4),
                Upgrade("direct.cadence", RunUpgradeRarity.Uncommon, 3),
                Upgrade("projectile.speed", RunUpgradeRarity.Rare, 2),
                Upgrade("objective.shield", RunUpgradeRarity.Epic, 1),
                Upgrade("enemy.pacing", RunUpgradeRarity.Common, 5)
            });
        }

        private static string DraftIds(RunUpgradeCatalog catalog, int seed, int reroll, int count = 3)
        {
            RunUpgradeDraft draft = RunUpgradeDraftService.Generate(catalog, new RunUpgradeState(), new RunUpgradeDraftRequest(count, seed, reroll));
            return string.Join(",", draft.Choices.Select(choice => choice.Id.Value).ToArray());
        }

        private static RunUpgradeDefinition Upgrade(string id, RunUpgradeRarity rarity = RunUpgradeRarity.Common, int weight = 1, int maxRank = 3, RunUpgradeEffectDescriptor[] effects = null, RunUpgradeId[] prerequisites = null, RunUpgradeId[] exclusions = null)
        {
            return new RunUpgradeDefinition(Id(id), rarity, weight, maxRank, effects ?? Effects(id), prerequisites, exclusions);
        }

        private static RunUpgradeId Id(string value) => new RunUpgradeId(value);
        private static RunUpgradeEffectId Effect(string value) => new RunUpgradeEffectId(value);
        private static RunUpgradeTargetId Target(string value) => new RunUpgradeTargetId(value);
        private static RunUpgradeEffectDescriptor[] Effects(string id) => new[] { EffectDescriptor("stat.additive", id.Replace('.', '/'), 1) };
        private static RunUpgradeEffectDescriptor EffectDescriptor(string effect, string target, double amount) => new RunUpgradeEffectDescriptor(Effect(effect), Target(target), amount);

        private static void ApplyStatEffect(StatBlock stats, RunUpgradeEffectDescriptor effect, string handle)
        {
            Assert.AreEqual("stat.additive", effect.EffectId.Value);
            stats.AddModifier(new StatModifier(new StatModifierHandle(handle), new ModifierSourceHandle(handle), new StatId(effect.TargetId.Value), StatModifierOperation.Additive, effect.Amount));
        }
    }
}
