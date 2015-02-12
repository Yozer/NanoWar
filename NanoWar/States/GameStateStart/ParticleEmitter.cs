namespace NanoWar.States.GameStateStart
{
    using System;

    using NanoWar.HelperClasses;
    using NanoWar.ParticleSystem;

    using SFML.Graphics;
    using SFML.System;

    internal class ParticleEmitter : EmitterBase
    {
        private static Random _rndgenerator = new Random();

        public static readonly TimeSpan ExplostionDuration = TimeSpan.FromSeconds(0.1f);

        private float _angle;

        private Color _color;

        private TimeSpan _interval;

        private int _particleCount;

        private Vector2f _position;

        public ParticleEmitter(Vector2f position, float angle, Color color, int particleCount)
        {
            _position = position;
            _color = color;
            _particleCount = particleCount;
            _angle = Math.Abs(angle - 360f);
            _interval = TimeSpan.FromMilliseconds(ExplostionDuration.TotalMilliseconds / _particleCount);
        }

        public override void EmitParticles(ParticleSystem particleSystem, TimeSpan deltaTime)
        {
            for (var i = 0;
                 i
                 < (deltaTime == TimeSpan.Zero
                        ? int.MaxValue
                        : deltaTime.TotalMilliseconds / _interval.TotalMilliseconds) && _particleCount > 0;
                 i++, _particleCount--)
            {
                var velocity = new PolarVector(
                    _rndgenerator.Next(7000, 9000) / 100f, 
                    _rndgenerator.Next((int)(_angle - 85f), (int)(_angle + 85f)));

                var particle = new Particle(TimeSpan.Zero) { Position = _position, Velocity = velocity, Color = _color };
                EmitParticle(particleSystem, particle);
            }
        }
    }
}