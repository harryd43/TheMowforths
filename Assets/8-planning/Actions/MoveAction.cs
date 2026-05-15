using Goap;
using Goap.ScriptableObject;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveAction", menuName = "Scriptable Objects/GoapActions/MoveAction")]
public class MoveAction : GoapActionObject
{
    public enum MoveTarget { Cover, Open }

    [SerializeField] private MoveTarget target;

    public override string ToString() { return name; }

    public override GoapAction.State Perform(GoapActor actor)
    {
        MoveBetweenNodes a = actor.GetComponent<MoveBetweenNodes>();
        if (a != null)
        {
            if (target == MoveTarget.Cover)
            {
                a.MoveToNode(0);
                if (a.AtNode(0))
                    return GoapAction.State.Complete;
            }
            else if (target == MoveTarget.Open)
            {
                a.MoveToNode(1);
                if (a.AtNode(1))
                    return GoapAction.State.Complete;
            }
            return GoapAction.State.Running;
        }
        return GoapAction.State.Failed;
    }
}
