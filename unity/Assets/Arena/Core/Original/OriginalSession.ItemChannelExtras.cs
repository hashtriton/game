using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemProtectionField
        {
            internal int actor, owner, counter, ticks;
            internal bool totem, burning;
            internal double due;
            internal OriginalPoint point;
            internal readonly HashSet<int> recipients = new HashSet<int>();
        }
        sealed class ItemCrossbowMotion
        {
            internal int actor, owner, target, shots;
            internal double due, angle, traveled, maximum;
            internal OriginalPoint actorStart, targetStart;
            internal bool shooting;
        }
        sealed class ItemCrossbowBolt
        { internal int owner,target; internal double due,travel,damage; internal OriginalPoint start; }
        sealed class SpaceBootState
        { internal int charge=50; internal bool held; internal OriginalPoint previous; }
        sealed class SpaceBootDash
        { internal int actor,owner,left; internal double due,angle,travel,range,damage; internal OriginalPoint point; }
        readonly List<ItemProtectionField> itemProtectionFields = new List<ItemProtectionField>();
        readonly List<ItemCrossbowMotion> itemCrossbowMotions = new List<ItemCrossbowMotion>();
        readonly List<ItemCrossbowBolt> itemCrossbowBolts = new List<ItemCrossbowBolt>();
        readonly Dictionary<int,SpaceBootState> spaceBootStates = new Dictionary<int,SpaceBootState>();
        readonly List<SpaceBootDash> spaceBootDashes = new List<SpaceBootDash>();
        double nextSpaceBootTick=.2;

        bool PreflightItemChannelExtra(Player player,string ability)
        {
            if(ability=="A0JE")
            {
                if(combatCatalog.Ability("A0I4")?.Number("DataB1")!=.2)return false;
                ValidateAbilityChanges(new[]{"A0SC","A17O","A0I4"});
            }
            if(ability=="A0YK"&&combatCatalog.Ability("A0YN")?.Text("BuffID1")!="B083")return false;
            if((ability=="A0FI"||ability=="A0NA")&&combatCatalog.Ability("A0NC")?.Number("Dur1")!=2)return false;
            if(ability=="A0C5"&&combatCatalog.Ability("A19J")?.Text("BuffID1")!="B06H")return false;
            if(ability=="A0M9"&&combatCatalog.Ability("A0MB")?.Text("BuffID1")!="B05L")return false;
            if(ability=="A104")ScriptedAbilityAttack(player);
            if(ability=="A11S")HeroCombatStats(player.slot).primary.Require();
            if(ability=="A0KP"&&combatCatalog.Ability("A03W")?.Number("Dur1")!=3)return false;
            return true;
        }

        void BeginItemChannelExtra(int actorId,string ability,int targetId,OriginalPoint point)
        {
            if(BeginItemLegacyActive(actorId,ability,targetId))return;
            var actor=world.UnitState(actorId);
            if(ability=="A0C5")
            {
                var field=new ItemProtectionField{actor=actorId,owner=actor.ownerSlot,due=world.Clock+1};
                field.burning=Array.Exists(PlayerAt(actor.ownerSlot).inventory.HeroSlots,i=>i?.itemId=="I096");
                foreach(var recipient in CasterUnits())
                    if(recipient.health>.405&&!AreEnemies(actor.ownerSlot,recipient.ownerSlot)&&IsNativeHeroPredicate(recipient)&&
                        !CasterHasType(recipient,"structure")&&!HasEffectiveUnitAbility(recipient,"A0K4")&&
                        (recipient.entityId==actorId||SquaredDistance(point,recipient.position)<=300*300))
                    {field.recipients.Add(recipient.entityId);AddItemSourceShield(recipient.entityId,700,10,1,"A19J");}
                itemProtectionFields.Add(field);return;
            }
            if(ability=="A0M9")
            {itemProtectionFields.Add(new ItemProtectionField{actor=actorId,owner=actor.ownerSlot,point=point,totem=true,due=world.Clock+.2});return;}
            if(ability=="A0KP")
            {
                var center=world.UnitState(targetId);if(center==null)return;
                foreach(var target in CasterUnits())
                    if(target.health>.405&&AreEnemies(actor.ownerSlot,target.ownerSlot)&&!CasterMagicImmune(target)&&
                        !CasterHasType(target,"structure")&&SquaredDistance(center.position,target.position)<=400*400)
                    {
                        ApplyTriggeredHit(actorId,actor.ownerSlot,target,250,OriginalTriggeredDamageMode.SpellMagic);
                        var current=world.UnitState(target.entityId);
                        if(current!=null&&current.health>.405&&!current.invulnerable&&!CasterHasType(current,"mechanical"))
                            AddTimedNativeStun(current.entityId,"B02Q",0,3);
                    }
                bool enemy=AreEnemies(actor.ownerSlot,center.ownerSlot);
                BeginItemSourceChain(actorId,targetId,enemy?550:800,!enemy);return;
            }
            if(ability=="A104")
            {
                var target=world.UnitState(targetId);if(target==null||HasEffectiveUnitAbility(target,"B06X"))return;
                world.SetPathingEnabled(actorId,false);world.SetPathingEnabled(targetId,false);
                itemCrossbowMotions.Add(new ItemCrossbowMotion{actor=actorId,owner=actor.ownerSlot,target=targetId,
                    actorStart=actor.position,targetStart=target.position,angle=Math.Atan2(target.position.y-actor.position.y,target.position.x-actor.position.x),
                    maximum=SourceUnitUserData(targetId)==2?200:400,due=world.Clock+.02});return;
            }
            if(ability=="A11S")
            {
                SyncSpaceBoots();var state=spaceBootStates[actorId];var stats=HeroCombatStats(actor.ownerSlot);
                BeginItemWrath(actorId,stats.primaryAttribute=="STR"?"A0HZ":stats.primaryAttribute=="AGI"?"A0DV":"A0JJ",6);
                double range=Math.Min(state.charge,Math.Sqrt(SquaredDistance(actor.position,point)));
                spaceBootDashes.Add(new SpaceBootDash{actor=actorId,owner=actor.ownerSlot,due=world.Clock+.03,
                    angle=Math.Atan2(point.y-actor.position.y,point.x-actor.position.x),range=range,
                    left=(int)(state.charge-range),damage=HeroCombatStats(actor.ownerSlot).primary.Require()*1.75,point=actor.position});return;
            }
            throw new InvalidOperationException("Unknown item channel "+ability);
        }

        void ObserveItemChannelDamage(OriginalWorldUnitView actor,double damage)
        {
            // Cw heals synchronously before damage, while Rp heals on Xp.
            // B06H residency is represented by its source marker A19J.
            if(actor!=null&&damage>0&&HasEffectiveUnitAbility(actor,"A19J"))
                world.UpdateProfile(actor.entityId,actor.profile,Math.Min(actor.profile.maxHealth,actor.health+damage),actor.mana);
        }

        void AdvanceItemChannelExtras()
        {
            AdvanceItemLegacyActives();
            SyncSpaceBoots();
            foreach(var field in itemProtectionFields.ToArray())
                while(itemProtectionFields.Contains(field)&&field.due<=world.Clock+1e-9)
                {
                    field.due+=field.totem?.2:1;field.ticks++;
                    if(field.totem)
                    {
                        // P8 uses the stale counter read once per callback,
                        // including its global off-radius ap removals.
                        int before=field.counter;
                        foreach(var target in CasterUnits())
                            if(target.health>.405&&IsNativeHeroPredicate(target)&&!AreEnemies(field.owner,target.ownerSlot)&&
                                target.health/target.profile.maxHealth<=.5&&before<3&&
                                !HasEffectiveUnitAbility(target,"A0MB")&&SquaredDistance(field.point,target.position)<=600*600)
                            {AddItemSourceShield(target.entityId,target.profile.maxHealth,0,.6,"A0MB");field.counter=before+1;}
                        foreach(var target in world.Snapshot().units)
                            if(target.health<.405||SquaredDistance(field.point,target.position)>600*600)
                            {RemoveItemSourceShield(target.entityId);field.counter=before-1;}
                        // Original subtracts float.2 until ==0, which can miss
                        // zero. Bound the authored6s lifetime to30 callbacks.
                        if(field.ticks>=30)
                        {foreach(var target in world.Snapshot().units)RemoveItemSourceShield(target.entityId);itemProtectionFields.Remove(field);ObserveScriptedHelperDeath();}
                    }
                    else
                    {
                        if(field.burning)
                            foreach(int id in field.recipients)
                            {
                                var recipient=world.UnitState(id);
                                if(recipient==null||recipient.health<=.405||!HasEffectiveUnitAbility(recipient,"A19J"))continue;
                                foreach(var enemy in CasterUnits())
                                    if(enemy.health>.405&&AreEnemies(field.owner,enemy.ownerSlot)&&!CasterHasType(enemy,"structure")&&
                                        !HasEffectiveUnitAbility(enemy,"A0K4")&&SquaredDistance(recipient.position,enemy.position)<=250*250)
                                        ApplyTriggeredHit(field.actor,field.owner,enemy,50,OriginalTriggeredDamageMode.ChaosUniversal);
                            }
                        if(field.ticks>=10)
                        {foreach(int id in field.recipients)RemoveItemSourceShield(id);itemProtectionFields.Remove(field);}
                    }
                }
            foreach(var motion in itemCrossbowMotions.ToArray())
                while(itemCrossbowMotions.Contains(motion)&&motion.due<=world.Clock+1e-9)
                {
                    var actor=world.UnitState(motion.actor);var target=world.UnitState(motion.target);
                    if(actor==null||target==null)
                    {
                        if(actor!=null)world.SetPathingEnabled(actor.entityId,true);
                        if(target!=null)world.SetPathingEnabled(target.entityId,true);
                        itemCrossbowMotions.Remove(motion);break;
                    }
                    if(!motion.shooting)
                    {
                        motion.due+=.02;motion.traveled+=20;
                        var a=new OriginalPoint(motion.actorStart.x-motion.traveled*Math.Cos(motion.angle),motion.actorStart.y-motion.traveled*Math.Sin(motion.angle));
                        var b=new OriginalPoint(motion.targetStart.x+motion.traveled*Math.Cos(motion.angle),motion.targetStart.y+motion.traveled*Math.Sin(motion.angle));
                        if(OriginalShieldBashRules.AllowsForcedPoint(a))world.ForcePosition(actor.entityId,OriginalShieldBashRules.ClampPlayable(a));
                        if(OriginalShieldBashRules.AllowsForcedPoint(b))world.ForcePosition(target.entityId,b);
                        if(motion.traveled>=motion.maximum)
                        {
                            world.SetPathingEnabled(actor.entityId,true);world.SetPathingEnabled(target.entityId,true);
                            if(combatCatalog.Unit(actor.rawcode).Text("weapTp1")!="normal"){motion.shooting=true;motion.due=world.Clock+.4;}
                            else itemCrossbowMotions.Remove(motion);
                        }
                    }
                    else
                    {
                        motion.due+=.4;
                        if(motion.shots++>=4){itemCrossbowMotions.Remove(motion);break;}
                        itemCrossbowBolts.Add(new ItemCrossbowBolt{owner=motion.owner,target=motion.target,start=actor.position,
                            damage=ScriptedAbilityAttack(PlayerAt(motion.owner)),due=world.Clock+.03});
                    }
                }
            foreach(var bolt in itemCrossbowBolts.ToArray())
                while(itemCrossbowBolts.Contains(bolt)&&bolt.due<=world.Clock+1e-9)
                {
                    bolt.due+=.03;bolt.travel+=24;var target=world.UnitState(bolt.target);
                    if(target==null){itemCrossbowBolts.Remove(bolt);break;}
                    if(bolt.travel<Math.Sqrt(SquaredDistance(bolt.start,target.position)))continue;
                    ApplyNativeTriggeredHit(0,bolt.owner,target,bolt.damage,OriginalTriggeredDamageMode.SpellNormal);
                    ApplyTriggeredHit(bolt.owner,bolt.owner,target,60,OriginalTriggeredDamageMode.ChaosUniversal);
                    itemCrossbowBolts.Remove(bolt);ObserveScriptedHelperDeath();
                }
            foreach(var dash in spaceBootDashes.ToArray())
                while(spaceBootDashes.Contains(dash)&&dash.due<=world.Clock+1e-9)
                {
                    dash.due+=.03;dash.travel+=18;var actor=world.UnitState(dash.actor);
                    if(actor==null){spaceBootDashes.Remove(dash);break;}
                    if(dash.travel>dash.range||actor.health<.405)
                    {
                        world.ForcePosition(dash.actor,dash.point);
                        ObserveScriptedHelperDeath();
                        foreach(var target in CasterUnits())
                            if(target.health>.405&&AreEnemies(dash.owner,target.ownerSlot)&&!CasterMagicImmune(target)&&
                                !CasterHasType(target,"structure")&&SquaredDistance(dash.point,target.position)<=dash.range*dash.range*.25)
                                ApplyTriggeredHit(dash.actor,dash.owner,target,dash.damage,OriginalTriggeredDamageMode.SpellMagic);
                        var state=spaceBootStates[dash.actor];state.charge=Math.Max(50,dash.left);state.previous=dash.point;
                        PublishSpaceBoots(PlayerAt(dash.owner),state);spaceBootDashes.Remove(dash);
                    }
                    else
                    {
                        var point=new OriginalPoint(dash.point.x+18*Math.Cos(dash.angle),dash.point.y+18*Math.Sin(dash.angle));
                        if(OriginalShieldBashRules.AllowsForcedPoint(point))dash.point=point;
                    }
                }
        }

        void SyncSpaceBoots()
        {
            foreach(var player in players)
            {
                if(player.inventory==null)continue;
                bool held=Array.Exists(player.inventory.HeroSlots,i=>i?.itemId=="I05P");
                if(!spaceBootStates.TryGetValue(player.slot,out var state))spaceBootStates[player.slot]=state=new SpaceBootState();
                if(held&&!state.held){state.previous=world.UnitState(player.slot).position;PublishSpaceBoots(player,state);}
                state.held=held;
            }
            while(nextSpaceBootTick<=world.Clock+1e-9)
            {
                nextSpaceBootTick+=.2;
                foreach(var player in players)
                {
                    if(!spaceBootStates.TryGetValue(player.slot,out var state)||!state.held)continue;
                    var actor=world.UnitState(player.slot);if(actor==null)continue;
                    int amount=(int)(.15*Math.Sqrt(SquaredDistance(state.previous,actor.position)));
                    if(amount>0&&amount<180&&state.charge<1200){state.charge=Math.Min(600,state.charge+amount);PublishSpaceBoots(player,state);}
                    state.previous=actor.position;
                }
            }
        }
        void PublishSpaceBoots(Player player,SpaceBootState state)
        {
            for(int i=0;i<6;i++)
            {var item=player.inventory.HeroSlots[i];if(item?.itemId=="I05P")player.inventory.SetScriptCharges(OriginalInventoryBag.Hero,i,item.instanceId,state.charge);}
        }
    }
}
