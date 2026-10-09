# bruisers: два варианта

Дата: 2026-10-09. Tool: built-in image_gen.imagegen. Use case: stylized-concept.
Новая генерация, без входных изображений.

## Exact prompt, attempt 1

```text
Use case: stylized-concept. Create one ORIGINAL premium stylized 3D fantasy paired character decision sheet, landscape 1920x1080 requested. Title "BRUISERS / TWO SILHOUETTES". Warm daylight, cool soft shadows, neutral pale gray-blue background, believable sculpted large forms and materials, NO toy armor, chibi, cartoon smile, flat vector, franchise character, logo or emblem, no other-game reference. LEFT HALF is "B1 / BASALT YOKE", RIGHT HALF is "B2 / ROOT KNUCKLE". They are substantially different silhouettes and must stay consistent in all repeated views.
For EACH HALF: top region show LARGE complete full-body 3/4 FRONT view including feet and any weapon, alongside a smaller but clearly detailed TOP-DOWN pitch56 camera reference. BOTTOM region per half has SIX small cutouts on a common baseline: 3 COLOR TOP-DOWN pitch56 views at total character heights 110px,135px,160px, then 3 PURE BLACK silhouettes OF THAT SAME CAMERA VIEW at heights110,135,160px. Exact labels "110", "135", "160" and section labels "3/4 FRONT", "CAMERA 56", "COLOR 1:1", "BLACK 1:1". DO NOT fill space by enlarging thumbnails, use lots of empty space. No internal detail or gray shading in silhouettes. This is serious asset-design selection material.
LEFT design: 2.9m heavy hostile humanoid stone creature. NOT a mossy rounded boulder golem from arena image. A huge continuous angular BASALT YOKE formed of three connected wide horizontal dark mineral slabs arching above low recessed head, long very massive forearms, tapering hips, thick short bent feet. Slabs have rounded sculpted worn edges and broad crack planes; only two broad dim orange fissures at neck and elbows, one short rust-red cloth belt panel, NO dense glowing vein network, NO spikes, NO moss. Hands open broad stone fists, no weapon. Distinct wide inverted U upper mass, low head and long arms. Dark bluish basalt with warm hostile accents.
RIGHT design: 2.9m heavy hostile original organic bruiser. Upright broad chest and narrow head, large asymmetric thick ROOT-WOOD arm on left extends to ankle as natural club fist, shorter right arm to upper thigh. Hide and dark matte bark in three broad smooth layered sweeps, thick powerful digitigrade but bipedal legs, wide triangular pelvis, no dangling thin vines, no mushrooms, no branch beard. Two orange amber growths sunk into left shoulder and sternum, one wide rust-red waist wrap. Silhouette distinct from stone slab bruiser: one vertical oversized forearm, asymmetrical tall body, tapering heavy chest. Serious sculpted creature, no cartoon smile.
All views match actual design. Avoid micro-ornamentation. Keep faction orange/red cues as broad patches visible from above. Every limb separates in projected silhouette; no idle effects that hide geometry.
```

Повторы и проверка: .local/codex-tasks/chars/progress.md.


## Exact prompt, attempt 2 (first retry)

Input image: prior attempt of this sheet.

```text
Use case: precise-object-edit / stylized-concept. Edit this original paired decision sheet. Fix worst defect: B2 has dense fine twig spikes and root strands unlike the requested clear masses. Remove ALL thin branches, fine strands, leaf tufts and small shoulder spikes. B2 body should consist of THREE broad smoothly sculpted layered wood sweeps, retain the very long left arm, short right arm, orange amber insets and red waist wrap. Make B1 head less like human mask and more a recessed single broad mineral wedge, no facial lips. Preserve asymmetric B2 silhouette and wide stone-yoke B1 silhouette and paired layout. All repeated views and pure black silhouettes must match. Preserve both full-body 3/4 front views, both enlarged 56 degree camera views and all bottom small game-camera color views and pure black silhouettes labelled110/135/160. Do not introduce additional panels or characters.
```


## Финальная верстка

Основной PNG 1920x1080 сверстан из сохраненного 2026-10-09-creeps-bruisers-source-v1.png локальным PowerShell/System.Drawing. Рисунок персонажа не дорисовывался. Crop и scale численные; нижние рамки 110/135/160px. Источник с первоначальными AI-подписями не использовать как точную pixel calibration. Для lineup верхняя шкала 150px/м. Layout data и renderer: .local/codex-tasks/chars/layout-data.json, render-layout.ps1. Дата: 2026-10-09.

