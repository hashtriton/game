using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Arena.Tests
{
    public sealed partial class OriginalItemInputTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        OriginalArenaRuntime runtime;
        OriginalSession session;
        OriginalWorld world;
        Mouse mouse;
        Keyboard keyboard;
        InputSettings.BackgroundBehavior previousBackground;

        [UnitySetUp] public IEnumerator Setup()
        {
            previousBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            mouse = InputSystem.AddDevice<Mouse>(); keyboard = InputSystem.AddDevice<Keyboard>();
            mouse.MakeCurrent(); keyboard.MakeCurrent();
            yield return SceneManager.LoadSceneAsync("Lia39Arena"); yield return null;
            runtime = UnityEngine.Object.FindFirstObjectByType<OriginalArenaRuntime>();
            Assert.That(runtime, Is.Not.Null);
            Assert.That(runtime.Host(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 0, "127.0.0.1"), Is.True);
            runtime.network.SendCommand(OriginalSessionCommandKind.SelectHero, heroId:"H008");
            runtime.network.SendCommand(OriginalSessionCommandKind.LobbyReady, ready:true);
            runtime.network.SendCommand(OriginalSessionCommandKind.Start);
            session = (OriginalSession)typeof(OriginalNetworkGame).GetField("session", Private).GetValue(runtime.network);
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(session);
            yield return null; yield return null;
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            if (runtime) runtime.Disconnect();
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = previousBackground;
            yield return null;
        }
        long Equip(string id)
        {
            var player = ((IList)typeof(OriginalSession).GetField("players", Private).GetValue(session))[0];
            var field = player.GetType().GetField("inventory");
            var inventory = ((OriginalInventory)field.GetValue(player)).Copy();
            var item = inventory.CreateInstance(id);
            Assert.That(inventory.TryPickup(item).Applied, Is.True);
            Assert.That(typeof(OriginalSession).GetMethod("ApplyEquipmentProfile", Private).Invoke(session,
                new object[]{player,inventory,world.UnitState(1)}), Is.True,id);
            field.SetValue(player,inventory);
            return item.instanceId;
        }
        void Point(Vector3 point, MouseButton? button=null)
        {
            var screen = runtime.viewCamera.WorldToScreenPoint(point);
            Assert.That(screen.z, Is.GreaterThan(0));
            var state = new MouseState {position = new Vector2(screen.x,screen.y)};
            if(button.HasValue)state=state.WithButton(button.Value);
            InputSystem.QueueStateEvent(mouse,state);
        }
        OriginalItemUseView Use(long id) => runtime.View.players[0].itemUses.Single(x=>x.instanceId==id);

        IEnumerator ClickHud(string name)
        {
            var button=UnityEngine.Object.FindFirstObjectByType<OriginalArenaHud>().GetComponentsInChildren<Button>()
                .Single(x=>x.gameObject.name==name);
            Assert.That(button.interactable,Is.True,name);
            var rect=(RectTransform)button.transform;
            var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point}.WithButton(MouseButton.Left));yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;yield return null;
        }

        [UnityTest] public IEnumerator DuelBetButtonsDebitAdjustSwitchAndDiscardThroughTheAuthority()
        {
            runtime.Disconnect();
            Assert.That(runtime.Host(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard),0,"127.0.0.1"),Is.True);
            session=(OriginalSession)typeof(OriginalNetworkGame).GetField("session",Private).GetValue(runtime.network);
            string hash=session.Snapshot().contentHash;
            foreach(int slot in new[]{2,3})
            {
                long connection=slot*10;
                foreach(var command in new[]{
                    new OriginalSessionCommand{kind=OriginalSessionCommandKind.Hello,sequence=1},
                    new OriginalSessionCommand{kind=OriginalSessionCommandKind.SelectHero,sequence=2,heroId=slot==2?"N0A0":"H024"},
                    new OriginalSessionCommand{kind=OriginalSessionCommandKind.LobbyReady,sequence=3,ready=true}})
                {
                    command.protocol=OriginalSession.Protocol;command.contentHash=hash;
                    Assert.That(session.Apply(connection,command),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                }
            }
            runtime.network.SendCommand(OriginalSessionCommandKind.SelectHero,heroId:"H008");
            runtime.network.SendCommand(OriginalSessionCommandKind.LobbyReady,ready:true);
            runtime.network.SendCommand(OriginalSessionCommandKind.Start);
            var catalogs=OriginalGameCatalogs.Load(runtime.network.dataAssets);
            var duel=new OriginalDuel(catalogs.Duels,OriginalDuelKind.Pairs,4,
                session.Snapshot().players.Select(p=>new OriginalDuelParticipant{slot=p.matchSlot,heroRawcode=p.heroId,rating=p.slot==1?0:1000-p.slot}).ToArray(),123);
            // Controlled countdown fixture; this test covers rendered input and
            // funded authority commands, not how the first four waves finish.
            typeof(OriginalSession).GetField("duel",Private).SetValue(session,duel);
            typeof(OriginalSession).GetField("duelId",Private).SetValue(session,1L);
            Assert.That(duel.Begin(),Is.True);
            typeof(OriginalSession).GetMethod("CollectDuelEvents",Private).Invoke(session,null);
            var bettor=((IList)typeof(OriginalSession).GetField("players",Private).GetValue(session))[0];
            ((OriginalInventory)bettor.GetType().GetField("inventory").GetValue(bettor)).GrantResources(1000,0);
            runtime.network.SendCommand(OriginalSessionCommandKind.SetQuickBuy,ready:false);
            yield return null;yield return null;
            long gold=runtime.View.players[0].gold;
            Assert.That(gold,Is.GreaterThanOrEqualTo(200));
            yield return ClickHud("НА ЛЕВОГО");
            Assert.That(runtime.View.duel.betStakes[1],Is.EqualTo(100));
            Assert.That(runtime.View.players[0].gold,Is.EqualTo(gold-100));
            yield return ClickHud("+100");Assert.That(runtime.View.duel.betStakes[1],Is.EqualTo(200));
            yield return ClickHud("-100");Assert.That(runtime.View.duel.betStakes[1],Is.EqualTo(100));
            Assert.That(runtime.View.players[0].gold,Is.EqualTo(gold-100));
            yield return ClickHud("НА ПРАВОГО");Assert.That(runtime.View.duel.betSides[1],Is.EqualTo(2));
            Assert.That(runtime.View.players[0].gold,Is.EqualTo(gold-100));
            yield return ClickHud("СБРОС БЕЗ ВОЗВРАТА");
            Assert.That(runtime.View.duel.betStakes[1],Is.Zero);
            Assert.That(runtime.View.players[0].gold,Is.EqualTo(gold-100));
            Assert.That(runtime.View.haltReason,Is.Null.Or.Empty);
        }

        [UnityTest] public IEnumerator HoveringPassiveItemShowsBonusesAndLeavingClosesTooltip()
        {
            Equip("I04J");runtime.network.SendCommand(OriginalSessionCommandKind.Stop);
            yield return null;yield return null;
            var hud=UnityEngine.Object.FindFirstObjectByType<OriginalArenaHud>();
            var button=hud.GetComponentsInChildren<Button>().Single(x=>x.gameObject.name=="Quick item 1");
            Assert.That(button.interactable,Is.False,"Passive item still supports pointer inspection.");
            var rect=(RectTransform)button.transform;
            var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;yield return null;
            var tooltip=hud.GetComponentsInChildren<RectTransform>(true).Single(x=>x.name=="Tooltip");
            Assert.That(tooltip.gameObject.activeSelf,Is.True);
            Assert.That(tooltip.GetComponentInChildren<Text>().text,Does.Contain("+16 к урону"));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(Screen.width/2,Screen.height/2)});
            yield return null;yield return null;
            Assert.That(tooltip.gameObject.activeSelf,Is.False);
            Assert.That(runtime.View.haltReason,Is.Null.Or.Empty);
        }

        [UnityTest] public IEnumerator QuickSlotClickAndNumberKeyArmTheSameAuthoritativeItem()
        {
            long healing=Equip("I00T"),spit=Equip("I08I");
            var actor=world.UnitState(1);var profile=actor.profile;profile.maxMana=10000;
            world.UpdateProfile(1,profile,actor.health,10000);
            runtime.network.SendCommand(OriginalSessionCommandKind.Stop);
            yield return null;yield return null;
            var button=UnityEngine.Object.FindFirstObjectByType<OriginalArenaHud>().GetComponentsInChildren<Button>()
                .Single(x=>x.gameObject.name=="Quick item 1");
            var rect=(RectTransform)button.transform;
            var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState {position=point});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState {position=point}.WithButton(MouseButton.Left));yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState {position=point});yield return null;yield return null;
            Assert.That(runtime.ArmedItemInstance,Is.EqualTo(healing));
            Assert.That(Use(healing).cooldownRemaining,Is.Zero);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Assert.That(runtime.ArmedItemInstance,Is.Zero);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Digit2));yield return null;yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Assert.That(runtime.ArmedItemInstance,Is.EqualTo(spit));
            Assert.That(Use(spit).cooldownRemaining,Is.Zero);
            Point(runtime.Hero.position+Vector3.right*1.2f,MouseButton.Left);yield return null;yield return null;
            Point(runtime.Hero.position);yield return null;
            Assert.That(runtime.ArmedItemInstance,Is.Zero);
            Assert.That(Use(spit).cooldownRemaining,Is.GreaterThan(0),runtime.Notice);
            Assert.That(runtime.View.haltReason,Is.Null.Or.Empty);
        }

        [UnityTest] public IEnumerator LongTooltipScrollsAndMenuBlocksItemHotkeysUntilClosed()
        {
            long item=Equip("I08I");runtime.network.SendCommand(OriginalSessionCommandKind.Stop);
            var actor=world.UnitState(1);var profile=actor.profile;profile.maxMana=10000;
            world.UpdateProfile(1,profile,actor.health,10000);
            yield return null;yield return null;
            var hud=UnityEngine.Object.FindFirstObjectByType<OriginalArenaHud>();
            var button=hud.GetComponentsInChildren<Button>().Single(x=>x.name=="Quick item 1");
            typeof(OriginalHudTooltipTarget).GetField("description",Private).SetValue(button.GetComponent<OriginalHudTooltipTarget>(),
                (Func<string>)(()=>string.Join("\n",Enumerable.Range(1,80).Select(i=>"Свойство предмета "+i))));
            var rect=(RectTransform)button.transform;
            var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;yield return null;
            var tooltip=hud.GetComponentsInChildren<RectTransform>(true).Single(x=>x.name=="Tooltip");
            var text=tooltip.GetComponentInChildren<Text>();
            Assert.That(text.rectTransform.rect.height,Is.GreaterThan(tooltip.rect.height));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point,scroll=new Vector2(0,-600)});yield return null;yield return null;
            Assert.That(text.rectTransform.anchoredPosition.y,Is.GreaterThan(0));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F10));yield return null;yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Assert.That(runtime.InputBlocked,Is.True);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Digit1));yield return null;yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Assert.That(runtime.ArmedItemInstance,Is.Zero);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F10));yield return null;yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Assert.That(runtime.InputBlocked,Is.False);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Digit1));yield return null;yield return null;
            Assert.That(runtime.ArmedItemInstance,Is.EqualTo(item));
            Assert.That(Use(item).cooldownRemaining,Is.Zero);
        }

        [UnityTest] public IEnumerator SelectingSummonShowsItsOwnSpellsAndClickUsesItsMana()
        {
            int id=OriginalWorld.FirstSummonEntityId;
            var origin=world.UnitState(1).position;
            Assert.That(world.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=id,ownerSlot=1,sourceHeroEntityId=1,rawcode="n01R",
                profile=new OriginalWorldUnitProfile{maxHealth=1000,maxMana=1000,moveSpeed=250,collisionRadius=16},
                health=1000,mana=1000,position=new OriginalPoint(origin.x+100,origin.y)}}),Is.True);
            runtime.network.SendCommand(OriginalSessionCommandKind.Stop);yield return null;yield return null;
            Assert.That(runtime.SelectOwnedUnit(id),Is.True);yield return null;yield return null;
            int slot=Array.FindIndex(runtime.SelectedUnitAbilities,a=>a.id=="A030");
            Assert.That(slot,Is.InRange(0,4));
            var hud=UnityEngine.Object.FindFirstObjectByType<OriginalArenaHud>();
            var cards=(Button[])typeof(OriginalArenaHud).GetField("castButtons",Private).GetValue(hud);
            var upgrades=(Button[])typeof(OriginalArenaHud).GetField("skillButtons",Private).GetValue(hud);
            Assert.That(upgrades.All(b=>!b.gameObject.activeSelf),Is.True);
            Assert.That(cards[slot].GetComponentInChildren<Text>().text,Does.Contain(runtime.SelectedAbilityName(slot)));
            var rect=(RectTransform)cards[slot].transform;
            var screen=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen}.WithButton(MouseButton.Left));yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen});yield return null;yield return null;
            Assert.That(runtime.ArmedSkillId,Is.EqualTo("A030"));
            double heroMana=world.UnitState(1).mana;
            Point(runtime.WorldPoint(world.UnitState(id).position)+Vector3.up*.8f,MouseButton.Left);yield return null;
            Point(runtime.WorldPoint(world.UnitState(id).position));
            float deadline=Time.realtimeSinceStartup+3;
            while(runtime.SelectedUnitAbilities.Single(a=>a.id=="A030").cooldownRemaining<=0&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(world.UnitState(id).mana,Is.LessThan(1000),runtime.Notice);
            Assert.That(world.UnitState(1).mana,Is.GreaterThanOrEqualTo(heroMana));
            Assert.That(runtime.ArmedSkillId,Is.Null);
            Assert.That(runtime.SelectedUnitAbilities.Single(a=>a.id=="A030").cooldownRemaining,Is.GreaterThan(0));
            Assert.That(runtime.SelectOwnedUnit(1),Is.True);yield return null;yield return null;
            Assert.That(upgrades.Count(b=>b.gameObject.activeSelf),Is.EqualTo(5));
            Assert.That(runtime.View.haltReason,Is.Null.Or.Empty);
        }

        [UnityTest] public IEnumerator GroundItemAndWellMouseOrdersApproachUseAndRemoveWorldVisuals()
        {
            var hero=world.UnitState(1);
            Assert.That(world.UpdateProfile(1,hero.profile,hero.health,0),Is.True);
            var player=((IList)typeof(OriginalSession).GetField("players",Private).GetValue(session))[0];
            var inventory=(OriginalInventory)player.GetType().GetField("inventory").GetValue(player);
            var item=inventory.CreateInstance("rman",0);
            var catalogs=OriginalGameCatalogs.Load(runtime.network.dataAssets);
            float distance=(float)(catalogs.Native.Constant("PickupItemRange").Require()*1.7/runtime.map.unitsPerMeter);
            var target=runtime.map.FindNearestWalkable(runtime.WorldPoint(hero.position)+Vector3.right*distance);
            var ground=(System.Collections.Generic.SortedDictionary<long,OriginalGroundItemView>)typeof(OriginalSession).GetField("groundItems",Private).GetValue(session);
            ground.Add(item.instanceId,new OriginalGroundItemView{item=item,position=new OriginalPoint(target.x*runtime.map.unitsPerMeter,target.z*runtime.map.unitsPerMeter)});
            runtime.network.SendCommand(OriginalSessionCommandKind.Stop);yield return null;yield return null;
            runtime.cameraControl.CenterAt(target);yield return new WaitForSecondsRealtime(.25f);
            var itemObject=GameObject.Find("Ground item "+item.instanceId);
            Assert.That(itemObject,Is.Not.Null,"Server ground items must be visible in the world.");
            Point(itemObject.transform.position,MouseButton.Right);yield return null;
            Point(itemObject.transform.position);yield return null;
            Assert.That(ground.ContainsKey(item.instanceId),Is.True,"A far pickup must approach rather than teleport or restore mana immediately.");
            float deadline=Time.realtimeSinceStartup+8;
            while(ground.ContainsKey(item.instanceId)&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(ground.ContainsKey(item.instanceId),Is.False,runtime.Notice);
            deadline=Time.realtimeSinceStartup+1.5f;
            while(GameObject.Find("Ground item "+item.instanceId)&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(GameObject.Find("Ground item "+item.instanceId),Is.Null);
            Assert.That(world.UnitState(1).mana,Is.EqualTo(world.UnitState(1).profile.maxMana).Within(.01));

            // Controlled well fixture tests rendered input and authority movement;
            // source appearance/refill timing is covered by HealingWell tests.
            typeof(OriginalSession).GetField("healingWellPresent",Private).SetValue(session,true);
            typeof(OriginalSession).GetField("healingWellMana",Private).SetValue(session,20d);
            hero=world.UnitState(1);
            Assert.That(world.UpdateProfile(1,hero.profile,hero.health-200,20),Is.True);
            runtime.network.SendCommand(OriginalSessionCommandKind.Stop);yield return null;yield return null;
            var wellPoint=runtime.WorldPoint(new OriginalPoint(-60,-380));
            runtime.cameraControl.CenterAt(wellPoint);yield return new WaitForSecondsRealtime(.25f);
            var wellObject=GameObject.Find("Healing well");
            Assert.That(wellObject,Is.Not.Null);
            Assert.That(runtime.wellPrefab,Is.Not.Null,"The existing licensed fountain must be referenced by the scene.");
            Assert.That(wellObject.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m&&m.shader.isSupported)),Is.True);
            var bounds=(Bounds)typeof(OriginalArenaRuntime).GetField("wellBounds",Private).GetValue(runtime);
            Point(bounds.center,MouseButton.Right);yield return null;Point(bounds.center);yield return null;
            Assert.That(runtime.View.well.mana,Is.EqualTo(20),"The distant click must not consume the well before arrival.");
            deadline=Time.realtimeSinceStartup+12;
            while(runtime.View.well.mana>0&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(runtime.View.well.mana,Is.Zero,runtime.Notice);
            Assert.That(world.UnitState(1).mana,Is.GreaterThan(20));
            Assert.That(runtime.View.haltReason,Is.Null.Or.Empty);
            runtime.Disconnect();yield return null;yield return null;
            Assert.That(GameObject.Find("Healing well"),Is.Null);
        }

        [UnityTest] public IEnumerator MinimapMouseMovesCameraAndIssuesAnAuthoritativeMove()
        {
            var surface=UnityEngine.Object.FindFirstObjectByType<OriginalMinimapInput>();
            var rect=(RectTransform)surface.transform;
            var bounds=runtime.map.WorldBounds;
            var target=runtime.map.FindNearestWalkable(runtime.Hero.position+Vector3.right*8);
            float x=Mathf.InverseLerp(bounds.min.x,bounds.max.x,target.x),y=Mathf.InverseLerp(bounds.min.z,bounds.max.z,target.z);
            var screen=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(
                Mathf.Lerp(rect.rect.xMin,rect.rect.xMax,x),Mathf.Lerp(rect.rect.yMin,rect.rect.yMax,y),0)));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen});yield return null;
            var before=runtime.viewCamera.transform.position;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen}.WithButton(MouseButton.Left));yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen});
            for(int i=0;i<10;i++)yield return null;
            Assert.That(Vector3.Distance(before,runtime.viewCamera.transform.position),Is.GreaterThan(.1f));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen}.WithButton(MouseButton.Right));yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen});yield return null;
            var hero=runtime.LocalUnit;
            Assert.That(hero.order,Is.EqualTo(OriginalWorldOrder.Move));
            Assert.That(hero.destination.x,Is.EqualTo(target.x*runtime.map.unitsPerMeter).Within(2));
            Assert.That(hero.destination.y,Is.EqualTo(target.z*runtime.map.unitsPerMeter).Within(2));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.A));yield return null;yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Assert.That(runtime.AttackTargetArmed,Is.True);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen}.WithButton(MouseButton.Left));yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screen});yield return null;
            Assert.That(runtime.AttackTargetArmed,Is.False);
            var attackGoals=(IDictionary)typeof(OriginalSession).GetField("playerAttackGoals",Private).GetValue(session);
            Assert.That(attackGoals.Contains(1),Is.True,"Minimap A-click must retain the authority's attack destination.");
            Assert.That(runtime.View.haltReason,Is.Null.Or.Empty);
        }

        [UnityTest] public IEnumerator WardItemsArmPointPlaceAtMouseTargetAndEscapeCancelsWithoutSpending()
        {
            foreach(string itemId in new[]{"I021","I094"})
            {
                long instance=Equip(itemId);runtime.network.SendCommand(OriginalSessionCommandKind.Stop);
                yield return null;yield return null;
                var before=runtime.View.players[0].inventory.heroSlots.Single(x=>x!=null&&x.instanceId==instance);
                int chargesBefore=before.charges;Assert.That(chargesBefore,Is.GreaterThan(0));
                Assert.That(Use(instance).targetMode,Is.EqualTo(OriginalAbilityTargetMode.UnitOrPoint));
                Assert.That(runtime.RequestItemUse(instance),Is.True);
                Assert.That(runtime.ArmedItemInstance,Is.EqualTo(instance));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                Assert.That(runtime.ArmedItemInstance,Is.Zero);
                Assert.That(runtime.View.players[0].inventory.heroSlots.Single(x=>x!=null&&x.instanceId==instance).charges,Is.EqualTo(chargesBefore));
                Assert.That(Use(instance).cooldownRemaining,Is.Zero);
                var hero=world.UnitState(1);
                Assert.That(world.TryFindFreeSpawn(new OriginalPoint(hero.position.x+200,hero.position.y),16,128,out var target),Is.True);
                double dx=target.x-hero.position.x,dy=target.y-hero.position.y;
                Assert.That(dx*dx+dy*dy,Is.LessThanOrEqualTo(500*500));
                var targetWorld=runtime.WorldPoint(target);runtime.cameraControl.CenterAt(targetWorld);
                yield return new WaitForSecondsRealtime(.25f);
                Assert.That(runtime.RequestItemUse(instance),Is.True);
                Point(targetWorld);yield return null;
                Point(targetWorld,MouseButton.Left);yield return null;
                Point(targetWorld);yield return null;
                string rawcode=itemId=="I021"?"ohwd":"o00J";
                float deadline=Time.realtimeSinceStartup+2;
                while(!runtime.View.world.units.Any(x=>x.kind==OriginalWorldUnitKind.Summon&&x.rawcode==rawcode)&&Time.realtimeSinceStartup<deadline)
                    yield return null;
                var ward=runtime.View.world.units.Single(x=>x.kind==OriginalWorldUnitKind.Summon&&x.rawcode==rawcode);
                Assert.That(ward.position.x,Is.EqualTo(target.x).Within(1));
                Assert.That(ward.position.y,Is.EqualTo(target.y).Within(1));
                var remaining=runtime.View.players[0].inventory.heroSlots.SingleOrDefault(x=>x!=null&&x.instanceId==instance);
                Assert.That(remaining==null?0:remaining.charges,Is.EqualTo(chargesBefore-1));
                Assert.That(runtime.ArmedItemInstance,Is.Zero);
                Assert.That(runtime.View.haltReason,Is.Null.Or.Empty);
            }
        }

        [UnityTest] public IEnumerator RepeatingArmedWardUsesTheExplicitMatchingItemSelfTarget()
        {
            foreach(string itemId in new[]{"I021","I094"})
            {
                long instance=Equip(itemId);runtime.network.SendCommand(OriginalSessionCommandKind.Stop);
                yield return null;yield return null;
                int chargesBefore=runtime.View.players[0].inventory.heroSlots.Single(x=>x!=null&&x.instanceId==instance).charges;
                var hero=world.UnitState(1);
                Assert.That(runtime.RequestItemUse(instance),Is.True);
                Assert.That(runtime.ArmedItemInstance,Is.EqualTo(instance));
                Assert.That(runtime.RequestItemUse(instance),Is.True);
                string rawcode=itemId=="I021"?"ohwd":"o00J";
                float deadline=Time.realtimeSinceStartup+2;
                while(!runtime.View.world.units.Any(x=>x.kind==OriginalWorldUnitKind.Summon&&x.rawcode==rawcode)&&Time.realtimeSinceStartup<deadline)
                    yield return null;
                var ward=runtime.View.world.units.Single(x=>x.kind==OriginalWorldUnitKind.Summon&&x.rawcode==rawcode);
                double dx=ward.position.x-hero.position.x,dy=ward.position.y-hero.position.y;
                Assert.That(dx*dx+dy*dy,Is.LessThanOrEqualTo(128*128));
                var remaining=runtime.View.players[0].inventory.heroSlots.SingleOrDefault(x=>x!=null&&x.instanceId==instance);
                Assert.That(remaining==null?0:remaining.charges,Is.EqualTo(chargesBefore-1));
                Assert.That(runtime.ArmedItemInstance,Is.Zero);
                Assert.That(runtime.View.haltReason,Is.Null.Or.Empty);
            }
        }

        [UnityTest] public IEnumerator UnitAndPointItemsUseTheActualMousePathAndEscapeCancelsWithoutSpending()
        {
            long healing=Equip("I00T"), spit=Equip("I08I");
            var actor=world.UnitState(1);var profile=actor.profile;profile.maxMana=10000;
            world.UpdateProfile(1,profile,actor.health,10000);
            runtime.network.SendCommand(OriginalSessionCommandKind.Stop);
            yield return null; yield return null;
            Assert.That(runtime.RequestItemUse(healing),Is.True,JsonUtility.ToJson(runtime.View.players[0]));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            Assert.That(runtime.ArmedItemInstance,Is.Zero);
            Assert.That(Use(healing).cooldownRemaining,Is.Zero);
            Assert.That(runtime.RequestItemUse(healing),Is.True);
            Point(runtime.Hero.position+Vector3.up*.8f,MouseButton.Left);
            yield return null; yield return null;
            Point(runtime.Hero.position);
            Assert.That(runtime.ArmedItemInstance,Is.Zero);
            Assert.That(Use(healing).cooldownRemaining,Is.GreaterThan(0),runtime.Notice);
            yield return null;
            Assert.That(runtime.RequestItemUse(spit),Is.True);
            Point(runtime.Hero.position+Vector3.right*1.2f,MouseButton.Left);
            yield return null; yield return null;
            Point(runtime.Hero.position);
            Assert.That(runtime.ArmedItemInstance,Is.Zero);
            Assert.That(Use(spit).cooldownRemaining,Is.GreaterThan(0),runtime.Notice);
            Assert.That(runtime.View.haltReason,Is.Null.Or.Empty);
        }
    }
}
