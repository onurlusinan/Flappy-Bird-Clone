# Online options for this game

Concrete designs for taking this flap-through-pipes game online, cheapest first. Each builds on the one before, so starting small doesn't waste work.

## Contents
1. Shared foundation (needed by every option)
2. Option A - Seeded runs + global leaderboard
3. Option B - Async ghost races
4. Option C - Live race (2-8 players, no interaction)
5. Option D - Live with interaction / battle royale
6. Questions to settle with the user

## 1. Shared foundation

Do this first regardless of option; it's also good single-player engineering.

- **Course from a seed.** `PipeSpawner` takes heights from a `System.Random(seed)` owned by the match instead of `UnityEngine.Random`. Same seed → same pipes on every device.
- **Own the bird simulation at a fixed tick** (e.g. 60 Hz): `velocity.y -= gravity * dt; if (flapThisTick) velocity.y = flapVelocity; position += velocity * dt`, and pipes advance `speed * dt` per tick. Collision = overlap test of the bird's circle/capsule against pipe and gap rectangles in sim space. The current values to preserve the feel: gravity scale 2.1 × 9.81, flap velocity 7, pipe speed 2.5, spawn interval 1.3 s, height range ±2.5. Rendering reads sim state (optionally interpolated between ticks).
- **Input log.** A run = `{ version, seed, tickRate, flaps: [tick, tick, ...] }`. Tiny (a few hundred bytes), and enough to replay the whole run.
- **Version the rules.** Any change to sim constants changes outcomes; include a rules version in every run/match and reject mismatches.

## 2. Option A - Seeded runs + global leaderboard

- Daily (or weekly) seed from the backend; everyone plays the same course. Optional "free play" with random seeds stays local.
- On death, submit `{ seed, rulesVersion, inputLog, claimedScore }`.
- Server-side validation (e.g. Cloud Code / a small backend function) replays the input log with the same sim code and accepts the score only if it matches. Sharing the sim code between client and server is easiest if the sim is a plain C# class with no UnityEngine dependency (Cloud Code supports C# modules).
- Stack: Authentication (anonymous + optional platform/social sign-in for names), Leaderboards, Cloud Code. No real-time netcode, no server running between matches.
- Effort: small. Running cost: low.

## 3. Option B - Async ghost races

- Everything from A, plus store the input logs of top/friends' runs.
- When starting a seeded run, download N ghost logs and replay them locally as translucent birds alongside the player. No network during play.
- Gives most of the "racing other people" feeling with zero latency issues.
- Effort: small-medium on top of A.

## 4. Option C - Live race (2-8 players, no interaction)

- Players join a session (invite code or quick match), the host/server sends the seed and a synchronized start time, everyone counts down and plays the same course simultaneously; last bird alive / furthest distance wins.
- **Local bird: fully client-simulated** for instant response. Each client sends its flap ticks (inputs) and occasional state snapshots.
- **Remote birds: replayed from their inputs** on the shared course, delayed by an interpolation buffer (~100-150 ms) so they move smoothly despite jitter. Because birds don't interact, a remote bird being slightly behind in time is invisible to the player.
- Authority: host or dedicated server re-simulates each player's inputs to decide deaths and the winner (prevents "I never died" cheats). With a client host, the host can cheat - acceptable for friends-only, not for ranked.
- Stack: Netcode for GameObjects (or lighter custom messages over Unity Transport) + Multiplayer Services sessions (lobby/relay or matchmaker). Relay avoids NAT problems for phone-to-phone.
- Effort: medium-large. Running cost: relay/matchmaking usage; dedicated servers only if ranked.

## 5. Option D - Live with interaction / battle royale

- Birds can bump each other, use items, or a large lobby is eliminated over time.
- Needs server authority over shared state, client-side prediction of the local bird with reconciliation when the server disagrees, and lag compensation for interactions. This is where flappy's "instant flap" feel is hardest to keep.
- Consider whether deterministic lockstep/rollback (e.g. Photon Quantum-style) fits better than state sync.
- Effort: large. Do C first and confirm people want more.

## 6. Questions to settle with the user

- Which option (or which first)? A recommendation: A → B → C, because each step reuses the previous one's sim, seed and input log.
- Account model: anonymous only, or display names / platform sign-in?
- Matchmaking for C: invite codes only, or public quick match?
- Win condition for C: last alive, highest score after N pipes, or first to X?
- Platforms and whether cross-play matters.
- Budget ceiling and whether free tiers are enough to start.
