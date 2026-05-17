\# Content Design



> This document describes gameplay content rules and balancing foundations.

> It does NOT describe Unity architecture or implementation details.



\---



\# Design Principles



\- Gameplay should stay lightweight and readable.

\- Risk/reward is more important than complexity.

\- Temporary consumables are intentionally strong.

\- Losing progress is part of pacing.

\- Rare rewards should feel exciting but not mandatory.

\- The player should regularly make small tactical decisions.

\- Inventory and card limits are part of game balance.

\- Hoarding powerful consumables is allowed, but carries risk through debuffs and penalties.



\---

# Limits

## Equipment Limit

Player can equip a maximum of 3 items total.

Equipment slots are unrestricted:
- player may equip multiple weapons;
- player may equip multiple armor pieces;
- player may equip multiple artifacts.

## Card Hand Limit

Player can hold a maximum of 3 cards.

\---

\# Rarity



\## Common

Standard content.

Can appear from normal gameplay sources.



\## Rare

Stronger and less frequent content.

Usually appears from:

\- Rare events

\- Special rewards



\---



\# Effects



\## Effect Model



All gameplay content can contain multiple effects.



Examples:

\- An item may provide battle power and escape bonus.

\- A card may heal and grant movement.

\- An event may damage the player and remove a card.



Effects should be stackable and reusable across:

\- Items

\- Cards

\- Events

\- Monsters

\- Rewards



\---



\## Effect Types



\### Power

Adds temporary or permanent battle power.



\### HpRestore

Restores or removes HP.



\### EscapeBonus

Improves escape attempts or grants instant escape.



\### Level

Adds or removes player levels.



\### ChangePosition

Moves the player to another tile.



\### GiveCard

Adds cards to player hand.



\### RemoveCard

Removes cards from player hand.



\### GiveItem

Adds equipment to inventory.



\### RemoveItem

Removes equipment from inventory.



\---



\# Usage Context



Defines where a card can be used.



\## BattleOnly

Can only be used during battle.



\## BoardOnly

Can only be used on the board outside battle.



\## Anywhere

Can be used at any time.



\---



\# Items



\## Item Structure



Items should contain:

\- itemId

\- itemName

\- description

\- sprite

\- rarity

\- ItemType

\- effects (List<EffectData>)



\---



\## Equipment Slots



\### Weapon

Provides permanent battle power.

\### Armor

Prevents HP loss penalties once, then breaks.

Armor only protects from HP loss.
Armor does not protect from:
- level loss
- card removal
- item removal


\### Artifact

Provides utility bonuses such as:

\- battle power

\- escape bonus



\---


## Item Replacement Rules

If player receives a new item while all 3 equipment slots are occupied:
- the item cannot be equipped;
- the player may later replace items manually through inventory management systems.

For MVP:
- items are only granted if there is a free equipment slot.

\---


\## Weapons



| Item | Effect | Rarity |

|---|---|---|

| Dagger | +2 Power | Common |

| Bow | +3 Power | Common |

| Sword | +4 Power | Rare |

| Katana | +5 Power | Rare |



\---



\## Armor



| Item | Effect | Rarity |

|---|---|---|

| Helmet | Prevent HP damage once | Common |

| Chestplate | Prevent HP damage once | Common |

| Boots | Prevent HP damage once | Common |



\---



\## Artifacts



| Item | Effect | Rarity |

|---|---|---|

| Necklace | +2 Power | Common |

| Rabbit Tail | +1 Escape Bonus | Common |

| Winged Boot | +2 Escape Bonus | Rare |



\---



\# Cards



\## Card Structure



Cards should contain:

\- cardId

\- cardName

\- description

\- sprite

\- rarity

\- usageContext

\- effects (List<EffectData>)



\---



\# BattleOnly Cards



| Card | Effect | Rarity |

|---|---|---|

| Speed Potion | +1 Escape Bonus | Common |

| Strength Potion | +3 Power | Common |

| Greater Speed Potion | +2 Escape Bonus | Rare |

| Greater Strength Potion | +5 Power | Rare |

| Lost Door | Instant Escape | Rare |



\---



\# BoardOnly Cards



| Card | Effect | Rarity |

|---|---|---|

| Treasure Map | Move to nearest Buff tile | Common |

| Monster Contract | Move to nearest battle tile | Common |



\---



\# Anywhere Cards



| Card | Effect | Rarity |

|---|---|---|

| Rowan Brew | +1 HP | Common |

| Golden Apple | +2 HP | Common |

| Experience Potion | +1 Level | Common |

| Mentor Teaching | +2 Levels | Rare |

| Hot Springs Retreat | Fully restore HP | Rare |



\---



\# Enemies



\## Enemy Structure



Enemies should contain:

\- enemyId

\- enemyName

\- sprite

\- baseLevel

\- optional modifiers

\- penalty effects



\---



\# Enemy Modifiers



Modifiers increase monster power and create variety between encounters.



\---



\# Enemy Penalties



Penalties apply only after:

\- player loses battle;

\- player fails escape attempt.



Player level cannot drop below level 1.



\---



\# Enemy List



\## Slime — Level 1



Modifiers:

\- Large: +1 Power



Penalty:

\- None



\---



\## Bat — Level 2



Modifiers:

\- Fast: +1 Power



Penalty:

\- Remove random card



\---



\## Skeleton — Level 3



Modifiers:

\- Helmet: +1 Power

\- Sword: +2 Power



Penalty:

\- Lose 1 HP



\---



\## Zombie — Level 3



Modifiers:

\- Zombie Horde: +5 Power



Penalty:

\- Lose 1 Level



\---



\## Wolf — Level 4



Modifiers:

\- Hungry: +1 Power

\- Alpha: +2 Power



Penalty:

\- Lose 1 HP



\---



\## Orc — Level 5



Modifiers:

\- Axe: +2 Power

\- Rage: +2 Power



Penalty:

\- Lose 1 Level



\---



\## Skeleton Rider — Level 7



Modifiers:

\- Mace: +1 Power

\- Strong Horse: +2 Power



Penalty:

\- Lose 1 Level



\---



\## Siren — Level 9



Modifiers:

\- Charming: +2 Power



Penalty:

\- Lose 1 Level



\---



\## Basilisk — Level 10



Modifiers:

\- Poison Breath: +2 Power



Penalty:

\- Lose 1 HP and 1 Level



\---



\## Yeti — Level 12



Modifiers:

\- Enraged: +2 Power



Penalty:

\- Lose 2 HP



\---



\## Minotaur — Level 14



Modifiers:

\- Fast: +1 Power



Penalty:

\- Lose 2 HP



\---



\## Stone Golem — Level 15



Modifiers:

\- Hardened: +2 Power



Penalty:

\- Lose 2 HP



\---



\## Wyvern — Level 16



Modifiers:

\- Bloodthirsty: +2 Power



Penalty:

\- Lose 2 HP



\---



\## Dragon — Level 18



Modifiers:

\- Fire Breathing: +2 Power



Penalty:

\- Lose 2 HP



\---



\## Hydra — Level 20



Modifiers:

\- Multiheaded: +2 Power



Penalty:

\- Lose 2 HP and 1 Level



\---



\## Djinn — Level 22



Modifiers:

\- Ancient: +2 Power



Penalty:

\- Lose 2 HP and 1 Level



\---



\## Cthulha — Level 25



Penalty:

\- Lose 2 HP and 1 Level



\---



\# Events



\## Buff Events



Possible positive events:

\- Restore 1 HP

\- Restore 2 HP

\- Gain random common card

\- Gain 1 Level



\---



\## Debuff Events



Possible negative events:

\- Lose 1 HP

\- Lose 2 HP

\- Lose 1 Level

\- Remove random common card

Rare cards cannot be removed by standard debuff events unless explicitly stated.

\---



\## Random Events



Random events may contain:

\- Buff events

\- Debuff events



\---



\## Rare Events



Rare events are usually positive.



Possible rare rewards:

\- Gain rare equipment

\- Gain rare card

\- Full heal

\- Gain 2 levels


\---

# Reward Sources

Possible reward sources:
- Battle victories
- Buff tiles
- Rare events
- Random events

\---

# Battle Rules

- Player wins if total power is higher than enemy power.
- If player loses, escape attempt begins.
- Escape succeeds on 5–6.
- Failed escape applies monster penalty.

