using System;
using System.Collections.Generic;
using Arena;
using UnityEngine;

namespace Game
{
    public enum Faction { Hero, Creep }

    /// <summary>Flat bonuses on top of a unit's base stats, filled from equipped items.</summary>
    [Serializable]
    public struct StatBonus
    {
        public float strength, agility, intelligence;
        public float health, mana, damage, armor;
        /// <summary>Fraction, 0.2 means +20% attack speed.</summary>
        public float attackSpeed;
        public float moveSpeed;
        public float healthRegen;
    }

    /// <summary>
    /// Stats, health and the melee swing of a hero or a creep. Movement and decisions live in
    /// <see cref="HeroController"/> and <see cref="CreepAi"/>; visuals in <see cref="ArenaActor"/>.
    /// </summary>
    [RequireComponent(typeof(ArenaActor))]
    public sealed class Unit : Targetable
    {
        // Attribute conversion. Placeholder numbers until the heroes get their own data.
        public const float HealthPerStrength = 25f;
        public const float ManaPerIntelligence = 15f;
        public const float ArmorPerAgility = 0.14f;
        public const float AttackSpeedPerAgility = 0.02f;
        // Damage lands this far past the nominal reach so a target that steps back mid swing is still hit.
        private const float HitSlack = 0.75f;

        private static readonly List<Unit> all = new List<Unit>();
        private static int serialCounter;

        public static IReadOnlyList<Unit> All => all;
        /// <summary>Victim, killer (may be null).</summary>
        public static event Action<Unit, Unit> Killed;

        public Faction faction;
        public string displayName = "Юнит";
        public GameObject modelPrefab;
        public ArenaMap map;

        public float baseMaxHealth = 100f;
        public float baseMaxMana;
        public float baseArmor;
        public float baseDamageMin = 5f;
        public float baseDamageMax = 7f;
        /// <summary>Reach measured from the attacker's centre to the target's edge.</summary>
        public float attackRange = 1.1f;
        public float baseAttackInterval = 1.5f;
        /// <summary>Share of the swing before the blow lands.</summary>
        public float swingFraction = 0.4f;
        public float baseMoveSpeed = 4f;
        /// <summary>
        /// Clearance used for routes and shoving. Every unit uses the same value: the pathing grid caches its clearance
        /// for one radius, so units with different ones would make it rebuild on each other's route renewals.
        /// </summary>
        public float pathRadius = 0.4f;
        public float healthRegen;
        public float manaRegen;
        public int bounty;

        [NonSerialized] public float health;
        [NonSerialized] public float mana;

        private StatBonus bonus;
        private ArenaActor actor;
        private Targetable pendingTarget;
        private float hitAt;
        private float swingLockedUntil;
        private float nextAttackAt;
        private int serial;

        public event Action<Unit, float, Unit> Damaged;
        public event Action<Unit, Unit> Died;

        public ArenaActor Actor => actor;
        public StatBonus Bonus => bonus;
        public override bool IsAlive => health > 0f;
        public bool IsSwinging => Time.time < swingLockedUntil;
        public bool CanAttackNow => IsAlive && Time.time >= nextAttackAt;

        public float MaxHealth => baseMaxHealth + bonus.health + bonus.strength * HealthPerStrength;
        public float MaxMana => baseMaxMana + bonus.mana + bonus.intelligence * ManaPerIntelligence;
        public float Armor => baseArmor + bonus.armor + bonus.agility * ArmorPerAgility;
        public float DamageMin => baseDamageMin + bonus.damage;
        public float DamageMax => baseDamageMax + bonus.damage;
        public float AttackInterval => baseAttackInterval / Mathf.Max(0.2f, 1f + bonus.attackSpeed + bonus.agility * AttackSpeedPerAgility);
        public float MoveSpeed => baseMoveSpeed + bonus.moveSpeed;
        public float HealthRegen => healthRegen + bonus.healthRegen;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            all.Clear();
            Killed = null;
        }

        private void Awake()
        {
            actor = GetComponent<ArenaActor>();
            serial = ++serialCounter;
            health = MaxHealth;
            mana = MaxMana;
        }

        private void OnEnable()
        {
            if (!all.Contains(this)) all.Add(this);
        }

        private void OnDisable()
        {
            all.Remove(this);
        }

        private void Start()
        {
            actor.Initialize(modelPrefab, height, radius, faction == Faction.Hero, null);
        }

        /// <summary>Replaces the item bonuses; the health and mana fractions are kept, like in Warcraft.</summary>
        public void SetBonus(StatBonus next)
        {
            var healthShare = MaxHealth > 0f ? health / MaxHealth : 1f;
            var manaShare = MaxMana > 0f ? mana / MaxMana : 1f;
            bonus = next;
            if (IsAlive) health = Mathf.Max(1f, MaxHealth * healthShare);
            mana = MaxMana * manaShare;
        }

        public override bool CanBeAttackedBy(Unit attacker) => IsAlive && attacker != null && attacker.faction != faction;

        public void Heal(float amount) => health = Mathf.Min(MaxHealth, health + Mathf.Max(0f, amount));
        public void RestoreMana(float amount) => mana = Mathf.Min(MaxMana, mana + Mathf.Max(0f, amount));

        public bool TrySpendMana(float amount)
        {
            if (mana < amount) return false;
            mana -= amount;
            return true;
        }

        /// <summary>Warcraft armor curve: each point reduces damage by about 6%, with diminishing returns.</summary>
        public static float DamageMultiplier(float armor)
        {
            return armor >= 0f ? 1f - 0.06f * armor / (1f + 0.06f * armor) : 2f - Mathf.Pow(0.94f, -armor);
        }

        public override void ApplyDamage(float amount, Unit source)
        {
            if (!IsAlive) return;
            var taken = amount * DamageMultiplier(Armor);
            health = Mathf.Max(0f, health - taken);
            Damaged?.Invoke(this, taken, source);
            if (health <= 0f) Die(source);
        }

        private void Die(Unit killer)
        {
            pendingTarget = null;
            all.Remove(this);
            actor.Die();
            Died?.Invoke(this, killer);
            Killed?.Invoke(this, killer);
        }

        /// <summary>Starts the swing animation; the blow lands after the wind-up if the target is still in reach.</summary>
        public void BeginAttack(Targetable target)
        {
            var interval = AttackInterval;
            var swing = Mathf.Min(interval, 0.9f);
            pendingTarget = target;
            hitAt = Time.time + swing * swingFraction;
            swingLockedUntil = hitAt + 0.12f;
            nextAttackAt = Time.time + interval;
            actor.Attack(swing);
        }

        private void Update()
        {
            if (!IsAlive) return;

            if (HealthRegen > 0f) Heal(HealthRegen * Time.deltaTime);
            if (manaRegen > 0f) RestoreMana(manaRegen * Time.deltaTime);

            if (pendingTarget != null && Time.time >= hitAt)
            {
                var target = pendingTarget;
                pendingTarget = null;
                if (target.IsAlive && target.EdgeDistance(transform.position) <= attackRange + HitSlack)
                    target.ApplyDamage(UnityEngine.Random.Range(DamageMin, DamageMax + 1f), this);
            }
        }

        // Bodies are solid: units shove each other apart instead of stacking, which is what makes a passage
        // between barrels a queue. The shove respects the pathing grid, so nobody is pushed into a wall.
        private void LateUpdate()
        {
            if (!IsAlive || map == null) return;
            foreach (var other in all)
            {
                if (other == this || !other.IsAlive) continue;
                var offset = transform.position - other.transform.position;
                offset.y = 0f;
                var minimum = radius + other.radius;
                var squared = offset.sqrMagnitude;
                if (squared >= minimum * minimum) continue;

                var distance = Mathf.Sqrt(squared);
                var direction = distance > 0.001f ? offset / distance : Quaternion.Euler(0f, serial * 47 % 360, 0f) * Vector3.forward;
                transform.position = map.Move(transform.position, direction * ((minimum - distance) * 0.5f), pathRadius);
            }
        }
    }
}
