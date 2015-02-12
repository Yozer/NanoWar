namespace NanoWar.States.GameStateLobby
{
    using System;
    using System.Collections.Generic;

    using NanoWar.GameClient;
    using NanoWar.Shapes;
    using NanoWar.States.GameStateMultiplayer;
    using NanoWar.States.GameStateStart;

    using SFML.Graphics;
    using SFML.System;

    internal class LobbyPanel : Drawable
    {
        private List<Text> _players = new List<Text>();

        private List<Sprite> _readyMarks = new List<Sprite>();

        private RoundedRectangleShape _shape = new RoundedRectangleShape(20f, 200);

        public LobbyPanel()
        {
            _shape.FillColor = Color.Transparent;
            _shape.OutlineThickness = 3.0f;
            _shape.OutlineColor = Color.White;
            _shape.Size = new Vector2f(350, 300);
            _shape.Origin = new Vector2f(_shape.Size.X / 2, _shape.Size.Y / 2);
            _shape.Position = new Vector2f(Game.Instance.Width / 2, Game.Instance.Height / 2 + _shape.Size.Y);

            Game.Instance.Players = GameClient.Instance.UpdatePlayersInLobby();
            Game.Instance.Player.IsReady = false;
            Game.Instance.AllPlayers = null;
            UpdatePlayerList();
        }

        public Vector2f Position
        {
            get
            {
                return _shape.Position;
            }

            set
            {
                _shape.Position = value;
            }
        }

        public Vector2f Origin
        {
            get
            {
                return _shape.Origin;
            }

            set
            {
                _shape.Origin = value;
            }
        }

        public float Width
        {
            get
            {
                return _shape.GetLocalBounds().Width;
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_shape);
            _players.ForEach(target.Draw);
            _readyMarks.ForEach(target.Draw);
        }

        private void ProcessEvents()
        {
            int eventType;
            while ((eventType = GameClient.Instance.PeekEvent()) != -1)
            {
                var eve = (EventLobby)eventType;
                if (eve == EventLobby.LobbyChanged)
                {
                    Game.Instance.Players = GameClient.Instance.UpdatePlayersInLobby();
                    Game.Instance.AllPlayers = null;

                    UpdatePlayerList();
                }
                else if (eve == EventLobby.LobbyDeleted)
                {
                    Game.Instance.StateMachine.PushState(new GameStateMultiplayer());
                }
                else if (eve == EventLobby.GameStarted)
                {
                    Game.Instance.StateMachine.PushState(new GameStateStart());
                }
            }
        }

        public void UpdatePlayerList()
        {
            var startY = _shape.Position.Y - _shape.GetLocalBounds().Height / 2 + 50;
            var startX = _shape.Position.X - _shape.GetLocalBounds().Width / 2 + 40;

            _players.ForEach(t => t.Dispose());
            _players.Clear();

            _readyMarks.ForEach(t => t.Dispose());
            _readyMarks.Clear();

            float maxLengthPlayer = 0;

            foreach (var player in Game.Instance.AllPlayers.Values)
            {
                var text = new Text(player.Name, ResourceManager.Instance["fonts/bebas_neue"] as Font, 30)
                               {
                                   Position
                                       =
                                       new Vector2f
                                       (
                                       startX, 
                                       startY)
                               };

                if (player.Name == Game.Instance.Player.Name)
                {
                    text.Style = Text.Styles.Bold;
                }

                if (player.IsReady)
                {
                    var spriteMark = new Sprite(ResourceManager.Instance["multiplayer/ok"] as Texture)
                                         {
                                             Position =
                                                 new Vector2f
                                                 (
                                                 startX, 
                                                 startY
                                                 + text
                                                       .GetLocalBounds
                                                       ()
                                                       .Height
                                                 / 2)
                                         };
                    _readyMarks.Add(spriteMark);
                }

                maxLengthPlayer = Math.Max(maxLengthPlayer, text.GetLocalBounds().Width + text.GetLocalBounds().Left);
                startY += text.GetLocalBounds().Top + text.GetLocalBounds().Height + 35;
                _players.Add(text);
            }

            _readyMarks.ForEach(t => t.Position = new Vector2f(t.Position.X + maxLengthPlayer + 35, t.Position.Y));
        }

        public void Update(float delta)
        {
            ProcessEvents();
        }

        public void Dispose()
        {
            _shape.Dispose();
            _players.ForEach(t => t.Dispose());
            _readyMarks.ForEach(t => t.Dispose());
        }
    }

    internal enum EventLobby
    {
        LobbyChanged, 

        LobbyDeleted, 

        GameStarted
    }
}