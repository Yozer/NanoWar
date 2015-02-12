namespace NanoWar.ParticleSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SFML.Graphics;
    using SFML.System;

    public class ParticleSystem : Transformable, Drawable
    {
        private List<TimeLink<ExpiringTime, IAffector>> _affectors = new List<TimeLink<ExpiringTime, IAffector>>();

        private List<TimeLink<ExpiringTime, EmitterBase>> _emitters = new List<TimeLink<ExpiringTime, EmitterBase>>();

        private bool _needsVertexUpdate = true;

        private Texture _texture;

        private List<IntRect> _textureRects = new List<IntRect>();

        private VertexArray _vertexArray = new VertexArray(PrimitiveType.Quads);

        public ParticleSystem(Texture texture)
        {
            Particles = new List<Particle>();
            _texture = texture;
        }

        public Texture Texture
        {
            get
            {
                return _texture;
            }

            set
            {
                _texture = value;
                _needsVertexUpdate = true;
            }
        }

        public IEnumerable<EmitterBase> Emitters
        {
            get
            {
                return _emitters.Select(t => t.Value);
            }
        }

        public IEnumerable<IAffector> Affectors
        {
            get
            {
                return _affectors.Select(t => t.Value);
            }
        }

        public List<Particle> Particles { get; private set; }

        public void Draw(RenderTarget target, RenderStates states)
        {
            UpdateVertices();
            states.Texture = null;
            if (_texture != null)
            {
                states.Texture = _texture;
            }

            states.Transform *= Transform;
            target.Draw(_vertexArray, states);
        }

        public int AddTextureRect(IntRect textureRect)
        {
            _textureRects.Add(textureRect);
            return _textureRects.Count - 1;
        }

        private void UpdateVertices()
        {
            if (!_needsVertexUpdate)
            {
                return;
            }

            _needsVertexUpdate = false;
            _vertexArray.Clear();

            foreach (var particle in Particles)
            {
                var rect = _textureRects.Count == 0 || particle.TextureIndex >= _textureRects.Count
                               ? new IntRect(0, 0, (int)_texture.Size.X, (int)_texture.Size.Y)
                               : _textureRects[particle.TextureIndex];

                var topleftoffset = new Vector2f(-(rect.Width / 2), -(rect.Height / 2));
                var toprightoffset = new Vector2f(rect.Width / 2, -(rect.Height / 2));
                var bottomrightoffset = new Vector2f(rect.Width / 2, rect.Height / 2);
                var bottomleftoffset = new Vector2f(-(rect.Width / 2), rect.Height / 2);
                var transform = Transform.Identity;
                transform.Translate(particle.Position);
                transform.Rotate(particle.Rotation);
                transform.Scale(particle.Scale);

                _vertexArray.Append(
                    new Vertex(
                        transform.TransformPoint(topleftoffset), 
                        particle.Color, 
                        new Vector2f(rect.Left, rect.Top)));
                _vertexArray.Append(
                    new Vertex(
                        transform.TransformPoint(toprightoffset), 
                        particle.Color, 
                        new Vector2f(rect.Left + (float)rect.Width, rect.Top)));
                _vertexArray.Append(
                    new Vertex(
                        transform.TransformPoint(bottomrightoffset), 
                        particle.Color, 
                        new Vector2f(rect.Left + (float)rect.Width, rect.Top + (float)rect.Height)));
                _vertexArray.Append(
                    new Vertex(
                        transform.TransformPoint(bottomleftoffset), 
                        particle.Color, 
                        new Vector2f(rect.Left, rect.Top + (float)rect.Height)));
            }
        }

        public void Update(TimeSpan deltaTime)
        {
            _needsVertexUpdate = true;
            for (var i = 0; i < _emitters.Count; i++)
            {
                _emitters[i].Value.EmitParticles(this, deltaTime);
                _emitters[i].Time.ElapsedTime += deltaTime;

                if (_emitters[i].Time.TimeUntilExpire != TimeSpan.Zero
                    && _emitters[i].Time.ElapsedTime >= _emitters[i].Time.TimeUntilExpire)
                {
                    _emitters[i].Value.EmitParticles(this, TimeSpan.Zero);
                    _emitters.RemoveAt(i);
                    i--;
                }
            }

            List<Particle> eraselist = null;
            foreach (var particle in Particles)
            {
                particle.ElapsedLifetime += deltaTime;
                if (particle.TotalLifetime != TimeSpan.Zero && particle.ElapsedLifetime > particle.TotalLifetime)
                {
                    if (eraselist == null)
                    {
                        eraselist = new List<Particle>();
                    }

                    eraselist.Add(particle);
                }
                else if (_affectors.Count != 0)
                {
                    particle.Position =
                        new Vector2f(
                            particle.Position.X + (float)(deltaTime.TotalSeconds * particle.Velocity.X), 
                            particle.Position.Y + (float)(deltaTime.TotalSeconds * particle.Velocity.Y));
                    particle.Rotation += (float)(deltaTime.TotalSeconds * particle.RotationSpeed);
                    foreach (var affector in _affectors)
                    {
                        affector.Value.ApplyAffector(particle, deltaTime, affector.Time.ElapsedTime);
                    }
                }
            }

            if (eraselist != null)
            {
                foreach (var currentParticle in eraselist)
                {
                    Particles.Remove(currentParticle);
                }
            }

            for (var i = 0; i < _affectors.Count; i++)
            {
                _affectors[i].Time.ElapsedTime += deltaTime;
                if (_affectors[i].Time.TimeUntilExpire != TimeSpan.Zero
                    && _affectors[i].Time.ElapsedTime >= _affectors[i].Time.TimeUntilExpire)
                {
                    _affectors.RemoveAt(i);
                    i--;
                }
            }
        }

        public void AddEmitter(EmitterBase Emitter)
        {
            AddEmitter(Emitter, TimeSpan.Zero);
        }

        public void AddEmitter(EmitterBase Emitter, TimeSpan timeToLive)
        {
            var emitter = new TimeLink<ExpiringTime, EmitterBase>(new ExpiringTime(timeToLive), Emitter);
            if (!_emitters.Contains(emitter))
            {
                _emitters.Add(emitter);
            }
        }

        public void AddAffector(IAffector Affector)
        {
            AddAffector(Affector, TimeSpan.Zero);
        }

        public void AddAffector(IAffector Affector, TimeSpan timeToLive)
        {
            var affector = new TimeLink<ExpiringTime, IAffector>(new ExpiringTime(timeToLive), Affector);
            if (!_affectors.Contains(affector))
            {
                _affectors.Add(affector);
            }
        }

        internal void AddParticle(Particle particle)
        {
            Particles.Add(particle);
            _needsVertexUpdate = true;
        }

        public void ClearEmitters()
        {
            _emitters.Clear();
        }

        public void ClearAffectors()
        {
            _affectors.Clear();
        }

        public void ClearParticles()
        {
            Particles.Clear();
            _needsVertexUpdate = true;
        }

        private class ExpiringTime
        {
            public ExpiringTime(TimeSpan newTimeUntilExpire)
            {
                ElapsedTime = TimeSpan.Zero;
                TimeUntilExpire = newTimeUntilExpire;
            }

            public TimeSpan ElapsedTime { get; set; }

            public TimeSpan TimeUntilExpire { get; private set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals((ExpiringTime)obj);
            }

            private bool Equals(ExpiringTime other)
            {
                return ElapsedTime.Equals(other.ElapsedTime) && TimeUntilExpire.Equals(other.TimeUntilExpire);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (ElapsedTime.GetHashCode() * 397) ^ TimeUntilExpire.GetHashCode();
                }
            }
        }

        private class TimeLink<T, TV>
        {
            public TimeLink(T newTime, TV newValue)
            {
                Time = newTime;
                Value = newValue;
            }

            public T Time { get; private set; }

            public TV Value { get; private set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return Equals((TimeLink<T, TV>)obj);
            }

            private bool Equals(TimeLink<T, TV> other)
            {
                return EqualityComparer<T>.Default.Equals(Time, other.Time)
                       && EqualityComparer<TV>.Default.Equals(Value, other.Value);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (EqualityComparer<T>.Default.GetHashCode(Time) * 397)
                           ^ EqualityComparer<TV>.Default.GetHashCode(Value);
                }
            }
        }
    }
}