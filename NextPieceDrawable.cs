using Microsoft.Maui.Graphics;
using System;

namespace VedTetris
{
    /// <summary>
    /// Drawable for the Next Piece preview panel.
    /// </summary>
    public class NextPieceDrawable : IDrawable
    {
        // Using a private backing field to avoid ambiguity
        private Game _gameInstance;
        
        /// <summary>
        /// Gets or sets the game instance to draw the next piece from.
        /// </summary>
        public Game Game 
        { 
            get { return _gameInstance; } 
            set { _gameInstance = value; } 
        }
        
        /// <summary>
        /// Draws the next tetromino piece that will appear in the game.
        /// </summary>
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_gameInstance?.NextTetromino == null) return;
            
            // Calculate appropriate block size
            float blockSize = Math.Min(dirtyRect.Width / 4, dirtyRect.Height / 4);
            
            // Center the tetromino
            var tetromino = _gameInstance.NextTetromino;
            int width = tetromino.Shape.GetLength(1);
            int height = tetromino.Shape.GetLength(0);
            
            float offsetX = (dirtyRect.Width - (width * blockSize)) / 2;
            float offsetY = (dirtyRect.Height - (height * blockSize)) / 2;
            
            // Draw blocks
            canvas.FillColor = tetromino.Color;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tetromino.Shape[y, x] != 0)
                    {
                        canvas.FillRectangle(offsetX + (x * blockSize), offsetY + (y * blockSize), blockSize, blockSize);
                    }
                }
            }
        }
    }
}