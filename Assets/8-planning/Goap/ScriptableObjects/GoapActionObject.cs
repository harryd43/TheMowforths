using UnityEngine;

namespace Goap
{
    namespace ScriptableObject
    {
        public abstract class GoapActionObject : UnityEngine.ScriptableObject, GoapAction
        {
            [SerializeField] private GoapPropositionObject[] _preconditions;
            public GoapProposition[] preconditions { get { return _preconditions; } }

            [SerializeField] private GoapPropositionObject[] _effects;
            public GoapProposition[] effects { get { return _effects;  } }

            [SerializeField] private GoapPropositionObject[] _negativeEffects;
            public GoapProposition[] negativeEffects { get { return _negativeEffects;  } }

            public abstract GoapAction.State Perform(GoapActor actor);

            public override string ToString() { return name; }
        }
    }
}