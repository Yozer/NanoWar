namespace NanoWar.States.GameStateMultiplayer
{
    using System.Collections.Generic;

    using NanoWar.GameClient;
    using NanoWar.Shapes;

    using SFML.Graphics;
    using SFML.System;
    using SFML.Window;

    internal class ServerSelector : Drawable
    {
        private Sprite _arrow;

        private LoadingAnimation _loadingAnimation = new LoadingAnimation();

        private List<Button> _lobbies = new List<Button>();

        private bool _right = true;

        private RoundedRectangleShape _shape = new RoundedRectangleShape(20f, 200);

        private bool _showLoadingAnimation;

        public float Velocity = 0.0005f;

        public ServerSelector()
        {
            _shape.FillColor = Color.Transparent;
            _shape.OutlineThickness = 3.0f;
            _shape.OutlineColor = Color.White;
            _shape.Size = new Vector2f(350, 300);
            _shape.Origin = new Vector2f(_shape.Size.X / 2, _shape.Size.Y / 2);
            _shape.Position = new Vector2f(Game.Instance.Width / 2, Game.Instance.Height / 2 + _shape.Size.Y);

            SelectedLobbyId = -1;

            _arrow = new Sprite(ResourceManager.Instance["multiplayer/arrow"] as Texture);
            _arrow.Origin = new Vector2f(0, _arrow.GetLocalBounds().Height / 2);
            _arrow.Position = new Vector2f(_shape.Position.X - _shape.GetLocalBounds().Width / 2 + 10, 0);
        }

        public int SelectedLobbyId { get; set; }

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

        public virtual void Draw(RenderTarget renderTarget, RenderStates renderStates)
        {
            _shape.Draw(renderTarget, renderStates);

            if (_showLoadingAnimation)
            {
                _loadingAnimation.Draw(renderTarget, renderStates);
            }

            _lobbies.ForEach(renderTarget.Draw);
            if (SelectedLobbyId != -1)
            {
                renderTarget.Draw(_arrow);
            }
        }

        public void UpdateServers()
        {
            _showLoadingAnimation = true;
            _lobbies.ForEach(t => t.Dispose());
            _lobbies.Clear();
            SelectedLobbyId = -1;

            Game.Instance.Lobbies = GameClient.Instance.UpdateLobbyList();

            var startY = _shape.Position.Y - _shape.GetLocalBounds().Height / 2 + 50;
            var startX = _shape.Position.X - _shape.GetLocalBounds().Width / 2 + 40;

            foreach (var lobby in Game.Instance.Lobbies.Values)
            {
                var button =
                    new Button(
                        lobby.Name + " - " + lobby.CurrentNumberOfPlayers + "/" + lobby.MaxiumNumberOfPlayers, 
                        ResourceManager.Instance["fonts/bebas_neue"] as Font, 
                        30, 
                        new Vector2f(startX, startY));
                button.Origin = new Vector2f(0, button.Origin.Y);
                button.IsAnimated = false;
                button.Data = lobby;

                button.OnClick += (sender, args) =>
                    {
                        SelectedLobbyId = ((sender as Button).Data as Lobby).Id;
                        _arrow.Position = new Vector2f(
                            _shape.Position.X - _shape.GetLocalBounds().Width / 2 + 10, 
                            (sender as Button).Position.Y - (sender as Button).GetLocalBounds().Height / 2);
                    };

                startY += button.GetLocalBounds().Top + button.GetLocalBounds().Height + 5;
                _lobbies.Add(button);
            }

            _showLoadingAnimation = false;
        }

        public void Update(float delta)
        {
            if (_showLoadingAnimation)
            {
                _loadingAnimation.Update(delta);
            }

            if (SelectedLobbyId != -1)
            {
                if (_arrow.Position.X >= _shape.Position.X - _shape.GetLocalBounds().Width / 2 + 20)
                {
                    _right = false;
                }
                else if (_arrow.Position.X <= _shape.Position.X - _shape.GetLocalBounds().Width / 2 + 10)
                {
                    _right = true;
                    Velocity = 0.0005f;
                }

                _arrow.Position = new Vector2f(
                    _arrow.Position.X + delta * Velocity * 45 * (_right ? 1 : -1), 
                    _arrow.Position.Y);
            }
        }

        public void HandleInput(MouseButtonEventArgs eventArgs)
        {
            _lobbies.ForEach(t => t.HandleInput(eventArgs));
        }

        public void Dispose()
        {
            _shape.Dispose();
            _loadingAnimation.Dipose();
            _lobbies.ForEach(t => t.Dispose());
            _arrow.Dispose();
        }
    }
}