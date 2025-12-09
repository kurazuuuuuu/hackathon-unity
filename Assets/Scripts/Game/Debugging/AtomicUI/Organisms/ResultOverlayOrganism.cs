using UnityEngine;
using UnityEngine.UI;
using Game.Debugging.AtomicUI.Atoms;

namespace Game.Debugging.AtomicUI.Organisms
{
    public class ResultOverlayOrganism : AtomBase
    {
        public override void Build(Transform parent)
        {
            var ui = EnsureObject(parent, "ResultOverlay");
            if (!IsAlive(ui)) return;

            // Full screen
            var rect = EnsureComponent<RectTransform>(ui.gameObject);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            new AtomImage("BG", new Color(0, 0, 0, 0.9f)).Build(ui);
             var bgT = GetChild(ui, "BG");
            if(IsAlive(bgT))
            {
                var r = EnsureComponent<RectTransform>(bgT.gameObject);
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
                r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            }

            ui.gameObject.SetActive(false);

            // Result Text Molecule (AtomText)
            new AtomText("ResultText", "WIN", 200, TMPro.TextAlignmentOptions.Center, Color.white).Build(ui);
            var txt = GetChild(ui, "ResultText");
            if(IsAlive(txt))
            {
                EnsureComponent<RectTransform>(txt.gameObject).anchoredPosition = new Vector2(0, 100);
            }

            // Return Button Molecule (AtomButton)
            new AtomButton("ReturnHomeButton", "Return to Home", 48).Build(ui);
            var btn = GetChild(ui, "ReturnHomeButton");
            if(IsAlive(btn))
            {
                var r = EnsureComponent<RectTransform>(btn.gameObject);
                r.anchoredPosition = new Vector2(0, -200);
                r.sizeDelta = new Vector2(500, 120);
            }
        }
    }
}
