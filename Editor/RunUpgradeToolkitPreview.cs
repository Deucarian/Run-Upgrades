using System;
using System.Linq;
using Deucarian.GameContentAuthoring.Editor;
using UnityEngine.UIElements;

namespace Deucarian.RunUpgrades.Editor
{
    internal static class RunUpgradeToolkitPreview
    {
        internal static void Add(VisualElement root, RunUpgradeAuthoringState state)
        {
            int rank = 1;
            var form = GameContentToolkitPreview.Add(root, () => RunUpgradeAuthoringPreview.GetPrimaryPreviewAsset(state), null,
                () => RunUpgradeGameContentPreviewSummaries.BuildRankTimeline(state)
                    .Select(item => new GameContentAuthoringPreviewRow(item.Label, item.Detail)).ToArray());
            form.Integer("preview-rank", "Rank", () => rank, value => { rank = Math.Max(1, Math.Min(state.MaxRank, value)); form.Refresh(); });
            form.ReadOnly(null, "Before / after", () => RunUpgradeAuthoringEffectSummary.BuildBeforeAfterSummary(RunUpgradeAuthoringEffectSummary.PrimaryEffect(state), rank));
            form.ReadOnly(null, "Cost", () => RunUpgradeAuthoringEffectSummary.GetCostForRank(state, rank).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
