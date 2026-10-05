# Dota LiA: Юниты

Снимок `012fab34e8c84ad0aa73cd4eadde1736e3c9df29`. Все 153 деклараций. Это статические определения; включение KV не доказывает доступность в матче. Отсутствующие поля наследуются из Dota или остаются неразрешенными.

Значения через `/` сохраняют порядок вектора по уровням. `MaxLevel` не дополняется предположениями. Полные вложенные modifiers/events/actions, включая повторяющиеся ключи, находятся в JSON и `kv_scalars.csv`. Имена из локализации используются как подписи, не как доказательство механики.

## имя не найдено - `fire_golem_10_wave`

Источник: [10_wave_megaboss.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L5) |
| `ModelScale` | 1.2 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L9) |
| `RingRadius` | 80 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L10) |
| `Level` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L11) |
| `ArmorPhysical` | 25 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L27) |
| `MagicalResistance` | 100 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L28) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L30) |
| `AttackDamageMin` | 251 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L31) |
| `AttackDamageMax` | 262 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L32) |
| `AttackRate` | 0.6 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L33) |
| `AttackAnimationPoint` | 0.26 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L34) |
| `AttackAcquisitionRange` | 1000 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L35) |
| `AttackRange` | 100 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L36) |
| `HealthBarOffset` | 260 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L39) |
| `BountyXP` | 1000 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L41) |
| `BountyGoldMin` | 82 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L42) |
| `BountyGoldMax` | 85 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L43) |
| `MovementSpeed` | 320 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L46) |
| `MovementTurnRate` | 0.5 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L47) |
| `StatusHealth` | 3000 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L49) |
| `StatusHealthRegen` | 1 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L50) |
| `StatusMana` | 0 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L51) |
| `StatusManaRegen` | 0 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L52) |
| `AttackType` | chaos | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L58) |
| `ArmorType` | heavy | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L59) |
| `VisionDaytimeRange` | 1400 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L61) |
| `VisionNighttimeRange` | 800 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L62) |
| `HasInventory` | 0 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L64) |

Способности: Ability1: `spell_immunity`; Ability2: `fire_golem_10_wave_megaboss_immolation`.

Lua: [Megaboss10Spawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/units/Megaboss10Spawn.lua#L1).

## Мегабосс - `10_wave_megaboss`

Источник: [10_wave_megaboss.txt:68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L68). Включен engine-root: `True`.

Предварительная роль по ID/пути: `megaboss_or_final_add`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L70) |
| `ModelScale` | 1.8 | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L73) |
| `RingRadius` | 90 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L74) |
| `Level` | 1 | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L75) |
| `AbilityLayout` | 6 | [163](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L163) |
| `ArmorPhysical` | 70 | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L165) |
| `MagicalResistance` | 0 | [166](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L166) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L168) |
| `AttackDamageMin` | 800 | [169](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L169) |
| `AttackDamageMax` | 800 | [170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L170) |
| `AttackRate` | 0.2 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L171) |
| `AttackAnimationPoint` | 0.3 | [172](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L172) |
| `AttackAcquisitionRange` | 2000 | [173](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L173) |
| `AttackRange` | 150 | [174](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L174) |
| `BountyXP` | 500 | [179](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L179) |
| `BountyGoldMin` | 0 | [180](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L180) |
| `BountyGoldMax` | 0 | [181](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L181) |
| `MovementSpeed` | 400 | [184](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L184) |
| `MovementTurnRate` | 0.5 | [185](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L185) |
| `StatusHealth` | 23000 | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L187) |
| `StatusHealthRegen` | 1 | [188](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L188) |
| `StatusMana` | 2000 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L189) |
| `StatusManaRegen` | 1 | [190](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L190) |
| `StatusStartingMana` | 1500 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L191) |
| `AttackType` | chaos | [197](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L197) |
| `ArmorType` | heavy | [198](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L198) |
| `VisionDaytimeRange` | 1800 | [200](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L200) |
| `VisionNighttimeRange` | 1800 | [201](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L201) |
| `HasInventory` | 0 | [203](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/10_wave_megaboss.txt#L203) |

Способности: Ability1: `10_wave_fire_golem`; Ability2: `10_wave_rejuvenation`; Ability3: `10_wave_antimagic`; Ability4: `10_wave_slow`; Ability5: `megaboss_10_return`; Ability6: `true_sight`.

Lua: [Megaboss10Stats.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/units/Megaboss10Stats.lua#L1).

## Мегабосс - `15_wave_megaboss`

Источник: [15_wave_megaboss.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `megaboss_or_final_add`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L5) |
| `ModelScale` | 1.6 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L8) |
| `RingRadius` | 105 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L9) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L10) |
| `AbilityLayout` | 6 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L26) |
| `ArmorPhysical` | 80 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L28) |
| `MagicalResistance` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L29) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L31) |
| `AttackDamageMin` | 2450 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L32) |
| `AttackDamageMax` | 2500 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L33) |
| `AttackRate` | 1.5 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L34) |
| `AttackAnimationPoint` | 0.3 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L35) |
| `AttackAcquisitionRange` | 2000 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L36) |
| `AttackRange` | 150 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L37) |
| `BountyXP` | 1500 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L39) |
| `BountyGoldMin` | 0 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L40) |
| `BountyGoldMax` | 0 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L41) |
| `MovementSpeed` | 250 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L44) |
| `MovementTurnRate` | 0.5 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L45) |
| `StatusHealth` | 26000 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L49) |
| `StatusHealthRegen` | 0.5 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L50) |
| `StatusMana` | 2500 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L51) |
| `StatusManaRegen` | 1.25 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L52) |
| `AttackType` | chaos | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L58) |
| `ArmorType` | heavy | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L59) |
| `VisionDaytimeRange` | 2000 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L61) |
| `VisionNighttimeRange` | 2000 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L62) |
| `HasInventory` | 0 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/15_wave_megaboss.txt#L64) |

Способности: Ability1: `15_megaboss_illusions`; Ability2: `15_megaboss_silence`; Ability3: `15_megaboss_astral`; Ability4: `megaboss_15_mana_break`; Ability5: `true_sight`; Ability6: `megaboss_15_ghoul_purge`.

Lua: [15_megaboss.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/15_megaboss.lua#L1).

## Мегабосс - `5_wave_megaboss`

Источник: [5_wave_megaboss.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `megaboss_or_final_add`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L5) |
| `ModelScale` | 1.3 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L8) |
| `RingRadius` | 90 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L9) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L10) |
| `AbilityLayout` | 5 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L26) |
| `ArmorPhysical` | 20 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L28) |
| `MagicalResistance` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L29) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L31) |
| `AttackDamageMin` | 181 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L32) |
| `AttackDamageMax` | 187 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L33) |
| `AttackRate` | 0.07 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L34) |
| `AttackAnimationPoint` | 0.3 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L35) |
| `AttackAcquisitionRange` | 2000 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L36) |
| `AttackRange` | 150 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L37) |
| `BountyXP` | 500 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L41) |
| `BountyGoldMin` | 0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L42) |
| `BountyGoldMax` | 0 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L43) |
| `MovementSpeed` | 350 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L46) |
| `MovementTurnRate` | 0.5 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L47) |
| `StatusHealth` | 4000 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L50) |
| `StatusHealthRegen` | 1 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L51) |
| `StatusMana` | 1000 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L52) |
| `StatusManaRegen` | 1 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L53) |
| `AttackType` | chaos | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L59) |
| `ArmorType` | heavy | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L60) |
| `VisionDaytimeRange` | 1800 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L62) |
| `VisionNighttimeRange` | 1800 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L63) |
| `HasInventory` | 0 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/5_wave_megaboss.txt#L65) |

Способности: Ability1: `5_megaboss_firestorm`; Ability2: `5_megaboss_stomp`; Ability3: `megaboss_5_bash`; Ability4: `megaboss_5_crit`; Ability5: `true_sight`.

Lua: [Megaboss5Stats.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/units/Megaboss5Stats.lua#L1).

## Заводной Гоблин - `android_clockwerk_goblin1`

Источник: [android_units.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L7) |
| `ModelScale` | 0.7 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L9) |
| `Level` | 0 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L10) |
| `ArmorPhysical` | 1 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L18) |
| `MagicalResistance` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L19) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L23) |
| `AttackDamageMin` | 10 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L24) |
| `AttackDamageMax` | 10 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L25) |
| `AttackRate` | 0.8 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L27) |
| `AttackAnimationPoint` | 0.33 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L28) |
| `AttackAcquisitionRange` | 500 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L29) |
| `AttackRange` | 90 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L30) |
| `BountyGoldMin` | 0 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L34) |
| `BountyGoldMax` | 0 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L35) |
| `MovementSpeed` | 270 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L40) |
| `MovementTurnRate` | 0.5 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L41) |
| `StatusHealth` | 125 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L45) |
| `StatusHealthRegen` | 2 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L46) |
| `StatusMana` | 0 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L47) |
| `StatsManaRegen` | 0 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L48) |
| `VisionDaytimeRange` | 900 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L52) |
| `VisionNighttimeRange` | 800 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L53) |
| `AttackType` | normal | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L61) |
| `ArmorType` | heavy | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L62) |
| `HealthBarOffset` | 140 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L65) |

Способности: Ability2: `android_pocket_factory_spawn_goblin1`.

## Заводной Гоблин - `android_clockwerk_goblin2`

Источник: [android_units.txt:82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L82). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L86) |
| `ModelScale` | 0.8 | [88](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L88) |
| `Level` | 0 | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L89) |
| `ArmorPhysical` | 3 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L97) |
| `MagicalResistance` | 0 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L98) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [102](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L102) |
| `AttackDamageMin` | 43 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L103) |
| `AttackDamageMax` | 43 | [104](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L104) |
| `AttackRate` | 0.7 | [106](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L106) |
| `AttackAnimationPoint` | 0.33 | [107](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L107) |
| `AttackAcquisitionRange` | 500 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L108) |
| `AttackRange` | 90 | [109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L109) |
| `BountyGoldMin` | 0 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L113) |
| `BountyGoldMax` | 0 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L114) |
| `MovementSpeed` | 270 | [119](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L119) |
| `MovementTurnRate` | 0.5 | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L120) |
| `StatusHealth` | 400 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L124) |
| `StatusHealthRegen` | 2 | [125](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L125) |
| `StatusMana` | 0 | [126](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L126) |
| `StatsManaRegen` | 0 | [127](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L127) |
| `VisionDaytimeRange` | 900 | [131](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L131) |
| `VisionNighttimeRange` | 800 | [132](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L132) |
| `AttackType` | normal | [140](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L140) |
| `ArmorType` | heavy | [141](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L141) |
| `HealthBarOffset` | 140 | [144](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L144) |

Способности: Ability2: `android_pocket_factory_spawn_goblin2`.

## Заводной Гоблин - `android_clockwerk_goblin3`

Источник: [android_units.txt:161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L161). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L165) |
| `ModelScale` | 0.9 | [167](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L167) |
| `Level` | 0 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L168) |
| `ArmorPhysical` | 5 | [177](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L177) |
| `MagicalResistance` | 0 | [178](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L178) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [182](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L182) |
| `AttackDamageMin` | 76 | [183](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L183) |
| `AttackDamageMax` | 76 | [184](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L184) |
| `AttackRate` | 0.6 | [186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L186) |
| `AttackAnimationPoint` | 0.33 | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L187) |
| `AttackAcquisitionRange` | 500 | [188](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L188) |
| `AttackRange` | 90 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L189) |
| `BountyGoldMin` | 0 | [193](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L193) |
| `BountyGoldMax` | 0 | [194](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L194) |
| `MovementSpeed` | 270 | [199](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L199) |
| `MovementTurnRate` | 0.5 | [200](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L200) |
| `StatusHealth` | 700 | [204](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L204) |
| `StatusHealthRegen` | 2 | [205](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L205) |
| `StatusMana` | 0 | [206](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L206) |
| `StatsManaRegen` | 0 | [207](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L207) |
| `VisionDaytimeRange` | 1800 | [211](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L211) |
| `VisionNighttimeRange` | 10 | [212](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L212) |
| `AttackType` | normal | [220](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L220) |
| `ArmorType` | heavy | [221](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L221) |
| `HealthBarOffset` | 140 | [224](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L224) |

Способности: Ability2: `android_pocket_factory_spawn_goblin3`.

## Мини-Завод - `android_pocket_factory_building1`

Источник: [android_units.txt:244](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L244). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [248](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L248) |
| `ModelScale` | 2.5 | [250](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L250) |
| `Level` | 4 | [251](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L251) |
| `HealthBarOffset` | 250 | [254](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L254) |
| `ArmorPhysical` | 5 | [262](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L262) |
| `MagicalResistance` | 0 | [263](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L263) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [267](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L267) |
| `AttackDamageMin` | 0 | [268](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L268) |
| `AttackDamageMax` | 0 | [269](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L269) |
| `AttackRate` | 0 | [271](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L271) |
| `AttackAnimationPoint` | 0 | [272](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L272) |
| `AttackAcquisitionRange` | 0 | [273](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L273) |
| `AttackRange` | 0 | [274](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L274) |
| `ProjectileSpeed` | 0 | [276](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L276) |
| `BountyGoldMin` | 0.0 | [280](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L280) |
| `BountyGoldMax` | 0.0 | [281](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L281) |
| `MovementSpeed` | 0 | [286](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L286) |
| `MovementTurnRate` | 0 | [287](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L287) |
| `StatusHealth` | 500 | [291](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L291) |
| `StatusMana` | 0 | [292](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L292) |
| `StatsManaRegen` | 0 | [293](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L293) |
| `VisionDaytimeRange` | 800 | [297](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L297) |
| `VisionNighttimeRange` | 600 | [298](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L298) |
| `AttackType` | siege | [306](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L306) |
| `ArmorType` | fortified | [307](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L307) |
| `RingRadius` | 180 | [310](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L310) |

## Мини-Завод - `android_pocket_factory_building2`

Источник: [android_units.txt:316](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L316). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [320](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L320) |
| `ModelScale` | 2.5 | [322](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L322) |
| `Level` | 5 | [323](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L323) |
| `HealthBarOffset` | 250 | [326](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L326) |
| `ArmorPhysical` | 5 | [334](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L334) |
| `MagicalResistance` | 0 | [335](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L335) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [339](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L339) |
| `AttackDamageMin` | 0 | [340](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L340) |
| `AttackDamageMax` | 0 | [341](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L341) |
| `AttackRate` | 0 | [343](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L343) |
| `AttackAnimationPoint` | 0 | [344](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L344) |
| `AttackAcquisitionRange` | 0 | [345](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L345) |
| `AttackRange` | 0 | [346](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L346) |
| `ProjectileSpeed` | 0 | [348](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L348) |
| `BountyGoldMin` | 0.0 | [352](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L352) |
| `BountyGoldMax` | 0.0 | [353](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L353) |
| `MovementSpeed` | 0 | [358](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L358) |
| `MovementTurnRate` | 0 | [359](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L359) |
| `StatusHealth` | 1000 | [363](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L363) |
| `StatusMana` | 0 | [364](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L364) |
| `StatsManaRegen` | 0 | [365](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L365) |
| `VisionDaytimeRange` | 800 | [369](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L369) |
| `VisionNighttimeRange` | 600 | [370](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L370) |
| `AttackType` | siege | [378](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L378) |
| `ArmorType` | fortified | [379](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L379) |
| `RingRadius` | 180 | [382](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L382) |

## Мини-Завод - `android_pocket_factory_building3`

Источник: [android_units.txt:388](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L388). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [392](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L392) |
| `ModelScale` | 2.5 | [394](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L394) |
| `Level` | 6 | [395](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L395) |
| `HealthBarOffset` | 250 | [398](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L398) |
| `ArmorPhysical` | 5 | [406](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L406) |
| `MagicalResistance` | 0 | [407](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L407) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [411](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L411) |
| `AttackDamageMin` | 0 | [412](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L412) |
| `AttackDamageMax` | 0 | [413](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L413) |
| `AttackRate` | 0 | [415](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L415) |
| `AttackAnimationPoint` | 0 | [416](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L416) |
| `AttackAcquisitionRange` | 0 | [417](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L417) |
| `AttackRange` | 0 | [418](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L418) |
| `ProjectileSpeed` | 0 | [420](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L420) |
| `BountyGoldMin` | 0.0 | [424](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L424) |
| `BountyGoldMax` | 0.0 | [425](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L425) |
| `MovementSpeed` | 0 | [430](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L430) |
| `MovementTurnRate` | 0 | [431](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L431) |
| `StatusHealth` | 1500 | [435](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L435) |
| `StatusMana` | 0 | [436](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L436) |
| `StatsManaRegen` | 0 | [437](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L437) |
| `VisionDaytimeRange` | 800 | [441](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L441) |
| `VisionNighttimeRange` | 600 | [442](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L442) |
| `AttackType` | siege | [450](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L450) |
| `ArmorType` | fortified | [451](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L451) |
| `RingRadius` | 180 | [454](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/android_units.txt#L454) |

## Белый Волк - `white_wolf_bm`

Источник: [beastmaster_creeps.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L5) |
| `ModelScale` | 1 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L8) |
| `Level` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L9) |
| `ArmorPhysical` | 4 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L26) |
| `MagicalResistance` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L27) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L29) |
| `AttackDamageMin` | 61 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L30) |
| `AttackDamageMax` | 65 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L31) |
| `AttackRate` | 1.15 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L32) |
| `AttackAnimationPoint` | 0.3 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L33) |
| `AttackAcquisitionRange` | 500 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L34) |
| `AttackRange` | 90 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L35) |
| `BountyXP` | 0 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L37) |
| `BountyGoldMin` | 0 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L38) |
| `BountyGoldMax` | 0 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L39) |
| `MovementSpeed` | 500 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L42) |
| `MovementTurnRate` | 0.6 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L43) |
| `StatusHealth` | 950 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L45) |
| `StatusHealthRegen` | 0 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L46) |
| `StatusMana` | 200 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L47) |
| `StatusManaRegen` | 1 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L48) |
| `AttackType` | normal | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L54) |
| `ArmorType` | heavy | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L55) |
| `VisionDaytimeRange` | 900 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L57) |
| `VisionNighttimeRange` | 800 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L58) |
| `HasInventory` | 0 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L60) |

Способности: Ability1: `white_wolf_bm_howl`; Ability2: `white_wolf_bm_crit`.

## Повелитель Джунглей - `jungle_stalker_bm`

Источник: [beastmaster_creeps.txt:63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L63). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L65) |
| `ModelScale` | 1 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L68) |
| `Level` | 1 | [69](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L69) |
| `ArmorPhysical` | 5 | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L86) |
| `MagicalResistance` | 0 | [87](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L87) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L89) |
| `AttackDamageMin` | 80 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L90) |
| `AttackDamageMax` | 80 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L91) |
| `AttackRate` | 0.85 | [92](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L92) |
| `AttackAnimationPoint` | 0.3 | [93](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L93) |
| `AttackAcquisitionRange` | 500 | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L94) |
| `AttackRange` | 128 | [95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L95) |
| `BountyXP` | 0 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L97) |
| `BountyGoldMin` | 0 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L98) |
| `BountyGoldMax` | 0 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L99) |
| `MovementSpeed` | 320 | [102](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L102) |
| `MovementTurnRate` | 0.6 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L103) |
| `StatusHealth` | 600 | [105](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L105) |
| `StatusHealthRegen` | 0 | [106](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L106) |
| `StatusMana` | 300 | [107](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L107) |
| `StatusManaRegen` | 1 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L108) |
| `AttackType` | normal | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L114) |
| `ArmorType` | heavy | [115](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L115) |
| `VisionDaytimeRange` | 900 | [117](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L117) |
| `VisionNighttimeRange` | 800 | [118](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L118) |
| `HasInventory` | 0 | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L120) |

Способности: Ability1: `jungle_stalker_bm_rejuvenation`; Ability2: `jungle_stalker_bm_crit`.

## Феникс - `phoenix_bm`

Источник: [beastmaster_creeps.txt:123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L123). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [125](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L125) |
| `ModelScale` | 1 | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L128) |
| `Level` | 1 | [129](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L129) |
| `ArmorPhysical` | 7 | [147](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L147) |
| `MagicalResistance` | 0 | [148](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L148) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [150](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L150) |
| `AttackDamageMin` | 100 | [151](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L151) |
| `AttackDamageMax` | 130 | [152](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L152) |
| `AttackRate` | 1.4 | [153](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L153) |
| `AttackAnimationPoint` | 0.43 | [154](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L154) |
| `AttackAcquisitionRange` | 500 | [155](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L155) |
| `AttackRange` | 600 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L156) |
| `ProjectileSpeed` | 1800 | [158](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L158) |
| `BountyXP` | 0 | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L160) |
| `BountyGoldMin` | 0 | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L161) |
| `BountyGoldMax` | 0 | [162](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L162) |
| `MovementSpeed` | 320 | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L165) |
| `MovementTurnRate` | 0.6 | [166](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L166) |
| `StatusHealth` | 1400 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L168) |
| `StatusHealthRegen` | -25 | [169](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L169) |
| `StatusMana` | 0 | [170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L170) |
| `StatusManaRegen` | 0 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L171) |
| `AttackType` | chaos | [177](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L177) |
| `ArmorType` | light | [178](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L178) |
| `VisionDaytimeRange` | 900 | [180](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L180) |
| `VisionNighttimeRange` | 800 | [181](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L181) |
| `HasInventory` | 0 | [183](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L183) |

Способности: Ability1: `spell_immunity`; Ability2: `phoenix_bm_spawn_egg`; Ability3: `phoenix_bm_phoenix_fire`.

## Яйцо Феникса - `phoenix_egg_bm`

Источник: [beastmaster_creeps.txt:186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L186). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [188](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L188) |
| `ModelScale` | 0.5 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L191) |
| `Level` | 1 | [192](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L192) |
| `ArmorPhysical` | 0 | [209](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L209) |
| `MagicalResistance` | 0 | [210](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L210) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [212](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L212) |
| `StatusHealth` | 200 | [216](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L216) |
| `StatusHealthRegen` | 0 | [217](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L217) |
| `StatusMana` | 0 | [218](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L218) |
| `StatusManaRegen` | 0 | [219](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L219) |
| `AttackType` | normal | [225](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L225) |
| `ArmorType` | heavy | [226](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L226) |
| `VisionDaytimeRange` | 600 | [228](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L228) |
| `VisionNighttimeRange` | 600 | [229](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L229) |
| `HasInventory` | 0 | [231](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L231) |

Способности: Ability1: `spell_immunity`; Ability2: `phoenix_egg_bm_spawn_phoenix`.

## Медведь - `bear_bm`

Источник: [beastmaster_creeps.txt:235](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L235). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [237](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L237) |
| `ModelScale` | 1 | [240](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L240) |
| `Level` | 1 | [241](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L241) |
| `ArmorPhysical` | 15 | [258](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L258) |
| `MagicalResistance` | 0 | [259](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L259) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [261](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L261) |
| `AttackDamageMin` | 350 | [262](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L262) |
| `AttackDamageMax` | 400 | [263](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L263) |
| `AttackRate` | 2.45 | [264](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L264) |
| `AttackAnimationPoint` | 0.3 | [265](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L265) |
| `AttackAcquisitionRange` | 500 | [266](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L266) |
| `AttackRange` | 128 | [267](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L267) |
| `BountyXP` | 0 | [269](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L269) |
| `BountyGoldMin` | 0 | [270](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L270) |
| `BountyGoldMax` | 0 | [271](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L271) |
| `MovementSpeed` | 300 | [274](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L274) |
| `MovementTurnRate` | 0.6 | [275](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L275) |
| `StatusHealth` | 3200 | [277](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L277) |
| `StatusHealthRegen` | 2 | [278](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L278) |
| `StatusMana` | 0 | [279](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L279) |
| `StatusManaRegen` | 0 | [280](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L280) |
| `AttackType` | normal | [286](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L286) |
| `ArmorType` | heavy | [287](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L287) |
| `VisionDaytimeRange` | 900 | [289](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L289) |
| `VisionNighttimeRange` | 800 | [290](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L290) |
| `HasInventory` | 0 | [292](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beastmaster_creeps.txt#L292) |

Способности: Ability1: `5_megaboss_crit`; Ability2: `5_megaboss_bash`.

## имя не найдено - `npc_crypt_lord_locust`

Источник: [beetle.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_thinker | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L7) |
| `ModelScale` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L9) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L10) |
| `ArmorPhysical` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L18) |
| `MagicalResistance` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L19) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L23) |
| `AttackDamageMin` | 13.0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L24) |
| `AttackDamageMax` | 14.0 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L25) |
| `AttackRate` | 1 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L27) |
| `AttackAnimationPoint` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L28) |
| `AttackAcquisitionRange` | 900 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L29) |
| `AttackRange` | 10 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L30) |
| `ProjectileSpeed` | 500 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L32) |
| `BountyGoldMin` | 26.0 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L36) |
| `BountyGoldMax` | 38.0 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L37) |
| `MovementSpeed` | 400 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L42) |
| `MovementTurnRate` | 0.2 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L43) |
| `StatusHealth` | 65 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L47) |
| `StatusHealthRegen` | 0.25 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L48) |
| `VisionDaytimeRange` | 0 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L53) |
| `VisionNighttimeRange` | 10 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L54) |
| `AttackType` | normal | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L62) |
| `ArmorType` | light | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L63) |
| `HealthBarOffset` | 140 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/beetle.txt#L66) |

## Зомби - `butcher_zombie_1`

Источник: [butcher_zombies.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L5) |
| `ModelScale` | 0.7 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L8) |
| `Level` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L9) |
| `ArmorPhysical` | 0 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L20) |
| `MagicalResistance` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L21) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L23) |
| `AttackDamageMin` | 15 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L24) |
| `AttackDamageMax` | 15 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L25) |
| `AttackRate` | 0.8 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L26) |
| `AttackAnimationPoint` | 0.5 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L27) |
| `AttackAcquisitionRange` | 500 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L28) |
| `AttackRange` | 100 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L29) |
| `BountyXP` | 0 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L33) |
| `BountyGoldMin` | 0 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L34) |
| `BountyGoldMax` | 0 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L35) |
| `MovementSpeed` | 300 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L38) |
| `MovementTurnRate` | 0.5 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L39) |
| `StatusHealth` | 200 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L42) |
| `StatusHealthRegen` | 1 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L43) |
| `StatusMana` | 0 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L44) |
| `StatusManaRegen` | 0 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L45) |
| `AttackType` | normal | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L51) |
| `ArmorType` | heavy | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L52) |
| `VisionDaytimeRange` | 800 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L54) |
| `VisionNighttimeRange` | 800 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L55) |
| `HasInventory` | 0 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L57) |

Способности: Ability1: `butcher_skin`.

## Зомби - `butcher_zombie_2`

Источник: [butcher_zombies.txt:60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L60). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L62) |
| `ModelScale` | 0.7 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L65) |
| `Level` | 1 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L66) |
| `ArmorPhysical` | 3 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L77) |
| `MagicalResistance` | 0 | [78](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L78) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L80) |
| `AttackDamageMin` | 40 | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L81) |
| `AttackDamageMax` | 40 | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L82) |
| `AttackRate` | 0.8 | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L83) |
| `AttackAnimationPoint` | 0.5 | [84](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L84) |
| `AttackAcquisitionRange` | 500 | [85](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L85) |
| `AttackRange` | 100 | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L86) |
| `BountyXP` | 0 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L90) |
| `BountyGoldMin` | 0 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L91) |
| `BountyGoldMax` | 0 | [92](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L92) |
| `MovementSpeed` | 300 | [95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L95) |
| `MovementTurnRate` | 0.5 | [96](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L96) |
| `StatusHealth` | 400 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L99) |
| `StatusHealthRegen` | 1 | [100](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L100) |
| `StatusMana` | 0 | [101](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L101) |
| `StatusManaRegen` | 0 | [102](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L102) |
| `AttackType` | normal | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L108) |
| `ArmorType` | heavy | [109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L109) |
| `VisionDaytimeRange` | 800 | [111](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L111) |
| `VisionNighttimeRange` | 800 | [112](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L112) |
| `HasInventory` | 0 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L114) |

Способности: Ability1: `butcher_skin`.

## Зомби - `butcher_zombie_3`

Источник: [butcher_zombies.txt:117](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L117). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [119](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L119) |
| `ModelScale` | 0.7 | [122](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L122) |
| `Level` | 1 | [123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L123) |
| `ArmorPhysical` | 3 | [134](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L134) |
| `MagicalResistance` | 0 | [135](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L135) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [137](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L137) |
| `AttackDamageMin` | 70 | [138](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L138) |
| `AttackDamageMax` | 70 | [139](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L139) |
| `AttackRate` | 0.8 | [140](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L140) |
| `AttackAnimationPoint` | 0.5 | [141](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L141) |
| `AttackAcquisitionRange` | 500 | [142](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L142) |
| `AttackRange` | 100 | [143](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L143) |
| `BountyXP` | 0 | [147](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L147) |
| `BountyGoldMin` | 0 | [148](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L148) |
| `BountyGoldMax` | 0 | [149](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L149) |
| `MovementSpeed` | 330 | [152](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L152) |
| `MovementTurnRate` | 0.5 | [153](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L153) |
| `StatusHealth` | 600 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L156) |
| `StatusHealthRegen` | 4 | [157](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L157) |
| `StatusMana` | 0 | [158](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L158) |
| `StatusManaRegen` | 0 | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L159) |
| `AttackType` | normal | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L165) |
| `ArmorType` | heavy | [166](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L166) |
| `VisionDaytimeRange` | 800 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L168) |
| `VisionNighttimeRange` | 800 | [169](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L169) |
| `HasInventory` | 0 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/butcher_zombies.txt#L171) |

Способности: Ability1: `butcher_zombie_bash`; Ability2: `butcher_skin`.

## имя не найдено - `camera_guy`

Источник: [camera_guy.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/camera_guy.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/camera_guy.txt#L5) |
| `Level` | 1 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/camera_guy.txt#L7) |

Способности: Ability1: `camera_passive`.

## Камень - `arena_rock`

Источник: [decorations.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L5) |
| `ModelScale` | 1.1 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L7) |
| `Level` | 1 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L8) |
| `AttackAcquisitionRange` | 0 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L9) |
| `ArmorPhysical` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L21) |
| `MagicalResistance` | 100 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L22) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L24) |
| `BountyXP` | 0 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L26) |
| `BountyGoldMin` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L27) |
| `BountyGoldMax` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L28) |
| `StatusHealth` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L32) |
| `StatusHealthRegen` | 0 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L33) |
| `StatusMana` | 0 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L34) |
| `StatusManaRegen` | 0 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L35) |
| `VisionDaytimeRange` | 0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L42) |
| `VisionNighttimeRange` | 0 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L43) |
| `HasInventory` | 0 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L45) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Камень - `arena_rock_2`

Источник: [decorations.txt:49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L49). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L51) |
| `ModelScale` | 0.4 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L53) |
| `Level` | 1 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L54) |
| `AttackAcquisitionRange` | 0 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L55) |
| `ArmorPhysical` | 0 | [67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L67) |
| `MagicalResistance` | 100 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L68) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L70) |
| `BountyXP` | 0 | [72](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L72) |
| `BountyGoldMin` | 0 | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L73) |
| `BountyGoldMax` | 0 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L74) |
| `StatusHealth` | 1 | [78](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L78) |
| `StatusHealthRegen` | 0 | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L79) |
| `StatusMana` | 0 | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L80) |
| `StatusManaRegen` | 0 | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L81) |
| `VisionDaytimeRange` | 0 | [88](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L88) |
| `VisionNighttimeRange` | 0 | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L89) |
| `HasInventory` | 0 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L91) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Камень - `arena_rock_3`

Источник: [decorations.txt:95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L95). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L97) |
| `ModelScale` | 0.6 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L99) |
| `Level` | 1 | [100](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L100) |
| `AttackAcquisitionRange` | 0 | [101](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L101) |
| `ArmorPhysical` | 0 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L113) |
| `MagicalResistance` | 100 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L114) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [116](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L116) |
| `BountyXP` | 0 | [118](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L118) |
| `BountyGoldMin` | 0 | [119](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L119) |
| `BountyGoldMax` | 0 | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L120) |
| `StatusHealth` | 1 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L124) |
| `StatusHealthRegen` | 0 | [125](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L125) |
| `StatusMana` | 0 | [126](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L126) |
| `StatusManaRegen` | 0 | [127](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L127) |
| `VisionDaytimeRange` | 0 | [134](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L134) |
| `VisionNighttimeRange` | 0 | [135](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L135) |
| `HasInventory` | 0 | [137](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L137) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Камень - `arena_rock_4`

Источник: [decorations.txt:140](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L140). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [142](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L142) |
| `ModelScale` | 0.4 | [144](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L144) |
| `Level` | 1 | [145](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L145) |
| `AttackAcquisitionRange` | 0 | [146](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L146) |
| `ArmorPhysical` | 0 | [158](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L158) |
| `MagicalResistance` | 100 | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L159) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L161) |
| `BountyXP` | 0 | [163](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L163) |
| `BountyGoldMin` | 0 | [164](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L164) |
| `BountyGoldMax` | 0 | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L165) |
| `StatusHealth` | 1 | [169](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L169) |
| `StatusHealthRegen` | 0 | [170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L170) |
| `StatusMana` | 0 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L171) |
| `StatusManaRegen` | 0 | [172](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L172) |
| `VisionDaytimeRange` | 0 | [179](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L179) |
| `VisionNighttimeRange` | 0 | [180](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L180) |
| `HasInventory` | 0 | [182](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L182) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Камень - `arena_rock_5`

Источник: [decorations.txt:185](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L185). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L187) |
| `ModelScale` | 1 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L189) |
| `Level` | 1 | [190](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L190) |
| `AttackAcquisitionRange` | 0 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L191) |
| `ArmorPhysical` | 0 | [203](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L203) |
| `MagicalResistance` | 100 | [204](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L204) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [206](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L206) |
| `BountyXP` | 0 | [208](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L208) |
| `BountyGoldMin` | 0 | [209](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L209) |
| `BountyGoldMax` | 0 | [210](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L210) |
| `StatusHealth` | 1 | [214](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L214) |
| `StatusHealthRegen` | 0 | [215](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L215) |
| `StatusMana` | 0 | [216](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L216) |
| `StatusManaRegen` | 0 | [217](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L217) |
| `VisionDaytimeRange` | 0 | [224](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L224) |
| `VisionNighttimeRange` | 0 | [225](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L225) |
| `HasInventory` | 0 | [227](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L227) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Коробка - `arena_center_box`

Источник: [decorations.txt:231](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L231). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [233](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L233) |
| `ModelScale` | 1 | [235](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L235) |
| `Level` | 1 | [236](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L236) |
| `AttackAcquisitionRange` | 0 | [237](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L237) |
| `ArmorPhysical` | 0 | [249](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L249) |
| `MagicalResistance` | 100 | [250](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L250) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [252](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L252) |
| `BountyXP` | 0 | [254](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L254) |
| `BountyGoldMin` | 0 | [255](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L255) |
| `BountyGoldMax` | 0 | [256](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L256) |
| `StatusHealth` | 1 | [260](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L260) |
| `StatusHealthRegen` | 0 | [261](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L261) |
| `StatusMana` | 0 | [262](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L262) |
| `StatusManaRegen` | 0 | [263](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L263) |
| `VisionDaytimeRange` | 0 | [270](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L270) |
| `VisionNighttimeRange` | 0 | [271](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L271) |
| `HasInventory` | 0 | [273](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L273) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Баррикады - `barricades`

Источник: [decorations.txt:277](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L277). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [279](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L279) |
| `ModelScale` | 0.85 | [281](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L281) |
| `Level` | 1 | [282](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L282) |
| `AttackAcquisitionRange` | 0 | [283](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L283) |
| `RingRadius` | 70 | [286](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L286) |
| `ArmorPhysical` | 0 | [297](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L297) |
| `MagicalResistance` | 100 | [298](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L298) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [300](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L300) |
| `BountyXP` | 0 | [302](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L302) |
| `BountyGoldMin` | 0 | [303](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L303) |
| `BountyGoldMax` | 0 | [304](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L304) |
| `StatusHealth` | 1 | [308](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L308) |
| `StatusHealthRegen` | 0 | [309](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L309) |
| `StatusMana` | 0 | [310](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L310) |
| `StatusManaRegen` | 0 | [311](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L311) |
| `VisionDaytimeRange` | 0 | [318](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L318) |
| `VisionNighttimeRange` | 0 | [319](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L319) |
| `HasInventory` | 0 | [321](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L321) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Баррикады - `barricades_small`

Источник: [decorations.txt:325](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L325). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [327](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L327) |
| `ModelScale` | 0.75 | [329](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L329) |
| `Level` | 1 | [330](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L330) |
| `AttackAcquisitionRange` | 0 | [331](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L331) |
| `RingRadius` | 65 | [334](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L334) |
| `ArmorPhysical` | 0 | [345](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L345) |
| `MagicalResistance` | 100 | [346](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L346) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [348](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L348) |
| `BountyXP` | 0 | [350](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L350) |
| `BountyGoldMin` | 0 | [351](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L351) |
| `BountyGoldMax` | 0 | [352](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L352) |
| `StatusHealth` | 1 | [356](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L356) |
| `StatusHealthRegen` | 0 | [357](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L357) |
| `StatusMana` | 0 | [358](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L358) |
| `StatusManaRegen` | 0 | [359](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L359) |
| `VisionDaytimeRange` | 0 | [366](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L366) |
| `VisionNighttimeRange` | 0 | [367](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L367) |
| `HasInventory` | 0 | [369](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L369) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Баррикады - `barricades_big`

Источник: [decorations.txt:373](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L373). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [375](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L375) |
| `ModelScale` | 0.95 | [377](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L377) |
| `Level` | 1 | [378](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L378) |
| `AttackAcquisitionRange` | 0 | [379](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L379) |
| `RingRadius` | 75 | [382](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L382) |
| `ArmorPhysical` | 0 | [393](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L393) |
| `MagicalResistance` | 100 | [394](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L394) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [396](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L396) |
| `BountyXP` | 0 | [398](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L398) |
| `BountyGoldMin` | 0 | [399](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L399) |
| `BountyGoldMax` | 0 | [400](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L400) |
| `StatusHealth` | 1 | [404](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L404) |
| `StatusHealthRegen` | 0 | [405](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L405) |
| `StatusMana` | 0 | [406](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L406) |
| `StatusManaRegen` | 0 | [407](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L407) |
| `VisionDaytimeRange` | 0 | [414](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L414) |
| `VisionNighttimeRange` | 0 | [415](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L415) |
| `HasInventory` | 0 | [417](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L417) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Бочка - `npc_dota_creature_barrel`

Источник: [decorations.txt:421](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L421). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [423](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L423) |
| `ModelScale` | 0.9 | [425](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L425) |
| `Level` | 1 | [426](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L426) |
| `AttackAcquisitionRange` | 0 | [427](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L427) |
| `ArmorPhysical` | 0 | [444](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L444) |
| `MagicalResistance` | 100 | [445](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L445) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [447](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L447) |
| `BountyXP` | 0 | [449](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L449) |
| `BountyGoldMin` | 0 | [450](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L450) |
| `BountyGoldMax` | 0 | [451](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L451) |
| `StatusHealth` | 1 | [455](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L455) |
| `StatusHealthRegen` | 0 | [456](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L456) |
| `StatusMana` | 0 | [457](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L457) |
| `StatusManaRegen` | 0 | [458](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L458) |
| `VisionDaytimeRange` | 0 | [465](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L465) |
| `VisionNighttimeRange` | 0 | [466](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L466) |
| `HasInventory` | 0 | [468](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L468) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Бочка - `small_barrel`

Источник: [decorations.txt:472](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L472). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [474](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L474) |
| `ModelScale` | 0.85 | [476](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L476) |
| `Level` | 1 | [477](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L477) |
| `AttackAcquisitionRange` | 0 | [478](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L478) |
| `ArmorPhysical` | 0 | [490](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L490) |
| `MagicalResistance` | 100 | [491](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L491) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [493](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L493) |
| `BountyXP` | 0 | [495](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L495) |
| `BountyGoldMin` | 0 | [496](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L496) |
| `BountyGoldMax` | 0 | [497](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L497) |
| `StatusHealth` | 1 | [501](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L501) |
| `StatusHealthRegen` | 0 | [502](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L502) |
| `StatusMana` | 0 | [503](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L503) |
| `StatusManaRegen` | 0 | [504](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L504) |
| `VisionDaytimeRange` | 0 | [511](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L511) |
| `VisionNighttimeRange` | 0 | [512](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L512) |
| `HasInventory` | 0 | [514](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L514) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Бочка - `small_barrel_side`

Источник: [decorations.txt:518](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L518). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [520](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L520) |
| `ModelScale` | 0.85 | [522](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L522) |
| `Level` | 1 | [523](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L523) |
| `AttackAcquisitionRange` | 0 | [524](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L524) |
| `ArmorPhysical` | 0 | [536](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L536) |
| `MagicalResistance` | 100 | [537](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L537) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [539](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L539) |
| `BountyXP` | 0 | [541](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L541) |
| `BountyGoldMin` | 0 | [542](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L542) |
| `BountyGoldMax` | 0 | [543](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L543) |
| `StatusHealth` | 1 | [547](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L547) |
| `StatusHealthRegen` | 0 | [548](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L548) |
| `StatusMana` | 0 | [549](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L549) |
| `StatusManaRegen` | 0 | [550](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L550) |
| `VisionDaytimeRange` | 0 | [558](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L558) |
| `VisionNighttimeRange` | 0 | [559](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L559) |
| `HasInventory` | 0 | [561](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L561) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Бочка - `big_barrel`

Источник: [decorations.txt:565](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L565). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [567](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L567) |
| `ModelScale` | 0.99 | [569](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L569) |
| `Level` | 1 | [570](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L570) |
| `AttackAcquisitionRange` | 0 | [571](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L571) |
| `ArmorPhysical` | 0 | [583](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L583) |
| `MagicalResistance` | 100 | [584](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L584) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [586](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L586) |
| `BountyXP` | 0 | [588](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L588) |
| `BountyGoldMin` | 0 | [589](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L589) |
| `BountyGoldMax` | 0 | [590](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L590) |
| `StatusHealth` | 1 | [594](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L594) |
| `StatusHealthRegen` | 0 | [595](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L595) |
| `StatusMana` | 0 | [596](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L596) |
| `StatusManaRegen` | 0 | [597](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L597) |
| `VisionDaytimeRange` | 0 | [604](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L604) |
| `VisionNighttimeRange` | 0 | [605](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L605) |
| `HasInventory` | 0 | [607](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L607) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## Бочка со Взрывчаткой - `tnt_barrel`

Источник: [decorations.txt:611](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L611). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [613](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L613) |
| `ModelScale` | 1.5 | [615](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L615) |
| `Level` | 1 | [616](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L616) |
| `AttackAcquisitionRange` | 0 | [617](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L617) |
| `ArmorPhysical` | 0 | [629](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L629) |
| `MagicalResistance` | 100 | [630](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L630) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [632](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L632) |
| `BountyXP` | 0 | [634](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L634) |
| `BountyGoldMin` | 0 | [635](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L635) |
| `BountyGoldMax` | 0 | [636](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L636) |
| `StatusHealth` | 1 | [640](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L640) |
| `StatusHealthRegen` | 0 | [641](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L641) |
| `StatusMana` | 0 | [642](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L642) |
| `StatusManaRegen` | 0 | [643](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L643) |
| `VisionDaytimeRange` | 0 | [650](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L650) |
| `VisionNighttimeRange` | 0 | [651](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L651) |
| `HasInventory` | 0 | [653](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L653) |

Способности: Ability1: `barrel_no_health_bar`; Ability2: `barrel_explosion`; Ability3: `barrel_invulnerability`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## имя не найдено - `barricades_halloween`

Источник: [decorations.txt:657](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L657). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [659](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L659) |
| `ModelScale` | 2.6 | [661](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L661) |
| `Level` | 1 | [662](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L662) |
| `AttackAcquisitionRange` | 0 | [663](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L663) |
| `RingRadius` | 70 | [666](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L666) |
| `ArmorPhysical` | 0 | [677](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L677) |
| `MagicalResistance` | 100 | [678](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L678) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [680](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L680) |
| `BountyXP` | 0 | [682](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L682) |
| `BountyGoldMin` | 0 | [683](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L683) |
| `BountyGoldMax` | 0 | [684](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L684) |
| `StatusHealth` | 1 | [688](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L688) |
| `StatusHealthRegen` | 0 | [689](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L689) |
| `StatusMana` | 0 | [690](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L690) |
| `StatusManaRegen` | 0 | [691](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L691) |
| `VisionDaytimeRange` | 0 | [698](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L698) |
| `VisionNighttimeRange` | 0 | [699](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L699) |
| `HasInventory` | 0 | [701](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L701) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## имя не найдено - `barricades_halloween2`

Источник: [decorations.txt:705](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L705). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [707](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L707) |
| `ModelScale` | 1 | [709](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L709) |
| `Level` | 1 | [710](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L710) |
| `AttackAcquisitionRange` | 0 | [711](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L711) |
| `RingRadius` | 70 | [714](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L714) |
| `ArmorPhysical` | 0 | [725](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L725) |
| `MagicalResistance` | 100 | [726](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L726) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [728](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L728) |
| `BountyXP` | 0 | [730](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L730) |
| `BountyGoldMin` | 0 | [731](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L731) |
| `BountyGoldMax` | 0 | [732](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L732) |
| `StatusHealth` | 1 | [736](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L736) |
| `StatusHealthRegen` | 0 | [737](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L737) |
| `StatusMana` | 0 | [738](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L738) |
| `StatusManaRegen` | 0 | [739](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L739) |
| `VisionDaytimeRange` | 0 | [746](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L746) |
| `VisionNighttimeRange` | 0 | [747](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L747) |
| `HasInventory` | 0 | [749](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L749) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## имя не найдено - `barricades_halloween3`

Источник: [decorations.txt:753](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L753). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [755](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L755) |
| `ModelScale` | 1 | [757](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L757) |
| `Level` | 1 | [758](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L758) |
| `AttackAcquisitionRange` | 0 | [759](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L759) |
| `RingRadius` | 70 | [762](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L762) |
| `ArmorPhysical` | 0 | [773](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L773) |
| `MagicalResistance` | 100 | [774](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L774) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [776](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L776) |
| `BountyXP` | 0 | [778](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L778) |
| `BountyGoldMin` | 0 | [779](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L779) |
| `BountyGoldMax` | 0 | [780](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L780) |
| `StatusHealth` | 1 | [784](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L784) |
| `StatusHealthRegen` | 0 | [785](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L785) |
| `StatusMana` | 0 | [786](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L786) |
| `StatusManaRegen` | 0 | [787](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L787) |
| `VisionDaytimeRange` | 0 | [794](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L794) |
| `VisionNighttimeRange` | 0 | [795](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L795) |
| `HasInventory` | 0 | [797](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/decorations.txt#L797) |

Способности: Ability1: `barrel_no_health_bar`.

Lua: [destrSpawn.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/destrSpawn.lua#L1).

## имя не найдено - `dummy_unit`

Источник: [dummy_unit.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L5) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L6) |
| `VisionDaytimeRange` | 0 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L7) |
| `VisionNighttimeRange` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L8) |
| `AbilityLayout` | 4 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L11) |

Способности: Ability1: `dummy_passive`.

## имя не найдено - `dummy_unit_vulnerable`

Источник: [dummy_unit.txt:15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L15). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L17) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L18) |
| `VisionDaytimeRange` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L19) |
| `VisionNighttimeRange` | 0 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L20) |
| `AbilityLayout` | 4 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L23) |

Способности: Ability1: `dummy_passive_vulnerable`.

## имя не найдено - `dummy_unit_building`

Источник: [dummy_unit.txt:27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L27). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L29) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L30) |
| `VisionDaytimeRange` | 0 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L31) |
| `VisionNighttimeRange` | 0 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L32) |
| `AbilityLayout` | 4 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L36) |

Способности: Ability1: `dummy_passive`.

## имя не найдено - `dummy_unit_anomaly`

Источник: [dummy_unit.txt:40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L40). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L42) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L43) |
| `VisionDaytimeRange` | 0 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L44) |
| `VisionNighttimeRange` | 0 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L45) |
| `AbilityLayout` | 4 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L48) |

Способности: Ability1: `dummy_passive`.

## имя не найдено - `dummy_unit_side_effect`

Источник: [dummy_unit.txt:52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L52). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L54) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L55) |
| `VisionDaytimeRange` | 0 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L56) |
| `VisionNighttimeRange` | 0 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L57) |
| `AbilityLayout` | 4 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L60) |

Способности: Ability1: `dummy_passive`.

## имя не найдено - `dummy_unit_phase_hero`

Источник: [dummy_unit.txt:64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L64). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L66) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L67) |
| `VisionDaytimeRange` | 0 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L68) |
| `VisionNighttimeRange` | 0 | [69](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L69) |
| `AbilityLayout` | 4 | [72](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L72) |

Способности: Ability1: `dummy_passive_nofly`.

## имя не найдено - `dummy_unit_pure_light`

Источник: [dummy_unit.txt:77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L77). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L79) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L80) |
| `VisionDaytimeRange` | 0 | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L81) |
| `VisionNighttimeRange` | 0 | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L82) |
| `AbilityLayout` | 4 | [85](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/dummy_unit.txt#L85) |

Способности: Ability1: `dummy_passive`; Ability2: `pure_light_ability`.

## Босс - `1_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L5) |
| `ModelScale` | 1.3 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L8) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L10) |
| `ArmorPhysical` | 7 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L26) |
| `MagicalResistance` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L27) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L29) |
| `AttackDamageMin` | 61 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L30) |
| `AttackDamageMax` | 62 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L31) |
| `AttackRate` | 0.85 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L32) |
| `AttackAnimationPoint` | 0.53 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L33) |
| `AttackAcquisitionRange` | 900 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L34) |
| `AttackRange` | 100 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L35) |
| `BountyXP` | 25 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L39) |
| `BountyGoldMin` | 35 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L40) |
| `BountyGoldMax` | 35 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L41) |
| `MovementSpeed` | 300 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L44) |
| `MovementTurnRate` | 0.5 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L45) |
| `StatusHealth` | 400 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L47) |
| `StatusHealthRegen` | 0.5 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L48) |
| `StatusMana` | 500 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L49) |
| `StatusManaRegen` | 1 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L50) |
| `AttackType` | normal | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L56) |
| `ArmorType` | heavy | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L57) |
| `VisionDaytimeRange` | 900 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L59) |
| `VisionNighttimeRange` | 800 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L60) |
| `HasInventory` | 0 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L62) |
| `PathfindingSearchDepthScale` | 0.5 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L64) |

Способности: Ability1: `1_wave_stomp_extreme`; Ability2: `wave_1_poison_extreme`.

Lua: [1_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/1_wave_bosses_extreme.lua#L1).

## Босс - `2_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L68). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L70) |
| `ModelScale` | 1 | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L73) |
| `Level` | 1 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L74) |
| `ArmorPhysical` | 10 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L90) |
| `MagicalResistance` | 0 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L91) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [93](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L93) |
| `AttackDamageMin` | 101 | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L94) |
| `AttackDamageMax` | 102 | [95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L95) |
| `AttackRate` | 0.95 | [96](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L96) |
| `AttackAnimationPoint` | 0.5 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L97) |
| `AttackAcquisitionRange` | 900 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L98) |
| `AttackRange` | 100 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L99) |
| `BountyXP` | 25 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L103) |
| `BountyGoldMin` | 35 | [104](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L104) |
| `BountyGoldMax` | 35 | [105](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L105) |
| `MovementSpeed` | 350 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L108) |
| `MovementTurnRate` | 0.5 | [109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L109) |
| `StatusHealth` | 700 | [111](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L111) |
| `StatusHealthRegen` | 0.5 | [112](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L112) |
| `StatusMana` | 250 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L113) |
| `StatusManaRegen` | 1 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L114) |
| `AttackType` | normal | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L120) |
| `ArmorType` | heavy | [121](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L121) |
| `VisionDaytimeRange` | 900 | [123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L123) |
| `VisionNighttimeRange` | 800 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L124) |
| `HasInventory` | 0 | [126](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L126) |
| `PathfindingSearchDepthScale` | 0.5 | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L128) |

Способности: Ability1: `wave_2_centaurs_revenge_extreme`; Ability2: `2_wave_war_stomp_extreme`; Ability3: `wave_2_aura_of_vengeance_extreme`; Ability4: `second_wave_wave_of_force_extreme`.

Lua: [2_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/2_wave_bosses_extreme.lua#L1).

## Босс - `3_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:132](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L132). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [134](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L134) |
| `ModelScale` | 1 | [137](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L137) |
| `Level` | 1 | [138](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L138) |
| `ArmorPhysical` | 13 | [154](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L154) |
| `MagicalResistance` | 0 | [155](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L155) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L156) |
| `AttackDamageMin` | 176 | [157](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L157) |
| `AttackDamageMax` | 182 | [158](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L158) |
| `AttackRate` | 0.9 | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L159) |
| `AttackAnimationPoint` | 0.3 | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L160) |
| `AttackAcquisitionRange` | 900 | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L161) |
| `AttackRange` | 100 | [162](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L162) |
| `BountyXP` | 25 | [166](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L166) |
| `BountyGoldMin` | 35 | [167](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L167) |
| `BountyGoldMax` | 35 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L168) |
| `MovementSpeed` | 270 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L171) |
| `MovementTurnRate` | 0.6 | [172](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L172) |
| `StatusHealth` | 1000 | [174](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L174) |
| `StatusHealthRegen` | 0.5 | [175](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L175) |
| `StatusMana` | 400 | [176](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L176) |
| `StatusManaRegen` | 1 | [177](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L177) |
| `AttackType` | normal | [183](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L183) |
| `ArmorType` | medium | [184](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L184) |
| `VisionDaytimeRange` | 900 | [186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L186) |
| `VisionNighttimeRange` | 800 | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L187) |
| `HasInventory` | 0 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L189) |
| `PathfindingSearchDepthScale` | 0.5 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L191) |

Способности: Ability1: `wave_3_evasion_extreme`; Ability2: `3_wave_rejuvenation_extreme`.

Lua: [3_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/3_wave_bosses_extreme.lua#L1).

## Босс - `4_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:195](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L195). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [197](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L197) |
| `ModelScale` | 0.9 | [200](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L200) |
| `Level` | 1 | [201](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L201) |
| `ArmorPhysical` | 14 | [217](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L217) |
| `MagicalResistance` | 0 | [218](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L218) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [220](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L220) |
| `AttackDamageMin` | 151 | [221](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L221) |
| `AttackDamageMax` | 154 | [222](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L222) |
| `AttackRate` | 0.8 | [223](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L223) |
| `AttackAnimationPoint` | 0.3 | [224](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L224) |
| `AttackAcquisitionRange` | 900 | [225](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L225) |
| `AttackRange` | 800 | [226](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L226) |
| `ProjectileSpeed` | 1200 | [228](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L228) |
| `BountyXP` | 25 | [230](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L230) |
| `BountyGoldMin` | 40 | [231](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L231) |
| `BountyGoldMax` | 40 | [232](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L232) |
| `MovementSpeed` | 270 | [235](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L235) |
| `MovementTurnRate` | 0.5 | [236](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L236) |
| `StatusHealth` | 1200 | [238](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L238) |
| `StatusHealthRegen` | 0.5 | [239](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L239) |
| `StatusMana` | 400 | [240](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L240) |
| `StatusManaRegen` | 1 | [241](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L241) |
| `AttackType` | pierce | [247](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L247) |
| `ArmorType` | heavy | [248](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L248) |
| `VisionDaytimeRange` | 900 | [250](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L250) |
| `VisionNighttimeRange` | 800 | [251](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L251) |
| `HasInventory` | 0 | [253](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L253) |
| `PathfindingSearchDepthScale` | 0.5 | [255](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L255) |

Способности: Ability1: `4_wave_death_coil`; Ability2: `4_wave_ensnare_extreme`; Ability3: `4_wave_true_hit_extreme`.

Lua: [4_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/4_wave_bosses_extreme.lua#L1).

## Босс - `6_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:259](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L259). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [261](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L261) |
| `ModelScale` | 1.2 | [264](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L264) |
| `Level` | 1 | [265](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L265) |
| `ArmorPhysical` | 16 | [276](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L276) |
| `MagicalResistance` | 0 | [277](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L277) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [279](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L279) |
| `AttackDamageMin` | 251 | [280](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L280) |
| `AttackDamageMax` | 253 | [281](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L281) |
| `AttackRate` | 0.85 | [282](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L282) |
| `AttackAnimationPoint` | 0.3 | [283](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L283) |
| `AttackAcquisitionRange` | 900 | [284](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L284) |
| `AttackRange` | 100 | [285](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L285) |
| `BountyXP` | 33 | [289](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L289) |
| `BountyGoldMin` | 40 | [290](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L290) |
| `BountyGoldMax` | 40 | [291](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L291) |
| `MovementSpeed` | 400 | [294](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L294) |
| `MovementTurnRate` | 0.6 | [295](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L295) |
| `StatusHealth` | 2200 | [297](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L297) |
| `StatusHealthRegen` | 0.5 | [298](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L298) |
| `StatusMana` | 800 | [299](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L299) |
| `StatusManaRegen` | 0.75 | [300](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L300) |
| `StatusStartingMana` | 350 | [301](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L301) |
| `AttackType` | normal | [307](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L307) |
| `ArmorType` | heavy | [308](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L308) |
| `VisionDaytimeRange` | 900 | [310](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L310) |
| `VisionNighttimeRange` | 800 | [311](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L311) |
| `PathfindingSearchDepthScale` | 0.5 | [313](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L313) |
| `HasInventory` | 0 | [316](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L316) |

Способности: Ability1: `6_wave_curse`; Ability2: `6_wave_cripple`; Ability3: `wave_6_invisibility`.

Lua: [6_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/6_wave_bosses_extreme.lua#L1).

## Босс - `7_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:319](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L319). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [321](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L321) |
| `ModelScale` | 1.5 | [324](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L324) |
| `Level` | 1 | [325](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L325) |
| `ArmorPhysical` | 18 | [341](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L341) |
| `MagicalResistance` | 0 | [342](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L342) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [344](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L344) |
| `AttackDamageMin` | 351 | [345](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L345) |
| `AttackDamageMax` | 355 | [346](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L346) |
| `AttackRate` | 0.45 | [347](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L347) |
| `AttackAnimationPoint` | 0.3 | [348](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L348) |
| `AttackAcquisitionRange` | 900 | [349](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L349) |
| `AttackRange` | 100 | [350](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L350) |
| `BountyXP` | 33 | [354](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L354) |
| `BountyGoldMin` | 40 | [355](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L355) |
| `BountyGoldMax` | 40 | [356](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L356) |
| `MovementSpeed` | 500 | [359](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L359) |
| `MovementTurnRate` | 0.5 | [360](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L360) |
| `StatusHealth` | 3200 | [362](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L362) |
| `StatusHealthRegen` | 0.5 | [363](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L363) |
| `StatusMana` | 450 | [364](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L364) |
| `StatusManaRegen` | 1 | [365](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L365) |
| `StatusStartingMana` | 400 | [366](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L366) |
| `AttackType` | normal | [372](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L372) |
| `ArmorType` | heavy | [373](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L373) |
| `VisionDaytimeRange` | 900 | [375](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L375) |
| `VisionNighttimeRange` | 800 | [376](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L376) |
| `PathfindingSearchDepthScale` | 0.5 | [378](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L378) |
| `HasInventory` | 0 | [381](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L381) |

Способности: Ability1: `7_wave_howl_of_terror_extreme`; Ability2: `7_wave_plague`; Ability3: `7_wave_sharp_claws_extreme`.

Lua: [7_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/7_wave_bosses_extreme.lua#L1).

## Босс - `8_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:384](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L384). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [386](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L386) |
| `ModelScale` | 1.5 | [389](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L389) |
| `Level` | 1 | [390](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L390) |
| `ArmorPhysical` | 22 | [406](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L406) |
| `MagicalResistance` | 0 | [407](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L407) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [409](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L409) |
| `AttackDamageMin` | 401 | [410](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L410) |
| `AttackDamageMax` | 404 | [411](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L411) |
| `AttackRate` | 0.95 | [412](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L412) |
| `AttackAnimationPoint` | 0.3 | [413](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L413) |
| `AttackAcquisitionRange` | 900 | [414](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L414) |
| `AttackRange` | 100 | [415](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L415) |
| `BountyXP` | 33 | [419](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L419) |
| `BountyGoldMin` | 50 | [420](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L420) |
| `BountyGoldMax` | 50 | [421](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L421) |
| `MovementSpeed` | 270 | [424](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L424) |
| `MovementTurnRate` | 0.6 | [425](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L425) |
| `StatusHealth` | 3300 | [427](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L427) |
| `StatusHealthRegen` | 3.0 | [428](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L428) |
| `StatusMana` | 300 | [429](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L429) |
| `StatusManaRegen` | 0.75 | [430](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L430) |
| `AttackType` | normal | [436](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L436) |
| `ArmorType` | heavy | [437](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L437) |
| `VisionDaytimeRange` | 900 | [439](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L439) |
| `VisionNighttimeRange` | 800 | [440](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L440) |
| `PathfindingSearchDepthScale` | 0.5 | [442](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L442) |
| `HasInventory` | 0 | [445](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L445) |

Способности: Ability1: `wave_8_cleave`; Ability2: `custom_spell_immunity`; Ability3: `wave_8_lifesteal_aura`; Ability4: `8_wave_storm_bolt`; Ability5: `wave_8_lifesteal_extreme`.

Lua: [8_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/8_wave_bosses.lua#L1).

## Босс - `9_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:448](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L448). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [450](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L450) |
| `ModelScale` | 1 | [453](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L453) |
| `Level` | 1 | [454](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L454) |
| `ProjectileSpeed` | 1300 | [456](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L456) |
| `ArmorPhysical` | 25 | [489](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L489) |
| `MagicalResistance` | 0 | [490](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L490) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [492](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L492) |
| `AttackDamageMin` | 401 | [493](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L493) |
| `AttackDamageMax` | 410 | [494](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L494) |
| `AttackRate` | 0.7 | [495](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L495) |
| `AttackAnimationPoint` | 0.3 | [496](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L496) |
| `AttackAcquisitionRange` | 900 | [497](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L497) |
| `AttackRange` | 600 | [498](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L498) |
| `BountyXP` | 33 | [500](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L500) |
| `BountyGoldMin` | 45 | [501](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L501) |
| `BountyGoldMax` | 45 | [502](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L502) |
| `MovementSpeed` | 220 | [505](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L505) |
| `MovementTurnRate` | 0.5 | [506](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L506) |
| `StatusHealth` | 4000 | [508](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L508) |
| `StatusHealthRegen` | 0.5 | [509](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L509) |
| `StatusMana` | 400 | [510](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L510) |
| `StatusManaRegen` | 1 | [511](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L511) |
| `AttackType` | pierce | [517](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L517) |
| `ArmorType` | medium | [518](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L518) |
| `VisionDaytimeRange` | 900 | [520](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L520) |
| `VisionNighttimeRange` | 800 | [521](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L521) |
| `PathfindingSearchDepthScale` | 0.5 | [523](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L523) |
| `HasInventory` | 0 | [526](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L526) |

Способности: Ability1: `9_wave_frost_nova`; Ability2: `wave_9_morphallaxis`.

Lua: [9_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/9_wave_bosses.lua#L1).

## Босс - `11_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:529](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L529). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [531](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L531) |
| `ModelScale` | 1.3 | [534](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L534) |
| `Level` | 1 | [535](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L535) |
| `ArmorPhysical` | 30 | [551](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L551) |
| `MagicalResistance` | 0 | [552](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L552) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [554](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L554) |
| `AttackDamageMin` | 451 | [555](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L555) |
| `AttackDamageMax` | 456 | [556](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L556) |
| `AttackRate` | 0.25 | [557](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L557) |
| `AttackAnimationPoint` | 0.3 | [558](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L558) |
| `AttackAcquisitionRange` | 900 | [559](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L559) |
| `AttackRange` | 100 | [560](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L560) |
| `BountyXP` | 48 | [564](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L564) |
| `BountyGoldMin` | 50 | [565](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L565) |
| `BountyGoldMax` | 50 | [566](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L566) |
| `MovementSpeed` | 320 | [569](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L569) |
| `MovementTurnRate` | 0.5 | [570](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L570) |
| `StatusHealth` | 7000 | [572](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L572) |
| `StatusHealthRegen` | 0.5 | [573](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L573) |
| `StatusMana` | 0 | [574](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L574) |
| `StatusManaRegen` | 0.75 | [575](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L575) |
| `AttackType` | chaos | [581](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L581) |
| `ArmorType` | medium | [582](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L582) |
| `VisionDaytimeRange` | 900 | [584](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L584) |
| `VisionNighttimeRange` | 800 | [585](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L585) |
| `PathfindingSearchDepthScale` | 0.5 | [587](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L587) |
| `HasInventory` | 0 | [590](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L590) |

Способности: Ability1: `wave_11_mana_break_extreme`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Босс - `12_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:593](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L593). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [595](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L595) |
| `ModelScale` | 1.2 | [598](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L598) |
| `Level` | 1 | [599](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L599) |
| `ArmorPhysical` | 30 | [615](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L615) |
| `MagicalResistance` | 0 | [616](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L616) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [618](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L618) |
| `AttackDamageMin` | 701 | [619](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L619) |
| `AttackDamageMax` | 704 | [620](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L620) |
| `AttackRate` | 0.75 | [621](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L621) |
| `AttackAnimationPoint` | 0.3 | [622](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L622) |
| `AttackAcquisitionRange` | 900 | [623](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L623) |
| `AttackRange` | 100 | [624](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L624) |
| `BountyXP` | 48 | [628](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L628) |
| `BountyGoldMin` | 50 | [629](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L629) |
| `BountyGoldMax` | 50 | [630](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L630) |
| `MovementSpeed` | 300 | [633](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L633) |
| `MovementTurnRate` | 0.5 | [634](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L634) |
| `StatusHealth` | 7000 | [636](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L636) |
| `StatusHealthRegen` | 0.5 | [637](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L637) |
| `StatusMana` | 600 | [638](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L638) |
| `StatusManaRegen` | 0 | [639](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L639) |
| `AttackType` | normal | [645](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L645) |
| `ArmorType` | heavy | [646](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L646) |
| `VisionDaytimeRange` | 900 | [648](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L648) |
| `VisionNighttimeRange` | 800 | [649](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L649) |
| `PathfindingSearchDepthScale` | 0.5 | [651](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L651) |
| `HasInventory` | 0 | [654](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L654) |

Способности: Ability1: `wave_12_bash`; Ability2: `12_wave_bloodlust_extreme`; Ability3: `12_wave_roots_extreme`.

Lua: [12_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/12_wave_bosses_extreme.lua#L1).

## Босс - `13_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:657](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L657). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [659](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L659) |
| `ModelScale` | 1.2 | [662](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L662) |
| `Level` | 1 | [663](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L663) |
| `ArmorPhysical` | 40 | [679](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L679) |
| `MagicalResistance` | 0 | [680](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L680) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [682](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L682) |
| `AttackDamageMin` | 801 | [683](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L683) |
| `AttackDamageMax` | 805 | [684](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L684) |
| `AttackRate` | 0.65 | [685](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L685) |
| `AttackAnimationPoint` | 0.3 | [686](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L686) |
| `AttackAcquisitionRange` | 900 | [687](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L687) |
| `AttackRange` | 100 | [688](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L688) |
| `BountyXP` | 48 | [692](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L692) |
| `BountyGoldMin` | 50 | [693](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L693) |
| `BountyGoldMax` | 50 | [694](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L694) |
| `MovementSpeed` | 270 | [697](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L697) |
| `MovementTurnRate` | 0.5 | [698](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L698) |
| `StatusHealth` | 8000 | [700](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L700) |
| `StatusHealthRegen` | 0.5 | [701](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L701) |
| `StatusMana` | 450 | [702](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L702) |
| `StatusManaRegen` | 1 | [703](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L703) |
| `AttackType` | normal | [709](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L709) |
| `ArmorType` | heavy | [710](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L710) |
| `VisionDaytimeRange` | 900 | [712](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L712) |
| `VisionNighttimeRange` | 800 | [713](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L713) |
| `PathfindingSearchDepthScale` | 0.5 | [715](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L715) |
| `HasInventory` | 0 | [718](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L718) |

Способности: Ability1: `custom_spell_immunity`; Ability2: `wave_13_command_aura_extreme`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Босс - `14_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:721](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L721). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [724](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L724) |
| `ModelScale` | 1.2 | [727](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L727) |
| `Level` | 1 | [728](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L728) |
| `ProjectileSpeed` | 1500 | [730](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L730) |
| `ArmorPhysical` | 50 | [747](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L747) |
| `MagicalResistance` | 0 | [748](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L748) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [750](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L750) |
| `AttackDamageMin` | 751 | [751](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L751) |
| `AttackDamageMax` | 760 | [752](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L752) |
| `AttackRate` | 0.8 | [753](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L753) |
| `AttackAnimationPoint` | 0.3 | [754](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L754) |
| `AttackAcquisitionRange` | 900 | [755](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L755) |
| `AttackRange` | 500 | [756](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L756) |
| `SplashAttack` | 1 | [758](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L758) |
| `SplashFullDamageRadius` | 75 | [759](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L759) |
| `SplashMediumRadius` | 150 | [760](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L760) |
| `SplashMediumDamage` | 0.5 | [761](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L761) |
| `SplashSmallRadius` | 225 | [762](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L762) |
| `SplashSmallDamage` | 0.25 | [763](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L763) |
| `BountyXP` | 48 | [765](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L765) |
| `BountyGoldMin` | 60 | [766](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L766) |
| `BountyGoldMax` | 60 | [767](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L767) |
| `MovementSpeed` | 270 | [770](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L770) |
| `MovementTurnRate` | 0.5 | [771](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L771) |
| `StatusHealth` | 7000 | [773](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L773) |
| `StatusHealthRegen` | 1.5 | [774](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L774) |
| `StatusMana` | 800 | [775](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L775) |
| `StatusManaRegen` | 1.5 | [776](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L776) |
| `AttackType` | chaos | [782](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L782) |
| `ArmorType` | heavy | [783](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L783) |
| `VisionDaytimeRange` | 900 | [785](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L785) |
| `VisionNighttimeRange` | 800 | [786](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L786) |
| `PathfindingSearchDepthScale` | 0.5 | [788](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L788) |
| `HasInventory` | 0 | [791](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L791) |

Способности: Ability1: `14_wave_storm_bolt_extreme`.

Lua: [14_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/14_wave_bosses_extreme.lua#L1).

## Босс - `16_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:794](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L794). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [796](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L796) |
| `ModelScale` | 1.7 | [799](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L799) |
| `Level` | 1 | [800](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L800) |
| `ProjectileSpeed` | 900 | [802](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L802) |
| `ArmorPhysical` | 30 | [818](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L818) |
| `MagicalResistance` | 0 | [819](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L819) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [821](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L821) |
| `AttackDamageMin` | 1001 | [822](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L822) |
| `AttackDamageMax` | 1011 | [823](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L823) |
| `AttackRate` | 1 | [824](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L824) |
| `AttackAnimationPoint` | 0.3 | [825](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L825) |
| `AttackAcquisitionRange` | 900 | [826](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L826) |
| `AttackRange` | 400 | [827](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L827) |
| `BountyXP` | 63 | [830](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L830) |
| `BountyGoldMin` | 50 | [831](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L831) |
| `BountyGoldMax` | 50 | [832](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L832) |
| `MovementSpeed` | 270 | [835](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L835) |
| `MovementTurnRate` | 0.6 | [836](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L836) |
| `StatusHealth` | 8500 | [838](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L838) |
| `StatusHealthRegen` | 0.5 | [839](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L839) |
| `StatusMana` | 800 | [840](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L840) |
| `StatusManaRegen` | 1 | [841](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L841) |
| `AttackType` | magic | [847](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L847) |
| `ArmorType` | heavy | [848](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L848) |
| `VisionDaytimeRange` | 900 | [850](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L850) |
| `VisionNighttimeRange` | 800 | [851](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L851) |
| `PathfindingSearchDepthScale` | 0.5 | [853](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L853) |
| `HasInventory` | 0 | [856](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L856) |

Способности: Ability1: `wave_16_mana_burn_extreme`; Ability2: `16_wave_slow_extreme`.

Lua: [16_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/16_wave_bosses_extreme.lua#L1).

## Босс - `17_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:859](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L859). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [861](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L861) |
| `ModelScale` | 1.7 | [864](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L864) |
| `Level` | 1 | [865](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L865) |
| `ArmorPhysical` | 66 | [881](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L881) |
| `MagicalResistance` | 75 | [882](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L882) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [884](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L884) |
| `AttackDamageMin` | 1101 | [885](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L885) |
| `AttackDamageMax` | 1105 | [886](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L886) |
| `AttackRate` | 0.75 | [887](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L887) |
| `AttackAnimationPoint` | 0.3 | [888](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L888) |
| `AttackAcquisitionRange` | 900 | [889](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L889) |
| `AttackRange` | 110 | [890](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L890) |
| `BountyXP` | 63 | [894](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L894) |
| `BountyGoldMin` | 60 | [895](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L895) |
| `BountyGoldMax` | 60 | [896](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L896) |
| `MovementSpeed` | 350 | [899](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L899) |
| `MovementTurnRate` | 0.5 | [900](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L900) |
| `StatusHealth` | 9000 | [902](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L902) |
| `StatusHealthRegen` | 0.5 | [903](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L903) |
| `StatusMana` | 0 | [904](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L904) |
| `StatusManaRegen` | 1 | [905](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L905) |
| `StatusStartingMana` | 0 | [906](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L906) |
| `AttackType` | normal | [912](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L912) |
| `ArmorType` | heavy | [913](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L913) |
| `VisionDaytimeRange` | 900 | [915](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L915) |
| `VisionNighttimeRange` | 800 | [916](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L916) |
| `PathfindingSearchDepthScale` | 0.5 | [918](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L918) |
| `HasInventory` | 0 | [921](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L921) |

Способности: Ability1: `wave_17_evasion_extreme`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Босс - `18_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:924](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L924). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [926](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L926) |
| `ModelScale` | 1.1 | [929](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L929) |
| `Level` | 1 | [930](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L930) |
| `ArmorPhysical` | 60 | [958](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L958) |
| `MagicalResistance` | 50 | [959](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L959) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [961](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L961) |
| `AttackDamageMin` | 1801 | [962](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L962) |
| `AttackDamageMax` | 1805 | [963](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L963) |
| `AttackRate` | 1.3 | [964](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L964) |
| `AttackAnimationPoint` | 0.3 | [965](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L965) |
| `AttackAcquisitionRange` | 900 | [966](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L966) |
| `AttackRange` | 250 | [967](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L967) |
| `ProjectileSpeed` | 1900 | [969](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L969) |
| `BountyXP` | 63 | [971](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L971) |
| `BountyGoldMin` | 55 | [972](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L972) |
| `BountyGoldMax` | 55 | [973](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L973) |
| `MovementSpeed` | 350 | [976](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L976) |
| `MovementTurnRate` | 0.5 | [977](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L977) |
| `StatusHealth` | 10000 | [979](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L979) |
| `StatusHealthRegen` | 1 | [980](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L980) |
| `StatusMana` | 0 | [981](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L981) |
| `StatusManaRegen` | 0 | [982](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L982) |
| `AttackType` | pierce | [988](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L988) |
| `ArmorType` | medium | [989](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L989) |
| `VisionDaytimeRange` | 900 | [991](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L991) |
| `VisionNighttimeRange` | 800 | [992](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L992) |
| `PathfindingSearchDepthScale` | 0.5 | [994](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L994) |
| `HasInventory` | 0 | [997](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L997) |

Способности: Ability1: `wave_18_bash_extreme`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Босс - `19_wave_boss_extreme`

Источник: [extreme_mode_bosses.txt:1000](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1000). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [1002](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1002) |
| `ModelScale` | 1.3 | [1005](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1005) |
| `Level` | 1 | [1006](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1006) |
| `ArmorPhysical` | 66 | [1022](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1022) |
| `MagicalResistance` | 0 | [1023](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1023) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [1025](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1025) |
| `AttackDamageMin` | 999 | [1026](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1026) |
| `AttackDamageMax` | 999 | [1027](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1027) |
| `AttackRate` | 2.05 | [1028](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1028) |
| `AttackAnimationPoint` | 0.3 | [1029](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1029) |
| `AttackAcquisitionRange` | 900 | [1030](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1030) |
| `AttackRange` | 100 | [1031](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1031) |
| `BountyXP` | 63 | [1035](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1035) |
| `BountyGoldMin` | 65 | [1036](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1036) |
| `BountyGoldMax` | 65 | [1037](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1037) |
| `MovementSpeed` | 320 | [1040](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1040) |
| `MovementTurnRate` | 0.5 | [1041](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1041) |
| `StatusHealth` | 12000 | [1043](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1043) |
| `StatusHealthRegen` | 1 | [1044](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1044) |
| `StatusMana` | 1000 | [1045](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1045) |
| `StatusManaRegen` | 1.25 | [1046](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1046) |
| `StatusStartingMana` | 500 | [1047](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1047) |
| `AttackType` | chaos | [1053](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1053) |
| `ArmorType` | heavy | [1054](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1054) |
| `VisionDaytimeRange` | 900 | [1056](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1056) |
| `VisionNighttimeRange` | 800 | [1057](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1057) |
| `PathfindingSearchDepthScale` | 0.5 | [1059](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1059) |
| `HasInventory` | 0 | [1063](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_bosses.txt#L1063) |

Способности: Ability1: `19_wave_polymorph_extreme`.

Lua: [19_wave_bosses_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/19_wave_bosses_extreme.lua#L1).

## Кобольд - `1_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L5) |
| `ModelScale` | 1 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L8) |
| `Level` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L9) |
| `ArmorPhysical` | 2 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L25) |
| `MagicalResistance` | 0 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L26) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L28) |
| `AttackDamageMin` | 16 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L29) |
| `AttackDamageMax` | 17 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L30) |
| `AttackRate` | 1.55 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L31) |
| `AttackAnimationPoint` | 0.3 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L32) |
| `AttackAcquisitionRange` | 0 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L33) |
| `AttackRange` | 100 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L34) |
| `BountyXP` | 25 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L38) |
| `BountyGoldMin` | 4 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L39) |
| `BountyGoldMax` | 4 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L40) |
| `MovementSpeed` | 300 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L43) |
| `MovementTurnRate` | 0.5 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L44) |
| `StatusHealth` | 130 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L46) |
| `StatusHealthRegen` | 0.5 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L47) |
| `StatusMana` | 0 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L48) |
| `StatusManaRegen` | 0 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L49) |
| `AttackType` | normal | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L55) |
| `ArmorType` | heavy | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L56) |
| `VisionDaytimeRange` | 800 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L58) |
| `VisionNighttimeRange` | 800 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L59) |
| `HasInventory` | 0 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L61) |
| `PathfindingSearchDepthScale` | 0.5 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L63) |

Способности: Ability1: `wave_1_poison_extreme`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Кентавр-Воин - `2_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L68). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L70) |
| `ModelScale` | 0.85 | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L73) |
| `Level` | 1 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L74) |
| `ArmorPhysical` | 0 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L90) |
| `MagicalResistance` | 0 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L91) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [93](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L93) |
| `AttackDamageMin` | 31 | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L94) |
| `AttackDamageMax` | 32 | [95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L95) |
| `AttackRate` | 1.55 | [96](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L96) |
| `AttackAnimationPoint` | 0.3 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L97) |
| `AttackAcquisitionRange` | 0 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L98) |
| `AttackRange` | 100 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L99) |
| `BountyXP` | 25 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L103) |
| `BountyGoldMin` | 4 | [104](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L104) |
| `BountyGoldMax` | 4 | [105](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L105) |
| `MovementSpeed` | 350 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L108) |
| `MovementTurnRate` | 0.5 | [109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L109) |
| `StatusHealth` | 220 | [111](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L111) |
| `StatusHealthRegen` | 0.5 | [112](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L112) |
| `StatusMana` | 0 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L113) |
| `StatusManaRegen` | 0 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L114) |
| `AttackType` | normal | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L120) |
| `ArmorType` | heavy | [121](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L121) |
| `VisionDaytimeRange` | 800 | [123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L123) |
| `VisionNighttimeRange` | 800 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L124) |
| `HasInventory` | 0 | [126](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L126) |
| `PathfindingSearchDepthScale` | 0.5 | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L128) |

Способности: Ability1: `wave_2_centaurs_revenge_extreme`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Драконид - `3_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:133](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L133). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [135](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L135) |
| `ModelScale` | 0.75 | [138](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L138) |
| `Level` | 1 | [139](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L139) |
| `ArmorPhysical` | 0 | [155](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L155) |
| `MagicalResistance` | 0 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L156) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [158](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L158) |
| `AttackDamageMin` | 51 | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L159) |
| `AttackDamageMax` | 57 | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L160) |
| `AttackRate` | 1.5 | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L161) |
| `AttackAnimationPoint` | 0.3 | [162](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L162) |
| `AttackAcquisitionRange` | 0 | [163](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L163) |
| `AttackRange` | 100 | [164](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L164) |
| `BountyXP` | 25 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L168) |
| `BountyGoldMin` | 5 | [169](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L169) |
| `BountyGoldMax` | 5 | [170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L170) |
| `MovementSpeed` | 270 | [173](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L173) |
| `MovementTurnRate` | 0.6 | [174](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L174) |
| `StatusHealth` | 375 | [176](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L176) |
| `StatusHealthRegen` | 0.5 | [177](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L177) |
| `StatusMana` | 200 | [178](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L178) |
| `StatusManaRegen` | 1 | [179](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L179) |
| `AttackType` | normal | [185](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L185) |
| `ArmorType` | medium | [186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L186) |
| `VisionDaytimeRange` | 800 | [188](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L188) |
| `VisionNighttimeRange` | 800 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L189) |
| `HasInventory` | 0 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L191) |
| `PathfindingSearchDepthScale` | 0.5 | [193](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L193) |

Способности: Ability1: `wave_3_evasion_extreme`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Темный тролль - `4_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:198](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L198). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [200](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L200) |
| `ModelScale` | 0.8 | [203](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L203) |
| `Level` | 1 | [204](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L204) |
| `ArmorPhysical` | 0 | [220](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L220) |
| `MagicalResistance` | 0 | [221](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L221) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [223](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L223) |
| `AttackDamageMin` | 41 | [224](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L224) |
| `AttackDamageMax` | 44 | [225](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L225) |
| `AttackRate` | 1.3 | [226](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L226) |
| `AttackAnimationPoint` | 0.3 | [227](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L227) |
| `AttackAcquisitionRange` | 0 | [228](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L228) |
| `AttackRange` | 450 | [229](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L229) |
| `ProjectileSpeed` | 1200 | [231](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L231) |
| `BountyXP` | 25 | [233](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L233) |
| `BountyGoldMin` | 5 | [234](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L234) |
| `BountyGoldMax` | 5 | [235](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L235) |
| `MovementSpeed` | 270 | [238](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L238) |
| `MovementTurnRate` | 0.5 | [239](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L239) |
| `StatusHealth` | 400 | [241](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L241) |
| `StatusHealthRegen` | 0.5 | [242](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L242) |
| `StatusMana` | 0 | [243](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L243) |
| `StatusManaRegen` | 0 | [244](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L244) |
| `AttackType` | pierce | [250](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L250) |
| `ArmorType` | heavy | [251](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L251) |
| `VisionDaytimeRange` | 800 | [253](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L253) |
| `VisionNighttimeRange` | 800 | [254](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L254) |
| `HasInventory` | 0 | [256](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L256) |
| `PathfindingSearchDepthScale` | 0.5 | [258](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L258) |

Способности: Ability1: `4_wave_true_hit_extreme`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Головорез - `6_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:263](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L263). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [267](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L267) |
| `ModelScale` | 0.85 | [270](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L270) |
| `Level` | 1 | [271](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L271) |
| `ArmorPhysical` | 2 | [282](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L282) |
| `MagicalResistance` | 0 | [283](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L283) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [285](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L285) |
| `AttackDamageMin` | 76 | [286](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L286) |
| `AttackDamageMax` | 78 | [287](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L287) |
| `AttackRate` | 0.85 | [288](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L288) |
| `AttackAnimationPoint` | 0.3 | [289](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L289) |
| `AttackAcquisitionRange` | 0 | [290](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L290) |
| `AttackRange` | 100 | [291](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L291) |
| `BountyXP` | 33 | [295](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L295) |
| `BountyGoldMin` | 6 | [296](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L296) |
| `BountyGoldMax` | 6 | [297](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L297) |
| `MovementSpeed` | 320 | [300](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L300) |
| `MovementTurnRate` | 0.6 | [301](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L301) |
| `StatusHealth` | 750 | [303](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L303) |
| `StatusHealthRegen` | 0.5 | [304](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L304) |
| `StatusMana` | 300 | [305](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L305) |
| `StatusManaRegen` | 0.75 | [306](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L306) |
| `AttackType` | normal | [312](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L312) |
| `ArmorType` | heavy | [313](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L313) |
| `VisionDaytimeRange` | 800 | [315](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L315) |
| `VisionNighttimeRange` | 800 | [316](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L316) |
| `PathfindingSearchDepthScale` | 0.5 | [318](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L318) |
| `HasInventory` | 0 | [322](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L322) |

Способности: Ability1: `wave_6_assassin_extreme`; Ability2: `wave_6_invisibility`.

Lua: [6_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/6_wave_creeps.lua#L1).

## Чумной Энт - `7_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:325](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L325). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [327](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L327) |
| `ModelScale` | 1 | [330](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L330) |
| `Level` | 1 | [331](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L331) |
| `ArmorPhysical` | 1 | [347](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L347) |
| `MagicalResistance` | 0 | [348](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L348) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [350](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L350) |
| `AttackDamageMin` | 111 | [351](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L351) |
| `AttackDamageMax` | 115 | [352](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L352) |
| `AttackRate` | 1.05 | [353](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L353) |
| `AttackAnimationPoint` | 0.3 | [354](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L354) |
| `AttackAcquisitionRange` | 0 | [355](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L355) |
| `AttackRange` | 100 | [356](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L356) |
| `BountyXP` | 33 | [360](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L360) |
| `BountyGoldMin` | 6 | [361](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L361) |
| `BountyGoldMax` | 6 | [362](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L362) |
| `MovementSpeed` | 400 | [365](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L365) |
| `MovementTurnRate` | 0.5 | [366](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L366) |
| `StatusHealth` | 800 | [368](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L368) |
| `StatusHealthRegen` | 0.5 | [369](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L369) |
| `StatusMana` | 250 | [370](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L370) |
| `StatusManaRegen` | 1 | [371](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L371) |
| `AttackType` | normal | [377](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L377) |
| `ArmorType` | heavy | [378](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L378) |
| `VisionDaytimeRange` | 800 | [380](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L380) |
| `VisionNighttimeRange` | 800 | [381](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L381) |
| `PathfindingSearchDepthScale` | 0.5 | [383](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L383) |
| `HasInventory` | 0 | [387](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L387) |

Способности: Ability1: `7_wave_plague`; Ability2: `7_wave_sharp_claws_extreme`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Властитель - `8_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:390](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L390). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [392](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L392) |
| `ModelScale` | 1.25 | [395](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L395) |
| `Level` | 1 | [396](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L396) |
| `ArmorPhysical` | 1 | [412](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L412) |
| `MagicalResistance` | 80 | [413](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L413) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [415](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L415) |
| `AttackDamageMin` | 126 | [416](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L416) |
| `AttackDamageMax` | 129 | [417](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L417) |
| `AttackRate` | 1.05 | [418](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L418) |
| `AttackAnimationPoint` | 0.3 | [419](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L419) |
| `AttackAcquisitionRange` | 0 | [420](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L420) |
| `AttackRange` | 100 | [421](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L421) |
| `BountyXP` | 33 | [425](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L425) |
| `BountyGoldMin` | 7 | [426](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L426) |
| `BountyGoldMax` | 7 | [427](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L427) |
| `MovementSpeed` | 270 | [430](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L430) |
| `MovementTurnRate` | 0.6 | [431](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L431) |
| `StatusHealth` | 950 | [433](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L433) |
| `StatusHealthRegen` | 0.25 | [434](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L434) |
| `StatusMana` | 300 | [435](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L435) |
| `StatusManaRegen` | 0.75 | [436](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L436) |
| `AttackType` | normal | [442](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L442) |
| `ArmorType` | heavy | [443](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L443) |
| `VisionDaytimeRange` | 800 | [445](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L445) |
| `VisionNighttimeRange` | 800 | [446](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L446) |
| `PathfindingSearchDepthScale` | 0.5 | [448](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L448) |
| `HasInventory` | 0 | [452](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L452) |

Способности: Ability1: `wave_8_cleave`; Ability2: `wave_8_lifesteal_extreme`; Ability3: `custom_spell_immunity`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Дух Воды - `9_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:455](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L455). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [457](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L457) |
| `ModelScale` | 0.7 | [460](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L460) |
| `Level` | 1 | [461](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L461) |
| `ProjectileSpeed` | 1300 | [463](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L463) |
| `ArmorPhysical` | 1 | [480](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L480) |
| `MagicalResistance` | 0 | [481](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L481) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [483](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L483) |
| `AttackDamageMin` | 141 | [484](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L484) |
| `AttackDamageMax` | 150 | [485](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L485) |
| `AttackRate` | 1.0 | [486](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L486) |
| `AttackAnimationPoint` | 0.3 | [487](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L487) |
| `AttackAcquisitionRange` | 0 | [488](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L488) |
| `AttackRange` | 450 | [489](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L489) |
| `BountyXP` | 33 | [491](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L491) |
| `BountyGoldMin` | 7 | [492](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L492) |
| `BountyGoldMax` | 7 | [493](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L493) |
| `MovementSpeed` | 220 | [496](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L496) |
| `MovementTurnRate` | 0.5 | [497](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L497) |
| `StatusHealth` | 975 | [499](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L499) |
| `StatusHealthRegen` | 0.5 | [500](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L500) |
| `StatusMana` | 0 | [501](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L501) |
| `StatusManaRegen` | 0 | [502](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L502) |
| `AttackType` | pierce | [508](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L508) |
| `ArmorType` | medium | [509](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L509) |
| `VisionDaytimeRange` | 800 | [511](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L511) |
| `VisionNighttimeRange` | 800 | [512](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L512) |
| `PathfindingSearchDepthScale` | 0.5 | [514](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L514) |
| `HasInventory` | 0 | [518](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L518) |

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Опустошитель - `11_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:521](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L521). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [523](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L523) |
| `ModelScale` | 0.8 | [526](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L526) |
| `Level` | 1 | [527](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L527) |
| `ArmorPhysical` | 15 | [543](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L543) |
| `MagicalResistance` | 0 | [544](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L544) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [546](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L546) |
| `AttackDamageMin` | 221 | [547](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L547) |
| `AttackDamageMax` | 226 | [548](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L548) |
| `AttackRate` | 0.55 | [549](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L549) |
| `AttackAnimationPoint` | 0.3 | [550](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L550) |
| `AttackAcquisitionRange` | 0 | [551](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L551) |
| `AttackRange` | 100 | [552](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L552) |
| `BountyXP` | 48 | [556](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L556) |
| `BountyGoldMin` | 8 | [557](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L557) |
| `BountyGoldMax` | 8 | [558](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L558) |
| `MovementSpeed` | 420 | [561](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L561) |
| `MovementTurnRate` | 0.5 | [562](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L562) |
| `StatusHealth` | 1250 | [564](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L564) |
| `StatusHealthRegen` | 0.5 | [565](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L565) |
| `StatusMana` | 300 | [566](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L566) |
| `StatusManaRegen` | 0 | [567](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L567) |
| `AttackType` | chaos | [573](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L573) |
| `ArmorType` | medium | [574](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L574) |
| `VisionDaytimeRange` | 800 | [576](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L576) |
| `VisionNighttimeRange` | 800 | [577](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L577) |
| `PathfindingSearchDepthScale` | 0.5 | [579](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L579) |
| `HasInventory` | 0 | [583](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L583) |

Способности: Ability1: `11_wave_immolation_extreme`; Ability2: `wave_11_mana_break_extreme_creeps`; Ability3: `wave_11_cleave`.

Lua: [11_wave_creeps_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/11_wave_creeps_extreme.lua#L1).

## Адский Медведь - `12_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:586](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L586). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [588](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L588) |
| `ModelScale` | 0.8 | [591](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L591) |
| `Level` | 1 | [592](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L592) |
| `ArmorPhysical` | 8 | [608](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L608) |
| `MagicalResistance` | 0 | [609](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L609) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [611](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L611) |
| `AttackDamageMin` | 241 | [612](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L612) |
| `AttackDamageMax` | 244 | [613](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L613) |
| `AttackRate` | 0.65 | [614](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L614) |
| `AttackAnimationPoint` | 0.3 | [615](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L615) |
| `AttackAcquisitionRange` | 0 | [616](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L616) |
| `AttackRange` | 100 | [617](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L617) |
| `BountyXP` | 48 | [621](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L621) |
| `BountyGoldMin` | 8 | [622](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L622) |
| `BountyGoldMax` | 8 | [623](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L623) |
| `MovementSpeed` | 300 | [626](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L626) |
| `MovementTurnRate` | 0.5 | [627](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L627) |
| `StatusHealth` | 1400 | [629](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L629) |
| `StatusHealthRegen` | 0.5 | [630](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L630) |
| `StatusMana` | 400 | [631](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L631) |
| `StatusManaRegen` | 0 | [632](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L632) |
| `AttackType` | normal | [638](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L638) |
| `ArmorType` | heavy | [639](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L639) |
| `VisionDaytimeRange` | 800 | [641](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L641) |
| `VisionNighttimeRange` | 800 | [642](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L642) |
| `HasInventory` | 0 | [644](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L644) |

Способности: Ability1: `12_wave_stomp`; Ability2: `wave_12_crit_extreme`.

Lua: [12_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/12_wave_creeps.lua#L1).

## Каменный Голем - `13_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:649](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L649). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [651](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L651) |
| `ModelScale` | 1 | [654](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L654) |
| `Level` | 1 | [655](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L655) |
| `ArmorPhysical` | 10 | [671](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L671) |
| `MagicalResistance` | 0 | [672](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L672) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [674](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L674) |
| `AttackDamageMin` | 281 | [675](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L675) |
| `AttackDamageMax` | 285 | [676](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L676) |
| `AttackRate` | 0.75 | [677](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L677) |
| `AttackAnimationPoint` | 0.3 | [678](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L678) |
| `AttackAcquisitionRange` | 0 | [679](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L679) |
| `AttackRange` | 100 | [680](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L680) |
| `BountyXP` | 48 | [684](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L684) |
| `BountyGoldMin` | 9 | [685](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L685) |
| `BountyGoldMax` | 9 | [686](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L686) |
| `MovementSpeed` | 270 | [689](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L689) |
| `MovementTurnRate` | 0.5 | [690](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L690) |
| `StatusHealth` | 1600 | [692](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L692) |
| `StatusHealthRegen` | 0.5 | [693](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L693) |
| `StatusMana` | 100 | [694](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L694) |
| `StatusManaRegen` | 1 | [695](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L695) |
| `AttackType` | normal | [701](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L701) |
| `ArmorType` | heavy | [702](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L702) |
| `VisionDaytimeRange` | 800 | [704](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L704) |
| `VisionNighttimeRange` | 800 | [705](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L705) |
| `PathfindingSearchDepthScale` | 0.5 | [707](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L707) |
| `HasInventory` | 0 | [711](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L711) |

Способности: Ability1: `13_wave_hurl_boulder`; Ability2: `custom_spell_immunity`.

Lua: [13_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/13_wave_creeps.lua#L1).

## Громовая Ящерица - `14_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:714](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L714). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [716](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L716) |
| `ModelScale` | 0.7 | [719](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L719) |
| `Level` | 1 | [720](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L720) |
| `ProjectileSpeed` | 1500 | [722](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L722) |
| `ArmorPhysical` | 15 | [739](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L739) |
| `MagicalResistance` | 0 | [740](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L740) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [742](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L742) |
| `AttackDamageMin` | 191 | [743](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L743) |
| `AttackDamageMax` | 200 | [744](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L744) |
| `AttackRate` | 0.8 | [745](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L745) |
| `AttackAnimationPoint` | 0.3 | [746](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L746) |
| `AttackAcquisitionRange` | 0 | [747](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L747) |
| `AttackRange` | 500 | [748](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L748) |
| `SplashAttack` | 1 | [750](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L750) |
| `SplashFullDamageRadius` | 75 | [751](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L751) |
| `SplashMediumRadius` | 150 | [752](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L752) |
| `SplashMediumDamage` | 0.5 | [753](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L753) |
| `SplashSmallRadius` | 225 | [754](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L754) |
| `SplashSmallDamage` | 0.25 | [755](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L755) |
| `BountyXP` | 48 | [757](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L757) |
| `BountyGoldMin` | 10 | [758](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L758) |
| `BountyGoldMax` | 10 | [759](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L759) |
| `MovementSpeed` | 270 | [762](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L762) |
| `MovementTurnRate` | 0.5 | [763](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L763) |
| `StatusHealth` | 1500 | [765](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L765) |
| `StatusHealthRegen` | 1.5 | [766](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L766) |
| `StatusMana` | 500 | [767](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L767) |
| `StatusManaRegen` | 1.5 | [768](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L768) |
| `AttackType` | chaos | [774](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L774) |
| `ArmorType` | heavy | [775](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L775) |
| `VisionDaytimeRange` | 800 | [777](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L777) |
| `VisionNighttimeRange` | 800 | [778](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L778) |
| `PathfindingSearchDepthScale` | 0.5 | [780](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L780) |
| `HasInventory` | 0 | [784](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L784) |

Способности: Ability1: `14_wave_shadow_strike_extreme`.

Lua: [14_wave_creeps_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/14_wave_creeps_extreme.lua#L1).

## Призрак - `16_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:787](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L787). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [789](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L789) |
| `ModelScale` | 1 | [792](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L792) |
| `Level` | 1 | [793](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L793) |
| `ProjectileSpeed` | 900 | [795](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L795) |
| `ArmorPhysical` | 20 | [811](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L811) |
| `MagicalResistance` | 0 | [812](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L812) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [814](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L814) |
| `AttackDamageMin` | 376 | [815](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L815) |
| `AttackDamageMax` | 386 | [816](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L816) |
| `AttackRate` | 0.8 | [817](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L817) |
| `AttackAnimationPoint` | 0.3 | [818](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L818) |
| `AttackAcquisitionRange` | 0 | [819](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L819) |
| `AttackRange` | 400 | [820](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L820) |
| `BountyXP` | 63 | [822](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L822) |
| `BountyGoldMin` | 11 | [823](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L823) |
| `BountyGoldMax` | 11 | [824](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L824) |
| `MovementSpeed` | 270 | [827](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L827) |
| `MovementTurnRate` | 0.6 | [828](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L828) |
| `StatusHealth` | 2000 | [830](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L830) |
| `StatusHealthRegen` | 0.5 | [831](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L831) |
| `StatusMana` | 400 | [832](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L832) |
| `StatusManaRegen` | 1 | [833](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L833) |
| `AttackType` | magic | [839](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L839) |
| `ArmorType` | heavy | [840](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L840) |
| `VisionDaytimeRange` | 800 | [842](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L842) |
| `VisionNighttimeRange` | 800 | [843](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L843) |
| `PathfindingSearchDepthScale` | 0.5 | [845](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L845) |
| `HasInventory` | 0 | [849](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L849) |

Способности: Ability1: `wave_16_mana_burn_extreme`; Ability2: `wave_16_evasion_extreme`.

Lua: [16_wave_creeps_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/16_wave_creeps_extreme.lua#L1).

## Хвататель - `17_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:852](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L852). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [854](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L854) |
| `ModelScale` | 1 | [857](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L857) |
| `Level` | 1 | [858](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L858) |
| `ArmorPhysical` | 40 | [874](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L874) |
| `MagicalResistance` | 75 | [875](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L875) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [877](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L877) |
| `AttackDamageMin` | 451 | [878](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L878) |
| `AttackDamageMax` | 455 | [879](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L879) |
| `AttackRate` | 1.05 | [880](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L880) |
| `AttackAnimationPoint` | 0.3 | [881](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L881) |
| `AttackAcquisitionRange` | 0 | [882](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L882) |
| `AttackRange` | 110 | [883](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L883) |
| `BountyXP` | 63 | [887](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L887) |
| `BountyGoldMin` | 11 | [888](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L888) |
| `BountyGoldMax` | 11 | [889](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L889) |
| `MovementSpeed` | 350 | [892](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L892) |
| `MovementTurnRate` | 0.5 | [893](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L893) |
| `StatusHealth` | 2500 | [895](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L895) |
| `StatusHealthRegen` | 0.5 | [896](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L896) |
| `StatusMana` | 300 | [897](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L897) |
| `StatusManaRegen` | 1 | [898](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L898) |
| `AttackType` | normal | [904](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L904) |
| `ArmorType` | heavy | [905](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L905) |
| `VisionDaytimeRange` | 800 | [907](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L907) |
| `VisionNighttimeRange` | 800 | [908](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L908) |
| `PathfindingSearchDepthScale` | 0.5 | [910](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L910) |
| `HasInventory` | 0 | [914](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L914) |

Способности: Ability1: `17_wave_purge_extreme`.

Lua: [17_wave_creeps_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/17_wave_creeps_extreme.lua#L1).

## Хранитель Пустоты - `18_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:917](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L917). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [919](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L919) |
| `ModelScale` | 1.1 | [922](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L922) |
| `Level` | 1 | [923](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L923) |
| `ArmorPhysical` | 10 | [940](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L940) |
| `MagicalResistance` | 50 | [941](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L941) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [943](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L943) |
| `AttackDamageMin` | 451 | [944](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L944) |
| `AttackDamageMax` | 455 | [945](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L945) |
| `AttackRate` | 1.1 | [946](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L946) |
| `AttackAnimationPoint` | 0.3 | [947](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L947) |
| `AttackAcquisitionRange` | 0 | [948](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L948) |
| `AttackRange` | 250 | [949](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L949) |
| `ProjectileSpeed` | 1900 | [951](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L951) |
| `BountyXP` | 63 | [953](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L953) |
| `BountyGoldMin` | 11 | [954](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L954) |
| `BountyGoldMax` | 11 | [955](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L955) |
| `MovementSpeed` | 350 | [958](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L958) |
| `MovementTurnRate` | 0.5 | [959](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L959) |
| `StatusHealth` | 2600 | [961](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L961) |
| `StatusHealthRegen` | 1 | [962](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L962) |
| `StatusMana` | 150 | [963](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L963) |
| `StatusManaRegen` | 1 | [964](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L964) |
| `AttackType` | pierce | [970](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L970) |
| `ArmorType` | medium | [971](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L971) |
| `VisionDaytimeRange` | 800 | [973](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L973) |
| `VisionNighttimeRange` | 800 | [974](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L974) |
| `PathfindingSearchDepthScale` | 0.5 | [976](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L976) |
| `HasInventory` | 0 | [980](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L980) |

Способности: Ability1: `18_wave_silence_extreme`.

Lua: [18_wave_creeps_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/18_wave_creeps_extreme.lua#L1).

## Адский Сатир - `19_wave_creep_extreme`

Источник: [extreme_mode_creeps.txt:991](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L991). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [993](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L993) |
| `ModelScale` | 1 | [996](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L996) |
| `Level` | 1 | [997](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L997) |
| `ArmorPhysical` | 25 | [1013](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1013) |
| `MagicalResistance` | 0 | [1014](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1014) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [1016](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1016) |
| `AttackDamageMin` | 666 | [1017](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1017) |
| `AttackDamageMax` | 666 | [1018](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1018) |
| `AttackRate` | 1.75 | [1019](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1019) |
| `AttackAnimationPoint` | 0.3 | [1020](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1020) |
| `AttackAcquisitionRange` | 0 | [1021](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1021) |
| `AttackRange` | 100 | [1022](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1022) |
| `BountyXP` | 63 | [1026](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1026) |
| `BountyGoldMin` | 12 | [1027](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1027) |
| `BountyGoldMax` | 12 | [1028](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1028) |
| `MovementSpeed` | 320 | [1031](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1031) |
| `MovementTurnRate` | 0.5 | [1032](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1032) |
| `StatusHealth` | 3000 | [1034](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1034) |
| `StatusHealthRegen` | 1 | [1035](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1035) |
| `StatusMana` | 500 | [1036](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1036) |
| `StatusManaRegen` | 1.25 | [1037](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1037) |
| `AttackType` | chaos | [1043](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1043) |
| `ArmorType` | heavy | [1044](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1044) |
| `VisionDaytimeRange` | 800 | [1046](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1046) |
| `VisionNighttimeRange` | 800 | [1047](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1047) |
| `PathfindingSearchDepthScale` | 0.5 | [1049](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1049) |
| `HasInventory` | 0 | [1053](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/extreme_mode_creeps.txt#L1053) |

Способности: Ability1: `19_wave_faerie_fire_extreme`.

Lua: [19_wave_creeps_extreme.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/19_wave_creeps_extreme.lua#L1).

## Дух Огня - `firelord_lava_spawn1`

Источник: [firelord_units.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L7) |
| `ModelScale` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L9) |
| `Level` | 4 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L10) |
| `FormationRank` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L11) |
| `IsSummoned` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L12) |
| `ArmorPhysical` | 1 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L16) |
| `MagicalResistance` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L17) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L21) |
| `AttackDamageMin` | 10 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L23) |
| `AttackDamageMax` | 20 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L24) |
| `AttackRate` | 0.6 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L26) |
| `AttackAnimationPoint` | 0.4 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L27) |
| `AttackAcquisitionRange` | 500 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L28) |
| `AttackRange` | 300 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L29) |
| `ProjectileSpeed` | 1000 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L31) |
| `BountyGoldMin` | 0 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L35) |
| `BountyGoldMax` | 0 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L36) |
| `MovementSpeed` | 300 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L41) |
| `MovementTurnRate` | 0.5 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L42) |
| `StatusHealth` | 450 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L46) |
| `StatusHealthRegen` | 0.25 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L47) |
| `StatusMana` | 0 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L48) |
| `StatsManaRegen` | 0 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L49) |
| `VisionDaytimeRange` | 900 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L53) |
| `VisionNighttimeRange` | 800 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L54) |
| `AttackType` | chaos | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L62) |
| `ArmorType` | heavy | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L63) |
| `HealthBarOffset` | 140 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L66) |

## Дух Огня - `firelord_lava_spawn2`

Источник: [firelord_units.txt:69](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L69). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L73) |
| `ModelScale` | 1 | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L75) |
| `Level` | 4 | [76](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L76) |
| `FormationRank` | 1 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L77) |
| `IsSummoned` | 1 | [78](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L78) |
| `ArmorPhysical` | 2 | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L82) |
| `MagicalResistance` | 0 | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L83) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [87](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L87) |
| `AttackDamageMin` | 20 | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L89) |
| `AttackDamageMax` | 35 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L90) |
| `AttackRate` | 0.55 | [92](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L92) |
| `AttackAnimationPoint` | 0.4 | [93](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L93) |
| `AttackAcquisitionRange` | 500 | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L94) |
| `AttackRange` | 400 | [95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L95) |
| `ProjectileSpeed` | 1000 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L97) |
| `BountyGoldMin` | 0 | [101](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L101) |
| `BountyGoldMax` | 0 | [102](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L102) |
| `MovementSpeed` | 300 | [107](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L107) |
| `MovementTurnRate` | 0.5 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L108) |
| `StatusHealth` | 600 | [112](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L112) |
| `StatusHealthRegen` | 0.25 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L113) |
| `StatusMana` | 0 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L114) |
| `StatsManaRegen` | 0 | [115](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L115) |
| `VisionDaytimeRange` | 900 | [119](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L119) |
| `VisionNighttimeRange` | 800 | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L120) |
| `AttackType` | chaos | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L128) |
| `ArmorType` | heavy | [129](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L129) |
| `HealthBarOffset` | 140 | [132](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L132) |

## Дух Огня - `firelord_lava_spawn3`

Источник: [firelord_units.txt:135](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L135). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [139](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L139) |
| `ModelScale` | 1 | [141](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L141) |
| `Level` | 4 | [142](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L142) |
| `FormationRank` | 1 | [143](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L143) |
| `IsSummoned` | 1 | [144](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L144) |
| `ArmorPhysical` | 4 | [148](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L148) |
| `MagicalResistance` | 0 | [149](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L149) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [153](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L153) |
| `AttackDamageMin` | 30 | [155](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L155) |
| `AttackDamageMax` | 60 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L156) |
| `AttackRate` | 0.5 | [158](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L158) |
| `AttackAnimationPoint` | 0.4 | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L159) |
| `AttackAcquisitionRange` | 500 | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L160) |
| `AttackRange` | 500 | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L161) |
| `ProjectileSpeed` | 1000 | [163](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L163) |
| `BountyGoldMin` | 0 | [167](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L167) |
| `BountyGoldMax` | 0 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L168) |
| `MovementSpeed` | 300 | [173](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L173) |
| `MovementTurnRate` | 0.5 | [174](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L174) |
| `StatusHealth` | 750 | [178](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L178) |
| `StatusHealthRegen` | 0.25 | [179](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L179) |
| `StatusMana` | 0 | [180](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L180) |
| `StatsManaRegen` | 0 | [181](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L181) |
| `VisionDaytimeRange` | 900 | [185](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L185) |
| `VisionNighttimeRange` | 800 | [186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L186) |
| `AttackType` | chaos | [194](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L194) |
| `ArmorType` | heavy | [195](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L195) |
| `HealthBarOffset` | 140 | [198](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L198) |

## Огненная Змея - `npc_serpent_ward_1`

Источник: [firelord_units.txt:201](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L201). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_shadowshaman_serpentward | [205](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L205) |
| `Level` | 6 | [208](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L208) |
| `ModelScale` | 1 | [209](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L209) |
| `ArmorPhysical` | 0 | [223](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L223) |
| `MagicalResistance` | 0 | [224](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L224) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [228](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L228) |
| `AttackDamageMin` | 20 | [229](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L229) |
| `AttackDamageMax` | 20 | [230](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L230) |
| `AttackRate` | 1 | [232](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L232) |
| `AttackAnimationPoint` | 0.3 | [233](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L233) |
| `AttackAcquisitionRange` | 600 | [234](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L234) |
| `AttackRange` | 600 | [235](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L235) |
| `ProjectileSpeed` | 900 | [237](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L237) |
| `HealthBarOffset` | 170 | [242](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L242) |
| `MovementSpeed` | 0 | [247](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L247) |
| `MovementTurnRate` | 0.5 | [248](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L248) |
| `BountyXP` | 0 | [252](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L252) |
| `BountyGoldMin` | 0 | [253](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L253) |
| `BountyGoldMax` | 0 | [254](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L254) |
| `StatusHealth` | 250 | [258](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L258) |
| `StatusHealthRegen` | 0.25 | [259](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L259) |
| `StatusMana` | 0 | [260](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L260) |
| `StatusManaRegen` | 0 | [261](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L261) |
| `AttackType` | chaos | [269](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L269) |
| `ArmorType` | heavy | [270](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L270) |
| `VisionDaytimeRange` | 900 | [274](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L274) |
| `VisionNighttimeRange` | 800 | [275](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L275) |
| `HasInventory` | 0 | [278](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L278) |

Способности: Ability1: `neutral_spell_immunity`.

## Огненная Змея - `npc_serpent_ward_2`

Источник: [firelord_units.txt:281](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L281). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_shadowshaman_serpentward | [285](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L285) |
| `Level` | 6 | [288](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L288) |
| `ModelScale` | 1 | [289](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L289) |
| `ArmorPhysical` | 0 | [303](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L303) |
| `MagicalResistance` | 0 | [304](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L304) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [308](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L308) |
| `AttackDamageMin` | 40 | [309](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L309) |
| `AttackDamageMax` | 40 | [310](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L310) |
| `AttackRate` | 1 | [312](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L312) |
| `AttackAnimationPoint` | 0.3 | [313](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L313) |
| `AttackAcquisitionRange` | 600 | [314](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L314) |
| `AttackRange` | 700 | [315](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L315) |
| `ProjectileSpeed` | 900 | [317](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L317) |
| `HealthBarOffset` | 170 | [322](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L322) |
| `MovementSpeed` | 0 | [327](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L327) |
| `MovementTurnRate` | 0.5 | [328](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L328) |
| `BountyXP` | 0 | [332](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L332) |
| `BountyGoldMin` | 0 | [333](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L333) |
| `BountyGoldMax` | 0 | [334](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L334) |
| `StatusHealth` | 500 | [338](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L338) |
| `StatusHealthRegen` | 0.25 | [339](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L339) |
| `StatusMana` | 0 | [340](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L340) |
| `StatusManaRegen` | 0 | [341](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L341) |
| `AttackType` | chaos | [349](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L349) |
| `ArmorType` | heavy | [350](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L350) |
| `VisionDaytimeRange` | 900 | [354](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L354) |
| `VisionNighttimeRange` | 800 | [355](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L355) |
| `HasInventory` | 0 | [358](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L358) |

Способности: Ability1: `neutral_spell_immunity`.

## Огненная Змея - `npc_serpent_ward_3`

Источник: [firelord_units.txt:361](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L361). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_shadowshaman_serpentward | [365](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L365) |
| `Level` | 6 | [368](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L368) |
| `ModelScale` | 1 | [369](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L369) |
| `ArmorPhysical` | 0 | [383](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L383) |
| `MagicalResistance` | 0 | [384](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L384) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [388](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L388) |
| `AttackDamageMin` | 60 | [389](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L389) |
| `AttackDamageMax` | 60 | [390](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L390) |
| `AttackRate` | 1 | [392](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L392) |
| `AttackAnimationPoint` | 0.3 | [393](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L393) |
| `AttackAcquisitionRange` | 600 | [394](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L394) |
| `AttackRange` | 800 | [395](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L395) |
| `ProjectileSpeed` | 900 | [397](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L397) |
| `HealthBarOffset` | 170 | [402](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L402) |
| `MovementSpeed` | 0 | [407](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L407) |
| `MovementTurnRate` | 0.5 | [408](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L408) |
| `BountyXP` | 0 | [412](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L412) |
| `BountyGoldMin` | 0 | [413](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L413) |
| `BountyGoldMax` | 0 | [414](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L414) |
| `StatusHealth` | 750 | [418](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L418) |
| `StatusHealthRegen` | 0.25 | [419](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L419) |
| `StatusMana` | 0 | [420](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L420) |
| `StatusManaRegen` | 0 | [421](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L421) |
| `AttackType` | chaos | [429](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L429) |
| `ArmorType` | heavy | [430](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L430) |
| `VisionDaytimeRange` | 900 | [434](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L434) |
| `VisionNighttimeRange` | 800 | [435](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L435) |
| `HasInventory` | 0 | [438](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L438) |

Способности: Ability1: `neutral_spell_immunity`.

## Огненная Змея - `npc_serpent_ward_4`

Источник: [firelord_units.txt:441](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L441). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_shadowshaman_serpentward | [445](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L445) |
| `Level` | 6 | [448](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L448) |
| `ModelScale` | 1 | [449](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L449) |
| `ArmorPhysical` | 0 | [463](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L463) |
| `MagicalResistance` | 0 | [464](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L464) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [468](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L468) |
| `AttackDamageMin` | 160 | [469](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L469) |
| `AttackDamageMax` | 160 | [470](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L470) |
| `AttackRate` | 1 | [472](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L472) |
| `AttackAnimationPoint` | 0.3 | [473](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L473) |
| `AttackAcquisitionRange` | 600 | [474](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L474) |
| `AttackRange` | 1000 | [475](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L475) |
| `ProjectileSpeed` | 900 | [477](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L477) |
| `HealthBarOffset` | 170 | [482](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L482) |
| `MovementSpeed` | 0 | [487](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L487) |
| `MovementTurnRate` | 0.5 | [488](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L488) |
| `BountyXP` | 0 | [492](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L492) |
| `BountyGoldMin` | 0 | [493](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L493) |
| `BountyGoldMax` | 0 | [494](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L494) |
| `StatusHealth` | 1200 | [498](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L498) |
| `StatusHealthRegen` | 0.25 | [499](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L499) |
| `StatusMana` | 0 | [500](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L500) |
| `StatusManaRegen` | 0 | [501](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L501) |
| `AttackType` | chaos | [509](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L509) |
| `ArmorType` | heavy | [510](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L510) |
| `VisionDaytimeRange` | 900 | [514](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L514) |
| `VisionNighttimeRange` | 800 | [515](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L515) |
| `HasInventory` | 0 | [518](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L518) |

Способности: Ability1: `neutral_spell_immunity`.

## Огненная Ловушка - `fire_trap_unit`

Источник: [firelord_units.txt:521](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L521). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_ward_base_truesight | [524](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L524) |
| `Level` | 0 | [526](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L526) |
| `ModelScale` | 1 | [527](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L527) |
| `HealthBarOffset` | 140 | [533](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L533) |
| `StatusHealth` | 100 | [541](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L541) |
| `StatusHealthRegen` | 0 | [542](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L542) |
| `StatusMana` | 0 | [543](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L543) |
| `StatusManaRegen` | 0 | [544](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L544) |
| `AttackType` | pierce | [552](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L552) |
| `ArmorType` | unarmored | [553](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L553) |
| `VisionDaytimeRange` | 400 | [557](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L557) |
| `VisionNighttimeRange` | 400 | [558](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/firelord_units.txt#L558) |

## Дух Воды - `npc_water_elemental_1`

Источник: [hermit_water_elementals.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L8) |
| `Level` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L12) |
| `ModelScale` | 0.6 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L13) |
| `CanBeDominated` | 0 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L14) |
| `ArmorPhysical` | 2 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L27) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L31) |
| `AttackDamageMin` | 15 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L32) |
| `AttackDamageMax` | 20 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L33) |
| `AttackRate` | 1.3 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L34) |
| `AttackAnimationPoint` | 0.45 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L35) |
| `AttackAcquisitionRange` | 400 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L36) |
| `AttackRange` | 300 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L37) |
| `ProjectileSpeed` | 1300 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L39) |
| `BountyXP` | 0 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L43) |
| `BountyGoldMin` | 0 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L44) |
| `BountyGoldMax` | 0 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L45) |
| `RingRadius` | 70 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L49) |
| `HealthBarOffset` | 190 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L50) |
| `MovementSpeed` | 220 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L55) |
| `MovementTurnRate` | 0.6 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L56) |
| `StatusHealth` | 300 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L60) |
| `StatusHealthRegen` | 0.25 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L61) |
| `StatusMana` | 0 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L62) |
| `StatusManaRegen` | 0.0 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L63) |
| `VisionDaytimeRange` | 900 | [67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L67) |
| `VisionNighttimeRange` | 800 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L68) |
| `AttackType` | magic | [76](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L76) |
| `ArmorType` | heavy | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L77) |

## Дух Воды - `npc_water_elemental_2`

Источник: [hermit_water_elementals.txt:92](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L92). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L97) |
| `Level` | 2 | [101](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L101) |
| `ModelScale` | 0.7 | [102](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L102) |
| `CanBeDominated` | 0 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L103) |
| `ArmorPhysical` | 4 | [116](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L116) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L120) |
| `AttackDamageMin` | 30 | [121](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L121) |
| `AttackDamageMax` | 35 | [122](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L122) |
| `AttackRate` | 1.1 | [123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L123) |
| `AttackAnimationPoint` | 0.45 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L124) |
| `AttackAcquisitionRange` | 400 | [125](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L125) |
| `AttackRange` | 300 | [126](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L126) |
| `ProjectileSpeed` | 1300 | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L128) |
| `BountyXP` | 0 | [132](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L132) |
| `BountyGoldMin` | 0 | [133](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L133) |
| `BountyGoldMax` | 0 | [134](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L134) |
| `RingRadius` | 70 | [138](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L138) |
| `HealthBarOffset` | 190 | [139](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L139) |
| `MovementSpeed` | 220 | [144](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L144) |
| `MovementTurnRate` | 0.6 | [145](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L145) |
| `StatusHealth` | 550 | [149](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L149) |
| `StatusHealthRegen` | 0.5 | [150](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L150) |
| `StatusMana` | 0 | [151](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L151) |
| `StatusManaRegen` | 0.0 | [152](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L152) |
| `VisionDaytimeRange` | 900 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L156) |
| `VisionNighttimeRange` | 800 | [157](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L157) |
| `AttackType` | magic | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L165) |
| `ArmorType` | heavy | [166](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L166) |

## Дух Воды - `npc_water_elemental_3`

Источник: [hermit_water_elementals.txt:180](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L180). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [185](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L185) |
| `Level` | 3 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L189) |
| `ModelScale` | 0.8 | [190](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L190) |
| `CanBeDominated` | 0 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L191) |
| `ArmorPhysical` | 6 | [204](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L204) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [208](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L208) |
| `AttackDamageMin` | 60 | [209](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L209) |
| `AttackDamageMax` | 65 | [210](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L210) |
| `AttackRate` | 0.9 | [211](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L211) |
| `AttackAnimationPoint` | 0.45 | [212](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L212) |
| `AttackAcquisitionRange` | 400 | [213](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L213) |
| `AttackRange` | 300 | [214](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L214) |
| `ProjectileSpeed` | 1300 | [216](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L216) |
| `BountyXP` | 0 | [220](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L220) |
| `BountyGoldMin` | 0 | [221](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L221) |
| `BountyGoldMax` | 0 | [222](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L222) |
| `RingRadius` | 70 | [226](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L226) |
| `HealthBarOffset` | 190 | [227](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L227) |
| `MovementSpeed` | 220 | [232](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L232) |
| `MovementTurnRate` | 0.6 | [233](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L233) |
| `StatusHealth` | 800 | [237](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L237) |
| `StatusHealthRegen` | 1.0 | [238](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L238) |
| `StatusMana` | 0 | [239](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L239) |
| `StatusManaRegen` | 0.0 | [240](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L240) |
| `VisionDaytimeRange` | 900 | [244](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L244) |
| `VisionNighttimeRange` | 800 | [245](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L245) |
| `AttackType` | magic | [253](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L253) |
| `ArmorType` | heavy | [254](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L254) |

## Дух Воды - `npc_water_elemental_4`

Источник: [hermit_water_elementals.txt:268](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L268). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [273](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L273) |
| `Level` | 4 | [277](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L277) |
| `ModelScale` | 1.0 | [278](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L278) |
| `CanBeDominated` | 0 | [279](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L279) |
| `ArmorPhysical` | 10 | [292](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L292) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [296](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L296) |
| `AttackDamageMin` | 120 | [297](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L297) |
| `AttackDamageMax` | 140 | [298](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L298) |
| `AttackRate` | 0.75 | [299](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L299) |
| `AttackAnimationPoint` | 0.45 | [300](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L300) |
| `AttackAcquisitionRange` | 400 | [301](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L301) |
| `AttackRange` | 300 | [302](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L302) |
| `ProjectileSpeed` | 1300 | [304](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L304) |
| `BountyXP` | 0 | [308](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L308) |
| `BountyGoldMin` | 0 | [309](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L309) |
| `BountyGoldMax` | 0 | [310](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L310) |
| `RingRadius` | 70 | [314](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L314) |
| `HealthBarOffset` | 190 | [315](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L315) |
| `MovementSpeed` | 300 | [320](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L320) |
| `MovementTurnRate` | 0.6 | [321](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L321) |
| `StatusHealth` | 1450 | [325](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L325) |
| `StatusHealthRegen` | 2.0 | [326](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L326) |
| `StatusMana` | 0 | [327](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L327) |
| `StatusManaRegen` | 0.0 | [328](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L328) |
| `VisionDaytimeRange` | 900 | [332](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L332) |
| `VisionNighttimeRange` | 800 | [333](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L333) |
| `AttackType` | magic | [341](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L341) |
| `ArmorType` | heavy | [342](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/hermit_water_elementals.txt#L342) |

Способности: Ability1: `water_elemental_4_orb`.

## Дух-Целитель - `item_lia_healing_ward_unit`

Источник: [item_lia_healing_ward_unit.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_ward_base | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L5) |
| `Level` | 1 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L8) |
| `ArmorPhysical` | 0 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L30) |
| `MagicalResistance` | 100 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L31) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L35) |
| `AttackDamageMin` | 0 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L36) |
| `AttackDamageMax` | 0 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L37) |
| `AttackRate` | 1 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L39) |
| `AttackAnimationPoint` | 0.5 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L40) |
| `AttackAcquisitionRange` | 800 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L41) |
| `AttackRange` | 500 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L42) |
| `ProjectileSpeed` | 900 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L44) |
| `BountyXP` | 0 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L48) |
| `BountyGoldMin` | 0 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L49) |
| `BountyGoldMax` | 0 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L50) |
| `MovementSpeed` | 0 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L59) |
| `MovementTurnRate` | 0 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L60) |
| `FollowRange` | 0 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L61) |
| `StatusHealth` | 1 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L65) |
| `StatusHealthRegen` | 0 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L66) |
| `StatusMana` | 0 | [67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L67) |
| `StatusManaRegen` | 0 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L68) |
| `AttackType` | normal | [76](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L76) |
| `ArmorType` | unarmored | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L77) |
| `VisionDaytimeRange` | 600 | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L81) |
| `VisionNighttimeRange` | 600 | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L82) |
| `HasInventory` | 0 | [84](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/item_lia_healing_ward_unit.txt#L84) |

Способности: Ability1: `healing_ward_ability`.

Lua: [item_lia_healing_ward_unit.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/units/item_lia_healing_ward_unit.lua#L1).

## Энт - `keeper_of_the_grove_treant_1`

Источник: [keeper_of_the_grove_units.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L7) |
| `ModelScale` | 0.8 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L9) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L10) |
| `ArmorPhysical` | 4 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L18) |
| `MagicalResistance` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L19) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L23) |
| `AttackDamageMin` | 60 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L24) |
| `AttackDamageMax` | 60 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L25) |
| `AttackRate` | 2.25 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L27) |
| `AttackAnimationPoint` | 0.467 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L28) |
| `AttackAcquisitionRange` | 500 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L29) |
| `AttackRange` | 120 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L30) |
| `BountyGoldMin` | 0 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L34) |
| `BountyGoldMax` | 0 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L35) |
| `MovementSpeed` | 170 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L40) |
| `MovementTurnRate` | 0.5 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L41) |
| `StatusHealth` | 350 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L45) |
| `StatusHealthRegen` | 1 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L46) |
| `StatusMana` | 0 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L47) |
| `StatsManaRegen` | 0 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L48) |
| `VisionDaytimeRange` | 900 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L52) |
| `VisionNighttimeRange` | 800 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L53) |
| `AttackType` | normal | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L61) |
| `ArmorType` | heavy | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L62) |

## Энт - `keeper_of_the_grove_treant_2`

Источник: [keeper_of_the_grove_units.txt:65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L65). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [69](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L69) |
| `ModelScale` | 0.9 | [71](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L71) |
| `Level` | 1 | [72](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L72) |
| `ArmorPhysical` | 8 | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L80) |
| `MagicalResistance` | 0 | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L81) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [85](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L85) |
| `AttackDamageMin` | 125 | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L86) |
| `AttackDamageMax` | 125 | [87](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L87) |
| `AttackRate` | 2.2 | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L89) |
| `AttackAnimationPoint` | 0.467 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L90) |
| `AttackAcquisitionRange` | 500 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L91) |
| `AttackRange` | 120 | [92](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L92) |
| `BountyGoldMin` | 0 | [96](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L96) |
| `BountyGoldMax` | 0 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L97) |
| `MovementSpeed` | 160 | [102](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L102) |
| `MovementTurnRate` | 0.5 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L103) |
| `StatusHealth` | 700 | [107](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L107) |
| `StatusHealthRegen` | 3 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L108) |
| `StatusMana` | 0 | [109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L109) |
| `StatsManaRegen` | 0 | [110](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L110) |
| `VisionDaytimeRange` | 900 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L114) |
| `VisionNighttimeRange` | 800 | [115](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L115) |
| `AttackType` | normal | [123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L123) |
| `ArmorType` | heavy | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L124) |

Способности: Ability1: `keeper_of_the_grove_treants_unity`; Ability2: `keeper_of_the_grove_treants_cyclone`.

## Энт - `keeper_of_the_grove_treant_3`

Источник: [keeper_of_the_grove_units.txt:127](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L127). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [131](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L131) |
| `ModelScale` | 1 | [133](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L133) |
| `Level` | 1 | [134](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L134) |
| `ArmorPhysical` | 16 | [143](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L143) |
| `MagicalResistance` | 0 | [144](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L144) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [148](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L148) |
| `AttackDamageMin` | 250 | [149](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L149) |
| `AttackDamageMax` | 250 | [150](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L150) |
| `AttackRate` | 2.15 | [152](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L152) |
| `AttackAnimationPoint` | 0.467 | [153](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L153) |
| `AttackAcquisitionRange` | 500 | [154](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L154) |
| `AttackRange` | 120 | [155](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L155) |
| `BountyGoldMin` | 0 | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L159) |
| `BountyGoldMax` | 0 | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L160) |
| `MovementSpeed` | 150 | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L165) |
| `MovementTurnRate` | 0.5 | [166](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L166) |
| `StatusHealth` | 1400 | [170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L170) |
| `StatusHealthRegen` | 15 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L171) |
| `StatusMana` | 0 | [172](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L172) |
| `StatsManaRegen` | 0 | [173](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L173) |
| `VisionDaytimeRange` | 900 | [177](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L177) |
| `VisionNighttimeRange` | 800 | [178](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L178) |
| `AttackType` | normal | [186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L186) |
| `ArmorType` | heavy | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L187) |

Способности: Ability1: `keeper_of_the_grove_treants_unity`; Ability2: `keeper_of_the_grove_treants_cyclone`; Ability3: `keeper_of_the_grove_treants_reborn`.

## Молодой Энт - `keeper_of_the_grove_young_treant`

Источник: [keeper_of_the_grove_units.txt:190](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L190). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [194](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L194) |
| `ModelScale` | 0.6 | [196](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L196) |
| `Level` | 0 | [197](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L197) |
| `ArmorPhysical` | 0 | [205](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L205) |
| `MagicalResistance` | 0 | [206](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L206) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [210](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L210) |
| `AttackDamageMin` | 80 | [211](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L211) |
| `AttackDamageMax` | 80 | [212](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L212) |
| `AttackRate` | 2 | [214](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L214) |
| `AttackAnimationPoint` | 0.467 | [215](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L215) |
| `AttackAcquisitionRange` | 500 | [216](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L216) |
| `AttackRange` | 100 | [217](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L217) |
| `BountyGoldMin` | 0 | [221](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L221) |
| `BountyGoldMax` | 0 | [222](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L222) |
| `MovementSpeed` | 220 | [227](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L227) |
| `MovementTurnRate` | 0.5 | [228](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L228) |
| `StatusHealth` | 550 | [232](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L232) |
| `StatusHealthRegen` | 5 | [233](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L233) |
| `StatusMana` | 0 | [234](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L234) |
| `StatsManaRegen` | 0 | [235](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L235) |
| `VisionDaytimeRange` | 900 | [239](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L239) |
| `VisionNighttimeRange` | 800 | [240](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L240) |
| `AttackType` | normal | [248](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L248) |
| `ArmorType` | heavy | [249](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L249) |

## Дух-Защитник - `keeper_of_the_grove_guardian_spirit`

Источник: [keeper_of_the_grove_units.txt:252](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L252). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [256](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L256) |
| `ModelScale` | 1 | [258](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L258) |
| `Level` | 0 | [259](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L259) |
| `ArmorPhysical` | 0 | [267](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L267) |
| `MagicalResistance` | 25 | [268](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L268) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [272](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L272) |
| `AttackDamageMin` | 6 | [273](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L273) |
| `AttackDamageMax` | 12 | [274](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L274) |
| `AttackRate` | 1.85 | [276](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L276) |
| `AttackAnimationPoint` | 0.4 | [277](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L277) |
| `AttackAcquisitionRange` | 800 | [278](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L278) |
| `AttackRange` | 610 | [279](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L279) |
| `BountyGoldMin` | 0 | [283](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L283) |
| `BountyGoldMax` | 0 | [284](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L284) |
| `MovementSpeed` | 265 | [289](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L289) |
| `MovementTurnRate` | 0.4 | [290](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L290) |
| `StatusHealth` | 525 | [292](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L292) |
| `StatusHealthRegen` | 1.15 | [293](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L293) |
| `StatusMana` | 75 | [294](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L294) |
| `StatusManaRegen` | 0.01 | [295](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L295) |
| `VisionDaytimeRange` | 900 | [299](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L299) |
| `VisionNighttimeRange` | 800 | [300](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L300) |
| `AttackType` | hero | [308](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L308) |
| `ArmorType` | hero | [309](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L309) |
| `ConsideredHero` | 1 | [311](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/keeper_of_the_grove_units.txt#L311) |

Способности: Ability1: `keeper_of_the_grove_guardian_spirit_narure_forces`.

## Мертвец - `necromancer_skeleton1`

Источник: [necromancer_creeps.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L5) |
| `ModelScale` | 0.8 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L9) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L10) |
| `ArmorPhysical` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L21) |
| `MagicalResistance` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L22) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L24) |
| `AttackDamageMin` | 15 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L25) |
| `AttackDamageMax` | 16 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L26) |
| `AttackRate` | 1.25 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L27) |
| `AttackAnimationPoint` | 0.56 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L28) |
| `AttackAcquisitionRange` | 500 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L29) |
| `AttackRange` | 100 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L30) |
| `BountyXP` | 0 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L32) |
| `BountyGoldMin` | 0 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L33) |
| `BountyGoldMax` | 0 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L34) |
| `MovementSpeed` | 270 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L37) |
| `MovementTurnRate` | 0.5 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L38) |
| `StatusHealth` | 275 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L40) |
| `StatusHealthRegen` | 0.5 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L41) |
| `StatusMana` | 0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L42) |
| `StatusManaRegen` | 0 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L43) |
| `AttackType` | normal | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L49) |
| `ArmorType` | heavy | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L50) |
| `VisionDaytimeRange` | 900 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L52) |
| `VisionNighttimeRange` | 800 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L53) |
| `HasInventory` | 0 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L55) |

## Мертвец - `necromancer_skeleton2`

Источник: [necromancer_creeps.txt:59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L59). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L61) |
| `ModelScale` | 0.9 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L65) |
| `Level` | 1 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L66) |
| `ArmorPhysical` | 2 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L77) |
| `MagicalResistance` | 0 | [78](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L78) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L80) |
| `AttackDamageMin` | 25 | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L81) |
| `AttackDamageMax` | 28 | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L82) |
| `AttackRate` | 1.25 | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L83) |
| `AttackAnimationPoint` | 0.56 | [84](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L84) |
| `AttackAcquisitionRange` | 500 | [85](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L85) |
| `AttackRange` | 100 | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L86) |
| `BountyXP` | 0 | [88](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L88) |
| `BountyGoldMin` | 0 | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L89) |
| `BountyGoldMax` | 0 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L90) |
| `MovementSpeed` | 270 | [93](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L93) |
| `MovementTurnRate` | 0.5 | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L94) |
| `StatusHealth` | 650 | [96](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L96) |
| `StatusHealthRegen` | 0.5 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L97) |
| `StatusMana` | 0 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L98) |
| `StatusManaRegen` | 0 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L99) |
| `AttackType` | normal | [105](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L105) |
| `ArmorType` | heavy | [106](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L106) |
| `VisionDaytimeRange` | 900 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L108) |
| `VisionNighttimeRange` | 800 | [109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L109) |
| `HasInventory` | 0 | [111](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L111) |

## Мертвец - `necromancer_skeleton3`

Источник: [necromancer_creeps.txt:115](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L115). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [117](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L117) |
| `ModelScale` | 1.0 | [121](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L121) |
| `Level` | 1 | [122](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L122) |
| `ArmorPhysical` | 3 | [133](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L133) |
| `MagicalResistance` | 0 | [134](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L134) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [136](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L136) |
| `AttackDamageMin` | 50 | [137](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L137) |
| `AttackDamageMax` | 55 | [138](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L138) |
| `AttackRate` | 1.25 | [139](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L139) |
| `AttackAnimationPoint` | 0.4 | [140](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L140) |
| `AttackAcquisitionRange` | 500 | [141](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L141) |
| `AttackRange` | 100 | [142](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L142) |
| `BountyXP` | 0 | [144](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L144) |
| `BountyGoldMin` | 0 | [145](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L145) |
| `BountyGoldMax` | 0 | [146](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L146) |
| `MovementSpeed` | 270 | [149](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L149) |
| `MovementTurnRate` | 0.5 | [150](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L150) |
| `StatusHealth` | 1250 | [152](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L152) |
| `StatusHealthRegen` | 0.5 | [153](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L153) |
| `StatusMana` | 0 | [154](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L154) |
| `StatusManaRegen` | 0 | [155](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L155) |
| `AttackType` | normal | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L161) |
| `ArmorType` | heavy | [162](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L162) |
| `VisionDaytimeRange` | 900 | [164](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L164) |
| `VisionNighttimeRange` | 800 | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L165) |
| `HasInventory` | 0 | [167](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L167) |

## Мертвец - `necromancer_skeleton4`

Источник: [necromancer_creeps.txt:170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L170). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [172](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L172) |
| `ModelScale` | 1.1 | [177](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L177) |
| `Level` | 1 | [178](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L178) |
| `ArmorPhysical` | 9 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L189) |
| `MagicalResistance` | 0 | [190](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L190) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [192](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L192) |
| `AttackDamageMin` | 100 | [193](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L193) |
| `AttackDamageMax` | 107 | [194](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L194) |
| `AttackRate` | 1.05 | [195](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L195) |
| `AttackAnimationPoint` | 0.4 | [196](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L196) |
| `AttackAcquisitionRange` | 500 | [197](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L197) |
| `AttackRange` | 100 | [198](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L198) |
| `BountyXP` | 0 | [200](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L200) |
| `BountyGoldMin` | 0 | [201](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L201) |
| `BountyGoldMax` | 0 | [202](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L202) |
| `MovementSpeed` | 270 | [205](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L205) |
| `MovementTurnRate` | 0.5 | [206](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L206) |
| `StatusHealth` | 1500 | [208](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L208) |
| `StatusHealthRegen` | 0.5 | [209](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L209) |
| `StatusMana` | 0 | [210](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L210) |
| `StatusManaRegen` | 0 | [211](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L211) |
| `AttackType` | normal | [217](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L217) |
| `ArmorType` | heavy | [218](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L218) |
| `VisionDaytimeRange` | 900 | [220](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L220) |
| `VisionNighttimeRange` | 800 | [221](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L221) |
| `HasInventory` | 0 | [223](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/necromancer_creeps.txt#L223) |

Способности: Ability1: `necromancer_skel_return`.

## Босс - `1_wave_boss`

Источник: [normal_mode_bosses.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L5) |
| `ModelScale` | 1.3 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L8) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L10) |
| `ArmorPhysical` | 2 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L26) |
| `MagicalResistance` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L27) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L29) |
| `AttackDamageMin` | 31 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L30) |
| `AttackDamageMax` | 32 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L31) |
| `AttackRate` | 1.25 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L32) |
| `AttackAnimationPoint` | 0.3 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L33) |
| `AttackAcquisitionRange` | 900 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L34) |
| `AttackRange` | 100 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L35) |
| `BountyXP` | 25 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L39) |
| `BountyGoldMin` | 35 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L40) |
| `BountyGoldMax` | 35 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L41) |
| `MovementSpeed` | 300 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L44) |
| `MovementTurnRate` | 0.5 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L45) |
| `StatusHealth` | 250 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L47) |
| `StatusHealthRegen` | 0.5 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L48) |
| `StatusMana` | 150 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L49) |
| `StatusManaRegen` | 1 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L50) |
| `AttackType` | normal | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L56) |
| `ArmorType` | heavy | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L57) |
| `VisionDaytimeRange` | 1400 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L59) |
| `VisionNighttimeRange` | 800 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L60) |
| `HasInventory` | 0 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L62) |
| `PathfindingSearchDepthScale` | 0.75 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L64) |

Способности: Ability1: `1_wave_stomp`; Ability2: `wave_1_poison`.

Lua: [1_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/1_wave_bosses.lua#L1).

## Босс - `2_wave_boss`

Источник: [normal_mode_bosses.txt:68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L68). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L70) |
| `ModelScale` | 1 | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L73) |
| `Level` | 1 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L74) |
| `ArmorPhysical` | 4 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L90) |
| `MagicalResistance` | 0 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L91) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [93](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L93) |
| `AttackDamageMin` | 61 | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L94) |
| `AttackDamageMax` | 62 | [95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L95) |
| `AttackRate` | 1.25 | [96](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L96) |
| `AttackAnimationPoint` | 0.3 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L97) |
| `AttackAcquisitionRange` | 900 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L98) |
| `AttackRange` | 100 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L99) |
| `BountyXP` | 25 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L103) |
| `BountyGoldMin` | 35 | [104](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L104) |
| `BountyGoldMax` | 35 | [105](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L105) |
| `MovementSpeed` | 350 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L108) |
| `MovementTurnRate` | 0.5 | [109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L109) |
| `StatusHealth` | 300 | [111](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L111) |
| `StatusHealthRegen` | 0.5 | [112](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L112) |
| `StatusMana` | 200 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L113) |
| `StatusManaRegen` | 1 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L114) |
| `AttackType` | normal | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L120) |
| `ArmorType` | heavy | [121](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L121) |
| `VisionDaytimeRange` | 1400 | [123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L123) |
| `VisionNighttimeRange` | 800 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L124) |
| `HasInventory` | 0 | [126](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L126) |
| `PathfindingSearchDepthScale` | 0.75 | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L128) |

Способности: Ability1: `second_wave_wave_of_force`; Ability2: `wave_2_aura_of_vengeance`; Ability3: `wave_2_centaurs_revenge`.

Lua: [2_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/2_wave_bosses.lua#L1).

## Босс - `3_wave_boss`

Источник: [normal_mode_bosses.txt:132](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L132). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [134](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L134) |
| `ModelScale` | 1 | [137](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L137) |
| `Level` | 1 | [138](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L138) |
| `ArmorPhysical` | 6 | [154](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L154) |
| `MagicalResistance` | 0 | [155](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L155) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L156) |
| `AttackDamageMin` | 101 | [157](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L157) |
| `AttackDamageMax` | 107 | [158](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L158) |
| `AttackRate` | 1.3 | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L159) |
| `AttackAnimationPoint` | 0.3 | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L160) |
| `AttackAcquisitionRange` | 900 | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L161) |
| `AttackRange` | 100 | [162](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L162) |
| `BountyXP` | 25 | [166](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L166) |
| `BountyGoldMin` | 35 | [167](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L167) |
| `BountyGoldMax` | 35 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L168) |
| `MovementSpeed` | 270 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L171) |
| `MovementTurnRate` | 0.6 | [172](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L172) |
| `StatusHealth` | 500 | [174](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L174) |
| `StatusHealthRegen` | 0.5 | [175](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L175) |
| `StatusMana` | 400 | [176](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L176) |
| `StatusManaRegen` | 1 | [177](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L177) |
| `AttackType` | normal | [183](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L183) |
| `ArmorType` | medium | [184](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L184) |
| `VisionDaytimeRange` | 1400 | [186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L186) |
| `VisionNighttimeRange` | 800 | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L187) |
| `HasInventory` | 0 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L189) |
| `PathfindingSearchDepthScale` | 0.75 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L191) |

Способности: Ability1: `wave_3_evasion`; Ability2: `3_wave_rejuvenation`.

Lua: [3_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/3_wave_bosses.lua#L1).

## Босс - `4_wave_boss`

Источник: [normal_mode_bosses.txt:195](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L195). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [197](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L197) |
| `ModelScale` | 0.9 | [200](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L200) |
| `Level` | 1 | [201](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L201) |
| `ArmorPhysical` | 7 | [217](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L217) |
| `MagicalResistance` | 0 | [218](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L218) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [220](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L220) |
| `AttackDamageMin` | 81 | [221](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L221) |
| `AttackDamageMax` | 84 | [222](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L222) |
| `AttackRate` | 0.9 | [223](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L223) |
| `AttackAnimationPoint` | 0.3 | [224](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L224) |
| `AttackAcquisitionRange` | 900 | [225](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L225) |
| `AttackRange` | 800 | [226](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L226) |
| `ProjectileSpeed` | 1200 | [228](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L228) |
| `BountyXP` | 25 | [230](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L230) |
| `BountyGoldMin` | 40 | [231](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L231) |
| `BountyGoldMax` | 40 | [232](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L232) |
| `MovementSpeed` | 270 | [235](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L235) |
| `MovementTurnRate` | 0.5 | [236](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L236) |
| `StatusHealth` | 650 | [238](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L238) |
| `StatusHealthRegen` | 0.5 | [239](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L239) |
| `StatusMana` | 300 | [240](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L240) |
| `StatusManaRegen` | 1 | [241](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L241) |
| `AttackType` | pierce | [247](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L247) |
| `ArmorType` | heavy | [248](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L248) |
| `VisionDaytimeRange` | 1400 | [250](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L250) |
| `VisionNighttimeRange` | 800 | [251](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L251) |
| `HasInventory` | 0 | [253](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L253) |
| `PathfindingSearchDepthScale` | 0.75 | [255](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L255) |

Способности: Ability1: `4_wave_death_coil`.

Lua: [4_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/4_wave_bosses.lua#L1).

## Босс - `6_wave_boss`

Источник: [normal_mode_bosses.txt:259](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L259). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [261](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L261) |
| `ModelScale` | 1.2 | [264](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L264) |
| `Level` | 1 | [265](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L265) |
| `ArmorPhysical` | 6 | [276](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L276) |
| `MagicalResistance` | 0 | [277](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L277) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [279](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L279) |
| `AttackDamageMin` | 151 | [280](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L280) |
| `AttackDamageMax` | 153 | [281](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L281) |
| `AttackRate` | 1.05 | [282](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L282) |
| `AttackAnimationPoint` | 0.3 | [283](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L283) |
| `AttackAcquisitionRange` | 900 | [284](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L284) |
| `AttackRange` | 100 | [285](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L285) |
| `BountyXP` | 33 | [289](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L289) |
| `BountyGoldMin` | 40 | [290](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L290) |
| `BountyGoldMax` | 40 | [291](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L291) |
| `MovementSpeed` | 400 | [294](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L294) |
| `MovementTurnRate` | 0.6 | [295](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L295) |
| `StatusHealth` | 1250 | [297](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L297) |
| `StatusHealthRegen` | 0.5 | [298](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L298) |
| `StatusMana` | 500 | [299](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L299) |
| `StatusManaRegen` | 0.75 | [300](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L300) |
| `StatusStartingMana` | 350 | [301](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L301) |
| `AttackType` | normal | [307](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L307) |
| `ArmorType` | heavy | [308](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L308) |
| `VisionDaytimeRange` | 1400 | [310](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L310) |
| `VisionNighttimeRange` | 800 | [311](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L311) |
| `PathfindingSearchDepthScale` | 0.75 | [313](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L313) |
| `HasInventory` | 0 | [316](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L316) |

Способности: Ability1: `6_wave_cripple`; Ability2: `wave_6_invisibility`.

Lua: [6_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/6_wave_bosses.lua#L1).

## Босс - `7_wave_boss`

Источник: [normal_mode_bosses.txt:319](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L319). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [321](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L321) |
| `ModelScale` | 1.5 | [324](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L324) |
| `Level` | 1 | [325](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L325) |
| `ArmorPhysical` | 6 | [341](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L341) |
| `MagicalResistance` | 0 | [342](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L342) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [344](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L344) |
| `AttackDamageMin` | 221 | [345](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L345) |
| `AttackDamageMax` | 225 | [346](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L346) |
| `AttackRate` | 0.55 | [347](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L347) |
| `AttackAnimationPoint` | 0.3 | [348](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L348) |
| `AttackAcquisitionRange` | 900 | [349](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L349) |
| `AttackRange` | 100 | [350](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L350) |
| `BountyXP` | 33 | [354](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L354) |
| `BountyGoldMin` | 40 | [355](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L355) |
| `BountyGoldMax` | 40 | [356](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L356) |
| `MovementSpeed` | 500 | [359](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L359) |
| `MovementTurnRate` | 0.5 | [360](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L360) |
| `StatusHealth` | 1600 | [362](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L362) |
| `StatusHealthRegen` | 0.5 | [363](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L363) |
| `StatusMana` | 450 | [364](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L364) |
| `StatusManaRegen` | 1 | [365](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L365) |
| `StatusStartingMana` | 400 | [366](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L366) |
| `AttackType` | normal | [372](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L372) |
| `ArmorType` | heavy | [373](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L373) |
| `VisionDaytimeRange` | 1400 | [375](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L375) |
| `VisionNighttimeRange` | 800 | [376](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L376) |
| `PathfindingSearchDepthScale` | 0.75 | [378](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L378) |
| `HasInventory` | 0 | [381](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L381) |

Способности: Ability1: `7_wave_howl_of_terror`; Ability2: `7_wave_plague`.

Lua: [7_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/7_wave_bosses.lua#L1).

## Босс - `8_wave_boss`

Источник: [normal_mode_bosses.txt:384](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L384). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [386](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L386) |
| `ModelScale` | 1.5 | [389](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L389) |
| `Level` | 1 | [390](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L390) |
| `ArmorPhysical` | 8 | [406](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L406) |
| `MagicalResistance` | 0 | [407](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L407) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [409](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L409) |
| `AttackDamageMin` | 251 | [410](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L410) |
| `AttackDamageMax` | 254 | [411](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L411) |
| `AttackRate` | 1.25 | [412](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L412) |
| `AttackAnimationPoint` | 0.3 | [413](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L413) |
| `AttackAcquisitionRange` | 900 | [414](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L414) |
| `AttackRange` | 100 | [415](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L415) |
| `BountyXP` | 33 | [419](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L419) |
| `BountyGoldMin` | 50 | [420](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L420) |
| `BountyGoldMax` | 50 | [421](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L421) |
| `MovementSpeed` | 270 | [424](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L424) |
| `MovementTurnRate` | 0.6 | [425](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L425) |
| `StatusHealth` | 1900 | [427](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L427) |
| `StatusHealthRegen` | 3.0 | [428](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L428) |
| `StatusMana` | 300 | [429](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L429) |
| `StatusManaRegen` | 0.75 | [430](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L430) |
| `AttackType` | normal | [436](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L436) |
| `ArmorType` | heavy | [437](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L437) |
| `VisionDaytimeRange` | 1400 | [439](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L439) |
| `VisionNighttimeRange` | 800 | [440](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L440) |
| `PathfindingSearchDepthScale` | 0.75 | [442](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L442) |
| `HasInventory` | 0 | [445](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L445) |

Способности: Ability1: `wave_8_cleave`; Ability2: `custom_spell_immunity`; Ability3: `wave_8_lifesteal_aura`; Ability4: `8_wave_storm_bolt`.

Lua: [8_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/8_wave_bosses.lua#L1).

## Босс - `9_wave_boss`

Источник: [normal_mode_bosses.txt:448](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L448). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [450](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L450) |
| `ModelScale` | 1 | [453](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L453) |
| `Level` | 1 | [454](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L454) |
| `ProjectileSpeed` | 1300 | [456](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L456) |
| `ArmorPhysical` | 4 | [489](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L489) |
| `MagicalResistance` | 0 | [490](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L490) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [492](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L492) |
| `AttackDamageMin` | 281 | [493](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L493) |
| `AttackDamageMax` | 290 | [494](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L494) |
| `AttackRate` | 0.7 | [495](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L495) |
| `AttackAnimationPoint` | 0.3 | [496](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L496) |
| `AttackAcquisitionRange` | 900 | [497](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L497) |
| `AttackRange` | 600 | [498](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L498) |
| `BountyXP` | 33 | [500](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L500) |
| `BountyGoldMin` | 45 | [501](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L501) |
| `BountyGoldMax` | 45 | [502](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L502) |
| `MovementSpeed` | 220 | [505](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L505) |
| `MovementTurnRate` | 0.5 | [506](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L506) |
| `StatusHealth` | 2400 | [508](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L508) |
| `StatusHealthRegen` | 0.5 | [509](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L509) |
| `StatusMana` | 400 | [510](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L510) |
| `StatusManaRegen` | 1 | [511](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L511) |
| `AttackType` | pierce | [517](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L517) |
| `ArmorType` | medium | [518](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L518) |
| `VisionDaytimeRange` | 1400 | [520](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L520) |
| `VisionNighttimeRange` | 800 | [521](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L521) |
| `PathfindingSearchDepthScale` | 0.75 | [523](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L523) |
| `HasInventory` | 0 | [526](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L526) |

Способности: Ability1: `9_wave_frost_nova`; Ability2: `wave_9_morphallaxis`.

Lua: [9_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/9_wave_bosses.lua#L1).

## Босс - `11_wave_boss`

Источник: [normal_mode_bosses.txt:529](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L529). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [531](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L531) |
| `ModelScale` | 1.2 | [534](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L534) |
| `Level` | 1 | [535](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L535) |
| `ArmorPhysical` | 16 | [551](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L551) |
| `MagicalResistance` | 0 | [552](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L552) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [554](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L554) |
| `AttackDamageMin` | 361 | [555](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L555) |
| `AttackDamageMax` | 366 | [556](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L556) |
| `AttackRate` | 0.25 | [557](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L557) |
| `AttackAnimationPoint` | 0.3 | [558](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L558) |
| `AttackAcquisitionRange` | 900 | [559](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L559) |
| `AttackRange` | 100 | [560](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L560) |
| `BountyXP` | 48 | [564](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L564) |
| `BountyGoldMin` | 50 | [565](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L565) |
| `BountyGoldMax` | 50 | [566](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L566) |
| `MovementSpeed` | 320 | [569](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L569) |
| `MovementTurnRate` | 0.5 | [570](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L570) |
| `StatusHealth` | 4000 | [572](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L572) |
| `StatusHealthRegen` | 0.5 | [573](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L573) |
| `StatusMana` | 0 | [574](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L574) |
| `StatusManaRegen` | 0.75 | [575](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L575) |
| `AttackType` | chaos | [581](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L581) |
| `ArmorType` | medium | [582](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L582) |
| `VisionDaytimeRange` | 1400 | [584](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L584) |
| `VisionNighttimeRange` | 800 | [585](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L585) |
| `PathfindingSearchDepthScale` | 1 | [587](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L587) |
| `HasInventory` | 0 | [590](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L590) |

Способности: Ability1: `wave_11_mana_break`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Босс - `12_wave_boss`

Источник: [normal_mode_bosses.txt:593](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L593). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [595](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L595) |
| `ModelScale` | 1 | [598](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L598) |
| `Level` | 1 | [599](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L599) |
| `ArmorPhysical` | 15 | [615](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L615) |
| `MagicalResistance` | 0 | [616](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L616) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [618](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L618) |
| `AttackDamageMin` | 481 | [619](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L619) |
| `AttackDamageMax` | 484 | [620](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L620) |
| `AttackRate` | 0.85 | [621](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L621) |
| `AttackAnimationPoint` | 0.3 | [622](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L622) |
| `AttackAcquisitionRange` | 900 | [623](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L623) |
| `AttackRange` | 100 | [624](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L624) |
| `BountyXP` | 48 | [628](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L628) |
| `BountyGoldMin` | 50 | [629](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L629) |
| `BountyGoldMax` | 50 | [630](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L630) |
| `MovementSpeed` | 300 | [633](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L633) |
| `MovementTurnRate` | 0.5 | [634](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L634) |
| `StatusHealth` | 4200 | [636](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L636) |
| `StatusHealthRegen` | 0.5 | [637](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L637) |
| `StatusMana` | 600 | [638](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L638) |
| `StatusManaRegen` | 0 | [639](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L639) |
| `AttackType` | normal | [645](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L645) |
| `ArmorType` | heavy | [646](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L646) |
| `VisionDaytimeRange` | 1400 | [648](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L648) |
| `VisionNighttimeRange` | 800 | [649](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L649) |
| `PathfindingSearchDepthScale` | 0.75 | [651](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L651) |
| `HasInventory` | 0 | [654](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L654) |

Способности: Ability1: `wave_12_bash`; Ability2: `12_wave_bloodlust`; Ability3: `12_wave_roots`.

Lua: [12_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/12_wave_bosses.lua#L1).

## Босс - `13_wave_boss`

Источник: [normal_mode_bosses.txt:657](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L657). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [659](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L659) |
| `ModelScale` | 1 | [663](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L663) |
| `Level` | 1 | [664](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L664) |
| `ArmorPhysical` | 20 | [680](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L680) |
| `MagicalResistance` | 0 | [681](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L681) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [683](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L683) |
| `AttackDamageMin` | 601 | [684](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L684) |
| `AttackDamageMax` | 605 | [685](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L685) |
| `AttackRate` | 0.65 | [686](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L686) |
| `AttackAnimationPoint` | 0.3 | [687](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L687) |
| `AttackAcquisitionRange` | 900 | [688](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L688) |
| `AttackRange` | 100 | [689](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L689) |
| `BountyXP` | 48 | [693](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L693) |
| `BountyGoldMin` | 50 | [694](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L694) |
| `BountyGoldMax` | 50 | [695](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L695) |
| `MovementSpeed` | 270 | [698](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L698) |
| `MovementTurnRate` | 0.5 | [699](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L699) |
| `StatusHealth` | 4500 | [701](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L701) |
| `StatusHealthRegen` | 0.5 | [702](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L702) |
| `StatusMana` | 450 | [703](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L703) |
| `StatusManaRegen` | 1 | [704](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L704) |
| `AttackType` | normal | [710](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L710) |
| `ArmorType` | heavy | [711](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L711) |
| `VisionDaytimeRange` | 1400 | [713](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L713) |
| `VisionNighttimeRange` | 800 | [714](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L714) |
| `PathfindingSearchDepthScale` | 0.75 | [716](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L716) |
| `HasInventory` | 0 | [719](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L719) |

Способности: Ability1: `custom_spell_immunity`; Ability2: `wave_13_command_aura`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Босс - `14_wave_boss`

Источник: [normal_mode_bosses.txt:722](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L722). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [724](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L724) |
| `ModelScale` | 1 | [727](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L727) |
| `Level` | 1 | [728](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L728) |
| `ProjectileSpeed` | 1500 | [730](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L730) |
| `ArmorPhysical` | 20 | [748](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L748) |
| `MagicalResistance` | 0 | [749](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L749) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [751](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L751) |
| `AttackDamageMin` | 501 | [752](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L752) |
| `AttackDamageMax` | 510 | [753](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L753) |
| `AttackRate` | 1 | [754](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L754) |
| `AttackAnimationPoint` | 0.3 | [755](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L755) |
| `AttackAcquisitionRange` | 900 | [756](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L756) |
| `AttackRange` | 500 | [757](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L757) |
| `SplashAttack` | 1 | [759](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L759) |
| `SplashFullDamageRadius` | 75 | [760](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L760) |
| `SplashMediumRadius` | 150 | [761](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L761) |
| `SplashMediumDamage` | 0.5 | [762](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L762) |
| `SplashSmallRadius` | 225 | [763](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L763) |
| `SplashSmallDamage` | 0.25 | [764](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L764) |
| `BountyXP` | 48 | [766](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L766) |
| `BountyGoldMin` | 60 | [767](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L767) |
| `BountyGoldMax` | 60 | [768](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L768) |
| `MovementSpeed` | 270 | [771](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L771) |
| `MovementTurnRate` | 0.5 | [772](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L772) |
| `StatusHealth` | 3500 | [774](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L774) |
| `StatusHealthRegen` | 1.5 | [775](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L775) |
| `StatusMana` | 800 | [776](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L776) |
| `StatusManaRegen` | 1.5 | [777](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L777) |
| `AttackType` | chaos | [783](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L783) |
| `ArmorType` | heavy | [784](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L784) |
| `VisionDaytimeRange` | 1400 | [786](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L786) |
| `VisionNighttimeRange` | 800 | [787](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L787) |
| `PathfindingSearchDepthScale` | 0.75 | [789](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L789) |
| `HasInventory` | 0 | [792](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L792) |

Способности: Ability1: `14_wave_storm_bolt`.

Lua: [14_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/14_wave_bosses.lua#L1).

## Босс - `16_wave_boss`

Источник: [normal_mode_bosses.txt:795](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L795). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [797](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L797) |
| `ModelScale` | 1.5 | [800](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L800) |
| `Level` | 1 | [801](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L801) |
| `ProjectileSpeed` | 900 | [803](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L803) |
| `ArmorPhysical` | 30 | [820](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L820) |
| `MagicalResistance` | 0 | [821](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L821) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [823](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L823) |
| `AttackDamageMin` | 751 | [824](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L824) |
| `AttackDamageMax` | 761 | [825](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L825) |
| `AttackRate` | 1 | [826](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L826) |
| `AttackAnimationPoint` | 0.3 | [827](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L827) |
| `AttackAcquisitionRange` | 900 | [828](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L828) |
| `AttackRange` | 400 | [829](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L829) |
| `BountyXP` | 63 | [832](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L832) |
| `BountyGoldMin` | 50 | [833](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L833) |
| `BountyGoldMax` | 50 | [834](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L834) |
| `MovementSpeed` | 270 | [837](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L837) |
| `MovementTurnRate` | 0.6 | [838](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L838) |
| `StatusHealth` | 4500 | [840](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L840) |
| `StatusHealthRegen` | 0.5 | [841](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L841) |
| `StatusMana` | 800 | [842](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L842) |
| `StatusManaRegen` | 1 | [843](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L843) |
| `AttackType` | magic | [849](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L849) |
| `ArmorType` | heavy | [850](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L850) |
| `VisionDaytimeRange` | 1400 | [852](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L852) |
| `VisionNighttimeRange` | 800 | [853](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L853) |
| `PathfindingSearchDepthScale` | 0.75 | [855](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L855) |
| `HasInventory` | 0 | [858](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L858) |

Способности: Ability1: `wave_16_mana_burn`; Ability2: `16_wave_slow`.

Lua: [16_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/16_wave_bosses.lua#L1).

## Босс - `17_wave_boss`

Источник: [normal_mode_bosses.txt:861](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L861). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [863](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L863) |
| `ModelScale` | 1.5 | [866](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L866) |
| `Level` | 1 | [867](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L867) |
| `ArmorPhysical` | 66 | [883](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L883) |
| `MagicalResistance` | 50 | [884](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L884) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [886](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L886) |
| `AttackDamageMin` | 901 | [887](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L887) |
| `AttackDamageMax` | 905 | [888](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L888) |
| `AttackRate` | 0.85 | [889](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L889) |
| `AttackAnimationPoint` | 0.3 | [890](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L890) |
| `AttackAcquisitionRange` | 900 | [891](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L891) |
| `AttackRange` | 110 | [892](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L892) |
| `BountyXP` | 63 | [896](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L896) |
| `BountyGoldMin` | 60 | [897](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L897) |
| `BountyGoldMax` | 60 | [898](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L898) |
| `MovementSpeed` | 350 | [901](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L901) |
| `MovementTurnRate` | 0.5 | [902](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L902) |
| `StatusHealth` | 5200 | [904](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L904) |
| `StatusHealthRegen` | 0.5 | [905](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L905) |
| `StatusMana` | 400 | [906](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L906) |
| `StatusManaRegen` | 1 | [907](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L907) |
| `StatusStartingMana` | 300 | [908](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L908) |
| `AttackType` | normal | [914](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L914) |
| `ArmorType` | heavy | [915](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L915) |
| `VisionDaytimeRange` | 1400 | [917](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L917) |
| `VisionNighttimeRange` | 800 | [918](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L918) |
| `PathfindingSearchDepthScale` | 0.75 | [920](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L920) |
| `HasInventory` | 0 | [923](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L923) |

Способности: Ability1: `wave_17_evasion`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Босс - `18_wave_boss`

Источник: [normal_mode_bosses.txt:926](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L926). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [928](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L928) |
| `ModelScale` | 1.1 | [931](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L931) |
| `Level` | 1 | [932](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L932) |
| `ArmorPhysical` | 30 | [960](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L960) |
| `MagicalResistance` | 50 | [961](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L961) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [963](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L963) |
| `AttackDamageMin` | 1401 | [964](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L964) |
| `AttackDamageMax` | 1405 | [965](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L965) |
| `AttackRate` | 1.5 | [966](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L966) |
| `AttackAnimationPoint` | 0.3 | [967](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L967) |
| `AttackAcquisitionRange` | 900 | [968](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L968) |
| `AttackRange` | 250 | [969](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L969) |
| `ProjectileSpeed` | 1900 | [971](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L971) |
| `BountyXP` | 63 | [973](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L973) |
| `BountyGoldMin` | 55 | [974](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L974) |
| `BountyGoldMax` | 55 | [975](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L975) |
| `MovementSpeed` | 350 | [978](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L978) |
| `MovementTurnRate` | 0.5 | [979](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L979) |
| `StatusHealth` | 5500 | [981](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L981) |
| `StatusHealthRegen` | 1 | [982](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L982) |
| `StatusMana` | 0 | [983](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L983) |
| `StatusManaRegen` | 0 | [984](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L984) |
| `AttackType` | pierce | [990](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L990) |
| `ArmorType` | medium | [991](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L991) |
| `VisionDaytimeRange` | 1400 | [993](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L993) |
| `VisionNighttimeRange` | 800 | [994](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L994) |
| `PathfindingSearchDepthScale` | 0.75 | [996](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L996) |
| `HasInventory` | 0 | [999](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L999) |

Способности: Ability1: `wave_18_bash`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Босс - `19_wave_boss`

Источник: [normal_mode_bosses.txt:1002](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1002). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_boss`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [1004](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1004) |
| `ModelScale` | 1 | [1007](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1007) |
| `Level` | 1 | [1008](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1008) |
| `ArmorPhysical` | 35 | [1024](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1024) |
| `MagicalResistance` | 0 | [1025](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1025) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [1027](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1027) |
| `AttackDamageMin` | 999 | [1028](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1028) |
| `AttackDamageMax` | 999 | [1029](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1029) |
| `AttackRate` | 2.05 | [1030](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1030) |
| `AttackAnimationPoint` | 0.3 | [1031](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1031) |
| `AttackAcquisitionRange` | 900 | [1032](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1032) |
| `AttackRange` | 100 | [1033](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1033) |
| `BountyXP` | 63 | [1037](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1037) |
| `BountyGoldMin` | 65 | [1038](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1038) |
| `BountyGoldMax` | 65 | [1039](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1039) |
| `MovementSpeed` | 320 | [1042](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1042) |
| `MovementTurnRate` | 0.5 | [1043](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1043) |
| `StatusHealth` | 7000 | [1045](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1045) |
| `StatusHealthRegen` | 1 | [1046](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1046) |
| `StatusMana` | 1000 | [1047](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1047) |
| `StatusManaRegen` | 1.25 | [1048](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1048) |
| `StatusStartingMana` | 500 | [1049](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1049) |
| `AttackType` | chaos | [1055](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1055) |
| `ArmorType` | heavy | [1056](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1056) |
| `VisionDaytimeRange` | 1400 | [1058](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1058) |
| `VisionNighttimeRange` | 800 | [1059](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1059) |
| `PathfindingSearchDepthScale` | 0.75 | [1061](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1061) |
| `HasInventory` | 0 | [1065](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_bosses.txt#L1065) |

Способности: Ability1: `19_wave_polymorph`.

Lua: [19_wave_bosses.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/19_wave_bosses.lua#L1).

## Кобольд - `1_wave_creep`

Источник: [normal_mode_creeps.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L5) |
| `ModelScale` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L9) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L10) |
| `ArmorPhysical` | 2 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L26) |
| `MagicalResistance` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L27) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L29) |
| `AttackDamageMin` | 16 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L30) |
| `AttackDamageMax` | 17 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L31) |
| `AttackRate` | 1.55 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L32) |
| `AttackAnimationPoint` | 0.3 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L33) |
| `AttackAcquisitionRange` | 0 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L34) |
| `AttackRange` | 100 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L35) |
| `BountyXP` | 25 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L39) |
| `BountyGoldMin` | 4 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L40) |
| `BountyGoldMax` | 4 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L41) |
| `MovementSpeed` | 300 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L44) |
| `MovementTurnRate` | 0.5 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L45) |
| `StatusHealth` | 130 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L47) |
| `StatusHealthRegen` | 0.5 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L48) |
| `StatusMana` | 0 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L49) |
| `StatusManaRegen` | 0 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L50) |
| `AttackType` | normal | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L56) |
| `ArmorType` | heavy | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L57) |
| `VisionDaytimeRange` | 800 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L59) |
| `VisionNighttimeRange` | 800 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L60) |
| `HasInventory` | 0 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L62) |
| `PathfindingSearchDepthScale` | 0.75 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L64) |

Способности: Ability1: `wave_1_poison`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Кентавр-Воин - `2_wave_creep`

Источник: [normal_mode_creeps.txt:69](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L69). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [71](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L71) |
| `ModelScale` | 0.85 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L74) |
| `Level` | 1 | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L75) |
| `ArmorPhysical` | 0 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L91) |
| `MagicalResistance` | 0 | [92](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L92) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L94) |
| `AttackDamageMin` | 31 | [95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L95) |
| `AttackDamageMax` | 32 | [96](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L96) |
| `AttackRate` | 1.55 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L97) |
| `AttackAnimationPoint` | 0.3 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L98) |
| `AttackAcquisitionRange` | 0 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L99) |
| `AttackRange` | 100 | [100](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L100) |
| `BountyXP` | 25 | [104](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L104) |
| `BountyGoldMin` | 4 | [105](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L105) |
| `BountyGoldMax` | 4 | [106](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L106) |
| `MovementSpeed` | 350 | [109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L109) |
| `MovementTurnRate` | 0.5 | [110](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L110) |
| `StatusHealth` | 220 | [112](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L112) |
| `StatusHealthRegen` | 0.5 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L113) |
| `StatusMana` | 0 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L114) |
| `StatusManaRegen` | 0 | [115](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L115) |
| `AttackType` | normal | [121](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L121) |
| `ArmorType` | heavy | [122](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L122) |
| `VisionDaytimeRange` | 800 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L124) |
| `VisionNighttimeRange` | 800 | [125](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L125) |
| `HasInventory` | 0 | [127](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L127) |
| `PathfindingSearchDepthScale` | 0.75 | [129](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L129) |

Способности: Ability1: `wave_2_centaurs_revenge`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Драконид - `3_wave_creep`

Источник: [normal_mode_creeps.txt:134](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L134). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [136](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L136) |
| `ModelScale` | 0.75 | [139](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L139) |
| `Level` | 1 | [140](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L140) |
| `ArmorPhysical` | 0 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L156) |
| `MagicalResistance` | 0 | [157](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L157) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L159) |
| `AttackDamageMin` | 51 | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L160) |
| `AttackDamageMax` | 57 | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L161) |
| `AttackRate` | 1.5 | [162](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L162) |
| `AttackAnimationPoint` | 0.3 | [163](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L163) |
| `AttackAcquisitionRange` | 0 | [164](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L164) |
| `AttackRange` | 100 | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L165) |
| `BountyXP` | 25 | [169](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L169) |
| `BountyGoldMin` | 5 | [170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L170) |
| `BountyGoldMax` | 5 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L171) |
| `MovementSpeed` | 270 | [174](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L174) |
| `MovementTurnRate` | 0.6 | [175](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L175) |
| `StatusHealth` | 375 | [177](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L177) |
| `StatusHealthRegen` | 0.5 | [178](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L178) |
| `StatusMana` | 200 | [179](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L179) |
| `StatusManaRegen` | 1 | [180](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L180) |
| `AttackType` | normal | [186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L186) |
| `ArmorType` | medium | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L187) |
| `VisionDaytimeRange` | 800 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L189) |
| `VisionNighttimeRange` | 800 | [190](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L190) |
| `HasInventory` | 0 | [192](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L192) |
| `PathfindingSearchDepthScale` | 0.75 | [194](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L194) |

Способности: Ability1: `wave_3_evasion`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Темный тролль - `4_wave_creep`

Источник: [normal_mode_creeps.txt:199](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L199). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [201](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L201) |
| `ModelScale` | 0.8 | [204](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L204) |
| `Level` | 1 | [205](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L205) |
| `ArmorPhysical` | 0 | [221](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L221) |
| `MagicalResistance` | 0 | [222](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L222) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [224](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L224) |
| `AttackDamageMin` | 41 | [225](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L225) |
| `AttackDamageMax` | 44 | [226](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L226) |
| `AttackRate` | 1.3 | [227](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L227) |
| `AttackAnimationPoint` | 0.3 | [228](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L228) |
| `AttackAcquisitionRange` | 0 | [229](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L229) |
| `AttackRange` | 450 | [230](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L230) |
| `ProjectileSpeed` | 1200 | [232](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L232) |
| `BountyXP` | 25 | [234](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L234) |
| `BountyGoldMin` | 5 | [235](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L235) |
| `BountyGoldMax` | 5 | [236](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L236) |
| `MovementSpeed` | 270 | [239](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L239) |
| `MovementTurnRate` | 0.5 | [240](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L240) |
| `StatusHealth` | 400 | [242](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L242) |
| `StatusHealthRegen` | 0.5 | [243](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L243) |
| `StatusMana` | 0 | [244](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L244) |
| `StatusManaRegen` | 0 | [245](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L245) |
| `AttackType` | pierce | [251](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L251) |
| `ArmorType` | heavy | [252](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L252) |
| `VisionDaytimeRange` | 800 | [254](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L254) |
| `VisionNighttimeRange` | 800 | [255](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L255) |
| `HasInventory` | 0 | [257](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L257) |
| `PathfindingSearchDepthScale` | 0.75 | [259](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L259) |

Способности: Ability1: `4_wave_true_hit`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Головорез - `6_wave_creep`

Источник: [normal_mode_creeps.txt:264](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L264). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [266](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L266) |
| `ModelScale` | 0.85 | [269](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L269) |
| `Level` | 1 | [270](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L270) |
| `ArmorPhysical` | 2 | [281](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L281) |
| `MagicalResistance` | 0 | [282](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L282) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [284](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L284) |
| `AttackDamageMin` | 76 | [285](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L285) |
| `AttackDamageMax` | 78 | [286](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L286) |
| `AttackRate` | 0.85 | [287](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L287) |
| `AttackAnimationPoint` | 0.3 | [288](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L288) |
| `AttackAcquisitionRange` | 0 | [289](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L289) |
| `AttackRange` | 100 | [290](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L290) |
| `BountyXP` | 33 | [294](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L294) |
| `BountyGoldMin` | 6 | [295](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L295) |
| `BountyGoldMax` | 6 | [296](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L296) |
| `MovementSpeed` | 320 | [299](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L299) |
| `MovementTurnRate` | 0.6 | [300](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L300) |
| `StatusHealth` | 750 | [302](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L302) |
| `StatusHealthRegen` | 0.5 | [303](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L303) |
| `StatusMana` | 300 | [304](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L304) |
| `StatusManaRegen` | 0.75 | [305](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L305) |
| `AttackType` | normal | [311](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L311) |
| `ArmorType` | heavy | [312](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L312) |
| `VisionDaytimeRange` | 800 | [314](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L314) |
| `VisionNighttimeRange` | 800 | [315](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L315) |
| `PathfindingSearchDepthScale` | 0.75 | [317](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L317) |
| `HasInventory` | 0 | [321](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L321) |

Способности: Ability1: `wave_6_assassin`; Ability2: `wave_6_invisibility`.

Lua: [6_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/6_wave_creeps.lua#L1).

## Чумной Энт - `7_wave_creep`

Источник: [normal_mode_creeps.txt:324](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L324). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [326](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L326) |
| `ModelScale` | 1 | [329](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L329) |
| `Level` | 1 | [330](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L330) |
| `ArmorPhysical` | 2 | [346](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L346) |
| `MagicalResistance` | 0 | [347](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L347) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [349](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L349) |
| `AttackDamageMin` | 111 | [350](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L350) |
| `AttackDamageMax` | 115 | [351](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L351) |
| `AttackRate` | 1.05 | [352](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L352) |
| `AttackAnimationPoint` | 0.3 | [353](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L353) |
| `AttackAcquisitionRange` | 0 | [354](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L354) |
| `AttackRange` | 100 | [355](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L355) |
| `BountyXP` | 33 | [359](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L359) |
| `BountyGoldMin` | 6 | [360](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L360) |
| `BountyGoldMax` | 6 | [361](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L361) |
| `MovementSpeed` | 400 | [364](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L364) |
| `MovementTurnRate` | 0.5 | [365](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L365) |
| `StatusHealth` | 800 | [367](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L367) |
| `StatusHealthRegen` | 0.5 | [368](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L368) |
| `StatusMana` | 250 | [369](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L369) |
| `StatusManaRegen` | 1 | [370](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L370) |
| `AttackType` | normal | [376](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L376) |
| `ArmorType` | heavy | [377](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L377) |
| `VisionDaytimeRange` | 800 | [379](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L379) |
| `VisionNighttimeRange` | 800 | [380](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L380) |
| `PathfindingSearchDepthScale` | 0.75 | [382](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L382) |
| `HasInventory` | 0 | [386](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L386) |

Способности: Ability1: `7_wave_plague`; Ability2: `7_wave_sharp_claws`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Властитель - `8_wave_creep`

Источник: [normal_mode_creeps.txt:389](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L389). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [391](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L391) |
| `ModelScale` | 1.25 | [394](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L394) |
| `Level` | 1 | [395](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L395) |
| `ArmorPhysical` | 1 | [411](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L411) |
| `MagicalResistance` | 80 | [412](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L412) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [414](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L414) |
| `AttackDamageMin` | 126 | [415](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L415) |
| `AttackDamageMax` | 129 | [416](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L416) |
| `AttackRate` | 1.05 | [417](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L417) |
| `AttackAnimationPoint` | 0.3 | [418](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L418) |
| `AttackAcquisitionRange` | 0 | [419](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L419) |
| `AttackRange` | 100 | [420](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L420) |
| `BountyXP` | 33 | [424](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L424) |
| `BountyGoldMin` | 7 | [425](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L425) |
| `BountyGoldMax` | 7 | [426](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L426) |
| `MovementSpeed` | 270 | [429](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L429) |
| `MovementTurnRate` | 0.6 | [430](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L430) |
| `StatusHealth` | 950 | [432](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L432) |
| `StatusHealthRegen` | 0.25 | [433](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L433) |
| `StatusMana` | 300 | [434](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L434) |
| `StatusManaRegen` | 0.75 | [435](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L435) |
| `AttackType` | normal | [441](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L441) |
| `ArmorType` | heavy | [442](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L442) |
| `VisionDaytimeRange` | 800 | [444](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L444) |
| `VisionNighttimeRange` | 800 | [445](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L445) |
| `PathfindingSearchDepthScale` | 0.75 | [447](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L447) |
| `HasInventory` | 0 | [451](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L451) |

Способности: Ability1: `wave_8_cleave`.

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Дух Воды - `9_wave_creep`

Источник: [normal_mode_creeps.txt:454](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L454). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [456](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L456) |
| `ModelScale` | 0.7 | [459](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L459) |
| `Level` | 1 | [460](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L460) |
| `ProjectileSpeed` | 1300 | [462](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L462) |
| `ArmorPhysical` | 1 | [479](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L479) |
| `MagicalResistance` | 0 | [480](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L480) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [482](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L482) |
| `AttackDamageMin` | 141 | [483](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L483) |
| `AttackDamageMax` | 150 | [484](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L484) |
| `AttackRate` | 1.0 | [485](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L485) |
| `AttackAnimationPoint` | 0.3 | [486](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L486) |
| `AttackAcquisitionRange` | 0 | [487](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L487) |
| `AttackRange` | 450 | [488](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L488) |
| `BountyXP` | 33 | [490](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L490) |
| `BountyGoldMin` | 7 | [491](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L491) |
| `BountyGoldMax` | 7 | [492](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L492) |
| `MovementSpeed` | 220 | [495](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L495) |
| `MovementTurnRate` | 0.5 | [496](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L496) |
| `StatusHealth` | 975 | [498](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L498) |
| `StatusHealthRegen` | 0.5 | [499](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L499) |
| `StatusMana` | 0 | [500](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L500) |
| `StatusManaRegen` | 0 | [501](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L501) |
| `AttackType` | pierce | [507](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L507) |
| `ArmorType` | medium | [508](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L508) |
| `VisionDaytimeRange` | 800 | [510](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L510) |
| `VisionNighttimeRange` | 800 | [511](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L511) |
| `PathfindingSearchDepthScale` | 0.75 | [513](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L513) |
| `HasInventory` | 0 | [517](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L517) |

Lua: [attack_wave_only.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/attack_wave_only.lua#L1).

## Опустошитель - `11_wave_creep`

Источник: [normal_mode_creeps.txt:520](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L520). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [522](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L522) |
| `ModelScale` | 0.8 | [525](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L525) |
| `Level` | 1 | [526](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L526) |
| `ArmorPhysical` | 8 | [542](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L542) |
| `MagicalResistance` | 0 | [543](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L543) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [545](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L545) |
| `AttackDamageMin` | 181 | [546](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L546) |
| `AttackDamageMax` | 186 | [547](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L547) |
| `AttackRate` | 0.65 | [548](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L548) |
| `AttackAnimationPoint` | 0.3 | [549](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L549) |
| `AttackAcquisitionRange` | 0 | [550](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L550) |
| `AttackRange` | 100 | [551](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L551) |
| `BountyXP` | 48 | [555](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L555) |
| `BountyGoldMin` | 8 | [556](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L556) |
| `BountyGoldMax` | 8 | [557](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L557) |
| `MovementSpeed` | 420 | [560](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L560) |
| `MovementTurnRate` | 0.5 | [561](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L561) |
| `StatusHealth` | 1100 | [563](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L563) |
| `StatusHealthRegen` | 0.5 | [564](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L564) |
| `StatusMana` | 300 | [565](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L565) |
| `StatusManaRegen` | 0.75 | [566](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L566) |
| `AttackType` | chaos | [572](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L572) |
| `ArmorType` | medium | [573](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L573) |
| `VisionDaytimeRange` | 800 | [575](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L575) |
| `VisionNighttimeRange` | 800 | [576](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L576) |
| `PathfindingSearchDepthScale` | 1 | [578](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L578) |
| `HasInventory` | 0 | [582](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L582) |

Способности: Ability1: `11_wave_immolation`; Ability2: `wave_11_cleave`.

Lua: [11_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/11_wave_creeps.lua#L1).

## Адский Медведь - `12_wave_creep`

Источник: [normal_mode_creeps.txt:585](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L585). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [587](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L587) |
| `ModelScale` | 0.8 | [590](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L590) |
| `Level` | 1 | [591](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L591) |
| `ArmorPhysical` | 8 | [607](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L607) |
| `MagicalResistance` | 0 | [608](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L608) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [610](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L610) |
| `AttackDamageMin` | 241 | [611](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L611) |
| `AttackDamageMax` | 244 | [612](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L612) |
| `AttackRate` | 0.65 | [613](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L613) |
| `AttackAnimationPoint` | 0.3 | [614](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L614) |
| `AttackAcquisitionRange` | 0 | [615](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L615) |
| `AttackRange` | 100 | [616](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L616) |
| `BountyXP` | 48 | [620](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L620) |
| `BountyGoldMin` | 8 | [621](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L621) |
| `BountyGoldMax` | 8 | [622](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L622) |
| `MovementSpeed` | 300 | [625](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L625) |
| `MovementTurnRate` | 0.5 | [626](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L626) |
| `StatusHealth` | 1400 | [628](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L628) |
| `StatusHealthRegen` | 0.5 | [629](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L629) |
| `StatusMana` | 100 | [630](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L630) |
| `StatusManaRegen` | 2 | [631](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L631) |
| `AttackType` | normal | [637](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L637) |
| `ArmorType` | heavy | [638](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L638) |
| `VisionDaytimeRange` | 800 | [640](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L640) |
| `VisionNighttimeRange` | 800 | [641](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L641) |
| `PathfindingSearchDepthScale` | 0.75 | [643](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L643) |
| `HasInventory` | 0 | [645](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L645) |

Способности: Ability1: `12_wave_stomp`; Ability2: `wave_12_crit`.

Lua: [12_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/12_wave_creeps.lua#L1).

## Каменный Голем - `13_wave_creep`

Источник: [normal_mode_creeps.txt:650](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L650). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [652](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L652) |
| `ModelScale` | 0.95 | [656](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L656) |
| `Level` | 1 | [657](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L657) |
| `ArmorPhysical` | 10 | [673](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L673) |
| `MagicalResistance` | 80 | [674](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L674) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [676](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L676) |
| `AttackDamageMin` | 281 | [677](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L677) |
| `AttackDamageMax` | 285 | [678](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L678) |
| `AttackRate` | 0.75 | [679](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L679) |
| `AttackAnimationPoint` | 0.3 | [680](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L680) |
| `AttackAcquisitionRange` | 0 | [681](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L681) |
| `AttackRange` | 100 | [682](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L682) |
| `BountyXP` | 48 | [686](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L686) |
| `BountyGoldMin` | 9 | [687](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L687) |
| `BountyGoldMax` | 9 | [688](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L688) |
| `MovementSpeed` | 270 | [691](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L691) |
| `MovementTurnRate` | 0.5 | [692](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L692) |
| `StatusHealth` | 1600 | [694](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L694) |
| `StatusHealthRegen` | 0.5 | [695](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L695) |
| `StatusMana` | 100 | [696](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L696) |
| `StatusManaRegen` | 1 | [697](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L697) |
| `AttackType` | normal | [703](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L703) |
| `ArmorType` | heavy | [704](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L704) |
| `VisionDaytimeRange` | 800 | [706](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L706) |
| `VisionNighttimeRange` | 800 | [707](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L707) |
| `PathfindingSearchDepthScale` | 0.75 | [709](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L709) |
| `HasInventory` | 0 | [713](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L713) |

Способности: Ability1: `13_wave_hurl_boulder`.

Lua: [13_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/13_wave_creeps.lua#L1).

## Громовая Ящерица - `14_wave_creep`

Источник: [normal_mode_creeps.txt:716](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L716). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [718](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L718) |
| `ModelScale` | 0.7 | [721](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L721) |
| `Level` | 1 | [722](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L722) |
| `ProjectileSpeed` | 1500 | [724](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L724) |
| `ArmorPhysical` | 15 | [741](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L741) |
| `MagicalResistance` | 0 | [742](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L742) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [744](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L744) |
| `AttackDamageMin` | 191 | [745](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L745) |
| `AttackDamageMax` | 200 | [746](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L746) |
| `AttackRate` | 0.8 | [747](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L747) |
| `AttackAnimationPoint` | 0.3 | [748](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L748) |
| `AttackAcquisitionRange` | 0 | [749](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L749) |
| `AttackRange` | 500 | [750](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L750) |
| `SplashAttack` | 1 | [752](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L752) |
| `SplashFullDamageRadius` | 75 | [753](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L753) |
| `SplashMediumRadius` | 150 | [754](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L754) |
| `SplashMediumDamage` | 0.5 | [755](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L755) |
| `SplashSmallRadius` | 225 | [756](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L756) |
| `SplashSmallDamage` | 0.25 | [757](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L757) |
| `BountyXP` | 48 | [759](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L759) |
| `BountyGoldMin` | 10 | [760](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L760) |
| `BountyGoldMax` | 10 | [761](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L761) |
| `MovementSpeed` | 270 | [764](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L764) |
| `MovementTurnRate` | 0.5 | [765](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L765) |
| `StatusHealth` | 1500 | [767](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L767) |
| `StatusHealthRegen` | 1.5 | [768](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L768) |
| `StatusMana` | 500 | [769](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L769) |
| `StatusManaRegen` | 1.5 | [770](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L770) |
| `AttackType` | chaos | [776](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L776) |
| `ArmorType` | heavy | [777](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L777) |
| `VisionDaytimeRange` | 800 | [779](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L779) |
| `VisionNighttimeRange` | 800 | [780](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L780) |
| `PathfindingSearchDepthScale` | 0.75 | [782](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L782) |
| `HasInventory` | 0 | [786](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L786) |

Способности: Ability1: `14_wave_shadow_strike`.

Lua: [14_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/14_wave_creeps.lua#L1).

## Призрак - `16_wave_creep`

Источник: [normal_mode_creeps.txt:789](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L789). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [791](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L791) |
| `ModelScale` | 1 | [795](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L795) |
| `Level` | 1 | [796](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L796) |
| `ProjectileSpeed` | 900 | [798](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L798) |
| `ArmorPhysical` | 20 | [814](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L814) |
| `MagicalResistance` | 0 | [815](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L815) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [817](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L817) |
| `AttackDamageMin` | 376 | [818](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L818) |
| `AttackDamageMax` | 386 | [819](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L819) |
| `AttackRate` | 0.8 | [820](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L820) |
| `AttackAnimationPoint` | 0.3 | [821](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L821) |
| `AttackAcquisitionRange` | 0 | [822](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L822) |
| `AttackRange` | 400 | [823](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L823) |
| `BountyXP` | 63 | [825](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L825) |
| `BountyGoldMin` | 11 | [826](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L826) |
| `BountyGoldMax` | 11 | [827](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L827) |
| `MovementSpeed` | 270 | [830](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L830) |
| `MovementTurnRate` | 0.6 | [831](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L831) |
| `StatusHealth` | 1700 | [833](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L833) |
| `StatusHealthRegen` | 0.5 | [834](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L834) |
| `StatusMana` | 400 | [835](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L835) |
| `StatusManaRegen` | 1 | [836](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L836) |
| `AttackType` | magic | [842](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L842) |
| `ArmorType` | heavy | [843](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L843) |
| `VisionDaytimeRange` | 800 | [845](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L845) |
| `VisionNighttimeRange` | 800 | [846](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L846) |
| `PathfindingSearchDepthScale` | 0.75 | [848](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L848) |
| `HasInventory` | 0 | [852](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L852) |

Способности: Ability1: `wave_16_mana_burn`.

Lua: [16_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/16_wave_creeps.lua#L1).

## Хвататель - `17_wave_creep`

Источник: [normal_mode_creeps.txt:855](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L855). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [857](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L857) |
| `ModelScale` | 1 | [860](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L860) |
| `Level` | 1 | [861](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L861) |
| `ArmorPhysical` | 40 | [877](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L877) |
| `MagicalResistance` | 50 | [878](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L878) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [880](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L880) |
| `AttackDamageMin` | 451 | [881](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L881) |
| `AttackDamageMax` | 455 | [882](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L882) |
| `AttackRate` | 1.05 | [883](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L883) |
| `AttackAnimationPoint` | 0.3 | [884](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L884) |
| `AttackAcquisitionRange` | 0 | [885](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L885) |
| `AttackRange` | 110 | [886](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L886) |
| `BountyXP` | 63 | [890](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L890) |
| `BountyGoldMin` | 11 | [891](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L891) |
| `BountyGoldMax` | 11 | [892](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L892) |
| `MovementSpeed` | 350 | [895](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L895) |
| `MovementTurnRate` | 0.5 | [896](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L896) |
| `StatusHealth` | 2200 | [898](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L898) |
| `StatusHealthRegen` | 0.5 | [899](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L899) |
| `StatusMana` | 300 | [900](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L900) |
| `StatusManaRegen` | 1 | [901](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L901) |
| `AttackType` | normal | [907](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L907) |
| `ArmorType` | heavy | [908](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L908) |
| `VisionDaytimeRange` | 800 | [910](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L910) |
| `VisionNighttimeRange` | 800 | [911](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L911) |
| `PathfindingSearchDepthScale` | 0.75 | [913](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L913) |
| `HasInventory` | 0 | [918](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L918) |

Способности: Ability1: `17_wave_purge`.

Lua: [17_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/17_wave_creeps.lua#L1).

## Хранитель Пустоты - `18_wave_creep`

Источник: [normal_mode_creeps.txt:921](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L921). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [923](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L923) |
| `ModelScale` | 1.1 | [926](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L926) |
| `Level` | 1 | [927](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L927) |
| `ArmorPhysical` | 10 | [945](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L945) |
| `MagicalResistance` | 50 | [946](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L946) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [948](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L948) |
| `AttackDamageMin` | 451 | [949](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L949) |
| `AttackDamageMax` | 455 | [950](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L950) |
| `AttackRate` | 1.1 | [951](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L951) |
| `AttackAnimationPoint` | 0.3 | [952](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L952) |
| `AttackAcquisitionRange` | 0 | [953](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L953) |
| `AttackRange` | 250 | [954](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L954) |
| `ProjectileSpeed` | 1900 | [956](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L956) |
| `BountyXP` | 63 | [958](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L958) |
| `BountyGoldMin` | 11 | [959](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L959) |
| `BountyGoldMax` | 11 | [960](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L960) |
| `MovementSpeed` | 350 | [963](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L963) |
| `MovementTurnRate` | 0.5 | [964](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L964) |
| `StatusHealth` | 2200 | [966](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L966) |
| `StatusHealthRegen` | 1 | [967](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L967) |
| `StatusMana` | 150 | [968](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L968) |
| `StatusManaRegen` | 1 | [969](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L969) |
| `AttackType` | pierce | [975](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L975) |
| `ArmorType` | medium | [976](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L976) |
| `VisionDaytimeRange` | 800 | [978](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L978) |
| `VisionNighttimeRange` | 800 | [979](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L979) |
| `PathfindingSearchDepthScale` | 0.75 | [981](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L981) |
| `HasInventory` | 0 | [985](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L985) |

Способности: Ability1: `18_wave_silence`.

Lua: [18_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/18_wave_creeps.lua#L1).

## Адский Сатир - `19_wave_creep`

Источник: [normal_mode_creeps.txt:996](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L996). Включен engine-root: `True`.

Предварительная роль по ID/пути: `wave_creep`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [998](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L998) |
| `ModelScale` | 1 | [1001](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1001) |
| `Level` | 1 | [1002](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1002) |
| `ArmorPhysical` | 25 | [1018](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1018) |
| `MagicalResistance` | 0 | [1019](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1019) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [1021](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1021) |
| `AttackDamageMin` | 666 | [1022](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1022) |
| `AttackDamageMax` | 666 | [1023](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1023) |
| `AttackRate` | 1.75 | [1024](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1024) |
| `AttackAnimationPoint` | 0.3 | [1025](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1025) |
| `AttackAcquisitionRange` | 0 | [1026](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1026) |
| `AttackRange` | 100 | [1027](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1027) |
| `BountyXP` | 63 | [1031](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1031) |
| `BountyGoldMin` | 12 | [1032](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1032) |
| `BountyGoldMax` | 12 | [1033](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1033) |
| `MovementSpeed` | 320 | [1036](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1036) |
| `MovementTurnRate` | 0.5 | [1037](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1037) |
| `StatusHealth` | 2500 | [1039](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1039) |
| `StatusHealthRegen` | 1 | [1040](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1040) |
| `StatusMana` | 500 | [1041](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1041) |
| `StatusManaRegen` | 1.25 | [1042](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1042) |
| `AttackType` | chaos | [1048](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1048) |
| `ArmorType` | heavy | [1049](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1049) |
| `VisionDaytimeRange` | 800 | [1051](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1051) |
| `VisionNighttimeRange` | 800 | [1052](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1052) |
| `PathfindingSearchDepthScale` | 0.75 | [1054](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1054) |
| `HasInventory` | 0 | [1058](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/normal_mode_creeps.txt#L1058) |

Способности: Ability1: `19_wave_faerie_fire`.

Lua: [19_wave_creeps.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/19_wave_creeps.lua#L1).

## имя не найдено - `npc_dummy_blank`

Источник: [npc_dummy_blank.txt:4](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L4). Включен engine-root: `True`.

Предварительная роль по ID/пути: `helper_or_decoration`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_base_additive | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L8) |
| `Level` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L11) |
| `ArmorPhysical` | 0 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L25) |
| `MagicalResistance` | 0 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L26) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L29) |
| `AttackDamageMin` | 0 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L30) |
| `AttackDamageMax` | 0 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L31) |
| `AttackRate` | 1 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L33) |
| `AttackAnimationPoint` | 0.5 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L34) |
| `AttackAcquisitionRange` | 800 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L35) |
| `AttackRange` | 500 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L36) |
| `ProjectileSpeed` | 900 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L38) |
| `AttributePrimary` | DOTA_ATTRIBUTE_STRENGTH | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L41) |
| `AttributeBaseStrength` | 0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L42) |
| `AttributeStrengthGain` | 0 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L43) |
| `AttributeBaseIntelligence` | 0 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L44) |
| `AttributeIntelligenceGain` | 0 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L45) |
| `AttributeBaseAgility` | 0 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L46) |
| `AttributeAgilityGain` | 0 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L47) |
| `BountyXP` | 0 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L50) |
| `BountyGoldMin` | 0 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L51) |
| `BountyGoldMax` | 0 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L52) |
| `MovementSpeed` | 450 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L59) |
| `MovementTurnRate` | 10 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L60) |
| `FollowRange` | 250 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L61) |
| `StatusHealth` | 1 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L64) |
| `StatusHealthRegen` | 0 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L65) |
| `StatusMana` | 0 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L66) |
| `StatusManaRegen` | 0 | [67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L67) |
| `VisionDaytimeRange` | 0 | [76](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L76) |
| `VisionNighttimeRange` | 0 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L77) |
| `AttackDesire` | 1.5 | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/npc_dummy_blank.txt#L80) |

## Орн - `orn_megaboss`

Источник: [orn_megaboss.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `megaboss_or_final_add`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L5) |
| `ModelScale` | 1.6 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L8) |
| `RingRadius` | 105 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L9) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L10) |
| `AbilityLayout` | 7 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L51) |
| `ArmorPhysical` | 80 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L53) |
| `MagicalResistance` | 25 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L54) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L56) |
| `AttackDamageMin` | 675 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L57) |
| `AttackDamageMax` | 725 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L58) |
| `AttackRate` | 0.4 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L59) |
| `AttackAnimationPoint` | 0.3 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L60) |
| `AttackAcquisitionRange` | 5000 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L61) |
| `AttackRange` | 180 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L62) |
| `BountyXP` | 0 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L66) |
| `BountyGoldMin` | 0 | [67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L67) |
| `BountyGoldMax` | 0 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L68) |
| `MovementSpeed` | 450 | [71](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L71) |
| `MovementTurnRate` | 0.6 | [72](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L72) |
| `StatusHealth` | 20000 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L74) |
| `StatusHealthRegen` | 50 | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L75) |
| `StatusMana` | 3000 | [76](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L76) |
| `StatusManaRegen` | 1 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L77) |
| `AttackType` | hero | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L83) |
| `ArmorType` | heavy | [84](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L84) |
| `VisionDaytimeRange` | 2000 | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L86) |
| `VisionNighttimeRange` | 2000 | [87](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L87) |
| `HasInventory` | 0 | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_megaboss.txt#L89) |

Способности: Ability1: `true_sight`; Ability2: `orn_all_bonuses`; Ability3: `orn_cleave_manabreak_lifesteal`; Ability4: `orn_return`; Ability5: `orn_bash`; Ability6: `orn_crit`; Ability7: `orn_mana_break`.

Lua: [MegabossOrnStats.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/units/MegabossOrnStats.lua#L1).

## имя не найдено - `orn_mutant_boss`

Источник: [orn_mutant_boss.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `megaboss_or_final_add`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L5) |
| `ModelScale` | 1.6 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L9) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L10) |
| `ArmorPhysical` | 50 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L26) |
| `MagicalResistance` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L27) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L29) |
| `AttackDamageMin` | 2000 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L30) |
| `AttackDamageMax` | 2000 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L31) |
| `AttackRate` | 1.2 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L32) |
| `AttackAnimationPoint` | 0.3 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L33) |
| `AttackAcquisitionRange` | 2000 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L34) |
| `AttackRange` | 90 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L35) |
| `BountyXP` | 100 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L39) |
| `BountyGoldMin` | 20 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L40) |
| `BountyGoldMax` | 20 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L41) |
| `MovementSpeed` | 300 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L44) |
| `MovementTurnRate` | 0.5 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L45) |
| `StatusHealth` | 2000 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L47) |
| `StatusHealthRegen` | 250 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L48) |
| `StatusMana` | 0 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L49) |
| `StatusManaRegen` | 0 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L50) |
| `AttackType` | chaos | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L56) |
| `ArmorType` | heavy | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L57) |
| `VisionDaytimeRange` | 2000 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L59) |
| `VisionNighttimeRange` | 2000 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L60) |
| `HasInventory` | 0 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/orn_mutant_boss.txt#L64) |

## Скелет-Воин - `book_of_the_dead_skeleton_melee`

Источник: [purchasable_units.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L5) |
| `ModelScale` | 1 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L8) |
| `Level` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L9) |
| `ArmorPhysical` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L20) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L22) |
| `AttackDamageMin` | 22 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L24) |
| `AttackDamageMax` | 22 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L25) |
| `AttackRate` | 1.5 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L26) |
| `AttackAnimationPoint` | 0.4 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L27) |
| `AttackAcquisitionRange` | 500 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L28) |
| `AttackRange` | 128 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L29) |
| `BountyXP` | 100 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L33) |
| `BountyGoldMin` | 5 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L34) |
| `BountyGoldMax` | 5 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L35) |
| `MovementSpeed` | 180 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L38) |
| `MovementTurnRate` | 0.4 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L39) |
| `StatusHealth` | 250 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L41) |
| `StatusHealthRegen` | 0.1 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L42) |
| `StatusMana` | 0 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L43) |
| `StatusManaRegen` | 0 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L44) |
| `HealthBarOffset` | 150 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L47) |
| `AttackType` | normal | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L53) |
| `ArmorType` | heavy | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L54) |
| `VisionDaytimeRange` | 900 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L56) |
| `VisionNighttimeRange` | 800 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L57) |
| `HasInventory` | 0 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L59) |

## Скелет-Лучник - `book_of_the_dead_skeleton_melee_ranged`

Источник: [purchasable_units.txt:62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L62). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L64) |
| `ModelScale` | 0.5 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L66) |
| `Level` | 1 | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L70) |
| `UseNeutralCreepBehavior` | 0 | [72](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L72) |
| `ArmorPhysical` | 1 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L77) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L79) |
| `AttackDamageMin` | 18 | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L81) |
| `AttackDamageMax` | 18 | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L82) |
| `AttackRate` | 1.2 | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L83) |
| `AttackAnimationPoint` | 0.3 | [84](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L84) |
| `AttackAcquisitionRange` | 500 | [85](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L85) |
| `AttackRange` | 500 | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L86) |
| `ProjectileSpeed` | 900 | [88](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L88) |
| `BountyXP` | 100 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L90) |
| `BountyGoldMin` | 5 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L91) |
| `BountyGoldMax` | 5 | [92](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L92) |
| `HealthBarOffset` | 140 | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L94) |
| `MovementSpeed` | 180 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L98) |
| `MovementTurnRate` | 0.4 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L99) |
| `StatusHealth` | 210 | [101](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L101) |
| `StatusHealthRegen` | 0.5 | [102](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L102) |
| `StatusMana` | 0 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L103) |
| `StatusManaRegen` | 0.75 | [104](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L104) |
| `AttackType` | pierce | [110](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L110) |
| `ArmorType` | heavy | [111](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L111) |
| `VisionDaytimeRange` | 1400 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L113) |
| `VisionNighttimeRange` | 1400 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L114) |

## Тролль-Защитник - `npc_lia_troll_defender`

Источник: [purchasable_units.txt:145](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L145). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [147](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L147) |
| `ModelScale` | 0.9 | [150](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L150) |
| `Level` | 1 | [151](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L151) |
| `ArmorPhysical` | 1 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L156) |
| `MagicalResistance` | 0 | [157](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L157) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L159) |
| `AttackDamageMin` | 30 | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L161) |
| `AttackDamageMax` | 30 | [162](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L162) |
| `AttackRate` | 1.6 | [163](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L163) |
| `AttackAnimationPoint` | 0.4 | [164](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L164) |
| `AttackAcquisitionRange` | 500 | [165](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L165) |
| `AttackRange` | 500 | [166](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L166) |
| `ProjectileSpeed` | 1200 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L168) |
| `BountyXP` | 100 | [170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L170) |
| `BountyGoldMin` | 5 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L171) |
| `BountyGoldMax` | 5 | [172](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L172) |
| `MovementSpeed` | 290 | [175](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L175) |
| `MovementTurnRate` | 0.5 | [176](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L176) |
| `StatusHealth` | 100 | [178](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L178) |
| `StatusHealthRegen` | 0 | [179](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L179) |
| `StatusMana` | 100 | [180](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L180) |
| `StatusManaRegen` | 0 | [181](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L181) |
| `AttackType` | pierce | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L187) |
| `ArmorType` | heavy | [188](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L188) |
| `VisionDaytimeRange` | 800 | [190](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L190) |
| `VisionNighttimeRange` | 900 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L191) |
| `HasInventory` | 0 | [193](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L193) |

Способности: Ability1: `invulnerability`; Ability2: `troll_defender_armor_aura`.

## Тролль-Лекарь - `npc_lia_troll_healer`

Источник: [purchasable_units.txt:213](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L213). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [215](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L215) |
| `ModelScale` | 0.9 | [218](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L218) |
| `Level` | 1 | [219](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L219) |
| `ArmorPhysical` | 15 | [224](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L224) |
| `MagicalResistance` | 0 | [225](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L225) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [227](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L227) |
| `AttackDamageMin` | 0 | [229](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L229) |
| `AttackDamageMax` | 0 | [230](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L230) |
| `AttackRate` | 1.8 | [231](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L231) |
| `AttackAnimationPoint` | 0.3 | [232](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L232) |
| `AttackAcquisitionRange` | 500 | [233](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L233) |
| `AttackRange` | 600 | [234](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L234) |
| `ProjectileSpeed` | 1200 | [236](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L236) |
| `BountyXP` | 0 | [238](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L238) |
| `BountyGoldMin` | 0 | [239](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L239) |
| `BountyGoldMax` | 0 | [240](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L240) |
| `MovementSpeed` | 290 | [243](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L243) |
| `MovementTurnRate` | 0.5 | [244](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L244) |
| `StatusHealth` | 700 | [246](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L246) |
| `StatusHealthRegen` | 0.5 | [247](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L247) |
| `StatusMana` | 100 | [248](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L248) |
| `StatusManaRegen` | 0.5 | [249](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L249) |
| `AttackType` | pierce | [255](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L255) |
| `ArmorType` | heavy | [256](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L256) |
| `VisionDaytimeRange` | 1000 | [258](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L258) |
| `VisionNighttimeRange` | 1000 | [259](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L259) |
| `HasInventory` | 0 | [261](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L261) |

Способности: Ability1: `spell_immunity`; Ability2: `troll_healer_heal`.

Lua: [TrollHealer.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/AI/TrollHealer.lua#L1).

## Привратник Ада - `npc_doom_guard_spawn`

Источник: [purchasable_units.txt:267](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L267). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [269](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L269) |
| `ModelScale` | 0.9 | [271](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L271) |
| `Level` | 6 | [272](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L272) |
| `UseNeutralCreepBehavior` | 0 | [278](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L278) |
| `ArmorPhysical` | 7 | [285](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L285) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [287](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L287) |
| `AttackDamageMin` | 140 | [288](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L288) |
| `AttackDamageMax` | 140 | [289](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L289) |
| `AttackRate` | 1.3 | [291](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L291) |
| `AttackAnimationPoint` | 0.467 | [292](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L292) |
| `AttackAcquisitionRange` | 1000 | [293](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L293) |
| `AttackRange` | 128 | [294](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L294) |
| `BountyXP` | 100 | [298](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L298) |
| `BountyGoldMin` | 0 | [299](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L299) |
| `BountyGoldMax` | 0 | [300](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L300) |
| `StatusHealth` | 1700 | [302](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L302) |
| `StatusHealthRegen` | 0.5 | [303](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L303) |
| `StatusMana` | 500 | [304](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L304) |
| `StatusManaRegen` | 1.25 | [305](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L305) |
| `AttackType` | chaos | [311](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L311) |
| `ArmorType` | heavy | [312](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L312) |
| `VisionDaytimeRange` | 1400 | [314](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L314) |
| `VisionNighttimeRange` | 1400 | [315](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L315) |
| `MovementSpeed` | 270 | [318](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L318) |
| `MovementTurnRate` | 0.4 | [319](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L319) |
| `HealthBarOffset` | 250 | [322](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L322) |

Способности: Ability1: `doom_spawn_cripple`; Ability2: `doom_spawn_stomp`.

## Адская Тварь - `hell_beast`

Источник: [purchasable_units.txt:356](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L356). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [358](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L358) |
| `ModelScale` | 1.0 | [361](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L361) |
| `Level` | 1 | [362](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L362) |
| `ArmorPhysical` | 5 | [395](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L395) |
| `MagicalResistance` | 0 | [396](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L396) |
| `HealthBarOffset` | 250 | [399](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L399) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [401](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L401) |
| `AttackDamageMin` | 60 | [404](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L404) |
| `AttackDamageMax` | 65 | [405](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L405) |
| `AttackRate` | 0.9 | [406](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L406) |
| `AttackAnimationPoint` | 0.3 | [407](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L407) |
| `AttackAcquisitionRange` | 500 | [408](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L408) |
| `AttackRange` | 100 | [409](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L409) |
| `BountyXP` | 0 | [413](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L413) |
| `BountyGoldMin` | 0 | [414](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L414) |
| `BountyGoldMax` | 0 | [415](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L415) |
| `MovementSpeed` | 300 | [418](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L418) |
| `MovementTurnRate` | 0.5 | [419](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L419) |
| `StatusHealth` | 900 | [421](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L421) |
| `StatusHealthRegen` | 0.5 | [422](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L422) |
| `StatusMana` | 300 | [423](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L423) |
| `StatusManaRegen` | 1 | [424](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L424) |
| `AttackType` | chaos | [430](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L430) |
| `ArmorType` | heavy | [431](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L431) |
| `VisionDaytimeRange` | 900 | [433](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L433) |
| `VisionNighttimeRange` | 800 | [434](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L434) |
| `HasInventory` | 0 | [436](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L436) |

Способности: Ability1: `hell_beast_bloodlust`; Ability2: `hell_beast_sleep`.

## Огненный Голем - `spherical_staff_fire_golem`

Источник: [purchasable_units.txt:439](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L439). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [441](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L441) |
| `ModelScale` | 1 | [446](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L446) |
| `Level` | 1 | [447](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L447) |
| `ArmorPhysical` | 16 | [464](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L464) |
| `MagicalResistance` | 100 | [465](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L465) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [467](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L467) |
| `AttackDamageMin` | 100 | [468](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L468) |
| `AttackDamageMax` | 120 | [469](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L469) |
| `AttackRate` | 0.95 | [470](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L470) |
| `AttackAnimationPoint` | 0.26 | [471](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L471) |
| `AttackAcquisitionRange` | 500 | [472](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L472) |
| `AttackRange` | 100 | [473](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L473) |
| `HealthBarOffset` | 260 | [475](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L475) |
| `BountyXP` | 0 | [477](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L477) |
| `BountyGoldMin` | 0 | [478](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L478) |
| `BountyGoldMax` | 0 | [479](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L479) |
| `MovementSpeed` | 320 | [482](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L482) |
| `MovementTurnRate` | 0.4 | [483](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L483) |
| `StatusHealth` | 1800 | [485](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L485) |
| `StatusHealthRegen` | 1 | [486](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L486) |
| `StatusMana` | 0 | [487](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L487) |
| `StatusManaRegen` | 0 | [488](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L488) |
| `AttackType` | chaos | [494](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L494) |
| `ArmorType` | heavy | [495](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L495) |
| `VisionDaytimeRange` | 900 | [497](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L497) |
| `VisionNighttimeRange` | 800 | [498](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L498) |
| `HasInventory` | 0 | [500](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L500) |

Способности: Ability1: `spell_immunity`; Ability2: `spherical_staff_fire_golem_immolation`.

## Кабан - `npc_lia_boar`

Источник: [purchasable_units.txt:503](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L503). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [505](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L505) |
| `ModelScale` | 0.9 | [508](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L508) |
| `Level` | 1 | [509](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L509) |
| `ArmorPhysical` | 0 | [514](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L514) |
| `MagicalResistance` | 0 | [515](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L515) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [517](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L517) |
| `AttackDamageMin` | 16 | [519](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L519) |
| `AttackDamageMax` | 17 | [520](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L520) |
| `AttackRate` | 1.25 | [521](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L521) |
| `AttackAnimationPoint` | 0.33 | [522](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L522) |
| `AttackAcquisitionRange` | 500 | [523](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L523) |
| `AttackRange` | 500 | [524](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L524) |
| `ProjectileSpeed` | 1050 | [526](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L526) |
| `BountyXP` | 100 | [528](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L528) |
| `BountyGoldMin` | 5 | [529](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L529) |
| `BountyGoldMax` | 5 | [530](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L530) |
| `MovementSpeed` | 310 | [533](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L533) |
| `MovementTurnRate` | 0.6 | [534](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L534) |
| `StatusHealth` | 100 | [536](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L536) |
| `StatusHealthRegen` | 0 | [537](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L537) |
| `StatusMana` | 100 | [538](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L538) |
| `StatusManaRegen` | 0 | [539](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L539) |
| `AttackType` | pierce | [545](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L545) |
| `ArmorType` | heavy | [546](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L546) |
| `VisionDaytimeRange` | 800 | [548](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L548) |
| `VisionNighttimeRange` | 900 | [549](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L549) |
| `HasInventory` | 0 | [551](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/purchasable_units.txt#L551) |

Способности: Ability1: `invulnerability`; Ability2: `boar_poison`.

## имя не найдено - `pure_light_totem`

Источник: [pure_light_totem.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_building | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L7) |
| `ModelScale` | 0.5 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L9) |
| `Level` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L10) |
| `HealthBarOffset` | 250 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L13) |
| `ArmorPhysical` | 5 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L21) |
| `MagicalResistance` | 100 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L22) |
| `AttackCapabilities` | DOTA_UNIT_CAP_NO_ATTACK | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L26) |
| `AttackDamageMin` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L27) |
| `AttackDamageMax` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L28) |
| `AttackRate` | 0 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L30) |
| `AttackAnimationPoint` | 0 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L31) |
| `AttackAcquisitionRange` | 0 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L32) |
| `AttackRange` | 0 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L33) |
| `ProjectileSpeed` | 0 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L35) |
| `BountyGoldMin` | 0 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L39) |
| `BountyGoldMax` | 0 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L40) |
| `MovementSpeed` | 0 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L45) |
| `MovementTurnRate` | 0 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L46) |
| `StatusHealth` | 500 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L50) |
| `StatusMana` | 0 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L51) |
| `StatsManaRegen` | 0 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L52) |
| `VisionDaytimeRange` | 800 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L56) |
| `VisionNighttimeRange` | 600 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L57) |
| `AttackType` | siege | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L65) |
| `ArmorType` | fortified | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L66) |
| `RingRadius` | 180 | [69](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/pure_light_totem.txt#L69) |

## Тень - `shadow_master_shadow`

Источник: [shadow_master_shadow.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L6) |
| `ModelScale` | 1 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L8) |
| `Level` | 0 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L9) |
| `ArmorPhysical` | 0 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L15) |
| `MagicalResistance` | 25 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L16) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L18) |
| `AttackDamageMin` | 2 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L19) |
| `AttackDamageMax` | 24 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L20) |
| `AttackRate` | 1.75 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L21) |
| `AttackAnimationPoint` | 0.3 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L22) |
| `AttackAcquisitionRange` | 800 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L23) |
| `AttackRange` | 100 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L24) |
| `BountyXP` | 0 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L26) |
| `BountyGoldMin` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L27) |
| `BountyGoldMax` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L28) |
| `MovementSpeed` | 275 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L31) |
| `MovementTurnRate` | 0.5 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L32) |
| `StatusHealth` | 530 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L34) |
| `StatusHealthRegen` | 1.0 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L35) |
| `StatusMana` | 75 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L36) |
| `StatusManaRegen` | 0.05 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L37) |
| `AttackType` | hero | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L43) |
| `ArmorType` | hero | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L44) |
| `VisionDaytimeRange` | 900 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L46) |
| `VisionNighttimeRange` | 800 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L47) |
| `ConsideredHero` | 1 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/shadow_master_shadow.txt#L49) |

Способности: Ability1: `shadow_return_to_owner`; Ability2: `shadow_shadow_mastery`; Ability3: `shadow_world_of_shadows`.

Lua: [PaintBlack.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/heroes/ShadowMaster/PaintBlack.lua#L1).

## Слабый Паук - `npc_dota_broodmother_spiderling_custom_1`

Источник: [spider_queen_spiders.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_broodmother_spiderling | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L5) |
| `ModelScale` | 0.4 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L8) |
| `Level` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L9) |
| `wearable` | 746 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L10) |
| `ArmorPhysical` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L17) |
| `MagicalResistance` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L18) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L20) |
| `AttackDamageMin` | 11 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L21) |
| `AttackDamageMax` | 12 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L22) |
| `AttackRate` | 1.05 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L23) |
| `AttackAnimationPoint` | 0.5 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L24) |
| `AttackAcquisitionRange` | 500 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L25) |
| `AttackRange` | 100 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L26) |
| `BountyXP` | 0 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L30) |
| `BountyGoldMin` | 0 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L31) |
| `BountyGoldMax` | 0 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L32) |
| `MovementSpeed` | 270 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L35) |
| `MovementTurnRate` | 0.5 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L36) |
| `StatusHealth` | 150 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L38) |
| `StatusHealthRegen` | 0.5 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L39) |
| `StatusMana` | 0 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L40) |
| `StatusManaRegen` | 0 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L41) |
| `AttackType` | normal | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L47) |
| `ArmorType` | heavy | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L48) |
| `VisionDaytimeRange` | 800 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L50) |
| `VisionNighttimeRange` | 800 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L51) |
| `HasInventory` | 0 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L53) |

## Сильный Паук - `npc_dota_broodmother_spiderling_custom_2`

Источник: [spider_queen_spiders.txt:56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L56). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_broodmother_spiderling | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L58) |
| `ModelScale` | 0.5 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L61) |
| `Level` | 2 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L62) |
| `wearable` | 746 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L63) |
| `ArmorPhysical` | 0 | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L70) |
| `MagicalResistance` | 0 | [71](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L71) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L73) |
| `AttackDamageMin` | 26 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L74) |
| `AttackDamageMax` | 27 | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L75) |
| `AttackRate` | 0.75 | [76](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L76) |
| `AttackAnimationPoint` | 0.5 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L77) |
| `AttackAcquisitionRange` | 500 | [78](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L78) |
| `AttackRange` | 100 | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L79) |
| `BountyXP` | 0 | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L83) |
| `BountyGoldMin` | 0 | [84](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L84) |
| `BountyGoldMax` | 0 | [85](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L85) |
| `MovementSpeed` | 270 | [88](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L88) |
| `MovementTurnRate` | 0.5 | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L89) |
| `StatusHealth` | 350 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L91) |
| `StatusHealthRegen` | 1 | [92](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L92) |
| `StatusMana` | 0 | [93](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L93) |
| `StatusManaRegen` | 0 | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L94) |
| `AttackType` | normal | [100](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L100) |
| `ArmorType` | heavy | [101](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L101) |
| `VisionDaytimeRange` | 800 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L103) |
| `VisionNighttimeRange` | 800 | [104](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L104) |
| `HasInventory` | 0 | [106](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L106) |

## Паук-Воин - `npc_dota_broodmother_spiderling_custom_3`

Источник: [spider_queen_spiders.txt:109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L109). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_broodmother_spiderling | [111](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L111) |
| `ModelScale` | 0.6 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L114) |
| `Level` | 3 | [115](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L115) |
| `wearable` | 746 | [116](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L116) |
| `ArmorPhysical` | 0 | [123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L123) |
| `MagicalResistance` | 0 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L124) |
| `AttackCapabilities` | DOTA_UNIT_CAP_MELEE_ATTACK | [126](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L126) |
| `AttackDamageMin` | 51 | [127](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L127) |
| `AttackDamageMax` | 52 | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L128) |
| `AttackRate` | 0.55 | [129](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L129) |
| `AttackAnimationPoint` | 0.5 | [130](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L130) |
| `AttackAcquisitionRange` | 500 | [131](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L131) |
| `AttackRange` | 100 | [132](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L132) |
| `BountyXP` | 0 | [136](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L136) |
| `BountyGoldMin` | 0 | [137](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L137) |
| `BountyGoldMax` | 0 | [138](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L138) |
| `MovementSpeed` | 270 | [141](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L141) |
| `MovementTurnRate` | 0.5 | [142](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L142) |
| `StatusHealth` | 550 | [144](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L144) |
| `StatusHealthRegen` | 1.5 | [145](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L145) |
| `StatusMana` | 0 | [146](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L146) |
| `StatusManaRegen` | 0 | [147](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L147) |
| `AttackType` | normal | [153](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L153) |
| `ArmorType` | heavy | [154](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L154) |
| `VisionDaytimeRange` | 800 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L156) |
| `VisionNighttimeRange` | 800 | [157](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L157) |
| `HasInventory` | 0 | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/spider_queen_spiders.txt#L159) |

## Дух Равнин - `spirit_of_the_plains1`

Источник: [wanderer_creeps.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L3). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L5) |
| `ModelScale` | 0.8 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L20) |
| `Level` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L21) |
| `ArmorPhysical` | 2 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L32) |
| `MagicalResistance` | 0 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L33) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L35) |
| `AttackDamageMin` | 20 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L36) |
| `AttackDamageMax` | 30 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L37) |
| `AttackRate` | 1.0 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L38) |
| `AttackAnimationPoint` | 0.33 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L39) |
| `AttackAcquisitionRange` | 500 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L40) |
| `AttackRange` | 450 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L41) |
| `ProjectileSpeed` | 1200 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L43) |
| `BountyXP` | 0 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L45) |
| `BountyGoldMin` | 0 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L46) |
| `BountyGoldMax` | 0 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L47) |
| `MovementSpeed` | 320 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L50) |
| `MovementTurnRate` | 0.6 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L51) |
| `StatusHealth` | 500 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L53) |
| `StatusHealthRegen` | 1.6 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L54) |
| `StatusMana` | 100 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L55) |
| `StatusManaRegen` | 2 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L56) |
| `AttackType` | normal | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L62) |
| `ArmorType` | heavy | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L63) |
| `VisionDaytimeRange` | 900 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L65) |
| `VisionNighttimeRange` | 800 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L66) |
| `HasInventory` | 0 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L68) |

Способности: Ability1: `spirit_of_the_plains_fog`.

## Дух Равнин - `spirit_of_the_plains2`

Источник: [wanderer_creeps.txt:71](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L71). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L73) |
| `ModelScale` | 0.8 | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L89) |
| `Level` | 1 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L90) |
| `ArmorPhysical` | 6 | [107](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L107) |
| `MagicalResistance` | 0 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L108) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [110](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L110) |
| `AttackDamageMin` | 40 | [111](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L111) |
| `AttackDamageMax` | 50 | [112](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L112) |
| `AttackRate` | 0.8 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L113) |
| `AttackAnimationPoint` | 0.33 | [114](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L114) |
| `AttackAcquisitionRange` | 500 | [115](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L115) |
| `AttackRange` | 450 | [116](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L116) |
| `ProjectileSpeed` | 1200 | [118](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L118) |
| `BountyXP` | 0 | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L120) |
| `BountyGoldMin` | 0 | [121](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L121) |
| `BountyGoldMax` | 0 | [122](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L122) |
| `MovementSpeed` | 320 | [125](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L125) |
| `MovementTurnRate` | 0.6 | [126](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L126) |
| `StatusHealth` | 800 | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L128) |
| `StatusHealthRegen` | 1.6 | [129](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L129) |
| `StatusMana` | 150 | [130](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L130) |
| `StatusManaRegen` | 2 | [131](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L131) |
| `AttackType` | normal | [137](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L137) |
| `ArmorType` | heavy | [138](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L138) |
| `VisionDaytimeRange` | 900 | [140](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L140) |
| `VisionNighttimeRange` | 800 | [141](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L141) |
| `HasInventory` | 0 | [143](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L143) |

Способности: Ability1: `spirit_of_the_plains_fog`; Ability2: `neutral_spell_immunity`.

## Дух Равнин - `spirit_of_the_plains3`

Источник: [wanderer_creeps.txt:146](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L146). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [148](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L148) |
| `ModelScale` | 0.8 | [162](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L162) |
| `Level` | 1 | [163](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L163) |
| `ArmorPhysical` | 10 | [180](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L180) |
| `MagicalResistance` | 0 | [181](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L181) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [183](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L183) |
| `AttackDamageMin` | 90 | [184](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L184) |
| `AttackDamageMax` | 110 | [185](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L185) |
| `AttackRate` | 0.5 | [186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L186) |
| `AttackAnimationPoint` | 0.33 | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L187) |
| `AttackAcquisitionRange` | 500 | [188](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L188) |
| `AttackRange` | 450 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L189) |
| `ProjectileSpeed` | 1200 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L191) |
| `BountyXP` | 0 | [193](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L193) |
| `BountyGoldMin` | 0 | [194](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L194) |
| `BountyGoldMax` | 0 | [195](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L195) |
| `MovementSpeed` | 320 | [198](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L198) |
| `MovementTurnRate` | 0.6 | [199](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L199) |
| `StatusHealth` | 1100 | [201](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L201) |
| `StatusHealthRegen` | 2.0 | [202](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L202) |
| `StatusMana` | 200 | [203](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L203) |
| `StatusManaRegen` | 2 | [204](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L204) |
| `AttackType` | normal | [210](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L210) |
| `ArmorType` | heavy | [211](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L211) |
| `VisionDaytimeRange` | 900 | [213](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L213) |
| `VisionNighttimeRange` | 800 | [214](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L214) |
| `HasInventory` | 0 | [216](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L216) |

Способности: Ability1: `spirit_of_the_plains_fog`; Ability2: `neutral_spell_immunity`; Ability3: `spirit_of_the_plains_slow`.

## Призрак - `ghost_1`

Источник: [wanderer_creeps.txt:221](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L221). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [223](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L223) |
| `ModelScale` | 1 | [227](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L227) |
| `Level` | 1 | [228](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L228) |
| `ArmorPhysical` | 7 | [262](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L262) |
| `MagicalResistance` | 0 | [263](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L263) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [265](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L265) |
| `AttackDamageMin` | 130 | [266](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L266) |
| `AttackDamageMax` | 140 | [267](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L267) |
| `AttackRate` | 0.9 | [268](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L268) |
| `AttackAnimationPoint` | 0.6 | [269](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L269) |
| `AttackAcquisitionRange` | 500 | [270](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L270) |
| `AttackRange` | 475 | [271](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L271) |
| `ProjectileSpeed` | 2500 | [273](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L273) |
| `BountyXP` | 0 | [275](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L275) |
| `BountyGoldMin` | 0 | [276](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L276) |
| `BountyGoldMax` | 0 | [277](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L277) |
| `MovementSpeed` | 300 | [280](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L280) |
| `MovementTurnRate` | 0.6 | [281](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L281) |
| `StatusHealth` | 1000 | [283](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L283) |
| `StatusHealthRegen` | 1.0 | [284](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L284) |
| `StatusMana` | 0 | [285](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L285) |
| `StatusManaRegen` | 0 | [286](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L286) |
| `AttackType` | chaos | [292](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L292) |
| `ArmorType` | heavy | [293](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L293) |
| `VisionDaytimeRange` | 900 | [295](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L295) |
| `VisionNighttimeRange` | 800 | [296](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L296) |
| `HasInventory` | 0 | [298](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L298) |

Способности: Ability1: `essence_of_the_ghost_aura1`.

## Призрак - `ghost_2`

Источник: [wanderer_creeps.txt:301](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L301). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [303](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L303) |
| `ModelScale` | 1 | [306](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L306) |
| `Level` | 1 | [307](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L307) |
| `ArmorPhysical` | 10 | [340](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L340) |
| `MagicalResistance` | 0 | [341](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L341) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [343](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L343) |
| `AttackDamageMin` | 180 | [344](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L344) |
| `AttackDamageMax` | 190 | [345](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L345) |
| `AttackRate` | 1.0 | [346](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L346) |
| `AttackAnimationPoint` | 0.6 | [347](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L347) |
| `AttackAcquisitionRange` | 500 | [348](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L348) |
| `AttackRange` | 475 | [349](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L349) |
| `ProjectileSpeed` | 2500 | [351](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L351) |
| `BountyXP` | 0 | [353](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L353) |
| `BountyGoldMin` | 0 | [354](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L354) |
| `BountyGoldMax` | 0 | [355](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L355) |
| `MovementSpeed` | 300 | [358](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L358) |
| `MovementTurnRate` | 0.6 | [359](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L359) |
| `StatusHealth` | 1500 | [361](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L361) |
| `StatusHealthRegen` | 1.0 | [362](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L362) |
| `StatusMana` | 0 | [363](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L363) |
| `StatusManaRegen` | 0 | [364](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L364) |
| `AttackType` | chaos | [370](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L370) |
| `ArmorType` | heavy | [371](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L371) |
| `VisionDaytimeRange` | 900 | [373](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L373) |
| `VisionNighttimeRange` | 800 | [374](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L374) |
| `HasInventory` | 0 | [376](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L376) |

Способности: Ability1: `essence_of_the_ghost_aura2`.

## Призрак - `ghost_3`

Источник: [wanderer_creeps.txt:379](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L379). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [381](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L381) |
| `ModelScale` | 1 | [385](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L385) |
| `Level` | 1 | [386](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L386) |
| `ArmorPhysical` | 15 | [420](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L420) |
| `MagicalResistance` | 0 | [421](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L421) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [423](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L423) |
| `AttackDamageMin` | 250 | [424](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L424) |
| `AttackDamageMax` | 260 | [425](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L425) |
| `AttackRate` | 1.1 | [426](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L426) |
| `AttackAnimationPoint` | 0.6 | [427](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L427) |
| `AttackAcquisitionRange` | 500 | [428](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L428) |
| `AttackRange` | 475 | [429](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L429) |
| `ProjectileSpeed` | 2500 | [431](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L431) |
| `BountyXP` | 0 | [433](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L433) |
| `BountyGoldMin` | 0 | [434](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L434) |
| `BountyGoldMax` | 0 | [435](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L435) |
| `MovementSpeed` | 400 | [438](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L438) |
| `MovementTurnRate` | 0.6 | [439](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L439) |
| `StatusHealth` | 2000 | [441](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L441) |
| `StatusHealthRegen` | 2.0 | [442](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L442) |
| `StatusMana` | 0 | [443](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L443) |
| `StatusManaRegen` | 0 | [444](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L444) |
| `AttackType` | chaos | [450](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L450) |
| `ArmorType` | heavy | [451](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L451) |
| `VisionDaytimeRange` | 900 | [453](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L453) |
| `VisionNighttimeRange` | 800 | [454](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L454) |
| `HasInventory` | 0 | [456](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L456) |

Способности: Ability1: `essence_of_the_ghost_aura3`.

## Призрак - `ghost_4`

Источник: [wanderer_creeps.txt:460](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L460). Включен engine-root: `True`.

Предварительная роль по ID/пути: `summon_or_other_requires_reference_review`. Конкретные spawn-вызовы проверяются отдельно.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | npc_dota_creature | [462](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L462) |
| `ModelScale` | 1 | [466](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L466) |
| `Level` | 1 | [467](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L467) |
| `ArmorPhysical` | 25 | [501](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L501) |
| `MagicalResistance` | 0 | [502](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L502) |
| `AttackCapabilities` | DOTA_UNIT_CAP_RANGED_ATTACK | [504](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L504) |
| `AttackDamageMin` | 320 | [505](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L505) |
| `AttackDamageMax` | 330 | [506](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L506) |
| `AttackRate` | 1.5 | [507](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L507) |
| `AttackAnimationPoint` | 0.6 | [508](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L508) |
| `AttackAcquisitionRange` | 500 | [509](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L509) |
| `AttackRange` | 475 | [510](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L510) |
| `ProjectileSpeed` | 2500 | [512](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L512) |
| `BountyXP` | 0 | [514](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L514) |
| `BountyGoldMin` | 0 | [515](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L515) |
| `BountyGoldMax` | 0 | [516](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L516) |
| `MovementSpeed` | 450 | [519](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L519) |
| `MovementTurnRate` | 0.6 | [520](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L520) |
| `StatusHealth` | 2500 | [522](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L522) |
| `StatusHealthRegen` | 10.0 | [523](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L523) |
| `StatusMana` | 0 | [524](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L524) |
| `StatusManaRegen` | 0 | [525](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L525) |
| `AttackType` | chaos | [531](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L531) |
| `ArmorType` | heavy | [532](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L532) |
| `VisionDaytimeRange` | 900 | [534](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L534) |
| `VisionNighttimeRange` | 800 | [535](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L535) |
| `HasInventory` | 0 | [537](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/units/wanderer_creeps.txt#L537) |

Способности: Ability1: `essence_of_the_ghost_aura4`.
