# Gameplay

Current project is a Unity 6 2D casual board game prototype with turn-based dice movement, board tile events, card/item effects, battle, and MVP reward flows.

`Docs/GDD.md` and `Docs/content_design.md` describe vision and content rules. This file describes only what currently exists in the project.

## Core Loop

Current turn loop:

1. Player clicks the roll dice button.
2. `TurnSystem` starts a turn if state is `WaitingForRoll`.
3. `DiceSystem` rolls `1..6`.
4. `PlayerMover` moves the player by rolled steps.
5. `TurnSystem` asks `TileEffectSystem` to resolve the reached tile.
6. Tile effects either apply immediately or wait for a deferred callback.
7. Battle/reward/single reward flows complete through callbacks.
8. Turn returns to `WaitingForRoll`.

There is currently one player token and one cyclic board path.

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

`BoardManager` stores these as an ordered serialized list and supports cyclic movement. It also supports forward lookup for nearest tile type, currently used by movement cards.

`BoardRoot/Board Background` displays `Assets/Art/Board/BoardBackground.png` behind the board through a low `SpriteRenderer` sorting order.

`Board Back Button` is a world-space button in the top-right camera area. It returns to `MainMenu`, is included in modal blur, and is blocked while battle/reward modals are active.

## Tile Types And Effects

Current `TileType` values:

- `RandomEvent`
- `RareEvent`
- `Battle`
- `Buff`
- `Debuff`

Current effect mapping:

- `RandomEvent` -> random event from buff/debuff pools.
- `RareEvent` -> random event from rare pool.
- `Battle` -> starts `BattleSystem`.
- `Buff` -> random buff event.
- `Debuff` -> random debuff event.

`Buff`, `Debuff`, `RandomEvent`, and `RareEvent` now use `EffectData` through `EffectResolver`.

Current buff event pool:
- restore 1 HP;
- restore 2 HP;
- offer random common card through single reward modal;
- gain 1 level.

Current debuff event pool:
- lose 1 HP;
- lose 2 HP;
- lose 1 level;
- remove random common card.

Current rare event pool:
- offer random rare equipment through single reward modal;
- offer random rare card through single reward modal;
- full heal;
- gain 2 levels.

RandomEvent chooses between buff/debuff pools with MVP 50/50 behavior when both are available.

## Movement

Player object is child of the current tile.

`PlayerMover`:
- moves via coroutine;
- moves to the same local offset inside each target tile;
- stops briefly on each tile;
- calls callback after movement ends;
- does not resolve tile effects itself.

The player starts on `Tile_00`.

Movement cards use the regular movement flow:
- `Treasure Map` moves to the nearest `Buff` tile.
- `Monster Contract` moves to the nearest `Battle` tile.

After movement card movement completes, the destination tile effect resolves normally.

## HP And Level

`PlayerStats` is attached to `Player`.

Current values:
- `maxHp = 5`
- `currentHp = 5`
- `level = 1`

HP is clamped between `0` and `maxHp`.

Level is clamped to minimum `1`.

Current stat changes:
- card effects can restore HP or change level;
- buff/debuff/rare events can restore HP, deal HP damage, full heal, gain level, or lose level;
- battle wins grant +1 level;
- failed battle escape can apply enemy penalties.

Armor can block HP loss from debuff/enemy HP penalties once, then breaks.

Game result checks:
- reaching level `10` opens the win result scene;
- reaching `0` HP opens the lose result scene.

## Cards

Cards are data-driven through `CardData.effects`.

`CardSystem` stores max 3 cards in hand.

`UsageContext` rules:
- `BattleOnly` cards can be used only during battle;
- `BoardOnly` cards can be used only outside battle;
- `Anywhere` cards can be used in either context.

If a card cannot be used in the current context or an effect cannot be applied, the whole card use fails and the card is not removed.

Implemented card effects:
- `HpRestore`
- `Level`
- `Power`
- `EscapeBonus`
- `ChangePosition`

Current card assets:
- `Small Heal`: common, Anywhere, `HpRestore +1`.
- `Shield`: common, BattleOnly, `EscapeBonus +1`.
- `Lucky Hit`: rare, BattleOnly, `Power +3`.
- `Experience Potion`: common, Anywhere, `Level +1`.
- `Treasure Map`: common, BoardOnly, nearest `Buff` tile.
- `Monster Contract`: common, BoardOnly, nearest `Battle` tile.

Cards can also be manually removed from the HUD using the small hover-fade X button.

Legacy `SmallHealCardEffect`, `ShieldCardEffect`, and `LuckyHitCardEffect` files still exist but are not the active `CardSystem` effect path.

## Items And Equipment

Items are represented by `ItemData` ScriptableObject assets.

Current item assets:
- `Dagger`: common weapon, `Power +2`.
- `Helmet`: common armor, no listed `EffectData`, but protects because `ItemType.Armor` is checked at runtime.
- `Necklace`: common artifact, `Power +1`.
- `Winged Boot`: rare artifact, `EscapeBonus +2`.

`PlayerInventory` stores currently equipped items:
- max 3 items total;
- item types are unrestricted categories, not slots;
- any mix of weapons, armor, and artifacts can be equipped.

Implemented item gameplay:
- equipped `Power` effects add to battle power;
- equipped `EscapeBonus` effects add to escape roll;
- armor blocks one HP loss penalty, then unequips/breaks.

Equipped items can be manually removed from HUD slots using the small hover-fade X button.

Not implemented:
- inventory window;
- item replacement flow;
- item drag/drop;
- game-state save/load persistence.

## Battle System

Battle system is functional MVP.

Current battle tile behavior:
- landing on `Battle` starts `BattleSystem`;
- a random `EnemyData` asset is selected;
- 0 or 1 enemy modifier is selected randomly;
- `BattleModalView` shows player and enemy names, portraits, separate power rows, separate total rows, status, and action button;
- the action button dynamically shows `Победа` when player power is higher, otherwise `Пытаться сбежать`;
- clicking `Победа` immediately grants victory and opens reward choices;
- clicking `Пытаться сбежать` rolls escape immediately;
- win requires player total power to be greater than enemy total power;
- win grants +1 level and opens battle reward choices;
- escape succeeds when base roll + escape bonuses is at least `5`;
- failed escape applies enemy `penaltyEffects`;
- closing/finishing the flow completes tile resolution.

Current enemy assets:
- `Slime`: base level 1, modifier `Large` with `Power +1`, no penalty effects.
- `Bat`: base level 2, modifier `Fast` with `Power +1`, penalty `RemoveCard 1`.
- `Orc`: base level 5, modifiers `Axe` and `Rage` with `Power +2`, penalty `Level -1`.

Current player battle power:
- player level;
- equipment `Power` effects;
- serialized equipment bonus field;
- temporary card `Power`;
- serialized card bonus field;
- optional battle dice bonus.

The roll dice button is reused for battle dice when battle dice is available. Battle dice can be used when the enemy total exceeds or equals player total by `0..6`.

Display details:
- player equipment row is hidden when the total equipment bonus is `0`;
- enemy modifier row uses the modifier name directly;
- dice/card hints are temporary for 2.5 seconds;
- escape result hints stay visible until the modal closes.

## Rewards

After battle victory:
- battle modal hides;
- `RewardSystem` opens `Reward Modal`;
- 3 random rewards are generated from serialized card/item reward pools;
- each option displays icon, name, and description;
- player chooses one reward or closes/skips the modal;
- card reward uses `CardSystem.AddCard`;
- item reward uses `PlayerInventory.TryEquip`;
- if hand/equipment is full, reward is not claimed and the modal stays open with a status message.

Single reward flow:
- `GiveCard` and `GiveItem` tile effects use `SingleRewardSystem`;
- `Single Reward Modal` shows one reward;
- reward description is shared through `RewardData.DisplayDescription`;
- Accept is disabled when hand/equipment is full;
- removing a card/item through HUD updates Accept state;
- closing declines the reward and continues the tile flow.

Current reward generation is MVP random selection. There is no weighting, duplicate protection, or reward chain logic.

## Event Notifications

Applied effects can show visual notifications through `EventNotificationSystem`.

Examples:
- `+1 HP`
- `-1 HP`
- `+1 Level`
- `Card gained`
- `Card removed`
- `Item gained`
- `HP loss blocked`

Notifications are data-driven by `EffectType` settings on `EventNotificationSystem`. They support success, blocked, no-effect, and failed statuses.

## Result Screen

`GameResultSystem` watches `PlayerStats` during board gameplay. When the player reaches the win or lose condition, it stores the result in `GameResultContext` and loads `ResultGameScene`.

Current result behavior:
- win condition: player level reaches `10`;
- lose condition: player HP reaches `0`;
- result scene chooses win/lose art through `ResultGameScreenController`;
- result scene can return to `MainMenu`.

## Main Menu And Settings

Current main menu behavior:
- New Game loads `BoardGame`;
- Settings opens a square modal over blurred background;
- Settings can save window size, fullscreen mode, and music volume;
- music volume is stored in `PlayerPrefs` and applied to `MainAudioMixer`;
- Continue and Exit are still visual-only.

Window size options are currently `1280x720`, `1600x900`, and `1920x1080`. Default music volume is `0.5`.

## Cursor

The custom cursor is loaded globally from `Assets/Resources/UI/GlobalCursor.prefab` when a scene does not already contain one.

Current behavior:
- cursor follows the mouse and hides the OS cursor while focused;
- hover state is based on selectable UI under the pointer;
- press state plays on any mouse press, including empty space.

## Not Implemented Yet

- game-state save/load;
- inventory window;
- reward weighting/rarity balancing;
- item replacement flow;
- rare reward chains;
- card draw/discard piles;
- final battle/event animations;
- final balancing;
- main menu Continue/Exit actions.
