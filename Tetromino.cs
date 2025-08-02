using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VedTetris
{
    /// <summary>
    /// Represents a single Tetris piece (Tetromino).
    /// </summary>
    public class Tetromino
    {
        public int Id { get; }
        public Point Position { get; set; }
        public int[,] Shape { get; private set; }
        public Color Color { get; }

        public Tetromino(int id, Point position, int[,] shape, Color color)
        {
            Id = id;
            Position = position;
            Shape = shape;
            Color = color;
        }

        public void Move(int dx, int dy)
        {
            Position = new Point(Position.X + dx, Position.Y + dy);
        }

        public void Rotate()
        {
            int width = Shape.GetLength(1);
            int height = Shape.GetLength(0);
            int[,] newShape = new int[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    newShape[x, y] = Shape[height - 1 - y, x];
                }
            }
            Shape = newShape;
        }

        public IEnumerable<Point> GetWorldPositions()
        {
            for (int y = 0; y < Shape.GetLength(0); y++)
            {
                for (int x = 0; x < Shape.GetLength(1); x++)
                {
                    if (Shape[y, x] != 0)
                    {
                        yield return new Point(Position.X + x, Position.Y + y);
                    }
                }
            }
        }

        public Tetromino Clone()
        {
            return new Tetromino(Id, new Point(Position.X, Position.Y), (int[,])Shape.Clone(), Color);
        }
    }
}
