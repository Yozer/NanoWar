namespace NanoWar.States.GameStateStart
{
    using System.Collections.Generic;

    using SFML.Graphics;

    public class PlayerInstance
    {
        public List<Cell> Cells = new List<Cell>();

        public List<UnitCell> UnitCells = new List<UnitCell>();

        public PlayerInstance(string name)
        {
            Name = name;
        }

        public int Id { get; set; }

        public Color Color { get; set; }

        public string Name { get; private set; }

        public bool IsReady { get; set; }

        public int LobbyId { get; set; }

        public void AddCell(Cell cell)
        {
            cell.Player = this;
            Cells.Add(cell);
        }

        public void Update(float delta)
        {
            foreach (var unitCell in UnitCells)
            {
                unitCell.Update(delta);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is PlayerInstance)
            {
                var c = obj as PlayerInstance;
                return c.Id == Id;
            }

            return false;
        }

        public void AddUnitCell(UnitCell unitCell)
        {
            unitCell.InitiateMoving();
            UnitCells.Add(unitCell);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}