namespace NanoWar.ParticleSystem
{
    using System;

    using SFML.Graphics;
    using SFML.System;

    public class Particle
    {
        public Particle(TimeSpan totalLifetime)
        {
            TextureIndex = 0;
            ElapsedLifetime = TimeSpan.Zero;
            Color = Color.White;
            Scale = new Vector2f(1, 1);
            RotationSpeed = 0;
            Rotation = 0;
            Velocity = new Vector2f(0, 0);
            Position = new Vector2f(0, 0);
            TotalLifetime = totalLifetime;
        }

        public Vector2f Position { get; set; }

        public Vector2f Velocity { get; set; }

        public float Rotation { get; set; }

        public float RotationSpeed { get; set; }

        public Vector2f Scale { get; set; }

        public Color Color { get; set; }

        public TimeSpan ElapsedLifetime { get; internal set; }

        public TimeSpan TotalLifetime { get; private set; }

        public int TextureIndex { get; set; }

        public TimeSpan RemainingLifetime
        {
            get
            {
                return TotalLifetime - ElapsedLifetime;
            }
        }

        public float ElapsedRatio
        {
            get
            {
                return (float)(ElapsedLifetime.TotalSeconds / TotalLifetime.TotalSeconds);
            }
        }

        public float RemainingRatio
        {
            get
            {
                return (float)(RemainingLifetime.TotalSeconds / TotalLifetime.TotalSeconds);
            }
        }
    }
}