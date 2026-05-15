using Goap;
using Goap.ScriptableObject;
using UnityEngine;

[CreateAssetMenu(fileName = "ShootAction", menuName = "Scriptable Objects/GoapActions/ShootAction")]
public class ShootAction : GoapActionObject
{
    public override string ToString() { return name; }

    public override GoapAction.State Perform(GoapActor actor)
    {
        ShootAtEnemies a = actor.GetComponent<ShootAtEnemies>();
        if (a != null)
        {
            a.Shoot();
            return GoapAction.State.Running;
        }
        return GoapAction.State.Failed;
    }
}
