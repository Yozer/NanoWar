namespace NanoWar.States.GameStateStart
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using NanoWar.GameClient;
    using NanoWar.States.GameStateFinish;
    using NanoWar.States.GameStateMultiplayer;

    using SFML.Graphics;

    internal class MultiplayerGame : GameMode
    {
        public enum EventGame
        {
            PlayerWin, 

            CellSent, 

            PlayerLeftGame, 

            Sync
        }

        private static TimeSpan _serverDiffrence;

        public MultiplayerGame(List<Cell> allCells)
            : base(allCells)
        {
        }

        public static DateTime ServerTime
        {
            get
            {
                return DateTime.Now - _serverDiffrence;
            }
        }

        public override void PrepareGame()
        {
            var colors = new List<Color> { Color.Green, Color.Blue };
            colors.Add(Color.Red);

            var i = 0;
            foreach (var player in Game.Instance.AllPlayers.Values.OrderBy(t => t.Id))
            {
                player.Color = colors[i % 3];
                i++;
            }

            if (Game.Instance.Player.Id == Game.Instance.CurrentLobby.Id)
            {
                // same id so we are owning the game
                GenerateCells();
                GameClient.Instance.SendGeneratedCells(AllCells);
            }
            else
            {
                var cells = GameClient.Instance.ReadGeneratedCells();
                foreach (var cell in cells)
                {
                    if (cell.Player != null)
                    {
                        Game.Instance.AllPlayers[cell.Player.Id].AddCell(cell);
                    }

                    AllCells.Add(cell);
                }
            }

            var serverTime = GameClient.Instance.GetServerTime();
            _serverDiffrence =
                TimeSpan.FromMilliseconds(
                    DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds - serverTime);
            var whenGameStarts = GameClient.Instance.GetTimeGameStarts();
            Thread.Sleep((int)(whenGameStarts - serverTime));
        }

        private void ProcessEvents()
        {
            int eventType;

            while ((eventType = GameClient.Instance.PeekEvent()) != -1)
            {
                var eve = (EventGame)eventType;
                if (eve == EventGame.CellSent)
                {
                    var cells = GameClient.Instance.ReadCellsInfo();

                    foreach (var cellInfo in cells)
                    {
                        var targetCell = AllCells.Single(t => t.Id == cellInfo.ToCellId);
                        var sourceCell = AllCells.Single(t => t.Id == cellInfo.FromCellId);

                        var movableCell = new UnitCell(
                            targetCell, 
                            sourceCell, 
                            cellInfo.Units, 
                            Game.Instance.AllPlayers[cellInfo.PlayerId]) {
                                                                            Id = cellInfo.Id 
                                                                         };

                        Game.Instance.AllPlayers[cellInfo.PlayerId].AddUnitCell(movableCell);
                    }
                }
                else if (eve == EventGame.PlayerWin)
                {
                    var playerId = GameClient.Instance.Read<int>();
                    string message;
                    if (playerId == Game.Instance.Player.Id)
                    {
                        message = "Gratulacje " + Game.Instance.Player.Name + ". Wygrałeś! ";
                    }
                    else
                    {
                        message = "Gracz " + Game.Instance.Players[playerId].Name + " cię zniszczył!";
                    }

                    Game.Instance.StateMachine.PushState(
                        new GameStateFinish(message, Game.Instance.Player.Id == playerId));
                    return;
                }
                else if (eve == EventGame.PlayerLeftGame)
                {
                    Game.Instance.StateMachine.PushState(new GameStateMultiplayer());
                }
                else if (eve == EventGame.Sync)
                {
                    var cellInfo = GameClient.Instance.Read<ShortCellInfo>();
                    var cell = AllCells.Single(t => t.Id == cellInfo.Id);
                    cell.Units = cellInfo.Units;

                    if (cell.PlayerId != cellInfo.OwnerId)
                    {
                        if (cell.Player != null)
                        {
                            cell.Player.Cells.RemoveAll(t => t.Id == cellInfo.Id);
                        }

                        cell.Player = cellInfo.OwnerId == -1 ? null : Game.Instance.AllPlayers[cellInfo.OwnerId];

                        if (cell.Player != null)
                        {
                            cell.Player.Cells.Add(cell);
                        }
                    }
                }
            }
        }

        public override void SendCells(List<UnitCell> movableCellList)
        {
            Task.Run(() => GameClient.Instance.SendCells(movableCellList));
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

                    // have to copy it
                    int unitId = playerInstance.UnitCells[i].Id;
                    Task.Run(() => GameClient.Instance.CellsReachedTarget(unitId));

                    playerInstance.UnitCells[i].Dispose();
                    playerInstance.UnitCells.RemoveAt(i);
                    i--;
                }
            }

            ProcessEvents();
        }
    }
}