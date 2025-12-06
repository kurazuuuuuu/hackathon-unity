using UnityEngine;
using UnityEditor;
using TMPro;

public class FontDebugger
{
    [MenuItem("Tools/Debug Font Settings")]
    public static void DebugFont()
    {
        string path = "Assets/TextMesh Pro/Fonts/DotGothic16-Regular SDF.asset";
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);

        if (fontAsset == null)
        {
            Debug.LogError("Font Asset not found at " + path);
            return;
        }

        Debug.Log($"--- Debugging Font: {fontAsset.name} ---");
        
        // Check Atlas Texture
        if (fontAsset.atlasTexture != null)
        {
            Debug.Log($"Atlas Texture: {fontAsset.atlasTexture.name}");
            Debug.Log($"Filter Mode: {fontAsset.atlasTexture.filterMode}");
            Debug.Log($"Aniso Level: {fontAsset.atlasTexture.anisoLevel}");
            Debug.Log($"Mip Map Bias: {fontAsset.atlasTexture.mipMapBias}");
            Debug.Log($"Width: {fontAsset.atlasTexture.width}, Height: {fontAsset.atlasTexture.height}");
            Debug.Log($"Texel Size: {fontAsset.atlasTexture.texelSize}");
        }
        else
        {
            Debug.LogError("Atlas Texture is NULL");
        }

        // Check Material
        if (fontAsset.material != null)
        {
            Debug.Log($"Material: {fontAsset.material.name}");
            Debug.Log($"Shader: {fontAsset.material.shader.name}");
            
            // Check specific keywords
            string[] keywords = fontAsset.material.shaderKeywords;
            Debug.Log($"Shader Keywords: {string.Join(", ", keywords)}");
            
            if (fontAsset.material.HasProperty("_TextureWidth"))
                Debug.Log($"_TextureWidth: {fontAsset.material.GetFloat("_TextureWidth")}");
        }
        else
        {
            Debug.LogError("Material is NULL");
        }
    }
}