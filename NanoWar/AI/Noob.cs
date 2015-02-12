namespace NanoWar.AI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NanoWar.States.GameStateStart;

    internal class Noob : Ai
    {
        private TimeSpan _lastDecision = TimeSpan.Zero;

        public Noob(PlayerInstance aiPlayerInstance, List<Cell> allCells)
            : base(aiPlayerInstance, allCells)
        {
        }

        public override void Decision(float delta)
        {
            _lastDecision = _lastDecision.Add(TimeSpan.FromMilliseconds(delta));
            if (_lastDecision < DecisionTime)
            {
                return;
            }

            _lastDecision = TimeSpan.Zero;
            var strongestCells = GetStrongestCells();
            if (!strongestCells.Any())
            {
                return;
            }

            var strongestCell = strongestCells.First();

            if (strongestCell.Units >= 3)
            {
                var cells = GetCellsNearestTo(strongestCell, OpponentCells).ToList();
                if (cells.Count > 0)
                {
                    var targetCell = cells.First();
                    Attack(strongestCell, targetCell);
                }
            }
        }
    }
}