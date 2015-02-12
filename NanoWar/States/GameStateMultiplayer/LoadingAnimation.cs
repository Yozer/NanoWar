namespace NanoWar.States.GameStateMultiplayer
{
    using System;

    using NanoWar.Animation;

    using SFML.Graphics;
    using SFML.System;

    internal class LoadingAnimation : Drawable
    {
        private Animator<Sprite, string> _animator = new Animator<Sprite, string>();

        private RotateAnimation<Sprite> _loadingAnimation = new RotateAnimation<Sprite>();

        private Sprite _loadingSprite;

        public LoadingAnimation()
        {
            _loadingSprite = new Sprite(ResourceManager.Instance["multiplayer/loading"] as Texture);
            _loadingSprite.Origin = new Vector2f(
                _loadingSprite.TextureRect.Width / 2, 
                _loadingSprite.TextureRect.Height / 2);
            _loadingSprite.Position = new Vector2f(Game.Instance.Width / 2, Game.Instance.Height / 2);
            _animator.AddAnimation("loading_animation", _loadingAnimation, TimeSpan.FromMilliseconds(1600));
            _animator.PlayAnimation("loading_animation");
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_loadingSprite);
        }

        public void Dipose()
        {
            _loadingSprite.Dispose();
        }

        public void Update(float delta)
        {
            if (_animator.AnimationsPlayingCount == 0)
            {
                _loadingSprite.Rotation = 0f;
                _animator.PlayAnimation("loading_animation");
            }

            _animator.Update(delta);
            _animator.Animate(_loadingSprite);
        }
    }
}