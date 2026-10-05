# Warcraft 3.4: навигация по данным

Это статическая выгрузка карты с SHA256 `525e3dfdf2d3926bfcaa92582d751dea03b19c458f6dbf6004e5c5f7b76ed63a`. Полный разбор путей и ограничения: [отчёт](../../../../docs/lia-warcraft-analysis.md). Не смешивать с соседней 3.9c.

| Задача | Файл |
| --- | --- |
| Все начально выбираемые герои | [heroes.md](heroes.md), [selectable-heroes.csv](selectable-heroes.csv) |
| Способности каждого героя, уровни, числа, runtime references | [hero-ability-ledger.md](hero-ability-ledger.md), [hero-abilities.csv](hero-abilities.csv) |
| Предметы и тексты | [items.md](items.md), [items.csv](items.csv), [items.json](items.json) |
| Крафт и повторяющиеся ингредиенты | [recipes.md](recipes.md), [recipes.json](recipes.json), [recipe-edges.csv](recipe-edges.csv) |
| Shop object -> inventory object | [shop-conversions.csv](shop-conversions.csv) |
| Вспомогательный реестр цен JASS | [item-price-registry.csv](item-price-registry.csv) |
| Волны, обычные/усиленные враги, мегабоссы | [waves.md](waves.md), [waves-default.csv](waves-default.csv), [unit-roster-assignments.csv](unit-roster-assignments.csv) |
| Тексты описаний волн | [wave-texts.json](wave-texts.json) |
| Все units/items/abilities/upgrades/buffs | Одноимённые `.json` и `.csv` |
| Каждое явное поле с происхождением | [all-object-fields.csv](all-object-fields.csv) |
| Исходные SLK/profile/binary записи | [slk.json](slk.json), [profiles.json](profiles.json), [objects.json](objects.json), [strings.json](strings.json) |
| Глобальные параметры | [gameplay-constants.csv](gameplay-constants.csv) |
| Функции, вызовы, callbacks и rawcodes | [function-index.csv](function-index.csv), [jass-functions.json](jass-functions.json) |
| Кандидаты числовых операций и все массивы | [jass-numeric-expressions.json](jass-numeric-expressions.json), [jass-balance-operations.json](jass-balance-operations.json), [jass-array-assignments.csv](jass-array-assignments.csv) |
| Проверка чтения и полноты | [extraction-manifest.json](extraction-manifest.json), [reader-validation.json](reader-validation.json), [catalog-validation.json](catalog-validation.json) |

`line` для SLK/profile обозначает начало исходной записи/секции, не обязательно точную строку конкретного поля. `offset` для binary означает байт в распакованном member. JASS lines относятся к нормализованному файлу в `.local/research/lia/warcraft/3.4/extracted/`; исходный script там же в `scripts/war3map.j`. Readable rawcodes-копия имеет те же номера строк.

Пустые значения не заменяют неполученные defaults движка. `realHP`, `realM`, `mindmg1/avgdmg1/maxdmg1` могут быть остатками шаблона и не являются фактическими итоговыми статами. `binary_overrides` оставлены с raw field ID/level/data pointer; они накладываются на SLK движком, в выгрузке не подменяются предположениями. Tooltip и explicit fields не гарантируют исполнение соответствующей логики в матче. Статический индекс прямых ссылок не является доказательством отсутствия косвенной JASS-логики.
