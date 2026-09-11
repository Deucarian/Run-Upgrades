using System;
using UnityEngine;
using UnityEngine.Events;
namespace Deucarian.RunUpgrades.Authoring
{
    public sealed class UpgradeDraftTrigger : MonoBehaviour
    {
        [SerializeField] private RunUpgradeHost host;
        [SerializeField, Min(1)] private int count = 3;
        [SerializeField] private UnityEvent offered = new UnityEvent();
        [SerializeField] private UnityEvent selected = new UnityEvent();
        public UpgradeDraft Current { get; private set; }
        public RunUpgradeSelectionResult LastSelection { get; private set; }
        private RunUpgradeHost Host => host != null ? host : throw new InvalidOperationException("Assign a configured RunUpgradeHost to UpgradeDraftTrigger.");
        public void Draft() { Current = Host.Draft(count); offered.Invoke(); }
        public void Select(int index)
        {
            if (Current == null || index < 0 || index >= Current.Choices.Count) throw new InvalidOperationException("Draft first, then select an index from the current offer's Choices.");
            LastSelection = Host.Select(Current.Choices[index]);
            if (LastSelection.Status == RunUpgradeSelectionStatus.Selected) selected.Invoke();
        }
    }
}
