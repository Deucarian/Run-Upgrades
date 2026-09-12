using System;
using UnityEngine;

namespace Deucarian.RunUpgrades.Authoring.Samples.DefinitionWorkflow
{
    /// <summary>Small caller example. The configured scene hosts own services and resource lifetimes.</summary>
    public sealed class RunUpgradesWorkflow : MonoBehaviour
    {
        [SerializeField] private UpgradeDraftTrigger trigger;
        private string status = "Ready. Choose an action below.";
        public string Status => status;
        public void Draft() { trigger.Draft(); status = "Choices: " + trigger.Current.Choices.Count; }
        public void Select() { trigger.Select(0); status = "Selection: " + trigger.LastSelection.Status; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, Math.Min(540, Screen.width - 48), Screen.height - 48), GUI.skin.box);
            GUILayout.Label("Run-Upgrades — definition workflow");
            GUILayout.Label("Definitions describe reusable upgrades. Offered choices are runtime handles: selecting an old or already consumed offer is rejected by the same core API.");
            GUILayout.Space(12);
            if (GUILayout.Button("Draft upgrades", GUILayout.Height(32))) { try { Draft(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Select first offered upgrade", GUILayout.Height(32))) { try { Select(); } catch (Exception error) { status = error.Message; } }
            GUILayout.Space(12);
            GUILayout.Label(status);
            GUILayout.EndArea();
        }
    }
}
