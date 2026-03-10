using UnityEngine;

[CreateAssetMenu(menuName = "BlendIn/Events/DeliveryRushEvent")]
public class DeliveryRushEvent : GameEvent
{
    public override bool TryGetCitizenReaction(CitizenAI citizen, out EventReaction reaction)
    {
        if (citizen == null
            || citizen.Archetype == null
            || citizen.Archetype.behaviorPreset != CitizenBehaviorPreset.DeliveryDriver)
        {
            reaction = default;
            return false;
        }

        reaction = new EventReaction
        {
            eventId = eventId,
            destinationTag = citizen.CurrentScheduleOption.destinationTag,
            hurry = true,
            waitSeconds = Mathf.Max(0.5f, citizenWaitSeconds),
            animationState = CitizenAnimationState.Walk
        };

        return true;
    }
}
