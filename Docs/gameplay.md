# Gameplay

Current project is a 2D casual board game prototype with turn-based dice movement.

## Core Loop

Current loop:

1. Player clicks roll dice button.
2. `TurnSystem` starts a turn if state is `WaitingForRoll`.
3. `DiceSystem` rolls `1..6`.
4. `PlayerMover` moves player by rolled steps.
5. When movement finishes, `TurnSystem` asks `TileEffectSystem` to resolve the tile.
6. Most MVP tile effects write a `Debug.Log`.
7. Battle tiles open the battle modal and complete tile resolution after the battle flow closes.
8. Turn returns to `WaitingForRoll`.

There is currently one player token and one board path.

## Board

Scene: `Assets/Scenes/BoardGame.unity`

Board currently has 10 tiles:

- `Tile_00` -> `RareEvent`
- `Tile_01` -> `Buff`
- `Tile_02` -> `Debuff`
- `Tile_03` -> `Battle`
- `Tile_04` -> `RandomEvent`
- `Tile_05` -> `Buff`
- `Tile_06` -> `Battle`
- `Tile_07` -> `Debuff`
- `Tile_08` -> `Buff`
- `Tile_09` -> `Battle`

`BoardManager` stores these as an ordered serialized list and supports cyclic movement.

## Tile Types

Current `TileType` values:

- `RandomEvent`
- `RareEvent`
- `Battle`
- `Buff`
- `Debuff`

Current effect mapping:

- `RandomEvent` -> logs `Event tile resolved`
- `RareEvent` -> logs `Rare tile resolved`
- `Battle` -> starts `BattleSystem`
- `Buff` -> logs `Buff tile resolved`
- `Debuff` -> logs `Debuff tile resolved`

Random event, rare event, buff, and debuff effects do not currently change HP, open UI, start battle, or grant items.

Battle tiles start a functional MVP battle flow.

## Movement

Player object is child of the current tile.

`PlayerMover`:
- moves via coroutine;
- moves to the same local offset inside each target tile;
- stops briefly on each tile;
- calls callback after movement ends;
- does not resolve tile effects.

The player starts on `Tile_00`.

## HP and Level

`PlayerStats` is attached to `Player`.

Current values:
- `maxHp = 5`
- `currentHp = 5`
- `level = 1`

Available methods:
- `TakeDamage(int amount)`
- `Heal(int amount)`
- `SetLevel(int level)`

HP is clamped between `0` and `maxHp`.

HUD listens to `PlayerStats` events and updates:
- level text;
- 5 HP hearts.

Current gameplay stat changes:
- `Small Heal` restores 1 HP when used;
- battle wins grant +1 level;
- failed battle escapes can deal HP damage or reduce level depending on enemy penalty.

## Cards

Cards are MVP.

`CardData` ScriptableObject contains:
- `cardId`
- `cardName`
- `description`
- `cardSprite`

Current card assets:
- `Assets/Data/Cards/SmallHeal.asset`
- `Assets/Data/Cards/Shield.asset`
- `Assets/Data/Cards/LuckyHit.asset`

`CardSystem` stores current hand with max 3 cards.

Current behavior:
- displays up to 3 cards in hand;
- clicking a card calls `CardSystem.UseCard(card)`;
- card effects are dispatched by `CardData.cardId`;
- `Small Heal` restores 1 HP and is consumed;
- `Shield` and `Lucky Hit` currently only log placeholder effect messages and are consumed;
- cards are displayed as complete ready-made sprites.

## Inventory

Inventory is not implemented as gameplay.

Current state:
- HUD has 3 passive inventory slots;
- `InventorySlotView` can show empty state or assigned icon;
- no item data model;
- no item pickup;
- no equipment effects;
- no drag-and-drop.

## Battle System

Battle system is implemented as MVP.

Current battle tile behavior:
- landing on `Battle` starts `BattleSystem`;
- `BattleSystem` selects a random enemy from serialized `EnemyData` assets;
- `BattleModalView` shows player and enemy names, portraits, power entries, totals, status, and action button;
- resolving battle compares player total power and enemy total power;
- win grants +1 player level;
- loss asks for an escape roll;
- escape succeeds on `5..6`;
- failed escape applies the current enemy penalty;
- closing the modal completes tile resolution and the turn can end.

Current enemy assets:
- `Assets/Data/Enemies/Slime.asset`: level 1, no bonus, lose 1 HP penalty;
- `Assets/Data/Enemies/Bat.asset`: level 2, +1 bonus, lose 1 level penalty;
- `Assets/Data/Enemies/OrcWarrior.asset`: level 5, +1 bonus, lose 2 HP penalty.

Current player battle power:
- player level;
- serialized equipment bonus;
- serialized card bonus;
- serialized dice bonus;
- optional battle dice bonus if enemy power exceeds player power by `1..6`.

The roll dice button is reused during active battle only when `BattleSystem.CanUseBattleDice` is true.

Not implemented yet:
- separate combat scene;
- reward choices after victory;
- real equipment inventory bonuses;
- real card bonus integration;
- enemy behavior beyond static `EnemyData`;
- polished battle UI/animation.

## Event System

Event system is not implemented.

Current event tile behavior:
- `RandomEvent` logs `Event tile resolved`;
- `RareEvent` logs `Rare tile resolved`.

No event windows or random event tables exist yet.
