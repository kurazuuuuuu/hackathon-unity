using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BattleSceneSetup
{
    [MenuItem("Tools/Setup Battle Scene Visuals")]
    public static void Setup()
    {
        // 1. Setup Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 12, -9); // Slightly higher
            mainCam.transform.rotation = Quaternion.Euler(55, 0, 0); // Steeper angle
            mainCam.backgroundColor = Color.black; // Black background to highlight table
            mainCam.clearFlags = CameraClearFlags.SolidColor; 
        }

        // 2. Create/Update Table
        GameObject table = GameObject.Find("BattleTable");
        if (table == null)
        {
            table = GameObject.CreatePrimitive(PrimitiveType.Plane);
            table.name = "BattleTable";
        }
        
        table.transform.position = Vector3.zero;
        table.transform.localScale = new Vector3(2, 1, 1.5f); // 20x15 units
        
        // Apply Texture
        string texturePath = "Assets/Textures/TableTexture.png";
        Texture2D tableTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        
        Material tableMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (tableMat.shader == null) tableMat = new Material(Shader.Find("Standard"));
        
        if (tableTex != null)
        {
            tableMat.mainTexture = tableTex;
            tableMat.color = Color.white;
            Debug.Log("Applied Table Texture.");
        }
        else
        {
            tableMat.color = new Color(0.3f, 0.2f, 0.1f); // Fallback brown
            Debug.LogWarning("Table Texture not found at " + texturePath);
        }
        
        tableMat.SetFloat("_Glossiness", 0.1f); // Less shiny
        
        Renderer rend = table.GetComponent<Renderer>();
        if (rend != null) rend.material = tableMat;

        // 3. Create Zones (Visual Markers)
        CreateZone("PlayerHandZone", new Vector3(0, 0.1f, -5), Color.green);
        CreateZone("PlayerActiveZone", new Vector3(0, 0.1f, -2), Color.cyan);
        CreateZone("EnemyActiveZone", new Vector3(0, 0.1f, 2), Color.red);
        CreateZone("EnemyHandZone", new Vector3(0, 0.1f, 5), Color.magenta);

        // 4. Lighting
        GameObject lightObj = GameObject.Find("Main Light");
        if (lightObj == null)
        {
            lightObj = new GameObject("Main Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
        }
        
        Light l = lightObj.GetComponent<Light>();
        l.intensity = 1.5f;
        l.color = new Color(1f, 0.95f, 0.9f);
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.Refresh();
        Debug.Log("Battle Scene Visuals Setup Complete. PLEASE CHECK GAME VIEW.");
    }

    private static void CreateZone(string name, Vector3 position, Color color)
    {
        GameObject zone = GameObject.Find(name);
        if (zone == null)
        {
            zone = new GameObject(name);
        }
        zone.transform.position = position;
        
        // Add a debug marker to make it visible in Scene view
        // Use OnDrawGizmos in a script usually, but here we can add a small sphere
        // Or just leave it empty. Let's add a small sphere for visibility if it doesn't exist
        if (zone.transform.childCount == 0)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Marker";
            marker.transform.parent = zone.transform;
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one * 0.5f;
            marker.GetComponent<Renderer>().material.color = color;
            // Disable collider so it doesn't interfere
            Object.DestroyImmediate(marker.GetComponent<Collider>());
        }
    }
}