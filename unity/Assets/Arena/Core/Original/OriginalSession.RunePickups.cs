using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class RuneVampireStatus{internal double remaining;}
        readonly Dictionary<int,RuneVampireStatus> runeVampires=new Dictionary<int,RuneVampireStatus>();
        static string RuneItemReward(int draw)
        {
            switch(draw){case 1:return "I066";case 2:return "I06P";case 3:return "I068";
                case 4:return "I06E";case 5:return "I06N";case 6:return "I06G";case 8:return "I0AK";default:return null;}
        }
        int RunePickupDraw(PickupTransaction transaction,int size)
        {
            uint value=transaction.randomAfter==0?unchecked((uint)seed)^0x5049434bu:transaction.randomAfter;
            if(value==0)value=0x6D2B79F5u;uint threshold=unchecked(0u-(uint)size)%(uint)size;
            do{value^=value<<13;value^=value>>17;value^=value<<5;}while(value<threshold);
            transaction.randomAfter=value;return 1+(int)(value%(uint)size);
        }
        bool PrepareAdditionalRunePickup(PickupTransaction transaction,Player player,OriginalWorldUnitView actor,OriginalInventory candidate,string id)
        {
            if(id!="rman"&&id!="rsps"&&id!="vamp"&&id!="I07G"&&id!="rspl"&&id!="rdis")return false;
            transaction.handled=true;
            if(actor.health<=.405){transaction.code=OriginalItemActionCode.NotAllowed;return true;}
            var change=new PickupPlayerChange{player=player,originalInventory=player.inventory,inventory=candidate,
                originalProgression=player.progression,stats=player.stats};
            try
            {
                if(id=="rman")
                {
                    var a=combatCatalog.Ability("APmr");
                    if(a.Text("code")!="AImr"||a.Number("DataA1")!=250||a.Number("Area1")!=2000)
                        throw new InvalidOperationException("mana-rune-declaration-changed");
                    foreach(var u in world.Snapshot().units)
                        if(ItemAuraRecipient(actor,u,a,1))transaction.resources.Add(new PreparedEquipmentProfile{entityId=u.entityId,
                            profile=u.profile.Copy(),health=u.health,mana=Math.Min(u.profile.maxMana,u.mana+250)});
                }
                else if(id=="rsps")
                {
                    var a=combatCatalog.Ability("ANse");
                    if(a.Text("code")!="ANse"||a.Number("Area1")!=1400||a.Text("BuffID1")!="BNss")
                        throw new InvalidOperationException("spell-shield-rune-declaration-changed");
                    // ITEMSTAT2 exact accepted unpaused H008 pickup: no BNss on
                    // H008/allyH024, AHtb80 passes. activationKnown=false is
                    // retained; no amount, duration or spell block is invented.
                }
                else if(id=="vamp"||id=="I07G")
                {
                    var a=combatCatalog.Ability("AIpv");
                    if(a.Text("code")!="AIpv"||a.Number("DataA1")!=50||a.Number("DataB1")!=1||
                        a.Number("Dur1")!=15||a.Number("HeroDur1")!=15||a.Text("BuffID1")!="BIpv")
                        throw new InvalidOperationException("vampire-rune-declaration-changed");
                    transaction.runeVampireActor=actor.entityId;
                }
                else if(id=="rspl")
                {
                    // Source bOv1..10 leaves7/9/10 unmapped. These successful
                    // pickups give no item, without a compensating reroll.
                    string reward=RuneItemReward(RunePickupDraw(transaction,10));
                    if(reward!=null)
                    {
                        var item=candidate.CreateInstance(reward,player.slot);var action=candidate.TryPickup(item);
                        if(!action.Applied){transaction.code=action.Code;return true;}
                        if(action.Code==OriginalItemActionCode.Grounded)transaction.ground.Add(new OriginalGroundItemView{item=CopyItem(action.Item),position=actor.position});
                        else{change.inventoryChanged=true;change.profile=PrepareEquipmentProfile(player,candidate,actor);}
                    }
                }
                else
                {
                    var a=combatCatalog.Ability("APdi");
                    if(a.Text("code")!="AIdi"||a.Number("Area1")!=500||a.Number("DataB1")!=250)
                        throw new InvalidOperationException("dispel-rune-declaration-changed");
                    foreach(var u in world.Snapshot().units)
                    {
                        if(u.health<=.405||u.hidden||SquaredDistance(actor.position,u.position)>500*500)continue;
                        if(!CasterMagicImmune(u)&&WeaponTargetTypeAllowed(a.Text("targs1"),combatCatalog.Unit(u.rawcode).Text("targType")))transaction.runeDispelTargets.Add(u.entityId);
                        // I6/R6 separately hL250 to source Player11 ordinary
                        // receivers, excluding boss userData1/2 and Amim.
                        if(u.ownerSlot==0&&SourceUnitUserData(u.entityId)!=1&&SourceUnitUserData(u.entityId)!=2&&!CasterMagicImmune(u))
                            transaction.runeDamageTargets.Add(u.entityId);
                    }
                }
                if(groundItems.Count+transaction.ground.Count>8192){transaction.code=OriginalItemActionCode.NoSpace;return true;}
                transaction.players.Add(change);
            }
            catch(InvalidOperationException){transaction.code=OriginalItemActionCode.UnresolvedRule;}
            return true;
        }
        void CommitRunePowerupPickup(PickupTransaction transaction)
        {
            if(transaction.runeVampireActor!=0)runeVampires[transaction.runeVampireActor]=new RuneVampireStatus{remaining=15};
            foreach(int id in transaction.runeDispelTargets)
            {
                var u=world.UnitState(id);if(u==null||u.health<=.405)continue;
                RemoveRuneDispellableBuffs(id);
                if(u.kind==OriginalWorldUnitKind.Hero)RemoveArcherBuffs(u.ownerSlot);
                itemHaste.Remove(id);runeVampires.Remove(id);RevealItemInvisibility(id);
                RefreshAbilityMovement(id);RescaleWeaponRate(id,world.Clock);
                int actor=transaction.players[0].player.slot;
                if(AreEnemies(actor,u.ownerSlot))
                {
                    // APDI3: hostile actualAsum n01R gets250; allied summon
                    // and hostileAmim receive no callback. Hostile ordinary
                    // bodies get a real0 event, which can satisfy aie watches.
                    // Illusion factories and other Asum aliases are transfers.
                    bool summoned=u.kind==OriginalWorldUnitKind.Summon||u.kind==OriginalWorldUnitKind.Illusion||HasEffectiveUnitAbility(u,"Asum");
                    ApplyNativeTriggeredHit(actor,actor,u,summoned?250:0,OriginalTriggeredDamageMode.SpellMagic);
                }
            }
            int picker=transaction.players[0].player.slot;
            foreach(int id in transaction.runeDamageTargets)
            {
                var u=world.UnitState(id);if(u!=null&&u.health>.405)
                    ApplyTriggeredHit(picker,picker,u,250,OriginalTriggeredDamageMode.SpellMagic);
            }
        }
        bool RunePhysicalControl(string token)
        {
            if(token=="curse-cold:A19U"||token=="native-doom:B0BN")return true;
            if(token.StartsWith("native-stun:BPSE:",StringComparison.Ordinal))return true;
            foreach(var state in nativeBashes)if(state.token==token&&state.buff=="BPSE")return true;
            if(token.StartsWith("native-bash:",StringComparison.Ordinal))
            {
                string id=token.Substring(token.LastIndexOf(':')+1);
                if(id.Length==4&&combatCatalog.Ability(id)?.Text("code")=="AHbh")return true;
            }
            const string prefix="native-root:";
            if(!token.StartsWith(prefix,StringComparison.Ordinal)||token.Length<prefix.Length+4)return false;
            return combatCatalog.Ability(token.Substring(prefix.Length,4))?.Text("code")=="Aens";
        }
        void RemoveRuneDispellableBuffs(int actor)
        {
            // APDI1: positive Bblo/Bspe/BIpv and negative Bslo disappear on
            // both teams; physical A19U/B0BL remains with speed0. Preserve
            // the actual timers of Aens aliases, not a fresh root duration.
            var physical=new Dictionary<string,ActorControl>();
            if(actorControls.TryGetValue(actor,out var controls))
                foreach(var pair in controls)if(RunePhysicalControl(pair.Key))physical.Add(pair.Key,pair.Value);
            // Blizzard spell-basics explicitly excludes poison (not Shadow
            // Strike) and Purge movement from dispel; PitLord says Doom cannot
            // be dispelled. Map poisons use Aven, ACpu uses Aprg, A0HR ANdo.
            // Preserve state objects, including their elapsed/tick phase.
            poisons.TryGetValue(actor,out var poison);bossDooms.TryGetValue(actor,out var doom);
            var purges=new Dictionary<string,OrdinaryNativeStatus>();
            if(ordinaryStatuses.TryGetValue(actor,out var statuses))
                foreach(var pair in statuses)if(combatCatalog.Ability(pair.Value.ability).Text("code")=="Aprg")purges.Add(pair.Key,pair.Value);
            // APDI4: actual A062/B00X and A0BJ/B00U weapon frost remain.
            var frost=new Dictionary<string,ArcherDebuff>();
            if(archerDebuffs.TryGetValue(actor,out var debuffs))
                foreach(var pair in debuffs)if(pair.Value.rules.abilityId=="A062"||pair.Value.rules.abilityId=="A0BJ")frost.Add(pair.Key,pair.Value);
            RemoveNegativeAbilityBuffs(actor);RemoveOrdinaryNativeBuffs(actor);
            if(frost.Count>0)
            {
                if(!archerDebuffs.TryGetValue(actor,out debuffs))archerDebuffs[actor]=debuffs=new Dictionary<string,ArcherDebuff>();
                foreach(var pair in frost)debuffs[pair.Key]=pair.Value;
            }
            if(poison!=null)poisons[actor]=poison;if(doom!=null)bossDooms[actor]=doom;
            if(purges.Count>0)
            {
                if(!ordinaryStatuses.TryGetValue(actor,out statuses))ordinaryStatuses[actor]=statuses=new Dictionary<string,OrdinaryNativeStatus>();
                foreach(var pair in purges)statuses[pair.Key]=pair.Value;
            }
            // Transfer APDI's observed both-team magic-buff removal to the
            // native Alsh/AIda carrier ledgers. U8 pulse timers, Knight source
            // effects, A0E5 resistance ability and A05M defend stance survive.
            itemLightningBuffs.RemoveAll(buff=>buff.actor==actor);
            foreach(var key in new List<(int actor,string buff)>(itemArmor.Keys))
                if(key.actor==actor)itemArmor.Remove(key);
            if(physical.Count==0)return;
            if(!actorControls.TryGetValue(actor,out controls))actorControls[actor]=controls=new Dictionary<string,ActorControl>(StringComparer.Ordinal);
            foreach(var pair in physical)controls[pair.Key]=pair.Value;
        }
        void AdvanceRunePowerups(double seconds)
        {
            foreach(var pair in new List<KeyValuePair<int,RuneVampireStatus>>(runeVampires))
            {
                var u=world.UnitState(pair.Key);
                if(u==null||u.health<=.405){runeVampires.Remove(pair.Key);continue;}
                if(u.paused)continue;pair.Value.remaining-=seconds;
                if(pair.Value.remaining<=1e-9)runeVampires.Remove(pair.Key);
            }
        }
        OriginalHeroStatsSnapshot ApplyRunePowerupStats(int slot,OriginalHeroStatsSnapshot value)
        {
            if(!runeVampires.ContainsKey(slot))return value;
            var result=value.Copy();result.itemAttackDamageBonus+=50;
            result.attackMinimum=Plus(result.attackMinimum,50);result.attackMaximum=Plus(result.attackMaximum,50);return result;
        }
        double RuneVampireFraction(int actor,OriginalWorldUnitView target)
        {
            if(!runeVampires.ContainsKey(actor))return 0;
            return WeaponTargetTypeAllowed(combatCatalog.Ability("AIpv").Text("targs1"),combatCatalog.Unit(target.rawcode).Text("targType"))?1:0;
        }
    }
}
