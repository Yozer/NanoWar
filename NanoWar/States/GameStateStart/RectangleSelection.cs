namespace NanoWar.States.GameStateStart
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SFML.Graphics;
    using SFML.System;

    internal class RectangleSelection : RectangleShape
    {
        public RectangleSelection()
        {
            OutlineColor = Color.White;
            FillColor = Color.Transparent;
            OutlineThickness = 2.0f;
            Position = new Vector2f(0, 0);
        }

        public bool IsDrawable { get; set; }

        private bool Intersect(Cell cell)
        {
            var circleDistance = new Vector2f(
                Math.Abs(cell.Position.X - (Position.X + Size.X / 2)), 
                Math.Abs(cell.Position.Y - (Position.Y + Size.Y / 2)));
            var sizeAbs = new Vector2f(Math.Abs(Size.X), Math.Abs(Size.Y));

            if (circleDistance.X > (sizeAbs.X / 2 + cell.Radius))
            {
                return false;
            }

            if (circleDistance.Y > (sizeAbs.Y / 2 + cell.Radius))
            {
                return false;
            }

            if (circleDistance.X <= sizeAbs.X / 2)
            {
                return true;
            }

            if (circleDistance.Y <= sizeAbs.Y / 2)
            {
                return true;
            }

            var cornerDistance = (circleDistance.X - sizeAbs.X / 2) * (circleDistance.X - sizeAbs.X / 2)
                                 + (circleDistance.Y - sizeAbs.Y / 2) * (circleDistance.Y - sizeAbs.Y / 2);

            return cornerDistance <= cell.RadiusPow;
        }

        public List<Cell> GetIntersectCells(List<Cell> cells)
        {
            return cells.Where(Intersect).ToList();
        }
    }
}