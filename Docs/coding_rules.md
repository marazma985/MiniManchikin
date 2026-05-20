# Coding Rules

These are current project rules for continuing development.

## Architecture Rules

- Do not use Singleton for gameplay systems.
- Do not use `FindObjectOfType`.
- Do not use `FindFirstObjectByType` or `FindAnyObjectByType` in runtime code for wiring dependencies.
- Pass dependencies through `[SerializeField]` Inspector references.
- Keep gameplay logic and UI separate.
- Avoid giant manager classes.
- Prefer small focused classes with one responsibility.
- Keep `System`, `View`, `Manager`, and `Controller` roles separate.

Known current exceptions:
- `MainMenuCursor` uses a static `Instance` bridge for menu hover/press targets. Treat this as existing main-menu UI glue only; do not copy this pattern into gameplay systems.
- Some `Reset()` editor convenience methods may use scene search APIs. This should not become runtime dependency wiring.

## Unity Rules

- Do not use `Update()` unless there is a clear need.
- Prefer events, C# Actions, coroutines, and explicit method calls.
- Use `[SerializeField] private` fields instead of public mutable fields.
- Keep scene object names stable; do not encode dynamic state in object names.
- Avoid rewriting unrelated scene objects or components when making a focused change.
- Prefer coroutine-based UI effects for short animations already present in the project.

## Data Rules

- Use ScriptableObject for data-driven content.
- Current data assets include `CardData`, `ItemData`, and `EnemyData`.
- Runtime systems should reference data assets instead of hard-coded content where possible.
- `EffectData` is the shared serializable foundation for gameplay effects.
- `EffectType` should be extended only when a real gameplay/content need exists.
- `ItemType` describes item category only; it must not be used to enforce equipment slots.
- Equipment limit is currently max 3 items total.
- `Rarity` is used by cards, items, rewards, and random removal/generation filters.
- `UsageContext` controls when cards can be used.

## Gameplay vs UI

Gameplay classes should not know about concrete UI widgets unless their role is explicitly to coordinate UI flow.

Examples:
- `DiceSystem` rolls only.
- `PlayerMover` moves only.
- `PlayerStats` stores HP/level only.
- `TurnSystem` coordinates gameplay systems only.
- `TileEffectSystem` resolves tile effects only.
- `EffectResolver` applies gameplay effects but does not own UI layout.
- `PlayerInventory` owns equipped items and equipment rules.
- `CardSystem` owns hand state and card use rules.

UI classes should display state and forward user intent.

Examples:
- `HudView` displays `PlayerStats` and equipped items.
- `DiceRollButtonController` asks `TurnSystem` or `BattleSystem` to roll.
- `CardHandView` displays `CardSystem.Hand`.
- `CardView` reports use/remove clicks.
- `InventorySlotView` reports remove clicks.
- `RewardModalView` and `SingleRewardModalView` report user choices.
- `EventNotificationView` displays one notification and animates itself.

## Current Naming Patterns

- Board gameplay code lives in `Assets/Scripts/Board`.
- Board UI view code currently also lives in `Assets/Scripts/Board`.
- Main menu UI code lives in `Assets/Scripts/UI`.
- Data assets live under `Assets/Data`.
- Board art lives under `Assets/Art/Board`.
- Main menu art may live under main menu art folders; keep asset organization stable when editing existing scenes.

## Effect Rules

- Prefer `EffectData` for new gameplay content.
- Do not add one-off hardcoded effect dispatch unless the effect cannot reasonably fit the current model.
- Shared tile effect application should go through `EffectResolver` where possible.
- Card effect application currently lives in `CardSystem`; keep card UI passive.
- Item battle bonuses should be read from `PlayerInventory.GetTotalEffectValue`.
- Armor protection is currently category-based through `ItemType.Armor`.
- If an effect cannot be applied, log a clear warning and avoid breaking the turn flow.
- If an effect is triggered but has no state change, use notification no-effect status where relevant.

## Tile Effect Guidelines

When adding or extending tile effects:
- keep `ITileEffect` as the effect contract;
- use `IDeferredTileEffect` when UI or delayed gameplay must finish before the turn ends;
- do not put all effect behavior into `TileEffectSystem`;
- let `TileEffectSystem` dispatch by `TileType`;
- use serialized pools for event content;
- use `EffectResolver` for supported unified effects;
- invoke callbacks exactly once for deferred effects.

## Card Guidelines

When adding real card effects:
- keep `CardData` as data;
- use `CardData.effects`;
- respect `UsageContext`;
- validate all effects before consuming a card;
- remove a card only after successful application;
- do not make `CardView` apply gameplay effects;
- keep `CardSystem.UseCard` as the gameplay entry point.

Legacy hardcoded card effect classes exist but should not be expanded for new card content.

## Inventory Guidelines

When adding inventory gameplay:
- keep `InventorySlotView` visual only;
- use `PlayerInventory` for equipped item state;
- do not add slot restrictions by `ItemType`;
- keep max equipped item limit centralized in `PlayerInventory`;
- item effects should not live in the view;
- manual UI removal should call `PlayerInventory.Unequip`.

## Battle Guidelines

When changing battle:
- do not implement battle inside `BattleTileEffect`;
- use `BattleTileEffect` only to trigger `BattleSystem`;
- keep enemy power based on `EnemyData.BaseLevel` and selected `EnemyModifier` effects;
- keep enemy penalties based on `EnemyData.penaltyEffects`;
- preserve item and card effect integration;
- preserve battle reward callback flow so tile resolution completes correctly.

## Reward Guidelines

When changing rewards:
- keep battle reward choice flow in `RewardSystem`;
- keep single reward flow in `SingleRewardSystem`;
- do not duplicate reward claim rules in UI views;
- failed claims due to full hand/equipment should keep the relevant modal open;
- single reward Accept state should update from `CardSystem.OnHandChanged` and `PlayerInventory.OnEquipmentChanged`.

## Notification Guidelines

When adding notifications:
- configure message templates and icons on `EventNotificationSystem`;
- call `ShowEffectNotification` with an explicit status when possible;
- do not put notification layout logic into gameplay systems;
- respect `showNotification` per `EffectType`;
- preserve coroutine animation flow in `EventNotificationView`;
- do not add `Update()` for notification animation.
