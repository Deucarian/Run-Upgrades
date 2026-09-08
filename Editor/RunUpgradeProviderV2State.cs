using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Deucarian.Attacks.Authoring;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.RunUpgrades.Authoring;
using Deucarian.WeaponSystems.Authoring;
using UnityEditor;
using UnityEngine;

namespace Deucarian.RunUpgrades.Editor
{
    internal sealed class RunUpgradeProviderV2State : GameContentAuthoringProviderSessionState<RunUpgradeAuthoringState>
    {
        public int SelectedRank = 1;

        public void BeginCreate()
        {
            Creating = true;
            WizardStep = 0;
            DetailScroll = Vector2.zero;
            SelectedRank = 1;
            ClearEditingState();
            PreviewStatus = "Previewing draft upgrade";
        }

        protected override void OnProviderSessionReset()
        {
            SelectedRank = 1;
        }
    }
}
