using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VedTetris
{
    /// <summary>
    /// Handles drawing the game state on the GraphicsView canvas.
    /// </summary>
    public class TetrisDrawable : IDrawable
    {
        public Game Game { get; set; }
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Game == null) return;

            float blockSize = (float)Math.Min(dirtyRect.Width / Game.BoardWidth, dirtyRect.Height / Game.BoardHeight);

            // Center the board
            float boardWidthPixels = blockSize * Game.BoardWidth;
            float boardHeightPixels = blockSize * Game.BoardHeight;
            float offsetX = (dirtyRect.Width - boardWidthPixels) / 2;
            float offsetY = (dirtyRect.Height - boardHeightPixels) / 2;


            // Draw the game board background
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(offsetX, offsetY, boardWidthPixels, boardHeightPixels);


            // Draw the landed blocks
            for (int y = 0; y < Game.BoardHeight; y++)
            {
                for (int x = 0; x < Game.BoardWidth; x++)
                {
                    if (Game.Board[x, y] != 0)
                    {
                        canvas.FillColor = Game.GetColorForBlock(Game.Board[x, y]);
                        canvas.FillRectangle(offsetX + x * blockSize, offsetY + y * blockSize, blockSize, blockSize);
                    }
                }
            }

            // Draw the current falling tetromino
            if (Game.CurrentTetromino != null)
            {
                canvas.FillColor = Game.CurrentTetromino.Color;
                foreach (var block in Game.CurrentTetromino.GetWorldPositions())
                {
                    canvas.FillRectangle(offsetX + (float)block.X * blockSize, offsetY + (float)block.Y * blockSize, blockSize, blockSize);
                }
            }
        }
    }
}
