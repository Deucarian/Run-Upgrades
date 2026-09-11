using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.RunUpgrades.Authoring;
using Deucarian.Attacks.Authoring;
using Deucarian.WeaponSystems.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.RunUpgrades.Editor.Definitions
{
    public sealed class RunUpgradeDefinitionSchema : DeucarianSerializedDefinitionSchema<RunUpgradeDefinitionAsset, RunUpgradeDefinitionSpec>
    {
        public override string Id => "upgrades";
        public override string DisplayName => "Run upgrades";
        protected override string IdPath => "_id";
        protected override string NamePath => "_displayName";
        public override void ValidateAssetReady(ScriptableObject asset)
        {
            base.ValidateAssetReady(asset);
            var issues = RunUpgradeDefinitionValidator.Validate((RunUpgradeDefinitionAsset)asset).Issues.Where(x => x.IsError).Select(x => x.Path + ": " + x.Message).ToArray();
            if (issues.Length > 0) throw new InvalidOperationException("Complete Run upgrades definition '" + asset.name + "' in Definitions: " + string.Join("; ", issues));
        }
        public override void RefreshCatalog(bool validateOnly = false)
        {
            var definitions = AssetDatabase.FindAssets("t:RunUpgradeDefinitionAsset", new[] { "Assets" })
                .Select(x => AssetDatabase.LoadAssetAtPath<RunUpgradeDefinitionAsset>(AssetDatabase.GUIDToAssetPath(x)))
                .Where(x => x != null).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("RunUpgrade definition IDs must be unique.");
            DeucarianDefinitionCatalog.Update<RunUpgradeDefinitionCatalog>("Assets/DeucarianDefinitions/Resources/Deucarian/Definitions/RunUpgradeDefinitionCatalog.asset", "definitions", definitions, validateOnly);
        }
        [MenuItem("Assets/Create/Deucarian/Upgrades/Run Upgrade Definition")]
        private static void CreateDefinition() { Selection.activeObject = DeucarianDefinitionSync.Create(new RunUpgradeDefinitionSchema(), "NewRunUpgrade"); }
    }
}
