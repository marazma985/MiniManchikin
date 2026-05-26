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
- `Board Back Button` exists as a world-space sprite object outside `Board UI Canvas`.
- Board back button returns to `MainMenu` and is blocked while modal roots are active.
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
- HUD hearts and inventory slot frames use sliced sprites from `PlayerHudList.png`.
- `GameResultSystem` listens to HP and level changes for win/lose conditions.

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
- Player equipment power row is hidden when its value is `0`.
- Power entries are rendered as separate row UI elements.
- Total power is rendered as a separate divider row with label/value columns.
- Monster modifier rows display the modifier name directly.
- Battle dice is available when enemy power minus player power is `0..6`.
- Main battle action button dynamically shows `Победа` or `Пытаться сбежать`.
- Victory immediately hides the battle modal and opens reward choice.
- Battle dice/card action hints are temporary; escape result hints stay visible until close.
- Winning battle grants +1 level.
- Winning battle opens `RewardSystem`.
- Losing battle state exposes the `Пытаться сбежать` action.
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
- Reward options display icon, name, and description.
- Card reward uses `CardSystem.AddCard`.
- Item reward uses `PlayerInventory.TryEquip`.
- Full hand/inventory keeps reward modal open and shows Russian status text.
- Reward modal can be closed/skipped.

### Single Rewards

- `SingleRewardSystem` exists.
- `SingleRewardModalView` displays one card/item reward.
- Single reward descriptions are read through `RewardData.DisplayDescription`.
- Tile `GiveCard` and `GiveItem` effects use the single reward modal.
- Accept is disabled when hand/equipment is full.
- Removing card/item from HUD updates Accept availability.
- Close declines reward and continues tile flow.

### Modal UI And Cursor

- Battle, reward, and single reward modals use Russian user-facing button/status text.
- Modal roots use `BattleBackgroundBlurView` for blurred camera background.
- `MainMenuCursor` is provided globally through `Assets/Resources/UI/GlobalCursor.prefab`.
- Cursor press animation plays on any mouse press, not only over clickable UI.
- Existing scenes rely on the global cursor instead of keeping a separate MainMenu-only cursor object.

### Event Notifications

- `EventNotificationSystem` exists on `BoardRoot`.
- `EventNotificationView` exists as a UI notification item.
- Notifications spawn under `Board UI Canvas/Event Notification Container`.
- Notification settings are configured per `EffectType`.
- Notifications support success, blocked, no-effect, and failed messages.
- Notification animation uses coroutine lifetime, fade, upward movement, and destroy.

### Result Screen

- `ResultGameScene.unity` exists.
- `GameResultSystem` exists on `BoardRoot`.
- Win triggers when player level reaches `10`.
- Lose triggers when player HP reaches `0`.
- `GameResultContext` stores the current result while loading the result scene.
- `ResultGameScreenController` displays win/lose art.
- Result screen has a button that clears result context and loads `MainMenu`.

### Main Menu

- `MainMenu.unity` exists.
- Sprite-based buttons exist.
- Continue can be visually locked.
- Custom cursor is provided by the global cursor prefab.
- Button hover/press feedback exists.
- New Game loads `BoardGame`.
- Settings opens a square settings modal with blurred background.
- Settings modal saves resolution, fullscreen mode, and music volume.
- Music volume uses `Assets/Audio/MainAudioMixer.mixer`.

## Tested / Recently Verified

- Board hierarchy contains current gameplay UI objects.
- `BoardBackground_0` sprite is referenced by `Board Background`.
- Card assets use `List<EffectData>`.
- Item assets use `ItemType` and `List<EffectData>`.
- Enemy assets use `baseLevel`, `modifiers`, and `penaltyEffects`.
- Reward and single reward systems are present in scene/scripts.
- Event notification settings exist per `EffectType`.
- `CardSystem` no longer dispatches by hardcoded `cardId`.
- `GameResultSystem` and `ResultGameScene` are present.
- No Docs update touched `GDD.md`.

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
- Continue and Exit are still visual MVP.
- Art is a mix of placeholder, test, and newer board/menu assets.

## Not Implemented Yet

- game-state save/load;
- inventory window;
- item replacement flow;
- card draw/discard pile;
- reward weighting and rarity balancing;
- rare reward chains;
- event modal text/flavor;
- final battle animations;
- final UI polish;
- final balance pass;
- main menu Continue/Exit behavior.
