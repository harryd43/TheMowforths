using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Genies.Services.Model;

// Base for selecting the actions

public abstract class ActionBehaviour : MonoBehaviour
{
    [Header("Action")]
    [SerializeField] public string actionName = "Unnamed Action";
    [SerializeField, Range(0f, 10f)] public float weight = 5f;

    [Header("Considerations")]
    [SerializeField] public List<Consideration> considerations = new List<Consideration>();

    //For agent builder
    protected AgentBuilder agent;
    protected NavMeshAgent navMeshAgent;
    protected Transform selfTransform;

    // speed override
    public virtual float? SpeedOverride => null;

    public virtual void Initialize(AgentBuilder thisAgent)
    {
        agent = thisAgent;
        navMeshAgent = thisAgent.GetComponent<NavMeshAgent>();
        selfTransform = transform;
    }

    // called by builder
    public virtual void UpdateInputs() { }
    public abstract void Execute();
    public virtual void OnDeactivated() { }

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
            if (score <= 0f)
            {
                return 0f;
            }
        }
        return Mathf.Clamp01(score * weight / 10f);
    }

    //Sets up defaults before manual setup
    protected virtual void Reset()
    {
        SetupDefaults();
    }

    protected virtual void SetupDefaults() { }

    protected static Consideration MakeConsideration(string name, ResponseCurveType curveType, float m, float k , float b, float c, float initialInput = 0f)
    {
        return new Consideration
        {
            name = name,
            inputs = new[]
            {
                new ConsiderationInput
                {
                    inputValue = initialInput,
                    responseCurve = new ResponseCurve(curveType, m, k, b, c)
                }
            }
        };
    }

    // single consideration input actions - perf
    protected void FeedSingleConsideration(float value)
    {
        if (considerations.Count > 0 && considerations[0].inputs.Length > 0)
        {
            considerations[0].inputs[0].inputValue = value;
        }
    }



}
