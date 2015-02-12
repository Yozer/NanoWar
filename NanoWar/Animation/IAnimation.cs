namespace NanoWar.Animation
{
    using SFML.Graphics;

    public interface IAnimation<T>
        where T : Drawable
    {
        void Animate(AnimatedObject<T> animatedObject, float progress);
    }
}