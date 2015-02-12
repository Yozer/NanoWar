namespace NanoWar.States.GameStateLobby
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NanoWar.GameClient;
    using NanoWar.Shapes;
    using NanoWar.States.GameStateMultiplayer;

    using SFML.Graphics;
    using SFML.System;
    using SFML.Window;

    internal class GameStateLobby : GameState
    {
        private const float InitialScale = 0.9f;

        private Sprite _background;

        private List<Button> _buttons = new List<Button>();

        private LobbyPanel _lobbyPanel = new LobbyPanel();

        public GameStateLobby()
        {
            Game.Instance.AudioManager.PauseAllBackground();
            Game.Instance.AudioManager.PlaySound("menu/background_music", true);

            _background = new Sprite(ResourceManager.Instance["menu/background"] as Texture);
            Game.Instance.Window.MouseButtonReleased += Window_MouseButtonReleased;
            PrepareUi();
        }

        private void PrepareUi()
        {
            var font = ResourceManager.Instance["fonts/bebas_neue"] as Font;

            _buttons.Add(
                new Button(
                    Game.Instance.CurrentLobby.Name, 
                    font, 
                    60, 
                    new Vector2f(Game.Instance.Width / 2, _lobbyPanel.Position.Y - _lobbyPanel.Origin.Y - 25)));
            _buttons.Last().IsAnimated = false;

            _buttons.Add(
                new Button(
                    "Wróć", 
                    font, 
                    50, 
                    new Vector2f(
                        Game.Instance.Width / 2 - _lobbyPanel.Width / 2 - 70, 
                        _lobbyPanel.Position.Y + _lobbyPanel.Origin.Y), 
                    InitialScale));
            _buttons.Last().OnClick += BackOnClick;
            _buttons.Last().Position = new Vector2f(
                _buttons.Last().Position.X, 
                _buttons.Last().Position.Y + _buttons.Last().GetLocalBounds().Height + 15);

            _buttons.Add(
                new Button(
                    "Gotowy", 
                    font, 
                    50, 
                    new Vector2f(
                        Game.Instance.Width / 2 + _lobbyPanel.Width / 2 + 40, 
                        _lobbyPanel.Position.Y + _lobbyPanel.Origin.Y), 
                    InitialScale));
            _buttons.Last().OnClick += ReadyOnClick;
            _buttons.Last().Position = new Vector2f(
                _buttons.Last().Position.X, 
                _buttons.Last().Position.Y + _buttons.Last().GetLocalBounds().Height + 15);
        }

        private void ReadyOnClick(object sender, EventArgs e)
        {
            (sender as Button).OnClick -= ReadyOnClick;
            GameClient.Instance.MarkAsReady();
            Game.Instance.Player.IsReady = true;

            _lobbyPanel.UpdatePlayerList();
        }

        private void BackOnClick(object sender, EventArgs e)
        {
            GameClient.Instance.LeaveLobby();
            Game.Instance.Lobbies.Clear();
            Game.Instance.Player.IsReady = false;
            Game.Instance.Player.LobbyId = -1;

            Game.Instance.StateMachine.PushState(new GameStateMultiplayer());
        }

        private void Window_MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            _buttons.ForEach(t => t.HandleInput(e));
        }

        public override void Draw()
        {
            Game.Instance.Window.Draw(_background);
            Game.Instance.Window.Draw(_lobbyPanel);
            _buttons.ForEach(t => Game.Instance.Window.Draw(t));
        }

        public override void Update(float delta)
        {
            _lobbyPanel.Update(delta);
            _buttons.ForEach(t => t.Update(delta));
        }

        public override void HandleInput()
        {
        }

        public override void Dispose()
        {
            _background.Dispose();
            _lobbyPanel.Dispose();
            _buttons.ForEach(t => t.Dispose());
            Game.Instance.Window.MouseButtonReleased -= Window_MouseButtonReleased;
        }
    }
}