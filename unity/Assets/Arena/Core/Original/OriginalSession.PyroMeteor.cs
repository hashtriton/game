using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class PyroMeteor
        { internal int actor, owner; internal OriginalPoint target; internal double direction, impactAt; }
        sealed class PyroMeteorTrailEffect
        { internal int actor, owner, rank; internal string ability; internal double next; internal OriginalPyroMeteorTrail rule; }
        readonly List<PyroMeteor> pyroMeteors = new List<PyroMeteor>();
        readonly List<PyroMeteorTrailEffect> pyroMeteorTrails = new List<PyroMeteorTrailEffect>();
        sealed class PyroFlameField
        {
            internal int actor, owner, rank;
            internal string ability;
            internal OriginalPoint position;
            internal double created, expires, next, damage, radius;
            internal readonly HashSet<int> excluded = new HashSet<int>();
        }
        readonly List<PyroFlameField> pyroFlameFields = new List<PyroFlameField>();
        readonly Dictionary<int, PyroFlameField> pyroFlameOwners = new Dictionary<int, PyroFlameField>();

        bool PyroFlameStrikeAvailable(string ability, int rank)
        {
            if ((ability != "A0SQ" && ability != "A0ST") || rank < 1 || rank > 3) return false;
            var d=combatCatalog.Ability(ability);
            return d!=null && d.overrides.Length==0 && d.Text("code")=="AHfs" && d.Text("BuffID"+rank)=="B06U" &&
                d.Text("targs"+rank)=="air,enemies,ground,neutral,debris" && d.Number("Area"+rank)==200 &&
                d.Number("DataA"+rank)==new[]{20.0,60,100}[rank-1] && d.Number("DataB"+rank)==1 &&
                d.Number("DataE"+rank)==1 && d.Number("DataF"+rank)==100500 &&
                d.Number("Dur"+rank)==(ability=="A0SQ"?6:9) && d.Number("HeroDur"+rank)==d.Number("Dur"+rank);
        }

        void BeginPyroMeteor(int actorId, OriginalPoint target)
        {
            OriginalPyroEffectRules.Point(target);
            var actor = world.UnitState(actorId);
            if (actor == null) throw new InvalidOperationException("Meteor caster unavailable.");
            var player = players.Find(p => p.slot == actor.ownerSlot);
            PyroMeteorRank(player, out string ability, out int rank);
            if (!TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic) || !PyroFlameStrikeAvailable(ability, rank))
                throw new InvalidOperationException("pyro-meteor-native-dependency-unavailable");
            pyroMeteors.Add(new PyroMeteor { actor = actorId, owner = actor.ownerSlot, target = target,
                direction = Math.Atan2(target.y - actor.position.y, target.x - actor.position.x),
                impactAt = world.Clock + OriginalPyroEffectRules.MeteorDelay });
        }
        static void PyroMeteorRank(Player player, out string helper, out int rank)
        {
            int ordinary = SkillRank(player, "A0SM"), upgraded = AuxiliaryRank(player, "A0SS");
            helper = ordinary > upgraded ? "A0SQ" : "A0ST";
            rank = ordinary > upgraded ? ordinary : upgraded;
            OriginalHeroRules.SkillLevel(rank);
        }
        void AdvancePyroMeteors()
        {
            foreach (var meteor in new List<PyroMeteor>(pyroMeteors))
            {
                if (world.Clock + 1e-9 < meteor.impactAt) continue;
                var player = players.Find(p => p.slot == meteor.owner);
                // tev reads current learned/upgraded ranks at impact, not at
                // the original order, and captures the trail helper rank then.
                PyroMeteorRank(player, out string helper, out int rank);
                if (!PyroFlameStrikeAvailable(helper, rank)) throw new InvalidOperationException("pyro-meteor-native-dependency-unavailable");
                double damage = 100 * rank;
                foreach (var target in world.Snapshot().units)
                    if (target.health > .405 && AreEnemies(meteor.owner, target.ownerSlot) && SquaredDistance(meteor.target, target.position) <= 200 * 200)
                    {
                        // Unlike vacuum/spheres, tev does not prefilter native
                        // magic immunity. The exact hL mode2 resolver owns it.
                        if (PyroHasChainBuff(target.entityId)) damage *= OriginalHeroRules.PyroChainAmplifier(SkillRank(player, "A0AE"));
                        ApplyTriggeredHit(meteor.actor, meteor.owner, target, damage, OriginalTriggeredDamageMode.SpellMagic);
                    }
                pyroMeteorTrails.Add(new PyroMeteorTrailEffect { actor = meteor.actor, owner = meteor.owner,
                    ability = helper, rank = rank, next = meteor.impactAt + OriginalPyroEffectRules.MeteorTrailTick,
                    rule = new OriginalPyroMeteorTrail(meteor.target, meteor.direction) });
                pyroMeteors.Remove(meteor);
            }
            foreach (var trail in new List<PyroMeteorTrailEffect>(pyroMeteorTrails))
            {
                while (!trail.rule.Completed && trail.next <= world.Clock + 1e-9)
                {
                    double at = trail.next; trail.next += OriginalPyroEffectRules.MeteorTrailTick;
                    if (trail.rule.Tick(out var point)) IssuePyroFlameStrike(trail.actor, trail.owner, trail.ability, trail.rank, point, at);
                }
                if (trail.rule.Completed) pyroMeteorTrails.Remove(trail);
            }
            AdvancePyroFlameFields();
        }
        void IssuePyroFlameStrike(int actor, int owner, string ability, int rank, OriginalPoint point, double at)
        {
            OriginalPyroEffectRules.Point(point);
            if(!OriginalCombatDefinition.IsFinite(at) || !PyroFlameStrikeAvailable(ability,rank))
                throw new InvalidOperationException("pyro-AHfs-native-rule-unavailable");
            var definition=combatCatalog.Ability(ability);
            var field=new PyroFlameField{actor=actor,owner=owner,ability=ability,rank=rank,position=point,
                created=at,expires=at+definition.Number("Dur"+rank),next=at+1,damage=definition.Number("DataA"+rank),radius=200};
            pyroFlameFields.Add(field); PulsePyroFlame(field,at);
        }
        void AdvancePyroFlameFields()
        {
            foreach(var field in new List<PyroFlameField>(pyroFlameFields))
            {
                while(field.next<=world.Clock+1e-9 && field.next<field.expires-1e-9)
                {double at=field.next;field.next+=1;PulsePyroFlame(field,at);}
                if(world.Clock+1e-9>=field.expires) pyroFlameFields.Remove(field);
            }
            foreach(var id in new List<int>(pyroFlameOwners.Keys))
                if(pyroFlameOwners[id].expires<=world.Clock+1e-9 || world.UnitState(id)==null)pyroFlameOwners.Remove(id);
        }
        void PulsePyroFlame(PyroFlameField field,double at)
        {
            // PYFIELD4/1.26: global field pulses, same-point equal recast does
            // not extend a target's series; a stronger replacement skips the
            // immediate pulse. Per-target priority at partial overlaps and
            // rank2 interpolation are explicit host generalizations. Radius
            //200 + target collision matches real210 hit/240 miss controls.
            foreach(var snapshot in world.Snapshot().units)
            {
                var target=world.UnitState(snapshot.entityId);
                if(target==null || target.health<=.405 || target.hidden || target.invulnerable ||
                    !AreEnemies(field.owner,target.ownerSlot) || field.excluded.Contains(target.entityId))continue;
                double radius=field.radius+target.profile.collisionRadius;
                if(SquaredDistance(field.position,target.position)>radius*radius)continue;
                if(pyroFlameOwners.TryGetValue(target.entityId,out var previous) && previous!=field && previous.expires>at+1e-9)
                {
                    if(previous.damage>=field.damage){field.excluded.Add(target.entityId);continue;}
                    previous.excluded.Add(target.entityId);pyroFlameOwners[target.entityId]=field;
                    continue;
                }
                pyroFlameOwners[target.entityId]=field;
                // Original tev's h011 is the damage source, not its hero.
                ApplyTriggeredHit(0,field.owner,target,field.damage,OriginalTriggeredDamageMode.SpellMagic);
            }
        }
    }
}
