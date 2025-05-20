using UnityEditor;
using UnityEngine;

public class ConvertHDRPToURP
{
    [MenuItem("Tools/Convert HDRP Materials to URP")]
    static void ConvertMaterials()
    {
        var materialGUIDs = AssetDatabase.FindAssets("t:Material");
        foreach (var guid in materialGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat.shader.name.Contains("HDRP"))
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                Debug.Log($"Converted: {mat.name}");
            }
        }
    }
}
