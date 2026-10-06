using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    internal static class OriginalWorldOptionTestSupport
    {
        internal const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        sealed class Navigation:IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn=>new OriginalPoint(135,1000);
            public long NavigationRevision=>0;
            public bool IsWalkable(double x,double y,double radius)=>true;
            public bool SegmentClear(OriginalPoint a,OriginalPoint b,double radius)=>true;
            public OriginalPoint[] FindPath(OriginalPoint a,OriginalPoint b,double radius)=>new[]{b};
            public bool SetDoodadAlive(int id,bool alive)=>true;
        }
        internal static T Load<T>(string name)=>JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/lia39-"+name+".json")));
        internal static OriginalSession Create(OriginalMatchOptions options=null,int count=1)
        {
            options=options??OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
            if(count>3)options.heroSelection=OriginalHeroSelection.Duplicates;
            var native=Load<OriginalNativeCatalog>("native126");var observed=Load<OriginalObservedCatalog>("observed126");
            var s=new OriginalSession(Load<OriginalMatchCatalog>("match"),Load<OriginalItemCatalog>("items"),Load<OriginalCombatCatalog>("combat"),Load<OriginalDuelCatalog>("duels"),new string('a',64),options,123);
            s.ConfigureProgression(native,observed);s.ConfigureWorld(new Navigation(),native,Array.Empty<OriginalWorldDoodadView>(),observed);
            s.ConfigureItems(native,Load<OriginalObservedItemCatalog>("observed-items126"),Load<OriginalItemPassiveCatalog>("item-passives"));
            string[] heroes={"H008","N0A0","H024"};
            for(int i=0;i<count;i++)
            {
                long connection=i==0?0:100+i;long seq=1;
                if(i!=0)Assert.That(s.Apply(connection,new OriginalSessionCommand{kind=OriginalSessionCommandKind.Hello,sequence=seq++,protocol=OriginalSession.Protocol,contentHash=new string('a',64)}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(s.Apply(connection,new OriginalSessionCommand{kind=OriginalSessionCommandKind.SelectHero,sequence=seq++,heroId=heroes[i%3]}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(s.Apply(connection,new OriginalSessionCommand{kind=OriginalSessionCommandKind.LobbyReady,sequence=seq++,ready=true}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            }
            Assert.That(s.Apply(0,new OriginalSessionCommand{kind=OriginalSessionCommandKind.Start,sequence=3}),Is.EqualTo(OriginalSessionReplyCode.Accepted));return s;
        }
        internal static object Call(OriginalSession s,string name,params object[] args)
        {
            var method=typeof(OriginalSession).GetMethod(name,Hidden);
            Assert.That(method,Is.Not.Null,"World option consumer missing: "+name);return method.Invoke(s,args);
        }
        internal static OriginalWorld World(OriginalSession s)=>(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
        internal static object Player(OriginalSession s,int slot)=>( (IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s)).Cast<object>().Single(p=>(int)p.GetType().GetField("slot").GetValue(p)==slot);
        internal static OriginalInventory Inventory(OriginalSession s,int slot)=>(OriginalInventory)Player(s,slot).GetType().GetField("inventory").GetValue(Player(s,slot));
        internal static void AdvanceWorldOnly(OriginalSession s,double seconds,string method)
        {
            while(seconds>1e-9){double step=Math.Min(.05,seconds);World(s).Advance(step);Call(s,method);seconds-=step;}
        }
    }
}
