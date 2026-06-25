using System;
using System.Collections.Generic;
using System.Globalization;
using Deucarian.Attacks.Authoring;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.RunUpgrades.Authoring;
using Deucarian.WeaponSystems.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.RunUpgrades.Editor
{
    [InitializeOnLoad]
    internal static class RunUpgradeGameContentAuthoringProviderRegistration
    {
        static RunUpgradeGameContentAuthoringProviderRegistration()
        {
            GameContentAuthoringProviderRegistry.Register(new RunUpgradeAuthoringProvider());
        }
    }

    internal sealed class RunUpgradeAuthoringProvider : IGameContentAuthoringProvider
    {
        private readonly RunUpgradeAuthoringState _state = new RunUpgradeAuthoringState();
        private readonly RunUpgradeGameContentPreviewController _preview = new RunUpgradeGameContentPreviewController();

        public string ProviderId => "com.deucarian.run-upgrades.upgrade";
        public string DisplayName => "Upgrade";
        public string Description => "Create a root RunUpgradeDefinition with economy and effect sections.";
        public int SortOrder => 140;
        public bool Enabled => true;
        public void OnSelected() { }
        public void DrawPreview(GameContentAuthoringPreviewContext context) { _preview.Draw(context, _state); }
        public void StopPreview() { _preview.Stop(); }

        public void Draw(GameContentAuthoringContext context)
        {
            _state.EnsureEffects();
            RunUpgradeDefinitionAsset preview = RunUpgradeDefinitionAssetCreator.BuildTransient(_state);
            GameContentAuthoringValidationResult report;
            try
            {
                report = RunUpgradeDefinitionAssetCreator.ValidateForCreation(_state, preview);
            }
            finally
            {
                RunUpgradeDefinitionAssetCreator.DestroyTransient(preview);
            }

            context.DrawSection("Upgrade Identity", () =>
            {
                _state.UpgradeId = EditorGUILayout.TextField("Stable ID", _state.UpgradeId);
                _state.DisplayName = EditorGUILayout.TextField("Display Name", _state.DisplayName);
                _state.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", _state.Icon, typeof(Sprite), false);
                _state.Description = EditorGUILayout.TextField("Description", _state.Description);
                _state.TagsCsv = EditorGUILayout.TextField("Tags", _state.TagsCsv);
                _state.OutputRoot = context.DrawOutputRootField(_state.OutputRoot);
            });

            context.DrawSection("Economy", () =>
            {
                _state.Rarity = (RunUpgradeRarity)EditorGUILayout.EnumPopup("Rarity", _state.Rarity);
                _state.Weight = EditorGUILayout.IntField("Draft Weight", _state.Weight);
                _state.MaxRank = EditorGUILayout.IntField("Max Rank", _state.MaxRank);
                _state.CostsCsv = EditorGUILayout.TextField("Per-Rank Costs", _state.CostsCsv);
            });

            context.DrawSection("Effects", () =>
            {
                for (int i = 0; i < _state.Effects.Count; i++)
                    DrawEffect(context, i);
                GUILayout.Space(4f);
                if (context.DrawSecondaryButton("Add Effect", true, GUILayout.Height(24f)))
                    _state.Effects.Add(new RunUpgradeEffectAuthoringState());
            });

            context.DrawSection("Prerequisites", () =>
            {
                _state.PrerequisitesCsv = EditorGUILayout.TextField("Prerequisites", _state.PrerequisitesCsv);
                _state.ExclusionsCsv = EditorGUILayout.TextField("Exclusions", _state.ExclusionsCsv);
            });

            context.DrawSection("Preview", () =>
            {
                foreach (string line in RunUpgradeDefinitionAssetCreator.GetPreviewLines(_state))
                    EditorGUILayout.LabelField(line, context.MutedStyle);
                GUILayout.Space(6f);
                context.DrawValidation(report, "Ready to create one root RunUpgradeDefinition asset with economy and effects sub-assets.");
                GUILayout.Space(8f);
                if (context.DrawCreateButton("Create Upgrade Asset", report.IsValid))
                    context.SetCreationResult(RunUpgradeDefinitionAssetCreator.CreateAssets(_state));
                context.DrawCreationResult();
            });
        }

        private void DrawEffect(GameContentAuthoringContext context, int index)
        {
            RunUpgradeEffectAuthoringState effect = _state.Effects[index];
            bool remove = false;
            context.DrawInlineCard(() =>
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Effect " + (index + 1).ToString(CultureInfo.InvariantCulture), context.SectionTitleStyle);
                    if (context.DrawSecondaryButton("Remove", _state.Effects.Count > 1, GUILayout.Width(72f)))
                        remove = true;
                }

                if (remove) return;

                effect.TargetKind = (RunUpgradeAuthoringTargetKind)EditorGUILayout.EnumPopup("Target", effect.TargetKind);
                effect.ModifierType = (RunUpgradeModifierType)EditorGUILayout.EnumPopup("Modifier", effect.ModifierType);
                effect.Amount = EditorGUILayout.DoubleField("Amount", effect.Amount);
                effect.Attack = (AttackDefinitionAsset)EditorGUILayout.ObjectField("Attack", effect.Attack, typeof(AttackDefinitionAsset), false);
                effect.Weapon = (WeaponDefinitionAsset)EditorGUILayout.ObjectField("Weapon", effect.Weapon, typeof(WeaponDefinitionAsset), false);
                effect.Enemy = (EnemyDefinitionAsset)EditorGUILayout.ObjectField("Enemy", effect.Enemy, typeof(EnemyDefinitionAsset), false);
                effect.TargetIdOverride = EditorGUILayout.TextField("Target ID Override", effect.TargetIdOverride);
                effect.EffectIdOverride = EditorGUILayout.TextField("Effect ID Override", effect.EffectIdOverride);
            });

            if (remove)
                _state.Effects.RemoveAt(index);
        }
    }

    internal sealed class RunUpgradeAuthoringState
    {
        public string UpgradeId = "upgrade.example.damage-up";
        public string DisplayName = "Damage Up";
        public Sprite Icon;
        public string Description = "Increase damage for an authored attack or weapon.";
        public string TagsCsv = "upgrade, damage";
        public string OutputRoot = "Assets/GameContent/Upgrades";
        public RunUpgradeRarity Rarity = RunUpgradeRarity.Common;
        public int Weight = 5;
        public int MaxRank = 3;
        public string CostsCsv = "10, 20, 35";
        public string PrerequisitesCsv = string.Empty;
        public string ExclusionsCsv = string.Empty;
        public readonly List<RunUpgradeEffectAuthoringState> Effects = new List<RunUpgradeEffectAuthoringState>();

        public void EnsureEffects()
        {
            if (Effects.Count == 0) Effects.Add(new RunUpgradeEffectAuthoringState());
        }
    }

    internal sealed class RunUpgradeEffectAuthoringState
    {
        public RunUpgradeAuthoringTargetKind TargetKind = RunUpgradeAuthoringTargetKind.AttackDamage;
        public RunUpgradeModifierType ModifierType = RunUpgradeModifierType.Additive;
        public double Amount = 1.5d;
        public AttackDefinitionAsset Attack;
        public WeaponDefinitionAsset Weapon;
        public EnemyDefinitionAsset Enemy;
        public string TargetIdOverride = string.Empty;
        public string EffectIdOverride = string.Empty;
    }

    internal sealed class RunUpgradeGameContentPreviewController
    {
        private string _status = "Preview idle";

        public void Draw(GameContentAuthoringPreviewContext context, RunUpgradeAuthoringState state)
        {
            if (context == null) return;
            context.SetStatus(_status);

            context.DrawCard("Upgrade Preview", () =>
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (context.DrawPrimaryButton("Preview Rank Impact", true, GUILayout.Height(26f)))
                        SetStatus(context, RunUpgradeGameContentPreviewSummaries.PreviewRankImpact(state));
                    if (context.DrawSecondaryButton("Stop Preview", true, GUILayout.Width(104f), GUILayout.Height(26f)))
                        Stop(context);
                }

                context.DrawStatus(_status);
            });

            context.DrawCard("Affected Assets", () =>
            {
                context.DrawSummaryRows(RunUpgradeGameContentPreviewSummaries.BuildAffectedRows(state));
            });

            context.DrawCard("Rank Table", () =>
            {
                context.DrawTimeline(RunUpgradeGameContentPreviewSummaries.BuildRankTimeline(state));
            });

            context.DrawCard("Compatibility", () =>
            {
                context.DrawSummaryRows(RunUpgradeGameContentPreviewSummaries.BuildCompatibilityRows(state));
            });

            context.DrawWarnings(RunUpgradeGameContentPreviewSummaries.BuildWarnings(state));
        }

        public void Stop()
        {
            _status = "Preview stopped";
        }

        private void Stop(GameContentAuthoringPreviewContext context)
        {
            Stop();
            context.SetStatus(_status);
        }

        private void SetStatus(GameContentAuthoringPreviewContext context, string status)
        {
            _status = string.IsNullOrWhiteSpace(status) ? "Preview idle" : status;
            context.SetStatus(_status);
        }
    }

    internal static class RunUpgradeGameContentPreviewSummaries
    {
        public static string PreviewRankImpact(RunUpgradeAuthoringState state)
        {
            if (state == null) return "Upgrade preview unavailable: authoring state is missing.";
            state.EnsureEffects();
            return "Upgrade preview: " + state.Effects.Count.ToString(CultureInfo.InvariantCulture) + " effect(s), max rank "
                + state.MaxRank.ToString(CultureInfo.InvariantCulture) + ", total first-rank delta "
                + GetFirstAmountSummary(state) + ".";
        }

        public static IReadOnlyList<GameContentAuthoringPreviewRow> BuildAffectedRows(RunUpgradeAuthoringState state)
        {
            if (state == null) return Array.Empty<GameContentAuthoringPreviewRow>();
            state.EnsureEffects();
            var rows = new List<GameContentAuthoringPreviewRow>();
            for (int i = 0; i < state.Effects.Count; i++)
            {
                RunUpgradeEffectRecipe effect = ToRecipe(state.Effects[i]);
                rows.Add(Row("Effect " + (i + 1).ToString(CultureInfo.InvariantCulture), effect.GetEffectId()));
                rows.Add(Row("Target", string.IsNullOrWhiteSpace(effect.GetTargetId()) ? "Not assigned" : effect.GetTargetId()));
            }

            return rows;
        }

        public static IReadOnlyList<GameContentAuthoringPreviewTimelineItem> BuildRankTimeline(RunUpgradeAuthoringState state)
        {
            if (state == null) return Array.Empty<GameContentAuthoringPreviewTimelineItem>();
            int[] costs = RunUpgradeDefinitionAssetCreator.ParseCosts(state.CostsCsv);
            int maxRank = Math.Max(1, state.MaxRank);
            var rows = new List<GameContentAuthoringPreviewTimelineItem>();
            for (int rank = 1; rank <= maxRank; rank++)
            {
                int cost = costs.Length == 0 ? 0 : costs[Math.Min(rank - 1, costs.Length - 1)];
                string detail = "Cost " + cost.ToString(CultureInfo.InvariantCulture) + ", cumulative effect x" + rank.ToString(CultureInfo.InvariantCulture);
                rows.Add(new GameContentAuthoringPreviewTimelineItem("Rank " + rank.ToString(CultureInfo.InvariantCulture), cost.ToString(CultureInfo.InvariantCulture), detail));
            }

            return rows;
        }

        public static IReadOnlyList<GameContentAuthoringPreviewRow> BuildCompatibilityRows(RunUpgradeAuthoringState state)
        {
            if (state == null) return Array.Empty<GameContentAuthoringPreviewRow>();
            return new[]
            {
                Row("Rarity", state.Rarity.ToString()),
                Row("Weight", state.Weight.ToString(CultureInfo.InvariantCulture)),
                Row("Prerequisites", string.IsNullOrWhiteSpace(state.PrerequisitesCsv) ? "None" : state.PrerequisitesCsv),
                Row("Exclusions", string.IsNullOrWhiteSpace(state.ExclusionsCsv) ? "None" : state.ExclusionsCsv),
                Row("Runtime", "Converts to pure RunUpgradeDefinition; display text, icons, tags, and costs stay authoring metadata.")
            };
        }

        public static IReadOnlyList<string> BuildWarnings(RunUpgradeAuthoringState state)
        {
            if (state == null) return new[] { "Upgrade preview state is missing." };
            state.EnsureEffects();
            var warnings = new List<string>();
            if (state.MaxRank <= 0) warnings.Add("Max rank must be greater than zero.");
            if (state.Weight <= 0) warnings.Add("Draft weight must be greater than zero.");
            if (RunUpgradeDefinitionAssetCreator.HasInvalidCostToken(state.CostsCsv)) warnings.Add("Per-rank costs contain invalid values; use comma-separated whole numbers.");
            int[] costs = RunUpgradeDefinitionAssetCreator.ParseCosts(state.CostsCsv);
            if (costs.Length > 0 && costs.Length != Math.Max(1, state.MaxRank)) warnings.Add("Per-rank cost count does not match max rank; last cost repeats in preview.");
            for (int i = 0; i < state.Effects.Count; i++)
            {
                RunUpgradeEffectRecipe recipe = ToRecipe(state.Effects[i]);
                if (string.IsNullOrWhiteSpace(recipe.GetTargetId()))
                    warnings.Add("Effect " + (i + 1).ToString(CultureInfo.InvariantCulture) + " has no affected asset or target ID.");
                if (string.IsNullOrWhiteSpace(recipe.GetEffectId()))
                    warnings.Add("Effect " + (i + 1).ToString(CultureInfo.InvariantCulture) + " has no effect ID.");
            }

            return warnings;
        }

        private static string GetFirstAmountSummary(RunUpgradeAuthoringState state)
        {
            if (state.Effects.Count == 0) return "0";
            return state.Effects[0].Amount.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static RunUpgradeEffectRecipe ToRecipe(RunUpgradeEffectAuthoringState state)
        {
            return RunUpgradeDefinitionAssetCreator.ToRecipe(state);
        }

        private static GameContentAuthoringPreviewRow Row(string label, string value)
        {
            return new GameContentAuthoringPreviewRow(label, value);
        }
    }

    internal static class RunUpgradeDefinitionAssetCreator
    {
        private const string DefaultRoot = "Assets/GameContent/Upgrades";

        public static RunUpgradeDefinitionAsset BuildTransient(RunUpgradeAuthoringState state)
        {
            return BuildRecipe(state, true);
        }

        public static GameContentAuthoringValidationResult ValidateForCreation(RunUpgradeAuthoringState state, RunUpgradeDefinitionAsset recipe)
        {
            var issues = ToSharedIssues(RunUpgradeDefinitionValidator.Validate(recipe));
            string folder = GetUpgradeFolder(state);
            string rootPath = GetRootPath(state);
            GameContentAuthoringEditorAssets.AddPathIssues(issues, state.OutputRoot, DefaultRoot, folder, rootPath, "Upgrade", "OutputRoot");
            AddCostInputIssues(issues, state.CostsCsv);
            if (GameContentAuthoringEditorAssets.HasDuplicateId<RunUpgradeDefinitionAsset>(state.UpgradeId, asset => asset.Id))
                issues.Add(GameContentAuthoringValidationIssue.Error("Upgrade.Id", "Upgrade IDs must be unique. Rename this upgrade or edit the existing asset instead of creating another."));
            return new GameContentAuthoringValidationResult(issues);
        }

        public static IReadOnlyList<string> GetPreviewLines(RunUpgradeAuthoringState state)
        {
            return new[]
            {
                "Folder: " + GetUpgradeFolder(state),
                "Root asset: " + GetFileStem(state) + "_RunUpgradeDefinition.asset",
                "Sections: Economy, Effects",
                "Runtime: converts to RunUpgradeDefinition with explicit effect and target IDs.",
                "Metadata: icon, description, tags, and per-rank costs stay available to gameplay UI."
            };
        }

        public static GameContentCreationResult CreateAssets(RunUpgradeAuthoringState state)
        {
            RunUpgradeDefinitionAsset preview = BuildRecipe(state, true);
            GameContentAuthoringValidationResult report;
            try
            {
                report = ValidateForCreation(state, preview);
                if (!report.IsValid)
                    return new GameContentCreationResult(false, "Fix validation errors before creating assets.", null);
            }
            finally
            {
                DestroyTransient(preview);
            }

            string folder = GetUpgradeFolder(state);
            string rootPath = GetRootPath(state);
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(rootPath) != null)
                return new GameContentCreationResult(false, "Asset already exists: " + rootPath, null);
            if (AssetDatabase.IsValidFolder(folder) && GameContentAuthoringEditorPaths.FolderContainsAssets(folder))
            {
                bool confirmed = GameContentAuthoringEditorAssets.ConfirmExistingFolder(folder, "Upgrade");
                if (!confirmed)
                    return new GameContentCreationResult(false, "Creation canceled before writing into existing folder.", null);
            }

            folder = GameContentAuthoringEditorPaths.EnsureFolder(folder, DefaultRoot);
            RunUpgradeDefinitionAsset root = BuildRecipe(state, false);
            AssetDatabase.CreateAsset(root, rootPath);
            GameContentAuthoringEditorAssets.AddSubAsset(root.Economy, root, GetFileStem(state) + "_Economy");
            GameContentAuthoringEditorAssets.AddSubAsset(root.Effects, root, GetFileStem(state) + "_Effects");
            root.Configure(state.UpgradeId, state.DisplayName, state.Icon, state.Description, GameContentAuthoringEditorAssets.SplitCsv(state.TagsCsv), root.Economy, root.Effects);
            EditorUtility.SetDirty(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return new GameContentCreationResult(true, "Created upgrade definition at " + rootPath, AssetDatabase.LoadAssetAtPath<RunUpgradeDefinitionAsset>(rootPath));
        }

        public static void DestroyTransient(RunUpgradeDefinitionAsset recipe)
        {
            if (recipe == null || recipe.hideFlags != HideFlags.HideAndDontSave) return;
            RunUpgradeEconomyDefinitionAsset economy = recipe.Economy;
            RunUpgradeEffectsDefinitionAsset effects = recipe.Effects;
            GameContentAuthoringEditorAssets.DestroyTransientObject(economy);
            GameContentAuthoringEditorAssets.DestroyTransientObject(effects);
            GameContentAuthoringEditorAssets.DestroyTransientObject(recipe);
        }

        public static RunUpgradeEffectRecipe ToRecipe(RunUpgradeEffectAuthoringState state)
        {
            return new RunUpgradeEffectRecipe(state.TargetKind, state.ModifierType, state.Amount, state.Attack, state.Weapon, state.Enemy, state.TargetIdOverride, state.EffectIdOverride);
        }

        public static int[] ParseCosts(string csv)
        {
            string[] values = GameContentAuthoringEditorAssets.SplitCsv(csv);
            var costs = new List<int>();
            for (int i = 0; i < values.Length; i++)
                if (int.TryParse(values[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int cost))
                    costs.Add(cost);
            return costs.ToArray();
        }

        public static bool HasInvalidCostToken(string csv)
        {
            string[] values = GameContentAuthoringEditorAssets.SplitCsv(csv);
            for (int i = 0; i < values.Length; i++)
                if (!int.TryParse(values[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    return true;
            return false;
        }

        private static void AddCostInputIssues(List<GameContentAuthoringValidationIssue> issues, string csv)
        {
            string[] values = GameContentAuthoringEditorAssets.SplitCsv(csv);
            for (int i = 0; i < values.Length; i++)
            {
                if (!int.TryParse(values[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    issues.Add(GameContentAuthoringValidationIssue.Error("Economy.Costs", "Per-rank costs must be comma-separated whole numbers."));
                    return;
                }
            }
        }

        private static RunUpgradeDefinitionAsset BuildRecipe(RunUpgradeAuthoringState state, bool transient)
        {
            state.EnsureEffects();
            var economy = ScriptableObject.CreateInstance<RunUpgradeEconomyDefinitionAsset>();
            var effects = ScriptableObject.CreateInstance<RunUpgradeEffectsDefinitionAsset>();
            var root = ScriptableObject.CreateInstance<RunUpgradeDefinitionAsset>();
            if (transient)
            {
                economy.hideFlags = HideFlags.HideAndDontSave;
                effects.hideFlags = HideFlags.HideAndDontSave;
                root.hideFlags = HideFlags.HideAndDontSave;
            }

            economy.Configure(state.Rarity, state.Weight, state.MaxRank, ParseCosts(state.CostsCsv));
            var recipes = new List<RunUpgradeEffectRecipe>();
            for (int i = 0; i < state.Effects.Count; i++)
                recipes.Add(ToRecipe(state.Effects[i]));
            effects.Configure(recipes, GameContentAuthoringEditorAssets.SplitCsv(state.PrerequisitesCsv), GameContentAuthoringEditorAssets.SplitCsv(state.ExclusionsCsv));
            root.Configure(state.UpgradeId, state.DisplayName, state.Icon, state.Description, GameContentAuthoringEditorAssets.SplitCsv(state.TagsCsv), economy, effects);
            return root;
        }

        private static string GetUpgradeFolder(RunUpgradeAuthoringState state)
        {
            string root = GameContentAuthoringEditorPaths.NormalizeAssetFolderPath(state.OutputRoot, DefaultRoot);
            return root.TrimEnd('/') + "/" + GetFileStem(state);
        }

        private static string GetRootPath(RunUpgradeAuthoringState state)
        {
            return GetUpgradeFolder(state) + "/" + GetFileStem(state) + "_RunUpgradeDefinition.asset";
        }

        private static string GetFileStem(RunUpgradeAuthoringState state)
        {
            return GameContentAuthoringEditorPaths.SanitizePathSegment(state.UpgradeId, "NewUpgrade");
        }

        private static List<GameContentAuthoringValidationIssue> ToSharedIssues(RunUpgradeDefinitionValidationReport report)
        {
            var issues = new List<GameContentAuthoringValidationIssue>();
            if (report == null) return issues;
            for (int i = 0; i < report.Issues.Count; i++)
            {
                RunUpgradeDefinitionValidationIssue issue = report.Issues[i];
                GameContentAuthoringValidationSeverity severity = issue.Severity == RunUpgradeDefinitionValidationSeverity.Error
                    ? GameContentAuthoringValidationSeverity.Error
                    : issue.Severity == RunUpgradeDefinitionValidationSeverity.Warning
                        ? GameContentAuthoringValidationSeverity.Warning
                        : GameContentAuthoringValidationSeverity.Info;
                issues.Add(new GameContentAuthoringValidationIssue(severity, issue.Path, issue.Message));
            }

            return issues;
        }
    }
}
