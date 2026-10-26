using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUIController : Singleton<DialogueUIController>
{
    [Header("Dialogue Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image portraitImage;

    [Header("Choices")]
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;
    public bool HasActiveChoices => choiceContainer.childCount > 0;

    public void ShowDialogueUI(bool show)
    {
        dialoguePanel.SetActive(show);
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        nameText.text = npcName;
        portraitImage.sprite = portrait;
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    public void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
    }

    public void CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        choiceButton.GetComponentInChildren<TextMeshProUGUI>().text = choiceText;

        Button btn = choiceButton.GetComponent<Button>();
        btn.onClick.AddListener(() => {
            onClick?.Invoke();
        });
    }

}
