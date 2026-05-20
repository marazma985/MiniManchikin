# Architecture

Current project: Unity 6, 2D casual board game with turn-based board movement inspired by Munchkin.

Main scene: `Assets/Scenes/BoardGame.unity`

Vision documents:
- `Docs/GDD.md` describes long-term game vision and planned mechanics.
- `Docs/content_design.md` describes content rules and future content design.

Those files are not a source of truth for implemented runtime state. Current implementation is defined by scenes, scripts, ScriptableObject assets, and this technical documentation.

## Core Scene Structure

Current `BoardGame.unity` root objects:
- `BoardRoot`
- `Main Camera`
- `DiceSystem`
- `Board UI Canvas`
- `EventSystem`
- `CardSystem`

Important children:
- `BoardRoot/Board Background`
- `BoardRoot/BoardPath_Line`
- `BoardRoot/Tile_00` ... `Tile_09`
- `Tile_00/Player`
- `Board UI Canvas/Battle Modal`
- `Board UI Canvas/Reward Modal`
- `Board UI Canvas/Single Reward Modal`
- `Board UI Canvas/Event Notification Container`
- `Board UI Canvas/Roll Dice Button`
- `Board UI Canvas/Player HUD`
- `Board UI Canvas/CardHand`

Dependencies are assigned through serialized Inspector references, with local component fallback only where the component is required on the same GameObject.

## Content Foundation

Files:
- `Assets/Scripts/Board/EffectType.cs`
- `Assets/Scripts/Board/EffectData.cs`
- `Assets/Scripts/Board/Rarity.cs`
- `Assets/Scripts/Board/UsageContext.cs`
- `Assets/Scripts/Board/ItemType.cs`

Implemented effect types:
- `Power`
- `HpRestore`
- `EscapeBonus`
- `Level`
- `ChangePosition`
- `GiveCard`
- `RemoveCard`
- `GiveItem`
- `RemoveItem`

`EffectData` is the shared serializable foundation used by card effects, item effects, enemy modifiers, enemy penalties, and tile events. It stores effect type, numeric value, optional content id, rarity filter, tile target data, random target flag, instant flag, and full-heal flag.

No giant runtime effect manager exists. Shared effect application currently lives in the small `EffectResolver` used by tile events and single reward sources.

## Board And Turns

Files:
- `Assets/Scripts/Board/BoardTile.cs`
- `Assets/Scripts/Board/BoardManager.cs`
- `Assets/Scripts/Board/DiceSystem.cs`
- `Assets/Scripts/Board/PlayerMover.cs`
- `Assets/Scripts/Board/TurnSystem.cs`
- `Assets/Scripts/Board/TurnState.cs`

Responsibilities:
- `BoardTile` stores stable tile index and `TileType`.
- `BoardManager` owns ordered tile list, current index, cyclic movement, and nearest tile lookup.
- `DiceSystem.Roll()` returns `1..6` and has no UI logic.
- `PlayerMover` moves the player by coroutine and invokes a callback when movement completes.
- `TurnSystem` coordinates dice roll, movement, tile resolution, and turn completion.

`TurnSystem.TryMoveFixedSteps(int steps)` is used by movement cards so `ChangePosition` cards still go through the normal movement and tile resolution flow.

## Tile Effects

Files:
- `Assets/Scripts/Board/TileEffectSystem.cs`
- `Assets/Scripts/Board/ITileEffect.cs`
- `Assets/Scripts/Board/IDeferredTileEffect.cs`
- `Assets/Scripts/Board/BattleTileEffect.cs`
- `Assets/Scripts/Board/BuffTileEffect.cs`
- `Assets/Scripts/Board/DebuffTileEffect.cs`
- `Assets/Scripts/Board/EventTileEffect.cs`
- `Assets/Scripts/Board/RareTileEffect.cs`
- `Assets/Scripts/Board/HealTileEffect.cs`
- `Assets/Scripts/Board/EffectResolver.cs`

Current mapping:
- `RandomEvent` -> `EventTileEffect`
- `RareEvent` -> `RareTileEffect`
- `Battle` -> `BattleTileEffect`
- `Buff` -> `BuffTileEffect`
- `Debuff` -> `DebuffTileEffect`

Current behavior:
- `BuffTileEffect` chooses one random buff `EffectData`.
- `DebuffTileEffect` chooses one random debuff `EffectData`.
- `EventTileEffect` chooses between buff/debuff pools with MVP 50/50 behavior when both pools exist.
- `RareTileEffect` chooses one random rare event.
- `BattleTileEffect` starts `BattleSystem`.
- Deferred tile effects complete the turn only after their callback is invoked.

`HealTileEffect` still exists but is not mapped to a tile type.

## Effect Resolver

File: `Assets/Scripts/Board/EffectResolver.cs`

`EffectResolver` is used by tile event effects and single reward effects. It supports:
- `HpRestore`
- `Level`
- `GiveCard`
- `RemoveCard`
- `GiveItem`

Current rules:
- positive `HpRestore` heals;
- negative `HpRestore` damages unless armor blocks it;
- full heal uses `EffectData.RestoreToFull`;
- `Level` clamps through `PlayerStats.SetLevel`;
- `GiveCard` and `GiveItem` open `SingleRewardSystem`;
- `RemoveCard` removes random cards matching `RarityFilter`.

Unsupported effect types log a warning and do not break the turn flow.

## Player Stats

File: `Assets/Scripts/Board/PlayerStats.cs`

`PlayerStats` is attached to `Player`.

Responsibilities:
- stores `currentHp`, `maxHp`, and `level`;
- max HP is currently `5`;
- clamps HP to `0..maxHp`;
- clamps level to minimum `1`;
- exposes `TakeDamage`, `Heal`, and `SetLevel`;
- exposes HP and level change events.

## Inventory And Items

Files:
- `Assets/Scripts/Board/PlayerInventory.cs`
- `Assets/Scripts/Board/ItemData.cs`
- `Assets/Scripts/Board/ItemType.cs`
- `Assets/Scripts/Board/InventorySlotView.cs`

`ItemData` is a ScriptableObject with:
- item id;
- item name;
- description;
- item sprite;
- rarity;
- item type;
- list of `EffectData`.

`ItemType` is a category, not an equipment slot:
- `Weapon`
- `Armor`
- `Artifact`

Equipment slots are unrestricted. `PlayerInventory` stores up to 3 equipped items total, with any mix of item types.

Implemented item gameplay:
- `Power` effects add battle power.
- `EscapeBonus` effects add to escape roll.
- `ItemType.Armor` blocks one HP loss penalty, then breaks and is unequipped.

No inventory window, item replacement flow, drag-and-drop, or save/load exists yet.

## Card System

Files:
- `Assets/Scripts/Board/CardData.cs`
- `Assets/Scripts/Board/CardSystem.cs`
- `Assets/Scripts/Board/CardHandView.cs`
- `Assets/Scripts/Board/CardView.cs`
- legacy files: `ICardEffect.cs`, `SmallHealCardEffect.cs`, `ShieldCardEffect.cs`, `LuckyHitCardEffect.cs`

`CardData` is a ScriptableObject with:
- card id;
- card name;
- description;
- card sprite;
- rarity;
- usage context;
- list of `EffectData`.

`CardSystem` stores max 3 cards in hand. It no longer depends on hardcoded `cardId` effect dispatch. It validates `UsageContext`, checks all effects before applying, applies effects, and removes the card only after successful application.

Implemented card effects:
- `HpRestore`
- `Level`
- `Power`
- `EscapeBonus`
- `ChangePosition`

`ChangePosition` supports nearest matching tile movement through `BoardManager.TryGetForwardDistanceToNearestTileType` and `TurnSystem.TryMoveFixedSteps`.

Legacy card effect classes still exist in the project but are not the active card gameplay path.

## Battle System

Files:
- `Assets/Scripts/Board/BattleSystem.cs`
- `Assets/Scripts/Board/BattleModalData.cs`
- `Assets/Scripts/Board/BattleModalView.cs`
- `Assets/Scripts/Board/BattlePowerEntry.cs`
- `Assets/Scripts/Board/EnemyData.cs`
- `Assets/Scripts/Board/EnemyModifier.cs`
- `Assets/Scripts/Board/MonsterPenaltyType.cs`

`BattleSystem` starts from `BattleTileEffect`, selects a random enemy, selects 0 or 1 random enemy modifier, builds modal data, compares power, resolves escape, applies penalties, and opens battle rewards after victory.

Player battle power currently includes:
- player level;
- item `Power` effects;
- serialized equipment bonus field;
- temporary card power bonus;
- serialized card bonus field;
- optional battle dice bonus.

Battle dice can be used when the enemy total exceeds or equals player total by `0..6` power.

Enemy total power currently includes:
- `EnemyData.BaseLevel`;
- selected `EnemyModifier` `Power` effects.

Enemy penalty effects currently support:
- negative `HpRestore` as HP loss;
- negative `Level` as level loss;
- `RemoveCard` as random common card removal.

Old `EnemyData` fields `bonusPower`, `penaltyType`, and `penaltyValue` remain serialized for compatibility but are not the current battle source of truth.

## Rewards

Files:
- `Assets/Scripts/Board/RewardType.cs`
- `Assets/Scripts/Board/RewardData.cs`
- `Assets/Scripts/Board/RewardSystem.cs`
- `Assets/Scripts/Board/RewardModalView.cs`
- `Assets/Scripts/Board/RewardView.cs`
- `Assets/Scripts/Board/SingleRewardSystem.cs`
- `Assets/Scripts/Board/SingleRewardModalView.cs`

Battle victory rewards:
- `RewardSystem` generates up to 3 random rewards from serialized card/item pools.
- `RewardModalView` shows those rewards.
- Selecting a card calls `CardSystem.AddCard`.
- Selecting an item calls `PlayerInventory.TryEquip`.
- If hand/equipment is full, the modal remains open and displays a status message.
- The reward modal can be closed/skipped.

Single reward flow:
- `SingleRewardSystem` shows one card/item reward from buff/rare event effects.
- Accept is disabled when hand/equipment is full.
- The modal updates when cards/equipment are manually removed.
- Close declines the reward and completes the tile effect flow.

No rarity weighting, duplicate protection, reward chains, or save/load exists yet.

## Event Notifications

Files:
- `Assets/Scripts/Board/EventNotificationSystem.cs`
- `Assets/Scripts/Board/EventNotificationView.cs`

`EventNotificationSystem` displays small notifications on the left side of the board UI. It is data-driven per `EffectType`.

Each notification setting has:
- effect type;
- icon;
- show/hide flag;
- positive message template;
- negative message template;
- blocked message template;
- no-effect message template;
- failed message template.

Notifications support success, blocked, no-effect, and failed statuses. Existing animation behavior uses coroutines: short lifetime, fade, move up, then destroy.

## HUD And UI Views

Files:
- `Assets/Scripts/Board/HudView.cs`
- `Assets/Scripts/Board/InventorySlotView.cs`
- `Assets/Scripts/Board/CardHandView.cs`
- `Assets/Scripts/Board/CardView.cs`
- `Assets/Scripts/Board/DiceRollButtonController.cs`

UI classes display data and forward user intent. They do not own gameplay rules.

Implemented UI behavior:
- roll button starts turn or battle dice when available;
- HUD displays level, HP hearts, and equipped item slots;
- inventory slots show item sprites and have hover-fade remove buttons;
- card views show card sprites, apply smooth hover highlight, and have hover-fade remove buttons;
- card remove buttons call `CardSystem.RemoveCard`;
- inventory remove buttons call `PlayerInventory.Unequip`.

## Main Menu UI

Scene: `Assets/Scenes/MainMenu.unity`

Files:
- `Assets/Scripts/UI/MainMenuSpriteButton.cs`
- `Assets/Scripts/UI/MainMenuButtonFeedback.cs`
- `Assets/Scripts/UI/MainMenuCursor.cs`
- `Assets/Scripts/UI/MainMenuCursorHoverTarget.cs`
- `Assets/Scripts/UI/MainMenuSkinSlots.cs`

Current behavior:
- sprite-based main menu visual layout;
- Continue can be locked through `continueAvailable`;
- buttons have hover feedback;
- custom cursor changes state on hover/press.

Current limitation:
- menu button `onClick` handlers are empty;
- no scene loading, settings, continue, save/load, or exit behavior is connected yet;
- `MainMenuCursor` uses a static `Instance` bridge for main menu UI only.

## Implemented vs Placeholder

Implemented:
- board movement and turn loop;
- tile effect dispatch with deferred callbacks;
- unified content effect data foundation;
- card effects through `EffectData`;
- item equipment and item battle effects;
- enemy modifiers and penalties through `EffectData`;
- battle modal flow, escape roll, battle dice, rewards;
- buff/debuff/random/rare tile effects through effect pools;
- single reward modal for tile reward events;
- event notifications;
- manual card/item removal from HUD.

MVP or placeholder:
- reward generation is random and unweighted;
- single reward modal is functional only;
- item inventory has no full inventory window;
- old serialized enemy fields remain for compatibility;
- legacy hardcoded card effect classes remain unused;
- Main Menu is visual-only;
- visuals are still mixed placeholder/final-in-progress.

Not implemented:
- save/load;
- settings menu;
- inventory window;
- item replacement flow;
- rare reward chains;
- polished animations;
- final balancing;
- final art pass.
