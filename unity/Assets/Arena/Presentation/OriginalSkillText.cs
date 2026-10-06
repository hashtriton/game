using System;
using System.Globalization;
using System.Text;
using Arena.Original;

namespace Arena
{
    // Presentation only. Numeric native fields come from the same declarations
    // and measured progression used by the session; source formulas reuse its
    // public pure rules. This class never learns, casts or mutates a session.
    public static class OriginalSkillText
    {
        public static string Describe(OriginalGameCatalogs catalogs, string heroId, string skillId, int rank)
        {
            if (catalogs == null) throw new ArgumentNullException(nameof(catalogs));
            return Describe(catalogs.Combat, catalogs.Native, catalogs.Observed, heroId, skillId, rank);
        }

        public static string Describe(OriginalCombatCatalog combat, OriginalNativeCatalog native,
            OriginalObservedCatalog observed, string heroId, string skillId, int rank)
        {
            if (combat == null || native == null || observed == null) throw new ArgumentNullException();
            string canonical = skillId == "A0SN" ? "A0SJ" : skillId == "A0SO" || skillId == "A0SR" ? "A0SP" : skillId == "A0SS" ? "A0SM" : skillId;
            var hero = Array.Find(combat.selectedHeroes, value => value.id == heroId);
            if (hero == null || Array.IndexOf(hero.skills, canonical) < 0) return "";
            int maximum = canonical == "A001" ? 15 : 3;
            if (rank < 0 || rank > maximum) throw new ArgumentOutOfRangeException(nameof(rank));
            int level = Math.Max(1, rank);
            var text = new StringBuilder(Name(skillId));
            text.Append(rank == 0 ? "\nНе изучено. Предварительный просмотр уровня 1." : "\nУровень " + rank + " / " + maximum);
            var learning = observed.Skill(heroId, canonical);
            text.Append("\nТребуется уровень героя: ").Append(learning.minimumHeroLevels[level - 1]);
            if (rank > 0 && rank < maximum) text.Append(". Следующий уровень навыка: с уровня героя ").Append(learning.minimumHeroLevels[rank]);
            text.Append(".\n\n");
            bool attackFormula = false;
            switch (skillId)
            {
                case "A001":
                    var stats = Array.Find(learning.rankEffects, value => value.known && value.rank == level);
                    if (stats == null) throw new InvalidOperationException("Missing measured attribute skill rank.");
                    text.Append("Пассивно. Постоянно даёт +").Append(N(stats.strengthBonus)).Append(" к силе, +")
                        .Append(N(stats.agilityBonus)).Append(" к ловкости и +").Append(N(stats.intelligenceBonus)).Append(" к интеллекту.")
                        .Append("\nЗа счёт этих характеристик: +").Append(N(stats.maxHPBonus)).Append(" к запасу здоровья и +")
                        .Append(N(stats.maxMPBonus)).Append(" к запасу маны. Указаны суммарные бонусы этого уровня.");
                    break;
                case "A05N":
                    var mirror = new OriginalMirrorImageRules(combat, level);
                    text.Append("Без выбора цели. Снимает отрицательные эффекты, ненадолго скрывает героя и создаёт ")
                        .Append(mirror.count).Append(" копии на ").Append(N(mirror.lifetime)).Append(" с. Копии наносят ")
                        .Append(P(mirror.outgoing)).Append("% своего урона и получают ").Append(P(mirror.incoming)).Append("% входящего урона.")
                        .Append("\nВокруг места применения, в радиусе 350, наносит ")
                        .Append(Formula(OriginalHeroRules.KnightMirrorDamage(level, 0), .2 + .1 * level)).Append(" урона с учётом брони.")
                        .Append("\nПовторное применение заменяет прежние копии. Копии сохраняют характеристики при создании; прямой бонус урона от предметов не копируется.");
                    attackFormula = true; Resources(text, mirror.manaCost, mirror.cooldown); break;
                case "A05M":
                    var shield = new OriginalDefendRules(combat, level);
                    text.Append("Переключение без выбора цели. Пока щит поднят, входящий колющий урон снижен на ")
                        .Append(P(1 - shield.pierceMultiplier)).Append("%, магический урон - на ").Append(P(1 - shield.magicMultiplier))
                        .Append("%, скорость движения - на ").Append(P(1 - shield.movementMultiplier)).Append("%.")
                        .Append("\nОбычный, геройский, осадный и хаотический типы атаки щит не ослабляет. Повторное применение опускает щит.");
                    Resources(text, 0, 0); break;
                case "A102":
                    var cripple = new OriginalCrippleRules(combat);
                    text.Append("Без выбора цели. Бьёт перед героем в секторе 80° на расстоянии до ")
                        .Append(N(OriginalHeroRules.KnightShieldRadius(level))).Append(": ")
                        .Append(N(OriginalHeroRules.KnightShieldDamage(level))).Append(" урона с учётом брони и отталкивание.")
                        .Append("\nНа ").Append(N(cripple.duration)).Append(" с уменьшает базовую часть урона атак цели на ")
                        .Append(P(cripple.damageReduction)).Append("%; прямые добавки к урону сохраняются.")
                        .Append("\nПод «Тёмными дарами» удар наносит чистый урон, а отталкивание сильнее в 1,5 раза.");
                    Resources(text, combat, skillId, level); break;
                case "A0E6":
                    var gift = combat.Ability("A0UU");
                    text.Append("Без выбора цели. На ").Append(N(OriginalHeroRules.KnightDarkGiftDuration(level)))
                        .Append(" с усиливает героя и его копии: +").Append(N(-gift.Number("DataC" + level)))
                        .Append(" брони, +").Append(P(gift.Number("DataB" + level))).Append("% скорости атаки, +")
                        .Append(P(gift.Number("DataA" + level))).Append("% скорости движения и ")
                        .Append(P(combat.Ability("A0E5").Number("DataB1"))).Append("% сопротивления магии.")
                        .Append("\nУсиливает «Удар щитом». Вновь созданные копии тоже получают дары. Сопротивление от предметов может заменить эту защиту, а не сложиться с ней.");
                    Resources(text, combat, skillId, level); break;
                case "A15W":
                    var shot = new OriginalArcherCastRules(combat, skillId, level);
                    text.Append("Выберите точку, дальность ").Append(N(shot.range)).Append(". Героиня отступает назад и выпускает стрелу, поражающую живых врагов вдоль пути: ")
                        .Append(Formula(OriginalHeroRules.ArcherPowerShotDamage(level, 0), .4 + .1 * level))
                        .Append(" урона с учётом брони каждой цели. Здания и механические цели не поражаются.")
                        .Append("\n«Заколдованный лук» открывает сочетания: после «Мастерства стрельбы» - три стрелы; после «Лука стихий» - перенос стихии. Требования указаны в пассивном навыке.");
                    attackFormula = true; Resources(text, shot.manaCost, shot.cooldown); break;
                case "A0AS":
                    var volley = new OriginalArcherCastRules(combat, skillId, level);
                    text.Append("Без выбора цели. Даёт +").Append(P(volley.attackSpeedBonus)).Append("% скорости атаки на ")
                        .Append(N(volley.duration)).Append(" с и выпускает стрелы максимум в 6 видимых врагов в радиусе ")
                        .Append(N(OriginalArcherVolleyRules.Range)).Append(". Каждая стрела наносит ")
                        .Append(P(OriginalHeroRules.ArcherVolleyDamage(1))).Append("% расчётной атаки урона с учётом брони.")
                        .Append("\nНа 3-м уровне «Заколдованного лука» залп после «Лука стихий» переносит активную стихию.");
                    attackFormula = true; Resources(text, volley.manaCost, volley.cooldown); break;
                case "A15X":
                    Elements(text, combat, native, level); attackFormula = true;
                    var bow = new OriginalArcherCastRules(combat, "A15Z", level);
                    Resources(text, bow.manaCost, bow.cooldown); break;
                case "A0AC":
                    text.Append("Пассивно. +").Append(N(combat.Ability("A0N6").Number("DataA" + level))).Append(" к урону обычных атак.")
                        .Append("\nУровень 1: «Мастерство стрельбы» → «Сильный выстрел» выпускает три стрелы веером.")
                        .Append("\nУровень 2: «Лук стихий» → «Сильный выстрел» добавляет эффект стихии поражённым целям.")
                        .Append("\nУровень 3: «Лук стихий» → «Мастерство стрельбы» добавляет эффект стихии всему залпу.")
                        .Append("\nСочетайте навыки подряд. Окно сочетания с «Мастерством стрельбы» - 5 с; завершённый «Сильный выстрел» сбрасывает сочетание. Улучшение сохраняет предыдущие сочетания.");
                    break;
                case "A0SJ":
                    text.Append("Выберите направление, дальность ").Append(N(combat.Ability(skillId).Number("Rng" + level)))
                        .Append(". Вакуум летит вперёд до 900 и затем взрывается; повторное применение взрывает его раньше бесплатно.")
                        .Append("\nСтягивает врагов в радиусе 200 и наносит от ").Append(N(OriginalHeroRules.PyroVacuumDamage(level, 0)))
                        .Append(" до ").Append(N(OriginalHeroRules.PyroVacuumDamage(level, 900)))
                        .Append(" магического урона: чем дальше пролетел вакуум, тем больше урон. «Кипучие оковы» усиливают попадание.");
                    Resources(text, combat, skillId, level); break;
                case "A0SN":
                    text.Append("Без выбора цели. Взорвать уже летящий вакуум в текущей точке. Дальность полёта определяет урон; враги в радиусе 200 стягиваются к взрыву.");
                    Resources(text, 0, 0); break;
                case "A0SP": case "A0SR":
                    text.Append("Без выбора цели. Создаёт 5 сфер вокруг героя примерно на 15 с. Повторными применениями выбирайте точки и запускайте по одной сфере бесплатно, на расстояние до ")
                        .Append(N(combat.Ability("A0SO").Number("Rng1"))).Append(".")
                        .Append("\nВзрыв сферы наносит ").Append(N(OriginalHeroRules.PyroSphereDamage(level, skillId == "A0SR")))
                        .Append(" магического урона в радиусе 200 и отталкивает врагов. «Кипучие оковы» усиливают попадание.")
                        .Append("\nСферический посох: урон сферы ").Append(N(OriginalHeroRules.PyroSphereDamage(level, true)))
                        .Append(", создание стоит ").Append(N(combat.Ability("A0SR").Number("Cost" + level)))
                        .Append(" маны. Число сфер и перезарядка не меняются; снятие посоха не сбрасывает уровень или перезарядку.");
                    Resources(text, combat, skillId, level); break;
                case "A0SO":
                    text.Append("Выберите точку на расстоянии до ").Append(N(combat.Ability(skillId).Number("Rng1")))
                        .Append(". Запускает одну из подготовленных сфер. Взрыв в радиусе 200 наносит ")
                        .Append(N(OriginalHeroRules.PyroSphereDamage(level, false))).Append(" магического урона, со Сферическим посохом - ")
                        .Append(N(OriginalHeroRules.PyroSphereDamage(level, true))).Append(". Враги отталкиваются; «Кипучие оковы» усиливают попадание.");
                    Resources(text, 0, 0); break;
                case "A0AE":
                    text.Append("Выберите точку, дальность ").Append(N(combat.Ability(skillId).Number("Rng" + level)))
                        .Append(". Создаёт оковы на ").Append(N(OriginalHeroRules.PyroChainDuration(level))).Append(" с в радиусе ")
                        .Append(N(combat.Ability("A0SK").Number("Area1"))).Append(". Удерживает врагов у края области и снижает их скорость движения на ")
                        .Append(P(-combat.Ability("A0SK").Number("DataA1"))).Append("%.")
                        .Append("\nПри попадании вакуума, сферы или метеорита по цели с оковами урон этой атаки увеличивается на ")
                        .Append(P(OriginalHeroRules.PyroChainAmplifier(level) - 1)).Append("%. При нескольких связанных целях усиление последовательно накапливается в одном взрыве. Сами оковы урона не наносят.");
                    Resources(text, combat, skillId, level); break;
                case "A0SM": case "A0SS":
                    string flame = skillId == "A0SS" ? "A0ST" : "A0SQ";
                    text.Append("Выберите точку, дальность ").Append(N(combat.Ability(skillId).Number("Rng" + level)))
                        .Append(". Через ").Append(N(OriginalPyroEffectRules.MeteorDelay)).Append(" с метеорит наносит ")
                        .Append(N(OriginalHeroRules.PyroMeteorImpact(level))).Append(" магического урона в радиусе 200, затем катится вперёд и оставляет 6 очагов огня.")
                        .Append("\nКаждый очаг наносит ").Append(N(combat.Ability(flame).Number("DataA" + level))).Append(" магического урона раз в секунду в течение ")
                        .Append(N(combat.Ability(flame).Number("Dur" + level))).Append(" с. Равные перекрывающиеся очаги не складываются на одной цели. Оковы усиливают первоначальный удар.")
                        .Append("\nСферический посох продлевает очаги до ").Append(N(combat.Ability("A0ST").Number("Dur" + level)))
                        .Append(" с; применение стоит ").Append(N(combat.Ability("A0SS").Number("Cost" + level)))
                        .Append(" маны. Урон за тик и перезарядка не меняются; снятие посоха не сбрасывает уровень или перезарядку.");
                    Resources(text, combat, skillId, level); break;
            }
            if (attackFormula) text.Append("\n\nРасчётная атака навыков зависит от основной характеристики, улучшения атаки и бонусов предметов. Это не случайный урон обычного выстрела.");
            return text.ToString();
        }

        static void Elements(StringBuilder text, OriginalCombatCatalog combat, OriginalNativeCatalog native, int rank)
        {
            var venom = new OriginalArcherDebuffRules(combat, native, "A166", rank);
            var frost = new OriginalArcherDebuffRules(combat, native, "A168", rank);
            var dark = new OriginalArcherDebuffRules(combat, native, "A165", rank);
            text.Append("Без выбора цели. Заряжает 5 попаданий текущей стихией. После применения следующая стихия готовится 18,9 с: яд → лёд → огонь → тьма → молния → яд.")
                .Append("\nЯд: ").Append(P(OriginalHeroRules.ArcherVenomDamage(rank, 1))).Append("% расчётной атаки урона с учётом брони и -")
                .Append(N(venom.armorReduction)).Append(" брони на ").Append(N(venom.duration)).Append(" с.")
                .Append("\nЛёд: ").Append(N(OriginalHeroRules.ArcherIceDamage(rank))).Append(" магического урона цели и ").Append(N(25 * rank))
                .Append(" соседним врагам в радиусе ").Append(N(frost.area)).Append(". Замедляет движение на ").Append(P(frost.movementSlow))
                .Append("%, атаку на ").Append(P(frost.attackSlow)).Append("% на ").Append(N(frost.duration)).Append(" с (героев - на ")
                .Append(N(frost.heroDuration)).Append(" с).")
                .Append("\nОгонь: ").Append(N(OriginalHeroRules.ArcherFireDamage(rank))).Append(" чистого урона в радиусе 150.")
                .Append("\nТьма: отталкивание, ").Append(P(dark.missChance)).Append("% вероятности промаха и -").Append(P(dark.attackSlow))
                .Append("% скорости атаки на ").Append(N(dark.duration)).Append(" с.")
                .Append("\nМолния: ").Append(N(OriginalHeroRules.ArcherLightningDamage(rank))).Append(" магического урона каждой из максимум 7 разных целей; дальность перехода 700.");
        }
        static void Resources(StringBuilder text, OriginalCombatCatalog combat, string id, int rank) =>
            Resources(text, combat.Ability(id).Number("Cost" + rank), combat.Ability(id).Number("Cool" + rank));
        static void Resources(StringBuilder text, double mana, double cooldown) =>
            text.Append("\n\nМана: ").Append(N(mana)).Append(". Перезарядка: ").Append(N(cooldown)).Append(" с.");
        static string Formula(double flat, double fraction) => N(flat) + " + " + P(fraction) + "% расчётной атаки";
        static string N(double value) => value.ToString("0.##", CultureInfo.GetCultureInfo("ru-RU"));
        static string P(double value) => N(value * 100);
        static string Name(string id)
        {
            switch (id)
            {
                case "A001": return "Характеристики";
                case "A05N": return "Иллюзии";
                case "A05M": return "Древний щит";
                case "A102": return "Удар щитом";
                case "A0E6": return "Тёмные дары";
                case "A15W": return "Сильный выстрел";
                case "A0AS": return "Мастерство стрельбы";
                case "A15X": return "Лук стихий";
                case "A0AC": return "Заколдованный лук";
                case "A0SJ": return "Вакуумный взрыв";
                case "A0SN": return "Взорвать вакуум";
                case "A0SP": return "Горящие сферы";
                case "A0SR": return "Горящие сферы со Сферическим посохом";
                case "A0SO": return "Запустить сферу";
                case "A0AE": return "Кипучие оковы";
                case "A0SM": return "Живой метеорит";
                case "A0SS": return "Живой метеорит со Сферическим посохом";
                default: return "";
            }
        }
    }
}
