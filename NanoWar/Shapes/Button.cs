namespace NanoWar.Shapes
{
    using System;

    using NanoWar.Animation;

    using SFML.Graphics;
    using SFML.System;
    using SFML.Window;

    internal class Button : Drawable
    {
        private ScaleAnimationHelper<Button> _animationHelper = new ScaleAnimationHelper<Button>();

        private Animator<Button, string> _animator = new Animator<Button, string>();

        private Text _text;

        public bool IsAnimated = true;

        public Button(string text, Font font, uint fontSize, Vector2f position, float scale = 1f)
        {
            _text = new Text(text, font, fontSize);
            _text.Origin = new Vector2f(
                _text.GetLocalBounds().Left + _text.GetLocalBounds().Width / 2, 
                _text.GetLocalBounds().Top + _text.GetLocalBounds().Height);
            _text.Position = position;
            _text.Scale = new Vector2f(scale, scale);

            var increaseAnimation = new ScaleAnimation<Button>(scale, 1f);
            var decreaseAnimation = new ScaleAnimation<Button>(1f, scale);
            _animator.AddAnimation("increase", increaseAnimation, TimeSpan.FromMilliseconds(200));
            _animator.AddAnimation("decrease", decreaseAnimation, TimeSpan.FromMilliseconds(200));
        }

        public Vector2f Position
        {
            get
            {
                return _text.Position;
            }

            set
            {
                _text.Position = value;
            }
        }

        public Vector2f Scale
        {
            get
            {
                return _text.Scale;
            }

            set
            {
                _text.Scale = value;
            }
        }

        public string Text
        {
            get
            {
                return _text.DisplayedString;
            }
        }

        public Vector2f Origin
        {
            get
            {
                return _text.Origin;
            }

            set
            {
                _text.Origin = value;
            }
        }

        public Color Color
        {
            get
            {
                return _text.Color;
            }

            set
            {
                _text.Color = value;
            }
        }

        public object Data { get; set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_text);
        }

        public event EventHandler OnClick;

        public void Update(float delta)
        {
            if (!IsAnimated)
            {
                return;
            }

            _animationHelper.UpdateAnimation(this, _animator);
            _animator.Update(delta);
            _animator.Animate(this);
        }

        public void HandleInput(MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (OnClick != null && _text.GetGlobalBounds().Contains(mouseButtonEventArgs.X, mouseButtonEventArgs.Y))
            {
                Game.Instance.AudioManager.PlaySound("menu/click_sound");
                OnClick(this, null);
            }
        }

        public void Dispose()
        {
            _text.Dispose();
        }

        public FloatRect GetGlobalBounds()
        {
            return _text.GetGlobalBounds();
        }

        public FloatRect GetLocalBounds()
        {
            return _text.GetLocalBounds();
        }
    }
}