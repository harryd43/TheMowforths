using System.Collections.Generic;

public class FSM<S, A>
{
    public delegate void StateDelegate(A agent, S state);
    public delegate bool TriggerDelegate(A agent, S state);

    public struct Transition {
        public TriggerDelegate trigger;
        public S goTo;
    }

    public Dictionary<S, Transition[]> StateTransitionTable { get; set; }

    public struct StateMethods {
        public StateDelegate Enter;
        public StateDelegate Update;
        public StateDelegate Leave;
    }
    public Dictionary<S, StateMethods> StateDelegatesTable  { get; set; }

    private enum StateLifecycle
    {
        Enter,
        Update,
        Leave
    }

    public void Enter(A agent, S state)
    {
        RunState(agent, state, StateLifecycle.Enter);
    }

    public S Update(A agent, S state)
    {
        RunState(agent, state, StateLifecycle.Update);
        return InvokeStateChanges(agent, state);
    }

    private void RunState(A agent, S state, StateLifecycle lifecycle)
    {
        if (StateDelegatesTable != null)
        {
            if (StateDelegatesTable.TryGetValue(state, out StateMethods methods))
            {
                if (lifecycle == StateLifecycle.Enter)
                    methods.Enter?.Invoke(agent, state);
                if (lifecycle == StateLifecycle.Update)
                    methods.Update?.Invoke(agent, state);
                if (lifecycle == StateLifecycle.Leave)
                    methods.Leave?.Invoke(agent, state);
            }
        }
    }

    private S InvokeStateChanges(A agent, S state)
    {
        if (StateTransitionTable != null)
        {
            if (StateTransitionTable.TryGetValue(state, out Transition[] transitions))
            {
                for (int i = 0; i < transitions.Length; i++)
                    if (transitions[i].trigger.Invoke(agent, state))
                    {
                        ChangeState(agent, state, transitions[i].goTo);
                        return transitions[i].goTo;
                    }
            }
        }
        return state;
    }

    private void ChangeState(A agent, S oldState, S newState)
    {
        RunState(agent, oldState, StateLifecycle.Leave);
        RunState(agent, newState, StateLifecycle.Enter);
    }
}
