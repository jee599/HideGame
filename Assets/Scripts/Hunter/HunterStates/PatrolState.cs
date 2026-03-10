public class PatrolState : IHunterState
{
    public void Enter(HunterAI hunter)
    {
        hunter.ApplySpeed(hunter.config != null ? hunter.config.patrolSpeed : 3f);
    }

    public void Tick(HunterAI hunter)
    {
        hunter.MoveAlongPatrol();

        var suspicion = hunter.Suspicion != null ? hunter.Suspicion.suspicion : 0f;
        if (suspicion >= 100f)
        {
            hunter.ChangeState(HunterState.Lockdown);
            return;
        }

        if (suspicion >= 70f)
        {
            hunter.ChangeState(HunterState.Chase);
            return;
        }

        if (suspicion >= 40f || hunter.SeesPlayer())
        {
            hunter.ChangeState(HunterState.Investigate);
        }
    }

    public void Exit(HunterAI hunter)
    {
    }
}
