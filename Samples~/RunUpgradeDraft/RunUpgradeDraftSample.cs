using System;
using UnityEngine;

namespace Deucarian.RunUpgrades.Samples
{
    /// <summary>Builds, drafts, and selects a single run-scoped upgrade on play.</summary>
    public sealed class RunUpgradeDraftSample : MonoBehaviour
    {
        [SerializeField] private int seed = 1234;
        [SerializeField] private int selectedRank;

        public int SelectedRank => selectedRank;

        private void Start()
        {
            var upgrade = new RunUpgradeDefinition(
                new RunUpgradeId("sample.damage"),
                RunUpgradeRarity.Common,
                weight: 10,
                maxRank: 3,
                effects: new[]
                {
                    new RunUpgradeEffectDescriptor(
                        new RunUpgradeEffectId("effect.additive"),
                        new RunUpgradeTargetId("stat.damage"),
                        amount: 5d)
                });
            var catalog = new RunUpgradeCatalog(new[] { upgrade });
            var state = new RunUpgradeState();
            RunUpgradeDraft draft = RunUpgradeDraftService.Generate(
                catalog,
                state,
                new RunUpgradeDraftRequest(choiceCount: 1, seed: seed));

            if (draft.Choices.Count > 0)
            {
                selectedRank = state.Select(catalog, draft.Choices[0].Id).Rank;
            }
        }
    }
}
