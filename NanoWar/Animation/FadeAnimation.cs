namespace NanoWar.Animation
{
    using SFML.Graphics;

    internal class FadeAnimation : IAnimation<Sprite>
    {
        public FadeAnimation(float fadeInRatio, float fadeOutRatio)
        {
            FadeInRatio = fadeInRatio;
            FadeOutRatio = fadeOutRatio;
        }

        public float FadeInRatio { get; set; }

        public float FadeOutRatio { get; set; }

        public void Animate(AnimatedObject<Sprite> animatedObject, float progress)
        {
            if (progress < FadeInRatio)
            {
                animatedObject.Color = new Color(
                    animatedObject.Color.R, 
                    animatedObject.Color.G, 
                    animatedObject.Color.B, 
                    (byte)(256f * progress / FadeInRatio));
            }
            else if (progress > 1f - FadeOutRatio)
            {
                animatedObject.Color = new Color(
                    animatedObject.Color.R, 
                    animatedObject.Color.G, 
                    animatedObject.Color.B, 
                    (byte)(256f * (1f - progress) / FadeOutRatio));
            }
        }
    }
}