using UnityEngine;

[CreateAssetMenu(menuName = "BlendIn/Events/LunchSaleEvent")]
public class LunchSaleEvent : GameEvent
{
    public override bool TryGetCitizenReaction(CitizenAI citizen, out EventReaction reaction)
    {
        reaction = new EventReaction
        {
            eventId = eventId,
            destinationTag = string.IsNullOrEmpty(reactionDestinationTag) ? "Restaurant" : reactionDestinationTag,
            hurry = false,
            waitSeconds = Mathf.Max(4f, citizenWaitSeconds),
            animationState = CitizenAnimationState.React
        };

        return citizen != null;
    }
}
