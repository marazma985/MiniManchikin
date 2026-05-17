# Coding Rules

These are current project rules for continuing development.

## Architecture Rules

- Do not use Singleton.
- Do not use `FindObjectOfType`.
- Do not use `FindFirstObjectByType` or `FindAnyObjectByType` in runtime code for wiring dependencies.
- Pass dependencies through `[SerializeField]` Inspector references.
- Keep gameplay logic and UI separate.
- Avoid giant manager classes.
- Prefer small focused classes with one responsibility.
- Keep `System`, `View`, `Manager`, and `Controller` roles separate.

Known current exception:
- `MainMenuCursor` uses a static `Instance` bridge for menu hover/press targets. Treat this as existing main-menu UI glue only; do not copy this pattern into gameplay systems.

## Unity Rules

- Do not use `Update()` unless there is a clear need.
- Prefer events, C# Actions, coroutines, and explicit method calls.
- Use `[SerializeField] private` fields instead of public mutable fields.
- Keep scene object names stable; do not encode dynamic state in object names.
- Avoid rewriting unrelated scene objects or components when making a focused change.

## Data Rules

- Use ScriptableObject for data-driven content.
- Current example: `CardData`.
- Runtime systems should reference data assets instead of hard-coded content where possible.

## Gameplay vs UI

Gameplay classes should not know about UI widgets.

Examples:
- `DiceSystem` rolls only.
- `PlayerMover` moves only.
- `PlayerStats` stores HP/level only.
- `TurnSystem` coordinates gameplay systems only.
- `TileEffectSystem` resolves tile effects only.

UI classes should display state and forward user intent.

Examples:
- `HudView` displays `PlayerStats`;
- `DiceRollButtonController` asks `TurnSystem` to roll;
- `CardHandView` displays `CardSystem.Hand`;
- `CardView` reports which card was clicked.

## Current Naming Patterns

- Board gameplay code lives in `Assets/Scripts/Board`.
- UI created for board gameplay currently also lives in `Assets/Scripts/Board`.
- Main menu UI code lives in `Assets/Scripts/UI`.
- Data assets live under `Assets/Data`.
- Placeholder board art lives under `Assets/Art/Board`.

## Extension Guidelines

When adding real tile effects:
- keep `ITileEffect` as the effect contract;
- add focused effect classes;
- do not put all effect behavior into `TileEffectSystem`;
- let `TileEffectSystem` dispatch by `TileType`.

When adding real card effects:
- keep `CardData` as card data;
- add separate card effect classes or handlers later;
- do not make `CardView` apply gameplay effects;
- keep `CardSystem.UseCard` as the gameplay entry point.

When adding inventory gameplay:
- keep `InventorySlotView` visual only;
- create a separate inventory model/system;
- item effects should not live in the view.

When adding battle:
- do not implement battle inside `BattleTileEffect` directly if it grows large;
- use `BattleTileEffect` to trigger a separate battle system or battle flow.
