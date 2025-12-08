using UnityEngine;
using UnityEngine.Playables;
using System.Threading.Tasks;

namespace Game.Gacha
{
    public class GachaAnimationController : MonoBehaviour
    {
        [Header("Timeline References")]
        [SerializeField] private PlayableDirector director; // The component that plays Timelines

        [Header("Assets (Timelines)")]
        [SerializeField] private PlayableAsset timelineNormal; // Blue
        [SerializeField] private PlayableAsset timelineGold;   // Gold/Rainbow
        
        // You can expand this for 3-star, 4-star, 5-star specific timelines

        public bool HasAnimation(int maxRarity)
        {
            if (maxRarity >= 5 && timelineGold != null) return true;
            if (timelineNormal != null) return true;
            return false;
        }

        private void Awake()
        {
            if (director == null) director = GetComponent<PlayableDirector>();
            if (director == null) director = gameObject.AddComponent<PlayableDirector>();
        }

        public async Task PlayAnimation(int maxRarity)
        {
            if (director == null)
            {
                Debug.LogWarning("GachaAnimationController: No PlayableDirector found. Skipping animation.");
                await Task.Delay(100);
                return;
            }

            // Select timeline based on rarity
            if (maxRarity >= 5 && timelineGold != null)
            {
                director.playableAsset = timelineGold;
            }
            else if (timelineNormal != null)
            {
                director.playableAsset = timelineNormal;
            }

            if (director.playableAsset != null)
            {
                director.Play();
                
                // Wait for completion
                // Simple wait: director.duration
                // Better wait: check state each frame
                
                float duration = (float)director.duration;
                // Clamp duration to avoid infinite wait if 0
                if (duration <= 0) duration = 2.0f; // Default dummy wait

                Debug.Log($"Playing Gacha Animation for {duration} seconds...");
                await Task.Delay((int)(duration * 1000));
            }
            else
            {
                Debug.Log("GachaAnimationController: No timeline assigned. Simulating wait.");
                await Task.Delay(2000); // 2 sec dummy
            }
        }

        public void Stop()
        {
            if (director != null) director.Stop();
        }
    }
}
