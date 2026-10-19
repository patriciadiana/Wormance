using UnityEngine;

public class NeighbourNPC : MonoBehaviour, IInteractable
{
    private DialogueComponent dialogueComponent;
    private GameObject presentToGive;

    private void Awake()
    {
        dialogueComponent = GetComponent<DialogueComponent>();
    }

    public void Setup(DialogueData data, GameObject present)
    {
        dialogueComponent.dialogueData = data;
        presentToGive = present;

        dialogueComponent.onDialogueEnded = HandleDialogueEnded;
    }

    void HandleDialogueEnded()
    {
        if (presentToGive != null)
        {
            Debug.Log($"[Neighbour] Gave present: {presentToGive.name}");
        }

        Destroy(gameObject);
    }

    public string GetPrompt()
    {
        return "Talk to Neighbour";
    }

    public void Interact(GameObject interactor)
    {
        if (dialogueComponent != null && dialogueComponent.dialogueData != null)
        {
            DialogueSystem.Instance.HandleInteraction(dialogueComponent);
        }
    }
}
