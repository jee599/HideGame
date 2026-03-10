using System;

public class ConditionNode : BTNode
{
    private readonly Func<CitizenAI, bool> _condition;

    public ConditionNode(Func<CitizenAI, bool> condition)
    {
        _condition = condition;
    }

    public override BTStatus Execute(CitizenAI agent)
    {
        return _condition != null && _condition(agent) ? BTStatus.Success : BTStatus.Failure;
    }
}
