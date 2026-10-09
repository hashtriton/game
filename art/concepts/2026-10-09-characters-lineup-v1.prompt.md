# Рекомендованный lineup

Дата: 2026-10-09. Tool: built-in image_gen.imagegen. Use case: compositing / stylized-concept.
Input images: hero B, raiders, bruisers, elites, финальные исправленные оригинальные листы.

## Exact prompt, attempt 1

```text
Use case: compositing / stylized-concept. Use these four ORIGINAL decision sheets as sources for a SINGLE polished relative-scale character LINEUP. Extract and accurately repeat ONLY hero B BREAKWATER from image1, raider R1 CINDER RUNNER from LEFT half image2, bruiser B1 BASALT YOKE from LEFT half image3, elite E1 KILN REGENT from LEFT half image4. Preserve their original designs and gear, do not invent others. Premium stylized 3D warm daylight and soft cool shadows. Plain pale blue-gray background, no scenery, effects, ring, HUD, emblems or logos.
1920x1080 landscape requested. TOP title "RECOMMENDED LINEUP / RELATIVE SCALE". One continuous row of four full-body FRONT three-quarter characters on EXACT SAME GROUND BASELINE, same camera distance/projection, no perspective depth variation. Hero B is 2.4m including helmet; R1 is2.1m including hood; B1 is2.9m to top yoke; E1 is3.5m to top collar. E1 must be tallest, B1 is wider and 21% taller than hero, raider is87.5% hero height. Body height ratio exactly 2.4 : 2.1 : 2.9 : 3.5, DO NOT make everyone equally tall. Under each its ID and height. Small horizontal height guides labelled2.1m,2.4m,2.9m,3.5m, subtle and legible.
BOTTOM strip: same four characters from oblique TOP-DOWN camera pitch56, a common projected pixels-per-metre ratio, hero 135px, raider118px,bruiser163px,elite197px tall as reading target. Then four matching pure black silhouettes at those same projected heights. Top down shortening can vary with pose; labels are design targets, not 3D measurement claims. Red/orange creep accents vs herowhite/cyan. Keep armor clean, sculptural and substantial, preserve short cloth, no spiky ornaments. This is a design preview, not a screenshot of Unity.
```


## Exact prompt, attempt 2 (one retry)

```text
Use case: precise-object-edit. Fix only relative scale and alignment of this ORIGINAL lineup. All four upper full-body FRONT three-quarter views must share a single foot-ground baseline and identical camera scale. HERO B head-to-sole height = 360 arbitrary units. R1 head-to-sole height = 315 units. B1 top-yoke-to-sole = 435 units. E1 top-collar-to-sole = 525 units. Exact ratio2.4:2.1:2.9:3.5. Currently hero and raider are too large relative to bruiser. Correct their relative sizing, no perspective depth offsets. Preserve the same character designs, original short cloth, cyan/white vs orange/red cues, title, labels, camera56 lower strip and silhouettes. Do not change gear or add anything. Lower pure black E1 silhouette should have the same collar opening and projection as its color view, not an unrelated ring.
```

## Финальная верстка

Основной PNG 1920x1080 сверстан из сохраненного 2026-10-09-characters-lineup-source-v1.png локальным PowerShell/System.Drawing. Рисунок персонажа не дорисовывался. Crop и scale численные; нижние рамки 110/135/160px. Источник с первоначальными AI-подписями не использовать как точную pixel calibration. Для lineup верхняя шкала 150px/м. Layout data и renderer: .local/codex-tasks/chars/layout-data.json, render-layout.ps1. Дата: 2026-10-09.

