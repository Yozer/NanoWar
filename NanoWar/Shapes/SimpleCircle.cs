namespace NanoWar.Shapes
{
    using System;

    using SFML.System;

    public class SimpleCircle
    {
        private float _radius;

        public float Radius
        {
            get
            {
                return _radius;
            }

            protected internal set
            {
                _radius = value;
                RadiusPow = _radius * _radius;
            }
        }

        public float RadiusPow { get; private set; }

        public virtual Vector2f Position { get; set; }

        public bool ContainsPoint(Vector2f point)
        {
            return (Position.X - point.X) * (Position.X - point.X) + (Position.Y - point.Y) * (Position.Y - point.Y)
                   < RadiusPow;
        }

        public bool ContainsPoint(Vector2i point)
        {
            return ContainsPoint(new Vector2f(point.X, point.Y));
        }

        public float GetDistanceBetweenCells(SimpleCircle circle)
        {
            var distanceX = Position.X - circle.Position.X;
            var distanceY = Position.Y - circle.Position.Y;
            return (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
        }

        public bool ContainsCircle(SimpleCircle circle)
        {
            if (circle.Radius > _radius)
            {
                return false;
            }

            var distance = GetDistanceBetweenCells(circle);
            return distance <= Math.Abs(_radius - circle.Radius);
        }

        public bool IntersectCircle(SimpleCircle circle, float padding = 0f)
        {
            var distance = GetDistanceBetweenCells(circle);
            return distance <= circle.Radius + _radius + padding;
        }
    }
}