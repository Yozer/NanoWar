namespace NanoWar.States.GameStateStart
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using NanoWar.AI;
    using NanoWar.States.GameStateFinish;

    using SFML.Graphics;

    internal class SingleplayerGame : GameMode
    {
        private Ai _ai;

        private PlayerInstance _aiPlayerInstance;

        private Stopwatch _gameTimeCounter = new Stopwatch();

        public SingleplayerGame(List<Cell> allCells)
            : base(allCells)
        {
            
            _aiPlayerInstance = new PlayerInstance("AI") { Id = -1 };
            Game.Instance.Players.Add(-1, _aiPlayerInstance);

            if (Ai.AiName == "Normal")
                _ai = new Normal(_aiPlayerInstance, allCells);
            else if(Ai.AiName == "Noob")
                _ai = new Noob(_aiPlayerInstance, allCells);

            _gameTimeCounter.Start();
        }

        private bool PlayerWon(PlayerInstance player)
        {
            return AllCells.Where(t => t.Player != null).All(t => t.Player.Id == player.Id)
                   && Game.Instance.AllPlayers.Values.Except(new List<PlayerInstance> { player })
                          .All(t => t.UnitCells.Count == 0);
        }

        public override void Update(float delta)
        {
            foreach (var playerInstance in Game.Instance.AllPlayers.Values)
            {
                for (var i = 0; i < playerInstance.UnitCells.Count; i++)
                {
                    // maybe linked list?
                    if (playerInstance.UnitCells[i].IsBusy)
                    {
                        continue;
                    }

                    playerInstance.UnitCells[i].Dispose();
                    playerInstance.UnitCells.RemoveAt(i);
                    i--;
                }
            }

            foreach (var playerInstance in Game.Instance.AllPlayers.Values)
            {
                if (PlayerWon(playerInstance))
                {
                    string message;
                    if (playerInstance.Id == Game.Instance.Player.Id)
                    {
                        message = "Gratulacje " + playerInstance.Name + ". Wygra³eœ! ";
                        UpdateBestResults();
                    }
                    else
                    {
                        message = "Uppsss komputer jest chyba sprytniejszy. " + playerInstance.Name + " ciê zniszczy³.";
                    }

                    Game.Instance.StateMachine.PushState(
                        new GameStateFinish(message, Equals(Game.Instance.Player, playerInstance)));
                    return;
                }
            }

            _ai.Decision(delta);
        }

        private void UpdateBestResults()
        {
            var currentGameTime = (int)_gameTimeCounter.ElapsedMilliseconds;
            var results = new List<dynamic>();
            if (File.Exists(Game.BestResultsFileName))
            {
                foreach (var line in File.ReadAllLines(Game.BestResultsFileName, Encoding.UTF8))
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    var splited = line.Split(':');
                    results.Add(new { Nick = splited[0], Time = int.Parse(splited[1]) });
                }
            }

            results = results.OrderBy(t => t.Time).ToList();
            if (results.Count == 8 && results.Last().Time <= currentGameTime)
            {
                return;
            }

            if (results.Count == 8)
            {
                results.RemoveAt(results.Count - 1);
            }

            results.Add(new { Nick = Game.Instance.Player.Name, Time = currentGameTime });
            results = results.OrderBy(t => t.Time).ToList();

            File.WriteAllLines(
                Game.BestResultsFileName, 
                results.Select(t => t.Nick.ToString() + ":" + t.Time.ToString()).Cast<string>(), 
                Encoding.UTF8);
        }

        public override void PrepareGame()
        {
            Game.Instance.Player.Color = Color.Green;
            _aiPlayerInstance.Color = Color.Red;
            GenerateCells();
        }

        public override void SendCells(List<UnitCell> movableCellList)
        {
            foreach (var unitCell in movableCellList)
            {
                unitCell.SourcePlayer.AddUnitCell(unitCell);
            }
        }
    }
}