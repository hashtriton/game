# Dota LiA: Предметы и рецепты

Снимок `012fab34e8c84ad0aa73cd4eadde1736e3c9df29`. Все 185 деклараций. Это статические определения; включение KV не доказывает доступность в матче. Отсутствующие поля наследуются из Dota или остаются неразрешенными.

Значения через `/` сохраняют порядок вектора по уровням. `MaxLevel` не дополняется предположениями. Полные вложенные modifiers/events/actions, включая повторяющиеся ключи, находятся в JSON и `kv_scalars.csv`. Имена из локализации используются как подписи, не как доказательство механики.

## Рецепт - `item_recipe_lia_alanith_spear`

Источник: [item_lia_alanith_spear.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=180`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L8) |
| `ItemCost` | 180 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L10) |
| `ItemKillable` | 0 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L13) |
| `ItemRecipe` | 1 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L15) |
| `ItemResult` | item_lia_alanith_spear | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L17) |

## Копьё Аланита - `item_lia_alanith_spear`

Источник: [item_lia_alanith_spear.txt:24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L24). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=490`.

Рецепт `item_recipe_lia_alanith_spear`: item_lia_spear + item_lia_mask_of_death; свиток 180; сумма объявленных цен 490, цена результата 490. [item_lia_alanith_spear.txt:20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L20).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L27) |
| `ItemCost` | 490 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L30) |
| `ItemKillable` | 0 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L31) |
| `ItemDroppable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L32) |
| `ItemSellable` | 1 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L33) |
| `ItemPurchasable` | 1 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L34) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE \| DOTA_ABILITY_BEHAVIOR_ATTACK | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L38) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 25 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L42) |
| `lifesteal_percent` | 17 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_alanith_spear.txt#L43) |

Lua: [item_lia_alanith_spear.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_alanith_spear.lua#L1).

## Амулет - `item_lia_amulet`

Источник: [item_lia_amulet.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=120`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L7) |
| `ItemCost` | 120 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L11) |
| `ItemKillable` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L16) |
| `ItemDroppable` | 1 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L17) |
| `ItemSellable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L19) |
| `ItemPurchasable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L20) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_health` | 100 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L24) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_amulet[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_BONUS[1]` | %bonus_health | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet.txt#L37) |

## Амулет Защиты - `item_lia_amulet_of_spell_shield`

Источник: [item_lia_amulet_of_spell_shield.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=300`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L6) |
| `ItemCost` | 300 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L8) |
| `ItemKillable` | 0 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L9) |
| `ItemDroppable` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L10) |
| `ItemSellable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L12) |
| `ItemPurchasable` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L13) |
| `AbilityCooldown` | 30.0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L16) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L18) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `block_cooldown` | 30.0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_amulet_of_spell_shield.txt#L22) |

Lua: [SpellShield.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/SpellShield.lua#L1); [SpellShield.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/SpellShield.lua#L1); [SpellShield.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/SpellShield.lua#L1).

## Древняя Перчатка - `item_lia_ancient_glove`

Источник: [item_lia_ancient_glove.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=150`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L7) |
| `ItemCost` | 150 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L11) |
| `ItemKillable` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L16) |
| `ItemDroppable` | 1 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L17) |
| `ItemSellable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L19) |
| `ItemPurchasable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L20) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `crit_chance` | 15 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L24) |
| `crit_mult` | 150 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L25) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_ancient_glove_crit[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_CRITICALSTRIKE[1]` | %crit_mult | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ancient_glove.txt#L63) |

## Антимагический Эликсир - `item_lia_anti_magic_potion`

Источник: [item_lia_anti_magic_potion.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=80`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_DONT_RESUME_ATTACK | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L8) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L9) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L10) |
| `AbilityCooldown` | 18 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L12) |
| `ItemKillable` | 0 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L14) |
| `ItemSellable` | 1 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L15) |
| `ItemDroppable` | 1 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L16) |
| `ItemPermanent` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L17) |
| `ItemCost` | 80 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L19) |
| `ItemStackable` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L22) |
| `ItemInitialCharges` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L24) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 6 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L28) |

Lua: [AntimagicPotion.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/AntimagicPotion.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_anti_magic_potion[1]/Properties[1]/MODIFIER_PROPERTY_MAGICAL_RESISTANCE_BONUS[1]` | 100 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L61) |
| `Modifiers[1]/modifier_item_lia_anti_magic_potion[1]/States[1]/MODIFIER_STATE_MAGIC_IMMUNE[1]` | MODIFIER_STATE_VALUE_ENABLED | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_anti_magic_potion.txt#L66) |

## Рецепт - `item_recipe_lia_armor_of_the_red_mist`

Источник: [item_lia_armor_of_the_red_mist.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=450`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L7) |
| `ItemCost` | 450 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L9) |
| `ItemKillable` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L11) |
| `ItemRecipe` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L13) |
| `ItemResult` | item_lia_armor_of_the_red_mist | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L15) |

## Доспехи Красного Тумана - `item_lia_armor_of_the_red_mist`

Источник: [item_lia_armor_of_the_red_mist.txt:22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L22). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=2555`.

Рецепт `item_recipe_lia_armor_of_the_red_mist`: item_lia_shield_of_death + item_lia_mithril_armor; свиток 450; сумма объявленных цен 2555, цена результата 2555. [item_lia_armor_of_the_red_mist.txt:18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L18).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L24) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_DONT_RESUME_MOVEMENT | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L27) |
| `ItemKillable` | 0 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L32) |
| `AbilityCooldown` | 14.0 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L41) |
| `AbilityManaCost` | 120 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L42) |
| `AbilityCastRange` | 400 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L44) |
| `ItemCost` | 2555 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L46) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 20 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L53) |
| `bonus_health` | 700 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L54) |
| `damage_blocked` | 60 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L55) |
| `bonus_magic_resist_percentage` | 30 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L56) |
| `damage` | 400 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L57) |
| `active_armor` | 25 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L58) |
| `radius` | 400 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L59) |
| `duration` | 5.0 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L60) |
| `duration_hero_tooltip` | 2.5 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_armor_of_the_red_mist.txt#L61) |

Lua: [item_lia_armor_of_the_red_mist.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_armor_of_the_red_mist.lua#L1).

## Топор - `item_lia_axe`

Источник: [item_lia_axe.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=85`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L7) |
| `ItemCost` | 85 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L11) |
| `ItemKillable` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L16) |
| `ItemDroppable` | 1 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L17) |
| `ItemSellable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L19) |
| `ItemPurchasable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L20) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_strength` | 6 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L24) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_axe[1]/Properties[1]/MODIFIER_PROPERTY_STATS_STRENGTH_BONUS[1]` | %bonus_strength | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_axe.txt#L37) |

## Рецепт - `item_recipe_lia_banner_of_victory`

Источник: [item_lia_banner_of_victory.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=320`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L7) |
| `ItemCost` | 320 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L9) |
| `ItemKillable` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L11) |
| `ItemRecipe` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L13) |
| `ItemResult` | item_lia_banner_of_victory | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L15) |

## Знамя Победы - `item_lia_banner_of_victory`

Источник: [item_lia_banner_of_victory.txt:22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L22). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=715`.

Рецепт `item_recipe_lia_banner_of_victory`: item_lia_helm + item_lia_mask_of_death; свиток 320; сумма объявленных цен 715, цена результата 715. [item_lia_banner_of_victory.txt:18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L18).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L25) |
| `ItemCost` | 715 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE \| DOTA_ABILITY_BEHAVIOR_AURA | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `aura_radius` | 800 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L39) |
| `aura_lifesteal_percent` | 12 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L40) |
| `aura_armor` | 5 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_banner_of_victory.txt#L41) |

Lua: [item_lia_banner_of_victory.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_banner_of_victory.lua#L1).

## Рецепт - `item_recipe_lia_battle_axe`

Источник: [item_lia_battle_axe.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=220`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L7) |
| `ItemCost` | 220 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L12) |
| `ItemRecipe` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L13) |
| `ItemResult` | item_lia_battle_axe | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L15) |

## Боевой Топор - `item_lia_battle_axe`

Источник: [item_lia_battle_axe.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=585`.

Рецепт `item_recipe_lia_battle_axe`: item_lia_ring_of_protection + item_lia_axe + item_lia_gloves_of_strength; свиток 220; сумма объявленных цен 585, цена результата 585. [item_lia_battle_axe.txt:18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L18).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L25) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L27) |
| `ItemCost` | 585 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L32) |
| `ItemKillable` | 0 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L36) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 16 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L40) |
| `bonus_strength` | 18 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L41) |
| `bonus_armor` | 4 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L42) |
| `bash_chance` | 16 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L43) |
| `bash_damage` | 80 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L44) |
| `bash_stun` | 1.25 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L45) |

Lua: [BattleAxe.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/BattleAxe.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_battle_axe[1]/Properties[1]/MODIFIER_PROPERTY_STATS_STRENGTH_BONUS[1]` | %bonus_strength | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L59) |
| `Modifiers[1]/modifier_item_lia_battle_axe[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_axe.txt#L60) |

## Рецепт - `item_recipe_lia_battle_javelin`

Источник: [item_lia_battle_javelin.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=415`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ID` | 1367 | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L5) |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L7) |
| `ItemCost` | 415 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L9) |
| `ItemKillable` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L11) |
| `ItemRecipe` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L13) |
| `ItemResult` | item_lia_battle_javelin | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L15) |

## Боевой Дротик - `item_lia_battle_javelin`

Источник: [item_lia_battle_javelin.txt:22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L22). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=800`.

Рецепт `item_recipe_lia_battle_javelin`: item_lia_runed_gloves + item_lia_spear + item_lia_gloves_of_haste; свиток 415; сумма объявленных цен 800, цена результата 800. [item_lia_battle_javelin.txt:18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L18).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L25) |
| `ItemCost` | 800 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 45 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L39) |
| `bonus_attack_speed` | 35 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L40) |
| `pierce_chance` | 15 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_battle_javelin.txt#L41) |

Lua: [item_lia_battle_javelin.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_battle_javelin.lua#L1).

## Рецепт - `item_recipe_lia_blade_of_incorporeality`

Источник: [item_lia_blade_of_incorporeality.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=500`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L7) |
| `ItemCost` | 500 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L9) |
| `ItemKillable` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L11) |
| `ItemRecipe` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L13) |
| `ItemResult` | item_lia_blade_of_incorporeality | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L15) |

## Клинок Бестелесности - `item_lia_blade_of_incorporeality`

Источник: [item_lia_blade_of_incorporeality.txt:22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L22). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1495`.

Рецепт `item_recipe_lia_blade_of_incorporeality`: item_lia_pantilus_blade + item_lia_magic_helm; свиток 500; сумма объявленных цен 1495, цена результата 1495. [item_lia_blade_of_incorporeality.txt:18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L18).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L25) |
| `ItemCost` | 1495 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L35) |
| `AbilityCooldown` | 16 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L37) |
| `AbilityManaCost` | 80 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L38) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 35 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L47) |
| `bonus_attack_speed` | 40 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L48) |
| `bonus_agility` | 20 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L49) |
| `bonus_armor` | 10 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L50) |
| `evasion` | 15 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L51) |
| `active_evasion` | 75 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L52) |
| `active_duration` | 4 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blade_of_incorporeality.txt#L53) |

Lua: [item_lia_blade_of_incorporeality.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_blade_of_incorporeality.lua#L1).

## Рецепт - `item_recipe_lia_blood_blade`

Источник: [item_lia_blood_blade.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=365`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L7) |
| `ItemCost` | 365 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L14) |
| `ItemResult` | item_lia_blood_blade | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L16) |

## Меч Крови - `item_lia_blood_blade`

Источник: [item_lia_blood_blade.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=900`.

Рецепт `item_recipe_lia_blood_blade`: item_lia_alanith_spear + item_lia_steel_sword; свиток 365; сумма объявленных цен 900, цена результата 900. [item_lia_blood_blade.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L27) |
| `ItemCost` | 900 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L30) |
| `ItemKillable` | 0 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L31) |
| `ItemDroppable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L32) |
| `ItemSellable` | 1 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L33) |
| `ItemPurchasable` | 1 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L34) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_ATTACK | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L38) |
| `AbilityManaCost` | 100 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L39) |
| `AbilityCooldown` | 13 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L40) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 70 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L50) |
| `lifesteal_percent` | 22 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L51) |
| `healtsteal_percent` | 8 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L52) |
| `healtsteal_radius` | 525 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L53) |
| `healtsteal_limit` | 1500 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L54) |
| `healtsteal_max_damage` | 250 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_blade.txt#L55) |

Lua: [item_lia_blood_blade.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_blood_blade.lua#L1).

## Рецепт - `item_recipe_lia_blood_moon`

Источник: [item_lia_blood_moon.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=750`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L7) |
| `ItemCost` | 750 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L14) |
| `ItemResult` | item_lia_blood_moon | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L16) |

## Кровавая Луна - `item_lia_blood_moon`

Источник: [item_lia_blood_moon.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=2135`.

Рецепт `item_recipe_lia_blood_moon`: item_lia_huge_axe + item_lia_hell_gloves; свиток 750; сумма объявленных цен 2135, цена результата 2135. [item_lia_blood_moon.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L26) |
| `ItemCost` | 2135 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L36) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 100 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L40) |
| `bonus_strength` | 50 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L41) |
| `bonus_attack_speed` | 40 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L42) |
| `cleave_percent` | 30 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L43) |
| `cleave_start_width` | 150 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L44) |
| `cleave_end_width` | 200 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L45) |
| `cleave_length` | 200 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_blood_moon.txt#L46) |

Lua: [item_lia_blood_moon.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_blood_moon.lua#L1).

## имя не найдено - `item_lia_boar`

Источник: [item_lia_boar.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=75`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L7) |
| `ItemCost` | 75 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L14) |
| `ItemPermanent` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L19) |
| `AbilityCooldown` | 20 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L20) |
| `ItemKillable` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L21) |
| `ItemSellable` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L22) |
| `ItemDroppable` | 1 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L23) |
| `ItemInitialCharges` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L24) |
| `ItemStackable` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L25) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 35 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boar.txt#L40) |

Lua: [Boar.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/Boar.lua#L1).

## Книга Мёртвых - `item_lia_book_of_the_dead`

Источник: [item_lia_book_of_the_dead.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=40`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L7) |
| `ItemStockMax` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L10) |
| `ItemStockTime` | 20 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L11) |
| `ItemStockInitial` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L12) |
| `ItemCost` | 40 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L14) |
| `ItemPermanent` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L19) |
| `AbilityCastRange` | 400 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L20) |
| `AbilityCooldown` | 20 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L21) |
| `AbilityManaCost` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L22) |
| `ItemKillable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L23) |
| `ItemSellable` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L24) |
| `ItemDroppable` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L25) |
| `ItemInitialCharges` | 1 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L26) |
| `ItemStackable` | 1 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L27) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 30 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_book_of_the_dead.txt#L41) |

Lua: [BookOfTheDeath.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/BookOfTheDeath.lua#L1).

## Сапоги - `item_lia_boots`

Источник: [item_lia_boots.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=90`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 90 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L18) |
| `BaseClass` | item_datadriven | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L19) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_movement_speed` | 50 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L23) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_boots[1]/Properties[1]/MODIFIER_PROPERTY_MOVESPEED_BONUS_UNIQUE[1]` | %bonus_movement_speed | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots.txt#L36) |

## Рецепт - `item_recipe_lia_boots_of_invisibility`

Источник: [item_lia_boots_of_invisibility.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=300`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L7) |
| `ItemCost` | 300 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L14) |
| `ItemResult` | item_lia_boots_of_invisibility | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L16) |

## Сапоги-Невидимки - `item_lia_boots_of_invisibility`

Источник: [item_lia_boots_of_invisibility.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=390`.

Рецепт `item_recipe_lia_boots_of_invisibility`: item_lia_boots; свиток 300; сумма объявленных цен 390, цена результата 390. [item_lia_boots_of_invisibility.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L26) |
| `ItemCost` | 390 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING \| DOTA_ABILITY_BEHAVIOR_IGNORE_CHANNEL | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L36) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_PHYSICAL | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L37) |
| `AbilityCastPoint` | 0 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L39) |
| `AbilityCooldown` | 18.0 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L40) |
| `AbilityManaCost` | 70 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L42) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_movement_speed` | 50 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L52) |
| `duration` | 6 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L53) |
| `invis_movespeed_percent` | 10 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L54) |
| `invis_damage` | 50 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_boots_of_invisibility.txt#L55) |

Lua: [item_lia_boots_of_invisibility.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_boots_of_invisibility.lua#L1).

## Рецепт - `item_recipe_lia_bounty_hunters_crossbow`

Источник: [item_lia_bounty_hunters_crossbow.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=650`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L7) |
| `ItemCost` | 650 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L9) |
| `ItemKillable` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L11) |
| `ItemRecipe` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L13) |
| `ItemResult` | item_lia_bounty_hunters_crossbow | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L15) |

## Арбалет Охотника за Головами - `item_lia_bounty_hunters_crossbow`

Источник: [item_lia_bounty_hunters_crossbow.txt:22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L22). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1900`.

Рецепт `item_recipe_lia_bounty_hunters_crossbow`: item_lia_battle_javelin + item_lia_demon_edge; свиток 650; сумма объявленных цен 2000, цена результата 1900. [item_lia_bounty_hunters_crossbow.txt:18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L18).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L25) |
| `ItemCost` | 1900 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L35) |
| `AbilityCooldown` | 16 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L37) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 100 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L46) |
| `bonus_attack_speed` | 35 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L47) |
| `pierce_chance` | 25 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L48) |
| `active_pierce_chance` | 50 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L49) |
| `active_duration` | 5 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_bounty_hunters_crossbow.txt#L50) |

Lua: [item_lia_bounty_hunters_crossbow.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_bounty_hunters_crossbow.lua#L1).

## Когти - `item_lia_claws`

Источник: [item_lia_claws.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=65`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L7) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L12) |
| `ItemDroppable` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L13) |
| `ItemSellable` | 1 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L15) |
| `ItemPurchasable` | 1 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L16) |
| `ItemCost` | 65 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L19) |
| `SideShop` | 1 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L23) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 16 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L27) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_claws[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_claws.txt#L40) |

## Рецепт - `item_recipe_lia_crown_of_death`

Источник: [item_lia_crown_of_death.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=200`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L7) |
| `ItemCost` | 200 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L14) |
| `ItemResult` | item_lia_crown_of_death | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L16) |

## Корона Смерти - `item_lia_crown_of_death`

Источник: [item_lia_crown_of_death.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=540`.

Рецепт `item_recipe_lia_crown_of_death`: item_lia_runed_bracers + item_lia_amulet; свиток 200; сумма объявленных цен 540, цена результата 540. [item_lia_crown_of_death.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L26) |
| `ItemCost` | 540 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L35) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L36) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L37) |
| `AbilityCastRange` | 500 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L39) |
| `AbilityCastPoint` | 0.0 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L40) |
| `AbilityCooldown` | 12.0 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L41) |
| `AbilityManaCost` | 80 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L42) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_magic_resist_percentage` | 25 | [72](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L72) |
| `bonus_health` | 200 | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L73) |
| `damage` | 275 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L74) |
| `range_tooltip` | 500 | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L75) |

Lua: [item_crown_of_death.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_crown_of_death.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_crown_of_death[1]/Properties[1]/MODIFIER_PROPERTY_MAGICAL_RESISTANCE_BONUS[1]` | %bonus_magic_resist_percentage | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L65) |
| `Modifiers[1]/modifier_item_lia_crown_of_death[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_BONUS[1]` | %bonus_health | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crown_of_death.txt#L66) |

## Рецепт - `item_recipe_lia_crusher`

Источник: [item_lia_crusher.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=650`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L7) |
| `ItemCost` | 650 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L14) |
| `ItemResult` | item_lia_crusher | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L16) |

## Сокрушитель - `item_lia_crusher`

Источник: [item_lia_crusher.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=2050`.

Рецепт `item_recipe_lia_crusher`: item_lia_pantilus_blade + item_lia_lightning_spear; свиток 650; сумма объявленных цен 2050, цена результата 2050. [item_lia_crusher.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L26) |
| `ItemCost` | 2050 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L36) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L37) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L38) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L39) |
| `AbilityCastRange` | 700 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L41) |
| `AbilityCooldown` | 8.0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L42) |
| `AbilityManaCost` | 100 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L43) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 75 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L54) |
| `bonus_attack_speed` | 45 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L55) |
| `bonus_agility` | 25 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L56) |
| `lightning_chance` | 22 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L57) |
| `crit_mult` | 190 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L58) |
| `lightning_damage` | 400 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L59) |
| `lightning_bounces` | 8 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L60) |
| `bounce_range` | 700 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L61) |
| `lightning_decay` | 0 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L62) |
| `time_between_bounces` | 0.2 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L63) |
| `lightning_cooldown` | 4 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L64) |

Lua: [Crusher.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/Crusher.lua#L1); [Crusher.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/Crusher.lua#L1); [Crusher.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/Crusher.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_crusher[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L86) |
| `Modifiers[1]/modifier_item_lia_crusher[1]/Properties[1]/MODIFIER_PROPERTY_STATS_AGILITY_BONUS[1]` | %bonus_agility | [87](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L87) |
| `Modifiers[1]/modifier_item_lia_crusher[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %bonus_attack_speed | [88](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L88) |
| `Modifiers[1]/modifier_item_lia_crusher_accuracy[1]/States[1]/MODIFIER_STATE_CANNOT_MISS[1]` | MODIFIER_STATE_VALUE_ENABLED | [135](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L135) |
| `Modifiers[1]/modifier_item_lia_crusher_accuracy[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_CRITICALSTRIKE[1]` | %crit_mult | [140](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_crusher.txt#L140) |

## Рецепт - `item_recipe_lia_demon_edge`

Источник: [item_lia_demon_edge.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=230`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L7) |
| `ItemCost` | 230 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L14) |
| `ItemResult` | item_lia_demon_edge | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L16) |

## Лезвие Демона - `item_lia_demon_edge`

Источник: [item_lia_demon_edge.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=550`.

Рецепт `item_recipe_lia_demon_edge`: item_lia_steel_sword + item_lia_claws + item_lia_spear; свиток 230; сумма объявленных цен 450, цена результата 550. [item_lia_demon_edge.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L26) |
| `ItemCost` | 550 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 55 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L52) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_demon_edge[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demon_edge.txt#L46) |

## Статуэтка Демона - `item_lia_demonic_figurine`

Источник: [item_lia_demonic_figure.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=150`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L7) |
| `ItemStockMax` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L10) |
| `ItemStockTime` | 30 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L11) |
| `ItemStockInitial` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L12) |
| `ItemCost` | 150 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L14) |
| `ItemPermanent` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L19) |
| `AbilityCastRange` | 400 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L20) |
| `AbilityCooldown` | 30 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L21) |
| `AbilityManaCost` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L22) |
| `ItemKillable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L23) |
| `ItemSellable` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L24) |
| `ItemDroppable` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L25) |
| `ItemInitialCharges` | 1 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L26) |
| `ItemStackable` | 1 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L27) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 60 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_demonic_figure.txt#L53) |

Lua: [SummonLocation.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/abilities/SummonLocation.lua#L1).

## Рецепт - `item_recipe_lia_divine_armor`

Источник: [item_lia_divine_armor.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=700`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L7) |
| `ItemCost` | 700 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L14) |
| `ItemResult` | item_lia_divine_armor | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L16) |

## Доспехи Бога - `item_lia_divine_armor`

Источник: [item_lia_divine_armor.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=2315`.

Рецепт `item_recipe_lia_divine_armor`: item_lia_shield_of_endurance + item_lia_dwarf_armor + item_lia_magic_helm; свиток 700; сумма объявленных цен 2315, цена результата 2315. [item_lia_divine_armor.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L26) |
| `ItemCost` | 2315 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 30 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L63) |
| `bonus_health` | 700 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L64) |
| `bonus_health_regen` | 25 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L65) |
| `evasion_percent` | 20 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L66) |

Lua: [onlyone.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/onlyone.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_divine_armor[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L54) |
| `Modifiers[1]/modifier_item_lia_divine_armor[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_BONUS[1]` | %bonus_health | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L55) |
| `Modifiers[1]/modifier_item_lia_divine_armor[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_REGEN_CONSTANT[1]` | %bonus_health_regen | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L56) |
| `Modifiers[1]/modifier_item_lia_divine_armor[1]/Properties[1]/MODIFIER_PROPERTY_EVASION_CONSTANT[1]` | %evasion_percent | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_divine_armor.txt#L57) |

## Порошок Прозрения - `item_lia_dust_of_appearance`

Источник: [item_lia_dust_of_appearance.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=45`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_dust | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_NO_TARGET | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L7) |
| `AbilityCooldown` | 14 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L9) |
| `AbilityManaCost` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L11) |
| `ItemPermanent` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L19) |
| `ItemStackable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L20) |
| `ItemKillable` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L21) |
| `ItemSellable` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L22) |
| `ItemDroppable` | 1 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L23) |
| `ItemInitialCharges` | 3 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L24) |
| `ItemAlertable` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L25) |
| `ItemCost` | 45 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L28) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 8 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L32) |
| `radius` | 1000 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L33) |
| `movespeed` | 0 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dust_of_appearance.txt#L34) |

## Рецепт - `item_recipe_lia_dwarf_armor`

Источник: [item_lia_dwarf_armor.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=200`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L7) |
| `ItemCost` | 200 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L14) |
| `ItemResult` | item_lia_dwarf_armor | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L16) |

## Гномья Броня - `item_lia_dwarf_armor`

Источник: [item_lia_dwarf_armor.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=515`.

Рецепт `item_recipe_lia_dwarf_armor`: item_lia_helm + item_lia_amulet; свиток 200; сумма объявленных цен 515, цена результата 515. [item_lia_dwarf_armor.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L26) |
| `ItemCost` | 515 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 10 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L53) |
| `bonus_health` | 175 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L54) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_dwarf_armor[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L46) |
| `Modifiers[1]/modifier_item_lia_dwarf_armor[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_BONUS[1]` | %bonus_health | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_dwarf_armor.txt#L47) |

## Рецепт - `item_recipe_lia_enchanted_shield`

Источник: [item_lia_enchanted_shield.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=275`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L7) |
| `ItemCost` | 275 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L14) |
| `ItemResult` | item_lia_enchanted_shield | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L16) |

## имя не найдено - `item_recipe_lia_enchanted_shield_2`

Источник: [item_lia_enchanted_shield.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L27) |
| `ItemCost` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L29) |
| `ItemRecipe` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L32) |
| `ItemResult` | item_lia_enchanted_shield_2 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L34) |

## Заколдованный Щит - `item_lia_enchanted_shield`

Источник: [item_lia_enchanted_shield.txt:41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L41). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=910`.

Рецепт `item_recipe_lia_enchanted_shield`: item_lia_dwarf_armor + item_lia_amulet; свиток 275; сумма объявленных цен 910, цена результата 910. [item_lia_enchanted_shield.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L43) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L46) |
| `ItemKillable` | 0 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L54) |
| `MaxUpgradeLevel` | 2 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L56) |
| `ItemBaseLevel` | 1 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L57) |
| `ItemCost` | 910 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L59) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 13 / 16 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L65) |
| `bonus_health` | 325 / 525 | [66](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L66) |
| `block_chance` | 50 | [67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L67) |
| `damage_block` | 75 / 100 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L68) |

Lua: [item_lia_enchanted_shield.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_enchanted_shield.lua#L1).

## Заколдованный Щит - `item_lia_enchanted_shield_2`

Источник: [item_lia_enchanted_shield.txt:73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L73). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=1185`.

Рецепт `item_recipe_lia_enchanted_shield_2`: item_recipe_lia_enchanted_shield + item_lia_enchanted_shield; свиток 0; сумма объявленных цен 1185, цена результата 1185. [item_lia_enchanted_shield.txt:37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L37).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L75) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [78](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L78) |
| `ItemKillable` | 0 | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L83) |
| `MaxUpgradeLevel` | 2 | [88](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L88) |
| `ItemBaseLevel` | 2 | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L89) |
| `ItemCost` | 1185 | [91](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L91) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 16 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L97) |
| `bonus_health` | 525 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L98) |
| `block_chance` | 50 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L99) |
| `damage_block` | 100 | [100](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_enchanted_shield.txt#L100) |

Lua: [item_lia_enchanted_shield.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_enchanted_shield.lua#L1).

## Рецепт - `item_recipe_lia_ferus_shield`

Источник: [item_lia_ferus_shield.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=750`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L7) |
| `ItemCost` | 750 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L14) |
| `ItemResult` | item_lia_ferus_shield | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L16) |

## Щит Феруса - `item_lia_ferus_shield`

Источник: [item_lia_ferus_shield.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=2090`.

Рецепт `item_recipe_lia_ferus_shield`: item_lia_shield_of_endurance + item_lia_banner_of_victory; свиток 750; сумма объявленных цен 2090, цена результата 2090. [item_lia_ferus_shield.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L26) |
| `ItemCost` | 2090 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_AOE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L36) |
| `AoERadius` | 350 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L37) |
| `AbilityCooldown` | 25 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L38) |
| `AbilityManacost` | 250 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L39) |
| `AbilityCastRange` | 350 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L40) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_health` | 550 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L50) |
| `aura_radius` | 800 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L51) |
| `aura_lifesteal_percent` | 15 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L52) |
| `aura_regen` | 15 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L53) |
| `aura_armor` | 13 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L54) |
| `panic_duration` | 4 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L55) |
| `panic_radius` | 350 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ferus_shield.txt#L56) |

Lua: [item_lia_ferus_shield.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_ferus_shield.lua#L1).

## Рецепт - `item_recipe_lia_fire_gloves`

Источник: [item_lia_fire_gloves.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=350`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L7) |
| `ItemCost` | 350 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L14) |
| `ItemResult` | item_lia_fire_gloves | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L16) |

## имя не найдено - `item_recipe_lia_fire_gloves_2`

Источник: [item_lia_fire_gloves.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L27) |
| `ItemCost` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L29) |
| `ItemRecipe` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L32) |
| `ItemResult` | item_lia_fire_gloves_2 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L34) |

## Перчатки Огня - `item_lia_fire_gloves`

Источник: [item_lia_fire_gloves.txt:41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L41). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=690`.

Рецепт `item_recipe_lia_fire_gloves`: item_lia_runed_gloves + item_lia_mana_stone; свиток 350; сумма объявленных цен 690, цена результата 690. [item_lia_fire_gloves.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L43) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_AOE \| DOTA_ABILITY_BEHAVIOR_TOGGLE | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L46) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L47) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L48) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L49) |
| `MaxUpgradeLevel` | 2 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L57) |
| `ItemBaseLevel` | 1 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L58) |
| `ItemKillable` | 0 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L60) |
| `AbilityCastRange` | 220 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L62) |
| `AbilityManaCost` | 20 / 30 | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L64) |
| `ItemCost` | 690 | [72](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L72) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana` | 350 / 500 | [78](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L78) |
| `bonus_damage` | 30 / 50 | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L79) |
| `bonus_attack_speed` | 30 / 45 | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L80) |
| `damage_per_second` | 80 / 100 | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L81) |
| `mana_cost_per_second` | 10 / 15 | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L82) |
| `radius` | 220 | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L83) |

Lua: [item_lia_fire_gloves.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_fire_gloves.lua#L1).

## Перчатки Огня - `item_lia_fire_gloves_2`

Источник: [item_lia_fire_gloves.txt:87](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L87). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=1040`.

Рецепт `item_recipe_lia_fire_gloves_2`: item_recipe_lia_fire_gloves + item_lia_fire_gloves; свиток 0; сумма объявленных цен 1040, цена результата 1040. [item_lia_fire_gloves.txt:37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L37).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L89) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_TOGGLE | [92](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L92) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [93](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L93) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L94) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L95) |
| `MaxUpgradeLevel` | 2 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L103) |
| `ItemBaseLevel` | 2 | [104](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L104) |
| `ItemKillable` | 0 | [106](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L106) |
| `AbilityCastRange` | 220 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L108) |
| `AbilityManaCost` | 30 | [110](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L110) |
| `ItemCost` | 1040 | [118](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L118) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana` | 500 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L124) |
| `bonus_damage` | 50 | [125](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L125) |
| `bonus_attack_speed` | 45 | [126](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L126) |
| `damage_per_second` | 100 | [127](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L127) |
| `mana_cost_per_second` | 15 | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L128) |
| `radius` | 220 | [129](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_gloves.txt#L129) |

Lua: [item_lia_fire_gloves.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_fire_gloves.lua#L1).

## Рецепт - `item_recipe_lia_fire_rod`

Источник: [item_lia_fire_rod.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=730`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L7) |
| `ItemCost` | 730 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L14) |
| `ItemResult` | item_lia_fire_rod | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L16) |

## Жезл Огня - `item_lia_fire_rod`

Источник: [item_lia_fire_rod.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1070`.

Рецепт `item_recipe_lia_fire_rod`: item_lia_orb_of_fire + item_lia_spear + item_lia_mana_stone; свиток 730; сумма объявленных цен 1070, цена результата 1070. [item_lia_fire_rod.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L26) |
| `ItemCost` | 1070 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_DIRECTIONAL \| DOTA_ABILITY_BEHAVIOR_POINT \| DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L36) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L37) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L38) |
| `SpellImmunityType` | SPELL_IMMUNITY_ENEMIES_NO | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L40) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L41) |
| `AbilityManaCost` | 400 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L42) |
| `AbilityCooldown` | 18 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L43) |
| `AbilityCastRange` | 700 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L44) |
| `AbilityCastPoint` | 0.0 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L45) |
| `AbilityDamage` | 600 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L46) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 75 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L57) |
| `bonus_mana` | 350 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L58) |
| `radius` | 180 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L59) |
| `wave_width` | 200 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L60) |
| `wave_range` | 700 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L61) |
| `wave_speed` | 900 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L62) |
| `wave_damage` | 600 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_rod.txt#L63) |

Lua: [item_lia_fire_rod.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_fire_rod.lua#L1).

## Рецепт - `item_recipe_lia_fire_sword`

Источник: [item_lia_fire_sword.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=200`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L7) |
| `ItemCost` | 200 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L14) |
| `ItemResult` | item_lia_fire_sword | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L16) |

## Огненный Меч - `item_lia_fire_sword`

Источник: [item_lia_fire_sword.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=395`.

Рецепт `item_recipe_lia_fire_sword`: item_lia_steel_sword + item_lia_ancient_glove; свиток 200; сумма объявленных цен 395, цена результата 395. [item_lia_fire_sword.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L26) |
| `ItemCost` | 395 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 16 | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L97) |
| `crit_chance` | 20 | [98](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L98) |
| `crit_mult` | 170 | [99](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L99) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_fire_sword[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L45) |
| `Modifiers[1]/modifier_item_lia_fire_sword_crit[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_CRITICALSTRIKE[1]` | %crit_mult | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_fire_sword.txt#L74) |

## Рецепт - `item_recipe_lia_ghost_blade`

Источник: [item_lia_ghost_blade.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=700`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L7) |
| `ItemCost` | 700 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L14) |
| `ItemResult` | item_lia_ghost_blade | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L16) |

## Призрачный Меч - `item_lia_ghost_blade`

Источник: [item_lia_ghost_blade.txt:24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L24). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=1805`.

Рецепт `item_recipe_lia_ghost_blade`: item_lia_pantilus_blade + item_lia_battle_axe; свиток 700; сумма объявленных цен 1805, цена результата 1805. [item_lia_ghost_blade.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L26) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L28) |
| `ItemCost` | 1805 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L33) |
| `ItemKillable` | 0 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L37) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 30 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L40) |
| `bonus_agility` | 25 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L41) |
| `bonus_strength` | 25 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L42) |
| `bonus_armor` | 6 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L43) |
| `bonus_attack_speed` | 50 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L44) |
| `bash_chance` | 18 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L45) |
| `bash_damage` | 175 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L46) |
| `bash_stun` | 1.5 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L47) |
| `crit_mult` | 175 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L48) |

Lua: [BattleAxe.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/BattleAxe.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_ghost_blade[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L61) |
| `Modifiers[1]/modifier_item_lia_ghost_blade[1]/Properties[1]/MODIFIER_PROPERTY_STATS_STRENGTH_BONUS[1]` | %bonus_strength | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L62) |
| `Modifiers[1]/modifier_item_lia_ghost_blade[1]/Properties[1]/MODIFIER_PROPERTY_STATS_AGILITY_BONUS[1]` | %bonus_agility | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L63) |
| `Modifiers[1]/modifier_item_lia_ghost_blade[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L64) |
| `Modifiers[1]/modifier_item_lia_ghost_blade[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %bonus_attack_speed | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ghost_blade.txt#L65) |

## Рецепт - `item_recipe_lia_glove_of_pain`

Источник: [item_lia_glove_of_pain.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=800`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L7) |
| `ItemCost` | 800 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L14) |
| `ItemResult` | item_lia_glove_of_pain | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L16) |

## Перчатка Боли - `item_lia_glove_of_pain`

Источник: [item_lia_glove_of_pain.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1590`.

Рецепт `item_recipe_lia_glove_of_pain`: item_lia_mana_staff + item_lia_mask; свиток 800; сумма объявленных цен 1590, цена результата 1590. [item_lia_glove_of_pain.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L26) |
| `ItemCost` | 1590 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L35) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_BOTH | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L36) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_CREEP | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L37) |
| `SpellImmunityType` | SPELL_IMMUNITY_ENEMIES_NO | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L39) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L40) |
| `AbilityCastRange` | 800 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L42) |
| `AbilityCastPoint` | 0.0 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L43) |
| `AbilityCooldown` | 25.0 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L44) |
| `AbilityManaCost` | 350 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L45) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana` | 400 | [150](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L150) |
| `bonus_intelligence` | 20 | [151](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L151) |
| `aura_radius` | 1000 | [152](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L152) |
| `aura_mana_regen` | 8 | [153](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L153) |
| `duration` | 8 | [154](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L154) |
| `damage` | 175 | [155](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L155) |
| `radius` | 275 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L156) |
| `think_interval` | 1.0 | [157](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L157) |

Lua: [GloveOfPain.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/GloveOfPain.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_glove_of_pain[1]/Properties[1]/MODIFIER_PROPERTY_MANA_BONUS[1]` | %bonus_mana | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L82) |
| `Modifiers[1]/modifier_item_lia_glove_of_pain[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L83) |
| `Modifiers[1]/modifier_item_glove_of_pain_aura[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT_UNIQUE[1]` | 8.0 | [94](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_glove_of_pain.txt#L94) |

## Перчатки Скорости - `item_lia_gloves_of_haste`

Источник: [item_lia_gloves_of_haste.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=55`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 55 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L18) |
| `BaseClass` | item_datadriven | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L19) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_speed` | 15 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L22) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_gloves_of_haste[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %bonus_attack_speed | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_haste.txt#L35) |

## Перчатки Силы - `item_lia_gloves_of_strength`

Источник: [item_lia_gloves_of_strength.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=200`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 200 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L17) |
| `BaseClass` | item_datadriven | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L18) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_strength` | 12 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L21) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_gloves_of_strength[1]/Properties[1]/MODIFIER_PROPERTY_STATS_STRENGTH_BONUS[1]` | %bonus_strength | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_gloves_of_strength.txt#L34) |

## Рецепт - `item_recipe_lia_hammer`

Источник: [item_lia_hammer.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=600`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L7) |
| `ItemCost` | 600 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L14) |
| `ItemResult` | item_lia_hammer | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L16) |

## Молот Превосходства - `item_lia_hammer`

Источник: [item_lia_hammer.txt:24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L24). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=965`.

Рецепт `item_recipe_lia_hammer`: item_lia_spear + item_lia_axe + item_lia_necklace + item_lia_mantle; свиток 600; сумма объявленных цен 965, цена результата 965. [item_lia_hammer.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L26) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L28) |
| `ItemCost` | 965 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L33) |
| `ItemKillable` | 0 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L37) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 35 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L41) |
| `bonus_strength` | 15 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L42) |
| `bonus_agility` | 15 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L43) |
| `bonus_intelligence` | 15 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L44) |
| `minibash_chance` | 30 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L45) |
| `bash_damage` | 175 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L46) |
| `bash_stun` | 0.01 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L47) |

Lua: [Hammer.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/Hammer.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_hammer[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L60) |
| `Modifiers[1]/modifier_item_lia_hammer[1]/Properties[1]/MODIFIER_PROPERTY_STATS_STRENGTH_BONUS[1]` | %bonus_strength | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L61) |
| `Modifiers[1]/modifier_item_lia_hammer[1]/Properties[1]/MODIFIER_PROPERTY_STATS_AGILITY_BONUS[1]` | %bonus_agility | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L62) |
| `Modifiers[1]/modifier_item_lia_hammer[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer.txt#L63) |

## Рецепт - `item_recipe_lia_hammer_of_titans`

Источник: [item_lia_hammer_of_titans.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=650`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L7) |
| `ItemCost` | 650 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L14) |
| `ItemResult` | item_lia_hammer_of_titans | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L16) |

## Молот Титанов - `item_lia_hammer_of_titans`

Источник: [item_lia_hammer_of_titans.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=2130`.

Рецепт `item_recipe_lia_hammer_of_titans`: item_lia_huge_axe + item_lia_hammer; свиток 650; сумма объявленных цен 2130, цена результата 2130. [item_lia_hammer_of_titans.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L26) |
| `ItemCost` | 2130 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L36) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 80 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L40) |
| `bonus_strength` | 25 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L41) |
| `bonus_agility` | 25 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L42) |
| `bonus_intelligence` | 25 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L43) |
| `minibash_chance` | 30 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L44) |
| `bash_damage` | 275 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L45) |
| `bash_stun` | 0.01 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L46) |
| `cleave_percent` | 50 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L47) |
| `splash_percent_ranged` | 20 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L48) |
| `cleave_start_width` | 150 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L49) |
| `cleave_end_width` | 200 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L50) |
| `cleave_length` | 200 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L51) |
| `splash_radius` | 200 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L52) |
| `attack_speed_slow` | -30 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L53) |
| `movement_slow_percentage` | -30 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L54) |
| `slow_duration` | 3 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hammer_of_titans.txt#L55) |

Lua: [item_lia_hammer_of_titans.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_hammer_of_titans.lua#L1).

## Духи-Целители - `item_lia_healing_ward`

Источник: [item_lia_healing_ward.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=30`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_POINT | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L8) |
| `AbilityCastRange` | 500 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L11) |
| `AbilityCooldown` | 0 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L13) |
| `AbilityCastPoint` | 0.0 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L14) |
| `AbilityManaCost` | 0 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L15) |
| `ItemCost` | 30 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L17) |
| `ItemStackable` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L21) |
| `ItemPermanent` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L22) |
| `ItemKillable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L23) |
| `ItemInitialCharges` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L24) |
| `ItemRequiresCharges` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L25) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `heal_percent_tooltip` | 3 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L29) |
| `duration` | 30 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_healing_ward.txt#L30) |

Lua: [item_lia_healing_ward.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_healing_ward.lua#L1); [HealingWard.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/HealingWard.lua#L1).

## Эликсир Здоровья - `item_lia_health_elixir`

Источник: [item_lia_health_elixir.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=45`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L7) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_DONT_RESUME_ATTACK | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L10) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L12) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L13) |
| `ItemKillable` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L18) |
| `ItemSellable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L19) |
| `ItemDroppable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L20) |
| `AbilityCastRange` | 100 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L22) |
| `AbilityCastPoint` | 0.0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L23) |
| `AbilityCooldown` | 40.0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L24) |
| `ItemCost` | 45 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L26) |
| `ItemStackable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L30) |
| `ItemPermanent` | 0 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L32) |
| `ItemInitialCharges` | 2 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L33) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `heal_amount` | 800 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_elixir.txt#L37) |

Lua: [item_lia_health_elixir.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_health_elixir.lua#L1).

## Отвар Здоровья - `item_lia_health_stone_potion`

Источник: [item_lia_health_stone_potion.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=15`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L7) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_DONT_RESUME_ATTACK | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L10) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L11) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L12) |
| `AbilityCastRange` | 100 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L16) |
| `AbilityCastPoint` | 0.0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L17) |
| `AbilityCooldown` | 40.0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L18) |
| `ItemCost` | 15 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L20) |
| `ItemKillable` | 0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L24) |
| `ItemSellable` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L25) |
| `ItemStackable` | 1 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L26) |
| `ItemPermanent` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L28) |
| `ItemInitialCharges` | 2 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L29) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_health_regen` | 5 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L33) |
| `heal_amount` | 400 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_health_stone_potion.txt#L34) |

Lua: [item_lia_health_stone_potion.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_health_stone_potion.lua#L1).

## Рецепт - `item_recipe_lia_hell_gloves`

Источник: [item_lia_hell_gloves.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=450`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L7) |
| `ItemCost` | 450 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L14) |
| `ItemResult` | item_lia_hell_gloves | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L16) |

## Адские Перчатки - `item_lia_hell_gloves`

Источник: [item_lia_hell_gloves.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=870`.

Рецепт `item_recipe_lia_hell_gloves`: item_lia_runed_gloves + item_lia_gloves_of_strength; свиток 450; сумма объявленных цен 870, цена результата 870. [item_lia_hell_gloves.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L26) |
| `ItemCost` | 870 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_speed` | 30 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L54) |
| `bonus_damage` | 60 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L55) |
| `bonus_strength` | 30 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L56) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_hell_gloves[1]/Properties[1]/MODIFIER_PROPERTY_STATS_STRENGTH_BONUS[1]` | %bonus_strength | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L46) |
| `Modifiers[1]/modifier_item_lia_hell_gloves[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %bonus_attack_speed | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L47) |
| `Modifiers[1]/modifier_item_lia_hell_gloves[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_gloves.txt#L48) |

## Рецепт - `item_recipe_lia_hell_mask`

Источник: [item_lia_hell_mask.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=265`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L7) |
| `ItemCost` | 265 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L14) |
| `ItemResult` | item_lia_hell_mask | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L16) |

## Адская Маска - `item_lia_hell_mask`

Источник: [item_lia_hell_mask.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=420`.

Рецепт `item_recipe_lia_hell_mask`: item_lia_mask + item_lia_mantle; свиток 265; сумма объявленных цен 420, цена результата 420. [item_lia_hell_mask.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L26) |
| `ItemCost` | 420 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L35) |
| `AbilityCooldown` | 30.0 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L37) |
| `AbilityManaCost` | 100 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L38) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana_regen` | 0.6 | [67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L67) |
| `bonus_intelligence` | 8 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L68) |
| `creep_duration` | 80 | [69](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L69) |

Lua: [HellMask.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/HellMask.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_hell_mask[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT[1]` | %bonus_mana_regen | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L60) |
| `Modifiers[1]/modifier_item_lia_hell_mask[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hell_mask.txt#L61) |

## Шлем - `item_lia_helm`

Источник: [item_lia_helm.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=195`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 195 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L17) |
| `BaseClass` | item_datadriven | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L18) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 6 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L22) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_helm[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_helm.txt#L35) |

## Рецепт - `item_recipe_lia_huge_axe`

Источник: [item_lia_huge_axe.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=265`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L7) |
| `ItemCost` | 265 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L14) |
| `ItemResult` | item_lia_huge_axe | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L16) |

## Огромный Топор - `item_lia_huge_axe`

Источник: [item_lia_huge_axe.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=515`.

Рецепт `item_recipe_lia_huge_axe`: item_lia_axe + item_lia_claws + item_lia_orb_of_fire; свиток 265; сумма объявленных цен 525, цена результата 515. [item_lia_huge_axe.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L26) |
| `ItemCost` | 515 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L36) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 32 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L40) |
| `bonus_strength` | 15 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L41) |
| `cleave_percent` | 25 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L42) |
| `cleave_start_width` | 150 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L43) |
| `cleave_end_width` | 200 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L44) |
| `cleave_length` | 200 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_huge_axe.txt#L45) |

Lua: [item_lia_huge_axe.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_huge_axe.lua#L1).

## имя не найдено - `item_recipe_lia_hyper_boots`

Источник: [item_lia_hyper_boots.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=400`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L7) |
| `ItemCost` | 400 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L14) |
| `ItemResult` | item_lia_hyper_boots | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L16) |

## Гиперсапоги - `item_lia_hyper_boots`

Источник: [item_lia_hyper_boots.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=490`.

Рецепт `item_recipe_lia_hyper_boots`: item_lia_boots; свиток 400; сумма объявленных цен 490, цена результата 490. [item_lia_hyper_boots.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L26) |
| `ItemCost` | 490 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L36) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_BOTH | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L37) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L38) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L39) |
| `CastFilterRejectCaster` | 1 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L40) |
| `AbilityCastPoint` | 0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L42) |
| `AbilityCooldown` | 18.0 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L43) |
| `AbilityCastRange` | 500 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L45) |
| `AbilityManaCost` | 50 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L47) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_movement_speed` | 60 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L57) |
| `max_distance` | 750 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L58) |
| `radius` | 250 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L59) |
| `stun_duration` | 1 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L60) |
| `damage` | 100 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L61) |
| `charge_speed` | 2500 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L62) |
| `cast_range` | 500 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L63) |

Lua: [item_lia_hyper_boots.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_hyper_boots.lua#L1).

## имя не найдено - `item_lia_hyper_boots_old`

Источник: [item_lia_hyper_boots.txt:67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L67). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=490`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L70) |
| `ItemCost` | 490 | [71](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L71) |
| `ItemKillable` | 0 | [72](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L72) |
| `ItemDroppable` | 1 | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L73) |
| `ItemSellable` | 1 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L74) |
| `ItemPurchasable` | 1 | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L75) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L79) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_BOTH | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L80) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L81) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L82) |
| `CastFilterRejectCaster` | 1 | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L83) |
| `AbilityCastPoint` | 0 | [85](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L85) |
| `AbilityCooldown` | 18.0 | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L86) |
| `AbilityCastRange` | 500 | [88](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L88) |
| `AbilityManaCost` | 50 | [90](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L90) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_movement_speed` | 20 | [166](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L166) |
| `max_distance` | 750 | [167](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L167) |
| `radius` | 250 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L168) |
| `stun_duration` | 1 | [169](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L169) |
| `damage` | 100 | [170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L170) |
| `charge_speed` | 2500 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L171) |
| `cast_range` | 500 | [172](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L172) |

Lua: [HyperBoots.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/HyperBoots.lua#L1); [HyperBoots.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/HyperBoots.lua#L1); [HyperBoots.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/HyperBoots.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_hyper_boots[1]/Properties[1]/MODIFIER_PROPERTY_MOVESPEED_BONUS_UNIQUE[1]` | %bonus_movement_speed | [144](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L144) |
| `Modifiers[1]/modifier_item_hyper_boots_active[1]/States[1]/MODIFIER_STATE_DISARMED[1]` | MODIFIER_STATE_VALUE_ENABLED | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L159) |
| `Modifiers[1]/modifier_item_hyper_boots_active[1]/States[1]/MODIFIER_STATE_MAGIC_IMMUNE[1]` | MODIFIER_STATE_VALUE_ENABLED | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_hyper_boots.txt#L160) |

## Рецепт - `item_recipe_lia_ice_sword`

Источник: [item_lia_ice_sword.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=500`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L7) |
| `ItemCost` | 500 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L14) |
| `ItemResult` | item_lia_ice_sword | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L16) |

## Ледяной Меч - `item_lia_ice_sword`

Источник: [item_lia_ice_sword.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1315`.

Рецепт `item_recipe_lia_ice_sword`: item_lia_orb_of_frost + item_lia_steel_sword; свиток 500; сумма объявленных цен 1315, цена результата 1315. [item_lia_ice_sword.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L26) |
| `ItemCost` | 1315 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L36) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L37) |
| `SpellImmunityType` | SPELL_IMMUNITY_ENEMIES_NO | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L38) |
| `AbilityCooldown` | 14.0 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L40) |
| `AbilityManaCost` | 275 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L41) |
| `AoERadius` | 500 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L43) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana` | 600 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L53) |
| `bonus_damage` | 60 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L54) |
| `aura_radius` | 900 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L55) |
| `aura_attack_speed` | 25 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L56) |
| `aura_movement_speed` | 15 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L57) |
| `orb_movespeed_slow` | -40 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L58) |
| `orb_attack_slow` | -25 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L59) |
| `orb_duration` | 3 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L60) |
| `freeze_duration` | 3 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L61) |
| `freeze_damage` | 200 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L62) |
| `freeze_radius` | 500 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ice_sword.txt#L63) |

Lua: [item_lia_ice_sword.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_ice_sword.lua#L1).

## Рецепт - `item_recipe_lia_knight_cuirass`

Источник: [item_lia_knight_cuirass.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=1050`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L7) |
| `ItemCost` | 1050 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L14) |
| `ItemResult` | item_lia_knight_cuirass | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L16) |

## Кираса Рыцаря - `item_lia_knight_cuirass`

Источник: [item_lia_knight_cuirass.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=2240`.

Рецепт `item_recipe_lia_knight_cuirass`: item_lia_knight_shield + item_lia_shield_of_endurance; свиток 1050; сумма объявленных цен 2240, цена результата 2240. [item_lia_knight_cuirass.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L26) |
| `ItemCost` | 2240 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L36) |
| `AbilityCooldown` | 30 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L37) |
| `AbilityManaCost` | 85 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L38) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 20 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L42) |
| `bonus_health` | 650 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L43) |
| `bonus_health_regen` | 25 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L44) |
| `damage_return` | 25 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L45) |
| `damage_return_abi` | 50 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L46) |
| `duration` | 5 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_cuirass.txt#L47) |

Lua: [item_lia_knight_cuirass.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_knight_cuirass.lua#L1).

## Рецепт - `item_recipe_lia_knight_shield`

Источник: [item_lia_knight_shield.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=300`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L7) |
| `ItemCost` | 300 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L14) |
| `ItemResult` | item_lia_knight_shield | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L16) |

## Рыцарский Щит - `item_lia_knight_shield`

Источник: [item_lia_knight_shield.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=565`.

Рецепт `item_recipe_lia_knight_shield`: item_lia_claws + item_lia_ring_of_protection + item_lia_amulet; свиток 300; сумма объявленных цен 565, цена результата 565. [item_lia_knight_shield.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L26) |
| `ItemCost` | 565 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L36) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 5 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L40) |
| `bonus_health` | 200 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L41) |
| `damage_return` | 20 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_knight_shield.txt#L42) |

Lua: [item_lia_knight_shield.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_knight_shield.lua#L1).

## Рецепт - `item_recipe_lia_lightning_bow`

Источник: [item_lia_lightning_bow.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=900`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L7) |
| `ItemCost` | 900 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L14) |
| `ItemResult` | item_lia_lightning_bow | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L16) |

## Лук Молний - `item_lia_lightning_bow`

Источник: [item_lia_lightning_bow.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=2960`.

Рецепт `item_recipe_lia_lightning_bow`: item_lia_lightning_spear + item_lia_magic_bow; свиток 900; сумма объявленных цен 2960, цена результата 2960. [item_lia_lightning_bow.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L26) |
| `ItemCost` | 2960 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L35) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L36) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L37) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L38) |
| `AbilityCooldown` | 30.0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L42) |
| `AbilityManaCost` | 120 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L43) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_speed` | 80 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L53) |
| `bonus_agility` | 90 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L54) |
| `crit_chance` | 25 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L55) |
| `crit_mult` | 200 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L56) |
| `heaven_wrath_damage` | 500 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L57) |
| `heaven_wrath_duration` | 8 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L58) |
| `heaven_wrath_radius` | 600 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L59) |
| `heaven_wrath_time_between_lightings` | 0.5 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L60) |
| `heaven_wrath_min_health_heroes` | 35 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L61) |

Lua: [LightningBow.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/LightningBow.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_lightning_bow[1]/Properties[1]/MODIFIER_PROPERTY_STATS_AGILITY_BONUS[1]` | %bonus_agility | [88](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L88) |
| `Modifiers[1]/modifier_item_lia_lightning_bow[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %bonus_attack_speed | [89](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L89) |
| `Modifiers[1]/modifier_item_lia_lightning_bow_crit[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_CRITICALSTRIKE[1]` | %crit_mult | [123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_bow.txt#L123) |

## Рецепт - `item_recipe_lia_lightning_spear`

Источник: [item_lia_lightning_spear.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=230`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L7) |
| `ItemCost` | 230 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L14) |
| `ItemResult` | item_lia_lightning_spear | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L16) |

## Копьё Молний - `item_lia_lightning_spear`

Источник: [item_lia_lightning_spear.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=880`.

Рецепт `item_recipe_lia_lightning_spear`: item_lia_fire_sword + item_lia_ancient_glove + item_lia_spear; свиток 230; сумма объявленных цен 885, цена результата 880. [item_lia_lightning_spear.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L26) |
| `ItemCost` | 880 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L35) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L36) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L37) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L38) |
| `AbilityCastRange` | 700 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L40) |
| `AbilityCooldown` | 8.0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L42) |
| `AbilityManaCost` | 100 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L43) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 45 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L55) |
| `crit_chance` | 22 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L56) |
| `crit_mult` | 190 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L57) |
| `lightning_damage` | 400 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L58) |
| `lightning_bounces` | 8 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L59) |
| `bounce_range` | 700 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L60) |
| `lightning_decay` | 0 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L61) |
| `time_between_bounces` | 0.2 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L62) |

Lua: [LightningSpear.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/LightningSpear.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_lightning_spear[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [84](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L84) |
| `Modifiers[1]/modifier_item_lia_lightning_spear_crit[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_CRITICALSTRIKE[1]` | %crit_mult | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lightning_spear.txt#L113) |

## Рецепт - `item_recipe_lia_lunar_necklace`

Источник: [item_lia_lunar_necklace.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=500`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L7) |
| `ItemCost` | 500 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L14) |
| `ItemResult` | item_lia_lunar_necklace | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L16) |

## Лунное Колье - `item_lia_lunar_necklace`

Источник: [item_lia_lunar_necklace.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1690`.

Рецепт `item_recipe_lia_lunar_necklace`: item_lia_magic_necklace + item_lia_amulet + item_lia_mana_stone; свиток 500; сумма объявленных цен 1790, цена результата 1690. [item_lia_lunar_necklace.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L26) |
| `ItemCost` | 1690 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L36) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L37) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L38) |
| `AbilityCooldown` | 24 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L39) |
| `AbilityManacost` | 60 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L40) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_health` | 300 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L50) |
| `bonus_mana` | 300 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L51) |
| `bonus_all_stats` | 35 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L52) |
| `duration` | 6 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L53) |
| `stat_percent` | 75 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L54) |
| `bonus_magic_resist_percentage_active` | 20 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L55) |
| `stat_bonus_max` | 75 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_lunar_necklace.txt#L56) |

Lua: [item_lia_lunar_necklace.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_lunar_necklace.lua#L1).

## Рецепт - `item_recipe_lia_magic_bow`

Источник: [item_lia_magic_bow.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=650`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L7) |
| `ItemCost` | 650 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L14) |
| `ItemResult` | item_lia_magic_bow | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L16) |

## Волшебный Лук - `item_lia_magic_bow`

Источник: [item_lia_magic_bow.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1180`.

Рецепт `item_recipe_lia_magic_bow`: item_lia_runed_gloves + item_lia_spear + item_lia_thugs_dagger; свиток 650; сумма объявленных цен 1180, цена результата 1180. [item_lia_magic_bow.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L26) |
| `ItemCost` | 1180 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_speed` | 40 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L54) |
| `bonus_damage` | 50 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L55) |
| `bonus_agility` | 30 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L56) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_magic_bow[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %bonus_attack_speed | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L46) |
| `Modifiers[1]/modifier_item_lia_magic_bow[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L47) |
| `Modifiers[1]/modifier_item_lia_magic_bow[1]/Properties[1]/MODIFIER_PROPERTY_STATS_AGILITY_BONUS[1]` | %bonus_agility | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_bow.txt#L48) |

## Рецепт - `item_recipe_lia_magic_helm`

Источник: [item_lia_magic_helm.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=280`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L7) |
| `ItemCost` | 280 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L14) |
| `ItemResult` | item_lia_magic_helm | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L16) |

## Магический Шлем - `item_lia_magic_helm`

Источник: [item_lia_magic_helm.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=475`.

Рецепт `item_recipe_lia_magic_helm`: item_lia_helm; свиток 280; сумма объявленных цен 475, цена результата 475. [item_lia_magic_helm.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L26) |
| `ItemCost` | 475 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 8 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L53) |
| `evasion_percent` | 15 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L54) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_magic_helm[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L46) |
| `Modifiers[1]/modifier_item_lia_magic_helm[1]/Properties[1]/MODIFIER_PROPERTY_EVASION_CONSTANT[1]` | %evasion_percent | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_helm.txt#L47) |

## Рецепт - `item_recipe_lia_magic_necklace`

Источник: [item_lia_magic_necklace.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=450`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L7) |
| `ItemCost` | 450 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L14) |
| `ItemResult` | item_lia_magic_necklace | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L16) |

## Магическое Ожерелье - `item_lia_magic_necklace`

Источник: [item_lia_magic_necklace.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1050`.

Рецепт `item_recipe_lia_magic_necklace`: item_lia_staff + item_lia_thugs_dagger + item_lia_gloves_of_strength; свиток 450; сумма объявленных цен 1050, цена результата 1050. [item_lia_magic_necklace.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L26) |
| `ItemCost` | 1050 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_all_stats` | 25 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L54) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_magic_necklace[1]/Properties[1]/MODIFIER_PROPERTY_STATS_STRENGTH_BONUS[1]` | %bonus_all_stats | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L46) |
| `Modifiers[1]/modifier_item_lia_magic_necklace[1]/Properties[1]/MODIFIER_PROPERTY_STATS_AGILITY_BONUS[1]` | %bonus_all_stats | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L47) |
| `Modifiers[1]/modifier_item_lia_magic_necklace[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_all_stats | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_necklace.txt#L48) |

## Рецепт - `item_recipe_lia_magic_staff`

Источник: [item_lia_magic_staff.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=300`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L7) |
| `ItemCost` | 300 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L14) |
| `ItemResult` | item_lia_magic_staff | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L16) |

## Волшебный Посох - `item_lia_magic_staff`

Источник: [item_lia_magic_staff.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=770`.

Рецепт `item_recipe_lia_magic_staff`: item_lia_staff + item_lia_staff + item_lia_mask; свиток 300; сумма объявленных цен 770, цена результата 770. [item_lia_magic_staff.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L26) |
| `ItemCost` | 770 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_intelligence` | 24 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L53) |
| `bonus_mana_regen` | 0.4 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L54) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_magic_staff[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L46) |
| `Modifiers[1]/modifier_item_lia_magic_staff[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT[1]` | %bonus_mana_regen_percentage | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magic_staff.txt#L47) |

## имя не найдено - `item_recipe_lia_magician_armor`

Источник: [item_lia_magician_armor.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=800`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L7) |
| `ItemCost` | 800 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L14) |
| `ItemResult` | item_lia_magician_armor | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L16) |

## Доспехи Заклинателя - `item_lia_magician_armor`

Источник: [item_lia_magician_armor.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=2205`.

Рецепт `item_recipe_lia_magician_armor`: item_lia_dwarf_armor + item_lia_magic_staff + item_lia_amulet; свиток 800; сумма объявленных цен 2205, цена результата 2205. [item_lia_magician_armor.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L26) |
| `ItemCost` | 2205 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L36) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 15 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L40) |
| `bonus_health` | 450 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L41) |
| `bonus_intelligence` | 40 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L42) |
| `bonus_mana_regeneration` | 4 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L43) |
| `bonus_spell_damage_percentage` | 15 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L44) |
| `bonus_debuff_duration_percentage` | 10 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_magician_armor.txt#L45) |

Lua: [item_lia_magician_armor.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_magician_armor.lua#L1).

## Эликсир Маны - `item_lia_mana_elixir`

Источник: [item_lia_mana_elixir.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=45`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_DONT_RESUME_ATTACK | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L9) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L10) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L11) |
| `ItemKillable` | 0 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L15) |
| `ItemSellable` | 1 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L16) |
| `ItemDroppable` | 1 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L17) |
| `AbilityCastRange` | 100 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L19) |
| `AbilityCastPoint` | 0.0 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L20) |
| `AbilityCooldown` | 40 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L21) |
| `ItemCost` | 45 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L23) |
| `ItemStackable` | 1 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L27) |
| `ItemPermanent` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L29) |
| `ItemInitialCharges` | 2 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L30) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `mana_amount` | 500 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_elixir.txt#L34) |

Lua: [item_lia_mana_elixir.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_mana_elixir.lua#L1).

## Рецепт - `item_recipe_lia_mana_staff`

Источник: [item_lia_mana_staff.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=400`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L7) |
| `ItemCost` | 400 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L14) |
| `ItemResult` | item_lia_mana_staff | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L16) |

## Посох Маны - `item_lia_mana_staff`

Источник: [item_lia_mana_staff.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=720`.

Рецепт `item_recipe_lia_mana_staff`: item_lia_staff + item_lia_mana_stone; свиток 400; сумма объявленных цен 720, цена результата 720. [item_lia_mana_staff.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L26) |
| `ItemCost` | 720 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana` | 300 | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L79) |
| `bonus_intelligence` | 15 | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L80) |
| `aura_radius` | 1000 | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L81) |
| `aura_mana_regen` | 4.0 | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L82) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_mana_staff[1]/Properties[1]/MODIFIER_PROPERTY_MANA_BONUS[1]` | %bonus_mana | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L55) |
| `Modifiers[1]/modifier_item_lia_mana_staff[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L56) |
| `Modifiers[1]/modifier_item_mana_staff_aura[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT_UNIQUE[1]` | 4.0 | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_staff.txt#L73) |

## Камень Маны - `item_lia_mana_stone`

Источник: [item_lia_mana_stone.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=120`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 120 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L16) |
| `BaseClass` | item_datadriven | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L17) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana` | 150 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L21) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_mana_stone[1]/Properties[1]/MODIFIER_PROPERTY_MANA_BONUS[1]` | %bonus_mana | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone.txt#L34) |

## Отвар Маны - `item_lia_mana_stone_potion`

Источник: [item_lia_mana_stone_potion.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=15`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_DONT_RESUME_ATTACK | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L8) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L9) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L10) |
| `AbilityCastRange` | 100 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L15) |
| `AbilityCastPoint` | 0.0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L16) |
| `AbilityCooldown` | 40.0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L17) |
| `ItemCost` | 15 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L19) |
| `ItemKillable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L23) |
| `ItemSellable` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L24) |
| `ItemStackable` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L25) |
| `ItemPermanent` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L27) |
| `ItemInitialCharges` | 2 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L28) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana_regen` | 0.25 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L32) |
| `mana_amount` | 250 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mana_stone_potion.txt#L33) |

Lua: [item_lia_mana_stone_potion.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_mana_stone_potion.lua#L1).

## Мантия - `item_lia_mantle`

Источник: [item_lia_mantle.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=85`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 85 | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L6) |
| `ItemKillable` | 0 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L7) |
| `ItemDroppable` | 1 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L8) |
| `ItemSellable` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L10) |
| `ItemPurchasable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L11) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L16) |
| `BaseClass` | item_datadriven | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L17) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_intelligence` | 6 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L21) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_mantle[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mantle.txt#L34) |

## Маска - `item_lia_mask`

Источник: [item_lia_mask.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=70`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L6) |
| `ItemCost` | 70 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L18) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana_regen` | 0.2 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L22) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_mask[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT[1]` | %bonus_mana_regen | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask.txt#L35) |

## Маска Смерти - `item_lia_mask_of_death`

Источник: [item_lia_mask_of_death.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask_of_death.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=200`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask_of_death.txt#L6) |
| `ItemCost` | 200 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask_of_death.txt#L9) |
| `ItemKillable` | 0 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask_of_death.txt#L10) |
| `ItemDroppable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask_of_death.txt#L11) |
| `ItemSellable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask_of_death.txt#L12) |
| `ItemPurchasable` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask_of_death.txt#L13) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE \| DOTA_ABILITY_BEHAVIOR_ATTACK | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask_of_death.txt#L17) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `lifesteal_percent` | 10 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mask_of_death.txt#L21) |

Lua: [item_lia_mask_of_death.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_mask_of_death.lua#L1).

## Рецепт - `item_recipe_lia_mithril_armor`

Источник: [item_lia_mithril_armor.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=330`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L7) |
| `ItemCost` | 330 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L14) |
| `ItemResult` | item_lia_mithril_armor | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L16) |

## Мифриловый Доспех - `item_lia_mithril_armor`

Источник: [item_lia_mithril_armor.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=1045`.

Рецепт `item_recipe_lia_mithril_armor`: item_lia_dwarf_armor + item_lia_ring_of_protection + item_lia_ring_of_protection; свиток 330; сумма объявленных цен 1005, цена результата 1045. [item_lia_mithril_armor.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L25) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_DONT_RESUME_MOVEMENT | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L28) |
| `ItemKillable` | 0 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L32) |
| `AbilityCooldown` | 14.0 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L41) |
| `AbilityManaCost` | 80 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L42) |
| `AbilityCastRange` | 400 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L43) |
| `ItemCost` | 1045 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L46) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 16 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L53) |
| `bonus_health` | 250 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L54) |
| `damage_blocked` | 45 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L55) |
| `radius` | 400 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L56) |
| `duration` | 5.0 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_mithril_armor.txt#L57) |

Lua: [item_lia_mithril_armor.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_mithril_armor.lua#L1).

## Ожерелье - `item_lia_necklace`

Источник: [item_lia_necklace.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=85`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L7) |
| `ItemCost` | 85 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L11) |
| `ItemKillable` | 0 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L15) |
| `ItemDroppable` | 1 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L16) |
| `ItemSellable` | 1 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L18) |
| `ItemPurchasable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L19) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_agility` | 6 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L23) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_necklace[1]/Properties[1]/MODIFIER_PROPERTY_STATS_AGILITY_BONUS[1]` | %bonus_agility | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_necklace.txt#L36) |

## Сфера Огня - `item_lia_orb_of_fire`

Источник: [item_lia_orb_of_fire.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=110`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L8) |
| `ItemCost` | 110 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L12) |
| `ItemKillable` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L16) |
| `ItemDroppable` | 1 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L17) |
| `ItemSellable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L19) |
| `ItemPurchasable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L20) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 15 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L24) |
| `radius` | 180 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_fire.txt#L25) |

Lua: [item_lia_orb_of_fire.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_orb_of_fire.lua#L1).

## Рецепт - `item_recipe_lia_orb_of_frost`

Источник: [item_lia_orb_of_frost.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=400`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L7) |
| `ItemCost` | 400 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L14) |
| `ItemResult` | item_lia_orb_of_frost | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L16) |

## Сфера Льда - `item_lia_orb_of_frost`

Источник: [item_lia_orb_of_frost.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=770`.

Рецепт `item_recipe_lia_orb_of_frost`: item_lia_mana_stone + item_lia_stormwind_horn; свиток 400; сумма объявленных цен 770, цена результата 770. [item_lia_orb_of_frost.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L26) |
| `ItemCost` | 770 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L36) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana` | 400 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L40) |
| `bonus_damage` | 30 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L41) |
| `aura_radius` | 900 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L42) |
| `aura_attack_speed` | 20 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L43) |
| `aura_movement_speed` | 10 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L44) |
| `orb_movespeed_slow` | -40 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L45) |
| `orb_attack_slow` | -25 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L46) |
| `orb_duration` | 3 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_orb_of_frost.txt#L47) |

Lua: [item_lia_orb_of_frost.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_orb_of_frost.lua#L1).

## Рецепт - `item_recipe_lia_pantilus_blade`

Источник: [item_lia_pantilus_blade.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=280`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L7) |
| `ItemCost` | 280 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L14) |
| `ItemResult` | item_lia_pantilus_blade | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L16) |

## Меч Пантилуса - `item_lia_pantilus_blade`

Источник: [item_lia_pantilus_blade.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=520`.

Рецепт `item_recipe_lia_pantilus_blade`: item_lia_steel_sword + item_lia_gloves_of_haste + item_lia_gloves_of_haste + item_lia_necklace; свиток 280; сумма объявленных цен 520, цена результата 520. [item_lia_pantilus_blade.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L26) |
| `ItemCost` | 520 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_speed` | 35 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L54) |
| `bonus_damage` | 30 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L55) |
| `bonus_agility` | 15 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L56) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_pantilus_blade[1]/Properties[1]/MODIFIER_PROPERTY_STATS_AGILITY_BONUS[1]` | %bonus_agility | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L46) |
| `Modifiers[1]/modifier_item_lia_pantilus_blade[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %bonus_attack_speed | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L47) |
| `Modifiers[1]/modifier_item_lia_pantilus_blade[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pantilus_blade.txt#L48) |

## Рецепт - `item_recipe_lia_poleaxe_of_rage`

Источник: [item_lia_poleaxe_of_rage.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=600`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L7) |
| `ItemCost` | 600 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L14) |
| `ItemResult` | item_lia_poleaxe_of_rage | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L16) |

## Секира Ярости - `item_lia_poleaxe_of_rage`

Источник: [item_lia_poleaxe_of_rage.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1390`.

Рецепт `item_recipe_lia_poleaxe_of_rage`: item_lia_fire_sword + item_lia_fire_sword; свиток 600; сумма объявленных цен 1390, цена результата 1390. [item_lia_poleaxe_of_rage.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L26) |
| `ItemCost` | 1390 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IGNORE_CHANNEL | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L34) |
| `AbilityCastPoint` | 0.0 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L37) |
| `AbilityCooldown` | 16.0 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L38) |
| `AbilityManaCost` | 90 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L39) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 40 | [139](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L139) |
| `crit_chance` | 25 | [140](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L140) |
| `crit_mult` | 220 | [141](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L141) |
| `berserk_bonus_attack_speed` | 250 | [142](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L142) |
| `berserk_bonus_movement_speed_percentage` | 20 | [143](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L143) |
| `berserk_extra_incoming_damage_percentage` | 10 | [144](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L144) |
| `berserk_duration` | 8.0 | [145](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L145) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_poleaxe_of_rage[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L70) |
| `Modifiers[1]/modifier_item_lia_poleaxe_of_rage_crit[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_CRITICALSTRIKE[1]` | %crit_mult | [100](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L100) |
| `Modifiers[1]/modifier_item_poleaxe_of_rage_berserk[1]/Properties[1]/MODIFIER_PROPERTY_MOVESPEED_BONUS_PERCENTAGE[1]` | %berserk_bonus_movement_speed_percentage | [131](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L131) |
| `Modifiers[1]/modifier_item_poleaxe_of_rage_berserk[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %berserk_bonus_attack_speed | [132](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L132) |
| `Modifiers[1]/modifier_item_poleaxe_of_rage_berserk[1]/Properties[1]/MODIFIER_PROPERTY_INCOMING_DAMAGE_PERCENTAGE[1]` | %berserk_extra_incoming_damage_percentage | [133](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_poleaxe_of_rage.txt#L133) |

## Зелье Невидимости - `item_lia_potion_of_invisibility`

Источник: [item_lia_potion_of_invisibility.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=60`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_DONT_RESUME_ATTACK \| DOTA_ABILITY_BEHAVIOR_IGNORE_CHANNEL | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L8) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L9) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L10) |
| `AbilityCooldown` | 13 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L12) |
| `ItemKillable` | 0 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L14) |
| `ItemSellable` | 1 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L15) |
| `ItemDroppable` | 1 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L16) |
| `ItemCost` | 60 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L18) |
| `ItemStackable` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L21) |
| `ItemPermanent` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L23) |
| `ItemInitialCharges` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L24) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 7 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L29) |

Lua: [EnchantedAxes.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/heroes/TrollCutthroat/EnchantedAxes.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_lia_potion_of_invisibility_windwalk[1]/States[1]/MODIFIER_STATE_INVISIBLE[1]` | MODIFIER_STATE_VALUE_ENABLED | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L75) |
| `Modifiers[1]/modifier_lia_potion_of_invisibility_windwalk[1]/States[1]/MODIFIER_STATE_NO_UNIT_COLLISION[1]` | MODIFIER_STATE_VALUE_ENABLED | [76](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invisibility.txt#L76) |

## Зелье Неуязвимости - `item_lia_potion_of_invulnerability`

Источник: [item_lia_potion_of_invulnerability.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=80`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_DONT_RESUME_ATTACK | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L8) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L9) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L10) |
| `AbilityCooldown` | 40 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L12) |
| `ItemKillable` | 0 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L14) |
| `ItemSellable` | 1 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L15) |
| `ItemDroppable` | 1 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L16) |
| `ItemPermanent` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L17) |
| `ItemCost` | 80 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L19) |
| `ItemStackable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L23) |
| `ItemInitialCharges` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L25) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 2 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L29) |

Lua: [PotionOfInvulnerability.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/PotionOfInvulnerability.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_potion_of_invulnerability[1]/States[1]/MODIFIER_STATE_INVULNERABLE[1]` | MODIFIER_STATE_VALUE_ENABLED | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_potion_of_invulnerability.txt#L58) |

## Рецепт - `item_recipe_lia_pure_light`

Источник: [item_lia_pure_light.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=705`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L7) |
| `ItemCost` | 705 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L14) |
| `ItemResult` | item_lia_pure_light | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L16) |

## Чистейший Свет - `item_lia_pure_light`

Источник: [item_lia_pure_light.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1500`.

Рецепт `item_recipe_lia_pure_light`: item_lia_mana_staff + item_lia_ring_of_regeneration; свиток 705; сумма объявленных цен 1500, цена результата 1500. [item_lia_pure_light.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L26) |
| `ItemCost` | 1500 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_POINT | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L36) |
| `AbilityCooldown` | 30 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L38) |
| `AbilityManaCost` | 350 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L39) |
| `AbilityCastRange` | 600 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L40) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana` | 350 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L52) |
| `bonus_intelligence` | 25 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L53) |
| `aura_mana_regen` | 15 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L54) |
| `aura_health_regen` | 15 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L55) |
| `aura_radius` | 1000 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L56) |
| `totem_damage_reduction` | 60 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L57) |
| `totem_duration` | 6 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L58) |
| `totem_targets` | 3 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L59) |
| `totem_radius` | 600 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_pure_light.txt#L60) |

Lua: [item_lia_pure_light.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_pure_light.lua#L1).

## Рецепт - `item_recipe_lia_blade_of_rage_agi`

Источник: [item_lia_rage_sword_agi.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=600`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L7) |
| `ItemCost` | 600 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L14) |
| `ItemResult` | item_lia_blade_of_rage_agi | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L16) |

## Меч Гнева (Ловкость) - `item_lia_blade_of_rage_agi`

Источник: [item_lia_rage_sword_agi.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1065`.

Рецепт `item_recipe_lia_blade_of_rage_agi`: item_lia_steel_sword + item_lia_runed_gloves + item_lia_thugs_dagger; свиток 600; сумма объявленных цен 1065, цена результата 1065. [item_lia_rage_sword_agi.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L26) |
| `ItemCost` | 1065 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L36) |
| `AbilityCooldown` | 16 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L37) |
| `AbilityManacost` | 90 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L38) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_speed` | 55 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L47) |
| `bonus_damage` | 50 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L48) |
| `bonus_agility` | 25 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L49) |
| `duration` | 5 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L50) |
| `agi_percent` | 75 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_agi.txt#L51) |

Lua: [item_lia_blade_of_rage_agi.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_blade_of_rage_agi.lua#L1).

## Рецепт - `item_recipe_lia_blade_of_rage_int`

Источник: [item_lia_rage_sword_int.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=600`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L7) |
| `ItemCost` | 600 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L14) |
| `ItemResult` | item_lia_blade_of_rage_int | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L16) |

## Меч Гнева (Интеллект) - `item_lia_blade_of_rage_int`

Источник: [item_lia_rage_sword_int.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1065`.

Рецепт `item_recipe_lia_blade_of_rage_int`: item_lia_steel_sword + item_lia_runed_gloves + item_lia_staff; свиток 600; сумма объявленных цен 1065, цена результата 1065. [item_lia_rage_sword_int.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L26) |
| `ItemCost` | 1065 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L36) |
| `AbilityCooldown` | 16 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L37) |
| `AbilityManacost` | 90 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L38) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_speed` | 55 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L47) |
| `bonus_damage` | 50 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L48) |
| `bonus_intellect` | 25 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L49) |
| `duration` | 5 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L50) |
| `int_percent` | 75 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_int.txt#L51) |

Lua: [item_lia_blade_of_rage_int.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_blade_of_rage_int.lua#L1).

## Рецепт - `item_recipe_lia_blade_of_rage_str`

Источник: [item_lia_rage_sword_str.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=600`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L7) |
| `ItemCost` | 600 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L14) |
| `ItemResult` | item_lia_blade_of_rage_str | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L16) |

## Меч Гнева (Сила) - `item_lia_blade_of_rage_str`

Источник: [item_lia_rage_sword_str.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1065`.

Рецепт `item_recipe_lia_blade_of_rage_str`: item_lia_steel_sword + item_lia_runed_gloves + item_lia_gloves_of_strength; свиток 600; сумма объявленных цен 1065, цена результата 1065. [item_lia_rage_sword_str.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L26) |
| `ItemCost` | 1065 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L36) |
| `AbilityCooldown` | 16 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L37) |
| `AbilityManacost` | 90 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L38) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_speed` | 55 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L47) |
| `bonus_damage` | 50 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L48) |
| `bonus_strength` | 25 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L49) |
| `duration` | 5 | [50](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L50) |
| `str_percent` | 75 | [51](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rage_sword_str.txt#L51) |

Lua: [item_lia_blade_of_rage_str.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_blade_of_rage_str.lua#L1).

## Кольцо Защиты - `item_lia_ring_of_protection`

Источник: [item_lia_ring_of_protection.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=80`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 80 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L17) |
| `BaseClass` | item_datadriven | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L18) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 3 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L21) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_ring_of_protection[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_protection.txt#L34) |

## Кольцо Регенерации - `item_lia_ring_of_regeneration`

Источник: [item_lia_ring_of_regeneration.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=75`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 75 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L17) |
| `BaseClass` | item_datadriven | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L18) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_health_regen` | 3 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L22) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_ring_of_regeneration[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_REGEN_CONSTANT[1]` | %bonus_health_regen | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_ring_of_regeneration.txt#L35) |

## Монетка - `item_lia_rune_gold`

Источник: [item_lia_rune_gold.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L7) |
| `ItemCost` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L11) |
| `ItemPermanent` | 0 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L15) |
| `AbilityCooldown` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L16) |
| `ItemKillable` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L17) |
| `ItemSellable` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L18) |
| `ItemDroppable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L19) |
| `ItemInitialCharges` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L20) |
| `ItemStackable` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L21) |
| `ItemCastOnPickup` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L22) |
| `ItemPurchasable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_gold.txt#L23) |

Lua: [runes.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/runes.lua#L1).

## Дерево - `item_lia_rune_lumber`

Источник: [item_lia_rune_lumber.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L7) |
| `ItemCost` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L11) |
| `ItemPermanent` | 0 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L15) |
| `AbilityCooldown` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L16) |
| `ItemKillable` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L17) |
| `ItemSellable` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L18) |
| `ItemDroppable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L19) |
| `ItemInitialCharges` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L20) |
| `ItemStackable` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L21) |
| `ItemCastOnPickup` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L22) |
| `ItemPurchasable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_lumber.txt#L23) |

Lua: [runes.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/runes.lua#L1).

## Книга Ловкости - `item_lia_rune_of_agility`

Источник: [item_lia_rune_of_agility.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=99999`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L7) |
| `ItemCost` | 99999 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L18) |
| `ItemPermanent` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L22) |
| `AbilityCooldown` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L23) |
| `ItemKillable` | 0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L24) |
| `ItemSellable` | 0 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L25) |
| `ItemDroppable` | 1 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L26) |
| `ItemInitialCharges` | 1 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L27) |
| `ItemStackable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L28) |
| `ItemCastOnPickup` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L29) |
| `ItemPurchasable` | 0 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_agility.txt#L30) |

Lua: [runes.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/runes.lua#L1).

## Руна Исцеления - `item_lia_rune_of_healing`

Источник: [item_lia_rune_of_healing.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L7) |
| `ItemCost` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L12) |
| `ItemPermanent` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L16) |
| `AbilityCooldown` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L17) |
| `ItemKillable` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L18) |
| `ItemSellable` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L19) |
| `ItemDroppable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L20) |
| `ItemInitialCharges` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L21) |
| `ItemStackable` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L22) |
| `ItemCastOnPickup` | 1 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L23) |
| `ItemPurchasable` | 0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L24) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `heal` | 450 | [69](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L69) |
| `radius` | 2000 | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_healing.txt#L70) |

## Книга Интеллекта - `item_lia_rune_of_intellect`

Источник: [item_lia_rune_of_intellect.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L7) |
| `ItemCost` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L18) |
| `ItemPermanent` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L22) |
| `AbilityCooldown` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L23) |
| `ItemKillable` | 0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L24) |
| `ItemSellable` | 0 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L25) |
| `ItemDroppable` | 1 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L26) |
| `ItemInitialCharges` | 1 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L27) |
| `ItemStackable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L28) |
| `ItemCastOnPickup` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L29) |
| `ItemPurchasable` | 0 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_intellect.txt#L30) |

Lua: [runes.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/runes.lua#L1).

## Руна Вампиризма - `item_lia_rune_of_lifesteal`

Источник: [item_lia_rune_of_lifesteal.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=99999`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L8) |
| `ItemCost` | 99999 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L18) |
| `ItemPermanent` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L22) |
| `AbilityCooldown` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L23) |
| `ItemKillable` | 0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L24) |
| `ItemSellable` | 0 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L25) |
| `ItemDroppable` | 1 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L26) |
| `ItemInitialCharges` | 1 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L27) |
| `ItemStackable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L28) |
| `ItemCastOnPickup` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L29) |
| `ItemPurchasable` | 0 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L30) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 15 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L34) |
| `bonus_damage` | 50 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L35) |
| `lifesteal_percent` | 100 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_lifesteal.txt#L36) |

## Руна Удачи - `item_lia_rune_of_luck`

Источник: [item_lia_rune_of_luck.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=99999`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L7) |
| `ItemCost` | 99999 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L12) |
| `ItemPermanent` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L16) |
| `AbilityCooldown` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L17) |
| `ItemKillable` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L18) |
| `ItemSellable` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L19) |
| `ItemDroppable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L20) |
| `ItemInitialCharges` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L21) |
| `ItemStackable` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L22) |
| `ItemCastOnPickup` | 1 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L23) |
| `ItemPurchasable` | 0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_luck.txt#L24) |

Lua: [runes.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/runes.lua#L1).

## Руна Маны - `item_lia_rune_of_mana`

Источник: [item_lia_rune_of_mana.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L7) |
| `ItemCost` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L11) |
| `ItemPermanent` | 0 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L15) |
| `AbilityCooldown` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L16) |
| `ItemKillable` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L17) |
| `ItemSellable` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L18) |
| `ItemDroppable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L19) |
| `ItemInitialCharges` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L20) |
| `ItemStackable` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L21) |
| `ItemCastOnPickup` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L22) |
| `ItemPurchasable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L23) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `mana` | 250 | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L70) |
| `radius` | 2000 | [71](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_mana.txt#L71) |

Lua: [mana.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/mana.lua#L1).

## Руна Защиты - `item_lia_rune_of_protection`

Источник: [item_lia_rune_of_protection.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=99999`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L7) |
| `ItemCost` | 99999 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L13) |
| `ItemPermanent` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L17) |
| `AbilityCooldown` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L18) |
| `ItemKillable` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L19) |
| `ItemSellable` | 0 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L20) |
| `ItemDroppable` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L21) |
| `ItemInitialCharges` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L22) |
| `ItemStackable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L23) |
| `ItemCastOnPickup` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L24) |
| `ItemPurchasable` | 0 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L25) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `radius` | 1400 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_protection.txt#L41) |

Lua: [runes.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/runes.lua#L1).

## Руна Восстановления - `item_lia_rune_of_restoration`

Источник: [item_lia_rune_of_restoration.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L7) |
| `ItemCost` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L11) |
| `ItemPermanent` | 0 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L15) |
| `AbilityCooldown` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L16) |
| `ItemKillable` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L17) |
| `ItemSellable` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L18) |
| `ItemDroppable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L19) |
| `ItemInitialCharges` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L20) |
| `ItemStackable` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L21) |
| `ItemCastOnPickup` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L22) |
| `ItemPurchasable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L23) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `mana` | 300 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L77) |
| `heal` | 300 | [78](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L78) |
| `radius` | 2000 | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_restoration.txt#L79) |

Lua: [mana.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/mana.lua#L1).

## Руна Скорости - `item_lia_rune_of_speed`

Источник: [item_lia_rune_of_speed.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L7) |
| `ItemCost` | 0 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L17) |
| `ItemPermanent` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L21) |
| `AbilityCooldown` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L22) |
| `ItemKillable` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L23) |
| `ItemSellable` | 0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L24) |
| `ItemDroppable` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L25) |
| `ItemInitialCharges` | 1 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L26) |
| `ItemStackable` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L27) |
| `ItemCastOnPickup` | 1 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L28) |
| `ItemPurchasable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L29) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `radius` | 2000 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L33) |
| `duration` | 15 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L34) |
| `speed_bonus` | 200 | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L35) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_rune_of_speed[1]/Properties[1]/MODIFIER_PROPERTY_MOVESPEED_BONUS_PERCENTAGE[1]` | %speed_bonus | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_speed.txt#L77) |

## Книга Силы - `item_lia_rune_of_strength`

Источник: [item_lia_rune_of_strength.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=0`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L7) |
| `ItemCost` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L18) |
| `ItemPermanent` | 0 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L22) |
| `AbilityCooldown` | 0 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L23) |
| `ItemKillable` | 0 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L24) |
| `ItemSellable` | 0 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L25) |
| `ItemDroppable` | 1 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L26) |
| `ItemInitialCharges` | 1 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L27) |
| `ItemStackable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L28) |
| `ItemCastOnPickup` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L29) |
| `ItemPurchasable` | 0 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_rune_of_strength.txt#L30) |

Lua: [runes.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/runes.lua#L1).

## Рунный Браслет - `item_lia_runed_bracers`

Источник: [item_lia_runed_bracers.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=220`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 220 | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L6) |
| `ItemKillable` | 0 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L7) |
| `ItemDroppable` | 1 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L8) |
| `ItemSellable` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L10) |
| `ItemPurchasable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L11) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L16) |
| `BaseClass` | item_datadriven | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L17) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_magic_resist_percentage` | 20 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L21) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_runed_bracers[1]/Properties[1]/MODIFIER_PROPERTY_MAGICAL_RESISTANCE_BONUS[1]` | %bonus_magic_resist_percentage | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_bracers.txt#L34) |

## Рецепт - `item_recipe_lia_runed_gloves`

Источник: [item_lia_runed_gloves.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=100`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L7) |
| `ItemCost` | 100 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L14) |
| `ItemResult` | item_lia_runed_gloves | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L16) |

## Рунные Перчатки - `item_lia_runed_gloves`

Источник: [item_lia_runed_gloves.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=220`.

Рецепт `item_recipe_lia_runed_gloves`: item_lia_claws + item_lia_gloves_of_haste; свиток 100; сумма объявленных цен 220, цена результата 220. [item_lia_runed_gloves.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L26) |
| `ItemCost` | 220 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_speed` | 20 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L53) |
| `bonus_damage` | 20 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L54) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_runed_gloves[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %bonus_attack_speed | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L46) |
| `Modifiers[1]/modifier_item_lia_runed_gloves[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_runed_gloves.txt#L47) |

## Рецепт - `item_recipe_lia_staff_of_power`

Источник: [item_lia_scepter_of_power.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=200`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L7) |
| `ItemCost` | 200 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L14) |
| `ItemResult` | item_lia_staff_of_power | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L16) |

## Скипетр Власти - `item_lia_staff_of_power`

Источник: [item_lia_scepter_of_power.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=790`.

Рецепт `item_recipe_lia_staff_of_power`: item_lia_mask + item_lia_amulet_of_spell_shield; свиток 200; сумма объявленных цен 570, цена результата 790. [item_lia_scepter_of_power.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L27) |
| `ItemCost` | 790 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L29) |
| `ItemKillable` | 0 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L30) |
| `ItemDroppable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L31) |
| `ItemSellable` | 1 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L33) |
| `ItemPurchasable` | 1 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L34) |
| `AbilityCooldown` | 20 | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L38) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L40) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana_regen` | 1.5 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L44) |
| `block_cooldown` | 20 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L45) |

Lua: [ScepterOfPower.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/ScepterOfPower.lua#L1); [ScepterOfPower.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/ScepterOfPower.lua#L1); [ScepterOfPower.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/ScepterOfPower.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_scepter_of_power[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT[1]` | %bonus_mana_regen | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scepter_of_power.txt#L58) |

## Свиток Восстановления - `item_lia_scroll_of_restoration`

Источник: [item_lia_scroll_of_restoration.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=70`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_AOE \| DOTA_ABILITY_BEHAVIOR_NO_TARGET | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L7) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L8) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L9) |
| `AbilityCooldown` | 25 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L11) |
| `ItemCost` | 70 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L13) |
| `ItemPermanent` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L18) |
| `ItemKillable` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L19) |
| `ItemSellable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L20) |
| `ItemDroppable` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L21) |
| `ItemInitialCharges` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L22) |
| `ItemStackable` | 1 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L23) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `health_restored` | 500 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L27) |
| `mana_restored` | 250 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L28) |
| `radius` | 1000 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_restoration.txt#L29) |

Lua: [mana.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/mana.lua#L1).

## Свиток Тайного Знания - `item_lia_scroll_of_secret_knowledge`

Источник: [item_lia_scroll_of_secret_knowledge.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=90`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_AOE \| DOTA_ABILITY_BEHAVIOR_NO_TARGET | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L7) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L8) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L9) |
| `AbilityCooldown` | 12 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L11) |
| `ItemCost` | 90 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L13) |
| `ItemPermanent` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L18) |
| `ItemKillable` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L19) |
| `ItemSellable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L20) |
| `ItemDroppable` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L21) |
| `ItemInitialCharges` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L22) |
| `ItemStackable` | 1 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L23) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 100 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L27) |
| `duration` | 6 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L28) |
| `radius` | 600 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L29) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_scroll_of_secret_knowledge[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_scroll_of_secret_knowledge.txt#L82) |

## Рецепт - `item_recipe_lia_seal_of_power_shop`

Источник: [item_lia_seal_of_power.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=850`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L6) |
| `ItemCost` | 850 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L8) |
| `ItemKillable` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L11) |
| `ItemRecipe` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L13) |
| `ItemResult` | item_lia_seal_of_power_shop | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L15) |

## Печать Силы - `item_lia_seal_of_power_shop`

Источник: [item_lia_seal_of_power.txt:22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L22). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1155`.

Рецепт `item_recipe_lia_seal_of_power_shop`: item_lia_spear + item_lia_helm; свиток 850; сумма объявленных цен 1155, цена результата 1155. [item_lia_seal_of_power.txt:18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L18).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L24) |
| `ItemCost` | 1155 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L26) |
| `ItemKillable` | 0 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L27) |
| `ItemDroppable` | 1 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L28) |
| `ItemSellable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L29) |
| `ItemPurchasable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L30) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_TOGGLE \| DOTA_ABILITY_BEHAVIOR_NO_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 50 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L39) |
| `bonus_armor` | 15 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L40) |
| `damage_needed_for_one_armor` | 20 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L41) |
| `armor_needed_for_one_damage` | 0.5 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L42) |
| `damage_limit` | 200 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L43) |
| `armor_limit` | 30 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L44) |
| `stats_lose_percent` | 25 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L45) |
| `armor_for_dmg_tooltip` | 1 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L46) |
| `dmg_tooltip` | 2 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L47) |
| `armor_tooltip` | 2 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_seal_of_power.txt#L48) |

Lua: [SealOfPower.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/SealOfPower.lua#L1).

## Рецепт - `item_recipe_lia_shield_of_death`

Источник: [item_lia_shield_of_death.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=400`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L7) |
| `ItemCost` | 400 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L14) |
| `ItemResult` | item_lia_shield_of_death | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L16) |

## Щит Смерти - `item_lia_shield_of_death`

Источник: [item_lia_shield_of_death.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1060`.

Рецепт `item_recipe_lia_shield_of_death`: item_lia_crown_of_death + item_lia_amulet; свиток 400; сумма объявленных цен 1060, цена результата 1060. [item_lia_shield_of_death.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L26) |
| `ItemCost` | 1060 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L35) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L36) |
| `SpellImmunityType` | SPELL_IMMUNITY_ENEMIES_NO | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L37) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L38) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L39) |
| `AbilityCastPoint` | 0.0 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L41) |
| `AbilityCooldown` | 14.0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L42) |
| `AbilityManaCost` | 120 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L43) |
| `AbilityCastRange` | 300 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L45) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_magic_resist_percentage` | 30 | [103](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L103) |
| `bonus_health` | 450 | [104](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L104) |
| `damage` | 350 | [105](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L105) |
| `radius` | 300 | [106](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L106) |
| `active_armor` | 20 | [107](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L107) |
| `duration` | 6.0 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L108) |

Lua: [shield_of_death.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/shield_of_death.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_shield_of_death[1]/Properties[1]/MODIFIER_PROPERTY_MAGICAL_RESISTANCE_BONUS[1]` | %bonus_magic_resist_percentage | [96](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L96) |
| `Modifiers[1]/modifier_item_lia_shield_of_death[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_BONUS[1]` | %bonus_health | [97](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_death.txt#L97) |

## Рецепт - `item_recipe_lia_shield_of_endurance`

Источник: [item_lia_shield_of_endurance.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=350`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L7) |
| `ItemCost` | 350 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L14) |
| `ItemResult` | item_lia_shield_of_endurance | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L16) |

## Щит Выносливости - `item_lia_shield_of_endurance`

Источник: [item_lia_shield_of_endurance.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=625`.

Рецепт `item_recipe_lia_shield_of_endurance`: item_lia_ring_of_protection + item_lia_ring_of_regeneration + item_lia_amulet; свиток 350; сумма объявленных цен 625, цена результата 625. [item_lia_shield_of_endurance.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L26) |
| `ItemCost` | 625 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L35) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 5 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L54) |
| `bonus_health` | 250 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L55) |
| `bonus_health_regen` | 10 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L56) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_shield_of_endurance[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L46) |
| `Modifiers[1]/modifier_item_lia_shield_of_endurance[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_BONUS[1]` | %bonus_health | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L47) |
| `Modifiers[1]/modifier_item_lia_shield_of_endurance[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_REGEN_CONSTANT[1]` | %bonus_health_regen | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_shield_of_endurance.txt#L48) |

## Копьё - `item_lia_spear`

Источник: [item_lia_spear.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=110`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L7) |
| `ItemCost` | 110 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L11) |
| `ItemDroppable` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L14) |
| `ItemSellable` | 1 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L16) |
| `ItemPurchasable` | 1 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L17) |
| `ItemKillable` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L18) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 24 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L23) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_spear[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spear.txt#L36) |

## Рецепт - `item_recipe_lia_spellbreaker`

Источник: [item_lia_spellbreaker.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=500`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L7) |
| `ItemCost` | 500 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L14) |
| `ItemResult` | item_lia_spellbreaker | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L16) |

## Разрушитель Заклинаний - `item_lia_spellbreaker`

Источник: [item_lia_spellbreaker.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=1185`.

Рецепт `item_recipe_lia_spellbreaker`: item_lia_enchanted_shield_2 + item_lia_staff_of_power; свиток 500; сумма объявленных цен 2475, цена результата 1185. [item_lia_spellbreaker.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L26) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L33) |
| `AbilityCooldown` | 15.0 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L34) |
| `ItemKillable` | 0 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L36) |
| `ItemCost` | 1185 | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L37) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_armor` | 20 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L44) |
| `bonus_health` | 600 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L45) |
| `block_chance` | 50 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L46) |
| `damage_block` | 125 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L47) |
| `bonus_mana_regen` | 1.5 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L48) |
| `block_cooldown` | 15.0 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spellbreaker.txt#L49) |

Lua: [item_lia_spellbreaker.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_spellbreaker.lua#L1).

## имя не найдено - `item_recipe_ultimate_scepter`

Источник: [item_lia_spherical_staff.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=1000`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L7) |
| `ItemCost` | 1000 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L14) |
| `ItemResult` | item_ultimate_scepter | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L16) |

## Сферический Посох - `item_ultimate_scepter`

Источник: [item_lia_spherical_staff.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1620`.

Рецепт `item_recipe_ultimate_scepter`: item_lia_hell_mask + item_lia_staff; свиток 1000; сумма объявленных цен 1620, цена результата 1620. [item_lia_spherical_staff.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L26) |
| `ItemCost` | 1620 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_POINT \| DOTA_ABILITY_BEHAVIOR_AOE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L36) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L37) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L38) |
| `ShouldBeSuggested` | 0 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L41) |
| `AbilityCastRange` | 800 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L43) |
| `AbilityCooldown` | 45.0 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L44) |
| `AbilityManaCost` | 250 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L45) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana_regen` | 1 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L58) |
| `bonus_int` | 30 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L59) |
| `infernal_duration` | 60 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L60) |
| `stun_duration` | 2.7 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L61) |
| `radius` | 220 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L62) |
| `damage` | 300 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L63) |
| `bonus_all_stats` | 0 | [68](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L68) |
| `bonus_health` | 0 | [69](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L69) |
| `bonus_mana` | 0 | [70](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spherical_staff.txt#L70) |

Lua: [item_lia_spherical_staff.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_spherical_staff.lua#L1).

## Кольцо Паука - `item_lia_spider_ring`

Источник: [item_lia_spider_ring.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=160`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L8) |
| `ItemCost` | 160 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L12) |
| `ItemKillable` | 0 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L16) |
| `ItemDroppable` | 1 | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L17) |
| `ItemSellable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L19) |
| `ItemPurchasable` | 1 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L20) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `armor_reduction` | 3 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L24) |
| `duration` | 6 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_spider_ring.txt#L25) |

Lua: [item_lia_spider_ring.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_spider_ring.lua#L1).

## Посох - `item_lia_staff`

Источник: [item_lia_staff.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=200`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 200 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L17) |
| `BaseClass` | item_datadriven | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L18) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_intelligence` | 12 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L21) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_staff[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff.txt#L34) |

## Рецепт - `item_recipe_lia_staff_of_help`

Источник: [item_lia_staff_of_help.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=330`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L7) |
| `ItemCost` | 330 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L14) |
| `ItemResult` | item_lia_staff_of_help | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L16) |

## имя не найдено - `item_recipe_lia_staff_of_help_2`

Источник: [item_lia_staff_of_help.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L27) |
| `ItemCost` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L29) |
| `ItemRecipe` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L32) |
| `ItemResult` | item_lia_staff_of_help_2 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L34) |

## Посох Помощи - `item_lia_staff_of_help`

Источник: [item_lia_staff_of_help.txt:42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L42). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=680`.

Рецепт `item_recipe_lia_staff_of_help`: item_lia_mask + item_lia_staff + item_lia_ring_of_protection; свиток 330; сумма объявленных цен 680, цена результата 680. [item_lia_staff_of_help.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L44) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_AURA \| DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L46) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L47) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L48) |
| `MaxUpgradeLevel` | 2 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L56) |
| `ItemBaseLevel` | 1 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L57) |
| `AbilityCastRange` | 800 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L61) |
| `AbilityCastPoint` | 0.2 | [62](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L62) |
| `AbilityCooldown` | 12.0 | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L63) |
| `AbilityManaCost` | 150 / 300 | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L65) |
| `ItemCost` | 680 | [67](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L67) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana_regen` | 0.5 / 0.8 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L74) |
| `bonus_intelligence` | 20 / 25 | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L75) |
| `bonus_armor_aura` | 7 / 10 | [76](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L76) |
| `radius_aura` | 1000 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L77) |
| `healing` | 500 / 700 | [78](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L78) |
| `max_bounces` | 8 / 10 | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L79) |
| `bounce_range` | 1000 | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L80) |

Lua: [StaffOfHelp.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/StaffOfHelp.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_staff_of_help[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [117](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L117) |
| `Modifiers[1]/modifier_item_lia_staff_of_help[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT[1]` | %bonus_mana_regen | [118](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L118) |
| `Modifiers[1]/modifier_item_staff_of_help_armor[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | 5 | [129](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L129) |

## Посох Помощи - `item_lia_staff_of_help_2`

Источник: [item_lia_staff_of_help.txt:135](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L135). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=1010`.

Рецепт `item_recipe_lia_staff_of_help_2`: item_recipe_lia_staff_of_help + item_lia_staff_of_help; свиток 0; сумма объявленных цен 1010, цена результата 1010. [item_lia_staff_of_help.txt:37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L37).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [137](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L137) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_AURA \| DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [139](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L139) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [140](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L140) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [141](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L141) |
| `MaxUpgradeLevel` | 2 | [150](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L150) |
| `ItemBaseLevel` | 2 | [151](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L151) |
| `AbilityCastRange` | 800 | [154](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L154) |
| `AbilityCastPoint` | 0.2 | [155](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L155) |
| `AbilityCooldown` | 12.0 | [156](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L156) |
| `AbilityManaCost` | 300 | [158](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L158) |
| `ItemCost` | 1010 | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L160) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_mana_regen` | 0.8 | [167](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L167) |
| `bonus_intelligence` | 25 | [168](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L168) |
| `bonus_armor_aura` | 10 | [169](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L169) |
| `radius_aura` | 1000 | [170](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L170) |
| `healing` | 700 | [171](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L171) |
| `max_bounces` | 10 | [172](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L172) |
| `bounce_range` | 1000 | [173](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L173) |

Lua: [StaffOfHelp.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/StaffOfHelp.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_staff_of_help_2[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [210](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L210) |
| `Modifiers[1]/modifier_item_lia_staff_of_help_2[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT[1]` | %bonus_mana_regen | [211](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L211) |
| `Modifiers[1]/modifier_item_staff_of_help_armor_2[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | 8 | [225](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_help.txt#L225) |

## Рецепт - `item_recipe_lia_staff_of_illusions`

Источник: [item_lia_staff_of_illusions.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=600`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L7) |
| `ItemCost` | 600 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L14) |
| `ItemResult` | item_lia_staff_of_illusions | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L16) |

## Посох Иллюзий - `item_lia_staff_of_illusions`

Источник: [item_lia_staff_of_illusions.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1490`.

Рецепт `item_recipe_lia_staff_of_illusions`: item_lia_magic_staff + item_lia_mana_stone; свиток 600; сумма объявленных цен 1490, цена результата 1490. [item_lia_staff_of_illusions.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L26) |
| `ItemCost` | 1490 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_UNIT_TARGET | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L35) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_BOTH | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L36) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L37) |
| `AbilityCastRange` | 600 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L40) |
| `AbilityCooldown` | 18.0 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L42) |
| `AbilityManaCost` | 240 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L43) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_intelligence` | 28 | [71](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L71) |
| `bonus_mana_regen` | 1.4 | [72](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L72) |
| `bonus_mana` | 250 | [73](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L73) |
| `illusion_outgoing_damage` | 100 | [74](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L74) |
| `illusion_incoming_damage` | 200 | [75](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L75) |
| `illusion_outgoing_damage_tooltip` | 100 | [76](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L76) |
| `illusion_duration` | 10 | [77](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L77) |

Lua: [StaffOfIllusions.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/StaffOfIllusions.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_staff_of_illusions[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [63](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L63) |
| `Modifiers[1]/modifier_item_lia_staff_of_illusions[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT[1]` | %bonus_mana_regen | [64](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L64) |
| `Modifiers[1]/modifier_item_lia_staff_of_illusions[1]/Properties[1]/MODIFIER_PROPERTY_MANA_BONUS[1]` | %bonus_mana | [65](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_illusions.txt#L65) |

## имя не найдено - `item_recipe_lia_staff_of_life`

Источник: [item_lia_staff_of_life.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=500`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L7) |
| `ItemCost` | 500 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L14) |
| `ItemResult` | item_lia_staff_of_life | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L16) |

## Посох Жизни - `item_lia_staff_of_life`

Источник: [item_lia_staff_of_life.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1350`.

Рецепт `item_recipe_lia_staff_of_life`: item_lia_magic_staff + item_lia_ring_of_protection; свиток 500; сумма объявленных цен 1350, цена результата 1350. [item_lia_staff_of_life.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L26) |
| `ItemCost` | 1350 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L30) |
| `ItemSellable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L31) |
| `ItemPurchasable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L32) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_POINT \| DOTA_ABILITY_BEHAVIOR_AOE | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L36) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L37) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [38](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L38) |
| `AbilityCastRange` | 700 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L40) |
| `AbilityCooldown` | 15 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L41) |
| `AbilityManaCost` | 375 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L42) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_intelligence` | 25 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L55) |
| `bonus_mana_regen` | 1.0 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L56) |
| `bonus_armor` | 5 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L57) |
| `ability_radius` | 300 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L58) |
| `restore_limit` | 700 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L59) |
| `ability_duration` | 10 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_life.txt#L60) |

Lua: [item_lia_staff_of_life.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_staff_of_life.lua#L1).

## Рецепт - `item_recipe_lia_staff_of_light`

Источник: [item_lia_staff_of_light.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=700`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L7) |
| `ItemCost` | 700 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L14) |
| `ItemResult` | item_lia_staff_of_light | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L16) |

## Посох Света - `item_lia_staff_of_light`

Источник: [item_lia_staff_of_light.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1545`.

Рецепт `item_recipe_lia_staff_of_light`: item_lia_magic_staff + item_lia_ring_of_regeneration; свиток 700; сумма объявленных цен 1545, цена результата 1545. [item_lia_staff_of_light.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L26) |
| `ItemCost` | 1545 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_POINT \| DOTA_ABILITY_BEHAVIOR_AOE | [35](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L35) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_ENEMY | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L36) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_HERO \| DOTA_UNIT_TARGET_BASIC | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L37) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_PURE | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L39) |
| `AbilityCastRange` | 600 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L41) |
| `AbilityCooldown` | 15 | [42](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L42) |
| `AbilityManaCost` | 300 | [43](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L43) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_intelligence` | 35 | [188](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L188) |
| `bonus_mana_regen` | 1.2 | [189](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L189) |
| `ability_radius` | 300 | [190](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L190) |
| `ability_damage` | 50 | [191](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L191) |
| `ability_duration` | 10 | [192](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L192) |
| `aura_radius` | 550 | [193](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L193) |
| `aura_health_regen` | 40 | [194](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L194) |
| `aura_health_perc_max` | 50 | [195](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L195) |

Lua: [StaffOfLight.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/StaffOfLight.lua#L1); [StaffOfLight.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/StaffOfLight.lua#L1); [StaffOfLight.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/StaffOfLight.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_staff_of_light[1]/Properties[1]/MODIFIER_PROPERTY_STATS_INTELLECT_BONUS[1]` | %bonus_intelligence | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L82) |
| `Modifiers[1]/modifier_item_lia_staff_of_light[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT[1]` | %bonus_mana_regen | [83](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L83) |
| `Modifiers[1]/item_staff_of_light_aura_regen_modifier[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_REGEN_CONSTANT[1]` | %aura_health_regen | [182](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_staff_of_light.txt#L182) |

## Стальной Меч - `item_lia_steel_sword`

Источник: [item_lia_steel_sword.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=45`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `ItemCost` | 45 | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L7) |
| `ItemKillable` | 0 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L8) |
| `ItemDroppable` | 1 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L9) |
| `ItemSellable` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L11) |
| `ItemPurchasable` | 1 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L12) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L16) |
| `BaseClass` | item_datadriven | [17](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L17) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 12 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L20) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_steel_sword[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_steel_sword.txt#L33) |

## Рог Ветров - `item_lia_stormwind_horn`

Источник: [item_lia_stormwind_horn.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=250`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_AURA \| DOTA_ABILITY_BEHAVIOR_PASSIVE | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L7) |
| `AbilityUnitTargetTeam` | DOTA_UNIT_TARGET_TEAM_FRIENDLY | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L8) |
| `AbilityUnitTargetType` | DOTA_UNIT_TARGET_ALL | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L9) |
| `AbilityCastPoint` | 0.0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L11) |
| `AbilityCastRange` | 900 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L12) |
| `ItemCost` | 250 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L16) |
| `ItemKillable` | 0 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L20) |
| `ItemSellable` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L21) |
| `ItemPurchasable` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L22) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `radius` | 900 | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L26) |
| `bonus_attack_speed` | 15 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L27) |
| `bonus_movement_speed_percent` | 10 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L28) |
| `regen` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L29) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/horn_aura_armor[1]/Properties[1]/MODIFIER_PROPERTY_MOVESPEED_BONUS_PERCENTAGE_UNIQUE[1]` | %bonus_movement_speed_percent | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L53) |
| `Modifiers[1]/horn_aura_armor[1]/Properties[1]/MODIFIER_PROPERTY_ATTACKSPEED_BONUS_CONSTANT[1]` | %bonus_attack_speed | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_stormwind_horn.txt#L54) |

## Рецепт - `item_recipe_lia_sword_of_solidarity`

Источник: [item_lia_sword_of_solidarity.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=550`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L7) |
| `ItemCost` | 550 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L14) |
| `ItemResult` | item_lia_sword_of_solidarity | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L16) |

## имя не найдено - `item_recipe_lia_sword_of_solidarity_2`

Источник: [item_lia_sword_of_solidarity.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=0`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L27) |
| `ItemCost` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L29) |
| `ItemRecipe` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L32) |
| `ItemResult` | item_lia_sword_of_solidarity_2 | [34](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L34) |

## Меч Единства - `item_lia_sword_of_solidarity`

Источник: [item_lia_sword_of_solidarity.txt:41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L41). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=750`.

Рецепт `item_recipe_lia_sword_of_solidarity`: item_lia_steel_sword + item_lia_ring_of_regeneration + item_lia_ring_of_protection; свиток 550; сумма объявленных цен 750, цена результата 750. [item_lia_sword_of_solidarity.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L44) |
| `ItemCost` | 750 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L45) |
| `ItemKillable` | 0 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L46) |
| `ItemDroppable` | 1 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L47) |
| `ItemSellable` | 1 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L48) |
| `ItemPurchasable` | 1 | [49](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L49) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_NO_TARGET | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L52) |
| `AbilityCooldown` | 28 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L53) |
| `MaxUpgradeLevel` | 2 | [60](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L60) |
| `ItemBaseLevel` | 1 | [61](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L61) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 50 / 90 | [107](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L107) |
| `bonus_armor` | 5 / 10 | [108](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L108) |
| `bonus_health_regen` | 5 / 15 | [109](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L109) |
| `replenish_amount` | 200 / 400 | [110](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L110) |
| `active_armor` | 14 / 24 | [111](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L111) |
| `replenish_radius` | 800 | [112](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L112) |
| `duration` | 16.0 | [113](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L113) |

Lua: [SwordOfSolidarity.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/SwordOfSolidarity.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_sword_of_solidarity[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [85](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L85) |
| `Modifiers[1]/modifier_item_lia_sword_of_solidarity[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_REGEN_CONSTANT[1]` | %bonus_health_regen | [86](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L86) |
| `Modifiers[1]/modifier_item_lia_sword_of_solidarity[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [87](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L87) |
| `Modifiers[1]/modifier_sword_or_solidarity_armor[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS_UNIQUE_ACTIVE[1]` | %active_armor | [101](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L101) |

## Меч Единства - `item_lia_sword_of_solidarity_2`

Источник: [item_lia_sword_of_solidarity.txt:117](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L117). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1300`.

Рецепт `item_recipe_lia_sword_of_solidarity_2`: item_lia_sword_of_solidarity + item_recipe_lia_sword_of_solidarity; свиток 0; сумма объявленных цен 1300, цена результата 1300. [item_lia_sword_of_solidarity.txt:37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L37).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [120](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L120) |
| `ItemCost` | 1300 | [121](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L121) |
| `ItemKillable` | 0 | [122](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L122) |
| `ItemDroppable` | 1 | [123](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L123) |
| `ItemSellable` | 1 | [124](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L124) |
| `ItemPurchasable` | 1 | [125](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L125) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_NO_TARGET | [128](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L128) |
| `AbilityCooldown` | 28 | [129](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L129) |
| `MaxUpgradeLevel` | 2 | [136](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L136) |
| `ItemBaseLevel` | 2 | [137](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L137) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_damage` | 90 | [182](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L182) |
| `bonus_armor` | 10 | [183](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L183) |
| `replenish_radius` | 800 | [184](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L184) |
| `bonus_health_regen` | 15 | [185](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L185) |
| `replenish_amount` | 400 | [186](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L186) |
| `active_armor` | 24 | [187](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L187) |
| `duration` | 16.0 | [188](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L188) |

Lua: [SwordOfSolidarity.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/SwordOfSolidarity.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_sword_of_solidarity_2[1]/Properties[1]/MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE[1]` | %bonus_damage | [159](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L159) |
| `Modifiers[1]/modifier_item_lia_sword_of_solidarity_2[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_REGEN_CONSTANT[1]` | %bonus_health_regen | [160](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L160) |
| `Modifiers[1]/modifier_item_lia_sword_of_solidarity_2[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [161](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L161) |
| `Modifiers[1]/modifier_sword_or_solidarity_armor_2[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS_UNIQUE_ACTIVE[1]` | %active_armor | [175](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_sword_of_solidarity.txt#L175) |

## Кинжал Разбойника - `item_lia_thugs_dagger`

Источник: [item_lia_thugs_dagger.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=200`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [5](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L5) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_PASSIVE | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L7) |
| `ItemCost` | 200 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L11) |
| `ItemKillable` | 0 | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L15) |
| `ItemDroppable` | 1 | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L16) |
| `ItemSellable` | 1 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L18) |
| `ItemPurchasable` | 1 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L19) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_agility` | 12 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L23) |

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_thugs_dagger[1]/Properties[1]/MODIFIER_PROPERTY_STATS_AGILITY_BONUS[1]` | %bonus_agility | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_thugs_dagger.txt#L36) |

## Рецепт - `item_recipe_lia_totem_of_persistence`

Источник: [item_lia_totem_of_persistence.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=330`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L7) |
| `ItemCost` | 330 | [9](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L9) |
| `ItemKillable` | 0 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L12) |
| `ItemRecipe` | 1 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L14) |
| `ItemResult` | item_lia_totem_of_persistence | [16](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L16) |

## Тотем Стойкости - `item_lia_totem_of_persistence`

Источник: [item_lia_totem_of_persistence.txt:23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L23). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1635`.

Рецепт `item_recipe_lia_totem_of_persistence`: item_lia_dwarf_armor + item_lia_staff_of_power + item_lia_runed_bracers; свиток 330; сумма объявленных цен 1855, цена результата 1635. [item_lia_totem_of_persistence.txt:19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L19).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [26](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L26) |
| `ItemCost` | 1635 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L28) |
| `ItemKillable` | 0 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L29) |
| `ItemDroppable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L30) |
| `ItemSellable` | 1 | [32](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L32) |
| `ItemPurchasable` | 1 | [33](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L33) |
| `AbilityCooldown` | 22.0 | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L36) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L39) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_magic_resist_percentage` | 40 | [44](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L44) |
| `bonus_mana_regen` | 1.75 | [45](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L45) |
| `bonus_armor` | 15 | [46](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L46) |
| `bonus_health` | 250 | [47](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L47) |
| `duration` | 6.0 | [48](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L48) |

Lua: [TotemOfPersistence.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/TotemOfPersistence.lua#L1).

| Modifier property/state | Значение | Строка |
| --- | --- | --- |
| `Modifiers[1]/modifier_item_lia_totem_of_persistence[1]/Properties[1]/MODIFIER_PROPERTY_MAGICAL_RESISTANCE_BONUS[1]` | %bonus_magic_resist_percentage | [79](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L79) |
| `Modifiers[1]/modifier_item_lia_totem_of_persistence[1]/Properties[1]/MODIFIER_PROPERTY_MANA_REGEN_CONSTANT[1]` | %bonus_mana_regen | [80](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L80) |
| `Modifiers[1]/modifier_item_lia_totem_of_persistence[1]/Properties[1]/MODIFIER_PROPERTY_PHYSICAL_ARMOR_BONUS[1]` | %bonus_armor | [81](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L81) |
| `Modifiers[1]/modifier_item_lia_totem_of_persistence[1]/Properties[1]/MODIFIER_PROPERTY_HEALTH_BONUS[1]` | %bonus_health | [82](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L82) |
| `Modifiers[1]/modifier_item_totem_of_persistence_active[1]/Properties[1]/MODIFIER_PROPERTY_MAGICAL_RESISTANCE_BONUS[1]` | 100 | [95](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L95) |
| `Modifiers[1]/modifier_item_totem_of_persistence_active[1]/States[1]/MODIFIER_STATE_MAGIC_IMMUNE[1]` | MODIFIER_STATE_VALUE_ENABLED | [100](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_totem_of_persistence.txt#L100) |

## Тролль-Защитник - `item_lia_troll_defender`

Источник: [item_lia_troll_defender.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=125`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L7) |
| `ItemStockInitial` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L10) |
| `ItemStockMax` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L11) |
| `ItemStockTime` | 30 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L12) |
| `ItemCost` | 125 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L14) |
| `ItemPermanent` | 0 | [18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L18) |
| `AbilityCooldown` | 20 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L19) |
| `ItemKillable` | 0 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L20) |
| `ItemSellable` | 1 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L21) |
| `ItemDroppable` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L22) |
| `ItemInitialCharges` | 1 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L23) |
| `ItemStackable` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L24) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 45 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_defender.txt#L39) |

Lua: [TrollDefender.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/TrollDefender.lua#L1).

## Тролль-Лекарь - `item_lia_troll_healer`

Источник: [item_lia_troll_healer.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=не задано`; `ItemCost=90`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L6) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET | [7](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L7) |
| `ItemStockInitial` | 1 | [10](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L10) |
| `ItemStockMax` | 1 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L11) |
| `ItemStockTime` | 20 | [12](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L12) |
| `ItemCost` | 90 | [14](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L14) |
| `ItemPermanent` | 0 | [19](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L19) |
| `AbilityCooldown` | 20 | [20](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L20) |
| `ItemKillable` | 0 | [21](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L21) |
| `ItemSellable` | 1 | [22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L22) |
| `ItemDroppable` | 1 | [23](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L23) |
| `ItemInitialCharges` | 1 | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L24) |
| `ItemStackable` | 1 | [25](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L25) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `duration` | 45 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_troll_healer.txt#L40) |

Lua: [TrollHealer.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/TrollHealer.lua#L1).

## Рецепт - `item_recipe_lia_widow_boots`

Источник: [item_lia_widow_boots.txt:3](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L3). Включен engine-root: `True`.

В `shops.txt`: `False`; `ItemRecipe=1`; `ItemPurchasable=не задано`; `ItemCost=200`.

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_datadriven | [6](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L6) |
| `ItemCost` | 200 | [8](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L8) |
| `ItemKillable` | 0 | [11](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L11) |
| `ItemRecipe` | 1 | [13](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L13) |
| `ItemResult` | item_lia_widow_boots | [15](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L15) |

## Сапоги Вдовы - `item_lia_widow_boots`

Источник: [item_lia_widow_boots.txt:22](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L22). Включен engine-root: `True`.

В `shops.txt`: `True`; `ItemRecipe=не задано`; `ItemPurchasable=1`; `ItemCost=1300`.

Рецепт `item_recipe_lia_widow_boots`: item_lia_boots_of_invisibility + item_lia_demon_edge + item_lia_spider_ring; свиток 200; сумма объявленных цен 1300, цена результата 1300. [item_lia_widow_boots.txt:18](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L18).

| Поле | Декларация | Строка |
| --- | --- | --- |
| `BaseClass` | item_lua | [24](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L24) |
| `ItemCost` | 1300 | [27](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L27) |
| `ItemKillable` | 0 | [28](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L28) |
| `ItemDroppable` | 1 | [29](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L29) |
| `ItemSellable` | 1 | [30](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L30) |
| `ItemPurchasable` | 1 | [31](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L31) |
| `AbilityBehavior` | DOTA_ABILITY_BEHAVIOR_NO_TARGET \| DOTA_ABILITY_BEHAVIOR_IMMEDIATE \| DOTA_ABILITY_BEHAVIOR_IGNORE_BACKSWING \| DOTA_ABILITY_BEHAVIOR_IGNORE_CHANNEL | [36](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L36) |
| `AbilityUnitDamageType` | DAMAGE_TYPE_MAGICAL | [37](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L37) |
| `AbilityCastPoint` | 0 | [39](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L39) |
| `AbilityCooldown` | 18.0 | [40](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L40) |
| `AbilityManaCost` | 70 | [41](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L41) |

| Параметр | Декларация | Строка |
| --- | --- | --- |
| `bonus_attack_damage` | 60 | [52](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L52) |
| `bonus_movement_speed` | 60 | [53](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L53) |
| `armor_reduction` | 3 | [54](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L54) |
| `max_stacks` | 5 | [55](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L55) |
| `debuff_duration` | 6 | [56](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L56) |
| `invis_duration` | 6 | [57](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L57) |
| `invis_movespeed_percent` | 30 | [58](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L58) |
| `invis_bonus_damage` | 100 | [59](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/npc/items/item_lia_widow_boots.txt#L59) |

Lua: [item_lia_widow_boots.lua:1](https://github.com/ZLOY5/LiA/blob/012fab34e8c84ad0aa73cd4eadde1736e3c9df29/game/scripts/vscripts/items/item_lia_widow_boots.lua#L1).
