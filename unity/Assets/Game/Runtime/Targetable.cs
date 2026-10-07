using UnityEngine;

namespace Game
{
    /// <summary>Anything an attack order can point at: units and breakable props.</summary>
    public abstract class Targetable : MonoBehaviour
    {
        public float radius = 0.4f;
        public float height = 1.8f;

        public abstract bool IsAlive { get; }
        public abstract bool CanBeAttackedBy(Unit attacker);
        public abstract void ApplyDamage(float amount, Unit source);

        public Vector3 Position => transform.position;

        /// <summary>Ground-plane distance from a point to the edge of this target.</summary>
        public float EdgeDistance(Vector3 from)
        {
            var delta = transform.position - from;
            delta.y = 0f;
            return Mathf.Max(0f, delta.magnitude - radius);
        }
    }
}
