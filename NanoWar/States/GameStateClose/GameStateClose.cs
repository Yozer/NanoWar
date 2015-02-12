namespace NanoWar.States.GameStateClose
{
    internal class GameStateClose : GameState
    {
        public override void Draw()
        {
        }

        public override void Update(float delta)
        {
            Game.Instance.Window.Close();
        }

        public override void HandleInput()
        {
        }

        public override void Dispose()
        {
        }
    }
}