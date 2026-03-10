using UnityEngine;

[CreateAssetMenu(menuName = "BlendIn/Events/PoliceCheckEvent")]
public class PoliceCheckEvent : GameEvent
{
    public override void OnEventTick(PlayerController player, SuspicionSystem suspicion)
    {
        if (player == null || suspicion == null)
        {
            return;
        }

        if (IsPlayerInAffectedZone(player))
        {
            suspicion.AddContinuousPenalty(20f);
        }
    }

    public override bool TryGetCitizenReaction(CitizenAI citizen, out EventReaction reaction)
    {
        reaction = new EventReaction
        {
            eventId = eventId,
            destinationTag = string.IsNullOrEmpty(reactionDestinationTag) ? "SideStreet" : reactionDestinationTag,
            hurry = true,
            waitSeconds = Mathf.Max(1.5f, citizenWaitSeconds),
            animationState = CitizenAnimationState.React
        };

        return citizen != null;
    }
}
