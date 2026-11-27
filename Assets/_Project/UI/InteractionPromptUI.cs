using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;

    private IInteractable currentTarget;

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
        currentTarget = target;
        Refresh();
    }

    void Update()
    {
        if (currentTarget != null)
            Refresh();
    }

    private void Refresh()
    {
        if (currentTarget == null)
        {
            Hide();
            return;
        }

        string prompt = currentTarget.GetPrompt();
        if (string.IsNullOrEmpty(prompt))
        {
            Hide();
            return;
        }

        promptText.text = $"Press [E] to {prompt}";
        promptRoot.SetActive(true);
    }

    private void Hide()
    {
        promptText.text = string.Empty;
        promptRoot.SetActive(false);
    }
}