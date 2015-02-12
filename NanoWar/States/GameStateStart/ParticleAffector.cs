namespace NanoWar.States.GameStateStart
{
    using System;

    using NanoWar.HelperClasses;
    using NanoWar.ParticleSystem;

    using SFML.Graphics;
    using SFML.System;

    internal class ParticleAffector : IAffector
    {
        private Vector2f _explosionOrigin;

        private float _maxDistance;

        private Vector2f _origin;

        private float _radius;

        public ParticleAffector(float maxDistance, Vector2f origin, float radius, Vector2f explosionOrigin)
        {
            _maxDistance = maxDistance;
            _origin = origin;
            _radius = radius;
            _explosionOrigin = explosionOrigin;
        }

        public void ApplyAffector(Particle currentParticle, TimeSpan elapsedTime, TimeSpan totalTime)
        {
            if (currentParticle.Color.A == 254)
            {
                if (MathHelper.DistanceBetweenTwoPints(currentParticle.Position, _origin) <= _radius)
                {
                    currentParticle.Velocity = new Vector2f(0, 0);
                    currentParticle.Color = new Color(0, 0, 0, 0);
                }
            }
            else if (MathHelper.DistanceBetweenTwoPints(currentParticle.Position, _explosionOrigin) >= _maxDistance)
            {
                currentParticle.Color = new Color(
                    currentParticle.Color.R, 
                    currentParticle.Color.G, 
                    currentParticle.Color.B, 
                    254);
                currentParticle.Velocity = 350
                                           * MathHelper.NormalizeVector(
                                               new Vector2f(
                                                 _origin.X - currentParticle.Position.X, 
                                                 _origin.Y - currentParticle.Position.Y));
            }
            else if (currentParticle.Color.A != 0)
            {
                var speed = (float)Math.Log(totalTime.TotalSeconds, 5.5f);
                if (float.IsInfinity(speed) || float.IsNaN(speed))
                {
                    return;
                }

                currentParticle.Velocity += MathHelper.NormalizeVector(currentParticle.Velocity) * speed;
            }
        }
    }
}