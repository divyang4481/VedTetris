# VedTetris

VedTetris is a classic Tetris game built using .NET MAUI, designed to run on multiple platforms including Windows, Android, iOS, and Mac Catalyst. Enjoy the timeless puzzle game with modern cross-platform technology!

## Features
- Classic Tetris gameplay
- Responsive controls (move, rotate, drop)
- Touch gesture support for mobile devices (swipe left/right/up/down)
- Next piece preview
- Level system with increasing difficulty
- Ghost piece showing where blocks will land
- Score tracking with level multipliers
- High score tracking (saved locally)
- Pause/resume functionality
- Game over and restart functionality
- Cross-platform support via .NET MAUI

## About .NET MAUI
[.NET MAUI](https://learn.microsoft.com/dotnet/maui/what-is-maui) (Multi-platform App UI) is a framework for building native, cross-platform applications with a single codebase in C#. It enables developers to create apps for Android, iOS, macOS, and Windows using the same project and code.

## Getting Started
1. **Requirements:**
   - [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
   - Visual Studio 2022 or later with MAUI workload installed
2. **Clone the repository:**
   ```sh
   git clone <your-repo-url>
   cd VedTetris
   ```
3. **Build and run:**
   - Open the solution in Visual Studio
   - Select your target platform (Windows, Android, iOS, or Mac Catalyst)
   - Press F5 to build and run

## Controls
- **Move Left/Right:** Use the on-screen buttons or swipe left/right
- **Rotate:** Use the rotate button or swipe up
- **Drop:** Use the drop button or swipe down
- **Pause/Resume:** Use the pause button
- **Restart:** After game over, press "Play Again" in the dialog

## Scoring System
- Clearing lines awards points based on the number of lines cleared at once:
  - 1 line: 100 points × current level
  - 2 lines: 300 points × current level
  - 3 lines: 500 points × current level
  - 4 lines (Tetris!): 800 points × current level
- The level increases for every 10 lines cleared
- Each level increases the game speed

## License
This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
