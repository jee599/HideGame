public class ReactToEventNode : BTNode
{
    public override BTStatus Execute(CitizenAI agent)
    {
        if (agent == null || !agent.TryGetActiveEventReaction(out var reaction))
        {
            return BTStatus.Failure;
        }

        return agent.TickEventReaction(reaction);
    }
}
