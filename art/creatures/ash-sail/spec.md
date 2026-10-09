# E2: Пепельный парус

## C0: десять приоритетов

1. Масса: высокий plate biped с длинными ногами, тонким waist и тяжелой левой наручью.
2. Силуэт: одна диагональ паруса против вынесенного вправо по анатомии crescent cleaver.
3. Sail: один жесткий dorsal slab, не cape и не пара симметричных крыльев.
4. Ткань: короткий красный front flap и два боковых; не длинный плащ.
5. Head: закрытая асимметричная blade mask, небольшая относительно sail.
6. Hands: левый fist под большой наручью, правый rigid grip вокруг cleaver handle.
7. Feet: длинные голени, компактные широкие toes, soles на Z=0.
8. Ember: оранжевый underside паруса, небольшой face seam и кромка blade, без bloom.
9. Family: blackened steel/basalt/red/orange, контраст с white/navy/cyan hero P4.
10. Animation: sail/head/forearm clearance, cleaver отдельно от ноги, четыре donor clips требуют retarget и проверки контактов.

## Пропорции, проектные метры до точной нормализации

| Часть | Размер или pivot |
| --- | --- |
| Полная высота rest с sail | 3.500 |
| Head top без sail | около 3.160 |
| Sail | диагональ от x -0.55/z 2.35 к x 1.05/z 3.50, толщина 0.16 |
| Torso | ширина0.72, глубина0.47, torso pivot z2.18 |
| Pelvis pivot / knee / ankle | 1.43 / 0.76 / 0.17 |
| Shoulder pivots | x +/-0.40, z2.50 |
| Elbow pivots | x +/-0.67, z1.97 |
| Forearm + hand | длина0.68, enlarged left width0.40 |
| Rest arms | около26 градусов от тела |
| Cleaver | handle0.75, crescent около0.88, lateral extent x -1.95 |
| Foot | 0.33 x 0.49 x 0.28 |
| Ground circle | radius <=0.60, весь torso/leg armor и cloth, без рук/sail/blade |

Subdivision with creases and selected bevel weights, real plate/cloth thickness, overlapping armor. Up to five procedural slots; later bake 1-2 atlas materials. No armature, animation, UV or texture bake. Exact measured values are in validation.json.
