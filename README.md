<p align="center"><img src="docs/images/icon.png" alt="Football Manager 27 Editor" width="160"></p>

# Football Manager 27 Editor

[![Build](https://github.com/Jackhamerp24/Football-Manager-27-Editor/actions/workflows/build.yml/badge.svg)](https://github.com/Jackhamerp24/Football-Manager-27-Editor/actions/workflows/build.yml)

English | [Tiếng Việt](README.vi.md) | [简体中文](README.zh-CN.md)

A live in-game editor for **Football Manager 27** and **Football Manager Touch** on **Windows (Steam and Xbox / Game Pass)** and **macOS Apple silicon (Steam and Touch)**. It reads and writes the memory of the running game (`fm.exe` / `game_plugin.dll` on Windows; `Football Manager 27` or `Football Manager Touch` / `game_plugin` on macOS via a small helper installed once on first connect) without modifying game files on disk.

For single-player, offline saves only. Do not use it online or in shared saves. Back up your save before editing; changes land in memory right away, and FM redraws the screen after you press **Continue**.

## How it works

Loading the player database (around 50,000 players) takes under a second. Every write goes through an edit journal with multi-step **Undo** and guarded checks:

- It refuses to attach to any game build that does not have a verified offset profile.
- Before writing, it verifies that the object's vtable and memory snapshot still match what was read earlier; after writing, it reads the value back to confirm and stops on any mismatch.
- Actions that append to game lists (languages, promises, work permits, future transfers, club vision, agreements) allocate blocks via the game's own allocator when supported so **Continue** and **Load Game** exit cleanly.

## Supported platforms and builds

Each active build is checked against live memory, calibration values, byte-exact write round-trips, game-heap allocations, and a Continue + Load Game crash gate before write access is unlocked.

| Platform | Edition | Supported builds | Notes |
|---|---|---|---|
| **Windows 10/11 (x64)** | **Steam** | **`27.1.0+2505117`**, `27.0.1+2503222`, `27.0.1+2494687`, `27.0.0+2493426`, `27.0.0+2486695` | Full read/write support, dual-role player-staff support, x64 runtime hooks |
| **Windows 10/11 (x64)** | **Xbox / Game Pass** | **`27.1.0+2505117`**, `27.0.1+2494687`, `27.0.0+2493426` | Full parity with Steam, dual-role player-staff support |
| **macOS 13+ (Apple silicon)** | **FM27 Steam** | **`27.1.0+2505117`**, `27.0.1+2503222` | NativeAOT `.dmg`, ARM64 runtime hooks, verified Continue + Load Game crash gate |
| **macOS 13+ (Apple silicon)** | **FM27 Touch** | **`27.1.0+2505719`**, `27.0.1+2503222`, `27.0.0+2492889` *(experimental)* | NativeAOT `.dmg`, `27.1.0`/`27.0.1` verified Continue + Load Game crash gate (Touch has no referee list) |

Other builds need a new offset profile; see [After a game patch](#after-a-game-patch).

## Download

Download the latest binaries from [Releases](../../releases/latest):

| File | Platform | Purpose |
|---|---|---|
| **`Fm27Editor.exe`** | **Windows (x64)** | The desktop editor app. Runs standalone from any folder with no installer or .NET required |
| **`Fm27Editor-mac-arm64.dmg`** | **macOS (Apple silicon)** | The desktop editor app for both FM27 Steam and FM27 Touch |
| `Fm27Editor.Tools.exe` | Windows (x64) | Command-line tools (`players`, `set`, `verify`, `newbuild`, `verify-build`) |

Start FM27 or FM27 Touch and load a save first:

- **Windows:** run `Fm27Editor.exe` (no administrator rights needed).
- **macOS:** open the `.dmg` and drag **FM27 Editor** to `/Applications`. The app is ad-hoc signed rather than Apple notarised, so on first launch open **System Settings → Privacy & Security → Open Anyway** (on macOS 13–14, right-click the app and choose **Open**). On first connect, macOS prompts once for your password to install `Fm27EditorHelper`.

### Build provenance and system access

Every release is built directly from this repository by the [Build workflow](.github/workflows/build.yml) on GitHub Actions; nothing is compiled locally or uploaded by hand.

- Check hashes against `SHA256SUMS.txt` (`Get-FileHash Fm27Editor.exe` on Windows or `shasum -a 256 Fm27Editor-mac-arm64.dmg` on macOS).
- Verify the signed build attestation with [GitHub CLI](https://cli.github.com):

  ```bash
  gh attestation verify Fm27Editor.exe --repo Jackhamerp24/Football-Manager-27-Editor
  gh attestation verify Fm27Editor-mac-arm64.dmg --repo Jackhamerp24/Football-Manager-27-Editor
  ```

Its only network request is a single startup check for a newer release on GitHub (nothing is sent, and it fails silently offline). The only files written on disk are app settings and logs (`%LOCALAPPDATA%\Fm27Editor` on Windows; user app support folders and `favorites.json` beside the binary) plus any exports or bug-report zips you ask for.

## What you can edit

| Area | Supported features |
|---|---|
| Players | Attributes, positions, personality classification and traits, CA/PA, reputations, height, DOB, nationality, birth city and appearance; current and future contracts, bonuses and clauses; career plans, relationships, languages, preferred moves; injuries, bans, unhappiness, national call-ups, work permits and all promise types; built-in role scores; dual-role player-staff profiles |
| Moving players | Move to club with fee and sell-on (`+0xC4` offer terms); loan; future transfer with date and full terms (instalments, appearance/goal payments, buy-back fee, sell-on, wage share, friendly) plus removal; free-agent signing; swap two players; release; captain / vice captain; attribute clone and player comparison |
| Staff | Attributes, CA/PA, personality, coach training ratings, caps and goals, coaching licence (including *"Studying to…"*), 15 role ratings, contract, career plans, relationships, languages, knowledge, formations and tendencies; move staff to another club with a new role and wage |
| Clubs | Information, facilities (including Corporate Facilities), finances, debts, affiliations, club vision, kits and colours, relationships, staff, squads and transfer centre |
| World and competitions | Nations (agreements, transfer values, preferences, Euro coefficients, squads), stadiums (owner, location, capacity, pitch size, coordinates), cities (nation, region, language, weather), competitions (VAR, prize money, squad rules, standings), awards (`AWARD_MANAGER`), injury types and referees *(full FM27 builds)* |
| Tools | Squad presets with preview and Undo (heal, fitness, morale, lift bans, remove unhappiness, work permit, Golden Generation, prevent retirement, max attributes, tactical understanding, team cohesion), Mass Edit (`Ctrl+M`), Hook Manager (Wonderkid Academy, Development Speed), Freezer, CSV/JSON and attribute export/import, Light and Dark themes, English, Vietnamese and Simplified Chinese UI (initial translation), currency conversion |

## Highlights

### Mass Edit (`Ctrl+M`)
Change hundreds of players, staff, clubs or nations in one go using the same filters as Advanced Search. Pick **Set to** or **Change by** for each field, review every old → new line item (and untick anything you want to skip), and undo either the whole batch, per person, or per field.

### Coach Training Rating
FM derives coaching stars from specialist attributes combined with **Determination**, **Authority** and **Motivating** (weighted twice). The editor mirrors FM's formula so star counts match the game:

| Area | What counts most |
|---|---|
| Goalkeeping: Shot Stopping | Goalkeeping (×9) |
| Goalkeeping: Handling and Distribution | Goalkeeping (×6), so it tops out at 4 stars |
| Defending, Attacking (Technical / Tactical) | Defending or Attacking (×6) plus Technical or Tactical (×3) |
| Possession (Technical / Tactical) | Possession (×3) plus Technical or Tactical (×6) |
| Strength, Quickness | Fitness (×9) |
| Set Pieces | Set Pieces (×3) plus Tactical Knowledge (×6) |

Dragging a star slider sets every underlying attribute to `stars × 4` on Save. In Mass Edit, "Change by" shifts each coach's stars clamped between 0 and 5.

### Runtime hooks (Windows x64 and macOS ARM64)
- **Wonderkid Academy (Experimental):** shapes newly generated youth intakes and regens inside your chosen PA range (`CA..200`) and optional Wonderkid % starting CA boost, scoped globally or by continents, nations or clubs. Existing players are never changed.
- **Development Speed:** speeds up training progression (`1` normal up to `20`) for selected players on their Tools card.

### Currency, unsaved changes and CA drift
- Money is shown in FM's current display currency (detected automatically) or any currency you choose in Settings, converted with the game's fixed rates while keeping internal pounds intact.
- Every player or staff sub-tab shows `*` while it holds unsaved edits.
- **How FM ability works:** FM treats CA as the source of truth; over a few in-game days, attributes drift toward the player's CA, so adjust CA alongside attributes when making lasting changes.

## Build from source and command-line tools

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download) (plus Xcode Command Line Tools on macOS to package the helper and bundle).

```bash
dotnet build Fm27Editor.slnx -c Release
dotnet test Fm27Editor.slnx -c Release
dotnet run --project src/Fm27Editor.App -c Release
```

To build the self-contained macOS NativeAOT app bundle and disk image (`dist/Fm27Editor-mac-arm64.dmg`):

```bash
VERSION=1.5.3 scripts/package-mac.sh
```

Run `Fm27Editor.Tools` while the game is open and a save is loaded (write commands require `--write`; otherwise they print their plan):

```bash
dotnet run --project src/Fm27Editor.Tools -c Release -- players "Kobbie Mainoo"
dotnet run --project src/Fm27Editor.Tools -c Release -- set "Kobbie Mainoo" Crossing 12 --write
dotnet run --project src/Fm27Editor.Tools -c Release -- gamecheck
```

| Command | What it does |
|---|---|
| `players [name]` | Lists matching players: in-game date, age, club, wage, contract expiry, CA/PA |
| `set <name> <field> <value> [--write]` | Writes an attribute, position, personality stat, `CA` or `PA` |
| `verify <golden.json>` | Compares live memory with values copied from the game UI |
| `find-offsets <golden.json> <out.json> [--date yyyy-MM-dd]` | Rebuilds the player layout from UI-captured values |
| `newbuild [--out <path>] [--layout macos\|touch-macos\|windows]` | Discovers RTTI vtables, roots and drifted offsets for a new patch (~30s) |
| `verify-build --profile <path> [--expect-players N ...] [--writes] [--negative-test] [--crash-gate ...]` | Release gate checks: one PASS/FAIL verdict |

More commands (staff, clubs, nations, presets, promises, transfers, future transfers) are listed at the top of `src/Fm27Editor.Tools/Program.cs`.

## Layout

| Path | Contents |
|---|---|
| `src/Fm27Editor.Core` | Process memory, build checks, offset profiles, readers/writers, Undo journal |
| `src/Fm27Editor.App` | Desktop app (Avalonia 11, code-only UI) |
| `src/Fm27Editor.Tools` | CLI diagnostics, `newbuild`, and `verify-build` release gate |
| `src/Fm27Editor.Helper` | macOS privileged memory helper (`fm27_helper.c`) |
| `offsets/<build>.json` | Embedded per-build offset profiles (`offsets/` next to the binary overrides built-ins) |
| `offsets/layouts/*.json` | Shared base layouts (`windows.json`, `macos.json`, `touch-macos.json`) |
| `tests/` | Core and App unit tests (`Fm27Editor.Core.Tests`, `Fm27Editor.App.Tests`) |
| `scripts/package-mac.sh` | macOS NativeAOT `.app` and `.dmg` packager |
| `re/tools/memscan` | Read-only memory scanning helper |

## After a game patch

The editor refuses to attach to an unprofiled build. To support a new patch, load a throwaway calibration save and run (prefix with `dotnet run --project src/Fm27Editor.Tools -c Release --`, or `Fm27Editor.Tools.exe` on Windows):

1. `newbuild --out offsets/<build>.json` discovers every RTTI vtable, root and drifted offset in ~30 seconds (`"readOnly": true`). Fix any `MANUAL` item using its printed hint.
2. `verify-build --profile offsets/<build>.json --expect-players N --expect-staff N --expect-humans N --negative-test --writes` checks vtables, game-UI counts, semantic invariants, database fixtures, byte-exact round-trip writes and game-heap allocations.
3. `verify-build ... --crash-gate state.json`, press **Continue** and **Load Game** in FM, then run `verify-build ... --crash-gate-verify state.json --release-gate --receipt receipt.json`. A `VERDICT: PASS` receipt unlocks `"readOnly"` and can go into a pull request.

## Not supported: women's football

The editor is built and checked for men's football only. Women's teams, players and competitions can show missing or wrong data and are untested for writes, so avoid editing them.

## Roadmap

Next:
- Player stats radar tab and visualisations.
- Shortlist support.
- Training calendar and coach allowances.
- Women's football support later on.

## Disclaimer and bug reports

Not affiliated with or endorsed by Sports Interactive or SEGA. Football Manager is their trademark. Editing memory can corrupt a save, so keep backups and use a throwaway save for anything unsure. **No warranty, no liability:** provided "as is".

If a save breaks or crashes, click the **bug icon** at the bottom right of the editor to create a diagnostic zip on your Desktop (your note, editor/game state, and write logs for the last 10 runs from `%LOCALAPPDATA%\Fm27Editor\logs`). No save file is included, nothing is uploaded automatically, and local folder paths are trimmed to the last folder name. Review the zip and attach it with your game build via a [GitHub Issue](https://github.com/Jackhamerp24/Football-Manager-27-Editor/issues) or email to **vuquyduong2004@gmail.com**.

## AI Acknowledgments

AI assistance was used in developing this repository, including:

- **Claude:** Haiku / Sonnet / Opus 5.5
- **Codex / ChatGPT:** Terra, Sol 6.1, 6 Astra, 6.2 Sol/Astra
- **Gemini / Antigravity:** Gemini 3.8 Flash, 4 Argon, 4 Hydrogen, 4 Neon
- **OpenCode Go:** DeepSeek v4.1 Flash 1017, MiMo v2.6 Flash, Muse Spark 1.3 Contributor, LongCat 2.5, GLM 5.3
- **Muse Code:** Muse Wonder 1.0, Muse Spark 1.3 Contributor
- **App logo:** Generated with ChatGPT Image

## Support

If the editor helps your saves, you can support maintenance and updates:

- [Ko-fi](https://ko-fi.com/quyduongvu) (card or PayPal)
- Vietnamese bank transfer (VietQR, Vietcombank): scan with any Vietnamese banking or e-wallet app

<img src="docs/images/donate-vietqr.png" alt="VietQR, Vietcombank" width="220">

## License

GPL-3.0-or-later. See [LICENSE](LICENSE).
