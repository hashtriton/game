# hero_c

Дата: 2026-10-09. Tool: built-in image_gen.imagegen. Use case: stylized-concept.
Новая генерация, без входных изображений.

## Exact prompt, attempt 1

```text
Use case: stylized-concept. Generate an original premium stylized 3D fantasy CHARACTER DECISION SHEET, landscape 1920x1080 native pixels requested. Warm daylight, cool soft shadows, sculpted believable anatomy, forged steel and woven fabric with restrained wear, clean large forms. No toy-like armor, no chibi, no plastic, no flat vector, no robotic human face, no franchise shapes, logos, emblems, characters or copied outfits. Pale blue-gray paper background. High quality art direction, calm neutral combat-ready stance. A consistent single character repeated accurately in all views.
Layout: upper left at x80..720 a LARGE full-body front three-quarter render with both feet and entire weapon visible. Upper right at x1000..1560 an enlarged oblique top-down 56 degrees camera reference. Along BOTTOM from y800 to1040, leave lots of empty space around six SMALL cutouts. First three are COLOR oblique TOP-DOWN 56 DEGREES game-camera cutouts, NOT front elevation on blue-gray paving at actual total character heights 110px, 135px, 160px (roughly 11.7%,14.3%,17% of a 941px high output) including weapon, not enlarged thumbnails; label each "110 px", "135 px", "160 px". Next three are pure BLACK silhouettes of those identical game-camera views, also total heights 110px,135px,160px; no gray shading, no internal details, no shadow. Large title upper left, section labels "3/4 FRONT", "CAMERA 56", "GAME SIZE / 1:1", "BLACK SILHOUETTE / 1:1". Clear editorial spacing. No extra characters, no props background, no stats, no invented measurement rulers. Silhouettes and game views must match the large design.
Title "C / WINDSTEP".
Adult female agile melee hero, 2.4m tall. Lean sinewy tall frame, visible determined natural human face with broad nose, medium brown skin and cropped curly dark hair, no beauty-filter skin. Small collar and completely narrow shoulders without giant pauldrons, fitted ivory leather/steel chest vest, blue-gray fitted trousers, wrapped calves, asymmetrical short white hip sash ending above knee. Right hand single straight slender-but-readable sword 1.25m with broad leaf-shaped tip and small offset guard. Left forearm very small SOLID circular buckler with cyan rim and dark opaque center. A large diagonal scabbard across upper back, no cape. Distinct narrow vertical leggy silhouette, large forward sword diagonal, circular left-hand punctuation. Strong realistic construction and athletic body, not armored heroine A recolor.
All 3 small color views AND all 3 black silhouettes MUST show the SAME 56 degree oblique TOP-DOWN view, see tops of shoulders and head, shortened legs in projection. Never use front standing elevation for the bottom row. Keep their top-to-bottom bounding boxes close to 110/135/160 native pixels, do not fill the bottom space.
```

Повторы и проверка: .local/codex-tasks/chars/progress.md.


## Exact prompt, attempt 2 (one retry)

Input image: 2026-10-09-hero-c-windstep-v1.png attempt 1.

```text
Use case: precise-object-edit / stylized-concept. Edit this decision sheet. Fix the worst defect: sword is almost the character's full height. Shorten the sword BLADE by 25% in EVERY view, preserving blade style and grip; increase blade width by10% to stay readable. Remove the long back scabbard from all views to avoid second diagonal silhouette. Preserve body, human face, outfit, buckler and titles. Ensure bottom black silhouettes reflect this shortened blade and absent scabbard. Keep full-body 3/4 front, enlarged 56 degree camera, and bottom six cutouts: 3 game-camera color views and three pure black silhouettes labelled110/135/160px. Preserve original concept only, no reference to other characters. Do not introduce new panels or text.
```


## Exact prompt, attempt 3 (second retry)

Input image: prior attempt of this sheet.

```text
Use case: precise-object-edit. Fix ONLY the three PURE BLACK silhouettes on bottom right of this sheet. They still have a diagonal scabbard handle sticking out behind the left shoulder and they show the old long sword. Remove that protruding handle and back scabbard entirely from ALL three black silhouettes, and shorten their sword to match the new SHORT leaf-tip sword in the color game-camera views immediately to the left. Keep the buckler, proportions, top-down camera and pose. Preserve ALL other art, title, layout and all six views unchanged. No additional elements.
```


## Финальная верстка

Основной PNG 1920x1080 сверстан из сохраненного 2026-10-09-hero-c-windstep-source-v1.png локальным PowerShell/System.Drawing. Рисунок персонажа не дорисовывался. Crop и scale численные; нижние рамки 110/135/160px. Источник с первоначальными AI-подписями не использовать как точную pixel calibration. Для lineup верхняя шкала 150px/м. Layout data и renderer: .local/codex-tasks/chars/layout-data.json, render-layout.ps1. Дата: 2026-10-09.

