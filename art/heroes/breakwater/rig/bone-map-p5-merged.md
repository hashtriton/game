# Bone map

Strategy B. One vertex group per mesh, weight 1.0 for every vertex. No donor geometry exported.

Arm chains change A to T by setting upper-arm Y to -90 (L), +90 (R), retaining all child/socket matrices. Evaluated source modifiers are frozen before binding; no source file is saved.

Donor bone head/tail directions and roll are retained; head targets use the table below. Tail length uses first anatomical child distance or hip ratio. All 44 imported bones including existing _end bones remain; no additional leaf bones. FBX -Z forward/Y up, scale 1, FBX_SCALE_ALL encodes units without 100x node scale. Unity import has no extra scale.

Body and Hips share the pelvis anchor because this model has no separate abdomen pivot. Neck follows the collar, Head helmet. Shoulder plates bind to proximal Shoulder to avoid the full upper-arm lift hitting the helmet. Joint cups use the proximal bone. Foot controls stay parented to Root; baked positions follow the animated shin ankle so no rigid boot separates. Per frame, negative minimum Z across evaluated boot and sabaton vertices only is cancelled by a common Root vertical lift. Equipment never contributes to the lift; fallen equipment can penetrate the floor; it is not planted-foot IK or terrain adaptation.

| Bone | Pivot rule | Head XYZ m |
| --- | --- | --- |
| Root | origin | 0.000000, 0.000000, 0.000000 |
| Body | HB_Pelvis | 0.000000, 0.000000, 1.104000 |
| Hips | HB_Pelvis | 0.000000, 0.000000, 1.104000 |
| Abdomen | midpoint pelvis/torso | 0.000000, 0.000000, 1.422000 |
| Torso | HB_Torso | 0.000000, 0.000000, 1.740000 |
| Neck | HB_Collar | 0.000000, 0.000000, 2.080000 |
| Head | HB_Helmet | 0.000000, 0.000000, 2.080000 |
| Shoulder.L | HB_UpperArm_L | 0.406000, 0.000000, 1.940000 |
| UpperArm.L | HB_UpperArm_L | 0.406000, 0.000000, 1.940000 |
| LowerArm.L | HB_Forearm_L | 0.766000, 0.000000, 1.940000 |
| Fist.L | HB_Gauntlet_L | 1.136000, -0.018000, 1.940000 |
| UpperLeg.L | HB_Thigh_L | 0.198000, 0.000000, 1.104000 |
| LowerLeg.L | HB_Shin_L | 0.198000, 0.000000, 0.590000 |
| Foot.L | HB_Boot_L | 0.198000, 0.010000, 0.150000 |
| Shoulder.R | HB_UpperArm_R | -0.406000, 0.000000, 1.940000 |
| UpperArm.R | HB_UpperArm_R | -0.406000, 0.000000, 1.940000 |
| LowerArm.R | HB_Forearm_R | -0.766000, 0.000000, 1.940000 |
| Fist.R | HB_Gauntlet_R | -1.136000, -0.018000, 1.940000 |
| UpperLeg.R | HB_Thigh_R | -0.198000, 0.000000, 1.104000 |
| LowerLeg.R | HB_Shin_R | -0.198000, 0.000000, 0.590000 |
| Foot.R | HB_Boot_R | -0.198000, 0.010000, 0.150000 |
| Weapon.R | Hand_R_socket | -1.161000, -0.018000, 1.940000 |
| Foot.L_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 0.198000, -0.093945, 0.148782 |
| Head_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 0.000000, -0.000000, 2.689989 |
| Fist1.L | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 1.258861, -0.018000, 1.941941 |
| Fist2.L | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 1.362906, -0.018000, 1.930495 |
| Fist2.L_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 1.463017, -0.018000, 1.887910 |
| Thumb1.L | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 1.202233, 0.007785, 1.929768 |
| Thumb2.L | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 1.260207, -0.065694, 1.890219 |
| Thumb2.L_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 1.316506, -0.107081, 1.839546 |
| Fist1.R | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -1.258861, -0.018000, 1.941942 |
| Fist2.R | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -1.362906, -0.018000, 1.930495 |
| Fist2.R_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -1.463017, -0.018000, 1.887910 |
| Weapon.R_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -1.299784, -0.018000, 1.924731 |
| Thumb1.R | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -1.202233, 0.007785, 1.929768 |
| Thumb2.R | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -1.260207, -0.065694, 1.890220 |
| Thumb2.R_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -1.316507, -0.107082, 1.839547 |
| LowerLeg.L_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 0.198000, 0.053235, 0.022431 |
| LowerLeg.R_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -0.198000, 0.053235, 0.022431 |
| PoleTarget.L | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 0.198138, -0.933714, 0.702391 |
| PoleTarget.L_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | 0.198138, -0.933714, 0.967700 |
| Foot.R_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -0.198000, -0.093945, 0.148782 |
| PoleTarget.R | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -0.202318, -0.933714, 0.702391 |
| PoleTarget.R_end | anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise | -0.202318, -0.933714, 0.967700 |
| Shield.P5 | shield handle centre in the frozen T pose | 1.166680, -0.130504, 1.824381 |

| Mesh | Bone | Reason |
| --- | --- | --- |
| HB_Back | Torso | proximal joint plate or anatomical rigid segment |
| HB_Belt | Hips | proximal joint plate or anatomical rigid segment |
| HB_Boot_L | Foot.L | proximal joint plate or anatomical rigid segment |
| HB_Boot_R | Foot.R | proximal joint plate or anatomical rigid segment |
| HB_Breastplate | Torso | proximal joint plate or anatomical rigid segment |
| HB_BreathSlot_-1_0 | Head | proximal joint plate or anatomical rigid segment |
| HB_BreathSlot_-1_1 | Head | proximal joint plate or anatomical rigid segment |
| HB_BreathSlot_1_0 | Head | proximal joint plate or anatomical rigid segment |
| HB_BreathSlot_1_1 | Head | proximal joint plate or anatomical rigid segment |
| HB_Brow | Head | proximal joint plate or anatomical rigid segment |
| HB_Buckle | Hips | proximal joint plate or anatomical rigid segment |
| HB_Cheek_L | Head | proximal joint plate or anatomical rigid segment |
| HB_Cheek_R | Head | proximal joint plate or anatomical rigid segment |
| HB_ChestInlay | Torso | proximal joint plate or anatomical rigid segment |
| HB_ChestLip | Torso | proximal joint plate or anatomical rigid segment |
| HB_Collar | Neck | proximal joint plate or anatomical rigid segment |
| HB_Couter_L | UpperArm.L | proximal joint plate or anatomical rigid segment |
| HB_Couter_R | UpperArm.R | proximal joint plate or anatomical rigid segment |
| HB_CrownRidge | Head | proximal joint plate or anatomical rigid segment |
| HB_Cuisse_L | UpperLeg.L | proximal joint plate or anatomical rigid segment |
| HB_Cuisse_R | UpperLeg.R | proximal joint plate or anatomical rigid segment |
| HB_Elbow_L | UpperArm.L | proximal joint plate or anatomical rigid segment |
| HB_Elbow_R | UpperArm.R | proximal joint plate or anatomical rigid segment |
| HB_Faceplate | Head | proximal joint plate or anatomical rigid segment |
| HB_Fauld | Hips | proximal joint plate or anatomical rigid segment |
| HB_Fauld_2 | Hips | proximal joint plate or anatomical rigid segment |
| HB_Fauld_3 | Hips | proximal joint plate or anatomical rigid segment |
| HB_Forearm_L | LowerArm.L | proximal joint plate or anatomical rigid segment |
| HB_Forearm_R | LowerArm.R | proximal joint plate or anatomical rigid segment |
| HB_Gauntlet_L | Fist.L | proximal joint plate or anatomical rigid segment |
| HB_Gauntlet_R | Fist.R | proximal joint plate or anatomical rigid segment |
| HB_Gorget | Neck | proximal joint plate or anatomical rigid segment |
| HB_Greave_L | LowerLeg.L | proximal joint plate or anatomical rigid segment |
| HB_Greave_R | LowerLeg.R | proximal joint plate or anatomical rigid segment |
| HB_GripFingers_L | Fist.L | proximal joint plate or anatomical rigid segment |
| HB_GripFingers_R | Fist.R | proximal joint plate or anatomical rigid segment |
| HB_GripThumb_L | Fist.L | proximal joint plate or anatomical rigid segment |
| HB_GripThumb_R | Fist.R | proximal joint plate or anatomical rigid segment |
| HB_Helmet | Head | proximal joint plate or anatomical rigid segment |
| HB_Knee_L | UpperLeg.L | proximal joint plate or anatomical rigid segment |
| HB_Knee_R | UpperLeg.R | proximal joint plate or anatomical rigid segment |
| HB_Knuckle_L | Fist.L | proximal joint plate or anatomical rigid segment |
| HB_Knuckle_R | Fist.R | proximal joint plate or anatomical rigid segment |
| HB_NeckGuard | Head | proximal joint plate or anatomical rigid segment |
| HB_PauldronEdge_L | Shoulder.L | proximal joint plate or anatomical rigid segment |
| HB_PauldronEdge_R | Shoulder.R | proximal joint plate or anatomical rigid segment |
| HB_PauldronLame_L | Shoulder.L | proximal joint plate or anatomical rigid segment |
| HB_PauldronLame_R | Shoulder.R | proximal joint plate or anatomical rigid segment |
| HB_PauldronLip_L | Shoulder.L | proximal joint plate or anatomical rigid segment |
| HB_PauldronLip_R | Shoulder.R | proximal joint plate or anatomical rigid segment |
| HB_Pauldron_L | Shoulder.L | proximal joint plate or anatomical rigid segment |
| HB_Pauldron_R | Shoulder.R | proximal joint plate or anatomical rigid segment |
| HB_Pelvis | Hips | proximal joint plate or anatomical rigid segment |
| HB_Poleyn_L | UpperLeg.L | proximal joint plate or anatomical rigid segment |
| HB_Poleyn_R | UpperLeg.R | proximal joint plate or anatomical rigid segment |
| HB_Pouches | Hips | proximal joint plate or anatomical rigid segment |
| HB_Rim | Shield.P5 | socket equipment |
| HB_Sabaton_L | Foot.L | proximal joint plate or anatomical rigid segment |
| HB_Sabaton_R | Foot.R | proximal joint plate or anatomical rigid segment |
| HB_Shield | Shield.P5 | socket equipment |
| HB_ShieldHandle | Shield.P5 | socket equipment |
| HB_ShieldHandleMount_0 | Shield.P5 | socket equipment |
| HB_ShieldHandleMount_1 | Shield.P5 | socket equipment |
| HB_ShieldPanel | Shield.P5 | socket equipment |
| HB_ShieldStrap | Shield.P5 | socket equipment |
| HB_Shield_Stripe | Shield.P5 | socket equipment |
| HB_Shin_L | LowerLeg.L | proximal joint plate or anatomical rigid segment |
| HB_Shin_R | LowerLeg.R | proximal joint plate or anatomical rigid segment |
| HB_Sword | Weapon.R | socket equipment |
| HB_SwordGrip | Weapon.R | socket equipment |
| HB_SwordGuard | Weapon.R | socket equipment |
| HB_SwordPommel | Weapon.R | socket equipment |
| HB_Tabard | Hips | proximal joint plate or anatomical rigid segment |
| HB_Thigh_L | UpperLeg.L | proximal joint plate or anatomical rigid segment |
| HB_Thigh_R | UpperLeg.R | proximal joint plate or anatomical rigid segment |
| HB_Torso | Torso | proximal joint plate or anatomical rigid segment |
| HB_UpperArm_L | UpperArm.L | proximal joint plate or anatomical rigid segment |
| HB_UpperArm_R | UpperArm.R | proximal joint plate or anatomical rigid segment |
| HB_VambraceAccent_L | LowerArm.L | proximal joint plate or anatomical rigid segment |
| HB_VambraceAccent_R | LowerArm.R | proximal joint plate or anatomical rigid segment |
| HB_Vambrace_L | LowerArm.L | proximal joint plate or anatomical rigid segment |
| HB_Vambrace_R | LowerArm.R | proximal joint plate or anatomical rigid segment |
| HB_VisorShadow | Head | proximal joint plate or anatomical rigid segment |

Changing proportions requires joint/anchor validation, socket fit and all four motion sheets. Longer weapons need no new weights but need sweep/ground checks. Lower stance in an input display pose is not retained: supply the rest A-pose input. Arbitrary changed naming fails rather than guessing.

Translation ratio uses the mapped donor Body head height, the parent of UpperLeg, as pelvis reference. Donor Hips is a spine branch rather than the leg parent. This is a deliberate proportion approximation verified on P3, not a general limb-length retarget. Required-pivot and neutral-rest preflight reject display joints before any output is written.

Rigid shoulder/hip plates have no collision avoidance or secondary controls. This recipe proves transform/animation transport, not production deformation quality.

P5 addition: Death alone cancels the minimum across ALL geometry with an 0.008 m interpolation margin. Idle, Run and Attack retain exactly the feet-only Root lift. Equipment cannot raise a standing body. P3 and P4 defaults are unchanged.

P5 Shield.P5 is a secondary carrier parented to LowerArm.L. Its origin follows the source grip through the forearm chain. Standing clips retain the torso-relative upright orientation, capped at shoulder +0.18 m with a local mount correction only if needed. Death follows the forearm orientation. No anatomical donor bone or source clip is changed. Maximum vertical mount correction: 0 m.

P5 Run alone stabilizes UpperArm.L, LowerArm.L and Fist.L toward the donor Idle carry, retaining 10 percent of donor Run rotation. This prevents the unshielded donor arm swing from carrying the shield through the chest or sword. The original donor data and P3/P4 bake remain untouched.

Opt-in merge joins all HB parts after baking. One mesh, sorted unique material slots, rigid bone weights preserved. Source mapping above describes the parts before joining.
