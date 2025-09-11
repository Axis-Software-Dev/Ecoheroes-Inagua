using UnityEngine;
using UnityEditor;

public class ShaderFixer
{
    [MenuItem("Tools/Fix Material Shaders")]
    public static void FixMaterials()
    {

        string[] allMaterialGUIDs = AssetDatabase.FindAssets("t:Material");

        foreach (string guid in allMaterialGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);


            if (material != null && material.shader == null)
            {
                Debug.Log($"Fixing material: {material.name} at {assetPath}");

                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader != null)
                {
                    material.shader = urpShader;
                }
                else
                {
                    Debug.LogError("Universal Render Pipeline/Lit shader not found. Make sure URP is installed.");
                }
            }
        }
        Debug.Log("Material shader check complete.");
    }
}