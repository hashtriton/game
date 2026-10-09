# Волнолом P3 - формы, пропорции и готовность к ригу

Дата: 2026-10-09 19:53:18 MSK. Только оригинальная clay geometry, Blender 5.2.2 LTS CLI. P1/P2 сохранены. Ни UV, ни текстуры, ни риг, ни экспорт, ни Unity не выполнялись.

## Источники и ограничения

- [Исходный концепт B](../../concepts/2026-10-09-hero-b-breakwater-source-v1.png): AI guidance, не ортографическая истина.
- [Front](ref/front.png), [side](ref/side.png), [back](ref/back.png), рядом точные `.prompt.md`. Built-in image_gen, 4 вызова. Первый back отвергнут из-за визора на затылке; сохранен исправленный back.
- Взяты: раскладка трех плечевых перекрытий, широкая спина, задний neck guard, форма накладных cuisses/greaves и rising toe.
- Отвергнуты: неверные стороны рук в side, отсутствие A-pose в некоторых видах, длинная единая юбка, chainmail/заклепки, шарообразные локти и дублирование нагрудника сзади. Все поверхности смоделированы заново кодом, изображение не является mesh source.
- [Donor facts](donor-facts.md) и прежний `.local/codex-tasks/hero-b/donor-probe.json` использованы как ранее полученные данные. В этом проходе FBX и Unity не открывались. Donor mesh не переносился.

## P2 vs P3 vs concept

Метры, кроме безразмерных отношений и triangles. P2 из прежнего validation JSON; P3 из свежих evaluated meshes. Width частей P3 - local envelope до pose rotation; плечи и общий span - world envelope. Chest depth P3 охватывает breastplate+backplate, у P2 указана глубина основного torso. Concept numbers - старые ручные пиксельные landmarks с перспективной погрешностью, не новые метры.

| Параметр | P2 | P3 | Концепт / ограничение |
| --- | --- | --- | --- |
| Body height rest | 2.4000 | 2.4000 | 2.4 m reference assumption |
| Body height display | - | 2.3731 | Not metric from AI |
| Helmet height | 0.3780 | 0.4200 | About 0.419 m from 98/561 px |
| Helmet heights total | 6.3492 | 5.7143 | About 5.72 |
| Outer shoulder span | 1.0251 | 1.2941 | 324/561 px, perspective ratio 0.578 |
| Chest depth | 0.4104 | 0.6087 | Not measurable reliably from source |
| Thigh width | 0.2986 | 0.3400 | Broad, no reliable orthographic m value |
| Shin width | 0.2773 | 0.3100 | Broad, no reliable orthographic m value |
| Boot length | 0.4114 | 0.4700 | Rising toe, no reliable m value |
| Forearm width | 0.2445 | 0.2840 | Thick, no reliable m value |
| Sole-to-hip / height | 0.4858 | 0.4600 | Approx 0.487 by old landmark, AI perspective |
| Torso/leg radius rest | 0.3672 | 0.4125 | Not measurable reliably |
| Equipped width rest | 1.2159 | 1.6923 | Not metric from AI |
| Equipped width display | - | 1.8138 | Sword and shield spread wider than rest |
| Evaluated triangles | 4988.0000 | 8420.0000 | Not applicable |

Pelvis width 0.6440 m, nominal waist 0.53 m: hips шире талии. Forearm + vambrace local width 0.3102 m. Shoulder span target 1.15-1.30, helmet 0.40-0.44, hip fraction 45-47% соблюдены.

Footprint проверяется по всем evaluated vertices torso/breastplate/back/chest-lip/pelvis/thigh/cuisse/knee/poleyn/shin/greave/boot/sabaton/belt/fauld/tabard/pouches. Шлем, плечи, руки, меч и щит исключены по brief. Radius считается от origin, не от произвольно подобранного центра. Rest 0.412499, display 0.413580 m. Полный display span 1.8138 больше neutral limit 1.7; ограничение 1.7 относится к rest. Это visual geometry, не collider.

## Конструкция

67 объектов: HB_Root, два sockets и 64 отдельных HB_ meshes. Все 37 first-pass names сохранены, новые parts добавлены. Два материала HB_Clay/HB_Cyan, cyan не emissive. Списки parents, scales, thickness, triangles и bounds в [validation-p3.json](validation-p3.json).

Контролируемые аналитические кривые с 12-24 contour samples вместо 8-sided flat lofts. Smooth polygons, sharp boundary normals и angle limit 40-45 degrees. Это альтернативный контролируемый smooth method, не безусловный subdivision всей сцены. Плечевая верхняя оболочка имеет более плотную сетку и иной arc; нижние две - меньшие partial shells. Нет perfect spheres или одинакового global bevel.

Plate surfaces используют Solidify 16-30 mm; shield 55 mm, rim 26 mm. Малые bevel 1-4 mm только на выбранных конструктивных кромках, part-specific, не на каждой детали. Breastplate имеет central ridge и V-shaped lower boundary; lower chest lame и 3 fauld lames перекрываются. Helmet: brow, настоящий recessed front slit, tapered faceplate, cheeks и rear neck guard. Колени/локти - выпуклые shaped cups, cuisses/greaves - накладки; shin имеет ridge; sabaton - наклонная toe roof. Shield gently convex, с поднятым rim и одной cyan stripe. Sword blade 0.66 m, 0.22 m width, 25 mm center ridge, отдельные crossguard/grip/pommel. Нет мелкого шума/рядов rivets.

Геометрические self-intersections между некоторыми shells автоматически не исключены. Ноль nonfinite vertices и zero-area triangles не доказывает отсутствие любого interpenetration. Static виды просмотрены; динамика не симулировалась.

## Поза и воспроизводимость

Система: meters, Z up, front -Y, left +X, origin между стопами rest, soles Z=0. [Builder](../../../tools/hero_breakwater/build_p3.py), `--variant p3 --pose both --revision 8`. `save_version=0`. Один набор meshes, pose() только меняет локальные rotations и pelvis translation, геометрия между позами не пересоздается.

| Pivot | Rest | Display delta / target |
| --- | --- | --- |
| HB_Torso | Upright | X +6 degrees |
| HB_UpperArm_L/R | Y -25/+25 | Arm out L 17, R 22 degrees; X -12 |
| HB_Forearm_L/R | Follows A-pose arm | X -50/-17 degrees |
| HB_Thigh_L/R | Straight | X -14; Y -3/+3 degrees |
| HB_Shin_L/R | Straight | X +28 degrees |
| HB_Boot_L/R | Planted | X -14; Y +3/-3; Z +4/-4 degrees |
| Forearm_L_socket | Rigid forearm attachment | X +56 degrees, compensates shield pitch |
| HB_Shield | Z -8 degrees | Additional Z -12 degrees |
| HB_Sword | World down | World Euler X -25, Y +38, Z 0, converted to local parent rotation |
| HB_Pelvis | Rest Z=1.104 | Y +0.06 m; initial Z -0.033 then exact sole alignment |

Sword is about 44.4 degrees from vertical, down-forward/outward. Feet center separation 0.4926 m; boots turned outward. Display body height 2.373144 m, pelvis Z 1.074488: lower than rest. Plate origins recentered to anatomical pivots while preserving children world matrices; same parent hierarchy and object names in both poses.

## Камера и силуэт

[Decision](sheet-p3.png), [P2/P3](sheet-p2-vs-p3.png), [8 facings](sheet-p3-directions.png). Judging renders use display. Fixed game camera pitch 56, yaw 0, FOV 45 vertical, distance 19, 1920x1080; HB_Root rotates 0..315 by 45. Eight actual full frames and geometry alpha masks retained. 110/135/160 crops normalize full equipped silhouette height, не обещают native gameplay pixel size. P2 comparison uses same camera/light/3.10 orthographic scale, but its original rest pose; body geometry untouched.

[Silhouette numbers](silhouette-metrics-p3.json), [concept threshold mask](concept-front-threshold-mask.png), [overlay](concept-front-threshold-overlay.png). Background RGB plane fitted to five known background patches; Euclidean threshold 32/255, closing 2 px, components >=1200 px, holes filled. Landmark ROI excludes source titles/rule and floor shadow outside boot regions. Sensitivity thresholds 28/32/36 retained. AI perspective, manually selected ROI and residual shadow at soles limit precision. P3 geometry masks are alpha-thresholded at 128, not extracted from clay luminance.

12% gate result and reviewer scores are authoritative in REPORT-2.md, with current exact metrics. Не деформировать PNG, чтобы искусственно получить PASS. The model still has a more vertical equipment arrangement and narrower full silhouette than the AI concept; matching both ratios has not been silently assumed.

## Rig readiness

Это статичная hierarchical study, не direct-bind-ready asset. Нет armature, skin weights, deformation loops, cloth controls или clip simulation.

- Under-arm/shoulder transition: HB_UpperArm/Elbow/Torso нужны skin weights и достаточные loops. Три HB_Pauldron shells должны оставаться в основном rigid, с отдельным shoulder-follow/lift control, чтобы не входить в gorget/chest при подъеме руки.
- Elbow: HB_Forearm/Elbow сочленение должно деформироваться; HB_Couter/Vambrace/Knuckle rigid, с проверкой сгиба и контакта cuff. Cups сейчас следуют upper arm, не сгибаются сами.
- Hip: HB_Pelvis/Thigh переход требует skin weights; Fauld/Tabard и pouches нуждаются в отдельных контролях или ограниченном rigid follow. Верх cuisses может попасть в lower fauld при глубоком сгибе.
- Knee: HB_Knee/Thigh/Shin transition нужен deformable base; Poleyn rigid attachment/secondary follow должен перекрывать joint, не входить в Greave при сгибе. Sabaton должен отдельно следовать foot/ankle.
- Idle: небольшой donor sway может ввести rigid shoulder edge в collar, shield interior в gauntlet. Это предполагаемый риск, не replay result.
- Run: чередование бедер и сгиб коленей опасны для fauld/cuisse и poleyn/greave; крупный shield может пересекать левое бедро. Нужны foot contacts и проверка рукавов.
- Attack: shoulder elevation и elbow flexion могут столкнуть верхнюю pauldron с gorget, couter с vambrace и shield с корпусом. Grip и crossguard attachment нужно проверить на source Weapon.R/Fist chain.
- Death: глубокий сгиб/падение повышает риск всех перечисленных кромок и shield/ground contact. Никаких measured collision/clip PASS здесь нет.

### Pivot vs donor joint

Ближайшая анатомическая donor bone, не произвольный nearest-neighbor по всем helper bones. Donor T-pose source высотой 2.941792 нормализован к 2.4: translated sole +0.003276, scale 0.81582926. P3 в A-pose. Offset включает разницу поз и пропорций; это не rig-error score. Donor origin/matrices исходного прохода, не свежий импорт. Raw values и axes доступны в donor-facts.md, normalized table в [rig-readiness-p3.json](rig-readiness-p3.json).

| Part | P3 rest pivot XYZ m | Nearest donor bone | Normalized donor head XYZ m | Offset m |
| --- | --- | --- | --- | --- |
| HB_Pelvis | 0.0000, 0.0000, 1.1040 | Hips | -0.0005, 0.0498, 0.8395 | 0.2692 |
| HB_Torso | 0.0000, 0.0000, 1.7400 | Torso | -0.0005, 0.0285, 1.2736 | 0.4673 |
| HB_Helmet | 0.0000, 0.0000, 2.0800 | Head | -0.0005, 0.0025, 1.7381 | 0.3419 |
| HB_Pauldron_L | 0.4060, 0.0000, 1.9400 | UpperArm.L | 0.2607, 0.0098, 1.5269 | 0.4381 |
| HB_Pauldron_R | -0.4060, 0.0000, 1.9400 | UpperArm.R | -0.2607, 0.0098, 1.5269 | 0.4381 |
| HB_UpperArm_L | 0.4060, 0.0000, 1.9400 | UpperArm.L | 0.2607, 0.0098, 1.5269 | 0.4381 |
| HB_UpperArm_R | -0.4060, 0.0000, 1.9400 | UpperArm.R | -0.2607, 0.0098, 1.5269 | 0.4381 |
| HB_Forearm_L | 0.5581, 0.0000, 1.6137 | LowerArm.L | 0.5801, 0.0283, 1.5269 | 0.0940 |
| HB_Forearm_R | -0.5581, 0.0000, 1.6137 | LowerArm.R | -0.5801, 0.0283, 1.5269 | 0.0940 |
| HB_Couter_L | 0.5581, 0.0000, 1.6137 | LowerArm.L | 0.5801, 0.0283, 1.5269 | 0.0940 |
| HB_Couter_R | -0.5581, 0.0000, 1.6137 | LowerArm.R | -0.5801, 0.0283, 1.5269 | 0.0940 |
| Hand_R_socket | -0.7251, -0.0180, 1.2557 | Fist.R | -0.9473, 0.0349, 1.5182 | 0.3480 |
| Forearm_L_socket | 0.6384, -0.0180, 1.4415 | LowerArm.L | 0.5801, 0.0283, 1.5269 | 0.1133 |
| HB_Thigh_L | 0.1980, 0.0000, 1.1040 | UpperLeg.L | 0.1601, 0.0300, 0.9471 | 0.1642 |
| HB_Thigh_R | -0.1980, 0.0000, 1.1040 | UpperLeg.R | -0.1658, 0.0300, 0.9471 | 0.1630 |
| HB_Poleyn_L | 0.1980, 0.0000, 0.5900 | LowerLeg.L | 0.1674, -0.0080, 0.4984 | 0.0969 |
| HB_Poleyn_R | -0.1980, 0.0000, 0.5900 | LowerLeg.R | -0.1730, -0.0080, 0.4984 | 0.0953 |
| HB_Shin_L | 0.1980, 0.0000, 0.5900 | LowerLeg.L | 0.1674, -0.0080, 0.4984 | 0.0969 |
| HB_Shin_R | -0.1980, 0.0000, 0.5900 | LowerLeg.R | -0.1730, -0.0080, 0.4984 | 0.0953 |
| HB_Boot_L | 0.1980, 0.0100, 0.1500 | Foot.L | 0.1685, 0.0369, 0.0208 | 0.1352 |
| HB_Boot_R | -0.1980, 0.0100, 0.1500 | Foot.R | -0.1741, 0.0369, 0.0208 | 0.1341 |

Для direct bind нужны: перевод A-pose в donor T/rest matrices либо явный retarget+bake; сохранение точных Legacy transform paths и scale curves; пропорциональная подгонка joints (особенно shoulder/hip/head), weights и armature; нормальные grips на Hand_R_socket/Forearm_L_socket; отдельные follow controls для plate lames/tabard; проверка Idle/Run/Attack/Death в Blender перед любым будущим authorized integration. Простое переименование parts или clips не делает текущую геометрию совместимой. Scene/source содержит только оригинальные meshes, без чужого рига.
