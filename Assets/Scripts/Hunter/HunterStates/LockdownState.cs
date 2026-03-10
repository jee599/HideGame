public class LockdownState : IHunterState
{
    public void Enter(HunterAI hunter)
    {
        hunter.ApplySpeed(hunter.config != null ? hunter.config.chaseSpeed : 4.5f);
    }

    public void Tick(HunterAI hunter)
    {
        var suspicion = hunter.Suspicion != null ? hunter.Suspicion.suspicion : 0f;
        if (suspicion < 70f)
        {
            hunter.ChangeState(HunterState.Chase);
            return;
        }

        if (hunter.SeesPlayer())
        {
            hunter.MoveToPlayer(0f);
        }
        else
        {
            hunter.MoveToLastKnownPosition(0f);
        }

        if (hunter.DistanceToPlayer() <= (hunter.config != null ? hunter.config.captureDistance : 1.75f))
        {
            GameManager.Instance?.EndGame(false);
            return;
        }

        if (hunter.StateElapsedTime >= (hunter.config != null ? hunter.config.lockdownDuration : 10f))
        {
            hunter.ChangeState(HunterState.Chase);
        }
    }

    public void Exit(HunterAI hunter)
    {
    }
}
