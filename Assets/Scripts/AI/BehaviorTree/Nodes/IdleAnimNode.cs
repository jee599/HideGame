public class IdleAnimNode : BTNode
{
    public override BTStatus Execute(CitizenAI agent)
    {
        if (agent == null)
        {
            return BTStatus.Failure;
        }

        agent.SetAnimationState(CitizenAnimationState.Idle);
        return BTStatus.Running;
    }
}
