using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossImageCast
        {
            internal int actor, owner;
            internal double effectAt;
            internal bool effect, born;
            internal OriginalBossImageRules rules;
        }
        readonly Dictionary<int,BossImageCast> bossImageCasts=new Dictionary<int,BossImageCast>();
        readonly Dictionary<int,double> bossImageCooldowns=new Dictionary<int,double>();
        readonly HashSet<int> bossImageInsideArena=new HashSet<int>();
        readonly HashSet<int> bossImageDamageWatches=new HashSet<int>();
        bool HasBossImageDamageWatch(int id)=>bossImageDamageWatches.Contains(id);
        bool BossImageControlsActor(int id)=>bossImageCasts.TryGetValue(id,out var cast)&&!cast.born;
        void CancelBossImageCastOnOrder(int id)
        { if(bossImageCasts.TryGetValue(id,out var cast)&&!cast.effect)bossImageCasts.Remove(id); }

        bool TryStartBossMirrorCast(int actorId)
        {
            var actor=world.UnitState(actorId);
            if(actor==null || actor.rawcode!="n017" || actor.kind!=OriginalWorldUnitKind.Enemy || actor.ownerSlot!=0 ||
                actor.health<=.405 || actor.hidden || actor.paused || ActorCastBlocked(actorId) || bossImageCasts.ContainsKey(actorId) ||
                bossImageCooldowns.TryGetValue(actorId,out var until)&&until>world.Clock+1e-9)return false;
            int living=0;
            foreach(var unit in world.Snapshot().units)
                if(unit.kind==OriginalWorldUnitKind.Illusion&&unit.rawcode=="n017"&&unit.health>.405&&
                    !AreEnemies(actor.ownerSlot,unit.ownerSlot)&&SquaredDistance(actor.position,unit.position)<=1000*1000)living++;
            if(living>=2)return false;
            var rules=new OriginalBossImageRules(combatCatalog);
            if(actor.mana<rules.cost || nextIllusionId>int.MaxValue-rules.count || !BossImagePlacement(actor,rules,out _))return false;
            CancelBossCastOnOrder(actorId);
            world.Stop(actorId); world.MarkCast(actorId);
            if(weaponCycles.TryGetValue(actorId,out var cycle))cycle.winding=false;
            bossImageCasts.Add(actorId,new BossImageCast{actor=actorId,owner=actor.ownerSlot,effectAt=world.Clock+rules.castPoint,rules=rules});
            return true;
        }

        void AdvanceBossImages()
        {
            foreach(var cast in new List<BossImageCast>(bossImageCasts.Values))
            {
                var actor=world.UnitState(cast.actor);
                if(!cast.effect)
                {
                    if(actor==null||actor.health<=.405||actor.hidden||actor.paused||ActorCastBlocked(cast.actor))
                    {bossImageCasts.Remove(cast.actor);continue;}
                    if(cast.effectAt>world.Clock+1e-9)continue;
                    if(!world.TrySpendMana(cast.actor,cast.rules.cost)){bossImageCasts.Remove(cast.actor);continue;}
                    cast.effect=true; bossImageCooldowns[cast.actor]=cast.effectAt+cast.rules.cooldown;
                    RemoveNegativeAbilityBuffs(cast.actor);
                    foreach(var old in new List<MirrorImage>(mirrorImages.Values))
                        if(old.source==cast.actor&&old.removeAt<0&&world.UnitState(old.actor)?.imageFactory==OriginalImageFactory.BossMirror)
                            EndImage(old,OriginalWorldRemovalReason.Replaced);
                    world.SetVisibility(cast.actor,false);
                }
                if(!cast.born&&world.Clock+1e-9>=cast.effectAt+cast.rules.creationDelay)
                {
                    actor=world.UnitState(cast.actor);
                    if(actor!=null&&actor.health>.405)
                    {
                        if(!BossImagePlacement(actor,cast.rules,out var points))throw new InvalidOperationException("boss-image-placement-unavailable");
                        var profile=actor.profile.Copy(); profile.moveSpeed=combatCatalog.Unit("n017").Number("spd");
                        var rows=new OriginalIllusionSpawn[cast.rules.count];
                        for(int i=0;i<rows.Length;i++)rows[i]=new OriginalIllusionSpawn{entityId=nextIllusionId+i,profile=profile.Copy(),
                            health=actor.health,mana=actor.mana,position=points[i+1]};
                        if(!world.TryPublishImages(cast.actor,cast.owner,OriginalImageFactory.BossMirror,rows))
                            throw new InvalidOperationException("boss-image-publication-unavailable");
                        foreach(var row in rows)
                        {
                            illusionCombatStats.Add(row.entityId,new ActorCombatStats(combatCatalog.Unit("n017").Number("def"),0));
                            mirrorImages.Add(row.entityId,new MirrorImage{actor=row.entityId,source=cast.actor,outgoing=1,incoming=1,
                                expires=world.Clock+cast.rules.lifetime,lastAdvanced=world.Clock});
                        }
                        nextIllusionId+=rows.Length;
                        world.ForcePosition(cast.actor,points[0]);
                        if(!world.SetVisibility(cast.actor,true))throw new InvalidOperationException("boss-image-reveal-placement-unavailable");
                    }
                    else if(actor!=null)world.SetVisibility(cast.actor,true);
                    cast.born=true;
                }
                // avv's timer survives caster death. Its location is DV's
                // fixed center (0,-2688), never the cast point or new position.
                if(world.Clock+1e-9>=cast.effectAt+.7)
                {
                    foreach(var target in world.Snapshot().units)
                        if(target.health>.405&&AreEnemies(cast.owner,target.ownerSlot)&&SquaredDistance(target.position,new OriginalPoint(0,-2688))<=350*350)
                            world.UpdateProfile(target.entityId,target.profile,target.health,Math.Max(0,target.mana-target.profile.maxMana*.12));
                    bossImageCasts.Remove(cast.actor);
                }
            }
            foreach(var image in world.Snapshot().units)
            {
                if(image.imageFactory!=OriginalImageFactory.BossMirror||image.health<=.405)continue;
                bool inside=image.position.x>=-576&&image.position.x<=576&&image.position.y>=-3328&&image.position.y<=-2112;
                if(!inside){bossImageInsideArena.Remove(image.entityId);continue;}
                if(!bossImageInsideArena.Add(image.entityId))continue;
                // XGv32340: i0(false) resets UL/uL bit abilities, not adds a
                // second copy of inherited bonuses. HP remains the copied max.
                var scaling=OriginalBossRules.Scaling("n017",match.Participants,Math.Max(1,match.Round),false,image.profile.maxHealth);
                illusionCombatStats[image.entityId]=new ActorCombatStats(combatCatalog.Unit("n017").Number("def")+scaling.armor,scaling.attack);
                var donor=world.UnitState(image.copySourceEntityId);
                if(donor!=null)world.UpdateProfile(image.entityId,image.profile,image.health,Math.Min(image.profile.maxMana,donor.mana));
                bossImageDamageWatches.Add(image.entityId);
            }
        }

        bool BossImagePlacement(OriginalWorldUnitView actor,OriginalBossImageRules rules,out OriginalPoint[] points)
        {
            // Source AOmi private placement is replaced with bounded reserved
            // rings. Every body is checked before image publication.
            points=new OriginalPoint[rules.count+1]; int count=0;
            for(int ring=1;ring<=4&&count<points.Length;ring++)
                for(int i=0;i<32&&count<points.Length;i++)
                {
                    double angle=i*Math.PI/16;
                    var point=new OriginalPoint(actor.position.x+rules.radius*ring*Math.Cos(angle),actor.position.y+rules.radius*ring*Math.Sin(angle));
                    if(!world.CanPlace(point,actor.profile.collisionRadius,actor.entityId))continue;
                    bool overlap=false;
                    for(int j=0;j<count;j++)if(SquaredDistance(point,points[j])<4*actor.profile.collisionRadius*actor.profile.collisionRadius)overlap=true;
                    if(!overlap)points[count++]=point;
                }
            return count==points.Length;
        }
    }
}
