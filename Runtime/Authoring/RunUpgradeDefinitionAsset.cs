using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.RunUpgrades.Authoring
{
    [CreateAssetMenu(menuName = "Deucarian/Upgrades/Run Upgrade Definition", fileName = "RunUpgradeDefinition")]
    public sealed class RunUpgradeDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string _id = "upgrade.example.damage-up";
        [SerializeField] private string _displayName = "Example Damage Up";
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _description = "Increase authored combat output.";
        [SerializeField] private string[] _tags = Array.Empty<string>();
        [SerializeField] private RunUpgradeEconomyDefinitionAsset _economy;
        [SerializeField] private RunUpgradeEffectsDefinitionAsset _effects;

        public string Id => _id ?? string.Empty;
        public string DisplayName => _displayName ?? string.Empty;
        public Sprite Icon => _icon;
        public string Description => _description ?? string.Empty;
        public IReadOnlyList<string> Tags => _tags ?? Array.Empty<string>();
        public RunUpgradeEconomyDefinitionAsset Economy => _economy;
        public RunUpgradeEffectsDefinitionAsset Effects => _effects;

        public void Configure(
            string id,
            string displayName,
            Sprite icon,
            string description,
            IReadOnlyList<string> tags,
            RunUpgradeEconomyDefinitionAsset economy,
            RunUpgradeEffectsDefinitionAsset effects)
        {
            _id = id ?? string.Empty;
            _displayName = displayName ?? string.Empty;
            _icon = icon;
            _description = description ?? string.Empty;
            _tags = CopyTags(tags);
            _economy = economy;
            _effects = effects;
        }

        public RunUpgradeDefinition ToRuntimeDefinition()
        {
            if (_economy == null) throw new InvalidOperationException("Upgrade definition has no economy section.");
            if (_effects == null) throw new InvalidOperationException("Upgrade definition has no effects section.");
            return new RunUpgradeDefinition(
                new RunUpgradeId(Id),
                _economy.Rarity,
                _economy.Weight,
                _economy.MaxRank,
                _effects.CreateRuntimeEffects(),
                _effects.CreatePrerequisites(),
                _effects.CreateExclusions());
        }

        public static RunUpgradeDefinitionAsset CreateTransient(
            string id,
            string displayName,
            RunUpgradeRarity rarity,
            int weight,
            int maxRank,
            IReadOnlyList<RunUpgradeEffectRecipe> effects,
            int[] costs = null,
            string description = "",
            IReadOnlyList<string> tags = null,
            IReadOnlyList<string> prerequisites = null,
            IReadOnlyList<string> exclusions = null)
        {
            var economy = CreateInstance<RunUpgradeEconomyDefinitionAsset>();
            economy.hideFlags = HideFlags.HideAndDontSave;
            economy.Configure(rarity, weight, maxRank, costs ?? Array.Empty<int>());

            var effectsSection = CreateInstance<RunUpgradeEffectsDefinitionAsset>();
            effectsSection.hideFlags = HideFlags.HideAndDontSave;
            effectsSection.Configure(effects, prerequisites, exclusions);

            var root = CreateInstance<RunUpgradeDefinitionAsset>();
            root.hideFlags = HideFlags.HideAndDontSave;
            root.Configure(id, displayName, null, description, tags ?? Array.Empty<string>(), economy, effectsSection);
            return root;
        }

        private static string[] CopyTags(IReadOnlyList<string> tags)
        {
            if (tags == null || tags.Count == 0) return Array.Empty<string>();
            var copy = new List<string>();
            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];
                if (!string.IsNullOrWhiteSpace(tag)) copy.Add(tag.Trim());
            }

            return copy.ToArray();
        }
    }
}
