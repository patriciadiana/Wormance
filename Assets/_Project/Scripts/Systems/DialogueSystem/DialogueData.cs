using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Wormance/Dialogue")]
public class DialogueData : ScriptableObject
{
    [Header("Speaker")]
    public string npcName;
    public Sprite npcPortrait;

    [Header("Lines")]
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float autoProgressDelay = 1.5f;

    [Header("Suspicion")]
    public float[] suspicionDeltaPerLine;

    [Header("Typing")]
    public float typingSpeed = 0.05f;

    [Header("Voice")]
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    [Header("Choices")]
    public DialogueChoice[] choices;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndex;

    [Header("Suspicion")]
    public float[] suspicionDeltaPerChoice;
}