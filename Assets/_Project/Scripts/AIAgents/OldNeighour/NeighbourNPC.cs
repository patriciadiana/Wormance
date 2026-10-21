using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(DialogueComponent))]
[RequireComponent(typeof(NavMeshAgent))]
public class NeighbourNPC : MonoBehaviour, IInteractable
{
    private DialogueComponent dialogueComponent;
    private NavMeshAgent agent;
    private GameObject presentToGive;
    private Transform goHomePoint;

    private void Awake()
    {
        dialogueComponent = GetComponent<DialogueComponent>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Setup(DialogueData data, GameObject present, Transform doorDestination)
    {
        dialogueComponent.dialogueData = data;
        presentToGive = present;

        dialogueComponent.onDialogueEnded = HandleDialogueEnded;

        agent.SetDestination(doorDestination.position);
    }

    void HandleDialogueEnded()
    {
        if (presentToGive != null)
        {
            Debug.Log($"[Neighbour] Gave present: {presentToGive.name}");
        }

        FindAnyObjectByType<NeighbourSpawner>()?.MarkAsMet();

        StartCoroutine(WalkAwayAndLeave());
    }

    IEnumerator WalkAwayAndLeave()
    {
        GetComponent<Collider>().enabled = false;

        Vector3 homeDirection = -transform.forward;
        Vector3 goAwayPoint = transform.position + (homeDirection * 20f);

        if (NavMesh.SamplePosition(goAwayPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        float timeout = 5f;
        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0) break;
            yield return null;
        }

        Destroy(gameObject);
    }

    public string GetPrompt()
    {
        return "Talk to Neighbour";
    }

    public void Interact(GameObject interactor)
    {
        if (dialogueComponent != null && dialogueComponent.dialogueData != null)
        {
            DialogueSystem.Instance.HandleInteraction(dialogueComponent);
        }
    }
}
