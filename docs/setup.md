# Состояние настройки

Ниже сохранены текущие результаты и история проверок настройки с 5 октября 2026 года. Самостоятельная Windows-сборка доступна для запуска и проверки. Полнота обычного прохождения и границы переноса описаны в [статусе игры](lia39-port-status.md).

## Итоговая Windows-проверка 6 октября 2026 года

Unity Editor `6000.6.4f1`, CLI `1.0.0-beta.12`, Pipeline `0.8.0-exp.1`. Итоговый код использует protocol 17 / `lia39-unity-rules-10`. Сцена `Lia39Arena` включена в самостоятельную сборку. Запрошенный skill `ui-ux-pro-max` установлен в проект и применен; новые модели отложены.

| Проверка | Фактический результат | Локальный артефакт |
| --- | --- | --- |
| Полный Unity EditMode, фильтр Original | 1426/1426 PASS, 683.3 с; до последнего узкого I02C AI-моста и информационной подписи | `.local/lia-port/unity-original-rules10-release.json` и `.xml` |
| Затронутый AI после последней правки | 32/32 PASS, 22.04 с | `.local/lia-port/unity-ai-items-rules10-final.json` и `.xml` |
| Полный Unity PlayMode на итоговом Player-коде | 26/26 PASS, 104.71 с | `.local/lia-port/unity-playmode-rules10-player.json` и `.xml` |
| Windows64 Player | `Succeeded`, 156155236 байт суммарного build output, 26792 мс, 0 ошибок, 1 ожидаемое предупреждение `Pipeline Disabled in Player` | `.local/lia-port/unity-build-rules10-final.json` |
| Самостоятельное приложение | Два процесса, Knight/Archer, localhost:17739, готовность/старт и общий исход; одиночный перезапуск, покупка, расход маны навыка, отметка приказа движения, меню и выход | Нативный smoke в готовом `builds/windows/Arena.exe`; итог описан в [отчете](lia39-overnight-progress.md) |

Локальный ZIP `builds/Arena-Windows-2026-10-06-rules10.zip` создан: 46763783 байта, SHA256 `bd80a14aa368b7e7d4fc511e2ad7a3824df087015ee59163cdde1d204e9aa0b8`. Бинарные сборки и `.local` не входят в Git; из клона репозитория запускайте Unity-проект по [инструкции](playing.md). Внутри ZIP находятся приложение, `Build-info.json`, инструкция и семь файлов лицензий; source files/хеши отмечены в Build-info. Полный результат упаковки хранится в `.local/lia-port/windows-package-final.json`.

Статическая проверка распакованного архива сравнила 204 файла с актуальной папкой сборки: все SHA256 совпали, расхождений и запрещенных файлов нет, Mono/D3D12 и семь лицензий присутствуют. Артефакт - `.local/lia-port/windows-package-validation.json`. GUI-запуск из распакованной папки отдельно не повторялся.

Эти результаты подтверждают сборку и проверенные маршруты. Два физических ПК, обычные 30 волн до финала и все частные распределения/тайминги native Warcraft не проверены. Исторические записи ниже относятся к своим срезам и не заменяют итоговые результаты.

## Проверка 6 октября, 08:40 MSK

- Unity Editor6000.6.4f1, CLI1.0.0-beta.12 и Pipeline0.8.0-exp.1 сохранены. Новый полный прогон после protocol7 пока не запускался: агенты интегрируют gameplay. Последний широкий Unity результат736 EditMode и9 PlayMode относится к предыдущему срезу и не переносится автоматически на новые файлы.
- Через bundled `Editor/Data/DotNetSdk/dotnet.exe run --project` повторно проверены BossWorld70/70, ItemScripts/Recipes10/10 и AttackMove3/3. Артефакты: `.local/lia-port/boss-world-check/image-hooks.json`, `.local/lia-port/item-script-check/recipes-first.json`, `.local/lia-port/attack-move-check/green.json`. Это portable исполнение тех же Core файлов, а не новый визуальный Unity smoke.
- Protocol9 добавляет адресную цель предмета и игровые команды быстрой сборки/атаки с движением. Изменения UI магазина требуют следующей реальной проверки GameView. Standalone сборка этого среза отсутствует; прежний исполняемый прототип не заменен.

## Инструменты и подтверждённые операции

| Компонент | Версия / результат | Доказательство |
| --- | --- | --- |
| Unity Editor | `6000.6.4f1`; проект `unity` открыт, Pipeline сообщает `ready` | [ProjectVersion.txt](../unity/ProjectSettings/ProjectVersion.txt), наблюдательная команда `unity status --project-path C:\Users\Netes\Desktop\proj\game\unity --format json --non-interactive` |
| Unity CLI | `1.0.0-beta.12` | `C:\Program Files\Unity Hub\resources\unity.exe --version` |
| Unity Pipeline | `0.8.0-exp.1` | [manifest.json](../unity/Packages/manifest.json) |
| Unity batch smoke | Компиляция и сохранение нейтральной сцены из трёх объектов прошли | [unity-batch-smoke.json](../verification/unity-batch-smoke.json); локальный лог `verification/unity-batch-smoke.log` исключён из Git |
| Unity CLI compile check | `unity recompile` завершился `success=true`, `up_to_date`, `compiling=false`, ошибок и предупреждений 0 | [unity-cli-recompile-smoke.json](../verification/unity-cli-recompile-smoke.json) |
| Unity MCP | `initialize`, `tools/list` и чтение иерархии сцены прошли через вспомогательный Python JSON-RPC клиент, вызванный Codex | [unity-mcp-hierarchy-smoke.json](../verification/unity-mcp-hierarchy-smoke.json) |
| Blender | `5.2.2 LTS`; создание сцены, сохранение `.blend`, экспорт GLB и повторный импорт прошли через CLI | [blender-smoke.json](../art/smoke/blender-smoke.json), [blender_smoke.py](../tools/blender_smoke.py) |
| Blender MCP | Официальный пакет `1.0.3` с локальной Windows поправкой; MCP SDK `1.30.0`, Python `3.14.2`. Повтор `initialize → tools/list → tools/call` прошёл: версия Blender и три объекта прочитаны | [blender-mcp-read-smoke.json](../verification/blender-mcp-read-smoke.json); [первая ошибка](../verification/blender-mcp-first-error.json) сохранена |
| Codex | Локальный CLI `0.160.0`; конфигурация Unity/Blender загружена из реального cwd `game` | `codex mcp list --json`, `codex mcp get blender --json` |
| Unity аккаунт/лицензия | Существующий вход: `loggedIn=true`, `sessionState=fresh`; лицензия `Unity Personal`, `Assigned`. Новый вход и активация не требовались | Безопасный итог `unity auth status` и `unity license`; идентификаторы аккаунта не сохраняются |
| Локальная сессия Codex | «Проверить стек Game» видит 6 нативных Unity tools и 1 Blender tool; прямой `mcp__unity__editor_status({})` прошёл: `ready`, `compiling=false`, `domainReloadInProgress=false` | [native-session-verification.json](../verification/native-session-verification.json) |

MCP настроен только для `game` через [проектный config.toml](../.codex/config.toml), транспорт `stdio`. Unity Pipeline слушает loopback `127.0.0.1:7800`. В конфиге Unity MCP разрешены шесть инструментов чтения. Blender MCP разрешает `execute_blender_code_for_cli` с запросом перед выполнением Python. Пути в конфиге и launch-файлах относятся к этому ПК; на другом ПК их нужно адаптировать.

Отдельный профиль Blender и фоновый процесс не являются sandbox операционной системы. Python Blender выполняется с правами пользователя Windows.

## Что ещё не подтверждено

- В исходной делегированной durable сессии каталог новых tools не обновлялся. Его custom JSON-RPC отчёты сохраняют `native_codex_tool_catalog_verified: false`. Отдельная новая локальная сессия Game действительно видит оба сервера и успешно вызвала Unity native tool. Дополнительный Blender Python вызов в ней не выполнялся: два отдельно разрешённых теста проведены через проверочный MCP клиент.
- После локального исправления тот же Blender MCP тест прошёл. [Проверка stdin без Python](../verification/blender-stdin-repro.json) сама зависания не воспроизвела; подробности исходного Windows механизма основаны на совпадающем [issue 58](https://projects.blender.org/lab/blender_mcp/issues/58), а не на отдельной трассировке внутри Blender.
- Фактический показ диалога подтверждения Blender Python в локальной Codex сессии не проверялся дополнительным исполнением. В конфиге выставлены `default_tools_approval_mode='prompt'` и per-tool `approval_mode='prompt'`; это не отменяет необходимость согласовывать каждый следующий Python вызов.
- Сборка игры, Play Mode, игровой прототип и проверка пользовательского сценария: на этом этапе они не выполнялись.

## Воспроизведение и локальные файлы

Исходники Unity, настройки проекта, тестовые сцены, собственные smoke-скрипты и безопасные JSON-отчёты включаются в репозиторий. `Library`, `Temp`, `Logs`, `.local`, виртуальные окружения, лицензии, токены и логи остаются вне Git согласно [.gitignore](../.gitignore).

Установленная документация Unity CLI в `.agents/skills/unity-cli` и отдельные файлы `*-mcp-catalog.json` исключены из Git. Протокольные smoke-отчёты содержат также каталог server tools; project allow-list в конфиге ограничивает доступные Codex инструменты. Навыки устанавливаются из официального Unity CLI командой `unity skill install codex --local` из каталога Unity проекта после проверки справки установленной версии. Зависимости Blender MCP находятся в локальном окружении; его содержимое в репозиторий не переносится.

Официальные инструкции: [Unity CLI и замена старого MCP](https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli), [Unity CLI / Pipeline](https://unity.com/blog/meet-the-unity-cli), [BlenderLab MCP](https://www.blender.org/lab/mcp-server/), [BlenderLab](https://www.blender.org/lab/).

## Начать работу в локальном Game

В боковой панели Codex откройте проект **Game** и задачу **«Проверить стек Game»**. Она уже создана в `C:\Users\Netes\Desktop\proj\game`, открыта штатной навигацией и получила нативные MCP tools. Продолжайте работу в ней или создайте обычную новую локальную задачу в том же проекте. Прочитайте `AGENTS.md` и документы контекста перед реализацией игры.

Для терминала на этом ПК:

```powershell
Set-Location -LiteralPath 'C:\Users\Netes\Desktop\proj\game'
& 'C:\Users\Netes\AppData\Local\OpenAI\Codex\bin\f544b3844e0f14e9\codex.exe' mcp list --json
& 'C:\Users\Netes\AppData\Local\OpenAI\Codex\bin\f544b3844e0f14e9\codex.exe'
```

Важно запускать CLI из реального каталога `game`: в этой версии `-C game` при запуске `mcp list` из другого cwd не подхватил project config. В проверенной локальной сессии конфиг загрузился без ручного изменения trust. Если другой клиент показывает запрос доверия проекту, владелец должен осознанно подтвердить именно эту папку; агент не меняет trust/security автоматически. После изменения MCP конфига начните новую локальную сессию либо штатно перезапустите MCP/клиент. Уже запущенная durable сессия и локальная Game сессия имеют разные каталоги инструментов.

Unity Editor должен быть открыт для `game\unity`. При необходимости:

```powershell
& 'C:\Program Files\Unity Hub\resources\unity.exe' open 'C:\Users\Netes\Desktop\proj\game\unity'
& 'C:\Program Files\Unity Hub\resources\unity.exe' status --project-path 'C:\Users\Netes\Desktop\proj\game\unity' --json
```

Blender GUI для выбранного CLI MCP не обязателен. Codex запускает STDIO сервер на время сессии, а подтверждённый Python tool запускает отдельный background Blender. Blender add-on, GUI preferences, Windows автозапуск и bridge сервер не устанавливались. Вызов CLI tool сначала пробует loopback `127.0.0.1:19876`; в тестах listener отсутствовал. Публичный порт и HTTP транспорт не используются. Проектный профиль отделяет настройки, но не ограничивает права Python на диск и сеть.

## Локальное исправление Blender MCP и откат

Официальный BlenderLab MCP `1.0.3` скачан из ссылки на [официальной странице](https://www.blender.org/lab/mcp-server/), bundle SHA256 `d6fe04dd17767f7c7fb452142f1f0e6787db575db49f3976a166142683b5b97e`, исходный commit `2cea8d566dde07fbac28a61d698909d69724e853`. Требуется Blender 5.1+. Пакет установлен в `game\.local\blender-mcp-venv`, исходники в `.local\blender-mcp-source`.

Это **локально изменённый официальный 1.0.3**, не официальный новый релиз. В `blmcp/tools_helpers/blender_cli.py` добавлена только строка `stdin=subprocess.DEVNULL` для child Blender. Оригинальный файл и установленная копия до изменения совпадали; исходный SHA256 `e77e7b967d021f817a19d9269d3bf44068559aed7487c326eab8c7f3ab1187a3`. Patch и hashes находятся в [blender-local-patch.json](../verification/blender-local-patch.json) и [patch-файле](../tools/blender-mcp-windows-stdin.patch). Повтор того же чтения версии/объектов после поправки прошёл. Проверочный клиент сохраняет промежуточные результаты и безопасный stderr, его timeout 150 секунд больше server timeout 120.

Для отката сначала завершите использующие этот MCP локальные сессии Codex. В двух каталогах `.local\blender-mcp-source\blmcp\tools_helpers` и `.local\blender-mcp-venv\Lib\site-packages\blmcp\tools_helpers` восстановите `blender_cli.py` из соседнего `blender_cli.py.official-v1.0.3.bak`, проверив исходный hash выше. Общую конфигурацию и текущий Blender пользователя менять не нужно. После отката первоначальный Windows timeout может вернуться; новое официальное обновление следует сначала проверить на том же отдельно согласованном smoke test.

Минимальный проект создан из `com.unity.template.3d` на уже скачанной стабильной ветке 6.6. Это техническая проверка Built-in render pipeline, не окончательный выбор графической архитектуры: Built-in помечен deprecated с Unity 6.5. Перед развитием прототипа следует выбрать поддерживаемый render pipeline, например URP, и подходящую LTS ветку; отдельный проект-дубликат для этого не нужен. CLI и Pipeline остаются beta/experimental.


## Разработка через C# и CLI

Unity MCP сейчас предоставляет только шесть инструментов чтения. Полное изменяющее управление редактором через MCP не включено. Codex может менять собственные C# файлы в `unity/Assets`, помещать Editor автоматизации в `unity/Assets/Editor` и вызывать официальный CLI или UnityEditor API через разрешённый shell. Эти операции проходят обычные approvals клиента; security, sandbox и MCP allow-list для них расширять не нужно. Не используйте `unity mcp configure codex`, если его план предлагает ослабить sandbox.

Реально проверено: Codex записал [GameSetupSmoke.cs](../unity/Assets/Editor/GameSetupSmoke.cs), Unity скомпилировал его, batch `-executeMethod GameSetupSmoke.Run` создал Cube, Camera и Light и сохранил `Assets/Smoke/SetupSmoke.unity`; лог содержит маркер успеха и exit code 0. Затем официальный CLI проверил компиляцию текущих файлов:

```powershell
& 'C:\Program Files\Unity Hub\resources\unity.exe' recompile --project-path 'C:\Users\Netes\Desktop\proj\game\unity' --timeout 120 --json
```

Результат последней команды `up_to_date` означает, что текущие скрипты уже скомпилированы; доказательство выполнения собственного C# метода находится в batch smoke отчёте. После новых правок проверяйте фактические compile errors и статус, а не только успешный запуск CLI.

Для повторения batch smoke сначала сохраните работу и закройте Editor **этого** Unity проекта. `GameSetupSmoke.Run` пересоздаёт нейтральную тестовую сцену. Уже проверенная команда:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'C:\Users\Netes\Desktop\proj\game\unity' -executeMethod GameSetupSmoke.Run -logFile 'C:\Users\Netes\Desktop\proj\game\verification\unity-batch-smoke.log'
```

Официальный `unity run <project> -- -executeMethod <Class.Method>` также описан установленным навыком, но этот вариант wrapper отдельно не запускался. Для открытого Editor доступны Pipeline команды через `unity command`; схема берётся из живого сервера до вызова. Изменяющие CLI команды выполняются как отдельные согласованные действия, независимо от read-only MCP.

Следующие стадии доступны в официальном CLI, но **ещё не выполнены**: `unity test <project> --mode EditMode --output <report>` и `unity build <project> --target StandaloneWindows64 --output-path <exe>`. Проверены справка CLI и присутствие Pipeline `run_tests` / `build`; это не доказательство прохождения тестов или выпуска сборки. Перед batch test/build сохраните и закройте тот же Editor; для сборки нужны выбранные сцены/build settings и установленный модуль целевой платформы. Test suite и игровая логика пока не созданы.

## Переработка первого героя, 5 октября 2026 года

Первая модель «Рубеж» технически сохранялась и экспортировалась, но пользователь отклонил ее художественное исполнение и потребовал удалить. По прямому поручению удалены каталог `art/heroes/rubezh`, три связанных скрипта создания/рендера/проверки и три JSON-отчета. Они не являются текущими ассетами или доказательством готовности новой модели. Исходный smoke-файл, Unity и общие настройки не изменялись.

Нативный Blender MCP был реально вызван в этой сессии через `mcp__blender__execute_blender_code_for_cli`: чтение версии `5.2.2 LTS`, создание, сохранение и экспорт через отдельный background Blender проходили. Этот факт подключения не подтверждает художественную приемку модели. Открытый GUI с несохраненной работой пользователя не затрагивался.

Подготовлен [новый концепт тяжелого бойца](../art/concepts/2026-10-05-heavy-warrior-v2.prompt.md) встроенным imagegen с панелью №2 принятого листа как стилевым ориентиром. Это растровое изображение, не 3D. Meshy 7 присутствует в read-only каталоге подключенного Higgsfield, но инструмент `generate_3d` отсутствует в текущем доступном каталоге tools. Попытка запросить стоимость через `estimate_image_cost` отклонена как неверный тип модели, задание не отправлено. Chrome на `https://higgsfield.ai/3d-jutsu` показывает Login. Генерация, списания, новые входы и загрузка концепта в этот сервис пока не выполнялись.

Пользователь прямо отказался от Meshy/Higgsfield: работать только локально в Blender. Внешняя генерация не запускалась. Через локальный background CLI создан [рабочий черновик veteran](../art/heroes/veteran/README.md) и рендеры Cycles/OptiX на RTX 2080 Ti. По просьбе пользователя файл открыт отдельным GUI-процессом; название окна `veteran.blend` подтверждено. Прежнее окно с несохраненной работой не затронуто.

После просмотра пользователь отклонил качество и несоответствие концепту. Подготовка к выдаче и экспорту остановлена. Открытый `.blend` после открытия не перезаписывался, поздние изменения скриптов пока не интегрированы в новый файл. `verification/veteran-build.json` и `veteran-review.json` относятся к промежуточным снимкам; последующая подготовка студии изменила hash открытого исходника. GLB, риг и интеграция Unity не выполнены. Нельзя считать техническую сохранность или рендер доказательством готовности героя.

## Локальная проба Astra, 5 октября 2026 года

По явному поручению пользователя отдельный агент `gpt-6-astra` создал пробу головы и нагрудника средствами установленного Blender. Внешняя генерация и скачанные ассеты не использовались. Сохранены [astra-bust.blend](../art/heroes/astra-bust-trial/astra-bust.blend), четыре финальных PNG и [описание ограничений](../art/heroes/astra-bust-trial/README.md). Несколько диагностических проходов исправили дефекты глаз, кожи, волос и непрерывности головы. Итоговое сравнение референса с реальными портретом, профилем и общим видом все равно не подтвердило нужный художественный уровень. Проба не является готовым игровым героем.

Независимая команда проверки повторно открыла финальный файл:

```powershell
& 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe' --background --factory-startup --disable-autoexec --offline-mode art/heroes/astra-bust-trial/astra-bust.blend --python-exit-code 1 --python tools/validate_astra_trial.py -- '01 Sculpt and eyes' '02 Groom' '03 Forged cuirass' '04 Mantle and collar'
```

Результат: exit 0, 7214 объектов геометрии, 6088420 evaluated triangles, 0 почти вырожденных треугольников по принятому порогу, 0 нечисловых координат и пропущенных материалов. [Отчет](../verification/astra-bust-review.json) содержит bounds, пороговую проверку и hash. Исходник SHA256 `BAEAF8F309BB33949D1EC4E36C0D5B0BCFCF7235192D87EB74D5DD0DEFF8F65F` не изменился при проверке. Все четыре Cycles/OptiX рендера созданы из этого снимка, логи: `.local/astra-bust/render-final.log`, `.local/astra-bust/independent-check-final.log`. `py_compile` четырех скриптов прошел. Исходный открытый `veteran.blend` остался с прежним hash `9BD3C7452C4A1D57AA0F6D5894856819BD46AA35CCF92C40D22C09B03711EEE8`.

Новая проба открыта отдельным GUI-процессом Blender для просмотра. Старые окна и файлы не заменялись. UV, ретопология для игрового бюджета, риг, анимации, экспорт и Unity в эту пробу не входили.

## Проверка метода моделирования и low-режима, 5 октября 2026 года

По новому поручению сначала исследованы локальный стек, публичные примеры и официальные документы моделей, затем выполнена [проба наковальни](blender-modeling-trial.md) двумя свежими агентами: `gpt-6-astra/low` и `gpt-6.1-sol/low`. Минимальный поддерживаемый в клиенте уровень - `low`. Общая постановка и независимые проверки выполнялись координатором; тест не сравнивает low и high на одном задании.

Подтверждены Blender 5.2.2 LTS, Codex CLI 0.160.0, MCP 1.0.3 / SDK 1.30.0. Настройки и пакеты не менялись. Новая геометрия создавалась локально через отдельные background CLI-процессы с `--factory-startup --disable-autoexec --offline-mode --python-exit-code 1`; внешние сервисы и готовые ассеты не применялись. Общие рендеры Cycles реально использовали OptiX / RTX 2080 Ti.

Сначала выполнены blockout и уточнение формы, затем финал с несколькими видами и исправлением выявленных артефактов. Повторное независимое открытие обоих `final.blend` с `--python tools/validate_anvil_trial.py` завершилось exit 0. Отчеты: [Astra](../verification/anvil-astra-low-final.json), [Sol](../verification/anvil-sol-low-final.json). Итог: 12 объектов / 11656 triangles у Astra, 16 / 12946 у Sol. У обоих отсутствуют найденные nonfinite-координаты, вырожденные triangles по порогу 1e-12 м², пустые material faces, boundary/nonmanifold edges и нарушения winding. Hash исходников совпадает с hash пяти финальных ракурсов, source unchanged.

Логи текущих подтвержденных запусков: `.local/anvil-low/astra-low/final-r2-render.log`, `.local/anvil-low/sol-low/final-r1-render.log`, оба `final-independent-qa.log`. Исходники - `tools/anvil_astra_low.py`, `tools/anvil_sol_low.py`; общий рендер - `tools/anvil_trial_render.py`, проверка - `tools/validate_anvil_trial.py`. Команды воспроизведения и найденные инструкции приведены в отчете.

Для GUI сохранены отдельные `presentation.blend` с общим студийным светом. Astra presentation открыта процессом PID 32120, Sol presentation - PID 40628; названия обоих окон с полными путями и Responding=true подтверждены. Рендеры просмотрены через view_image; интерактивное вращение мышью в GUI не проверялось. Старые открытые veteran и astra-bust файлы сохранили прежние SHA256, несохраненное состояние GUI не затрагивалось.

Это статичные учебные props. Полная UV-развертка, экспорт, Unity, игровые материалы, collision mesh и анимация не проверялись. Численный валидатор не доказывает художественное качество и не ищет все самопересечения. Готовность героя не подтверждена этим тестом.

## Новая модель рыцаря, 5 октября 2026 года

По следующему поручению создан [knight-v3](../art/heroes/knight-v3/README.md), полностью локально в Blender 5.2.2 LTS. Выбрана новая геометрия и закрытый шлем; прежние файлы героя сохранены. Работа проходила через серую сборку, несколько ракурсов, независимое визуальное ревью и исправления. Шлем как отдельный компонент подготовлен агентом `gpt-6.1-sol/low`, тело и интеграция - координатором. Это не отдельный сравнительный тест моделей.

Исходники: `tools/knight_build.py`, `knight_shapes.py`, `knight_helmet.py`, `knight_render.py`, `knight_validate.py`. Использовались отдельные background CLI-процессы с `--factory-startup --disable-autoexec --offline-mode --python-exit-code 1`; MCP и настройки не изменялись. [Команды воспроизведения](../art/heroes/knight-v3/README.md#воспроизведение) включают построение, Cycles-рендер, повторное открытие для QA и подготовку сцены просмотра.

Финальный r8 сохранен в `art/heroes/knight-v3/knight.blend`, SHA256 `82b857cc787277a630bece2f2eb90d86d79c1b440dcf008622ddb94d0c2a4474`. Геометрический [отчет](../verification/knight-v3-knight.json): 160 объектов, 105806 evaluated triangles, все проверяемые дефекты равны нулю; отсутствие UV отмечено предупреждением. Сохранены шесть реальных PNG, рендер Cycles действительно использовал OptiX / RTX 2080 Ti. Hash QA и всех шести ракурсов совпадает с финальным исходником; рендер и проверка его не меняли.

Отдельный `knight-presentation.blend` содержит камеру и свет. Его повторный background QA тоже прошел; студийный пол явно исключен из геометрии персонажа. Модель открыта отдельным GUI-процессом Blender, итоговый PID 28676; подтверждены полный путь в названии окна и Responding=true. Интерактивное вращение мышью не проверялось, визуальная оценка выполнялась по рендерам. Старые окна не закрывались; обновлялось только созданное в этом проходе окно новой модели без признака несохраненных изменений.

Логи: `.local/knight-v3/final-build-r8.log`, `final-render-r8.log`, `qa-final-r8.log`, `qa-presentation.log`, `prepare-final.log`. `py_compile` пяти скриптов, проверка локальных ссылок и `git diff --check` прошли. Сохранность прежних исходников подтверждена прежними SHA256: `veteran.blend` - `9BD3C7452C4A1D57AA0F6D5894856819BD46AA35CCF92C40D22C09B03711EEE8`, `astra-bust.blend` - `BAEAF8F309BB33949D1EC4E36C0D5B0BCFCF7235192D87EB74D5DD0DEFF8F65F`.

Результат остается статичной визуальной моделью: качество концепта не достигнуто, кисти и ткань условны, художественная приемка пользователем не получена. UV, игровая ретопология, риг, анимации, экспорт, collision mesh и Unity не выполнены. Валидатор не исключает все самопересечения. Коммит, push, публикация и внешние генераторы не использовались.

## Арена 3.9c в Unity (5 октября 2026)

По новому поручению продолжена локальная игра с готовыми бесплатными моделями. Пользователь отклонил произвольную круглую арену, выбрал Warcraft 3.9c и потребовал извлечь карту из локального оригинала. [Описание и управление](arena-map.md). Параллельные изменения Blender/art сохранены и не входят в коммит арены.

Подтверждено: Unity Editor 6000.6.4f1, Unity CLI 1.0.0-beta.12, Pipeline 0.8.0-exp.1, URP 17.6.0. CLI `self-update --check` подтвердил актуальность; URP выбран через живой Package Manager как совместимый. Native MCP `editor_status` и реальная операция live CLI `eval` выполнены. Действующая лицензия позволила компиляцию, тесты и Windows Player build. MCP настройки этим этапом не менялись.

Источник: локальный Life_in_Arena_v3_9_c.w3x с SHA256 02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34. Полный `py -3 -X utf8 tools/research/lia_validate.py` прошел 21 проверку. Новый extractor находится отдельно в tools/arena; reviewed research файлы не изменены. Чужие mesh/texture/audio/JASS в Unity не импортированы.

Результат: `Assets/Arena/Scenes/Lia39Arena.unity`, Terrain 65x65, исходная WPM 256x256, 1372 отображаемых объекта из 2736 записей и 23 сервисных точки. Невидимые блокировщики и технические эффекты не отображаются. Реальная scene сверка: максимальная ошибка координат исходных roots 0m, ошибка высоты в vertices <0.0003m. 245 исходных размещений бочек показаны готовой CC0 моделью; 94 stock-бочки северо-восточного массива отдельно сверены по бинарным offsets. Модели Kenney Castle/Pirate/Fantasy Town и Quaternius имеют локальные license/source manifests.

Проверки: `unity recompile` без ошибок; `run_tests --mode editor --filter Arena.Tests.EditMode --filter_type assembly` 36/36; `run_tests --mode playmode --filter Arena.Tests.PlayMode --filter_type assembly --async_tests true` 5/5. PlayMode проверяет клавиатуру и ПКМ, способности, паузу/повтор, осмотр карты, реальный размер анимированного героя, проигрыш от врагов и управляемое прохождение 5 волн/босса через actual damage. Последнее является ускоренным интеграционным тестом, не обычным пользовательским прохождением. После исправления обзорной камеры дополнительно прошел сценарий осмотра, паузы и Tab в обе стороны.

Исправления, подтвержденные повторной проверкой: невидимый герой из-за двойного применения skin scale (`BakeMesh(true)`+Transform matrix), удары через стены, застревание у конечной точки ПКМ, отсутствие меню паузы в осмотре, перекрытие южного края HUD, пропавшие UI textures при ортографическом режиме. Обзор использует узкую перспективу; независимая проверка реальных PNG подтвердила все зоны и читаемый HUD.

Артефакты: `verification/arena-map-edit-tests.json`, `arena-map-play-tests.json`, `arena-map-camera-tests.json`, `arena-map-scene-coordinates.json`, `arena-map-build.json`, `lia39-map.png`, `lia39-play.png`. Полный подробный build report хранится локально в `.local/arena-map/`, не в Git. Windows output: `unity/Builds/Lia39Arena/Lia39Arena.exe`. Предупреждение сборки о выключенном Pipeline в Player ожидаемо: управляющий HTTP сервер в игру не включался.

Ограничения: stock bounds и object footprints частично приближены, оригинальная графика заменена CC0, часть мелкого декора/эффектов пропущена. Магазины пока являются объектами окружения. Полные механики 3.9c, разрушение бочек, исходный выбор героев, предметы и экономика не реализованы. Warcraft runtime не проверялся. Сохраненная карта и пробный бой не означают готовности полной игры или ее художественной приемки пользователем.

Финальная Windows сборка `build_319468085129`: Succeeded, 0 errors, 125483784 bytes. EXE запущен, создано отвечающее окно `Arena Map Prototype`, графическое устройство инициализировано, managed exceptions не обнаружены. Player log содержит диагностическую строку `d3d12: failed to query info queue interface (0x80004002).`; она сохранена в отчете запуска. Ввод и прохождение проверялись в Editor PlayMode, отдельно в standalone подтверждены процесс, окно и startup log. Финальные снимки обзора и северо-восточного участка: `verification/lia39-overview.png`, `verification/lia39-barrels.png`. Runtime кадровый счетчик продвигается, финальная консоль Editor без ошибок. Запущенное окно игры оставлено пользователю; Editor остановлен на сохраненной Lia39Arena.


## 2026-10-06: текущий порт 3.9c

Работа идет в существующем `unity` через Unity CLI/Pipeline на loopback 127.0.0.1:7800. Editor 6000.6.4f1. `unity recompile --project-path .../game/unity --format json` после подключения экипировки и A05M завершился с 0 errors / 0 warnings. Реальный Play до этой компиляции подтвержден изменением frameCount 20973 -> 21021; контрольный бой записан в `.local/lia-port/choke-range-live.jsonl`, изображение `unity/Assets/Screenshots/choke-range-final.png`. Новый полный набор Original EditMode и сетевой PlayMode запускаются отдельно.

Клиент Warcraft 1.26 используется только для контролируемых локальных замеров исходной карты. CUA/sky управляет предоставленным пользователем окном `Warcraft III`, cache читается отдельным проверяющим parser. Новые модели/текстуры/звуки оригинала в Unity не добавлялись. Измерения выполнены исследовательскими копиями с собственным скриптом; writer проверяет исходный SHA и сохранение 1474 payload. Актуальные границы механик и доказательства находятся в `docs/lia39-port-status.md`. Старая Windows сборка из предыдущего этапа не представляет текущую полноту Original-порта; новую сборку еще не заявляем готовой.

### Проверка 6 октября, 07:22 MSK

Live Unity CLI `run_tests --mode editor --filter Original --filter_type testName --async_tests true` и последующий `test_status`: 736/736 PASS за258.46с. Сюда вошли настоящие маски навигации B009 и сохранение исходных бочек при удалении динамических препятствий. `run_tests --mode playmode --filter OriginalNetworkGameTests --filter_type testName --async_tests true`: 9/9 PASS за14.82с. Полные отчеты: `.local/lia-port/unity-original-protocol7.json`, `.local/lia-port/unity-network-protocol7.json`. Перед запуском исправлены три ошибки тестовой сборки, обращавшейся к internal Copy(); редактор затем сообщил 0 errors / 0 warnings.

После этого прогона добавлены новые boss drivers и типы призывов/иллюзий в еще не выпущенный protocol8. Они проходят сфокусированные portable наборы и независимое ревью, но не входят в указанные736/9. Нужны новый общий Unity прогон, проверка обычного матча, визуальная приемка и новый Windows build. Межмашинная сеть этим локальным PlayMode набором не проверена. Выключение ПК не запланировано, поскольку итоговая игра еще не завершена.

### Дополнение 6 октября, 15:00 MSK

Unity Editor6000.6.4f1, Unity CLI1.0.0-beta.12 и Pipeline0.8.0-exp.1 продолжают работать на локальном loopback127.0.0.1:7800. Предварительный StandaloneWindows64 build завершен успешно: `.local/lia-port/windows-build-status.json`, executable `builds/windows/Arena.exe`, 0errors/1expectedwarning о выключенном Runtime Pipeline. Он не содержит последних protocol16 дополнений и не считается итоговой сборкой.

Два отдельных экземпляра Arena были запущены и через настоящее Windows UI создали совместный матч H008/N0A0. Подтверждены выбор/готовность/start, покупка I007 с вычетом45gold и показ результата поражения обоим игрокам. Логи `.local/lia-port/standalone-host.log` и `standalone-client.log` не содержали runtime exceptions. Клавиатурный native smoke не подтвержден этим UI проходом; actual Input System сценарии отдельно проверены в Unity.

Actual Unity focused suites: Gameplay4/4, Rune14/14, RandomHero18/18. Файлы результатов перечислены в `docs/lia39-overnight-progress.md`. Эти проверки не заменяют окончательный общий прогон, органический матч и новый standalone smoke.

Существующий CC0 KenneyFantasyTown `fountain-round-detail.fbx` назначен полю runtime.wellPrefab, сцена Lia39Arena сохранена через Unity CLI (`.local/lia-port/unity-well-model-binding.json`). Протокол колодца16 и Core consumers завершаются; новый asset моделирования не создавался.

## Иконки магазина в выбранном рисованном стиле - 7 октября 2026

Пользователь выбрал preview в духе Dota 2 и разрешил заменить игровые иконки и показать скриншот. Встроенный imagegen создал 134 самостоятельных рисунка и служебную печать. [Исходные атласы и промпты](../art/icons/dota-inspired/README.md), [соответствие ID](../tools/art/dota-style-manifest.json), [импорт](../tools/art/import_dota_icons.ps1) сохранены в проекте. Из 134 рисунков подготовлены 233 PNG: 13 производных вариантов и 86 рецептов. Рецепты используют рисунок своего готового результата с маленькой печатью; витрина показывает finished displayId. Файлы `unity/Assets/Game/Art/Icons/<ID>.png` остаются 128 x 128 RGBA. Все 233 `.png.meta` сохранены побайтно, все 233 ссылки HudAssets разрешаются.

Команды и операции этой сессии: `powershell.exe -NoProfile -File tools/art/import_dota_icons.ps1 -ValidateOnly`, staging, затем `-Apply`; повторный staging детерминирован для всех 233 SHA256. До замены сохранены и проверены PNG/meta в `.local/art/dota-icons-before/png`, отчет - `.local/art/dota-icons-staging/import-report.json`. Unity CLI 1.0.0-beta.12, Editor 6000.6.4f1, Pipeline 0.8.0-exp.1. Через живой Editor выполнены asset refresh, проверка 233 non-null bindings и `run_tests --mode playmode --filter Game.Tests.HudTests --filter_type testName --async_tests true`: 6/6 PASS, включая actual mouse click, hover tooltip, гайды, покупки и B/G/Escape.

Реальные Game View screenshot с `source=screen` получены при `Screen.width=1920`, `Screen.height=1080`: [Мечи и луки](../verification/shop-dota-style.png), [Базовые](../verification/shop-dota-basic.png), [Артефакты](../verification/shop-dota-artifacts.png). Перед съемкой подтверждено продвижение кадров. На время съемки выбран существующий Full HD preset Game View и остановлено игровое время; после съемки возвращены Free Aspect, timeScale=1, Play завершен. Scene builder и игровые классы не изменялись.

Отдельная находка вне скоупа: в длительном Play после появления экрана поражения зафиксированы `NullReferenceException` в `GameHud.UpdateHeroPanel`, `GameHud.HeroPanel.cs:120` (обновление золота). Конкретная null-ссылка не установлена; обращения к иконкам в этой строке нет. Код не исправлялся. Финальная съемка сделана после свежего Play без экрана поражения; герой 700/700 HP, 240/240 маны.

Windows player пересобран штатным `Game.EditorTools.GameBuild.BuildWindows()` в `builds/game-arena/Arena.exe`. Результат CLI сохранен в `.local/art/dota-icons-staging/windows-build-result.json`; отдельный запуск player после этой сборки не выполнялся. До следующего поручения о commit/push публикация, commit и push не выполнялись.

Последующее поручение пользователя разрешило commit/push набора иконок. В его скоуп входят PNG/meta, атласы, промпты, manifest, импорт, необходимый каталог предметов и скриншоты. Код нового меню и прочих параллельных изменений остается вне этого ассетного коммита. Скриншоты и HUD-тесты относятся к текущему рабочему дереву с новым меню; ассетный коммит сам не добавляет витрину в чистый checkout.

## Дневная арена, срез 1 - 8 октября 2026

Продолжен остановленный после M1 проход на `style/ow2-arena`, baseline `3c8dae4d6350374297e454df11cea1856972b65c`. Использован уже работающий Editor PID13296, второй Editor не запускался. Подтверждены Editor6000.6.4f1, CLI1.0.0-beta.12, Pipeline0.8.0-exp.1, URP17.6.0. Все операции выполнены CLI через существующий loopback127.0.0.1:7800; рабочий каталог `game/unity`. Новые пакеты, сторонние ассеты, credentials и настройки MCP не добавлялись. Git операции записи не выполнялись.

Фактически выполненные проверки, все финальные команды с exit0:

- `py -3 -X utf8 tools/art/gen_daylight.py --validate`, затем `--validate --sets ground cobble`: оригинальные периодические PBR материалы, детерминизм, стыки, normals. Итоговые 27 JPEG1024x1024 занимают2582555байт. Результаты `.local/art/daylight/validation.json`, contact sheet и 2x2 tiling preview просмотрены.
- `unity command recompile --format json`, `unity command menu --format json -- --path "Game/Build Arena scene"`: последняя компиляция0errors/0warnings, пересборка сцены exit0. SSAO активен в Forward+, DepthNormals. Нулевые normals у четырех крон, ivy и banner устранены масштабированием finite differences до Unity normalization; повторная live проверка показала0. Emission keyword всех четырех emissive материалов после сохранения true, GI flags BakedEmissive, без bake.
- `unity command run_tests --format json -- --mode editor --async_tests true --filter Game.Tests --timeout 300`, затем `unity command test_status --format json`:18/18 PASS за1.5с после финальной правки свечения.
- `unity command run_tests --format json -- --mode playmode --async_tests true --filter Game.Tests --timeout 600`, затем `unity command test_status --format json`:27/27 PASS за93.63с после финальной правки свечения.
- Исходный `Right_click_on_a_barrel_attacks_it` воспроизводимо падал: synthetic mouse в(0,0) включал edge pan во время teleport fixture и выводил target за viewport. Исправлен только этот тест: временно выключает edgeScroll и возвращает его в finally. Actual click и строгий Assert.AreSame сохранены, Runtime не изменен. Focused RED0/1, GREEN1/1, затем полный27/27; подробности в локальном REPORT и независимом ревью.
- `unity command menu --format json --timeout 900 -- --path "Game/Build Windows player"`: финальный Player после emission fix Succeeded,0errors,1warning,314119962байт,12.301789с. Output `builds/game-arena/Arena.exe`. Warning: `Pipeline: No RuntimePipelineConfig asset found (Project Settings > Pipeline > Runtime). Pipeline will be disabled in Player builds.` Отдельный запуск этого EXE не выполнялся.
- Первая холодная сборка превысила300с CLI и дала exit6; Editor продолжил работу, BuildReport позже подтвердил Succeeded за467с с ошибкой только этого timeout. Последующий чистый cached build и окончательный build после material fix завершились exit0. Ошибки диагностических CLI/eval вызовов сохранены в command-index, не выдаются за ошибки игрового кода.
- `unity command eval_file --format json -- --file "C:/Users/Netes/Desktop/proj/game/.local/codex-tasks/ow2-arena/verify_scene.cs"`:257destructibles,6explosive,4units,1TerrainCollider, max terrain height error0.000289917м. Layout побайтно равен baseline; независимое сравнение saved scene подтвердило прежние gameplay компоненты, root transforms, camera settings и модельные skeleton/mesh/animation references.
- Отдельный actual Input System smoke F9:130 ->630gold. Полный PlayMode проверяет B/G/Escape, покупки, навигацию, бой, разрушение бочек и угловой массив.
- `unity command capture_game_view --width 1920 --height 1080 --source screen --format json`: три финальных PNG в `.local/codex-tasks/ow2-arena/shots/`; live Screen1920x1080 и продвижение frameCount подтверждены. Только Editor helper временно ставил позы и кадр. `py -3 -X utf8 .local/codex-tasks/ow2-arena/compare.py --final-roi-bottom 867`: PIL comparison и metrics.json. После съемки Play остановлен, timeScale1, Free Aspect восстановлен; Arena.unity сохранена, dirty=false, compiling=false. Финальный `console_status`:0errors/0warnings.

Сборка Unity автоматически добавляет два SSAO resource IDs в runtime cache `UniversalRenderPipelineGlobalSettings.asset`; после завершения подтверждена ровно эта автоматическая разница и восстановлены исходные bytes с import. Постоянная общая конфигурация не менялась. Performance Test Framework автоматически создал и удалил свои Resources; временные InitTestScene исчезли после runner. Источники и механизмы проверены в установленном package code, подробности в локальном ревью.

Полные команды, exit codes и выводы: `.local/codex-tasks/ow2-arena/command-index.md` и `commands/*.json`; размеры новых/бинарных файлов: `new-files.md`, `binary-files.md`, `file-audit.json`. Проверка source/docs `git diff --check` exit0; полный check возвращает2 из-за trailing spaces пустых Unity YAML fields, это не полный PASS. Художественный критерий premium качества выполнен частично: повторяемость мостовой, простая листва/кладка, слабое пламя и временные модели остаются. Новый вид не утвержден пользователем как финальный; прежняя HUD проблема после поражения этим срезом не исправлялась.

## Уточнение дневной арены, срез 1b - 8 октября 2026

После перезагрузки подтверждено отсутствие Editor; процессы `unity.exe mcp` были клиентами. Запущен один Editor6000.6.4f1, PID10820, штатным executable с `-projectPath game/unity -logFile .local/codex-tasks/ow2-arena/editor-1b.log`, окно hidden. Первый editor_status дал PIPELINE_UNREACHABLE во время старта; ready получен после48.424с,0compile errors. Stale UnityLockfile не удалялся, модальные диалоги не потребовались. CLI1.0.0-beta.12, Pipeline0.8.0-exp.1, URP17.6.0, loopback127.0.0.1:7800. Git writes, новые пакеты, downloads, imagegen и Blender не использовались.

Фактически выполненные проверки текущего среза:

- Procedural ground/cobble validation:6JPEG deterministic, short slab0.727-0.992м при repeat8м, long0.859-1.438м. `gen_leaves.py --validate`, `gen_fire.py --validate` и stone validation: alpha padding, finite normals, детерминизм, periodic fire phase и неизменность остальных39images. Evidence `.local/art/daylight/p1-notes.md`, `leaf-validation.json`, `p4-stone-validation.json`, `fire-validation.json`. Масштаб также показан1м grid в `shots/1b/P1-scale-1m.png`.
- `unity command recompile --format json`, `unity command recompile_status --format json`, `unity command menu --format json -- --path "Game/Build Arena scene"`: финальный rebuild exit0,0errors/0warnings. После каждого P1-P8 получен одинаковый кадр и просмотрены comparison sheets; художественные ограничения записаны в progress-1b.md.
- `unity command run_tests --format json -- --mode editor --async_tests true --filter Game.Tests --timeout 300` с `test_status`:18/18 PASS,0failed/0skipped,1.78с. PlayMode equivalent с `--mode playmode --timeout 600`:27/27 PASS,0failed/0skipped,136.06с.
- P8 временно вернул exact original HEAD body `Right_click_on_a_barrel_attacks_it`; один single run на финальной сцене PASS1/1,5.61с. Изоляция edge scroll удалена, весь InputOrderTests.cs равен HEAD. Старый failure не повторился, точный изменившийся фактор Editor focus/device/viewport не установлен. Визуальная причина не доказана.
- `unity command menu --format json --timeout 900 -- --path "Game/Build Windows player"` выполнен ровно один раз: Succeeded,exit0,0errors/1warning,104.7168055с,354942874байт, `builds/game-arena/Arena.exe`. Warning о RuntimePipelineConfig относится к управляющему Pipeline. Отдельный EXE не запускался; Player cutout/shadows проверены сборкой и shader references, но не live render.
- Live audits до/после1b: layout SHA256 `92ad8fdf69931645cfdc658ea921ea180c0a9748ab09395619afe425db3fd0b3` равен,257destructibles/6explosive/4units/1TerrainCollider, terrain heights/camera/unit/barrel contracts сохранены.234used meshes, invalid vertices/normals/tangents0. Leaf alpha test true,cutoff0.42. Исправление декоративного torch parent rotation подтверждено3torch, max offset5.99e-7м и0deg.
- Camera edge sweep:72cases,18вне прежнего terrain, все покрыты outer visual ground при16:9, limits/zoom8,19,46. Collider не добавлен; произвольные aspect ratios не проверены. F9 actual Keyboard down/up:130 ->630gold, device удален. Общий PlayMode также проверяет магазины B/G/Escape и покупки.
- Три native1920x1080 кадра, before/after P1-P8, reference sheets и `metrics-1b.json` сохранены в taskfolder. Above-HUD mean encoded luma0.529752,p5 0.189687,p50 0.589981, HSV0.139100; reference HSV0.260694. ROI прямоугольные и содержат разные объекты; это sanity check, не художественная оценка.
- Финальный same-camera Editor snapshot:2386draw/66setPass/2047805triangles против P1 2108/46/1624865. FPS/CPU/GPU frame-time benchmark не проводился; отсутствие большого performance regression остается непроверенным. Число новых pillar torches ограничено3, grass объединена в153patches; итог55lights/66PS.
- Финальное состояние: Playfalse,compilingfalse,timeScale1,Free Aspect0,Arena.unity saved/dirtyfalse,console0errors/0warnings. Два автоматических Unity build-cache resource refs в GlobalSettings удалены guarded restoration только собственных additions, hash равен началу1b; постоянные общие настройки не менялись.

Итоговые artifacts и честные ограничения: `.local/codex-tasks/ow2-arena/REPORT-1b.md`, `command-index-1b.md`, `hygiene-1b.md`, `review-1b.md`. Полная художественная цель не достигнута: регулярные paving rows/repeat, простые руины и повтор листовых sprigs сохраняют вид прототипа. D-019 остается без окончательного утверждения. HUD, модели, Runtime и предшествующие пользовательские файлы сохранены.

Историческое наблюдение исполнителя 1b до последующего уточнения организатора: после нашего завершения Play17:09UTC новый test runner появился17:21:54UTC; инициатор на тот момент не был установлен, root/агенты 1b его не запускали. Read-only `test_status` зафиксировал running, затем completed27/27за111.79с, временная InitTestScene удалена runner. Handover state17:26:43UTC подтвердил Playfalse/compilingfalse/timeScale1/Arena dirtyfalse. `ProjectSettings/TimeManager.asset` тогда сохранял позднее изменение serializedVersion2/FixedTimestep RationalTime вместо float0.02, остальные поля прежние; исполнитель 1b файл не восстанавливал и не объявлял total checkout hygiene чистым. Timeline и raw artifacts сохранены в `late-runner-evidence-1b.json`. Это исторический snapshot, исправление происхождения и текущая проверка приведены ниже.

## Уточнение происхождения TimeManager и статуса, A4 среза 1c - 8 октября 2026

По уточнению организатора в `.local/codex-tasks/ow2-arena/brief-1c.md` поздний PlayMode run через `unity command` был его собственной проверкой. Unity повторно сериализовал Fixed Timestep в `unity/ProjectSettings/TimeManager.asset` и создал временные `Assets/InitTestScene*`; это побочный эффект runner, а не правка игрового поведения исполнителем 1b. После проверки организатор восстановил только TimeManager командой `git restore`. Исполнитель A4 эту команду и Unity операции не выполнял. Исходный `REPORT-1b.md` сохранен; уточнение записано отдельно в `.local/codex-tasks/ow2-arena/REPORT-1b-addendum.md`.

Read-only проверка A4 в 17:41:42 UTC: `git diff --exit-code -- unity/ProjectSettings/TimeManager.asset` и `git diff --cached --exit-code -- unity/ProjectSettings/TimeManager.asset` завершились с exit 0 и пустым выводом; файл содержит `Fixed Timestep: 0.02`, временных `unity/Assets/InitTestScene*` нет. Это подтверждает отсутствие прежнего TimeManager finding на момент проверки, но не объявляет весь checkout или исторический audit 1b чистым. Новые компиляция, Unity tests, rebuild и Windows build в документационном пункте A4 не запускались.

8 октября 2026 года пользователь принял арену среза 1b как базовый вид; финальное качество не утверждено. HUD, модели персонажей и доработка композиции остаются открытыми (D-019). История проверок и ограничения выше сохранены.

## Точечный проход арены, срез 1c - 8 октября 2026

Использован тот же Editor6000.6.4f1 PID10820 через Unity CLI и loopback127.0.0.1:7800; второй Editor не запускался, Git writes не выполнялись. A1:39малых curbs удалены,62slabs/23large curbs сохранены,0thin-curbs. A3: три hoops объединены в один mesh на каждой из288barrels, exact buffer/transform comparison PASS. Counts1b ->1c: MeshRenderer3498 ->2883,GameObject3702 ->3087,shadowCasters3322 ->2707; same-camera draw2388 ->1732. Frame time/FPS не измерены.

`Game/Build Arena scene`, итоговая компиляция и console0errors/0warnings. `unity command run_tests --format json -- --mode editor --async_tests true --filter Game.Tests --timeout 300` с `test_status`:18/18PASS; `--mode playmode --timeout 600`:27/27PASS,failed/skipped0. Post-tests contracts целиком равны1b: layout SHA92ad8fdf...,257destructibles/6explosive/4units/1TerrainCollider, terrain heightSHA46B297A0..., camera values прежние. Leaf winding7032triangles positive; preview invalid-input probes state unchanged; temporary Texture2D error cleanup1441 ->1441. HUD/Runtime/Tests/модели не изменялись. Windows player в1c не пересобран и не запускался; shaders/material keyword sets не менялись.

A2 частично выполнен: baked anisotropic damp darkening0.06 вместо0.16 визуально уменьшил облачные пятна, но actual terrain alphamap сразу после Build содержит толькоlayer0. World damp/worn lane persistence не подтвержден; два минимальных lifecycle эксперимента не помогли и возвращены. Дальнейшие fixes остановлены по лимиту трех проходовbrief; полный acceptance1c не заявляется. Причина сброса пока неизвестна. Три итоговых native1920x1080frames и before/after sheets получены и просмотрены.

После каждогоPlay/test проверены git status и отсутствиеInitTestScene. При re-serialization TimeManager exact HEAD bytes восстановлены через `git show` и write_bytes, без `git restore` и других Git writes. Три character materials, legacy поля которых сбрасывал штатныйbuilder, через Editor API возвращены к exactbaselineSHA. Foliage.mat/meta удалены только после0AssetDatabase/text consumers, с локальнымbackup; FoliageLight/textures сохранены. Исторический REPORT-1b не переписан. Команды, размеры, source diff, ограничения и результаты: `.local/codex-tasks/ow2-arena/REPORT-1c.md`, `command-index-1c.md`, `hygiene-1c.md`.

После собственных tests и guard probes появился дополнительный runner18:48:02UTC, инициатор не установлен; root/агенты1c его не запускали. Read-only status18:51:32UTC: completed27/27, не наш повтор. Он самостоятельно удалил InitTestScene, TimeManager уже равенHEAD. Свежий handover18:53:19UTC: Arena saved/dirtyfalse,Playfalse,compilingfalse,timeScale1; console18:53:22UTC0errors/0warnings. Исторический audit во время его temp scene былFAIL, итоговый inventory проверяется послеcleanup.

## HUD, срез 2: проверка 8 октября 2026

Активный Editor: 6000.6.4f1, PID 10820, один экземпляр для `game/unity`. Unity CLI из `C:/Program Files/Unity Hub/resources/unity.exe`: 1.0.0-beta.12. Pipeline 0.8.0-exp.1, URP 17.6.0 из manifest. CLI использовал существующий локальный Editor, новый Editor и пакеты не устанавливались.

- `unity command recompile`, `recompile_status`: финальная компиляция completed, failed=false. Консоль перед финальными тестами и после них: 0 errors, 0 warnings.
- `unity command menu -- --path "Game/HUD/Render hero portrait"`: рендер текущего CC0 prefab в PNG, импорт skin выполнен. Результат просмотрен в portrait и runtime HUD; первый пустой render исправлен расчетом baked geometry и полным viewport с учетом Windows DPI. Геометрия модели и материалы источника не менялись.
- `unity command run_tests --format json -- --mode editor --async_tests true --filter Game.Tests --timeout 300`, затем `test_status`: 18/18 PASS, 0 failed/skipped.
- `unity command run_tests --format json -- --mode playmode --async_tests true --filter Game.Tests --timeout 600`, затем `test_status`: финальный 31/31 PASS (27 существующих + 4 новых), 0 failed/skipped. Предыдущий промежуточный прогон 30/30 тоже прошел. Для высокой подсказки отдельно записан red run 0/1: bottom 233 вместо 244; после исправления тот же сценарий входит в 31/31.
- Input System tests: реальный hover нижнего bag, запрет продажи вне лавки и продажа ПКМ внутри, mouse B/G, Esc, F9 +500, HP/MP 70%/60%, инертность Q/W/E/R и overlay при HP=0. Actual ApplyDamage death дополнительно снят в Play Mode.
- `capture_game_view -- --width 1920 --height 1080 --source screen`: full, damaged, 6 предметов/золото, tooltip, shop, death и отдельный toast. Временные состояния созданы Editor-only helper, scene/assets не сохранялись; frameCount рос. Console capture: 0 errors/0 warnings.
- 233 item tooltip с footer продажи: max I08A 817px; 134 entries полок с настоящим ShelfFooter: max I08D 839px. Только oversized tooltip уменьшается до свободных 828px: scale=0.9868892, реальные world corners bottom=243.999969/top=1072; нормальные tooltip остаются scale=1.
- Preset 1280x720 отсутствует среди существующих Game View размеров. По brief такой кадр не снимался, screenshot 1080 не выдавался за реальный 720.
- Reused `contracts-1b.cs`: before/after равны по всем группам; layout SHA256 `92ad8fdf69931645cfdc658ea921ea180c0a9748ab09395619afe425db3fd0b3`, 257 destructibles, 6 explosive, 4 units, 1 TerrainCollider.

`Game/Build Arena scene` не запускался: этот menu перезаписывает запрещенные для HUD-среза Generated/Scenes и Graphics/Quality settings. Существующая принятая арена сохранена; compile/runtime/contract checks не выдаются за rebuild. Это невыполненная часть приемки brief.

Evidence: `.local/codex-tasks/hud/commands/`, `tests-editor-result.json`, `tests-playmode-result.json`, `contracts-result.json`, `shots/`. После PlayMode проверялся status; известных InitTestScene остатков нет, TimeManager проверен против точных HEAD bytes без Git writes. Независимое ревью кода и кадров выполнил hud_review; исправлены tooltip overlap и центровка labels. `unity command menu --timeout 900 --format json -- --path "Game/Build Windows player"` выполнена ровно один раз: Succeeded, 0 errors, 1 warning, 355493522 bytes, 32 seconds. Предупреждение: RuntimePipelineConfig отсутствует, управляющий Pipeline выключен в Player. Консоль после build имела 1 warning; после сохранения evidence и очистки финальная консоль 0 errors/0 warnings. EXE после этой сборки отдельно не запускался.

Во время BuildPipeline Unity автоматически добавила две runtime references SSAO в `unity/Assets/UniversalRenderPipelineGlobalSettings.asset` (66 bytes). До build outside guard был чист, после build отличается только этот файл. Это не разрешенная HUD-правка: восстановление не выполнялось, потому что brief разрешает запись вне скоупа только для TimeManager. Приемка diff hygiene неполная; безопасная baseline-копия и diff лежат в `.local/codex-tasks/hud/baseline-urp-global.asset` и `outside-build-side-effect.diff`. InitTestScene и временные PerformanceTestRun files отсутствуют. Scene сохранена, isDirty=false, Play stopped, timeScale=1, Game View возвращен на исходный index 0.


## HUD, срез 2b: 8-9 октября 2026

Продолжение утвержденного brief-2. Ветка `style/ow2-arena`, HEAD `3c8dae4d6350374297e454df11cea1856972b65c`, дерево dirty, upstream нет; Git-записей не было. Использован прежний Editor 6000.6.4f1, PID 10820, CLI 1.0.0-beta.12, Pipeline 0.8.0-exp.1. Независимый `review-findings.md` записан до baseline и правок проекта; работали два ревьюера.

- Один `unity command menu --format json -- --path "Game/Build Arena scene"`: success, console 0 errors/0 warnings. Контракт до/после полностью равен: layout SHA `92ad8fdf69931645cfdc658ea921ea180c0a9748ab09395619afe425db3fd0b3`; 257 destructibles, 6 explosive, 4 units, 1 TerrainCollider, прежние camera values и terrain heights. NativeSerialized baseline terrain alphamap SHA `A98F7F8E54FF35D198F8FA1198CB5C0D86FC9E3AD9F0F6886E5AA706B8D6836D` тоже совпал. Все 12 453 объекта Scene равны после нормализации local fileID. Сразу после rebuild изменились 35 файлов; 31 материал имел реальные serialized resets. Позднее эти 31 файла оказались точно равны baseline, причина возврата не доказана. Остались четыре файла с равной проверенной семантикой.
- После B: `run_tests --mode editor --filter Game.Tests --async_tests true --timeout 300` - 18/18; `--mode playmode --timeout 600` - 31/31. После C - 18/18 и 34/34, failed/skipped 0. Три новых HUD-теста проверяют ввод через прозрачные углы, узкий layout, большие и отрицательные числа, сегменты 0/100%. Первый red run: 10 PASS/3 FAIL; green: 13/13. Усиленный numeric test: 1/1; mutation без auto-fit правильно дал FAIL (226 px вместо <=212), затем исходник восстановлен точно.
- Два визуальных прохода: backing alpha 0.8, скорости 20 px, B/G 23 px, светлые locked glyphs, maximum HP/MP и более яркие цифры маны. Парные кадры на светлой арене и листве: все 22 текстовых региона >=4.5:1, минимум 5.08:1. Метод и регионы в `contrast-2.*`. Кадры 1920x1080 получены через `capture_game_view --source screen`.
- Сняты существующие presets 1920x1080, 2560x1440 и 16:10 (native 810x507). Зазор shop/HUD не меньше 14 canvas px в 16:9 и 38 в 16:10; кластеры не пересекаются. Геометрия 4:3 canvas 1440x1080 подтверждена тестом; 21:9 проверен по anchors/formula. Native presets 4:3, 21:9 и 1280x720 отсутствуют, такие кадры не заявляются.
- Один `unity command menu --timeout 900 --format json -- --path "Game/Build Windows player"`: Succeeded, 0 errors, 1 expected warning, 355 758 190 bytes, 30 seconds. Warning: RuntimePipelineConfig отсутствует, Pipeline disabled in Player. GlobalSettings и TimeManager восстановлены точными HEAD bytes, GlobalSettings повторно импортирован в Editor.
- `builds/game-arena/Arena.exe` запущен скрыто в batch mode на 12 секунд, затем остановлен только собственный PID. Данные предметов загрузились, исключений и ошибки отсутствующего HudSkin нет. Это startup smoke, не проверка изображения или ввода Windows. Лог: `.local/codex-tasks/hud/windows-startup-2.log`.

Портрет, семь исходных glyphs, 233 item icons, шрифты, старые окна и gameplay source сохранены. Не измерены profiler/FPS и десятки creeps; не проверены весь 64-bit диапазон валют, native 4:3/21:9 и интерактивный Windows-ввод. Итог: `.local/codex-tasks/hud/REPORT-2.md`; финальное состояние в `final2-state.json`, hygiene в `hygiene-2.json`. Исторический REPORT.md среза 2 не переписан.

## Бочки с первого удара, 9 октября 2026

Использован уже открытый Editor 6000.6.4f1 через Unity CLI 1.0.0-beta.12, loopback 127.0.0.1:7800. Второй Editor не запускался, Git writes и Windows build не выполнялись.

- До правки live scene содержала 257 бочек с HP=90. Узкий `run_tests --mode playmode --async_tests true --filter barrel_breaks_on_the_first_hit --timeout 120`: 0/2, обе проверки подтвердили отсутствие разрушения за первый интервал атаки.
- Один `unity command menu --timeout 120 --format json -- --path "Game/Build Arena scene"`: success=true, console 0 errors/0 warnings. Builder теперь задает HP=1 всем 257 разрушаемым бочкам (251 обычная, 6 взрывных); crates сохранены.
- `contracts-1b.cs` до/после совпал за исключением HP. Layout SHA `92ad8fdf69931645cfdc658ea921ea180c0a9748ab09395619afe425db3fd0b3`, 4 units, 1 TerrainCollider, terrain height SHA `46B297A0DE034C306B1B083E35BD89B4D09978ACB468B7DB503BAE1DDB2F14DE`. Canonical comparison 12 453 scene objects: ровно 257 HP changes, других изменений нет. Heights, alphamaps и параметры TerrainData совпали.
- `unity command run_tests --format json -- --mode editor --async_tests true --filter Game.Tests --timeout 300`: 18/18, 1.43 с. Полный `--mode playmode --timeout 600`: 35/36, 110.61 с. Обе новые first-hit проверки прошли через HeroController.OrderAttack, разрушение до самой ранней возможности второго взмаха, удаление объекта и открытие клетки.
- Сбой полного PlayMode: InputOrderTests.Right_click_on_a_barrel_attacks_it ожидал Barrel#367, выбрал Barrel#372. Отдельный повтор с фильтром этого теста: 1/1, 3.95 с, без изменения source/assertions. Причина разового сбоя не доказана; полный 36/36 не заявляется и не повторялся по brief.
- После rebuild 31 материал имел другой SHA. К финалу 21 вернулся к исходным байтам после import/test lifecycle; 10 восстановлены из локального baseline через LoadSerializedFileAndForget/CopySerialized/SaveAssets. Все 31 побайтно равны baseline. Числа взрыва не изменены.

Evidence и пофайловая классификация: `.local/codex-tasks/barrels/REPORT.md`. Последний Windows player собран до изменения сцены и не подтверждает новый HP. Приемка по полному зеленому PlayMode остается неполной.

## Лавки, гайды и tooltip, срез 2c, 9 октября 2026

Unity 6000.6.4f1 и CLI 1.0.0-beta.12, loopback 127.0.0.1:7800. После ночной паузы проверены status718/HEAD и отсутствие Editor; запущен ровно один PID3292 с логом `.local/codex-tasks/shop/editor.log`, ready, console0 errors/0 warnings. Повторного rebuild не было; прежний `Game/Build Arena scene` завершился success 9 октября в 04:19 MSK.

- `py -3 -X utf8 tools/art/gen_shop_skin.py`, `unity command menu --format json -- --path "Game/HUD/Import skin"`: пять sprites, window/tooltip border40, fonts и прежние sprites сохранены. Первый import столкнулся с reload соединения, повтор успешен.
- `HudVisualPreview.ShopState` и `capture_game_view --source screen`: семь состояний на 1920x1080,2560x1440 и существующем 16:10 (native810x507), плюс все остальные гайды. Window/HUD gap не меньше14 canvas-px. До/после сняты из точного baseline и текущего runtime; Text-disabled backing сохраняет flow layout через временное выключение LayoutGroup/ContentSizeFitter.
- `unity command run_tests --format json -- --mode editor --async_tests true --filter Game.Tests --timeout 300`: один полный run,18/18,0 failed/skipped/inconclusive. Такой же `--mode playmode --timeout 600`: один полный run,41/41 (36 прежних+5 новых),0 failed/skipped/inconclusive. Во время suites только последовательный foreground `test_status`; прежние assertions не изменены.
- 301 видимый текстовый регион на парных1080p кадрах: все >=4.5:1, минимум5.987:1. Еще13 полей перекрыты tooltip и отмечены как невидимые. Sweep233 bag+134 shelf tooltip: минимум19.5976px;48 next labels помещаются. Это проверенные кадры и каталог, не гарантия для произвольного контента/фона.
- `contracts-1b.cs`: layout SHA92ad8fdf69931645cfdc658ea921ea180c0a9748ab09395619afe425db3fd0b3,257 destructibles/6 explosive/4 units/1 TerrainCollider/HP1, before=after. Canonical сравнение содержит12453 равных объектов; heights/alphamaps/layers TerrainData равны baseline. Двенадцать материалов восстановлены через Editor CopySerialized после прежнего rebuild; TerrainLit/Post/scene имеют равную canonical семантику.
- `unity command menu --timeout 900 --format json -- --path "Game/Build Windows player"`: один build,Succeeded,0 errors/1 ожидаемое Pipeline warning,357275658 bytes,22 seconds. `Arena.exe` hidden batch/nographics startup12s: работал, данные загрузились, исключений и missing HudSkin нет; собственный PID39244 остановлен.
- GlobalSettings и TimeManager восстановлены exact HEAD bytes, GlobalSettings импортирован в Editor. Изображение и интерактивный ввод standalone, FPS/profiler и художественная приемка не проверены. Доказательства и отчет: `.local/codex-tasks/shop/REPORT.md`.
