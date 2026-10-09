# Rig readiness: basalt-yoke

Static rigid blockout, без armature, weights, actions, UV, export или runtime проверки.
Facing -Y, Z up, 1 unit = 1 m. Rest A-pose 26 градусов, display состоит из поворотов joint pivots и малой корректировки soles.
Ни один rigid mineral/plate не должен деформироваться. Torso/limbs follow bones; Cloth_* требует 2-3 chain bones или отдельного offline bake.

## Фактические pivot offsets

Источник: сохраненный donor-facts.md, SHA256 556761cceec14b1c609709e13902df82b838f6fbe72368fcae1981dae0d21582. Свежий import donor не выполнялся: Unity paths запрещены brief.
Donor head positions умножены на 0.98579369 = 2.9/2.941792. Для E2 это размер с sail, поэтому таблица является диагностикой расхождения, а не инструкцией uniform scale bind.
Offsets = creep rest world pivot - scaled donor bone head. Donor T и creep A имеют разные axes/rest rotations. Совпадение имен не подключает Legacy paths.

| Part | Donor bone | Actual pivot m | Scaled donor head m | Delta XYZ m | Length m |
| --- | --- | --- | --- | --- | ---: |
| B1_Pelvis | Hips | 0.0000, 0.0000, 0.9741 | -0.0006, 0.0601, 1.0112 | 0.0006, -0.0601, -0.0370 | 0.0706 |
| B1_Torso | Torso | 0.0000, 0.0000, 1.4564 | -0.0006, 0.0344, 1.5357 | 0.0006, -0.0344, -0.0793 | 0.0865 |
| B1_Head | Head | 0.0000, -0.3416, 2.3607 | -0.0006, 0.0031, 2.0969 | 0.0006, -0.3447, 0.2637 | 0.4340 |
| B1_UpperArm_L | UpperArm.L | 0.9847, 0.0000, 2.3305 | 0.3150, 0.0119, 1.8417 | 0.6697, -0.0119, 0.4888 | 0.8292 |
| B1_Forearm_L | LowerArm.L | 1.3263, -0.0151, 1.6473 | 0.7010, 0.0342, 1.8417 | 0.6253, -0.0493, -0.1944 | 0.6566 |
| B1_Thigh_L | UpperLeg.L | 0.2813, 0.0000, 0.9741 | 0.1935, 0.0362, 1.1412 | 0.0879, -0.0362, -0.1670 | 0.1922 |
| B1_Shin_L | LowerLeg.L | 0.3617, -0.0121, 0.5119 | 0.2023, -0.0096, 0.5990 | 0.1594, -0.0024, -0.0870 | 0.1817 |
| B1_Foot_L | Foot.L | 0.3919, 0.0100, 0.1603 | 0.2036, 0.0445, 0.0219 | 0.1883, -0.0345, 0.1383 | 0.2362 |
| B1_UpperArm_R | UpperArm.R | -0.9847, 0.0000, 2.3305 | -0.3150, 0.0119, 1.8417 | -0.6697, -0.0119, 0.4888 | 0.8292 |
| B1_Forearm_R | LowerArm.R | -1.3263, -0.0151, 1.6473 | -0.7010, 0.0342, 1.8417 | -0.6253, -0.0493, -0.1944 | 0.6566 |
| B1_Thigh_R | UpperLeg.R | -0.2813, 0.0000, 0.9741 | -0.2003, 0.0362, 1.1412 | -0.0810, -0.0362, -0.1670 | 0.1892 |
| B1_Shin_R | LowerLeg.R | -0.3617, -0.0121, 0.5119 | -0.2091, -0.0096, 0.5990 | -0.1526, -0.0024, -0.0870 | 0.1757 |
| B1_Foot_R | Foot.R | -0.3919, 0.0100, 0.1603 | -0.2104, 0.0445, 0.0219 | -0.1814, -0.0345, 0.1383 | 0.2308 |

## Специальные части и clips

Yoke и дочерние минералы жестко follow Torso. Extra bone не обязателен, но shoulder clearance нужно проверить на T conversion.
Forearm_* включает ладонь через rigid children Fist/Knuckle. Можно одним LowerArm influence, но donor Fist/Thumb paths должны сохраняться.
Idle: ярмо/голова и fists/chest. Run: кулаки/колени, короткие ноги, contact и excessive Root lift. Attack: sword donor motion не равен ударам кулаками; нужен offline retarget или собственная attack. Death: ярмо/ground и сохранение веса.
Оценка опытного rigger: 2-4 рабочих дня bind/rest conversion + еще 2-4 для четырех clips и контактов. Это оценка, не измерение этой сессии.

A -> donor T conversion либо offline retarget+bake обязателен. Сохранить 44 donor bones, точные transform paths и Legacy Idle/Run/Attack/Death contract из donor-facts. В этой задаче ни один clip не проигрывался.
Сейчас техническая структура пригодна как вход для отдельного rig pass. Художественное принятие определяется review, не geometry PASS. После bind повторить все facings/contacts и клипы.
Пять procedural look-test slots должны быть baked в 1-2 atlas materials; bake и game performance не проверены.
