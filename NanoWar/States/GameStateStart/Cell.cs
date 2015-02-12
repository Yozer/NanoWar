namespace NanoWar.States.GameStateStart
{
    using System;
    using System.Collections.Generic;

    using NanoWar.HelperClasses;
    using NanoWar.Shapes;

    using SFML.Graphics;
    using SFML.System;

    public class Cell : SimpleCircle, Drawable
    {
        public enum CellSizeEnum
        {
            Tiny, 

            Small, 

            Medium, 

            Big, 

            Huge
        }

        private const uint TextFontSize = 16;

        public static readonly Dictionary<CellSizeEnum, CellSettings> CellSettingsDic =
            new Dictionary<CellSizeEnum, CellSettings>
                {
                    {
                        CellSizeEnum.Tiny, 
                        new CellSettings
                            {
                                Scale = new Vector2f(0.1f, 0.1f), 
                                IncreaseUnitsTime = 3000, 
                                MaxUnits = 20
                            }
                    }, 
                    {
                        CellSizeEnum.Small, 
                        new CellSettings
                            {
                                Scale = new Vector2f(0.15f, 0.15f), 
                                IncreaseUnitsTime = 2500, 
                                MaxUnits = 30
                            }
                    }, 
                    {
                        CellSizeEnum.Medium, 
                        new CellSettings
                            {
                                Scale = new Vector2f(0.2f, 0.2f), 
                                IncreaseUnitsTime = 2000, 
                                MaxUnits = 40
                            }
                    }, 
                    {
                        CellSizeEnum.Big, 
                        new CellSettings
                            {
                                Scale = new Vector2f(0.25f, 0.25f), 
                                IncreaseUnitsTime = 1500, 
                                MaxUnits = 50
                            }
                    }, 
                    {
                        CellSizeEnum.Huge, 
                        new CellSettings
                            {
                                Scale = new Vector2f(0.3f, 0.3f), 
                                IncreaseUnitsTime = 1000, 
                                MaxUnits = 60
                            }
                    }
                };

        private CellSettings _cellSettings;

        private List<Cilium> _cilium = new List<Cilium>();

        private float _clockIncreaseUnits;

        private PlayerInstance _player;

        private Sprite _spriteCellBubble;

        private Sprite _spriteCellInside;

        private int _units;

        private Text _unitsText;

        public bool WasHover = false;

        public Cell(Vector2f position, int startUnits, CellSizeEnum sizeEnum, int id)
        {
            _cellSettings = CellSettingsDic[sizeEnum];

            _unitsText = new Text(string.Empty, ResourceManager.Instance["fonts/verdana"] as Font, TextFontSize);

            _spriteCellBubble = new Sprite(ResourceManager.Instance["game/cell_bubble"] as Texture)
                                    {
                                        Scale =
                                            _cellSettings
                                            .Scale
                                    };
            _spriteCellBubble.Origin = new Vector2f(
                _spriteCellBubble.TextureRect.Width / 2, 
                _spriteCellBubble.TextureRect.Height / 2);

            _spriteCellInside = new Sprite(ResourceManager.Instance["game/cell_inside_neutral"] as Texture)
                                    {
                                        Scale =
                                            _cellSettings
                                            .Scale
                                    };
            _spriteCellInside.Origin = new Vector2f(
                _spriteCellInside.TextureRect.Width / 2, 
                _spriteCellInside.TextureRect.Height / 2);

            Type = sizeEnum;
            Id = id;
            Radius = (float)_spriteCellBubble.TextureRect.Height / 2 * _cellSettings.Scale.X;
            Units = startUnits;
            Position = position;
        }

        public int Units
        {
            get
            {
                return _units;
            }

            set
            {
                _units = value;
                var newScale = _cellSettings.Scale.X * _units / _cellSettings.MaxUnits;
                if (newScale <= 0.09f)
                {
                    newScale = 0.09f;
                }
                else if (newScale > _cellSettings.Scale.X)
                {
                    newScale = _cellSettings.Scale.X;
                }

                _spriteCellInside.Scale = new Vector2f(newScale, newScale);
                UpdateText();
            }
        }

        public PlayerInstance Player
        {
            get
            {
                return _player;
            }

            set
            {
                if (_player == null || value == null || _player.Id != value.Id)
                {
                    var colorName = string.Empty;
                    if (value == null)
                    {
                        colorName = "neutral";
                    }
                    else if (value.Color.R == 255 && value.Color.G == 0 && value.Color.B == 0)
                    {
                        colorName = "red";
                    }
                    else if (value.Color.R == 0 && value.Color.G == 255 && value.Color.B == 0)
                    {
                        colorName = "green";
                    }
                    else if (value.Color.R == 0 && value.Color.G == 0 && value.Color.B == 255)
                    {
                        colorName = "blue";
                    }

                    _spriteCellInside.Texture = ResourceManager.Instance["game/cell_inside_" + colorName] as Texture;
                }

                _player = value;
            }
        }

        public int Id { get; set; }

        public CellSizeEnum Type { get; private set; }

        public override Vector2f Position
        {
            get
            {
                return _spriteCellBubble.Position;
            }

            set
            {
                _spriteCellBubble.Position = value;
                _spriteCellInside.Position = value;
                _unitsText.Position = value;
                base.Position = value;
                PrepareCilium();
            }
        }

        public int PlayerId
        {
            get
            {
                return Player == null ? -1 : Player.Id;
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_spriteCellBubble);
            target.Draw(_spriteCellInside);
            target.Draw(_unitsText);
            _cilium.ForEach(target.Draw);
        }

        private void PrepareCilium()
        {
            _cilium.ForEach(t => t.Dispose());
            _cilium.Clear();

            var count = (int)(150 * _cellSettings.Scale.X); // 120 magic number
            var angleStep = 360f / count;

            for (var i = 0; i < count; i++)
            {
                var witka = new Cilium { Rotation = angleStep * i };
                witka.Position = MathHelper.RotatePoint(
                    Position, 
                    Position + new Vector2f(Radius, 0), 
                    MathHelper.DegreeToRadian(witka.Rotation));
                _cilium.Add(witka);
            }
        }

        public void Update(float delta)
        {
            _cilium.ForEach(t => t.Update(delta));
            if (Player == null)
            {
                return;
            }

            _clockIncreaseUnits += delta;
            if (!(_clockIncreaseUnits >= _cellSettings.IncreaseUnitsTime))
            {
                return;
            }

            if (Units < _cellSettings.MaxUnits)
            {
                ++Units;
            }

            _clockIncreaseUnits = 0;
        }

        private void UpdateText()
        {
            _unitsText.DisplayedString = _units.ToString();
            _unitsText.Origin = new Vector2f(
                _unitsText.GetLocalBounds().Left + _unitsText.GetLocalBounds().Width / 2, 
                _unitsText.GetLocalBounds().Top + _unitsText.GetLocalBounds().Height / 2);
        }

        public void Dispose()
        {
            _cilium.ForEach(t => t.Dispose());
            _unitsText.Dispose();
            _spriteCellBubble.Dispose();
            _spriteCellInside.Dispose();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Cell))
            {
                return false;
            }

            var cell = obj as Cell;
            return cell.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public class CellSettings
        {
            public Vector2f Scale { get; set; }

            public int MaxUnits { get; set; }

            public int IncreaseUnitsTime { get; set; }
        }
    }

    internal class Cilium : Transformable, Drawable
    {
        private const float Height = 2f;

        private const float Width = 17f;

        private const uint PointCount = 20;

        private static Random _rand = new Random();

        private VertexArray _points = new VertexArray();

        private float _totalTime;

        public Cilium()
        {
            _totalTime = NextFloat(_rand) % (int)Math.PI * 2;
            _points.PrimitiveType = PrimitiveType.LinesStrip;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            states.Transform *= Transform;
            target.Draw(_points, states);
        }

        public void Update(float delta)
        {
            _totalTime += delta / 500f;
            _points.Clear();

            for (var index = 0; index < PointCount; index++)
            {
                var x = (float)(index / (float)PointCount * Math.PI);
                var y = Height * (float)(Math.Sin(x + _totalTime) + Math.Cos(_totalTime + Math.PI / 2));
                _points.Append(new Vertex(new Vector2f(index / (float)PointCount * Width, y), Color.White));
            }
        }

        public new void Dispose()
        {
            _points.Clear();
            base.Dispose();
        }

        private static float NextFloat(Random random)
        {
            var mantissa = (random.NextDouble() * 2.0) - 1.0;
            var exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }
    }
}