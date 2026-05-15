using Goap;
using Goap.ScriptableObject;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAlive", menuName = "Scriptable Objects/GoapProposition/EnemyAlive")]
public class EnemyAlive : GoapPropositionObject
{
    public bool invert;

    public override bool IsTrue(GoapActor actor)
    {
        TrackEnemies a = actor.GetComponent<TrackEnemies>();
        if (a != null)
            return invert ? ! a.HasEnemy() : a.HasEnemy();
        return invert ? true : false;
    }
}


