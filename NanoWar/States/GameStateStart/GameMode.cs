namespace NanoWar.States.GameStateStart
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NanoWar.HelperClasses;

    using SFML.System;

    internal abstract class GameMode
    {
        protected List<Cell> AllCells;

        protected GameMode(List<Cell> allCells)
        {
            AllCells = allCells;
        }

        public abstract void Update(float delta);

        public abstract void PrepareGame();

        public abstract void SendCells(List<UnitCell> movableCellList);

        protected void GenerateCells()
        {
            var id = 0;
            var random = new Random();

            // rand cells
            var cellsNumPerPlayer = random.Next(2, 4);
            var neutralCells = random.Next(4, 7);

            var cellTypes = new List<Cell.CellSizeEnum>();
            var cellUnits = new List<int>();

            for (var i = 0; i < cellsNumPerPlayer; i++)
            {
                cellTypes.Add((Cell.CellSizeEnum)random.Next(1, 5));
            }

            for (var i = 0; i < cellsNumPerPlayer; i++)
            {
                cellUnits.Add(random.Next(10, Cell.CellSettingsDic[cellTypes[i]].MaxUnits));
            }

            for (var i = 0; i < neutralCells; i++)
            {
                cellTypes.Add((Cell.CellSizeEnum)random.Next(1, 5));
            }

            for (var i = 0; i < neutralCells; i++)
            {
                cellUnits.Add(random.Next(5, Cell.CellSettingsDic[cellTypes[i]].MaxUnits / 3));
            }

            var playerMaxUnits = cellUnits.Take(cellsNumPerPlayer).Max() - 1;

            // all neutral units ale bigger than max player unit fix it
            if (cellUnits.TrueForAll(t => t > playerMaxUnits))
            {
                cellUnits[cellTypes.IndexOf((Cell.CellSizeEnum)cellTypes.Min(t => (int)t))] = playerMaxUnits - 1;
            }

            List<float> distances = new List<float>();
            Cell centralCellMain = null;

            foreach (var playerInstance in Game.Instance.AllPlayers.Values)
            {
                var centralCell = new Cell(Vector2f.Zero, cellUnits[0], cellTypes[0], id++);

                RandPositionForCell(centralCell, random, centralCellMain == null ? new Vector2f(Game.Instance.Width / 2, Game.Instance.Height / 2) : centralCellMain.Position, centralCellMain == null ? 300 : 100);
                playerInstance.AddCell(centralCell);
                AllCells.Add(centralCell);

                if (centralCellMain == null)
                {
                    centralCellMain = centralCell;
                }

                for (var i = 1; i < cellsNumPerPlayer; i++)
                {
                    var cell = new Cell(Vector2f.Zero, cellUnits[i], cellTypes[i], id++);
                    if (distances.Count < i)
                    {
                        RandPositionForCell(cell, random, centralCellMain.Position, random.Next(200, 800));
                        distances.Add(cell.GetDistanceBetweenCells(centralCell));
                    }
                    else
                    {
                        RandPositionForCell(cell, random, centralCell.Position, distances[i - 1]);
                    }
                    playerInstance.AddCell(cell);
                    AllCells.Add(cell);
                }
            }

            for (var i = 0; i < neutralCells; i++)
            {
                var cell = new Cell(Vector2f.Zero, cellUnits[i], cellTypes[i], id++);
                RandPositionForCell(cell, random, Vector2f.Zero);
                AllCells.Add(cell);
            }
        }

        protected void RandPositionForCell(Cell cell, Random random, Vector2f centralCell, float distance = -1)
        {
            const float padding = 35f;
            int counter = 0;
            do
            {
                var position = new Vector2f(
                        random.Next((int)(cell.Radius + padding), (int)(Game.Instance.Width - cell.Radius - padding)),
                        random.Next((int)(cell.Radius + padding + 50f), (int)(Game.Instance.Height - cell.Radius - padding)));
                cell.Position = position;

                if (!AllCells.Any(t => t.IntersectCircle(cell, padding)))
                {
                    if (distance == -1)
                    {
                        return;
                    }

                    float currentDistance = MathHelper.DistanceBetweenTwoPints(cell.Position, centralCell);
                    if (Math.Abs(currentDistance - distance) <= 150f && counter <= 1000)
                    {
                        return;
                    }           
                }

                counter++;
            } while (true);
        }
    }
}