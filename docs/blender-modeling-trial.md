# Проверка локального 3D workflow и моделей Codex

Дата: 2026-10-05. Скоуп: исследование стека, затем наковальня как простая локальная проба. Герой не переделывается в рамках этого теста.

## Диагноз стека

Проверены Blender 5.2.2 LTS (d13f752e3b9c), Codex CLI 0.160.0, Blender MCP 1.0.3, MCP SDK 1.30.0 и RTX 2080 Ti 11264 MiB. Наш официальный BlenderLab MCP исполняет Python в Blender. Прямой background CLI использует тот же bpy API. Переход между этими транспортами сам по себе не улучшит форму.

В [.codex/config.toml](../.codex/config.toml) включен CLI execution tool. Локальный Windows-патч MCP добавляет stdin=DEVNULL; проверенное отличие не относится к моделированию. Специализированной text/image-to-3D нейросети в использованных скриптах нет. Концепт не преобразовывался в mesh нейросетью: агент писал геометрию кодом.

В [предыдущем Astra builder](../tools/astra_bust_trial.py) лицо строилось из плотной аналитической оболочки. Волосы, борода, орнамент и броня появлялись до первого рендера. Финальные 7214 объектов и 6088420 evaluated triangles не исправили базовые пропорции. Это проверенная причина сложности контроля формы в нашей реализации, а не доказательство неспособности Astra создавать 3D вообще. Технический PASS старого бюста не являлся художественным приемочным тестом.

## Что показывают найденные примеры

Источники ниже являются самоотчетами авторов, а не воспроизведенными нами benchmark.

| Источник | Описанный автором процесс | Что можно перенести |
| --- | --- | --- |
| [Astra: больничный коридор](https://www.reddit.com/r/OpenAI/comments/1wem6s7/chatgpt_blender_mcp_built_this_from_scratch/) | Автор заявляет Blender MCP без внешних ассетов; отдельно blockout, детали, движение камеры, освещение | Ранние промежуточные проверки, отдельные задачи на геометрию и свет |
| [Корпус Raspberry Pi](https://www.reddit.com/r/ClaudeAI/comments/1sz748q/fully_3dmodeled_raspberry_pi_5_enclosure_made_100/) | bpy, размеры, вырезы, детали, рендер | Простые твердые поверхности подходят для контролируемой пробы; пригодность к печати не доказана |
| [Hoverboard / clockwork](https://www.reddit.com/r/ClaudeAI/comments/1v6pvby/opus_5_is_very_good_at_blender_3d/) | Автор называет официальный MCP и Python | Успешный local-only процесс возможен; заголовок не доказывает превосходство LLM |
| [Might have cracked Blender MCP](https://www.reddit.com/r/ClaudeAI/comments/1vuccmd/might_have_cracked_blender_mcp_for_claude/) | В ответах автор называет Trellis 2 для исходной формы | Такой результат нельзя сравнивать с Blender-only моделированием как одинаковый стек |

Таким образом, дело не сводится к одному секретному MCP. Смешиваются разные задачи: сценография и механические props, органические персонажи, локальное моделирование и импорт нейронного mesh. Найденные публикации не дают контролируемого сравнения Astra/Sol или уровней рассуждения именно для персонажей.

## Модель и уровень рассуждения

- [GPT-6 Astra](https://developers.openai.com/api/docs/models/gpt-6-astra): поддерживается low; это минимальное значение из доступного в нашем клиенте перечня. Предыдущая локальная проба бюста запускалась с high.
- [GPT-6.1 Sol](https://developers.openai.com/api/docs/models/gpt-6.1-sol): low поддерживается, none/minimal не поддерживаются. OpenAI рекомендует сравнивать модели на своих задачах; специфической гарантии качества Blender-моделей эта страница не дает.
- [Reasoning guide](https://developers.openai.com/api/docs/guides/reasoning): меньший effort ориентирован на задержку и расход токенов. Это не заявленный режим улучшения художественного качества. Больше рассуждений тоже не заменяет наблюдение за формой.

Для этой пробы созданы два свежих агента: /root/anvil_astra_low с gpt-6-astra/low и /root/anvil_sol_low с gpt-6.1-sol/low. Это реальные параметры spawn_agent. Общая постановка, рендеры, ревью и отчет выполняются координатором; вся сессия целиком не является автономным запуском одной low-модели. Условия в [brief](../art/tests/anvil-low/brief.md).

## Примененный метод

Основа - [наковальня Blender Guru](https://www.blenderguru.com/posts/2018/1/17/creating-an-anvil-full-series) и [Scenario hard-surface](https://github.com/scenario-labs/skills/blob/main/skills/dcc/blender/scenario-blender-hard-surface/SKILL.md). Прочитаны структура первого курса и текст второй инструкции. Видео целиком не просмотрены. Готовые ассеты, код toolkit и текстуры не скачивались, пакет не устанавливался. Использована адаптация к Blender 5.2, а не заявление о точном прохождении всех видеоуроков.

1. Единое численное задание на силуэт, масштаб и конструкцию.
2. Только крупные объемы; серые виды front, side, top и 3/4.
3. Исправление рога, шейки и опоры до мелких деталей.
4. Отверстия и фаски с проверкой стыков; затем крепления и материалы.
5. Единый свет, крупный план и независимая техническая проверка; художественный вывод отдельно от технических чисел.

Дополнительная полезная инструкция: [reference modeling](https://github.com/colinurbs/claude-modeling-skills). Для будущего героя потребуется согласованный набор видов и отдельная ранняя проверка головы. Данный prop-тест не подтверждает качество органической анатомии.

## Журнал пробы

- Сначала оба агента создали 2 объекта: наковальню и колоду. Общий Cycles renderer реально использовал OptiX / RTX 2080 Ti.
- Первый просмотр четырех видов выявил у обеих реализаций слишком плоский рог. У Astra были ступеньки в месте соединения, у Sol дополнительно выступ опоры за колоду. Координатор запросил уточнение крупной формы до детализации.
- После уточнения крупной формы оба варианта перешли к отверстиям, фаскам, креплениям и материалам. Общий renderer выявил ошибки, не видимые в одной геометрической статистике.
- У Astra в первом финале обнаружены 26 faces без материала и 5 почти вырожденных evaluated triangles. Исправлены material slots и сетка, затем во втором раунде выровнена граница светлого материала у рога. На итоговом снимке этих технических ошибок нет.
- У Sol первый финал прошел технический тест, но рендер показал неверное сглаживание рабочей поверхности около отверстий и ступенчатую границу материала. В одном финальном раунде исправлены normals/material assignment и ослаблена процедурная фактура.

## Проверенный результат

| Вариант | Объекты | Evaluated triangles | Финальные раунды исправления | Техническая проверка |
| --- | ---: | ---: | ---: | --- |
| Astra low | 12 | 11656 | 2 | PASS |
| 6.1 Sol low | 16 | 12946 | 1 | PASS |

У обоих: 0 nonfinite vertices, почти нулевых triangles по порогу 1e-12 м², отсутствующих material faces, boundary/nonmanifold edges, нарушений winding и отрицательных закрытых компонентов. По каждой версии повторно прочитан saved .blend отдельным Blender-процессом. SHA проверки совпадает с SHA пяти общих финальных рендеров. Варианты не являются game-ready ассетами: UV не завершены, export/Unity/анимация не проверялись. Самопересечения не проверяются численным валидатором автоматически.

- [Astra: исходная модель](../art/tests/anvil-low/astra-low/final.blend), [сцена со светом](../art/tests/anvil-low/astra-low/presentation.blend), [рендер](../art/tests/anvil-low/astra-low/previews/final-beauty.png), [QA](../verification/anvil-astra-low-final.json).
- [Sol: исходная модель](../art/tests/anvil-low/sol-low/final.blend), [сцена со светом](../art/tests/anvil-low/sol-low/presentation.blend), [рендер](../art/tests/anvil-low/sol-low/previews/final-beauty.png), [QA](../verification/anvil-sol-low-final.json).

Первый blockout, refined и предыдущие финальные .blend сохранены для сравнения. Сохраняются только текущие общие финальные PNG; старые картинки можно восстановить из снимков.

## Независимая визуальная оценка и рекомендация

Проверяющему переданы точные копии всех пяти PNG в нейтральных папках A/B без имен моделей: A = Sol, B = Astra. Он просмотрел все десять видов и предпочел A как учебный prop: плавнее переход рога, убедительнее металл и дерево. У B отметил более понятные прижимные скобы и читаемые фаски, но также заломы/стянутые блики у основания рога и более пластиковые материалы. У A остаются несколько слишком острые края и регулярные годовые кольца. Блокирующих дефектов для демонстрации учебной пробы не отметил.

Моя рекомендация для следующего простого предмета - 6.1 Sol low с этим же циклом проверок. В данной паре итог Sol предпочтен независимым просмотром и потребовал меньше финальных исправлений. Это не статистический benchmark и не доказательство превосходства Sol для всех 3D-задач. Проба показала работоспособность local-only процесса на простом твердом предмете, но художественное качество героя по-прежнему не подтверждено.

Главное изменение метода: промежуточная форма стала отдельным проверяемым результатом. Новый уровень рассуждения, конкретная LLM и менее сложный объект менялись одновременно относительно старого бюста, поэтому нельзя приписывать улучшение именно low. Следующий тест органической формы следует проводить отдельно, с несколькими согласованными референсами, прежде чем детализировать броню и волосы.

## Воспроизведение

Команды выполняются из корня game. Скрипты и .blend не являются Unity-ассетами с подтвержденным импортом.

```powershell
$blenderTrial = 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe'
& $blenderTrial --background --factory-startup --disable-autoexec --offline-mode --python-exit-code 1 --python tools/anvil_astra_low.py -- --stage blockout
& $blenderTrial --background --factory-startup --disable-autoexec --offline-mode --python-exit-code 1 --python tools/anvil_sol_low.py -- --stage blockout
& $blenderTrial --background --factory-startup --disable-autoexec --offline-mode art/tests/anvil-low/astra-low/blockout.blend --python-exit-code 1 --python tools/anvil_trial_render.py -- clay
& $blenderTrial --background --factory-startup --disable-autoexec --offline-mode --python-exit-code 1 --python tools/anvil_astra_low.py -- --stage final
& $blenderTrial --background --factory-startup --disable-autoexec --offline-mode --python-exit-code 1 --python tools/anvil_sol_low.py -- --stage final
& $blenderTrial --background --factory-startup --disable-autoexec --offline-mode art/tests/anvil-low/astra-low/final.blend --python-exit-code 1 --python tools/anvil_trial_render.py -- present
& $blenderTrial --background --factory-startup --disable-autoexec --offline-mode art/tests/anvil-low/astra-low/final.blend --python-exit-code 1 --python tools/validate_anvil_trial.py
& $blenderTrial --background --factory-startup --disable-autoexec --offline-mode art/tests/anvil-low/sol-low/final.blend --python-exit-code 1 --python tools/validate_anvil_trial.py
```

Renderer общий для обоих вариантов. Он не перезаписывает загруженный .blend; SHA проверяется до/после. Вариант, файл и режим меняются явно. Режим present сохраняет отдельный presentation.blend для просмотра в GUI; prepare делает это без повторного рендера. Builder перезаписывает только свой выбранный этап: перед повторной сборкой сохраняйте собственные ручные правки в другой файл. Успех повторяемой команды не утверждается без нового запуска.
