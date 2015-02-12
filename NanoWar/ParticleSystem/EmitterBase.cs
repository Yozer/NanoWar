namespace NanoWar.ParticleSystem
{
    using System;

    public abstract class EmitterBase
    {
        public abstract void EmitParticles(ParticleSystem particleSystem, TimeSpan deltaTime);

        protected void EmitParticle(ParticleSystem particleSystem, Particle particle)
        {
            particleSystem.AddParticle(particle);
        }
    }
}