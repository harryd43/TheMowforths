using UnityEngine;

[CreateAssetMenu(fileName = "TeamInfo", menuName = "Scriptable Objects/TeamInfo")]
public class TeamInfo : ScriptableObject
{
    [SerializeField] private Material material;
    public Material GetMaterial() { return material; }
}
