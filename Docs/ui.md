# UI

Current board UI is in `Assets/Scenes/BoardGame.unity`.

Main Canvas:
- `Board UI Canvas`

Canvas children:
- `Battle Modal`
- `Reward Modal`
- `Single Reward Modal`
- `Event Notification Container`
- `Roll Dice Button`
- `Player HUD`
- `CardHand`

There is also an `EventSystem` in the scene.

## Board Background

World object:
- `BoardRoot/Board Background`

Purpose:
- displays `Assets/Art/Board/BoardBackground.png` behind board elements;
- uses `SpriteRenderer` with low sorting order.

It is not a Canvas UI object.

## Roll Dice Button

Object:
- `Board UI Canvas/Roll Dice Button`

Scripts:
- Unity `Button`
- `DiceRollButtonController`

Responsibilities:
- requests `TurnSystem.TryRollDice()`;
- disables/enables itself based on `TurnState`;
- during active battle, requests `BattleSystem.RollBattleDice()`;
- during active battle, is interactable only when battle dice can be used;
- logs dice result through `TurnSystem.DiceRolled`.

The button does not directly call `DiceSystem.Roll()` or `PlayerMover.MoveSteps()`.

## Player HUD

Object:
- `Board UI Canvas/Player HUD`

Script:
- `HudView`

Displays:
- avatar image;
- level badge and `TextMeshProUGUI` level text;
- 5 HP hearts;
- 3 equipped item slots.

HUD depends on:
- `PlayerStats`;
- `PlayerInventory`;
- heart sprites;
- `InventorySlotView[]`.

HUD only displays values and forwards remove clicks to `PlayerInventory.Unequip`.

## Inventory Slots

Objects:
- `Inventory Slot 01`
- `Inventory Slot 02`
- `Inventory Slot 03`

Script:
- `InventorySlotView`

Each slot has:
- background `Image`;
- item icon `Image`;
- remove item `Button`;
- remove button `CanvasGroup` for hover fade.

Current behavior:
- empty slot hides icon;
- occupied slot shows equipped `ItemData.ItemSprite`;
- remove X appears on pointer hover and fades out on pointer exit;
- clicking remove X raises `RemoveClicked`;
- `HudView` handles that event and calls `PlayerInventory.Unequip(item)`.

Slots do not apply item effects.

## Card Hand

Object:
- `Board UI Canvas/CardHand`

Script:
- `CardHandView`

Children:
- `CardSlot_01`
- `CardSlot_02`
- `CardSlot_03`

Each card slot has:
- `Image`;
- `Button`;
- `CardView`;
- remove card `Button`;
- remove button `CanvasGroup` for hover fade.

Current behavior:
- hand displays up to 3 cards from `CardSystem.Hand`;
- empty slots are hidden;
- each `CardView` displays `CardData.CardSprite`;
- main card click calls `CardSystem.UseCard(card)`;
- remove X click calls `CardSystem.RemoveCard(card)`;
- remove X does not trigger card use;
- card hover highlight fades smoothly;
- card hover highlight is suppressed while hovering the remove X.

Card UI does not display separate name or description text because current card art is treated as complete card art.

## Card Assets

Card data:
- `Assets/Data/Cards/SmallHeal.asset`
- `Assets/Data/Cards/Shield.asset`
- `Assets/Data/Cards/LuckyHit.asset`
- `Assets/Data/Cards/ExperiencePotion.asset`
- `Assets/Data/Cards/TreasureMap.asset`
- `Assets/Data/Cards/MonsterContract.asset`

Card sprites include:
- `Assets/Art/Board/Cards/SmallHeal.png`
- `Assets/Art/Board/Cards/Shield.png`
- `Assets/Art/Board/Cards/LuckyHit.png`
- card test sprites under `Assets/Art/Board/Cards/test`

## Battle Modal

Object:
- `Board UI Canvas/Battle Modal`

Script:
- `BattleModalView`

Default scene state:
- inactive.

Displays:
- player name;
- player portrait;
- player power entries;
- player total power;
- enemy name;
- enemy portrait;
- enemy power entries;
- enemy total power;
- battle status;
- action button label.

Controls:
- `Resolve Battle Button`

Current behavior:
- `BattleSystem` shows the modal when a battle starts;
- the modal forwards resolve/escape/close clicks through `ResolveRequested`;
- button text changes between resolve, escape, reward-wait, and close states;
- after win, battle modal hides while reward modal is shown;
- player portrait is not changed by `BattleModalView` when battle opens.

No battle layout polish or animations are implemented.

## Reward Modal

Object:
- `Board UI Canvas/Reward Modal`

Scripts:
- `RewardModalView`
- `RewardView` on each reward option

Children include:
- `Reward Panel`
- `Reward Option 01`
- `Reward Option 02`
- `Reward Option 03`
- `Reward Limit Status`
- `Close Reward Button`

Current behavior:
- opens after battle victory;
- displays up to 3 card/item rewards;
- selecting a reward asks `RewardSystem` to claim it;
- if card hand is full, shows `Card hand is full`;
- if equipment inventory is full, shows `Equipment inventory is full`;
- status text fades in and out;
- close button skips the reward and completes battle flow.

## Single Reward Modal

Object:
- `Board UI Canvas/Single Reward Modal`

Scripts:
- `SingleRewardModalView`
- controlled by `SingleRewardSystem`

Displays:
- reward icon;
- reward name;
- reward description;
- status text;
- Accept button;
- close button.

Current behavior:
- used by single card/item rewards from tile effects;
- Accept is disabled if hand/equipment is full;
- status text shows why Accept is unavailable;
- after manual removal of card/item from HUD, Accept state refreshes;
- close declines the reward and completes the tile flow.

## Event Notifications

Object:
- `Board UI Canvas/Event Notification Container`

Scripts:
- `EventNotificationSystem` on `BoardRoot`
- `EventNotificationView` on notification item template

Displays:
- icon;
- message text.

Current behavior:
- new notification instances are spawned under the container;
- multiple notifications can coexist;
- each notification waits briefly, moves upward, fades out, and destroys itself;
- messages and icons are configured per `EffectType` on `EventNotificationSystem`.

## HUD Assets

Current board HUD art includes:
- `Assets/Art/Board/HUD/AvatarPlaceholder.png`
- `Assets/Art/Board/HUD/HeartFull.png`
- `Assets/Art/Board/HUD/HeartEmpty.png`
- `Assets/Art/Board/HUD/InventorySlot.png`
- `Assets/Art/Board/HUD/TestItemIcon.png`

Board art includes:
- `Assets/Art/Board/BoardBackground.png`
- `Assets/Art/Board/TilePlaceholder.png`
- `Assets/Art/Board/PathSegmentPlaceholder.png`
- `Assets/Art/Board/PlayerPlaceholder.png`
- `Assets/Art/Board/PlayerPlaceholder2.asset`

Some art remains placeholder/test content.

## Main Menu

Scene:
- `Assets/Scenes/MainMenu.unity`

Canvas:
- `Main Menu Canvas`

Main objects:
- `Background`
- `Game Title`
- `Buttons`
- `Continue Button`
- `New Game Button`
- `Settings Button`
- `Exit Button`
- `Custom Cursor`

Scripts:
- `MainMenuSpriteButton`
- `MainMenuButtonFeedback`
- `MainMenuCursor`
- `MainMenuCursorHoverTarget`
- `MainMenuSkinSlots`

Current behavior:
- sprite-based visual buttons;
- Continue button can be locked through `continueAvailable`;
- buttons have hover lift feedback;
- custom cursor follows mouse and changes state on hover/press.

Current limitation:
- button `onClick` calls are empty;
- Continue/New Game/Settings/Exit do not perform actions yet;
- no save/load or settings UI is connected.

## Working UI Elements

Currently working:
- roll dice button starts turn;
- roll dice button blocks duplicate roll during movement/resolution;
- roll dice button can be reused for battle dice;
- HP hearts update from `PlayerStats`;
- level text updates from `PlayerStats`;
- equipped item slots update from `PlayerInventory`;
- card hand displays up to 3 cards;
- card click routes through `CardSystem`;
- card/item remove X buttons free slots;
- battle modal opens from battle tiles;
- reward modal opens after battle win;
- single reward modal opens from tile reward events;
- event notifications display applied effects.

Not implemented:
- inventory window;
- card animations;
- drag-and-drop;
- item/card tooltips;
- settings UI for board scene;
- main menu button actions;
- save/load UI.
