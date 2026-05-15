using UnityEngine;

namespace Goap
{
    namespace ScriptableObject
    {
        [CreateAssetMenu(fileName = "GoapAgent", menuName = "Scriptable Objects/GoapAgent")]
        public class GoapAgentObject : UnityEngine.ScriptableObject, GoapAgent
        {
            [SerializeField] private GoapActionObject[] _actions;
            public GoapAction[] actions { get { return _actions; } }

            [SerializeField] private GoapGoalObject[] _goals;
            public GoapGoal[] goals { get { return _goals;  } }

            [SerializeField] private GoapPropositionObject[] _knownPropositions; // Used for testing which propositions are true at the start 
            public GoapProposition[] knownPropositions { get { return _knownPropositions; } }

            public override string ToString() { return name; }
        }
    }
}
