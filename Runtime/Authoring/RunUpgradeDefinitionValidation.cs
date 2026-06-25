using System;
using System.Collections.Generic;

namespace Deucarian.RunUpgrades.Authoring
{
    public enum RunUpgradeDefinitionValidationSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    public readonly struct RunUpgradeDefinitionValidationIssue
    {
        public RunUpgradeDefinitionValidationIssue(RunUpgradeDefinitionValidationSeverity severity, string path, string message)
        {
            Severity = severity;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public RunUpgradeDefinitionValidationSeverity Severity { get; }
        public string Path { get; }
        public string Message { get; }
        public bool IsError => Severity == RunUpgradeDefinitionValidationSeverity.Error;
        public static RunUpgradeDefinitionValidationIssue Error(string path, string message) => new RunUpgradeDefinitionValidationIssue(RunUpgradeDefinitionValidationSeverity.Error, path, message);
        public static RunUpgradeDefinitionValidationIssue Warning(string path, string message) => new RunUpgradeDefinitionValidationIssue(RunUpgradeDefinitionValidationSeverity.Warning, path, message);
    }

    public sealed class RunUpgradeDefinitionValidationReport
    {
        private readonly RunUpgradeDefinitionValidationIssue[] _issues;

        public RunUpgradeDefinitionValidationReport(IReadOnlyList<RunUpgradeDefinitionValidationIssue> issues)
        {
            if (issues == null || issues.Count == 0)
            {
                _issues = Array.Empty<RunUpgradeDefinitionValidationIssue>();
                return;
            }

            _issues = new RunUpgradeDefinitionValidationIssue[issues.Count];
            for (int i = 0; i < issues.Count; i++) _issues[i] = issues[i];
        }

        public IReadOnlyList<RunUpgradeDefinitionValidationIssue> Issues => _issues;
        public bool IsValid
        {
            get
            {
                for (int i = 0; i < _issues.Length; i++)
                    if (_issues[i].IsError)
                        return false;
                return true;
            }
        }
    }

    public static class RunUpgradeDefinitionValidator
    {
        public static RunUpgradeDefinitionValidationReport Validate(RunUpgradeDefinitionAsset definition)
        {
            var issues = new List<RunUpgradeDefinitionValidationIssue>();
            if (definition == null)
            {
                issues.Add(RunUpgradeDefinitionValidationIssue.Error("Upgrade", "Upgrade definition is missing."));
                return new RunUpgradeDefinitionValidationReport(issues);
            }

            if (string.IsNullOrWhiteSpace(definition.Id)) issues.Add(RunUpgradeDefinitionValidationIssue.Error("Upgrade.Id", "Upgrade ID is required."));
            if (string.IsNullOrWhiteSpace(definition.DisplayName)) issues.Add(RunUpgradeDefinitionValidationIssue.Warning("Upgrade.DisplayName", "Display name is empty."));
            ValidateEconomy(definition.Economy, issues);
            ValidateEffects(definition.Effects, issues);
            if (issues.Count == 0)
            {
                try
                {
                    definition.ToRuntimeDefinition();
                }
                catch (Exception exception)
                {
                    issues.Add(RunUpgradeDefinitionValidationIssue.Error("Upgrade.Runtime", "Runtime conversion failed: " + exception.Message));
                }
            }

            return new RunUpgradeDefinitionValidationReport(issues);
        }

        private static void ValidateEconomy(RunUpgradeEconomyDefinitionAsset economy, List<RunUpgradeDefinitionValidationIssue> issues)
        {
            if (economy == null)
            {
                issues.Add(RunUpgradeDefinitionValidationIssue.Error("Economy", "Economy section is required."));
                return;
            }

            if (economy.Weight <= 0) issues.Add(RunUpgradeDefinitionValidationIssue.Error("Economy.Weight", "Draft weight must be greater than zero."));
            if (economy.MaxRank <= 0) issues.Add(RunUpgradeDefinitionValidationIssue.Error("Economy.MaxRank", "Max rank must be greater than zero."));
            int[] costs = economy.Costs;
            if (costs.Length > 0 && costs.Length != economy.MaxRank)
                issues.Add(RunUpgradeDefinitionValidationIssue.Warning("Economy.Costs", "Per-rank costs should either be empty or match max rank."));
            for (int i = 0; i < costs.Length; i++)
                if (costs[i] < 0)
                    issues.Add(RunUpgradeDefinitionValidationIssue.Error("Economy.Costs[" + i.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]", "Cost cannot be negative."));
        }

        private static void ValidateEffects(RunUpgradeEffectsDefinitionAsset effects, List<RunUpgradeDefinitionValidationIssue> issues)
        {
            if (effects == null)
            {
                issues.Add(RunUpgradeDefinitionValidationIssue.Error("Effects", "Effects section is required."));
                return;
            }

            if (effects.Effects.Count == 0)
                issues.Add(RunUpgradeDefinitionValidationIssue.Error("Effects", "At least one upgrade effect is required."));

            for (int i = 0; i < effects.Effects.Count; i++)
            {
                RunUpgradeEffectRecipe effect = effects.Effects[i];
                string path = "Effects[" + i.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]";
                if (effect == null)
                {
                    issues.Add(RunUpgradeDefinitionValidationIssue.Error(path, "Effect entry is empty."));
                    continue;
                }

                if (double.IsNaN(effect.Amount) || double.IsInfinity(effect.Amount))
                    issues.Add(RunUpgradeDefinitionValidationIssue.Error(path + ".Amount", "Amount must be finite."));
                if (effect.ModifierType == RunUpgradeModifierType.Multiplicative && effect.Amount <= 0d)
                    issues.Add(RunUpgradeDefinitionValidationIssue.Error(path + ".Modifier", "Multiplicative modifiers must be greater than zero."));
                if (effect.ModifierType == RunUpgradeModifierType.SetValue && effect.Amount < 0d)
                    issues.Add(RunUpgradeDefinitionValidationIssue.Error(path + ".Modifier", "Set value modifiers cannot be negative."));
                if (string.IsNullOrWhiteSpace(effect.GetEffectId()))
                    issues.Add(RunUpgradeDefinitionValidationIssue.Error(path + ".EffectId", "Effect ID is required."));
                if (string.IsNullOrWhiteSpace(effect.GetTargetId()))
                    issues.Add(RunUpgradeDefinitionValidationIssue.Error(path + ".Target", "Choose an affected asset or enter a target ID override."));
            }
        }
    }
}
