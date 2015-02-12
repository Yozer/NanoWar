namespace NanoWar.States.GameStateStart
{
    using System;
    using System.Linq;

    using NanoWar.HelperClasses;
    using NanoWar.ParticleSystem;
    using NanoWar.Shapes;

    using SFML.Graphics;
    using SFML.System;

    public class UnitCell : SimpleCircle, Drawable
    {
        private const uint FontSize = 17;

        private const float Velocity = 0.005f;

        private static float _lastPlayedSoundAbsorption;

        private bool _animatingParticles;

        private float _currentVelocity;

        private bool _increasing;

        private Vector2f _initialScale;

        private Vector2f _moveDirection; // normalized vector

        private ParticleSystem _particleSystem;

        private Sprite _sprite;

        private Text _text;

        private float _totalTime;

        public UnitCell(Cell targetCell, Cell sourceCell, int units, PlayerInstance player)
        {
            TargetCell = targetCell;
            SourceCell = sourceCell;
            Units = UnitsLeft = units;
            _currentVelocity = Velocity;

            string colorName = null;
            if (SourceCell.Player.Color.R == 255 && SourceCell.Player.Color.G == 0 && SourceCell.Player.Color.B == 0)
            {
                colorName = "red";
            }
            else if (SourceCell.Player.Color.R == 0 && SourceCell.Player.Color.G == 255 && SourceCell.Player.Color.B == 0)
            {
                colorName = "green";
            }
            else if (SourceCell.Player.Color.R == 0 && SourceCell.Player.Color.G == 0
                     && SourceCell.Player.Color.B == 255)
            {
                colorName = "blue";
            }

            _sprite = new Sprite(ResourceManager.Instance["game/unit_cell_" + colorName] as Texture);

            _text = new Text(units.ToString(), ResourceManager.Instance["fonts/verdana"] as Font, FontSize);
            _text.Origin = new Vector2f(
                _text.GetLocalBounds().Left + _text.GetLocalBounds().Width / 2, 
                _text.GetLocalBounds().Top);

            _particleSystem = new ParticleSystem(ResourceManager.Instance["game/particle"] as Texture);

            var scale = 0.5f + Units * 0.0125f;
            if (scale >= 1f)
            {
                scale = 1f;
            }

            Scale = _initialScale = new Vector2f(scale, scale);

            Radius = _sprite.GetGlobalBounds().Height / 2;
            _sprite.Origin = new Vector2f(_sprite.GetLocalBounds().Width / 2, _sprite.GetLocalBounds().Height / 2);
            SourcePlayer = player;
        }

        public int Units { get; private set; }

        public Cell TargetCell { get; private set; }

        public Cell SourceCell { get; private set; }

        public PlayerInstance SourcePlayer { get; private set; }

        public int Id { get; set; }

        public Vector2f Scale
        {
            get
            {
                return _sprite.Scale;
            }

            set
            {
                _sprite.Scale = value;
            }
        }

        public override Vector2f Position
        {
            get
            {
                return _sprite.Position;
            }

            set
            {
                _sprite.Position = value;
                base.Position = value;
            }
        }

        public bool IsBusy
        {
            get
            {
                return IsMoving || _animatingParticles;
            }
        }

        public bool IsMoving { get; private set; }

        public int UnitsLeft { get; private set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (IsMoving)
            {
                target.Draw(_sprite);
                target.Draw(_text);
            }
            else if (_animatingParticles)
            {
                target.Draw(_particleSystem);
            }
        }

        public void InitiateMoving()
        {
            _moveDirection =
                MathHelper.NormalizeVector(
                    new Vector2f(
                        TargetCell.Position.X - SourceCell.Position.X, 
                        TargetCell.Position.Y - SourceCell.Position.Y));

            Position = (SourceCell.Radius - Radius) * _moveDirection + SourceCell.Position;

            // set start position to edge of source circle
            var delta = TargetCell.Position - SourceCell.Position;
            _sprite.Rotation = (float)(Math.Atan2(delta.Y, delta.X) * 180f / Math.PI);
            _text.Position = Position - new Vector2f(0, _sprite.GetGlobalBounds().Height);

            SourceCell.Units -= Units;

            var emitter = new ParticleEmitter(
                TargetCell.Position + _moveDirection * (-1) * (TargetCell.Radius + Radius), 
                Math.Abs(MathHelper.RadianToDegree(Math.Atan2(_moveDirection.Y, _moveDirection.X)) - 180), 
                SourcePlayer.Color, 
                Units);
            _particleSystem.AddEmitter(emitter, ParticleEmitter.ExplostionDuration);

            var affector = new ParticleAffector(
                50f, 
                TargetCell.Position, 
                TargetCell.Radius, 
                TargetCell.Position + _moveDirection * (-1) * TargetCell.Radius);
            _particleSystem.AddAffector(affector, TimeSpan.Zero);
            IsMoving = true;
            Game.Instance.AudioManager.PlaySound("game/cell_out");
        }

        private void UpdateTargetCell(int particlesAbsorbed)
        {
            if (particlesAbsorbed == 0)
            {
                return;
            }

            if (particlesAbsorbed != 0 && _lastPlayedSoundAbsorption > 100)
            {
                _lastPlayedSoundAbsorption = 0;
                Game.Instance.AudioManager.PlaySound("game/cell_in", false, 3);
            }

            UnitsLeft -= particlesAbsorbed;
            if (TargetCell.Player == null || !Equals(TargetCell.Player, SourcePlayer))
            {
                TargetCell.Units -= particlesAbsorbed;
            }
            else
            {
                TargetCell.Units += particlesAbsorbed;
            }

            if (TargetCell.Units < 0)
            {
                if (TargetCell.Player != null)
                {
                    TargetCell.Player.Cells.Remove(TargetCell);
                }

                SourcePlayer.AddCell(TargetCell);
                TargetCell.Units *= -1;
            }
        }

        public void Update(float delta)
        {
            if (IsMoving && IntersectCircle(TargetCell))
            {
                Game.Instance.AudioManager.PlaySound("game/unit_cell_explosion");
                IsMoving = false;
                _animatingParticles = true;
            }
            else if (_animatingParticles)
            {
                _lastPlayedSoundAbsorption += delta;
                _particleSystem.Update(TimeSpan.FromMilliseconds(delta));
                UpdateTargetCell(_particleSystem.Particles.Count(t => t.Color.A == 0));
                _particleSystem.Particles.RemoveAll(t => t.Color.A == 0);

                if (_particleSystem.Particles.Count == 0)
                {
                    _particleSystem.ClearAffectors();
                    _particleSystem.ClearParticles();
                    _animatingParticles = false;
                }
            }
            else
            {
                if (!_increasing)
                {
                    _totalTime += delta / 1000;
                }
                else
                {
                    _totalTime -= delta / 1000;
                }

                var move = (0.6f + _initialScale.X / 2) * (float)Math.Pow(1.1f, _totalTime * 100);

                Scale = _increasing
                            ? new Vector2f(Scale.X - _currentVelocity, Scale.Y + _currentVelocity)
                            : new Vector2f(Scale.X + 3 * _currentVelocity, Scale.Y - 3 * _currentVelocity);
                Position += _moveDirection * move;
                _text.Position += _moveDirection * move;

                if (Scale.Y <= _initialScale.X * 0.8f)
                {
                    _increasing = true;
                }
                else if (Scale.Y >= _initialScale.X)
                {
                    _increasing = false;
                    _totalTime = 0;
                }
            }
        }

        public void Dispose()
        {
            _text.Dispose();
            _sprite.Dispose();
        }
    }
}