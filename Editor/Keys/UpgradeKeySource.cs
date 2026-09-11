using System;
using Deucarian.Editor;
using Deucarian.RunUpgrades.Authoring;

namespace Deucarian.RunUpgrades.Editor
{
    public sealed class UpgradeKeySource : DeucarianAssetKeySource<RunUpgradeDefinitionAsset>
    {
        public override Type KeyType => typeof(UpgradeKey);
        public override Type DefinitionSetAttribute => typeof(UpgradeKeySetAttribute);
        public override string GeneratedClassName => "ProjectUpgrades";
        protected override DeucarianKeyChoice ReadDefinition(RunUpgradeDefinitionAsset asset) =>
            new DeucarianKeyChoice(asset.Id, asset.DisplayName);
    }
}
