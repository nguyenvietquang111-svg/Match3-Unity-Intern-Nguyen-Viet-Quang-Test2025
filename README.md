# My Assessment

## Tasks

# Task 1 - Reskin

This project task is to reskin all board items into fish using the assets already included in the Unity project.

## Goal

- Replace the existing item visuals with fish sprites.
- Keep the gameplay logic unchanged for this task.
- Use only the fish assets already available in `Assets/Textures/Fish`.

## Fish Asset Mapping

- `itemNormal01` -> `fish_1`
- `itemNormal02` -> `fish_2`
- `itemNormal03` -> `fish_3`
- `itemNormal04` -> `fish_4`
- `itemNormal05` -> `fish_5`
- `itemNormal06` -> `fish_6`
- `itemNormal07` -> `rainbow_fish`

## Files Updated

- `Assets/Resources/prefabs/itemNormal01.prefab`
- `Assets/Resources/prefabs/itemNormal02.prefab`
- `Assets/Resources/prefabs/itemNormal03.prefab`
- `Assets/Resources/prefabs/itemNormal04.prefab`
- `Assets/Resources/prefabs/itemNormal05.prefab`
- `Assets/Resources/prefabs/itemNormal06.prefab`
- `Assets/Resources/prefabs/itemNormal07.prefab`

## Notes

- The fish sprites are already imported in the project.
- No gameplay rules are changed in Task 1.
- This task is complete once every board item uses a fish sprite.

# Task 2 - Change the Gameplay

## Summary
Task 2 changes the game from match-3 style to a tap-to-store gameplay:

- Tap an item on the board to move it to the bottom cells.
- Once an item is moved down, it cannot be moved back in Normal/Autoplay/Auto Lose modes.
- When exactly 3 identical items exist in the bottom cells, they are cleared.
- The player wins when the board is cleared.
- The player loses when all 5 bottom cells are filled.

## Implemented Flow

1. The board is generated at runtime from the game settings.
2. Each board item can be tapped once.
3. Tapped items move into the first available bottom cell.
4. The game checks whether 3 identical bottom items are present.
5. If yes, those items are removed with a clear animation.
6. If the board is empty, the player wins.
7. If the bottom area is full, the player loses.

## Extra Modes Added

- `Play`: normal gameplay.
- `Autoplay`: the game automatically taps items until it wins.
- `Auto Lose`: the game automatically taps items to force a loss.

## Notes

- The bottom area uses 5 cells.
- The initial board is balanced so the game can still be completed.

# Task 3 - Gameplay Improvements

## Summary
Task 3 adds the extra polish and the new time-based mode requested in the test.

## Implemented Improvements

- The initial board is generated with all fish types included.
- Item movement has a visible animation when moving from the board to the bottom cells.
- Clearing 3 identical items now uses a scale-to-zero animation.

## Time Attack Mode

- Added a `Time Attack` button on the Home screen.
- Time Attack is a separate game mode.
- In this mode, the player does not lose when the 5 bottom cells are filled.
- The player can tap an item in the bottom area to return it to its original board cell.
- The player loses if the board is not cleared within 1 minute.

## How to Play Time Attack

1. Open the Home screen.
2. Click `Time Attack`.
3. Tap items from the board to move them into the bottom cells.
4. Tap a bottom item to send it back to its original board cell.
5. Clear the whole board before the timer reaches zero.

