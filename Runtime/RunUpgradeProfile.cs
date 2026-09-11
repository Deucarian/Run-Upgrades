using System;

namespace Deucarian.RunUpgrades
{
    /// <summary>One run's convenience API. The supplied state and existing draft generator remain authoritative.</summary>
    public sealed class RunUpgradeProfile
    {
        private readonly RunUpgradeCatalog catalog;
        private readonly RunUpgradeState state;
        private readonly int seed;
        private int sequence;
        private int activeOffer;

        public RunUpgradeProfile(RunUpgradeCatalog catalog, RunUpgradeState state, int seed)
        { this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); this.state = state ?? throw new ArgumentNullException(nameof(state)); this.seed = seed; }

        public int GetRank(IUpgradeKey upgrade)
        {
            if (upgrade == null) throw new ArgumentNullException(nameof(upgrade), "Select an UpgradeKey or reuse a named upgrade definition.");
            var id = new RunUpgradeId(upgrade.Id);
            if (!catalog.TryGet(id, out _)) throw new InvalidOperationException("Upgrade '" + upgrade.Id + "' is absent from this run. Add its definition to the RunUpgradeCatalog used by this profile.");
            return state.GetRank(id);
        }

        public UpgradeDraft Draft(int count = 3)
        {
            int next = checked(sequence + 1);
            var draft = RunUpgradeDraftService.Generate(catalog, state, new RunUpgradeDraftRequest(count, seed, next - 1));
            var choices = new UpgradeChoiceHandle[draft.Choices.Count];
            for (int i = 0; i < choices.Length; i++) choices[i] = new UpgradeChoiceHandle(this, next, draft.Choices[i]);
            sequence = next;
            activeOffer = next;
            return new UpgradeDraft(choices);
        }

        public RunUpgradeSelectionResult Select(UpgradeChoiceHandle choice)
        {
            if (choice == null) return new RunUpgradeSelectionResult(RunUpgradeSelectionStatus.MissingChoice, default, 0);
            if (!ReferenceEquals(choice.Owner, this)) return new RunUpgradeSelectionResult(RunUpgradeSelectionStatus.ForeignChoice, choice.Definition.Id, 0);
            if (choice.Offer != activeOffer) return new RunUpgradeSelectionResult(RunUpgradeSelectionStatus.ExpiredChoice, choice.Definition.Id, state.GetRank(choice.Definition.Id));
            var result = state.Select(catalog, choice.Definition.Id);
            if (result.Status == RunUpgradeSelectionStatus.Selected) activeOffer = 0;
            return result;
        }
        public RunUpgradeSnapshot Snapshot => state.CreateSnapshot();
    }
}
