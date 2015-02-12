namespace NanoWar.States.GameStateMenu
{
    using System;

    using NanoWar.Animation;

    using SFML.Graphics;
    using SFML.System;

    public class MenuItem : Text
    {
        private const float InitialScale = 0.9f;

        private ScaleAnimationHelper<Text> _animationHelper = new ScaleAnimationHelper<Text>();

        private Animator<Text, string> _animator = new Animator<Text, string>();

        private ScaleAnimation<Text> _decreaseAnimation = new ScaleAnimation<Text>(1f, InitialScale);

        private ScaleAnimation<Text> _increaseAnimation = new ScaleAnimation<Text>(InitialScale, 1f);

        public MenuItem()
        {
            Scale = new Vector2f(InitialScale, InitialScale);
            _animator.AddAnimation("increase", _increaseAnimation, TimeSpan.FromMilliseconds(200));
            _animator.AddAnimation("decrease", _decreaseAnimation, TimeSpan.FromMilliseconds(200));
        }

        public string LinkType { get; set; }

        public string LinkPath { get; set; }

        public bool IsActive { get; set; }

        public void Update(float delta)
        {
            _animationHelper.UpdateAnimation(this, _animator);

            _animator.Update(delta);
            _animator.Animate(this);
        }

        public void Dipose()
        {
            Dispose();
        }
    }
}