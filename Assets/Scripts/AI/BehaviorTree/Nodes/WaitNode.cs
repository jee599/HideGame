public class WaitNode : BTNode
{
    private bool _waiting;

    public override BTStatus Execute(CitizenAI agent)
    {
        if (agent == null || !agent.HasReachedDestination())
        {
            _waiting = false;
            return BTStatus.Failure;
        }

        if (!_waiting)
        {
            agent.BeginWait(agent.GetWaitDuration(), CitizenAnimationState.Idle);
            _waiting = true;
            return BTStatus.Running;
        }

        if (agent.IsBusy)
        {
            return BTStatus.Running;
        }

        _waiting = false;
        return BTStatus.Success;
    }
}
