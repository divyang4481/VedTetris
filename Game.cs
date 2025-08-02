using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VedTetris
{
    /// <summary>
    /// Represents the main game logic and state.
    /// </summary>
    public class Game
    {
        public readonly int BoardWidth = 10;
        public readonly int BoardHeight = 20;
        public int[,] Board { get; private set; }
        public Tetromino CurrentTetromino { get; private set; }
        public Tetromino NextTetromino { get; private set; }
        public int Score { get; private set; }
        public int LinesCleared { get; private set; }
        public int Level { get; private set; } = 1;
        public bool IsGameOver { get; private set; }
        public bool IsPaused { get; private set; }
        
        // Calculate current game speed based on level
        public int GetGameSpeed() => Math.Max(100, 500 - ((Level - 1) * 40));

        private readonly Random _random = new Random();
        private readonly List<Tetromino> _tetrominoes;

        public Game()
        {
            _tetrominoes = new List<Tetromino>
            {
                new Tetromino(1, new Point(0, 0), new[,] { { 1, 1, 1, 1 } }, Colors.Cyan), // I
                new Tetromino(2, new Point(0, 0), new[,] { { 1, 1 }, { 1, 1 } }, Colors.Yellow), // O
                new Tetromino(3, new Point(0, 0), new[,] { { 0, 1, 0 }, { 1, 1, 1 } }, Colors.Purple), // T
                new Tetromino(4, new Point(0, 0), new[,] { { 0, 1, 1 }, { 1, 1, 0 } }, Colors.Green), // S
                new Tetromino(5, new Point(0, 0), new[,] { { 1, 1, 0 }, { 0, 1, 1 } }, Colors.Red), // Z
                new Tetromino(6, new Point(0, 0), new[,] { { 1, 0, 0 }, { 1, 1, 1 } }, Colors.Blue), // J
                new Tetromino(7, new Point(0, 0), new[,] { { 0, 0, 1 }, { 1, 1, 1 } }, Colors.Orange) // L
            };
            Reset();
        }

        public void Reset()
        {
            Board = new int[BoardWidth, BoardHeight];
            Score = 0;
            LinesCleared = 0;
            Level = 1;
            IsGameOver = false;
            IsPaused = false;
            NextTetromino = GetRandomTetromino();
            SpawnNewTetromino();
        }

        private Tetromino GetRandomTetromino()
        {
            return _tetrominoes[_random.Next(_tetrominoes.Count)].Clone();
        }

        private void SpawnNewTetromino()
        {
            CurrentTetromino = NextTetromino;
            CurrentTetromino.Position = new Point(BoardWidth / 2 - 1, 0);
            NextTetromino = GetRandomTetromino();

            if (CheckCollision())
            {
                IsGameOver = true;
            }
        }

        public void GameTick()
        {
            if (IsGameOver || IsPaused) return;

            CurrentTetromino.Move(0, 1);
            if (CheckCollision())
            {
                CurrentTetromino.Move(0, -1);
                PlaceTetromino();
                ClearLines();
                SpawnNewTetromino();
            }
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
        }

        public void MoveLeft()
        {
            if (IsGameOver || IsPaused) return;
            
            CurrentTetromino.Move(-1, 0);
            if (CheckCollision())
            {
                CurrentTetromino.Move(1, 0);
            }
        }

        public void MoveRight()
        {
            if (IsGameOver || IsPaused) return;
            
            CurrentTetromino.Move(1, 0);
            if (CheckCollision())
            {
                CurrentTetromino.Move(-1, 0);
            }
        }

        public void Rotate()
        {
            if (IsGameOver || IsPaused) return;
            
            CurrentTetromino.Rotate();
            if (CheckCollision())
            {
                // Try to shift if blocked by walls
                CurrentTetromino.Move(-1, 0);
                if (!CheckCollision())
                    return;
                
                CurrentTetromino.Move(2, 0);
                if (!CheckCollision())
                    return;
                
                CurrentTetromino.Move(-1, 0);
                
                // Rotate back if still colliding
                CurrentTetromino.Rotate(); // Rotate back
                CurrentTetromino.Rotate();
                CurrentTetromino.Rotate();
            }
        }

        public void Drop()
        {
            if (IsGameOver || IsPaused) return;
            
            while (!CheckCollision())
            {
                CurrentTetromino.Move(0, 1);
            }
            CurrentTetromino.Move(0, -1);
            GameTick();
        }
        
        public Tetromino GetGhostPiece()
        {
            if (CurrentTetromino == null || IsGameOver || IsPaused) 
                return null;
            
            Tetromino ghost = CurrentTetromino.Clone();
            
            // Move ghost down until it collides
            while (true)
            {
                ghost.Move(0, 1);
                if (CheckCollisionForPiece(ghost))
                {
                    ghost.Move(0, -1);
                    return ghost;
                }
            }
        }

        private bool CheckCollision()
        {
            return CheckCollisionForPiece(CurrentTetromino);
        }

        private bool CheckCollisionForPiece(Tetromino piece)
        {
            foreach (var block in piece.GetWorldPositions())
            {
                int x = (int)block.X;
                int y = (int)block.Y;

                // Check for collision with the side walls
                if (x < 0 || x >= BoardWidth)
                {
                    return true;
                }

                // Check for collision with the floor
                if (y >= BoardHeight)
                {
                    return true;
                }

                // Check for collision with other blocks on the board (if inside the play area)
                if (y >= 0)
                {
                    if (Board[x, y] != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void PlaceTetromino()
        {
            foreach (var block in CurrentTetromino.GetWorldPositions())
            {
                int x = (int)block.X;
                int y = (int)block.Y;

                // Ensure the block is within the board boundaries before placing
                if (x >= 0 && x < BoardWidth && y >= 0 && y < BoardHeight)
                {
                    Board[x, y] = CurrentTetromino.Id;
                }
            }
        }

        private void ClearLines()
        {
            int linesCleared = 0;
            for (int y = BoardHeight - 1; y >= 0; y--)
            {
                bool lineIsFull = true;
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (Board[x, y] == 0)
                    {
                        lineIsFull = false;
                        break;
                    }
                }

                if (lineIsFull)
                {
                    linesCleared++;
                    for (int row = y; row > 0; row--)
                    {
                        for (int col = 0; col < BoardWidth; col++)
                        {
                            Board[col, row] = Board[col, row - 1];
                        }
                    }
                    y++; // Re-check the same line
                }
            }

            if (linesCleared > 0)
            {
                // Update total lines and level
                LinesCleared += linesCleared;
                Level = (LinesCleared / 10) + 1;
                
                // Award points (more points for more lines cleared at once)
                // Use traditional Tetris scoring: 100, 300, 500, 800 for 1-4 lines
                int points = 0;
                switch (linesCleared)
                {
                    case 1: points = 100; break;
                    case 2: points = 300; break;
                    case 3: points = 500; break;
                    case 4: points = 800; break; // Tetris!
                }
                
                // Multiply by level for increasing difficulty rewards
                Score += points * Level;
            }
        }

        public Color GetColorForBlock(int id)
        {
            return _tetrominoes.Find(t => t.Id == id)?.Color ?? Colors.Black;
        }
    }
}
