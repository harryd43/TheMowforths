using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Goap 
{
    public class BFS : GoapPlanningAlgorithm
    {
        /// <summary>
        /// Uses GOAP to search for a path from the goal preconditions to the currently satisfied propositions.
        /// </summary>
        /// <param name="goal">The goal to try to achieve</param>
        /// <param name="startingPropositions">The propositions that are true in the current state</param>
        /// <param name="actionSet">The set of actions that can be used in the plan</param>
        /// <returns>An ordered list of actions that will achieve the goal, or null if no plan is possible</returns>
        public GoapPlan CreatePlan(GoapGoal goal, IEnumerable<GoapProposition> startingPropositions, IEnumerable<GoapAction> actionSet)
        {
            if (actionSet == null || actionSet.Count() == 0)
                return null;

            GoapNode goalNode = new();
            goalNode.propositions = new HashSet<GoapProposition>(goal.preconditions);

            GoapNode startNode = new GoapNode();
            startNode.propositions = new HashSet<GoapProposition>(startingPropositions);

            List<GoapAction> actions = GoapNode.Plan(startNode, goalNode, actionSet);

            GoapPlan plan = new GoapPlan
            {
                goal = goal,
                actions = actions
            };
            return plan;
        }

        private class GoapNode
        {
            public HashSet<GoapProposition> propositions;
            public GoapNode parent;
            public GoapAction actionToReach;

            public static List<GoapAction> Plan(GoapNode start, GoapNode goal, IEnumerable<GoapAction> actionSet)
            {
                if (actionSet.Any((GoapAction a) => a == null))
                {
                    Debug.LogError("GoapAgent.Plan has null actions in its action set");
                    return null;
                }
                Debug.Log("Start is  " + start);
                Debug.Log("Goal is  " + goal);

                int panic = 100;
                // Currently implements BFS. Need to switch to A*.
                Queue<GoapNode> openSet = new();
                openSet.Enqueue(start);

                while (openSet.Count > 0)
                {
                    if (panic-- <= 0)
                    {
                        Debug.Log("Panic due to too many BFS iterations");
                        break;
                    }
                    GoapNode current = openSet.Dequeue();
                    Debug.Log("Checking node " + current);
                    bool currentIsGoal = HasCompatiblePropositions(current, goal);
                    if (currentIsGoal)
                    {
                        List<GoapAction> plan = new();
                        if (current == start) // If no actions are required to reach goal, return an empty plan
                            return plan;
                        do
                        {
                            plan.Add(current.actionToReach);
                            current = current.parent;
                        } while (current.parent != null);
                        plan.Reverse();
                        Debug.Log($"Found plan [{plan.ToCommaSeparatedString()}]");
                        return plan;
                    }
                    else
                    {
                        IEnumerable<GoapAction> actionsWeCanPerform = ActionsThatMeetPreconditions(current, actionSet);
                        foreach (GoapAction action in actionsWeCanPerform)
                        {
                            GoapNode child = ApplyAction(current, action);
                            openSet.Enqueue(child);
                            // we should mark this in some way so we don't search states we've been to before
                        }
                    }
                }

                return null; // No plan is possible
            }

            /// <summary>
            /// Returns true if every proposition in the goal node can be found in the test node.
            /// </summary>
            /// <param name="test">Node to test whether it is compatible with goal</param>
            /// <param name="goal">Node holding required set of propositions</param>
            /// <returns></returns>
            static bool HasCompatiblePropositions(GoapNode test, GoapNode goal)
            {
                if (goal.propositions == null)
                    return true;
                return goal.propositions.All(g => test.propositions.Contains(g));
            }

            static IEnumerable<GoapAction> ActionsThatMeetPreconditions(GoapNode node, IEnumerable<GoapAction> actionSet)
            {
                return actionSet.Where(a => node.MeetsActionPreconditions(a));
            }

            static GoapNode ApplyAction(GoapNode oldNode, GoapAction action)
            {
                GoapNode newNode = new GoapNode();
                newNode.parent = oldNode;
                newNode.actionToReach = action;

                // Apply effects of the actions to propositions true in new state
                newNode.propositions = new HashSet<GoapProposition>(oldNode.propositions);
                for (int e = 0; e < action.effects.Length; e++)
                    newNode.propositions.Add(action.effects[e]);
                for (int e = 0; e < action.negativeEffects.Length; e++)
                    newNode.propositions.Remove(action.negativeEffects[e]);
                return newNode;
            }

            public bool MeetsActionPreconditions(GoapAction action)
            {
                if (action.preconditions == null)
                    return true;
                return action.preconditions.All(a => propositions.Contains(a));
            }

            public override string ToString()
            {
                return $"[{propositions.ToCommaSeparatedString()}]" + (actionToReach != null ? $" (by {actionToReach})" : "");
            }
        }
    }
}