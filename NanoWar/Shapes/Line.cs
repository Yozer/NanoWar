namespace NanoWar.Shapes
{
    using System;
    using System.Collections.Generic;

    using NanoWar.HelperClasses;

    using SFML.Graphics;
    using SFML.System;

    internal class Line : Drawable
    {
        private const float LineWidth = 2.0f;

        private const float DottedLineWidth = 15f;

        private const float DottedLineSpaces = 5f;

        private const float AnimationSpeed = 0.6f;

        private float _angle;

        private Vector2f _direction;

        private Vector2f _end;

        private float _moveFactor;

        private List<RectangleShape> _rectangleShapes = new List<RectangleShape>();

        public Line(Vector2f start)
        {
            Start = start;
            _angle = float.NaN;
        }

        public Vector2f End
        {
            get
            {
                return _end;
            }

            set
            {
                _end = value;
                if (Math.Abs(_end.X - value.Y) < MathHelper.Epsilon && Math.Abs(_end.X - value.Y) < MathHelper.Epsilon)
                {
                    return;
                }

                _angle = MathHelper.RadianToDegree(Math.Atan2(value.Y - Start.Y, value.X - Start.X));
                _direction = -MathHelper.NormalizeVector(new Vector2f(Start.X - _end.X, Start.Y - _end.Y));
            }
        }

        public Vector2f Start { get; private set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _rectangleShapes.ForEach(target.Draw);
        }

        private void ConstructLine()
        {
            _rectangleShapes.ForEach(t => t.Dispose());
            _rectangleShapes.Clear();
            var length = MathHelper.DistanceBetweenTwoPints(Start, _end) - _moveFactor;
            var startPoint = Start + _direction * _moveFactor;

            while (length > 0)
            {
                var rectangle =
                    new RectangleShape(new Vector2f(length - DottedLineWidth < 0 ? length : DottedLineWidth, LineWidth))
                        {
                            Position
                                =
                                startPoint, 
                            Rotation
                                =
                                _angle
                        };
                _rectangleShapes.Add(rectangle);
                length -= DottedLineWidth + DottedLineSpaces;
                startPoint += _direction * (DottedLineWidth + DottedLineSpaces);
            }

            if (_moveFactor > DottedLineSpaces)
            {
                var rectangle = new RectangleShape(new Vector2f(_moveFactor - DottedLineSpaces, LineWidth))
                                    {
                                        Position
                                            =
                                            Start, 
                                        Rotation
                                            =
                                            _angle
                                    };
                _rectangleShapes.Add(rectangle);
            }

            _moveFactor += AnimationSpeed;
            if (_moveFactor >= DottedLineSpaces + DottedLineWidth)
            {
                _moveFactor = 0;
            }
        }

        public void Update(float delta)
        {
            if (!float.IsNaN(_angle))
            {
                ConstructLine();
            }
        }

        public void Dispose()
        {
            _rectangleShapes.ForEach(t => t.Dispose());
        }
    }
}