# raiders: два варианта

Дата: 2026-10-09. Tool: built-in image_gen.imagegen. Use case: stylized-concept.
Новая генерация, без входных изображений.

## Exact prompt, attempt 1

```text
Use case: stylized-concept. Create one ORIGINAL premium stylized 3D fantasy paired character decision sheet, landscape 1920x1080 requested. Title "RAIDERS / TWO SILHOUETTES". Warm daylight, cool soft shadows, neutral pale gray-blue background, believable sculpted large forms and materials, NO toy armor, chibi, cartoon smile, flat vector, franchise character, logo or emblem, no other-game reference. LEFT HALF is "R1 / CINDER RUNNER", RIGHT HALF is "R2 / SPLIT CREST". They are substantially different silhouettes and must stay consistent in all repeated views.
For EACH HALF: top region show LARGE complete full-body 3/4 FRONT view including feet and any weapon, alongside a smaller but clearly detailed TOP-DOWN pitch56 camera reference. BOTTOM region per half has SIX small cutouts on a common baseline: 3 COLOR TOP-DOWN pitch56 views at total character heights 110px,135px,160px, then 3 PURE BLACK silhouettes OF THAT SAME CAMERA VIEW at heights110,135,160px. Exact labels "110", "135", "160" and section labels "3/4 FRONT", "CAMERA 56", "COLOR 1:1", "BLACK 1:1". DO NOT fill space by enlarging thumbnails, use lots of empty space. No internal detail or gray shading in silhouettes. This is serious asset-design selection material.
LEFT design: 2.1m adult original hostile humanoid raider with a long lean forward-leaning trunk, compact wedge-shaped CLOSED leather-and-dark iron hood with two small eye openings, no horns, narrow shoulders, split rust-red short tunic at upper thighs. Two organic tough blackened shin guards, one single broad cleaver in right hand held out diagonally and an unarmed left hand. Dark charcoal/umber body with large orange collar panel, not cyan. Distinct low leaning diagonal wedge silhouette. Premium sculpted credible figure, not goblin stereotype.
RIGHT design: 2.1m original hostile upright humanoid raider, exposed angular clay-colored face without beard or hair, a tall broad DOUBLE SPLIT fan crest formed of dark keratin behind the skull, one raised thick segmented shoulder on right, left shoulder bare and low. Short broad legs and longer compact arms, three wide rust-red sash strips ending ABOVE knees, one hooked triangular hand axe, no second weapon. Dark stone-like skin with orange eyes restrained, distinct Y-shaped upper silhouette. Organic serious creature, not a robot, not a tiny toy.
All views match actual design. Avoid micro-ornamentation. Keep faction orange/red cues as broad patches visible from above. Every limb separates in projected silhouette; no idle effects that hide geometry.
```

Повторы и проверка: .local/codex-tasks/chars/progress.md.


## Exact prompt, attempt 2 (first retry)

Input image: prior attempt of this sheet.

```text
Use case: precise-object-edit / stylized-concept. Edit this original paired decision sheet. Fix the worst defect: R1 has many long shredded loose cloth straps that hamper readable run animation. Replace ALL R1 waist cloth with exactly TWO broad short rust-red fabric panels ending at upper thigh, no loose ribbons or frays. Preserve R1 lean wedge posture and cleaver. R2 must stay Y-crest distinct; reduce its crest height by20% preserving split fan shape. Clean up tiny grit in both. Apply changes consistently across front, camera, small color and black silhouette views. Preserve labels and paired layout. Preserve both full-body 3/4 front views, both enlarged 56 degree camera views and all bottom small game-camera color views and pure black silhouettes labelled110/135/160. Do not introduce additional panels or characters.
```

