using UnityEngine;

namespace Game.Debugging.AtomicUI.Atoms
{
    /// <summary>
    /// Atomic Designの全構成要素の基底クラス
    /// 堅牢な生存確認（Zombie Check）とコンポーネント取得機能を提供する
    /// </summary>
    public abstract class AtomBase
    {
        protected bool CreatedNew = false;

        public abstract void Build(Transform parent);

        /// <summary>
        /// 厳密な生存確認
        /// </summary>
        protected bool IsAlive(Object obj)
        {
            if (obj == null) return false;
            // Zombie check
            if (obj is Transform t)
            {
                try { var g = t.gameObject; if (g == null) return false; } 
                catch { return false; }
            }
            else if (obj is GameObject go)
            {
                try { var trans = go.transform; if (trans == null) return false; } 
                catch { return false; }
            }
            return true;
        }

        protected Transform GetChild(Transform parent, string name)
        {
            if (!IsAlive(parent)) return null;
            try
            {
                var t = parent.Find(name);
                return IsAlive(t) ? t : null;
            }
            catch { return null; }
        }

        protected Transform EnsureObject(Transform parent, string name)
        {
            Transform tf = GetChild(parent, name);
            CreatedNew = (tf == null);
            
            if (tf == null && IsAlive(parent))
            {
                var go = new GameObject(name);
                tf = go.transform;
                tf.SetParent(parent, false);
            }
            return tf;
        }

        protected T EnsureComponent<T>(GameObject go) where T : Component
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
    }
}
