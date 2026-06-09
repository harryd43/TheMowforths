using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

// Bundles values from response curve for a consideration on wether to take action or not, and how much to care about it
// Input source defined per agent

[System.Serializable]
public struct ConsiderationInput
{
    public float inputValue; // Normalized input value (0-1) for the consideration

    public ResponseCurve responseCurve; 
}

[System.Serializable]
public class Consideration
{
    public string name;

    public ConsiderationInput[] inputs;

    public float Evaluate()
    {
        if (inputs == null || inputs.Length == 0)
        {
            return 0f;
        }

        float score = 1f;

        foreach (ConsiderationInput input in inputs)
        {
            float curveScore = input.responseCurve.Evaluate(input.inputValue);
            
            score*= curveScore; // Multiply scores together for all inputs

            if (score <= 0f)
            {
                return 0f;
            }
        }

        // Account for scores with more inputs being unfairly low - Dave Mark talk GDC
        int count = inputs.Length;
        float modFactor = 1f - (1f / count);

        // Average of all scores
        float total = 0f;
        foreach (ConsiderationInput input in inputs)
        {
            total += input.responseCurve.Evaluate(input.inputValue);
        }
        float average = total / count;

        float compensatedValue = score + (modFactor * (average - score));

        return Mathf.Clamp01(compensatedValue);
    }
}


