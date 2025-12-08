using NUnit.Framework;
using UnityEngine;
using Game.UI;

namespace Tests
{
    public class GachaColorTest
    {
        [Test]
        public void VerifyRarityColors()
        {
            // Verify Rarity 3 is Cyan
            Assert.AreEqual(Color.cyan, GachaResultView.GetRarityColor(3), "Rarity 3 should be Cyan");
            
            // Verify others just in case
            Assert.AreEqual(Color.yellow, GachaResultView.GetRarityColor(5), "Rarity 5 should be Yellow");
            Assert.AreEqual(new Color(0.8f, 0, 0.8f), GachaResultView.GetRarityColor(4), "Rarity 4 should be Purple");
        }
    }
}
