using UnityEngine;

namespace Game.Battle.StatusEffects
{
    public abstract class StatusEffect : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        public Sprite Icon;
        public int Duration; // Turns remaining
        public bool IsStackable;
        
        // References (set at runtime)
        protected Card ownerCard;
        protected Player ownerPlayer; // For player-wide status
        
        public virtual void Initialize(Card card)
        {
            ownerCard = card;
        }

        public virtual void OnTurnStart() { }
        public virtual void OnTurnEnd() 
        {
            Duration--;
            if (Duration <= 0)
            {
                Remove();
            }
        }
        
        public virtual void OnTakeDamage(ref int damage) { }
        public virtual void OnDealDamage(ref int damage) { }
        
        protected void Remove()
        {
            if (ownerCard != null)
            {
                ownerCard.RemoveStatus(this);
            }
        }
        
        public virtual StatusEffect Clone()
        {
            var clone = Instantiate(this);
            clone.Duration = this.Duration;
            return clone;
        }
    }
}
