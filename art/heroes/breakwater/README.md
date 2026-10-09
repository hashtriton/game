# B / Волнолом - clay blockout

2026-10-09, два статических варианта собственного героя, созданных только Blender CLI. Рекомендуемый вариант для дальнейшего рассмотрения: P2. Художественная приемка пока не пройдена; оба варианта остаются блок-аутом.

- [Сравнение P1/P2](sheet-compare.png), [P1](sheet-p1.png), [P2](sheet-p2.png). Смотреть при 100% масштабе.
- [P1 .blend](blockout-p1.blend), [P2 .blend](blockout-p2.blend).
- [Численный spec](spec.md), [donor facts](donor-facts.md).
- [Техническая проверка P1](validation-p1.json), [P2](validation-p2.json).
- `renders/p1` и `renders/p2`: 1280x1280 front, right side, back, top, three-quarter; 1920x1080 game/game-front, true crops, 110/135/160 color и черные geometry masks.

`game` соответствует runtime camera relative rear direction; `game-front` использует тот же pitch 56, distance 19, vertical FOV 45 и противоположную сторону для сравнения с концептом. Это эквивалентно повернутому к игровой камере герою. Файлы camera.json фиксируют обе стороны отдельно. Обе сцены модели остаются ориентированы front -Y.

Два материала, 2.4 м, 4988 evaluated triangles, отдельные именованные пластины, socket parents. Нет UV, textures, rig, actions или экспорта. A-pose не является donor bind pose. Нужны будущие weights, bind conversion/retarget и проверка clips; текущие scripts ничего под unity не записывают.

Воспроизведение из корня game, по очереди для p1/p2:

```powershell
$heroBlender = 'C:/Program Files/Blender Foundation/Blender 5.2/blender.exe'
& $heroBlender --background --factory-startup --disable-autoexec --offline-mode --python-exit-code 1 --python tools/hero_breakwater/build_blockout.py -- --variant p1
& $heroBlender --background --factory-startup --disable-autoexec --offline-mode art/heroes/breakwater/blockout-p1.blend --python-exit-code 1 --python tools/hero_breakwater/render_views.py -- --variant p1
& $heroBlender --background --factory-startup --disable-autoexec --offline-mode art/heroes/breakwater/blockout-p1.blend --python-exit-code 1 --python tools/hero_breakwater/validate_blockout.py -- --variant p1
py -3 -X utf8 tools/hero_breakwater/compose_sheets.py --variant all
```

Builder перезаписывает только собственный выбранный blockout; перед ручными изменениями сохраните отдельную копию. Renderer добавляет studio только в память и не сохраняет .blend. Проверено Blender 5.2.2 LTS, Cycles OptiX, RTX 2080 Ti, 32 samples, denoising. Ошибка HIP initialization в логах не препятствует реально выбранному OptiX; API сообщает предупреждения будущего удаления use_nodes в версии 6.0.

Первоначальное независимое ревью и его повтор после двух групп правок сохранены в локальном REPORT. Технический PASS не означает художественное утверждение или готовность к Legacy clips.
