using System;
using System.Collections.Generic;
using Deucarian.Attacks.Authoring;
using Deucarian.WeaponSystems.Authoring;
using UnityEngine;

namespace Deucarian.RunUpgrades.Authoring
{
    public enum RunUpgradeAuthoringTargetKind
    {
        AttackDamage = 0,
        AttackRate = 1,
        ProjectileSpeed = 2,
        Range = 3,
        EnemyReward = 4,
        WeaponStat = 5,
        StatusEffectPower = 6,
        StatusEffectDuration = 7
    }

    public enum RunUpgradeModifierType
    {
        Additive = 0,
        Multiplicative = 1,
        SetValue = 2
    }

    [Serializable]
    public sealed class RunUpgradeEffectRecipe
    {
        [SerializeField] private RunUpgradeAuthoringTargetKind _targetKind = RunUpgradeAuthoringTargetKind.AttackDamage;
        [SerializeField] private RunUpgradeModifierType _modifierType = RunUpgradeModifierType.Additive;
        [SerializeField] private double _amount = 1d;
        [SerializeField] private AttackDefinitionAsset _attack;
        [SerializeField] private WeaponDefinitionAsset _weapon;
        [SerializeField] private EnemyDefinitionAsset _enemy;
        [SerializeField] private string _targetIdOverride = string.Empty;
        [SerializeField] private string _effectIdOverride = string.Empty;

        public RunUpgradeAuthoringTargetKind TargetKind => _targetKind;
        public RunUpgradeModifierType ModifierType => _modifierType;
        public double Amount => _amount;
        public AttackDefinitionAsset Attack => _attack;
        public WeaponDefinitionAsset Weapon => _weapon;
        public EnemyDefinitionAsset Enemy => _enemy;
        public string TargetIdOverride => _targetIdOverride ?? string.Empty;
        public string EffectIdOverride => _effectIdOverride ?? string.Empty;

        public RunUpgradeEffectRecipe()
        {
        }

        public RunUpgradeEffectRecipe(
            RunUpgradeAuthoringTargetKind targetKind,
            RunUpgradeModifierType modifierType,
            double amount,
            AttackDefinitionAsset attack = null,
            WeaponDefinitionAsset weapon = null,
            EnemyDefinitionAsset enemy = null,
            string targetIdOverride = "",
            string effectIdOverride = "")
        {
            _targetKind = targetKind;
            _modifierType = modifierType;
            _amount = amount;
            _attack = attack;
            _weapon = weapon;
            _enemy = enemy;
            _targetIdOverride = targetIdOverride ?? string.Empty;
            _effectIdOverride = effectIdOverride ?? string.Empty;
        }

        public void Configure(
            RunUpgradeAuthoringTargetKind targetKind,
            RunUpgradeModifierType modifierType,
            double amount,
            AttackDefinitionAsset attack,
            WeaponDefinitionAsset weapon,
            EnemyDefinitionAsset enemy,
            string targetIdOverride,
            string effectIdOverride)
        {
            _targetKind = targetKind;
            _modifierType = modifierType;
            _amount = amount;
            _attack = attack;
            _weapon = weapon;
            _enemy = enemy;
            _targetIdOverride = targetIdOverride ?? string.Empty;
            _effectIdOverride = effectIdOverride ?? string.Empty;
        }

        public RunUpgradeEffectDescriptor ToRuntimeDescriptor()
        {
            return new RunUpgradeEffectDescriptor(
                new RunUpgradeEffectId(GetEffectId()),
                new RunUpgradeTargetId(GetTargetId()),
                _amount);
        }

        public string GetTargetId()
        {
            if (!string.IsNullOrWhiteSpace(_targetIdOverride)) return _targetIdOverride.Trim();
            if (_weapon != null) return _weapon.Id;
            if (_attack != null) return _attack.Id;
            if (_enemy != null) return _enemy.Id;
            return string.Empty;
        }

        public string GetEffectId()
        {
            if (!string.IsNullOrWhiteSpace(_effectIdOverride)) return _effectIdOverride.Trim();
            switch (_targetKind)
            {
                case RunUpgradeAuthoringTargetKind.AttackDamage:
                    return _modifierType == RunUpgradeModifierType.Multiplicative ? "template.direct.damage_multiplier" : "template.direct.damage_bonus";
                case RunUpgradeAuthoringTargetKind.AttackRate:
                    return "template.weapon.fire_rate_intent";
                case RunUpgradeAuthoringTargetKind.ProjectileSpeed:
                    return "template.projectile.speed_multiplier";
                case RunUpgradeAuthoringTargetKind.Range:
                    return "template.weapon.range_intent";
                case RunUpgradeAuthoringTargetKind.EnemyReward:
                    return "template.reward.credits_multiplier";
                case RunUpgradeAuthoringTargetKind.WeaponStat:
                    return _modifierType == RunUpgradeModifierType.SetValue ? "template.weapon.stat_set_intent" : "template.weapon.stat_intent";
                case RunUpgradeAuthoringTargetKind.StatusEffectPower:
                    return "template.status.power_intent";
                case RunUpgradeAuthoringTargetKind.StatusEffectDuration:
                    return "template.status.duration_intent";
                default:
                    return string.Empty;
            }
        }
    }

    public sealed class RunUpgradeEffectsDefinitionAsset : ScriptableObject
    {
        [SerializeField] private RunUpgradeEffectRecipe[] _effects = Array.Empty<RunUpgradeEffectRecipe>();
        [SerializeField] private string[] _prerequisites = Array.Empty<string>();
        [SerializeField] private string[] _exclusions = Array.Empty<string>();

        public IReadOnlyList<RunUpgradeEffectRecipe> Effects => _effects ?? Array.Empty<RunUpgradeEffectRecipe>();
        public IReadOnlyList<string> Prerequisites => _prerequisites ?? Array.Empty<string>();
        public IReadOnlyList<string> Exclusions => _exclusions ?? Array.Empty<string>();

        public void Configure(IReadOnlyList<RunUpgradeEffectRecipe> effects, IReadOnlyList<string> prerequisites, IReadOnlyList<string> exclusions)
        {
            _effects = CopyEffects(effects);
            _prerequisites = CopyIds(prerequisites);
            _exclusions = CopyIds(exclusions);
        }

        public RunUpgradeEffectDescriptor[] CreateRuntimeEffects()
        {
            if (_effects == null || _effects.Length == 0) return Array.Empty<RunUpgradeEffectDescriptor>();
            var descriptors = new RunUpgradeEffectDescriptor[_effects.Length];
            for (int i = 0; i < _effects.Length; i++)
                descriptors[i] = _effects[i].ToRuntimeDescriptor();
            return descriptors;
        }

        public RunUpgradeId[] CreatePrerequisites()
        {
            return CreateIds(_prerequisites);
        }

        public RunUpgradeId[] CreateExclusions()
        {
            return CreateIds(_exclusions);
        }

        private static RunUpgradeEffectRecipe[] CopyEffects(IReadOnlyList<RunUpgradeEffectRecipe> source)
        {
            if (source == null || source.Count == 0) return Array.Empty<RunUpgradeEffectRecipe>();
            var copy = new RunUpgradeEffectRecipe[source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                RunUpgradeEffectRecipe item = source[i];
                copy[i] = item == null
                    ? null
                    : new RunUpgradeEffectRecipe(item.TargetKind, item.ModifierType, item.Amount, item.Attack, item.Weapon, item.Enemy, item.TargetIdOverride, item.EffectIdOverride);
            }

            return copy;
        }

        private static string[] CopyIds(IReadOnlyList<string> source)
        {
            if (source == null || source.Count == 0) return Array.Empty<string>();
            var ids = new List<string>();
            for (int i = 0; i < source.Count; i++)
            {
                string value = source[i];
                if (!string.IsNullOrWhiteSpace(value)) ids.Add(value.Trim());
            }

            return ids.ToArray();
        }

        private static RunUpgradeId[] CreateIds(IReadOnlyList<string> ids)
        {
            if (ids == null || ids.Count == 0) return Array.Empty<RunUpgradeId>();
            var result = new RunUpgradeId[ids.Count];
            for (int i = 0; i < ids.Count; i++)
                result[i] = new RunUpgradeId(ids[i]);
            return result;
        }
    }
}
