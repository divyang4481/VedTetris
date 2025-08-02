using System;
using System.Collections.Generic;
using System.Linq;

namespace VedTetris
{
    /// <summary>
    /// Represents the main game logic and state.
    /// </summary>
    public class Game
    {
        public readonly int BoardWidth = 10;
        public readonly int BoardHeight = 20;
        public int[,] Board { get; private set; } = new int[10, 20];
        public Tetromino? CurrentTetromino { get; private set; }
        public Tetromino? NextTetromino { get; private set; }
        public Tetromino? HeldPiece { get; private set; }
        public int Score { get; private set; }
        public int LinesCleared { get; private set; }
        public int Level { get; private set; } = 1;
        public bool IsGameOver { get; private set; }
        public bool IsPaused { get; private set; }
        public List<int> LinesBeingCleared { get; private set; } = [];
        
        private bool _canHold = true;
        private bool _animatingLineClear = false;
        private int _previousLevel = 1;
        
        // Events for sound and UI feedback - made nullable
        public event Action<int>? OnLinesCleared;
        public event Action? OnTetris;
        public event Action? OnLevelUp;
        public event Action? OnGameOver;
        public event Action? OnMove;
        public event Action? OnRotate;
        public event Action? OnDrop;
        public event Action? OnHold;
        
        // Calculate current game speed based on level
        public int GetGameSpeed() => Math.Max(100, 500 - ((Level - 1) * 40));

        private readonly Random _random = new();
        private readonly List<Tetromino> _tetrominoes;

        public Game()
        {
            _tetrominoes =
            [
                new Tetromino(1, new(0, 0), new[,] { { 1, 1, 1, 1 } }, Colors.Cyan), // I
                new Tetromino(2, new(0, 0), new[,] { { 1, 1 }, { 1, 1 } }, Colors.Yellow), // O
                new Tetromino(3, new(0, 0), new[,] { { 0, 1, 0 }, { 1, 1, 1 } }, Colors.Purple), // T
                new Tetromino(4, new(0, 0), new[,] { { 0, 1, 1 }, { 1, 1, 0 } }, Colors.Green), // S
                new Tetromino(5, new(0, 0), new[,] { { 1, 1, 0 }, { 0, 1, 1 } }, Colors.Red), // Z
                new Tetromino(6, new(0, 0), new[,] { { 1, 0, 0 }, { 1, 1, 1 } }, Colors.Blue), // J
                new Tetromino(7, new(0, 0), new[,] { { 0, 0, 1 }, { 1, 1, 1 } }, Colors.Orange) // L
            ];
            Reset();
        }

        public void Reset()
        {
            Board = new int[BoardWidth, BoardHeight];
            Score = 0;
            LinesCleared = 0;
            Level = 1;
            _previousLevel = 1;
            IsGameOver = false;
            IsPaused = false;
            HeldPiece = null;
            _canHold = true;
            _animatingLineClear = false;
            LinesBeingCleared.Clear();
            NextTetromino = GetRandomTetromino();
            SpawnNewTetromino();
        }

        private Tetromino GetRandomTetromino()
        {
            return _tetrominoes[_random.Next(_tetrominoes.Count)].Clone();
        }

        private void SpawnNewTetromino()
        {
            if (NextTetromino == null) return;
            
            CurrentTetromino = NextTetromino;
            CurrentTetromino.Position = new(BoardWidth / 2 - 1, 0);
            NextTetromino = GetRandomTetromino();
            _canHold = true;

            if (CheckCollision())
            {
                IsGameOver = true;
                OnGameOver?.Invoke();
            }
        }

        public void GameTick()
        {
            if (IsGameOver || IsPaused || _animatingLineClear || CurrentTetromino == null) return;

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

        public bool HoldCurrentPiece()
        {
            if (!_canHold || IsGameOver || IsPaused || CurrentTetromino == null) return false;
            
            OnHold?.Invoke();
            
            if (HeldPiece == null)
            {
                HeldPiece = CurrentTetromino.Clone();
                HeldPiece.Position = new(0, 0);
                SpawnNewTetromino();
            }
            else
            {
                var temp = HeldPiece;
                HeldPiece = CurrentTetromino.Clone();
                HeldPiece.Position = new(0, 0);
                CurrentTetromino = temp;
                CurrentTetromino.Position = new(BoardWidth / 2 - 1, 0);
                
                if (CheckCollision())
                {
                    // If held piece can't be placed, game over
                    IsGameOver = true;
                    OnGameOver?.Invoke();
                    return false;
                }
            }
            
            _canHold = false;
            return true;
        }

        public void MoveLeft()
        {
            if (IsGameOver || IsPaused || CurrentTetromino == null) return;
            
            CurrentTetromino.Move(-1, 0);
            if (CheckCollision())
            {
                CurrentTetromino.Move(1, 0);
            }
            else
            {
                OnMove?.Invoke();
            }
        }

        public void MoveRight()
        {
            if (IsGameOver || IsPaused || CurrentTetromino == null) return;
            
            CurrentTetromino.Move(1, 0);
            if (CheckCollision())
            {
                CurrentTetromino.Move(-1, 0);
            }
            else
            {
                OnMove?.Invoke();
            }
        }

        public void Rotate()
        {
            if (IsGameOver || IsPaused || CurrentTetromino == null) return;
            
            CurrentTetromino.Rotate();
            if (CheckCollision())
            {
                // Try to shift if blocked by walls (wall kicks)
                CurrentTetromino.Move(-1, 0);
                if (!CheckCollision())
                {
                    OnRotate?.Invoke();
                    return;
                }
                
                CurrentTetromino.Move(2, 0);
                if (!CheckCollision())
                {
                    OnRotate?.Invoke();
                    return;
                }
                
                CurrentTetromino.Move(-1, 0);
                
                // Rotate back if still colliding
                CurrentTetromino.Rotate();
                CurrentTetromino.Rotate();
                CurrentTetromino.Rotate();
            }
            else
            {
                OnRotate?.Invoke();
            }
        }

        public void Drop()
        {
            if (IsGameOver || IsPaused || CurrentTetromino == null) return;
            
            while (!CheckCollision())
            {
                CurrentTetromino.Move(0, 1);
            }
            CurrentTetromino.Move(0, -1);
            OnDrop?.Invoke();
            GameTick();
        }
        
        public Tetromino? GetGhostPiece()
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
            if (CurrentTetromino == null) return true;
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
            if (CurrentTetromino == null) return;
            
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

        private async void ClearLines()
        {
            var fullLines = new List<int>();
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
                if (lineIsFull) fullLines.Add(y);
            }

            if (fullLines.Count > 0)
            {
                // Start line clear animation
                LinesBeingCleared = fullLines;
                _animatingLineClear = true;
                
                // Trigger sound effects
                if (fullLines.Count == 4)
                {
                    OnTetris?.Invoke();
                }
                else
                {
                    OnLinesCleared?.Invoke(fullLines.Count);
                }
                
                // Wait for animation
                await Task.Delay(500);
                
                // Actually clear the lines
                foreach (var line in fullLines.OrderByDescending(x => x))
                {
                    for (int row = line; row > 0; row--)
                    {
                        for (int col = 0; col < BoardWidth; col++)
                        {
                            Board[col, row] = Board[col, row - 1];
                        }
                    }
                    // Clear top row
                    for (int col = 0; col < BoardWidth; col++)
                    {
                        Board[col, 0] = 0;
                    }
                }
                
                // Update statistics
                LinesCleared += fullLines.Count;
                Level = (LinesCleared / 10) + 1;
                
                // Check for level up
                if (Level > _previousLevel)
                {
                    OnLevelUp?.Invoke();
                    _previousLevel = Level;
                }
                
                // Award points (traditional Tetris scoring)
                int points = fullLines.Count switch
                {
                    1 => 100,
                    2 => 300,
                    3 => 500,
                    4 => 800, // Tetris!
                    _ => 0
                };
                
                Score += points * Level;
                
                // End animation
                LinesBeingCleared.Clear();
                _animatingLineClear = false;
            }
        }

        public Color GetColorForBlock(int id)
        {
            return _tetrominoes.Find(t => t.Id == id)?.Color ?? Colors.Black;
        }
        
        // Check if board is completely clear (perfect clear)
        public bool IsBoardClear()
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (Board[x, y] != 0)
                        return false;
                }
            }
            return true;
        }
    }
}
