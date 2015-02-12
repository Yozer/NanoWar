namespace NanoWar.Animation
{
    using SFML.Graphics;
    using SFML.System;
    using SFML.Window;

    public class ScaleAnimation<T> : IAnimation<T>
        where T : Drawable
    {
        public ScaleAnimation(float initialScale, float targetScale)
        {
            InitialScale = initialScale;
            TargetScale = targetScale;
        }

        public float InitialScale { get; private set; }

        public float TargetScale { get; private set; }

        public void Animate(AnimatedObject<T> animatedObject, float progress)
        {
            var factor = (TargetScale - InitialScale) * progress;
            animatedObject.Scale = new Vector2f(InitialScale + factor, InitialScale + factor);
        }
    }

    public class ScaleAnimationHelper<T>
        where T : Drawable
    {
        private bool _isAnimating;

        private bool _isIncreasing;

        public void UpdateAnimation(AnimatedObject<T> animatedObject, dynamic animator)
        {
            if (
                !animatedObject.GetGlobalBounds()
                     .Contains(Mouse.GetPosition(Game.Instance.Window).X, Mouse.GetPosition(Game.Instance.Window).Y))
            {
                if (_isIncreasing && !_isAnimating)
                {
                    _isIncreasing = false;
                    _isAnimating = true;
                    animator.PlayAnimation("decrease");
                }
                else if (_isIncreasing && _isAnimating)
                {
                    animator.StopAnimation("increase");
                    _isAnimating = false;
                }
            }
            else
            {
                if (!_isIncreasing && !_isAnimating)
                {
                    animator.PlayAnimation("increase");
                    _isIncreasing = _isAnimating = true;
                }
                else if (animator.AnimationsPlayingCount == 0)
                {
                    _isAnimating = false;
                }
            }
        }
    }
}