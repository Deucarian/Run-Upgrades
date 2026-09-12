// <deucarian-definition schema="upgrades" />
// Editable declaration. Use the package definition editor or edit the values below.
namespace Deucarian.ProjectDefinitions.Definition_upgrades
{
    public static class Definition_UpgradeSamplePower
    {
        public static global::Deucarian.RunUpgrades.Editor.Definitions.RunUpgradeDefinitionSpec Value =>
        // definition-value
        new global::Deucarian.RunUpgrades.Editor.Definitions.RunUpgradeDefinitionSpec
        {
            Description = "Increase authored combat output.",
            Economy = new global::Deucarian.RunUpgrades.Editor.Definitions.RunUpgradeEconomyDefinitionSpec
            {
                Costs = new global::System.Int32[]
                {
                },
                MaxRank = 3,
                Rarity = global::Deucarian.RunUpgrades.RunUpgradeRarity.Common,
                Weight = 5,
            },
            Effects = new global::Deucarian.RunUpgrades.Editor.Definitions.RunUpgradeEffectsDefinitionSpec
            {
                Effects = new global::Deucarian.RunUpgrades.Editor.Definitions.RunUpgradeEffectSpec[]
                {
                    new global::Deucarian.RunUpgrades.Editor.Definitions.RunUpgradeEffectSpec
                    {
                        Amount = 2d,
                        Attack = global::Deucarian.Editor.Definitions.DeucarianDefinitionAssets.Load<global::Deucarian.Attacks.Authoring.AttackDefinitionAsset>("2cc39080dfae04a4ca6171121c9d07c4", 11400000L),
                        EffectIdOverride = "",
                        Enemy = null,
                        ModifierType = global::Deucarian.RunUpgrades.Authoring.RunUpgradeModifierType.Additive,
                        TargetIdOverride = "",
                        TargetKind = global::Deucarian.RunUpgrades.Authoring.RunUpgradeAuthoringTargetKind.AttackDamage,
                        Weapon = null,
                    },
                },
                Exclusions = new global::System.String[]
                {
                },
                Prerequisites = new global::System.String[]
                {
                },
            },
            Icon = null,
            Id = "92ece89b0b9549d795e2c67a6a85483a",
            Name = "UpgradeSamplePower",
            Tags = new global::System.String[]
            {
            },
        };
        // end-definition-value
    }
}
