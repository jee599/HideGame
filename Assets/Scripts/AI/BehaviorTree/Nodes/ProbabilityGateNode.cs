using System;
using UnityEngine;

public class ProbabilityGateNode : BTNode
{
    private readonly Func<CitizenAI, float> _chanceProvider;

    public ProbabilityGateNode(Func<CitizenAI, float> chanceProvider)
    {
        _chanceProvider = chanceProvider;
    }

    public override BTStatus Execute(CitizenAI agent)
    {
        if (_chanceProvider == null)
        {
            return BTStatus.Failure;
        }

        var chance = Mathf.Clamp01(_chanceProvider(agent));
        return UnityEngine.Random.value <= chance ? BTStatus.Success : BTStatus.Failure;
    }
}
