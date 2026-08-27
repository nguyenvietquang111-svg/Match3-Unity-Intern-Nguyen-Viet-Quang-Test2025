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

