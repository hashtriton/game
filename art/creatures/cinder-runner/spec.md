# Угольный гонец / R1

## Десять главных признаков

1. Худой поданный вперед корпус, без тяжелого круглого нагрудника.
2. Закрытый клиновидный капюшон с продольным гребнем и глубоким темным лицом.
3. Широкий рыже-оранжевый ворот, читаемый сверху как цельная зона.
4. Большой прямой тесак справа, вынесенный от голени.
5. Две короткие красные полы с промежутком, без лент и длинного плаща.
6. Узкая талия и отдельный пояс, без игрушечных шарообразных суставов.
7. Темные длинные бронированные ноги с перекрывающимися пластинами.
8. Крупные конструктивные наручи, закрытый хват справа и свободная кисть слева.
9. Умеренный жар под воротом и в двух швах, без bloom и светящихся глаз.
10. Общая стальная семья с героем, но вражеские темные массы и теплый цвет; риски наклона, воротника, ткани и swing тесака проверяются позже в риге.

## Пропорции и конструкция

| Параметр | План, м |
| --- | ---: |
| Rest height | 2.10 |
| Pelvis pivot Z | 1.04 |
| Knee pivot Z | 0.55 |
| Ankle pivot Z | 0.13 |
| Torso pivot Z | 1.22 |
| Shoulder pivot Z | 1.65 |
| Hood height | 0.36 |
| Torso width / depth | 0.48 / 0.32 |
| Collar span | 0.77 |
| Boot length / width | 0.34 / 0.17 |
| Cleaver blade length / width | 0.68 / 0.19 |
| Cloth length | 0.29 |

1 unit = 1 m, Z вверх, перед -Y, soles Z=0, rest A-pose 28 градусов. Display задается поворотом исходных pivots. Отдельные rigid parts R1_Head/Torso/Pelvis/UpperArm/Forearm/Thigh/Shin/Foot, дополнительные plates, cloth и Cleaver. Корень R1_Root; оружие под Hand_R_socket.

Пять procedural materials: dark basalt, blackened worn steel, rust-orange collar cloth, moderate ember underside, red cloth. После review ворот revision4 сделан матовой тканью поверх нижних plates. Object coordinates, Bevel edge wear и AO dirt на basalt/steel, low-contrast Wave weave на обеих тканях. UV/images отсутствуют; будущий bake в 1-2 atlas materials.

Свет и камеры через read-only import p4_render.setup: Sun 2.9, sky .48, ACES 2.0, exposure .7, pale floor, Cycles OptiX 48 denoise. Game camera pitch56/FOV45/distance19/1920x1080. Масштабированные crops 110/135/160 px отдельно от native mock.

## Финальный корректирующий проход

Revision4: torso lean34, head9 градусов, thigh24/knee45/foot21, leg spread4 и foot yaw8 градусов. Обе подошвы приводятся кZ=0 общим pelvis offset; никакого rebuild geometry при pose нет. Display ниже rest, основание rigid parts сохранено. Ступни удлинены до фактического shape length около.37 м, вместо исходного плана.34 м. Верхние orange shoulders имеют ridge/fold, нижняя plate остается steel. Два abdominal overlaps добавлены, shader steel roughness.57, cloth.78, ember strength.42. Никаких фактических клипов или deformation тестов нет.
