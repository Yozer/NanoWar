namespace NanoWar.ParticleSystem
{
    using System;

    public interface IAffector
    {
        void ApplyAffector(Particle currentParticle, TimeSpan elapsedTime, TimeSpan totalTime);
    }
}