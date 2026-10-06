# LiA3.9c: параметры матча, проклятия, аколит и колодец

Рабочий срез от 6 октября 2026 года. Это перечень реализованных потребителей исходных правил и доказательств, а не процент готовности игры или подтверждение полного обычного прохождения.

Исходник: карта3.9c SHA256 `02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34`, локальный `war3map.normalized.j` в `.local/research/lia/warcraft/3.9c/extracted/`. Unity использует собственный C#, существующие модели и производные параметры. Оригинальные Warcraft JASS и map payload остаются исследовательскими материалами и не входят в runtime сборку.

## Параметры матча

`OriginalDifficulty.Custom=0` соответствует Qc=0. Классификатор сравнивает восемь survival flags из `wZ:15155..15173`. Выбор героев, возврат в центр и компактные лавки не меняют сложность. `M4:19047..19072` задает Custom стандартные множители золота/опыта, без hard overlays. Алтарь после мегабосса соответствует стандартной ветке с условием всей живой группы (`19119..19130`). Public OriginalMatch regression проходит цикл до пятого раунда и проверяет доход, опыт и алтарь.

Защитные LTbr в Standard защищены от атак противников, но герой может прорубать проход. Rune и explosive LTex consumers описываются также в item/combat отчетах.

## Девять проклятий

| ID | Source JASS | Потребитель Unity |
|---|---|---|
| A19L | iEv27205..27232, A1AO/A1AN | Armor companion, снятие Bspe до повторного расчета скорости, cap250 |
| A19M | ibv/iIv27233..27274, A19U | Move-only net5s вне13 исключенных областей; независимый таймер дает четыре порции3%maxMP |
| A19N | icv27276..27317 | Каждый подходящий союзник в250 теряет2%maxHP один раз за тик, без оружейного damage event |
| A19O | hL2971..3003 | Scripted damage+20%, universal backlash10% и половина vamp |
| A19P | FL/fL2934..2969 | Инверсия vamp восстановления через существующий центральный обработчик |
| A19Q | iDv27318..27353 | Дневной треугольник потери/восстановления HP,480s на сутки |
| A19R | iYv/iHv27357..27383,27608..27621 | I09D блокирует slots2..5, A19V/A0V2 magic immunity; смерть Player11 unit с userData2 освобождает один slot |
| A19S | iJv27384..27418, A19W | Native mask8 блокирует items, не skills,3s; scripted magic damage15 врагам в150 |
| A19T | iKv27419..27443 | Смерть героя убивает других живых героев с тем же проклятием вне PvP |

Назначение использует `iSv27528..27587`: первые восемь проклятий уникальны, у больших групп ровно двое получают A19T. Современное назначение происходит при Start с отдельным детерминированным RNG; приватный RNG Warcraft и анимация выбора не воспроизводятся.

Дуэльное событие amount0 удаляет marker и его companions. Активность требует level>0. Новые периодические эффекты прекращаются на прежнем получателе; новый получатель получает overlay и эффект. Уже доставленные net/silence и независимый mana timer сохраняют свою длительность. Codec разрешает только curse rank1.

Уровень доказательств: source/static-path и derived consumers. A19U учитывает target mask без invulnerable; script mana timer остается даже при отказе native order. Отдельного native A19U/invulnerability измерения нет. HeavySoul выражает снятие haste и итоговый cap, а не отдельный ledger native item ability removal A07N/A01Y/A13G; stacking других native movement families остается transfer. Дневная фаза начинается с6 при Start, исходное время меню не воспроизводится.

## Награда аколита

Отдельные QH/PH/sH pools учитывают min(eventDamage,prelife) канонического героя (`11117..11137`), урон по Player11 units с owner источника, включая призванных (`11360..11406`), и канонический SPELL_EFFECT с буквальным `Mr or (cr and hK(id))` (`11107..11115`). Строгое `>` оставляет самый ранний slot при равенстве. Нулевые pools также выбирают slot1; дополнительный alive/connected фильтр не придуман.

`xU11293..11305` дает+2STR лидеру QH,+2AGI лидеру PH,+2INT лидеру sH. `OU11310..11313` откладывает награду на1s. `D418701..18733` возвращается до OU при ожидающей дуэли, поэтому награда выдаётся один раз после всей пары и гладиаторской серии. После completed30 бонуса нет. Authority regression проверяет весь цикл +0/+0/+2. Постоянные атрибуты используют существующую vitality `ratio-nearest-approximation`; native rounding SetHeroStr/Agi/Int отдельно не измерялся.

Эта же source seam применяется к лавке u00E. Открытие обычных shops перед pairs/gladiator не создает аколита и не сбрасывает его stock. После всей серии появляется один свежий shop с отдельной измеренной задержкой stock readiness. Все обычные vendors сохраняют прежние правила доступа; два n0AL вне Wi/Nn остаются доступны в волне. n040 из xU является Aloc кругом, не vendor. u00O за Fn относится к исключенному clan scope.

## Лечебный колодец

`xU11169..11209` создает e00M после первой волны в(-60,-380). Easy пополняет после каждого completed round<30; Extreme после кратных3; Standard/Custom/Nightmare после нечетных. Серия дуэлей пополняет только после последнего перехода. Ambt имеет запас2000, Area400 и transfer ratios1/1.

Пять own-script native probes сохранили1474 исходных payload, заменяя только script/listfile/attributes.59 строк включают e00M днем/ночью, положительный stock emow контроль, оба ресурса, near-full redistribution, реальные XY и три героя. `tools/arena/extract_observed_well.py` проверяет source/probe/script hash, schema/matrix, свежий CRC parse, rawcodes, order admission, дебет, стабильность XY/max/resources всех замеров и пассивное восстановление по far controls.

Runtime Warcraft1.26 подтвердил: e00M не восстанавливает ману днем/ночью; stock emow ночью восстанавливает примерно1.25MP/s. При обоих дефицитах запас20 делится10HP+10MP. Неиспользованная половина перенаправляется другому ресурсу; дефицит2/2 тратит4 и оставляет16. Все H008/N0A0/H024 с native body24 лечатся на425, но не на426. `IsUnitInRange(well,hero,400)` совпадает с admission, а point range predicates отличаются. Значение неизвестного caster body не выдумывается.

Единый `HealingWellActorRange` используется прямым UseWell и InteractWell. Для трех измеренных героев reach425; для других органических тел применяется явно заявленный transfer Area400+authored recipient radius. Правый клик авторитетно проводит героя внутрь радиуса и повторно проверяет владельца, жизнь, тип, дальность и ресурсы. Mechanical/structure units не допускаются. Protocol16 добавляет OriginalWellView с present/position/mana/maxMana и строгую codec validation.

Каждый rechargeoff вернул0. Поэтому выключенный autocast не заявлен: default/private recipient selection отдельно не измерялся. Source e00M auto field не добавляет Ambt. Original spawn/refill callback в own-script probes не запускался; его проверка основана на source и authority regression.

Локальная native сводка: `.local/lia-port/abilities/well-observations.json`, SHA256 `d5e4544fd939b22b4d1e2d36170553564f294ee79f24124f9980ca71a1676734`. Immutable Campaigns captures перечислены в нормалайзере и не входят в сборку.

## Проверка рабочего среза

- Portable Core/codec focused27/27: `.local/lia-port/world-options-check/cold-invulnerability-green.json`. RED сохранены до исправлений OU cadence, inactive curse, haste recalculation, invulnerable net и observed reach.
- Дополнительный source P1 лавки u00E: RED перед pairs, затем30/30 вместе с тремя `OriginalAcolyteShopTests`, `.local/lia-port/world-options-check/acolyte-shop-duels-green.json`.
- Лавка независимо reviewed CLEAN с source D4/O4/Q3; fresh3/3, `.local/lia-port/runes-barrels-check/shop-independent-review-green.json`. Root также подтвердил узкий diff без изменения доступа обычных shops.
- Python strict admission/adversarial14/14: `py -3 -X utf8 -m unittest discover -s tools/arena -p test_extract_observed_well.py -v`.
- Native Warcraft:59/59 well rows завершены, source hashes и свежий CRC parse сверены. Это не ordinary campaign run.
- Независимое review: critical Curses/RoundBonuses/Well/sharedhooks/Inventory CLEAN; focused27/27, соседние interactions72/72 и отдельные source repro в `.local/lia-port/curse-review-check/`.
- Независимое strict Well review CLEAN после двух findings:14/14 и59 native строк, `.local/lia-port/runes-barrels-check/well-independent-strict2.txt`.
- Полный Python `tools/arena` discovery:422/422 PASS,135.74s, `.local/lia-port/world-options-check/python-arena-final.json`. `lia_validate.py`:21 проверки PASS,76JSON/61CSV/363350rows,22.08s, `lia-validate-final.json`. В `tools/research` нет `test*.py`, поэтому discovery сообщил0 и не считается пройденным набором.
- Actual Unity UI, общий набор, build и network smoke проверяются отдельно оркестратором; portable tests не подменяют их результаты.
