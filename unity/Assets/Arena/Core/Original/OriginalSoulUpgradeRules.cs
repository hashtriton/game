using System;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalSoulUpgradeView
    {
        public string id, name;
        public int rank, maximumRank, soulCost;
        public bool unlocked, researching;
        public double remainingSeconds;
    }

    // 3.9c map SHA02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34.
    // research/lia/warcraft/3.9c/upgrades.json: UpgradeData.slk1035..1290.
    // v7v23038..23211 independently lists the six basic IDs and2..11 soul costs;
    // qP5999..6076 unlocks the six unique researches after60 completed basics.
    public static class OriginalSoulUpgradeRules
    {
        static readonly string[] basicIds = { "R002", "R003", "R001", "R000", "R004", "R006" };
        static readonly string[] uniqueIds = { "R00G", "R00F", "R00J", "R00H", "R00E", "R00I" };
        public static readonly System.Collections.Generic.IReadOnlyList<string> BasicIds = Array.AsReadOnly(basicIds);
        public static readonly System.Collections.Generic.IReadOnlyList<string> UniqueIds = Array.AsReadOnly(uniqueIds);
        public static bool IsBasic(string id) => Array.IndexOf(basicIds, id) >= 0;
        public static bool IsUnique(string id) => Array.IndexOf(uniqueIds, id) >= 0;
        public static int Maximum(string id) => IsBasic(id) ? 10 : IsUnique(id) ? 1 : 0;
        public static int Cost(string id, int rank)
        {
            if (rank < 0 || rank >= Maximum(id)) throw new ArgumentOutOfRangeException(nameof(rank));
            return IsBasic(id) ? 2 + rank : 90;
        }
        public static string Name(string id)
        {
            switch (id)
            {
                case "R002": return "Непробиваемая шкура";
                case "R003": return "Магическая аура";
                case "R001": return "Оружие";
                case "R000": return "Плотность";
                case "R004": return "Сила духа";
                case "R006": return "Проворность";
                case "R00G": return "Наследие Катаклизма";
                case "R00F": return "Поступь Титана";
                case "R00J": return "Клинок Расколотого Мира";
                case "R00H": return "Благословение Небожителя";
                case "R00E": return "Эманация Бездны";
                case "R00I": return "Призрачный Сдвиг";
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
        public static string Description(string id)
        {
            switch (id)
            {
                case "R002": return "+80 к максимальному здоровью за уровень";
                case "R003": return "+50 к максимальной мане за уровень";
                case "R001": return "+5 к атаке за уровень";
                case "R000": return "+1 к броне за уровень";
                case "R004": return "+0,15 здоровья и маны в секунду за уровень";
                case "R006": return "+5% к скорости атаки и +4 к скорости движения за уровень";
                case "R00G": return "+50 к основной характеристике";
                case "R00F": return "Каждые 2 секунды наносит урон от силы вокруг героя";
                case "R00J": return "Атаки могут выпускать три волны магического урона";
                case "R00H": return "Позволяет парить над землёй и даёт уклонение";
                case "R00E": return "Аура снижает базовую защиту ближайших врагов";
                case "R00I": return "15% шанс предварительно восстановить здоровье на величину входящего урона свыше 10";
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
    }
}
