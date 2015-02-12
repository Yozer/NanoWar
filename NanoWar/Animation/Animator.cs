namespace NanoWar.Animation
{
    using System;
    using System.Collections.Generic;

    using SFML.Graphics;

    public class Animator<T, U>
        where T : Drawable
    {
        private Dictionary<U, TimeSpan> _animationDurationList = new Dictionary<U, TimeSpan>();

        private Dictionary<U, IAnimation<T>> _animationList = new Dictionary<U, IAnimation<T>>();

        private Dictionary<U, TimeSpan> _playingAnimationDurationList = new Dictionary<U, TimeSpan>();

        private Dictionary<U, bool> _playingAnimationLoopList = new Dictionary<U, bool>();

        public int AnimationsPlayingCount
        {
            get
            {
                return _playingAnimationDurationList.Keys.Count;
            }
        }

        public U[] AnimationsPlaying
        {
            get
            {
                var keys = new U[_playingAnimationDurationList.Keys.Count];
                _playingAnimationDurationList.Keys.CopyTo(keys, 0);
                return keys;
            }
        }

        public void AddAnimation(U animationId, IAnimation<T> animation, TimeSpan duration)
        {
            if (_animationList.ContainsKey(animationId))
            {
                return;
            }

            _animationList.Add(animationId, animation);
            _animationDurationList.Add(animationId, duration);
        }

        public void RemoveAnimation(U animationId)
        {
            if (_animationList.ContainsKey(animationId))
            {
                _animationList.Remove(animationId);
                _animationDurationList.Remove(animationId);
            }

            if (_playingAnimationDurationList.ContainsKey(animationId))
            {
                _playingAnimationDurationList.Remove(animationId);
                _playingAnimationLoopList.Remove(animationId);
            }
        }

        public void PlayAnimation()
        {
            PlayAnimation(default(U));
        }

        public void PlayAnimation(U animationId)
        {
            PlayAnimation(animationId, false, true);
        }

        public void PlayAnimation(U animationId, bool loopAnimation)
        {
            PlayAnimation(animationId, loopAnimation, true);
        }

        public void PlayAnimation(U animationId, bool loopAnimation, bool stopOtherAnimations)
        {
            if (stopOtherAnimations)
            {
                _playingAnimationDurationList.Clear();
                _playingAnimationLoopList.Clear();
            }

            if (!_animationList.ContainsKey(animationId))
            {
                return;
            }

            if (_playingAnimationDurationList.ContainsKey(animationId))
            {
                _playingAnimationDurationList[animationId] = TimeSpan.Zero;
                _playingAnimationLoopList[animationId] = loopAnimation;
            }
            else
            {
                _playingAnimationDurationList.Add(animationId, TimeSpan.Zero);
                _playingAnimationLoopList.Add(animationId, loopAnimation);
            }
        }

        public void StopAnimation(U animationId)
        {
            if (!_playingAnimationDurationList.ContainsKey(animationId))
            {
                return;
            }

            _playingAnimationDurationList.Remove(animationId);
            _playingAnimationLoopList.Remove(animationId);
        }

        public void StopAllAnimations()
        {
            foreach (var id in AnimationsPlaying)
            {
                StopAnimation(id);
            }
        }

        public void Update(float delta)
        {
            List<object> eraselist = null;
            var deltaTime = TimeSpan.FromMilliseconds(delta);

            foreach (var id in AnimationsPlaying)
            {
                _playingAnimationDurationList[id] += deltaTime;
                if (_playingAnimationDurationList[id] > _animationDurationList[id])
                {
                    if (_playingAnimationLoopList[id])
                    {
                        while (_playingAnimationDurationList[id] > _animationDurationList[id])
                        {
                            _playingAnimationDurationList[id] -= _animationDurationList[id];
                        }
                    }
                    else
                    {
                        if (eraselist == null)
                        {
                            eraselist = new List<object>();
                        }

                        eraselist.Add(id);
                    }
                }
            }

            if (eraselist != null)
            {
                foreach (U id in eraselist)
                {
                    StopAnimation(id);
                }
            }
        }

        public void Animate(AnimatedObject<T> animatedObject)
        {
            foreach (var id in _playingAnimationDurationList.Keys)
            {
                _animationList[id].Animate(
                    animatedObject, 
                    (float)
                    (_playingAnimationDurationList[id].TotalMilliseconds / _animationDurationList[id].TotalMilliseconds));
            }
        }
    }
}