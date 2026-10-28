using System.Collections;
using UnityEngine;

public class DialogueSystem : Singleton<DialogueSystem>
{
    [SerializeField] private PlayerInputHandler inputHandler;
    private DialogueUIController ui;
    private DialogueComponent activeDialogue;

    protected override void Awake()
    {
        base.Awake();
        ui = DialogueUIController.Instance;
    }

    void Update()
    {
        if (!IsDialogueActive()) return;
        if (inputHandler == null) return;

        if (inputHandler.InteractPressed)
        {
            inputHandler.ConsumeInteract();
            OnDialogueTap();
        }
    }

    public void OnDialogueTap()
    {
        if (activeDialogue == null) return;
        if (ui.HasActiveChoices) return;
        NextLine();
    }

    public void HandleInteraction(DialogueComponent dialogue)
    {
        if (activeDialogue != null && activeDialogue != dialogue)
            return;

        if (dialogue.isDialogueActive)
            NextLine();
        else
            StartDialogue(dialogue);
    }

    public void StartDialogue(DialogueComponent dialogue)
    {
        if (activeDialogue != null) return;
        activeDialogue = dialogue;

        GameStateMachine.Instance.SetState(GameState.Dialogue);

        dialogue.isDialogueActive = true;
        dialogue.dialogueIndex = 0;

        ui.SetNPCInfo(dialogue.dialogueData.npcName, dialogue.dialogueData.npcPortrait);
        ui.ShowDialogueUI(true);

        SuspicionBarUI[] allBars = FindObjectsByType<SuspicionBarUI>();

        foreach (var bar in allBars)
        {
            bar.ShowBar(false); // hide all first

            if (bar.targetNpcId == dialogue.dialogueData.npcId)
            {
                bar.SetTargetNPC(dialogue.dialogueData.npcId);
                bar.ShowBar(true);
            }
        }

        DisplayCurrentLine();
    }

    void NextLine()
    {
        if (activeDialogue == null) return;

        var dialogue = activeDialogue;

        if (dialogue.isTyping)
        {
            StopAllCoroutines();
            ui.SetDialogueText(dialogue.dialogueData.dialogueLines[dialogue.dialogueIndex]);
            dialogue.isTyping = false;
            return;
        }

        ui.ClearChoices();

        if (dialogue.dialogueData.endDialogueLines.Length > dialogue.dialogueIndex &&
            dialogue.dialogueData.endDialogueLines[dialogue.dialogueIndex])
        {
            EndDialogue();
            return;
        }

        foreach (DialogueChoice choice in dialogue.dialogueData.choices)
        {
            if (choice.dialogueIndex == dialogue.dialogueIndex)
            {
                DisplayChoice(choice);
                return;
            }
        }

        dialogue.dialogueIndex++;

        if (dialogue.dialogueIndex < dialogue.dialogueData.dialogueLines.Length)
            DisplayCurrentLine();
        else
            EndDialogue();
    }

    void DisplayCurrentLine()
    {
        if (activeDialogue == null) return;

        ApplyLineEffects(activeDialogue.dialogueIndex);

        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    void ApplyLineEffects(int index)
    {
        var data = activeDialogue.dialogueData;

        if (data.suspicionDeltaPerLine == null) return;
        if (index < 0 || index >= data.suspicionDeltaPerLine.Length) return;

        float delta = data.suspicionDeltaPerLine[index];
        if (Mathf.Approximately(delta, 0f)) return;

        SuspicionManager.Instance.ModifySuspicion(data.npcId, delta);
    }

    IEnumerator TypeLine()
    {
        var dialogue = activeDialogue;
        dialogue.isTyping = true;
        ui.SetDialogueText("");

        string line = dialogue.dialogueData.dialogueLines[dialogue.dialogueIndex];

        foreach (char c in line)
        {
            ui.SetDialogueText(ui.dialogueText.text += c);

            //TODO for when we have audio
            //if (dialogue.dialogueData.voiceSound != null)
                //SoundManager.PlayVoice(dialogue.dialogueData.voiceSound, dialogue.dialogueData.voicePitch);

            yield return new WaitForSeconds(dialogue.dialogueData.typingSpeed);
        }

        dialogue.isTyping = false;

        if (dialogue.dialogueData.autoProgressLines.Length > dialogue.dialogueIndex &&
            dialogue.dialogueData.autoProgressLines[dialogue.dialogueIndex])
        {
            yield return new WaitForSeconds(dialogue.dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void ApplyChoiceEffects(int dialogueIndex, int choiceIndex)
    {
        var data = activeDialogue.dialogueData;

        foreach (var choice in data.choices)
        {
            if (choice.dialogueIndex != dialogueIndex) continue;
            if (choice.suspicionDeltaPerChoice == null) continue;
            if (choiceIndex < 0 || choiceIndex >= choice.suspicionDeltaPerChoice.Length) continue;

            float delta = choice.suspicionDeltaPerChoice[choiceIndex];
            if (Mathf.Approximately(delta, 0f)) continue;

            SuspicionManager.Instance.ModifySuspicion(data.npcId, delta);
        }
    }

    void ChooseOption(int nextIndex, int choiceIndex)
    {
        if (activeDialogue == null) return;

        ApplyChoiceEffects(activeDialogue.dialogueIndex, choiceIndex);

        activeDialogue.dialogueIndex = nextIndex;
        ui.ClearChoices();
        DisplayCurrentLine();
    }

    void DisplayChoice(DialogueChoice choice)
    {
        var dialogue = activeDialogue;

        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndex[i];
            int choiceIndex = i;
            ui.CreateChoiceButton(
                choice.choices[i],
                () => ChooseOption(nextIndex, choiceIndex)
            );
        }
    }

    public void EndDialogue()
    {
        if (activeDialogue == null) return;

        StopAllCoroutines();

        var endedDialogue = activeDialogue;
        activeDialogue = null;

        ui.SetDialogueText("");
        ui.ShowDialogueUI(false);

        SuspicionBarUI[] allBars = FindObjectsByType<SuspicionBarUI>();
        foreach (var bar in allBars)
        {
            bar.ShowBar(false);
        }

        GameStateMachine.Instance.SetState(GameState.Playing);

        endedDialogue.isDialogueActive = false;
        endedDialogue.onDialogueEnded?.Invoke();
    }

    public bool IsDialogueActive() => activeDialogue != null;
}