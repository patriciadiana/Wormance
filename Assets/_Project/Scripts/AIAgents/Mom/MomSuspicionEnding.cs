
using System.Collections;
using UnityEngine;
using TMPro;

public class MomSuspicionEnding : MonoBehaviour
{
    [Header("Suspicion")]
    [SerializeField] private string momNpcId = "Mom";
    [SerializeField] private float suspicionLimit = 100f;

    [Header("Mom")]
    [SerializeField] private GameObject momPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private DialogueData endingDialogue;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float dialogueDistance = 3f;

    [Header("Main Door")]
    [SerializeField] private DoorInteraction mainDoor;

    [Header("Timing")]
    [SerializeField] private float arrivalDelay = 1f;

    [Header("Ending UI")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    private bool endingStarted;
    private bool dialogueStarted;

    private DialogueComponent spawnedMomDialogue;
    private GameObject spawnedMom;

    private void OnEnable()
    {
        SuspicionManager.OnSuspicionChanged += HandleSuspicionChanged;
    }

    private void OnDisable()
    {
        SuspicionManager.OnSuspicionChanged -= HandleSuspicionChanged;
    }

    private void HandleSuspicionChanged(string npcId, float suspicion)
    {
        if (npcId != momNpcId || suspicion < suspicionLimit || endingStarted)
            return;

        endingStarted = true;
        StartCoroutine(PlayEnding());
    }

    private IEnumerator PlayEnding()
    {
        GameStateMachine.Instance.SetState(GameState.Paused);

        yield return new WaitForSeconds(arrivalDelay);

        if (momPrefab == null || spawnPoint == null ||
            endingDialogue == null || player == null)
        {
            yield break;
        }

        spawnedMom = Instantiate(
            momPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        spawnedMomDialogue = spawnedMom.GetComponent<DialogueComponent>();

        if (spawnedMomDialogue == null)
            yield break;

        spawnedMomDialogue.dialogueData = endingDialogue;

        if (mainDoor != null && !mainDoor.IsOpen)
        {
            mainDoor.Interact(gameObject);
            yield return new WaitUntil(() => !mainDoor.IsMoving);
        }

        GameStateMachine.Instance.SetState(GameState.Playing);

        while (Vector3.Distance(player.position, spawnedMom.transform.position)
               > dialogueDistance)
        {
            yield return null;
        }

        if (dialogueStarted)
            yield break;

        dialogueStarted = true;

        spawnedMomDialogue.onDialogueEnded += HandleEndingDialogueFinished;

        DialogueSystem.Instance.HandleInteraction(spawnedMomDialogue);
    }

    private void HandleEndingDialogueFinished()
    {
        if (spawnedMomDialogue != null)
            spawnedMomDialogue.onDialogueEnded -= HandleEndingDialogueFinished;

        GameStateMachine.Instance.SetState(GameState.GameOver);

        if (titleText != null)
            titleText.text = "A Mother's Intuition";

        if (descriptionText != null)
        {
            descriptionText.text =
                "Your girlfriend's mother had grown increasingly suspicious of you. " +
                "Eventually, she decided to visit and see the truth for herself. " +
                "Unfortunately, she didn't like what she found. " +
                "Your relationship is destroyed.";
        }

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
        }
    }
}
