using System;
using System.Globalization;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.RunUpgrades.Editor
{
    internal static class RunUpgradeRecordToolkit
    {
        internal static VisualElement RunUpgrade(GameContentRecordDescriptor record)
        {
            if (!GameContentRecordProjectionRegistry<UpgradeContentRecordProjection>.TryProject(record, out var p)) return null;
            var root = new VisualElement { name = "record-runupgrade-details" };
            var form = new DeucarianEditorWorkspaceForm(root);
            if (!string.Equals(record.Description, p.Description, StringComparison.Ordinal))
                form.ReadOnly(null, "Description", () => Convert.ToString(p.Description, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Rarity", () => Convert.ToString(p.Rarity, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Amount per rank", () => Convert.ToString(p.EffectAmount, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Max rank", () => Convert.ToString(p.MaxRank, CultureInfo.InvariantCulture));
            form = form.Section("Upgrade details", true);
            form.ReadOnly(null, "Effect", () => Convert.ToString(p.EffectKind, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Category", () => Convert.ToString(p.Category, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Weight", () => Convert.ToString(p.Weight, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Target", () => Convert.ToString(p.Target, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Prerequisites", () => Convert.ToString(p.PrerequisiteSummary, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Class gates", () => Convert.ToString(p.ClassGateSummary, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "References", () => Convert.ToString(p.ReferenceSummary, CultureInfo.InvariantCulture));
            form.ReadOnly(null, "Comparison", () => Convert.ToString(p.ComparisonSummary, CultureInfo.InvariantCulture));
            if (record.Preview != null) GameContentToolkitPreview.Add(root, () => record.Preview, null);
            return root;
        }

    }
}
