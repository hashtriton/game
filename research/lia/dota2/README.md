# Dota LiA: исследовательские выгрузки

Источник, выводы и ограничения: [docs/lia-dota2-analysis.md](../../../docs/lia-dota2-analysis.md). Формулы экономики, статов, боя и финала: [docs/lia-dota2-core-balance.md](../../../docs/lia-dota2-core-balance.md).

- [Герои](heroes.md), [способности](abilities.md), [предметы](items.md), [юниты](units.md): читаемые каталоги всех деклараций с русскими подписями и исходными строками.
- Одноименные `.json`: все direct fields, nested nodes, specials и metadata. `.csv`: плоские direct fields для таблиц.
- `kv_nodes.json` и `kv_scalars.csv`: вся структура KV1 и каждое scalar-поле с путями повторений; `kv3_net_tables.json`: отдельный список net tables.
- `ability_values.csv`, `parameters.csv`, `parameter_levels.csv`: specials и поля уровней без выдуманных defaults. `vector_index` является индексом объявленного вектора, а не доказанным runtime-level.
- `shops.csv`, `recipes.csv/json`, `recipe_trees.json`: торговые категории, все 66 рецептов и деревья компонентов.
- `wave_unit_declarations.json`: определения юнитов для каждого из 20 этапов. Реальные counts/scaling/spawn conditions - в core balance, а не в эвристике имени.
- `include_graph.json`, `kv_lua_links.*`, `lua_dependencies.*`, `lua_inventory.*`: графы потенциального подключения. Literal reachability не доказывает активность.
- `lua_functions.*`, `lua_expressions.json`, `lua_calls.json`: полный AST-индекс функций, присваиваний, return, ветвлений, циклов и вызовов для 680 успешно разобранных Lua. Три ошибочных текста учтены явно в validation.
- `special_consumers.*`, `kv_special_references.csv`, `ability_links.csv`, `entity_kv_references.csv`, `lua_entity_literal_references.*`: ссылки и кандидаты на потребителей параметров.
- `manifest.json`, `source_inventory.csv`, `*_validation.json`, `validation.json`: происхождение, SHA256 и результаты сверки.

Все source paths относительны `.local/research/lia/dota2/LiA-012fab34e8c84ad0aa73cd4eadde1736e3c9df29/`; все line numbers однобазовые. Данные UTF-8. Отсутствующий default означает неизвестное значение движка, а не ноль. Raw strings авторитетнее convenience `typed`. Повторные ключи/ID сохранены. Доступность предмета в матче и `ItemCost` транзакции не выводятся из одного shop flag/рецепта.

Оригиналы и зависимости инструментов находятся только в ignored `.local`; исходный игровой код не исполнялся. Общая лицензия на повторное использование исходных материалов не найдена. Воспроизведение: команды в основном отчете; при полной проверке используйте `dota_validate.py --reproduce`.
