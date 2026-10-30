# Unity networking stack options

What exists and when it fits. **Verify current package names, versions, features and pricing in the Package Manager / official docs before recommending or installing** - this area changes frequently and these notes may be out of date.

## Contents
1. Unity first-party
2. Third-party netcode
3. Third-party backends
4. Choosing for this game
5. Integration notes for this codebase

## 1. Unity first-party

- **Netcode for GameObjects (NGO)** - `com.unity.netcode.gameobjects`. GameObject-based high-level netcode: `NetworkObject`, `NetworkBehaviour`, `NetworkVariable`, RPCs, scene management. Supports client-server/host and (2.x) distributed authority. Good default for small casual matches.
- **Netcode for Entities** - DOTS/ECS-based, server-authoritative with built-in prediction. Powerful, but implies an ECS rewrite; overkill here.
- **Unity Transport (UTP)** - `com.unity.transport`. Low-level UDP/WebSocket transport under NGO; usable directly for custom lightweight messaging. Has a network simulator pipeline for latency/loss testing.
- **Multiplayer Services** - `com.unity.services.multiplayer`. Unified "sessions" API over Lobby, Relay, Matchmaker (and dedicated hosting options). Create/join by code, quick join, relay connection for NAT traversal.
- **Authentication** - anonymous sign-in plus platform/social providers; required by the other services.
- **Leaderboards**, **Cloud Save**, **Cloud Code** (server-side C# or JS functions, e.g. to validate replays before posting a score).
- **Multiplayer Play Mode** - `com.unity.multiplayer.playmode`. Run additional virtual players from the editor to test without builds.
- **Multiplayer Tools** - network profiler, runtime stats, network simulator presets.

All UGS services need the project linked to a Unity Cloud project (Project Settings → Services) - a step the user has to do in the editor/dashboard.

## 2. Third-party netcode

- **Mirror** - mature, open-source, high-level, large community; server-authoritative with host mode.
- **FishNet** - open-source, feature-rich (prediction, lag compensation, observers), good performance.
- **Photon Fusion** - commercial; host/server or shared modes, built-in prediction/rollback and interest management; hosted cloud (CCU-based pricing).
- **Photon Quantum** - commercial deterministic rollback engine; strong for competitive interactive games, requires writing the sim in its framework.
- **Photon PUN 2** - older and in maintenance; avoid for new work.

## 3. Third-party backends

- **Nakama** (Heroic Labs) - open-source game server: auth, leaderboards, matchmaking, realtime/authoritative matches, storage; self-host or managed.
- **PlayFab** (Microsoft) - accounts, leaderboards, cloud scripts, matchmaking, server hosting.
- **Firebase** - auth + database/functions; fine for async leaderboards/ghosts, not real-time netcode.

## 4. Choosing for this game

| Goal | Reasonable default | Why |
|---|---|---|
| Leaderboard + daily seed (Option A) | Authentication + Leaderboards + Cloud Code | No servers to run; Cloud Code can replay-validate submissions |
| Ghost races (Option B) | A + Cloud Save (or a small blob store) for input logs | Still no real-time networking |
| Live race 2-8 (Option C) | Multiplayer Services sessions (relay) + NGO or custom UTP messages | Phone-to-phone via relay, invite codes/quick join built in |
| Interactive / ranked live (Option D) | Re-evaluate: NGO with server authority + dedicated hosting, or Fusion/Quantum | Prediction/rollback quality matters most here |

Staying first-party keeps one dashboard, one auth system and one SDK style; third-party wins when a specific feature (rollback, open-source self-hosting, existing team experience) matters more.

## 5. Integration notes for this codebase

- Services are async; wrap them in plain C# service classes owned by `ApplicationContext` and expose UniTask-returning methods. Initialize UGS (`UnityServices.InitializeAsync`) and sign in from the app context's `Initialize`, not from scene scripts.
- Keep NGO's `NetworkManager` on the persistent App object (it's DontDestroyOnLoad already); let `SceneManager` remain the single place that switches scenes - with NGO's scene management enabled, the server drives scene loads, so decide which one owns switching before mixing them.
- Network-facing UI (connecting, lobby, errors) fits the popup system: one `PopupData` + `TypedPopup` per screen, `CloseOnDarkinatorClick = false` for blocking states.
- Add new states to `GameFlow` for online flows rather than branching on "isOnline" flags throughout.
