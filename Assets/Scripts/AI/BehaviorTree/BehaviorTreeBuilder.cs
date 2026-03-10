public static class BehaviorTreeBuilder
{
    public static BehaviorTree Build(CitizenAI agent)
    {
        return agent != null && agent.Archetype != null
            ? agent.Archetype.behaviorPreset switch
            {
                CitizenBehaviorPreset.DeliveryDriver => BuildDeliveryRoutine(),
                CitizenBehaviorPreset.Guard => BuildGuardRoutine(),
                CitizenBehaviorPreset.Couple => BuildSocialRoutine(includePhone: false),
                CitizenBehaviorPreset.Elder => BuildSocialRoutine(includePhone: false),
                CitizenBehaviorPreset.StreetVendor => BuildVendorRoutine(),
                _ => BuildSocialRoutine(includePhone: true)
            }
            : BuildFallbackRoutine();
    }

    private static BehaviorTree BuildSocialRoutine(bool includePhone)
    {
        var socialSequence = new Sequence(
            new ConditionNode(a => a.CanSocialize && a.TryGetNearbyRelatedCitizen(out _)),
            new ProbabilityGateNode(a => a.Personality != null ? a.Personality.sociability : 0f),
            new SocialInteractNode());

        var scheduleSequence = new Sequence(
            new ScheduleNode(),
            new MoveToNode(),
            new WaitNode());

        if (includePhone)
        {
            var phoneSequence = new Sequence(
                new ConditionNode(a => a.CanCheckPhone),
                new ProbabilityGateNode(a => a.Personality != null ? a.Personality.phoneAddiction : 0f),
                new PhoneCheckNode());

            return new BehaviorTree(new Selector(
                new Sequence(new ConditionNode(a => a.TryGetActiveEventReaction(out _)), new ReactToEventNode()),
                socialSequence,
                phoneSequence,
                scheduleSequence,
                new IdleAnimNode()));
        }

        return new BehaviorTree(new Selector(
            new Sequence(new ConditionNode(a => a.TryGetActiveEventReaction(out _)), new ReactToEventNode()),
            socialSequence,
            scheduleSequence,
            new IdleAnimNode()));
    }

    private static BehaviorTree BuildDeliveryRoutine()
    {
        return new BehaviorTree(new Selector(
            new Sequence(new ConditionNode(a => a.TryGetActiveEventReaction(out _)), new ReactToEventNode()),
            new Sequence(new ScheduleNode(), new MoveToNode(), new WaitNode()),
            new IdleAnimNode()));
    }

    private static BehaviorTree BuildGuardRoutine()
    {
        return new BehaviorTree(new Selector(
            new Sequence(new ConditionNode(a => a.TryGetActiveEventReaction(out _)), new ReactToEventNode()),
            new Sequence(
                new ConditionNode(a => a.CanSocialize && a.TryGetNearbyRelatedCitizen(out _)),
                new ProbabilityGateNode(a => a.Personality != null ? a.Personality.sociability * 0.5f : 0f),
                new SocialInteractNode()),
            new Sequence(new ScheduleNode(), new MoveToNode(), new WaitNode()),
            new IdleAnimNode()));
    }

    private static BehaviorTree BuildVendorRoutine()
    {
        return new BehaviorTree(new Selector(
            new Sequence(new ConditionNode(a => a.TryGetActiveEventReaction(out _)), new ReactToEventNode()),
            new Sequence(new ScheduleNode(), new MoveToNode(), new WaitNode()),
            new Sequence(
                new ConditionNode(a => a.CanSocialize && a.TryGetNearbyRelatedCitizen(out _)),
                new ProbabilityGateNode(a => a.Personality != null ? a.Personality.sociability : 0f),
                new SocialInteractNode()),
            new IdleAnimNode()));
    }

    private static BehaviorTree BuildFallbackRoutine()
    {
        return new BehaviorTree(new IdleAnimNode());
    }
}
