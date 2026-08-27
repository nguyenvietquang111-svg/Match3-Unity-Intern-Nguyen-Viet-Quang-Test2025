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

