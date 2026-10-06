# Nightmare3.9c: дефекты исходных волн22/29

Исследован 6 октября2026 исходник3.9c SHA256 `02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34`. Карта и исследовательские snapshots не переписываются. Исправление Unity должно отдельно обозначаться как derived исправление исходного дефекта, а не доказательство native parity всей кампании.

## Причина

Флаг Gn классифицирован правильно: `UZ/wZ15103..15170` задает пресеты по восьми flags, `M419049` сбрасывает Gn, а `Qc==4` включает его на19076. Других присваиваний Gn нет. Поэтому устранение сбоя через изменение пресета меняло бы сам Nightmare.

`EMv31728..31814` берет localDL=Vr. Сохраненный spawn counter начинается с1 и на четвертом tick достигает5. При Gn он временно переключает roster на следующую волну; исходная ветка исключает24 и перескакивает мегабоссов после4/9/14/19.

- На22-й волне получается DL23/u00L (`Er23:19955`). Проверка специальной ветки23 выполнена раньше подмены, поэтому Elv не вызывается и cocoon timer не запускается. u00L содержит A0K4, который исключает его смерть из counter.
- На29-й получается DL30. Er30 нигде не присваивается и равен0. `OW12796..12800` напрямую вызывает CreateUnit, без fallback.
- `Ezv32006` заранее резервирует `nC*3+bC`, где `nC=V4()=3*Cr+10` (`18400..18405`). `E7v32078..32080` допускает лишь Player11 non-illusion deaths без A0K4; `Xvv32129..32135` уменьшает bD только по этой ветке. Регистрация gA находится на86510..86514.

Для трех участников это19 ticks и четыре подмены по три позиции, то есть12 отсутствующих eligible deaths. При партиях2/6 последний tick сам является проблемной подменой. Простое удаление unresolved guard оставляет незавершаемый бой.

## Native endpoints

NGATE1 сохранил1474 исходных payload; script/listfile/attributes заменены только в локальном исследовательском экземпляре. В Warcraft1.26 выполнены пять строк:

| Native case | Created/type | A0K4 | Actual deaths | E7v eligible deaths |
|---|---|---|---|---|
| u00L | 1/u00L | 1 | 1 | 0 |
| literal rawcode0 | 0/0 | 0 | 0 | 0 |
| hfoo control | 1/hfoo | 0 | 1 | 1 |
| n023 | 1/n023 | 0 | 1 | 1 |
| n0AU | 1/n0AU | 0 | 1 | 1 |

Это прямые engine getters и death callbacks, вызванные KillUnit. bD не записывался и completion не подделывался. EMv/Ezv/Xvv оригинального матча здесь не запускались; полный deficit остается source/static-path выводом. Отдельного обычного прохождения Nightmare3.9c этим probe не доказано.

Immutable Profile5 capture: `.local/lia-port/research-map/cache-captures/20261006T131412085865Z-583ba3a9b946/parsed.json`. Campaigns SHA256 `583ba3a9b946c4cb37ce7e34ad8a1fe62ebeeb5d45364d1aadc2a61f468e9ec4`; probe map `df6ee2e5965247b39cff092c6ee6ad210a26e17f36b8b5242433eeea08ae4222`; script `fc96ce8a9cbebcf9ad402dfdac9598b67f8fde955b251e8f1bb366d31aa9b18c`.

Strict admission: `tools/arena/extract_observed_nightmare_gates.py`, тесты `test_extract_observed_nightmare_gates.py`,6/6 PASS. Свежий CRC parse и source/probe/script hash сверены; невалидные null actors, owner, death event, A0K4 filter, meta и matrix отклоняются. Локальный normalized output `.local/lia-port/abilities/nightmare-endpoint-observations.json`, SHA256 `55910da14cc0bb3e9e851e0c1ff93516dd74596da01816ed1487061d313a78ec`.

## Граница допустимого исправления

Наиболее близкий исходнику ремонт сохраняет реальные roster IDs и расписание: u00L22 остается обычным source spawn без Elv и не считается погибающим участником волны; null Er30 не превращается в выдуманного юнита. Бюджет уменьшает недоступные eligible deaths. Проверка окончания должна обработать last-invalid-tick без поддельного EnemyKilled и не пропускать запланированные valid spawns.

Нельзя молча создавать вместо Er30 Орна/другого крипа, регистрировать22 cocoon через Elv с выдуманным hatch budget или объявлять исходную карту исправленной. Разница counter budget явно является Unity derived override. Нужны тесты всех партий1..8, current source spawns, counted=false cocoon, отсутствие null entity, wave completion и последующий полный controlled Nightmare campaign. Этот документ фиксирует source/native evidence; реализация и финальный результат кампании проверяются отдельно.

Опубликованная коррекция прошла независимое review CLEAN и свежие97/97 portable Match cases: `.local/lia-port/world-options-check/nightmare-counter-independent-green.json`. InitialDeathBudget меняется только в двух исходно сломанных Nightmare waves; публичная baseline CountedEnemies не переписана. У u00L нет выдуманного hatch timer. Пропущенный Er30 сохраняет шесть координатных RNG draws. Pending spawn ticks не допускают преждевременный transition на последней невалидной попытке у партий2/6. Флаг difficulty и другие roster branches сохранены.

Отдельный controlled Nightmare3 campaign сообщил30/Won,12duels,4455codec checks и0Halt/0wire failures. Это проверка штатных consumers с явными fixture interventions, не обычное прохождение или native whole-map parity.

В [списке изменений3.8](https://lifeinarena.ucoz.ru/forum/6-7046-1) описана смесь монстров текущей и будущей волны. Это более ранняя версия, которая не подтверждает исправление пограничных веток22/29 именно в3.9c.
