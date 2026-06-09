using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PartyCard : MonoBehaviour
{
    [SerializeField] public GameObject selectedCard;
    [SerializeField] public AgentController agentController;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Image cardPicture;
    [SerializeField] private UnityEngine.UI.Slider healthSlider;

    private static PartyCard currentlySelected;
    public void SelectCard()
    {
        if (agentController != null)
        {
            if (currentlySelected != null)
            {
                currentlySelected.selectedCard.SetActive(false);
            }

            currentlySelected = this;
            selectedCard.SetActive(true);

            playerInput.SelectAgent(agentController);
        }
        
    }
    void Update()
    {
        if (agentController == null)
        {
            cardPicture.color = new Color(1f, 0.3f, 0.3f, 1f);
            selectedCard.SetActive(false);
        }
        
        cardPicture.color = new Color(1f, 1f, 1f, 1f);

        MowforthHealth health = agentController.GetComponent<MowforthHealth>();
        if (health != null && healthSlider != null)
        {
            healthSlider.value = health.currentHealth / health.maxHealth;
        }
        
    }

}
