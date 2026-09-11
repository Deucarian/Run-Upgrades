namespace Deucarian.RunUpgrades.Samples.SimpleUsage
{
    [UpgradeKeySet]
    public static class Upgrades
    {
        public static UpgradeKey Damage => new Definition();
        private sealed class Definition : UpgradeKey
        {
            public Definition() : base("sample.damage") { }
        }
    }
}
