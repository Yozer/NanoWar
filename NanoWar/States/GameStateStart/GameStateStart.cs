namespace NanoWar.States.GameStateStart
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NanoWar.GameClient;
    using NanoWar.Shapes;
    using NanoWar.States.GameStateMenu;

    using SFML.Graphics;
    using SFML.System;
    using SFML.Window;

    internal class GameStateStart : GameState
    {
        private List<Cell> _allCells = new List<Cell>();

        private Sprite _background;

        private GameMode _gameMode;

        private List<Cell> _lastSelection = new List<Cell>();

        private List<Line> _lines = new List<Line>();

        private Text _playerInfoText;

        private RectangleSelection _rectangleSelection = new RectangleSelection();

        private Selector _selector = new Selector();

        public GameStateStart()
        {
            Game.Instance.AudioManager.PauseAllBackground();
            Game.Instance.AudioManager.PlaySound("game/background_music", true);

            if (Game.Instance.AllPlayers.Count != 1)
            {
                _gameMode = new MultiplayerGame(_allCells);
            }
            else
            {
                _gameMode = new SingleplayerGame(_allCells);
            }

            _gameMode.PrepareGame();

            _background = new Sprite(ResourceManager.Instance["menu/background"] as Texture);

            _playerInfoText = new Text(
                Game.Instance.Player.Name, 
                ResourceManager.Instance["fonts/bebas_neue"] as Font, 
                25) {
                       Position = new Vector2f(5, 5), Color = Game.Instance.Player.Color 
                    };

            // register events
            Game.Instance.Window.MouseButtonPressed += MouseButtonPressed;
            Game.Instance.Window.MouseButtonReleased += MouseButtonReleased;
            Game.Instance.Window.KeyReleased += KeyReleased;
        }

        public override void Draw()
        {
            Game.Instance.Window.Draw(_background);
            _lines.ForEach(t => Game.Instance.Window.Draw(t));

            _allCells.ForEach(t => Game.Instance.Window.Draw(t));

            Game.Instance.AllPlayers.Values.ToList()
                .ForEach(t => t.UnitCells.ForEach(x => Game.Instance.Window.Draw(x)));

            if (_selector.Cell != null && _selector.Units != 0)
            {
                Game.Instance.Window.Draw(_selector);
            }

            if (_rectangleSelection.IsDrawable)
            {
                Game.Instance.Window.Draw(_rectangleSelection);
            }

            Game.Instance.Window.Draw(_playerInfoText);
        }

        public override void Update(float delta)
        {
            _gameMode.Update(delta);

            _allCells.ForEach(t => t.Update(delta));
            Game.Instance.AllPlayers.Values.ToList().ForEach(t => t.Update(delta));

            var count = _lastSelection.Count;
            _lastSelection.RemoveAll(t => !Equals(t.Player, Game.Instance.Player));

            // delete selected cells which was taken by enemy
            if (count != _lastSelection.Count)
            {
                UpdateLines();
            }

            if (_selector.Cell != null)
            {
                _selector.Update(delta);
                _selector.MaxUnits = _lastSelection.Except(new List<Cell> { _selector.Cell }).Sum(t => t.Units - 1);
            }

            _lines.ForEach(t => t.Update(delta));
        }

        // mouse move
        public override void HandleInput()
        {
            var mousePoint = new Vector2f(
                Mouse.GetPosition(Game.Instance.Window).X, 
                Mouse.GetPosition(Game.Instance.Window).Y);

            if (_rectangleSelection.IsDrawable)
            {
                _rectangleSelection.Size = mousePoint - _rectangleSelection.Position;
            }

            var hoverCell = GetCellCondition(t => t.ContainsPoint(mousePoint));
            if (hoverCell != null && !hoverCell.WasHover)
            {
                Game.Instance.AudioManager.PlaySound("game/cell_mouse_hover");
            }

            if (hoverCell != null)
            {
                _allCells.Except(new List<Cell> { hoverCell }).ToList().ForEach(t => t.WasHover = false);
                hoverCell.WasHover = true;
                _lines.ForEach(t => t.End = hoverCell.Position);
            }
            else
            {
                _allCells.ForEach(t => t.WasHover = false);
                _lines.ForEach(t => t.End = mousePoint);
            }
        }

        private void UpdateLines()
        {
            _lines.Where(t => _lastSelection.All(x => x.Position != t.Start)).ToList().ForEach(t => t.Dispose());
            _lines.RemoveAll(t => _lastSelection.All(x => x.Position != t.Start));
            foreach (var selectedCell in _lastSelection.Where(t => _lines.All(x => x.Start != t.Position)))
            {
                _lines.Add(new Line(selectedCell.Position));
            }
        }

        private void SendCells()
        {
            var existed = _lastSelection.Remove(_selector.Cell);
            if (_lastSelection.Count == 0)
            {
                return;
            }

            var unisPerCell = _selector.Units / _lastSelection.Count;
            var toSend = _selector.Units;
            var i = 1;
            var unitCellList = new List<UnitCell>();

            foreach (var cell in _lastSelection.OrderBy(t => t.Units).Where(t => Equals(t.Player, Game.Instance.Player))
                )
            {
                var units = (cell.Units - 1) < unisPerCell ? (cell.Units - 1) : unisPerCell;

                if (units > 0)
                {
                    var unitCell = new UnitCell(_selector.Cell, cell, units, Game.Instance.Player);
                    toSend -= unitCell.Units;

                    unitCellList.Add(unitCell);
                }

                if (i == _lastSelection.Count)
                {
                    unisPerCell = toSend;
                }
                else
                {
                    unisPerCell = toSend / (_lastSelection.Count - i);
                }

                i++;
            }

            _gameMode.SendCells(unitCellList);
            if (existed)
            {
                _lastSelection.Add(_selector.Cell);
                UpdateLines();
            }
        }

        private Cell GetCellCondition(Func<Cell, bool> rule)
        {
            return _allCells.SingleOrDefault(rule);
        }

        private void DisposeLines()
        {
            _lines.ForEach(t => t.Dispose());
            _lines.Clear();
        }

        public override void Dispose()
        {
            Game.Instance.AllPlayers.Values.ToList().ForEach(t => t.UnitCells.ForEach(x => x.Dispose()));
            Game.Instance.Players.Clear();

            Game.Instance.Player.Cells.Clear();
            Game.Instance.Player.UnitCells.Clear();

            _allCells.ForEach(t => t.Dispose());
            _allCells.Clear();

            DisposeLines();

            _background.Dispose();

            _selector.Dispose();
            _rectangleSelection.Dispose();

            _playerInfoText.Dispose();

            Game.Instance.Window.MouseButtonPressed -= MouseButtonPressed;
            Game.Instance.Window.MouseButtonReleased -= MouseButtonReleased;
            Game.Instance.Window.KeyReleased -= KeyReleased;
            GameClient.Dispose();
        }

        private void MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (e.Button != Mouse.Button.Left)
            {
                return;
            }

            var mousePoint = new Vector2f(e.X, e.Y);
            var cell = GetCellCondition(t => t.ContainsPoint(mousePoint));

            if (cell != null && _lines.Count != 0)
            {
                if (Keyboard.IsKeyPressed(Keyboard.Key.LControl))
                {
                    // add cell to selection
                    if (!_lastSelection.Contains(cell))
                    {
                        _lastSelection.Add(cell);
                    }

                    UpdateLines();
                }

                // let's initiate selecting number of units
                else
                {
                    _selector.Cell = cell;
                }
            }
            else
            {
                _lastSelection.Clear();
                UpdateLines();
            }

            if (_selector.Cell == null)
            {
                // show selection rect only when we aren't selecting units
                _rectangleSelection.Position = mousePoint;
                _rectangleSelection.IsDrawable = true;
            }
        }

        private void MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.Button != Mouse.Button.Left)
            {
                return;
            }

            // we released mouse so end selecting units
            if (_selector.Cell != null)
            {
                // mouse released at target cell
                if (_selector.TargetCellContainsPoint(new Vector2i(e.X, e.Y)))
                {
                    SendCells();
                }

                _selector.Cell = null;
            }
            else if (!Keyboard.IsKeyPressed(Keyboard.Key.LControl))
            {
                // we wasn't selecting additional cell using ctrl
                _lastSelection = _rectangleSelection.GetIntersectCells(Game.Instance.Player.Cells);
                UpdateLines();
            }

            _rectangleSelection.IsDrawable = false;
        }

        private void KeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.A)
            {
                // mark all
                _rectangleSelection.IsDrawable = false;
                _lastSelection = new List<Cell>();
                foreach (var cell in Game.Instance.Player.Cells)
                {
                    _lastSelection.Add(cell);
                }

                UpdateLines();
            }
            else if (e.Code == Keyboard.Key.Escape)
            {
                Game.Instance.StateMachine.PushState(new GameStateMenu());
            }
        }
    }
}