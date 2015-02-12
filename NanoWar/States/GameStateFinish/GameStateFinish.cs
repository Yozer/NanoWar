namespace NanoWar.States.GameStateFinish
{
    using NanoWar.States.GameStateMenu;

    using SFML.Graphics;
    using SFML.System;
    using SFML.Window;

    internal class GameStateFinish : GameState
    {
        private Sprite _background;

        private Text _text;

        private bool _won;

        public GameStateFinish(string message, bool won)
        {
            _won = won;
            Game.Instance.AudioManager.PauseAllBackground();
            Game.Instance.AudioManager.PlaySound("finish/" + (won ? "win" : "lose") + "_music");

            _background = new Sprite(ResourceManager.Instance["menu/background"] as Texture);

            _text = new Text(message, ResourceManager.Instance["fonts/bebas_neue"] as Font, 50);
            _text.Origin = new Vector2f(_text.GetLocalBounds().Width / 2, _text.GetLocalBounds().Height / 2);
            _text.Position = new Vector2f(Game.Instance.Width / 2, Game.Instance.Height / 2);

            Game.Instance.Window.KeyReleased += WindowOnKeyReleased;
        }

        private void WindowOnKeyReleased(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Code == Keyboard.Key.Escape)
            {
                Game.Instance.StateMachine.PushState(new GameStateMenu());
            }
        }

        public override void Draw()
        {
            Game.Instance.Window.Draw(_background);
            Game.Instance.Window.Draw(_text);
        }

        public override void Update(float delta)
        {
        }

        public override void HandleInput()
        {
        }

        public override void Dispose()
        {
            Game.Instance.AudioManager.RemoveSound("finish/" + (_won ? "win" : "lose") + "_music");
            _text.Dispose();
            _background.Dispose();
        }
    }
}