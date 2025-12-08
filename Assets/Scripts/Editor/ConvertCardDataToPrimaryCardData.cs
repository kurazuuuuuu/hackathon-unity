using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

namespace Game.Editor
{
    public class ConvertCardDataToPrimaryCardData : EditorWindow
    {
        [MenuItem("Tools/Convert CardData to PrimaryCardData")]
        public static void Convert()
        {
            string cardsPath = "Assets/Resources/Cards";
            string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { cardsPath });
            
            int convertedCount = 0;
            
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CardData oldCard = AssetDatabase.LoadAssetAtPath<CardData>(path);
                
                if (oldCard == null) continue;
                
                // Check if this should be a PrimaryCardData (has health > 0 or CardType is Primary)
                if (oldCard.CardType == CardType.Primary || oldCard.Health > 0)
                {
                    // Create new PrimaryCardData
                    PrimaryCardData newCard = ScriptableObject.CreateInstance<PrimaryCardData>();
                    
                    // Copy all fields using reflection
                    var oldFields = typeof(CardData).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    var newFields = typeof(PrimaryCardData).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    foreach (var oldField in oldFields)
                    {
                        var newField = global::System.Array.Find(newFields, f => f.Name == oldField.Name);
                        if (newField != null && newField.FieldType == oldField.FieldType)
                        {
                            newField.SetValue(newCard, oldField.GetValue(oldCard));
                        }
                    }
                    
                    // Delete old asset
                    AssetDatabase.DeleteAsset(path);
                    
                    // Create new asset at same path
                    AssetDatabase.CreateAsset(newCard, path);
                    
                    Debug.Log($"Converted {path} to PrimaryCardData (Health: {newCard.Health})");
                    convertedCount++;
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Conversion complete. Converted {convertedCount} cards to PrimaryCardData.");
        }
    }
}
