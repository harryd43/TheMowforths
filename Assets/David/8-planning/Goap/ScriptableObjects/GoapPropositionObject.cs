namespace Goap
{
    namespace ScriptableObject
    {
        public abstract class GoapPropositionObject : UnityEngine.ScriptableObject, GoapProposition
        {
            public override string ToString() { return name; }

            public abstract bool IsTrue(GoapActor actor);
        }
    }
}
