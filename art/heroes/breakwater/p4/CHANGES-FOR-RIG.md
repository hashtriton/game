# P4: изменения для повторного rig pass

Исходник: [P3 rest](../blockout-p3.blend), P4: [rest](blockout-p4.blend) и [display](blockout-p4-display.blend). Это новый procedural look-test, не готовый animated asset. P3 файлы и скрипты не изменялись.

## Контракт

- Все 67 прежних объектов, включая HB_Root, Hand_R_socket и Forearm_L_socket, сохранены. Имена, родители и rest world pivots совпадают с P3 с точностью 1e-6 m. Никакой armature, action или weight painting нет.
- HB_Root стоит в origin; pivot pelvis/torso/helmet/upper arms/elbows/hips/knees/boots не перенесен. HB_Sword по-прежнему под Hand_R_socket, HB_Shield под Forearm_L_socket.
- Mesh data и часть modifiers заменены внутри прежних объектов. Существующий rig нельзя считать проверенным на новых vertex indices; нужно заново назначить weights и включить новые дочерние детали.
- Скрипт расширяет helpers build_p3 в памяти; display задает rest-relative rotations, возвращение rest идемпотентно. rest_location/rest_rotation/rest_parent_inverse сохранены. Фактическую re-run совместимость чужого rig script эта задача не проверяет.

## Измененная геометрия

- HB_Pauldron_L/R, HB_PauldronLip_L/R, HB_PauldronLame_L/R: три широкие перекрывающиеся sloped shell plates с толщиной .034 m; верхняя пластина более плоская после review. Добавлены HB_PauldronEdge_L/R, родители - исходные caps. Оценку художественного качества слоев см. в REPORT-3.
- HB_Sword: закрытый клинок .90x.22 m, shallow fuller с обеих сторон; outward winding. HB_SwordGuard/HB_SwordGrip изменены; HB_SwordPommel сохранен. Длина/вершины и поверхность для weights отличаются.
- HB_Shield/HB_Rim/HB_ShieldPanel/HB_Shield_Stripe: кривизна .135 m, shell .068 m, поднятая рамка .050 m; ширина .52/.40 m. Добавлены HB_ShieldHandle, HB_ShieldStrap и HB_ShieldHandleMount_0/1 под HB_Shield.
- HB_Brow: более тяжелая надбровная пластина. Добавлены HB_VisorShadow, HB_CrownRidge, HB_BreathSlot_-1_0/1 и HB_BreathSlot_1_0/1 под HB_Helmet. HB_Cheek_L/R и HB_NeckGuard сохранены как базовые отдельные plates.
- HB_Gauntlet_L/R и HB_Knuckle_L/R: новая ладонь и knuckle plate вокруг соответствующего grip. Новые HB_GripFingers_L/R и HB_GripThumb_L/R под gauntlets. В display gauntlet world transform вычисляется по sword/handle; этот procedural constraint следует заменить wrist bones/IK. Rest pivot остается исходным, shape center может быть смещен относительно pivot.
- HB_Couter_L/R и HB_Vambrace_L/R: новые cup/elongated plate shells; HB_VambraceAccent_L/R - новые cyan детали под vambraces.
- HB_Poleyn_L/R: после review заменены цельными curved knee cups, чтобы убрать треугольные прорези прежних patches.
- HB_Boot_L/R и HB_Sabaton_L/R: ширина уменьшена до .82 исходной. Внешние углы HB_Thigh/Cuisse/Knee/Poleyn/Shin/Greave/Boot/Sabaton подогнаны к радиусу .42 m через mesh coordinates в display, без переноса pivots. Следовательно требуется повторная проверка articulation и пересечений при движении.
- HB_ChestInlay под HB_Torso: новая металлическая вставка.

## Display и материалы

- Knee bends L20/R35 градусов; spread 5 градусов; yaw стоп 7 градусов, стопы .557 m друг от друга. Torso lean 8 градусов, head down 5 градусов. Обе подошвы приведены к ground; правый boot имеет небольшую pose translation для контакта с полом.
- Плечи/forearms задаются display rotations; socket L дополнительно компенсирует shield pitch на 48 градусов. Sword world carry выбран по eight-facing visibility probe, Euler -20/28/-7 градусов.
- Шесть материалов: painted white steel, brushed steel, navy cloth/leather, dark joint steel, cyan accent и restrained bronze. Object coordinates, без UV или внешних карт. Это look-test: игровой bake должен свести материалы к 1-2 atlas slots.
- [validation-p4.json](validation-p4.json) содержит geometry bounds, material slots, pivots, fingerprints и checks обеих поз. Reopen/rebuild evidence находится в .local/codex-tasks/hero-b.
