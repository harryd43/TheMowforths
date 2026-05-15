using System.Collections.Generic;
using Goap.ScriptableObject;
using UnityEngine;

namespace Goap
{
    public class GoapPlanner :  MonoBehaviour, GoapActor
    {
        [SerializeField] private GoapAgentObject agent;
        [SerializeField] private GoapPlan plan;

        private GoapPlanningAlgorithm algorithm = new BFS();

        private Dictionary<GoapGoal, GoapGoal.State> goalStates = new Dictionary<GoapGoal, GoapGoal.State>();
        public Dictionary<GoapGoal, GoapGoal.State> getGoalStates() { return goalStates; }

        void Start()
        {
            plan = null;
        }

        void Update()
        {
            if (plan != null && !plan.goal.CheckValid(this))
                plan.valid = false;
            if (plan == null || !plan.valid)
                UpdatePlan();
            EnactPlan();
        }

        [ContextMenu("Test Plan")]
        private void UpdatePlan()
        {
            GoapGoal unsatisfiedGoal = SelectGoal();

            if (unsatisfiedGoal != null)
            {
                List<GoapProposition> startingPropositions = new List<GoapProposition>(agent.knownPropositions).FindAll(p => p.IsTrue(this));
                plan = algorithm.CreatePlan(unsatisfiedGoal, startingPropositions, agent.actions);
            }
            else
            {
                Debug.Log("Planner has no unsatisfied goals that it can attempt");
                plan = null;
            }
            
        }

        private GoapGoal SelectGoal()
        {
            GoapGoal unsatisfiedGoal = null;
            if (agent != null && agent.goals != null)
                for (int i = 0; i < agent.goals.Length; i++)
                {
                    GoapGoal goal = agent.goals[i];
                    if (!IsGoalAvailable(goal))
                        goalStates[goal] = GoapGoal.State.NotRelevant;
                    else if (IsGoalSatisfied(goal))
                        goalStates[goal] = GoapGoal.State.Satisfied;
                    else
                    {
                        goalStates[goal] = GoapGoal.State.Unsatisfied;
                        // The first unsatisfied goal is the one to attempt but we test the others to update the dictionary
                        if (unsatisfiedGoal == null) 
                            unsatisfiedGoal = goal;
                    }
                }
            return unsatisfiedGoal;
        }

        private void EnactPlan()
        {
            if (plan != null)
            {
                GoapAction action = plan.GetNextAction();
                if (action != null)
                {
                    GoapAction.State actionState = action.Perform(this);
                    if (actionState == GoapAction.State.Failed)
                        Debug.LogWarning($"Goap action {action} failed");
                    else if (actionState == GoapAction.State.Complete)
                        plan.actions.RemoveAt(0);
                }
            }
        }

        private bool IsGoalAvailable(GoapGoal goal)
        {
            for (int i = 0; i < goal.considerOnlyWhen.Length; i++)
                if (!goal.considerOnlyWhen[i].IsTrue(this))
                    return false;
            return true;
        }

        private bool IsGoalSatisfied(GoapGoal goal)
        {
            for (int i = 0; i < goal.preconditions.Length; i++)
                if (!goal.preconditions[i].IsTrue(this))
                    return false;
            return true;
        }
    }
}
