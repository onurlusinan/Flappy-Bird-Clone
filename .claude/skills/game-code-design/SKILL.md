---
name: game-code-design
description: Object-oriented and gameplay architecture guidance for this Unity game - where new code should live, how systems talk to each other (contexts, services, events, state machines), composition vs inheritance, interfaces, ScriptableObjects, pooling, per-frame performance, and keeping simulation separate from presentation so features like online play stay possible. Use this when designing or reviewing any gameplay feature, refactoring, adding a new system/manager/service, deciding class structure, or when code "feels messy" - including small requests like "add a power-up", "add a second bird" or "clean this script up".
---

# Game code design

The goal is code where each piece has one job, dependencies point one way, and a feature can be added without touching five unrelated scripts. The old version of this game showed what happens otherwise: five bools copied between scripts every frame, tag lookups everywhere, and bugs that depended on `Update` order. Most rules below exist to prevent exactly that.

## Where code lives

Decide ownership first; the rest follows.

- **App-lifetime services** (survive scene loads: saving, audio, popups, scene switching, later: networking/session) → plain C# classes owned by `ApplicationContext`, constructed there, disposed there. Not singletons scattered across MonoBehaviours.
- **Scene-lifetime services** (pools, the current match) → owned by that scene's `Context` (`GameSceneContext`), created in `Initialize`, torn down in `Deactivate`/`Dispose`.
- **Scene objects** (bird, spawner, HUD) → MonoBehaviours with serialized references, initialized by the scene's `ContextController.OnContextReady` in a known order. Prefer an explicit `Initialize(...)` that receives dependencies over `Awake`/`Start` reaching out to find them.
- **Data and tuning** (medal thresholds, popup config, future: level/difficulty presets) → ScriptableObject assets in `Assets/Settings/`. They hold authored data, never runtime state - a ScriptableObject mutated in Play mode keeps the change in the editor.

A MonoBehaviour should be a thin adapter between Unity (transforms, physics callbacks, serialized wiring) and logic. If a class needs no Unity callbacks, make it a plain C# class - it's easier to test and to run on a server.

## How systems talk

- **Owner calls down, child reports up.** `GameFlow` calls `bird.Flap()`; the bird raises `Scored`/`Died`. A child never reaches up into its owner or sideways into siblings.
- **One owner of state.** Game state lives in `GameFlow` (`Ready → Playing ⇄ Paused → Dead`) as an enum + `switch`. Other code asks or is told; nobody keeps a copy that has to be synced each frame. When a new mode appears (countdown, spectating, waiting-for-players), add a state rather than a bool.
- **C# events / `Action` callbacks** for in-code notifications; `UnityEvent` only where a designer wires it in the inspector (buttons). Always unsubscribe in `OnDestroy` (or when the owner is disposed).
- **No `Find*`, tags or string lookups** to locate collaborators. Serialized references or constructor/`Initialize` parameters. Collision categories use marker components (`Hazard`, `Pipe`) via `TryGetComponent`.
- **Enums over strings** for keys (`SoundId`, `SceneId`, `GamePoolKey`): typos become compile errors.

## Inheritance, composition, interfaces

- Default to composition: small components and plain classes combined by an owner.
- Inherit when there's a real framework contract with shared behavior: `Popup` → `TypedPopup<TData>` → `GameOverPopup`; `PoolableObject<TKey>`; `Context`. Keep hierarchies shallow (≤ 2-3 levels) and base classes abstract.
- Add an interface when there are (or will imminently be) two implementations, or to put a seam where something must be swappable: `ISaveParticipant`; for online play, things like an input source (local tap vs. remote player) or a match/session transport. Don't add `IFoo` for a single class "just in case".
- `sealed` by default; mark the few classes meant for extension `abstract`.

## Simulation vs presentation

Keep "what happens" separate from "how it looks", even in single-player:

- Simulation: bird position/velocity, pipe positions, collisions, score, game state, the RNG seed for pipe heights.
- Presentation: sprites, animators, tweens, sounds, UI text, screen shake.

Presentation reacts to simulation events and may lag or be skipped; simulation never reads from presentation (e.g. never decide game logic from an animation's state). This split is what makes replays, ghosts, server validation and spectating possible later - see the `online-multiplayer` skill. Seeded, deterministic randomness (`System.Random` with a known seed owned by the match, not `UnityEngine.Random` called from anywhere) is part of this.

## Performance habits that matter for a mobile game

- Nothing in `Update` that searches (`Find*`, `GetComponent`, LINQ) or allocates (string concatenation, `new List`, closures, boxing). Cache in `Initialize`; update UI text only when the value changes (`TMP_Text.SetText("{0}", n)` is allocation-free).
- Pool anything spawned repeatedly (`PoolService<TKey>`, register in the scene context with a pre-warm count). Pooled objects reset their state in `OnGetFromPool`.
- One manager iterating a list beats N objects each with their own `Update` (see `PipeSpawner.ScrollPipes`).
- Physics changes in `FixedUpdate` or via velocity; input read in `Update`. Never change global physics settings per frame.

## Reviewing a change

Ask, in roughly this order:
1. Who owns this state? Is there exactly one copy?
2. Does any dependency point up or sideways? Any `Find*`/tag/string lookup?
3. Is the lifetime right (app vs scene vs object), and is everything subscribed/allocated also released?
4. Does logic depend on presentation, frame order, or `UnityEngine.Random` from multiple places?
5. Does anything allocate or search per frame?
6. Does it follow the conventions in the `unity-dev` skill (sealed, namespace = folder, serialized private fields, why-only comments)?
