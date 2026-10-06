using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemRootFlight { internal int target,actor; internal double due,duration; }
        sealed class ItemRootChain
        {
            internal int actor,owner,previous,jumps;internal double elapsed,next=.3;internal bool complete;
            internal readonly HashSet<int> visited=new HashSet<int>();
            internal readonly List<ItemRootFlight> flights=new List<ItemRootFlight>();
            internal readonly List<double> helperDeaths=new List<double>();
        }
        readonly List<ItemRootChain> itemRootChains=new List<ItemRootChain>();

        NativeItemActionRule ChainRootItemRule()
        {
            var a=combatCatalog.Ability("A0TM");var h=combatCatalog.Ability("A0TL");
            if(a.Text("code")!="Aens"||a.Number("Cost1")!=400||a.Number("Cool1")!=20||a.Number("Rng1")!=700||
                a.Number("Dur1")!=.01||h.Text("code")!="Aens"||h.Number("Dur1")!=5||h.Number("HeroDur1")!=5||h.Text("BuffID1")!="B06Z,B06Z")
                throw new InvalidOperationException("Chain ensnare declaration changed.");
            return new NativeItemActionRule{abilityId=a.id,cooldownGroup=itemCatalog.Item("I09X").cooldownId,
                requiresCharge=false,targetMode=OriginalAbilityTargetMode.Unit,range=700,manaCost=400,cooldown=20,sourceEffect="chain-root"};
        }
        OriginalSessionReplyCode ValidateItemChainRoot(Player player,OriginalSessionCommand command,bool enforceRange=true)
        {
            var actor=world.UnitState(OriginalWorld.HeroEntityId(player.slot));var target=world.UnitState(command.targetId);
            if(command.targetKind!=OriginalWorldTargetKind.Unit||actor==null||target==null||target.health<=.405||target.invulnerable||
                !AreEnemies(actor.ownerSlot,target.ownerSlot)||!CanSeeForCombat(actor.ownerSlot,target)||
                !WeaponTargetTypeAllowed(combatCatalog.Ability("A0TM").Text("targs1"),combatCatalog.Unit(target.rawcode).Text("targType"))||
                enforceRange&&SquaredDistance(actor.position,target.position)>700*700)return OriginalSessionReplyCode.InvalidCommand;
            return OriginalSessionReplyCode.Accepted;
        }
        void BeginItemEnsnare(OriginalWorldUnitView actor,int targetId)
        {
            var target=world.UnitState(targetId);if(target==null)return;
            var chain=new ItemRootChain{actor=actor.entityId,owner=actor.ownerSlot,previous=targetId};chain.visited.Add(targetId);
            // Native Aens missile speed1500 and two zero callbacks transfer
            // from WTRAIT1. Exact A0TM castpoint and moving-target flight are
            // host policies. Both the .01 primary and five-second helper land.
            double due=Math.Max(.005,Math.Sqrt(SquaredDistance(actor.position,target.position))/1500);
            chain.flights.Add(new ItemRootFlight{actor=actor.entityId,target=targetId,due=due,duration=.01});
            chain.flights.Add(new ItemRootFlight{target=targetId,due=due,duration=5});
            chain.helperDeaths.Add(1.5);itemRootChains.Add(chain);
        }
        void AdvanceItemEnsnare(double seconds)
        {
            foreach(var chain in itemRootChains.ToArray())
            {
                chain.elapsed+=seconds;
                while(true)
                {
                    ItemRootFlight first=null;
                    foreach(var pending in chain.flights)if(first==null||pending.due<first.due)first=pending;
                    double nextJump=chain.complete?double.PositiveInfinity:chain.next;
                    double death=chain.helperDeaths.Count==0?double.PositiveInfinity:chain.helperDeaths[0];
                    // IT creates an h011 at each launch, with independent1.5s
                    // timed life. Its global CA death survives caster death and
                    // chain termination, regardless of flight/root duration.
                    if(death<=chain.elapsed+1e-9&&death<=nextJump&&(first==null||death<=first.due))
                    {chain.helperDeaths.RemoveAt(0);ObserveScriptedHelperDeath();continue;}
                    if(first!=null&&first.due<=chain.elapsed+1e-9&&first.due<=nextJump)
                    {chain.flights.Remove(first);LandItemEnsnare(chain,first);continue;}
                    if(nextJump>chain.elapsed+1e-9)break;
                    var previous=world.UnitState(chain.previous);OriginalWorldUnitView selected=null;double nearest=500*500;
                    if(previous!=null&&chain.jumps<8)
                        foreach(var candidate in world.Snapshot().units)
                            if(candidate.health>.4&&!chain.visited.Contains(candidate.entityId)&&AreEnemies(chain.owner,candidate.ownerSlot)&&
                                CanSeeForCombat(chain.owner,candidate)&&!CasterHasType(candidate,"structure")&&!CasterHasType(candidate,"mechanical"))
                            {
                                double distance=SquaredDistance(previous.position,candidate.position);
                                if(distance<nearest){nearest=distance;selected=candidate;}
                            }
                    if(selected==null){chain.complete=true;continue;}
                    chain.visited.Add(selected.entityId);chain.previous=selected.entityId;chain.jumps++;
                    chain.flights.Add(new ItemRootFlight{target=selected.entityId,due=chain.next+Math.Max(.005,Math.Sqrt(nearest)/1500),duration=5});
                    chain.helperDeaths.Add(chain.next+1.5);
                    chain.next+=.3;
                }
                // NT survives caster death. Removed previous handles terminate
                // safely; no fabricated origin is substituted for native null.
                if(chain.complete&&chain.flights.Count==0&&chain.helperDeaths.Count==0)itemRootChains.Remove(chain);
            }
        }
        void LandItemEnsnare(ItemRootChain chain,ItemRootFlight flight)
        {
            var target=world.UnitState(flight.target);
            if(target==null||target.health<=.405||target.invulnerable)return;
            ApplyResolvedUnitHit(flight.actor,chain.owner,target,0);target=world.UnitState(flight.target);
            if(target==null||target.health<=.405||target.invulnerable)return;
            if(world.Stop(target.entityId))OnAcceptedWorldOrder(target.entityId);
            SetActorControl(target.entityId,"native-root:A0TL:B06Z",OriginalActorControlMask.Move,flight.duration,true,true);
            ApplyResolvedUnitHit(flight.actor,chain.owner,world.UnitState(flight.target),0);
        }
    }
}
