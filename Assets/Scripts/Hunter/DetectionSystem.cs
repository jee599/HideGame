using UnityEngine;

public class DetectionSystem : MonoBehaviour
{
    public Transform viewOrigin;
    public LayerMask obstructionMask = ~0;

    private HunterAI _hunter;

    private void Awake()
    {
        _hunter = GetComponent<HunterAI>();
    }

    public bool CanSeePlayer(PlayerController player)
    {
        if (player == null || _hunter == null || _hunter.config == null)
        {
            return false;
        }

        var origin = viewOrigin != null ? viewOrigin.position : transform.position + Vector3.up * 1.6f;
        var targetPosition = player.transform.position + Vector3.up * 1.2f;
        var toPlayer = targetPosition - origin;
        var distance = toPlayer.magnitude;
        if (distance > _hunter.GetCurrentViewRange())
        {
            return false;
        }

        if (_hunter.config.viewAngle < 360f)
        {
            var angle = Vector3.Angle(transform.forward, toPlayer);
            if (angle > _hunter.config.viewAngle * 0.5f)
            {
                return false;
            }
        }

        if (Physics.Raycast(origin, toPlayer.normalized, out var hit, distance, obstructionMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform != player.transform && !hit.transform.IsChildOf(player.transform))
            {
                return false;
            }
        }

        return true;
    }

    public float DistanceToPlayer(PlayerController player)
    {
        return player == null ? float.MaxValue : Vector3.Distance(transform.position, player.transform.position);
    }
}
