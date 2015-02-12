namespace NanoWar.States.GameStateIntro
{
    using System;
    using System.Collections.Generic;

    using NanoWar.Animation;
    using NanoWar.States.GameStateMenu;

    using SFML.Graphics;
    using SFML.System;

    internal class GameStateIntro : GameState
    {
        private Animator<Sprite, string> _animator = new Animator<Sprite, string>();

        private FadeAnimation _fadeInAnimation = new FadeAnimation(1f, 0f);

        private FadeAnimation _fadeOutAnimation = new FadeAnimation(0f, 1f);

        private bool _fadingIn = true;

        private Queue<Sprite> _sprites = new Queue<Sprite>();

        public GameStateIntro()
        {
            _sprites.Enqueue(CreateSprite("intro/dolby_logo"));
            _sprites.Enqueue(CreateSprite("intro/nvidia_logo"));

            _animator.AddAnimation("fade_in", _fadeInAnimation, TimeSpan.FromMilliseconds(1300));
            _animator.AddAnimation("fade_out", _fadeOutAnimation, TimeSpan.FromMilliseconds(1300));
            _animator.PlayAnimation("fade_in");

            Game.Instance.Window.SetMouseCursorVisible(false);
        }

        public override void Draw()
        {
            if (_sprites.Count == 0)
            {
                return;
            }

            Game.Instance.Window.Draw(_sprites.Peek());
        }

        public override void Update(float delta)
        {
            if (_sprites.Count == 0)
            {
                Game.Instance.StateMachine.PushState(new GameStateMenu());
                return;
            }

            if (_animator.AnimationsPlayingCount == 0)
            {
                if (_fadingIn)
                {
                    _animator.PlayAnimation("fade_out");
                    _fadingIn = false;
                }
                else
                {
                    _sprites.Dequeue();
                    if (_sprites.Count != 0)
                    {
                        _animator.PlayAnimation("fade_in");
                        _fadingIn = true;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            _animator.Update(delta);
            _animator.Animate(_sprites.Peek());
        }

        public override void HandleInput()
        {
        }

        public override void Dispose()
        {
            while (_sprites.Count > 0)
            {
                _sprites.Dequeue().Dispose();
            }

            Game.Instance.Window.SetMouseCursorVisible(true);
        }

        private Sprite CreateSprite(string texturePath)
        {
            var sprite = new Sprite { Texture = ResourceManager.Instance[texturePath] as Texture };

            if (sprite.Texture != null)
            {
                sprite.Origin = new Vector2f(sprite.Texture.Size.X / 2, sprite.Texture.Size.Y / 2);
            }

            sprite.Position = new Vector2f(Game.Instance.Width / 2, Game.Instance.Height / 2);
            sprite.Color = new Color(sprite.Color.R, sprite.Color.G, sprite.Color.B, 0);

            return sprite;
        }
    }
}