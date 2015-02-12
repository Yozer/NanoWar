namespace NanoWar.Shapes
{
    using System;

    using SFML.Graphics;
    using SFML.System;

    internal class RoundedRectangleShape : Shape
    {
        private uint _cornerPointCount;

        private float _radius;

        private Vector2f _size;

        public RoundedRectangleShape(Vector2f size, float radius, uint cornerPointCount)
        {
            Size = size;
            Radius = radius;
            CornerPointCount = cornerPointCount;
        }

        public RoundedRectangleShape(float radius, uint cornerPointCount)
        {
            Size = Vector2f.Zero;
            Radius = radius;
            CornerPointCount = cornerPointCount;
        }

        public uint CornerPointCount
        {
            get
            {
                return _cornerPointCount;
            }

            set
            {
                _cornerPointCount = value;
                Update();
            }
        }

        public float Radius
        {
            get
            {
                return _radius;
            }

            set
            {
                _radius = value;
                Update();
            }
        }

        public Vector2f Size
        {
            get
            {
                return _size;
            }

            set
            {
                _size = value;
                Update();
            }
        }

        public new Vector2f Origin
        {
            get
            {
                return base.Origin;
            }

            set
            {
                base.Origin = value;
                Update();
            }
        }

        public new Vector2f Position
        {
            get
            {
                return base.Position - new Vector2f(0, Size.Y);
            }

            set
            {
                base.Position = value;
                Update();
            }
        }

        public override uint GetPointCount()
        {
            return _cornerPointCount * 4;
        }

        public override Vector2f GetPoint(uint index)
        {
            if (index >= _cornerPointCount * 4)
            {
                return new Vector2f(0, 0);
            }

            var deltaAngle = 90.0f / (_cornerPointCount - 1);
            var center = new Vector2f(0, 0);
            var centerIndex = index / _cornerPointCount;

            switch (centerIndex)
            {
                case 0:
                    center.X = _size.X - _radius;
                    center.Y = _radius;
                    break;
                case 1:
                    center.X = _radius;
                    center.Y = _radius;
                    break;
                case 2:
                    center.X = _radius;
                    center.Y = _size.Y - _radius;
                    break;
                case 3:
                    center.X = _size.X - _radius;
                    center.Y = _size.Y - _radius;
                    break;
            }

            return new Vector2f(
                _radius * (float)Math.Cos(deltaAngle * (index - centerIndex) * Math.PI / 180) + center.X, 
                _radius * (float)Math.Sin(deltaAngle * (index - centerIndex) * Math.PI / 180) - center.Y);
        }
    }
}