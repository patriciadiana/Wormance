using System.Collections;
using UnityEngine;

public class FreakEnding : MonoBehaviour
{
    [Header("Ending 1: stolen at full suspicion")]
    [SerializeField] private string suspicionTitle = "Too Suspicious";
    [TextArea(3, 6)]
    [SerializeField] private string suspicionDescription = "Jeremy saw right through you. He took the worm and was gone before you could stop him.";

    [Header("Ending 2: stolen after the player's choices")]
    [SerializeField] private string choiceTitle = "A Fair Deal";
    [TextArea(3, 6)]
    [SerializeField] private string choiceDescription = "You made your choices, and Jeremy kept his end of the deal. He left with the worm.";

    [Header("Settings")]
    /* Short pause so the player sees the worm get taken before the panel appears */
    [SerializeField] private float delayBeforeEnding = 1.5f;

    private bool triggered;

    // Jeremy stole the worm because suspicion was full
    public void TriggerSuspicionEnding()
    {
        StartEnding(suspicionTitle, suspicionDescription);
    }

    // Jeremy stole the worm because of the player's choices (JeremyEnding dialogue)
    public void TriggerChoiceEnding()
    {
        StartEnding(choiceTitle, choiceDescription);
    }

    private void StartEnding(string title, string description)
    {
        if (triggered) return; // only one ending per game
        triggered = true;

        StartCoroutine(ShowEndingAfterDelay(title, description));
    }

    private IEnumerator ShowEndingAfterDelay(string title, string description)
    {
        yield return new WaitForSeconds(delayBeforeEnding);

        // EndingScreenUI listens to this and shows its panel
        GameEvents.RaiseGameEnded(title, description);
    }
}