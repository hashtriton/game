using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Arena
{
    public sealed class ArenaGame : MonoBehaviour
    {
        public GameObject heroPrefab;
        public GameObject meleePrefab;
        public GameObject rangedPrefab;
        public GameObject bossPrefab;
        public Camera arenaCamera;
        public Material effectMaterial;
        public ArenaMap arenaMap;
        public float arenaRadius = 13f;

        private sealed class Enemy
        {
            public ArenaActor Actor;
            public float Health;
            public float Speed;
            public float Damage;
            public float Cooldown;
            public float Windup;
            public Vector3 Target;
            public bool Ranged;
            public bool Boss;
            public List<Vector3> Path;
            public int PathIndex;
            public float Repath;
        }

        private sealed class Projectile
        {
            public GameObject Visual;
            public Vector3 Velocity;
            public float Damage;
            public float Remaining;
        }

        private readonly List<Enemy> enemies = new List<Enemy>();
        private readonly List<Projectile> projectiles = new List<Projectile>();
        private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
        private ArenaRun run;
        private ArenaActor hero;
        private Transform world;
        private PointerEventData pointer;
        private EventSystem pointerEventSystem;
        private RunPhase observedPhase;
        private Vector3 moveTarget;
        private Vector3 pointerTarget;
        private Vector3 aimDirection = Vector3.forward;
        private Vector3 dashDirection;
        private bool hasMoveTarget;
        private float spawnRemaining;
        private float attackRemaining;
        private float skillRemaining;
        private float dodgeRemaining;
        private float dashRemaining;
        private float invulnerableRemaining;
        private int spawnedThisWave;
        private List<Vector3> heroPath;
        private int heroPathIndex;

        public ArenaRun Run => run;
        public Transform Hero => hero != null ? hero.transform : null;
        public int ActiveEnemyCount => enemies.Count;
        public float AttackRemaining => attackRemaining;
        public float SkillRemaining => skillRemaining;
        public float DodgeRemaining => dodgeRemaining;
        public const float SkillCooldown = 5f;
        public const float DodgeCooldown = 2.7f;

        private Vector3 Center => transform.position;
        private bool CanAct => run != null && run.Phase == RunPhase.Wave && !run.IsPaused && hero != null;
        private bool CanMove => run != null && (run.Phase == RunPhase.Wave || run.Phase == RunPhase.Ready) && !run.IsPaused && hero != null;

        private void Awake()
        {
            if (arenaCamera == null) arenaCamera = Camera.main;
            run = new ArenaRun();
            CreateWorld();
        }

        public void StartRun()
        {
            if (run == null || !run.StartWave()) return;
            BeginWave();
        }

        public void RestartRun()
        {
            Time.timeScale = 1f;
            ClearWorld();
            if (run == null) run = new ArenaRun();
            else run.Reset();
            CreateWorld();
            StartRun();
        }

        public void ChooseUpgrade(int index)
        {
            if (run == null || run.IsPaused || index < 0 || index > 2) return;
            if (!run.ChooseUpgrade((UpgradeKind)index)) return;
            ArenaEffects.Ring(world, effectMaterial, Hero.position, 0.7f,
                new Color(0.3f, 1f, 0.75f), 0.55f, 2f);
            BeginWave();
        }

        public void TogglePause()
        {
            if (run == null || (run.Phase != RunPhase.Ready && run.Phase != RunPhase.Wave && run.Phase != RunPhase.Upgrade)) return;
            run.SetPaused(!run.IsPaused);
            Time.timeScale = run.IsPaused ? 0f : 1f;
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
            if (run != null) run.SetPaused(false);
        }

        private void CreateWorld()
        {
            var root = new GameObject("Arena runtime");
            root.transform.SetParent(transform, false);
            world = root.transform;
            hero = CreateActor("Hero", heroPrefab, arenaMap ? arenaMap.HeroSpawn : Center, 1.9f, 0.45f, true);
            aimDirection = Vector3.forward;
            hasMoveTarget = false;
            heroPath = null;
            attackRemaining = skillRemaining = dodgeRemaining = dashRemaining = invulnerableRemaining = 0f;
            observedPhase = run.Phase;
        }

        private void ClearWorld()
        {
            enemies.Clear();
            projectiles.Clear();
            if (world != null)
            {
                world.gameObject.SetActive(false);
                Destroy(world.gameObject);
            }
            hero = null;
        }

        private ArenaActor CreateActor(string actorName, GameObject prefab, Vector3 position,
            float height, float radius, bool isHero)
        {
            var root = new GameObject(actorName);
            root.transform.SetParent(world, false);
            root.transform.position = position;
            var actor = root.AddComponent<ArenaActor>();
            actor.Initialize(prefab, height, radius, isHero, effectMaterial);
            return actor;
        }

        private void BeginWave()
        {
            hasMoveTarget = false;
            spawnedThisWave = 0;
            spawnRemaining = 0.65f;
            observedPhase = run.Phase;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) TogglePause();
            if (!CanMove)
            {
                ObservePhase();
                return;
            }

            var dt = Time.deltaTime;
            attackRemaining = Mathf.Max(0f, attackRemaining - dt);
            skillRemaining = Mathf.Max(0f, skillRemaining - dt);
            dodgeRemaining = Mathf.Max(0f, dodgeRemaining - dt);
            invulnerableRemaining = Mathf.Max(0f, invulnerableRemaining - dt);
            ReadInput(dt);
            if (!CanAct) return;
            UpdateSpawning(dt);
            UpdateEnemies(dt);
            if (CanAct) UpdateProjectiles(dt);
            ObservePhase();
        }

        private void ReadInput(float dt)
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var input = Vector2.zero;
            if (keyboard != null)
            {
                input.x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
                input.y = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            }
            var forward = arenaCamera != null ? arenaCamera.transform.forward : Vector3.forward;
            var right = arenaCamera != null ? arenaCamera.transform.right : Vector3.right;
            forward.y = right.y = 0f;
            if (forward.sqrMagnitude < .001f) forward = Vector3.forward;
            var movement = Vector3.ClampMagnitude(forward.normalized * input.y + right.normalized * input.x, 1f);
            var overUi = PointerOverUi(mouse);
            if (mouse != null && arenaCamera != null && !overUi)
            {
                var ray = arenaCamera.ScreenPointToRay(mouse.position.ReadValue());
                var floor = new Plane(Vector3.up, Hero.position);
                bool hitFloor = floor.Raycast(ray, out var distance);
                if (Physics.Raycast(ray, out var groundHit, 400f) && groundHit.collider is TerrainCollider)
                { distance = groundHit.distance; hitFloor = true; }
                if (hitFloor)
                {
                    var point = ray.GetPoint(distance);
                    var direction = point - Hero.position;
                    direction.y = 0f;
                    if (direction.sqrMagnitude > 0.01f) aimDirection = direction.normalized;
                    if (mouse.rightButton.wasPressedThisFrame || (mouse.rightButton.isPressed && (point-pointerTarget).sqrMagnitude > 1f))
                    {
                        pointerTarget = point;
                        moveTarget = ClampPosition(point, hero.Radius);
                        heroPath = arenaMap ? arenaMap.FindPath(Hero.position, moveTarget, hero.Radius) : null;
                        heroPathIndex = 0;
                        if (heroPath != null && heroPath.Count > 0) moveTarget = heroPath[heroPath.Count-1];
                        hasMoveTarget = true;
                    }
                }
            }
            if (input.sqrMagnitude > 0f) hasMoveTarget = false;
            else if (hasMoveTarget)
            {
                if (heroPath != null && heroPath.Count == 0) hasMoveTarget = false;
                while (heroPath != null && heroPathIndex < heroPath.Count && FlatDistance(Hero.position, heroPath[heroPathIndex]) < .3f)
                    heroPathIndex++;
                var target = heroPath != null && heroPathIndex < heroPath.Count ? heroPath[heroPathIndex] : moveTarget;
                var towardTarget = target - Hero.position;
                towardTarget.y = 0f;
                if (FlatDistance(moveTarget, Hero.position) <= Mathf.Max(0.1f, run.MoveSpeed * dt)) hasMoveTarget = false;
                else if (hasMoveTarget) movement = towardTarget.normalized * Mathf.Min(1,towardTarget.magnitude/Mathf.Max(.001f,run.MoveSpeed*dt));
            }
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                TryDodge(movement.sqrMagnitude > 0.01f ? movement : aimDirection);
            if (dashRemaining > 0f)
            {
                var step = Mathf.Min(dt, dashRemaining);
                Hero.position = Move(Hero.position, dashDirection * (20f * step), hero.Radius);
                dashRemaining = Mathf.Max(0f, dashRemaining - dt);
                hero.Face(dashDirection, dt);
                hero.SetMoving(true);
            }
            else
            {
                Hero.position = Move(Hero.position, movement * (run.MoveSpeed * dt), hero.Radius);
                hero.Face(mouse != null && mouse.leftButton.isPressed && !overUi ? aimDirection :
                    movement.sqrMagnitude > 0.01f ? movement : aimDirection, dt);
                hero.SetMoving(movement.sqrMagnitude > 0.01f);
            }
            if (!overUi && mouse != null && mouse.leftButton.isPressed) TryMeleeAttack();
            if (keyboard != null && keyboard.qKey.wasPressedThisFrame) TrySkill();
        }

        private bool PointerOverUi(Mouse mouse)
        {
            if (mouse == null || EventSystem.current == null) return false;
            if (pointer == null || pointerEventSystem != EventSystem.current)
            {
                pointerEventSystem = EventSystem.current;
                pointer = new PointerEventData(EventSystem.current);
            }
            pointer.position = mouse.position.ReadValue();
            uiHits.Clear();
            EventSystem.current.RaycastAll(pointer, uiHits);
            return uiHits.Count > 0;
        }

        public bool TryMeleeAttack()
        {
            if (!CanAct || attackRemaining > 0f || dashRemaining > 0f) return false;
            attackRemaining = run.AttackCooldown;
            hero.Face(aimDirection, 1f);
            hero.Attack(Mathf.Min(0.32f, run.AttackCooldown));
            ArenaEffects.Arc(world, effectMaterial, Hero.position, 1.65f,
                new Color(1f, 0.83f, 0.28f), 0.2f, 130f, aimDirection, 0.3f);
            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                var delta = enemies[i].Actor.transform.position - Hero.position;
                delta.y = 0f;
                var distance = delta.magnitude;
                if (distance <= 1.85f + enemies[i].Actor.Radius && ClearLine(Hero.position,enemies[i].Actor.transform.position) &&
                    (distance < 0.7f || Vector3.Dot(aimDirection, delta.normalized) >= 0.38f))
                    HitEnemy(i, run.Damage);
            }
            ObservePhase();
            return true;
        }

        public bool TrySkill()
        {
            if (!CanAct || skillRemaining > 0f || dashRemaining > 0f) return false;
            skillRemaining = SkillCooldown;
            hero.Attack(0.4f);
            ArenaEffects.Ring(world, effectMaterial, Hero.position, 1.3f,
                new Color(0.2f, 0.85f, 1f), 0.4f, 1.6f);
            for (var i = enemies.Count - 1; i >= 0; i--)
            {
                var delta = enemies[i].Actor.transform.position - Hero.position;
                delta.y = 0f;
                if (delta.magnitude > 3.4f + enemies[i].Actor.Radius || !ClearLine(Hero.position,enemies[i].Actor.transform.position)) continue;
                enemies[i].Actor.transform.position = Move(
                    enemies[i].Actor.transform.position, delta.normalized * 1.1f, enemies[i].Actor.Radius);
                HitEnemy(i, run.Damage * 1.65f);
            }
            ObservePhase();
            return true;
        }

        public bool TryDodge(Vector3 direction)
        {
            direction.y = 0f;
            if (!CanAct || dodgeRemaining > 0f || direction.sqrMagnitude < 0.001f) return false;
            hasMoveTarget = false;
            dodgeRemaining = DodgeCooldown;
            dashRemaining = 0.22f;
            invulnerableRemaining = 0.28f;
            dashDirection = direction.normalized;
            ArenaEffects.Ring(world, effectMaterial, Hero.position, 0.55f,
                new Color(0.4f, 0.9f, 1f), 0.25f, 0.9f);
            return true;
        }

        private void UpdateSpawning(float dt)
        {
            if (!CanAct || run.PendingEnemies <= 0) return;
            spawnRemaining -= dt;
            if (spawnRemaining > 0f) return;
            spawnRemaining = 0.55f;
            if (!run.SpawnEnemy()) return;
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var radial = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            var position = Center + radial * (arenaRadius - 1.1f);
            if ((position - Hero.position).sqrMagnitude < 25f) position = Center - radial * (arenaRadius - 1.1f);
            if (arenaMap)
            {
                position = arenaMap.SpawnPoints[spawnedThisWave % arenaMap.SpawnPoints.Length];
                position = arenaMap.FindNearestWalkable(position + new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f)));
            }
            var boss = run.Wave == ArenaRun.TotalWaves;
            var ranged = !boss && run.Wave >= 2 && spawnedThisWave % 3 == 2;
            spawnedThisWave++;
            var actor = CreateActor(boss ? "Arena boss" : ranged ? "Ranged enemy" : "Melee enemy",
                boss ? bossPrefab : ranged ? rangedPrefab : meleePrefab, position,
                boss ? 3.4f : 1.6f, boss ? 0.95f : 0.4f, false);
            enemies.Add(new Enemy
            {
                Actor = actor,
                Health = boss ? 680f : ranged ? 26f + run.Wave * 9f : 32f + run.Wave * 12f,
                Speed = boss ? 2.05f : ranged ? 2.15f : 2.55f + run.Wave * 0.12f,
                Damage = boss ? 32f : ranged ? 9f + run.Wave : 7f + run.Wave * 1.5f,
                Cooldown = 0.8f,
                Ranged = ranged,
                Boss = boss
            });
            ArenaEffects.Ring(world, effectMaterial, position, actor.Radius + 0.3f,
                boss ? new Color(1f, 0.18f, 0.12f) : new Color(1f, 0.55f, 0.18f), 0.65f, 0.8f);
        }

        private void UpdateEnemies(float dt)
        {
            for (var i = 0; i < enemies.Count && CanAct; i++)
            {
                var enemy = enemies[i];
                enemy.Cooldown = Mathf.Max(0f, enemy.Cooldown - dt);
                var delta = Hero.position - enemy.Actor.transform.position;
                delta.y = 0f;
                if (enemy.Windup > 0f)
                {
                    enemy.Windup -= dt;
                    enemy.Actor.SetMoving(false);
                    if (enemy.Windup <= 0f) ResolveEnemyAttack(enemy);
                    continue;
                }

                enemy.Actor.Face(delta, dt);
                var range = enemy.Boss ? 7.5f : enemy.Ranged ? 8f : 1.65f;
                var inRange = delta.sqrMagnitude <= range * range && ClearLine(enemy.Actor.transform.position, Hero.position);
                if (inRange && enemy.Cooldown <= 0f)
                {
                    enemy.Windup = enemy.Boss ? 1.05f : enemy.Ranged ? 0.65f : 0.48f;
                    enemy.Cooldown = enemy.Boss ? 3.1f : enemy.Ranged ? 2.5f : 1.65f;
                    enemy.Target = enemy.Boss || enemy.Ranged ? Hero.position :
                        enemy.Actor.transform.position + delta.normalized * 1.15f;
                    enemy.Actor.Attack(enemy.Windup);
                    if (!enemy.Ranged)
                        ArenaEffects.Ring(world, effectMaterial, enemy.Target,
                            enemy.Boss ? 3.05f : 1.05f, new Color(1f, 0.12f, 0.13f, 0.95f),
                            enemy.Windup, 0f, false);
                    continue;
                }

                var movement = !inRange || (!enemy.Boss && !enemy.Ranged && delta.magnitude > 1.15f)
                    ? delta.normalized * enemy.Speed : Vector3.zero;
                if (arenaMap && !inRange)
                {
                    enemy.Repath -= dt;
                    if (enemy.Path == null || enemy.Repath <= 0)
                    {
                        enemy.Path = arenaMap.FindPath(enemy.Actor.transform.position, Hero.position, enemy.Actor.Radius);
                        enemy.PathIndex = 0; enemy.Repath = 1.2f + i * .015f;
                    }
                    while (enemy.PathIndex < enemy.Path.Count && FlatDistance(enemy.Actor.transform.position, enemy.Path[enemy.PathIndex]) < .35f) enemy.PathIndex++;
                    if (enemy.PathIndex < enemy.Path.Count)
                    {
                        var next = enemy.Path[enemy.PathIndex]-enemy.Actor.transform.position; next.y=0;
                        movement = next.normalized * Mathf.Min(enemy.Speed,next.magnitude/Mathf.Max(.001f,dt));
                    }
                    else movement = Vector3.zero;
                }
                if (enemy.Ranged && delta.magnitude < 4.5f) movement = -delta.normalized * enemy.Speed * 0.65f;
                var separation = Vector3.zero;
                for (var j = 0; j < enemies.Count; j++)
                {
                    if (i == j) continue;
                    var away = enemy.Actor.transform.position - enemies[j].Actor.transform.position;
                    away.y = 0f;
                    var minimum = enemy.Actor.Radius + enemies[j].Actor.Radius + 0.18f;
                    var distance = away.magnitude;
                    if (distance > 0.001f && distance < minimum)
                        separation += away / distance * ((minimum - distance) / minimum * 3.5f);
                }
                movement += separation;
                enemy.Actor.transform.position = Move(enemy.Actor.transform.position, movement * dt, enemy.Actor.Radius);
                enemy.Actor.SetMoving(movement.sqrMagnitude > 0.02f);
            }
        }

        private void ResolveEnemyAttack(Enemy enemy)
        {
            if (!CanAct) return;
            if (enemy.Ranged)
            {
                var origin = enemy.Actor.transform.position + Vector3.up * 0.8f;
                var target = enemy.Target + Vector3.up * 0.8f;
                var direction = (target - origin).normalized;
                projectiles.Add(new Projectile
                {
                    Visual = ArenaEffects.Orb(world, effectMaterial, origin, 0.32f,
                        new Color(1f, 0.37f, 0.18f)),
                    Velocity = direction * 6.2f,
                    Damage = enemy.Damage,
                    Remaining = 5f
                });
            }
            else
            {
                var radius = enemy.Boss ? 3.05f : 1.05f;
                ArenaEffects.Ring(world, effectMaterial, enemy.Target, radius,
                    new Color(1f, 0.3f, 0.12f), 0.27f, 0.2f);
                var delta = Hero.position - enemy.Target;
                delta.y = 0f;
                if (delta.sqrMagnitude <= (radius + hero.Radius * 0.5f) * (radius + hero.Radius * 0.5f))
                    HitHero(enemy.Damage);
            }
        }

        private void UpdateProjectiles(float dt)
        {
            for (var i = projectiles.Count - 1; i >= 0 && CanAct; i--)
            {
                var projectile = projectiles[i];
                projectile.Remaining -= dt;
                var from = projectile.Visual.transform.position;
                var next = from + projectile.Velocity * dt;
                var segment = next - from;
                var heroPoint = Hero.position + Vector3.up * 0.8f;
                var t = segment.sqrMagnitude > 0.00001f
                    ? Mathf.Clamp01(Vector3.Dot(heroPoint - from, segment) / segment.sqrMagnitude) : 0f;
                var hit = (heroPoint - (from + segment * t)).sqrMagnitude < 0.62f * 0.62f;
                if (hit) HitHero(projectile.Damage);
                if (hit || projectile.Remaining <= 0f || (arenaMap ? !ClearLine(from,next) : (next - Center).sqrMagnitude > (arenaRadius + 2f) * (arenaRadius + 2f)))
                {
                    Destroy(projectile.Visual);
                    projectiles.RemoveAt(i);
                }
                else projectile.Visual.transform.position = next;
            }
        }

        private void HitEnemy(int index, float damage)
        {
            var enemy = enemies[index];
            enemy.Health -= damage;
            ArenaEffects.Ring(world, effectMaterial, enemy.Actor.transform.position,
                enemy.Actor.Radius + 0.1f, new Color(1f, 0.85f, 0.52f), 0.15f, 0.6f);
            if (enemy.Health > 0f) return;
            enemy.Actor.Die();
            enemies.RemoveAt(index);
            run.EnemyDefeated();
        }

        private void HitHero(float damage)
        {
            if (!CanAct || invulnerableRemaining > 0f || !run.TakeDamage(damage)) return;
            invulnerableRemaining = 0.14f;
            ArenaEffects.Ring(world, effectMaterial, Hero.position, 0.6f,
                new Color(1f, 0.18f, 0.15f), 0.25f, 0.5f);
        }

        private Vector3 ClampPosition(Vector3 position, float radius)
        {
            if (arenaMap) return arenaMap.FindNearestWalkable(position);
            var offset = position - Center;
            offset.y = 0f;
            position = Center + Vector3.ClampMagnitude(offset, Mathf.Max(1f, arenaRadius - radius));
            return position;
        }

        private Vector3 Move(Vector3 from, Vector3 displacement, float radius) =>
            arenaMap ? arenaMap.Move(from,displacement,radius) : ClampPosition(from+displacement,radius);

        private static float FlatDistance(Vector3 a, Vector3 b) => new Vector2(a.x-b.x,a.z-b.z).magnitude;

        private bool ClearLine(Vector3 from, Vector3 to)
        {
            if (!arenaMap) return true;
            int steps = Mathf.Max(1,Mathf.CeilToInt(FlatDistance(from,to)/.3f));
            for (int i=1;i<=steps;i++) if (!arenaMap.IsWalkable(Vector3.Lerp(from,to,i/(float)steps))) return false;
            return true;
        }

        private void ObservePhase()
        {
            if (run == null || run.Phase == observedPhase) return;
            observedPhase = run.Phase;
            if (run.Phase == RunPhase.Wave) return;
            hasMoveTarget = false;
            dashRemaining = 0f;
            foreach (var projectile in projectiles)
                if (projectile.Visual != null) Destroy(projectile.Visual);
            projectiles.Clear();
            foreach (var enemy in enemies) enemy.Actor.SetMoving(false);
            if (hero != null)
            {
                if (run.Phase == RunPhase.Lost) hero.Die();
                else hero.SetMoving(false);
            }
            if (run.Phase == RunPhase.Won)
                ArenaEffects.Ring(world, effectMaterial, Hero.position, 1f,
                    new Color(1f, 0.85f, 0.3f), 1.7f, 5f);
        }
    }
}
