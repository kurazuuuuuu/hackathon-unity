using UnityEngine;
using TMPro;
using Game.Debugging.AtomicUI.Atoms;

namespace Game.Debugging.AtomicUI.Molecules
{
    public class TurnInfoMolecule : AtomBase
    {
        private string _name;
        public TurnInfoMolecule(string name)
        {
            _name = name;
        }

        public override void Build(Transform parent)
        {
            // 1. Turn Count (Center)
            var centerArea = EnsureObject(parent, "CenterInfoArea");
            if (IsAlive(centerArea))
            {
                var rect = EnsureComponent<RectTransform>(centerArea.gameObject);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0, 50); // Slightly above center
                rect.sizeDelta = new Vector2(400, 200);

                new AtomText("TurnInfoText", "Turn 1", 80, TextAlignmentOptions.Center, new Color(1f, 0.9f, 0.7f), true).Build(centerArea);
            }

            // 2. Battle Log (Left)
            var logArea = EnsureObject(parent, "BattleLogArea");
            if (IsAlive(logArea))
            {
                var rect = EnsureComponent<RectTransform>(logArea.gameObject);
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.anchoredPosition = new Vector2(400, 0); // 400px from left edge
                rect.sizeDelta = new Vector2(600, 800);

                // Sample Logs
                string sampleLog = "Player 1 attacked!\nPlayer 2 took 5 dmg\nPlayer 1 used Skill";
                new AtomText("LogText", sampleLog, 48, TextAlignmentOptions.Left, Color.white, true).Build(logArea);
            }
        }
    }
}
