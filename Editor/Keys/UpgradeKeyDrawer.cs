using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.RunUpgrades.Editor
{
    [CustomPropertyDrawer(typeof(UpgradeKey), true)]
    public sealed class UpgradeKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(UpgradeKey);
        public override Type DefinitionSetAttribute => typeof(UpgradeKeySetAttribute);
        public override string SetupHint => "Select an existing UpgradeKey; declare reusable keys once in a [UpgradeKeySet] class.";
    }
}
