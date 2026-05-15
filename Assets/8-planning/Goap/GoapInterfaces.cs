using System;
using System.Collections.Generic;

namespace Goap
{
    /// <summary>
    /// The interface exposed to `GoapGoal`, `GoapAction`, and `GoapProposition`
    /// </summary>
    public interface GoapActor {
        public T GetComponent<T>();
    }
    
    /// <summary>
    /// A proposition is an object that can be evaluated by Goap as either true or false
    /// through the `IsTrue` method.
    /// </summary>
    public interface GoapProposition
    {
        bool IsTrue(GoapActor actor);
    }

    public interface GoapAction
    {
        enum State {
            Failed,
            Running,
            Complete
        }

        GoapProposition[] preconditions { get; }
        GoapProposition[] effects { get; }
        GoapProposition[] negativeEffects { get; }

        GoapAction.State Perform(GoapActor actor);
    }

    public interface GoapGoal
    {
        public enum State {
            NotRelevant,
            Unsatisfied,
            Satisfied,
        }

        GoapProposition[] preconditions { get; }
        GoapProposition[] considerOnlyWhen  { get; }

        bool CheckValid(GoapActor actor);
    }

    public interface GoapAgent {
        GoapAction[] actions { get; }
        GoapGoal[] goals  { get; }
        GoapProposition[] knownPropositions  { get; }
    }

    interface GoapPlanningAlgorithm
    {
        GoapPlan CreatePlan(GoapGoal goal, IEnumerable<GoapProposition> startingPropositions, IEnumerable<GoapAction> actionSet);
    }


    [Serializable]
    public class GoapPlan
    {
        public List<GoapAction> actions;
        public GoapGoal goal;
        public bool valid = true;

        public GoapAction GetNextAction()
        {
            if (actions?.Count > 0)
                return actions[0];
            return null;
        }
    }

}