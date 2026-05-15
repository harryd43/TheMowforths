using Goap;
using Goap.ScriptableObject;
using UnityEngine;

[CreateAssetMenu(fileName = "UnderFire", menuName = "Scriptable Objects/GoapProposition/UnderFire")]
public class UnderFire : GoapPropositionObject
{
    public bool invert;

    public override bool IsTrue(GoapActor actor)
    {
        Actor a = actor.GetComponent<Actor>();
        if (a != null)
            return invert ? !a.IsUnderFire() : a.IsUnderFire();
        return invert ? true : false;
    }
}


