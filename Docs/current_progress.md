# Current Progress

This document describes the current state of the Unity 6 board game project.

`GDD.md` and `content_design.md` are intentionally not mirrored here as implemented state. They remain vision/content-design documents.

## Implemented

### Board

- `BoardGame.unity` scene exists.
- Board has 10 tiles: `Tile_00` through `Tile_09`.
- Each tile has `BoardTile` with `index` and `TileType`.
- `BoardManager` stores ordered serialized tile list.
- Board path is visualized through sprite segments under `BoardPath_Line`.
- `BoardRoot/Board Background` displays the board background sprite behind the board.
- Player starts on `Tile_00`.

### Turn Flow

- `DiceSystem` rolls `1..6`.
- `TurnSystem` coordinates turn states.
- Duplicate dice roll is blocked while not in `WaitingForRoll`.
- `PlayerMover` moves player by dice result.
- `TurnSystem.TryMoveFixedSteps` supports fixed-step movement cards.
- After movement, `TileEffectSystem` resolves current tile.
- Deferred tile effects complete through callbacks.
- Turn returns to `WaitingForRoll` after tile resolution completes.

### Content Foundation

- `EffectType` exists.
- `EffectData` exists as the shared serializable effect structure.
- `Rarity` exists.
- `UsageContext` exists.
- `ItemType` exists and is a category, not a slot restriction.
- `CardData`, `ItemData`, and `EnemyData` use the new content model.

### Tile Effects

Current tile effect classes:
- `EventTileEffect`
- `RareTileEffect`
- `BattleTileEffect`
- `BuffTileEffect`
- `DebuffTileEffect`
- `HealTileEffect`

Current behavior:
- `BuffTileEffect` applies a random buff event through `EffectResolver`.
- `DebuffTileEffect` applies a random debuff event through `EffectResolver`.
- `EventTileEffect` chooses from buff/debuff pools.
- `RareTileEffect` chooses from rare event pool.
- `BattleTileEffect` starts `BattleSystem` and waits for battle completion.
- `HealTileEffect` exists but is not mapped to a current `TileType`.

Current effect pools are serialized on `TileEffectSystem` and have default entries for HP, level, card, and item events.

### Effect Resolver

- `EffectResolver` applies tile event `EffectData`.
- Supports `HpRestore`, `Level`, `GiveCard`, `RemoveCard`, and `GiveItem`.
- HP loss can be blocked by armor.
- Card/item reward effects open `SingleRewardSystem`.
- Common card removal respects `Rarity.Common`.
- Unsupported effects warn without breaking the turn.

### Player Stats and HUD

- `PlayerStats` exists on `Player`.
- Current HP starts at `5`.
- Max HP is `5`.
- Level starts at `1`.
- Level cannot go below `1`.
- `HudView` displays level, 5 hearts, and 3 equipped item slots.
- HUD listens to `PlayerStats` and `PlayerInventory`.

### Inventory And Items

- `PlayerInventory` exists.
- Player can equip up to 3 items total.
- Equipment item types are unrestricted.
- Equipped items are displayed in HUD slots.
- Equipped items can be manually removed with hover-fade X buttons.
- `PlayerInventory.GetTotalEffectValue` sums equipped item effects.
- `PlayerInventory.TryBreakArmorForHpLoss` breaks one armor item to prevent HP loss.

Current test item assets:
- `Dagger`
- `Helmet`
- `Necklace`
- `Winged Boot`

### Cards

- `CardData` ScriptableObject contains sprite, rarity, usage context, and `List<EffectData>`.
- `CardSystem` stores max 3 cards in hand.
- `CardSystem.UseCard` validates `UsageContext`.
- Effects are applied from `CardData.effects`.
- Cards are removed only after successful use.
- `CardHandView` displays card hand.
- `CardView` displays card sprites, smooth hover highlight, and remove X.
- Cards can be manually removed from the HUD.

Implemented card effects:
- `HpRestore`
- `Level`
- `Power`
- `EscapeBonus`
- `ChangePosition`

Current test card assets:
- `Small Heal`
- `Shield`
- `Lucky Hit`
- `Experience Potion`
- `Treasure Map`
- `Monster Contract`

Legacy hardcoded card effect files remain but are no longer the active `CardSystem` dispatch path.

### Battle

- `BattleSystem` exists on `BoardRoot`.
- Battle tiles start `BattleSystem` through `BattleTileEffect`.
- `BattleModalView` exists on inactive `Board UI Canvas/Battle Modal`.
- `BattleSystem` selects a random `EnemyData` asset.
- `BattleSystem` selects 0 or 1 random `EnemyModifier`.
- Enemy total power uses base level plus modifier `Power` effects.
- Player total power uses level, equipment power, card power, serialized bonus fields, and optional battle dice.
- Battle dice is available when enemy power minus player power is `0..6`.
- Winning battle grants +1 level.
- Winning battle opens `RewardSystem`.
- Losing battle asks for escape roll.
- Escape succeeds on final value `>= 5`.
- Escape bonus includes equipped item effects and temporary card escape bonus.
- Failed escape applies enemy `penaltyEffects`.
- Armor protects only from HP loss penalties and then breaks.

Current enemy assets:
- `Slime`
- `Bat`
- `Orc Warrior`

### Rewards

- `RewardSystem` exists.
- Battle victory opens a 3-choice reward modal.
- Reward choices can be card or item rewards.
- Card reward uses `CardSystem.AddCard`.
- Item reward uses `PlayerInventory.TryEquip`.
- Full hand/inventory keeps reward modal open and shows status text.
- Reward modal can be closed/skipped.

### Single Rewards

- `SingleRewardSystem` exists.
- `SingleRewardModalView` displays one card/item reward.
- Tile `GiveCard` and `GiveItem` effects use the single reward modal.
- Accept is disabled when hand/equipment is full.
- Removing card/item from HUD updates Accept availability.
- Close declines reward and continues tile flow.

### Event Notifications

- `EventNotificationSystem` exists on `BoardRoot`.
- `EventNotificationView` exists as a UI notification item.
- Notifications spawn under `Board UI Canvas/Event Notification Container`.
- Notification settings are configured per `EffectType`.
- Notifications support success, blocked, no-effect, and failed messages.
- Notification animation uses coroutine lifetime, fade, upward movement, and destroy.

### Main Menu

- `MainMenu.unity` exists.
- Sprite-based buttons exist.
- Continue can be visually locked.
- Custom cursor exists.
- Button hover/press feedback exists.

## Tested / Recently Verified

- Board hierarchy contains current gameplay UI objects.
- `BoardBackground_0` sprite is referenced by `Board Background`.
- Card assets use `List<EffectData>`.
- Item assets use `ItemType` and `List<EffectData>`.
- Enemy assets use `baseLevel`, `modifiers`, and `penaltyEffects`.
- Reward and single reward systems are present in scene/scripts.
- Event notification settings exist per `EffectType`.
- `CardSystem` no longer dispatches by hardcoded `cardId`.
- No Docs update touched `GDD.md` or `content_design.md`.

## MVP / Placeholder

- Reward generation is simple random selection.
- Reward rarity weighting is not implemented.
- Reward duplicate protection is not implemented.
- Single reward modal is functional MVP only.
- Item inventory has no inventory window.
- Equipment has no replacement flow.
- Helmet currently works as armor through `ItemType.Armor`, not through an explicit armor effect.
- Old `EnemyData` fields `bonusPower`, `penaltyType`, and `penaltyValue` remain serialized for compatibility.
- Legacy card effect classes remain in the project but are not used by active card gameplay.
- Main menu is visual MVP; buttons have no connected actions.
- Art is a mix of placeholder, test, and newer board/menu assets.

## Not Implemented Yet

- save/load;
- settings menu;
- inventory window;
- item replacement flow;
- card draw/discard pile;
- reward weighting and rarity balancing;
- rare reward chains;
- event modal text/flavor;
- final battle animations;
- final UI polish;
- final balance pass;
- main menu New Game/Continue/Settings/Exit behavior.
