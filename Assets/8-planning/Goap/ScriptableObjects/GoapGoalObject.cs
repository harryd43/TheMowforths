using UnityEngine;

namespace Goap
{
    namespace ScriptableObject
    {
        [CreateAssetMenu(fileName = "GoapGoal", menuName = "Scriptable Objects/GoapGoal")]
        public class GoapGoalObject : UnityEngine.ScriptableObject, GoapGoal
        {
            [SerializeField] private GoapPropositionObject[] _preconditions;
            public GoapProposition[] preconditions { get { return _preconditions; } }

            [SerializeField] private GoapPropositionObject[] _considerOnlyWhen;
            public GoapProposition[] considerOnlyWhen { get { return _considerOnlyWhen; } }

            public override string ToString() { return name; }

            public bool CheckValid(GoapActor actor) { return true;  }
        }
    }
}
