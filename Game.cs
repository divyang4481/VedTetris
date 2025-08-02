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
        public int Score { get; private set; }
        public bool IsGameOver { get; private set; }

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
            IsGameOver = false;
            SpawnNewTetromino();
        }

        private void SpawnNewTetromino()
        {
            CurrentTetromino = _tetrominoes[_random.Next(_tetrominoes.Count)].Clone();
            CurrentTetromino.Position = new Point(BoardWidth / 2 - 1, 0);

            if (CheckCollision())
            {
                IsGameOver = true;
            }
        }

        public void GameTick()
        {
            if (IsGameOver) return;

            CurrentTetromino.Move(0, 1);
            if (CheckCollision())
            {
                CurrentTetromino.Move(0, -1);
                PlaceTetromino();
                ClearLines();
                SpawnNewTetromino();
            }
        }

        public void MoveLeft()
        {
            CurrentTetromino.Move(-1, 0);
            if (CheckCollision())
            {
                CurrentTetromino.Move(1, 0);
            }
        }

        public void MoveRight()
        {
            CurrentTetromino.Move(1, 0);
            if (CheckCollision())
            {
                CurrentTetromino.Move(-1, 0);
            }
        }

        public void Rotate()
        {
            CurrentTetromino.Rotate();
            if (CheckCollision())
            {
                CurrentTetromino.Rotate(); // Rotate back
                CurrentTetromino.Rotate();
                CurrentTetromino.Rotate();
            }
        }

        public void Drop()
        {
            while (!CheckCollision())
            {
                CurrentTetromino.Move(0, 1);
            }
            CurrentTetromino.Move(0, -1);
            GameTick();
        }

        private bool CheckCollision()
        {
            foreach (var block in CurrentTetromino.GetWorldPositions())
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
            Score += linesCleared * linesCleared * 100; // Simple scoring
        }

        public Color GetColorForBlock(int id)
        {
            return _tetrominoes.Find(t => t.Id == id)?.Color ?? Colors.Black;
        }
    }

}
