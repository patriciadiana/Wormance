using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public string GetPrompt() => "Test interactable";

    public void Interact(GameObject interactor)
    {
        Debug.Log($"[TestInteractable] Interact called by {interactor.name}");
    }

}
