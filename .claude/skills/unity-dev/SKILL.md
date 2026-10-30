---
name: unity-dev
description: How to work on this Unity 6 project safely - project layout, code conventions, editing scenes/prefabs/assets through batchmode editor scripts instead of hand-editing YAML, verifying changes headlessly, and the Unity gotchas this project has already hit. Use this whenever a task touches anything under Assets/, ProjectSettings/ or Packages/ - adding or changing scripts, scenes, prefabs, ScriptableObjects, packages, build settings, input, UI or TextMesh Pro - and whenever you need to check that a change actually works in Unity, even if the user just says "add X to the game" or "fix this Unity error".
---

# Unity development in this project

The project is Unity **6000.5.6f1**, 2D, built-in render pipeline, uGUI + TextMesh Pro, Input System only. There is no CLI build; the only ways to know a change works are compiling and running Unity (batchmode or the user pressing Play).

## Project map

| Path | What lives there |
|---|---|
| `Assets/Scripts/Common/` | Reusable framework, own `Common.asmdef`: `Core/` (Context, ContextController), `Core/Pooling/` (PoolService<TKey>), `Core/Saving/` (SaveService, ISaveParticipant), `Popups/` (uGUI PopupService, Darkinator), `Configs/` |
| `Assets/Scripts/App/` | `ApplicationContext` (persistent services) + its controller in the Starter scene |
| `Assets/Scripts/Scenes/` | `SceneId` enum (must match build index), `SceneManager`, `SceneTransition` fade |
| `Assets/Scripts/Game/` | Game scene: `GameSceneContext(Controller)`, `GameFlow` state machine, `Bird/`, `Pipes/`, `UI/` (HUD + popups) |
| `Assets/Scripts/Audio/`, `Progress/`, `Title/` | AudioPlayer + SoundId, BestScoreRecord (save participant), title screen |
| `Assets/Scripts/Editor/PlayModeBootstrap.cs` | Play always starts from `Starter.unity` so the app context exists |
| `Assets/Settings/` | ScriptableObject configs: PopupCatalog, PopupServiceConfig, DarkinatorConfig, MedalTable |
| `Assets/Prefabs/Popups/` | Popup prefabs (root = overlay Canvas + `TypedPopup<TData>`), Darkinator |

Boot flow: `Starter` (App object, DontDestroyOnLoad: context controller, AudioPlayer, SceneTransition, the only EventSystem) → `Title` → `Game`. Scene controllers reach app services through `ApplicationContextController.Current`.

## Code conventions

Match the existing code; it follows these consistently:
- `sealed` on every concrete class not designed for inheritance; one class per file; file name = class name; **namespace = folder path** under `Assets/Scripts` (`Game/Bird/` → `namespace Game.Bird`).
- Braces on every `if`/`else`; trailing commas in multi-line initializers.
- `[SerializeField] private` fields wired in the scene/prefab, not `Find*`, tags or `GetComponent` in `Update`. Auto-properties with `private set`.
- Comments are rare and explain *why* (a constraint, a non-obvious consequence), never restate the code.
- Async uses UniTask (`.Forget()` for fire-and-forget), tweens use LitMotion. UI animations that may run while paused must use `MotionScheduler.UpdateIgnoreTimeScale`.
- Input: poll `Pointer.current` / `Keyboard.current` (see `GameInput`). The legacy `Input` class throws at runtime - active input handling is "Input System only".
- No obsolete APIs: `FindAnyObjectByType`/`FindObjectsByType`, `Rigidbody2D.bodyType`/`linearVelocity`.

For design-level guidance (state machines, events, pooling, when to add an interface) see the `game-code-design` skill.

## Changing scenes, prefabs and assets

Scene and prefab YAML is fragile: fileIDs (including **negative** ones), stripped prefab-instance objects, `m_AddedGameObjects`, and GUID references all have to stay consistent. So:

1. **Prefer a temporary editor script run in batchmode** over editing YAML. Put it in `Assets/Editor/<TaskName>/`, expose a `public static void Run()`, and delete the script when it has done its job. Use `scripts/unity_batch.sh` to run it (it checks the editor is closed, runs the method and prints errors). The editor must be **closed** - batchmode refuses a project that's open; if it's open, ask the user to close it rather than waiting or killing it.
2. Patterns that proved necessary (see `references/editor-scripting.md` for code):
   - `PrefabUtility.LoadPrefabContents` → modify → `SaveAsPrefabAsset` → `UnloadPrefabContents`.
   - **Opening or creating a scene unloads assets that nothing references yet**, so a reference you loaded earlier becomes "fake null". Load assets by path *after* `OpenScene`/`NewScene`, at the point of use.
   - Set serialized fields through `SerializedObject` and fail loudly (throw) if a property or value is missing; end with `EditorApplication.Exit(0/1)`.
   - Find scene objects by full path including inactive ones (walk `scene.GetRootGameObjects()` + `Transform.Find`); HUD canvases sit under `UI/` and some are inactive.
   - Move/rename assets with `AssetDatabase.MoveAsset` (or `git mv` file **and** `.meta` together) so GUIDs survive.
3. If a batch run fails partway, revert only what it touched before retrying: `git checkout -- <paths>` then `git clean -fd -- <paths>` for new files. In zsh an unmatched glob aborts the *whole* command, so don't rely on `rm foo*` in cleanup.
4. Small, surgical YAML edits are acceptable when the editor is open and the scene isn't loaded in it - but parse the file properly (documents split on `--- !u!<class> &<fileID>`, fileIDs can be negative) and re-check every reference afterwards.

Before deleting any asset as "unused", grep its GUID across `Assets/` and `ProjectSettings/` - and remember an object can be referenced from inside a prefab instance's added objects, not only by obvious owners.

## Verifying a change

Use the strongest check that applies and say plainly which ones ran:

1. **Compile**: `scripts/unity_batch.sh` with no method; it reports `error CS` / `warning CS`. Aim for zero warnings.
2. **Static scan**: missing scripts (`GameObjectUtility.GetMonoBehavioursWithMissingScriptCount`) and unassigned object references on our own components across all prefabs and build scenes; unresolved GUIDs (package GUIDs resolve under `Library/PackageCache`).
3. **Scripted play-through**: an editor script that enters Play mode (`EnterPlayModeOptions.DisableDomainReload`, reset afterwards), drives the game through `EditorApplication.update` steps with timeouts, and fails on any logged error/exception. `references/editor-scripting.md` has the skeleton. Reflection into private methods (e.g. `GameFlow.StartRound`) is fine for a throwaway test.
4. **Visuals need a human.** Batchmode runs with `-nographics`: nothing is rendered, so an opaque overlay, a broken shader or a mis-anchored UI element passes every automated check. (This happened: a stale full-screen black image passed the play-through.) After any change that can affect what's drawn, tell the user exactly what to look at when they press Play, and ask for a screenshot if something seems off.

Test runs write a real save file to `~/Library/Application Support/OnurSinanTogrul/Flappy Bird Clone/Saves`; delete it afterwards so test scores don't become the player's best.

## Known gotchas

- **TextMesh Pro resources** live in the current layout: all shaders and `.cginc` files together in `Assets/TextMesh Pro/Shaders/`. A shader complaining it can't open `TMPro_Properties.cginc` means the folders got split again.
- `SceneId` values must equal build indices; update both together.
- HUD canvases have no `GraphicRaycaster` on purpose - `GameInput` ignores taps over raycastable UI, so a full-screen raycast target would swallow every flap.
- Popups are destroyed on scene unload and fire `OnClosed` then; anything in `OnClosed` must be safe during teardown.
- The original 2020 project is kept on branch `deprecated/og`; a branch switch to it makes Unity reimport everything. Close the editor before switching.
