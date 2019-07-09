using System;
using IctBaden.Framework.Tron;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable NotAccessedField.Global

namespace IctBaden.Framework.Automat
{
    public abstract class StateMachine
    {
        public delegate void StateChangedHandler(EventArgs e);
        public delegate void DoneHandler(EventArgs e);

        public event StateChangedHandler StateChanged;
        public event StateChangedHandler Done;

        public object Result { get; private set; }

        public State CurrentState { get; private set; }
        public string CurrentStateName => (CurrentState == null) ? "<null>" : CurrentState.GetType().Name;
        protected State LastState;
        protected StateTimeoutCollection StateTimeouts;
        private readonly State _initialState;

        protected StateMachine(Type initial)
        {
            Result = null;
            StateTimeouts = new StateTimeoutCollection();
            _initialState = Activator.CreateInstance(initial) as State;
        }

        public void Start()
        {
            GoState(_initialState, null);
        }

        public bool IsDone => CurrentState == null;

        public void SetDone(object result)
        {
            Result = result;
            GoState(null, null);
        }

        public void GoState<T>() where T : State, new()
        {
            var t = new T();
            GoState(t, null);
        }
        public void GoState<T>(object input) where T : State, new()
        {
            var t = new T();
            GoState(t, input);
        }
        public void GoState(State newState, object input)
        {
            if (CurrentState != null)
            {
                CurrentState.ClearTimeout();
                CurrentState.OnExit();
            }

            LastState = CurrentState;
            CurrentState = newState;

            TronTrace.TraceInformation($"GoState {GetType().Name}:{CurrentStateName}");

            StateChanged?.Invoke(new EventArgs());

            if (CurrentState != null)
            {
                CurrentState.SetContext(this);
                if (input != null)
                    CurrentState.OnEnter(input);
                else
                    CurrentState.OnEnter();
            }
            else
            {
                Done?.Invoke(new EventArgs());
            }
        }

        public void SignalInput(object input)
        {
            CurrentState?.OnInput(input);
        }

        #region Timeouts
        public void ClearTimeout(string timerName)
        {
            StateTimeouts.Stop(timerName);
        }
        public void ClearAllTimeouts()
        {
            StateTimeouts.StopAll();
        }

        public void SetTimeout(string timerName, long timeoutMilliseconds)
        {
            StateTimeouts.Stop(timerName);
            var t = new StateTimeout(timerName, timeoutMilliseconds, OnTimeout);
            StateTimeouts.Add(t);
            t.Activate();
        }

        private void OnTimeout(object timerName)
        {
            var name = timerName as string;
            if (string.IsNullOrEmpty(name))
                return;

            TronTrace.PrintLine("OnTimeout:" + name);

            if (!StateTimeouts.IsRunning(name))
            {
                TronTrace.PrintLine("NOT RUNNING");
                return;
            }

            StateTimeouts.Stop(name);
            CurrentState?.OnTimeout(name);
        }
        #endregion
    }

}
