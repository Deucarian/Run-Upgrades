# API Notes

Run Upgrades is pure runtime C#.

- Definitions identify upgrades, rarity, weight, max rank, prerequisites, exclusions, and explicit effect descriptors.
- `RunUpgradeState` owns selected ranks and banished IDs.
- `RunUpgradeDraftService` generates deterministic drafts from a catalog, state, seed, reroll index, choice count, and locked choices.
- Effects are descriptors only. Application is owned by game adapters.

Effect examples:

- `stat.additive -> weapon.direct.damage`
- `sample.projectile.speed_multiplier -> projectile.basic`
- `sample.enemy.spawn_delay_ticks -> encounter.basic`

These names are examples, not package-owned semantics.
