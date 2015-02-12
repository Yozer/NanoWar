namespace NanoWar.AI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NanoWar.States.GameStateStart;

    internal abstract class Ai
    {
        protected static Random Rand = new Random();

        private PlayerInstance _aiPlayerInstance;

        private List<Cell> _allCells;

        protected Ai(PlayerInstance aiPlayerInstance, List<Cell> allCells)
        {
            _aiPlayerInstance = aiPlayerInstance;
            _allCells = allCells;
        }

        protected IEnumerable<Cell> OpponentCells
        {
            get
            {
                return Game.Instance.Player.Cells.Concat(_allCells.Where(t => t.Player == null));
            }
        }

        protected IEnumerable<Cell> NeutralCells
        {
            get
            {
                return _allCells.Where(t => t.Player == null);
            }
        }

        protected IEnumerable<Cell> MyCells
        {
            get
            {
                return _aiPlayerInstance.Cells;
            }
        }

        public static TimeSpan DecisionTime { get; set; }
        public static string AiName { get; set; }

        public abstract void Decision(float delta);

        protected IEnumerable<Cell> GetStrongestCells()
        {
            return _aiPlayerInstance.Cells.OrderByDescending(t => t.Units);
        }

        protected int MaxHelpValue(Cell helper)
        {
            return helper.Units
                   - Game.Instance.Player.UnitCells.Where(t => Equals(t.TargetCell, helper)).Sum(t => t.Units);
        }

        protected IEnumerable<Cell> GetPlayerCells()
        {
            return _allCells.Where(t => t.Player == null || !Equals(t.Player, _aiPlayerInstance)).OrderBy(t => t.Units);
        }

        protected IEnumerable<Cell> GetCellsNearestTo(Cell sourceCell)
        {
            return GetCellsNearestTo(sourceCell, _allCells.Except(_aiPlayerInstance.Cells));
        }

        protected IEnumerable<Cell> GetCellsNearestTo(Cell sourceCell, IEnumerable<Cell> cells)
        {
            return cells.OrderBy(t => t.GetDistanceBetweenCells(sourceCell));
        }

        protected int GetUnitsDiffrence(Cell cell)
        {
            return _aiPlayerInstance.UnitCells.Where(t => Equals(t.TargetCell, cell)).Sum(t => t.UnitsLeft)
                   - Game.Instance.Player.UnitCells.Where(t => Equals(t.TargetCell, cell)).Sum(t => t.UnitsLeft)
                   + (cell.Units * (Equals(cell.Player, _aiPlayerInstance) ? 1 : -1));
        }

        protected bool CanTakeCell(Cell attackCell, Cell targetCell)
        {
            return attackCell.Units - 1 > targetCell.Units;
        }

        protected IEnumerable<Cell> GetCellsNearestTo(Cell sourceCell, IEnumerable<Cell> cells, float distance)
        {
            return
                cells.Select(t => new { cell = t, distance = t.GetDistanceBetweenCells(sourceCell) })
                    .Where(t => t.distance <= distance)
                    .OrderBy(t => t.distance)
                    .Select(t => t.cell);
        }

        protected void Attack(Cell fromCell, Cell toCell, int units)
        {
            if (fromCell.Units - units < 1 || units <= 0)
            {
                return;
            }

            var unitCell = new UnitCell(toCell, fromCell, units, _aiPlayerInstance);
            fromCell.Player.AddUnitCell(unitCell);
        }

        protected void Attack(Cell fromCell, Cell toCell)
        {
            var unitCell = new UnitCell(toCell, fromCell, fromCell.Units - 1, _aiPlayerInstance);
            fromCell.Player.AddUnitCell(unitCell);
        }
    }
}