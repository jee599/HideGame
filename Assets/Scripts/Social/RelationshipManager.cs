using System.Collections.Generic;
using UnityEngine;

public class RelationshipManager : MonoBehaviour
{
    public static RelationshipManager Instance { get; private set; }

    [Header("Relationship Counts")]
    [Range(20, 30)] public int pairCount = 24;
    [Range(0f, 1f)] public float coupleChance = 0.15f;
    [Range(0f, 1f)] public float colleagueChance = 0.35f;
    [Range(0f, 1f)] public float conflictChance = 0.2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void GenerateRelationships()
    {
        var citizens = CitizenAI.GetAllCitizens();
        if (citizens == null || citizens.Count < 2)
        {
            return;
        }

        ClearAllRelationships(citizens);

        var eligibleCitizens = new List<CitizenAI>(citizens.Count);
        for (var i = 0; i < citizens.Count; i++)
        {
            var citizen = citizens[i];
            if (citizen == null)
            {
                continue;
            }

            if (citizen.Archetype != null && citizen.Archetype.behaviorPreset == CitizenBehaviorPreset.Couple)
            {
                continue;
            }

            eligibleCitizens.Add(citizen);
        }

        Shuffle(eligibleCitizens);

        var pairBudget = Mathf.Min(pairCount, eligibleCitizens.Count / 2);
        for (var pairIndex = 0; pairIndex < pairBudget; pairIndex++)
        {
            var a = eligibleCitizens[pairIndex * 2];
            var b = eligibleCitizens[pairIndex * 2 + 1];
            if (a == null || b == null || a == b || AlreadyLinked(a, b))
            {
                continue;
            }

            var type = RollType();
            a.AddRelationship(b, type);
            b.AddRelationship(a, type);
        }
    }

    private static void ClearAllRelationships(IReadOnlyList<CitizenAI> citizens)
    {
        for (var i = 0; i < citizens.Count; i++)
        {
            citizens[i]?.ClearRelationships();
        }
    }

    private static bool AlreadyLinked(CitizenAI a, CitizenAI b)
    {
        var links = a.Relationships;
        for (var i = 0; i < links.Count; i++)
        {
            if (links[i].Other == b)
            {
                return true;
            }
        }

        return false;
    }

    private RelationshipType RollType()
    {
        var roll = Random.value;
        if (roll <= coupleChance)
        {
            return RelationshipType.Couple;
        }

        if (roll <= coupleChance + colleagueChance)
        {
            return RelationshipType.Colleague;
        }

        if (roll <= coupleChance + colleagueChance + conflictChance)
        {
            return RelationshipType.Conflict;
        }

        return RelationshipType.Acquaintance;
    }

    private static void Shuffle(List<CitizenAI> citizens)
    {
        for (var i = citizens.Count - 1; i > 0; i--)
        {
            var swapIndex = Random.Range(0, i + 1);
            (citizens[i], citizens[swapIndex]) = (citizens[swapIndex], citizens[i]);
        }
    }
}
