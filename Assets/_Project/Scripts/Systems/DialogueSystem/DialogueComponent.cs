using UnityEngine;

public class DialogueComponent : MonoBehaviour
{
    [Header("Data")]
    public DialogueData dialogueData;

    [Header("Not editable")]
    public int dialogueIndex;
    public bool isTyping;
    public bool isDialogueActive;
}