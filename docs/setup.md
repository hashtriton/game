# Состояние настройки

Проверенный срез на 5 октября 2026 года перед первым коммитом. Основная задача настройки Unity и Blender ещё выполняется. Этот документ фиксирует доступные результаты и не означает завершение настройки или готовность игры.

## Инструменты и подтверждённые операции

| Компонент | Версия / результат | Доказательство |
| --- | --- | --- |
| Unity Editor | `6000.6.4f1`; проект `unity` открыт, Pipeline сообщает `ready` | [ProjectVersion.txt](../unity/ProjectSettings/ProjectVersion.txt), наблюдательная команда `unity status --project-path C:\Users\Netes\Desktop\proj\game\unity --format json --non-interactive` |
| Unity CLI | `1.0.0-beta.12` | `C:\Program Files\Unity Hub\resources\unity.exe --version` |
| Unity Pipeline | `0.8.0-exp.1` | [manifest.json](../unity/Packages/manifest.json) |
| Unity batch smoke | Компиляция и сохранение нейтральной сцены из трёх объектов прошли | [unity-batch-smoke.json](../verification/unity-batch-smoke.json); локальный лог `verification/unity-batch-smoke.log` исключён из Git |
| Unity MCP | `initialize`, `tools/list` и чтение иерархии сцены прошли через вспомогательный Python JSON-RPC клиент, вызванный Codex | [unity-mcp-hierarchy-smoke.json](../verification/unity-mcp-hierarchy-smoke.json) |
| Blender | `5.2.2 LTS`; создание сцены, сохранение `.blend`, экспорт GLB и повторный импорт прошли через CLI | [blender-smoke.json](../art/smoke/blender-smoke.json), [blender_smoke.py](../tools/blender_smoke.py) |
| Blender MCP | `initialize` и `tools/list` прошли; `tools/call` завершился клиентским таймаутом через 120 секунд | [blender-mcp-first-error.json](../verification/blender-mcp-first-error.json) |

MCP настроен только для `game` через [проектный config.toml](../.codex/config.toml), транспорт `stdio`. Unity Pipeline слушает loopback `127.0.0.1:7800`. В конфиге Unity MCP разрешены шесть инструментов чтения. Blender MCP разрешает `execute_blender_code_for_cli` с запросом перед выполнением Python. Пути в конфиге и launch-файлах относятся к этому ПК; на другом ПК их нужно адаптировать.

Отдельный профиль Blender и фоновый процесс не являются sandbox операционной системы. Python Blender выполняется с правами пользователя Windows.

## Что ещё не подтверждено

- Обнаружение и успешный вызов Unity/Blender MCP как нативных инструментов активной сессии Codex. Отчёт Unity явно содержит `native_codex_tool_catalog_verified: false`.
- Успешное выполнение Blender MCP команды и первопричина таймаута. [Проверка stdin](../verification/blender-stdin-repro.json) не воспроизвела зависание при обычной загрузке сцены без Python.
- Локальная поправка Blender MCP `stdin=subprocess.DEVNULL` подготовлена и зафиксирована в [отчёте](../verification/blender-local-patch.json) и [patch-файле](../tools/blender-mcp-windows-stdin.patch). Повторный MCP вызов после неё ещё не подтверждён; считать таймаут исправленным нельзя.
- Версия Codex, состояние Unity аккаунта и лицензии отдельной безопасной проверкой в этом срезе. Успех компиляции не заменяет эту проверку.
- Сборка игры, Play Mode, игровой прототип и проверка пользовательского сценария: на этом этапе они не выполнялись.

## Воспроизведение и локальные файлы

Исходники Unity, настройки проекта, тестовые сцены, собственные smoke-скрипты и безопасные JSON-отчёты включаются в репозиторий. `Library`, `Temp`, `Logs`, `.local`, виртуальные окружения, лицензии, токены и логи остаются вне Git согласно [.gitignore](../.gitignore).

Установленная документация Unity CLI в `.agents/skills/unity-cli` и сгенерированный полный каталог MCP не включаются в Git. Навыки устанавливаются из официального Unity CLI командой `unity skill install codex --local` из каталога Unity проекта после проверки справки установленной версии. Зависимости Blender MCP находятся в локальном окружении; его содержимое в репозиторий не переносится.

Официальные инструкции: [Unity CLI и замена старого MCP](https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli), [Unity CLI / Pipeline](https://unity.com/blog/meet-the-unity-cli), [BlenderLab MCP](https://www.blender.org/lab/mcp-server/), [BlenderLab](https://www.blender.org/lab/).
