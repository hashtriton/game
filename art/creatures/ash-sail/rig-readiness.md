# Rig readiness: ash-sail

Static rigid blockout, без armature, weights, actions, UV, export или runtime проверки.
Facing -Y, Z up, 1 unit = 1 m. Rest A-pose 26 градусов, display состоит из поворотов joint pivots и малой корректировки soles.
Ни один rigid mineral/plate не должен деформироваться. Torso/limbs follow bones; Cloth_* требует 2-3 chain bones или отдельного offline bake.

## Фактические pivot offsets

Источник: сохраненный donor-facts.md, SHA256 556761cceec14b1c609709e13902df82b838f6fbe72368fcae1981dae0d21582. Свежий import donor не выполнялся: Unity paths запрещены brief.
Donor head positions умножены на 1.18975101 = 3.5/2.941792. Для E2 это размер с sail, поэтому таблица является диагностикой расхождения, а не инструкцией uniform scale bind.
Offsets = creep rest world pivot - scaled donor bone head. Donor T и creep A имеют разные axes/rest rotations. Совпадение имен не подключает Legacy paths.

| Part | Donor bone | Actual pivot m | Scaled donor head m | Delta XYZ m | Length m |
| --- | --- | --- | --- | --- | ---: |
| E2_Pelvis | Hips | 0.0000, 0.0000, 1.4321 | -0.0008, 0.0726, 1.2204 | 0.0008, -0.0726, 0.2118 | 0.2239 |
| E2_Torso | Torso | 0.0000, 0.0000, 2.1948 | -0.0008, 0.0416, 1.8534 | 0.0008, -0.0416, 0.3414 | 0.3439 |
| E2_Head | Head | 0.0000, 0.0000, 2.7744 | -0.0008, 0.0037, 2.5308 | 0.0008, -0.0037, 0.2436 | 0.2437 |
| E2_UpperArm_L | UpperArm.L | 0.4068, 0.0000, 2.5202 | 0.3802, 0.0144, 2.2228 | 0.0266, -0.0144, 0.2974 | 0.2989 |
| E2_Forearm_L | LowerArm.L | 0.6813, -0.0356, 1.9812 | 0.8460, 0.0413, 2.2228 | -0.1647, -0.0769, -0.2415 | 0.3023 |
| E2_Thigh_L | UpperLeg.L | 0.2339, 0.0153, 1.4321 | 0.2335, 0.0437, 1.3773 | 0.0004, -0.0284, 0.0549 | 0.0618 |
| E2_Shin_L | LowerLeg.L | 0.2796, -0.0153, 0.7508 | 0.2441, -0.0116, 0.7229 | 0.0355, -0.0036, 0.0279 | 0.0453 |
| E2_Foot_L | Foot.L | 0.2949, 0.0153, 0.1508 | 0.2457, 0.0537, 0.0265 | 0.0492, -0.0385, 0.1244 | 0.1392 |
| E2_UpperArm_R | UpperArm.R | -0.4068, 0.0000, 2.5202 | -0.3802, 0.0144, 2.2228 | -0.0266, -0.0144, 0.2974 | 0.2989 |
| E2_Forearm_R | LowerArm.R | -0.6813, -0.0356, 1.9812 | -0.8460, 0.0413, 2.2228 | 0.1647, -0.0769, -0.2415 | 0.3023 |
| E2_Thigh_R | UpperLeg.R | -0.2339, 0.0153, 1.4321 | -0.2418, 0.0437, 1.3773 | 0.0079, -0.0284, 0.0549 | 0.0623 |
| E2_Shin_R | LowerLeg.R | -0.2796, -0.0153, 0.7508 | -0.2524, -0.0116, 0.7229 | -0.0273, -0.0036, 0.0279 | 0.0392 |
| E2_Foot_R | Foot.R | -0.2949, 0.0153, 0.1508 | -0.2539, 0.0537, 0.0265 | -0.0410, -0.0385, 0.1244 | 0.1365 |
| Hand_R_socket | Weapon.R | -0.9762, -0.0915, 1.2999 | -1.5109, 0.0758, 2.2149 | 0.5347, -0.1674, -0.9150 | 1.0729 |

## Специальные части и clips

Sail жесткий Torso attachment. Дополнительная bone не нужна для rigid follow, но tip sweep и ground contact в Death требуют контроля; one-sided flexible cape не используется.
Cleaver rigid parent Hand_R_socket, socket под Forearm_R. После conversion назначить Weapon.R, проконтролировать wrist/grip и weapon path, а не переносить текущую world carry compensation в runtime.
Idle: sail/head и vambrace/chest. Run: blade/leg clearance и cloth. Attack: crescent blade sweep, увеличенный forearm, tip через sail. Death: rigid sail может первым упереться в пол, потребуется bake collision/Root correction.
Оценка опытного rigger: 2-3 рабочих дня bind/rest conversion + еще 2-4 для четырех clips, хватов и контактов. Это оценка, не измерение этой сессии.

A -> donor T conversion либо offline retarget+bake обязателен. Сохранить 44 donor bones, точные transform paths и Legacy Idle/Run/Attack/Death contract из donor-facts. В этой задаче ни один clip не проигрывался.
Сейчас техническая структура пригодна как вход для отдельного rig pass. Художественное принятие определяется review, не geometry PASS. После bind повторить все facings/contacts и клипы.
Пять procedural look-test slots должны быть baked в 1-2 atlas materials; bake и game performance не проверены.
