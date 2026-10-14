using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "Talk";

    private DialogueComponent dialogueComponent;

    void Awake()
    {
        dialogueComponent = GetComponent<DialogueComponent>();
    }

    public string GetPrompt() => prompt;

    public void Interact(GameObject interactor)
    {
        if (dialogueComponent == null || dialogueComponent.dialogueData == null)
        {
            Debug.LogWarning($"[TestInteractable] No dialogue assigned on {name}");
            return;
        }

        DialogueSystem.Instance.HandleInteraction(dialogueComponent);
    }
}