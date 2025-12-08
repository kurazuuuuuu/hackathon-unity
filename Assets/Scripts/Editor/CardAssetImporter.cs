using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using Game.Abilities;
using Game.Abilities.Actions;

namespace Game.Editor
{
    [InitializeOnLoad]
    public class CardAssetImporter : EditorWindow
    {
        static CardAssetImporter()
        {
            // Delay call to ensure assets are ready
            EditorApplication.delayCall += () => 
            {
                if (!SessionState.GetBool("ImportedCSV", false))
                {
                    Import();
                    SessionState.SetBool("ImportedCSV", true);
                }
            };
        }

        private const string CSV_PATH = "Docs/学内ハッカソン __ てつやチーム - カード一覧.csv";
        private const string CARDS_ROOT = "Assets/Resources/Cards";
        private const string ABILITIES_ROOT = "Assets/Resources/Abilities/Generated";

        [MenuItem("Tools/Import Card Data from CSV")]
        public static void Import()
        {
            EnsureDirectory(ABILITIES_ROOT);

            string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, CSV_PATH);
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"CSV not found at {fullPath}");
                return;
            }

            string[] lines = File.ReadAllLines(fullPath);
            int importedCount = 0;

            foreach (var line in lines)
            {
                var cols = ParseCsvLine(line);
                if (cols.Count < 3) continue;

                // Skip headers
                if (cols[1] == "ID" || string.IsNullOrEmpty(cols[1])) continue;

                string id = cols[1].Trim();
                if (string.IsNullOrEmpty(id)) continue;

                // Find Card Asset
                CardDataBase cardData = FindCardData(id);
                if (cardData == null)
                {
                    Debug.LogWarning($"CardData not found for ID: {id}");
                    continue;
                }

                // Import Health for PrimaryCardData
                var primaryData = cardData as PrimaryCardData;
                if (primaryData != null && cols.Count > 3)
                {
                    string healthStr = cols[3].Trim();
                    if (int.TryParse(healthStr, out int health))
                    {
                        // Use reflection to set private field
                        var healthField = typeof(PrimaryCardData).GetField("health", 
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        if (healthField != null)
                        {
                            healthField.SetValue(primaryData, health);
                            Debug.Log($"Set health for {id}: {health}");
                        }
                    }
                }

                // Extract Description
                // CSV index: Primary(6=Special), Support(6=Special), Special(6=Special)
                // Note: CSV columns might vary slightly, but generally 6th index (0-based) seems to be "特殊効果" based on file view
                // Let's verify based on "ID" column index being 1.
                // 0: , 1: ID, 2: Name/Type, 3: HP/-, 4: Power, 5: Cost, 6: Special Effect
                
                string effectDesc = (cols.Count > 6) ? cols[6] : "";
                
                // Unescape CSV quotes if needed
                effectDesc = effectDesc.Replace("\"\"", "\""); 

                if (!string.IsNullOrEmpty(effectDesc) && effectDesc != "-")
                {
                    // Generate or Update Ability
                    UpdateCardAbility(cardData, id, effectDesc);
                }
                
                EditorUtility.SetDirty(cardData);
                importedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Import Complete. Updated {importedCount} cards.");
        }

        private static void UpdateCardAbility(CardDataBase card, string id, string desc)
        {
            string abilityName = $"Ability_{id}";
            string path = $"{ABILITIES_ROOT}/{abilityName}.asset";
            
            // Try to find existing
            CardAbility ability = AssetDatabase.LoadAssetAtPath<CardAbility>(path);
            
            // Determine Type based on text
            bool isNew = (ability == null);
            
            if (isNew)
            {
                ability = CreateAbilityInstance(desc);
                AssetDatabase.CreateAsset(ability, path);
            }
            else
            {
                // If exists, checks if type matches heuristics? 
                // For simplicity, we only create new if missing, or update description.
                // Re-creating might break references if we change type.
                // Let's just update description field via SerializedObject or reflection if possible
            }
            
            // Update Description field using SerializedObject to access private field
            SerializedObject so = new SerializedObject(ability);
            so.Update();
            SerializedProperty descProp = so.FindProperty("description");
            if (descProp != null)
            {
                descProp.stringValue = desc;
            }
            
            // Try to configure generic params if simple
            ConfigureAbilityParams(ability, desc);
            
            so.ApplyModifiedProperties();

            // Assign to card
            SerializedObject cardSo = new SerializedObject(card);
            cardSo.Update();
            SerializedProperty abProp = cardSo.FindProperty("ability");
            if (abProp != null)
            {
                abProp.objectReferenceValue = ability;
            }
            cardSo.ApplyModifiedProperties();
        }

        private static CardAbility CreateAbilityInstance(string desc)
        {
            // Heuristics
            if (desc.Contains("ダメージ") && !desc.Contains("回復")) return ScriptableObject.CreateInstance<DamageAbility>();
            if (desc.Contains("回復")) return ScriptableObject.CreateInstance<HealAbility>();
            if (desc.Contains("ドロー")) return ScriptableObject.CreateInstance<DrawCardAbility>();
            if (desc.Contains("バフ") || desc.Contains("強化")) return ScriptableObject.CreateInstance<BuffDebuffAbility>();
            
            return ScriptableObject.CreateInstance<GenericAbility>();
        }
        
        private static void ConfigureAbilityParams(CardAbility ability, string desc)
        {
            // Basic parameter extraction
            if (ability is DamageAbility dmg)
            {
                int val = ExtractNumber(desc);
                var dmgSo = new SerializedObject(dmg);
                dmgSo.Update();
                dmgSo.FindProperty("fixedDamage").intValue = val > 0 ? val : 0;
                if (desc.Contains("全体")) dmgSo.FindProperty("targetType").enumValueIndex = (int)DamageAbility.DamageTargetType.AllEnemies;
                else dmgSo.FindProperty("targetType").enumValueIndex = (int)DamageAbility.DamageTargetType.SingleEnemy;
                dmgSo.ApplyModifiedProperties();
            }
            else if (ability is HealAbility heal)
            {
                int val = ExtractNumber(desc);
                var healSo = new SerializedObject(heal);
                healSo.Update();
                healSo.FindProperty("fixedHeal").intValue = val > 0 ? val : 0;
                if (desc.Contains("全体") || desc.Contains("全員")) healSo.FindProperty("targetType").enumValueIndex = (int)HealAbility.HealTargetType.AllAllies;
                else if (desc.Contains("指定") || desc.Contains("味方")) healSo.FindProperty("targetType").enumValueIndex = (int)HealAbility.HealTargetType.TargetAlly;
                else healSo.FindProperty("targetType").enumValueIndex = (int)HealAbility.HealTargetType.Self;
                healSo.ApplyModifiedProperties();
            }
            else if (ability is DrawCardAbility draw)
            {
                int val = ExtractNumber(desc);
                var drawSo = new SerializedObject(draw);
                drawSo.Update();
                drawSo.FindProperty("drawCount").intValue = val > 0 ? val : 1;
                drawSo.ApplyModifiedProperties();
            }
        }

        private static int ExtractNumber(string text)
        {
             if (string.IsNullOrEmpty(text)) return 0;

             // Normalize full-width numbers to half-width
             text = text.Replace("１", "1").Replace("２", "2").Replace("３", "3")
                        .Replace("４", "4").Replace("５", "5").Replace("６", "6")
                        .Replace("７", "7").Replace("８", "8").Replace("９", "9").Replace("０", "0");

             // Extract first number found
             var match = Regex.Match(text, @"\d+");
             if (match.Success)
             {
                 if (int.TryParse(match.Value, out int result))
                 {
                     return result;
                 }
                 // Handle overflow or other parse errors gracefully
                 Debug.LogWarning($"Failed to parse number from: {match.Value}");
             }
             
             return 0;
        }

        private static CardDataBase FindCardData(string id)
        {
            // Search for CardDataBase (includes both CardData and PrimaryCardData)
            string[] guids = AssetDatabase.FindAssets($"t:CardDataBase {id}");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CardDataBase data = AssetDatabase.LoadAssetAtPath<CardDataBase>(path);
                // We rely on filename matching ID mostly
                if (Path.GetFileNameWithoutExtension(path) == id) return data;
            }
            return null;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string current = "";
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            result.Add(current);
            return result;
        }
    }
}
