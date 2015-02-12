namespace NanoWar.Animation
{
    using System.Collections.Generic;
    using System.Linq;

    using SFML.Graphics;

    public class FrameAnimation<T> : IAnimation<T>
        where T : Drawable
    {
        private List<Frame> _frames = new List<Frame>();

        private bool _isnormalized;

        public void Animate(AnimatedObject<T> animatedObject, float progress)
        {
            if (!(_frames.Count > 0))
            {
                return;
            }

            Normalize();
            var prog = progress;
            foreach (var frame in _frames)
            {
                prog -= frame.Duration;
                if (prog < 0)
                {
                    animatedObject.TextureRect = frame.SubRect;
                    break;
                }
            }
        }

        public void AddFrame(float relativeDuration, IntRect textureRect)
        {
            _frames.Add(new Frame(relativeDuration, textureRect));
            _isnormalized = false;
        }

        public void ClearFrames()
        {
            _frames.Clear();
            _isnormalized = false;
        }

        private void Normalize()
        {
            if (_isnormalized)
            {
                return;
            }

            var sum = _frames.Sum(frame => frame.Duration);
            foreach (var frame in _frames)
            {
                frame.Duration /= sum;
            }

            _isnormalized = true;
        }

        private class Frame
        {
            public Frame(float newDuration, IntRect newSubRect)
            {
                Duration = newDuration;
                SubRect = newSubRect;
            }

            public float Duration { get; set; }

            public IntRect SubRect { get; private set; }
        }
    }
}