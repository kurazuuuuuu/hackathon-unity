using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Game.Debugging.AtomicUI.Atoms;
using Game.Debugging.AtomicUI.Molecules;
using Game.Debugging.AtomicUI.Organisms;
using Game.Debugging.AtomicUI.Templates;

namespace Game.Debugging.AtomicUI.Pages
{
    public class BattlePageBuilder : MonoBehaviour
    {
#if UNITY_EDITOR
        private static bool _isConstructing;
        private static int _retryCount;
        private const int MAX_RETRIES = 50;

        [MenuItem("Tools/Battle/Construct Atomic UI")]
        public static void Construct()
        {
            if (_isConstructing) return;
            
            _isConstructing = true;
            _retryCount = 0;
            EditorApplication.update += ConstructionLoop;
            Debug.Log("[BattlePageBuilder] Start Construction Loop...");
        }

        private static void ConstructionLoop()
        {
            if (!_isConstructing)
            {
                EditorApplication.update -= ConstructionLoop;
                return;
            }

            try
            {
                _retryCount++;
                bool success = AttemptBuild();

                if (success)
                {
                    Debug.Log($"[BattlePageBuilder] Construction Complete! (Retries: {_retryCount})");
                    Finish();
                }
                else
                {
                    if (_retryCount >= MAX_RETRIES)
                    {
                        Debug.LogError($"[BattlePageBuilder] Construction Failed after {MAX_RETRIES} retries.");
                        Finish();
                    }
                    else
                    {
                        // Continue next frame
                        // Debug.Log($"[BattlePageBuilder] Retrying... ({_retryCount})");
                    }
                }
            }
            catch (global::System.Exception e)
            {
                Debug.LogWarning($"[BattlePageBuilder] Unexpected error during loop: {e.Message}. Retrying...");
                if (_retryCount >= MAX_RETRIES) Finish();
            }
        }

        private static void Finish()
        {
            _isConstructing = false;
            EditorApplication.update -= ConstructionLoop;
            _retryCount = 0;
        }

        private static bool AttemptBuild()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var go = new GameObject("Canvas");
                canvas = go.AddComponent<Canvas>();
                go.AddComponent<CanvasScaler>();
                go.AddComponent<GraphicRaycaster>();
            }

            // Switch to Screen Space - Camera to support 3D objects
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var cam = Camera.main;
            if (cam == null) cam = Object.FindObjectOfType<Camera>();
            canvas.worldCamera = cam;
            canvas.planeDistance = 100; // Far enough to allow 3D objects in between
            
            // Adjust Scaler
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(3840, 2160);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Debug.Log("[BattlePageBuilder] Constructing Atomic UI (ScreenSpace-Camera)...");

            var root = canvas.transform;

            try
            {
                // 1. Template (Layout scaffolding)
                new BattleTemplate().Build(root);

                // 2. Systems
                EnsureSystems();

                // 3. Background (Atom / Molecule)
                new AtomImage("BackgroundLayer", Color.white, Resources.Load<Sprite>("UI/battle_bg_fantasy"), false).Build(root);
                // Ensure DropZone on BG
                var bg = GetChild(root, "BackgroundLayer");
                if (bg != null)
                {
                    var rect = EnsureComponent<RectTransform>(bg.gameObject);
                    if (rect != null)
                    {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.sizeDelta = Vector2.zero; // Stretch
                        rect.anchoredPosition = Vector2.zero;
                    }
                    var img = bg.GetComponent<Image>();
                    if (img != null)
                    {
                        img.preserveAspect = false; // Force stretch
                        img.raycastTarget = false; // クリックを通過させる
                    }
                    
                    var dz = bg.gameObject.GetComponent<Game.CardDropZone>();
                    if (dz == null) bg.gameObject.AddComponent<Game.CardDropZone>();
                }

                // 4. Player HUDs
                new PlayerHUDOrganism(true).Build(root);
                new PlayerHUDOrganism(false).Build(root);

                // 5. Center Info (Turn & Log)
                // TurnInfoMolecule internally generates CenterInfoArea (Turn) and BattleLogArea (Log)
                new TurnInfoMolecule("IgnoredName").Build(root);

                // 6. Skip Button
                // Layout: Center-Right
                string skipName = "SkipButton";
                new AtomButton(skipName, "SKIP TURN", 48).Build(root);
                var sT = GetChild(root, skipName);
                if(IsAlive(sT))
                {
                    var sr = EnsureComponent<RectTransform>(sT.gameObject);
                    sr.anchoredPosition = new Vector2(800, -200); // Right side, slightly lower
                    sr.sizeDelta = new Vector2(300, 100);
                }

                // 7. Overlays
                new ActionMenuOrganism().Build(root);
                new ResultOverlayOrganism().Build(root);

                return true; // If we reached here without exception, assume success for this frame
            }
            catch (global::System.Exception e)
            {
                Debug.LogError($"[BattlePageBuilder] AttemptBuild Failed: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private static void EnsureSystems()
        {
            if (Object.FindObjectOfType<Game.Battle.BattleManager>() == null)
                new GameObject("BattleManager").AddComponent<Game.Battle.BattleManager>();
            
            if (Object.FindObjectOfType<Game.CardManager>() == null)
                new GameObject("CardManager").AddComponent<Game.CardManager>();

            var sc = Object.FindObjectOfType<Game.Scenes.BattleScene>();
            if (sc == null)
            {
                sc = new GameObject("BattleSceneController").AddComponent<Game.Scenes.BattleScene>();
                sc.useDebugPlayers = true;
            }

            var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            else
            {
                // StandaloneInputModuleがなければ追加
                if (es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
                {
                    es.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
        }

        private static Transform GetChild(Transform parent, string name)
        {
            if (parent == null) return null;
            try
            {
                var t = parent.Find(name);
                return (t != null) ? t : null;
            }
            catch { return null; }
        }

        private static bool IsAlive(Object obj)
        {
            return obj != null;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            if (!IsAlive(go)) return null;
            try
            {
                var c = go.GetComponent<T>();
                if (c == null) c = go.AddComponent<T>();
                return c;
            }
            catch
            {
                return null;
            }
        }

        // Helper to attach Deck Click Logic
        public static void AttachDeckLogic(Transform deckTransform)
        {
             // This is Editor time execution, so we can't hook up runtime events easily via UnityEvent in code if target objects don't exist.
             // BUT, we can add a helper component to the Button that finds the Player and calls Draw.
             // Or better, simply ensure the Button's onClick has a persistent listener?
             // Since this is "Construct", we can't easily reference runtime instances.
             // Strategy: Add a small script "DeckButton" that handles the click at runtime.
             
             if(deckTransform == null) return;
             var db = deckTransform.gameObject.GetComponent<DeckButton>();
             if(db == null) db = deckTransform.gameObject.AddComponent<DeckButton>();
        }
#endif
    }
    
    // Runtime script for Deck Button
    public class DeckButton : MonoBehaviour
    {
        void Start()
        {
            var btn = GetComponent<UnityEngine.UI.Button>();
            if(btn != null) btn.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            Debug.Log("Deck Clicked! Attempting to Draw...");
            // Find Player (assume P1 for now since UI is usually local player)
            // Or determine by parent name?
            // "Player1Area" -> P1.
            bool isP1 = transform.parent != null && transform.parent.name.Contains("Player1");
            
            // Find BattleManager directly
            var bm = Object.FindObjectOfType<Game.Battle.BattleManager>();
            var cm = Object.FindObjectOfType<Game.CardManager>();
            
            if(bm != null && cm != null)
            {
                var p = isP1 ? bm.Player1 : bm.Player2; 
                if(p != null) p.DrawCard(cm);
            }
        }
    }
}
