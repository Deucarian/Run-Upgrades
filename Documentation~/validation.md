# Validation Notes

Unity version: `6000.3.5f1`.

Validated through shared project:

`C:\Repositories\Deucarian-Validation\AllPackages-TestProject`

No per-package validation project was created for Phase 1R.

## Results

Import:

- `AllPackages-TestProject-phase1r-import.log`: clean, exit code 0.

EditMode:

- Pass 1: 19 passed, 0 failed, 0 skipped, 0 inconclusive, duration 0.803 seconds.
- Pass 2: 19 passed, 0 failed, 0 skipped, 0 inconclusive, duration 0.895 seconds.

PlayMode:

- No Run Upgrades runtime PlayMode behavior is required. PlayMode validation is covered by the Auto Defense sample integration in the shared project:
  - Pass 1: 1 passed, 0 failed, duration 2.395 seconds.
  - Pass 2: 1 passed, 0 failed, duration 2.501 seconds.

## Boundary

Run Upgrades does not apply effects directly. The Basic Auto Defense sample uses a sample-owned adapter to translate descriptors into direct-damage support, projectile speed, objective healing, and enemy pacing. The runtime package has no dependency on Auto Defense, Combat, Weapon Systems, Progression, Persistence, UI, or Entities.
