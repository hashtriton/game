# Повторная волна23: исходный BD и пустые rect

6 октября2026 исследована карта3.9c SHA256 `02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34`. Источник `Elv31696..31727` увеличивает глобальный BD и выбирает один из восьми local rect. BD не сбрасывается между попытками волны. После восьмой позиции local array slot не задан.

Прежний Unity guard останавливал достижимую третью попытку wave23 у партии3, хотя оригинал выполняет native getters и продолжает timer. Для проверки создан COCOON1 в локальной исследовательской папке, без изменения исходной карты, Unity Assets и ее проверенных snapshots.

Native Warcraft1.26,3/3 строк:

| Source BD после Elv | Actual X/Y | Paused | Реальный первый EKv callback | Timer remaining/mC |
|---|---|---|---|---|
| 8 | -1984/576 | 1 | 1 | 29/40 |
| 9 | 0/0 | 1 | 1 | 29/40 |
| 10 | 0/0 | 1 | 1 | 29/40 |

Прямые `GetRectCenterX/Y(null)` вернули0/0; положительный kV control вернул(-1984,576). После первого callback позиции и пауза не изменились, saved unit reference совпал с измеряемым u00L. Таким образом, исходное поведение после позиции8 не является неизвестным циклом или новой случайной точкой.

Тела оригинальных Elv/EKv/tK/OW/XW перенесены в fixture буквально, с заменой только namespace и BJ last-created-unit имени. Диагностический timer wrapper вызывает оригинальный EKv с тем же GetExpiredTimer. Строгая проверка обращает замену имен назад и сверяет byte SHA каждого тела с исходником. Cleanup отменяет timer и удаляет actor только после записи всех observations. Полный30s hatch, одновременные overlap actors и оригинальный whole match в этом probe не измерялись.

Raw meta.method унаследовал общее описание scaffold с SetHeroLevel/no map triggers; это не описание фактической процедуры COCOON1 и не основание для admission. Основание - byte hashes source bodies, диагностический wrapper и точная matrix. cD=4 является явной настройкой fixture, не измеренным результатом полного hatch. Raw capture не переписан.

Immutable Profile5 capture: `.local/lia-port/research-map/cache-captures/20261006T133939791009Z-eca62e2d6bf4/parsed.json`. Campaigns SHA256 `eca62e2d6bf451533aeae4fb684a2141bb25b196c539d9f7d9377c39ab0e1231`; probe map `94b1cdd55f8d7f303bed4ec885bf8d62631254bbd981eb7161e3c26f9b252d04`; script `64bce21ad3ac1436bd4c9e27b8434ba430ca9cc8d43f75ab1844da820656f7d6`. Все1474 не-script payload оригинала сохранены.

`tools/arena/extract_observed_cocoon_retry.py` проверяет свежий CRC parse, hash цепочку, matrix/schema, исходные тела функций, rawcode/A0K4, BD, null/positive rect controls, координаты/паузу до и после callback и сохраненный timer state. Adversarial тесты отклоняют цикл старых rect, reset BD, отсутствие callback, подмену source body/actor и неверную длительность.7/7 PASS.

Normalized local artifact: `.local/lia-port/abilities/cocoon-retry-observations.json`, SHA256 `ba17c75dbb9348ac406586dc7d7a03c0adfba617799fb126026af95c36e97520`. Native(0,0) fallback подтвержден для этих source paths. Unity altar retries, hatch budget и фактическая world placement проверяются отдельно при исправлении прежнего guard; оригинальные Warcraft functions не входят в игровую сборку.

Опубликованный Match diff независимо reviewed CLEAN. Свежие105/105 portable cases проходят: `.local/lia-port/world-options-check/cocoon-counter-independent-green.json`. Проверены реальные death/altar retry transitions, постоянный index, BD8/9/10, callback30→29, destroy/hatch budgets в cohorts4/6 и безопасный host integer overflow. Переполненный index является host robustness test, не native измерением огромного BD. Ранее исправленные Nightmare22/29, fast-last-tick и RNG consumption regressions также входят в этот набор. Actual Unity world/scene integration проверяется оркестратором.
