using System;
using System.Collections.Generic;

namespace Deucarian.RunUpgrades
{
    /// <summary>A choice issued by a particular run and offer. Callers never construct choice IDs.</summary>
    public sealed class UpgradeChoiceHandle
    {
        internal UpgradeChoiceHandle(RunUpgradeProfile owner, int offer, RunUpgradeDefinition definition)
        { Owner = owner; Offer = offer; Definition = definition; }
        internal RunUpgradeProfile Owner { get; }
        internal int Offer { get; }
        public RunUpgradeDefinition Definition { get; }
    }

    public sealed class UpgradeDraft
    {
        internal UpgradeDraft(UpgradeChoiceHandle[] choices) { Choices = Array.AsReadOnly(choices); }
        public IReadOnlyList<UpgradeChoiceHandle> Choices { get; }
    }
}
