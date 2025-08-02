using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VedTetris
{
    /// <summary>
    /// Handles drawing the game state on the GraphicsView canvas with enhanced visual effects.
    /// </summary>
    public class TetrisDrawable : IDrawable
    {
        public Game? Game { get; set; }
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Game == null) return;

            float blockSize = (float)Math.Min(dirtyRect.Width / Game.BoardWidth, dirtyRect.Height / Game.BoardHeight);

            // Center the board
            float boardWidthPixels = blockSize * Game.BoardWidth;
            float boardHeightPixels = blockSize * Game.BoardHeight;
            float offsetX = (dirtyRect.Width - boardWidthPixels) / 2;
            float offsetY = (dirtyRect.Height - boardHeightPixels) / 2;

            // Draw the game board background with gradient
            DrawBoardBackground(canvas, offsetX, offsetY, boardWidthPixels, boardHeightPixels);
            
            // Draw grid lines
            DrawGridLines(canvas, offsetX, offsetY, boardWidthPixels, boardHeightPixels, blockSize);

            // Draw ghost piece (shadow of where the current piece will land)
            if (!Game.IsGameOver && !Game.IsPaused)
            {
                DrawGhostPiece(canvas, offsetX, offsetY, blockSize);
            }

            // Draw the landed blocks with enhanced visuals
            DrawLandedBlocks(canvas, offsetX, offsetY, blockSize);

            // Draw line clearing animation
            DrawLineClearAnimation(canvas, offsetX, offsetY, blockSize);

            // Draw the current falling tetromino
            if (Game.CurrentTetromino != null && !Game.IsGameOver)
            {
                DrawTetromino(canvas, Game.CurrentTetromino, offsetX, offsetY, blockSize, false);
            }
            
            // Draw pause overlay
            if (Game.IsPaused)
            {
                DrawPauseOverlay(canvas, dirtyRect);
            }
        }
        
        private static void DrawBoardBackground(ICanvas canvas, float offsetX, float offsetY, float width, float height)
        {
            // Dark background
            canvas.FillColor = Color.FromArgb("#1a2332");
            canvas.FillRectangle(offsetX, offsetY, width, height);
            
            // Border
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(offsetX, offsetY, width, height);
        }
        
        private void DrawGridLines(ICanvas canvas, float offsetX, float offsetY, float width, float height, float blockSize)
        {
            if (Game == null) return;
            
            canvas.StrokeColor = Colors.Gray.WithAlpha(0.2f);
            canvas.StrokeSize = 1;
            
            // Vertical lines
            for (int x = 1; x < Game.BoardWidth; x++)
            {
                canvas.DrawLine(offsetX + x * blockSize, offsetY, offsetX + x * blockSize, offsetY + height);
            }
            
            // Horizontal lines
            for (int y = 1; y < Game.BoardHeight; y++)
            {
                canvas.DrawLine(offsetX, offsetY + y * blockSize, offsetX + width, offsetY + y * blockSize);
            }
        }
        
        private void DrawGhostPiece(ICanvas canvas, float offsetX, float offsetY, float blockSize)
        {
            if (Game == null) return;
            
            var ghost = Game.GetGhostPiece();
            if (ghost != null)
            {
                DrawTetromino(canvas, ghost, offsetX, offsetY, blockSize, true);
            }
        }
        
        private void DrawLandedBlocks(ICanvas canvas, float offsetX, float offsetY, float blockSize)
        {
            if (Game == null) return;
            
            for (int y = 0; y < Game.BoardHeight; y++)
            {
                for (int x = 0; x < Game.BoardWidth; x++)
                {
                    if (Game.Board[x, y] != 0)
                    {
                        // Skip blocks that are being cleared
                        if (Game.LinesBeingCleared.Contains(y))
                            continue;
                            
                        Color blockColor = Game.GetColorForBlock(Game.Board[x, y]);
                        DrawEnhancedBlock(canvas, blockColor, offsetX + x * blockSize, offsetY + y * blockSize, blockSize);
                    }
                }
            }
        }
        
        private void DrawLineClearAnimation(ICanvas canvas, float offsetX, float offsetY, float blockSize)
        {
            if (Game == null) return;
            
            if (Game.LinesBeingCleared.Count > 0)
            {
                // Flash effect for clearing lines
                var flashColor = Colors.White.WithAlpha(0.7f);
                canvas.FillColor = flashColor;
                
                foreach (var lineY in Game.LinesBeingCleared)
                {
                    canvas.FillRectangle(offsetX, offsetY + lineY * blockSize, Game.BoardWidth * blockSize, blockSize);
                }
            }
        }
        
        private static void DrawTetromino(ICanvas canvas, Tetromino tetromino, float offsetX, float offsetY, float blockSize, bool isGhost)
        {
            foreach (var block in tetromino.GetWorldPositions())
            {
                if (block.Y >= 0) // Only draw visible blocks
                {
                    if (isGhost)
                    {
                        DrawGhostBlock(canvas, tetromino.Color, offsetX + (float)block.X * blockSize, offsetY + (float)block.Y * blockSize, blockSize);
                    }
                    else
                    {
                        DrawEnhancedBlock(canvas, tetromino.Color, offsetX + (float)block.X * blockSize, offsetY + (float)block.Y * blockSize, blockSize);
                    }
                }
            }
        }
        
        private static void DrawEnhancedBlock(ICanvas canvas, Color color, float x, float y, float size)
        {
            // Main block
            canvas.FillColor = color;
            canvas.FillRectangle(x, y, size, size);
            
            // Highlight (top-left) - create brighter version of the color
            var highlightColor = Color.FromRgba(
                Math.Min(255, (int)(color.Red * 255 + 40)),
                Math.Min(255, (int)(color.Green * 255 + 40)),
                Math.Min(255, (int)(color.Blue * 255 + 40)),
                255
            );
            canvas.FillColor = highlightColor;
            
            // Top highlight
            canvas.FillRectangle(x + 2, y + 2, size - 4, size * 0.2f);
            // Left highlight
            canvas.FillRectangle(x + 2, y + 2, size * 0.2f, size - 4);
            
            // Shadow (bottom-right) - create darker version of the color
            var shadowColor = Color.FromRgba(
                Math.Max(0, (int)(color.Red * 255 - 60)),
                Math.Max(0, (int)(color.Green * 255 - 60)),
                Math.Max(0, (int)(color.Blue * 255 - 60)),
                255
            );
            canvas.FillColor = shadowColor;
            
            // Bottom shadow
            canvas.FillRectangle(x + 2, y + size - size * 0.2f - 2, size - 4, size * 0.2f);
            // Right shadow
            canvas.FillRectangle(x + size - size * 0.2f - 2, y + 2, size * 0.2f, size - 4);
            
            // Border
            canvas.StrokeColor = Colors.Black.WithAlpha(0.3f);
            canvas.StrokeSize = 1;
            canvas.DrawRectangle(x, y, size, size);
        }
        
        private static void DrawGhostBlock(ICanvas canvas, Color color, float x, float y, float size)
        {
            // Outline only for ghost piece
            canvas.StrokeColor = color.WithAlpha(0.4f);
            canvas.StrokeSize = 2;
            canvas.DrawRectangle(x + 1, y + 1, size - 2, size - 2);
            
            // Inner cross pattern
            canvas.DrawLine(x + size * 0.25f, y + size * 0.5f, x + size * 0.75f, y + size * 0.5f);
            canvas.DrawLine(x + size * 0.5f, y + size * 0.25f, x + size * 0.5f, y + size * 0.75f);
        }
        
        private static void DrawPauseOverlay(ICanvas canvas, RectF dirtyRect)
        {
            // Semi-transparent overlay
            canvas.FillColor = Colors.Black.WithAlpha(0.6f);
            canvas.FillRectangle(dirtyRect);
            
            // Pause text
            canvas.FontColor = Colors.White;
            canvas.FontSize = 24;
            canvas.DrawString("PAUSED", dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
