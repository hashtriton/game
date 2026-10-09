# Breakwater - numeric blockout specification

Дата: 2026-10-09. Источник: собственный выбранный концепт B, `art/concepts/2026-10-09-hero-b-breakwater-source-v1.png` (1672x941). Это AI guidance, не ортографическая 3D-истина. Геометрия создается заново, donor mesh не переносится.

## Измерение концепта

Pillow прочитал source; landmarks размечены по пикселям в `.local/codex-tasks/hero-b/concept-measurements.json` и overlay PNG. Погрешность ручных landmarks около 5-10 px, перспективный размер частей не равен метрам.

| Landmark | Pixel box x0,y0,x1,y1 | Размер | Относительно body height 561 px |
| --- | --- | --- | --- |
| Helmet top to sole | 223,68,579,629 | 561 px height | 1.000 |
| Helmet | 367,68,447,166 | 80x98 px | 0.175 height |
| Shoulder plates | 244,122,568,254 | 324 px span | 0.578 width |
| Torso visible | 292,177,512,330 | 220x153 px | 0.273 height |
| Hip to sole | 222,356,583,629 | 273 px height | 0.487 height |
| Shield | 513,150,677,614 | 164x464 px | 0.827 height |

Концепт имеет примерно 5.72 helmet heights. Нельзя механически принять эти размеры: front 3/4 и AI shield несогласованы с footprint. Используем около 6.67 helmet heights для P1 и 6.35 для P2, более компактный щит и корпус внутри 0.8 м круга.

## Coordinates and pose

- Units METRIC, scale_length=1. One unit=1 m. Origin between soles, sole Z=0, helmet top Z=2.4 exactly. Front -Y, left +X, right -X, Z up.
- Compact relaxed A-pose: arms about 12 degrees from vertical, elbows slightly forward, hands at Z=1.20. It is for shape review, not a final bind configuration. Donor is T-pose; future skin must be transformed to its rest matrices before bind, or retargeted and baked offline. No promise of direct clip reuse from A-pose.
- Rigid sword parent: Hand_R_socket; shield and stripe parent chain: Forearm_L_socket -> HB_Shield -> HB_Shield_Stripe. Sockets parent the relevant forearm, with world transform preserved.
- Standing body fits 2.4 m. Arms/shoulders are outside the collision cylinder; the explicit torso/legs footprint is checked by evaluated vertices. It is a visual bound, not a collider change.
- FBX donor reports Y-up, FrontAxis=Z sign +, centimetre UnitScaleFactor=1. Blender import converts it to Z-up metres. Unity meta: globalScale=1, useFileUnits/useFileScale=1, bakeAxisConversion=0, animationType=1. Intended later export axes forward=-Z/up=Y maps Blender (x,y,z) to Unity (x,z,-y); front -Y becomes +Z, sole Z=0 becomes Y=0. This convention is a plan, not a performed export or Unity import.

## Initial dimensions in metres (before S5)

Widths/depths are maximum unrotated local envelopes; final evaluated world bounds and triangle counts are authoritative in validation JSON. Bevel trims corners, not plate steps. Script header is the executable numeric source.

| Part | P1 width x depth x height | P2 change |
| --- | --- | --- |
| Helmet | 0.300 x 0.360 x 0.360 | Uniform dimensions x1.05; top stays 2.4 |
| Torso | 0.740 x 0.460 x 0.700 | Width x0.96, depth x0.90, waist +0.066 |
| Back | 0.580 x 0.095 x 0.530 | Same torso factors |
| Shoulder span | 1.100 nominal | x0.92, including plate centers |
| Each pauldron | 0.350 x 0.420 x 0.345 | Width x0.92; L 3 percent larger depth, small angle offset |
| Upper arm | 0.225 x 0.255 x 0.365 | X positions narrower, same joint segmentation |
| Forearm with gauntlet | 0.250 x 0.285 x 0.440 | Same |
| Pelvis | 0.540 x 0.365 x 0.260 | Hip +0.066 |
| Split tabard | 0.445 x 0.055 x 0.390 | Bottom +0.066, same short split |
| Thigh each | 0.285 x 0.310 x 0.480 | Legs ground-to-hip 1.100 -> 1.166 (+6 percent) |
| Shin each | 0.285 x 0.320 x 0.500 | Leg Z values scaled 1.06 |
| Boot each | 0.280 x 0.420 x 0.260 | Same footprint, upper leg boundary adjusted |
| Belt | 0.595 x 0.405 x 0.095 | Waist +0.066 |
| Pouches | Two, 0.125 x 0.095 x 0.165 | Waist +0.066 |
| Shield | 0.460 x 0.115 x 1.640 | Same; top 1.78, bottom 0.14, close to forearm |
| Sword | Blade 0.145 wide, 0.036 thick, 0.790 long; grip 0.190, guard 0.260 | Same |

Torso/leg max radius <=0.4; neutral full X span <=about1.3 including sword/shield. Shield may protrude in Y and its overreach is measured. Sword points down-forward instead of far outward to avoid violating the width limit. Chest is a tapered keystone with a raised center and stepped plate edges, not a barrel sphere. Helmet face is a closed tapered plate with an actual narrow recessed visor; no eyes or open face.

## Proportion landmarks

| Landmark Z | P1 | P2 |
| --- | --- | --- |
| Sole | 0 | 0 |
| Boot top | 0.260 | 0.276 |
| Knee center | 0.610 | 0.647 |
| Hip | 1.100 | 1.166 |
| Belt center | 1.340 | 1.406 |
| Shoulder center | 1.975 | 1.975 |
| Helmet bottom | 2.040 | 2.022 |
| Helmet top | 2.400 | 2.400 |

## Construction limits and decisions

- Big extruded/lofted forms, bevel modifiers only. No smoothing subdivision, decimation, UV, textures, rig, actions, export or cape. All armour pieces named HB_. Independent left/right meshes permit restrained asymmetry without a live Mirror dependency.
- Two materials total: neutral grey clay and flat non-emissive cyan stripe. No dark face material: visor reads through geometry/shadow.
- Bevel widths: chest 0.012, back 0.009, pauldrons 0.018/0.015, helmet 0.006, limb plates 0.010/0.008, boots 0.008, belt 0.003, pouches 0.006, shield 0.016, sword 0.0025 m. These are part-specific, never a global identical round-over.
- Overlapping rigid plate lips are deliberate static construction. No rows of rivets or small procedural surface detail. Non-spherical joint sleeves remain segmented, not polished ball joints.
- P1 follows compact concept mass within constraints; P2 narrows shoulders 8 percent, lengthens legs 6 percent, increases helmet 5 percent and reduces torso depth 10 percent at constant height.
- Final evaluated bounds after S5/S6 are recorded below and in validation JSON. Artistic approval belongs to the user.

## Final evaluated values

После S5: направленные слоеные наплечники, грудь 0.78 nominal, приподнятый ворот, closed rear helmet bridge, бедра полнее, юбка укорочена до 0.31 м, щит сужен книзу, клинок 0.18 м. Высота 2.4 м сохранена. Ширина фаски накладки щита 0.0015 м вместо 0.006: это устранило collapse тонкого mesh, а не снизило порог проверки. Общая фаска щита 0.016 м, направленные наплечники 0.012/0.010 м. Остальные фаски как выше.

| Final metric | P1 | P2 |
| --- | --- | --- |
| body_height | 2.400000095 | 2.400000080 |
| body_z_min | 0.000000000 | 0.000000015 |
| body_z_max | 2.400000095 | 2.400000095 |
| footprint_radius | 0.383151510 | 0.367242033 |
| total_width | 1.290091634 | 1.215851605 |
| total_triangles | 4988.000000000 | 4988.000000000 |

| Rigid equipment maximum radial overreach beyond 0.4 m | P1 m | P2 m |
| --- | --- | --- |
| HB_Shield | 0.388292631 | 0.358476209 |
| HB_Sword | 0.322959456 | 0.292809805 |

| Part final world envelope X x Y x Z (m) | P1 | P2 |
| --- | --- | --- |
| HB_Back | 0.563027 x 0.110002 x 0.530000 | 0.539831 x 0.106808 x 0.464000 |
| HB_Belt | 0.591026 x 0.402899 x 0.095000 | 0.591026 x 0.402899 x 0.095000 |
| HB_Boot_L | 0.280000 x 0.411381 x 0.260000 | 0.280000 x 0.411398 x 0.275600 |
| HB_Boot_R | 0.280000 x 0.411381 x 0.260000 | 0.280000 x 0.411398 x 0.275600 |
| HB_Buckle | 0.085000 x 0.037000 x 0.068000 | 0.085000 x 0.037000 x 0.068000 |
| HB_ChestLip | 0.741521 x 0.469131 x 0.075000 | 0.711466 x 0.422628 x 0.075000 |
| HB_Collar | 0.452294 x 0.361482 x 0.164000 | 0.452294 x 0.361482 x 0.164000 |
| HB_Elbow_L | 0.199909 x 0.216376 x 0.076000 | 0.199909 x 0.216375 x 0.076000 |
| HB_Elbow_R | 0.199909 x 0.216376 x 0.076000 | 0.199909 x 0.216375 x 0.076000 |
| HB_Forearm_L | 0.245400 x 0.291718 x 0.410000 | 0.244470 x 0.291720 x 0.410000 |
| HB_Forearm_R | 0.245400 x 0.291718 x 0.410000 | 0.244470 x 0.291720 x 0.410000 |
| HB_Gauntlet_L | 0.191624 x 0.207970 x 0.170000 | 0.191625 x 0.207970 x 0.170000 |
| HB_Gauntlet_R | 0.191624 x 0.207970 x 0.170000 | 0.191625 x 0.207970 x 0.170000 |
| HB_Helmet | 0.299271 x 0.368690 x 0.360000 | 0.314271 x 0.387010 x 0.378000 |
| HB_Knee_L | 0.288584 x 0.325107 x 0.154000 | 0.288855 x 0.325387 x 0.163240 |
| HB_Knee_R | 0.288584 x 0.325107 x 0.154000 | 0.288855 x 0.325387 x 0.163240 |
| HB_PauldronLip_L | 0.297582 x 0.340837 x 0.117000 | 0.273761 x 0.340530 x 0.117000 |
| HB_PauldronLip_R | 0.297582 x 0.340837 x 0.117000 | 0.273761 x 0.340530 x 0.117000 |
| HB_Pauldron_L | 0.351190 x 0.369770 x 0.364433 | 0.323387 x 0.369770 x 0.363435 |
| HB_Pauldron_R | 0.353225 x 0.359000 x 0.365927 | 0.324753 x 0.359000 x 0.365410 |
| HB_Pelvis | 0.571083 x 0.365000 x 0.260000 | 0.571083 x 0.365000 x 0.260000 |
| HB_Pouches | 0.613514 x 0.113179 x 0.165000 | 0.613514 x 0.113179 x 0.165000 |
| HB_Shield | 0.465436 x 0.173386 x 1.640000 | 0.465436 x 0.173386 x 1.640000 |
| HB_ShieldPanel | 0.410528 x 0.064127 x 1.571120 | 0.410528 x 0.064127 x 1.571120 |
| HB_Shield_Stripe | 0.033096 x 0.007564 x 1.566000 | 0.033096 x 0.007564 x 1.566000 |
| HB_Shin_L | 0.277170 x 0.311484 x 0.415000 | 0.277299 x 0.311584 x 0.439900 |
| HB_Shin_R | 0.277170 x 0.311484 x 0.415000 | 0.277299 x 0.311584 x 0.439900 |
| HB_Sword | 0.260000 x 0.463399 x 0.964379 | 0.260000 x 0.463399 x 0.964379 |
| HB_Tabard | 0.443824 x 0.049000 x 0.309683 | 0.443824 x 0.049000 x 0.309683 |
| HB_Thigh_L | 0.298587 x 0.322597 x 0.480000 | 0.298588 x 0.322623 x 0.508800 |
| HB_Thigh_R | 0.298587 x 0.322597 x 0.480000 | 0.298588 x 0.322623 x 0.508800 |
| HB_Torso | 0.765290 x 0.456000 x 0.690000 | 0.733449 x 0.410400 x 0.624000 |
| HB_UpperArm_L | 0.252054 x 0.251705 x 0.365000 | 0.249050 x 0.251712 x 0.365000 |
| HB_UpperArm_R | 0.252054 x 0.251705 x 0.365000 | 0.249050 x 0.251712 x 0.365000 |

37 scene objects per variant: 34 mesh parts, HB_Root and two sockets. No armature, animations, UV or image texture nodes. Modifiers are named Plate_edge and type BEVEL; Mirror was not needed because left/right meshes are independent. Every final object, parent, scale, bevel width/segments and evaluated triangle count is listed in validation-p1/p2.json.
P2 head height ratio is exactly 1.05, thigh upper landmark ratio 1.06. Shoulder contour X factor is 0.92, with slight deviations in world envelopes caused by deliberately different plate angles. Total equipped width ratio differs from shoulder ratio because equipment sizes remain unchanged.
The compact A-pose is about 12 degrees at the shoulder-to-elbow centerline. Hands are rigid simplified masses, not production grip topology. Current sword blade has no detailed edge bevel profile; only the big form is approved for comparison.
No artistic PASS: the independent reviewer prefers P2 but still sees mechanical segments, weak sword reading at 110 px and less mass than the concept. These are remaining shape risks, not hidden technical failures. No additional detail/rig pass is authorized here.
