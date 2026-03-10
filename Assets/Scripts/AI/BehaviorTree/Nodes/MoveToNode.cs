public class MoveToNode : BTNode
{
    public override BTStatus Execute(CitizenAI agent)
    {
        if (agent == null || !agent.MoveToScheduleDestination())
        {
            return BTStatus.Failure;
        }

        return agent.HasReachedDestination() ? BTStatus.Success : BTStatus.Running;
    }
}
