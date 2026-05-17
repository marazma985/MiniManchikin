# UI

Current UI is in `Assets/Scenes/BoardGame.unity`.

Main Canvas:
- `Board UI Canvas`

Canvas children:
- `Roll Dice Button`
- `Player HUD`
- `CardHand`
- `Battle Modal`

There is also an `EventSystem` in the scene.

## Roll Dice Button

Object: `Board UI Canvas/Roll Dice Button`

Scripts:
- Unity `Button`
- `DiceRollButtonController`

Responsibilities:
- requests `TurnSystem.TryRollDice()`;
- disables/enables itself based on `TurnState`;
- during an active battle, requests `BattleSystem.RollBattleDice()` instead of a turn roll;
- during an active battle, is interactable only when battle dice can be used;
- logs dice result through `TurnSystem.DiceRolled`.

The button does not directly call `DiceSystem.Roll()` or `PlayerMover.MoveSteps()`.

## Player HUD

Object: `Board UI Canvas/Player HUD`

Script:
- `HudView`

Displays:
- avatar placeholder image;
- level badge and `TextMeshProUGUI` level text;
- 5 HP hearts using `Image[] heartImages`;
- 3 passive inventory slots.

HUD depends on:
- `PlayerStats`;
- heart sprites;
- inventory slot views.

HUD only displays values and does not change HP or level.

## HUD Assets

Current placeholder HUD art:

- `Assets/Art/Board/HUD/AvatarPlaceholder.png`
- `Assets/Art/Board/HUD/HeartFull.png`
- `Assets/Art/Board/HUD/HeartEmpty.png`
- `Assets/Art/Board/HUD/InventorySlot.png`
- `Assets/Art/Board/HUD/TestItemIcon.png`

These are temporary placeholders.

## Inventory Slots

Objects:
- `Inventory Slot 01`
- `Inventory Slot 02`
- `Inventory Slot 03`

Script:
- `InventorySlotView`

Each slot has:
- background `Image`;
- icon `Image`;
- optional `itemIcon` Sprite.

Current behavior:
- empty slot hides icon;
- occupied slot shows assigned icon;
- slots are not buttons;
- slots have no gameplay logic.

## Card Hand

Object: `Board UI Canvas/CardHand`

Script:
- `CardHandView`

Children:
- `CardSlot_01`
- `CardSlot_02`
- `CardSlot_03`

Each card slot has:
- `Image`;
- `Button`;
- `CardView`.

Current behavior:
- hand displays up to 3 cards from `CardSystem.Hand`;
- empty slots are hidden;
- each `CardView` displays only `CardData.cardSprite`;
- clicking a card calls `CardSystem.UseCard(card)`;
- successful card use removes that card from hand;
- card effects are resolved by `CardSystem`, not by UI.

Card UI does not display separate name or description text because card art already contains complete visual content.

## Card Assets

Card data:
- `Assets/Data/Cards/SmallHeal.asset`
- `Assets/Data/Cards/Shield.asset`
- `Assets/Data/Cards/LuckyHit.asset`

Card sprites:
- `Assets/Art/Board/Cards/SmallHeal.png`
- `Assets/Art/Board/Cards/Shield.png`
- `Assets/Art/Board/Cards/LuckyHit.png`

## Battle Modal

Object: `Board UI Canvas/Battle Modal`

Script:
- `BattleModalView`

Default scene state:
- inactive (`m_IsActive: 0`)

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
- button text changes between `Resolve Battle`, `Roll Escape`, and `Close`;
- the modal hides after battle completion.

Current limitation:
- battle modal is functional MVP;
- no animations;
- no reward choice UI;
- no equipment or card selection UI inside battle.

## Main Menu

Scene: `Assets/Scenes/MainMenu.unity`

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
- roll dice button can be reused for battle dice when battle dice is available;
- HP hearts update from `PlayerStats`;
- level text updates from `PlayerStats`;
- inventory slot can show assigned icon;
- card hand displays 3 cards;
- card click routes through `CardSystem`;
- `Small Heal` restores 1 HP and is consumed;
- battle modal opens from battle tiles and resolves MVP battle flow.

Not implemented:
- card animations;
- drag-and-drop;
- item tooltips;
- battle reward UI;
- event UI;
- settings UI for board scene;
- main menu button actions.
