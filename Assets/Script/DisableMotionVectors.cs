using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class DisableMotionVectors : EditorWindow
{
    [MenuItem("Tools/Quest Optimization/Disable All Motion Vectors")]
    static void DisableAll()
    {
        SkinnedMeshRenderer[] allSkinned = FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);
        int count = 0;

        foreach (var smr in allSkinned)
        {
            smr.skinnedMotionVectors = false;
            smr.updateWhenOffscreen = false;
            smr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            smr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            EditorUtility.SetDirty(smr.gameObject);
            count++;
        }

        Debug.Log($"✅ Disabled motion vectors on {count} SkinnedMeshRenderers");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
