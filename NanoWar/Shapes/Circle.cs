namespace NanoWar.Shapes
{
    using System;

    using NanoWar.HelperClasses;

    using SFML.Graphics;
    using SFML.System;

    public class Circle : Drawable
    {
        private CircleShape _shape;

        private Text _text;

        public Circle(Vector2f position, float radius, uint fontSize)
        {
            _shape = new CircleShape(radius, 200);
            _shape.Origin = new Vector2f(_shape.Radius, _shape.Radius);
            _shape.Position = position;

            RadiusPow = _shape.Radius * _shape.Radius;

            _text = new Text(string.Empty, ResourceManager.Instance["fonts/verdana"] as Font, fontSize)
                        {
                            Color =
                                Color
                                .Black
                        };

            UpdateCircleText(string.Empty);
        }

        public string Text
        {
            get
            {
                return _text.DisplayedString;
            }

            set
            {
                UpdateCircleText(value);
            }
        }

        public Vector2f Position
        {
            get
            {
                return _shape.Position;
            }

            set
            {
                _shape.Position = value;
                UpdateCircleText(_text.DisplayedString);
            }
        }

        public float Radius
        {
            get
            {
                return _shape.Radius;
            }
        }

        public Vector2f Scale
        {
            get
            {
                return _shape.Scale;
            }

            set
            {
                _shape.Scale = value;
            }
        }

        public Color Color
        {
            get
            {
                return _shape.FillColor;
            }

            set
            {
                _shape.FillColor = value;
            }
        }

        public float RadiusPow { get; set; }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_shape);
            target.Draw(_text);
        }

        private void UpdateCircleText(string str)
        {
            _text.DisplayedString = str;
            _text.Origin = new Vector2f(
                _text.GetLocalBounds().Left + _text.GetLocalBounds().Width / 2, 
                _text.GetLocalBounds().Top + _text.GetLocalBounds().Height / 2);
            _text.Position = _shape.Position;
        }

        public virtual void Dispose()
        {
            _text.Dispose();
            _shape.Dispose();
        }

        public bool ContainsPoint(Vector2f point)
        {
            return (_shape.Position.X - point.X) * (_shape.Position.X - point.X)
                   + (_shape.Position.Y - point.Y) * (_shape.Position.Y - point.Y) < RadiusPow;
        }

        private double GetDistanceBetweenCircles(Circle circle)
        {
            var distanceX = Position.X - circle.Position.X;
            var distanceY = Position.Y - circle.Position.Y;
            return Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
        }

        public bool ContainsCircle(Circle circle)
        {
            // bigger circle cannot be inside smaller one
            if (circle.Radius > Radius)
            {
                return false;
            }

            var distance = GetDistanceBetweenCircles(circle);

            return distance <= Math.Abs(Radius - circle.Radius);
        }

        public bool IntersectCircle(Circle circle)
        {
            var distance = GetDistanceBetweenCircles(circle);

            // check distance
            return distance <= circle.Radius + Radius;
        }

        public override bool Equals(object obj)
        {
            if (obj is Circle)
            {
                var c = obj as Circle;
                return Math.Abs(c.Position.X - Position.X) < MathHelper.Epsilon
                       && Math.Abs(c.Position.Y - Position.Y) < MathHelper.Epsilon
                       && Math.Abs(c.Radius - Radius) < MathHelper.Epsilon;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}