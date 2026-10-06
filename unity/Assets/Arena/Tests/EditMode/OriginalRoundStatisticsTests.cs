using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalRoundStatisticsTests
    {
        [Test] public void DamagePoolsClampOverkillAndChooseStrictSourceMaxima()
        {
            var type=typeof(OriginalSession).Assembly.GetType("Arena.Original.OriginalRoundStatistics");
            Assert.That(type,Is.Not.Null,"Acolyte per-wave damage and spell pools are missing.");
            var ledger=Activator.CreateInstance(type,new object[]{3});
            void Record(string method,params object[] args)=>type.GetMethod(method).Invoke(ledger,args);
            Record("RecordTaken",2,100d,10d);Record("RecordTaken",3,10d,20d);
            Record("RecordDealt",3,200d,50d);Record("RecordDealt",2,20d,50d);
            Record("RecordCast",3);Record("RecordCast",3);Record("RecordCast",2);
            Assert.That((int[])type.GetMethod("Leaders").Invoke(ledger,null),Is.EqualTo(new[]{2,3,3}));
            type.GetMethod("Reset").Invoke(ledger,null);
            Assert.That((int[])type.GetMethod("Leaders").Invoke(ledger,null),Is.EqualTo(new[]{1,1,1}),"Zero pools still select match-slot1 in xU.");
        }
    }
}
