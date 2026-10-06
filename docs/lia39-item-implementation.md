# Предметы LiA 3.9c: исполнение и оставшиеся механики

Срез 2026-10-06. Исходная карта SHA256 `02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34`, native Warcraft III `1.26.0.6401`. Это текущая матрица портирования, не заявление о полном совпадении с оригиналом.

## Доступность

- Каталог: 440 ID; 134 уникальных исходных предложения магазинов, 99 результатов рецептов, 233 уникальных inventory ID в объединении.
- Применимый постоянный профиль: 256/440, в магазинно-рецептурном объединении 229/233. Отдельные C6 powerups применяются транзакцией и не требуют профиля экипировки.
- Результаты рецептов: 99/99 имеют применимый профиль. ITEMSTAT2 подтвердил для exact I029/I05D отсутствие добавочной armor от A059 поверх A04S5/A0518. Разрешение ограничено этими двумя предметами и rank1, статическое отсутствующее поле не перезаписано.
- Прямые магазинные inventory ID: 130/134 имеют профиль. Остальные четыре - C6 powerups (`I05F`, `I07W`, `I0AI`, `I0AT`); все четыре исполняются. I0AT использует полный исходный пул139 наград; AIrg I0AJ закрыт native87 и отдельным consumer.
- Есть 165 публичных маршрутов Use в объединении: 86 заряженных рецептов и 79 обычных активных ID. Это наличие исполнителя, а не доказательство всех частных взаимодействий.
- Все31 исторически незавершённых прямых active ID теперь имеют Use-исполнитель, включая I049/I00Z. Точный список31 сохранён ниже для сверки; наличие маршрута не доказывает все частные native взаимодействия.

Числа получены actual Core coverage harness `.local/lia-port/item-coverage-check/ItemCoverageCheck.csproj`, `coverage.json`, `final-route-counts.json`, `functional-families.json`. `EffectsComplete` и старые `native-family-unimplemented` записи в статическом плане не являются актуальным реестром runtime-dispatcher: часть семейств уже исполняется внешними Session consumers.

## Рабочие механики

| Механика | Исполнение и граница доказательства |
| --- | --- |
| Атрибуты, HP/MP, armor, flat damage, IAS, boots, regen | Изолированные native equip43+221+8 и декларации. Последовательные vitality transitions, учет экземпляров и servant/hero. Смешанные наборы и часть float rounding явно derived. |
| Crit AOcr / bash AHbh / feedback Afbk | Общий unit/item resolver; в доступных предметах 7 ID содержат crit, 5 bash. У двух bash отсутствует DataC: stun работает, дополнительный урон не выдуман. Порядок нескольких procs и host RNG derived. |
| AUts / Assk | Отражение/входящий множитель/блок по authored параметрам и общим consumers. Stage/minimum и сочетание копий отдельно маркированы. |
| AIva / AUav | Приоритет orb по физическому слоту, лечение от фактической потери HP основной атакой; aura только соответствующему оружию/targets. Совместное сложение маски и aura derived. |
| AIob / AIfb secondary passives | Все шесть ID имеют consumer `ItemOrbSecondary` в обычном weapon release/impact. I01N и spellbook-child A062 у I0B0, A0BJ у I03N: движение -40%, IAS -25%, B00X/B00U, срок 3 с / 1 с для native героя. I02A A07K: соседям15; I02C/I0A6 A08B:75, героям12/60, radius180, Amim допускается. Вторичные огненные события идут до основной атаки; уже выпущенный projectile сохраняет captured orb после снятия предмета. ITEMORB2 измерил четыре точных предметных строки; перенос на I0B0/I0A6, slot arbitration и частные сочетания остаются declared/host policy. |
| AIsr | Хронологический last-added ledger; item instance/occurrence и Knight A0E5. Удаление возвращает предыдущую resistance. |
| AHad/AHab/AUau/AOae/Aoar/Aakb | Общая aura-модель с target filters, базовыми статами до временных modifiers; strongest-per-buff, scan и linger являются явной host policy. |
| A1CS/B0CH Dawn Boots | Source yQ каждые1s прямо восстанавливает.5%maxHP/MP; копии не умножают buff. Membership и timer phase derived, отсутствующий Hab1 не выдуман. |
| hL/FL item spell power и magic vamp | Исходные per-slot handlers, Nemesis pool, проценты и флаги. Отделены от native helper/proc damage, чтобы не удваивать усиление. |
| I00Z | ANeg SP/SR и SM/SS: canonical ranks/points/cooldown, native aliases, повторные экземпляры/снятие без потери ранга; переход в середине pre-effect cast отдельно не измерен. |
| I0B7 / I0AE | Исходные disassembly и Nemesis/reset handlers; target instance, возврат ингредиентов, pool/timer. |
| C6 / native pickup | Валюта, XP и награды C6 по recipient guards; gold/lmbr, tstr/tdex/tint, rhe2/rres и rspd15s. Постоянные атрибуты сохраняются при level-up и смене вещей. |
| Руны rman/vamp/rspl/rdis/rsps | Ground powerup transaction: rman +250MP органическим союзникам в2000; vamp/I07G +50weapon damage и100%actual primary HP loss на15s; rspl исходные семь наград из1..10, пустые7/9/10 без reroll, конверсия world/inventory и fallback через servant/ground. APDI3: rdis снимает Bblo/Bslo/Bspe/BIpv с обеих сторон, сохраняет физическую A19U/B0BL сеть и I0AJ/B0B1; hostile nonimmune Asum получает native250, ally/Amim не получает, hostile ordinary вызывает actual0 callback, отдельно I6/R6 hL250 в500 по sourceOwner11 с userData!=1/2 и без magic immunity. Rsps accepted consume сохраняет ITEMSTAT2 activationKnown=false: shield не выдуман. Full-bag AIpv, stacking/refresh/pause, другие buffaliases/illusion factories, Alsh/AIda carrier и native/source event order явно derived. Rawcurse abilities, Knight source timers/U8 lightning pulses/A0E5 resistance и A05M stance сохраняются. |
| Item controls | Silence/root допускают potion; stun/sleep очередь без раннего списания; Doom/pause отказ. Native ITEMCTL1. |
| Заряженные рецепты | 86 доступных ID через общую recipe transaction; QuickBuy, stock/debit/instance и atomic profile preflight. |
| I048 / AIil | Публичный Unit-target Use, 240MP/CD18, image10s с outgoing1/incoming2. Canonical три героя и допустимые ordinary donors, exact A04U исключение, atomic preflight и detached lineage. Общая native фабрика измерена; перенос ordinary f3 и размещение derived, item passives не копируются без доказательства. |
| I082/I083 Power Seal | MW/qW/jW: новый физический item handle при переключении, global1s armor/damage расчет, cap30/200 и self16 armor probe с восстановлением HP. Снятие убирает текущий бонус. Native AIha активация и смешанные modifiers probe являются host policy. |
| I09L/I09M/I09N Warpath | FT/GT/jT/kT/KT: атрибуты и три режима, heal/mana/AGI area/execute/reflect с исходными условиями. Сохранена особенность ranged reflect: item у защитника, wx у атакующего. Aura membership без linger и RNG host policy, не native replay. |
| I072/I05Q Fortitude | dq/Vq/nq: 6/8 секунд counter, проверка до decrement, refresh и +1 guardian. Amim A18P раскрыт из точного A18O spellbook. I05Q guardian сохраняет фазу после drop, выбирает последнюю допустимую цель, наносит300 magic и дает1 даже без цели. Helper placement/movement и отсутствующие A18Q numeric axes отдельно ограничены. |

Публичные активы: `I022/I03L` здоровье, `I023/I03M` мана; `I01A/I01D/I01M/I02G/I07E/I07K` призывы; `I02H/I02E/I03Z` armor actions; `I01L` area restore; `I06M` invisible7s/fade2 и `I06O` invulnerable3s; `I021/I094` wards; `I0AE/I0B7` исходные scripts. Все17 AIda script actions также имеют route; их отдельные helper boundaries описаны в собственных тестах. Дополнительно `I01J` реализован как summon, но не входит в текущие 233 прямых/recipe ID.

## Оставшиеся функциональные границы

| Scope | Текущий статус |
| --- | --- |
| I049 / A0W8 и I00Z / S000 | Публичные Use исполняются. SUMSCRIPT2 подтверждает I00Z native summon/cost, SUMSCRIPT3 строго отделяет A14K spellbook callbacks у I049. Manual10 и SummonScript11 portable tests прошли; A0YP движение отдельно подтверждено CONFUSE2 ниже. |
| AIob / AIfb secondary passives | Шесть прежних passive gaps закрыты исполнителями и portable8/8. Отдельно сохраняются native границы: private snapshot instant, exact radius edge/body inclusion, air/ward/invulnerable axes, orb stacking/priority, source death/reflection и status pause/refresh. Эти сочетания имеют проверенные host policies, но четырёх native строк недостаточно для заявления полной engine parity. |
| rsps, ground rune | Публичный consume исполняется вне233 shop/recipe union. ITEMSTAT2 accepted unpaused H008 pickup не дал BNss на H008/allyH024; AHtb80 прошёл. Rune positive effect не доказан и shield numbers/срок не подставляются. Этот scoped transfer не является универсальным native default0. |
| Wards | Оба Use выбирают Point либо живого видимого Unit; дальний приказ сначала подходит к declared range500. Explicit matching owned item target ставит рядом с героем. Mana ward применяет исходные3% maxMP/s. Наблюдавшееся удвоение native refresh пока не объяснено, см. ниже. |
| Наборы и native precision | Private RNG, порядок native groups, некоторые inherited поля, refresh/stacking/pause и геометрия перенесены как явные host policies. Наличие рабочего consumer не превращает эти границы в measured parity. |
| Проверка полного матча | Требуются общий Unity regression и обычный матч с покупками, рецептами и навыками. Portable focused tests не заменяют этот сценарий. |

Независимый срез orb scope: `item-orb-check/final-independent-portable.json`8/8, `final-independent-strict.txt`5/5. ITEMORB2 Campaigns SHA256 `ff43f58dc53f0232f8dc3ee328a1b672843235b4a2d997273402cc0b53f6569d`, probe map `5bf92f6d44124f1fd16ed723166a061b334533a8aa85482551ddc55e1b026739`, script `cded3cb5dae98957a7f704871a5ac661f67b78f9e416e2e8a3ef6e1c39602473`. Normalizer `tools/arena/extract_observed_item_orbs.py` проверяет fresh CRC, original map bytes, exact четыре строки, положительные атаки, actual post-damage HP, buff expiry, cadence и отрицательные recipients. I0B0 использует A062 через A1D0 spellbook (`AbilityData.slk:38978`); I0A6 напрямую A08B (`AbilityData.slk:9977`). Ревью не нашло нового исправляемого дефекта в данном scope; Unity и standalone повторяет root.

Rune/barrel consumer срез: `.local/lia-port/runes-barrels-check/final-green.json`19/19 = Rune14 + LifeSteal5; соседние Pickup12/12, Doodad3/3, Weapons20/20. Первый focused RED9/9 сохранил прежние отсутствующие consumers. Дополнительные checks проверяют все1..10 rspl draws, RNG только при commit, stale transaction, полные hero+servant bags, pause/refresh и public host Advance + actual snapshot codec. Общий Unity/standalone прогон остаётся отдельной проверкой root.

APdi independent review выявил два воспроизведённых bugs: native250 ошибочно бил allied summon, а hostile ordinary0 не доходил до aie orb watcher. Focused RED2/23 сохранён в `apdi-damage-red.json`; отдельный physical-net RED1/20 подтвердил потерю A19U при generic clear. Carrier regression после исправления fixture уровня/навыка отдельно проверил исчезновение Alsh buff с сохранением U8 timer/A05M stance; локальная копия Core до carrier fix находится в `apdi-carrier-valid-red.json`, Assets для этого не откатывались.

Native APDI3 provenance: `Campaigns.w3v` SHA780eafb957fa62ec42a341ad0086cf39d3f59522ff84ed551a12f251543d06b2, probeMap SHA67eef7b300f7b9346ce8e41e33672238c3723fa62bba7c4979034d0bf8af5284, ownScript SHA561e461ce2127ac9a623524463d94e0b14de9f7e457ce6d97f90348cfbefa3d0. Четыре admitted records, fresh CRC/hash и strict mutation suite5/5: `tools/arena/extract_observed_rune_dispel.py`, `test_extract_observed_rune_dispel.py`. APDI1 immutable rejected summon row с false order852008 не использовался как negative; APDI2 actualI01A дал3n01V с ACmi/nativeMagicImmune1 и только затем APDI3 заменил контроль на nonimmuneI02G/n01R. Native factoryAsum, ownership, sampledflags и immediateHP900→650 выделяют damage250 отдельно от отсутствующего в probe sourceI6hL. Это не исчерпывающий native dispel по doom/stun/air/ward/structure/invulnerability и не доказательство privategrouporder.

Актуальный focused consumer результат после APdi fixes: `.local/lia-port/runes-barrels-check/apdi-final-green.json`24/24 = Rune19 + LifeSteal5. Проверены physical-net preservation, allied magic removal, Alsh carrier без удаления U8 timer/Defend stance, hostileAsum native250 отдельно от sourcehL250, Amim/ally negatives, actual0 callback к Archer watcher и B0B1 с сохранённым следующим resourcepulse. Уровень доказательства среза portableCore; actual Unity/root release остаются отдельными результатами.

Соседний portable batch после этих APdi fixes: `.local/lia-port/runes-barrels-check/apdi-neighbor-green.json`38/38. Включены прямые `[Test]` methods из Pickup, Doodad, Archer, ItemProtection и ArcherDebuff fixtures; параметризованные `[TestCase]` и полный actualUnity suite выполняются отдельно root. Strict normalizer проверяет фактическую позицию hostile helper240/752 с самого initial snapshot, вместо запрошенной CreateUnit позиции300/700, и допустимую .1s quantization окон after/final; edge radius этим не объявляется измеренным.

Последующее taxonomy ревью добавило подтверждённые primary exceptions: [Blizzard Spell Basics](https://classic.battle.net/war3/basics/spellbasics.shtml) исключает обычный poison (отдельно от Shadow Strike) и замедление Purge из dispel, [Pit Lord](https://classic.battle.net/war3/neutral/pitlord.shtml) исключает Doom. В3.9c четыре measuredpoison aliases имеют nativecodeAven, ACpu имеет Aprg, A0HR имеет ANdo. Core сохраняет их исходные stateobjects/expiry/pulsephase и Doom item/castcontrol. Focused RED2/26 и GREEN26/26 (Rune21 + LifeSteal5): `apdi-exceptions-red.json`, `apdi-exceptions-green.json`. Это primary+map-code transfer, не новый native APdi runtime для этих трёх семейств.

После этого minimumtaxonomy diff повторно запущен соседний portable batch: `apdi-taxonomy-neighbor-green.json`38/38 (Pickup12, Doodad3, Archer12, ItemProtection5, ArcherDebuff6). Изменений общего negative-cleanse для других callers нет; новый preservation helper применяется только к runeAPdi.

После явно выделенного владельцем окна ПК APDI4 выполнен и сохранён immutable: четыре admitted actualnativecontrols I01N/A062/B00X, I03N/A0BJ/B00U, A0H5/AHbh/BPSE weaponbash100%, AHtb/BPSE spellstun. Во всех четырёх prebuff1 и accepted APdi с реальным enemy damage0 от отдельного picker; buff сохраняется immediate/final. Weapon source Stop+Abun до pickup, без PauseUnit; AHtb имеет правильный SPELL_EFFECT caster/target, weaponrows не имеют spell effects. Map SHA62e815a21bc8449b673f6c41467f85aa51b49fbfe80e034467a654b77da88141, ownScript SHA011f9f6e6539910e0f14f3e7c8fb0875e29b91ee4fc62cd9a5c383c68b5fee70, Campaigns SHA861dc24347fa68e095662dca2de8616cd2e9f61bbf97525158bfe48ee16af4c7. Final.22s находится внутри nativehero1/3s duration; естественное expiry этим коротким native run не измерено.

APDI4 strict freshCRC/hash + mutation4/4: `tools/arena/extract_observed_rune_dispel4.py`, `test_extract_observed_rune_dispel4.py`. Core сохраняет исходные A062/A0BJ debuffobjects и BPSE/AHbh controlobjects с их сроком/remaining, не начинает таймер заново. Исправлен прежний ошибочный fixture, считавший AIob снятым: positive magical test теперь использует measuredAPDI3 Aslo/A071/Bslo. Valid focused RED2/28→fresh forced-rebuild GREEN28/28 (Rune23 + LifeSteal5), `apdi4-valid-red.json`/`apdi4-fresh-green.json`; ранний `apdi4-green.json` был incremental stale output после Copy-Item с сохранённым старым mtime и не является результатом актуального Core. Parent source review stateobjects принят CLEAN. Allied targets, другие AHbh/BPSE aliases/custombuffs остаются transfer; AUfn/Bfro, полная taxonomy, privategrouporder и все radius/type axes не объявляются nativeexact. Desktop освобождён сразу после capture для следующего замера.

## Ward boundary

ITEMWARD2 (campaign `b27eb9dffab9d081a2d4d032c2cc635fe9c1643e8fa39d6ccd5c77bf7d62c3bd`) измерил один `ohwd/o00J` на строку, HP5, MP0, speed0, armor0 и срок 30/15 секунд. Лечение стабильно +3% maxHP/s. Mana ward сначала дает около3% maxMP/s, затем около6%, с отдельным провалом. Ни среднее значение, ни постоянные6% не объявлены native правилом. Исполнитель временно использует authored3%, с явным ограничением. Acv37261..37285 выбирает spell target X/Y и заменяет их на позицию caster только при matching target-item rawcode. Обе ветки теперь представлены публичным Use: Point metadata + координаты, либо explicit matching inventory item target. Точка(0,0) является настоящей координатой. Range500 взят из A0UK/A0UL Rng1; принадлежность item reference игроку и free-spawn solver являются host policies. Радиус ауры500 authored, свободное размещение с body16 и aura membership derived. Native outer channel placement/unit targeting/подход к дальней цели этим probe не измерялись.

Ward Point gap закрыт узким diff в rules/ItemUse/ItemSummons и runtime input без нового enum/protocol. Повторный вызов того же armed ward отправляет явный item-self target; успешная отправка снимает intent, отказ отправки сохраняет его. Other summon items и I08I Point используют прежние маршруты. Validation finite/range/kind/item выполняется до очереди и списания; optional World отсутствие возвращает NotReady. Current focused56/56 после forced rebuild, baseline54/19 и отдельный null-world RED56/2 сохранены в `.local/lia-port/ward-point-check/`. Две новые actual PlayMode проверки point/Escape/позиции/одного charge и item-self подготовлены; фактический Unity запуск выполняет root отдельно. Native ward profile/lifetime не повышаются до доказательства полного outer channel targeting.

## Проверки

- Native cache readers проверяют CRC, immutable map/script/campaign hashes, exact matrix и отдельные положительные/отрицательные controls.
- Исторический focused run до последних handlers: Pickup11/11, Equipment9/9, ItemUse23/23, ItemNative26/26; item-ward normalizer5/5, ITEMARM5/5, ITEMSTAT6/6, visibility wire3/3. Два новых RED→GREEN: Dawn pulse и native Mechanical classification. Все portable actual C#, не замена Unity smoke.
- Независимое ревью NativePickups/I05P и hL/profile завершено CLEAN. I01L и FL/AIcb прошли независимое ревью. Dawn, A059, статусы и rspd получили независимое CLEAN review. Полный обычный матч с этими предметами еще не завершен.
- Новые отдельные actual Core проверки: I0486/6, PowerSeal/Warpath7/7, Fortitude5/5, все с независимым CLEAN review. I017/I05E public exchange8/8 и independent CLEAN; ITEMEX2 strict5/5 с fresh CRC и независимым review. Native costs90/0, мгновенные5 стадий, 104 max-change chunks и порядок DROP-before-intrinsic/PICKUP-after-intrinsic подтверждены. I017 B010 даёт +20% движения/+200% IAS/cast-only block8s и восемь zero callbacks; pause/refresh/mixed modifier composition остаются host policies.

## Контрольный список Use ID

Исходный список31 ID сохранен для сверки последующих consumers; это больше не счетчик недоступных маршрутов. У каждого есть прямая способность активации, а вложенный spellbook сам по себе не заменяет актив. Опубликованные ниже маршруты отмечены явно; агрегаты в разделе доступности относятся к свежему coverage run после публичных I049/I00Z. Список не включает powerups C6.

| Item | Active ID / family | Исходные handlers |
| --- | --- | --- |
| I00Z | S000 / AUin | W7, kgv, kGv |
| I013 | A01X / AOwk | nW |
| I015 | A028 / AOcl | jQ, kgv, kGv |
| I017 | A0UD / ACtc | Реализован G8 + measured A0VG/B010, public8/8 с I05E |
| I01Y | AIxs / Aami | native |
| I01B | A07W / Alsh | T8, kgv, kGv |
| I026 | A088 / ANfd | native |
| I02C | A08A / AOsh | K8, l8, kgv, kGv |
| I048 | AIil / AIil | N6, m6; Use опубликован, scope выше |
| I06R | A0OU / Aroa | PT |
| I06J | A0FI / Aste | M7 |
| I072 | A0FK / Aami | dq/Vq/nq; Use опубликован |
| I07P | A0JE / Auhf | W8, kgv, kGv |
| I04B | A06W / AOwk | vVv |
| I07Y | A028 / AOcl | jQ, kgv, kGv |
| I082 | A0K0 / AIha | MW/qW/jW/yP; Use I082/I083 опубликован |
| I08U | A0NA / Aste | M7 |
| I090 | A0OS / Aroa | PT, wT |
| I09L | A0SW / AIha | FT/GT/jT/kT/KT; Use I09L/M/N опубликован |
| I09X | A0TM / Aens | BT, kgv, kGv |
| I049 | A0W8 / ANfd | j7 |
| I05D | A0YK / AIha | hK, eW |
| I05E | A15B / ACtc | Реализованы Tu/wu/Wu, native Cost0 и callbacks88, public8/8 с I017 |
| I05Q | A11Z / Aami | dq/Vq/nq/cq/Bq/Oq; Use и guardian опубликованы |
| I07C | A158 / ACtc | hK, bt |
| I07M | A16M / Alsh | T8, U8, kgv, kGv |
| I08D | A0JE / Auhf | W8, kgv, kGv |
| I0AJ | A18H / AIrg | native |
| I0AL | A1DZ / ANdh | xu |
| I0AP | A19K / AOwk | native |
| I0B2 | A1CW / AIsa | qS |

## Новые native/source действия, 2026-10-06

- I0AJ / A18H: мгновенно расходует один заряд без MP-cost и восстанавливает по0.1 MP десять раз за10s, всего1 MP. ITEMREGEN1 d67b21c5efac с positive stock clarity подтвердил отсутствие дополнительного HP восстановления. Положительный входящий урон прекращает оставшиеся импульсы по ITEMSTAT2. Нулевые damage callbacks не отменяют эффект; pause/повторное применение/full-MP refusal и inherited Cool0 остаются host policy.
- I0AL / A1DZ:125 MP, CD18, вражеская organic цель в700. Исходный ZT снимает до300 текущей MP и наносит1.5 от снятой маны через hL Universal. B06X подавляет source damage/burn; cast-only запрет3s переносится из native mask8, неизвестные ANdh miss/movement/IAS поля не объявлены measured0.
- I04B / A06W:250 MP, CD25. Source vEv создает A05V silence helper, запрещающий атаки и заклинания organic врагам в350 на4s. Активация и helper delivery исполняются сразу как explicit host policy, не как доказанный native cast timing.
- I026 / A088:80 MP, CD12, вражеская organic цель в500,275 native MAGIC. Перенос мгновенной активации/типа урона из семейства derived; урон не получает source hL item amplification.
- I013/I0AP: Wind Walk передает бонус первой реально выпущенной атаке и расходует его ровно один раз. Уже выпущенный снаряд сохраняет bonus после Stop; source I013 sweep и I0AP бонус отделены от обычного Cripple white damage.

Проверка этого блока: ItemUse31/31 actual portable, Weapon20/20 actual portable, ITEMREGEN5/5 свежий CRC; I0AL/I04B, WindWalk и AIrg получили независимое CLEAN review. Это focused evidence, не завершенный Unity match.

## Последние исходные предметы, 2026-10-06

- I09X / A0TM:400 MP, CD20, начальная цель в700. Исходный bT/NT выбирает до восьми следующих не посещённых врагов каждые0.3s, строго ближе500 к предыдущей цели; отсекает невидимых, здания и механических. Каждый A0TL удерживает движение5s. Цепь продолжает работу после смерти героя; каждый h011 отдельно уведомляет глобальный CA через1.5s. Полёт1500 и два zero callbacks перенесены из измеренного семейства Aens, точные задержки этого item alias остаются derived.
- I0B2 / A1CW: CD24, активный бонус движения20% снимается исходным PS через5s. Каждые0.2s перемещение хотя бы по одной оси более20 создаёт неподвижный участок радиуса150, либо250 во время актива. Участок делает шесть импульсов по22.5 hL MAGIC через0.5s и переживает смерть владельца. Сохранена буквальная особенность HS: ключ первого юнита группы не обновляется внутри цикла, поэтому один участок повреждает только первую подходящую цель в host порядке. Порядок native group и проекция helper body не измерены; Cost0 AIsa inherited/derived.
- I0AT: source xC содержит139 исходных предметов с весами1/0.8/0.5, сумма130.8. Сервер выбирает награду отдельным детерминированным RNG только при успешном commit; точную приватную последовательность Warcraft не обещает. Полный инвентарь оставляет принадлежащую игроку награду на земле. Проверены все139 candidate profiles, списание цены/stock, повтор команды и сохранение RNG при незавершённой подготовке.

Focused checks этого блока: ItemUse34/34, Pickup12/12, ItemEffects81/81 actual portable. I0B2 независимо CLEAN; I09X helper-death regression воспроизведён RED и исправлен GREEN, повторное независимое ревью CLEAN. I0AT независимое ревью CLEAN. Это не evidence полного обычного матча.

## Контроль покрытия после ITEMSHELL2

Текущий actual session run:99/99 recipe outputs имеют применимый профиль;134 прямых shop inventory IDs состоят из130 профилей и четырёх работающих C6 powerups (`I05F/I07W/I0AI/I0AT`). В объединении233 ID нет неприменимого equipment/pickup результата. Это проверка применимости, а не доказательство всех частных native взаимодействий.

Все31 исторических active IDs имеют публичный маршрут. Свежий actual Core coverage после I049/I00Z содержит165 Use routes:79 обычных и86 заряженных рецептов. У recipe outputs61 обычный active route, у прямых shop ID18 обычных и86 recipe-use. Данные находятся в `.local/lia-port/item-coverage-check/coverage.json` и `final-route-counts.json`.

- I01Y / AIxs: заряд,0MP, CD18,6s. ITEMSHELL2 `cf6553fc43ab` подтвердил подавление40/200 SPELLS/MAGIC без damage callback; SPELLS/NORMAL, CHAOS/NORMAL и CHAOS/UNIVERSAL в тех же последовательностях не изменились. В runtime gate ограничен typed SpellMagic; native immunity type flag, hostile-control immunity, arbitrary damage magnitudes и repeat/pause/dispel остаются отдельными границами. DataB100 не превращён в недоказанный100HP absorption pool.
- I01B / A07W и I07M / A16M:350/450MP, CD25, range800, многоразовые. Нативный carrier buff8s мгновенно подтверждён self-контролями. Исходный U8 делает восемь импульсов150/200 hL MAGIC в275 вокруг носителя; не поражает здания, magic-immune и носителей того же buff. Timer сохраняется после смерти героя или носителя; удаление retained carrier останавливает host callback. B06X подавляет только создание source timer. Native data не добавляет отдельный скрытый damage поверх U8: в тесте self/enemy100/ally200 никаких native damage events не было. Другие targets и same-time expiry/stacking применяют явную host policy.

ITEMSHELL2 strict normalizer5/5 с fresh CRC получил независимый CLEAN; focused runtime ItemUse37/37 прошёл, включая upgraded enemy carrier; независимое ревью runtime завершено CLEAN, reviewer повторил37/37. I09X helper death и I0AT независимое ревью завершено CLEAN, Pickup12/12 и ItemUse34/34 повторены reviewer.


## Предметные подсказки (Presentation)

`OriginalItemEffectText` читает тот же `OriginalInventoryEffects`, что и базовые бонусы. Отдельные `DescribeActive` и `DescribePassive` не изменяют игровой state: мировой ID магазина разрешается в inventory ID, числа нативных семейств берутся из combat catalog, состав spellbook - из замороженных effective rows. Описаны все текущие публичные active families, 86 charged recipe-use, особые криты/оглушение/вампиризм/орб порчи/блок/отражение/сопротивление/ауры и усиление заклинаний. Простые бонусы характеристик не повторяются. Название native orb-family само по себе не используется для обещания неработающего яда или вторичной цепи.

Проверка: ItemTextCheck 7/7 (233 shop/recipe ID, world alias, detached read-only, числа/ограничения, generic recipe descriptions). Special-passive layer прошёл независимое ревью Waves; 11 ANcl/legacy7/Scripts/chain3 просмотрены root, уточнения disassembly и длины shockwave внесены. Итоговый Additional29 целиком независимо проверен Layout, CLEAN. Интеграция в OriginalItemText выполняется root и проверяется отдельно в Unity.


## Финальный A0YP movement control

CONFUSE2 (native94b, campaign `809839b2ff26d45cce88cee552fde72b8ba83df9bab696eb0c4bab1e21e30e2d`) завершил4/4 строки: self, ally100, enemy100, outside1200. У свежих unpaused hfoo до/при/после A0YP getter оставался270. Все три реальные одно-секундные команды перемещения дали261..268WC; расхождение в несколько единиц не используется для восстановления внутреннего frame scheduler. B084 появился только у носителя A0YP, сохранился при удалении способности на7s и исчез между9.0 и9.1s. При добавлении зарегистрирован ровно один self zero callback, HP420 неизменно.

Отсутствующие числовые поля A0YP остаются отсутствующими. Для текущего movement consumer эти данные подтверждают отсутствие добавочного замедления; IAS, иные unit types, pause, stacking и точная фаза aura refresh не измерены. CONFUSE1 failed rows сохранены отдельно: первый прибор ошибочно считал штатный self zero посторонним событием. CONFUSE2 допускает только этот точный source/target/damage0, все прочие strays остаются fatal.

Строгий reader `tools/arena/extract_observed_confusion.py` проверяет immutable hashes, fresh CRC,4-row matrix,120 samples, positive moves, HP/identity/buff/rank и zero callback. Python5/5 повторены независимым reviewer, CLEAN; свежая CRC extraction равна сохранённому артефакту. Артефакт `.local/lia-port/abilities/confusion-native-observations.json`.

## Подход к дальней цели и UnitOrPoint (protocol17)

CASTAPPROACH1, Campaigns SHA256 `bdaff66c36e8b3078c35a60ae7dac09568e70e56298675616605f6ce3027fa60`, карта3.9c SHA256 `02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34`: 13 isolated native rows измерили I021/I094 Point near200/far1000/farStop0.2, Unit ally/enemy и I06J/A0FI Unit near/far/Stop. Дальний приказ перемещает героя до SPELL_EFFECT/USE и расхода; Stop отменяет эффект и расход в8s окне. Ward Unit передаёт фактические GetSpellTargetX/Y юнита. Native Point API возвращал false даже при завершившемся положительном эффекте; этот bool не использован как критерий отказа. Исходный Acv37261..37285 связывает эти координаты с outer ward placement и matching item-self. Нормализованные наблюдения SHA256 `02c1ef1d4c348d6d199eb57ac050886291eca9c9133495966e39aa8390b11c46`, strict13/13 и22 adversarial PASS, независимое ревью CLEAN. Первоначальный ITEMWARD2 по-прежнему доказывает только profile/lifetime/aura, а прежний Point-only diff выше сохранён как исторический этап.

`OriginalSession.CastApproach` хранит private actor-to-command intent. Pure item/skill providers отделяют eligibility от distance и используют прежние family ranges, текущий skill alias/native rank, bag/slot/exact instance и live Unit/ground item identity. До исполнения нет расхода, cooldown или эффекта. На каждом host tick повторяются проверки; в дальности вызывается ровно один прежний consumer. Pending Item-control очередь повторно планирует приказ после снятия контроля; Cast-only silence не блокирует предметы. Accepted replacement orders, forced taunt, source duel Stop, death/removal и потеря target/resources/instance отменяют старый intent. Неуспешный replacement сохраняет прежний приказ. Unrelated inventory action не отменяет валидный instance.

UnitOrPoint добавлен хвостом enum3; Protocol17 и `lia39-unity-rules-10` меняют handshake вместе. Codec разрешает этот mode только I021/I094, hero/summon/прочие item modes не расширены. Mouse сначала выбирает видимого Unit, затем terrain Point; повторное нажатие ward выбирает явный matching item-self. Queue не сериализуется: уже существующий Move/destination показывает подход на клиентах.

Declared500/700 и прочие family ranges сохранены. Достижимый standoff, bounded16-angle search, .8 preferred radius, private collision/path routing, moving targets и transfer поведения на остальные targeted items/hero/summon skills являются derived host policy. Native13 stationary H008L50 rows не измеряли private buffers, препятствия, stun+approach, другие item families или skill far orders. Измеренные effect distances не объявлены новой универсальной формулой cast range. In-range consumers сохраняют свои placement/target rules. Полностью недостижимый standoff отклоняется без изменения приказа или расхода.

Portable public42/42 PASS после meaningful baseline far7/8 RED; existing item-use/Archer/provider соседние проверки и actual Unity mouse/network/build проверяются отдельно. Отдельный valid horizontal Archer approach обнаружил округление tiny negative facing в360; root разрешил canonical [0,360) guard. Узкая baseline facing mutation дала2/7 RED, current cases прошли. Артефакты `.local/lia-port/cast-approach-check/`, native prerequisite `.local/lia-port/ward-point-contract-review/native-findings.md`. Полная готовность Player здесь не заявлена.

Независимое queue review воспроизвело ещё две P1: exact outer standoff округлялся за strict range, а accepted direct UseWell сохранял старый intent. Focused3cases дали2RED; root разрешил numerical interior margin max(range,abs(targetXY))*1e-12 и адресную отмену только acceptedUseWell. Declared range не расширен, rejectedUseWell и unrelated inventory действия сохраняют прежний intent. Current42/42 portable PASS; .local/lia-port/cast-approach-check/peer-red.json сохраняет прежний симптом.


World arrivalEpsilon1e-7 может остановить actor на800 при target1600.00000005, поэтому interior margin теперь max(1e-6WC,scaled1e-12). Focused quantizer1/1RED и final42/42GREEN, independent boundary+well CLEAN. Это numeric host stability, declared/native range не изменён.



## Последние native/source границы - 2026-10-06

**ITEMPOINTUNIT1, native:** обе строки I02C/A08A (Unit и Point) дали фактические EFF1/USE1 и расход400MP; primary native damage callback равен0. Unit API вернул1, Point API0 при завершенном касте. Strict2/2 и18 adversarial tests прошли. Campaigns SHA256 `7308a2fba33bcb77a86303b3952a866234eb5e91d70e691c50b8f854b4d0e975`, normalized SHA256 `50cbfea79853fe73092ea604e084470b029b30f97ab3dcfee198cfc6cb23d63e`; локальная проверка и границы: `.local/lia-port/item-point-unit-check/verification.md` (не входит в Git). Source K8/l8:22062..22080 использует GetSpellTargetX/Y; jZ600 отсутствовал в native probe. Узкий [disconnected AI bridge](../unity/Assets/Arena/Core/Original/OriginalSession.DisconnectedActions.cs) передает координаты текущего выбранного enemy в существующий Point consumer при исходном selector radius700. Ручная Point metadata сохранена; far/moving targets, private geometry и полный source AI остаются derived/неизмеренными.

**hL, source и portable:** B0B1 либо A19O добавляет один общий +0.2 spell power (2975..2978); их совместное наличие не дает +0.4. B0AS умножает положительный magic vamp на2 после A19O halving и до A19M×0.7 (3008..3013). [Consumer](../unity/Assets/Arena/Core/Original/OriginalSession.ItemSpellEffects.cs) использует существующие AIrg state и A17O carrier с исходным6s lifecycle. После публикации13/13 portable прошли; локальные доказательства: `.local/lia-port/item-spell-buff-draft/review-notes.md` (не входят в Git). Это source-path transfer и controlled Core, не новый native замер комбинаций buff.

Результаты native и portable этого блока отделены от actual Unity/Player. Последующий I02C AI diff, PlayMode и build проверяет root; их текущий финальный verdict здесь не заявлен. GOLDHARPY1 и предел native bounty описаны в [боевой матрице](lia39-combat-implementation.md).
