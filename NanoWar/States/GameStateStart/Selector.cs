namespace NanoWar.States.GameStateStart
{
    using SFML.Graphics;
    using SFML.System;

    internal class Selector : Drawable
    {
        private const uint FontSize = 22;

        private const int SpeedOfUnitsIncrease = 70; // ms

        private Cell _cell;

        private float _clockIncreasingUnits;

        private float _currentSpeedOfUnitsLoading;

        private Text _text;

        private int _units;

        public Selector()
        {
            _text = new Text(string.Empty, ResourceManager.Instance["fonts/verdana"] as Font, FontSize);
        }

        public int Units
        {
            get
            {
                return _units;
            }

            private set
            {
                _units = value;
                _text.DisplayedString = _units.ToString();
                _text.Position = new Vector2f(_cell.Position.X, _cell.Position.Y - _cell.Radius - 55f);
                _text.Origin = new Vector2f(_text.GetLocalBounds().Left + _text.GetLocalBounds().Width / 2, 0);
            }
        }

        public int MaxUnits { get; set; }

        public Cell Cell
        {
            get
            {
                return _cell;
            }

            set
            {
                _cell = value;
                if (Cell == null)
                {
                    return;
                }

                Units = 1;
                _currentSpeedOfUnitsLoading = SpeedOfUnitsIncrease;
                _clockIncreasingUnits = 0;
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_text);
        }

        public void Update(float delta)
        {
            _clockIncreasingUnits += delta;
            if (!(_clockIncreasingUnits >= _currentSpeedOfUnitsLoading))
            {
                return;
            }

            if (Units < MaxUnits)
            {
                ++Units;
                _currentSpeedOfUnitsLoading *= 0.4f; // faster
            }

            _clockIncreasingUnits = 0;
        }

        public bool TargetCellContainsPoint(Vector2i point)
        {
            return _cell.ContainsPoint(point);
        }

        public void Dispose()
        {
            _text.Dispose();
        }
    }
}