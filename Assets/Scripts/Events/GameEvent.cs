using UnityEngine;

public struct EventReaction
{
    public string eventId;
    public string destinationTag;
    public bool hurry;
    public float waitSeconds;
    public CitizenAnimationState animationState;
}

public abstract class GameEvent : ScriptableObject
{
    public string eventId = "Event";
    public string displayName = "Event";
    [Min(1f)] public float duration = 20f;
    public string[] affectedZoneTags;
    public string reactionDestinationTag = string.Empty;
    public bool citizensHurry;
    [Min(0f)] public float citizenWaitSeconds = 2f;

    public virtual void OnEventStart()
    {
    }

    public virtual void OnEventEnd()
    {
    }

    public virtual void OnEventTick(PlayerController player, SuspicionSystem suspicion)
    {
    }

    public virtual bool TryGetCitizenReaction(CitizenAI citizen, out EventReaction reaction)
    {
        reaction = default;
        if (citizen == null)
        {
            return false;
        }

        reaction = new EventReaction
        {
            eventId = eventId,
            destinationTag = reactionDestinationTag,
            hurry = citizensHurry,
            waitSeconds = citizenWaitSeconds,
            animationState = CitizenAnimationState.React
        };

        return true;
    }

    protected bool IsPlayerInAffectedZone(PlayerController player)
    {
        if (player == null || affectedZoneTags == null || affectedZoneTags.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < affectedZoneTags.Length; i++)
        {
            if (string.Equals(player.CurrentZoneTag, affectedZoneTags[i]))
            {
                return true;
            }
        }

        return false;
    }
}
