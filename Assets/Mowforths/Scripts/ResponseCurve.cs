using UnityEngine;

// Implements of different response curve types, mappiong normalized input (0-1) to output (0-1) for modelling how much an agent cares about x values
// Bends number into score
public enum ResponseCurveType
{
    Linear,
    Quadratic,
    Exponential,
    Logistic,
    Logit
}

[System.Serializable]
public struct ResponseCurve
{
    public ResponseCurveType curveType;
    public float m; // slope
    public float k; // bend
    public float b; // vert shift
    public float c; // hor shift

    // Constructor for curve creation
    public ResponseCurve(ResponseCurveType type, float m, float k, float b, float c)
    {
        this.curveType = type;
        this.m = m;
        this.k = k;
        this.b = b;
        this.c = c;
    }

    public float Evaluate(float x)
    {
        x = Mathf.Clamp01(x); // Ensure x is between 0 and 1

        float y;

        switch (curveType)
        {
            case ResponseCurveType.Linear:
                y = m * (x - c) + b; // x, y same change
                break;

            case ResponseCurveType.Quadratic:
                y = m * Mathf.Pow(x - c, k) + b; // x changes make bigger / smaller y change
                break;

            case ResponseCurveType.Exponential:
                y = Mathf.Pow(k, m * (x - c)) + b; // up up up
                break;

            case ResponseCurveType.Logistic:
                y = k / (1f + 1000f * Mathf.Exp(m * (x - c))) + b; // sigmoid
                break;

            case ResponseCurveType.Logit:
                float inner = m * (x - c);
                inner = Mathf.Clamp(inner, 0.001f, 0.999f);
                y = k * Mathf.Log(inner / (1f - inner), 100f) + b; // inverse sigmoid
                break;

            default:
                y = x; // Default to linear if type is unrecognized
                break;
        }
        return Mathf.Clamp01(y); // Ensure output is between 0 and 1
    }
}
