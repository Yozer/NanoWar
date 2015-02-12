using NanoWar.States.GameStateIntro;

namespace NanoWar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;

    using IniParser;

    using NanoWar.AI;
    using NanoWar.GameClient;
    using NanoWar.States.GameStateMenu;
    using NanoWar.States.GameStateStart;

    using SFML.Graphics;
    using SFML.Window;

    internal class Game
    {
        private const string IniFileName = "settings.ini";

        public const string BestResultsFileName = "best_results";

        private static Game _instance;

        private Dictionary<int, PlayerInstance> _allPlayers;

        public Dictionary<int, Lobby> Lobbies = new Dictionary<int, Lobby>();

        public Game(uint width = 1365, uint height = 768)
        {
            if (!File.Exists(IniFileName))
            {
                MessageBox.Show(string.Format("Brak pliku {0}!", IniFileName));
                Environment.Exit(1);
            }

            ReadIniFile(IniFileName);

            Players = new Dictionary<int, PlayerInstance>();

            Width = width;
            Height = height;

            var settings = new ContextSettings { AntialiasingLevel = 8 };
            Window = new RenderWindow(new VideoMode(Width, Height), "Nano War", Styles.Close, settings);

            Window.Closed += (s, a) =>
                {
                    GameClient.GameClient.Dispose();
                    Window.Close();
                };
            Window.SetVerticalSyncEnabled(true);
            Window.SetKeyRepeatEnabled(false);

            StateMachine = new StateMachine();
            AudioManager = new AudioManager();
        }

        private GameState CurrentState
        {
            get
            {
                return StateMachine.PeekState();
            }
        }

        public uint Width { get; private set; }

        public uint Height { get; private set; }

        public RenderWindow Window { get; private set; }

        public static Game Instance
        {
            get
            {
                return _instance ?? (_instance = new Game());
            }
        }

        public StateMachine StateMachine { get; private set; }

        public AudioManager AudioManager { get; private set; }

        public PlayerInstance Player { get; set; }

        public Dictionary<int, PlayerInstance> Players { get; set; }

        public Dictionary<int, PlayerInstance> AllPlayers
        {
            get
            {
                if (_allPlayers == null || (Players != null && _allPlayers.Count != Players.Count + 1))
                {
                    _allPlayers = new Dictionary<int, PlayerInstance> { { Player.Id, Player } };
                    if (Players != null && Players.Count != 0)
                    {
                        foreach (var entry in Players)
                        {
                            _allPlayers.Add(entry.Key, entry.Value);
                        }
                    }
                }

                return _allPlayers;
            }

            set
            {
                _allPlayers = value;
            }
        }

        public Lobby CurrentLobby { get; set; }

        private void ReadIniFile(string file)
        {
            var parser = new FileIniDataParser();
            var data = parser.ReadFile(file);

            Player = new PlayerInstance(data["Player"]["Nick"]);
            Ai.DecisionTime = TimeSpan.FromMilliseconds(Convert.ToDouble(data["AI"]["DecisionTime"]));
            Ai.AiName = data["AI"]["Type"];
            GameClient.GameClient.Instance.Ip = data["Server"]["IP"];
            GameClient.GameClient.Instance.Port = Convert.ToInt32(data["Server"]["Port"]);
        }

        public void Start()
        {
            StateMachine.PushState(new GameStateIntro());
            var clock = new Stopwatch();

            while (Window.IsOpen)
            {
                Window.DispatchEvents();

                CurrentState.HandleInput();
                CurrentState.Update((float)clock.Elapsed.TotalMilliseconds);
                clock.Restart();

                Window.Clear();
                CurrentState.Draw();
                Window.Display();

                while (StateMachine.Count != 0 && StateMachine.Count != 1)
                {
                    StateMachine.PopState();
                }
            }
        }
    }
}