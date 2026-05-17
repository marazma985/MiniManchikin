# Current Progress

This document describes the current state of the Unity 6 board game project.

## Implemented

### Board

- `BoardGame.unity` scene exists.
- Board has 10 tiles: `Tile_00` through `Tile_09`.
- Each tile has `BoardTile` with `index` and `TileType`.
- `BoardManager` stores ordered serialized tile list.
- Board path is visualized through sprite segments under `BoardPath_Line`.
- Player starts on `Tile_00`.

### Turn Flow

- `DiceSystem` rolls `1..6`.
- `TurnSystem` coordinates turn states.
- Duplicate dice roll is blocked while not in `WaitingForRoll`.
- `PlayerMover` moves player by dice result.
- After movement, `TileEffectSystem` resolves current tile.
- Non-deferred tile effects complete immediately.
- Deferred tile effects, currently battle, complete through callback.
- Turn returns to `WaitingForRoll` after tile resolution completes.

### Tile Effects

Current effects:

- `EventTileEffect`
- `RareTileEffect`
- `BattleTileEffect`
- `BuffTileEffect`
- `DebuffTileEffect`

`EventTileEffect`, `RareTileEffect`, `BuffTileEffect`, and `DebuffTileEffect` are MVP and log only.

`BattleTileEffect` is a deferred tile effect. It starts `BattleSystem` and lets the turn continue only after battle completion.

`HealTileEffect` exists but is not currently mapped to a `TileType`.

### Battle

- `BattleSystem` exists on `BoardRoot`.
- `BattleSystem` is referenced by `TileEffectSystem`.
- Battle tiles start `BattleSystem` through `BattleTileEffect`.
- `BattleModalView` exists on inactive `Board UI Canvas/Battle Modal`.
- `BattleSystem` selects a random `EnemyData` asset.
- Current enemy assets are `Slime`, `Bat`, and `Orc Warrior`.
- Battle compares player total power against enemy total power.
- Winning battle grants +1 level.
- Losing battle asks for escape roll.
- Escape succeeds on `5..6`.
- Failed escape applies enemy penalty.
- Roll dice button can be reused for battle dice when the player is short by `1..6` power.
- Closing the battle modal completes tile resolution and allows the turn to end.

### Player Stats and HUD

- `PlayerStats` exists on `Player`.
- Current HP starts at `5`.
- Max HP is `5`.
- Level starts at `1`.
- `HudView` displays level, 5 hearts, and 3 passive inventory slots.
- `InventorySlotView` can show empty state or assigned icon.

### Cards

- `CardData` ScriptableObject exists.
- `CardSystem` stores max 3 cards in hand.
- `ICardEffect` exists as the card effect contract.
- `CardSystem.UseCard` dispatches by `CardData.CardId`.
- `CardHandView` displays card hand.
- `CardView` displays a complete card sprite and reacts to click.
- Clicking a card asks `CardSystem` to use it.
- Successfully used cards are removed from hand.
- `Small Heal` restores 1 HP through `PlayerStats.Heal(1)`.
- `Shield` and `Lucky Hit` currently resolve as placeholder log effects.

Current test cards:
- `Small Heal`
- `Shield`
- `Lucky Hit`

## Tested

Previously verified:

- dice roll returns values in `1..6`;
- roll button starts turn;
- duplicate roll is blocked during movement;
- tile effects dispatch and log correct messages;
- `PlayerStats.TakeDamage`, `Heal`, and `SetLevel` update HUD;
- inventory slot icon can be assigned and cleared;
- card hand displays 3 cards;
- clicking `CardSlot_01` logs `Card used: Small Heal`;
- `Small Heal` restores 1 HP and is consumed after successful use;
- console was clean after recent checks.

Checked during documentation sync:
- battle modal flow exists in scene and is wired to `BattleSystem`;
- `TileEffectSystem` references `BattleSystem`;
- roll dice button references both `TurnSystem` and `BattleSystem`;
- main menu button `onClick` lists are empty.

## Not Implemented Yet

- real event/buff/debuff tile effects;
- actual healing from tiles;
- event windows;
- random event table;
- rare event table;
- buff/debuff gameplay rules;
- battle reward choices;
- equipment bonus integration into real inventory;
- card bonus integration into real battle card effects;
- inventory gameplay model;
- item data assets;
- complete card effects for `Shield` and `Lucky Hit`;
- card cost/usage limits;
- card draw/discard pile;
- save/load;
- animations;
- final art;
- main menu button actions.

## Known MVP Limitations

- Most visuals are placeholder sprites.
- Card sprites are generated placeholders.
- Only `Small Heal` applies a real gameplay effect.
- `Shield` and `Lucky Hit` do not apply real gameplay effects yet.
- Inventory slots are passive visuals.
- Event, rare event, buff, and debuff tiles only log messages.
- Battle is functional MVP, but rewards/equipment/card integration are placeholders.
- `HealTileEffect` exists but is not connected to current `TileType`.
- Main menu is visual MVP; its buttons have no connected actions.

## Recommended Next Step

Recommended next development step:

Expand the existing battle MVP with real reward/equipment/card integration.

Suggested path:

1. Define battle victory rewards.
2. Decide how equipment bonuses are represented outside UI slots.
3. Define how `Shield` and `Lucky Hit` participate in battle.
4. Add the minimal supporting battle/card context needed for those effects.
5. Keep `CardView`, `CardHandView`, and `InventorySlotView` UI-only.

Alternative next step:

Connect main menu button actions for New Game/Exit while keeping save/load and settings as placeholders.
