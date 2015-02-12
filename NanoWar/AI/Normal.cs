namespace NanoWar.AI
{
    using System;
    using System.Collections.Generic;

    using NanoWar.States.GameStateStart;

    internal class Normal : Ai
    {
        private TimeSpan _decisionTime = TimeSpan.FromMilliseconds(1000);

        private TimeSpan _lastDecision = TimeSpan.Zero;

        public Normal(PlayerInstance aiPlayerInstance, List<Cell> allCells)
            : base(aiPlayerInstance, allCells)
        {
        }

        public override void Decision(float delta)
        {
            _lastDecision = _lastDecision.Add(TimeSpan.FromMilliseconds(delta));
            if (_lastDecision < _decisionTime)
            {
                return;
            }

            _lastDecision = TimeSpan.Zero;
            var myCellsOrdered = GetStrongestCells();

            foreach (var cell in myCellsOrdered)
            {
                // detect enemy attack
                IEnumerable<Cell> nearestCells;

                var maxHelpValue = MaxHelpValue(cell);
                if (maxHelpValue > 1)
                {
                    nearestCells = GetCellsNearestTo(cell, MyCells);
                    foreach (var nearestCell in nearestCells)
                    {
                        var unitsDiffrence = GetUnitsDiffrence(nearestCell);
                        if (unitsDiffrence <= 0 && (unitsDiffrence * -1 + 1) <= maxHelpValue)
                        {
                            Attack(
                                cell, 
                                nearestCell, 
                                Math.Min(unitsDiffrence * (-1) + 1 + Rand.Next(5, 8), cell.Units - 1));
                        }
                    }
                }

                // try to take nearest neutral cells
                nearestCells = GetCellsNearestTo(cell, NeutralCells, 200);
                foreach (var nearestCell in nearestCells)
                {
                    if (CanTakeCell(cell, nearestCell))
                    {
                        var unitsDiffrence = GetUnitsDiffrence(nearestCell);
                        if (unitsDiffrence <= 0)
                        {
                            var units = Math.Min(unitsDiffrence * (-1) + 1, cell.Units - 1);
                            if (units < 5 && nearestCell.Units > units)
                            {
                                continue;
                            }

                            Attack(cell, nearestCell, units);
                        }
                    }
                }

                // try to take nearest enemy cells
                nearestCells = GetCellsNearestTo(cell);
                foreach (var nearestCell in nearestCells)
                {
                    var unitsDiffrence = GetUnitsDiffrence(nearestCell);
                    if (unitsDiffrence <= 0)
                    {
                        if (unitsDiffrence * (-1) + 4 >= cell.Units)
                        {
                            continue;
                        }

                        var units = Rand.Next(unitsDiffrence * (-1) + 4, cell.Units);

                        // Math.Min(unitsDiffrence * (-1) + 1 + rand.Next(5, 8), cell.Units - 1);
                        Attack(cell, nearestCell, units);
                    }
                }

                nearestCells = GetCellsNearestTo(cell, NeutralCells, 500);
                foreach (var nearestCell in nearestCells)
                {
                    if (CanTakeCell(cell, nearestCell))
                    {
                        var unitsDiffrence = GetUnitsDiffrence(nearestCell);
                        if (unitsDiffrence <= 0)
                        {
                            var units = Math.Min(unitsDiffrence * (-1) + 1, cell.Units - 1);
                            if (units < 5 && nearestCell.Units > units)
                            {
                                continue;
                            }

                            Attack(cell, nearestCell, units);
                        }
                    }
                }
            }
        }
    }
}