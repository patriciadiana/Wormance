using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private GameObject promptRoot;  
    [SerializeField] private TMP_Text promptText; 

    void OnEnable()
    {
        interactor.OnTargetChanged += HandleTargetChanged;
    }

    void OnDisable()
    {
        interactor.OnTargetChanged -= HandleTargetChanged;
    }

    void Start()
    {
        promptRoot.SetActive(false);
    }

    void HandleTargetChanged(IInteractable target)
    {
        if (target == null)
        {
            promptRoot.SetActive(false);
            return;
        }

        promptText.text = $"[E] {target.GetPrompt()}";
        promptRoot.SetActive(true);
    }
}
