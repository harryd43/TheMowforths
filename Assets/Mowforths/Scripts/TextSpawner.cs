using UnityEngine;

// Attach to agent to show floating text, called by brain
public class TextSpawner : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0);
    [SerializeField] private float displayDuration = 3f;

    private FloatingText currentText;

    public void ShowText(string text)
    {
        if (currentText != null)
        {
            Destroy(currentText.gameObject);
        }

        GameObject textObject = new GameObject("FloatingText");
        textObject.transform.position =  transform.position + offset;

        textObject.transform.SetParent(null);

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 60;
        textMesh.characterSize = 0.07f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        textMesh.fontStyle = FontStyle.Bold;

        FloatingText floatText = textObject.AddComponent<FloatingText>();
        floatText.Init(textMesh, displayDuration);

        currentText = floatText;

        TextFollower follower = textObject.AddComponent<TextFollower>();
        follower.Init(transform, offset);
    }
}
