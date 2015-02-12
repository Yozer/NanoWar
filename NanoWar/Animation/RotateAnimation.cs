namespace NanoWar.Animation
{
    using System;

    using SFML.Graphics;

    internal class RotateAnimation<T> : IAnimation<T>
        where T : Drawable
    {
        public void Animate(AnimatedObject<T> animatedObject, float progress)
        {
            animatedObject.Rotation = (float)(Math.Floor(progress * 24) * (360f / 24));
        }
    }
}