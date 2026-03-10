public class ChaseState : IHunterState
{
    public void Enter(HunterAI hunter)
    {
        hunter.ApplySpeed(hunter.config != null ? hunter.config.chaseSpeed : 4.5f);
    }

    public void Tick(HunterAI hunter)
    {
        var suspicion = hunter.Suspicion != null ? hunter.Suspicion.suspicion : 0f;

        if (suspicion >= 100f)
        {
            hunter.ChangeState(HunterState.Lockdown);
            return;
        }

        if (hunter.SeesPlayer())
        {
            hunter.MoveToPlayer(0.5f);
        }
        else
        {
            hunter.MoveToLastKnownPosition(0.5f);
            if (hunter.IsPlayerHiddenInCrowd() && hunter.IsPlayerHiddenInCrowdLongEnough())
            {
                hunter.ChangeState(HunterState.Investigate);
                return;
            }
        }

        if ((suspicion < 70f && hunter.HasLostPlayerLongEnough(1.5f))
            || (hunter.HasReachedDestination() && hunter.HasLostPlayerLongEnough(2.5f)))
        {
            hunter.ChangeState(HunterState.Investigate);
        }
    }

    public void Exit(HunterAI hunter)
    {
    }
}
