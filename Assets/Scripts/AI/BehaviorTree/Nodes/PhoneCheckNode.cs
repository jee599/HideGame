public class PhoneCheckNode : BTNode
{
    private bool _started;

    public override BTStatus Execute(CitizenAI agent)
    {
        if (agent == null || !agent.CanCheckPhone)
        {
            _started = false;
            return BTStatus.Failure;
        }

        if (!_started)
        {
            agent.BeginPhoneCheck();
            _started = true;
            return BTStatus.Running;
        }

        if (agent.IsBusy)
        {
            return BTStatus.Running;
        }

        _started = false;
        return BTStatus.Success;
    }
}
