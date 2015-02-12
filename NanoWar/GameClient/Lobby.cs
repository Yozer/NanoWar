namespace NanoWar.GameClient
{
    public class Lobby
    {
        public Lobby(int id, string name, int maxiumNumberOfPlayers, int currentNumberOfPlayers)
        {
            CurrentNumberOfPlayers = currentNumberOfPlayers;
            MaxiumNumberOfPlayers = maxiumNumberOfPlayers;
            Name = name;
            Id = id;
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public int MaxiumNumberOfPlayers { get; private set; }

        public int CurrentNumberOfPlayers { get; private set; }
    }
}