using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalShopStockCodecTests
    {
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/lia39-"+name+".json")));
        static void Advance(OriginalSession session,double seconds)
        { while(seconds>1e-9){double step=Math.Min(.05,seconds);session.Advance(step);seconds-=step;}Assert.That(session.HaltReason,Is.Null); }
        static OriginalSession Create(bool compact)
        {
            var options=OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);options.compactShops=compact;
            var s=new OriginalSession(Load<OriginalMatchCatalog>("match"),Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"),Load<OriginalDuelCatalog>("duels"),new string('a',64),options,123);
            s.ConfigureItems(Load<OriginalNativeCatalog>("native126"),Load<OriginalObservedItemCatalog>("observed-items126"));
            foreach(var kind in new[]{OriginalSessionCommandKind.SelectHero,OriginalSessionCommandKind.LobbyReady,OriginalSessionCommandKind.Start})
                Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=s.Snapshot().players[0].acknowledgedSequence+1,kind=kind,heroId="H008",ready=true}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,2.01);return s;
        }
        static OriginalNetworkResponse Response(OriginalSession s)
        {
            var snapshot=s.Snapshot();return new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,
                code=OriginalSessionReplyCode.Accepted,assignedSlot=1,acknowledgedSequence=snapshot.players[0].acknowledgedSequence,snapshot=snapshot};
        }
        static void NextPreparation(OriginalSession s)
        {
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=s.Snapshot().players[0].acknowledgedSequence+1,kind=OriginalSessionCommandKind.WaveReady}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,2.01);
            foreach(var enemy in s.Snapshot().enemies)Assert.That(s.ReportEnemyKilled(enemy.entityId),Is.True);
            Advance(s,3.01);
        }
        [Test] public void FreshAcolytePendingStockRoundTripsBeforeReadinessInBothLayouts()
        {
            foreach(bool compact in new[]{false,true})
            {
                var s=Create(compact);var codec=new OriginalUnitySessionCodec();
                NextPreparation(s);Advance(s,1.1);
                var response=Response(s);var pending=response.snapshot.shops.Single(x=>x.unitId=="u00E");
                Assert.That(pending.stock.All(x=>!x.known&&x.available==0&&x.nextAvailableAt>response.snapshot.time),Is.True);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out var decoded),Is.True,compact?"compact":"standard");
                Assert.That(decoded.snapshot.shops.Single(x=>x.unitId=="u00E").stock[0].nextAvailableAt,Is.EqualTo(pending.stock[0].nextAvailableAt));
                Advance(s,2.3);response=Response(s);
                Assert.That(response.snapshot.shops.Single(x=>x.unitId=="u00E").stock.All(x=>x.known&&x.available>0),Is.True);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.True);
                NextPreparation(s);Advance(s,1.1);response=Response(s);
                Assert.That(response.snapshot.shops.Single(x=>x.unitId=="u00E").stock.All(x=>!x.known&&x.available==0),Is.True);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.True);
            }
        }
        [Test] public void PendingStockCannotPublishInventedCountOrInvalidDeadline()
        {
            var s=Create(false);NextPreparation(s);Advance(s,1.1);var codec=new OriginalUnitySessionCodec();
            var response=Response(s);var stock=response.snapshot.shops.Single(x=>x.unitId=="u00E").stock[0];
            stock.available=1;Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.False);
            stock.available=0;stock.nextAvailableAt=-1;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.False);
            response=Response(s);response.snapshot.shops=response.snapshot.shops.Concat(new[]{new OriginalShopView{instanceId=999,unitId="n004",stock=Array.Empty<OriginalShopStockView>()}}).ToArray();
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.False,"No additional shops beyond the selected layout.");
        }
    }
}
