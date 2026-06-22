using System;
using System.Collections.Generic;
using Deucarian.GameplayFoundation;

namespace Deucarian.RunUpgrades
{
    /// <summary>Stable identifier for a run upgrade definition.</summary>
    public readonly struct RunUpgradeId : IEquatable<RunUpgradeId>, IComparable<RunUpgradeId>
    {
        private readonly ContentId _value;
        public RunUpgradeId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(RunUpgradeId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is RunUpgradeId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(RunUpgradeId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    /// <summary>Stable identifier for an adapter-owned effect kind.</summary>
    public readonly struct RunUpgradeEffectId : IEquatable<RunUpgradeEffectId>, IComparable<RunUpgradeEffectId>
    {
        private readonly ContentId _value;
        public RunUpgradeEffectId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(RunUpgradeEffectId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is RunUpgradeEffectId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(RunUpgradeEffectId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    /// <summary>Stable identifier for an adapter-owned effect target.</summary>
    public readonly struct RunUpgradeTargetId : IEquatable<RunUpgradeTargetId>, IComparable<RunUpgradeTargetId>
    {
        private readonly ContentId _value;
        public RunUpgradeTargetId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(RunUpgradeTargetId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is RunUpgradeTargetId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(RunUpgradeTargetId other) => _value.CompareTo(other._value);
        public override string ToString() => Value;
    }

    public enum RunUpgradeRarity { Common = 0, Uncommon = 1, Rare = 2, Epic = 3, Legendary = 4 }
    public enum RunUpgradeSelectionStatus { Selected = 0, UnknownUpgrade = 1, Banished = 2, PrerequisiteMissing = 3, Excluded = 4, MaxRankReached = 5 }

    /// <summary>Explicit effect descriptor. Applying it is game-owned adapter work.</summary>
    public readonly struct RunUpgradeEffectDescriptor
    {
        public RunUpgradeEffectDescriptor(RunUpgradeEffectId effectId, RunUpgradeTargetId targetId, double amount)
        {
            if (effectId.IsEmpty) throw new ArgumentException("Effect id cannot be empty.", nameof(effectId));
            if (targetId.IsEmpty) throw new ArgumentException("Target id cannot be empty.", nameof(targetId));
            if (double.IsNaN(amount) || double.IsInfinity(amount)) throw new ArgumentOutOfRangeException(nameof(amount));
            EffectId = effectId; TargetId = targetId; Amount = amount;
        }
        public RunUpgradeEffectId EffectId { get; }
        public RunUpgradeTargetId TargetId { get; }
        public double Amount { get; }
    }

    /// <summary>Immutable authored run upgrade definition.</summary>
    public sealed class RunUpgradeDefinition
    {
        public RunUpgradeDefinition(RunUpgradeId id, RunUpgradeRarity rarity, int weight, int maxRank, IReadOnlyList<RunUpgradeEffectDescriptor> effects, IReadOnlyList<RunUpgradeId> prerequisites = null, IReadOnlyList<RunUpgradeId> exclusions = null)
        {
            if (id.IsEmpty) throw new ArgumentException("Upgrade id cannot be empty.", nameof(id));
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));
            if (maxRank <= 0) throw new ArgumentOutOfRangeException(nameof(maxRank));
            Id = id; Rarity = rarity; Weight = weight; MaxRank = maxRank; Effects = CopyEffects(effects); Prerequisites = CopyIds(prerequisites); Exclusions = CopyIds(exclusions);
        }
        public RunUpgradeId Id { get; }
        public RunUpgradeRarity Rarity { get; }
        public int Weight { get; }
        public int MaxRank { get; }
        public IReadOnlyList<RunUpgradeEffectDescriptor> Effects { get; }
        public IReadOnlyList<RunUpgradeId> Prerequisites { get; }
        public IReadOnlyList<RunUpgradeId> Exclusions { get; }
        private static RunUpgradeEffectDescriptor[] CopyEffects(IReadOnlyList<RunUpgradeEffectDescriptor> source)
        {
            if (source == null || source.Count == 0) throw new ArgumentException("At least one effect descriptor is required.", nameof(source));
            var copy = new RunUpgradeEffectDescriptor[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            return copy;
        }
        private static RunUpgradeId[] CopyIds(IReadOnlyList<RunUpgradeId> source)
        {
            if (source == null) return Array.Empty<RunUpgradeId>();
            var copy = new RunUpgradeId[source.Count];
            var seen = new HashSet<RunUpgradeId>();
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].IsEmpty) throw new ArgumentException("Upgrade id cannot be empty.");
                if (!seen.Add(source[i])) throw new ArgumentException("Duplicate upgrade id in list: " + source[i]);
                copy[i] = source[i];
            }
            Array.Sort(copy);
            return copy;
        }
    }

    /// <summary>Validated collection of upgrade definitions.</summary>
    public sealed class RunUpgradeCatalog
    {
        private readonly Dictionary<RunUpgradeId, RunUpgradeDefinition> _definitions = new Dictionary<RunUpgradeId, RunUpgradeDefinition>();
        private readonly RunUpgradeDefinition[] _ordered;
        public RunUpgradeCatalog(IReadOnlyList<RunUpgradeDefinition> definitions)
        {
            if (definitions == null || definitions.Count == 0) throw new ArgumentException("At least one upgrade definition is required.", nameof(definitions));
            _ordered = new RunUpgradeDefinition[definitions.Count];
            for (int i = 0; i < definitions.Count; i++)
            {
                RunUpgradeDefinition definition = definitions[i] ?? throw new ArgumentException("Upgrade definition cannot be null.");
                if (_definitions.ContainsKey(definition.Id)) throw new ArgumentException("Duplicate upgrade id: " + definition.Id);
                _definitions.Add(definition.Id, definition);
                _ordered[i] = definition;
            }
            Array.Sort(_ordered, (a, b) => a.Id.CompareTo(b.Id));
        }
        public IReadOnlyList<RunUpgradeDefinition> Definitions => _ordered;
        public bool TryGet(RunUpgradeId id, out RunUpgradeDefinition definition) => _definitions.TryGetValue(id, out definition);
    }

    /// <summary>Mutable selected-upgrade state for one run.</summary>
    public sealed class RunUpgradeState
    {
        private readonly Dictionary<RunUpgradeId, int> _ranks = new Dictionary<RunUpgradeId, int>();
        private readonly HashSet<RunUpgradeId> _banished = new HashSet<RunUpgradeId>();

        public int GetRank(RunUpgradeId id) => _ranks.TryGetValue(id, out int rank) ? rank : 0;
        public bool IsBanished(RunUpgradeId id) => _banished.Contains(id);
        public bool Banish(RunUpgradeId id) { if (id.IsEmpty) throw new ArgumentException("Upgrade id cannot be empty.", nameof(id)); return _banished.Add(id); }

        public RunUpgradeSelectionResult Select(RunUpgradeCatalog catalog, RunUpgradeId id)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (!catalog.TryGet(id, out RunUpgradeDefinition definition)) return new RunUpgradeSelectionResult(RunUpgradeSelectionStatus.UnknownUpgrade, id, GetRank(id));
            RunUpgradeSelectionStatus status = RunUpgradeDraftService.GetAvailability(catalog, this, definition);
            if (status != RunUpgradeSelectionStatus.Selected) return new RunUpgradeSelectionResult(status, id, GetRank(id));
            int nextRank = GetRank(id) + 1;
            _ranks[id] = nextRank;
            return new RunUpgradeSelectionResult(RunUpgradeSelectionStatus.Selected, id, nextRank);
        }

        public RunUpgradeSnapshot CreateSnapshot()
        {
            var ranks = new RunUpgradeRankSnapshot[_ranks.Count];
            int index = 0;
            foreach (KeyValuePair<RunUpgradeId, int> pair in _ranks) ranks[index++] = new RunUpgradeRankSnapshot(pair.Key, pair.Value);
            Array.Sort(ranks, (a, b) => a.Id.CompareTo(b.Id));
            var banished = new RunUpgradeId[_banished.Count];
            _banished.CopyTo(banished);
            Array.Sort(banished);
            return new RunUpgradeSnapshot(ranks, banished);
        }

        public static RunUpgradeState FromSnapshot(RunUpgradeSnapshot snapshot)
        {
            var state = new RunUpgradeState();
            if (snapshot == null) return state;
            for (int i = 0; i < snapshot.Ranks.Count; i++) state._ranks.Add(snapshot.Ranks[i].Id, snapshot.Ranks[i].Rank);
            for (int i = 0; i < snapshot.Banished.Count; i++) state._banished.Add(snapshot.Banished[i]);
            return state;
        }
    }

    public readonly struct RunUpgradeSelectionResult
    {
        public RunUpgradeSelectionResult(RunUpgradeSelectionStatus status, RunUpgradeId id, int rank)
        {
            Status = status; Id = id; Rank = rank;
        }
        public RunUpgradeSelectionStatus Status { get; }
        public RunUpgradeId Id { get; }
        public int Rank { get; }
        public bool Succeeded => Status == RunUpgradeSelectionStatus.Selected;
    }

    public sealed class RunUpgradeDraftRequest
    {
        public RunUpgradeDraftRequest(int choiceCount, int seed, int rerollIndex = 0, IReadOnlyList<RunUpgradeId> lockedChoices = null)
        {
            if (choiceCount <= 0) throw new ArgumentOutOfRangeException(nameof(choiceCount));
            if (rerollIndex < 0) throw new ArgumentOutOfRangeException(nameof(rerollIndex));
            ChoiceCount = choiceCount; Seed = seed; RerollIndex = rerollIndex; LockedChoices = Copy(lockedChoices);
        }
        public int ChoiceCount { get; }
        public int Seed { get; }
        public int RerollIndex { get; }
        public IReadOnlyList<RunUpgradeId> LockedChoices { get; }
        private static RunUpgradeId[] Copy(IReadOnlyList<RunUpgradeId> source)
        {
            if (source == null) return Array.Empty<RunUpgradeId>();
            var copy = new RunUpgradeId[source.Count];
            var seen = new HashSet<RunUpgradeId>();
            for (int i = 0; i < source.Count; i++) { if (!seen.Add(source[i])) throw new ArgumentException("Duplicate locked choice: " + source[i]); copy[i] = source[i]; }
            return copy;
        }
    }

    public sealed class RunUpgradeDraft
    {
        public RunUpgradeDraft(IReadOnlyList<RunUpgradeDefinition> choices)
        {
            Choices = Copy(choices);
        }
        public IReadOnlyList<RunUpgradeDefinition> Choices { get; }
        private static RunUpgradeDefinition[] Copy(IReadOnlyList<RunUpgradeDefinition> source)
        {
            if (source == null) return Array.Empty<RunUpgradeDefinition>();
            var copy = new RunUpgradeDefinition[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            return copy;
        }
    }

    public static class RunUpgradeDraftService
    {
        public static RunUpgradeDraft Generate(RunUpgradeCatalog catalog, RunUpgradeState state, RunUpgradeDraftRequest request)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (request == null) throw new ArgumentNullException(nameof(request));
            var choices = new List<RunUpgradeDefinition>(request.ChoiceCount);
            var used = new HashSet<RunUpgradeId>();
            for (int i = 0; i < request.LockedChoices.Count && choices.Count < request.ChoiceCount; i++)
            {
                if (!catalog.TryGet(request.LockedChoices[i], out RunUpgradeDefinition locked)) continue;
                if (GetAvailability(catalog, state, locked) != RunUpgradeSelectionStatus.Selected) continue;
                if (used.Add(locked.Id)) choices.Add(locked);
            }
            var random = new DeterministicRandom(Mix(request.Seed, request.RerollIndex));
            while (choices.Count < request.ChoiceCount)
            {
                RunUpgradeDefinition picked = PickWeighted(catalog, state, used, random);
                if (picked == null) break;
                used.Add(picked.Id);
                choices.Add(picked);
            }
            return new RunUpgradeDraft(choices);
        }

        public static RunUpgradeSelectionStatus GetAvailability(RunUpgradeCatalog catalog, RunUpgradeState state, RunUpgradeDefinition definition)
        {
            if (state.IsBanished(definition.Id)) return RunUpgradeSelectionStatus.Banished;
            if (state.GetRank(definition.Id) >= definition.MaxRank) return RunUpgradeSelectionStatus.MaxRankReached;
            for (int i = 0; i < definition.Prerequisites.Count; i++)
                if (state.GetRank(definition.Prerequisites[i]) <= 0) return RunUpgradeSelectionStatus.PrerequisiteMissing;
            for (int i = 0; i < definition.Exclusions.Count; i++)
                if (state.GetRank(definition.Exclusions[i]) > 0) return RunUpgradeSelectionStatus.Excluded;
            for (int i = 0; i < catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition selected = catalog.Definitions[i];
                if (state.GetRank(selected.Id) <= 0) continue;
                for (int e = 0; e < selected.Exclusions.Count; e++)
                    if (selected.Exclusions[e].Equals(definition.Id)) return RunUpgradeSelectionStatus.Excluded;
            }
            return RunUpgradeSelectionStatus.Selected;
        }

        private static RunUpgradeDefinition PickWeighted(RunUpgradeCatalog catalog, RunUpgradeState state, HashSet<RunUpgradeId> used, IRandomSource random)
        {
            int total = 0;
            for (int i = 0; i < catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = catalog.Definitions[i];
                if (used.Contains(definition.Id) || GetAvailability(catalog, state, definition) != RunUpgradeSelectionStatus.Selected) continue;
                checked { total += definition.Weight; }
            }
            if (total == 0) return null;
            int roll = random.Range(0, total);
            int cursor = 0;
            for (int i = 0; i < catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = catalog.Definitions[i];
                if (used.Contains(definition.Id) || GetAvailability(catalog, state, definition) != RunUpgradeSelectionStatus.Selected) continue;
                cursor += definition.Weight;
                if (roll < cursor) return definition;
            }
            return null;
        }

        private static int Mix(int seed, int rerollIndex)
        {
            unchecked
            {
                uint value = (uint)seed;
                value ^= (uint)(rerollIndex + 0x9E3779B9);
                value *= 0x85EBCA6B;
                value ^= value >> 13;
                return (int)value;
            }
        }
    }

    public readonly struct RunUpgradeRankSnapshot
    {
        public RunUpgradeRankSnapshot(RunUpgradeId id, int rank)
        {
            if (id.IsEmpty) throw new ArgumentException("Upgrade id cannot be empty.", nameof(id));
            if (rank <= 0) throw new ArgumentOutOfRangeException(nameof(rank));
            Id = id; Rank = rank;
        }
        public RunUpgradeId Id { get; }
        public int Rank { get; }
    }

    public sealed class RunUpgradeSnapshot
    {
        public RunUpgradeSnapshot(IReadOnlyList<RunUpgradeRankSnapshot> ranks, IReadOnlyList<RunUpgradeId> banished)
        {
            Ranks = Copy(ranks); Banished = CopyIds(banished);
        }
        public IReadOnlyList<RunUpgradeRankSnapshot> Ranks { get; }
        public IReadOnlyList<RunUpgradeId> Banished { get; }
        private static RunUpgradeRankSnapshot[] Copy(IReadOnlyList<RunUpgradeRankSnapshot> source)
        {
            if (source == null) return Array.Empty<RunUpgradeRankSnapshot>();
            var copy = new RunUpgradeRankSnapshot[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            Array.Sort(copy, (a, b) => a.Id.CompareTo(b.Id));
            return copy;
        }
        private static RunUpgradeId[] CopyIds(IReadOnlyList<RunUpgradeId> source)
        {
            if (source == null) return Array.Empty<RunUpgradeId>();
            var copy = new RunUpgradeId[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            Array.Sort(copy);
            return copy;
        }
    }
}
