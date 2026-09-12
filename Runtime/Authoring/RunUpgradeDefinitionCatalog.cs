using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.RunUpgrades.Authoring
{
    /// <summary>Generated project definitions; scene composition supplies the existing runtime's other dependencies.</summary>
    public sealed class RunUpgradeDefinitionCatalog : ScriptableObject
    {
        public const string ResourcePath = "Deucarian/Definitions/RunUpgradeDefinitionCatalog";
        [SerializeField] private RunUpgradeDefinitionAsset[] definitions = Array.Empty<RunUpgradeDefinitionAsset>();
        public IReadOnlyList<RunUpgradeDefinitionAsset> Definitions => Array.AsReadOnly(definitions);
        public static RunUpgradeDefinitionCatalog LoadProject() => Resources.Load<RunUpgradeDefinitionCatalog>(ResourcePath) ??
            throw new InvalidOperationException("Create a RunUpgrade definition in the Definitions editor before loading the project catalog.");
        public RunUpgradeDefinitionAsset Get(UpgradeKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key), "Select a definition in the Inspector or pass a generated key.");
            foreach (var definition in definitions) if (definition != null && definition.Id == key.Id) return definition;
            throw new InvalidOperationException("The RunUpgrade catalog does not contain '" + key.Id + "'. Synchronize this definition in the Definitions editor.");
        }
        public RunUpgradeDefinition[] CreateRuntimeDefinitions()
        {
            var result = new RunUpgradeDefinition[definitions.Length];
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < result.Length; i++)
            {
                var definition = definitions[i];
                if (definition == null || !ids.Add(definition.Id)) throw new InvalidOperationException("The RunUpgrade catalog contains a missing or duplicate definition. Synchronize it in the Definitions editor.");
                try { result[i] = definition.ToRuntimeDefinition(); }
                catch (Exception error) { throw new InvalidOperationException("Complete RunUpgrade definition '" + definition.DisplayName + "' in the Definitions editor: " + error.Message, error); }
            }
            return result;
        }
    }
}
