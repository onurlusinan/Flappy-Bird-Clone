# Netcode fundamentals

Enough depth to implement and debug real-time play. Skim the section you need.

## Contents
1. Topologies and authority
2. Ticks and clocks
3. What to send: inputs vs state
4. Remote entities: interpolation
5. Local entity: prediction and reconciliation
6. Determinism
7. Bandwidth and reliability
8. Security
9. Mobile realities

## 1. Topologies and authority

- **Dedicated server**: server runs the authoritative sim; clients send inputs, receive state. Best fairness and cheat resistance; costs hosting.
- **Client-hosted (listen server)**: one player's device is the server. Free to run, but the host has zero latency and can cheat, and the match dies if the host leaves (unless host migration).
- **Relay**: not a topology but a transport path - traffic goes via a relay server so devices behind NAT/mobile carriers can connect. Pairs with client-hosted.
- **Distributed authority** (Netcode for GameObjects 2.x option): each client owns some objects and a service arbitrates. Useful when each player owning their own avatar is natural - which matches "each bird simulated by its owner" - but ownership means trusting that client.
- **Peer-to-peer lockstep/rollback**: everyone runs the same deterministic sim from shared inputs. Minimal bandwidth, needs strict determinism.

Authority is per piece of state: decide for each of {bird position, death, score, pipes, match result} who is the source of truth.

## 2. Ticks and clocks

- Run the sim on a fixed tick (e.g. 30-60 Hz) independent of frame rate; render interpolates between the last two ticks.
- Every message carries a tick number. Clients estimate server time with round-trip-time sampling (send local time, server echoes with its tick; offset ≈ serverTick + RTT/2). Smooth the estimate; never jump time backwards.
- A synchronized start ("go at server tick T") is how all players begin the same course together.

## 3. What to send: inputs vs state

- **Inputs** (e.g. "flap at tick 1234"): tiny, and with a deterministic sim fully define the outcome. Lost/late inputs must be handled (send recent inputs redundantly in each packet).
- **State snapshots** (position, velocity, alive, score): robust to non-determinism, larger. Delta-compress against the last acknowledged snapshot.
- Common hybrid: inputs as the primary stream, periodic state snapshots as a correction/checksum.

## 4. Remote entities: interpolation

Render remote players slightly in the past: keep a buffer of received states keyed by tick, render at `now - interpolationDelay` (typically 2-3 send intervals, ~100 ms), and lerp between the two surrounding states. If the buffer runs dry, extrapolate briefly then hold. This hides jitter at the cost of showing others a little late - fine when players don't interact.

## 5. Local entity: prediction and reconciliation

When the server is authoritative over your own avatar: apply your input immediately (prediction), keep a history of (tick, input, predicted state); when an authoritative state for tick N arrives, compare with your history, and if they differ, reset to the server state and re-apply inputs N+1..now. Smooth visible corrections over a few frames. Only needed when you don't simulate your own bird authoritatively.

## 6. Determinism

- Same inputs + same seed + same code → same result, on every device. Requirements: fixed timestep, no frame-time-dependent logic, seeded RNG owned by the sim, no iteration over unordered collections affecting results, no Unity physics in the authoritative path.
- Floating point: identical code on the same architecture is usually consistent; across CPUs/compilers (ARM phones vs x64 server, Mono vs IL2CPP) small differences can appear. For a simple flappy sim, either use fixed-point math or validate with a small tolerance (positions within ε, same pipe passed / same death tick).
- Test it: replay the same log twice, and on two devices, compare final state.

## 7. Bandwidth and reliability

- Unreliable-sequenced channels for frequent state; reliable for events (match start, death, results, chat).
- Budget: a flappy race needs very little - a flap input is a few bytes; 8 players × 20 updates/s × ~16 bytes is a few KB/s.
- Send rate can be lower than tick rate (e.g. tick 60, send 20) when interpolating.

## 8. Security

- Never trust client-reported outcomes for anything ranked. Validate by re-simulating inputs on the server/host.
- Validate every message: size, rate, tick within an acceptable window, values in range. Disconnect on garbage.
- Keep secrets (service keys, admin endpoints) off the client; use the backend's player auth tokens.
- Rate-limit score submissions and session creation.

## 9. Mobile realities

- Apps get backgrounded: in a live match treat it as a disconnect after a short grace period.
- Networks switch (Wi-Fi ↔ cellular): support reconnecting to the same session.
- Cellular latency and jitter are worse than desktop; test with 150-250 ms and loss.
- Battery and data: keep send rates modest.
