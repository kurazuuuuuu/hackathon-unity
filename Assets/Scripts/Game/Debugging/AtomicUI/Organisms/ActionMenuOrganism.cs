using UnityEngine;
using UnityEngine.UI;
using Game.Debugging.AtomicUI.Atoms;

namespace Game.Debugging.AtomicUI.Organisms
{
    public class ActionMenuOrganism : AtomBase
    {
        public override void Build(Transform parent)
        {
            var ui = EnsureObject(parent, "ActionSelectionUI");
            if (!IsAlive(ui)) return;

            // Full screen overlay props
            var rect = EnsureComponent<RectTransform>(ui.gameObject);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            EnsureComponent<CanvasGroup>(ui.gameObject);
            if (ui.GetComponent<Game.UI.BattleActionSelectionUI>() == null)
                ui.gameObject.AddComponent<Game.UI.BattleActionSelectionUI>();

            new AtomImage("BG", new Color(0, 0, 0, 0.6f)).Build(ui);
            var bgT = GetChild(ui, "BG");
            if(IsAlive(bgT)) // Expand BG
            {
                var r = bgT.GetComponent<RectTransform>();
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
                r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            }
            
            // Click to close (Invisible button covering all?)
            EnsureComponent<Button>(ui.gameObject);

            // Container for buttons
            var buttons = EnsureObject(ui, "Buttons");
            if (IsAlive(buttons))
            {
                var lg = EnsureComponent<VerticalLayoutGroup>(buttons.gameObject);
                lg.spacing = 30;
                lg.childAlignment = TextAnchor.MiddleCenter;
                lg.childControlWidth = false; lg.childControlHeight = false;
                
                var r = EnsureComponent<RectTransform>(buttons.gameObject);
                r.sizeDelta = new Vector2(500, 300);

                // Attack / Skill Buttons (Atoms)
                new AtomButton("AttackButton", "通常攻撃", 40).Build(buttons);
                
                var atkB = GetChild(buttons, "AttackButton");
                if(IsAlive(atkB)) EnsureComponent<RectTransform>(atkB.gameObject).sizeDelta = new Vector2(400, 100);

                new AtomButton("SkillButton", "特殊効果", 40).Build(buttons);
                var skB = GetChild(buttons, "SkillButton");
                if(IsAlive(skB)) EnsureComponent<RectTransform>(skB.gameObject).sizeDelta = new Vector2(400, 100);
            }

            ui.gameObject.SetActive(false);
        }
    }
}
