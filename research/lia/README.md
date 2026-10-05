# Данные исследования Life in Arena

Дата: 2026-10-05. Начните с [сводки механик и баланса](../../docs/lia-mechanics-balance.md).

Для дальнейшей реализации обязательны [правила применения](../../docs/lia-data-use.md), [результат независимого ревью](../../docs/lia-review.md) и проверка [snapshot](review/reviewed-snapshot.json). Выгрузки содержат производные сторонние данные и фрагменты кода, а не только собственные аналитические тексты.

## Оригиналы

- Warcraft 3.4: [Life_in_Arena_v3_4.w3x](../../.local/research/lia/warcraft/Life_in_Arena_v3_4.w3x).
- Warcraft 3.9c: [Life_in_Arena_v3_9_c.w3x](../../.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x).
- Dota: [ZIP фиксированного commit](../../.local/research/lia/dota2/012fab34e8c84ad0aa73cd4eadde1736e3c9df29.zip) и [распакованный snapshot](../../.local/research/lia/dota2/LiA-012fab34e8c84ad0aa73cd4eadde1736e3c9df29/).

Пути к оригиналам действуют на этом ПК. `.local/` исключена из Git. Происхождение/размеры/hashes записаны в manifests, поэтому отсутствие оригиналов на другом ПК нельзя подменять утверждением о воспроизводимости до повторной загрузки.

## Каталоги

| Версия | Герои | Предметы | Способности | Противники/прочие units | Рецепты |
| --- | --- | --- | --- | --- | --- |
| Warcraft 3.4 | [CSV](warcraft/3.4/selectable-heroes.csv) | [CSV](warcraft/3.4/items.csv) | [CSV](warcraft/3.4/abilities.csv) | [CSV](warcraft/3.4/units.csv) | [CSV](warcraft/3.4/recipes.csv) |
| Warcraft 3.9c | [CSV](warcraft/3.9c/selectable-heroes.csv) | [CSV](warcraft/3.9c/items.csv) | [CSV](warcraft/3.9c/abilities.csv) | [CSV](warcraft/3.9c/units.csv) | [CSV](warcraft/3.9c/recipes.csv) |
| Dota snapshot | [Карточки](dota2/heroes.md) | [Карточки](dota2/items.md) | [Карточки](dota2/abilities.md) | [Карточки](dota2/units.md) | [Деревья JSON](dota2/recipe_trees.json) |

Warcraft CSV `abilities` и `units` содержат все восстановленные определения, включая встроенные/вспомогательные; только `selectable-heroes` отражает отдельный исследованный реестр выбора. `HP`, `realHP`, `manaN`, `realM` - поля исходной SLK, не независимый замер итоговых значений в матче. Поля, не записанные в кастомке, могут наследоваться от движка.

## Формулы и доказательства

- [Dota: 456 комбинаций наград по числу участников, сложности и волне](core/dota_round_economy.csv).
- [Dota: параметры Spawn боссов для 1..8 героев](core/dota_boss_scaling.csv).
- [Dota: XP_TABLE и дополнительные атрибуты на 50 уровнях](core/dota_progression.csv).
- [Dota: источники, полнота и ограничения](dota2/validation.json); [manifest](dota2/manifest.json).
- Warcraft: [3.4 manifest](warcraft/3.4/extraction-manifest.json), [3.9c manifest](warcraft/3.9c/extraction-manifest.json), [3.4 каталог-проверка](warcraft/3.4/catalog-validation.json), [3.9c каталог-проверка](warcraft/3.9c/catalog-validation.json).
- Warcraft: [изменения полей баланса 3.4 -> 3.9c](warcraft/version-balance-diff.csv), [полное сравнение](warcraft/version-field-diff.json), [сводка сравнения](warcraft/version-diff-summary.json).
- [Чтение и классификация всех оставшихся безымянных блоков Warcraft](unnamed-archive-audit.json).
- Profile конфликты: [3.4 повторы](warcraft/3.4/profile-duplicates.json), [3.4 между файлами](warcraft/3.4/profile-conflicts.json), [3.9c повторы](warcraft/3.9c/profile-duplicates.json), [3.9c между файлами](warcraft/3.9c/profile-conflicts.json). Полный журнал `_assignments` в `profiles.json`; merged values не доказывают engine precedence.
- [Изолированный повтор Warcraft/core](review/reproduction.json); повтор Dota указан в `dota2/validation.json`.
- [Общая независимая проверка целостности](validation-summary.json).
- [Реестр внешних первоисточников](review/source-register.json).

В папках каждой Warcraft-версии: `all-object-fields.csv` для точечных запросов, `hero-abilities.csv`/`item-abilities.csv` для связей, `recipe-edges.csv` для графа компонентов, `jass-functions.json` для поиска функции/вызовов/rawcodes, `jass-balance-operations.json` для изменений состояния, `profiles.json` для gameplay constants и строк. Все значения обеих версий хранятся раздельно.

В Dota: `parameters.csv` и `parameter_levels.csv` для числовых рядов, `ability_links.csv`, `kv_lua_links.csv`, `special_consumers.csv` для связи данных с кодом, `lua_functions.json`/`lua_calls.json`/`lua_expressions.json` для пути исполнения. Индексы выражений не являются готовым симулятором Lua и не доказывают runtime-reachability.

## Как пользоваться

1. Выберите одну версию и найдите героя/предмет по имени или исходному ID.
2. Посмотрите связанные abilities/компоненты, затем все уровни нужного параметра.
3. Перейдите к source path/line или offset; проверьте вызывающий trigger/modifier, а не только tooltip.
4. Сопоставьте значение с экономикой, типом урона, условиями стека и размером команды той же версии.
5. Сохраняйте неопределенность у inherited/default/неподтвержденных значений. Не заменяйте ее нулем и не берите число из другой версии без явного решения.

Собственные скрипты находятся в [`tools/research/`](../../tools/research/). Команды воспроизведения каждой платформы и зависимости перечислены в [Dota-отчете](../../docs/lia-dota2-analysis.md) и [Warcraft-отчете](../../docs/lia-warcraft-analysis.md). Общая проверка запускается из корня проекта: `py -3 -X utf8 tools/research/lia_validate.py`.
