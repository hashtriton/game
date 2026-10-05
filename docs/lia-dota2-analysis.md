# Dota 2 Life in Arena: полный статический каталог

Свежая проверка 5 октября 2026 года. Полный доступный архив [ZLOY5/LiA](https://github.com/ZLOY5/LiA) скачан локально, распакован и разобран без исполнения кода кастомки. Нынешний `master`, проверенный через официальный GitHub API, совпал с прежним snapshot: [`012fab34e8c84ad0aa73cd4eadde1736e3c9df29`](https://github.com/ZLOY5/LiA/tree/012fab34e8c84ad0aa73cd4eadde1736e3c9df29), 30 сентября 2026, 06:06:16 UTC.

Это воспроизводимая база всех доступных определений и индекс исполняемых выражений. Она позволяет изучать цифры по каждому герою, предмету, способности и юниту. Она не доказывает поведение опубликованной версии Workshop и не заменяет матч в Dota. Все динамические условия, унаследованные значения движка и недостающие зависимости ниже обозначены отдельно.

## Что получено

| Слой | Проверенный объем |
| --- | ---: |
| Файлы полного архива репозитория | 2447 |
| Размер ZIP | 85 299 759 bytes |
| Размер распакованных файлов | 189 118 429 bytes |
| KV1-документы в `game/scripts` и addoninfo | 278, ошибок разбора 0 |
| KV3 с именами custom net tables | 1, обработан отдельно |
| Ссылки `#base`/`#include` | 269, все цели найдены |
| Герои с включением из engine-root | 56 |
| Способности | 581 декларация, 580 уникальных ID |
| Предметы | 185: 102 указаны в основном магазине, 66 рецептов, 17 прочих |
| Юниты | 153, включая summons, helpers, декорации и боссов |
| Scalar KV-поля с точными строками | 23 370 |
| Параметры способностей/предметов и прямые числовые ability-поля | 2912 |
| Строки значений по индексам уровней, включая scalar | 4611 |
| Lua-файлы | 683: 680 разобраны AST, 3 синтаксически проблемных файла учтены отдельно |
| Узлы Lua AST | 136 284 |
| Функции / assignments, returns, условия, циклы / вызовы | 3573 / 13 294 / 14 289 |
| Literal Lua-зависимости / ссылки KV на Lua | 405 / 573 |
| Прочтения special values в Lua | 1488 |
| Найденные русские подписи | 732 из 975 деклараций, все 56 героев имеют подпись |

Сырой ZIP и оригиналы находятся только в `.local/research/lia/dota2/`, исключенном из Git. Путь исходников: `.local/research/lia/dota2/LiA-012fab34e8c84ad0aa73cd4eadde1736e3c9df29/`. Полная копия относится к дереву файлов данного commit; история Git, внешние файлы Dota и содержимое Workshop в нее не входят.

SHA256 ZIP: `6a6793b5cddd69b8a453aa6c11b92d8291b9a2698b97716508df961523764afb`. Хэши каждого файла находятся в [source_inventory.csv](../research/lia/dota2/source_inventory.csv), происхождение и URL - в [manifest.json](../research/lia/dota2/manifest.json). Все 2447 распакованных файлов сопоставлены с ZIP по SHA256 и размеру.

## Навигация по деталям

- [Все герои](../research/lia/dota2/heroes.md): имя, ID, начальные статы, приросты, HP/mana/regen, атака, броня, дальность, скорость, слоты способностей, каждая цифра с исходной строкой. Машинные версии: [CSV](../research/lia/dota2/heroes.csv), [JSON](../research/lia/dota2/heroes.json).
- [Все способности](../research/lia/dota2/abilities.md): MaxLevel, RequiredLevel, интервал повышения, mana/cooldown/range/cast point, типы целей и урона, specials, modifier properties, Lua-ссылки. [CSV](../research/lia/dota2/abilities.csv), [JSON](../research/lia/dota2/abilities.json), [значения по индексам уровней](../research/lia/dota2/parameter_levels.csv).
- [Все предметы и рецепты](../research/lia/dota2/items.md): цена, состав, shop membership, passives и активные параметры. [CSV](../research/lia/dota2/items.csv), [JSON](../research/lia/dota2/items.json), [рецепты](../research/lia/dota2/recipes.csv), [деревья сборки](../research/lia/dota2/recipe_trees.json), [магазин с категориями](../research/lia/dota2/shops.csv).
- [Все юниты](../research/lia/dota2/units.md): HP, damage min/max, attack interval/range, armor/magic resistance, speed, bounty, abilities и предварительная роль. [CSV](../research/lia/dota2/units.csv), [JSON](../research/lia/dota2/units.json), [определения для всех 20 волн](../research/lia/dota2/wave_unit_declarations.json).
- [Числовые формулы core balance](lia-dota2-core-balance.md): полный цикл матча, counts, награды, XP, статы, death/revive, дуэли, финальный босс, runes, upgrades, damage filter. [456 сочетаний round/mode/player count](../research/lia/core/dota_round_economy.csv), [масштабирование боссов](../research/lia/core/dota_boss_scaling.csv), [50 уровней](../research/lia/core/dota_progression.csv).
- [Все Lua-функции](../research/lia/dota2/lua_functions.csv), [все выражения](../research/lia/dota2/lua_expressions.json), [вызовы](../research/lia/dota2/lua_calls.json), [инвентарь и достижимость](../research/lia/dota2/lua_inventory.csv), [Lua-зависимости](../research/lia/dota2/lua_dependencies.csv), [KV -> Lua](../research/lia/dota2/kv_lua_links.csv).
- [KV-потребители specials](../research/lia/dota2/kv_special_references.csv), [Lua-потребители specials](../research/lia/dota2/special_consumers.csv), [связи hero/unit -> abilities](../research/lia/dota2/ability_links.csv), [literal entity references в Lua](../research/lia/dota2/lua_entity_literal_references.csv), [ссылки между KV-сущностями](../research/lia/dota2/entity_kv_references.csv).

Самая полная форма данных - JSON каждой категории: `nodes` сохраняет вложенные modifiers, events, target filters, precache, actions, рецепты, повторяющиеся ключи и исходные строки. [kv_scalars.csv](../research/lia/dota2/kv_scalars.csv) позволяет искать любое поле сразу по всему корпусу. У пути ключа есть индекс повторения `[1]`, `[2]` и далее; это предотвращает потерю повторных SpawnUnit, RunScript, item, precache и одинаковых компонентов рецепта.

## Как интерпретировать числа

`declared` содержит только прямые поля исходника. Отсутствие поля не превращается в ноль. Например, `MaxLevel`, `StatusMana`, `AbilityDamage`, `ItemPurchasable` могут наследоваться из движка или другого класса; такой default здесь не придуман. `typed` является удобным числовым представлением, а исходная строка остается в `nodes.value` и `kv_scalars.raw`.

Числовой вектор `70 90 110` сохраняется как `[70,90,110]`; scalar `8` сохраняется как одно объявленное значение и не размножается по предполагаемым уровням. Вложенные `AbilityValues` с `value`, `RequiresScepter` и другими метаданными сохранены целиком. Lua может переопределить стоимость, число волн, длительность или урон в зависимости от предметов и условий; итоговую стоимость нельзя надежно получить только из поля `AbilityManaCost`.

Пример: у Alchemist Fire Potion объявлены mana `70/90/110`, cooldown 8, damage `30/60/120`, 2 волны; рядом присутствуют отдельные scepter-поля для 4 волн и mana `140/180/220`. Все варианты выгружены, условие применения остается в соответствующем Lua. Источник: [npc_lia_hero_alchemist.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/abilities/npc_lia_hero_alchemist.txt#L3).

`included_from_engine_root=true` означает наличие пути через `npc_*_custom.txt` и `#base`. Это не равнозначно selectable hero, активному слоту, покупаемому item или уже созданному enemy. Например, включенные unused/legacy ability definitions существуют рядом с рабочими. `reachable_from_bootstrap_or_included_kv` - потенциальная достижимость Lua-файла по literal `require`, `LinkLuaModifier`, `ScriptFile`, а не доказательство вызова каждой его функции.

`special_consumers` связывает special с потенциально связанным файлом Lua. Если receiver читает другую способность, связь не является доказанным потребителем именно данного ID. `lexical_conditions` сохраняет окружающие if/else, но не заменяет анализ ранних return, таймеров, callbacks и вызовов из других функций. Встроенные Dota callbacks могут вызываться без явной Lua-ссылки.

## Магазин, цены и рецепты

В основном `shops.txt` 121 строка размещения, но только 102 уникальных ID. Все они относятся к non-recipe definitions. Еще 66 определений - рецепты, 17 non-recipe items в основной список не включены. Эти 17 не следует автоматически объявлять недоступными: среди них 12 рун, четыре варианта `_2` и `item_lia_hyper_boots_old`, которые могут создаваться скриптом или заменой предмета. Точные ID и условия флагов сохранены в item CSV. Источник магазина: [shops.txt](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/shops.txt).

Каждый рецепт сохраняет ID результата, номер альтернативы, повторяющиеся компоненты, их объявленные цены и цену рецепта. Арифметическая сумма не выдается за подтвержденную транзакцию Dota. У девяти рецептов сумма объявленных component costs + recipe cost отличается от `ItemCost` результата:

| Результат | Сумма компонентов и рецепта | ItemCost результата | Разница |
| --- | ---: | ---: | ---: |
| bounty_hunters_crossbow | 2000 | 1900 | +100 |
| demon_edge | 450 | 550 | -100 |
| huge_axe | 525 | 515 | +10 |
| lightning_spear | 885 | 880 | +5 |
| lunar_necklace | 1790 | 1690 | +100 |
| mithril_armor | 1005 | 1045 | -40 |
| staff_of_power | 570 | 790 | -220 |
| spellbreaker | 2475 | 1185 | +1290 |
| totem_of_persistence | 1855 | 1635 | +220 |

Полные ID начинаются с `item_lia_`. Источники и строки каждой строки таблицы: [catalog_validation.json](../research/lia/dota2/catalog_validation.json), `recipes_with_cost_difference`. Это неоднозначность данных для расчетов стоимости сборки/продажи; вывод об exploit или реальной ошибке покупок без движка не делается.

## Подтвержденные расхождения деклараций и обработчиков

### Battle Axe

В KV объявлены цена 585, strength 18, armor 4, bonus damage 16, bash chance 16%, bash damage 80 и stun 1.25. Однако property `MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE` закомментировано. Поэтому число 16 присутствует в наборе значений, но не подключено этим passive modifier. Strength и armor подключены. Источник: [item_lia_battle_axe.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L23).

`OnAttackLanded -> Random` с pseudo-random item basher вызывает `Bash`. Lua пропускает illusion caster. `IsActiveBash` подавляет этот modifier, когда у другого modifier больше bash chance; при равном chance предпочитается созданный позже. В `Bash` создается таблица для 80 magic damage, но `ApplyDamage` или другой вызов применения этой таблицы отсутствует. При успешном допуске вызываются stun modifier на 1.25 и звук. Источник: [items/BattleAxe.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/BattleAxe.lua#L1).

Следовательно, для проектирования нельзя складывать "16 обычного + 80 bash" только на основании KV. Проверено отсутствие вызова в данном файле и отключенная property; фактическая реакция Dota на stun, другие предметы и иммунитеты в матче не проверена.

### Несогласованные имена параметров

- `item_lia_magic_staff`: modifier запрашивает `%bonus_mana_regen_percentage`, объявлено `bonus_mana_regen=0.4`. INT 24 имеет совпадающий ключ; mana regen требует отдельной runtime-проверки. [item_lia_magic_staff.txt:47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L47).
- `16_wave_slow_extreme`: объявлены `attack_slow=-75`, `movespeed_slow_pct=-75`, но modifier ссылается на `%cripple_attack_slow` и `%cripple_movespeed_slow_pct`. Lua `GhostSlow` выбирает длительность для героя/другой цели и применяет этот modifier; совпадение имен не восстанавливает. [extreme_mode_creep_actives.txt:1018](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/abilities/extreme_mode_creep_actives.txt#L1018), [GhostSlow.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/abilities/GhostSlow.lua#L1).
- Всего найдены 7 KV-ссылок `%...` без одноименного direct field или special в этой сущности. Остальные - кандидаты для ручной проверки, не готовый список багов. Ссылка на `%AbilityDamage` при наличии прямого `AbilityDamage` правильно считается разрешенной. [catalog_validation.json](../research/lia/dota2/catalog_validation.json).

`orn_mutant_boss_slow` объявлен дважды, в normal и extreme файлах. Проверенные direct fields и special values совпадают. Дубликат сохраняется как две записи; не утверждается различие баланса и не подменяется предположением о порядке слияния Valve KV. [normal:2418](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/abilities/normal_mode_creep_actives.txt#L2418), [extreme:1409](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/abilities/extreme_mode_creep_actives.txt#L1409).

## Волны и формулы, которые нельзя копировать изолированно

Обычные этапы - 1..4, 6..9, 11..14, 16..19. Для каждого объявлены `{R}_wave_creep`, `{R}_wave_boss` и обе `_extreme` версии. Этапы 5/10/15 используют соответствующий megaboss, 20 - `orn_megaboss` и дополнительные существа. Все их исходные статы выгружены. Light выбирает обычные unit IDs и меняет экономику; extreme имеет отдельные unit definitions. Таблица по имени не включает все поздние summons: их полная исполняемая последовательность разобрана в [core balance](lia-dota2-core-balance.md).

Количество обычных врагов зависит от учитываемых героев: 40/52/64/76/88/100/112/124 + 2 элиты для N=1..8. Раундовые награды дополнительно зависят от N, режима, номера волны и скорости завершения. XP делится между живыми по custom filter. Нельзя сравнивать цены предметов без этой экономики.

Сила, ловкость, интеллект имеют custom коэффициенты; обнаружено различие между `_G` константами и параметрами реально вызываемого API regen, поэтому следует читать [core report, раздел статов](lia-dota2-core-balance.md). Аналогично множитель attack-type/armor-type задается `damage_table.kv`, а damage filtering преобразует engine damage. Промежуточное выражение с `0.05*armor` не является автоматически итоговой формулой брони.

Для будущей самостоятельной игры эти данные полезны как измеряемый набор исходных зависимостей: время до первой сборки, прирост силы между волнами, число целей для AoE, роли контроля и восстановления, награда за скорость, нагрузка поздних штрафов и смена правил на боссах. Они не являются утвержденным дизайном нашего проекта.

## Полнота и открытые границы

Все доступные KV declarations разобраны. В Lua 680 файлов прошли синтаксический AST-разбор; три остальных содержат видимые ошибки и не были автоматически исправлены:

| Файл относительно `game/scripts/vscripts` | Наблюдение | Найденный путь загрузки |
| --- | --- | --- |
| `abilities/7_wave_damage_block.lua` | Идентификатор начинается с `7` | Нет в literal graph |
| `abilities/9_wave_incorporiety.lua` | Идентификаторы `2_wave_cenraurs_revenge` начинаются с цифры | Нет в literal graph |
| `heroes/Hermit/modifier_hermit_decrepify.lua:23` | `self:GetParent:GetAttackType()` | Нет; actual `hermit_astral.lua` связывает modifier со своим же файлом |

Это ошибки текста снимка, а не основание объявить всю опубликованную игру нерабочей. Старые файлы могут не загружаться. Состояние проверяемо через [lua_inventory.csv](../research/lia/dota2/lua_inventory.csv) и [lua_validation.json](../research/lia/dota2/lua_validation.json).

Найдены 4 отсутствующие цели `LinkLuaModifier` (Archmage shooting star cooldown, Earth Lord split earth thinker, Spider Queen infection debuff, Demonologist ritual status effect) и 1 отсутствующая KV `ScriptFile` для `stats_bonus_fix`. Сохраняются также 5 несовпадений регистра путей. Данные перечислены с источниками в [lua_validation.json](../research/lia/dota2/lua_validation.json). Это известные ограничения локальной полноты зависимостей; достижимость конкретного callback и реакция движка отдельно не доказаны.

В графе 628 Lua-файлов потенциально достижимы из bootstrap или включенных KV. Остальные не объявляются мертвым кодом: карты VMAP, строковая конкатенация, внешние Dota defaults и динамическая загрузка могут добавлять пути. Бинарные VMAP/ресурсы сохранены и инвентаризированы, но геометрия, world triggers и поведение карты не декодированы полностью. Нет runtime-проверки pathfinding, AI timing, псевдослучайного распределения, stack order, конечного урона, очередности engine filters или покупок.

Steam Workshop [407750024](https://steamcommunity.com/sharedfiles/filedetails/?id=407750024) напрямую связывает игру с этим GitHub и описывает 53 героя. Число 56 относится к включенным определениям снимка; из разницы нельзя вывести число доступных героев актуальной публикации. Пакет Workshop не скачивался и не сравнивался с commit.

В корне репозитория не найдено общей LICENSE, GitHub API вернул `license=null`. Разрешение использовать исходный код, названия, модели, текстуры, звуки и другие материалы в нашей игре не установлено. Полные оригиналы изолированы в `.local`, выгрузки предназначены для исследования и самостоятельного проектирования.

## Воспроизведение и проверка

Рабочая папка: `C:\Users\Netes\Desktop\proj\game`.

```powershell
# При отсутствии локального parser-deps: официальные PyPI wheels, pinned версии.
py -3 -m pip install --only-binary=:all: --target .local/research/lia/dota2/parser-deps -r tools/research/dota_requirements.txt
py -3 -X utf8 tools/research/dota_extract.py
py -3 -X utf8 tools/research/dota_lua_extract.py
py -3 -X utf8 tools/research/dota_catalog.py
py -3 -X utf8 tools/research/dota_validate.py --reproduce
py -3 -X utf8 tools/research/dota_test_validator.py
py -3 -X utf8 tools/research/lia_core_balance.py
```

KV-parser использует standard library. Lua-parser - [luaparser 4.2.0](https://pypi.org/project/luaparser/4.2.0/) из официального PyPI, установлен только в `.local`; зависимости зафиксированы в `dota_requirements.txt`. Source files читаются как данные, Lua никогда не исполняется. ZIP доступен по immutable codeload URL из manifest, исходные API-ответы и pip report сохранены рядом с локальным архивом.

[validation.json](../research/lia/dota2/validation.json) подтверждает полноту распаковки, KV fixtures, сохранение duplicates/comments/vectors/source lines, все 975 entity ID source lines, независимые samples статов/рецептов, отсутствие применения battleaxe damage и повторяемость результатов. Полный повтор трех стадий дал одинаковые SHA256 у 50 артефактов. Отдельный regression-тест подтвердил, что ошибка subprocess остается FAIL даже при неизменившихся hashes. `extraction_checks_passed=true` означает корректность выгрузки и проверок; `runtime_verified=false` сохранено явно.
