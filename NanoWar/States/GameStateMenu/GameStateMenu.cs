namespace NanoWar.States.GameStateMenu
{
    internal class GameStateMenu : GameState
    {
        private MenuManager _menuManager;

        public GameStateMenu()
        {
            _menuManager = new MenuManager("menu/main_menu");

            Game.Instance.AudioManager.PauseAllBackground();
            Game.Instance.AudioManager.PlaySound("menu/background_music", true);
        }

        public override void Draw()
        {
            Game.Instance.Window.Draw(_menuManager);
        }

        public override void Update(float delta)
        {
            _menuManager.Update(delta);
        }

        public override void HandleInput()
        {
        }

        public override void Dispose()
        {
            _menuManager.Dispose();
        }
    }
}