# Editor-script building blocks

Copy what you need into a temporary script under `Assets/Editor/<TaskName>/`, run it with `scripts/unity_batch.sh <Class>.<Method>`, and delete it afterwards.

## Contents
1. Skeleton with fail-loud helpers
2. Prefab edits
3. Scene edits
4. Static scan
5. Scripted play-through

## 1. Skeleton

```csharp
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class MyTask
{
    public static void Run()
    {
        try
        {
            // ... steps ...
            AssetDatabase.SaveAssets();
            Debug.Log("[MyTask] Done.");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogError("[MyTask] Failed: " + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void Require(bool condition, string what)
    {
        if (!condition) throw new InvalidOperationException("Step failed: " + what);
    }

    // Load at the point of use: OpenScene/NewScene unload assets nothing references yet.
    private static T Load<T>(string path) where T : Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        Require(asset != null, "load " + path);
        return asset;
    }

    private static void SetRefs(Object target, params (string name, Object value)[] refs)
    {
        var so = new SerializedObject(target);
        foreach (var (name, value) in refs)
        {
            var property = so.FindProperty(name);
            Require(property != null, $"{target.GetType().Name}.{name}");
            Require(value != null, $"value for {target.GetType().Name}.{name}");
            property.objectReferenceValue = value;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
```

Lists: `property.arraySize = n;` then `GetArrayElementAtIndex(i)` and `FindPropertyRelative("field")` for nested serializable classes.

## 2. Prefab edits

```csharp
var root = PrefabUtility.LoadPrefabContents(path);
// modify root / children; DestroyImmediate(component) is fine here
PrefabUtility.SaveAsPrefabAsset(root, path);
PrefabUtility.UnloadPrefabContents(root);
```

New prefab from scratch: build a temporary GameObject, `PrefabUtility.SaveAsPrefabAsset(go, path)`, then `Object.DestroyImmediate(go)`.

Removing a persistent UnityEvent listener: `UnityEditor.Events.UnityEventTools.RemovePersistentListener(evt, i)` from the last index down.

Removing components whose script was deleted: `GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go)`.

## 3. Scene edits

```csharp
var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
var prefabRef = Load<GameObject>("Assets/Prefabs/Pipe.prefab"); // after OpenScene, not before

static GameObject FindInScene(Scene scene, string path)   // "UI/ScoreCountCanvas/ScoreCount", finds inactive too
{
    var parts = path.Split('/');
    var current = scene.GetRootGameObjects().First(r => r.name == parts[0]).transform;
    for (var i = 1; i < parts.Length; i++) current = current.Find(parts[i]);
    return current.gameObject;
}

var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef, scene); // keeps the prefab link
Require(EditorSceneManager.SaveScene(scene), "save scene");
```

Build scenes: assign `EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true), ... }` in `SceneId` order.

## 4. Static scan

```csharp
foreach (var path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }).Select(AssetDatabase.GUIDToAssetPath))
{
    var root = PrefabUtility.LoadPrefabContents(path);
    foreach (var t in root.GetComponentsInChildren<Transform>(true))
    {
        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0) Fail($"{path}: missing script on {t.name}");
        foreach (var mb in t.GetComponents<MonoBehaviour>())
            if (mb != null && mb.GetType().Assembly.GetName().Name is "Assembly-CSharp" or "Common") CheckRefs(path, mb);
    }
    PrefabUtility.UnloadPrefabContents(root);
}
// CheckRefs: iterate new SerializedObject(mb).GetIterator() with NextVisible(true); flag ObjectReference
// properties that are null (skip m_Script and fields that are optional by design, e.g. Popup.backButton).
```

Do the same for each scene in `EditorBuildSettings.scenes` and for ScriptableObjects in `Assets/Settings`.

## 5. Scripted play-through

Drive Play mode from the editor with a list of steps; each step is a condition polled every editor tick with a timeout.

```csharp
[InitializeOnLoad]
public static class PlayCheck
{
    static IEnumerator<Func<bool>> _steps; static double _deadline; static string _step;
    static readonly List<string> Failures = new();

    static PlayCheck()
    {
        Application.logMessageReceived += (msg, _, type) =>
        {
            if (type is (LogType.Error or LogType.Exception or LogType.Assert) && EditorApplication.isPlaying) Failures.Add(msg);
        };
    }

    public static void Run()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;          // no domain reload: statics survive into Play
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        _steps = Steps().GetEnumerator();
        EditorApplication.update += Tick;
        EditorApplication.isPlaying = true;                         // PlayModeBootstrap starts it in Starter
    }

    static IEnumerable<Func<bool>> Steps()
    {
        _step = "reach Title";
        yield return () => SceneManager.GetActiveScene().name == "Title" && !ApplicationContextController.Current.SceneManager.IsSwitching;
        // ... invoke buttons via button.onClick.Invoke(), private methods via reflection, assert with Expect(...)
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        if (_deadline == 0) { if (!_steps.MoveNext()) { Finish(); return; } _deadline = EditorApplication.timeSinceStartup + 15; }
        if (_steps.Current()) _deadline = 0;
        else if (EditorApplication.timeSinceStartup > _deadline) { Failures.Add("timeout: " + _step); Finish(); }
    }

    static void Finish()
    {
        EditorApplication.update -= Tick;
        EditorSettings.enterPlayModeOptionsEnabled = false;          // restore the project setting
        Debug.Log(Failures.Count == 0 ? "[PlayCheck] PASSED" : "[PlayCheck] FAILED: " + string.Join(" | ", Failures));
        EditorApplication.Exit(Failures.Count == 0 ? 0 : 1);
    }
}
```

Tips:
- Capture any "before" value (e.g. the previous best score) before the step that changes it - a bird can die mid-step.
- A dumb autopilot crashing into a pipe is not a game bug; log the death cause and positions before concluding anything.
- This proves logic only. It renders nothing; list the visual checks for the user.
