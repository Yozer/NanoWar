namespace NanoWar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class StateMachine
    {
        private List<GameState> _states = new List<GameState>();

        public int Count
        {
            get
            {
                return _states.Count;
            }
        }

        public void PushState(GameState state)
        {
            _states.Add(state);
        }

        public void PushState(string stateName)
        {
            PushState(GetStateByName(stateName));
        }

        public void PopState()
        {
            if (_states.Count != 0)
            {
                var state = _states.First();
                _states.RemoveAt(0);
                state.Dispose();
            }
        }

        public void ChangeState(GameState state)
        {
            if (_states.Count != 0)
            {
                PopState();
            }

            PushState(state);
        }

        public void ChangeState(string stateName)
        {
            ChangeState(GetStateByName(stateName));
        }

        public GameState PeekState()
        {
            return _states.Count == 0 ? null : _states.First();
        }

        private GameState GetStateByName(string stateName)
        {
            var elementType = Type.GetType(string.Format("NanoWar.States.{0}.{0}", stateName));
            return Activator.CreateInstance(elementType) as GameState;
        }
    }

    internal abstract class GameState
    {
        public abstract void Draw();

        public abstract void Update(float delta);

        public abstract void HandleInput();

        public abstract void Dispose();
    }
}