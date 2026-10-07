# Девять волновых крипов: современное стилизованное фэнтези

Дата: 5 октября 2026 года. Генератор: встроенный image_gen, один новый лист 3x3. Это визуальный концепт по просьбе пользователя, не игровой ассет и не утверждение нового канона.

[Изображение](2026-10-05-nine-wave-creeps-warcraft-modern-v1.png).

## Состав и происхождение

Основа: Warcraft Life in Arena 3.9c, SHA256 карты `02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34`. Принадлежность к обычным волнам взята из массива `Er` функции `c5`, отдельно от усиленных противников `Xr` и альтернативного режима `F5`. Это статические исследовательские данные, не новая проверка матча. Внешность - новая художественная интерпретация типов существ, оригинальные meshes и textures не использованы.

Все ссылки на выгрузки относятся к [каталогу 3.9c](../../research/lia/warcraft/3.9c/README.md). Строки указаны для CSV в текущем репозитории; проверены ID, display_name и file через Python csv.DictReader.

| Позиция | Тип | ID | Этап c5 | unit-roster-assignments.csv | units.csv | Model cue |
| --- | --- | --- | ---: | ---: | ---: | --- |
| 1 | Хрустальный арахнид | n008 | 1 | 94 | 151 | ArchnathidGreen |
| 2 | Иглогрив | n00D | 2 | 95 | 155 | RazorManeChief |
| 3 | Молодой драконид | n00F | 3 | 96 | 157 | DragonSpawnGreen |
| 4 | Темный тролль | n00I | 4 | 97 | 161 | DarkTroll |
| 5 | Чумной энт | n00N | 7 | 99 | 170 | CorruptedEnt |
| 6 | Дух океана | n00R | 9 | 101 | 174 | SeaElemental |
| 7 | Осадный голем | n023 | 22 | 107 | 541 | GolemStatue |
| 8 | Ледяной гигант | n0AO | 26 | 110 | 800 | MagnataurBrown |
| 9 | Болотная гидра | n0AQ | 27 | 111 | 802 | Hydra |

Художественный ориентир: [art-direction.md](../../docs/art-direction.md), выразительные силуэты и современное стилизованное 3D с корнями в Warcraft. Номера на листе не являются последовательностью волн; взяты девять различимых видов из разных обычных этапов.

## Точный промпт

```text
Use case: stylized-concept.
Create ONE high-resolution square fantasy creature concept sheet with EXACTLY NINE distinct full-body creatures in a perfectly aligned 3 by 3 grid. This is a premium modern reimagining of nine regular wave enemies from Warcraft III Life in Arena 3.9c. Preserve their species/archetypes, but invent original, sophisticated silhouettes, anatomy, equipment, surface design and materials. Warcraft-inspired heroic stylized 3D fantasy art direction: powerful exaggerated readable forms, sculptural anatomy, expertly layered crafted details, expressive faces, gorgeous cinematic rendering, modern AAA game-art quality. Stylish, formidable and memorable, never toy-like or childish. Modern means art quality and design sophistication, not science fiction.
Composition: equal nine square panels, thin understated dark dividers, every creature entirely contained in its own panel, complete body including feet, tails and horns visible, generous breathing room, dynamic three-quarter view with subtly elevated camera. Each figure fills its panel beautifully and remains readable as a game enemy from above. Restrained charcoal misty studio backgrounds with a subtle grounded stone floor and contact shadows; rich rim lighting, soft directional key, restrained magical VFX. Consistent art direction throughout, while all nine have radically distinct silhouettes and distinct natural color identities. No busy scenery. No page heading, logo or watermark. Use small elegant legible Russian captions centered at the bottom of each panel, the exact nine captions below. Allow enough bottom space for text without covering anatomy.
Reading order left to right, top to bottom:

TOP LEFT, caption "ХРУСТАЛЬНЫЙ АРАХНИД": a fierce low-slung scorpion-like arachnid, not a generic spider. Articulated chitin armor with luminous clear turquoise and jade crystal growths, enormous sharp pincer claws, multiple evenly articulated walking legs, an imposing arching segmented stinging tail, dark polished emerald exoskeleton, predatory tiny glowing eyes. Crystals integrated organically, tactile chitin, imposing weight. Entire tail fits inside panel.

TOP CENTER, caption "ИГЛОГРИВ": a stocky bipedal boar-like warrior, broad muscular chest, savage porcine snout, thick tusks, ridge of long rigid bristles and natural quills running from head down the back, powerful hoofed feet. Warm auburn fur, russet skin, dark weathered bronze and leather armor with clever asymmetry, heavy cleaver in one hand. Not a four-legged wild boar. Strong gritty charisma and angular original tribal craftsmanship.

TOP RIGHT, caption "МОЛОДОЙ ДРАКОНИД": a powerful grounded green reptilian dragonspawn warrior, upright muscular dragon torso and two weapon-bearing arms over a sturdy four-legged reptilian lower body, long scaled tail, swept horns, no wings. Elegant jade and olive overlapping scales, pale chest, dark brass minimalist armor, a finely shaped polearm, ember amber eyes. Agile threatening dragon-centaur silhouette, not a flying dragon.

MIDDLE LEFT, caption "ТЕМНЫЙ ТРОЛЛЬ": tall lean muscular dark violet troll skirmisher, broad long arms, predatory tusks, alert narrow eyes, swept-back mohawk, distinctive ears. Two elegantly brutal throwing axes, layered smoked leather and matte iron, restrained violet markings and original bone accents. Athletic forward stance, expressive original face. Cunning dangerous regular enemy, not an ornate hero.

MIDDLE CENTER, caption "ЧУМНОЙ ЭНТ": a menacing corrupted walking tree, large sculptural trunk torso, angular gnarled branch arms, root feet, cracked near-black bark and rotten moss. An expressive hollow wooden face, sickly lime bioluminescent fungal growths and restrained spores glowing in bark fissures. Organic asymmetry and interesting growth rings, forest-monster silhouette; no human armor.

MIDDLE RIGHT, caption "ДУХ ОКЕАНА": a sentient sea elemental with a powerful upright torso and arms formed from flowing translucent deep-blue and turquoise water, broad shoulders of curling surf, pearl-bright gaze, a twisting water column instead of legs. A few coral and eroded rock fragments naturally suspended inside its body, foam accents, beautiful believable refraction and caustics. Sculpted readable elegant water creature, not a person in blue armor.

BOTTOM LEFT, caption "ОСАДНЫЙ ГОЛЕМ": a hulking siege automaton constructed from expertly interlocking iron and weathered bronze armor plates over a solid core, short massive legs, enormous battering fists, broad squat brutal silhouette, tiny furnace eye slit and restrained amber glow from joints. Original medieval-fantasy engineering, embossed geometric craft, material wear; no futuristic robot technology, no gun, no giant crystal body.

BOTTOM CENTER, caption "ЛЕДЯНОЙ ГИГАНТ": an intimidating magnataur-like northern giant, four-legged shaggy mammoth lower body, upright thick muscular humanoid upper torso, broad beastlike head with magnificent curved tusks and horns. Snow-dusted dark brown and ivory fur, frosty gray skin, heavy dark forged metal bracers, a massive ice-encrusted stone maul held in both arms, cold blue breath. Readable mammoth-centaur anatomy and tremendous mass, not a humanoid ice golem or yeti.

BOTTOM RIGHT, caption "БОЛОТНАЯ ГИДРА": a low powerful quadrupedal swamp hydra with three clearly distinct sinuous necks and three reptilian heads, one coherent broad body, a thick tail and wet armored olive-green scales. Bone ridges, dark jade plating, subtly lime acid-lit mouths, swamp moss on its back, amber predatory eyes. Each head has its own expression and direction; whole three-headed creature visible.

Render like exquisitely crafted modern stylized 3D characters with physically plausible rich materials, cinematic visual appeal, strong individual character and clear anatomy. Controlled effects accent the creature without obscuring it. No repeated creatures, no extra panels, no framing cutoffs, no random decorative heroes or backgrounds. EXACTLY nine captioned images in one unified 3x3 sheet.
```

## Проверка изображения

Визуально проверены сетка 3x3, девять разных существ, подписи и соответствие выбранным типам. Картинка не подтверждает точную анатомию исходных моделей или готовность 3D-ассетов для Unity.

