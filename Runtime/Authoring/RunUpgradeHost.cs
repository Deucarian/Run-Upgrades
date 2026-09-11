using Deucarian.Diagnostics;
using System;
using UnityEngine;

namespace Deucarian.RunUpgrades.Authoring
{
    [DisallowMultipleComponent]
    public sealed class RunUpgradeHost : MonoBehaviour, IDiagnosticProvider
    {
        private RunUpgradeProfile profile;
        private bool destroyed;
        public void Configure(RunUpgradeProfile value)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(RunUpgradeHost));
            if (profile != null) throw new InvalidOperationException("RunUpgradeHost '" + name + "' is already configured.");
            profile = value ?? throw new ArgumentNullException(nameof(value));
        }
        public int GetRank(UpgradeKey upgrade) => Profile.GetRank(upgrade);
        public UpgradeDraft Draft(int count = 3) => Profile.Draft(count);
        public RunUpgradeSelectionResult Select(UpgradeChoiceHandle choice) => Profile.Select(choice);
        private RunUpgradeProfile Profile => profile ?? throw new InvalidOperationException("RunUpgradeHost '" + name + "' is not configured. Supply this run's RunUpgradeProfile during startup.");
        private void OnDestroy() { diagnosticRegistration?.Dispose(); diagnosticRegistration = null;  destroyed = true; profile = null; }
        private DiagnosticProviderRegistration diagnosticRegistration;
        private void Awake() => diagnosticRegistration = DiagnosticProviderRegistry.Register(this);
        string IDiagnosticProvider.ProviderId => "run-upgrades.host." + GetInstanceID();
        string IDiagnosticProvider.DisplayName => "RunUpgradeHost";
        void IDiagnosticProvider.Collect(DiagnosticReportBuilder builder)
        {
            bool configured = profile != null;
            builder.AddSection(((IDiagnosticProvider)this).ProviderId, "RunUpgradeHost")
                .AddItem("configured", "Configured", configured ? "Ready" : "Call Configure during startup",
                    configured ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning)
                .AddItem("enabled", "Enabled", isActiveAndEnabled.ToString());
        }
    }
}
