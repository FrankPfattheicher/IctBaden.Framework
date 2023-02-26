// ReSharper disable UnusedMember.Global

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedParameter.Global
namespace IctBaden.Framework.Automat
{
    public class State<TStateMachine> : State where TStateMachine : StateMachine
    {
        public new TStateMachine? Automat => base.Automat as TStateMachine;
    }

    public class State
    {
        public StateMachine? Automat { get; private set; }

        internal void SetContext(StateMachine automat)
        {
            Automat = automat;
        }

        #region State methods

        public virtual void OnEnter()
        {
        }

        public virtual void OnEnter(object input)
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnInput(object input)
        {
        }

        public virtual void OnTimeout(string timeout)
        {
            OnTimeout();
        }

        public virtual void OnTimeout()
        {
        }

        #endregion

        #region GoState()

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

        public void GoState(State newState, object? input)
        {
            Automat?.GoState(newState, input);
        }

        #endregion

        public void Done(object result)
        {
            Automat?.SetDone(result);
        }

        #region Timeout

        public void SetTimeout(long timeoutMilliseconds)
        {
            if (Automat?.CurrentState == null)
                return;
            if (Automat.CurrentStateName != GetType().Name)
                return;
            Automat.SetTimeout("####" + Automat.CurrentStateName, timeoutMilliseconds);
        }

        public void SetTimeout(string timerName, long timeoutMilliseconds)
        {
            Automat?.SetTimeout(timerName, timeoutMilliseconds);
        }

        public void ClearTimeout()
        {
            if (Automat?.CurrentState == null)
                return;
            Automat.ClearTimeout("####" + Automat.CurrentStateName);
        }

        public void ClearTimeout(string timerName)
        {
            Automat?.ClearTimeout(timerName);
        }

        public void ClearAllTimeouts()
        {
            Automat?.ClearAllTimeouts();
        }

        #endregion
    }
}