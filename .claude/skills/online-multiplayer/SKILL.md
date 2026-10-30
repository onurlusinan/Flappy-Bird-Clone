---
name: online-multiplayer
description: Designing and building online play for this Unity game - choosing a multiplayer model (async ghosts/leaderboards vs real-time races vs shared sessions), authority and anti-cheat, netcode concepts (ticks, latency, prediction, interpolation, determinism), the Unity 6 networking stack (Netcode for GameObjects, Unity Transport, Multiplayer Services sessions/relay/lobby, Authentication, Leaderboards, Cloud Code) and alternatives (Mirror, FishNet, Photon, Nakama), plus how to test multiplayer locally. Use this for anything involving online, multiplayer, networking, netcode, servers, lobbies, matchmaking, rooms, leaderboards, accounts, sync, lag, cheating or "playing with friends", even if the user only says "make it online" or "add versus mode".
---

# Online multiplayer

Networking decisions are expensive to reverse: they shape the simulation code, the backend bill and what cheating is possible. So the order is **design → model → stack → build**, and the first two are conversations with the user, not choices to make silently.

## 1. Pin down the design first

Before proposing technology, get answers (offer sensible defaults, but let the user decide):

- **What does "online" mean here?** Global leaderboard? Racing friends' ghosts? Live head-to-head where you see other birds? Battle-royale with many players and eliminations?
- **Live or asynchronous?** Async (scores, ghost replays, daily seeded runs) needs no real-time netcode at all and is far cheaper to build and run.
- **Players per match**, matchmaking (random vs. invite code vs. friends), and whether players can **affect each other** (collisions, items) or just race in parallel.
- **Platforms** (mobile only? cross-play with desktop/WebGL?), and **budget** (free tiers, who pays for servers).
- **How competitive?** Casual friends-only tolerates client trust; ranked/global leaderboards need server validation.

Record decisions in the project (e.g. a short `Docs/Online.md`) so later sessions build on them instead of re-asking.

## 2. Pick the model

Read `references/flappy-online.md` for the concrete options for *this* game, with trade-offs. The short version:

| Model | What it is | Netcode needed | Cheat resistance |
|---|---|---|---|
| Async leaderboard + daily seed | Everyone plays the same seeded course; scores submitted | None (HTTP/backend) | Needs replay validation for anything ranked |
| Async ghosts | Record inputs + seed, replay others' runs as ghosts | None (store/download replays) | Same validation as above, ghosts come free from it |
| Live race, no interaction | 2-N players, same seed, see each other's birds | Light: each client simulates its own bird, sends state/inputs; others interpolated | Server or host validates inputs |
| Live with interaction | Players can collide/affect each other | Full: server authority, prediction + reconciliation | Strong, but hardest to make feel good |

Flappy-style games reward responsiveness above everything: a flap must react *instantly*. That's why the local player should always simulate their own bird locally, and why "players don't physically interact" models are so much cheaper here.

## 3. Pick the stack

Read `references/unity-networking-stack.md` before recommending anything. It covers Unity's first-party stack (Netcode for GameObjects, Unity Transport, Multiplayer Services sessions/relay/lobby/matchmaker, Authentication, Leaderboards, Cloud Code/Cloud Save) and the main alternatives, with when each fits.

Package versions, pricing and free-tier limits change often and may have moved since this skill was written. Before adding a package or quoting a cost, check the current docs or the Package Manager - don't rely on remembered version numbers.

## 4. Build it so the existing architecture still holds

- Networking/session is an **app-lifetime service** owned by `ApplicationContext` (connect, sign in, current session); the current match is owned by the game scene's context. Follow `game-code-design`.
- Extend `GameFlow` with explicit states (e.g. `Connecting`, `WaitingForPlayers`, `Countdown`, `Spectating`) instead of bools.
- **Make the simulation deterministic and separate from presentation** before networking it:
  - pipes from a seeded `System.Random` owned by the match (seed comes from the server/host);
  - bird motion from a fixed-timestep integration you own (velocity += gravity·dt; flap sets velocity) rather than relying on `Rigidbody2D`, because PhysX/Box2D results aren't guaranteed identical across devices, and validation/replays need bit-for-bit or tolerance-bounded agreement;
  - inputs recorded as (tick, flap) pairs - that one stream drives replays, ghosts, validation and remote players.
- Treat everything from the network as untrusted: validate sizes, rates and ranges on the receiving side; never accept a client-reported score for a ranked board without checking it against a replay.
- Handle the unhappy paths up front: disconnect mid-match, host leaving (host migration or end match), app backgrounded on mobile (pausing isn't possible in a live match), reconnect, version mismatch between clients.

`references/netcode-fundamentals.md` explains ticks, clocks, interpolation, prediction/reconciliation, bandwidth and determinism in enough depth to implement them.

## 5. Test it like a networked game

- **Multiple players on one machine**: Unity's Multiplayer Play Mode (virtual players in the editor) or builds + editor. Automate what you can, but also have the user try two real devices before calling a feature done.
- **Bad networks**: simulate latency (100-250 ms), jitter and 1-5 % packet loss (Unity Transport's network simulator / Multiplayer Tools). A feature that only works on localhost isn't working.
- **Determinism checks**: run the same seed + input stream twice (and on two devices if possible) and compare final state/score; a mismatch means validation will reject honest players.
- Batchmode tests (see `unity-dev`) still help for logic, but never prove "feels responsive" or "remote birds look smooth" - that needs a human watching.
