#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Game.Data;
using System.IO;

namespace Game.Debugging
{
    public class UserDataDebugTool
    {
        [MenuItem("Debug/Generate Test User Data")]
        public static void GenerateTestData()
        {
            UserData data = new UserData("DebugUser");
            data.GachaTickets = 100;
            data.IsFirstGacha = true;
            
            // Add some dummy cards if needed
            // data.AddCard("3A_Sword", 1);

            // Convert to API Schema (DTO)
            UserGameProfileDto profile = data.ToApiProfile();

            string json = JsonUtility.ToJson(profile, true);
            string path = Path.Combine(Application.persistentDataPath, "test_user_profile.json");
            
            File.WriteAllText(path, json);
            
            Debug.Log($"Test User Profile (DTO) generated at: {path}\n{json}");
            EditorUtility.RevealInFinder(path);
        }
    }
}
#endif
