using System;
using System.Globalization;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using UnityEditor;
using UnityEngine;

namespace Deucarian.RunUpgrades.Editor
{
    public sealed class UpgradeContentRecordProjection
    {
        public UpgradeContentRecordProjection(
            GameContentRecordDescriptor record,
            string description,
            string category,
            string rarity,
            double weight,
            int maxRank,
            string effectKind,
            double effectAmount,
            string target,
            string prerequisiteSummary,
            string classGateSummary,
            string referenceSummary,
            string comparisonSummary)
        {
            Record = record;
            Description = description ?? string.Empty;
            Category = category ?? string.Empty;
            Rarity = rarity ?? string.Empty;
            Weight = weight;
            MaxRank = maxRank;
            EffectKind = effectKind ?? string.Empty;
            EffectAmount = effectAmount;
            Target = target ?? string.Empty;
            PrerequisiteSummary = prerequisiteSummary ?? string.Empty;
            ClassGateSummary = classGateSummary ?? string.Empty;
            ReferenceSummary = referenceSummary ?? string.Empty;
            ComparisonSummary = comparisonSummary ?? string.Empty;
        }

        public GameContentRecordDescriptor Record { get; }
        public string Description { get; }
        public string Category { get; }
        public string Rarity { get; }
        public double Weight { get; }
        public int MaxRank { get; }
        public string EffectKind { get; }
        public double EffectAmount { get; }
        public string Target { get; }
        public string PrerequisiteSummary { get; }
        public string ClassGateSummary { get; }
        public string ReferenceSummary { get; }
        public string ComparisonSummary { get; }
    }

    internal sealed class UpgradePackAwareLensState
    {
        public readonly GameContentRecordLensBrowserState Browser = new GameContentRecordLensBrowserState();
        public UpgradePackAwareSubtypeFilter SubtypeFilter;
    }

    internal enum UpgradePackAwareSubtypeFilter
    {
        All = 0,
        WeaponUpgrade = 1,
        Passive = 2,
        PickupMagnet = 3,
        Mutation = 4,
        Evolution = 5,
        MetaUpgrade = 6
    }

    internal static class UpgradePackAwareLensView
    {
        private static readonly string[] SubtypeLabels =
        {
            "All Upgrades",
            "Weapon Upgrade",
            "Passive",
            "Pickup / Magnet",
            "Mutation",
            "Evolution",
            "Meta Upgrade"
        };

        public static void Draw(
            GameContentAuthoringSurfaceContext context,
            GameContentLensDescriptor lens,
            UpgradePackAwareLensState state)
        {
            GameContentRecordLensBrowser.Draw(
                context,
                lens,
                state.Browser,
                DrawDetails,
                DrawPreview,
                record => MatchesSubtype(record, state.SubtypeFilter),
                () => state.SubtypeFilter = (UpgradePackAwareSubtypeFilter)EditorGUILayout.Popup(
                    (int)state.SubtypeFilter,
                    SubtypeLabels));
        }

        internal static bool MatchesSubtype(
            GameContentRecordDescriptor record,
            UpgradePackAwareSubtypeFilter filter)
        {
            if (record == null) return false;
            switch (filter)
            {
                case UpgradePackAwareSubtypeFilter.WeaponUpgrade:
                    return record.HasCapability(GameContentRecordCapabilities.WeaponUpgrade);
                case UpgradePackAwareSubtypeFilter.Passive:
                    return record.HasCapability(GameContentRecordCapabilities.Passive);
                case UpgradePackAwareSubtypeFilter.PickupMagnet:
                    return record.HasCapability(GameContentRecordCapabilities.PickupMagnet);
                case UpgradePackAwareSubtypeFilter.Mutation:
                    return record.HasCapability(GameContentRecordCapabilities.Mutation);
                case UpgradePackAwareSubtypeFilter.Evolution:
                    return record.HasCapability(GameContentRecordCapabilities.Evolution);
                case UpgradePackAwareSubtypeFilter.MetaUpgrade:
                    return record.HasCapability(GameContentRecordCapabilities.MetaUpgrade);
                default:
                    return record.HasCapability(GameContentRecordCapabilities.Upgrade);
            }
        }

        private static void DrawDetails(GameContentRecordDescriptor record)
        {
            if (!GameContentRecordProjectionRegistry<UpgradeContentRecordProjection>.TryProject(record, out UpgradeContentRecordProjection projection))
            {
                EditorGUILayout.HelpBox("No installed adapter exposes common Upgrade fields for this record.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Upgrade", DeucarianEditorStyles.SectionTitle);
            if (!string.IsNullOrWhiteSpace(projection.Description))
                EditorGUILayout.LabelField(projection.Description, EditorStyles.wordWrappedLabel);
            GameContentRecordLensBrowser.DrawRow("Category", projection.Category);
            GameContentRecordLensBrowser.DrawRow("Rarity", Empty(projection.Rarity));
            Row("Weight", projection.Weight);
            GameContentRecordLensBrowser.DrawRow("Max Rank", projection.MaxRank.ToString(CultureInfo.InvariantCulture));
            GameContentRecordLensBrowser.DrawRow("Effect", projection.EffectKind);
            Row("Amount", projection.EffectAmount);
            GameContentRecordLensBrowser.DrawRow("Target", Empty(projection.Target));
            GameContentRecordLensBrowser.DrawRow("Prerequisites", Empty(projection.PrerequisiteSummary));
            GameContentRecordLensBrowser.DrawRow("Class Gates", Empty(projection.ClassGateSummary));
            GameContentRecordLensBrowser.DrawRow("References", Empty(projection.ReferenceSummary));
            GameContentRecordLensBrowser.DrawRow("Comparison", Empty(projection.ComparisonSummary));
        }

        private static void DrawPreview(GameContentRecordDescriptor record)
        {
            EditorGUILayout.LabelField(record.DisplayName, DeucarianEditorStyles.SectionTitle);
            if (!GameContentRecordProjectionRegistry<UpgradeContentRecordProjection>.TryProject(record, out UpgradeContentRecordProjection projection))
            {
                EditorGUILayout.HelpBox("Preview adapter unavailable.", MessageType.Warning);
                return;
            }

            DeucarianEditorStatusBadge.Draw("Read-only pack record", DeucarianEditorStatus.Info, GUILayout.MinWidth(138f));
            GameContentRecordLensBrowser.DrawRow("Category", projection.Category);
            GameContentRecordLensBrowser.DrawRow("Rank Range", "1 - " + Math.Max(1, projection.MaxRank).ToString(CultureInfo.InvariantCulture));
            GameContentRecordLensBrowser.DrawRow("Effect", projection.EffectKind);
            Row("Per Rank", projection.EffectAmount);
            GameContentRecordLensBrowser.DrawRow("Target", Empty(projection.Target));
            if (!string.IsNullOrWhiteSpace(projection.ComparisonSummary))
                EditorGUILayout.HelpBox(projection.ComparisonSummary, MessageType.Info);
        }

        private static void Row(string label, double value)
        {
            GameContentRecordLensBrowser.DrawRow(label, value.ToString("0.###", CultureInfo.InvariantCulture));
        }

        private static string Empty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "None" : value;
        }
    }
}
