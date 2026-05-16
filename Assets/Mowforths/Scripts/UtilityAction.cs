using UnityEngine;
using System.Collections.Generic;

// Decides final action score based on the scores of its considerations, and how much to care about it

public enum ActionType
{
    // Movement
    MoveToTarget,
    Idle,

    // Macca
    SmashObstacle,

    // Shane
    ThrowBomb,
    PickFight,

    // Sarah
    StealthMode,
    CallPolice,

    // Ashley
    GrabTarget,
    GetLostAndWander,

    // Christine
    TalkToGhost,
}

[System.Serializable]
public class UtilityAction
{
    public string name;

    public ActionType actionType;

    public List<Consideration> considerations = new List<Consideration>();

    [Range(0f, 10f)] // weighting factor for how much to care about this action compared to others
    public float weight = 1f;

    public float CalculateUtility()
    {

        if (considerations == null || considerations.Count == 0)
        {
            return 0f;
        }

        float score = 1f;

        foreach (Consideration consideration in considerations)
        {
            score *= consideration.Evaluate();

            if (score <= 0)
            {
                return 0f; // if any consideration is 0 or less, the whole action is not viable
            }
        }

        return Mathf.Clamp01(score * weight / 10f); // normalize score and apply weight
    }
}