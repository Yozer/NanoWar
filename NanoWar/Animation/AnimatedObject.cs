namespace NanoWar.Animation
{
    using SFML.Graphics;
    using SFML.System;

    public class AnimatedObject<T>
        where T : Drawable
    {
        private dynamic _object;

        public AnimatedObject(T Object)
        {
            _object = Object;
        }

        public IntRect TextureRect
        {
            get
            {
                return _object.TextureRect;
            }

            set
            {
                _object.TextureRect = value;
            }
        }

        public Color Color
        {
            get
            {
                return _object.Color;
            }

            set
            {
                _object.Color = value;
            }
        }

        public Vector2f Position
        {
            get
            {
                return _object.Position;
            }

            set
            {
                _object.Position = value;
            }
        }

        public float Rotation
        {
            get
            {
                return _object.Rotation;
            }

            set
            {
                _object.Rotation = value;
            }
        }

        public Vector2f Scale
        {
            get
            {
                return _object.Scale;
            }

            set
            {
                _object.Scale = value;
            }
        }

        public static implicit operator AnimatedObject<T>(T Object)
        {
            return new AnimatedObject<T>(Object);
        }

        public FloatRect GetGlobalBounds()
        {
            return _object.GetGlobalBounds();
        }
    }
}