using System.Collections.Generic;
using UnityEngine;

public class DestinationPoint : MonoBehaviour
{
    private static readonly List<DestinationPoint> RegisteredPoints = new List<DestinationPoint>();

    public string zoneTag = "Office";
    public int capacity = 5;
    public bool countsAsShelter;
    public bool countsAsCrowdZone;
    public Transform[] standPoints;
    public Transform[] sitPoints;

    private void OnEnable()
    {
        if (!RegisteredPoints.Contains(this))
        {
            RegisteredPoints.Add(this);
        }
    }

    private void OnDisable()
    {
        RegisteredPoints.Remove(this);
    }

    public Vector3 GetBestPoint(Vector3 origin, bool preferSit = false)
    {
        var pool = preferSit && sitPoints != null && sitPoints.Length > 0 ? sitPoints : standPoints;
        if (pool == null || pool.Length == 0)
        {
            return transform.position;
        }

        Transform best = pool[0];
        var bestDistance = Vector3.SqrMagnitude(origin - best.position);

        for (var i = 1; i < pool.Length; i++)
        {
            var candidate = pool[i];
            if (candidate == null)
            {
                continue;
            }

            var candidateDistance = Vector3.SqrMagnitude(origin - candidate.position);
            if (candidateDistance < bestDistance)
            {
                best = candidate;
                bestDistance = candidateDistance;
            }
        }

        return best != null ? best.position : transform.position;
    }

    public static DestinationPoint GetClosest(string requestedZoneTag, Vector3 origin)
    {
        DestinationPoint best = null;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < RegisteredPoints.Count; i++)
        {
            var point = RegisteredPoints[i];
            if (point == null || !string.Equals(point.zoneTag, requestedZoneTag))
            {
                continue;
            }

            var distance = Vector3.SqrMagnitude(point.transform.position - origin);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = point;
            }
        }

        return best;
    }

    public static DestinationPoint GetNearest(Vector3 origin, float maxDistance)
    {
        DestinationPoint best = null;
        var bestDistance = maxDistance * maxDistance;

        for (var i = 0; i < RegisteredPoints.Count; i++)
        {
            var point = RegisteredPoints[i];
            if (point == null)
            {
                continue;
            }

            var distance = Vector3.SqrMagnitude(point.transform.position - origin);
            if (distance < bestDistance)
            {
                best = point;
                bestDistance = distance;
            }
        }

        return best;
    }
}
