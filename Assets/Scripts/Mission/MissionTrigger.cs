using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MissionTrigger : MonoBehaviour
{
    public string zoneTag = "Cafe";
    public bool countsAsShelter;

    private void Reset()
    {
        var ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            ownCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        MissionManager.Instance?.NotifyEnteredTrigger(this, player);
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        MissionManager.Instance?.NotifyExitedTrigger(this, player);
    }
}
