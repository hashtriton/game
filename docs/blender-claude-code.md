# Claude Code и Blender: живой мост

Состояние на 5 октября 2026 года.

По поручению пользователя подключить Blender к Claude Code с тесной интеграцией выбран официальный BlenderLab MCP в полном режиме: тот же сервер `blmcp` 1.0.3 из `.local/blender-mcp-venv` плюс официальный add-on `mcp` 1.0.3 внутри GUI Blender. Add-on даёт живой мост: Claude Code выполняет Python и читает сцену в открытом окне, снимает скриншоты окна и областей, рендерит вьюпорт, переходит к объектам. Режим только с `_for_cli` (фоновые процессы) остаётся доступен как запасной.

| Шаг | Результат | Доказательство |
| --- | --- | --- |
| Add-on | `mcp-1.0.3.zip` со [страницы BlenderLab](https://www.blender.org/lab/mcp-server/), SHA256 `A7A9DA816192502E5A0A202A396444E266B47D8FC4F74AD4698048BD43040707`; код прочитан до установки. Установлен `blender --command extension install-file -r user_default -e` только в проектный профиль `.local/blender-profile` | Вывод `STATUS Installed "mcp"` |
| Настройки профиля | `127.0.0.1:19876`, автостарт моста, online access только в проектном профиле (без него add-on не запускает сокет) | [blender_mcp_profile_setup.py](../tools/blender_mcp_profile_setup.py), вывод `BLMCP_PROFILE_OK` |
| Claude Code | Проектный [.mcp.json](../.mcp.json), сервер `blender`, транспорт stdio; `claude mcp get blender` видит его в scope Project config | Статус до одобрения владельцем: `Pending approval` |
| Живой вызов | GUI запущен [blender-gui.ps1](../tools/blender-gui.ps1), порт слушает только `127.0.0.1`; через MCP `tools/list` вернул 26 инструментов, `get_objects_summary` прочитал сцену открытого окна | Проверочный клиент [mcp_probe.py](../tools/mcp_probe.py) |

Запуск GUI с мостом: `powershell -File tools/blender-gui.ps1 [-File путь.blend]`. Обычный профиль Blender пользователя не изменялся: вне проектного профиля add-on не загружается.

Риск: пока открыт GUI с проектным профилем, любой локальный процесс текущего пользователя может отправить Python на `127.0.0.1:19876` без аутентификации. BlenderLab прямо предупреждает, что код исполняется без защиты данных. Закрытие окна останавливает мост. Внешний порт не открывается.

Не подтверждено: одобрение `.mcp.json` в новой сессии Claude Code и вызов нативных `mcp__blender__*` из неё. В этой сессии сервер вызывался проверочным JSON-RPC клиентом, потому что каталог инструментов активного агента не обновляется на лету.

Первый герой, собранный в этой связке: [Hexblade](../art/heroes/hexblade/README.md).
