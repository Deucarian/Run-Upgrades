using System;
using Deucarian.Attacks.Authoring;
using Deucarian.Editor;
using Deucarian.GameContentAuthoring.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using Deucarian.WeaponSystems.Authoring;
using Deucarian.RunUpgrades.Authoring;

namespace Deucarian.RunUpgrades.Editor
{
    internal static class RunUpgradeToolkitAuthoring
    {
        internal static VisualElement Create(GameContentAuthoringSurfaceContext context)
        {
            var asset = context.SelectedItem?.Asset as RunUpgradeDefinitionAsset;
            return GameContentToolkitDraftEditor.Create(context,
                () => asset == null ? new RunUpgradeAuthoringState() : RunUpgradeAuthoringDraft.FromUpgradeAsset(asset),
                RunUpgradeAuthoringDraft.BuildStateFingerprint,
                state => asset == null ? RunUpgradeAuthoringSession.ValidateDraft(state) : RunUpgradeDefinitionAssetCreator.ValidateForUpdate(state, asset),
                state => asset == null ? RunUpgradeDefinitionAssetCreator.CreateAssets(state) : RunUpgradeDefinitionAssetCreator.UpdateExistingAsset(asset, state),
                (root, state) => { Fields(root, state, context); RunUpgradeToolkitPreview.Add(root, state); });
        }
        private static void Fields(VisualElement root, RunUpgradeAuthoringState state, GameContentAuthoringSurfaceContext context)
        {
            var form = new DeucarianEditorWorkspaceForm(root);
            form.Text("DisplayName", "Name", () => state.DisplayName, value => state.DisplayName = value);
            form.Enum("Rarity", "Rarity", () => state.Rarity, value => state.Rarity = value);
            form.IntegerWithSlider("Weight", "Weight", 0, 100, () => state.Weight, value => state.Weight = value);
            form.Asset("Icon", "Icon", typeof(Sprite), () => state.Icon, value => state.Icon = (Sprite)value);
            var advanced = form.Section("Advanced data", true);
            advanced.Text("UpgradeId", "Upgrade Id", () => state.UpgradeId, value => state.UpgradeId = value);
            advanced.Text("Description", "Description", () => state.Description, value => state.Description = value);
            advanced.Text("TagsCsv", "Tags Csv", () => state.TagsCsv, value => state.TagsCsv = value);
            advanced.Text("OutputRoot", "Output Root", () => state.OutputRoot, value => state.OutputRoot = value);
            advanced.Integer("MaxRank", "Max Rank", () => state.MaxRank, value => state.MaxRank = value);
            advanced.Text("CostsCsv", "Costs Csv", () => state.CostsCsv, value => state.CostsCsv = value);
            advanced.Text("PrerequisitesCsv", "Prerequisites Csv", () => state.PrerequisitesCsv, value => state.PrerequisitesCsv = value);
            advanced.Text("ExclusionsCsv", "Exclusions Csv", () => state.ExclusionsCsv, value => state.ExclusionsCsv = value);
            Effects(root, state, context);
        }
        private static void Effects(VisualElement root, RunUpgradeAuthoringState state, GameContentAuthoringSurfaceContext context)
        {
            var section = new DeucarianEditorWorkspaceForm(root).Section("Effects", true);
            for (int i = 0; i < state.Effects.Count; i++)
            {
                int index = i; var item = state.Effects[i];
                var form = section.Section("Effect " + (i + 1), true);
                form.Enum("TargetKind", "Target Kind", () => item.TargetKind, value => item.TargetKind = value);
                form.Enum("ModifierType", "Modifier Type", () => item.ModifierType, value => item.ModifierType = value);
                form.Decimal("Amount", "Amount", () => item.Amount, value => item.Amount = value);
                form.Asset("Attack", "Attack", typeof(AttackDefinitionAsset), () => item.Attack, value => item.Attack = (AttackDefinitionAsset)value);
                form.Asset("Weapon", "Weapon", typeof(WeaponDefinitionAsset), () => item.Weapon, value => item.Weapon = (WeaponDefinitionAsset)value);
                form.Asset("Enemy", "Enemy", typeof(EnemyDefinitionAsset), () => item.Enemy, value => item.Enemy = (EnemyDefinitionAsset)value);
                form.Text("TargetIdOverride", "Target Id Override", () => item.TargetIdOverride, value => item.TargetIdOverride = value);
                form.Text("EffectIdOverride", "Effect Id Override", () => item.EffectIdOverride, value => item.EffectIdOverride = value);
                var up = DeucarianEditorWorkspaceControls.Button("Up", () =>
                {
                    state.Effects.RemoveAt(index); state.Effects.Insert(index - 1, item); context.RequestRepaint();
                });
                var down = DeucarianEditorWorkspaceControls.Button("Down", () =>
                {
                    state.Effects.RemoveAt(index); state.Effects.Insert(index + 1, item); context.RequestRepaint();
                });
                up.SetEnabled(index > 0); down.SetEnabled(index < state.Effects.Count - 1);
                form.Root.Add(DeucarianEditorWorkspaceControls.EndActions(up, down,
                    DeucarianEditorWorkspaceControls.Button("Remove", () => { state.Effects.Remove(item); context.RequestRepaint(); })));
            }
            section.Action("content-add-effects", "Add effect", () =>
            {
                state.Effects.Add(new RunUpgradeEffectAuthoringState()); context.RequestRepaint();
            });
        }
    }
}
