namespace NanoWar.States.GameStateMultiplayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NanoWar.GameClient;
    using NanoWar.Shapes;
    using NanoWar.States.GameStateLobby;
    using NanoWar.States.GameStateMenu;

    using SFML.Graphics;
    using SFML.System;
    using SFML.Window;

    internal class GameStateMultiplayer : GameState
    {
        private const float InitialScale = 0.9f;

        private Sprite _background;

        private List<Button> _buttons = new List<Button>();

        private ServerSelector _serverSelector = new ServerSelector();

        public GameStateMultiplayer()
        {
            _background = new Sprite(ResourceManager.Instance["menu/background"] as Texture);

            Game.Instance.AudioManager.PauseAllBackground();
            Game.Instance.AudioManager.PlaySound("menu/background_music", true);

            PrepareUi();

            Game.Instance.Window.MouseButtonReleased += Window_MouseButtonReleased;

            try
            {
                var id = GameClient.Instance.InitConnection();
                Game.Instance.Player.Id = id;
            }
            catch
            {
            }

            if (GameClient.Connected)
            {
                RefeshGameClick(null, null);
            }
        }

        private void PrepareUi()
        {
            var font = ResourceManager.Instance["fonts/bebas_neue"] as Font;

            _buttons.Add(
                new Button(
                    "Lista gier", 
                    font, 
                    60, 
                    new Vector2f(Game.Instance.Width / 2, _serverSelector.Position.Y - _serverSelector.Origin.Y - 25)));
            _buttons.Last().IsAnimated = false;

            _buttons.Add(
                new Button(
                    "Stwórz nową grę", 
                    font, 
                    50, 
                    new Vector2f(
                        Game.Instance.Width / 2 - _serverSelector.Width / 2 - 70, 
                        _serverSelector.Position.Y + _serverSelector.Origin.Y), 
                    InitialScale));

            _buttons.Last().OnClick += CreateNewGameClick;
            _buttons.Last().Position = new Vector2f(
                _buttons.Last().Position.X, 
                _buttons.Last().Position.Y + _buttons.Last().GetLocalBounds().Height + 15);

            _buttons.Add(
                new Button(
                    "Odśwież listę", 
                    font, 
                    50, 
                    new Vector2f(
                        Game.Instance.Width / 2 + _serverSelector.Width / 2 + 40, 
                        _serverSelector.Position.Y + _serverSelector.Origin.Y), 
                    InitialScale));
            _buttons.Last().OnClick += RefeshGameClick;
            _buttons.Last().Position = new Vector2f(
                _buttons.Last().Position.X, 
                _buttons.Last().Position.Y + _buttons.Last().GetLocalBounds().Height + 15);

            _buttons.Add(
                new Button(
                    "Dołącz", 
                    font, 
                    70, 
                    new Vector2f(Game.Instance.Width / 2, _serverSelector.Position.Y + _serverSelector.Origin.Y), 
                    InitialScale));
            _buttons.Last().OnClick += JoinToLobbyClick;
            _buttons.Last().Position = new Vector2f(
                _buttons.Last().Position.X, 
                _buttons.Last().Position.Y + _buttons.Last().GetLocalBounds().Height + 15);
        }

        private void JoinToLobbyClick(object sender, EventArgs e)
        {
            if (_serverSelector.SelectedLobbyId == -1)
            {
                return;
            }

            Game.Instance.CurrentLobby = Game.Instance.Lobbies[_serverSelector.SelectedLobbyId];
            GameClient.Instance.JoinLobby(_serverSelector.SelectedLobbyId);
            Game.Instance.StateMachine.PushState(new GameStateLobby());
        }

        private void RefeshGameClick(object sender, EventArgs e)
        {
            _serverSelector.UpdateServers();
        }

        private void CreateNewGameClick(object sender, EventArgs e)
        {
            var lobby = GameClient.Instance.CreateLobby();
            Game.Instance.Lobbies.Add(lobby.Id, lobby);
            Game.Instance.CurrentLobby = lobby;
            Game.Instance.StateMachine.PushState(new GameStateLobby());
        }

        private void Window_MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            _buttons.ForEach(t => t.HandleInput(e));
            _serverSelector.HandleInput(e);
        }

        public override void Draw()
        {
            Game.Instance.Window.Draw(_background);
            _buttons.ForEach(t => Game.Instance.Window.Draw(t));
            Game.Instance.Window.Draw(_serverSelector);
        }

        public override void Update(float delta)
        {
            ProcessEvents();

            _buttons.ForEach(t => t.Update(delta));

            _serverSelector.Update(delta);
        }

        private void ProcessEvents()
        {
            if (!GameClient.Connected)
            {
                Game.Instance.StateMachine.PushState(new GameStateMenu());
                return;
            }

            int eventType;
            while ((eventType = GameClient.Instance.PeekEvent()) != -1)
            {
                var eve = (EventMultiplayer)eventType;
                if (eve == EventMultiplayer.UpdateLobbyList)
                {
                    _serverSelector.UpdateServers();
                }
            }
        }

        public override void HandleInput()
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
            {
                GameClient.Dispose();
                Game.Instance.StateMachine.PushState(new GameStateMenu());
            }
        }

        public override void Dispose()
        {
            _serverSelector.Dispose();
            _background.Dispose();
            _buttons.ForEach(t => t.Dispose());
            Game.Instance.Window.MouseButtonReleased -= Window_MouseButtonReleased;
        }
    }

    internal enum EventMultiplayer
    {
        UpdateLobbyList
    }
}