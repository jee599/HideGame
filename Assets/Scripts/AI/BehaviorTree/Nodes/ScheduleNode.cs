public class ScheduleNode : BTNode
{
    public override BTStatus Execute(CitizenAI agent)
    {
        return agent != null && agent.RefreshSchedule() ? BTStatus.Success : BTStatus.Failure;
    }
}
