using System;
using System.Collections.Generic;
using Arena;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// A barrel or crate of the original layout. The hero cuts passages through them: when one breaks,
    /// its footprint leaves the pathing grid and every route is rebuilt around the gap.
    /// </summary>
    public sealed class Destructible : Targetable
    {
        private static readonly List<Destructible> all = new List<Destructible>();

        public static IReadOnlyList<Destructible> All => all;
        public static event Action<Destructible> Broken;

        public ArenaMap map;
        public int editorId;
        public float maxHealth = 90f;
        /// <summary>Explosive barrels hurt everything around them when they break.</summary>
        public bool explosive;
        public float blastRadius = 3.2f;
        public float blastDamage = 220f;
        public GameObject breakEffect;

        private float health;
        private float shake;
        private Vector3 restPosition;

        public override bool IsAlive => health > 0f;
        public float Health => health;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            all.Clear();
            Broken = null;
        }

        private void Awake()
        {
            health = maxHealth;
            restPosition = transform.position;
        }

        private void OnEnable()
        {
            if (!all.Contains(this)) all.Add(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        public override bool CanBeAttackedBy(Unit attacker) => IsAlive && attacker != null && attacker.faction == Faction.Hero;

        public override void ApplyDamage(float amount, Unit source)
        {
            if (!IsAlive) return;
            health -= amount;
            shake = 0.14f;
            if (health <= 0f) Break(source);
        }

        private void Update()
        {
            if (shake <= 0f) return;
            shake -= Time.deltaTime;
            var strength = Mathf.Max(0f, shake) * 0.6f;
            transform.position = restPosition + new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)) * strength;
            if (shake <= 0f) transform.position = restPosition;
        }

        private void Break(Unit source)
        {
            health = 0f;
            transform.position = restPosition;
            all.Remove(this);
            if (map != null) map.SetDoodadAlive(editorId, false);
            if (breakEffect != null)
                Destroy(Instantiate(breakEffect, restPosition + Vector3.up * 0.4f, Quaternion.identity), 3f);
            if (explosive) Explode(source);
            Broken?.Invoke(this);
            Destroy(gameObject);
        }

        private void Explode(Unit source)
        {
            // Copy first: damage can kill a unit, which edits the registry.
            var victims = new List<Unit>(Unit.All);
            foreach (var unit in victims)
            {
                var offset = unit.transform.position - restPosition;
                offset.y = 0f;
                if (offset.magnitude > blastRadius + unit.radius) continue;
                unit.ApplyDamage(blastDamage, source);
            }
            // Neighbouring barrels go with it.
            var neighbours = new List<Destructible>(all);
            foreach (var other in neighbours)
            {
                var offset = other.transform.position - restPosition;
                offset.y = 0f;
                if (offset.magnitude <= blastRadius * 0.6f) other.ApplyDamage(blastDamage, source);
            }
        }
    }
}
