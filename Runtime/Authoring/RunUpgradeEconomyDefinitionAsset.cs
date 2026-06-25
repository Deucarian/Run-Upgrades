using System;
using UnityEngine;

namespace Deucarian.RunUpgrades.Authoring
{
    public sealed class RunUpgradeEconomyDefinitionAsset : ScriptableObject
    {
        [SerializeField] private RunUpgradeRarity _rarity = RunUpgradeRarity.Common;
        [SerializeField] private int _weight = 5;
        [SerializeField] private int _maxRank = 3;
        [SerializeField] private int[] _costs = Array.Empty<int>();

        public RunUpgradeRarity Rarity => _rarity;
        public int Weight => _weight;
        public int MaxRank => _maxRank;
        public int[] Costs => CopyCosts(_costs);

        public void Configure(RunUpgradeRarity rarity, int weight, int maxRank, int[] costs)
        {
            _rarity = rarity;
            _weight = weight;
            _maxRank = maxRank;
            _costs = CopyCosts(costs);
        }

        public int GetCostForRank(int rank)
        {
            if (rank <= 0) return 0;
            if (_costs == null || _costs.Length == 0) return 0;
            int index = Math.Min(rank - 1, _costs.Length - 1);
            return Math.Max(0, _costs[index]);
        }

        private static int[] CopyCosts(int[] source)
        {
            if (source == null || source.Length == 0) return Array.Empty<int>();
            var copy = new int[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }
    }
}
