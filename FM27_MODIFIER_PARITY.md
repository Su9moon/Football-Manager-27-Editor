# FM24 Modifier Feature Parity — FM27 Port

Status: **source inventory in progress**  
Target: **Football Manager 27.0.1 Steam Windows (x64)**  
Repository baseline: `8052646d0b3ddc4879ff1135e5dea3113aa4077a`  
Working branch: `feature/fm24-modifier-parity`  
Inventory date: 2026-11-26

This file records the feature scope requested for the FM24-style modifier and the implementation evidence found in the current FM27 editor source. It is not a claim that every existing feature has been live-verified on the user's exact Steam executable.

## Target build

The repository's `AGENTS.md` lists Steam Windows build `27.0.1+2503222` and Game Pass build `27.0.1+2494687`. The user's reported version is Steam 27.0.1, but the full build suffix is not yet confirmed. Let the editor's existing build detection select the profile; do not guess or hard-code the build.

## Parity inventory

Status meanings:

- **Present** — the source has a corresponding implementation and UI entry point.
- **Partial** — a related capability exists, but it does not cover the requested behavior or full scope.
- **Missing in source** — no corresponding implementation/UI path was found in the inspected repository tree. This must be confirmed before implementation.
- **Live validation pending** — code presence does not establish behavior on the user's exact game build.

| Requested modifier capability | Current source evidence | Status / gap |
|---|---|---|
| Player development / CA growth multiplier | `Core/Presets/AcceleratedDevelopment.cs`; `App/PlayerDevelopmentSpeedView.cs`; `App/HookManagerView.cs`; `Core.Tests/AcceleratedDevelopmentTests.cs` | **Present.** Per-player training-gain multiplier, 1.1x–20x; this is not a general game-speed control. Live validation on the user's exact build pending. |
| Hidden-attribute growth multiplier | No hidden-attribute growth hook found in the inspected Presets/UI tree. Existing player fields support editing, which is not the same as accelerating growth. | **Missing in source.** Requires reverse-engineering a separate game update path; do not reuse the CA hook by assumption. |
| Board always accepts board requests / Club Vision | `App/ForceBoardRequestView.cs`; `Core/ForceAccept/ForceAcceptPlanner.cs`; `Core.Tests/ForceAcceptPlannerTests.cs`; `Core.Tests/ForceAcceptApplyTests.cs`; profile `forceAccept` section | **Partial for the requested 27.0.1 Steam target.** The planner and UI exist, but `offsets/27.0.1+2503222-steam.json` has no `forceAccept` section, so this build has no measured board/Club Vision RVAs and the feature remains unavailable. The 27.1.0 Steam and Game Pass profiles do carry measured board offsets. |
| Stadium new-build / expansion capacity | `Core/Stadiums/Stadiums.cs` includes `BuildDate`, `RebuildDate`, `Capacity`, and `ExpansionCapacity`; `App/StadiumView.cs`; `Core.Tests/StadiumTests.cs` | **Partial.** Stadium records and capacity/expansion fields can be edited. No evidence yet of creating a new stadium or bypassing the game's build/expansion workflow. |
| Green-card / work-permit bypass | `Core/Players/WorkPermit.cs`; `Core/Players/WorkPermitBatch.cs`; `App/PlayerWorkPermitView.cs`; corresponding WorkPermit tests | **Partial.** Existing grants can be added/edited/removed and denied-club rows removed. This is a direct permit-data editor, not a verified automatic eligibility bypass. |
| Never retire | `Core/Players/KeepPlaying.cs`; `Core/Players/PlannedRetirements.cs`; `App/SquadPresetView.cs`; `Core.Tests/KeepPlayingTests.cs` | **Present.** Removes planned retirement records and clears retirement flags/traits through guarded writes. Live validation pending. |
| Guaranteed job interview | No corresponding job-application/interview feature found in the inspected source tree. | **Missing in source.** |
| Always accept contract / wage / renewal offers | `Core/ForceAccept/ForceAcceptPlanner.cs` explicitly documents `ContractOffers` as **NOT IMPLEMENTED** because the behavior requires a guarded hook specific to the human manager. | **Missing / explicitly not implemented.** Do not enable the current overwrite candidate; measure and implement a guarded hook first. |
| Always accept transfer offers | `Core/Presets/ForceTransferOffersHook.cs` and `Core.Tests/ForceTransferOffersHookTests.cs` | **Partial.** Existing x64 hook forces acceptance only when the configured buying club matches the managed-club list. It is not a universal acceptance switch; exact 27.0.1 Steam live validation pending. |
| Bypass player transfer reluctance | No dedicated implementation found in the inspected source tree. | **Missing in source.** |
| Boost transfer interest | No dedicated implementation found in the inspected source tree. | **Missing in source.** |
| TransferRoom immediate response | No TransferRoom response hook or UI found in the inspected source tree. | **Missing in source.** |
| Always-positive player conversation outcomes | No conversation outcome hook or UI found in the inspected source tree. | **Missing in source.** |
| Configurable youth intake count | `Core/Clubs/YouthCandidates.cs` and `App/ClubYouthIntakeView.cs` edit the game's pre-intake candidate records. No evidence found that they resize or regenerate the candidate count. | **Partial.** Candidate editing exists; intake quantity control not found. |
| Generate manager's son | No corresponding manager-child generation implementation found in the inspected source tree. | **Missing in source.** |
| Youth preview rating control | Youth candidate CA/PA/attributes/positions can be edited before intake; no separate preview-rating field or rating recalculation control found. | **Partial / missing dedicated control.** |
| Golden Generation PA 170–200 | `Core/Presets/GoldenGenerationHook.cs`; `App/HookManagerView.cs`; GoldenGeneration tests | **Present.** Configurable PA range up to 200, optional wonderkid CA boost, scoped to all/club/nation/continent. This affects new regens, not existing players. |
| Auto-remove unhappiness | `Core/Players/Unhappiness.cs`; `App/SquadPresetView.cs`; `Core.Tests/UnhappinessTests.cs` | **Present.** Squad preset and player-level implementation exist. |
| Heal injuries | Player healer / injury readers and `SquadPresetView`'s Heal all preset; injury tests | **Present.** |
| Restore fitness / condition | `SquadPresetView.cs` fitness and heal presets; player preset tests | **Present.** Exact mapping is the project's existing preset behavior; do not conflate condition with training development. |
| Attribute cap 25 | `Core/Players/MaxAttributes.cs` explicitly sets performance attributes to **20**, leaving Dirtiness and Injury Proneness unchanged. | **Partial / not equivalent.** Current feature is Max Attributes = 20, not a configurable 25 cap. Do not change the stored scale without independent game evidence. |
| Game speed control | `Core/Game/GameClock.cs` only reads the in-game date. Development Speed is a per-player CA training-gain multiplier. No global game-time speed control found. | **Missing in source.** |

## 27.0.1 Steam profile readiness\n\nDirect inspection of the checked-in JSON profiles confirms:\n\n- `offsets/27.0.1+2503222-steam.json` exists and identifies build `27.0.1+2503222`. It has youth-candidate and stadium profiles, but **no `forceAccept` section**. The current Board Requests UI therefore cannot be considered available on this target build yet.\n- `offsets/27.1.0+2505117-steam.json` and `offsets/27.1.0+2505117-gamepass.json` include measured board-request and Club Vision RVAs/original bytes. Those values must not be copied into 27.0.1.\n- The 27.0.1 Steam profile has no contract-offer offsets either. Contract acceptance remains explicitly unimplemented in the planner.\n\nThis is the first concrete target-build gap to resolve. Measure the 27.0.1 Steam sites from its own executable and add them only after byte/signature uniqueness and runtime behavior are independently confirmed.\n\n## Implementation order

Prioritise features that can be implemented with evidence from the supported Steam 27.0.1 binary and existing guarded patch infrastructure:

1. Complete the contract-offer hook: it is explicitly identified as unimplemented in `ForceAcceptPlanner`. Measure the human-manager identity and target instruction sequence for each supported Windows build; preserve AI-to-AI offer behavior.
2. Investigate transfer reluctance / transfer-interest paths. Keep these as separate modifiers unless binary evidence proves a shared decision function.
3. Investigate positive conversation outcomes and job interview acceptance.
4. Investigate TransferRoom response timing and global game-speed control independently.
5. Extend youth intake only after identifying the candidate-vector ownership, count semantics, and safe reallocation path. Manager's-son generation is a separate creation workflow.
6. Treat stadium build creation and a true attribute cap of 25 as separate research items; current stadium editing and 1–20 attribute writes do not prove either.

For every code hook: use profile-backed signatures/offsets, require a unique match, preserve original bytes, pause the game during patch installation/removal, expose an explicit off/restore path, and fail closed on unknown bytes or unsupported builds. No hard-coded build RVAs or build-name branches.

## Verification boundary

- Unit tests can validate planners, guards, byte sequences, and fake-memory writes without a live game.
- A passing unit test does **not** prove a hook is correct in FM27.
- The user's exact Steam build suffix remains unconfirmed. Do not claim live validation until the game build is detected and the feature is checked in a throwaway save.
- Do not run memory writes against the user's real save. Any live hook/write check must use a backed-up throwaway save and explicit user approval.
- Keep release-log platform columns honest: `Checked`, `Reads`, `Not checked`, `Missing`, or `—`.
