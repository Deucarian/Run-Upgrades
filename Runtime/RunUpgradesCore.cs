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

    /// <summary>Stable identifier for a mutually exclusive draft-option group.</summary>
    public readonly struct RunUpgradeDraftGroupId : IEquatable<RunUpgradeDraftGroupId>, IComparable<RunUpgradeDraftGroupId>
    {
        private readonly ContentId _value;
        public RunUpgradeDraftGroupId(string value) { _value = new ContentId(value); }
        public string Value => _value.Value;
        public bool IsEmpty => _value.IsEmpty;
        public bool Equals(RunUpgradeDraftGroupId other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is RunUpgradeDraftGroupId other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
        public int CompareTo(RunUpgradeDraftGroupId other) => _value.CompareTo(other._value);
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

    /// <summary>Validated input for game-owned candidates participating in a run-upgrade draft.</summary>
    public readonly struct RunUpgradeDraftOption
    {
        public RunUpgradeDraftOption(RunUpgradeId id, double weight)
            : this(id, weight, DefaultGroup(id))
        {
        }

        public RunUpgradeDraftOption(RunUpgradeId id, double weight, RunUpgradeDraftGroupId dedupeGroup)
        {
            if (id.IsEmpty) throw new ArgumentException("Draft option id cannot be empty.", nameof(id));
            if (weight <= 0d || double.IsNaN(weight) || double.IsInfinity(weight))
                throw new ArgumentOutOfRangeException(nameof(weight), "Draft option weight must be positive and finite.");
            if (dedupeGroup.IsEmpty) throw new ArgumentException("Draft option dedupe group cannot be empty.", nameof(dedupeGroup));
            Id = id;
            Weight = weight;
            DedupeGroup = dedupeGroup;
        }

        public RunUpgradeId Id { get; }
        public double Weight { get; }
        public RunUpgradeDraftGroupId DedupeGroup { get; }

        private static RunUpgradeDraftGroupId DefaultGroup(RunUpgradeId id)
        {
            if (id.IsEmpty) throw new ArgumentException("Draft option id cannot be empty.", nameof(id));
            return new RunUpgradeDraftGroupId(id.Value);
        }
    }

    /// <summary>Ordered identifiers selected for a deterministic run-upgrade draft.</summary>
    public sealed class RunUpgradeDraftSelection
    {
        internal RunUpgradeDraftSelection(IReadOnlyList<RunUpgradeId> choiceIds)
        {
            ChoiceIds = Copy(choiceIds);
        }

        public IReadOnlyList<RunUpgradeId> ChoiceIds { get; }

        private static RunUpgradeId[] Copy(IReadOnlyList<RunUpgradeId> source)
        {
            if (source == null || source.Count == 0) return Array.Empty<RunUpgradeId>();
            var copy = new RunUpgradeId[source.Count];
            for (int i = 0; i < source.Count; i++) copy[i] = source[i];
            return copy;
        }
    }

    /// <summary>
    /// Deterministic weighted selection for caller-filtered options. Caller ordering is part of the deterministic input.
    /// </summary>
    public static class RunUpgradeDraftSelector
    {
        public static RunUpgradeDraftSelection Select(IReadOnlyList<RunUpgradeDraftOption> options, RunUpgradeDraftRequest request)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var ordered = new RunUpgradeDraftOption[options.Count];
            var optionIndexes = new Dictionary<RunUpgradeId, int>();
            for (int i = 0; i < options.Count; i++)
            {
                RunUpgradeDraftOption option = options[i];
                ValidateOption(option, i);
                if (optionIndexes.ContainsKey(option.Id))
                    throw new ArgumentException("Duplicate draft option id: " + option.Id, nameof(options));
                optionIndexes.Add(option.Id, i);
                ordered[i] = option;
            }

            var choices = new List<RunUpgradeId>(request.ChoiceCount);
            var selectedIds = new HashSet<RunUpgradeId>();
            var selectedGroups = new HashSet<RunUpgradeDraftGroupId>();
            for (int i = 0; i < request.LockedChoices.Count && choices.Count < request.ChoiceCount; i++)
            {
                RunUpgradeId lockedId = request.LockedChoices[i];
                if (!optionIndexes.TryGetValue(lockedId, out int optionIndex)) continue;
                RunUpgradeDraftOption option = ordered[optionIndex];
                if (selectedIds.Contains(option.Id) || selectedGroups.Contains(option.DedupeGroup)) continue;
                selectedIds.Add(option.Id);
                selectedGroups.Add(option.DedupeGroup);
                choices.Add(option.Id);
            }

            var random = new DeterministicRandom(Mix(request.Seed, request.RerollIndex));
            while (choices.Count < request.ChoiceCount)
            {
                int selectedIndex = PickWeighted(ordered, selectedIds, selectedGroups, random);
                if (selectedIndex < 0) break;
                RunUpgradeDraftOption selected = ordered[selectedIndex];
                selectedIds.Add(selected.Id);
                selectedGroups.Add(selected.DedupeGroup);
                choices.Add(selected.Id);
            }

            return new RunUpgradeDraftSelection(choices);
        }

        private static void ValidateOption(RunUpgradeDraftOption option, int index)
        {
            if (option.Id.IsEmpty) throw new ArgumentException("Draft option at index " + index + " has an empty id.", nameof(option));
            if (option.Weight <= 0d || double.IsNaN(option.Weight) || double.IsInfinity(option.Weight))
                throw new ArgumentOutOfRangeException(nameof(option), "Draft option at index " + index + " has a non-positive or non-finite weight.");
            if (option.DedupeGroup.IsEmpty)
                throw new ArgumentException("Draft option at index " + index + " has an empty dedupe group.", nameof(option));
        }

        private static int PickWeighted(
            IReadOnlyList<RunUpgradeDraftOption> options,
            HashSet<RunUpgradeId> selectedIds,
            HashSet<RunUpgradeDraftGroupId> selectedGroups,
            IRandomSource random)
        {
            bool useIntegerPath = true;
            long integerTotal = 0L;
            int availableCount = 0;
            for (int i = 0; i < options.Count; i++)
            {
                RunUpgradeDraftOption option = options[i];
                if (!IsAvailable(option, selectedIds, selectedGroups)) continue;
                availableCount++;
                if (option.Weight != Math.Floor(option.Weight) || option.Weight > int.MaxValue)
                {
                    useIntegerPath = false;
                    continue;
                }

                if (!useIntegerPath) continue;
                integerTotal += (long)option.Weight;
                if (integerTotal > int.MaxValue) useIntegerPath = false;
            }

            if (availableCount == 0) return -1;
            if (useIntegerPath)
                return PickWeightedInteger(options, selectedIds, selectedGroups, random, (int)integerTotal);
            return PickWeightedDouble(options, selectedIds, selectedGroups, random);
        }

        private static int PickWeightedInteger(
            IReadOnlyList<RunUpgradeDraftOption> options,
            HashSet<RunUpgradeId> selectedIds,
            HashSet<RunUpgradeDraftGroupId> selectedGroups,
            IRandomSource random,
            int totalWeight)
        {
            int roll = random.Range(0, totalWeight);
            int cursor = 0;
            for (int i = 0; i < options.Count; i++)
            {
                RunUpgradeDraftOption option = options[i];
                if (!IsAvailable(option, selectedIds, selectedGroups)) continue;
                cursor += (int)option.Weight;
                if (roll < cursor) return i;
            }
            return -1;
        }

        private static int PickWeightedDouble(
            IReadOnlyList<RunUpgradeDraftOption> options,
            HashSet<RunUpgradeId> selectedIds,
            HashSet<RunUpgradeDraftGroupId> selectedGroups,
            IRandomSource random)
        {
            double totalWeight = 0d;
            double maximumWeight = 0d;
            int lastAvailableIndex = -1;
            for (int i = 0; i < options.Count; i++)
            {
                RunUpgradeDraftOption option = options[i];
                if (!IsAvailable(option, selectedIds, selectedGroups)) continue;
                totalWeight += option.Weight;
                maximumWeight = Math.Max(maximumWeight, option.Weight);
                lastAvailableIndex = i;
            }

            bool normalize = double.IsInfinity(totalWeight);
            if (normalize)
            {
                totalWeight = 0d;
                for (int i = 0; i < options.Count; i++)
                {
                    RunUpgradeDraftOption option = options[i];
                    if (IsAvailable(option, selectedIds, selectedGroups))
                        totalWeight += option.Weight / maximumWeight;
                }
            }

            double roll = random.Range(0d, totalWeight);
            double cursor = 0d;
            for (int i = 0; i < options.Count; i++)
            {
                RunUpgradeDraftOption option = options[i];
                if (!IsAvailable(option, selectedIds, selectedGroups)) continue;
                cursor += normalize ? option.Weight / maximumWeight : option.Weight;
                if (roll < cursor) return i;
            }
            return lastAvailableIndex;
        }

        private static bool IsAvailable(
            RunUpgradeDraftOption option,
            HashSet<RunUpgradeId> selectedIds,
            HashSet<RunUpgradeDraftGroupId> selectedGroups)
        {
            return !selectedIds.Contains(option.Id) && !selectedGroups.Contains(option.DedupeGroup);
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
            var options = new List<RunUpgradeDraftOption>(catalog.Definitions.Count);
            for (int i = 0; i < catalog.Definitions.Count; i++)
            {
                RunUpgradeDefinition definition = catalog.Definitions[i];
                if (GetAvailability(catalog, state, definition) == RunUpgradeSelectionStatus.Selected)
                    options.Add(new RunUpgradeDraftOption(definition.Id, definition.Weight));
            }

            RunUpgradeDraftSelection selection = RunUpgradeDraftSelector.Select(options, request);
            var choices = new List<RunUpgradeDefinition>(selection.ChoiceIds.Count);
            for (int i = 0; i < selection.ChoiceIds.Count; i++)
            {
                if (catalog.TryGet(selection.ChoiceIds[i], out RunUpgradeDefinition definition))
                    choices.Add(definition);
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
