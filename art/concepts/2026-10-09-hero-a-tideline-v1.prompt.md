# A: Прилив

Дата: 2026-10-09. Tool: built-in image_gen.imagegen. Use case: stylized-concept.
Новый оригинальный дизайн. Без входных изображений.

## Exact prompt, attempt 1

```text
Use case: stylized-concept. Generate an original premium stylized 3D fantasy CHARACTER DECISION SHEET, landscape 1920x1080 native pixels requested. Warm daylight, cool soft shadows, sculpted believable anatomy, forged steel and woven fabric with restrained wear, clean large forms. No toy-like armor, no chibi, no plastic, no flat vector, no robotic human face, no franchise shapes, logos, emblems, characters or copied outfits. Pale blue-gray paper background. High quality art direction, calm neutral combat-ready stance. A consistent single character repeated accurately in all views.
Layout: upper left at x80..720 a LARGE full-body front three-quarter render with both feet and entire weapon visible. Upper right at x1000..1560 an enlarged oblique top-down 56 degrees camera reference. Along BOTTOM from y800 to1040, leave lots of empty space around six SMALL cutouts. First three are COLOR game-camera cutouts on blue-gray paving at actual total character heights 110px, 135px, 160px including weapon, not enlarged thumbnails; label each "110 px", "135 px", "160 px". Next three are pure BLACK silhouettes of those identical game-camera views, also total heights 110px,135px,160px; no gray shading, no internal details, no shadow. Large title upper left, section labels "3/4 FRONT", "CAMERA 56", "GAME SIZE / 1:1", "BLACK SILHOUETTE / 1:1". Clear editorial spacing. No extra characters, no props background, no stats, no invented measurement rulers. Silhouettes and game views must match the large design.
Title "A / TIDELINE". Female melee hero, 2.4m in game, adult age about 32, naturally sculpted thoughtful determined human face, short dark swept-back hair with a narrow silver forelock, no long ponytail. Athletic strong physique and elongated practical legs. Clean asymmetrical ivory shoulder mantle on LEFT only, fitted blue-gray cuirass with a diagonally sweeping seam, exposed fabric elbow and knee articulation, navy cropped split tabard ending above knees. Right hand holds a single elegant broad slightly forward-curved sword, unique asymmetric blunt angular guard, blade length 1.1m, physical steel with only a very narrow cyan fuller. LEFT forearm carries a COMPACT hooked crescent shield with solid dark metal outer rim and opaque ivory center, cyan edge only, not a transparent pane. Distinct silhouette is single shoulder sweep + short offset tabard + compact crescent left shield + curved sword, no cape. White and cool cyan faction accents, no orange fabric. Both hands/weapon grips anatomically correct; sword projects away from legs to remain a separate shape even at 110px. Original design, no brand references.
```

Повторы и проверка: .local/codex-tasks/chars/progress.md.


## Exact prompt, attempt 2 (one retry)

Input image: 2026-10-09-hero-a-tideline-v1.png attempt 1.

```text
Use case: precise-object-edit / stylized-concept. Edit this decision sheet. Fix the worst defect: all six bottom thumbnails currently wrongly use FRONT ELEVATION. Replace those six with true 56 degree OBLIQUE TOP-DOWN camera versions identical to the upper-right view, showing top of head and shoulders and foreshortened legs. Also shorten the curved sword 25%, keeping the same guard and shape. Preserve the natural face, armor, hair, shield, identity, title and overall layout. Maintain pure black silhouettes at three sizes. Keep full-body 3/4 front, enlarged 56 degree camera, and bottom six cutouts: 3 game-camera color views and three pure black silhouettes labelled110/135/160px. Preserve original concept only, no reference to other characters. Do not introduce new panels or text.
```


## Финальная верстка

Основной PNG 1920x1080 сверстан из сохраненного 2026-10-09-hero-a-tideline-source-v1.png локальным PowerShell/System.Drawing. Рисунок персонажа не дорисовывался. Crop и scale численные; нижние рамки 110/135/160px. Источник с первоначальными AI-подписями не использовать как точную pixel calibration. Для lineup верхняя шкала 150px/м. Layout data и renderer: .local/codex-tasks/chars/layout-data.json, render-layout.ps1. Дата: 2026-10-09.

