# Deucarian Run Upgrades

`com.deucarian.run-upgrades` owns deterministic roguelite run upgrade drafting and selected-upgrade state.

It does not apply effects. Games translate selected `RunUpgradeEffectDescriptor` values through their own adapters into stats, weapon configuration, Auto Defense modules, Tower Defense towers, or other package APIs.

Runtime dependency: `com.deucarian.gameplay-foundation`.

The package is suitable for Idle Auto Defense and classic Tower Defense because it drafts abstract choices and returns explicit effect descriptors; games decide how those effects mutate their own runtime state.
