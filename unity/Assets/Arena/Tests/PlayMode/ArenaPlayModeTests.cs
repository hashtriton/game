using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Arena.Tests
{
    public sealed class ArenaPlayModeTests
    {
        ArenaGame game;
        Keyboard keyboard;
        Mouse mouse;
        InputSettings.BackgroundBehavior previousBackground;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            previousBackground=InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            keyboard=InputSystem.AddDevice<Keyboard>();
            mouse=InputSystem.AddDevice<Mouse>();
            keyboard.MakeCurrent();mouse.MakeCurrent();
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("Lia39Arena");
            yield return null;
            game=Object.FindAnyObjectByType<ArenaGame>();
            Assert.That(game,Is.Not.Null);
            Assert.That(game.Run.Phase,Is.EqualTo(RunPhase.Ready));
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            if(keyboard!=null && keyboard.added)InputSystem.RemoveDevice(keyboard);
            if(mouse!=null && mouse.added)InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior=previousBackground;
            Time.timeScale=1;
            yield return null;
        }

        void Key(params Key[] keys)=>InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));
        void Pointer(Vector3 world,int button=-1)
        {
            var point=game.arenaCamera.WorldToScreenPoint(world);
            var state=new MouseState{position=new Vector2(point.x,point.y)};
            if(button>=0)state=state.WithButton((MouseButton)button);
            InputSystem.QueueStateEvent(mouse,state);
        }

        [UnityTest]
        public IEnumerator KeyboardPointerAbilitiesPauseAndRetry_WorkThroughRuntime()
        {
            Key(UnityEngine.InputSystem.Key.Enter);
            yield return null;yield return null;
            Key();
            Assert.That(game.Run.Phase,Is.EqualTo(RunPhase.Wave),"Enter starts run via HUD");
            var initial=game.Hero.position;
            Key(UnityEngine.InputSystem.Key.D);
            yield return new WaitForSeconds(.25f);
            Key();yield return null;
            Assert.That(game.Hero.position.x,Is.GreaterThan(initial.x+.5f),"WASD moves");
            var target=game.Hero.position+new Vector3(-2,0,-2);
            target=game.arenaMap.FindNearestWalkable(target);
            float before=Vector3.Distance(target,game.Hero.position);
            Pointer(target,1);
            yield return new WaitForSeconds(.3f);
            Pointer(target);yield return null;
            Assert.That(Vector3.Distance(target,game.Hero.position),Is.LessThan(before-.5f),"RMB moves toward floor target");
            Key(UnityEngine.InputSystem.Key.Q);
            yield return null;yield return null;
            Key();
            Assert.That(game.SkillRemaining,Is.GreaterThan(4),"Q starts skill cooldown");
            Assert.That(game.TrySkill(),Is.False,"skill cannot be spammed");
            Key(UnityEngine.InputSystem.Key.Space);
            yield return null;yield return null;
            Key();
            Assert.That(game.DodgeRemaining,Is.GreaterThan(2),"Space starts dash");
            Assert.That(game.TryDodge(Vector3.right),Is.False,"dash cannot be spammed");
            Key(UnityEngine.InputSystem.Key.Escape);
            yield return null;yield return null;Key();
            Assert.That(game.Run.IsPaused,Is.True);
            var paused=game.Hero.position;float hp=game.Run.Health;
            Key(UnityEngine.InputSystem.Key.W);
            yield return new WaitForSecondsRealtime(.15f);Key();
            Assert.That(game.Hero.position,Is.EqualTo(paused));
            Assert.That(game.Run.Health,Is.EqualTo(hp));
            Assert.That(game.TryMeleeAttack(),Is.False);
            Assert.That(game.TrySkill(),Is.False);
            game.RestartRun();yield return null;
            Assert.That(Time.timeScale,Is.EqualTo(1));
            Assert.That(game.Run.Wave,Is.EqualTo(1));
            Assert.That(game.Run.Kills,Is.Zero);
            Assert.That(game.Run.Health,Is.EqualTo(140));
            Assert.That(Vector3.Distance(game.Hero.position,game.arenaMap.HeroSpawn),Is.LessThan(.01f));
            Assert.That(game.ActiveEnemyCount,Is.Zero);
        }

        [UnityTest]
        public IEnumerator AnimatedModelsRemainVisibleAndCameraFramesHero()
        {
            yield return new WaitForSeconds(.15f);
            var renderers=game.Hero.GetComponentsInChildren<SkinnedMeshRenderer>();
            Assert.That(renderers.Length,Is.GreaterThan(0));
            var bounds=new Bounds();bool any=false;
            foreach(var renderer in renderers)
            {
                var mesh=new Mesh();renderer.BakeMesh(mesh,true);
                foreach(var vertex in mesh.vertices)
                {
                    var point=renderer.transform.TransformPoint(vertex);
                    if(!any){bounds=new Bounds(point,Vector3.zero);any=true;}else bounds.Encapsulate(point);
                }
                Object.Destroy(mesh);
            }
            Assert.That(bounds.size.y,Is.InRange(1.5f,2.3f),"actual skinned vertices must stay human-sized after animation");
            Assert.That(renderers[0].bounds.size.y,Is.InRange(1.5f,2.5f),"Independent rendered bounds must also remain human-sized");
            Assert.That(bounds.min.y-game.Hero.position.y,Is.InRange(-.15f,.25f));
            var visible=game.arenaCamera.WorldToViewportPoint(game.Hero.position);
            Assert.That(visible.y,Is.InRange(.18f,.81f),"Hero must remain between HUD panels");
            Assert.That(visible.x,Is.InRange(.2f,.8f));
            Assert.That(game.Hero.GetComponentInChildren<Animation>().IsPlaying("Idle"),Is.True);
            foreach(var renderer in game.Hero.GetComponentsInChildren<Renderer>())
                Assert.That(renderer.sharedMaterials.All(m=>m && m.shader.isSupported),Is.True);
        }

        [UnityTest]
        public IEnumerator OriginalMapCanBeExploredBeforeCombat()
        {
            var start=game.Hero.position;
            Key(UnityEngine.InputSystem.Key.S);
            yield return new WaitForSeconds(.3f);Key();yield return null;
            Assert.That(Vector3.Distance(start,game.Hero.position),Is.GreaterThan(.5f));
            Assert.That(game.Run.Phase,Is.EqualTo(RunPhase.Ready));
            Assert.That(game.ActiveEnemyCount,Is.Zero);
            Key(UnityEngine.InputSystem.Key.Escape);yield return null;yield return null;Key();
            Assert.That(game.Run.IsPaused,Is.True,"Exploration has a working exit/pause menu");
            game.TogglePause();
            Key(UnityEngine.InputSystem.Key.Tab);yield return null;yield return null;Key();
            Assert.That(game.arenaCamera.GetComponent<ArenaMapCamera>().Overview,Is.True);
            Assert.That(game.arenaCamera.WorldToViewportPoint(new Vector3(0,0,-64)).y,Is.GreaterThan(.15f));
            Assert.That(game.arenaCamera.WorldToViewportPoint(new Vector3(0,0,64)).y,Is.LessThan(.885f));
            yield return null;
            Key(UnityEngine.InputSystem.Key.Tab);yield return null;yield return null;Key();
            Assert.That(game.arenaCamera.GetComponent<ArenaMapCamera>().Overview,Is.False);
            var heroView=game.arenaCamera.WorldToViewportPoint(game.Hero.position);
            Assert.That(heroView.z,Is.InRange(.1f,game.arenaCamera.farClipPlane));
            Assert.That(heroView.y,Is.InRange(.18f,.81f));
        }

        [UnityTest]
        public IEnumerator StationaryHeroCanLoseAndRestartWithoutStaleEnemies()
        {
            game.StartRun();
            Time.timeScale=12;
            float limit=Time.realtimeSinceStartup+20;
            while(game.Run.Phase==RunPhase.Wave && Time.realtimeSinceStartup<limit)yield return null;
            Assert.That(game.Run.Phase,Is.EqualTo(RunPhase.Lost),"real enemies must damage hero to death");
            Assert.That(game.Run.Health,Is.Zero);
            game.RestartRun();yield return null;
            Assert.That(game.Run.Health,Is.EqualTo(140));
            Assert.That(game.Run.Kills,Is.Zero);
            Assert.That(game.ActiveEnemyCount,Is.Zero);
            Assert.That(Object.FindObjectsByType<ArenaActor>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ControlledCombatCompletesAllWavesAndBossWithRealDamage()
        {
            // Deterministic integration probe, not a natural human playthrough: move spawned
            // enemies into range and protect the hero while using the actual damage/death code.
            game.StartRun();Time.timeScale=5;
            var flags=BindingFlags.NonPublic|BindingFlags.Instance;
            var aim=typeof(ArenaGame).GetField("aimDirection",flags);
            var invulnerability=typeof(ArenaGame).GetField("invulnerableRemaining",flags);
            float deadline=Time.realtimeSinceStartup+45;
            int upgrades=0;bool bossSeen=false;
            while(game.Run.Phase!=RunPhase.Won && Time.realtimeSinceStartup<deadline)
            {
                invulnerability.SetValue(game,100f);
                aim.SetValue(game,Vector3.forward);
                foreach(var actor in Object.FindObjectsByType<ArenaActor>(FindObjectsSortMode.None))
                    if(!actor.IsHero && !actor.IsDead)actor.transform.position=game.Hero.position+Vector3.forward;
                game.TryMeleeAttack();game.TrySkill();
                if(game.Run.Wave==5 && game.ActiveEnemyCount>0)bossSeen=true;
                if(game.Run.Phase==RunPhase.Upgrade){game.ChooseUpgrade(0);upgrades++;}
                yield return null;
            }
            Assert.That(bossSeen,Is.True);
            Assert.That(upgrades,Is.EqualTo(4));
            Assert.That(game.Run.Phase,Is.EqualTo(RunPhase.Won));
            Assert.That(game.Run.Kills,Is.EqualTo(43));
            Assert.That(game.ActiveEnemyCount,Is.Zero);
            Assert.That(game.TryMeleeAttack(),Is.False);
            game.RestartRun();yield return null;
            Assert.That(game.Run.Damage,Is.EqualTo(24));
            Assert.That(game.Run.Kills,Is.Zero);
        }
    }
}
