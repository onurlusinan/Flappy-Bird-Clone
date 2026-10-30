using UnityEditor;
using UnityEditor.SceneManagement;

namespace Editor
{
    // Always boot through Starter.unity on Play so ApplicationContextController exists, even when editing another scene.
    [InitializeOnLoad]
    public static class PlayModeBootstrap
    {
        private const string StarterScenePath = "Assets/Scenes/Starter.unity";

        static PlayModeBootstrap()
        {
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StarterScenePath);
        }
    }
}