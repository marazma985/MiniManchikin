# UI

Current board UI is in `Assets/Scenes/BoardGame.unity`.

Other runtime UI scenes:
- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/ResultGameScene.unity`

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

World-space UI-like objects:
- `Board Back Button`

## Board Background

World object:
- `BoardRoot/Board Background`

Purpose:
- displays `Assets/Art/Board/BoardBackground.png` behind board elements;
- uses `SpriteRenderer` with low sorting order.

It is not a Canvas UI object.

## Board Back Button

Object:
- `Board Back Button`

Script:
- `BoardBackButtonController`

Responsibilities:
- anchors itself to the top-right camera area;
- returns to `MainMenu`;
- cross-fades normal/hover/pressed sprites with the same "new underneath, old fades above" pattern as the roll dice button;
- ignores hover/click while configured modal roots are active or the pointer is over UI.

It is intentionally outside `Board UI Canvas`, so it is captured by modal blur and visually sits below modal panels.

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
- logs dice result through `TurnSystem.DiceRolled`;
- cross-fades between normal, hover, pressed, and disabled sprites over `0.2` seconds.

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

Current HUD sprites are assigned from `Assets/Art/Board/HUD/PlayerHudList.png`:
- `PlayerHudList_1` full heart;
- `PlayerHudList_2` empty heart;
- `PlayerHudList_3` empty inventory slot;
- `PlayerHudList_4` occupied inventory slot.

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
- player power rows;
- player total row;
- enemy name;
- enemy portrait;
- enemy power rows;
- enemy total row;
- battle status;
- action button label.

Controls:
- battle action button (scene object may still be named `Resolve Battle Button`)

Current behavior:
- `BattleSystem` shows the modal when a battle starts;
- the modal forwards resolve/escape/close clicks through `ResolveRequested`;
- button text dynamically changes between `Победа`, `Пытаться сбежать`, and `Закрыть`;
- victory immediately hides the battle modal and opens the reward modal;
- power rows are instantiated from player/enemy row prefabs;
- player equipment row is hidden when its value is `0`;
- total rows use `BattlePowerTotalRowView` with label/value columns and a color divider;
- temporary action hints clear after 2.5 seconds;
- escape result hints remain until close;
- after win, battle modal hides while reward modal is shown;
- player portrait is not changed by `BattleModalView` when battle opens.

The modal uses Russian user-facing labels/status text.

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
- each option shows icon, name, and description from `RewardData.DisplayDescription`;
- selecting a reward asks `RewardSystem` to claim it;
- if card hand is full, shows Russian full-hand status;
- if equipment inventory is full, shows Russian full-inventory status;
- status text fades in and out;
- close button skips the reward and completes battle flow.

The modal uses Russian user-facing labels/status text.

## Single Reward Modal

Object:
- `Board UI Canvas/Single Reward Modal`

Scripts:
- `SingleRewardModalView`
- controlled by `SingleRewardSystem`

Displays:
- reward icon;
- reward name;
- reward description from `RewardData.DisplayDescription`;
- status text;
- Accept button;
- close button.

Current behavior:
- used by single card/item rewards from tile effects;
- Accept is disabled if hand/equipment is full;
- status text shows why Accept is unavailable;
- after manual removal of card/item from HUD, Accept state refreshes;
- close declines the reward and completes the tile flow.

The modal uses Russian user-facing labels/status text.

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

## Result Screen

Scene:
- `Assets/Scenes/ResultGameScene.unity`

Script:
- `ResultGameScreenController`

Displays:
- win or lose result image;
- button to return to `MainMenu`.

Current behavior:
- reads result from `GameResultContext`;
- defaults to lose art if no result context exists;
- clears result context before returning to main menu.

## HUD Assets

Current board HUD art includes:
- `Assets/Art/Board/HUD/PlayerHudList.png`
- `Assets/Art/Board/HUD/RollDiseList.png`
- `Assets/Art/Board/ButtonBackList.png`

Board art includes:
- `Assets/Art/Board/BoardBackground.png`
- `Assets/Art/Board/TilePlaceholder.png`
- `Assets/Art/Board/PathSegmentPlaceholder.png`
- `Assets/Art/Board/PlayerPlaceholder.png`
- `Assets/Art/Board/PlayerPlaceholder2.asset`
- `Assets/Art/ShareArt/ButtonsAndCell.png`
- `Assets/Art/ShareArt/Modal.png`
- `Assets/Art/ShareArt/ModalSquare.png`
- `Assets/Art/ShareArt/Cursor.png`

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
- settings modal is generated/owned by `MainMenuSettingsModalView`

Scripts:
- `MainMenuSceneLoader`
- `MainMenuSpriteButton`
- `MainMenuButtonFeedback`
- `MainMenuCursor`
- `MainMenuCursorHoverTarget`
- `MainMenuSkinSlots`
- `MainMenuSettingsModalView`
- `GameSettingsService`

Current behavior:
- sprite-based visual buttons;
- Continue button can be locked through `continueAvailable`;
- buttons have hover lift feedback;
- New Game loads `BoardGame`;
- Settings opens a square modal using `Assets/Art/ShareArt/ModalSquare.png`;
- settings modal blurs the menu background;
- settings can save window size, fullscreen mode, and music volume;
- music volume is applied through `Assets/Audio/MainAudioMixer.mixer`;
- custom cursor is supplied globally from `Assets/Resources/UI/GlobalCursor.prefab`;
- custom cursor follows mouse, changes state on hover, and plays press state for any mouse press.

Current limitation:
- Continue and Exit do not perform actions yet;
- no game-state save/load is connected.

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
- battle modal displays separate power rows and total rows;
- reward modal opens after battle win;
- reward modal displays reward descriptions;
- single reward modal opens from tile reward events;
- event notifications display applied effects.
- board back button returns to MainMenu.
- main menu New Game and Settings buttons are connected.
- result screen displays win/lose state and returns to MainMenu.

Not implemented:
- inventory window;
- card animations;
- drag-and-drop;
- item/card tooltips;
- board-scene settings UI;
- main menu Continue/Exit actions;
- game-state save/load UI.
