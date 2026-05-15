using Goap;
using Goap.ScriptableObject;
using UnityEngine;

[CreateAssetMenu(fileName = "InCover", menuName = "Scriptable Objects/GoapProposition/InCover")]
public class InCover : GoapPropositionObject
{
    public bool invert;
    public override bool IsTrue(GoapActor actor)
    {
        Actor a = actor.GetComponent<Actor>();
        if (a != null)
            return invert ? !a.IsBehindWall() : a.IsBehindWall();
        return invert ? true : false;
    }
}
