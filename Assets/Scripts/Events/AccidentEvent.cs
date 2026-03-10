using UnityEngine;

[CreateAssetMenu(menuName = "BlendIn/Events/AccidentEvent")]
public class AccidentEvent : GameEvent
{
    public override bool TryGetCitizenReaction(CitizenAI citizen, out EventReaction reaction)
    {
        reaction = new EventReaction
        {
            eventId = eventId,
            destinationTag = string.IsNullOrEmpty(reactionDestinationTag) ? "Accident" : reactionDestinationTag,
            hurry = false,
            waitSeconds = Mathf.Max(2f, citizenWaitSeconds),
            animationState = CitizenAnimationState.React
        };

        return citizen != null;
    }
}
