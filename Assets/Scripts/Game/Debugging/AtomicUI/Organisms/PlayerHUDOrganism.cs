using UnityEngine;
using UnityEngine.UI;
using Game.Debugging.AtomicUI.Atoms;
using Game.Debugging.AtomicUI.Molecules;

namespace Game.Debugging.AtomicUI.Organisms
{
    public class PlayerHUDOrganism : AtomBase
    {
        private bool _isPlayer1;

        public PlayerHUDOrganism(bool isPlayer1)
        {
            _isPlayer1 = isPlayer1;
        }

        public override void Build(Transform parent)
        {
            // Base Card Dimensions from Prefab (Assets/Resources/Prefabs/Card.prefab)
            // 280x390
            float BASE_WIDTH = 280f;
            float BASE_HEIGHT = 390f;
            float SCALE = 1.6f; // Scaling for 4K visibility

            float cardW = BASE_WIDTH * SCALE; // ~448
            float cardH = BASE_HEIGHT * SCALE; // ~624

            string prefix = _isPlayer1 ? "Player1" : "Player2";
            var areaName = $"{prefix}Area";
            // Layout (Template usually handles this, but Organism ensures its root exists)
            // Using Template values passed down or hardcoded for now (System consistency).
            
            var area = EnsureObject(parent, areaName);
            if (!IsAlive(area)) return;

            var r = EnsureComponent<RectTransform>(area.gameObject);
            if (CreatedNew)
            {
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.one;
                r.sizeDelta = Vector2.zero;
                r.anchoredPosition = Vector2.zero;
            }

            // --- 1. Unit Status (Stats) ---
            // P1: Left (-1500), P2: Right (1500)
            // --- 1. Unit Status (Stats) ---
            // P1: Left (-1500), P2: Right (1500)
            // --- 1. Unit Status (Stats) ---
            // P1 (Bottom): Left side.
            // P2 (Top): Opponent side. MD says "Back".
            // To fix cutoff, we reduce X offset magnitude (closer to center).
            // Canvas is 3840 wide. +/- 1920 is edge. -1500 is safe usually? 
            // 1500 + 900/2 (width/2) = 1950 > 1920. Cutoff confirmed!
            // Need to set pivot or adjust pos.
            // If Pivot is Center (0.5), Rect is at 1500. Edge is 1500 + 450 = 1950. Clip!
            // Let's use +/- 1200.
            float statsX = _isPlayer1 ? -1200 : 1200;
            
            // P1 (Front) -> +Y (User requested this inversion)
            // P2 (Back) -> -Y
            float statsY = _isPlayer1 ? 600 : -600; 
            
            string statusName = $"{prefix}_StatusPanel";
            // Check if UnitStatusMolecule supports creation properly
            // We can just use the Molecule logic
            // P1: Left Aligned. P2: Right Aligned (for symmetry).
            new UnitStatusMolecule(statusName, _isPlayer1 ? "Player 1" : "Player 2", !_isPlayer1).Build(area);
            bool statusNew = CreatedNew; // Track if Molecule panel was just ensured
            
            var statusT = GetChild(area, statusName);
            if(IsAlive(statusT) && statusNew)
            {
                var sr = EnsureComponent<RectTransform>(statusT.gameObject);
                sr.anchoredPosition = new Vector2(statsX, statsY);
            }

            // --- 2. Primary Card Zone ---
            // Center (0), but Y offset
            // P1: Lower Center (-500), P2: Upper Center (+500)
            var pT = GetChild(area, $"{prefix}_PrimaryZone");
            bool zoneIsNew = (pT == null);
            if (pT == null)
            {
                new CardZoneMolecule($"{prefix}_PrimaryZone", false).Build(area);
                pT = GetChild(area, $"{prefix}_PrimaryZone");
            }
            
            if(IsAlive(pT) && zoneIsNew)
            {
                var pr = EnsureComponent<RectTransform>(pT.gameObject);
                // Swapped
                float zoneY = _isPlayer1 ? 500 : -500; 
                pr.anchoredPosition = new Vector2(0, zoneY);
                // Size to fit 3 cards + spacing
                // 3 * 448 + 2 * 50 = 1444
                pr.sizeDelta = new Vector2(cardW * 3 + 150, cardH + 50);
            }

            // --- 3. Hand Zone ---
            // P1: Bottom Center, Face Up
            // P2: Top/Hidden (For now, minimal visual or ignored)
            // --- 3. Hand Zone (Dynamic Arc Layout) ---
            if (_isPlayer1)
            {
                // Container for Hand
                var handContainer = EnsureObject(area, "HandArea");
                if (IsAlive(handContainer) && CreatedNew)
                {
                    var hr = EnsureComponent<RectTransform>(handContainer.gameObject);
                    // Swapped 
                    hr.anchoredPosition = new Vector2(0, 900); // Top? No wait.
                    // If -900 was Back, +900 is Front.
                    hr.sizeDelta = new Vector2(2500, 800); 

                    // We need a script to handle the runtime arc layout because cards are added dynamically.
                    // For now, let's attach a layout helper or configured HorizontalLayoutGroup for fallback
                    // But the request is "Dynamic Card Layout: P1 Hand: Arc/Fan layout".
                    // Since cards are instantiated by Player.cs -> CardManager, we need a container that arranges them.
                    // Let's add a custom "ArcLayoutGroup" component if we can definition one, 
                    // OR we just set the container and assume valid layout script will be attached later?
                    // NO, we must implement the logic. 
                    // Let's add a simple spacing layout for now, and rely on `HandArea` script (if exists) 
                    // or just HorizontalLayoutGroup with negative spacing for overlap?
                    
                    // Arc Layout requires custom script. Let's try to add a HorizontalLayoutGroup first for stability,
                    // but with settings that look "premium" (centered, overlap).
                    var gl = EnsureComponent<HorizontalLayoutGroup>(handContainer.gameObject);
                    gl.childAlignment = TextAnchor.LowerCenter;
                    gl.spacing = -100; // Overlap
                    gl.childControlHeight = false; // Allow manual height tweaks? No, layout controls it.
                    gl.childControlWidth = false;
                    gl.childForceExpandHeight = false;
                    gl.childForceExpandWidth = false;

                    // 動的レイアウトコントローラーを追加
                    EnsureComponent<UI.HandLayoutController>(handContainer.gameObject);

                    // Note: True Arc Layout (Rotation) requires a custom script (e.g. `HandLayout.cs`).
                    // We can't write new MonoBehaviours easily here without a separate file.
                    // For this iteration, let's stick to "Premium Horizontal w/ Overlap" which looks decent,
                    // unless we can patch `Player.cs` or `BattleScene` to animate them.
                    // Let's stick to the Overlap Fan style for now which is "Genshin-ish".
                }
            }
            else
            {
                // P2 Hand: Face-down Fan
                var hContainer = EnsureObject(area, $"{prefix}_HandVisuals");
                if(IsAlive(hContainer) && CreatedNew)
                {
                    var hcr = EnsureComponent<RectTransform>(hContainer.gameObject);
                    // Swapped
                    hcr.anchoredPosition = new Vector2(0, -950); 
                    hcr.sizeDelta = new Vector2(1000, 400); 
                    
                    // Use Horizontal Layout for P2 as well, simplified
                    var gl = EnsureComponent<HorizontalLayoutGroup>(hContainer.gameObject);
                    gl.childAlignment = TextAnchor.UpperCenter;
                    gl.spacing = -40; // Tight overlap
                    gl.childControlHeight = false;
                    gl.childControlWidth = false;
                    
                    // Create visual cards
                    int cardCount = 5;
                    float visualScale = 0.6f;
                    float vW = BASE_WIDTH * visualScale; 
                    float vH = BASE_HEIGHT * visualScale;
                    
                    for(int i=0; i<cardCount; i++)
                    {
                        var cardName = $"OpponentCard_{i}";
                        // Face down card atom
                        new AtomImage(cardName, new Color(0.15f, 0.15f, 0.25f, 1f)).Build(hContainer);
                        var c = GetChild(hContainer, cardName);
                        if(IsAlive(c) && CreatedNew)
                        {
                            var cr = EnsureComponent<RectTransform>(c.gameObject);
                            cr.sizeDelta = new Vector2(vW, vH); 
                            // Rotation for chaos/fan feel?
                            // LayoutGroup overrides rotation usually. 
                            // If we want rotation, we need to disable LayoutGroup after frame? 
                            // Or use non-layout group positioning.
                        }
                    }
                }
            }

            // --- 4. Deck (3D) ---
            // P1: Right (1600), P2: Left (-1600)
            string deckName = $"{prefix}_Deck";
            // Pass scale to DeckMolecule? Or let it handle it. DeckMolecule constructor has scale.
            // DeckMolecule(string name, int count, float scale)
            // 280 * scale. 
            // We want match visual scale.
            new DeckMolecule(deckName, 5, SCALE).Build(area);
            
            var deckT = GetChild(area, deckName);
            if(IsAlive(deckT))
            {
                var dr = EnsureComponent<RectTransform>(deckT.gameObject);
                float deckX = _isPlayer1 ? 1600 : -1600;
                // Swapped
                float deckY = _isPlayer1 ? 500 : -500;
                dr.anchoredPosition = new Vector2(deckX, deckY);
                
                // P2 Deck Rotation: Face opponent
                if(!_isPlayer1)
                {
                    // The DeckMolecule's model is inside "3DModel" child usually, or we rotate the root?
                    // Rotating root UI element might mess up RectTransform if not careful, but for 3D visual it's fine.
                    // Actually, DeckMolecule creates "3DModel" child. We should rotate that if possible, 
                    // or just rotate the root dr.localRotation = Quaternion.Euler(0, 180, 0);
                    // But UI rotation affects children interaction? (Invisible button is on root).
                    // Rotating 180 Y is fine.
                    dr.localRotation = Quaternion.Euler(0, 0, 180); // Z rotation for 2D? No, we want 3D rotation.
                    // Wait, ScreenSpace-Camera. 
                    // If we want it to look "upside down" for P2? Or just on the left?
                    // "Opponent side" usually implies rotated 180 Z (upside down) if 2D card game.
                    // If 3D, maybe the "back" of the deck is visible? But both see backs.
                    // Let's just rotate Z 180 so the "DECK" text (if any) is upside down?
                    // Actually DeckMolecule uses Card Back texture.
                    // Let's try Z-rotation 180 to delineate "Opponent".
                    dr.localRotation = Quaternion.Euler(0, 0, 180);
                }
                
                if(_isPlayer1)
                {
                     if(deckT.gameObject.GetComponent<Game.Debugging.AtomicUI.Pages.DeckButton>() == null)
                        deckT.gameObject.AddComponent<Game.Debugging.AtomicUI.Pages.DeckButton>();
                }
            }
        }
    }
}
