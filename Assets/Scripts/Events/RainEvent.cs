using UnityEngine;

[CreateAssetMenu(menuName = "BlendIn/Events/RainEvent")]
public class RainEvent : GameEvent
{
    public override void OnEventTick(PlayerController player, SuspicionSystem suspicion)
    {
        if (player == null || suspicion == null)
        {
            return;
        }

        if (!player.IsSheltered)
        {
            suspicion.AddContinuousPenalty(15f);
        }
    }

    public override bool TryGetCitizenReaction(CitizenAI citizen, out EventReaction reaction)
    {
        reaction = new EventReaction
        {
            eventId = eventId,
            destinationTag = string.IsNullOrEmpty(reactionDestinationTag) ? "Shelter" : reactionDestinationTag,
            hurry = true,
            waitSeconds = Mathf.Max(2f, citizenWaitSeconds),
            animationState = CitizenAnimationState.React
        };

        return citizen != null;
    }
}
