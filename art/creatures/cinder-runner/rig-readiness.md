# R1 / готовность к ригу

Это статический blockout, без armature, weights, actions, FBX и проигрывания clips. Совпадение part stems не означает bind-совместимость.

Источник donor facts: `art/heroes/breakwater/donor-facts.md`; snapshot `.local/codex-tasks/hero-b/donor-probe.json`, SHA256 `9384effea164e40511f16a6ea7defca3ed27a58c06f4578047e291d687fa9126`. 44 bones, T-pose, перед -Y. Snapshot прочитан; donor FBX и Unity не импортировались.

Сравнение head_world после единообразной нормализации к2.1 м: factor=0.71385065, исходная высота2.941792 м, donor ground Z=-0.003276 м. XYZ=(rawX*s,rawY*s,(rawZ-ground)*s). Это условное сравнение pivots, не решение retarget. Данные R1 - реальные world pivots rest из validation.json.

| Часть | Donor bone | R1 pivot XYZ, м | Donor normalized head XYZ, м | Delta XYZ, м | Длина delta, м |
| --- | --- | --- | --- | --- | ---: |
| R1_Root | Root | 0.00000, 0.00000, 0.00000 | -0.00094, 0.00008, 0.00061 | 0.00094, -0.00008, -0.00061 | 0.00112 |
| R1_Pelvis | Hips | 0.00000, 0.00000, 1.04000 | -0.00046, 0.04355, 0.73455 | 0.00046, -0.04355, 0.30545 | 0.30854 |
| R1_Torso | Torso | 0.00000, -0.02500, 1.22000 | -0.00046, 0.02493, 1.11440 | 0.00046, -0.04993, 0.10560 | 0.11682 |
| R1_Head | Head | 0.00000, -0.07000, 1.78000 | -0.00046, 0.00222, 1.52080 | 0.00046, -0.07222, 0.25920 | 0.26907 |
| Hand_R_socket | Weapon.R | -0.62500, -0.06500, 0.95500 | -0.90653, 0.04550, 1.33131 | 0.28153, -0.11050, -0.37631 | 0.48278 |
| R1_UpperArm_L | UpperArm.L | 0.28000, -0.02500, 1.65000 | 0.22810, 0.00861, 1.33600 | 0.05190, -0.03361, 0.31400 | 0.32003 |
| R1_Forearm_L | LowerArm.L | 0.45500, -0.02000, 1.32000 | 0.50763, 0.02479, 1.33600 | -0.05263, -0.04479, -0.01600 | 0.07094 |
| R1_Hand_L | Fist.L | 0.59000, -0.05500, 1.05500 | 0.82885, 0.03053, 1.32847 | -0.23885, -0.08553, -0.27347 | 0.37303 |
| R1_Thigh_L | UpperLeg.L | 0.18000, 0.00000, 1.04000 | 0.14010, 0.02622, 0.82869 | 0.03990, -0.02622, 0.21131 | 0.21663 |
| R1_Shin_L | LowerLeg.L | 0.18000, -0.01500, 0.55000 | 0.14647, -0.00697, 0.43606 | 0.03353, -0.00803, 0.11394 | 0.11904 |
| R1_Foot_L | Foot.L | 0.18000, 0.01500, 0.13000 | 0.14741, 0.03224, 0.01822 | 0.03259, -0.01724, 0.11178 | 0.11770 |
| R1_UpperArm_R | UpperArm.R | -0.28000, -0.02500, 1.65000 | -0.22810, 0.00861, 1.33600 | -0.05190, -0.03361, 0.31400 | 0.32003 |
| R1_Forearm_R | LowerArm.R | -0.45500, -0.02000, 1.32000 | -0.50763, 0.02479, 1.33600 | 0.05263, -0.04479, -0.01600 | 0.07094 |
| R1_Hand_R | Fist.R | -0.59000, -0.05500, 1.05500 | -0.82885, 0.03053, 1.32847 | 0.23885, -0.08553, -0.27347 | 0.37303 |
| R1_Thigh_R | UpperLeg.R | -0.18000, 0.00000, 1.04000 | -0.14505, 0.02622, 0.82869 | -0.03495, -0.02622, 0.21131 | 0.21578 |
| R1_Shin_R | LowerLeg.R | -0.18000, -0.01500, 0.55000 | -0.15142, -0.00697, 0.43606 | -0.02858, -0.00803, 0.11394 | 0.11774 |
| R1_Foot_R | Foot.R | -0.18000, 0.01500, 0.13000 | -0.15236, 0.03224, 0.01822 | -0.02764, -0.01724, 0.11178 | 0.11643 |

## Что потребуется

- Rest A-pose: верхние руки28 градусов от вертикали, donor T-pose. Нужен перевод A->T до bind или отдельный rest-aware offline retarget. Forearm/hand offsets значительные и ожидаемые; копирование matrices недостаточно.
- Head, collar plates, shin/thigh plates, boots и cleaver - rigid attachments. Underbody torso/abdomen, upper arms, elbows, thighs/knees требуют deformation weights и проверки швов. Нынешняя отдельная rigid геометрия не дает качественного сгиба сама.
- Hand_R_socket реально parent=R1_Forearm_R, cleaver parent=socket. Donor использует Fist.R и Weapon.R, не Hand.R. Будущий wrist/Fist.R bone должен принять grip socket; current part origins и hand center не являются IK-контрактом.
- Cloth_L/R требуют по1-2 дополнительных кости либо привязки верхней части к pelvis и отдельного bake. Collar scarf rigid для blockout; для Run возможна1-2 дополнительные кости или ограниченный rigid follow. Ember underside следует за collar, отдельные VFX не нужны.
- Idle: сохранить наклон и контакт ступней, проверить дыхание и ворот. Run: плечи/колени/две полы, таз, slide подошв. Attack: свой cleaver swing, контакт кисти, clearance голени и recovery; donor Attack не проверен. Death: упасть без проникающих plates/cleaver, runtime удаление creep через0.85s только исторический контракт из brief.
- Имена Idle/Run/Attack/Death, Legacy paths и rest matrices должны соответствовать выбранному экспортному маршруту. Никакой runtime-готовности здесь не заявлено.

## Оценка усилий

Для опытного character artist после принятия формы: примерно2-4 рабочих дня на A->T, torso/limb weights, grip и cloth bones; еще1-2 дня на правки четырех clips, контакт и экспортные проверки. Это плановая оценка, не измеренная автономная скорость. UV/atlas bake и художественная переработка в нее не входят.

Фактическая geometry: rest 2.100000 м, display 1.822776 м; 7352 evaluated triangles; радиусы 0.325685/0.428154 м.

Пять look-test materials нужно запечь в1-2 atlas materials. UV, normal/AO/albedo/roughness/emission bake, collision и animated bounds не выполнены.
