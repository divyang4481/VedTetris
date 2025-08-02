using Microsoft.Maui.Graphics;
using System;

namespace VedTetris
{
    /// <summary>
    /// Drawable for the Hold Piece preview panel.
    /// </summary>
    public class HoldPieceDrawable : IDrawable
    {
        private Game? _gameInstance;
        
        /// <summary>
        /// Gets or sets the game instance to draw the held piece from.
        /// </summary>
        public Game? Game 
        { 
            get { return _gameInstance; } 
            set { _gameInstance = value; } 
        }
        
        /// <summary>
        /// Draws the held tetromino piece.
        /// </summary>
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_gameInstance?.HeldPiece == null) 
            {
                // Draw empty hold area
                canvas.StrokeColor = Colors.Gray.WithAlpha(0.5f);
                canvas.StrokeSize = 2;
                canvas.DrawRectangle(dirtyRect.X + 5, dirtyRect.Y + 5, dirtyRect.Width - 10, dirtyRect.Height - 10);
                return;
            }
            
            // Calculate appropriate block size
            float blockSize = Math.Min(dirtyRect.Width / 4, dirtyRect.Height / 4);
            
            // Center the tetromino
            var tetromino = _gameInstance.HeldPiece;
            int width = tetromino.Shape.GetLength(1);
            int height = tetromino.Shape.GetLength(0);
            
            float offsetX = (dirtyRect.Width - (width * blockSize)) / 2;
            float offsetY = (dirtyRect.Height - (height * blockSize)) / 2;
            
            // Draw blocks with slight dimming to show it's held
            Color heldColor = tetromino.Color.WithAlpha(0.8f);
            canvas.FillColor = heldColor;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tetromino.Shape[y, x] != 0)
                    {
                        canvas.FillRectangle(offsetX + (x * blockSize), offsetY + (y * blockSize), blockSize, blockSize);
                        
                        // Add border for held piece
                        canvas.StrokeColor = Colors.White.WithAlpha(0.5f);
                        canvas.StrokeSize = 1;
                        canvas.DrawRectangle(offsetX + (x * blockSize), offsetY + (y * blockSize), blockSize, blockSize);
                    }
                }
            }
        }
    }
}