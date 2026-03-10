using UnityEngine;
using UnityEngine.AI;

public class SpawnZone : MonoBehaviour
{
    public string zoneTag = "Home";
    public Vector3 size = new Vector3(6f, 1f, 6f);
    public Color gizmoColor = new Color(0.2f, 0.7f, 1f, 0.25f);

    public Vector3 GetRandomPoint()
    {
        var offset = new Vector3(
            Random.Range(-size.x * 0.5f, size.x * 0.5f),
            0f,
            Random.Range(-size.z * 0.5f, size.z * 0.5f));

        var target = transform.TransformPoint(offset);
        if (NavMesh.SamplePosition(target, out var hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return target;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, size);
    }
}
