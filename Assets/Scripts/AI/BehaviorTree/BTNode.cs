public enum BTStatus
{
    Running,
    Success,
    Failure
}

public abstract class BTNode
{
    public abstract BTStatus Execute(CitizenAI agent);
}
