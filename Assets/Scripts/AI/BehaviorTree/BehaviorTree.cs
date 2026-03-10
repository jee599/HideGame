using UnityEngine;

public sealed class Selector : BTNode
{
    private readonly BTNode[] _children;

    public Selector(params BTNode[] children)
    {
        _children = children;
    }

    public override BTStatus Execute(CitizenAI agent)
    {
        for (var i = 0; i < _children.Length; i++)
        {
            var result = _children[i].Execute(agent);
            if (result == BTStatus.Success || result == BTStatus.Running)
            {
                return result;
            }
        }

        return BTStatus.Failure;
    }
}

public sealed class Sequence : BTNode
{
    private readonly BTNode[] _children;

    public Sequence(params BTNode[] children)
    {
        _children = children;
    }

    public override BTStatus Execute(CitizenAI agent)
    {
        for (var i = 0; i < _children.Length; i++)
        {
            var result = _children[i].Execute(agent);
            if (result == BTStatus.Failure)
            {
                return BTStatus.Failure;
            }

            if (result == BTStatus.Running)
            {
                return BTStatus.Running;
            }
        }

        return BTStatus.Success;
    }
}

public sealed class Inverter : BTNode
{
    private readonly BTNode _child;

    public Inverter(BTNode child)
    {
        _child = child;
    }

    public override BTStatus Execute(CitizenAI agent)
    {
        var result = _child.Execute(agent);
        return result switch
        {
            BTStatus.Success => BTStatus.Failure,
            BTStatus.Failure => BTStatus.Success,
            _ => BTStatus.Running
        };
    }
}

public sealed class RandomSelector : BTNode
{
    public readonly struct WeightedChild
    {
        public WeightedChild(BTNode node, float weight)
        {
            Node = node;
            Weight = weight;
        }

        public BTNode Node { get; }
        public float Weight { get; }
    }

    private readonly WeightedChild[] _children;

    public RandomSelector(params WeightedChild[] children)
    {
        _children = children;
    }

    public override BTStatus Execute(CitizenAI agent)
    {
        if (_children == null || _children.Length == 0)
        {
            return BTStatus.Failure;
        }

        var totalWeight = 0f;
        for (var i = 0; i < _children.Length; i++)
        {
            totalWeight += Mathf.Max(0f, _children[i].Weight);
        }

        if (totalWeight <= 0f)
        {
            return _children[Random.Range(0, _children.Length)].Node.Execute(agent);
        }

        var roll = Random.Range(0f, totalWeight);
        var runningWeight = 0f;
        for (var i = 0; i < _children.Length; i++)
        {
            runningWeight += Mathf.Max(0f, _children[i].Weight);
            if (roll <= runningWeight)
            {
                return _children[i].Node.Execute(agent);
            }
        }

        return _children[_children.Length - 1].Node.Execute(agent);
    }
}

public class BehaviorTree
{
    private readonly BTNode _root;

    public BehaviorTree(BTNode root)
    {
        _root = root;
    }

    public void Tick(CitizenAI agent)
    {
        _root?.Execute(agent);
    }
}
