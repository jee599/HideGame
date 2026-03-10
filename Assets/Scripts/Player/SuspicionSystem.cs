using System;
using UnityEngine;

public enum SuspicionBand
{
    Ignore,
    Investigate,
    Chase,
    Lockdown
}

public class SuspicionSystem : MonoBehaviour
{
    public float suspicion = 0f;
    public float decayRate = 2f;

    public SuspicionBand CurrentBand { get; private set; }

    public event Action<float> SuspicionChanged;
    public event Action<SuspicionBand> BandChanged;

    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (_player != null)
        {
            if (_player.IsDirectionless)
            {
                AddContinuousPenalty(5f);
            }

            if (_player.IsRunning)
            {
                AddContinuousPenalty(15f);
            }

            if (_player.IsLoitering)
            {
                AddContinuousPenalty(3f);
            }

            if (_player.IsMovingAgainstCrowd)
            {
                AddContinuousPenalty(10f);
            }

            if (_player.IsHunterEyeContact)
            {
                AddContinuousPenalty(20f);
            }

            if (_player.IsActingNatural)
            {
                AddInstant(-decayRate * Time.deltaTime);
            }
        }
    }

    public void AddContinuousPenalty(float amountPerSecond)
    {
        SetSuspicion(suspicion + amountPerSecond * Time.deltaTime);
    }

    public void AddInstant(float amount)
    {
        SetSuspicion(suspicion + amount);
    }

    public void AddCollisionPenalty()
    {
        AddInstant(8f);
    }

    public void AddFailedDisguisePenalty()
    {
        AddInstant(30f);
    }

    public void AddIgnoredEventPenalty()
    {
        AddContinuousPenalty(15f);
    }

    public void ResetSuspicion()
    {
        SetSuspicion(0f);
    }

    private void SetSuspicion(float value)
    {
        var clamped = Mathf.Clamp(value, 0f, 100f);
        if (Mathf.Approximately(clamped, suspicion))
        {
            return;
        }

        suspicion = clamped;
        SuspicionChanged?.Invoke(suspicion);

        var band = EvaluateBand(suspicion);
        if (band != CurrentBand)
        {
            CurrentBand = band;
            BandChanged?.Invoke(CurrentBand);
        }
    }

    private static SuspicionBand EvaluateBand(float value)
    {
        if (value >= 100f)
        {
            return SuspicionBand.Lockdown;
        }

        if (value >= 70f)
        {
            return SuspicionBand.Chase;
        }

        if (value >= 40f)
        {
            return SuspicionBand.Investigate;
        }

        return SuspicionBand.Ignore;
    }
}
