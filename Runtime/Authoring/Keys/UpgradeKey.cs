using System;
using UnityEngine;

namespace Deucarian.RunUpgrades
{
    /// <summary>A declared Upgrade identity. Reuse a named definition or select it in the Inspector.</summary>
    [Serializable]
    public class UpgradeKey : IUpgradeKey, IEquatable<UpgradeKey>
    {
        [SerializeField] private string definitionId;

        /// <summary>For central definition sets and generated declarations; ordinary callers reuse those keys.</summary>
        protected UpgradeKey(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id != id.Trim())
                throw new ArgumentException("A UpgradeKey definition needs a non-empty stable ID without surrounding whitespace.", nameof(id));
            definitionId = id;
        }

        public string Id => !string.IsNullOrWhiteSpace(definitionId) ? definitionId :
            throw new InvalidOperationException("No UpgradeKey is selected. Select an existing definition in the Inspector or assign a named key from a UpgradeKeySet declaration.");
        public bool Equals(UpgradeKey other) => other != null && string.Equals(definitionId, other.definitionId, StringComparison.Ordinal);
        public override bool Equals(object other) => other is UpgradeKey key && Equals(key);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(definitionId ?? string.Empty);
        public override string ToString() => definitionId ?? string.Empty;
    }
}
