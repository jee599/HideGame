using UnityEngine;

[CreateAssetMenu(menuName = "BlendIn/Events/BlackoutEvent")]
public class BlackoutEvent : GameEvent
{
    public override void OnEventTick(PlayerController player, SuspicionSystem suspicion)
    {
        if (player == null || suspicion == null)
        {
            return;
        }

        if (!player.IsInCrowd && !player.IsSheltered)
        {
            suspicion.AddContinuousPenalty(5f);
        }
    }

    public override bool TryGetCitizenReaction(CitizenAI citizen, out EventReaction reaction)
    {
        reaction = new EventReaction
        {
            eventId = eventId,
            destinationTag = string.IsNullOrEmpty(reactionDestinationTag) ? "Exit" : reactionDestinationTag,
            hurry = true,
            waitSeconds = Mathf.Max(1f, citizenWaitSeconds),
            animationState = CitizenAnimationState.React
        };

        return citizen != null;
    }
}
